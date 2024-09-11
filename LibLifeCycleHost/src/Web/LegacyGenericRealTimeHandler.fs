module internal LibLifeCycleHost.Web.LegacyGenericRealTimeHandler

// TODO: delete this once all clients are migrated to work against the V1 API

open System
open System.Collections.Concurrent
open System.Reactive.Linq
open System.Reflection
open System.Threading
open System.Threading.Tasks
open Fable.SignalR
open FSharp.Control
open LibLifeCycleHost.AccessControl
open Microsoft.AspNetCore.SignalR
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.FSharp.Core
open Orleans
open LibLifeCycle
open LibLifeCycleTypes.LegacyRealTime
open LibLifeCycleHost.Web.LegacyJsonEncoding
open LibLifeCycleHost.Web.RealTimeSubjectData
open LibLifeCycleHost.Web.WebApiJsonEncoding
open LibLifeCycleHost
open LibLifeCycleCore

exception private UnsupportedLifeCycleException of lifeCycleName: string
    with override this.Message = sprintf "Life cycle named '%s' is not supported" this.lifeCycleName

// We need an interface type specific to this API version so as to avoid DI clashes with other API versions.
type private ILegacyRealTimeSubjectData<'Session, 'Subject> =
    inherit IRealTimeSubjectData<'Session, 'Subject>

