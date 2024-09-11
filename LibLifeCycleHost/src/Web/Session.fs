module internal LibLifeCycleHost.Web.Session

open System
open System.Reactive.Linq
open System.Threading.Tasks
open LibLifeCycleHost.Web.WebApiJsonEncoding
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open LibLifeCycleHost.Web.Config
open LibLifeCycleHost.Web.RealTimeSubjectData
open LibLifeCycle
open LibLifeCycleCore
open System.Globalization
open Giraffe
open Microsoft.Extensions.Logging
open Orleans
open LibLifeCycleHost.AccessControl
open System.Net
open LibLifeCycleHost

let createCallOriginFromHttpContext (httpContext: HttpContext): CallOrigin =
    if httpContext = null then
        CallOrigin.Internal
    else
        CallOrigin.External {
            RemoteAddress =
                httpContext.Connection.RemoteIpAddress
                |> Option.ofObj
                |> Option.map (fun ipAddress -> ipAddress.ToString())
                |> Option.defaultValue ""
            Headers =
                httpContext.Request.Headers
                |> Seq.map (fun kvp -> (kvp.Key, kvp.Value |> Set.ofSeq))
                |> Map.ofSeq
        }

[<RequireQualifiedAccess>]
type private RevalidationTimestampAction =
| None
| Set of RevalidateOn: DateTimeOffset
| Clear

[<RequireQualifiedAccess>]
type private RevalidationRequestAction<'Session> =
| Continue of MaybeSession: Option<'Session>
| ContinueAfterRevalidation of SessionHandler: EcosystemSessionHandler<'Session> * Session: 'Session

