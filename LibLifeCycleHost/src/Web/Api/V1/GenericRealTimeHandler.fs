module internal LibLifeCycleHost.Web.Api.V1.GenericRealTimeHandler

open System
open System.Collections.Concurrent
open System.Reactive.Linq
open System.Threading
open System.Threading.Tasks
open Fable.SignalR
open FSharp.Control
open Microsoft.AspNetCore.SignalR
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.FSharp.Core
open Orleans
open LibLifeCycle
open LibLifeCycleHost.Web
open LibLifeCycleHost.Web.RealTimeSubjectData
open LibLifeCycleHost.Web.WebApiJsonEncoding
open LibLifeCycleHost.Web.Api.V1.JsonEncoding
open LibLifeCycleHost
open LibLifeCycleCore
open LibLifeCycleTypes.Api.V1.Shared
open LibLifeCycleTypes.Api.V1.RealTime

exception private UnsupportedLifeCycleException of lifeCycleName: string
    with override this.Message = $"Life cycle named '%s{this.lifeCycleName}' is not supported"

exception private ClientVersionAheadOfWebNodeVersion of Version: ComparableVersion * ClientVersion: ComparableVersion
    with override this.Message = $"Client has version {this.ClientVersion}, which is ahead of backend version {this.Version}, and the discrepancy could not be resolved"

// We need an interface type specific to this API version so as to avoid DI clashes with other API versions.
type private IV1RealTimeSubjectData<'Session, 'Subject> =
    inherit IRealTimeSubjectData<'Session, 'Subject>

type private SharedSubjectPipelineCacheKey = string

type private SharedSubjectPipelineCacheItem<'Subject> =
    {
        Pipeline: IObservable<ApiSubjectChange<'Subject>>
        ForceSync: IObserver<unit>
    }

type private SubjectPipelineCacheKey =
    {
        HubConnectionId: string
        SubjectId: string
        MaybeProjectionName: Option<string>
    }

