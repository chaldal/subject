module internal LibLifeCycleHost.Web.LegacyGenericHttpHandler

// TODO: delete this once all clients are migrated to work against the V1 API

open System
open System.Net
open System.Reflection
open LibLifeCycle.LifeCycles.Meta.Types
open LibLifeCycleHost.AccessControl
open LibLifeCycleTypes
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Http
open Giraffe
open LibLifeCycle
open LibLifeCycleCore
open LibLifeCycleHost
open LibLifeCycleHost.Web.Http
open LibLifeCycleHost.Web.LegacyJsonEncoding
open LibLifeCycleHost.Web.WebApiJsonEncoding
open Orleans
open System.Threading.Tasks
open LibLifeCycleTypes.File
open LibLifeCycleTypes.LegacyHttp

type AnchorTypeForModule = private AnchorTypeForModule of unit // To get typeof<Module>

type private ActOrConstructAndWaitOnLifeEventResult<'Subject, 'SubjectId, 'LifeEvent
        when 'Subject   :> Subject<'SubjectId>
        and  'SubjectId :> SubjectId
        and  'LifeEvent :> LifeEvent>
with
    member this.ToApi() =
        match this with
        | ActOrConstructAndWaitOnLifeEventResult.LifeEventTriggered (finalValueAfterEvent, triggeredEvent) ->
            ApiActOrConstructAndWaitOnLifeEventResult.LifeEventTriggered (finalValueAfterEvent.Subject, triggeredEvent)
        | ActOrConstructAndWaitOnLifeEventResult.WaitOnLifeEventTimedOut initialValueAfterActionOrConstruction ->
            ApiActOrConstructAndWaitOnLifeEventResult.WaitOnLifeEventTimedOut (initialValueAfterActionOrConstruction.Subject)