// Figures out what actions are required with respect to session revalidation. Does not perform these actions itself.
let private getRevalidationActions
        (hostEcosystemGrainFactory: IGrainFactory)
        (grainPartition: GrainPartition)
        (clock: Service<Clock>)
        (maybeSessionHandler: Option<EcosystemSessionHandler<'Session>>)
        (sessionHandle: SessionHandle)
        (maybeRevalidateOn: Option<DateTimeOffset>)
        : Task<RevalidationTimestampAction * RevalidationRequestAction<'Session>> =
    backgroundTask {
        // Including a grace period in timestamp checks is critical for revalidation in a SignalR context. This is because it determines when to perform
        // revalidation using Observable.Timer, and that may tick just prior to the timestamp provided to it (presumably due to rounding errors). This
        // means that immediately after the timer fires, DateTimeOffset.UtcNow could actually be greater than the timestamp rather than equal to or
        // less than it. Without a grace period, that would cause revalidation to be skipped below, since the revalidation timestamp is in the future.
        let gracePeriod = TimeSpan.FromSeconds(1)

        match maybeSessionHandler|> Option.bind (fun h -> h.MaybeRevalidator |> Option.map (fun r  -> h, r)) with
        | Some (sessionHandler, revalidator) ->
            match maybeRevalidateOn with
            | None ->
                let! maybeSession = Sessions.resolveSession (SessionSource.Handle sessionHandle) sessionHandler hostEcosystemGrainFactory grainPartition

                match maybeSession with
                | None ->
                    return (RevalidationTimestampAction.None, None |> RevalidationRequestAction.Continue)
                | Some session ->
                    match revalidator.GetRevalidateOn session with
                    | None ->
                        return (RevalidationTimestampAction.None, session |> Some |> RevalidationRequestAction.Continue)
                    | Some revalidateOn ->
                        let! now = clock.Query Clock.Now

                        if revalidateOn.Subtract(gracePeriod) > now then
                            return (RevalidationTimestampAction.Set revalidateOn, session |> Some |> RevalidationRequestAction.Continue)
                        else
                            return (RevalidationTimestampAction.Set revalidateOn, RevalidationRequestAction.ContinueAfterRevalidation (sessionHandler, session))
            | Some revalidateOn ->
                let! now = clock.Query Clock.Now

                if revalidateOn.Subtract(gracePeriod) > now then
                    // Fast path (no need to load the session): revalidation isn't due yet based on the passed in timestamp.
                    return (RevalidationTimestampAction.None, None |> RevalidationRequestAction.Continue)
                else
                    let! maybeSession = Sessions.resolveSession (SessionSource.Handle sessionHandle) sessionHandler hostEcosystemGrainFactory grainPartition

                    match maybeSession with
                    | None ->
                        return (RevalidationTimestampAction.Clear, None |> RevalidationRequestAction.Continue)
                    | Some session ->
                        match revalidator.GetRevalidateOn session with
                        | None ->
                            return (RevalidationTimestampAction.Clear, session |> Some |> RevalidationRequestAction.Continue)
                        | Some revalidateOn ->
                            let! now = clock.Query Clock.Now

                            if revalidateOn.Subtract(gracePeriod) > now then
                                return (RevalidationTimestampAction.Set revalidateOn, session |> Some |> RevalidationRequestAction.Continue)
                            else
                                return (RevalidationTimestampAction.Set revalidateOn, RevalidationRequestAction.ContinueAfterRevalidation (sessionHandler, session))
        | None ->
            return (RevalidationTimestampAction.None, None |> RevalidationRequestAction.Continue)
    }

// Revalidates a session if necessary, with no assumptions about how or where revalidation timestamps are stored (they're returned to the caller).
let private maybeRevalidateSession<'Session, 'T>
        (hostEcosystemGrainFactory: IGrainFactory)
        (grainPartition: GrainPartition)
        (clock: Service<Clock>)
        (maybeSessionHandler: Option<EcosystemSessionHandler<'Session>>)
        (sessionHandle: SessionHandle)
        (maybeRevalidateOn: Option<DateTimeOffset>)
        (successContinuation: RevalidationTimestampAction -> Task<'T>)
        (failureContinuation: RevalidateError -> Task<'T>): Task<'T> =
    backgroundTask {
        let! revalidationTimeStampAction, requestAction =
            getRevalidationActions hostEcosystemGrainFactory grainPartition clock maybeSessionHandler sessionHandle maybeRevalidateOn

        // Process the request according to returned action.
        match requestAction with
        | RevalidationRequestAction.Continue _ -> return! successContinuation revalidationTimeStampAction
        | RevalidationRequestAction.ContinueAfterRevalidation (sessionHandler, session) ->
            let! now = clock.Query Clock.Now
            let! revalidationResult = Sessions.revalidateSession session sessionHandler now hostEcosystemGrainFactory grainPartition

            match revalidationResult with
            | Ok _ ->
                // Successful revalidation.
                return! successContinuation revalidationTimeStampAction
            | Error error ->
                return! failureContinuation error
    }

// HTTP session management centers around the use of encrypted cookies as a storage mechanism for session ID and revalidation timestamps.
module Http =
    let private sessionHandleCookieName (ecosystem: Ecosystem) = (sprintf "_sess_%s" ecosystem.Name)

    let private sessionRevalidateOnCookieName (ecosystem: Ecosystem) = (sprintf "_sess_rv_%s" ecosystem.Name)

    let private sessionRevalidateOnFormat = "yyyy-MM-dd HH:mm:ssZ"

    // TODO: should I use codecs instead, or simple delimited string?
    let sessionHandleEncoder = generateAutoEncoder<SessionHandle>
    let sessionHandleDecoder = generateAutoDecoder<SessionHandle>

    let private encryptValue
            (cryptographer: ApiSessionCryptographer)
            (value: string) =
        let encryptedValue = cryptographer.Encrypt value
        Convert.ToBase64String(encryptedValue)

    let private decryptValue
            (cryptographer: ApiSessionCryptographer)
            (encryptedValue: string) =
        let encryptedValue = Convert.FromBase64String(encryptedValue)
        cryptographer.Decrypt encryptedValue

    let private removeObsoleteApiCookie (name: string) (httpContext: HttpContext) =
        let httpConfig = httpContext.RequestServices.GetRequiredService<HttpCookieConfiguration>()
        // used to set Api cookie domain to the same domain as apps, no longer the case, remove to disambiguate
        // this code can be deleted when all ecosystems update and all client cookies either updated or expired
        let appCookieDomain = httpConfig.GetAppCookieDomainForHostName httpContext.Request.Host.Value
        if not (String.IsNullOrEmpty appCookieDomain) then
            let cookieOptions = CookieOptions()
            cookieOptions.Path <- "/"
            cookieOptions.Domain <- appCookieDomain
            httpContext.Response.Cookies.Delete(name, cookieOptions)

    let private addApiCookie
        (cryptographer: ApiSessionCryptographer)
        (httpContext: HttpContext)
        (name: string)
        (value: string) =
        let encryptedValue = encryptValue cryptographer value
        let cookieOptions = CookieOptions()
        let isDevHost =
            (httpContext.RequestServices.GetService typeof<HttpHandlerSettings> :?> HttpHandlerSettings)
            |> Option.ofObj
            |> Option.map(fun s -> s.IsDevHost)
            |> Option.defaultValue false

        cookieOptions.HttpOnly <- true
        cookieOptions.Expires <- DateTimeOffset.UtcNow.AddYears(2)

        // Note, this will not work if the API service and frontend web are on different root domains
        // In such cases, SameSiteMode.None is the correct value, but Safari has decided to not allow cross-origin
        // cookies based on a user browser preference that is enabled by default
        cookieOptions.SameSite <- SameSiteMode.Lax

        if not isDevHost then // disable in dev, breaks native dev
            cookieOptions.Secure <- true

        // don't set cookie domain, client will set it for specific api host, which is fine
        // cookieOptions.Domain <- ?

        httpContext.Response.Cookies.Append(name, encryptedValue, cookieOptions)

        removeObsoleteApiCookie name httpContext

    let private removeApiCookie
            (httpContext: HttpContext)
            (name: string) =
        httpContext.Response.Cookies.Delete(name)
        removeObsoleteApiCookie name httpContext

    let addSessionRevalidateOnCookie
            (cryptographer: ApiSessionCryptographer)
            (ecosystem: Ecosystem)
            (httpContext: HttpContext)
            (revalidateOn: DateTimeOffset) =
        addApiCookie cryptographer httpContext (sessionRevalidateOnCookieName ecosystem) (revalidateOn.ToString(sessionRevalidateOnFormat, CultureInfo.InvariantCulture))

    let private getMaybeEncryptedRevalidateOn
            (ecosystem: Ecosystem)
            (httpContext: HttpContext) =
        let hasSessionRevalidateOn, encryptedSessionRevalidateOn = httpContext.Request.Cookies.TryGetValue(sessionRevalidateOnCookieName ecosystem)
        let maybeEncryptedSessionRevalidateOn =
            match hasSessionRevalidateOn with
            | true -> Some encryptedSessionRevalidateOn
            | false -> None
        maybeEncryptedSessionRevalidateOn

    let getMaybeRevalidateOn
            (cryptographer: ApiSessionCryptographer)
            (ecosystem: Ecosystem)
            (httpContext: HttpContext) =
        getMaybeEncryptedRevalidateOn ecosystem httpContext
        |> Option.bind (fun encryptedSessionRevalidateOn ->
            let sessionRevalidateOnResult = decryptValue cryptographer encryptedSessionRevalidateOn
            match sessionRevalidateOnResult with
            | Ok sessionRevalidateOnStr ->
                let success, parsedValue = DateTimeOffset.TryParseExact(sessionRevalidateOnStr, [| sessionRevalidateOnFormat |], null, DateTimeStyles.RoundtripKind)
                if success then Some parsedValue else None
            | Error _ -> None)

    let removeSessionRevalidateOnCookie
            (ecosystem: Ecosystem)
            (httpContext: HttpContext) =
        removeApiCookie httpContext (sessionRevalidateOnCookieName ecosystem)

    let private addSessionHandleCookie
            (cryptographer: ApiSessionCryptographer)
            (ecosystem: Ecosystem)
            (httpContext: HttpContext)
            (sessionHandle: SessionHandle) =

        let serializedSessionHandle = sessionHandle |> sessionHandleEncoder |> Encode.toString
        addApiCookie cryptographer httpContext (sessionHandleCookieName ecosystem) serializedSessionHandle

    let getMaybeEncryptedSessionHandle
            (ecosystem: Ecosystem)
            (httpContext: HttpContext) =
        let hasSessionHandle, encryptedSessionHandle = httpContext.Request.Cookies.TryGetValue(sessionHandleCookieName ecosystem)
        let maybeEncryptedSessionHandle =
            match hasSessionHandle with
            | true -> Some encryptedSessionHandle
            | false -> None
        maybeEncryptedSessionHandle

    let getSessionHandleFromEncryptedSessionHandle
            (cryptographer: ApiSessionCryptographer)
            (maybeEncryptedSessionHandle: Option<string>)
            : SessionHandle =
        maybeEncryptedSessionHandle
        |> Option.bind (fun encryptedSessionHandle ->
            let sessionHandleResult = decryptValue cryptographer encryptedSessionHandle
            match sessionHandleResult with
            | Ok serializedSessionHandle ->
                Decode.fromString sessionHandleDecoder serializedSessionHandle |> Result.toOption
            | Error _ -> None)
        |> Option.defaultValue SessionHandle.NoSession

    // A convenience for most use cases where the HttpContext is still available (i.e. still executing within context of the original request).
    let getSessionHandle
            (cryptographer: ApiSessionCryptographer)
            (ecosystem: Ecosystem)
            (httpContext: HttpContext): SessionHandle =
        let maybeEncryptedSessionHandle = getMaybeEncryptedSessionHandle ecosystem httpContext
        getSessionHandleFromEncryptedSessionHandle cryptographer maybeEncryptedSessionHandle

    // Determines whether the session subject to be accessed is the same as the existing session and, if not, clears the session grain's state. This is an
    // important precaution for cases where an old session is re-used. i.e. it prevents an unauthorized client taking ownership of existing session state.
    let clearSessionIfRequired
            (hostEcosystemGrainFactory: IGrainFactory)
            (grainPartition: GrainPartition)
            (cryptographer: ApiSessionCryptographer)
            (ecosystem: Ecosystem)
            (_lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (httpContext: HttpContext)
            (sessionSubjectIdStr: string)
            (getSessionHandleFromSession: 'Subject -> SessionHandle) =
        backgroundTask {
            let cookieSessionHandle = getSessionHandle cryptographer ecosystem httpContext

            let (GrainPartition partitionGuid) = grainPartition
            let sessionGrain = hostEcosystemGrainFactory.GetGrain<ISubjectGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>>(partitionGuid, sessionSubjectIdStr)

            let! mustClearSessionSubject =
                match cookieSessionHandle with
                | SessionHandle.NoSession ->
                    Task.FromResult true
                | SessionHandle.Session (cookieSessionId, _) ->
                    if sessionSubjectIdStr <> cookieSessionId then
                        Task.FromResult true
                    else
                        // lookup backend authenticated user and compare to the cookie user. It's only for Session HTTP endpoints, i.e. not much overhead
                        backgroundTask {
                            // TODO: use repo for speed? can't be bothered to wire it up
                            match! sessionGrain.Get { SessionHandle = SessionHandle.NoSession; CallOrigin = CallOrigin.Internal } with
                            | Ok maybeVersionedSessionSubject ->
                                let backendSessionHandle =
                                    maybeVersionedSessionSubject
                                    |> Option.map (fun v -> getSessionHandleFromSession v.Subject)
                                    |> Option.defaultValue SessionHandle.NoSession
                                return backendSessionHandle <> cookieSessionHandle

                            | Error GrainGetError.AccessDenied ->
                                return false // unexpected but let's no-op
                        }

            if mustClearSessionSubject then
                do! sessionGrain.SessionOnlyClearState ()

                // note that removal of sessionHandle cookie can be followed by setting it again (see setSessionHandleCookieIfChanged)
                // e.g. clear old cookie, authenticate, send new cookie - it all can happen in a single HTTP request
                // tested that it works:  first removal results in Set-Header that sets the cookie value to empty string, then it's set to proper latest handle value
                removeApiCookie httpContext (sessionHandleCookieName ecosystem)
        }

    let setSessionHandleCookieIfChanged
        (cryptographer: ApiSessionCryptographer)
        (ecosystem: Ecosystem)
        (httpContext: HttpContext)
        (latestSessionHandle: SessionHandle) =
        let decryptedSessionHandle = getSessionHandle cryptographer ecosystem httpContext
        if decryptedSessionHandle <> latestSessionHandle then
            addSessionHandleCookie cryptographer ecosystem httpContext latestSessionHandle

    // Applies necessary session revalidation behavior to the HTTP pipeline.
    let sessionRevalidationHandler<'Session>
            (hostEcosystemGrainFactory: IGrainFactory)
            (grainPartition: GrainPartition)
            (clock: Service<Clock>)
            (cryptographer: ApiSessionCryptographer)
            (ecosystem: Ecosystem)
            (maybeSessionHandler: Option<EcosystemSessionHandler<'Session>>)
            (ignoreErrors: bool): HttpHandler =
        fun next httpContext ->
            backgroundTask {
                let sessionHandle = getSessionHandle cryptographer ecosystem httpContext

                match sessionHandle, maybeSessionHandler with
                | SessionHandle.Session (sessionId, userId), _ ->
                    sessionId, (userId |> function Anonymous -> "<anon>" | Authenticated (userId, _) -> userId)
                | SessionHandle.NoSession, Some _
                | SessionHandle.NoSession, None -> "<anon>", "<anon>"
                |> fun (sessionIdStr, userIdStr) ->
                    httpContext.Items.Add ("AppInsights_UserId", userIdStr)
                    httpContext.Items.Add ("AppInsights_SessionId", sessionIdStr)

                let maybeRevalidateOn = getMaybeRevalidateOn cryptographer ecosystem httpContext

                return!
                    maybeRevalidateSession
                        hostEcosystemGrainFactory
                        grainPartition
                        clock
                        maybeSessionHandler
                        sessionHandle
                        maybeRevalidateOn
                        (fun revalidationTimestampAction ->
                            backgroundTask {
                                match revalidationTimestampAction with
                                | RevalidationTimestampAction.Set timestamp ->
                                    addSessionRevalidateOnCookie cryptographer ecosystem httpContext timestamp
                                | RevalidationTimestampAction.Clear ->
                                    removeSessionRevalidateOnCookie ecosystem httpContext
                                | RevalidationTimestampAction.None ->
                                    ()
                                return! next httpContext
                            }
                        )
                        (fun error ->
                            let logger = httpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Revalidate")
                            logger.LogWarning $"Session revalidation failed. Handle: %A{sessionHandle}, Details: %A{error}"

                            if ignoreErrors then
                                next httpContext
                            else
                                match error with
                                | RevalidateError.Internal _ ->
                                    httpContext.SetStatusCode(int HttpStatusCode.InternalServerError)
                                    earlyReturn httpContext
                                | RevalidateError.External _ ->
                                    httpContext.SetStatusCode(int HttpStatusCode.ServiceUnavailable)
                                    earlyReturn httpContext
                        )
            }

// SignalR session management uses observables to manage and expose state. This is possible because SignalR clients are pinned to a single web server.
// It is not possible to use the HTTP context because that is only available when the SignalR stream is first initialized. We need to be able to do
// things like revalidate a session long after the HTTP context is available.
module SignalR =
    let private getMaybeRevalidateOn
            (maybeSession: Option<'Session>)
            (maybeSessionHandler: Option<EcosystemSessionHandler<'Session>>): Option<DateTimeOffset> =
        let maybeRevalidator =
            maybeSessionHandler
            |> Option.bind (fun sessionHandler -> sessionHandler.MaybeRevalidator)

        match maybeSession, maybeRevalidator with
        | Some session, Some revalidator ->
            revalidator.GetRevalidateOn session
        | _ ->
            None

    let getMaybeSessionWithIsValid
            (logger: IFsLogger)
            (hostEcosystemGrainFactory: IGrainFactory)
            (grainPartition: GrainPartition)
            (clock: Service<Clock>)
            (cryptographer: ApiSessionCryptographer)
            (maybeSessionHandler: Option<EcosystemSessionHandler<'Session>>)
            (maybeSessionRealTimeSubjectData: Option<IRealTimeSubjectData<'Session, 'Session>>)
            (maybeEncryptedSessionHandle: Option<string>): IObservable<Option<'Session> * bool> =
        let sessionHandle =
            match maybeEncryptedSessionHandle with
            | Some encryptedSessionHandle ->
                Observable
                    .FromAsync(fun () ->
                        Http.getSessionHandleFromEncryptedSessionHandle cryptographer (Some encryptedSessionHandle)
                        |> Task.FromResult)
                    .Catch(fun _ -> Observable.Return(SessionHandle.NoSession))
            | None ->
                Observable.Return(SessionHandle.NoSession)

        let sessionHandleWithMaybeSession =
            sessionHandle
                .Select(fun sessionHandle ->
                    match sessionHandle with
                    | SessionHandle.Session (sessionId, _) ->
                        let sessionSharedObservable =
                            match maybeSessionRealTimeSubjectData with
                            | None ->
                                Observable.Return(None: Option<'Session>)
                            | Some sessionRealTimeSubjectData ->
                                sessionRealTimeSubjectData.GetRawSubjectObservable logger sessionId
                        sessionSharedObservable
                            .Select(fun maybeSession -> (sessionHandle, maybeSession))
                            .Catch(fun _ -> Observable.Return((sessionHandle, None)))
                    | SessionHandle.NoSession ->
                        Observable.Return((sessionHandle, None))
                )
                .Switch()
                .Publish()
                .RefCount()

        let isValidSession =
            sessionHandleWithMaybeSession
                .Select(fun (sessionHandle, maybeSession) ->
                    let maybeRevalidateOn = getMaybeRevalidateOn maybeSession maybeSessionHandler
                    (sessionHandle, maybeRevalidateOn)
                )
                .Select(fun (sessionHandle, maybeRevalidateOn) ->
                    match maybeRevalidateOn with
                    | Some revalidateOn ->
                        // The timer may tick just before revalidateOn, but the revalidation logic includes a grace period to accommodate that possibility.
                        // We could also have accounted for it here by doing:
                        //
                        // Observable.Timer(revalidateOn).Delay(100ms).DoWhile(fun () -> DateTimeOffset.UtcNow <= revalidateOn).LastAsync()
                        //
                        // However, the grace period is more broadly applicable and probably a little easier to understand.
                        Observable
                            .Timer(revalidateOn)
                            .Select(fun _ -> (sessionHandle, maybeRevalidateOn))
                    | None ->
                        Observable.Return((sessionHandle, maybeRevalidateOn))
                )
                .Switch()
                .Select(fun (sessionHandle, maybeRevalidateOn) ->
                    Observable
                        .FromAsync(fun () ->
                            maybeRevalidateSession
                                hostEcosystemGrainFactory
                                grainPartition
                                clock
                                maybeSessionHandler
                                sessionHandle
                                maybeRevalidateOn
                                (fun _ -> Task.FromResult(true))
                                (fun _ -> Task.FromResult(false))
                        )
                        .Catch(fun _ -> Observable.Return(false))
                )
                .Switch()
                .DistinctUntilChanged()
                // An already established session will mean that no value is produced until the revalidation timer ticks, so we need to seed a starting value.
                .StartWith(true)

        let maybeSessionWithIsValid =
            Observable
                .CombineLatest(
                    sessionHandleWithMaybeSession,
                    isValidSession,
                    fun (_, maybeSession) isValidSession -> (maybeSession, isValidSession)
                )
                .Do(
                    (fun _ -> ()),
                    // Should never occur, so we log for analytical purposes.
                    (fun (e: exn) -> logger.ErrorExn e $"maybeSessionWithRevalidationStatus failed")
                )

        maybeSessionWithIsValid
