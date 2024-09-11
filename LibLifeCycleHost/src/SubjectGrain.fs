namespace LibLifeCycleHost
// Grains can't be in Modules, due to a bug in the way Orleans builds up Grain identity

open System
open System.Threading.Tasks
open LibLifeCycle.LifeCycles.RequestRateCounter
open LibLifeCycleHost
open LibLifeCycleHost.TelemetryModel
open Orleans
open Orleans.Runtime
open LibLifeCycle
open LibLifeCycleHost.GrainStorageModel
open LibLifeCycleCore
open LibLifeCycleHost.AccessControl
open LibLifeCycleHost.SubjectReminder
open LibLifeCycleHost.Web
open LibLifeCycleHost.Storage.SqlServer.SqlServerGrainStorageHandler
open Microsoft.Extensions.DependencyInjection

type private SubjectGrainPrimaryStore<'Subject, 'Constructor, 'SubjectId, 'LifeEvent, 'LifeAction, 'OpError
                when 'Subject      :> Subject<'SubjectId>
                and  'Constructor  :> Constructor
                and  'LifeEvent    :> LifeEvent
                and  'LifeEvent    : comparison
                and  'LifeAction   :> LifeAction
                and  'SubjectId    :> SubjectId
                and 'SubjectId : comparison
                and  'OpError      :> OpError>() =

        let mutable _value: Option<SubjectStateContainer<'Subject, 'Constructor, 'SubjectId, 'LifeEvent, 'LifeAction, 'OpError>> = None
        let mutable _isPotentiallyInconsistent = false

        member _.Value with get () =
            _value

        member _.Update value =
            _value <- value
            _isPotentiallyInconsistent <- false

        member _.IsPotentiallyInconsistent with get () =
            _isPotentiallyInconsistent

        member _.MarkAsPotentiallyInconsistent () =
            _isPotentiallyInconsistent <- true

        member _.MarkAsConsistent () =
            _isPotentiallyInconsistent <- false


[<RequireQualifiedAccess>]
type private ConstructContext =
| External   of SessionHandle * CallOrigin
| InterGrain of Option<SideEffectDedupInfo> * Option<ConstructSubscriptions> * OkIfAlreadyInitialized: bool
with
    member this.SessionHandle =
        match this with
        | External (x, _) -> x
        | InterGrain _ -> SessionHandle.NoSession

    member this.CallOrigin =
        match this with
        | External (_, x) -> x
        | InterGrain _ -> CallOrigin.Internal

    member this.MaybeDedupInfo =
        match this with
        | External _ -> None
        | InterGrain (x, _, _) -> x

    member this.MaybeConstructSubscriptions =
        match this with
        | External _ -> None
        | InterGrain (_, x, _) -> x

[<RequireQualifiedAccess>]
type private ActContext =
| External   of SessionHandle * CallOrigin
| InterGrain of Option<SideEffectDedupInfo> * Option<BeforeActSubscriptions>
with
    member this.SessionHandle =
        match this with
        | External (x, _) -> x
        | InterGrain _ -> SessionHandle.NoSession

    member this.CallOrigin =
        match this with
        | External (_, x) -> x
        | InterGrain _ -> CallOrigin.Internal

    member this.MaybeDedupInfo =
        match this with
        | External _ -> None
        | InterGrain (x, _) -> x

    member this.MaybeBeforeActSubscriptions =
        match this with
        | External _ -> None
        | InterGrain (_, x) -> x

    member this.ShouldWarnOnOpError =
        match this with
        | External _ -> true
        | InterGrain _ -> false

[<RequireQualifiedAccess>]
type private ActMaybeConstructContext =
| External   of SessionHandle * CallOrigin
| InterGrain of Option<SideEffectDedupInfo> * Option<ConstructSubscriptions>
with
    member this.ConstructContext =
        match this with
        | External (x1, x2) -> ConstructContext.External (x1, x2)
        // dedup info is erased and reserved for Act part
        | InterGrain (_dedupInfo, maybeConstructSubscriptions) -> ConstructContext.InterGrain (None, maybeConstructSubscriptions, (* okIfAlreadyInitialized *) true)

    member this.ActContext =
        match this with
        | External (x1, x2) -> ActContext.External (x1, x2)
        | InterGrain (dedupInfo, maybeSubscriptions) -> ActContext.InterGrain (dedupInfo, maybeSubscriptions)

type private SafeServiceProvider (innerProvider: IServiceProvider) =
    interface IServiceProvider with
        member _.GetService(serviceType: Type) =
            try
                innerProvider.GetService serviceType
            with
            // wrap known exceptions so side effects don't fail permanently
            // can dispose mid-transition during silo shutdown
            | :? ObjectDisposedException as ex ->
                raise (TransientSubjectException ("SafeServiceProvider", ex.ToString()))

module private Dedup =
    let sideEffectDedupDataFor
        (maxDedupCacheSize: uint16)
        (maybeCache: Option<SideEffectDedupCache>)
        (maybeDedupInfo: Option<SideEffectDedupInfo * (* lastUpdatedOn *) DateTimeOffset>)
        : SideEffectDedupCache * Option<UpsertDedupData> =
        match maybeCache, maybeDedupInfo with
        | None, None ->
            Map.empty, None

        | None, Some (dedupInfo, lastUpdatedOn) ->
             Map.ofOneItem (dedupInfo.Caller, (dedupInfo.Id, lastUpdatedOn)),
             (Some { DedupInfo = dedupInfo; IsInsert = true; EvictedDedupInfoToDelete = None })

        | Some currentCache, None ->
            currentCache, None

        | Some currentCache, Some (dedupInfo, lastUpdatedOn) ->

            let isInsert = not <| currentCache.ContainsKey dedupInfo.Caller
            let updatedCache, evicted =
                let upd = currentCache.AddOrUpdate (dedupInfo.Caller, (dedupInfo.Id, lastUpdatedOn))
                if upd.Count <= (int maxDedupCacheSize) then
                    upd, None
                else
                    // evict oldest. Full scan, need a proper priority queue if large cache needed
                    let evictedCaller, (evictedId, evictedWasCalledOn) =
                        upd
                        |> Map.toSeq
                        |> Seq.minBy (fun (_, (_, on: DateTimeOffset)) -> on)
                    (upd.Remove evictedCaller, Some ({ Id = evictedId; Caller = evictedCaller }, evictedWasCalledOn))

            updatedCache,
            (Some { DedupInfo = dedupInfo; IsInsert = isInsert; EvictedDedupInfoToDelete = evicted })

type SubjectGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId
                when 'Subject              :> Subject<'SubjectId>
                and  'LifeAction           :> LifeAction
                and  'OpError              :> OpError
                and  'Constructor          :> Constructor
                and  'LifeEvent            :> LifeEvent
                and  'LifeEvent            :  comparison
                and  'SubjectId            :> SubjectId
                and  'SubjectId            :  comparison>
        (
            lifeCycleAdapter: HostedLifeCycleAdapter<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>,
            grainStorageHandler: IGrainStorageHandler<'Subject, 'LifeAction, 'Constructor, 'LifeEvent, 'SubjectId, 'OpError>,
            grainScopedServiceProviderUnsafe: IServiceProvider,
            hostEcosystemGrainFactory: IGrainFactory,
            adapters: HostedOrReferencedLifeCycleAdapterRegistry,
            ctx: IGrainActivationContext,
            valueSummarizers: ValueSummarizers,
            clock: Service<Clock>,
            unscopedLogger: Microsoft.Extensions.Logging.ILogger<'Subject>,
            operationTracker: OperationTracker
        ) as this =

    inherit Grain()

    let grainScopedServiceProvider = (SafeServiceProvider grainScopedServiceProviderUnsafe) :> IServiceProvider

    // The registration for Service<PermanentFailureManager> requires GrainPartition which requires IGrainActivationContext, which is only available within the grainScopedServiceProvider, and not within any child scopes.
    // We'll need something like Autofac, where we can resolve the value from the root scope, and override the registration within the child scope to that resolved value
    // Until then we'll pass this along via a mutable child-scoped property
    let prepareChildScope () : IServiceScope =
        let scope = grainScopedServiceProvider.CreateScope()
        scope.ServiceProvider.GetRequiredService<ContainerForIGrainActivationContext>().Value <- Some ctx
        scope

    let shouldSendConstructorTelemetry (ctor: 'Constructor) =
        (lifeCycleAdapter :> IHostedLifeCycleAdapter<_, _, _, _, _, _>).ShouldSendTelemetry (ShouldSendTelemetryFor.Constructor ctor)

    let shouldSendActionTelemetry (action: 'LifeAction) =
        (lifeCycleAdapter :> IHostedLifeCycleAdapter<_, _, _, _, _, _>).ShouldSendTelemetry (ShouldSendTelemetryFor.LifeAction action)

    let shouldSendSelfDeleteTelemetry (_subjectId: 'SubjectId) =
        // TODO: model self-delete telemetry properly e.g. add  | SelfDelete of 'SubjectId case
        // meanwhile just disable it for RequestRateCounter
        typeof<'Subject> <> typeof<RequestRateCounter>

    let remindersImplementation = grainStorageHandler.RemindersImplementation

    let maxDedupCacheSize = lifeCycleAdapter.LifeCycle.Storage.MaxDedupCacheSize

    let (grainPartition, grainPKey) =
        let (grainPartition, pKey) = ctx.GrainIdentity.GetPrimaryKey()
        ((grainPartition |> GrainPartition), pKey)

    let logger = newGrainScopedLogger valueSummarizers unscopedLogger lifeCycleAdapter.LifeCycle.Name grainPartition grainPKey

    let sideEffectTrackerHook = (grainScopedServiceProvider.GetService typeof<ISideEffectTrackerHook> :?> ISideEffectTrackerHook) |> Option.ofObj

    let reminderHook = grainScopedServiceProvider.GetService<IReminderHook>() |> Option.ofObj

    let sessionOnlyClearStateMutex = new System.Threading.SemaphoreSlim(1)

    let mutable maybeClearExpiredTimer = None

    let mutable observerManager = Unchecked.defaultof<ObserverManager<string, ISubjectGrainObserver<'Subject, 'SubjectId>>>

    let mutable isGrainDeactivated = false

    do
        observerManager <-
            ObserverManager<string, ISubjectGrainObserver<'Subject, 'SubjectId>>(
                TimeSpan.FromMinutes(5.0),
                logger,
                (fun () -> DateTimeOffset.Now), // We do not want our time overriden here (if the clock is moved forward), as this is used for heartbeating and expiry detection of observers
                (fun () ->
                    logger.Debug "First observer added"

                    // We have a first observer, so delay deactivation indefinitely.
                    this.CallDelayDeactivation TimeSpan.MaxValue

                    // Ensure expired observers are cleared out pro-actively rather than relying on notifications.
                    match maybeClearExpiredTimer with
                    | None ->
                        maybeClearExpiredTimer <-
                            this.CallRegisterTimerReentrant
                                (TimeSpan.FromMinutes(1.0))
                                (fun grain -> grain.InternalOnlyClearExpiredObservers())
                            |> Some
                    | Some _ -> failwith "Expected no timer to be running"
                ),
                (fun () ->
                    logger.Debug "Last observer removed"

                    // No more observers, so we no longer need to delay deactivation.
                    this.CallDelayDeactivation TimeSpan.MinValue

                    // Clean up the expiry timer.
                    match maybeClearExpiredTimer with
                    | Some clearExpiredTimer ->
                        clearExpiredTimer.Dispose()
                        maybeClearExpiredTimer <- None
                    | None -> failwith "Expected a timer to be running"
                )
            )

    let primaryStore = SubjectGrainPrimaryStore()

    let mutable dedupInfoEvictedInShortTimeWarningCount: byte = 0uy

    // There's unfortunately no better way to access the Orleans Task Scheduler, other than to wait for OnActivateAsync
    // We'll need this to run async tasks that post back into the orleans context
    let mutable orleansTaskScheduler: TaskScheduler = null

    let awaitersByLifeEvent = System.Collections.Generic.Dictionary<'LifeEvent, List<AwaiterWithTimeout<'Subject, 'LifeEvent, 'SubjectId>>>()

    let updateGrainSideEffect (sideEffectId: GrainSideEffectId) (sideEffectResult: GrainSideEffectResult) : Task =
        backgroundTask {
            do! grainStorageHandler.UpdateSideEffectStatus grainPKey sideEffectId sideEffectResult
            sideEffectTrackerHook |> Option.iter (fun hook -> hook.OnSideEffectProcessed sideEffectId)
        }

    let mutable maybeSideEffectProcessor: Option<MailboxProcessor<SideEffectProcessorMessage<'LifeAction, 'OpError>>> = None

    let sideEffectProcessor (thisSubjectId: 'SubjectId) =
        match maybeSideEffectProcessor with
        | Some mailboxProcessor ->
            mailboxProcessor
        | None ->
            if orleansTaskScheduler = null then
                invalidOp("Side Effect Processor cannot be initialized before OnActivateAsync() is called")

            let mailboxProcessor =
                getSideEffectMailboxProcessor<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId> grainScopedServiceProvider grainPartition thisSubjectId updateGrainSideEffect
                    this.IsGrainDeactivated logger valueSummarizers operationTracker

            maybeSideEffectProcessor <- Some mailboxProcessor
            mailboxProcessor.Start()
            mailboxProcessor

    // We want to make as much logic as functional as possible, to prevent direct manipulation of the state of this grain
    let (processAction,
         calculateDueTimerActionOrNextDueTick,
         addToOthersSubscribing,
         removeFromOthersSubscribing,
         getActionForTriggeredSubscription,
         processConstruction,
         mapTriggerSubscriptionSideEffects,
         rethrowIfTransientInternalCallOrPassIn,
         testingOnlyInitializeDirectlyToValue) =
        getGrainFunctions adapters grainScopedServiceProvider lifeCycleAdapter

    let applyReminderUpdateToGrain (thisSubjectId: 'SubjectId) (maybeReminderUpdate: Option<ReminderUpdate>) =
        match maybeReminderUpdate with
        | None -> Task.CompletedTask
        | Some reminderUpdate ->
            task {
                try
                    match reminderUpdate.On with
                    | Some nextTickOn ->
                        let! now = clock.Query Now
                        do! this.SetTickReminder nextTickOn now reminderUpdate.AssumedCurrent
                        reminderHook
                        |> Option.iter (
                            fun hook ->
                                hook.RegisterReminder logger lifeCycleAdapter.LifeCycle.Def.LifeCycleKey grainPartition thisSubjectId nextTickOn
                            )
                    | None ->
                        do! this.ClearTickReminder reminderUpdate.AssumedCurrent
                        reminderHook
                        |> Option.iter (
                            fun hook ->
                                hook.ClearReminder logger lifeCycleAdapter.LifeCycle.Def.LifeCycleKey grainPartition thisSubjectId
                            )
                with
                | ex ->
                    // just swallow exception, reminder service will update from SubjectReminderTable, or _Meta will detect and solve it
                    logger.ErrorExn ex "ApplyReminderUpdate %a ==> EXCEPTION" (logger.P "reminderUpdate") reminderUpdate
            }

    let queueSideEffectGroupForAsyncProcessing (thisSubjectId: 'SubjectId) (sideEffectGroup: SideEffectGroup<'LifeAction, 'OpError>) =
        sideEffectTrackerHook
        |> Option.iter (
            fun hook ->
                hook.OnNewSideEffects (
                    sideEffectGroup.SideEffects
                    |> NonemptyMap.toSeq
                    |> Seq.map (
                        fun (id, se) ->
                            id,
                            match se with
                            | GrainSideEffect.Persisted e ->
                                match e with
                                | GrainPersistedSideEffect.RunActionOnSelf (x1, x2) ->
                                    GrainPersistedSideEffect.RunActionOnSelf (x1 :> LifeAction, x2)
                                | GrainPersistedSideEffect.Rpc x ->
                                    GrainPersistedSideEffect.Rpc x
                                | GrainPersistedSideEffect.RpcTransactionStep x ->
                                    GrainPersistedSideEffect.RpcTransactionStep x
                                | GrainPersistedSideEffect.RpcTriggerSubscriptionOnGrain x ->
                                    GrainPersistedSideEffect.RpcTriggerSubscriptionOnGrain x
                                | GrainPersistedSideEffect.TriggerTimerActionOnSelf (x1, x2) ->
                                    GrainPersistedSideEffect.TriggerTimerActionOnSelf (x1 :> LifeAction, x2)
                                | GrainPersistedSideEffect.HandleSubscriptionResponseOnSelf (x1, x2, x3, x4) ->
                                    GrainPersistedSideEffect.HandleSubscriptionResponseOnSelf (TriggerSubscriptionResponse<LifeAction, OpError>.CastUnsafe x1, x2, x3, x4)
                                | GrainPersistedSideEffect.TryDeleteSelf (x1, x2, x3) ->
                                    GrainPersistedSideEffect.TryDeleteSelf (x1, x2, x3)
                                | GrainPersistedSideEffect.IngestTimeSeries (x1, x2, x3) ->
                                    GrainPersistedSideEffect.IngestTimeSeries (x1, x2, x3)
                                | GrainPersistedSideEffect.ObsoleteNoop x ->
                                    GrainPersistedSideEffect.ObsoleteNoop x
                                |> GrainSideEffect.Persisted
                            | GrainSideEffect.Transient (GrainTransientSideEffect.ConnectorRequest(x1, x2, x3, x4, _func)) ->
                                GrainSideEffect.Transient (GrainTransientSideEffect.ConnectorRequest(x1, x2, x3, x4, Unchecked.defaultof<obj -> LifeAction>))
                            | GrainSideEffect.Transient (GrainTransientSideEffect.ConnectorRequestMultiResponse(x1, x2, x3, x4, _func)) ->
                                GrainSideEffect.Transient (GrainTransientSideEffect.ConnectorRequestMultiResponse(x1, x2, x3, x4, Unchecked.defaultof<obj -> LifeAction>))
                            )))
        // Asynchrously process side-effects
        SideEffectProcessorMessage.ProcessSideEffectGroup sideEffectGroup
        |> (sideEffectProcessor thisSubjectId).Post

    let getGrainIdHash () = this.GrainReference.GetUniformHashCode()

    let catchExnButRethrowTransientIfInternalCall_OnResultTask (callOrigin: CallOrigin) (startTask: unit -> Task<Result<'Ok, 'Error>>) : Task<Result<'Ok, Choice<'Error, Exception>>> =
        task {
            try
                let! res = startTask ()
                return
                    match res with
                    | Ok x -> Ok x
                    | Error e -> e |> Choice1Of2 |> Error
            with
            | :? Orleans.Storage.InconsistentStateException as ex ->
                // re-raise orleans inconsistent state exception because it's transient (unless there's an error in storage handler implementation)
                // do not wrap it in TransientSubjectException, otherwise Orleans won't recycle the grain and the grain will keep throwing.
                primaryStore.MarkAsPotentiallyInconsistent ()
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw()
                return shouldNotReachHereBecause "line above throws"

            | ex ->
                primaryStore.MarkAsPotentiallyInconsistent ()
                let ex = rethrowIfTransientInternalCallOrPassIn callOrigin ex
                return ex |> Choice2Of2 |> Error
        }

    let catchExnButRethrowTransientIfInternalCall_OnSimpleTask (callOrigin: CallOrigin) (startTask: unit -> Task<'Ok>) : Task<Result<'Ok, Exception>> =
        task {
            let! result = catchExnButRethrowTransientIfInternalCall_OnResultTask callOrigin (fun () ->
                    task {
                        let! ok = startTask ()
                        return Ok ok
                    })
            match result with
            | Ok ok ->
                return Ok ok
            | Error (Choice2Of2 exn) ->
                return Error exn
            | Error (Choice1Of2 ()) ->
                return failwith "unexpected"
        }

    let catchExnButRethrowTransientIfInternalCall_OnVoidTask (callOrigin: CallOrigin) (startTask: unit -> Task) : Task<Result<unit, Exception>> =
        task {
            let! result = catchExnButRethrowTransientIfInternalCall_OnResultTask callOrigin (fun () ->
                    task {
                        do! startTask ()
                        return Ok ()
                    })
            match result with
            | Ok () ->
                return Ok ()
            | Error (Choice2Of2 exn) ->
                return Error exn
            | Error (Choice1Of2 ()) ->
                return failwith "unexpected"
        }

    /// Must be called only for ISubjectClientGrain methods, so clients don't lose exception details
    let wrapClientExceptions (methodName: string) (createTask: unit -> Task<'T>) : Task<'T> =
        task {
            try
                let! res = createTask ()
                return res
            with
            | :? Orleans.Storage.InconsistentStateException as ex ->
                // do not wrap inconsistent state exception, otherwise Orleans won't recycle the grain and it will keep throwing.
                primaryStore.MarkAsPotentiallyInconsistent ()
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw()
                return shouldNotReachHereBecause "line above throws"

            | :? TransientSubjectException as ex ->
                // no need to wrap TransientSubjectException twice, just rethrow, and no need to log because should be already logged
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw()
                return shouldNotReachHereBecause "line above throws"
            | :? PermanentSubjectException as ex ->
                // no need to wrap PermanentSubjectException twice, just rethrow, and no need to log because should be already logged
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw()
                return shouldNotReachHereBecause "line above throws"

            | ex ->
                logger.ErrorExn ex "Method %a ==> EXCEPTION" (logger.P "method") methodName
                return TransientSubjectException ($"Life Cycle %s{lifeCycleAdapter.LifeCycle.Name} %s{methodName}", ex.ToString()) |> raise
        }

    let triggerAwaiter
            (now: DateTimeOffset)
            (awaiter: AwaiterWithTimeout<'Subject, 'LifeEvent, 'SubjectId>)
            (versionedSubject: VersionedSubject<'Subject, 'SubjectId>)
            (lifeEvent: 'LifeEvent) =
        if awaiter.ExpiryTime >= now then
            awaiter.Awaiter.EventTriggered versionedSubject lifeEvent

    let notifyAndRemoveLifeEventAwaiters (now: DateTimeOffset, raisedLifeEvents: List<'LifeEvent>, versionedSubject: VersionedSubject<'Subject, 'SubjectId>) : unit =
        raisedLifeEvents
        |> Seq.collect (
            fun raisedLifeEvent ->
                awaitersByLifeEvent
                |> Seq.map (|KeyValue|)
                |> Seq.choose (
                    fun (subscribedLifeEvent, awaiters) ->
                        if lifeCycleAdapter.LifeCycle.LifeEventSatisfies.Value { Raised = raisedLifeEvent; Subscribed = subscribedLifeEvent } then
                            awaiters
                            |> Seq.iter (fun awaiter -> triggerAwaiter now awaiter versionedSubject raisedLifeEvent)
                            Some subscribedLifeEvent
                        else
                            None
                )
            )
        |> Seq.iter (
            fun lifeEventsToRemove ->
                awaitersByLifeEvent.Remove lifeEventsToRemove |> ignore
            )

    let updatePrimaryStoreWithoutNotify value  =
        primaryStore.Update value

    let updatePrimaryStoreWithNotify payload =
        let value, subjectChange, notifyLifeEventsPayload =
            match payload with
            | Some (currentStateContainer, raisedLifeEventsPayload) ->
                Some (SubjectStateContainer.Committed currentStateContainer),
                SubjectChange.Updated
                  { Subject = currentStateContainer.CurrentSubjectState.Subject
                    AsOf = currentStateContainer.CurrentSubjectState.LastUpdatedOn
                    Version = currentStateContainer.Version },
                raisedLifeEventsPayload |> Option.map (fun (now, events) -> now, events, currentStateContainer.VersionedSubject)
            | None ->
                None, SubjectChange.NotInitialized, None

        // this order is important:  first store new state to grain memory, then notify life event awaiters, only then realtime updates
        primaryStore.Update value

        notifyLifeEventsPayload |> Option.iter notifyAndRemoveLifeEventAwaiters

        observerManager.NotifyObservers
            (fun _ -> true)
            (fun o -> o.OnUpdate(subjectChange))

    let initializeSubject now raisedLifeEvents dedupInfo insertData =
        task {
            match primaryStore.Value with
            | None -> ()
            | Some x -> failwithf "unexpected initializeSubject when primary store is not None: %A" x

            match! grainStorageHandler.InitializeSubject grainPKey dedupInfo insertData with
            | Result.Ok success ->
                // Post side effects to queue before updating primaryStore, because mailbox is reliable but updatePrimaryStore is not / can throw if there are realtime issues
                let subjectId = insertData.DataToInsert.UpdatedSubjectState.Subject.SubjectId
                do! applyReminderUpdateToGrain subjectId insertData.DataToInsert.ReminderUpdate
                insertData.DataToInsert.SideEffectGroup
                |> Option.iter (queueSideEffectGroupForAsyncProcessing subjectId)

                let dedupCache, _ =
                    Dedup.sideEffectDedupDataFor maxDedupCacheSize None
                        (dedupInfo |> Option.map (fun d -> d, insertData.DataToInsert.UpdatedSubjectState.LastUpdatedOn))

                let newSubjectCurrentStateContainer =
                    { CurrentSubjectState      = insertData.DataToInsert.UpdatedSubjectState
                      CurrentOthersSubscribing = insertData.CreatorSubscribing
                      ETag                     = success.ETag
                      Version                  = success.Version
                      NextSideEffectSeqNum     = insertData.DataToInsert.NextSideEffectSeq
                      SideEffectDedupCache     = dedupCache
                      SkipHistoryOnNextOp      = success.SkipHistoryOnNextOp }

                (newSubjectCurrentStateContainer, Some (now, raisedLifeEvents))
                |> Some
                |> updatePrimaryStoreWithNotify

                return Ok newSubjectCurrentStateContainer

            | Error err ->
                return Error err
        }

    let prepareInitializeSubject preparedSubjectInsertData uniqueIndicesToReserve transactionId : Task<Result<unit, 'OpError>> =
        task {
            match primaryStore.Value with
            | None -> ()
            | Some x -> failwithf "unexpected prepareInitializeSubject when primary store is not None: %A" x

            match! grainStorageHandler.PrepareInitializeSubject grainPKey preparedSubjectInsertData uniqueIndicesToReserve transactionId with
            | Ok eTag ->
                SubjectStateContainer.PreparedInitialize (preparedSubjectInsertData, eTag, transactionId) |> Some |> updatePrimaryStoreWithoutNotify
                return Ok ()
            | Error err ->
                return Error err
        }

    let commitInitializeSubject (overrideTraceContextParentId: string) (preparedSubjectInsertData: PreparedSubjectInsertData<'Subject, 'Constructor, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>) subjectTransactionId eTag =
        task {
            match primaryStore.Value with
            | Some (SubjectStateContainer.PreparedInitialize _) -> ()
            | None
            | Some (SubjectStateContainer.Committed _)
            | Some (SubjectStateContainer.PreparedAction _) ->
                failwithf "unexpected commitInitializeSubject when primary store is not PreparedInitialize: %A" primaryStore.Value

            let insertData = preparedSubjectInsertData.Upcast overrideTraceContextParentId
            let! success = grainStorageHandler.CommitInitializeSubject grainPKey eTag subjectTransactionId insertData

            // Post side effects to queue before updating primaryStore, because mailbox is reliable but updatePrimaryStore is not / can throw if there are realtime issues
            let subjectId = insertData.DataToInsert.UpdatedSubjectState.Subject.SubjectId
            do! applyReminderUpdateToGrain subjectId insertData.DataToInsert.ReminderUpdate
            insertData.DataToInsert.SideEffectGroup
            |> Option.iter (queueSideEffectGroupForAsyncProcessing subjectId)

            let newSubjectCurrentStateContainer =
                { CurrentSubjectState      = insertData.DataToInsert.UpdatedSubjectState
                  CurrentOthersSubscribing = insertData.CreatorSubscribing
                  ETag                     = success.ETag
                  Version                  = success.Version
                  NextSideEffectSeqNum     = insertData.DataToInsert.NextSideEffectSeq
                  SideEffectDedupCache     = Map.empty
                  SkipHistoryOnNextOp      = success.SkipHistoryOnNextOp }

            ({ newSubjectCurrentStateContainer with NextSideEffectSeqNum = insertData.DataToInsert.NextSideEffectSeq }, None)
            // TODO: raisedLifeEvents awaiting? but nothing can await them while it's being constructed in transaction?
            |> Some
            |> updatePrimaryStoreWithNotify
        }

    let rollbackPrepareInitialize
        (preparedState: PreparedSubjectInsertData<'Subject, 'Constructor, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>)
        subjectTransactionId
        eTag
        : Task =
        task {
            do! grainStorageHandler.RollbackInitializeSubject
                    grainPKey preparedState.PreparedDataToInsert.UniqueIndicesToReleaseOnRollback subjectTransactionId eTag

            None |> updatePrimaryStoreWithoutNotify
            this.CallDeactivateOnIdle()
            logger.Info "ROLLBACK ==> DELETED NEW PREPARED"
        }

    let updateSubject
            (now: DateTimeOffset)
            (raisedLifeEvents: list<'LifeEvent>)
            (dedupInfo: Option<SideEffectDedupInfo>)
            (currentStateContainer: SubjectCurrentStateContainer<'Subject, 'SubjectId, 'LifeEvent, 'LifeAction>)
            (updateData: SubjectUpdateData<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>)
            : Task<Result<SubjectCurrentStateContainer<'Subject, 'SubjectId, 'LifeEvent, 'LifeAction>,'OpError>> =
        task {
            match primaryStore.Value with
            | Some (SubjectStateContainer.Committed _) -> ()
            | Some (SubjectStateContainer.PreparedAction _)
            | Some (SubjectStateContainer.PreparedInitialize _)
            | None -> failwithf "unexpected updateSubject when primary store is not Committed: %A" primaryStore.Value

            let newDedupCache, dedupData =
                Dedup.sideEffectDedupDataFor
                    maxDedupCacheSize (Some currentStateContainer.SideEffectDedupCache)
                    (dedupInfo |> Option.map (fun d -> d, updateData.DataToUpdate.UpdatedSubjectState.LastUpdatedOn))

            match! grainStorageHandler.UpdateSubject grainPKey dedupData updateData with
            | Ok success ->

                // Post side effects to queue before updating primaryStore, because mailbox is reliable but updatePrimaryStore is not / can throw if there are realtime issues
                let subjectId = currentStateContainer.CurrentSubjectState.Subject.SubjectId
                do! applyReminderUpdateToGrain subjectId updateData.DataToUpdate.ReminderUpdate
                updateData.DataToUpdate.SideEffectGroup
                |> Option.iter (queueSideEffectGroupForAsyncProcessing subjectId)

                let newSubjectStateContainer =
                    { CurrentSubjectState = updateData.DataToUpdate.UpdatedSubjectState
                      CurrentOthersSubscribing = currentStateContainer.CurrentOthersSubscribing
                      ETag = success.NewETag
                      Version = success.NewVersion
                      NextSideEffectSeqNum = updateData.DataToUpdate.NextSideEffectSeq
                      SideEffectDedupCache = newDedupCache
                      SkipHistoryOnNextOp = success.SkipHistoryOnNextOp }

                (newSubjectStateContainer, Some (now, raisedLifeEvents)) |> Some |> updatePrimaryStoreWithNotify

                match dedupData |> Option.bind (fun d -> d.EvictedDedupInfoToDelete) with
                | None -> ()
                | Some (evicted, evictedWasCalledOn) ->
                    if dedupInfoEvictedInShortTimeWarningCount < 5uy && (now - evictedWasCalledOn).TotalMinutes < 1. then
                        dedupInfoEvictedInShortTimeWarningCount <- dedupInfoEvictedInShortTimeWarningCount + 1uy
                        logger.Info "Dedup info evicted in less than a minute: %a Consider increasing MaxDedupCacheSize to reduce risk of duplicate inter-grain calls. This warning will stop showing after few repetitions."
                            (logger.P "dedupInfo") evicted

                return Ok newSubjectStateContainer

            | Error err ->
                return Error err
        }

    let prepareUpdateSubject
            (currentStateContainer: SubjectCurrentStateContainer<'Subject, 'SubjectId, 'LifeEvent, 'LifeAction>)
            (preparedUpdateData: PreparedSubjectUpdateData<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>)
            (uniqueIndicesToReserve: list<UniqueIndexToReserveOnPrepare<'OpError>>)
            (subjectTransactionId: SubjectTransactionId)
            : Task<Result<unit, 'OpError>> =
        task {
            match primaryStore.Value with
            | Some (SubjectStateContainer.Committed _) -> ()
            | Some (SubjectStateContainer.PreparedAction _)
            | Some (SubjectStateContainer.PreparedInitialize _)
            | None -> failwithf "unexpected prepareUpdateSubject when primary store is not Committed: %A" primaryStore.Value

            match! grainStorageHandler.PrepareUpdateSubject grainPKey currentStateContainer.ETag preparedUpdateData uniqueIndicesToReserve subjectTransactionId with
            | Ok eTag ->
                SubjectStateContainer.PreparedAction ({ currentStateContainer with ETag = eTag }, preparedUpdateData, subjectTransactionId)
                |> Some
                |> updatePrimaryStoreWithoutNotify
                return Ok ()
            | Error err ->
                return Error err
        }

    let commitUpdateSubject
            (overrideTraceContextParentId: string)
            (currentStateContainer: SubjectCurrentStateContainer<'Subject, 'SubjectId, 'LifeEvent, 'LifeAction>)
            (preparedUpdateData: PreparedSubjectUpdateData<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>)
            (subjectTransactionId: SubjectTransactionId)
            : Task =
        task {
            match primaryStore.Value with
            | Some (SubjectStateContainer.PreparedAction _) -> ()
            | Some (SubjectStateContainer.Committed _)
            | Some (SubjectStateContainer.PreparedInitialize _)
            | None -> failwithf "unexpected commitUpdateSubject when primary store is not PreparedAction: %A" primaryStore.Value

            let updateData =
                preparedUpdateData.Upcast
                    currentStateContainer.ETag
                    currentStateContainer.Version
                    currentStateContainer.NextSideEffectSeqNum
                    overrideTraceContextParentId
                    (mapTriggerSubscriptionSideEffects currentStateContainer.CurrentOthersSubscribing)
            let! success = grainStorageHandler.CommitUpdateSubject grainPKey subjectTransactionId updateData

            // Post side effects to queue before updating primaryStore, because mailbox is reliable but updatePrimaryStore is not / can throw if there are realtime issues
            let subjectId = currentStateContainer.CurrentSubjectState.Subject.SubjectId
            do! applyReminderUpdateToGrain subjectId updateData.DataToUpdate.ReminderUpdate
            updateData.DataToUpdate.SideEffectGroup
            |> Option.iter (queueSideEffectGroupForAsyncProcessing subjectId)

            let subjectCurrentStateContainer =
                { CurrentSubjectState = preparedUpdateData.PreparedDataToUpdate.UpdatedSubjectState
                  CurrentOthersSubscribing = currentStateContainer.CurrentOthersSubscribing
                  ETag = success.NewETag
                  Version = success.NewVersion
                  NextSideEffectSeqNum = updateData.DataToUpdate.NextSideEffectSeq
                  SideEffectDedupCache = currentStateContainer.SideEffectDedupCache
                  SkipHistoryOnNextOp = success.SkipHistoryOnNextOp }
            (subjectCurrentStateContainer, None)
            |> Some // TODO: raisedLifeEvents? but nothing can await them while it's being updated in transaction?
            |> updatePrimaryStoreWithNotify
        }

    let rollbackUpdateSubject
            (currentStateContainer: SubjectCurrentStateContainer<'Subject, 'SubjectId, 'LifeEvent, 'LifeAction>)
            (preparedState: PreparedSubjectUpdateData<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>)
            (subjectTransactionId: SubjectTransactionId)
            : Task =
        task {
            let! newETag =
                grainStorageHandler.RollbackUpdateSubject
                    grainPKey
                    currentStateContainer.ETag
                    preparedState.PreparedDataToUpdate.UniqueIndicesToReleaseOnRollback
                    subjectTransactionId
            { currentStateContainer with ETag = newETag } |> SubjectStateContainer.Committed |> Some |> updatePrimaryStoreWithoutNotify
        }

    let addSubscriptions
        (currentStateContainer: SubjectCurrentStateContainer<'Subject, 'SubjectId, 'LifeEvent, 'LifeAction>)
        (updatedOthersSubscribing: OthersSubscribing<'LifeEvent>)
        (subscriptionsToAdd: SubscriptionsToAdd<'LifeEvent>) =
        task {
            let! newETag = grainStorageHandler.AddSubscriptions grainPKey currentStateContainer.ETag subscriptionsToAdd
            let newCurrentStateContainer =
                { currentStateContainer with
                    ETag = newETag
                    CurrentOthersSubscribing = updatedOthersSubscribing }

            match primaryStore.Value with
            | Some (SubjectStateContainer.Committed _) ->
                SubjectStateContainer.Committed newCurrentStateContainer
            | Some (SubjectStateContainer.PreparedAction (_, preparedState, transactionId)) ->
                SubjectStateContainer.PreparedAction (newCurrentStateContainer, preparedState, transactionId)
            | None
            | Some (SubjectStateContainer.PreparedInitialize _) ->
                shouldNotReachHereBecause "this code executes only for current state"
            |> Some
            |> updatePrimaryStoreWithoutNotify

            return newCurrentStateContainer.VersionedSubject
        }

    let enqueueSideEffects
            (currentStateContainer: SubjectCurrentStateContainer<'Subject, 'SubjectId, 'LifeEvent, 'LifeAction>)
            (maybeDedupInfo: Option<SideEffectDedupInfo>)
            (sideEffectGroups: NonemptyKeyedSet<GrainSideEffectSequenceNumber,SideEffectGroup<'LifeAction,'OpError>>)
            : Task =
        task {
            match primaryStore.Value with
            | Some (SubjectStateContainer.Committed _) -> ()
            | Some (SubjectStateContainer.PreparedAction _)
            | Some (SubjectStateContainer.PreparedInitialize _)
            | None -> failwithf "unexpected enqueueSideEffects when primary store is not Committed: %A" primaryStore.Value

            let nextSideEffectSeqNum = currentStateContainer.NextSideEffectSeqNum + uint64 sideEffectGroups.Count.Value

            let! now = clock.Query Now
            let newDedupCache, dedupData =
                Dedup.sideEffectDedupDataFor maxDedupCacheSize (Some currentStateContainer.SideEffectDedupCache) (maybeDedupInfo |> Option.map (fun d -> d, now))

            let! newETag =
                grainStorageHandler.EnqueueSideEffects grainPKey now dedupData currentStateContainer.ETag nextSideEffectSeqNum sideEffectGroups

            let subjectId = currentStateContainer.CurrentSubjectState.Subject.SubjectId
            sideEffectGroups.Values
            |> Seq.sortBy (fun g -> g.SequenceNumber)
            |> Seq.iter (queueSideEffectGroupForAsyncProcessing subjectId)

            { currentStateContainer with
                ETag = newETag
                SideEffectDedupCache = newDedupCache
                NextSideEffectSeqNum = nextSideEffectSeqNum }
            |> SubjectStateContainer.Committed
            |> Some
            |> updatePrimaryStoreWithoutNotify

            match dedupData |> Option.bind (fun d -> d.EvictedDedupInfoToDelete) with
            | None -> ()
            | Some (evicted, evictedWasCalledOn) ->
                if dedupInfoEvictedInShortTimeWarningCount < 5uy && (now - evictedWasCalledOn).TotalMinutes < 1. then
                    dedupInfoEvictedInShortTimeWarningCount <- dedupInfoEvictedInShortTimeWarningCount + 1uy
                    logger.Info "Dedup info evicted in less than a minute: %a Consider increasing MaxDedupCacheSize to reduce risk of duplicate inter-grain calls. This warning will stop showing after few repetitions."
                        (logger.P "dedupInfo") evicted
        }

    let enqueueActionsOnSelf
            (currentStateContainer: SubjectCurrentStateContainer<'Subject, 'SubjectId, 'LifeEvent, 'LifeAction>)
            (dedupInfo: SideEffectDedupInfo)
            (traceContext: TraceContext)
            (actionsToEnqueue: NonemptyList<'LifeAction>)
            : Task =
        let sideEffectGroups =
            actionsToEnqueue |> NonemptyList.mapi (fun i action ->
                let sideEffect = (action, Some traceContext) |> RunActionOnSelf |> GrainSideEffect.Persisted
                makeNewSideEffectsGroup (currentStateContainer.NextSideEffectSeqNum + uint64 i) [sideEffect] |> Option.get)
            |> NonemptyKeyedSet.ofNonemptyList
        enqueueSideEffects currentStateContainer (Some dedupInfo) sideEffectGroups

    let mapSubscriptionResponseToSideEffect
        (currentStateContainer: SubjectCurrentStateContainer<'Subject, 'SubjectId, 'LifeEvent, 'LifeAction>)
        (triggerType: SubscriptionTriggerType)
        (lifeEvent: LifeEvent)
        (traceContext: TraceContext)
        (response: TriggerSubscriptionResponse<'LifeAction, 'OpError>)
        : Option<GrainSideEffect<'LifeAction, 'OpError>> =
        match response with
        | TriggerSubscriptionResponse.Exn _
        | TriggerSubscriptionResponse.ActError _
        | TriggerSubscriptionResponse.ActNotAllowed _ -> false
        | TriggerSubscriptionResponse.ActOk action ->
            match lifeCycleAdapter.LifeCycle.ResponseHandler
                      (SideEffectResponse.Success
                           (SideEffectSuccess.ActOk (
                                { SubjectId = currentStateContainer.CurrentSubjectState.Subject.SubjectId
                                  LifeCycleKey = lifeCycleAdapter.LifeCycle.Def.LifeCycleKey }, action)))
                      |> Seq.tryHead with
            | None -> true
            | Some decision ->
                match decision with
                | SideEffectResponseDecision_ (Choice1Of2 success) ->
                    match success with
                    | SideEffectSuccessDecision.RogerThat -> true
                    | SideEffectSuccessDecision.Continue _ -> false
                | SideEffectResponseDecision_ (Choice2Of2 (_: SideEffectFailureDecision<_>)) ->
                    false
        |> fun isActOkWithDefaultResponseHandling ->
            if isActOkWithDefaultResponseHandling then
                // don't waste a side effect for ActOk that will be simply dismissed
                None
            else
                GrainPersistedSideEffect.HandleSubscriptionResponseOnSelf (response, triggerType, lifeEvent, traceContext)
                |> GrainSideEffect.Persisted
                |> Some

    let setTickState
            (currentStateContainer: SubjectCurrentStateContainer<'Subject, 'SubjectId, 'LifeEvent, 'LifeAction>)
            (tickState: TickState)
            (maybeReminderUpdate: Option<ReminderUpdate>)
            (maybeSideEffect: Option<GrainSideEffect<'LifeAction, 'OpError>>)
            : Task =
        task {
            match primaryStore.Value with
            | Some (SubjectStateContainer.Committed _) -> ()
            | Some (SubjectStateContainer.PreparedAction _)
            | Some (SubjectStateContainer.PreparedInitialize _)
            | None -> failwithf "unexpected setTickState when primary store is not Committed: %A" primaryStore.Value

            let sideEffectGroup =
                maybeSideEffect |> Option.toList |> makeNewSideEffectsGroup currentStateContainer.NextSideEffectSeqNum

            let nextSideEffectSeqNum = currentStateContainer.NextSideEffectSeqNum + (sideEffectGroup |> Option.map (fun _ -> 1UL) |> Option.defaultValue 0UL)
            let! newETag =
                grainStorageHandler.SetTickState grainPKey currentStateContainer.ETag nextSideEffectSeqNum tickState sideEffectGroup

            let subjectId = currentStateContainer.CurrentSubjectState.Subject.SubjectId

            do! applyReminderUpdateToGrain subjectId maybeReminderUpdate
            sideEffectGroup |> Option.iter (queueSideEffectGroupForAsyncProcessing subjectId)

            { currentStateContainer with
                CurrentSubjectState = { currentStateContainer.CurrentSubjectState with TickState = tickState }
                ETag = newETag
                NextSideEffectSeqNum = nextSideEffectSeqNum }
            |> SubjectStateContainer.Committed
            |> Some
            |> updatePrimaryStoreWithoutNotify
        }

    let setTickStateAndSubscriptions (currentStateContainer: SubjectCurrentStateContainer<'Subject, 'SubjectId, 'LifeEvent, 'LifeAction>)
        tickState subscriptions maybeReminderUpdate sideEffectGroup : Task =
        task {
            match primaryStore.Value with
            | Some (SubjectStateContainer.Committed _) -> ()
            | Some (SubjectStateContainer.PreparedAction _)
            | Some (SubjectStateContainer.PreparedInitialize _)
            | None -> failwithf "unexpected setTickStateAndSubscriptions when primary store is not Committed: %A" primaryStore.Value

            let nextSideEffectSeqNum = currentStateContainer.NextSideEffectSeqNum + (match sideEffectGroup with Some _ -> 1UL | None -> 0UL)
            let! newETag =
                grainStorageHandler.SetTickStateAndSubscriptions grainPKey currentStateContainer.ETag nextSideEffectSeqNum tickState subscriptions sideEffectGroup

            let subjectId = currentStateContainer.CurrentSubjectState.Subject.SubjectId
            do! applyReminderUpdateToGrain subjectId maybeReminderUpdate
            sideEffectGroup
            |> Option.iter (queueSideEffectGroupForAsyncProcessing subjectId)

            { currentStateContainer with
                CurrentSubjectState =
                    { currentStateContainer.CurrentSubjectState with
                        TickState = tickState
                        OurSubscriptions = subscriptions }
                ETag = newETag
                NextSideEffectSeqNum = nextSideEffectSeqNum }
            |> SubjectStateContainer.Committed
            |> Some |> updatePrimaryStoreWithoutNotify
        }

    let retryPermanentFailures (currentStateContainer: SubjectCurrentStateContainer<'Subject, 'SubjectId, 'LifeEvent, 'LifeAction>) sideEffectIdsToRetry
            : Task =
        task {
            match primaryStore.Value with
            | Some (SubjectStateContainer.Committed _) -> ()
            | Some (SubjectStateContainer.PreparedAction _)
            | Some (SubjectStateContainer.PreparedInitialize _)
            | None -> failwithf "unexpected retryPermanentFailures when primary store is not Committed: %A" primaryStore.Value

            let! failedSideEffectGroups =
                grainStorageHandler.ReadFailedSideEffects grainPKey sideEffectIdsToRetry

            // increase group numbers!
            match
                failedSideEffectGroups.Values
                |> Seq.sortBy (fun g -> g.SequenceNumber)
                |> Seq.mapi (fun i g -> { g with SequenceNumber = currentStateContainer.NextSideEffectSeqNum + uint64 i; RehydratedFromStorage = true })
                |> NonemptyKeyedSet.ofSeq
            with
            | None ->
                // nothing found
                ()
            | Some retryingSideEffectGroups ->
                let nextSideEffectSeqNum =
                    currentStateContainer.NextSideEffectSeqNum + (uint64 retryingSideEffectGroups.Count.Value)
                let! newETag =
                    grainStorageHandler.RetrySideEffects grainPKey currentStateContainer.ETag nextSideEffectSeqNum retryingSideEffectGroups

                let subjectId = currentStateContainer.CurrentSubjectState.Subject.SubjectId
                retryingSideEffectGroups.Values
                |> Seq.sortBy (fun g -> g.SequenceNumber)
                |> Seq.iter (queueSideEffectGroupForAsyncProcessing subjectId)

                { currentStateContainer with
                    ETag = newETag
                    NextSideEffectSeqNum = nextSideEffectSeqNum }
                |> SubjectStateContainer.Committed
                |> Some |> updatePrimaryStoreWithoutNotify
        }

    let assertStateConsistency () : Task =
        task {
            let expectedETagIfCreated =
                match primaryStore.Value with
                | None ->
                    None
                | Some (SubjectStateContainer.PreparedInitialize (_, eTag, _)) ->
                    Some eTag
                | Some (SubjectStateContainer.Committed currentStateContainer)
                | Some (SubjectStateContainer.PreparedAction (currentStateContainer, _, _)) ->
                    Some currentStateContainer.ETag

            do! grainStorageHandler.AssertStateConsistency grainPKey expectedETagIfCreated
            primaryStore.MarkAsConsistent ()
        }

    let awaitLifeEvent (lifeEvent: 'LifeEvent) (awaiter: ILifeEventAwaiter<'Subject,'LifeEvent, 'SubjectId>) (timeout: TimeSpan) =
        task {
            if timeout > TimeSpan.Zero then
                let! now = clock.Query Now
                match awaitersByLifeEvent.TryGetValue lifeEvent with
                | true, awaiters ->
                    awaitersByLifeEvent[lifeEvent] <- { Awaiter = awaiter; ExpiryTime = now.Add timeout}::awaiters
                | false, _ ->
                    awaitersByLifeEvent[lifeEvent] <- [{ Awaiter = awaiter; ExpiryTime = now.Add timeout}]

                this.CallRegisterTimerReentrant timeout (fun grainInterface -> grainInterface.InternalOnlyAwaitTimerExpired lifeEvent)
                |> ignore
        }

    // Caution! Shutdown must happen only during grain deactivation!
    // If grain stays alive and keeps accepting requests, stalled side effects will pile up and automatic ForceAwake won't help.
    let shutdownOnceSideEffectProcessor : Lazy<Task> =
        // Implemented as a lazy so it runs just once
        lazy(
            task {
                match maybeSideEffectProcessor with
                | Some sideEffectProcessor ->
                    try
                        do! sideEffectProcessor.PostAndAsyncReply SideEffectProcessorMessage.ShutdownSideEffectProcessor
                    finally
                        (sideEffectProcessor :> IDisposable).Dispose()
                | None ->
                    ()
            }
            |> Task.Ignore
        )

    let waitForPendingSideEffectsToFinish () : Task =
        task {
            match maybeSideEffectProcessor with
            | Some sideEffectProcessor ->
                do! sideEffectProcessor.PostAndAsyncReply SideEffectProcessorMessage.NoOpAndContinue
            | None ->
                ()
        }
        |> Task.Ignore

    let mutable maybeReminderScheduledAsTimerCancelHandle : Option<IDisposable> = None
    let cancelReminderScheduledAsTimer () =
        maybeReminderScheduledAsTimerCancelHandle |> Option.iter (fun disp -> disp.Dispose())
        maybeReminderScheduledAsTimerCancelHandle <- None

    let authorizeSubjectAndContinueWith
            (sessionHandle: SessionHandle)
            (callOrigin: CallOrigin)
            (accessEvent: AccessEvent<'Subject, 'LifeAction, 'Constructor, 'SubjectId>)
            (unauthorizedResult: 'T)
            (authorizedContinuation: unit -> Task<'T>)
            (maybeAuditor: Option<Auditor>): Task<'T> =
        let sessionSource =
            match sessionHandle with
            | SessionHandle.Session (sessId, _auditUserId) when lifeCycleAdapter.LifeCycle.IsSessionLifeCycle && sessId = grainPKey ->
                // Optimize ACL checks for Session lifecycle since we have Session available right here
                match primaryStore.Value with
                | Some (SubjectStateContainer.Committed currentStateContainer)
                | Some (SubjectStateContainer.PreparedAction (currentStateContainer, _, _)) ->
                    currentStateContainer.CurrentSubjectState.Subject
                    |> box
                    |> Some
                    |> SessionSource.InMemory
                | _ ->
                    SessionSource.InMemory None
            | _ -> SessionSource.Handle sessionHandle
        lifeCycleAdapter.LifeCycle.Invoke
            { new FullyTypedLifeCycleFunction<_, _, _, _, _, _, _> with
                member _.Invoke lifeCycle =
                    Subjects.authorizeSubjectAndContinueWith hostEcosystemGrainFactory grainPartition lifeCycle.MaybeApiAccess lifeCycle.SessionHandling sessionSource callOrigin accessEvent (fun () -> unauthorizedResult |> Task.FromResult) authorizedContinuation maybeAuditor
            }

    let createTraceContext () =
        let userId, sessionId = OrleansRequestContext.getTelemetryUserIdAndSessionId ()
        let parentId =
            RequestContext.Get LibLifeCycleCore.OrleansEx.TraceContextGrainCallFilter.ParentActivityIdKey :?> string
            |> fun id -> if id = null then "" else id
        { TelemetryUserId = userId; TelemetrySessionId = sessionId; ParentId = parentId }

    // Needed because F# can't access protected methods from within lambdas
    member private _.CallDelayDeactivation (timeSpan: TimeSpan) =
        base.DelayDeactivation timeSpan

    /// In order to guarantee re-entrancy, you MUST call an interface method on the grain parameter of the callback.
    /// Any other code that runs within the callback directly will not respect re-entracy, and could potentially be interleaved
    /// with other ongoing asynchronous grain calls. Single-threadedness is still guaranteed in the callback.
    member private _.CallRegisterTimerReentrant timeout (callback: ISubjectGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId> -> Task) : IDisposable =
        let mutable timerCancelHandle = { new IDisposable with member _.Dispose() = () }
        let timerCallback =
            Func<obj, Task>(
                fun _ ->
                    backgroundTask { // Timer callback runs on the threadpool  by default
                        timerCancelHandle.Dispose()
                        // Invoked timers don't respect grain reentrancy
                        // https://github.com/dotnet/orleans/issues/2574 (Make timer callbacks respect reentrancy of grains)
                        // This is a workaround
                        let asInterface = this.AsReference<ISubjectGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>>()
                        do! Task.startOnScheduler orleansTaskScheduler (fun _ -> callback asInterface |> Task.asUnit)
                    }
                    |> Task.Ignore
            )

        timerCancelHandle <- this.RegisterTimer(timerCallback, (), timeout, liveSubjectReminderMaxDelay)
        timerCancelHandle

    // used by SideEffectProcessor to check if its host grain is still alive, and drop the messages if it's not.
    member private _.IsGrainDeactivated () : bool =
        isGrainDeactivated

    // Needed because F# can't access protected methods from within lambdas
    member private _.UnregisterReminder(reminder: IGrainReminder) =
        base.UnregisterReminder reminder

    member private _.SetTickReminder (nextTickOn: DateTimeOffset) (now: DateTimeOffset) (assumedCurrent: AssumedCurrentReminder) : Task =
        let timeSpanCorrected =
            let timeSpan = nextTickOn - now + subjectReminderImplicitDelayToReduceEarlyTicks
            if timeSpan < liveSubjectReminderMinDelay then
                liveSubjectReminderMinDelay
            else if timeSpan > liveSubjectReminderMaxDelay then
                liveSubjectReminderMaxDelay
            else
                timeSpan

        match remindersImplementation with
        | RemindersImplementation.NotPersisted ->
            // For volatile subjects, simply register a timer. Timers aren't persisted, but that's OK, as volatile subjects
            // aren't persisted either (in fact, it's better, as we don't need to clean up after ourselves)
            cancelReminderScheduledAsTimer()
            maybeReminderScheduledAsTimerCancelHandle <-
                this.CallRegisterTimerReentrant timeSpanCorrected (fun grainInterface -> grainInterface.InternalOrTestingOnlyTriggerReminder())
                |> Some
            Task.CompletedTask

        | RemindersImplementation.Persisted ->
            // Optimization: spare the Reminder Service call if both assumed current and new ticks are far enough in the future to be picked up by Reminder Table refresh.
            // Note, that even though SubjectReminderTable has no-op writes, RegisterOrUpdateReminder is still potentially a cross-silo call.
            let currentTickIsPotentiallyApplied =
                match assumedCurrent with
                | AssumedCurrentReminder.NotSet -> false
                | AssumedCurrentReminder.Set assumedCurrentNextTickOn ->
                    assumedCurrentNextTickOn < now.Add liveSubjectReminderScheduleAheadLimit
                | AssumedCurrentReminder.Unknown -> true
            let newTickNeedToBeApplied = timeSpanCorrected < liveSubjectReminderScheduleAheadLimit

            // TODO move short term ticks as timers that are much more lightweight? See NotPersisted case handler
            // We already have SubjectReminderTable as a reminders backup, and Meta stalled timers detection,
            // but recovery interval is long (up to subjectReminderTableRefreshIntervalWithSafePadding), not good enough.
            // In newer Orleans versions, Reminder Table refresh interval is configurable, maybe reduce it to keep fewer live reminders
            match currentTickIsPotentiallyApplied, newTickNeedToBeApplied with
            | _, true ->
                this.RegisterOrUpdateReminder(SubjectReminderName, timeSpanCorrected, defaultReminderPeriod) |> Task.Ignore
            | true, false ->
                let reminder = getSubjectGrainReminderToUnregister this
                this.UnregisterReminder reminder
            | false, false ->
                Task.CompletedTask

        | RemindersImplementation.TestNotPersistedManuallyTriggered ->
            // in SUT there's simply nothing to do, all timers must be triggered manually via hook
            Task.CompletedTask

    member private _.ClearTickReminder (assumedCurrent: AssumedCurrentReminder) : Task=
        task {
            match remindersImplementation with
            | RemindersImplementation.NotPersisted ->
                cancelReminderScheduledAsTimer()
            | RemindersImplementation.Persisted ->
                let! now = clock.Query Now
                match assumedCurrent with
                // Optimization: spare the Reminder Service call if already assumed empty or far enough in the future to be cleared by Reminder Table refresh
                | AssumedCurrentReminder.NotSet -> ()
                | AssumedCurrentReminder.Set assumedCurrentNextTickOn
                    when assumedCurrentNextTickOn > now.Add liveSubjectReminderScheduleAheadLimit -> ()
                | AssumedCurrentReminder.Set _ | AssumedCurrentReminder.Unknown ->
                    let reminder = getSubjectGrainReminderToUnregister this
                    do! this.UnregisterReminder reminder
            | RemindersImplementation.TestNotPersistedManuallyTriggered ->
                ()
        }

    member private _.CallBaseOnActivateAsync() =
        base.OnActivateAsync()

    member private _.CallBaseOnDeactivateAsync() =
        base.OnDeactivateAsync()

    override this.OnActivateAsync() =
        orleansTaskScheduler <- TaskScheduler.Current

        task {
            match lifeCycleAdapter.LifeCycle.Storage.Type with
            | StorageType.Volatile ->
                Noop

            | StorageType.Persistent _
            | StorageType.Custom _ ->
                match! grainStorageHandler.TryReadState grainPKey with
                | Some readStateResult ->
                    readStateResult.SubjectStateContainer
                    |> Some
                    |> updatePrimaryStoreWithoutNotify

                    match readStateResult.PersistedGrainIdHash with
                    | None ->
                        ()
                    | Some persistedHash ->
                        match grainStorageHandler with
                        | :? SqlServerGrainStorageHandler<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId> as sqlServerGrainStorageHandler ->
                            // perform GrainIdHash repair only for SQL server storage
                            let expectedHash = getGrainIdHash ()
                            if persistedHash <> expectedHash then
                                    match readStateResult.SubjectStateContainer with
                                    | SubjectStateContainer.PreparedInitialize _ ->
                                        ()
                                    | SubjectStateContainer.PreparedAction (current, prepared, tId) ->
                                        let! newConcurrencyToken = sqlServerGrainStorageHandler.RepairGrainIdHash current.CurrentSubjectState.Subject.SubjectId.IdString current.ETag expectedHash
                                        SubjectStateContainer.PreparedAction ({ current with ETag = newConcurrencyToken }, prepared, tId)
                                        |> Some
                                        |> updatePrimaryStoreWithoutNotify
                                    | SubjectStateContainer.Committed current ->
                                        let! newConcurrencyToken = sqlServerGrainStorageHandler.RepairGrainIdHash current.CurrentSubjectState.Subject.SubjectId.IdString current.ETag expectedHash
                                        SubjectStateContainer.Committed { current with ETag = newConcurrencyToken }
                                        |> Some
                                        |> updatePrimaryStoreWithoutNotify
                        | _ -> ()

                    KeyedSet.toSeq readStateResult.PendingSideEffects
                    |> Seq.sortBy (fun sideEffectGroup -> sideEffectGroup.SequenceNumber)
                    |> Seq.iter (queueSideEffectGroupForAsyncProcessing readStateResult.SubjectStateContainer.SubjectId)

                | None ->
                    updatePrimaryStoreWithoutNotify None

            do! this.CallBaseOnActivateAsync()
        } |> Task.Ignore

    override _.OnDeactivateAsync() =
        task {
            if observerManager.Count <> 0 then
                logger.Error "Grain deactivating despite having %a observers" (logger.P "observerCount") observerManager.Count

            let shutdownTask = shutdownOnceSideEffectProcessor.Force()
            match! Task.WhenAny(shutdownTask, Task.Delay 5000) with
            | task when task = shutdownTask ->
                ()
            | _ ->
                logger.Warn "Grain is deactivating, but side-effect processor didn't shut down after waiting for 5 seconds"

            do! this.CallBaseOnDeactivateAsync()

            isGrainDeactivated <- true
        } |> Task.Ignore

    // duplicate grain activations lead to stalled side effects and phantom errors caused by outdated state
    // dummy assert of consistency can reveal a zombie grain and deactivate it before too much damage is done
    member private _.WithConsistencyCheckOnError (f: Task<Result<'Ok, 'Err>>) : Task<Result<'Ok, 'Err>> =
        task {
            match! f with
            | Ok res ->
                // happy path probably already performed storage write with ETag update i.e. this is not a zombie
                return Ok res
            | Error err ->
                // Error path doesn't write to storage i.e. doesn't check for consistency so just do it.
                do! assertStateConsistency ()
                return Error err
        }

    member private _.InvokeTryDedup
            (maybeDedupInfo: Option<SideEffectDedupInfo>)
            // noop may not resolve to the very same state (e.g. concurrent actions were applied between retries)
            // but it's fine because ultimate consumers reduced to Result<unit, _> anyway
            (resolveNoopResult: SubjectStateContainer<_, _, _, _, _, _> -> 'T)
            (invoke: unit -> Task<'T>)
            : Task<'T> =
        match maybeDedupInfo, primaryStore.Value with
        // if created and requested to dedup then try to do so
        | Some dedupInfo, Some ((SubjectStateContainer.Committed currentStateContainer) as stateContainer)
        | Some dedupInfo, Some ((SubjectStateContainer.PreparedAction (currentStateContainer, _, _)) as stateContainer) ->
            match currentStateContainer.SideEffectDedupCache.TryGetValue dedupInfo.Caller with
            | true, (lastSideEffectId, _) when lastSideEffectId = dedupInfo.Id ->
                stateContainer
                |> resolveNoopResult
                |> Task.FromResult
            | false, _
            | true, _ ->
                invoke ()
        // no dedup if not initialized yet or not requested
        | _, None
        | None, _
        | _, Some (SubjectStateContainer.PreparedInitialize _) ->
            invoke ()

    member private _.Subscribe (subscriptionNamesToEvents: Map<SubscriptionName, LifeEvent>) (subscriber: SubjectPKeyReference)
        : Task<Result<VersionedSubject<'Subject, 'SubjectId>, Choice<GrainSubscriptionError, Exception>>> =
        task {
            match primaryStore.Value with
            | None
            | Some (SubjectStateContainer.PreparedInitialize _) ->
                logger.Info "SUBSCRIBE %a ==> SUBSCRIBER %a ==> Error SubjectNotInitialized" (logger.P "subscriptions") subscriptionNamesToEvents (logger.P "subscriber") subscriber
                return GrainSubscriptionError.SubjectNotInitialized grainPKey |> Choice1Of2 |> Error

            | Some (SubjectStateContainer.Committed currentStateContainer)
            // allow to subscribe to subject in transaction because why not?
            | Some (SubjectStateContainer.PreparedAction (currentStateContainer, _, _)) ->
                match addToOthersSubscribing currentStateContainer.CurrentOthersSubscribing subscriptionNamesToEvents subscriber with
                | None ->
                    // Nothing to do
                    do! assertStateConsistency ()
                    logger.Info "SUBSCRIBE %a ==> SUBSCRIBER %a ==> NOOP" (logger.P "subscriptions") subscriptionNamesToEvents (logger.P "subscriber") subscriber
                    return Ok currentStateContainer.VersionedSubject

                | Some (updatedOthersSubscribing, newSubscriptions) ->
                    logger.Info "SUBSCRIBE %a ==> SUBSCRIBER %a ==> OK" (logger.P "subscriptions") subscriptionNamesToEvents (logger.P "subscriber") subscriber
                    let subscriptionsToAdd = { Subscriber = subscriber; NewSubscriptions = newSubscriptions }
                    match!
                        fun () -> addSubscriptions currentStateContainer updatedOthersSubscribing subscriptionsToAdd
                        |> catchExnButRethrowTransientIfInternalCall_OnSimpleTask CallOrigin.Internal with
                    | Error exn ->
                        return exn |> Choice2Of2 |> Error
                    | Ok versionedSubject ->
                        return Ok versionedSubject
        }



    member private this.RunActionOnCurrentStateContainer
        (currentStateContainer: SubjectCurrentStateContainer<'Subject, 'SubjectId, 'LifeEvent, 'LifeAction>)
        (context: ActContext)
        (action: 'LifeAction)
        (serviceScope: IServiceScope)
        : Task<Result<VersionedSubject<'Subject, 'SubjectId>, TransitionBuilderError<'OpError>>> =
        this.InvokeTryDedup
            context.MaybeDedupInfo
             (function
                 SubjectStateContainer.Committed current
                 | SubjectStateContainer.PreparedAction (current, _, _) ->
                     current.VersionedSubject |> Ok
                 | _ -> failwith "unexpected")
            (fun () ->
                task {
                    let! now = clock.Query Now
                    let traceContext = createTraceContext ()

                    let currentStateContainer, maybeSubscriptionsToAdd =
                        optional {
                            let! s = context.MaybeBeforeActSubscriptions
                            let! r = addToOthersSubscribing currentStateContainer.CurrentOthersSubscribing s.Subscriptions s.Subscriber
                            return s, r
                        }
                        |> function
                            | None ->
                                currentStateContainer, None
                            | Some (s, (updatedOthersSubscribing, newSubscriptions)) ->
                                { currentStateContainer with CurrentOthersSubscribing = updatedOthersSubscribing },
                                Some { Subscriber = s.Subscriber; NewSubscriptions = newSubscriptions }

                    let! processActionResult = processAction now traceContext currentStateContainer.NextSideEffectSeqNum currentStateContainer.CurrentSubjectState currentStateContainer.CurrentOthersSubscribing action context.CallOrigin serviceScope.ServiceProvider
                    match maybeSubscriptionsToAdd, processActionResult with
                    | _, Ok (Choice1Of2 (updatedState, maybeReminderUpdate, blobActions, sideEffectGroup, indexActions, raisedLifeEvents)) ->
                        let updateData =
                            {
                                DataToUpdate = {
                                    UpdatedSubjectState = updatedState
                                    TraceContext        = traceContext
                                    ReminderUpdate      = maybeReminderUpdate
                                    NextSideEffectSeq   = currentStateContainer.NextSideEffectSeqNum + (match sideEffectGroup with Some _ -> 1UL | None -> 0UL)
                                    SideEffectGroup     = sideEffectGroup
                                    BlobActions         = blobActions
                                    IndexActions        = indexActions
                                }
                                ActionThatCausedUpdate  = action
                                SubscriptionsToAdd      = maybeSubscriptionsToAdd
                                ExpectedETag            = currentStateContainer.ETag
                                CurrentVersion          = currentStateContainer.Version
                                SkipHistory             = currentStateContainer.SkipHistoryOnNextOp
                            }

                        match!
                            fun () -> updateSubject now raisedLifeEvents context.MaybeDedupInfo currentStateContainer updateData
                            |> catchExnButRethrowTransientIfInternalCall_OnResultTask context.CallOrigin with
                        | Ok res ->
                            return Ok res.VersionedSubject
                        | Error (Choice1Of2 err) -> return err |> LifeCycleError |> Error
                        | Error (Choice2Of2 err) -> return err |> LifeCycleException |> Error

                    | Some subscriptionsToAdd, Ok (Choice2Of2 None) ->
                        // no-op transition with subscriptions and without new tick state - just use addSubscription
                        match!
                            fun () -> addSubscriptions currentStateContainer currentStateContainer.CurrentOthersSubscribing subscriptionsToAdd
                            |> catchExnButRethrowTransientIfInternalCall_OnSimpleTask context.CallOrigin with
                        | Error exn ->
                            return exn |> LifeCycleException |> Error
                        | Ok versionedSubject ->
                            return Ok versionedSubject

                    | Some subscriptionsToAdd, Ok (Choice2Of2 (Some (tickState, reminderUpdate))) ->
                        // noop transition with subscriptions and new tick state - piggyback on existing updateSubject.
                        // // TODO: should be a separate function?
                        let updatedSubjectState = { currentStateContainer.CurrentSubjectState with TickState = tickState }

                        let updateData =
                            {
                                DataToUpdate = {
                                    UpdatedSubjectState = updatedSubjectState
                                    TraceContext        = traceContext
                                    ReminderUpdate      = Some reminderUpdate
                                    NextSideEffectSeq   = currentStateContainer.NextSideEffectSeqNum
                                    SideEffectGroup     = None
                                    BlobActions         = []
                                    IndexActions        = []
                                }
                                // TODO: find a way to not overwrite previous action, it's a no-op
                                ActionThatCausedUpdate  = action
                                SubscriptionsToAdd      = Some subscriptionsToAdd
                                ExpectedETag            = currentStateContainer.ETag
                                CurrentVersion          = currentStateContainer.Version
                                SkipHistory             = currentStateContainer.SkipHistoryOnNextOp
                            }

                        match!
                            fun () -> updateSubject now [] context.MaybeDedupInfo currentStateContainer updateData
                            |> catchExnButRethrowTransientIfInternalCall_OnResultTask context.CallOrigin with
                        | Ok res ->
                            return Ok res.VersionedSubject
                        | Error (Choice1Of2 err) -> return err |> LifeCycleError |> Error
                        | Error (Choice2Of2 err) -> return err |> LifeCycleException |> Error

                    | None, Ok (Choice2Of2 (Some (tickState, reminderUpdate))) ->
                        // noop transition with new tick state & without subscriptions
                        // Notes: DedupInfo not written but second attempt (if any) going to be a "true" noop anyway
                        let! res =
                            fun () -> setTickState currentStateContainer tickState (Some reminderUpdate) None
                            |> catchExnButRethrowTransientIfInternalCall_OnVoidTask context.CallOrigin
                        return
                            res
                            |> Result.mapBoth
                                (fun _ -> currentStateContainer.VersionedSubject)
                                (fun exn -> exn |> LifeCycleException)

                    | None, Ok (Choice2Of2 None) ->
                        // noop transition without new tick state nor subscriptions. A "true" noop
                        do! assertStateConsistency ()
                        return Ok currentStateContainer.VersionedSubject

                    | None, Error err ->
                        return err |> Error

                    | Some subscriptionsToAdd, Error err ->
                        // transition errored but there are subscriptions? still must add them
                        match!
                            fun () -> addSubscriptions currentStateContainer currentStateContainer.CurrentOthersSubscribing subscriptionsToAdd
                            |> catchExnButRethrowTransientIfInternalCall_OnSimpleTask context.CallOrigin with
                        | Error subscribeExn ->
                            // have to choose only one error of two, prefer exception: unlike 'OpError it can't be compensated or suppressed
                            // which is desirable in this scenario, so subscription does not get lost
                            match err with
                            | LifeCycleException actionExn ->
                                return actionExn |> LifeCycleException |> Error
                            | LifeCycleError _
                            | TransitionNotAllowed _ ->
                                return subscribeExn |> LifeCycleException |> Error
                        | Ok _ ->
                            return Error err
                })

    member private this.RunPrepareActionOnCurrentStateContainer
        (transactionId: SubjectTransactionId)
        (currentStateContainer: SubjectCurrentStateContainer<'Subject, 'SubjectId, 'LifeEvent, 'LifeAction>)
        (action: 'LifeAction)
        (serviceScope: IServiceScope)
        : Task<Result<unit, Choice<GrainPrepareTransitionError<'OpError>, Exception>>> =
        task {
            let context = ActContext.InterGrain (None, None) // no need to dedup on prepare, it's already idempotent
            let! now = clock.Query Now
            let traceContext = createTraceContext ()

            match! processAction now traceContext currentStateContainer.NextSideEffectSeqNum currentStateContainer.CurrentSubjectState currentStateContainer.CurrentOthersSubscribing action context.CallOrigin serviceScope.ServiceProvider with
            | Ok processRes ->
                let updateData, raisedLifeEvents =
                    match processRes with
                    | Choice1Of2 (updatedState, maybeReminderUpdate, blobActions, sideEffectGroup, indexActions, raisedLifeEvents) ->
                        {
                            DataToUpdate = {
                                UpdatedSubjectState = updatedState
                                TraceContext        = traceContext
                                ReminderUpdate      = maybeReminderUpdate
                                NextSideEffectSeq   = currentStateContainer.NextSideEffectSeqNum + (match sideEffectGroup with Some _ -> 1UL | None -> 0UL)
                                SideEffectGroup     = sideEffectGroup
                                BlobActions         = blobActions
                                IndexActions        = indexActions
                            }
                            ActionThatCausedUpdate  = action
                            SubscriptionsToAdd      = None
                            ExpectedETag            = currentStateContainer.ETag
                            CurrentVersion          = currentStateContainer.Version
                            SkipHistory             = currentStateContainer.SkipHistoryOnNextOp
                        },
                        raisedLifeEvents
                    | Choice2Of2 maybeTickStateAndReminderUpdate ->
                        // no-op inside transaction? still write prepared state to lock it.
                        // TODO: consider no-op commit which writes nothing to main table or history
                        let currentState =
                            match maybeTickStateAndReminderUpdate with
                            | None ->
                                currentStateContainer.CurrentSubjectState
                            | Some (tickState, _) ->
                                { currentStateContainer.CurrentSubjectState with TickState = tickState }
                        {
                            DataToUpdate = {
                                UpdatedSubjectState = currentState
                                TraceContext        = traceContext
                                ReminderUpdate      = maybeTickStateAndReminderUpdate |> Option.map snd
                                NextSideEffectSeq   = currentStateContainer.NextSideEffectSeqNum
                                SideEffectGroup     = None
                                BlobActions         = []
                                IndexActions        = []
                            }
                            ActionThatCausedUpdate  = action
                            SubscriptionsToAdd      = None
                            ExpectedETag            = currentStateContainer.ETag
                            CurrentVersion          = currentStateContainer.Version
                            SkipHistory             = currentStateContainer.SkipHistoryOnNextOp
                        },
                        []

                match updateData.TryCastToPrepared raisedLifeEvents with
                | Error PrepareWriteDataError.TransientSideEffectsNotAllowed ->
                    return TransientSideEffectsNotAllowedInTransaction |> Choice2Of2 |> Error
                | Ok (preparedUpdateData, uniqueIndicesToReserve) ->
                    match!
                        fun () -> prepareUpdateSubject currentStateContainer preparedUpdateData uniqueIndicesToReserve transactionId
                        |> catchExnButRethrowTransientIfInternalCall_OnResultTask context.CallOrigin with
                    | Ok () -> return Ok ()
                    | Error (Choice1Of2 err) -> return err |> GrainPrepareTransitionError.TransitionError |> Choice1Of2 |> Error
                    | Error (Choice2Of2 exn) -> return exn |> Choice2Of2 |> Error

            | Error err ->
                return
                    match err with
                    | LifeCycleError err -> err |> GrainPrepareTransitionError.TransitionError |> Choice1Of2 |> Error
                    | TransitionNotAllowed -> GrainPrepareTransitionError.TransitionNotAllowed |> Choice1Of2 |> Error
                    | LifeCycleException exn -> exn |> Choice2Of2 |> Error
        }

        member private _.RunCommitPreparedOnStateContainer (stateContainer: SubjectStateContainer<'Subject, 'Constructor, 'SubjectId, 'LifeEvent, 'LifeAction, 'OpError>) (transactionId: SubjectTransactionId) : Task =
            task {
                let traceContext = createTraceContext ()
                match stateContainer with
                | SubjectStateContainer.Committed _ ->
                    do! assertStateConsistency ()
                    // probably just an odd retry after success, should be safe to dismiss
                    logger.Info "COMMIT PREPARED ==> ALREADY COMMITTED. NOOP"
                    return Ok ()

                | SubjectStateContainer.PreparedAction (_, _, preparedTransactionId)
                | SubjectStateContainer.PreparedInitialize (_, _, preparedTransactionId)
                    when transactionId <> preparedTransactionId ->
                    do! assertStateConsistency ()
                    logger.Info "COMMIT PREPARED ==> ALREADY COMMITTED AND PREPARED BY ANOTHER TXN: %a. NOOP" (logger.P "txnId") preparedTransactionId
                    return Ok ()

                | SubjectStateContainer.PreparedAction (currentStateContainer, preparedState, transactionId) ->
                    // deliberately NOT treating non-transient exceptions as permanent: Commit must succeed, whatever it takes
                    do! commitUpdateSubject traceContext.ParentId currentStateContainer preparedState transactionId
                    if shouldSendActionTelemetry preparedState.ActionThatCausedUpdate then
                        logger.Info "COMMIT PREPARED ==> OK"
                    return Ok ()

                | SubjectStateContainer.PreparedInitialize (preparedState, eTag, transactionId) ->
                    // deliberately NOT treating non-transient exceptions as permanent: Commit must succeed, whatever it takes
                    do! commitInitializeSubject traceContext.ParentId preparedState transactionId eTag
                    if shouldSendConstructorTelemetry preparedState.ConstructorThatCausedInsert then
                        logger.Info "COMMIT PREPARED ==> OK"
                    return Ok ()
            }
            |> Task.Ignore

        member private _.RunRollbackPreparedOnStateContainer (stateContainer: SubjectStateContainer<'Subject, 'Constructor, 'SubjectId, 'LifeEvent, 'LifeAction, 'OpError>) (transactionId: SubjectTransactionId) : Task =
            task {
                match stateContainer with
                | SubjectStateContainer.Committed _ ->
                    do! assertStateConsistency ()
                    logger.Info "ROLLBACK PREPARED ==> ALREADY ROLLED BACK. NOOP"

                | SubjectStateContainer.PreparedAction (_, _, preparedTransactionId)
                | SubjectStateContainer.PreparedInitialize (_, _, preparedTransactionId)
                    when transactionId <> preparedTransactionId ->
                    do! assertStateConsistency ()
                    logger.Info "ROLLBACK PREPARED ==> ALREADY ROLLED BACK AND PREPARED BY ANOTHER TXN: %a. NOOP" (logger.P "txnId") preparedTransactionId

                | SubjectStateContainer.PreparedAction (currentStateContainer, preparedState, transactionId) ->
                    do! rollbackUpdateSubject currentStateContainer preparedState transactionId

                | SubjectStateContainer.PreparedInitialize (preparedState, eTag, transactionId) ->
                    do! rollbackPrepareInitialize preparedState transactionId eTag
                    if shouldSendConstructorTelemetry preparedState.ConstructorThatCausedInsert then
                        logger.Info "ROLLBACK PREPARED ==> OK"
            }
            |> Task.Ignore

        member private _.RunCheckPhantomPreparedOnStateContainer (stateContainer: SubjectStateContainer<'Subject, 'Constructor, 'SubjectId, 'LifeEvent, 'LifeAction, 'OpError>) (transactionId: SubjectTransactionId) : Task =
            task {
                // this has identical behavior to RunRollbackPreparedOnStateContainer but has opposite expectation i.e.
                // nothing should be prepared. Emits warning otherwise, but still rolls back
                match stateContainer with
                | SubjectStateContainer.Committed _ ->
                    do! assertStateConsistency ()
                | SubjectStateContainer.PreparedAction (_, _, preparedTransactionId)
                | SubjectStateContainer.PreparedInitialize (_, _, preparedTransactionId)
                    when transactionId <> preparedTransactionId ->
                    do! assertStateConsistency ()

                | SubjectStateContainer.PreparedAction (currentStateContainer, preparedState, preparedTransactionId) ->
                    do! rollbackUpdateSubject currentStateContainer preparedState preparedTransactionId
                    logger.Warn "ROLLED BACK PHANTOM PREPARED ACTION ==> OK. Transaction: %a" (logger.P "txnId") preparedTransactionId

                | SubjectStateContainer.PreparedInitialize (preparedState, eTag, preparedTransactionId) ->
                    // deliberately NOT treating non-transient exceptions as permanent: Rollback must succeed, whatever it takes
                    do! rollbackPrepareInitialize preparedState preparedTransactionId eTag
                    logger.Warn "ROLLED BACK PHANTOM PREPARED INITIALIZE ==> OK. Transaction: %a" (logger.P "txnId") preparedTransactionId
            }
            |> Task.Ignore

    member private _.RunAction (context: ActContext) (action: 'LifeAction) (serviceScope: IServiceScope) : Task<Result<VersionedSubject<'Subject, 'SubjectId>, Choice<GrainTransitionError<'OpError>, Exception>>> =
        task {
            match primaryStore.Value with
            | Some (SubjectStateContainer.Committed currentStateContainer) ->
                return!
                    authorizeSubjectAndContinueWith
                        context.SessionHandle
                        context.CallOrigin
                        (AccessEvent.Act(currentStateContainer.VersionedSubject.Subject, action))
                        (GrainTransitionError.AccessDenied |> Choice1Of2 |> Error)
                        (fun () ->
                            task {
                                match! this.RunActionOnCurrentStateContainer currentStateContainer context action serviceScope with
                                | Ok updatedSubj ->
                                    if shouldSendActionTelemetry action then
                                        logger.Info "ACT %a ==> OK" (logger.P "action") action
                                    return Ok updatedSubj
                                | Error (LifeCycleError err) ->
                                    (if context.ShouldWarnOnOpError then logger.Warn else logger.Info) "ACT %a ==> ERROR %a" (logger.P "action") action (logger.P "error") err
                                    return err |> GrainTransitionError.TransitionError |> Choice1Of2 |> Error
                                | Error TransitionNotAllowed ->
                                    logger.Warn "ACT %a ==> ERROR TransitionNotAllowed" (logger.P "action") action
                                    return GrainTransitionError.TransitionNotAllowed |> Choice1Of2 |> Error
                                | Error (LifeCycleException ex) ->
                                    logger.ErrorExn ex "ACT %a ==> ERROR EXCEPTION" (logger.P "action") action
                                    return ex |> Choice2Of2 |> Error
                            }
                        )
                        None

            | Some (SubjectStateContainer.PreparedAction _) ->
                // Should we still subscribe MaybeBeforeActSubscriptions mid-transaction just like Subscribe does?
                // Probably not. Id would break atomicity and result in duplicate SubscribeOk response handling.
                logger.Warn "ACT %a ==> LockedInTransaction" (logger.P "action") action
                return GrainTransitionError.LockedInTransaction |> Choice1Of2 |> Error

            | Some (SubjectStateContainer.PreparedInitialize _)
            | None ->
                logger.Warn "ACT %a ==> SubjectNotInitialized" (logger.P "action") action
                return grainPKey |> GrainTransitionError.SubjectNotInitialized |> Choice1Of2 |> Error
        }
        |> this.WithConsistencyCheckOnError

    member private _.RunPrepareAction (action: 'LifeAction) (transactionId: SubjectTransactionId) (serviceScope: IServiceScope) : Task<Result<unit, SubjectFailure<GrainPrepareTransitionError<'OpError>>>> =
        task {
            match primaryStore.Value with
            | Some (SubjectStateContainer.PreparedAction (_, prepared, preparedInTransactionId))
                when preparedInTransactionId = transactionId && Object.Equals(prepared.ActionThatCausedUpdate, action) ->
                // idempotency / Noop
                return Ok ()

            | Some (SubjectStateContainer.PreparedAction (_, _, transactionId)) ->
                logger.Warn "ACT TRANS %a ==> ConflictingPrepare" (logger.P "action") action
                return transactionId |> GrainPrepareTransitionError.ConflictingPrepare |> SubjectFailure.Err |> Error

            | Some (SubjectStateContainer.Committed currentStateContainer) ->
                match! this.RunPrepareActionOnCurrentStateContainer transactionId currentStateContainer action serviceScope with
                | Ok () ->
                    if shouldSendActionTelemetry action then
                        logger.Info "ACT TRANS %a ==> OK" (logger.P "action") action
                    return Ok ()
                | Error (Choice1Of2 err) ->
                    logger.Warn "ACT TRANS %a ==> ERROR %a" (logger.P "action") action (logger.P "error") err
                    return err |> SubjectFailure.Err |> Error
                | Error (Choice2Of2 exn) ->
                    logger.ErrorExn exn "ACT TRANS %a ==> ERROR EXCEPTION" (logger.P "action") action
                    return exn.ToString() |> SubjectFailure.Exn |> Error

            | Some (SubjectStateContainer.PreparedInitialize _)
            | None ->
                logger.Warn "ACT TRANS %a ==> SubjectNotInitialized" (logger.P "action") action
                return grainPKey |> GrainPrepareTransitionError.SubjectNotInitialized |> SubjectFailure.Err |> Error
        }
        |> this.WithConsistencyCheckOnError

        member private this.Construct (context: ConstructContext) (subjectId: 'SubjectId) (ctor: 'Constructor) (serviceScope: IServiceScope) : Task<Result<VersionedSubject<'Subject, 'SubjectId>, Choice<GrainConstructionError<'OpError>, Exception>>> =
            authorizeSubjectAndContinueWith
                context.SessionHandle
                context.CallOrigin
                (AccessEvent.Construct ctor)
                (GrainConstructionError.AccessDenied |> Choice1Of2 |> Error)
                (fun () ->
                    // one more dedup indirection to eliminate SubjectAlreadyInitialized. Can we do better?
                    this.InvokeTryDedup
                        context.MaybeDedupInfo
                        (function
                            SubjectStateContainer.Committed current
                            | SubjectStateContainer.PreparedAction (current, _, _) ->
                                current.VersionedSubject |> Ok
                            | SubjectStateContainer.PreparedInitialize _ -> failwith "unexpected prepared initialize on dedup")
                        (fun () ->
                    task {
                        match primaryStore.Value with
                        | Some subjectStateContainer ->
                            match context with
                            | ConstructContext.External _
                            | ConstructContext.InterGrain (_, _, (* okIfAlreadyInitialized *) false) ->
                                logger.Warn "CTOR %a ==> SubjectAlreadyInitialized" (logger.P "ctor") ctor
                                match context.MaybeConstructSubscriptions with
                                | Some constructSubscriptions when constructSubscriptions.Subscriptions.IsNonempty ->
                                    // already created, but still need to add some subscriptions
                                    match! this.Subscribe constructSubscriptions.Subscriptions constructSubscriptions.Subscriber with
                                    | Ok _
                                    | Error (Choice1Of2 (_: GrainSubscriptionError)) ->
                                        return GrainConstructionError.SubjectAlreadyInitialized grainPKey |> Choice1Of2 |> Error
                                    | Error (Choice2Of2 exn) ->
                                        // have to choose only one error of two, prefer exception: unlike AlreadyInitialized it can't be compensated or suppressed
                                        // which is desirable in this scenario, so subscription does not get lost
                                        return exn |> Choice2Of2 |> Error
                                | Some _ // when constructSubscriptions.Subscriptions.IsEmpty
                                | None ->
                                    return GrainConstructionError.SubjectAlreadyInitialized grainPKey |> Choice1Of2 |> Error

                            | ConstructContext.InterGrain (_, maybeConstructSubscriptions, (* okIfAlreadyInitialized *) true) ->
                                match subjectStateContainer with
                                | SubjectStateContainer.PreparedInitialize _ ->
                                    return InvalidOperationException "Subject with same id being created in transaction" |> raise
                                | SubjectStateContainer.PreparedAction (currentStateContainer, _, _)
                                | SubjectStateContainer.Committed currentStateContainer ->
                                    match maybeConstructSubscriptions with
                                    | Some constructSubscriptions when constructSubscriptions.Subscriptions.IsNonempty ->
                                        // already created, but still need to add some subscriptions
                                        match! this.Subscribe constructSubscriptions.Subscriptions constructSubscriptions.Subscriber with
                                        | Ok versionedSubject ->
                                            return versionedSubject |> Ok

                                        | Error (Choice1Of2 (GrainSubscriptionError.SubjectNotInitialized _)) ->
                                            // Impossible since we checked for it above
                                            return currentStateContainer.VersionedSubject |> Ok

                                        | Error (Choice2Of2 exn) ->
                                            return exn |> Choice2Of2 |> Error

                                    | Some _ // when constructSubscriptions.Subscriptions.IsEmpty
                                    | None ->
                                        return currentStateContainer.VersionedSubject |> Ok

                        | None ->
                            match! this.ForceInitialize ctor subjectId context serviceScope with
                            | Ok (SubjectStateContainer.Committed currentStateContainer) ->
                                if shouldSendConstructorTelemetry ctor then
                                    logger.Info "CTOR %a ==> OK" (logger.P "ctor") ctor
                                return currentStateContainer.VersionedSubject |> Ok

                            | Error (Choice1Of2 err) ->
                                logger.Warn "CTOR %a ==> ERROR %a" (logger.P "ctor") ctor (logger.P "error") err
                                this.CallDeactivateOnIdle()
                                return err |> GrainConstructionError.ConstructionError |> Choice1Of2 |> Error

                            | Error (Choice2Of2 ex) ->
                                logger.ErrorExn ex "CTOR %a ==> ERROR EXCEPTION" (logger.P "ctor") ctor
                                this.CallDeactivateOnIdle()
                                return ex |> Choice2Of2 |> Error

                            | Ok (SubjectStateContainer.PreparedInitialize _)
                            | Ok (SubjectStateContainer.PreparedAction _) ->
                                return InvalidOperationException "ForceInitialize must not create subject in transaction" |> raise
                    }))
                None
            |> this.WithConsistencyCheckOnError

    member private _.RunCommitPrepared (transactionId: SubjectTransactionId) : Task =
        task {
            match primaryStore.Value with
            | Some currentStateContainer ->
                do! this.RunCommitPreparedOnStateContainer currentStateContainer transactionId
            | None ->
                // probably just an odd retry after success followed by deletion, should be safe to dismiss
                do! assertStateConsistency ()
                logger.Info "COMMIT PREPARED ==> ALREADY COMMITTED (SubjectNotInitialized). NOOP"
        }
        |> Task.Ignore

    member private _.RunRollbackPrepared (transactionId: SubjectTransactionId) : Task =
        task {
            match primaryStore.Value with
            | Some currentStateContainer ->
                do! this.RunRollbackPreparedOnStateContainer currentStateContainer transactionId
            | None ->
                // probably just an odd retry after success, should be safe to dismiss
                do! assertStateConsistency ()
                logger.Info "ROLLBACK PREPARED ==> ALREADY ROLLED BACK (SubjectNotInitialized). NOOP"
        }
        |> Task.Ignore

    member private _.RunCheckPhantomPrepared (transactionId: SubjectTransactionId) : Task =
        task {
            match primaryStore.Value with
            | None ->
                do! assertStateConsistency ()
            | Some currentStateContainer ->
                do! this.RunCheckPhantomPreparedOnStateContainer currentStateContainer transactionId
        }
        |> Task.Ignore

    member private _.ForceInitialize
        (ctor: 'Constructor)
        (subjectId: 'SubjectId)
        (context: ConstructContext)
        (serviceScope: IServiceScope)
        : Task<Result<SubjectStateContainer<'Subject, 'Constructor, 'SubjectId, 'LifeEvent, 'LifeAction, 'OpError>, Choice<'OpError, Exception>>> =

        this.InvokeTryDedup
            context.MaybeDedupInfo
            (id >> Ok)
            (fun () ->
                task {
                    let! now = clock.Query Now
                    let traceContext = createTraceContext ()

                    match! processConstruction now traceContext context.CallOrigin serviceScope.ServiceProvider subjectId ctor context.MaybeConstructSubscriptions with
                    | Ok (initialValue, maybeReminderUpdate, creatorSubscribing, blobActions, constructionSideEffects, indexActions, raisedLifeEvents) ->
                        let writeData = {
                            UpdatedSubjectState = initialValue
                            TraceContext        = traceContext
                            ReminderUpdate      = maybeReminderUpdate
                            NextSideEffectSeq   = match constructionSideEffects with Some _ -> 1UL | None -> 0UL
                            SideEffectGroup     = constructionSideEffects
                            BlobActions         = blobActions
                            IndexActions        = indexActions
                        }

                        let grainIdHash = getGrainIdHash ()

                        let insertData = {
                            DataToInsert = writeData
                            CreatorSubscribing = creatorSubscribing
                            ConstructorThatCausedInsert = ctor
                            GrainIdHash = grainIdHash
                        }

                        match!
                            fun () -> initializeSubject now raisedLifeEvents context.MaybeDedupInfo insertData
                            |> catchExnButRethrowTransientIfInternalCall_OnResultTask context.CallOrigin with
                        | Ok currentStateContainer ->
                            return currentStateContainer |> SubjectStateContainer.Committed |> Ok
                        | Error err ->
                            return Error err

                    | Error err ->
                        return
                            match err with
                            | LifeCycleCtorError err -> Choice1Of2 err
                            | LifeCycleCtorException exn -> Choice2Of2 exn
                            |> Error
                })

    member private this.ForcePrepareInitialize
        (transactionId: SubjectTransactionId)
        (ctor: 'Constructor)
        (subjectId: 'SubjectId)
        (serviceScope: IServiceScope)
        : Task<Result<unit, Choice<'OpError, Exception>>> =

        let context = ConstructContext.InterGrain (None, None, false)
        // no need to dedup on prepare, it's already idempotent

        task {
            let! now = clock.Query Now
            let traceContext = createTraceContext ()

            match! processConstruction now traceContext context.CallOrigin serviceScope.ServiceProvider subjectId ctor context.MaybeConstructSubscriptions with
            | Ok (initialValue, maybeReminderUpdate, creatorSubscribing, blobActions, constructionSideEffects, indexActions, raisedLifeEvents) ->
                let writeData = {
                    UpdatedSubjectState = initialValue
                    TraceContext        = traceContext
                    ReminderUpdate      = maybeReminderUpdate
                    SideEffectGroup     = constructionSideEffects
                    NextSideEffectSeq   = match constructionSideEffects with Some _ -> 1UL | None -> 0UL
                    BlobActions         = blobActions
                    IndexActions        = indexActions
                }

                let grainIdHash = getGrainIdHash ()

                let insertData = {
                    DataToInsert = writeData
                    CreatorSubscribing = creatorSubscribing
                    ConstructorThatCausedInsert = ctor
                    GrainIdHash = grainIdHash
                }

                match insertData.TryCastToPrepared raisedLifeEvents with
                | Error PrepareWriteDataError.TransientSideEffectsNotAllowed ->
                    return TransientSideEffectsNotAllowedInTransaction |> Choice2Of2 |> Error
                | Ok (preparedInsertData, uniqueIndicesToReserve) ->
                    return!
                        fun () -> prepareInitializeSubject preparedInsertData uniqueIndicesToReserve transactionId
                        |> catchExnButRethrowTransientIfInternalCall_OnResultTask context.CallOrigin

            | Error err ->
                return
                    match err with
                    | LifeCycleCtorError err -> Choice1Of2 err
                    | LifeCycleCtorException exn -> Choice2Of2 exn
                    |> Error
        }

    member private _.RunActionMaybeInitialize (context: ActMaybeConstructContext) (action: 'LifeAction) (ctor: 'Constructor) (serviceScope: IServiceScope) : Task<Result<VersionedSubject<'Subject, 'SubjectId>, Choice<GrainOperationError<'OpError>, Exception>>> =
        task {
            match primaryStore.Value with
            | None ->
                let constructContext = context.ConstructContext
                // FIXME, ID Generation should never happen from within grain.
                // TODO: To fix it properly we need to to be able to parse typed 'SubjectId out of IdString
                // (not always can pass via parameters, e.g. GrainConnector sometimes knows only IdStr)
                match!
                    fun () ->
                        let (IdGenerationResult idGenTask) = lifeCycleAdapter.LifeCycle.GenerateId constructContext.CallOrigin serviceScope.ServiceProvider ctor
                        idGenTask
                    |> catchExnButRethrowTransientIfInternalCall_OnResultTask constructContext.CallOrigin with
                | Ok subjectId when subjectId.IdString = grainPKey ->
                    return!
                        authorizeSubjectAndContinueWith
                            constructContext.SessionHandle
                            constructContext.CallOrigin
                            (AccessEvent.Construct ctor)
                            (GrainOperationError.AccessDenied |> Choice1Of2 |> Error)
                            (fun () ->
                                task {
                                    match! this.ForceInitialize ctor subjectId constructContext serviceScope with
                                    | Ok (SubjectStateContainer.Committed currentStateContainer) ->
                                        match! this.RunActionOnCurrentStateContainer currentStateContainer context.ActContext action serviceScope with
                                        | Ok updatedVersionedSubj ->
                                            if shouldSendActionTelemetry action || shouldSendConstructorTelemetry ctor then
                                                logger.Info "ACT+CTOR ==> CTOR %a ==> ACT %a ==> OK" (logger.P "ctor") ctor (logger.P "action") action
                                            return Ok updatedVersionedSubj
                                        | Error (LifeCycleError err) ->
                                            logger.Warn "ACT+CTOR ==> CTOR %a ==> OK ==> ACT %a ==> ERROR %a" (logger.P "ctor") ctor (logger.P "action") action (logger.P "error") err
                                            return err |> GrainOperationError.TransitionError |> Choice1Of2 |> Error
                                        | Error TransitionNotAllowed ->
                                            logger.Warn "ACT+CTOR ==> CTOR %a ==> OK ==> ACT %a ==> ERROR TransitionNotAllowed" (logger.P "ctor") ctor (logger.P "action") action
                                            return GrainOperationError.TransitionNotAllowed |> Choice1Of2 |> Error
                                        | Error (LifeCycleException exn) ->
                                            logger.ErrorExn exn "ACT+CTOR ==> CTOR %a ==> OK ==> ACT %a ==> ERROR EXCEPTION" (logger.P "ctor") ctor (logger.P "action") action
                                            return exn |> Choice2Of2 |> Error

                                    | Error (Choice1Of2 err) ->
                                        logger.Warn "ACT+CTOR ==> CTOR %a ==> ERROR %a" (logger.P "ctor") ctor (logger.P "error") err
                                        this.CallDeactivateOnIdle()
                                        return err |> GrainOperationError.ConstructionError |> Choice1Of2 |> Error

                                    | Error (Choice2Of2 exn) ->
                                        logger.ErrorExn exn "ACT+CTOR ==> CTOR %a ==> ERROR EXCEPTION" (logger.P "ctor") ctor
                                        this.CallDeactivateOnIdle()
                                        return exn |> Choice2Of2 |> Error

                                    | Ok (SubjectStateContainer.PreparedInitialize _)
                                    | Ok (SubjectStateContainer.PreparedAction _) ->
                                        return InvalidOperationException "ForceInitialize must not create subject in transaction" |> raise
                                }
                            )
                            None

                | Ok ambiguousSubjectId ->
                    let badKey = ambiguousSubjectId.IdString
                    logger.Warn "ACT+CTOR ==> CTOR %a ==> ERROR IdStabilityViolated %a" (logger.P "ctor") ctor (logger.P "badKey") badKey
                    this.CallDeactivateOnIdle()
                    return badKey |> IdStabilityViolatedException |> Choice2Of2 |> Error

                | Error (Choice1Of2 err) ->
                    logger.Warn "ACT+CTOR ==> IDGEN %a ==> ERROR %a" (logger.P "ctor") ctor (logger.P "error") err
                    this.CallDeactivateOnIdle()
                    return err |> GrainOperationError.ConstructionError |> Choice1Of2 |> Error

                | Error (Choice2Of2 exn) ->
                    logger.ErrorExn exn "ACT+CTOR ==> IDGEN %a ==> ERROR EXCEPTION" (logger.P "ctor") ctor
                    this.CallDeactivateOnIdle()
                    return exn |> Choice2Of2 |> Error

            | Some (SubjectStateContainer.Committed currentStateContainer) ->
                let actContext = context.ActContext
                return!
                    authorizeSubjectAndContinueWith
                        actContext.SessionHandle
                        actContext.CallOrigin
                        (AccessEvent.Act(currentStateContainer.VersionedSubject.Subject, action))
                        (GrainOperationError.AccessDenied |> Choice1Of2 |> Error)
                        (fun () ->
                            task {
                                match! this.RunActionOnCurrentStateContainer currentStateContainer actContext action serviceScope with
                                | Ok updatedVersionedSubj ->
                                    if shouldSendActionTelemetry action then
                                        logger.Info "ACT+CTOR ==> ACT %a ==> OK" (logger.P "action") action
                                    return Ok updatedVersionedSubj
                                | Error (LifeCycleError err) ->
                                    logger.Warn "ACT+CTOR ==> ACT %a ==> ERROR %a" (logger.P "action") action (logger.P "error") err
                                    return err |> GrainOperationError.TransitionError |> Choice1Of2 |> Error
                                | Error TransitionNotAllowed ->
                                    logger.Warn "ACT+CTOR ==> ACT %a ==> ERROR TransitionNotAllowed" (logger.P "action") action
                                    return GrainOperationError.TransitionNotAllowed |> Choice1Of2 |> Error
                                | Error (LifeCycleException exn) ->
                                    logger.ErrorExn exn "ACT+CTOR ==> ACT %a ==> ERROR EXCEPTION" (logger.P "action") action
                                    return exn |> Choice2Of2 |> Error
                            }
                        )
                        None

            | Some (SubjectStateContainer.PreparedInitialize _)
            | Some (SubjectStateContainer.PreparedAction _) ->
                logger.Warn "ACT+CTOR ==> ACT %a ==> ERROR subject in transaction" (logger.P "action") action
                return GrainOperationError.LockedInTransaction |> Choice1Of2 |> Error
        }
        |> this.WithConsistencyCheckOnError

    member private _.Get (sessionHandle: SessionHandle) (callOrigin: CallOrigin) : Task<Result<Option<VersionedSubject<'Subject, 'SubjectId>>, GrainGetError>> =
        task {
            // deliberately pay premium of this database check on every Get. it's better than lying to client
            // and it's advised to use Repo's GetById anyway because it doesn't lock the grain
            do! assertStateConsistency ()

            match primaryStore.Value with
            | Some (SubjectStateContainer.Committed currentStateContainer)
            | Some (SubjectStateContainer.PreparedAction (currentStateContainer, _, _)) ->
                let versionedSubject = currentStateContainer.VersionedSubject
                return!
                    authorizeSubjectAndContinueWith
                        sessionHandle
                        callOrigin
                        (AccessEvent.Read (versionedSubject.Subject, SubjectProjection.OriginalProjection))
                        (GrainGetError.AccessDenied |> Error)
                        (fun () -> versionedSubject |> Some |> Ok |> Task.FromResult)
                        None
            | _ ->
                return None |> Ok
        }


    member private _.Prepared (sessionHandle: SessionHandle) (callOrigin: CallOrigin) (transactionId: SubjectTransactionId): Task<Result<Option<'Subject>, GrainGetError>> =
        task {
            // deliberately pay premium of this database check on every Prepared query. it's better than
            // risk of lying to client in the middle of transaction.
            do! assertStateConsistency ()

            let maybeSubject =
                match primaryStore.Value with
                | Some (SubjectStateContainer.PreparedAction (_, prepared, tId)) ->
                    if tId = transactionId then Some prepared.PreparedDataToUpdate.UpdatedSubjectState.Subject else None
                | Some (SubjectStateContainer.PreparedInitialize (prepared, _, tId)) ->
                    if tId = transactionId then Some prepared.PreparedDataToInsert.UpdatedSubjectState.Subject else None
                | Some (SubjectStateContainer.Committed _)
                | None ->
                    None

            return!
                match maybeSubject with
                | Some subject ->
                    authorizeSubjectAndContinueWith
                        sessionHandle
                        callOrigin
                        (AccessEvent.Read (subject, SubjectProjection.OriginalProjection))
                        (GrainGetError.AccessDenied |> Error)
                        (fun () -> subject |> Some |> Ok |> Task.FromResult)
                        None
                | None ->
                    None |> Ok |> Task.FromResult
        }

    member private _.GetMaybeConstruct (sessionHandle: SessionHandle) (callOrigin: CallOrigin) (subjectId: 'SubjectId) (ctor: 'Constructor) (serviceScope: IServiceScope) : Task<Result<VersionedSubject<'Subject, 'SubjectId>, GrainMaybeConstructionError<'OpError>>> =
        task {
            do! assertStateConsistency ()

            match primaryStore.Value with
            | None ->
                return!
                    authorizeSubjectAndContinueWith
                        sessionHandle
                        callOrigin
                        (AccessEvent.Construct ctor)
                        (GrainMaybeConstructionError.AccessDenied |> Error)
                        (fun () ->
                            task {
                                match! this.ForceInitialize ctor subjectId (ConstructContext.External (sessionHandle, callOrigin)) serviceScope with
                                | Ok (SubjectStateContainer.Committed currentStateContainer) ->
                                    if shouldSendConstructorTelemetry ctor then
                                        logger.Info "CURRENT+CTOR ==> CTOR %a ==> OK" (logger.P "ctor") ctor
                                    return Ok currentStateContainer.VersionedSubject

                                | Error (Choice1Of2 err) ->
                                    logger.Warn "CURRENT+CTOR ==> CTOR %a ==> ERROR %a" (logger.P "ctor") ctor (logger.P "error") err
                                    return err |> GrainMaybeConstructionError.ConstructionError |> Error

                                | Error (Choice2Of2 exn) ->
                                    logger.ErrorExn exn "CURRENT+CTOR ==> CTOR %a ==> EXCEPTION" (logger.P "ctor") ctor
                                    return TransientSubjectException ($"Life cycle %s{lifeCycleAdapter.LifeCycle.Name} %s{nameof(this.GetMaybeConstruct)}", exn.ToString()) |> raise

                                | Ok (SubjectStateContainer.PreparedInitialize _)
                                | Ok (SubjectStateContainer.PreparedAction _) ->
                                    return InvalidOperationException "ForceInitialize must not create subject in transaction" |> raise
                            }
                        )
                        None

            | Some (SubjectStateContainer.Committed currentStateContainer)
            | Some (SubjectStateContainer.PreparedAction (currentStateContainer, _, _)) ->
                return currentStateContainer.VersionedSubject |> Ok

            | Some (SubjectStateContainer.PreparedInitialize _) ->
                logger.Warn "CURRENT+CTOR ==> ERROR subject in transaction"
                return GrainMaybeConstructionError.InitializingInTransaction |> Error
        }

    member private _.GetVersionedSubject () : Option<VersionedSubject<'Subject, 'SubjectId>> =
        primaryStore.Value
        |> Option.map (fun subjectStateContainer -> subjectStateContainer.CurrentVersionedSubject)

    member private this.Observe
            (address: string)
            (maybeClientVersion: Option<ComparableVersion>)
            (observer: ISubjectGrainObserver<'Subject, 'SubjectId>) =
        task {
            // Would be great to always assert consistency on Observe but looks like it's used way too often
            if primaryStore.IsPotentiallyInconsistent then
                do! assertStateConsistency ()

            let maybeVersionedSubject = this.GetVersionedSubject ()

            observerManager.AddOrUpdateObserver address observer

            return
                match maybeClientVersion, maybeVersionedSubject with
                | _, None ->
                    // Subject does not exist, so notify the client accordingly.
                    SubjectChange.NotInitialized
                    |> observer.OnUpdate

                    ()
                | None, Some versionedSubject ->
                    // Client has no version at all, so send it.
                    versionedSubject
                    |> SubjectChange.Updated
                    |> observer.OnUpdate

                    ()
                | Some clientVersion, Some versionedSubject ->
                    if clientVersion < (versionedSubject.AsOf.Ticks, versionedSubject.Version) then
                        // Version held by client is outdated, so send current.
                        versionedSubject
                        |> SubjectChange.Updated
                        |> observer.OnUpdate
                    // else version held by client is up to date, so nothing to do

                    ()
        }

    member private _.Unobserve
            (address: string) =
        observerManager.RemoveObserver address
        Task.CompletedTask

    member private _.CallDeactivateOnIdleDirect() =
        this.DeactivateOnIdle()

    member this.CallDeactivateOnIdle() =
        Task.startOnScheduler orleansTaskScheduler
            (fun () ->
                task {
                    this.CallDeactivateOnIdleDirect()
                })
            |> ignore

    member _.OnSubjectReminder (logUnexpectedTicks: bool) : Task =
        match primaryStore.Value with
        | Some (SubjectStateContainer.Committed currentStateContainer) ->
            task {
                let! now = clock.Query Now
                match currentStateContainer.CurrentSubjectState.TickState with
                | TickState.Scheduled (scheduledNextTickOn, maybeTraceContext) ->
                    match calculateDueTimerActionOrNextDueTick now currentStateContainer.CurrentSubjectState.Subject currentStateContainer.CurrentSubjectState.LastUpdatedOn with
                    | Choice1Of2 dueAction ->
                        match dueAction with
                        | TimerAction.RunAction action ->
                            if shouldSendActionTelemetry action then
                                logger.Info "TIMER ==> QUEUE TRIGGER (due action: %a)" (logger.P "action") action
                            let grainSideEffect = (action, maybeTraceContext) |> TriggerTimerActionOnSelf |> GrainSideEffect.Persisted
                            // Note that TickState.Fired does not clear the reminder in runtime eagerly, this is intentional to allow optimizations
                            // and save unnecessary cross-silo calls to Reminder Service
                            do! setTickState currentStateContainer (TickState.Fired scheduledNextTickOn) None (Some grainSideEffect)

                        | TimerAction.DeleteSelf ->
                            let sendTelemetry = shouldSendSelfDeleteTelemetry currentStateContainer.CurrentSubjectState.Subject.SubjectId
                            if sendTelemetry then
                                logger.Info "TIMER ==> DELETE_SELF"

                            let tryDeleteSelfSideEffect =
                                (currentStateContainer.Version, currentStateContainer.NextSideEffectSeqNum + 1UL, 0uy)
                                |> TryDeleteSelf |> GrainSideEffect.Persisted
                            do! setTickState currentStateContainer (TickState.Fired scheduledNextTickOn) None (Some tryDeleteSelfSideEffect)

                        match remindersImplementation with
                        | RemindersImplementation.NotPersisted
                        | RemindersImplementation.Persisted ->
                            let latency =
                                let timeItShouldHaveFired = max scheduledNextTickOn currentStateContainer.CurrentSubjectState.LastUpdatedOn
                                now - timeItShouldHaveFired
                            if latency > TimeSpan.FromSeconds 10 then
                                logger.Warn "Tick fired later than it should, latency: %a seconds"
                                    (logger.P "seconds") (int64 latency.TotalSeconds)
                        // don't do in tests to not fail simulations
                        | RemindersImplementation.TestNotPersistedManuallyTriggered -> ()

                    | Choice2Of2 maybeCalculatedNextTickOn ->
                        match maybeCalculatedNextTickOn with
                        | Some calculatedNextTickOn ->
                            // tick scheduled earlier than subject calculates (e.g. timer func changed in new deployment)
                            if calculatedNextTickOn > scheduledNextTickOn then
                                if logUnexpectedTicks then
                                    logger.Warn "TIMER ==> TICK STATE CORRECTION: reschedule, tick scheduled earlier than calculated"
                                // reset trace context because it can be a different timer action than was initially scheduled
                                do! setTickState currentStateContainer (TickState.Scheduled (calculatedNextTickOn, None)) (Some { On = Some calculatedNextTickOn; AssumedCurrent = AssumedCurrentReminder.Unknown }) None
                            // tick scheduled later than subject calculates, but somehow reminder still invoked too early.
                            // Possible reason is reminder "drifts" and fires before due time (see https://github.com/dotnet/orleans/issues/5659)
                            elif calculatedNextTickOn < scheduledNextTickOn then
                                if logUnexpectedTicks then
                                    logger.Warn "TIMER ==> TICK STATE CORRECTION: reschedule, tick scheduled LATER than calculated, but invoked too early (by %a seconds)"
                                        (logger.P "seconds") (int64 (calculatedNextTickOn - now).TotalSeconds)
                                do! setTickState currentStateContainer (TickState.Scheduled (calculatedNextTickOn, None)) (Some { On = Some calculatedNextTickOn; AssumedCurrent = AssumedCurrentReminder.Unknown }) None
                            // tick scheduled same as subject calculates, but drifted
                            else
                                // downgrade to Info? This happens for subjects that run a tight timer loop i.e. outdated reminder reattempted again before new one,
                                // however some other "drift" scenario can't be ruled out, so just in case reapply grain reminder to do its job, no need to touch storage
                                if logUnexpectedTicks then
                                    logger.Warn "TIMER ==> REMINDER UPDATE: tick scheduled as calculated but invoked too early (by %a seconds)"
                                        (logger.P "seconds") (int64 (calculatedNextTickOn - now).TotalSeconds)
                                do! applyReminderUpdateToGrain currentStateContainer.CurrentSubjectState.Subject.SubjectId (Some { On = Some calculatedNextTickOn; AssumedCurrent = AssumedCurrentReminder.Unknown })
                        | None ->
                            // tick scheduled but subject has no timers (e.g. new deployment) just clear it out
                            if logUnexpectedTicks then
                                logger.Warn "TIMER ==> TICK STATE CORRECTION: NoTick, triggered but subject has no calculated timer"
                            do! setTickState currentStateContainer TickState.NoTick (Some { On = None; AssumedCurrent = AssumedCurrentReminder.Unknown }) None

                | TickState.NoTick ->
                    if logUnexpectedTicks then
                        // downgrade to Info? it's not uncommon to receive duplicate reminder after it has fired and cleared
                        logger.Warn "TIMER ==> REMINDER UPDATE: NoTick, clear reminder"
                    do! applyReminderUpdateToGrain currentStateContainer.CurrentSubjectState.Subject.SubjectId (Some { On = None; AssumedCurrent = AssumedCurrentReminder.Unknown })

                | TickState.Fired _ ->
                    if logUnexpectedTicks then
                        // downgrade to Info? it's not uncommon to receive duplicate reminder just after it has fired
                        logger.Warn "TIMER ==> REMINDER UPDATE: Fired, clear reminder"
                    do! applyReminderUpdateToGrain currentStateContainer.CurrentSubjectState.Subject.SubjectId (Some { On = None; AssumedCurrent = AssumedCurrentReminder.Unknown })

            }
            |> Task.Ignore

        // just keep timers if in transaction, new ReminderUpdate will apply on Commit, or will stay the same on Rollback. It means 1 min period delays are possible, unfortunately.
        // TODO: better management of reminder in transactional state, currently it's very naive, both SubjectGrain and SubjectReminderTable disregard Prepared state and keep committed TickState and reminder in runtime intact
        | Some (SubjectStateContainer.PreparedInitialize _) ->
            if logUnexpectedTicks then
                logger.Warn "TIMER ==> NOOP: PreparedInitialize, keep reminder"
            Task.CompletedTask
        | Some (SubjectStateContainer.PreparedAction _) ->
            if logUnexpectedTicks then
                logger.Warn "TIMER ==> NOOP: PreparedAction, keep reminder"
            Task.CompletedTask
        // just clear reminder if subject not initialized
        | None ->
            if logUnexpectedTicks then
                logger.Warn "TIMER ==> REMINDER UPDATE: Subject Not even constructed! clear reminder"
            // can't do applyReminderUpdateToGrain because subjectId is unknown, hope this will never run
            this.ClearTickReminder AssumedCurrentReminder.Unknown

    interface IRemindable with

        member this.ReceiveReminder(reminderName: string, status: TickStatus): Task =
            if reminderName = SubjectReminderName then
                // TODO: try to use status.FirstTickTime in OnSubjectReminder to correlate it with calculated timers
                // it's tricky because it doesn't always match NextTickOn on happy path (see minSchedulableReminder)
                ignore status.FirstTickTime
                this.OnSubjectReminder (* logUnexpectedTicks *) true
            elif reminderName = MetaKeepAliveReminderName then
                task {
                    if grainPKey = MetaLifeCycleName then
                        // if Meta of Meta grain becomes a zombie there's nothing else that can save it but odd consistency check
                        do! assertStateConsistency ()

                    let! now = clock.Query Now
                    match primaryStore.Value with
                    | Some (SubjectStateContainer.Committed { CurrentSubjectState = { TickState = TickState.Scheduled (on, _) } })
                        when on < now ->
                        // process overdue tick if Orleans Reminder service failed to deliver it by normal means
                        do! this.OnSubjectReminder (* logUnexpectedTicks *) true
                    | _ ->
                        ()
                }
            else
                logger.Warn "Unexpected reminder received in subject grain: %a" (logger.P "reminderName") reminderName
                Task.CompletedTask

    interface ISubjectGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId> with

        member _.InternalOnlyAwaitTimerExpired(lifeEvent: 'LifeEvent): Task =
            task {
                let! now = clock.Query Now
                match awaitersByLifeEvent.TryGetValue lifeEvent with
                | true, awaiters ->
                    awaitersByLifeEvent[lifeEvent] <- (awaiters |> List.where (fun awaiter -> awaiter.ExpiryTime > now))
                | false, _ ->
                    Noop
            } |> Task.Ignore

        member _.InternalOnlyClearExpiredObservers() : Task =
            // this internally calls base.DelayDeactivation so has to be a grain member
            observerManager.ClearExpiredObservers()
            Task.CompletedTask

        member this.Construct (clientGrainCallContext: ClientGrainCallContext) (subjectId: 'SubjectId) (ctor: 'Constructor) : Task<Result<VersionedSubject<'Subject, 'SubjectId>, GrainConstructionError<'OpError>>> =
            fun () -> task {
                use serviceScope = prepareChildScope ()
                let! res =
                    this.Construct (ConstructContext.External (clientGrainCallContext.SessionHandle, clientGrainCallContext.CallOrigin)) subjectId ctor serviceScope
                return
                    match res with
                    | Ok subj -> Ok subj
                    | Error (Choice1Of2 err) -> Error err
                    | Error (Choice2Of2 ex) ->
                        TransientSubjectException ($"Life cycle %s{lifeCycleAdapter.LifeCycle.Name} Construct", ex.ToString()) |> raise
            }
            |> wrapClientExceptions "Construct"

        member this.ConstructNoContent (clientGrainCallContext: ClientGrainCallContext) (subjectId: 'SubjectId) (ctor: 'Constructor) : Task<Result<unit, GrainConstructionError<'OpError>>> =
            fun () -> task {
                use serviceScope = prepareChildScope ()
                let! res =
                    this.Construct (ConstructContext.External (clientGrainCallContext.SessionHandle, clientGrainCallContext.CallOrigin)) subjectId ctor serviceScope
                return
                    match res with
                    | Ok _ -> Ok ()
                    | Error (Choice1Of2 err) -> Error err
                    | Error (Choice2Of2 ex) ->
                        TransientSubjectException ($"Life cycle %s{lifeCycleAdapter.LifeCycle.Name} ConstructNoContent", ex.ToString()) |> raise
            }
            |> wrapClientExceptions "ConstructNoContent"

        member this.ConstructAndWait (clientGrainCallContext: ClientGrainCallContext) (subjectId: 'SubjectId) (ctor: 'Constructor) (lifeEventToAwaitOn: 'LifeEvent) (awaiter: ILifeEventAwaiter<'Subject, 'LifeEvent, 'SubjectId>) (timeout: TimeSpan) : Task<Result<VersionedSubject<'Subject, 'SubjectId>, GrainConstructionError<'OpError>>> =
            fun () -> task {
                use serviceScope = prepareChildScope ()
                do! awaitLifeEvent lifeEventToAwaitOn awaiter timeout
                let! res = this.Construct (ConstructContext.External (clientGrainCallContext.SessionHandle, clientGrainCallContext.CallOrigin)) subjectId ctor serviceScope
                return
                    match res with
                    | Ok subj -> Ok subj
                    | Error (Choice1Of2 err) -> Error err
                    | Error (Choice2Of2 ex) ->
                        TransientSubjectException ($"Life cycle %s{lifeCycleAdapter.LifeCycle.Name} ConstructAndWait", ex.ToString()) |> raise
            }
            |> wrapClientExceptions "ConstructAndWait"

        member _.ConstructInterGrain (okIfAlreadyInitialized: bool) (maybeDedupInfo: Option<SideEffectDedupInfo>) (includeResponse: bool) (subjectId: 'SubjectId) (ctor: 'Constructor) (maybeConstructSubscriptions: Option<ConstructSubscriptions>) : Task<Result<Option<'Subject>, SubjectFailure<GrainConstructionError<'OpError>>>> =
            task {
                if includeResponse then
                    logger.Warn "ConstructInterGrain uses deprecated includeResponse=true. CTOR %a" (logger.P "ctor") ctor
                use serviceScope = prepareChildScope ()
                let! res =
                    this.Construct (ConstructContext.InterGrain (maybeDedupInfo, maybeConstructSubscriptions, okIfAlreadyInitialized)) subjectId ctor serviceScope
                return
                    match res with
                    | Ok versionedSubject -> Ok (if includeResponse then Some versionedSubject.Subject else None)
                    | Error (Choice1Of2 err) -> err |> SubjectFailure.Err |> Error
                    | Error (Choice2Of2 ex) -> ex.ToString() |> SubjectFailure.Exn |> Error
            }

        member _.PrepareInitialize (subjectId: 'SubjectId) (ctor: 'Constructor) (transactionId: SubjectTransactionId) : Task<Result<unit, SubjectFailure<GrainPrepareConstructionError<'OpError>>>> =
            task {
                use serviceScope = prepareChildScope ()
                match primaryStore.Value with
                | Some (SubjectStateContainer.PreparedInitialize (prepared, _, preparedInTransactionId))
                    when preparedInTransactionId = transactionId && Object.Equals(prepared.ConstructorThatCausedInsert, ctor) ->
                    // idempotency / Noop
                    return () |> Ok

                | Some (SubjectStateContainer.PreparedInitialize (_, _, transactionId)) ->
                    logger.Warn "CTOR PREPARE %a ==> ConflictingPrepare" (logger.P "ctor") ctor
                    return transactionId |> GrainPrepareConstructionError.ConflictingPrepare |> SubjectFailure.Err |> Error

                | Some (SubjectStateContainer.PreparedAction _)
                | Some (SubjectStateContainer.Committed _) ->
                    logger.Warn "CTOR PREPARE %a ==> SubjectAlreadyInitialized" (logger.P "ctor") ctor
                    return GrainPrepareConstructionError.SubjectAlreadyInitialized grainPKey |> SubjectFailure.Err |> Error

                | None ->
                    match! this.ForcePrepareInitialize transactionId ctor subjectId serviceScope with
                    | Ok _ ->
                        if shouldSendConstructorTelemetry ctor then
                            logger.Info "CTOR PREPARE %a ==> OK" (logger.P "ctor") ctor
                        return () |> Ok
                    | Error (Choice1Of2 err) ->
                        logger.Warn "CTOR PREPARE %a ==> ERROR %a" (logger.P "ctor") ctor (logger.P "error") err
                        this.CallDeactivateOnIdle()
                        return err |> GrainPrepareConstructionError.ConstructionError |> SubjectFailure.Err |> Error
                    | Error (Choice2Of2 exn) ->
                        logger.ErrorExn exn "CTOR PREPARE %a ==> ERROR EXCEPTION" (logger.P "ctor") ctor
                        this.CallDeactivateOnIdle()
                        return exn.ToString() |> SubjectFailure.Exn |> Error
            }
            |> this.WithConsistencyCheckOnError

        member _.Act (clientGrainCallContext: ClientGrainCallContext) (action: 'LifeAction) : Task<Result<VersionedSubject<'Subject, 'SubjectId>, GrainTransitionError<'OpError>>> =
            fun () -> task {
                use serviceScope = prepareChildScope ()
                let! res =
                    this.RunAction (ActContext.External (clientGrainCallContext.SessionHandle, clientGrainCallContext.CallOrigin)) action serviceScope
                return
                    match res with
                    | Ok subj -> Ok subj
                    | Error (Choice1Of2 err) -> Error err
                    | Error (Choice2Of2 ex) ->
                        TransientSubjectException ($"Life cycle %s{lifeCycleAdapter.LifeCycle.Name} Act", ex.ToString()) |> raise
            }
            |> wrapClientExceptions "Act"

        member _.ActNoContent (clientGrainCallContext: ClientGrainCallContext) (action: 'LifeAction) : Task<Result<unit, GrainTransitionError<'OpError>>> =
            fun () -> task {
                use serviceScope = prepareChildScope ()
                let! res =
                    this.RunAction (ActContext.External (clientGrainCallContext.SessionHandle, clientGrainCallContext.CallOrigin)) action serviceScope
                return
                    match res with
                    | Ok _ -> Ok ()
                    | Error (Choice1Of2 err) -> Error err
                    | Error (Choice2Of2 ex) ->
                        TransientSubjectException ($"Life cycle %s{lifeCycleAdapter.LifeCycle.Name} ActNoContent", ex.ToString()) |> raise
            }
            |> wrapClientExceptions "ActNoContent"

        member _.ActInterGrain (maybeDedupInfo: Option<SideEffectDedupInfo>) (includeResponse: bool) (action: 'LifeAction) : Task<Result<Option<'Subject>, SubjectFailure<GrainTransitionError<'OpError>>>> =
            task {
                if includeResponse then
                    logger.Warn "ActInterGrain uses deprecated includeResponse=true. CTOR %a" (logger.P "action") action
                let! res = (this :> ISubjectGrain<_, _, _, _, _, _>).ActInterGrainV2 maybeDedupInfo action (* maybeBeforeActSubscriptions *) None
                return res |> Result.map (fun _ -> None)
            }

        member _.ActInterGrainV2 (maybeDedupInfo: Option<SideEffectDedupInfo>) (action: 'LifeAction) (maybeBeforeActSubscriptions: Option<BeforeActSubscriptions>) : Task<Result<unit, SubjectFailure<GrainTransitionError<'OpError>>>> =
            task {
                use serviceScope = prepareChildScope ()
                let! res =
                    this.RunAction (ActContext.InterGrain (maybeDedupInfo, maybeBeforeActSubscriptions)) action serviceScope
                return
                    match res with
                    | Ok _ -> Ok ()
                    | Error (Choice1Of2 err) -> err |> SubjectFailure.Err |> Error
                    | Error (Choice2Of2 ex) -> ex.ToString() |> SubjectFailure.Exn |> Error
            }

        member _.RunPrepareAction (action: 'LifeAction) (transactionId: SubjectTransactionId) : Task<Result<unit, SubjectFailure<GrainPrepareTransitionError<'OpError>>>> =
            task {
                use serviceScope = prepareChildScope ()
                let! res =
                    this.RunPrepareAction action transactionId serviceScope
                return res |> Result.map ignore
            }

        member _.RunCommitPrepared (transactionId: SubjectTransactionId) : Task =
            this.RunCommitPrepared transactionId

        member _.RunRollbackPrepared (transactionId: SubjectTransactionId) : Task =
            this.RunRollbackPrepared transactionId

        member _.RunCheckPhantomPrepared (transactionId: SubjectTransactionId) : Task =
            this.RunCheckPhantomPrepared transactionId

        member _.ActAndWait (clientGrainCallContext: ClientGrainCallContext) (action: 'LifeAction) (lifeEventToAwaitOn: 'LifeEvent) (awaiter: ILifeEventAwaiter<'Subject, 'LifeEvent, 'SubjectId>) (timeout: TimeSpan) : Task<Result<VersionedSubject<'Subject, 'SubjectId>, GrainTransitionError<'OpError>>> =
            fun () -> task {
                use serviceScope = prepareChildScope ()
                do! awaitLifeEvent lifeEventToAwaitOn awaiter timeout
                let! res = this.RunAction (ActContext.External (clientGrainCallContext.SessionHandle, clientGrainCallContext.CallOrigin)) action serviceScope
                return
                    match res with
                    | Ok subj -> Ok subj
                    | Error (Choice1Of2 err) -> Error err
                    | Error (Choice2Of2 ex) ->
                        TransientSubjectException ($"Life cycle %s{lifeCycleAdapter.LifeCycle.Name} ActAndWait", ex.ToString()) |> raise
            }
            |> wrapClientExceptions "ActAndWait"

        member _.ActMaybeConstruct (clientGrainCallContext: ClientGrainCallContext) (action: 'LifeAction) (ctor: 'Constructor): Task<Result<VersionedSubject<'Subject, 'SubjectId>, GrainOperationError<'OpError>>> =
            fun () -> task {
                use serviceScope = prepareChildScope ()
                let! res =
                    this.RunActionMaybeInitialize (ActMaybeConstructContext.External (clientGrainCallContext.SessionHandle, clientGrainCallContext.CallOrigin)) action ctor serviceScope
                return
                    match res with
                    | Ok subj -> Ok subj
                    | Error (Choice1Of2 err) -> Error err
                    | Error (Choice2Of2 ex) ->
                        TransientSubjectException ($"Life cycle %s{lifeCycleAdapter.LifeCycle.Name} ActMaybeConstruct", ex.ToString()) |> raise
            }
            |> wrapClientExceptions "ActMaybeConstruct"

        member _.ActMaybeConstructNoContent (clientGrainCallContext: ClientGrainCallContext) (action: 'LifeAction) (ctor: 'Constructor): Task<Result<unit, GrainOperationError<'OpError>>> =
            fun () -> task {
                use serviceScope = prepareChildScope ()
                let! res =
                    this.RunActionMaybeInitialize (ActMaybeConstructContext.External (clientGrainCallContext.SessionHandle, clientGrainCallContext.CallOrigin)) action ctor serviceScope
                return
                    match res with
                    | Ok _ -> Ok ()
                    | Error (Choice1Of2 err) -> Error err
                    | Error (Choice2Of2 ex) ->
                        TransientSubjectException ($"Life cycle %s{lifeCycleAdapter.LifeCycle.Name} ActMaybeConstructNoContent", ex.ToString()) |> raise
            }
            |> wrapClientExceptions "ActMaybeConstructNoContent"

        member _.ActMaybeConstructInterGrain (maybeDedupInfo: Option<SideEffectDedupInfo>) (includeResponse: bool) (action: 'LifeAction) (ctor: 'Constructor) (maybeConstructSubscriptions: Option<ConstructSubscriptions>) : Task<Result<Option<'Subject>, SubjectFailure<GrainOperationError<'OpError>>>> =
            task {
                if includeResponse then
                    logger.Warn "ActMaybeConstructInterGrain uses deprecated includeResponse=true. CTOR %a; ACT %a" (logger.P "ctor") ctor (logger.P "action") action
                use serviceScope = prepareChildScope ()
                let! res =
                    this.RunActionMaybeInitialize (ActMaybeConstructContext.InterGrain (maybeDedupInfo, maybeConstructSubscriptions)) action ctor serviceScope
                return
                    match res with
                    | Ok versionedSubject -> Ok (if includeResponse then Some versionedSubject.Subject else None)
                    | Error (Choice1Of2 err) -> err |> SubjectFailure.Err |> Error
                    | Error (Choice2Of2 ex) -> ex.ToString() |> SubjectFailure.Exn |> Error
            }

        member _.ActMaybeConstructAndWait (clientGrainCallContext: ClientGrainCallContext) (action: 'LifeAction) (ctor: 'Constructor) (lifeEventToAwaitOn: 'LifeEvent) (awaiter: ILifeEventAwaiter<'Subject, 'LifeEvent, 'SubjectId>) (timeout: TimeSpan) : Task<Result<VersionedSubject<'Subject, 'SubjectId>, GrainOperationError<'OpError>>> =
            fun () -> task {
                use serviceScope = prepareChildScope ()
                do! awaitLifeEvent lifeEventToAwaitOn awaiter timeout
                let! res = this.RunActionMaybeInitialize (ActMaybeConstructContext.External (clientGrainCallContext.SessionHandle, clientGrainCallContext.CallOrigin)) action ctor serviceScope
                return
                    match res with
                    | Ok subj -> Ok subj
                    | Error (Choice1Of2 err) -> Error err
                    | Error (Choice2Of2 ex) ->
                        TransientSubjectException ($"Life cycle %s{lifeCycleAdapter.LifeCycle.Name} ActMaybeConstructAndWait", ex.ToString()) |> raise
            }
            |> wrapClientExceptions "ActMaybeConstructAndWait"

        member _.Get (clientGrainCallContext: ClientGrainCallContext) : Task<Result<Option<VersionedSubject<'Subject, 'SubjectId>>, GrainGetError>> =
            fun () -> this.Get clientGrainCallContext.SessionHandle clientGrainCallContext.CallOrigin
            |> wrapClientExceptions "Get"

        member _.Prepared (clientGrainCallContext: ClientGrainCallContext) (transactionId: SubjectTransactionId): Task<Result<Option<'Subject>, GrainGetError>> =
            fun () -> this.Prepared clientGrainCallContext.SessionHandle clientGrainCallContext.CallOrigin transactionId
            |> wrapClientExceptions "Prepared"

        member _.GetMaybeConstruct (clientGrainCallContext: ClientGrainCallContext) (subjectId: 'SubjectId) (ctor: 'Constructor): Task<Result<VersionedSubject<'Subject, 'SubjectId>, GrainMaybeConstructionError<'OpError>>> =
            fun () -> task {
                use serviceScope = prepareChildScope ()
                return! this.GetMaybeConstruct clientGrainCallContext.SessionHandle clientGrainCallContext.CallOrigin subjectId ctor serviceScope
            }
            |> wrapClientExceptions "GetMaybeConstruct"

        member _.MaybeConstructNoContent (clientGrainCallContext: ClientGrainCallContext) (subjectId: 'SubjectId) (ctor: 'Constructor): Task<Result<unit, GrainMaybeConstructionError<'OpError>>> =
            fun () -> task {
                use serviceScope = prepareChildScope ()
                match! this.GetMaybeConstruct clientGrainCallContext.SessionHandle clientGrainCallContext.CallOrigin subjectId ctor serviceScope with
                | Ok _ -> return Ok ()
                | Error err -> return Error err
            }
            |> wrapClientExceptions "MaybeConstructNoContent"

        member _.ForceAwake () : Task = // simply used to wake up the grain
            task {
                // stalled side effects & ForceAwake is a common symptom of a zombie grain. Don't wait, assert state is up to date
                do! assertStateConsistency ()
                match primaryStore.Value with
                | None ->
                    this.CallDeactivateOnIdle()
                | Some stateContainer ->
                    let! now = clock.Query Now
                    match stateContainer with
                    | SubjectStateContainer.Committed { CurrentSubjectState = { TickState = TickState.Scheduled (on, _) } }
                        when on < now ->
                        // process overdue tick if force awaken because of stalled timer that Orleans Reminder service failed to deliver by normal means
                        do! this.OnSubjectReminder (* logUnexpectedTicks *) true
                    | _ ->
                        // there's a known rare production bug when Side Effect processor dies suddenly after a transient failure and stops processing further requests
                        // if ForceAwake becomes frequent the workaround is to reactivate grain with a new SE processor.
                        let! now = clock.Query Now
                        if now.Millisecond % 8 = 0 then
                            this.CallDeactivateOnIdle()
                        else
                            ()
            }

        member _.TryDeleteSelf (sideEffectId: GrainSideEffectId) (requiredVersion: uint64) (requiredNextSideEffectSequenceNumber: uint64) (retryAttempt: byte) : Task =
            task {
                match primaryStore.Value with
                | None ->
                    do! assertStateConsistency()
                    this.CallDeactivateOnIdle()
                | Some stateContainer ->
                    match stateContainer with
                    | SubjectStateContainer.PreparedAction (_, _, transactionId)
                    | SubjectStateContainer.PreparedInitialize (_, _, transactionId) ->
                        do! assertStateConsistency()
                        logger.Info "TRY_DELETE_SELF ==> Subject in transaction %a ==> NOOP" (logger.P "transactionId") transactionId
                    | SubjectStateContainer.Committed currentStateContainer ->
                        if currentStateContainer.NextSideEffectSeqNum = requiredNextSideEffectSequenceNumber && currentStateContainer.Version = requiredVersion then
                            do! grainStorageHandler.ClearState grainPKey currentStateContainer.ETag currentStateContainer.SkipHistoryOnNextOp (Some sideEffectId)
                            do! applyReminderUpdateToGrain currentStateContainer.CurrentSubjectState.Subject.SubjectId (Some { On = None; AssumedCurrent = AssumedCurrentReminder.Unknown })
                            updatePrimaryStoreWithNotify None // delete subj
                            logger.Info "TRY_DELETE_SELF ==> OK"
                            this.CallDeactivateOnIdle()
                        else
                            do! assertStateConsistency()
                            let! now = clock.Query Now
                            match calculateDueTimerActionOrNextDueTick now currentStateContainer.CurrentSubjectState.Subject currentStateContainer.CurrentSubjectState.LastUpdatedOn with
                            | Choice1Of2 TimerAction.DeleteSelf ->
                                logger.Warn "TRY_DELETE_SELF ==> Subject changed concurrently (Version %a vs %a, SideEffectSeqNum %a vs %a) but still needs to be deleted ==> RETRY (attempt %a)"
                                    (logger.P "requiredVersion") requiredVersion
                                    (logger.P "actualVersion") currentStateContainer.Version
                                    (logger.P "expectedSeqNum") requiredNextSideEffectSequenceNumber
                                    (logger.P "actualSeqNum") currentStateContainer.NextSideEffectSeqNum
                                    (logger.P "attempt") retryAttempt
                                // can't just ClearState before concurrent side effects are dispatched, post TryDeleteSelf again
                                let tryDeleteSelfSideEffect =
                                    (currentStateContainer.Version, currentStateContainer.NextSideEffectSeqNum + 1UL, retryAttempt + 1uy)
                                    |> TryDeleteSelf |> GrainSideEffect.Persisted
                                do! enqueueSideEffects currentStateContainer (* maybeDedupInfo *) None
                                        (makeNewSideEffectsGroup currentStateContainer.NextSideEffectSeqNum [tryDeleteSelfSideEffect]
                                        |> Option.get
                                        |> NonemptyKeyedSet.ofOneItem)
                            | Choice2Of2 _
                            | Choice1Of2 (TimerAction.RunAction _) ->
                                logger.Warn "TRY_DELETE_SELF ==> Subject changed concurrently (Version %a vs %a, SideEffectSeqNum %a vs %a) and no longer needs to be deleted ==> NOOP (attempt %a)"
                                    (logger.P "requiredVersion") requiredVersion
                                    (logger.P "actualVersion") currentStateContainer.Version
                                    (logger.P "expectedSeqNum") requiredNextSideEffectSequenceNumber
                                    (logger.P "actualSeqNum") currentStateContainer.NextSideEffectSeqNum
                                    (logger.P "attempt") retryAttempt
            }

        member _.IsConstructed() : Task<bool> =
            fun () ->
                match primaryStore.Value with
                | Some stateContainer ->
                    match stateContainer with
                    | SubjectStateContainer.PreparedInitialize _ ->
                        // prepared, but not initialized yet
                        false
                    | SubjectStateContainer.Committed _
                    | SubjectStateContainer.PreparedAction _ ->
                        true
                | None ->
                    false
                |> Task.FromResult
            |> wrapClientExceptions "IsConstructed"

        member _.Subscribe (subscriptionNamesToEvents: Map<SubscriptionName, LifeEvent>) (subscriber: SubjectPKeyReference) : Task<Result<unit, SubjectFailure<GrainSubscriptionError>>> =
            task {
                let! res =
                    this.Subscribe subscriptionNamesToEvents subscriber
                    |> this.WithConsistencyCheckOnError
                return
                    match res with
                    | Ok _ -> Ok ()
                    | Error (Choice1Of2 err) -> err |> SubjectFailure.Err |> Error
                    | Error (Choice2Of2 ex) -> ex.ToString() |> SubjectFailure.Exn |> Error
            }

        member _.Unsubscribe (subscriptions: Set<SubscriptionName>) (subscriber: SubjectPKeyReference) : Task =
            task {
                match primaryStore.Value with
                | None
                | Some (SubjectStateContainer.PreparedInitialize _) ->
                    ()

                | Some (SubjectStateContainer.Committed currentStateContainer)
                | Some (SubjectStateContainer.PreparedAction (currentStateContainer, _, _)) ->
                    match removeFromOthersSubscribing currentStateContainer.CurrentOthersSubscribing subscriptions subscriber with
                    | None ->
                        // Nothing to do
                        do! assertStateConsistency ()
                        logger.Info "UNSUBSCRIBE %a ==> SUBSCRIBER %a ==> NOOP" (logger.P "subscriptions") subscriptions (logger.P "subscriber") subscriber

                    | Some (updatedOthersSubscribing, removedSubscriptions) ->
                        logger.Info "UNSUBSCRIBE %a ==> SUBSCRIBER %a ==> OK" (logger.P "subscriptions") subscriptions (logger.P "subscriber") subscriber

                        let! newETag =
                            grainStorageHandler.RemoveSubscriptions grainPKey currentStateContainer.ETag subscriber removedSubscriptions

                        let newCurrentStateContainer =
                            { currentStateContainer with
                                ETag = newETag
                                CurrentOthersSubscribing = updatedOthersSubscribing }

                        match primaryStore.Value with
                            | Some (SubjectStateContainer.Committed _) ->
                                SubjectStateContainer.Committed newCurrentStateContainer
                            | Some (SubjectStateContainer.PreparedAction (_, preparedState, transactionId)) ->
                                SubjectStateContainer.PreparedAction (newCurrentStateContainer, preparedState, transactionId)
                            | None
                            | Some (SubjectStateContainer.PreparedInitialize _) ->
                                shouldNotReachHereBecause "this code executes only for current state"
                        |> Some
                        |> updatePrimaryStoreWithoutNotify
            } |> Task.Ignore

        member _.TriggerSubscription (maybeDedupInfo: Option<SideEffectDedupInfo>) (subscriptionTriggerType: SubscriptionTriggerType) (triggeredLifeEvent: LifeEvent) : Task<Result<unit, SubjectFailure<GrainTriggerSubscriptionError<'OpError>>>> =
            this.InvokeTryDedup
                maybeDedupInfo
                (ignore >> Ok)
                (fun () ->
                    task {
                        use serviceScope = prepareChildScope ()
                        match primaryStore.Value with
                        | Some (SubjectStateContainer.Committed currentStateContainer) ->

                            let enqueueResponse traceContext response =
                                match mapSubscriptionResponseToSideEffect currentStateContainer subscriptionTriggerType triggeredLifeEvent traceContext response with
                                | None -> Task.CompletedTask
                                | Some responseSideEffect ->
                                    enqueueSideEffects currentStateContainer maybeDedupInfo
                                        (makeNewSideEffectsGroup currentStateContainer.NextSideEffectSeqNum [responseSideEffect]
                                         |> Option.get
                                         |> NonemptyKeyedSet.ofOneItem)

                            match getActionForTriggeredSubscription currentStateContainer.CurrentSubjectState subscriptionTriggerType triggeredLifeEvent with
                            | Some (Ok action) ->
                                let! now = clock.Query Now
                                let traceContext = createTraceContext ()
                                let! res = processAction now traceContext currentStateContainer.NextSideEffectSeqNum currentStateContainer.CurrentSubjectState currentStateContainer.CurrentOthersSubscribing action CallOrigin.Internal serviceScope.ServiceProvider

                                match res with
                                | Ok (Choice1Of2 (updatedState, maybeReminderUpdate, blobActions, sideEffectGroup, indexActions, raisedLifeEvents)) ->
                                    if shouldSendActionTelemetry action then
                                        logger.Info "TRIGGER %a ==> ACT %a ==> OK" (logger.P "subscription") subscriptionTriggerType (logger.P "action") action

                                    let sideEffectGroup =
                                        match sideEffectGroup,
                                            mapSubscriptionResponseToSideEffect currentStateContainer subscriptionTriggerType triggeredLifeEvent traceContext
                                                (TriggerSubscriptionResponse.ActOk action) with
                                        | _, None -> sideEffectGroup
                                        | None, Some responseSideEffect ->
                                            makeNewSideEffectsGroup currentStateContainer.NextSideEffectSeqNum [responseSideEffect]
                                        | Some g, Some responseSideEffect ->
                                            makeNewSideEffectsGroup g.SequenceNumber (Seq.append g.SideEffects.Values (Seq.ofOneItem responseSideEffect))

                                    let! res =
                                        fun () ->
                                            {
                                                DataToUpdate = {
                                                    UpdatedSubjectState = updatedState
                                                    TraceContext        = traceContext
                                                    ReminderUpdate      = maybeReminderUpdate
                                                    NextSideEffectSeq   = currentStateContainer.NextSideEffectSeqNum + (match sideEffectGroup with Some _ -> 1UL | None -> 0UL)
                                                    SideEffectGroup     = sideEffectGroup
                                                    BlobActions         = blobActions
                                                    IndexActions        = indexActions
                                                }
                                                ActionThatCausedUpdate  = action
                                                SubscriptionsToAdd      = None
                                                ExpectedETag            = currentStateContainer.ETag
                                                CurrentVersion          = currentStateContainer.Version
                                                SkipHistory             = currentStateContainer.SkipHistoryOnNextOp
                                            }
                                            |> updateSubject now raisedLifeEvents maybeDedupInfo currentStateContainer
                                        |> catchExnButRethrowTransientIfInternalCall_OnResultTask CallOrigin.Internal

                                    match res with
                                    | Ok _ ->
                                        return Ok ()

                                    | Error (Choice1Of2 err) ->
                                        logger.Info "TRIGGER %a ==> ACT %a ==> ERROR %a" (logger.P "subscription") subscriptionTriggerType (logger.P "action") action (logger.P "error") err
                                        do! enqueueResponse traceContext (TriggerSubscriptionResponse.ActError (err, action))
                                        return Ok ()
                                    | Error (Choice2Of2 exn) ->
                                        do! enqueueResponse traceContext (TriggerSubscriptionResponse.Exn (exn.ToString(), Some action))
                                        return Ok ()

                                | Ok (Choice2Of2 (Some (tickState, reminderUpdate))) -> // noop transition with new tick state
                                    // Note that DedupInfo not written but second attempt (if any) going to be a "true" noop anyway
                                    let maybeSubscriptionResponseSideEffect =
                                        mapSubscriptionResponseToSideEffect currentStateContainer subscriptionTriggerType triggeredLifeEvent traceContext (TriggerSubscriptionResponse.ActOk action)
                                    match!
                                        fun () -> setTickState currentStateContainer tickState (Some reminderUpdate) maybeSubscriptionResponseSideEffect
                                        |> catchExnButRethrowTransientIfInternalCall_OnVoidTask CallOrigin.Internal with
                                    | Ok () ->
                                        return Ok ()
                                    | Error exn ->
                                        do! enqueueResponse traceContext (TriggerSubscriptionResponse.Exn (exn.ToString(), Some action))
                                        return Ok ()

                                | Ok (Choice2Of2 None) -> // noop transition without new tick state. A "true" noop
                                    do! assertStateConsistency ()
                                    do! enqueueResponse traceContext (TriggerSubscriptionResponse.ActOk action)
                                    return Ok ()

                                | Error (LifeCycleError err) ->
                                    logger.Info "TRIGGER %a ==> ACT %a ==> ERROR %a" (logger.P "subscription") subscriptionTriggerType (logger.P "action") action (logger.P "error") err
                                    do! enqueueResponse traceContext (TriggerSubscriptionResponse.ActError (err, action))
                                    return Ok ()

                                | Error TransitionNotAllowed ->
                                    logger.Warn "TRIGGER %a ==> ACT %a ==> ERROR TransitionNotAllowed" (logger.P "subscription") subscriptionTriggerType (logger.P "action") action
                                    do! enqueueResponse traceContext (TriggerSubscriptionResponse.ActNotAllowed action)
                                    return Ok ()

                                | Error (LifeCycleException exn) ->
                                    logger.ErrorExn exn "TRIGGER %a ==> ACT %a ==> ERROR EXCEPTION" (logger.P "subscription") subscriptionTriggerType (logger.P "action") action
                                    do! enqueueResponse traceContext (TriggerSubscriptionResponse.Exn (exn.ToString(), Some action))
                                    return Ok ()

                            | Some (Error x) ->
                                let exn = x.ResolveTriggeredActionException
                                logger.ErrorExn exn "TRIGGER %a ==> RESOLVE ACTION ==> ERROR EXCEPTION" (logger.P "subscription") subscriptionTriggerType
                                do! enqueueResponse (createTraceContext()) (TriggerSubscriptionResponse.Exn (exn.ToString(), None))
                                return Ok ()

                            | None ->
                                do! assertStateConsistency ()
                                logger.Info "TRIGGER %a ==> EVENT %a ==> NOOP (Subject: %a)" (logger.P "subscription") subscriptionTriggerType (logger.P "triggered life event") triggeredLifeEvent (logger.P "subject") currentStateContainer.CurrentSubjectState.Subject
                                return Ok Nothing

                        | Some (SubjectStateContainer.PreparedAction _ )
                        | Some (SubjectStateContainer.PreparedInitialize _) ->
                            return GrainTriggerSubscriptionError.LockedInTransaction |> SubjectFailure.Err |> Error

                        | None ->
                            if lifeCycleAdapter.LifeCycle.IsSessionLifeCycle then
                                // SubjectNotInitialized is waived for sessions, see also SessionOnlyClearState
                                do! assertStateConsistency ()
                                logger.Warn "TRIGGER %a ==> ERROR SubjectNotInitialized downgraded to OK for sessions" (logger.P "subscription") subscriptionTriggerType
                                return Ok Nothing
                            else
                                logger.Info "TRIGGER %a ==> ERROR SubjectNotInitialized" (logger.P "subscription") subscriptionTriggerType
                                return GrainTriggerSubscriptionError.SubjectNotInitialized grainPKey |> SubjectFailure.Err |> Error
                    })
            |> this.WithConsistencyCheckOnError

        member _.EnqueueActions (dedupInfo: SideEffectDedupInfo) (actions: NonemptyList<'LifeAction>) : Task<Result<unit, SubjectFailure<GrainEnqueueActionError>>> =
            this.InvokeTryDedup
                (Some dedupInfo)
                (fun _ -> Ok ())
                (fun () ->
                    task {
                        match primaryStore.Value with
                        // no handler
                        | None ->
                            logger.Warn "ENQUEUE ACTION %a ==> SubjectNotInitialized" (logger.P "action") actions.Head
                            return GrainEnqueueActionError.SubjectNotInitialized grainPKey |> SubjectFailure.Err |> Error

                        // let's not enqueue anything in the middle of txn for now, will consider it later if ever needed
                        | Some (SubjectStateContainer.PreparedAction _)
                        | Some (SubjectStateContainer.PreparedInitialize _) ->
                            logger.Warn "ENQUEUE ACTIONS %a ==> LockedInTransaction" (logger.P "actions") actions
                            return GrainEnqueueActionError.LockedInTransaction |> SubjectFailure.Err |> Error

                        | Some (SubjectStateContainer.Committed currentStateContainer) ->
                            let traceContext = createTraceContext()
                            match!
                                fun () -> enqueueActionsOnSelf currentStateContainer dedupInfo traceContext actions
                                |> catchExnButRethrowTransientIfInternalCall_OnVoidTask CallOrigin.Internal with
                            | Ok () ->
                                if actions.ToList |> List.exists shouldSendActionTelemetry then
                                    logger.Info "ENQUEUE ACTIONS %a ==> OK" (logger.P "actions") actions
                                return Ok ()
                            | Error exn ->
                                logger.ErrorExn exn "ENQUEUE ACTIONS %a ==> ERROR EXCEPTION" (logger.P "actions") actions
                                return exn.ToString() |> SubjectFailure.Exn |> Error
                        })
            |> this.WithConsistencyCheckOnError

        member _.TriggerTimer (dedupInfo: SideEffectDedupInfo) (tentativeDueAction: 'LifeAction) : Task<Result<Option<'LifeAction>, GrainTriggerTimerError<'OpError, 'LifeAction>>> =
            ignore tentativeDueAction // the only reason for this parameter is to make telemetry filter work
            this.InvokeTryDedup
                (Some dedupInfo)
                (fun _ -> Ok None)
                (fun () ->
                    task {
                        let! now = clock.Query Now
                        match primaryStore.Value with
                        | Some (SubjectStateContainer.Committed currentStateContainer) ->
                            match calculateDueTimerActionOrNextDueTick now currentStateContainer.CurrentSubjectState.Subject currentStateContainer.CurrentSubjectState.LastUpdatedOn with
                            | Choice1Of2 (TimerAction.RunAction action) ->
                                use serviceScope = prepareChildScope ()
                                let traceContext = createTraceContext ()
                                let! res = processAction now traceContext currentStateContainer.NextSideEffectSeqNum currentStateContainer.CurrentSubjectState currentStateContainer.CurrentOthersSubscribing action CallOrigin.Internal serviceScope.ServiceProvider

                                match res with
                                | Ok (Choice1Of2 (updatedState, maybeReminderUpdate, blobActions, sideEffectGroup, indexActions, raisedLifeEvents)) ->
                                    if shouldSendActionTelemetry action then
                                        logger.Info "TRIGGER TIMER ACTION ==> ACT %a ==> OK" (logger.P "action") action

                                    let! res =
                                        fun () ->
                                            {
                                                DataToUpdate = {
                                                    UpdatedSubjectState = updatedState
                                                    TraceContext        = traceContext
                                                    ReminderUpdate      = maybeReminderUpdate
                                                    NextSideEffectSeq   = currentStateContainer.NextSideEffectSeqNum + (match sideEffectGroup with Some _ -> 1UL | None -> 0UL)
                                                    SideEffectGroup     = sideEffectGroup
                                                    BlobActions         = blobActions
                                                    IndexActions        = indexActions
                                                }
                                                ActionThatCausedUpdate  = action
                                                SubscriptionsToAdd      = None
                                                ExpectedETag            = currentStateContainer.ETag
                                                CurrentVersion          = currentStateContainer.Version
                                                SkipHistory             = currentStateContainer.SkipHistoryOnNextOp
                                            }
                                            |> updateSubject now raisedLifeEvents (Some dedupInfo) currentStateContainer
                                        |> catchExnButRethrowTransientIfInternalCall_OnResultTask CallOrigin.Internal

                                    return res |> Result.mapBoth (fun _ -> Some action) (function
                                        | Choice1Of2 err -> GrainTriggerTimerError.TransitionError (err, action)
                                        | Choice2Of2 exn -> GrainTriggerTimerError.Exn (exn.ToString(), Some action))

                                | Ok (Choice2Of2 (Some (tickState, reminderUpdate))) -> // noop transition with new tick state
                                    // Note that DedupInfo not written but second attempt (if any) going to be a "true" noop anyway
                                    let! res =
                                        fun () -> setTickState currentStateContainer tickState (Some reminderUpdate) None
                                        |> catchExnButRethrowTransientIfInternalCall_OnVoidTask CallOrigin.Internal
                                    return res |> Result.mapBoth (fun () -> Some action) (fun exn -> GrainTriggerTimerError.Exn (exn.ToString(), Some action))

                                | Ok (Choice2Of2 None) -> // noop transition without new tick state. A "true" noop
                                    do! assertStateConsistency ()
                                    return Ok (Some action)

                                | Error (LifeCycleError err) ->
                                    logger.Warn "TRIGGER TIMER ACTION ==> ACT %a ==> ERROR %a" (logger.P "action") action (logger.P "error") err
                                    return GrainTriggerTimerError.TransitionError (err, action) |> Error

                                | Error TransitionNotAllowed ->
                                    logger.Warn "TRIGGER TIMER ACTION ==> ACT %a ==> ERROR TransitionNotAllowed" (logger.P "action") action
                                    return GrainTriggerTimerError.TransitionNotAllowed action |> Error

                                | Error (LifeCycleException exn) ->
                                    logger.ErrorExn exn "TRIGGER TIMER ACTION ==> ACT %a ==> ERROR EXCEPTION" (logger.P "action") action
                                    return GrainTriggerTimerError.Exn (exn.ToString(), Some action) |> Error

                            | Choice1Of2 TimerAction.DeleteSelf ->
                                do! assertStateConsistency ()
                                logger.Info "TRIGGER TIMER ACTION ==> Expecting DeleteSelf ==> NOOP (Subject: %a)" (logger.P "subject") currentStateContainer.CurrentSubjectState.Subject
                                return Ok None

                            | Choice2Of2 maybeNextDueTick ->
                                let calculatedTickState = maybeNextDueTick |> Option.map (fun on -> TickState.Scheduled (on, None)) |> Option.defaultValue TickState.NoTick
                                if calculatedTickState = currentStateContainer.CurrentSubjectState.TickState then
                                    // subject updated concurrently and TriggerTimer rendered redundant
                                    logger.Info "TRIGGER TIMER ACTION ==> NOOP"
                                else
                                    // very rare but possible path: tick Fired & TriggerTimer SE queued, host stops, timers func changes, host restarts, TriggerTimer runs - must fix the tick
                                    do! setTickState currentStateContainer calculatedTickState (Some { On = maybeNextDueTick; AssumedCurrent = AssumedCurrentReminder.Unknown }) None
                                    logger.Warn "TRIGGER TIMER ACTION ==> No Due Action, tick needs correction => %a" (logger.P "calculatedTickState") calculatedTickState
                                return Ok None

                        | Some (SubjectStateContainer.PreparedAction _) ->
                            logger.Warn "TRIGGER TIMER ACTION  ==> LockedInTransaction"
                            return GrainTriggerTimerError.LockedInTransaction |> Error

                        | None
                        | Some (SubjectStateContainer.PreparedInitialize _) ->
                            logger.Warn "TRIGGER TIMER ACTION ==> SubjectNotInitialized"
                            return GrainTriggerTimerError.SubjectNotInitialized grainPKey |> Error
                    })
            |> this.WithConsistencyCheckOnError

        // Need to interleave because it waits until pending side effects are processed before clearing the state, and some side effects may
        // call back into the same grain (e.g. EnqueueActions) which would lead to a deadlock if not interleaved.
        [<Orleans.Concurrency.AlwaysInterleave>]
        member _.SessionOnlyClearState () : Task =
            task {
                // handle possible concurrency manually
                do! sessionOnlyClearStateMutex.WaitAsync()
                try
                    if not lifeCycleAdapter.LifeCycle.IsSessionLifeCycle then
                        failwith "Can only clear session state"

                    do! assertStateConsistency()

                    // Wait till all side effects are completed, should interleave if a side effect calls back this same grain

                    // TODO: seems like it still deadlocks when side effect calls back into the grain, investigate

                    let finishSideEffectsTask = waitForPendingSideEffectsToFinish ()
                    match! Task.WhenAny(finishSideEffectsTask, Task.Delay 5000) with
                    | task when task = finishSideEffectsTask ->
                        ()
                    | _ ->
                        raise (TransientSubjectException ("SessionOnlyClearState", "must wait for pending side effects to finish, but side-effects haven’t completed processing after waiting for 5 seconds"))

                    match primaryStore.Value with
                    | None -> ()
                    | Some (SubjectStateContainer.Committed currentStateContainer) ->
                        do! grainStorageHandler.ClearState grainPKey currentStateContainer.ETag currentStateContainer.SkipHistoryOnNextOp (* sideEffectId *) None
                        updatePrimaryStoreWithoutNotify None

                        // TODO: try to unsubscribe session before nuking it (disclaimer: it's VERY tricky to do it reliably due to risk of partial failure and racing re-construction of the session)
                        // It was considered to replace forced deletion with special action-like re-construction, similar to revalidation, but it's also very involved because there's code
                        // further down in Api layer which actually wants session to be non-existent
                        if currentStateContainer.CurrentSubjectState.OurSubscriptions.IsNonempty then
                            // See also Session special-case in TriggerSubscription method that waives SubjectNotInitialized
                            logger.Warn "CLEAR SESSION STATE ==> OK but left orphaned subscriptions"

                        if shouldSendSelfDeleteTelemetry currentStateContainer.CurrentSubjectState.Subject.SubjectId then
                            logger.Info "CLEAR SESSION STATE ==> OK"
                    | Some (SubjectStateContainer.PreparedAction _) | Some (SubjectStateContainer.PreparedInitialize _) ->
                        failwith "Not supported within txns"
                finally
                    sessionOnlyClearStateMutex.Release() |> ignore
            }

        member this.RetryPermanentFailures (sideEffectIdsToRetry: NonemptySet<GrainSideEffectId>) : Task =
            task {
                do! assertStateConsistency ()
                match primaryStore.Value with
                | Some (SubjectStateContainer.Committed current) ->
                    do! retryPermanentFailures current sideEffectIdsToRetry
                | Some (SubjectStateContainer.PreparedInitialize _)
                | Some (SubjectStateContainer.PreparedAction _) ->
                    failwith "Unexpected to retry any side effects for a subject in transaction"
                | None ->
                    this.CallDeactivateOnIdle()
            }
            |> Task.Ignore

        member this.InternalOrTestingOnlyTriggerReminder() : Task =
            match remindersImplementation with
            | RemindersImplementation.NotPersisted ->
                cancelReminderScheduledAsTimer()
            | RemindersImplementation.Persisted
            | RemindersImplementation.TestNotPersistedManuallyTriggered ->
                ()
            this.OnSubjectReminder (* logUnexpectedTicks *) false

        member _.TestingOnlyInitializeDirectlyToValue (subjectId: 'SubjectId) (initialValue: 'Subject) (someRandomCtor: 'Constructor) : Task<Result<'Subject, GrainConstructionError<'OpError>>> =
            task {
                match primaryStore.Value with
                | Some _ ->
                    logger.Info "UNSAFE_DIRECT_INIT %a ==> CTOR %a ==> ERROR SubjectAlreadyInitialized" (logger.P "initial value") initialValue (logger.P "ctor") someRandomCtor
                    return Error (GrainConstructionError.SubjectAlreadyInitialized grainPKey)
                | None ->
                    logger.Info "UNSAFE_DIRECT_INIT %a ==> CTOR %a ==> OK" (logger.P "initial value") initialValue (logger.P "ctor") someRandomCtor
                    let! now = clock.Query Now
                    let traceContext = createTraceContext ()
                    let! (updatedState, maybeReminderUpdate, blobActions, sideEffectGroup, indexActions, raisedLifeEvents) =
                        testingOnlyInitializeDirectlyToValue now traceContext subjectId initialValue
                    let writeData = {
                        UpdatedSubjectState = updatedState
                        TraceContext        = traceContext
                        ReminderUpdate      = maybeReminderUpdate
                        NextSideEffectSeq   = match sideEffectGroup with Some _ -> 1UL | None -> 0UL
                        SideEffectGroup     = sideEffectGroup
                        BlobActions         = blobActions
                        IndexActions        = indexActions
                    }

                    let grainIdHash = getGrainIdHash ()
                    let insertData = {
                        DataToInsert = writeData
                        CreatorSubscribing = Map.empty
                        ConstructorThatCausedInsert = someRandomCtor
                        GrainIdHash = grainIdHash
                    }

                    match! initializeSubject now raisedLifeEvents None insertData with
                    | Ok currentStateContainer ->
                        return currentStateContainer.CurrentSubjectState.Subject |> Ok
                    | Error err ->
                        return err |> GrainConstructionError.ConstructionError |> Error
            }

        member _.RefreshTimersAndSubs() : Task<Result<unit, GrainRefreshTimersAndSubsError>> =
            task {
                do! assertStateConsistency ()

                match primaryStore.Value with
                | Some (SubjectStateContainer.PreparedInitialize _)
                | Some (SubjectStateContainer.PreparedAction _) ->
                    return Ok ()
                | None ->
                    logger.Warn "REBUILD TIMERS+SUBS ==> SubjectAlreadyInitialized"
                    return GrainRefreshTimersAndSubsError.SubjectNotInitialized grainPKey |> Error
                | Some (SubjectStateContainer.Committed currentStateContainer) ->
                    let traceContext = createTraceContext ()
                    let currentSubjectState = currentStateContainer.CurrentSubjectState
                    match
                        updateTimersAndSubscriptions
                            lifeCycleAdapter.LifeCycle
                            currentSubjectState.LastUpdatedOn
                            traceContext
                            currentSubjectState.Subject
                            currentSubjectState.TickState
                            currentSubjectState.OurSubscriptions
                            (* repairGrainIdHash *) false with
                    | None ->
                        return Ok () // no update needed
                    | Some (updatedTickState, updatedOurSubscriptions, maybeReminderUpdate, updatedSideEffects) ->
                        let sideEffectGroup = makeNewSideEffectsGroup currentStateContainer.NextSideEffectSeqNum updatedSideEffects
                        do! setTickStateAndSubscriptions currentStateContainer updatedTickState updatedOurSubscriptions maybeReminderUpdate sideEffectGroup
                        return Ok ()
            }

        member this.Observe (address: string) (maybeClientVersion: Option<ComparableVersion>) (observer: ISubjectGrainObserver<'Subject, 'SubjectId>) =
            this.Observe address maybeClientVersion observer

        member _.Unobserve (address: string) =
            this.Unobserve address

    interface ITrackedGrain with
        member this.GetTelemetryData (methodInfo: System.Reflection.MethodInfo) (args: obj[]) : Option<TrackedGrainTelemetryData> =
            let thisAsInterface = this :> ISubjectGrain<_, _, _, _, _, _>
            // ideally they should send telemetry on error
            let untrackedMethods = [
                nameof(thisAsInterface.Observe)
                nameof(thisAsInterface.Unobserve)
                nameof(thisAsInterface.Unsubscribe)
                nameof(thisAsInterface.Get)
                nameof(thisAsInterface.Prepared)
                nameof(thisAsInterface.GetMaybeConstruct)
                nameof(thisAsInterface.IsConstructed)
                nameof(thisAsInterface.TryDeleteSelf)
                nameof(thisAsInterface.InternalOnlyAwaitTimerExpired)
                nameof(thisAsInterface.InternalOnlyClearExpiredObservers)
                nameof(thisAsInterface.InternalOrTestingOnlyTriggerReminder)
            ]
            if (methodInfo <> null && List.contains methodInfo.Name untrackedMethods) ||
                // grain filters fail to get method info for some generic methods and pass null, yet they still run correctly.  See https://github.com/dotnet/orleans/issues/6578
                // e.g. ConstructAndWait, Observe and similar. We don't want to track some of them
                (methodInfo = null &&
                    args
                    |> Seq.exists (
                        function
                        // must be Observe
                        | :? ISubjectGrainObserver<'Subject, 'SubjectId> ->
                            true
                        | _ ->
                            false)) then
                false
            else
                match lifeCycleAdapter.LifeCycle.ShouldSendTelemetry with
                | None ->
                    // track all by default
                    true
                | Some shouldSendTelemetry ->
                    match args with
                    | null -> Array.empty
                    | _ -> args
                    |> Seq.collect (
                        function
                        | :? 'LifeAction as action ->
                            match (box action) with
                            | :? IRedactable as redactable ->
                                ShouldSendTelemetryFor.LifeAction (redactable.Redact() |> unbox) |> Seq.singleton
                            | _ ->
                                ShouldSendTelemetryFor.LifeAction action |> Seq.singleton
                        | :? 'Constructor as ctor ->
                            match (box ctor) with
                            | :? IRedactable as redactable ->
                                ShouldSendTelemetryFor.Constructor (redactable.Redact() |> unbox) |> Seq.singleton
                            | _ ->
                                ShouldSendTelemetryFor.Constructor ctor |> Seq.singleton
                        | :? LifeEvent as event ->
                            ShouldSendTelemetryFor.LifeEvent event |> Seq.singleton
                        | :? NonemptyList<'LifeAction> as actions ->
                            actions.ToList |> Seq.map (fun action ->
                                match (box action) with
                                | :? IRedactable as redactable ->
                                    ShouldSendTelemetryFor.LifeAction (redactable.Redact() |> unbox)
                                | _ ->
                                    ShouldSendTelemetryFor.LifeAction action)
                        | _ ->
                            Seq.empty)
                    |> List.ofSeq
                    |> function
                        | [] ->
                            true
                        | [x]
                        // act-maybe-construct cases, consider only action
                        | [ShouldSendTelemetryFor.LifeAction _ as x; ShouldSendTelemetryFor.Constructor _]
                        | [ShouldSendTelemetryFor.Constructor _; ShouldSendTelemetryFor.LifeAction _ as x] ->
                            shouldSendTelemetry x
                        | xs ->
                            xs |> List.exists shouldSendTelemetry
            |> fun shouldSent ->
                let takesArgOfType (t: Type) =
                    args
                    |> Array.exists (fun arg -> if arg = null then false else arg.GetType().IsAssignableTo(t))
                if shouldSent then
                    if methodInfo <> null then
                        if methodInfo.Name = nameof(thisAsInterface.TriggerSubscription) then
                            OperationType.GrainCallTriggerSubscription, "TriggerSub"
                        elif methodInfo.Name = nameof(thisAsInterface.TriggerTimer) then
                            OperationType.GrainCallTriggerTimer, "TriggerTimer"
                        else
                            OperationType.GrainCallVanilla, methodInfo.Name
                    // working around Orleans bug to deduce actual method name (append "?" to indicate deduction)
                    // TODO: why don't we just track grain telemetry explicitly in methods? It'll be simpler and more reliable albeit verbose (or is it), and we'll lose auto-coverage of new members
                    elif takesArgOfType typeof<SubscriptionTriggerType> && takesArgOfType typeof<LifeEvent> then
                         OperationType.GrainCallTriggerSubscription, "TriggerSub?"
                    else
                        if takesArgOfType typeof<'LifeEvent> && takesArgOfType typeof<'LifeAction> && takesArgOfType typeof<'Constructor> then
                            "ActMaybeConstructAndWait?"
                        elif takesArgOfType typeof<'LifeEvent> && takesArgOfType typeof<'Constructor> then
                            "ConstructAndWait?"
                        elif takesArgOfType typeof<'LifeEvent> && takesArgOfType typeof<'LifeAction> then
                            "ActAndWait?"
                        elif takesArgOfType typeof<ClientGrainCallContext> && takesArgOfType typeof<'LifeAction> && takesArgOfType typeof<'Constructor> then
                            "ActMaybeConstruct?"
                        elif takesArgOfType typeof<ClientGrainCallContext> && takesArgOfType typeof<'LifeAction> then
                            "Act?"
                        elif takesArgOfType typeof<ClientGrainCallContext> && takesArgOfType typeof<'Constructor> then
                            "Construct?"
                        elif takesArgOfType typeof<'LifeAction> && takesArgOfType typeof<'Constructor> && takesArgOfType typeof<Option<ConstructSubscriptions>> then
                            "ActMaybeConstructInterGrain?"
                        elif takesArgOfType typeof<'LifeAction> && takesArgOfType typeof<Option<BeforeActSubscriptions>> then
                            "ActInterGrain?"
                        elif takesArgOfType typeof<'SubjectId> && takesArgOfType typeof<'Constructor> && takesArgOfType typeof<Option<ConstructSubscriptions>> then
                            "ConstructInterGrain?"
                        elif takesArgOfType typeof<'LifeAction> && takesArgOfType typeof<SubjectTransactionId> then
                            "RunPrepareAction?"
                        elif takesArgOfType typeof<'Constructor> && takesArgOfType typeof<SubjectTransactionId> then
                            "PrepareInitialize?"
                        elif takesArgOfType typeof<Map<SubscriptionName, LifeEvent>> then
                            "Subscribe?"
                        elif takesArgOfType typeof<Set<SubscriptionName>> then
                            "Unsubscribe?"
                        elif takesArgOfType typeof<NonemptyList<'LifeAction>> then
                            "EnqueueActions?"
                        else
                            "NULL?"
                        |> fun deducedMethodName -> OperationType.GrainCallVanilla, deducedMethodName
                    |> fun (operationType, methodName) ->
                        Some { Type = operationType; Name = $"%s{lifeCycleAdapter.LifeCycle.Def.LifeCycleKey.LocalLifeCycleName} %s{methodName}"; Scope = logger.Scope; Partition = grainPartition }
                else
                    None