type private RealTimeSubjectData<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId, 'Session
            when 'Subject              :> Subject<'SubjectId>
            and  'LifeAction           :> LifeAction
            and  'OpError              :> OpError
            and  'Constructor          :> Constructor
            and  'LifeEvent            :> LifeEvent
            and  'LifeEvent            : comparison
            and  'SubjectId            :> SubjectId
            and  'SubjectId            : comparison>
        (
            // Taking IServiceProvider is an anti-pattern, but we have little choice. GrainPartition is a scoped service and Fable.SignalR does not
            // support method-level DI.
            serviceProvider: IServiceProvider,
            hostEcosystemGrainFactory: IGrainFactory,
            lifeCycleAdapter: HostedLifeCycleAdapter<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>
        ) =

    static let wholeSubjectAccessControlledSubjectChangeEncoder = generateAutoEncoder<AccessControlledSubjectChange<'Subject, 'SubjectId>>

    let accessControlledSubjectionProjectionChangeEncoders = buildAccessControlledSubjectProjectionChangeEncoders lifeCycleAdapter

    static let observablesBySubjectId = ConcurrentDictionary<string, IObservable<SubjectChange<'Subject, 'SubjectId>>>()

    let getOrCreateObservableForSubject
            (logger: IFsLogger)
            (idStr: string): IObservable<SubjectChange<'Subject, 'SubjectId>> =
        observablesBySubjectId.GetOrAdd(
            idStr,
            (fun idStr ->
                logger.Trace
                    "No subject observable in cache for life cycle %a, subject ID %a, so creating it"
                    (logger.P "LifeCycleName")
                    lifeCycleAdapter.LifeCycle.Def.LifeCycleKey.LocalLifeCycleName
                    (logger.P "SubjectId")
                    idStr

                let subjectChangeObservable =
                    RealTime.createObservableForSubject<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>
                        logger
                        serviceProvider
                        (fun _ -> hostEcosystemGrainFactory |> Task.FromResult)
                        idStr
                        (Observable.Never<unit>())
                let autoCleanUpSubjectObservable =
                    subjectChangeObservable
                        // We replay so that new subscribers receive the last known state of the subject immediately.
                        .Replay(1)
                        .OnConnectableDisconnected(fun () ->
                            logger.Trace
                                "No more observers are active against subject observable for life cycle %a, subject ID %a, so removing it from the cache"
                                (logger.P "LifeCycleName")
                                lifeCycleAdapter.LifeCycle.Def.LifeCycleKey.LocalLifeCycleName
                                (logger.P "SubjectId")
                                idStr
                            observablesBySubjectId.TryRemove(idStr) |> ignore)
                        .RefCount()
                autoCleanUpSubjectObservable)
        )

    let getAccessControlledSubjectChange
            (maybeProjectionName: Option<string>)
            (maybeSession: Option<'Session>)
            (callOrigin: CallOrigin)
            (subjectChange: SubjectChange<'Subject, 'SubjectId>): Task<AccessControlledSubjectChange<'Subject, 'SubjectId>> =
        match subjectChange with
        | SubjectChange.Updated versionedSubject ->
            backgroundTask {
                let grainPartition = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<GrainPartition>()

                let! accessControlledSubject =
                    lifeCycleAdapter.LifeCycle.Invoke
                        { new FullyTypedLifeCycleFunction<_, _, _, _, _, _, _> with
                            member _.Invoke lifeCycle =
                                AccessControl.Subjects.getAccessControlledSubjectForRead
                                    hostEcosystemGrainFactory
                                    grainPartition
                                    lifeCycle.MaybeApiAccess
                                    lifeCycle.SessionHandling
                                    (AccessControl.SessionSource.InMemory (maybeSession |> Option.map box))
                                    callOrigin
                                    versionedSubject.Subject
                                    (maybeProjectionName |> Option.map Projection |> Option.defaultValue OriginalProjection)
                                    None
                        }
                let accessControlledSubjectChange =
                    match accessControlledSubject with
                    | Granted _ ->
                        {
                            Subject = versionedSubject.Subject
                            UpdatedOn = versionedSubject.AsOf
                        }
                        |> LibLifeCycleTypes.LegacyRealTime.SubjectChange.Updated
                        |> AccessControlled.Granted
                    | Denied subjectId -> AccessControlled.Denied subjectId
                return accessControlledSubjectChange
            }
        | SubjectChange.NotInitialized ->
            LibLifeCycleTypes.LegacyRealTime.SubjectChange.NotInitialized |> Granted |> Task.FromResult

    let toApiSubjectChange (subjectChange: SubjectChange<'Subject, 'SubjectId>): LibLifeCycleTypes.LegacyRealTime.SubjectChange<'Subject, 'SubjectId> =
        match subjectChange with
        | SubjectChange.Updated subjectUpdate ->
            {
                Subject = subjectUpdate.Subject
                UpdatedOn = subjectUpdate.AsOf
            }
            |> LibLifeCycleTypes.LegacyRealTime.SubjectChange.Updated
        | SubjectChange.NotInitialized -> LibLifeCycleTypes.LegacyRealTime.SubjectChange.NotInitialized

    let encodeAccessControlledSubjectChange (encoder: Encoder<AccessControlledSubjectChange<'Subject, 'SubjectId>>) accessControlledSubjectChange =
        accessControlledSubjectChange
        |> encoder
        |> Encode.toString

    member _.Observe
            (logger: IFsLogger)
            (maybeSessionWithIsValid: IObservable<Option<'Session> * bool>)
            (callOrigin: CallOrigin)
            (lifeCycleName: string)
            (maybeProjectionName: Option<string>)
            (idStr: string)
            (maybeCurrentVersion: Option<ComparableVersion>)
            (cancellationToken: CancellationToken): IObservable<string> =
        logger.Debug
            "Observing life cycle %a, subject ID %a"
            (logger.P "LifeCycleName") lifeCycleName
            (logger.P "SubjectId") idStr

        let maybeEncoder =
            match maybeProjectionName with
            | None ->
                wholeSubjectAccessControlledSubjectChangeEncoder |> Some
            | Some projectionName ->
                accessControlledSubjectionProjectionChangeEncoders.TryFind projectionName

        match maybeEncoder with
        | Some encoder ->
            // This part of the observable pipeline can be shared among any number of clients, so we create only one of those per subject instance.
            let sharedObservable = getOrCreateObservableForSubject logger idStr

            // This part of the pipeline deals with session revalidation and can be shared between any number of real-time streams from the same SignalR
            // hub (since the session is the same). It yields both the session itself and a Boolean indicating whether it is valid or not.
            let maybeSessionWithRevalidationStatus = maybeSessionWithIsValid

            // HACK: only required for legacy code. We're assuming any Some means we shouldn't send a current value, which is true for legacy.
            let sendCurrentValue =
                match maybeCurrentVersion with
                | None -> true
                | Some _ -> false

            // Honor the sendCurrentValue setting.
            let sharedObservable =
                sharedObservable
                    .Skip(if sendCurrentValue then 0 else 1)

            // Now we can construct the remainder of the pipeline.
            let clientSpecificObservable =
                match typeof<'Subject> = typeof<'Session> with
                | true ->
                    // Observing the session, so no need to do any authorization checks. And whilst we do revalidation as required, we don't let it
                    // influence the stream in any way. After all, if revalidation fails, the session subject will change to reflect that, and we need
                    // that changes to reach clients.
                    //
                    // The use of WithLatestFrom here ensures that changes to maybeSessionWithRevalidationStatus do not cause session values to be
                    // re-sent to clients, but still subscribes to maybeSessionWithRevalidationStatus to ensure revalidation occurs. We also seed that
                    // with a value to ensure we're never waiting for it.
                    Observable
                        .WithLatestFrom(
                            sharedObservable
                                .Select(fun subjectChange -> subjectChange |> toApiSubjectChange |> AccessControlled.Granted)
                                .Select(encodeAccessControlledSubjectChange encoder),
                            maybeSessionWithRevalidationStatus
                                .Select(fun _ -> ())
                                .StartWith(()),
                            fun encodedAccessControlledSubjectChange _ -> encodedAccessControlledSubjectChange
                        )
                | false ->
                    // Observing anything other than a session subject means we need to combine it with the latest session to perform authorization checks,
                    // as well as ensure session revalidation occurs as required. Unlike when observing the session itself, failure to revalidate means we
                    // cannot send the subject data to the client.
                    Observable
                        .CombineLatest(
                            sharedObservable,
                            maybeSessionWithRevalidationStatus,
                            fun subjectChange (maybeSession, isValid) ->
                                if isValid then
                                    Observable.FromAsync(fun () -> getAccessControlledSubjectChange maybeProjectionName maybeSession callOrigin subjectChange)
                                else
                                    Observable.Never<AccessControlledSubjectChange<'Subject, 'SubjectId>>()
                        )
                        .Concat()
                        .Select(encodeAccessControlledSubjectChange encoder)

            clientSpecificObservable
                .TakeUntilCancelled(cancellationToken)

        | None ->
            Observable.Throw(sprintf "Projection %A is not registered" maybeProjectionName |> invalidOp)

    interface ILegacyRealTimeSubjectData<'Session, 'Subject> with
        member this.Observe
                (logger: IFsLogger)
                (maybeSessionWithIsValid: IObservable<Option<'Session> * bool>)
                (callOrigin: CallOrigin)
                (lifeCycleName: string)
                (maybeProjectionName: Option<string>)
                (_hubConnectionId: string)
                (subjectId: string)
                (maybeCurrentVersion: Option<ComparableVersion>)
                (cancellationToken: CancellationToken): IObservable<string> =
            this.Observe logger maybeSessionWithIsValid callOrigin lifeCycleName maybeProjectionName subjectId maybeCurrentVersion cancellationToken

        member _.GetRawSubjectObservable
                (logger: IFsLogger)
                (idStr: string): IObservable<Option<'Subject>> =
            let sharedObservable = getOrCreateObservableForSubject logger idStr
            sharedObservable
                .Select(fun subjectChange ->
                    match subjectChange with
                    | SubjectChange.Updated subjectSnapshot -> Some subjectSnapshot.Subject
                    | SubjectChange.NotInitialized -> None)

let private streamToClientGeneric<'Subject, 'Session>
        (logger: IFsLogger)
        (ecosystem: Ecosystem)
        (maybeSessionHandler: Option<EcosystemSessionHandler<'Session>>)
        (msg: ClientStreamApi)
        (fableHub: FableHub<ClientApi, ServerApi>)
        (cancellationToken: CancellationToken): Collections.Generic.IAsyncEnumerable<ServerStreamApi> =
    let observeSubjectOrProjection lifeCycleName maybeProjection subjectIdStr maybeCurrentVersion =
        let httpContext = fableHub.Context.GetHttpContext()

        // We have to resolve this up-front from the HttpContext because once the initial stream setup request completes, the HttpContext will no longer
        // be accessible.
        let maybeEncryptedSessionHandle = Session.Http.getMaybeEncryptedSessionHandle ecosystem httpContext

        let serviceProvider = fableHub.Services
        let hostEcosystemGrainFactory = serviceProvider.GetRequiredService<IGrainFactory>()
        let grainPartition = serviceProvider.GetRequiredService<GrainPartition>()
        let clock = serviceProvider.GetRequiredService<Service<Clock>>()
        let cryptographer = serviceProvider.GetRequiredService<ApiSessionCryptographer>()
        let maybeSessionRealTimeSubjectData =
            if typeof<'Session> = typeof<NoSession> then
                None
            else
                serviceProvider.GetRequiredService<ILegacyRealTimeSubjectData<'Session, 'Session>>()
                :> IRealTimeSubjectData<'Session, 'Session>
                |> Some
        let maybeSessionWithIsValid =
            RealTime.MaybeSessionWithIsValidObservableCache<'Session>.GetOrCreate
                logger
                hostEcosystemGrainFactory
                grainPartition
                clock
                cryptographer
                maybeSessionHandler
                maybeSessionRealTimeSubjectData
                fableHub.Context.ConnectionId
                maybeEncryptedSessionHandle

        let serviceProvider = fableHub.Services
        let realTimeSubjectData = serviceProvider.GetRequiredService<ILegacyRealTimeSubjectData<'Session, 'Subject>>()
        let callOrigin = Session.createCallOriginFromHttpContext (fableHub.Context.GetHttpContext())

        (realTimeSubjectData.Observe logger maybeSessionWithIsValid callOrigin lifeCycleName maybeProjection fableHub.Context.ConnectionId subjectIdStr maybeCurrentVersion cancellationToken)
        |> AsyncSeq.ofObservableBuffered
        |> AsyncSeq.map ServerStreamApi.SubjectChanged
        |> AsyncSeq.toAsyncEnum

    match msg with
    | ClientStreamApi.ObserveSubject (lifeCycleName, subjectIdStr, sendCurrentValue) ->
        let maybeCurrentVersion =
            if sendCurrentValue then
                None
            else
                Some ComparableVersion.MaxValue
        observeSubjectOrProjection lifeCycleName None subjectIdStr maybeCurrentVersion
    | ClientStreamApi.ObserveSubjectProjection (lifeCycleName, projectionName, subjectIdStr, sendCurrentValue) ->
        let maybeCurrentVersion =
            if sendCurrentValue then
                None
            else
                Some ComparableVersion.MaxValue
        observeSubjectOrProjection lifeCycleName (Some projectionName) subjectIdStr maybeCurrentVersion

let internal legacyStreamToClient
        (ecosystem: Ecosystem)
        (msg: ClientStreamApi)
        (fableHub: FableHub<ClientApi, ServerApi>)
        (cancellationToken: CancellationToken) =
    match msg with
    | ClientStreamApi.ObserveSubject (lifeCycleName, _, _)
    | ClientStreamApi.ObserveSubjectProjection (lifeCycleName, _, _, _) ->
        let serviceProvider = fableHub.Services
        let lifeCycleAdapters = serviceProvider.GetRequiredService<HostedLifeCycleAdapterCollection>()
        let maybeLifeCycleAdapter = lifeCycleAdapters.GetLifeCycleAdapterByLocalName lifeCycleName

        match maybeLifeCycleAdapter with
        | Some lifeCycleAdapter ->
            let logger = fableHub.Services.GetRequiredService<ILoggerFactory>().CreateLogger("RealTime")
            let summarizers = fableHub.Services.GetRequiredService<ValueSummarizers>()
            let fsLogger = newRealTimeScopedLogger summarizers logger lifeCycleName fableHub.Context.ConnectionId

            lifeCycleAdapter.LifeCycle.Invoke
                { new FullyTypedLifeCycleFunction<_> with
                    member _.Invoke (lifeCycle: LifeCycle<'Subject, _, _, _, _, _, _, _, 'Session, _, _>) =
                        streamToClientGeneric<'Subject, 'Session> fsLogger ecosystem lifeCycle.SessionHandler msg fableHub cancellationToken }
        | None ->
            let exn = UnsupportedLifeCycleException lifeCycleName
            Observable.Throw<ServerStreamApi>(exn)
            |> AsyncSeq.ofObservableBuffered
            |> AsyncSeq.toAsyncEnum

let internal registerRealTimeSubjectData
        (services: IServiceCollection)
        (lifeCycleAdapter: IHostedLifeCycleAdapter) =
    lifeCycleAdapter.LifeCycle.Invoke
        { new FullyTypedLifeCycleFunction<_> with
            member _.Invoke (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, _, 'Env>) =
                services
                   .AddSingleton<RealTimeSubjectData<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId, 'Session>>()
                    .AddSingleton<ILegacyRealTimeSubjectData<'Session, 'Subject>>(fun serviceProvider -> serviceProvider.GetRequiredService<RealTimeSubjectData<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId, 'Session>>() :> ILegacyRealTimeSubjectData<'Session, 'Subject>) }
    |> ignore
