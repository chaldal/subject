module internal LibLifeCycleHost.Web.RealTime

open System
open System.Collections.Concurrent
open System.Reactive.Disposables
open System.Reactive.Linq
open System.Threading.Tasks
open Microsoft.Extensions.DependencyInjection
open LibLifeCycleHost.Web.RealTimeSubjectData
open LibLifeCycle
open LibLifeCycleCore
open Orleans
open LibLifeCycleHost

// A shared cache of observables to monitor session subjects along with a flag indicating their validity. The observables are cached by the hub
// connection ID, and are automatically cleaned up when there are no more observers.
type MaybeSessionWithIsValidObservableCache<'Session>() =
    static let cache = ConcurrentDictionary<string, IObservable<Option<'Session> * bool>>()

    static member GetOrCreate
            (logger: IFsLogger)
            (hostEcosystemGrainFactory: IGrainFactory)
            (grainPartition: GrainPartition)
            (clock: Service<Clock>)
            (cryptographer: ApiSessionCryptographer)
            (maybeSessionHandler: Option<EcosystemSessionHandler<'Session>>)
            (maybeSessionRealTimeSubjectData: Option<IRealTimeSubjectData<'Session, 'Session>>)
            (hubConnectionId: string)
            (maybeEncryptedSessionHandle: Option<string>)=
        let result =
            cache.GetOrAdd(
                hubConnectionId,
                (fun hubConnectionId ->
                    logger.Trace
                        "No session with is validated observable in cache, so creating it for %a"
                        (logger.P "MaybeEncryptedSessionHandle")
                        maybeEncryptedSessionHandle

                    let maybeSessionWithIsValidated =
                        Session.SignalR.getMaybeSessionWithIsValid
                            logger
                            hostEcosystemGrainFactory
                            grainPartition
                            clock
                            cryptographer
                            maybeSessionHandler
                            maybeSessionRealTimeSubjectData
                            maybeEncryptedSessionHandle
                    let maybeSessionWithIsValidated =
                        maybeSessionWithIsValidated
                            // We replay so that new subscribers receive the last known state of the subject immediately.
                            .Replay(1)
                            .OnConnectableDisconnected(fun () ->
                                logger.Trace "No more observers are active against the session with is validated observable, so removing it from the cache"
                                cache.TryRemove(hubConnectionId) |> ignore
                            )
                            // The grace period for this session pipeline is likely unnecessary for most scenarios, since the client will generally have
                            // many subscriptions, all of which depend on this session pipeline, and therefore at least one of which will keep this pipeline
                            // from expiring. However, technically clients could have a single subscription that is occasionally refreshed and that should
                            // be able to occur without the session pipeline being recreated.
                            .RefCount(TimeSpan.FromSeconds(5))

                    maybeSessionWithIsValidated
                )
            )
        result

/// Create an observable to monitor a given subject for changes. As long as the observable remains active, a grain observer will be kept alive against
/// the specific subject.
let createObservableForSubject<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId
            when 'Subject              :> Subject<'SubjectId>
            and  'LifeAction           :> LifeAction
            and  'OpError              :> OpError
            and  'Constructor          :> Constructor
            and  'LifeEvent            :> LifeEvent
            and  'LifeEvent            : comparison
            and  'SubjectId            :> SubjectId
            and  'SubjectId            : comparison>
        (logger: IFsLogger)
        (serviceProvider: IServiceProvider)
        (getGrainFactory: IServiceProvider -> Task<IGrainFactory>)
        (idStr: string)
        (forceSync: IObservable<unit>): IObservable<SubjectChange<'Subject, 'SubjectId>> =
    Observable
        .Using(
            // Tracks the version of the subject held by this observable (which is running on a web node). If messages are lost between the grain
            // and web node, the versions can get out of sync, which we occasionally correct for when re-observing the grain.
            (fun () -> new System.Reactive.Subjects.BehaviorSubject<Option<ComparableVersion>>(None)),
            fun (maybeCurrentVersion: System.Reactive.Subjects.BehaviorSubject<Option<ComparableVersion>>) ->
                Observable
                    .Create(
                        (fun (observer: IObserver<SubjectChange<'Subject, 'SubjectId>>) ->
                            // Generate an address - it matters not, but must be unique to distinguish one observer from another.
                            let address = Guid.NewGuid().ToString()

                            backgroundTask {
                                try
                                    let! grainFactory = getGrainFactory serviceProvider
                                    let (GrainPartition grainPartition) = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<GrainPartition>()
                                    let grain = grainFactory.GetGrain<ISubjectGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>>(grainPartition, idStr)

                                    let grainObserver =
                                        { new ISubjectGrainObserver<'Subject, 'SubjectId> with
                                            member _.OnUpdate (subjectUpdate: LibLifeCycleCore.SubjectChange<'Subject, 'SubjectId>) =
                                                // Keep a record of what version we have locally so we can use it during sync with grain.
                                                subjectUpdate.MaybeComparableVersion
                                                |> maybeCurrentVersion.OnNext

                                                // Update the observer.
                                                subjectUpdate
                                                |> observer.OnNext
                                        }
                                    let! observerRef = grainFactory.CreateObjectReference<ISubjectGrainObserver<'Subject, 'SubjectId>> grainObserver

                                    // We need to sync with the backing grain for two reasons:
                                    //
                                    //  1. If we don't, the grain will assume we're MIA and remove our observer.
                                    //  2. Network hiccups may mean that subject updates are lost, temporarily leaving the web node behind.
                                    //
                                    // A periodic sync solves both issues, but is not proactive for cases where a client ends up ahead of a web node.
                                    // Such a scenario is likely rare, but this pipeline is shared between any number of clients that are observing the
                                    // same subject, so allowing syncs to be forced when a discrepancy is detected will benefit all clients sharing the
                                    // pipeline.
                                    let timerSync =
                                        Observable
                                            .Interval(TimeSpan.FromMinutes(1.0))
                                            .Select(fun _ -> ())

                                    let syncSignal =
                                        Observable
                                            .Merge(
                                                timerSync,
                                                forceSync
                                            )
                                            // Just in case there is spamming of forceSync, or it happens to trigger right about when the timer does.
                                            .Throttle(TimeSpan.FromSeconds(1))

                                    let sync =
                                        Observable
                                            .WithLatestFrom(
                                                syncSignal,
                                                maybeCurrentVersion,
                                                fun _ maybeCurrentVersion -> maybeCurrentVersion
                                            )
                                            .StartWith(None)
                                            .SelectMany(fun maybeCurrentVersion ->
                                                logger.Trace
                                                    "Observing grain with address %a, maybe current version %a"
                                                    (logger.P "Address")
                                                    address
                                                    (logger.P "MaybeCurrentVersion")
                                                    maybeCurrentVersion
                                                grain.Observe address maybeCurrentVersion observerRef
                                            )
                                            .Subscribe(
                                                (fun _ -> ()),
                                                // Ensure any failures in re-observing are passed onto the observer.
                                                observer.OnError
                                            )

                                    let unobserve =
                                        Disposable.Create(
                                            fun () ->
                                                try
                                                    logger.Trace
                                                        "Unobserving grain with address %a"
                                                        (logger.P "Address")
                                                        address
                                                    grain.Unobserve address |> ignore
                                                with
                                                | exn -> observer.OnError exn

                                                // Ensure garbage collection does not prematurely clean up this object, since we only hold a weak
                                                // reference to it (via observerRef).
                                                GC.KeepAlive grainObserver
                                            )

                                    let disposables =
                                        [
                                            sync
                                            unobserve
                                        ]
                                        |> (fun d -> new CompositeDisposable(d))
                                        :> IDisposable

                                    return disposables
                                with
                                | exn ->
                                    observer.OnError exn
                                    return Disposable.Empty
                            }
                        )
                    )
        )