type private SubjectPipelineCacheItem =
    {
        SubjectChange: IObservable<string>
        MaybeClientVersion: IObserver<Option<ComparableVersion>>
    }

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
            lifeCycleAdapter: IHostedOrReferencedLifeCycleAdapter<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>
        ) =


    static let wholeSubjectAccessControlledSubjectChangeEncoder = generateAutoEncoder<AccessControlledSubjectChange<'Subject, 'SubjectId>>

    let accessControlledSubjectProjectionChangeEncoders = buildAccessControlledSubjectChangeEncoders lifeCycleAdapter

    static let sharedSubjectPipelineCache = ConcurrentDictionary<SharedSubjectPipelineCacheKey, SharedSubjectPipelineCacheItem<'Subject>>()

    static let subjectPipelineCache = ConcurrentDictionary<SubjectPipelineCacheKey, SubjectPipelineCacheItem>()

    let toApiSubjectChange (subjectChange: LibLifeCycleCore.SubjectChange<'Subject, 'SubjectId>): ApiSubjectChange<'Subject> =
        match subjectChange with
        | SubjectChange.Updated subjectUpdate ->
            {
                Data = subjectUpdate.Subject
                Version = (subjectUpdate.AsOf.Ticks, subjectUpdate.Version)
            }
            |> ApiSubjectChange.Updated
        | SubjectChange.NotInitialized -> ApiSubjectChange.NotInitialized

    let getGrainFactory (serviceProvider: IServiceProvider): Task<IGrainFactory> =
        backgroundTask {
            let biosphereGrainProvider = serviceProvider.GetService<IBiosphereGrainProvider>()
            let! biosphereGrainProvider = biosphereGrainProvider.GetGrainFactory lifeCycleAdapter.ReferencedLifeCycle.Def.LifeCycleKey.EcosystemName
            return biosphereGrainProvider
        }

    let getOrCreateSharedSubjectPipelineCacheItem
            (logger: IFsLogger)
            (idStr: string): SharedSubjectPipelineCacheItem<'Subject> =
        let cacheKey = idStr

        sharedSubjectPipelineCache.GetOrAdd(
            cacheKey,
            (fun _cacheKey ->
                logger.Trace
                    "No subject observable in cache for life cycle %a, subject ID %a, so creating it"
                    (logger.P "LifeCycleName")
                    lifeCycleAdapter.ReferencedLifeCycle.Def.LifeCycleKey.LocalLifeCycleName
                    (logger.P "SubjectId")
                    idStr

                let forceSync = new System.Reactive.Subjects.Subject<unit>()

                let subjectChangeObservable =
                    RealTime.createObservableForSubject<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>
                        logger
                        serviceProvider
                        getGrainFactory
                        idStr
                        forceSync
                let apiSubjectChangeObservable =
                    subjectChangeObservable
                    |> Observable.map toApiSubjectChange
                let autoCleanUpSubjectObservable =
                    apiSubjectChangeObservable
                        // We replay so that new subscribers receive the last known state of the subject immediately.
                        .Replay(1)
                        .OnConnectableDisconnected(fun () ->
                            logger.Trace
                                "No more observers are active against subject observable for life cycle %a, subject ID %a, so removing it from the cache"
                                (logger.P "LifeCycleName")
                                lifeCycleAdapter.ReferencedLifeCycle.Def.LifeCycleKey.LocalLifeCycleName
                                (logger.P "SubjectId")
                                idStr
                            sharedSubjectPipelineCache.TryRemove(idStr) |> ignore)
                        // Allow a grace period between last observer and throwing away the pipeline. This is critical because the frontend will *replace*
                        // its current observer with another, which means the current observer is disposed and then the new one created to replace it.
                        // Without a grace period, this pipeline would be removed from the cache and the replacement would cause the pipeline to be
                        // recreated, thereby negating the benefit of the cache in the first place.
                        .RefCount(TimeSpan.FromSeconds(5))

                {
                    Pipeline = autoCleanUpSubjectObservable
                    ForceSync = forceSync
                }
            )
        )

    let getAccessControlledSubjectChange
            (maybeProjectionName: Option<string>)
            (maybeSession: Option<'Session>)
            (callOrigin: CallOrigin)
            (subjectChange: ApiSubjectChange<'Subject>): Task<AccessControlledSubjectChange<'Subject, 'SubjectId>> =
        match subjectChange with
        | ApiSubjectChange.Updated versionedData ->
            backgroundTask {
                let grainPartition = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<GrainPartition>()
                let hostEcosystemGrainFactory = serviceProvider.GetRequiredService<IGrainFactory>()

                let! accessControlledSubject =
                    lifeCycleAdapter.ReferencedLifeCycle.Invoke
                        { new FullyTypedReferencedLifeCycleFunction<_, _, _, _, _, _, _> with
                            member _.Invoke referencedLifeCycle =
                                AccessControl.Subjects.getAccessControlledSubjectForRead
                                    hostEcosystemGrainFactory
                                    grainPartition
                                    referencedLifeCycle.MaybeApiAccess
                                    referencedLifeCycle.SessionHandling
                                    (AccessControl.SessionSource.InMemory (maybeSession |> Option.map box))
                                    callOrigin
                                    versionedData.Data
                                    (maybeProjectionName |> Option.map Projection |> Option.defaultValue OriginalProjection)
                                    None
                        }
                let accessControlledSubjectChange =
                    match accessControlledSubject with
                    | Granted _ ->
                        versionedData
                        |> ApiSubjectChange.Updated
                        |> AccessControlled.Granted
                    | Denied subjectId -> AccessControlled.Denied subjectId
                return accessControlledSubjectChange
            }
        | ApiSubjectChange.NotInitialized ->
            ApiSubjectChange.NotInitialized |> Granted |> Task.FromResult

    let encodeAccessControlledSubjectChange (encoder: Encoder<AccessControlledSubjectChange<'Subject, 'SubjectId>>) accessControlledSubjectChange =
        accessControlledSubjectChange
        |> encoder
        |> Encode.toString

    let getOrCreateSubjectPipelineCacheItem
            (logger: IFsLogger)
            (maybeSessionWithIsValid: IObservable<Option<'Session> * bool>)
            (callOrigin: CallOrigin)
            (maybeProjectionName: Option<string>)
            (hubConnectionId: string)
            (subjectId: string) : SubjectPipelineCacheItem =
        let cacheKey =
            {
                HubConnectionId = hubConnectionId
                SubjectId = subjectId
                MaybeProjectionName = maybeProjectionName
            }

        subjectPipelineCache.GetOrAdd(
            cacheKey,
            (fun _cacheKey ->
                logger.Trace
                    "No cache item is in cache, so creating it for hub connection ID %a, subject ID %a"
                    (logger.P "HubConnectionId")
                    hubConnectionId
                    (logger.P "SubjectId")
                    subjectId

                let maybeClientVersion = new System.Reactive.Subjects.ReplaySubject<Option<ComparableVersion>>(1)

                let maybeEncoder =
                    match maybeProjectionName with
                    | None ->
                        wholeSubjectAccessControlledSubjectChangeEncoder |> Some
                    | Some projectionName ->
                        accessControlledSubjectProjectionChangeEncoders.TryFind projectionName

                let subjectChangePipeline =
                    match maybeEncoder with
                    | Some encoder ->
                        // This part of the observable pipeline can be shared among any number of clients, so we create only one of those per
                        // subject instance.
                        let sharedSubjectPipelineCacheItem = getOrCreateSharedSubjectPipelineCacheItem logger subjectId

                        let versionFilteredSharedObservable =
                            Observable
                                .Defer(
                                    fun () ->
                                        Observable
                                            .CombineLatest(
                                                sharedSubjectPipelineCacheItem.Pipeline,
                                                maybeClientVersion,
                                                fun subjectChange maybeClientVersion ->
                                                    match subjectChange with
                                                    | ApiSubjectChange.Updated versionedData ->
                                                        match maybeClientVersion, versionedData.Version with
                                                        | Some currentVersionHeldByClient, subjectChangeVersion ->
                                                            if currentVersionHeldByClient < subjectChangeVersion then
                                                                // Client's version is out of date.
                                                                Observable.Return(subjectChange)
                                                            elif currentVersionHeldByClient = subjectChangeVersion then
                                                                // Client already has this version.
                                                                Observable.Empty<ApiSubjectChange<'Subject>>()
                                                            else
                                                                // Client has a version ahead of this web node.
                                                                logger.Warn
                                                                    "Client version, %a, is ahead of web node version, %a"
                                                                    (logger.P "ClientVersion")
                                                                    currentVersionHeldByClient
                                                                    (logger.P "WebNodeVersion")
                                                                    subjectChangeVersion

                                                                Observable
                                                                    .Return(())
                                                                    // Tell the shared pipeline that we want to force a sync.
                                                                    .Do(fun _ -> sharedSubjectPipelineCacheItem.ForceSync.OnNext(()))
                                                                    // Wait a bit to allow the sync to occur (we can't know how long to wait, since it may not even
                                                                    // complete due to network issues)
                                                                    .Delay(TimeSpan.FromSeconds(2))
                                                                    // Now fail so that the outer retry is instigated. If we fail even after the retries,
                                                                    // the entire pipeline will fail and that's exactly what we want. This pipeline is
                                                                    // specific to the client claiming to be ahead of the web node, so it failing is
                                                                    // acceptable and desirable given the discrepancy.
                                                                    .Select(fun _ -> Observable.Throw<ApiSubjectChange<'Subject>>(ClientVersionAheadOfWebNodeVersion (subjectChangeVersion, currentVersionHeldByClient)))
                                                                    .Switch()
                                                        | None, _ ->
                                                            // Client doesn't have any version, so forward on whatever we have.
                                                            Observable.Return(subjectChange)
                                                    | ApiSubjectChange.NotInitialized ->
                                                        Observable.Return(subjectChange)
                                            )
                                            .Concat()
                                )
                                .Retry(5)

                        match typeof<'Subject> = typeof<'Session> with
                        | true ->
                            // Observing the session, so no need to do any authorization checks. And whilst we do revalidation as required,
                            // we don't let it influence the stream in any way. After all, if revalidation fails, the session subject will
                            // change to reflect that, and we need that changes to reach clients.
                            //
                            // The use of WithLatestFrom here ensures that changes to maybeSessionWithIsValid do not cause session values to
                            // be re-sent to clients, but still subscribes to maybeSessionWithIsValid to ensure revalidation occurs. We also
                            // seed that with a value to ensure we're never waiting for it.
                            Observable
                                .WithLatestFrom(
                                    versionFilteredSharedObservable,
                                    maybeSessionWithIsValid
                                        .Select(fst)
                                        .StartWith(None),
                                    fun subjectChange maybeSession ->
                                        Observable.FromAsync(fun () -> getAccessControlledSubjectChange maybeProjectionName maybeSession callOrigin subjectChange)
                                )
                                .Concat()
                                .DistinctUntilChanged()
                                .Select(encodeAccessControlledSubjectChange encoder)
                        | false ->
                            // Observing anything other than a session subject means we need to combine it with the latest session to perform
                            // authorization checks, as well as ensure session revalidation occurs as required. Unlike when observing the
                            // session itself, failure to revalidate means we cannot send the subject data to the client.
                            Observable
                                .Create(
                                    fun (observer: IObserver<AccessControlledSubjectChange<'Subject, 'SubjectId>>) ->
                                        // Only increments for versioned subject changes, not session changes. This is because session changes
                                        // do not necessarily necessitate the subject be re-sent to the client, whereas changes to the versioned
                                        // subject do. It might seem like changes to the latter could also be withheld if they have not changed,
                                        // but there are scenarios where data can be lost in transit so that the client will re-observe the stream
                                        // with an older version of the data than the server might have expected (because from the server's point
                                        // of view, it has already sent that version). Here is a concrete scenario:
                                        //
                                        // 1. Real-time stream established by client, server creates observable and caches it, sending through
                                        //    the latest subject.
                                        // 2. Before the client receives that data, it resets the stream because it has established a session
                                        //    (or perhaps the data just gets lost in transit due to the network cord being pulled or whatever)
                                        // 3. The client re-establishes the same stream, but this time the server can use the cached observable
                                        //    to service it.
                                        //
                                        // If that observable did not allow the already-sent value through again, the client would not be able
                                        // to brought up to date.
                                        let mutable changeCounter = 0

                                        Observable
                                            .CombineLatest(
                                                versionFilteredSharedObservable.Select(fun v -> (v, Interlocked.Increment(&changeCounter))),
                                                maybeSessionWithIsValid,
                                                fun (subjectChange, counter) (maybeSession, isValid) ->
                                                    if isValid then
                                                        Observable
                                                            .FromAsync(fun () -> getAccessControlledSubjectChange maybeProjectionName maybeSession callOrigin subjectChange)
                                                            .Select(fun v -> (v, counter))
                                                    else
                                                        Observable.Never<AccessControlledSubjectChange<'Subject, 'SubjectId> * int>()
                                            )
                                            .Concat()
                                            .DistinctUntilChanged()
                                            .Select(fst)
                                            .Subscribe(observer)
                                )
                                .Select(encodeAccessControlledSubjectChange encoder)

                    | None ->
                        Observable.Throw(sprintf "Projection %A is not registered" maybeProjectionName |> invalidOp)

                // Ensure we clean up when there are no more observers.
                let publishedSubjectChangePipeline =
                    subjectChangePipeline
                        .Publish()
                        .OnConnectableDisconnected(fun () ->
                            logger.Trace
                                "No more observers are active against the subject change observable, so removing it from the cache for hub connection ID %a, subject ID %a"
                                (logger.P "HubConnectionId")
                                hubConnectionId
                                (logger.P "SubjectId")
                                subjectId
                            subjectPipelineCache.TryRemove(cacheKey) |> ignore
                        )
                        .RefCount(TimeSpan.FromSeconds(5))

                {
                    SubjectChange = publishedSubjectChangePipeline
                    MaybeClientVersion = maybeClientVersion
                }
            )
        )

    member _.Observe
            (logger: IFsLogger)
            // This part of the pipeline deals with session revalidation and can be shared between any number of real-time streams from the same SignalR
            // hub connection (since the session is the same). It yields both the session itself and a Boolean indicating whether it is valid or not.
            (maybeSessionWithIsValid: IObservable<Option<'Session> * bool>)
            (callOrigin: CallOrigin)
            (lifeCycleName: string)
            (maybeProjectionName: Option<string>)
            (hubConnectionId: string)
            (subjectId: string)
            (maybeCurrentVersion: Option<ComparableVersion>)
            (cancellationToken: CancellationToken): IObservable<string> =
        logger.Debug
            "Observing life cycle %a, subject ID %a"
            (logger.P "LifeCycleName") lifeCycleName
            (logger.P "SubjectId") subjectId

        // First, get or create the pipeline along with an observer of the current version.
        let subjectPipelineCacheItem =
            getOrCreateSubjectPipelineCacheItem
                logger
                maybeSessionWithIsValid
                callOrigin
                maybeProjectionName
                hubConnectionId
                subjectId

        // Then, return the pipeline with cancellation semantics applied.
        Observable
            .Create(
                fun (observer: IObserver<string>) ->
                    // Ensure the observer is observing the pipeline first...
                    let subscription =
                        subjectPipelineCacheItem
                            .SubjectChange
                            .Subscribe(observer)

                    // ...then ensure the client's current version is made available to the cached pipeline.
                    subjectPipelineCacheItem.MaybeClientVersion.OnNext(maybeCurrentVersion)

                    subscription
            )
            .TakeUntilCancelled(cancellationToken)

    interface IV1RealTimeSubjectData<'Session, 'Subject> with
        member this.Observe
                (logger: IFsLogger)
                (maybeSessionWithIsValid: IObservable<Option<'Session> * bool>)
                (callOrigin: CallOrigin)
                (lifeCycleName: string)
                (maybeProjectionName: Option<string>)
                (hubConnectionId: string)
                (subjectId: string)
                (maybeClientVersion: Option<ComparableVersion>)
                (cancellationToken: CancellationToken): IObservable<string> =
            this.Observe logger maybeSessionWithIsValid callOrigin lifeCycleName maybeProjectionName hubConnectionId subjectId maybeClientVersion cancellationToken

        member _.GetRawSubjectObservable
                (logger: IFsLogger)
                (idStr: string): IObservable<Option<'Subject>> =
            let sharedSubjectPipelineCacheItem = getOrCreateSharedSubjectPipelineCacheItem logger idStr
            sharedSubjectPipelineCacheItem
                .Pipeline
                .Select(fun subjectChange ->
                    match subjectChange with
                    | ApiSubjectChange.Updated versionedData -> Some versionedData.Data
                    | ApiSubjectChange.NotInitialized -> None)

let private streamToClientGeneric<'Subject, 'Session>
        (logger: IFsLogger)
        (ecosystem: Ecosystem)
        (maybeSessionHandler: Option<EcosystemSessionHandler<'Session>>)
        (lifeCycleName: string)
        (subjectIdStr: string)
        (maybeProjectionName: Option<string>)
        (maybeClientVersion: Option<ComparableVersion>)
        (fableHub: FableHub<ClientApi, ServerApi>)
        (cancellationToken: CancellationToken): Collections.Generic.IAsyncEnumerable<ServerStreamApi> =
    let observeSubjectOrProjection lifeCycleName maybeProjection subjectIdStr maybeClientVersion =
        let httpContext = fableHub.Context.GetHttpContext()

        // We have to resolve this up-front from the HttpContext because once the initial stream setup request completes, the HttpContext will no longer
        // be accessible.
        let maybeEncryptedSessionHandle = Session.Http.getMaybeEncryptedSessionHandle ecosystem httpContext

        asyncSeq {
            let serviceProvider = fableHub.Services
            let hostEcosystemGrainFactory = serviceProvider.GetService<IGrainFactory>()

            let grainPartition = serviceProvider.GetRequiredService<GrainPartition>()
            let clock = serviceProvider.GetRequiredService<Service<Clock>>()
            let cryptographer = serviceProvider.GetRequiredService<ApiSessionCryptographer>()
            let maybeSessionRealTimeSubjectData =
                if typeof<'Session> = typeof<NoSession> then
                    None
                else
                    serviceProvider.GetService<IV1RealTimeSubjectData<'Session, 'Session>>()
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
            let realTimeSubjectData = serviceProvider.GetRequiredService<IV1RealTimeSubjectData<'Session, 'Subject>>()
            let callOrigin = Session.createCallOriginFromHttpContext (fableHub.Context.GetHttpContext())

            yield!
                (realTimeSubjectData.Observe logger maybeSessionWithIsValid callOrigin lifeCycleName maybeProjection fableHub.Context.ConnectionId subjectIdStr maybeClientVersion cancellationToken)
                |> AsyncSeq.ofObservableBuffered
                |> AsyncSeq.map ServerStreamApi.SubjectChanged
        }
        |> AsyncSeq.toAsyncEnum

    observeSubjectOrProjection lifeCycleName maybeProjectionName subjectIdStr maybeClientVersion

let internal v1StreamToClient
        (ecosystem: Ecosystem)
        (msg: ClientStreamApi)
        (fableHub: FableHub<ClientApi, ServerApi>)
        (cancellationToken: CancellationToken)
        : Collections.Generic.IAsyncEnumerable<ServerStreamApi> =
    match msg with
    | ClientStreamApi.ObserveSubjectV2 (ecosystemName, lifeCycleName, subjectIdStr, maybeProjectionName, maybeClientVersion) ->
        let maybeReferencedLifeCycle: Option<IReferencedLifeCycle> =
            let isNativeEcosystem = ecosystemName = ecosystem.Name

            if isNativeEcosystem then
                let serviceProvider = fableHub.Services
                let lifeCycleAdapters = serviceProvider.GetRequiredService<HostedLifeCycleAdapterCollection>()
                let maybeLifeCycleAdapter = lifeCycleAdapters.GetLifeCycleAdapterByLocalName lifeCycleName

                maybeLifeCycleAdapter
                |> Option.map (fun lifeCycleAdapter ->
                    lifeCycleAdapter.LifeCycle.Invoke
                        { new FullyTypedLifeCycleFunction<_> with
                            member _.Invoke lifeCycle =
                                lifeCycle.ToReferencedLifeCycle()
                                :> IReferencedLifeCycle
                        }
                )
            else
                ecosystem.ReferencedEcosystems
                |> Map.tryFind ecosystemName
                |> Option.bind (fun referencedEcosystem ->
                    referencedEcosystem.LifeCycles
                    |> List.tryFind (fun referencedLifeCycle -> referencedLifeCycle.Name = lifeCycleName)
                )

        match maybeReferencedLifeCycle with
        | Some referencedLifeCycle ->
            let logger = fableHub.Services.GetRequiredService<ILoggerFactory>().CreateLogger("RealTime")
            let summarizers = fableHub.Services.GetRequiredService<ValueSummarizers>()
            let fsLogger = newRealTimeScopedLogger summarizers logger lifeCycleName fableHub.Context.ConnectionId

            referencedLifeCycle.Invoke
                { new FullyTypedReferencedLifeCycleFunction<_> with
                    member _.Invoke (referencedLifeCycle: ReferencedLifeCycle<'Subject, _, _, _, _, _, _, _, 'Session, _>) =
                        streamToClientGeneric<'Subject, 'Session>
                            fsLogger
                            ecosystem
                            referencedLifeCycle.SessionHandler
                            lifeCycleName
                            subjectIdStr
                            maybeProjectionName
                            maybeClientVersion
                            fableHub
                            cancellationToken
                }
        | None ->
            let exn = UnsupportedLifeCycleException lifeCycleName
            Observable.Throw<ServerStreamApi>(exn)
            |> AsyncSeq.ofObservableBuffered
            |> AsyncSeq.toAsyncEnum

    // Legacy - can be removed once all clients are migrated. Can then rename the V2 implementation above after suitable delay.
    | ClientStreamApi.ObserveSubject (lifeCycleName, subjectIdStr, maybeProjectionName, maybeClientVersion) ->
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
                    member _.Invoke (lifeCycle: LifeCycle<'Subject2, _, _, _, _, _, _, _, 'Session2, _, _>) =
                        streamToClientGeneric<'Subject2, 'Session2>
                            fsLogger
                            ecosystem
                            lifeCycle.SessionHandler
                            lifeCycleName
                            subjectIdStr
                            maybeProjectionName
                            maybeClientVersion
                            fableHub
                            cancellationToken
                }
        | None ->
            let exn = UnsupportedLifeCycleException lifeCycleName
            Observable.Throw<ServerStreamApi>(exn)
            |> AsyncSeq.ofObservableBuffered
            |> AsyncSeq.toAsyncEnum

let internal registerRealTimeSubjectData
        (services: IServiceCollection)
        (lifeCycleAdapter: IHostedOrReferencedLifeCycleAdapter) =
    lifeCycleAdapter.ReferencedLifeCycle.Invoke
        { new FullyTypedReferencedLifeCycleFunction<_> with
            member _.Invoke (referencedLifeCycle: ReferencedLifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, 'Session, _>) =
                services
                    .AddSingleton<RealTimeSubjectData<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId, 'Session>>(fun serviceProvider ->
                        let adapter = { ReferencedLifeCycle = referencedLifeCycle }
                        RealTimeSubjectData<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId, 'Session>(serviceProvider, adapter)
                    )
                    .AddSingleton<IV1RealTimeSubjectData<'Session, 'Subject>>(fun serviceProvider -> serviceProvider.GetRequiredService<RealTimeSubjectData<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId, 'Session>>() :> IV1RealTimeSubjectData<'Session, 'Subject>)
        }
    |> ignore