let private getActOrConstructAndWaitOnLifeEventResultProjectionEncoder<'Subject, 'Projection, 'LifeEvent when 'LifeEvent :> LifeEvent>
     (projection: UntypedSubjectProjectionDef<'Subject>)
    : Encoder<ApiActOrConstructAndWaitOnLifeEventResult<'Subject, 'LifeEvent>> =
    let typedProjection = fun subj -> projection.Projection subj :?> 'Projection
    let projectionEncoder = generateAutoEncoder<ApiActOrConstructAndWaitOnLifeEventResult<'Projection, 'LifeEvent>>
    (function
    | ApiActOrConstructAndWaitOnLifeEventResult.WaitOnLifeEventTimedOut subject -> (typedProjection subject) |> ApiActOrConstructAndWaitOnLifeEventResult.WaitOnLifeEventTimedOut
    | ApiActOrConstructAndWaitOnLifeEventResult.LifeEventTriggered(subject, lifeEvent) -> ((typedProjection subject), lifeEvent) |> ApiActOrConstructAndWaitOnLifeEventResult.LifeEventTriggered)
    >> projectionEncoder

let private buildActOrConstructAndWaitOnLifeEventResultProjectionEncoders
    (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>) =
    lifeCycleDef.ProjectionDefs.Map
    |> Map.map (fun _ projection ->
        typeof<AnchorTypeForModule>.DeclaringType
            .GetMethod(nameof getActOrConstructAndWaitOnLifeEventResultProjectionEncoder, BindingFlags.NonPublic ||| BindingFlags.Static)
            .MakeGenericMethod([| typeof<'Subject>; projection.ProjectionType; typeof<'LifeEvent> |])
            .Invoke(null, [| projection |])
            :?> Encoder<ApiActOrConstructAndWaitOnLifeEventResult<'Subject, 'LifeEvent>>)

let getLegacyGenericViewHttpHandler<'Input, 'Output, 'OpError when 'OpError :> OpError>
        (ecosystem: Ecosystem)
        (cryptographer: ApiSessionCryptographer)
        (viewAdapter: ViewAdapter<'Input, 'Output, 'OpError>)
        : HttpHandler =
    let outputEncoder = generateAutoEncoder<'Output>

    let opErrorEncoder = generateAutoEncoder<'OpError>

    let inputDecoder = generateAutoDecoder<'Input>

    let readHandler =
        fun (_next: HttpFunc) (httpContext: HttpContext) ->
            task {
                let! resInput =
                    if typeof<'Input> <> typeof<NoInput> then
                        if httpContext.Request.Method = "GET" then
                            match httpContext.TryGetQueryStringValue "i" with
                            | Some inputQueryString ->
                                Decode.fromString inputDecoder inputQueryString
                            | _ ->
                                Error "Query string param \"i\" expected with JSON-encoded view input"
                            |> Task.FromResult
                        else
                            readJsonBody httpContext inputDecoder
                    else
                        NoInput |> box :?> 'Input |> Ok |> Task.FromResult

                match resInput with
                | Ok input ->
                    let sessionHandle = Session.Http.getSessionHandle cryptographer ecosystem httpContext
                    let serviceProvider = httpContext.RequestServices
                    let grainPartition = serviceProvider.GetRequiredService<GrainPartition>()
                    let hostEcosystemGrainFactory = serviceProvider.GetRequiredService<IGrainFactory>()
                    let callOrigin = Session.createCallOriginFromHttpContext httpContext
                    let! res =
                        Views.authorizeViewAndContinueWith
                            hostEcosystemGrainFactory
                            grainPartition
                            viewAdapter.View
                            (SessionSource.Handle sessionHandle)
                            callOrigin
                            input
                            (GrainExecutionError.AccessDenied |> Error)
                            (fun () ->
                                task {
                                    let (ViewResult viewTask) = viewAdapter.View.Read callOrigin httpContext.RequestServices input
                                    match! viewTask with
                                    | Ok output -> return Ok output
                                    | Error err -> return err |> GrainExecutionError.ExecutionError |> Error
                                }
                            )
                    match res with
                    | Ok output ->
                        match box output with
                        | :? File as file ->
                            // Encode files as raw
                            return! respondFile httpContext file None None
                        | :? HttpFile as httpFile ->
                            // Encode text files as raw, with charset encoding
                            return! respondFile httpContext httpFile.File httpFile.MaybeTextEncoding httpFile.MaybeDownloadFileName
                        | _ ->
                            return! respondJson outputEncoder output httpContext
                    | Error (GrainExecutionError.ExecutionError err) ->
                        httpContext.SetStatusCode (int HttpStatusCode.UnprocessableEntity)
                        return! respondJson opErrorEncoder err httpContext
                    | Error (GrainExecutionError.AccessDenied) ->
                        httpContext.SetStatusCode (int HttpStatusCode.Forbidden)
                        return! earlyReturn httpContext
                | Error err ->
                    httpContext.SetStatusCode (int HttpStatusCode.BadRequest)
                    return! httpContext.WriteStringAsync err
            }

    subRoute
        (sprintf "/%s" viewAdapter.View.Name)
        (choose [
            GET >=> choose [
                route "/" >=> readHandler
            ]
            POST >=> choose [
                route "/" >=> readHandler
            ]
        ])

let getLegacyGenericSubjectHttpHandler<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId
                       when 'Subject              :> Subject<'SubjectId>
                       and  'LifeAction           :> LifeAction
                       and  'OpError              :> OpError
                       and  'Constructor          :> Constructor
                       and  'LifeEvent            :> LifeEvent
                       and  'LifeEvent            : comparison
                       and  'SubjectIndex         :> SubjectIndex<'OpError>
                       and  'SubjectId            :> SubjectId
                       and  'SubjectId            : comparison>
        (hostEcosystemGrainFactory: IGrainFactory)
        (grainPartition: GrainPartition)
        (clock: Service<Clock>)
        (cryptographer: ApiSessionCryptographer)
        (ecosystem: Ecosystem)
        (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
        (lifeCycleAdapter: HostedLifeCycleAdapter<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>)
        : HttpHandler =

    let subjectBaseEncoder                              = generateAutoEncoder<'Subject>
    let opErrorEncoder                                  = generateAutoEncoder<'OpError>
    let subjectBaseWithLifeEventEncoder                 = generateAutoEncoder<ApiActOrConstructAndWaitOnLifeEventResult<'Subject, 'LifeEvent>>
    let accessControlledBaseSubjectListEncoder          = generateAutoEncoder<List<AccessControlled<'Subject, 'SubjectId>>>
    let accessControlledBaseSubjectListWithCountEncoder = generateAutoEncoder<ListWithTotalCount<AccessControlled<'Subject, 'SubjectId>>>
    let auditTrailEncoder                               = generateAutoEncoder<List<UntypedSubjectAuditData>>
    let auditTrailTypedEncoder                          = generateAutoEncoder<List<SubjectAuditData<'LifeAction, 'Constructor>>>
    let snapshotEncoder                                 = generateAutoEncoder<TemporalSnapshot<'Subject, 'LifeAction, 'Constructor, 'SubjectId>>
    let sideEffectPermanentFailuresListEncoder          = generateAutoEncoder<List<SideEffectPermanentFailure>>

    let subjectProjectionEncoders = buildSubjectProjectionEncoders lifeCycleDef
    let accessControlledSubjectProjectionListEncoders = buildAccessControlledSubjectProjectionListEncoders lifeCycleDef
    let accessControlledSubjectProjectionListWithTotalCountEncoders = buildAccessControlledSubjectProjectionListWithTotalCountEncoders lifeCycleDef
    let actOrConstructAndWaitOnLifeEventResultProjectionEncoders = buildActOrConstructAndWaitOnLifeEventResultProjectionEncoders lifeCycleDef

    let subjectEncoder (projectionName: Option<string>) : Result<Encoder<'Subject> * SubjectProjection, unit> =
        match projectionName with
        | None ->
            (subjectBaseEncoder, OriginalProjection) |> Ok
        | Some projectionName ->
            match subjectProjectionEncoders.TryFind projectionName with
            | None ->
                Error ()
            | Some projectionEncoder ->
                (projectionEncoder, (Projection projectionName)) |> Ok

    let accessControlledSubjectListEncoder (projectionName: Option<string>) : Result<Encoder<List<AccessControlled<'Subject, 'SubjectId>>> * SubjectProjection, unit> =
        match projectionName with
        | None ->
            (accessControlledBaseSubjectListEncoder, OriginalProjection) |> Ok
        | Some projectionName ->
            match accessControlledSubjectProjectionListEncoders.TryFind projectionName with
            | None ->
                Error ()
            | Some projectionEncoder ->
                (projectionEncoder, (Projection projectionName)) |> Ok

    let accessControlledSubjectListWithTotalCountEncoder (projectionName: Option<string>) : Result<Encoder<ListWithTotalCount<AccessControlled<'Subject, 'SubjectId>>> * SubjectProjection, unit> =
        match projectionName with
        | None ->
            (accessControlledBaseSubjectListWithCountEncoder, OriginalProjection) |> Ok
        | Some projectionName ->
            match accessControlledSubjectProjectionListWithTotalCountEncoders.TryFind projectionName with
            | None ->
                Error ()
            | Some projectionEncoder ->
                (projectionEncoder, (Projection projectionName)) |> Ok

    let actOrConstructAndWaitOnLifeEventResultEncoder (projectionName: Option<string>) : Result<Encoder<ApiActOrConstructAndWaitOnLifeEventResult<'Subject, 'LifeEvent>> * SubjectProjection, unit> =
        match projectionName with
        | None ->
            (subjectBaseWithLifeEventEncoder, OriginalProjection) |> Ok
        | Some projectionName ->
            match actOrConstructAndWaitOnLifeEventResultProjectionEncoders.TryFind projectionName with
            | None ->
                Error ()
            | Some projectionWithLifeEventEncoder ->
                (projectionWithLifeEventEncoder, (Projection projectionName)) |> Ok

    let actionDecoder                   = generateAutoDecoder<'LifeAction>
    let idDecoder                       = generateAutoDecoder<'SubjectId>
    let constructorDecoder              = generateAutoDecoder<'Constructor>
    let manyIdsDecoder                  = generateAutoDecoder<NonemptySet<'SubjectId>>

    let getMaybeConstructDecoder        = generateAutoDecoder<GetMaybeConstruct<'SubjectId, 'Constructor>>
    let actMaybeConstructDecoder        = generateAutoDecoder<ActMaybeConstruct<'LifeAction, 'Constructor>>
    let actAndWaitDecoder               = generateAutoDecoder<ActAndWaitOnLifeEvent<'LifeAction, 'LifeEvent>>
    let actMaybeConstructAndWaitDecoder = generateAutoDecoder<ActMaybeConstructAndWaitOnLifeEvent<'LifeAction, 'Constructor, 'LifeEvent>>
    let constructAndWaitDecoder         = generateAutoDecoder<ConstructAndWaitOnLifeEvent<'Constructor, 'LifeEvent>>

    let indexQueryDecoder               = generateAutoDecoder<IndexQuery<'SubjectIndex>>
    let indexPredicateDecoder           = generateAutoDecoder<PreparedIndexPredicate<'SubjectIndex>>
    let resultSetOptionsDecoder         = generateAutoDecoder<ResultSetOptions<'SubjectIndex>>

    let respondMaybeConstructionError maybeConstructionError (httpContext: HttpContext) =
        match maybeConstructionError with
        | GrainMaybeConstructionError.ConstructionError err ->
            httpContext.SetHttpHeader ("X-Operation", "Construction")
            httpContext.SetStatusCode (int HttpStatusCode.UnprocessableEntity)
            respondJson opErrorEncoder err httpContext
        | GrainMaybeConstructionError.InitializingInTransaction ->
            httpContext.SetHttpHeader ("X-Operation", "Construction")
            httpContext.SetStatusCode (int HttpStatusCode.Locked)
            httpContext.WriteStringAsync "Subject is locked in transaction"
        | GrainMaybeConstructionError.AccessDenied ->
            httpContext.SetStatusCode (int HttpStatusCode.Forbidden)
            earlyReturn httpContext

    let respondGrainOperationError (grainOperationError: GrainOperationError<'OpError>) (httpContext: HttpContext) =
        match grainOperationError with
        | GrainOperationError.TransitionError err ->
            httpContext.SetHttpHeader ("X-Operation", "Transition")
            httpContext.SetStatusCode (int HttpStatusCode.UnprocessableEntity)
            respondJson opErrorEncoder err httpContext
        | GrainOperationError.ConstructionError err ->
            httpContext.SetHttpHeader ("X-Operation", "Construction")
            httpContext.SetStatusCode (int HttpStatusCode.UnprocessableEntity)
            respondJson opErrorEncoder err httpContext
        | GrainOperationError.TransitionNotAllowed ->
            httpContext.SetHttpHeader ("X-Operation", "Transition")
            httpContext.SetStatusCode (int HttpStatusCode.UnprocessableEntity)
            httpContext.WriteStringAsync "Transition not allowed"
        | GrainOperationError.LockedInTransaction ->
            httpContext.SetStatusCode (int HttpStatusCode.Locked)
            httpContext.WriteStringAsync "Subject is locked in transaction"
        | GrainOperationError.AccessDenied ->
            httpContext.SetStatusCode (int HttpStatusCode.Forbidden)
            earlyReturn httpContext

    let respondGrainTransitionError (grainTransitionError: GrainTransitionError<'OpError>) (httpContext: HttpContext) =
        match grainTransitionError with
        | GrainTransitionError.SubjectNotInitialized pKey ->
            httpContext.SetStatusCode (int HttpStatusCode.NotFound)
            httpContext.WriteStringAsync (sprintf "Subject with key %s doesn't exists" pKey)
        | GrainTransitionError.TransitionError err ->
            httpContext.SetStatusCode (int HttpStatusCode.UnprocessableEntity)
            respondJson opErrorEncoder err httpContext
        | GrainTransitionError.TransitionNotAllowed ->
            httpContext.SetStatusCode (int HttpStatusCode.UnprocessableEntity)
            httpContext.WriteStringAsync "Transition not allowed"
        | GrainTransitionError.LockedInTransaction ->
            httpContext.SetStatusCode (int HttpStatusCode.Locked)
            httpContext.WriteStringAsync "Subject is locked in transaction"
        | GrainTransitionError.AccessDenied ->
            httpContext.SetStatusCode (int HttpStatusCode.Forbidden)
            earlyReturn httpContext

    let respondGrainConstructionError (grainConstructionError: GrainConstructionError<'OpError>) (httpContext: HttpContext) =
        match grainConstructionError with
        | GrainConstructionError.SubjectAlreadyInitialized pKey ->
            httpContext.SetStatusCode (int HttpStatusCode.Conflict)
            httpContext.WriteStringAsync (sprintf "Subject with key %s already exists" pKey)
        | GrainConstructionError.ConstructionError err ->
            httpContext.SetStatusCode (int HttpStatusCode.UnprocessableEntity)
            respondJson opErrorEncoder err httpContext
        | GrainConstructionError.AccessDenied ->
            httpContext.SetStatusCode (int HttpStatusCode.Forbidden)
            earlyReturn httpContext

    let applyActTelemetryRules = applyActTelemetryRules lifeCycleAdapter
    let applyConstructTelemetryRules = applyConstructTelemetryRules lifeCycleAdapter

    let withinRateLimits
        (httpContext: HttpContext)
        (rateLimitEvent: RateLimitEvent<'LifeAction, 'Constructor>)
        : Task<bool> =
        withinRateLimits hostEcosystemGrainFactory grainPartition lifeCycleAdapter httpContext rateLimitEvent ecosystem.EnforceRateLimits

    let authorizeSubjectAndContinueWith
            (httpContext: HttpContext)
            (accessEvent: AccessEvent<'Subject, 'LifeAction, 'Constructor, 'SubjectId>)
            (statusCode: HttpStatusCode)
            (sessionSource: SessionSource)
            (authorizedContinuation: unit -> Task<Option<HttpContext>>): Task<Option<HttpContext>> =
        lifeCycleAdapter.LifeCycle.Invoke
            { new FullyTypedLifeCycleFunction<_, _, _, _, _, _, _> with
                member _.Invoke lifeCycle =
                    authorizeSubjectAndContinueWith lifeCycle.MaybeApiAccess lifeCycle.SessionHandling sessionSource httpContext accessEvent statusCode authorizedContinuation None
            }

    let makeGrainConnector = makeHostEcosystemGrainConnector hostEcosystemGrainFactory grainPartition ecosystem

    let isSessionSubject = lifeCycleAdapter.LifeCycle.IsSessionLifeCycle

    let getSessionHandleFromSession (subject: 'Subject) =
        lifeCycleAdapter.LifeCycle.Invoke
            { new FullyTypedLifeCycleFunction<_, _, _, _, _, _, _> with
                member _.Invoke (lifeCycle: LifeCycle<_, _, _, _, _, _, _, _, 'Session, _, _>) =
                    lifeCycle.SessionHandler
                    |> Option.map (fun h ->
                        let typedSession = subject |> box :?> 'Session
                        let sessionIdStr = h.GetIdStr typedSession
                        let userId = h.GetUserId typedSession
                        SessionHandle.Session (sessionIdStr, userId))
                    |> Option.defaultValue SessionHandle.NoSession
            }

    let getSessionSourceAndSessionHandleFromSessionSubject (subject: 'Subject) =
        box subject |> Some |> SessionSource.InMemory, getSessionHandleFromSession subject

    let sessionGetMaybeConstructHandler : HttpHandler =
        fun _ httpContext ->
            let maybeProjectionName = tryGetProjectionName httpContext
            task {
                match subjectEncoder maybeProjectionName with
                | Ok (encoder, projection) ->
                    match! readJsonBodyAndValidate httpContext getMaybeConstructDecoder (fun getMaybeConstruct -> getMaybeConstruct.AsUntyped) with
                    | Ok getMaybeConstruct ->
                        do! Session.Http.clearSessionIfRequired hostEcosystemGrainFactory grainPartition cryptographer ecosystem lifeCycleDef httpContext getMaybeConstruct.Id.IdString getSessionHandleFromSession
                        let grainConnector = makeGrainConnector httpContext
                        match! grainConnector.GetMaybeConstruct lifeCycleDef getMaybeConstruct.Id getMaybeConstruct.Constructor with
                        | Ok versionedSubject ->
                            let idStr = versionedSubject.Subject.SubjectId |> getIdString
                            httpContext.SetHttpHeader ("X-Subject-Id", idStr)
                            let sessionSource, latestSessionHandle = getSessionSourceAndSessionHandleFromSessionSubject versionedSubject.Subject
                            Session.Http.setSessionHandleCookieIfChanged cryptographer ecosystem httpContext latestSessionHandle
                            return!
                                authorizeSubjectAndContinueWith
                                    httpContext
                                    (AccessEvent.Read(versionedSubject.Subject, projection))
                                    HttpStatusCode.ResetContent
                                    sessionSource
                                    (fun () -> respondJson encoder versionedSubject.Subject httpContext)
                        | Error err ->
                            return! respondMaybeConstructionError err httpContext
                    | Error err ->
                        httpContext.SetStatusCode (int HttpStatusCode.BadRequest)
                        return! httpContext.WriteStringAsync err
                | Error () ->
                    httpContext.SetStatusCode (int HttpStatusCode.NotFound)
                    return! httpContext.WriteStringAsync "Projection Not Defined"
            }

    let sessionActHandler (idStr: string) : HttpHandler =
        fun _ httpContext ->
            let maybeProjectionName = tryGetProjectionName httpContext
            task {
                match subjectEncoder maybeProjectionName with
                | Ok (encoder, projection) ->
                    do! Session.Http.clearSessionIfRequired hostEcosystemGrainFactory grainPartition cryptographer ecosystem lifeCycleDef httpContext idStr getSessionHandleFromSession

                    match! readJsonBodyAndValidate httpContext actionDecoder (fun action -> action :> LifeAction) with
                    | Ok action ->
                        applyActTelemetryRules httpContext action
                        match! withinRateLimits httpContext (RateLimitEvent.Act action) with
                        | true ->
                            let grainConnector = makeGrainConnector httpContext
                            match! grainConnector.Act lifeCycleDef idStr action with
                            | Ok versionedSubject ->
                                let sessionSource, latestSessionHandle = getSessionSourceAndSessionHandleFromSessionSubject versionedSubject.Subject
                                Session.Http.setSessionHandleCookieIfChanged cryptographer ecosystem httpContext latestSessionHandle
                                return!
                                    authorizeSubjectAndContinueWith
                                        httpContext
                                        (AccessEvent.Read(versionedSubject.Subject, projection))
                                        HttpStatusCode.ResetContent
                                        sessionSource
                                        (fun () -> respondJson encoder versionedSubject.Subject httpContext)
                            | Error err ->
                                return! respondGrainTransitionError err httpContext
                        | false ->
                            httpContext.SetStatusCode (int HttpStatusCode.TooManyRequests)
                            return! earlyReturn httpContext
                    | Error err ->
                        httpContext.SetStatusCode (int HttpStatusCode.BadRequest)
                        return! httpContext.WriteStringAsync err
                | Error () ->
                    httpContext.SetStatusCode (int HttpStatusCode.NotFound)
                    return! httpContext.WriteStringAsync "Projection Not Defined"
            }

    let sessionActAndWaitHandler (idStr: string) : HttpHandler =
        fun _ httpContext ->
            let maybeProjectionName = tryGetProjectionName httpContext
            let timeout =
                httpContext.TryGetQueryStringValue "waitForMs"
                |> Option.bind (
                    fun waitForMsStr ->
                        match UInt32.TryParse waitForMsStr with
                        | true, waitForMs ->
                            TimeSpan.FromMilliseconds (float waitForMs) |> Some
                        | false, _ ->
                            None
                    )
                |> Option.defaultWith (fun _ -> TimeSpan.FromSeconds 10.0)

            task {
                match actOrConstructAndWaitOnLifeEventResultEncoder maybeProjectionName with
                | Ok (encoder, projection) ->
                    do! Session.Http.clearSessionIfRequired hostEcosystemGrainFactory grainPartition cryptographer ecosystem lifeCycleDef httpContext idStr getSessionHandleFromSession

                    match! readJsonBodyAndValidate httpContext actAndWaitDecoder (fun actAndWait -> actAndWait.AsUntyped) with
                    | Ok actWait ->
                        applyActTelemetryRules httpContext actWait.Action
                        match! withinRateLimits httpContext (RateLimitEvent.Act actWait.Action) with
                        | true ->
                            let grainConnector = makeGrainConnector httpContext
                            match! grainConnector.ActAndWait lifeCycleDef idStr actWait.Action actWait.LifeEvent timeout with
                            | Ok subjAndLifeEvent ->
                                let sessionSource, latestSessionHandle = getSessionSourceAndSessionHandleFromSessionSubject subjAndLifeEvent.VersionedSubject.Subject
                                Session.Http.setSessionHandleCookieIfChanged cryptographer ecosystem httpContext latestSessionHandle
                                return!
                                    authorizeSubjectAndContinueWith
                                        httpContext
                                        (AccessEvent.Read(subjAndLifeEvent.VersionedSubject.Subject, projection))
                                        HttpStatusCode.ResetContent
                                        sessionSource
                                        (fun () -> respondJson encoder (subjAndLifeEvent.ToApi()) httpContext)
                            | Error err ->
                                return! respondGrainTransitionError err httpContext
                        | false ->
                            httpContext.SetStatusCode (int HttpStatusCode.TooManyRequests)
                            return! earlyReturn httpContext
                    | Error err ->
                        httpContext.SetStatusCode (int HttpStatusCode.BadRequest)
                        return! httpContext.WriteStringAsync err
                | Error () ->
                    httpContext.SetStatusCode (int HttpStatusCode.NotFound)
                    return! httpContext.WriteStringAsync "Projection Not Defined"
            }

    let sessionActMaybeConstructHandler (idStr: string) : HttpHandler =
        fun _ httpContext ->
            let maybeProjectionName = tryGetProjectionName httpContext
            task {
                match subjectEncoder maybeProjectionName with
                | Ok (encoder, projection) ->
                    do! Session.Http.clearSessionIfRequired hostEcosystemGrainFactory grainPartition cryptographer ecosystem lifeCycleDef httpContext idStr getSessionHandleFromSession

                    match! readJsonBodyAndValidate httpContext actMaybeConstructDecoder (fun actMaybeConstruct -> actMaybeConstruct.AsUntyped) with
                    | Ok actMaybeConstruct ->
                        applyActTelemetryRules httpContext actMaybeConstruct.Action
                        match! withinRateLimits httpContext (RateLimitEvent.Act actMaybeConstruct.Action) with
                        | true ->
                            let grainConnector = makeGrainConnector httpContext
                            match! grainConnector.ActMaybeConstruct lifeCycleDef idStr actMaybeConstruct.Action actMaybeConstruct.Constructor with
                            | Ok versionedSubject ->
                                let sessionSource, latestSessionHandle = getSessionSourceAndSessionHandleFromSessionSubject versionedSubject.Subject
                                Session.Http.setSessionHandleCookieIfChanged cryptographer ecosystem httpContext latestSessionHandle
                                return!
                                    authorizeSubjectAndContinueWith
                                        httpContext
                                        (AccessEvent.Read(versionedSubject.Subject, projection))
                                        HttpStatusCode.ResetContent
                                        sessionSource
                                        (fun () -> respondJson encoder versionedSubject.Subject httpContext)
                            | Error err ->
                                return! respondGrainOperationError err httpContext
                        | false ->
                            httpContext.SetStatusCode (int HttpStatusCode.TooManyRequests)
                            return! earlyReturn httpContext
                    | Error err ->
                        httpContext.SetStatusCode (int HttpStatusCode.BadRequest)
                        return! httpContext.WriteStringAsync err
                | Error () ->
                    httpContext.SetStatusCode (int HttpStatusCode.NotFound)
                    return! httpContext.WriteStringAsync "Projection Not Defined"
            }

    let sessionActMaybeConstructAndWaitHandler (idStr: string) : HttpHandler =
        fun _ httpContext ->
            let maybeProjectionName = tryGetProjectionName httpContext
            let timeout =
                httpContext.TryGetQueryStringValue "waitForMs"
                |> Option.bind (
                    fun waitForMsStr ->
                        match UInt32.TryParse waitForMsStr with
                        | true, waitForMs ->
                            TimeSpan.FromMilliseconds (float waitForMs) |> Some
                        | false, _ ->
                            None
                    )
                |> Option.defaultWith (fun _ -> TimeSpan.FromSeconds 10.0)

            task {
                match actOrConstructAndWaitOnLifeEventResultEncoder maybeProjectionName with
                | Ok (encoder, projection) ->
                    do! Session.Http.clearSessionIfRequired hostEcosystemGrainFactory grainPartition cryptographer ecosystem lifeCycleDef httpContext idStr getSessionHandleFromSession

                    match! readJsonBodyAndValidate httpContext actMaybeConstructAndWaitDecoder (fun actConstructWait -> actConstructWait.AsUntyped) with
                    | Ok actConstructWait ->
                        applyActTelemetryRules httpContext actConstructWait.Action
                        match! withinRateLimits httpContext (RateLimitEvent.Act actConstructWait.Action) with
                        | true ->
                            let grainConnector = makeGrainConnector httpContext
                            match! grainConnector.ActMaybeConstructAndWait lifeCycleDef idStr actConstructWait.Action actConstructWait.Constructor actConstructWait.LifeEvent timeout with
                            | Ok subjAndLifeEvent ->
                                let sessionSource, latestSessionHandle = getSessionSourceAndSessionHandleFromSessionSubject subjAndLifeEvent.VersionedSubject.Subject
                                Session.Http.setSessionHandleCookieIfChanged cryptographer ecosystem httpContext latestSessionHandle
                                return!
                                    authorizeSubjectAndContinueWith
                                        httpContext
                                        (AccessEvent.Read(subjAndLifeEvent.VersionedSubject.Subject, projection))
                                        HttpStatusCode.ResetContent
                                        sessionSource
                                        (fun () -> respondJson encoder (subjAndLifeEvent.ToApi()) httpContext)
                            | Error err ->
                                return! respondGrainOperationError err httpContext
                        | false ->
                            httpContext.SetStatusCode (int HttpStatusCode.TooManyRequests)
                            return! earlyReturn httpContext
                    | Error err ->
                        httpContext.SetStatusCode (int HttpStatusCode.BadRequest)
                        return! httpContext.WriteStringAsync err
                | Error () ->
                    httpContext.SetStatusCode (int HttpStatusCode.NotFound)
                    return! httpContext.WriteStringAsync "Projection Not Defined"
            }

    let getAccessControlledSubjects
            (httpContext: HttpContext)
            (projection: SubjectProjection)
            (subjects: List<'Subject>)
            (totalCount: uint64) =
        task {
            let sessionHandle = Session.Http.getSessionHandle cryptographer ecosystem httpContext
            let callOrigin = Session.createCallOriginFromHttpContext httpContext
            let serviceProvider = httpContext.RequestServices
            let grainPartition = serviceProvider.GetRequiredService<GrainPartition>()
            let hostEcosystemGrainFactory = serviceProvider.GetRequiredService<IGrainFactory>()
            let! accessControlledSubjects, accessControlledTotalCount =
                lifeCycleAdapter.LifeCycle.Invoke
                    { new FullyTypedLifeCycleFunction<_, _, _, _, _, _, _> with
                        member _.Invoke lifeCycle =
                            Subjects.getAccessControlledSubjectsForRead
                                hostEcosystemGrainFactory
                                grainPartition
                                lifeCycle.MaybeApiAccess
                                lifeCycle.SessionHandling
                                (SessionSource.Handle sessionHandle)
                                callOrigin
                                subjects
                                totalCount
                                projection
                                None
                    }
            return accessControlledSubjects, accessControlledTotalCount
        }

    let constructHandler : HttpHandler =
        fun _ httpContext ->
            let maybeProjectionName = tryGetProjectionName httpContext
            let callOrigin = Session.createCallOriginFromHttpContext httpContext
            task {
                match subjectEncoder maybeProjectionName with
                | Ok (encoder, projection) ->
                    match! readJsonBodyAndValidate httpContext constructorDecoder (fun ctor -> ctor :> Constructor) with
                    | Ok ctor ->
                        applyConstructTelemetryRules httpContext ctor
                        match! withinRateLimits httpContext (RateLimitEvent.Construct ctor) with
                        | true ->
                            let (IdGenerationResult idGenTask) = lifeCycleAdapter.LifeCycle.GenerateId callOrigin httpContext.RequestServices ctor
                            match! idGenTask with
                            | Ok subjId ->
                                let grainConnector = makeGrainConnector httpContext
                                match! grainConnector.Construct lifeCycleDef subjId ctor with
                                | Ok versionedSubject ->
                                    let idStr = getIdString subjId
                                    httpContext.SetHttpHeader ("X-Subject-Id", idStr)
                                    return!
                                        authorizeSubjectAndContinueWith
                                            httpContext
                                            (AccessEvent.Read(versionedSubject.Subject, projection))
                                            HttpStatusCode.ResetContent
                                            (SessionSource.Handle grainConnector.SessionHandle)
                                            (fun () -> respondJson encoder versionedSubject.Subject httpContext)

                                | Error grainConstructionError ->
                                    return! respondGrainConstructionError grainConstructionError httpContext

                            | Error err ->
                                httpContext.SetStatusCode (int HttpStatusCode.UnprocessableEntity)
                                return! respondJson opErrorEncoder err httpContext

                        | false ->
                            httpContext.SetStatusCode (int HttpStatusCode.TooManyRequests)
                            return! earlyReturn httpContext

                    | Error err ->
                        httpContext.SetStatusCode (int HttpStatusCode.BadRequest)
                        return! httpContext.WriteStringAsync err
                | Error () ->
                    httpContext.SetStatusCode (int HttpStatusCode.NotFound)
                    return! httpContext.WriteStringAsync "Projection Not Defined"
            }

    let getMaybeConstructHandler : HttpHandler =
        fun _ httpContext ->
            let maybeProjectionName = tryGetProjectionName httpContext
            task {
                match subjectEncoder maybeProjectionName with
                | Ok (encoder, projection) ->
                    match! readJsonBodyAndValidate httpContext getMaybeConstructDecoder (fun getMaybeConstruct -> getMaybeConstruct.AsUntyped) with
                    | Ok getMaybeConstruct ->
                        let grainConnector = makeGrainConnector httpContext
                        match! grainConnector.GetMaybeConstruct lifeCycleDef getMaybeConstruct.Id getMaybeConstruct.Constructor with
                        | Ok versionedSubject ->
                            return!
                                authorizeSubjectAndContinueWith
                                    httpContext
                                    (AccessEvent.Read(versionedSubject.Subject, projection))
                                    HttpStatusCode.ResetContent
                                    (SessionSource.Handle grainConnector.SessionHandle)
                                    (fun () -> respondJson encoder versionedSubject.Subject httpContext)
                        | Error err ->
                            return! respondMaybeConstructionError err httpContext
                    | Error err ->
                        httpContext.SetStatusCode (int HttpStatusCode.BadRequest)
                        return! httpContext.WriteStringAsync err
                | Error () ->
                    httpContext.SetStatusCode (int HttpStatusCode.NotFound)
                    return! httpContext.WriteStringAsync "Projection Not Defined"
            }

    let actHandler (idStr: string) : HttpHandler =
        fun _ httpContext ->
            let maybeProjectionName = tryGetProjectionName httpContext
            task {
                match subjectEncoder maybeProjectionName with
                | Ok (encoder, projection) ->
                    match! readJsonBodyAndValidate httpContext actionDecoder (fun action -> action :> LifeAction) with
                    | Ok action ->
                        applyActTelemetryRules httpContext action
                        match! withinRateLimits httpContext (RateLimitEvent.Act action) with
                        | true ->
                            let grainConnector = makeGrainConnector httpContext
                            match! grainConnector.Act lifeCycleDef idStr action with
                            | Ok versionedSubject ->
                                return!
                                    authorizeSubjectAndContinueWith
                                        httpContext
                                        (AccessEvent.Read(versionedSubject.Subject, projection))
                                        HttpStatusCode.ResetContent
                                        (SessionSource.Handle grainConnector.SessionHandle)
                                        (fun () -> respondJson encoder versionedSubject.Subject httpContext)
                            | Error err ->
                                return! respondGrainTransitionError err httpContext
                        | false ->
                            httpContext.SetStatusCode (int HttpStatusCode.TooManyRequests)
                            return! earlyReturn httpContext
                    | Error err ->
                        httpContext.SetStatusCode (int HttpStatusCode.BadRequest)
                        return! httpContext.WriteStringAsync err
                | Error () ->
                    httpContext.SetStatusCode (int HttpStatusCode.NotFound)
                    return! httpContext.WriteStringAsync "Projection Not Defined"
            }

    let actAndWaitHandler (idStr: string) : HttpHandler =
        fun _ httpContext ->
            let maybeProjectionName = tryGetProjectionName httpContext
            let timeout =
                httpContext.TryGetQueryStringValue "waitForMs"
                |> Option.bind (
                    fun waitForMsStr ->
                        match UInt32.TryParse waitForMsStr with
                        | true, waitForMs ->
                            TimeSpan.FromMilliseconds (float waitForMs) |> Some
                        | false, _ ->
                            None
                    )
                |> Option.defaultWith (fun _ -> TimeSpan.FromSeconds 10.0)

            task {
                match actOrConstructAndWaitOnLifeEventResultEncoder maybeProjectionName with
                | Ok (encoder, projection) ->
                    match! readJsonBodyAndValidate httpContext actAndWaitDecoder (fun actAndWait -> actAndWait.AsUntyped) with
                    | Ok actWait ->
                        applyActTelemetryRules httpContext actWait.Action
                        match! withinRateLimits httpContext (RateLimitEvent.Act actWait.Action) with
                        | true ->
                            let grainConnector = makeGrainConnector httpContext
                            match! grainConnector.ActAndWait lifeCycleDef idStr actWait.Action actWait.LifeEvent timeout with
                            | Ok subjAndLifeEvent ->
                                return!
                                    authorizeSubjectAndContinueWith
                                        httpContext
                                        (AccessEvent.Read(subjAndLifeEvent.VersionedSubject.Subject, projection))
                                        HttpStatusCode.ResetContent
                                        (SessionSource.Handle grainConnector.SessionHandle)
                                        (fun () -> respondJson encoder (subjAndLifeEvent.ToApi()) httpContext)
                            | Error err ->
                                return! respondGrainTransitionError err httpContext
                        | false ->
                            httpContext.SetStatusCode (int HttpStatusCode.TooManyRequests)
                            return! earlyReturn httpContext
                    | Error err ->
                        httpContext.SetStatusCode (int HttpStatusCode.BadRequest)
                        return! httpContext.WriteStringAsync err
                | Error () ->
                    httpContext.SetStatusCode (int HttpStatusCode.NotFound)
                    return! httpContext.WriteStringAsync "Projection Not Defined"
            }

    let actMaybeConstructHandler (idStr: string) : HttpHandler =
        fun _ httpContext ->
            let maybeProjectionName = tryGetProjectionName httpContext
            task {
                match subjectEncoder maybeProjectionName with
                | Ok (encoder, projection) ->
                    match! readJsonBodyAndValidate httpContext actMaybeConstructDecoder (fun actMaybeConstruct -> actMaybeConstruct.AsUntyped) with
                    | Ok actMaybeConstruct ->
                        applyActTelemetryRules httpContext actMaybeConstruct.Action
                        match! withinRateLimits httpContext (RateLimitEvent.Act actMaybeConstruct.Action) with
                        | true ->
                            let grainConnector = makeGrainConnector httpContext
                            match! grainConnector.ActMaybeConstruct lifeCycleDef idStr actMaybeConstruct.Action actMaybeConstruct.Constructor with
                            | Ok versionedSubject ->
                                return!
                                    authorizeSubjectAndContinueWith
                                        httpContext
                                        (AccessEvent.Read(versionedSubject.Subject, projection))
                                        HttpStatusCode.ResetContent
                                        (SessionSource.Handle grainConnector.SessionHandle)
                                        (fun () -> respondJson encoder versionedSubject.Subject httpContext)
                            | Error err ->
                                return! respondGrainOperationError err httpContext
                        | false ->
                            httpContext.SetStatusCode (int HttpStatusCode.TooManyRequests)
                            return! earlyReturn httpContext
                    | Error err ->
                        httpContext.SetStatusCode (int HttpStatusCode.BadRequest)
                        return! httpContext.WriteStringAsync err
                | Error () ->
                    httpContext.SetStatusCode (int HttpStatusCode.NotFound)
                    return! httpContext.WriteStringAsync "Projection Not Defined"
            }

    let actMaybeConstructAndWaitHandler (idStr: string) : HttpHandler =
        fun _ httpContext ->
            let maybeProjectionName = tryGetProjectionName httpContext
            let timeout =
                httpContext.TryGetQueryStringValue "waitForMs"
                |> Option.bind (
                    fun waitForMsStr ->
                        match UInt32.TryParse waitForMsStr with
                        | true, waitForMs ->
                            TimeSpan.FromMilliseconds (float waitForMs) |> Some
                        | false, _ ->
                            None
                    )
                |> Option.defaultWith (fun _ -> TimeSpan.FromSeconds 10.0)

            task {
                match actOrConstructAndWaitOnLifeEventResultEncoder maybeProjectionName with
                | Ok (encoder, projection) ->
                    match! readJsonBodyAndValidate httpContext actMaybeConstructAndWaitDecoder (fun actConstructWait -> actConstructWait.AsUntyped) with
                    | Ok actMaybeConstructAndWait ->
                        applyActTelemetryRules httpContext actMaybeConstructAndWait.Action
                        match! withinRateLimits httpContext (RateLimitEvent.Act actMaybeConstructAndWait.Action) with
                        | true ->
                            let grainConnector = makeGrainConnector httpContext
                            match! grainConnector.ActMaybeConstructAndWait lifeCycleDef idStr actMaybeConstructAndWait.Action actMaybeConstructAndWait.Constructor actMaybeConstructAndWait.LifeEvent timeout with
                            | Ok subjAndLifeEvent ->
                                return!
                                    authorizeSubjectAndContinueWith
                                        httpContext
                                        (AccessEvent.Read(subjAndLifeEvent.VersionedSubject.Subject, projection))
                                        HttpStatusCode.ResetContent
                                        (SessionSource.Handle grainConnector.SessionHandle)
                                        (fun () -> respondJson encoder (subjAndLifeEvent.ToApi()) httpContext)
                            | Error err ->
                                return! respondGrainOperationError err httpContext
                        | false ->
                            httpContext.SetStatusCode (int HttpStatusCode.TooManyRequests)
                            return! earlyReturn httpContext
                    | Error err ->
                        httpContext.SetStatusCode (int HttpStatusCode.BadRequest)
                        return! httpContext.WriteStringAsync err
                | Error () ->
                    httpContext.SetStatusCode (int HttpStatusCode.NotFound)
                    return! httpContext.WriteStringAsync "Projection Not Defined"
            }

    let constructAndWaitHandler : HttpHandler =
        fun _ (httpContext: HttpContext) ->
            let maybeProjectionName = tryGetProjectionName httpContext
            let callOrigin = Session.createCallOriginFromHttpContext httpContext
            let timeout =
                httpContext.TryGetQueryStringValue "waitForMs"
                |> Option.bind (
                    fun waitForMsStr ->
                        match UInt32.TryParse waitForMsStr with
                        | true, waitForMs ->
                            TimeSpan.FromMilliseconds (float waitForMs) |> Some
                        | false, _ ->
                            None
                    )
                |> Option.defaultWith (fun _ -> TimeSpan.FromSeconds 10.0)

            task {
                match actOrConstructAndWaitOnLifeEventResultEncoder maybeProjectionName with
                | Ok (encoder, projection) ->
                    match! readJsonBodyAndValidate httpContext constructAndWaitDecoder (fun constructAndWait -> constructAndWait.AsUntyped) with
                    | Ok constructAndWait ->
                        applyConstructTelemetryRules httpContext constructAndWait.Constructor
                        match! withinRateLimits httpContext (RateLimitEvent.Construct constructAndWait.Constructor) with
                        | true ->
                            let (IdGenerationResult idGenTask) = lifeCycleAdapter.LifeCycle.GenerateId callOrigin httpContext.RequestServices constructAndWait.Constructor
                            match! idGenTask with
                            | Ok subjId ->
                                let grainConnector = makeGrainConnector httpContext
                                match! grainConnector.ConstructAndWait lifeCycleDef subjId constructAndWait.Constructor constructAndWait.LifeEvent timeout with
                                | Ok subjAndLifeEvent ->
                                    return!
                                        authorizeSubjectAndContinueWith
                                            httpContext
                                            (AccessEvent.Read(subjAndLifeEvent.VersionedSubject.Subject, projection))
                                            HttpStatusCode.ResetContent
                                            (SessionSource.Handle grainConnector.SessionHandle)
                                            (fun () -> respondJson encoder (subjAndLifeEvent.ToApi()) httpContext)
                                | Error grainConstructionError ->
                                    return! respondGrainConstructionError grainConstructionError httpContext
                            | Error err ->
                                httpContext.SetStatusCode (int HttpStatusCode.UnprocessableEntity)
                                return! respondJson opErrorEncoder err httpContext
                        | false ->
                            httpContext.SetStatusCode (int HttpStatusCode.TooManyRequests)
                            return! earlyReturn httpContext
                    | Error err ->
                        httpContext.SetStatusCode (int HttpStatusCode.BadRequest)
                        return! httpContext.WriteStringAsync err
                | Error () ->
                    httpContext.SetStatusCode (int HttpStatusCode.NotFound)
                    return! httpContext.WriteStringAsync "Projection Not Defined"
            }

    let getByIdStrHandler (idStr: string) : HttpHandler =
        fun _ httpContext ->
            let maybeProjectionName = tryGetProjectionName httpContext
            task {
                match subjectEncoder maybeProjectionName with
                | Ok (encoder, projection) ->
                    let subjectRepo  = httpContext.RequestServices.GetRequiredService<ISubjectRepo<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'SubjectIndex, 'OpError>>()

                    match! subjectRepo.GetByIdStr idStr with
                    | Some subj ->
                        let sessionHandle = Session.Http.getSessionHandle cryptographer ecosystem httpContext
                        return!
                            authorizeSubjectAndContinueWith
                                httpContext
                                (AccessEvent.Read(subj.Subject, projection))
                                HttpStatusCode.Forbidden
                                (SessionSource.Handle sessionHandle)
                                (fun () -> respondJson encoder subj.Subject httpContext)
                    | None ->
                        httpContext.SetStatusCode (int HttpStatusCode.NotFound)
                        return! httpContext.WriteStringAsync ""
                | Error () ->
                    httpContext.SetStatusCode (int HttpStatusCode.NotFound)
                    return! httpContext.WriteStringAsync "Projection Not Defined"
            }

    let getFromGrainByIdStrHandler (idStr: string) : HttpHandler =
        fun _ httpContext ->
            let maybeProjectionName = tryGetProjectionName httpContext
            task {
                match subjectEncoder maybeProjectionName with
                | Ok (encoder, projection) ->
                    let grainConnector = makeGrainConnector httpContext

                    match! grainConnector.GetSubjectFromGrain lifeCycleDef idStr with
                    | Ok (Some versionedSubject) ->
                        return!
                            authorizeSubjectAndContinueWith
                                httpContext
                                (AccessEvent.Read(versionedSubject.Subject, projection))
                                HttpStatusCode.Forbidden
                                (SessionSource.Handle grainConnector.SessionHandle)
                                (fun () -> respondJson encoder versionedSubject.Subject httpContext)

                    | Ok None ->
                        httpContext.SetStatusCode (int HttpStatusCode.NotFound)
                        return! httpContext.WriteStringAsync ""

                    | Error GrainGetError.AccessDenied ->
                        httpContext.SetStatusCode (int HttpStatusCode.Forbidden)
                        return! httpContext.WriteStringAsync ""

                | Error () ->
                    httpContext.SetStatusCode (int HttpStatusCode.NotFound)
                    return! httpContext.WriteStringAsync "Projection Not Defined"
            }

    let getHandler : HttpHandler =
        fun _ httpContext ->
            let maybeProjectionName = tryGetProjectionName httpContext
            task {
                match subjectEncoder maybeProjectionName with
                | Ok (encoder, projection) ->
                    match! readJsonBodyAndValidate httpContext idDecoder (fun id -> id :> SubjectId) with
                    | Ok id ->
                        let subjectRepo = httpContext.RequestServices.GetRequiredService<ISubjectRepo<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'SubjectIndex, 'OpError>>()
                        match! subjectRepo.GetById id with
                        | Some subj ->
                            let sessionHandle = Session.Http.getSessionHandle cryptographer ecosystem httpContext
                            return!
                                authorizeSubjectAndContinueWith
                                    httpContext
                                    (AccessEvent.Read(subj.Subject, projection))
                                    HttpStatusCode.Forbidden
                                    (SessionSource.Handle sessionHandle)
                                    (fun () -> respondJson encoder subj.Subject httpContext)
                        | None ->
                            httpContext.SetStatusCode (int HttpStatusCode.NotFound)
                            return! httpContext.WriteStringAsync ""
                    | Error err ->
                        httpContext.SetStatusCode (int HttpStatusCode.BadRequest)
                        return! httpContext.WriteStringAsync err
                | Error () ->
                    httpContext.SetStatusCode (int HttpStatusCode.NotFound)
                    return! httpContext.WriteStringAsync "Projection Not Defined"
            }

    let getManyHandler: HttpHandler =
        fun _ httpContext ->
            let maybeProjectionName = tryGetProjectionName httpContext
            let subjectRepo  = httpContext.RequestServices.GetRequiredService<ISubjectRepo<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'SubjectIndex, 'OpError>>()
            task {
                match accessControlledSubjectListEncoder maybeProjectionName with
                | Ok (encoder, accessEvent) ->
                    match! readJsonBodyAndValidate httpContext manyIdsDecoder (NonemptySet.map (fun id -> id :> SubjectId)) with
                    | Ok ids ->
                        let! subjects = subjectRepo.GetByIds ids.ToSet
                        let! accessControlledSubjects, _ = getAccessControlledSubjects httpContext accessEvent (subjects |> List.map VersionedSubject.subject) 0UL
                        return! respondJson encoder accessControlledSubjects httpContext
                    | Error err ->
                        httpContext.SetStatusCode (int HttpStatusCode.BadRequest)
                        return! httpContext.WriteStringAsync err
                | Error () ->
                    httpContext.SetStatusCode (int HttpStatusCode.NotFound)
                    return! httpContext.WriteStringAsync "Projection Not Defined"
            }

    let indexQueryHandler: HttpHandler =
        fun _ httpContext ->
            let maybeProjectionName = tryGetProjectionName httpContext
            let subjectRepo  = httpContext.RequestServices.GetRequiredService<ISubjectRepo<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'SubjectIndex, 'OpError>>()
            task {
                match accessControlledSubjectListEncoder maybeProjectionName with
                | Ok (encoder, accessEvent) ->
                    match! readJsonBodyAndValidate httpContext indexQueryDecoder id with
                    | Ok indexQuery ->
                        let! subjects = subjectRepo.FilterFetchSubjects indexQuery
                        let! accessControlledSubjects, _ = getAccessControlledSubjects httpContext accessEvent (subjects |> List.map VersionedSubject.subject) 0UL
                        return! respondJson encoder accessControlledSubjects httpContext

                    | Error err ->
                        httpContext.SetStatusCode (int HttpStatusCode.BadRequest)
                        return! httpContext.WriteStringAsync err
                | Error () ->
                    httpContext.SetStatusCode (int HttpStatusCode.NotFound)
                    return! httpContext.WriteStringAsync "Projection Not Defined"
            }

    let indexQueryWithTotalCountHandler: HttpHandler =
        fun _ httpContext ->
            let maybeProjectionName = tryGetProjectionName httpContext
            let subjectRepo  = httpContext.RequestServices.GetRequiredService<ISubjectRepo<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'SubjectIndex, 'OpError>>()
            task {
                match accessControlledSubjectListWithTotalCountEncoder maybeProjectionName with
                | Ok (encoder, accessEvent) ->
                    match! readJsonBodyAndValidate httpContext indexQueryDecoder id with
                    | Ok indexQuery ->
                        let! (subjects, totalCount) = subjectRepo.FilterFetchSubjectsWithTotalCount indexQuery
                        let! accessControlledSubjects, accessControlledTotalCount =
                            getAccessControlledSubjects httpContext accessEvent (subjects |> List.map VersionedSubject.subject) totalCount
                        return! respondJson encoder { Data = accessControlledSubjects; TotalCount = accessControlledTotalCount } httpContext

                    | Error err ->
                        httpContext.SetStatusCode (int HttpStatusCode.BadRequest)
                        return! httpContext.WriteStringAsync err
                | Error () ->
                    httpContext.SetStatusCode (int HttpStatusCode.NotFound)
                    return! httpContext.WriteStringAsync "Projection Not Defined"
            }

    let indexCountHandler: HttpHandler =
        fun _ httpContext ->
            let subjectRepo  = httpContext.RequestServices.GetRequiredService<ISubjectRepo<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'SubjectIndex, 'OpError>>()
            task {
                match! readJsonBodyAndValidate httpContext indexPredicateDecoder id with
                | Ok indexPredicate ->
                    let! res = subjectRepo.FilterCountSubjects indexPredicate
                    return! httpContext.WriteStringAsync (res.ToString())

                | Error err ->
                    httpContext.SetStatusCode (int HttpStatusCode.BadRequest)
                    return! httpContext.WriteStringAsync err
            }


    let auditTrailGenericHandler (idStr: string) (toPayload: SubjectAuditData<'LifeAction, 'Constructor> -> 'T) (encoder: Encoder<List<'T>>) : HttpHandler =
        fun _ httpContext ->
            let subjectRepo = httpContext.RequestServices.GetRequiredService<ISubjectRepo<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'SubjectIndex, 'OpError>>()
            task {
                let offset =
                    httpContext.TryGetQueryStringValue "offset"
                    |> Option.bind (
                        fun offset ->
                            match UInt64.TryParse offset with
                            | true, v -> Some v
                            | false, _ -> None)
                    |> Option.defaultValue 0UL

                let pageSize =
                    httpContext.TryGetQueryStringValue "pageSize"
                    |> Option.bind (
                        fun offset ->
                            match UInt16.TryParse offset with
                            | true, v -> Some v
                            | false, _ -> None)
                    |> Option.defaultValue 10us

                let page = { Offset = offset; Size = pageSize }

                // One potentially wasted operation to do a permission check
                match! subjectRepo.GetByIdStr idStr with
                | Some subj ->
                    let! auditTrail = subjectRepo.FetchAuditTrail idStr page
                    let sessionHandle = Session.Http.getSessionHandle cryptographer ecosystem httpContext
                    let untypedAuditTrail =
                        auditTrail
                        |> List.map toPayload
                    return!
                        authorizeSubjectAndContinueWith
                            httpContext
                            (AccessEvent.ReadHistory subj.Subject)
                            HttpStatusCode.Forbidden
                            (SessionSource.Handle sessionHandle)
                            (fun () -> respondJson encoder untypedAuditTrail httpContext)
                | None ->
                    httpContext.SetStatusCode (int HttpStatusCode.NotFound)
                    return! httpContext.WriteStringAsync ""
            }

    let auditTrailHandler (idStr: string) : HttpHandler =
        auditTrailGenericHandler
            idStr
            (fun auditTrailData -> {
                AsOf = auditTrailData.AsOf
                OperationStr =
                    match auditTrailData.Operation with
                    | Ok (SubjectAuditOperation.Act act) -> sprintf "%A" act
                    | Ok (SubjectAuditOperation.Construct ctor) -> sprintf "Create %A" ctor
                    | Error str -> sprintf "Error %s" str
                Version = auditTrailData.Version
                By = auditTrailData.By
            })
            auditTrailEncoder

    let auditTrailTypedHandler (idStr: string) : HttpHandler =
        auditTrailGenericHandler
            idStr
            id
            auditTrailTypedEncoder

    let auditSnapshotHandler (idStr: string) (version: uint64) : HttpHandler =
        fun _ httpContext ->
            let subjectRepo = httpContext.RequestServices.GetRequiredService<ISubjectRepo<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'SubjectIndex, 'OpError>>()
            task {
                // One potentially wasted operation to do a permission check
                let! maybeSubject = subjectRepo.GetByIdStr idStr
                let! maybeSnapshot = subjectRepo.GetVersionSnapshotByIdStr idStr (GetSnapshotOfVersion.Specific version)
                match maybeSubject, maybeSnapshot with
                | Some subj, Some snapshot ->
                    let sessionHandle = Session.Http.getSessionHandle cryptographer ecosystem httpContext
                    return!
                        authorizeSubjectAndContinueWith
                            httpContext
                            (AccessEvent.ReadHistory subj.Subject)
                            HttpStatusCode.Forbidden
                            (SessionSource.Handle sessionHandle)
                            (fun () -> respondJson snapshotEncoder snapshot httpContext)
                | None, _
                | _, None ->
                    httpContext.SetStatusCode (int HttpStatusCode.NotFound)
                    return! httpContext.WriteStringAsync ""
            }

    let allHandler: HttpHandler =
        fun _ httpContext ->
            let maybeProjectionName = tryGetProjectionName httpContext
            let subjectRepo  = httpContext.RequestServices.GetRequiredService<ISubjectRepo<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'SubjectIndex, 'OpError>>()
            task {
                match accessControlledSubjectListEncoder maybeProjectionName with
                | Ok (encoder, accessEvent) ->
                    match! readJsonBodyAndValidate httpContext resultSetOptionsDecoder id with
                    | Ok resultSetOptions ->
                        let! subjects = subjectRepo.FetchAllSubjects resultSetOptions
                        let! accessControlledSubjects, _ = getAccessControlledSubjects httpContext accessEvent (subjects |> List.map VersionedSubject.subject) 0UL
                        return! respondJson encoder  accessControlledSubjects httpContext

                    | Error err ->
                        httpContext.SetStatusCode (int HttpStatusCode.BadRequest)
                        return! httpContext.WriteStringAsync err
                | Error () ->
                    httpContext.SetStatusCode (int HttpStatusCode.NotFound)
                    return! httpContext.WriteStringAsync "Projection Not Defined"
            }

    let allWithTotalCountHandler: HttpHandler =
        fun _ httpContext ->
            let maybeProjectionName = tryGetProjectionName httpContext
            let subjectRepo  = httpContext.RequestServices.GetRequiredService<ISubjectRepo<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'SubjectIndex, 'OpError>>()
            task {
                match accessControlledSubjectListWithTotalCountEncoder maybeProjectionName with
                | Ok (encoder, accessEvent) ->
                    match! readJsonBodyAndValidate httpContext resultSetOptionsDecoder id with
                    | Ok resultSetOptions ->
                        let! (subjects, totalCount) = subjectRepo.FetchAllSubjectsWithTotalCount resultSetOptions
                        let! accessControlledSubjects, accessControlledTotalCount =
                            getAccessControlledSubjects httpContext accessEvent (subjects |> List.map VersionedSubject.subject) totalCount
                        return! respondJson encoder { Data = accessControlledSubjects; TotalCount = accessControlledTotalCount } httpContext

                    | Error err ->
                        httpContext.SetStatusCode (int HttpStatusCode.BadRequest)
                        return! httpContext.WriteStringAsync err
                | Error () ->
                    httpContext.SetStatusCode (int HttpStatusCode.NotFound)
                    return! httpContext.WriteStringAsync "Projection Not Defined"
            }

    let allHandlerGet: HttpHandler =
        fun _ httpContext ->
            let maybeProjectionName = tryGetProjectionName httpContext
            let subjectRepo  = httpContext.RequestServices.GetRequiredService<ISubjectRepo<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'SubjectIndex, 'OpError>>()
            task {
                match accessControlledSubjectListEncoder maybeProjectionName with
                | Ok (encoder, accessEvent) ->
                    let! subjects = subjectRepo.FetchAllSubjects (ResultSetOptions<'SubjectIndex>.OrderByFastestWithPage ({ Size = 10000us; Offset = 0uL }))
                    let! accessControlledSubjects, _ = getAccessControlledSubjects httpContext accessEvent (subjects |> List.map VersionedSubject.subject) 0UL
                    return! respondJson encoder accessControlledSubjects httpContext
                | Error () ->
                    httpContext.SetStatusCode (int HttpStatusCode.NotFound)
                    return! httpContext.WriteStringAsync "Projection Not Defined"
            }

    let blobHandlerGet (idStr: string, blobGuidStr: string) : HttpHandler =
        fun _ httpContext ->
            let blobRepo  = httpContext.RequestServices.GetRequiredService<IBlobRepo>()
            task {
                // TODO: add TryFromTinyUuid, requires Convert.TryFromBase64String (requires .net5 )
                let subjectRepo  = httpContext.RequestServices.GetRequiredService<ISubjectRepo<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'SubjectIndex, 'OpError>>()

                match! subjectRepo.GetByIdStr idStr with
                | Some subj ->
                    let blobGuid = Guid.FromTinyUuid blobGuidStr
                    let sessionHandle = Session.Http.getSessionHandle cryptographer ecosystem httpContext
                    return!
                        authorizeSubjectAndContinueWith
                            httpContext
                            (AccessEvent.ReadBlob (subj.Subject, blobGuid))
                            HttpStatusCode.Forbidden
                            (SessionSource.Handle sessionHandle)
                            (fun () ->
                                task {
                                    let lcKey = lifeCycleDef.Key
                                    match! blobRepo.GetBlobData lcKey.EcosystemName { LifeCycleName = lcKey.LocalLifeCycleName; SubjectIdStr = idStr } blobGuid with
                                    | Some blobData ->
                                        match blobData.MimeType with
                                        | Some mimeType ->
                                            httpContext.SetHttpHeader ("Content-Type", mimeType.Value)
                                        | None ->
                                            ()

                                        match httpContext.TryGetQueryStringValue "download" with
                                        | Some "1" ->
                                            httpContext.SetHttpHeader ("Content-Disposition", "attachment")
                                        | _ ->
                                            ()

                                        return! httpContext.WriteBytesAsync blobData.Data
                                    | None ->
                                        httpContext.SetStatusCode (int HttpStatusCode.NotFound)
                                        return! httpContext.WriteStringAsync ""
                                }
                            )

                | None ->
                    httpContext.SetStatusCode (int HttpStatusCode.NotFound)
                    return! httpContext.WriteStringAsync ""
            }

    let permanentFailuresHandlerGet (scope: UpdatePermanentFailuresScope) : HttpHandler =
        fun _ httpContext ->
            task {
                // authorize if has access to Meta
                let metaRepo  = httpContext.RequestServices.GetRequiredService<ISubjectRepo<Meta, MetaAction, MetaConstructor, MetaId, MetaIndex, MetaOpError>>()
                let metaAdapter = httpContext.RequestServices.GetRequiredService<HostedLifeCycleAdapter<Meta, MetaAction, MetaOpError, MetaConstructor, MetaLifeEvent, MetaId>>()

                match! metaRepo.GetByIdStr lifeCycleDef.Key.LocalLifeCycleName with
                | Some meta ->
                    let sessionHandle = Session.Http.getSessionHandle cryptographer ecosystem httpContext
                    return!
                        metaAdapter.LifeCycle.Invoke
                            { new FullyTypedLifeCycleFunction<_, _, _, _, _, _, _> with
                                member _.Invoke lifeCycle =
                                    Http.authorizeSubjectAndContinueWith
                                        lifeCycle.MaybeApiAccess
                                        lifeCycle.SessionHandling
                                        (SessionSource.Handle sessionHandle)
                                        httpContext
                                        (AccessEvent.Read (meta.Subject, SubjectProjection.OriginalProjection))
                                        HttpStatusCode.Forbidden
                                        (fun () ->
                                            task {
                                                let subjectRepo  = httpContext.RequestServices.GetRequiredService<ISubjectRepo<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'SubjectIndex, 'OpError>>()
                                                let! failures = subjectRepo.GetSideEffectPermanentFailures scope
                                                return! respondJson sideEffectPermanentFailuresListEncoder failures httpContext
                                            })
                                        None
                            }
                | None ->
                    httpContext.SetStatusCode (int HttpStatusCode.NotFound)
                    return! httpContext.WriteStringAsync ""
            }

    subRoute
        (sprintf "/%s" lifeCycleDef.Key.LocalLifeCycleName)
        (
            if isSessionSubject then
                // For session XHRs, we still want to perform revalidation, but ignore any errors so that the client still receives the consequent session change.
                // This approach is applied in the real-time stream as well.
                let sessionInitHandler =
                    lifeCycleAdapter.LifeCycle.Invoke
                        { new FullyTypedLifeCycleFunction<_, _, _, _, _, _, _> with
                            member _.Invoke lifeCycle =
                                Session.Http.sessionRevalidationHandler hostEcosystemGrainFactory grainPartition clock cryptographer ecosystem lifeCycle.SessionHandler true }

                POST >=> choose [
                    route  "/getMaybeConstruct" >=>       (         sessionInitHandler >=> sessionGetMaybeConstructHandler)
                    routef "/act/%s"                      (fun v -> sessionInitHandler >=> sessionActHandler v)
                    routef "/actAndWait/%s"               (fun v -> sessionInitHandler >=> sessionActAndWaitHandler v)
                    routef "/actMaybeConstruct/%s"        (fun v -> sessionInitHandler >=> sessionActMaybeConstructHandler v)
                    routef "/actMaybeConstructAndWait/%s" (fun v -> sessionInitHandler >=> sessionActMaybeConstructAndWaitHandler v)
                ]
            else
                // For non-session XHRs, we perform revalidation and ensure any errors result in a failed request.
                let nonSessionInitHandler =
                    lifeCycleAdapter.LifeCycle.Invoke
                        { new FullyTypedLifeCycleFunction<_, _, _, _, _, _, _> with
                            member _.Invoke lifeCycle =
                                Session.Http.sessionRevalidationHandler hostEcosystemGrainFactory grainPartition clock cryptographer ecosystem lifeCycle.SessionHandler false }

                choose
                    [
                        GET >=> choose [
                            // these routes are pretty useful for debugging, since you can't issue a POST directly from the browser
                            route  "/debug/all" >=>                  (                              nonSessionInitHandler >=> allHandlerGet)
                            routef "/debug/get/%s"                   (fun subjId ->                 nonSessionInitHandler >=> getByIdStrHandler subjId)
                            routef "/debug/getFromGrain/%s"          (fun subjId ->                 nonSessionInitHandler >=> getFromGrainByIdStrHandler subjId)
                            routef "/debug/failures/seqNum/%s/%s"    (fun (subjId, seqNum) ->       nonSessionInitHandler >=> permanentFailuresHandlerGet ((subjId, UInt64.Parse seqNum) |> UpdatePermanentFailuresScope.SeqNum))
                            routef "/debug/failures/nextSeqBatch/%i" (fun batchSize ->              nonSessionInitHandler >=> permanentFailuresHandlerGet (batchSize |> uint8 |> UpdatePermanentFailuresScope.NextSeqBatch))
                            routef "/debug/failures/subject/%s"      (fun subjId ->                 nonSessionInitHandler >=> permanentFailuresHandlerGet (subjId |> UpdatePermanentFailuresScope.Subject))
                            routef "/debug/failures/single/%s/%s"    (fun (subjId, sideEffectId) -> nonSessionInitHandler >=> permanentFailuresHandlerGet ((subjId, Guid.Parse sideEffectId) |> UpdatePermanentFailuresScope.Single))

                            routef "/audit/%s" (fun subjId ->               nonSessionInitHandler >=> auditTrailHandler subjId)
                            routef "/auditTyped/%s" (fun subjId ->          nonSessionInitHandler >=> auditTrailTypedHandler subjId)
                            routef "/audit/%s/%s" (fun (subjId, version) -> nonSessionInitHandler >=> auditSnapshotHandler subjId (UInt64.Parse version))
                            routef "/blob/%s/%s" (fun (subjId, blobId) ->   nonSessionInitHandler >=> blobHandlerGet (subjId, blobId))
                        ]
                        PUT >=> choose [
                            route "/" >=>              (nonSessionInitHandler >=> constructHandler)
                            route "/constructWait" >=> (nonSessionInitHandler >=> constructAndWaitHandler)
                        ]
                        POST >=> choose [
                            routef "/act/%s"                      (fun subjId -> nonSessionInitHandler >=> actHandler subjId)
                            routef "/actAndWait/%s"               (fun subjId -> nonSessionInitHandler >=> actAndWaitHandler subjId)
                            routef "/actMaybeConstruct/%s"        (fun subjId -> nonSessionInitHandler >=> actMaybeConstructHandler subjId)
                            routef "/actMaybeConstructAndWait/%s" (fun subjId -> nonSessionInitHandler >=> actMaybeConstructAndWaitHandler subjId)

                            // XmlHttpRequest does not support bodies in GET requests,
                            // so these become POST for now. Later we may decide to implement
                            // a custom HTTP verb.
                            route  "/getMany" >=>             (nonSessionInitHandler >=> getManyHandler)
                            route  "/get" >=>                 (nonSessionInitHandler >=> getHandler)
                            route  "/getMaybeConstruct" >=>   (nonSessionInitHandler >=> getMaybeConstructHandler)
                            route  "/index" >=>               (nonSessionInitHandler >=> indexQueryHandler)
                            route  "/indexWithTotalCount" >=> (nonSessionInitHandler >=> indexQueryWithTotalCountHandler)
                            route  "/index/count" >=>         (nonSessionInitHandler >=> indexCountHandler)
                            route  "/all" >=>                 (nonSessionInitHandler >=> allHandler)
                            route  "/allWithTotalCount" >=>   (nonSessionInitHandler >=> allWithTotalCountHandler)
                        ]
                    ]
        )
