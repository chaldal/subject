[<AutoOpen>]
module LibLifeCycleHost.HostedLifeCycleAdapter

open System
open LibLifeCycle
open LibLifeCycle.LifeCycles.Meta
open LibLifeCycle.MetaServices
open LibLifeCycleCore
open LibLifeCycleHost
open Microsoft.Extensions.DependencyInjection
open System.Threading.Tasks
open Orleans

// TODO:  IHostedLifeCycleAdapter & IHostedOrReferencedLifeCycleAdapter can be replaced with function/invoke pattern

// Helps to use a life cycle from a non-generic context when both LifeCycleDef and LifeCycle (impl) are available
type IHostedLifeCycleAdapter =
    inherit IHostedOrReferencedLifeCycleAdapter
    abstract member LifeCycle:               ILifeCycle
    abstract member LifeCycleName:           string
    abstract member Storage:                 StorageType
    abstract member SubjectGrainType:        Type
    abstract member TestingOnlyTriggerReminder: hostEcosystemGrainFactory: IGrainFactory -> grainPartition: GrainPartition -> subjectId: SubjectId -> Task
    abstract member GetGrainStorageHandler:  serviceProvider: IServiceProvider -> IGrainStorageHandler
    abstract member RebuildTimersSubsBatch:  serviceProvider: IServiceProvider -> grainPartition: GrainPartition -> batchCursor: Option<string> -> batchSize: uint16 -> parallelism: uint16 -> repairGrainIdHash: bool -> Task<RebuildTimersSubsBatchResult>
    abstract member UpdatePermanentFailures: serviceProvider: IServiceProvider -> hostEcosystemGrainFactory: IGrainFactory -> grainPartition: GrainPartition -> scope: UpdatePermanentFailuresScope -> filters: Set<UpdatePermanentFailuresFilter> -> operation: UpdatePermanentFailuresOperation -> Task<Result<LastUpdatePermanentFailuresResult, PermanentFailuresUpdateError>>
    abstract member ForceAwake:              hostEcosystemGrainFactory: IGrainFactory -> grainPartition: GrainPartition -> subjectIdStr: string -> Task

type IHostedLifeCycleAdapter<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId
                       when 'Subject              :> Subject<'SubjectId>
                       and  'LifeAction           :> LifeAction
                       and  'OpError              :> OpError
                       and  'Constructor          :> Constructor
                       and  'LifeEvent            :> LifeEvent
                       and  'LifeEvent            : comparison
                       and  'SubjectId            :> SubjectId
                       and  'SubjectId            : comparison> =
    inherit IHostedOrReferencedLifeCycleAdapter<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>
    inherit IHostedLifeCycleAdapter
    abstract member LifeCycle: ILifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>

type HostedLifeCycleAdapter<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId
                       when 'Subject              :> Subject<'SubjectId>
                       and  'LifeAction           :> LifeAction
                       and  'OpError              :> OpError
                       and  'Constructor          :> Constructor
                       and  'LifeEvent            :> LifeEvent
                       and  'LifeEvent            : comparison
                       and  'SubjectId            :> SubjectId
                       and  'SubjectId            : comparison>
                       (lifeCycle: ILifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>) =

    // To implement the IHostedOrReferencedLifeCycleAdapter members, we use composition of separate adapter, but doing so
    // requires an Invoke call to create it.
    let hostedOrReferencedLifeCycleAdapter =
        lifeCycle.Invoke
            { new FullyTypedLifeCycleFunction<_, _, _, _, _, _, _> with
                member _.Invoke (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env>) =
                    {
                        ReferencedLifeCycle = lifeCycle.ToReferencedLifeCycle()
                    }
                    :> IHostedOrReferencedLifeCycleAdapter<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>
            }

    // grain calls invoked from _Meta's _Meta to itself will deadlock, use this check to no-op them away
    member private this.IsMetaOfMeta (subjectIdStr: string) =
        // TODO: try to get rid of IsMetaOfMeta hack, either set AllowCallChainReentrancy to true, or move to side effects
        typeof<'SubjectId> = typeof<MetaId> &&
        lifeCycle.Name = MetaLifeCycleName &&
        subjectIdStr = MetaLifeCycleName

    member this.LifeCycle = lifeCycle

    interface IHostedLifeCycleAdapter with
        member this.ReferencedLifeCycle   = hostedOrReferencedLifeCycleAdapter.ReferencedLifeCycle
        member this.IsSessionLifeCycle    = lifeCycle.IsSessionLifeCycle
        member this.LifeCycleName         = lifeCycle.Name
        member this.LifeCycle             = lifeCycle
        member this.Storage               = lifeCycle.Storage.Type
        member _.SubjectGrainType         = typeof<ISubjectGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>>

        member this.RunActionOnGrain (grainProvider: IBiosphereGrainProvider) (grainPartition: GrainPartition) (maybeDedupInfo: Option<SideEffectDedupInfo>) (subjectIdStr: string) (action: LifeAction) (maybeBeforeActSubscriptions: Option<BeforeActSubscriptions>) : Task<Result<unit, SubjectFailure<GrainTransitionError<OpError>>>> =
            hostedOrReferencedLifeCycleAdapter.RunActionOnGrain grainProvider grainPartition maybeDedupInfo subjectIdStr action maybeBeforeActSubscriptions

        member this.TriggerTimerActionOnGrain (grainProvider: IBiosphereGrainProvider) (grainPartition: GrainPartition) (dedupInfo: SideEffectDedupInfo) (subjectId: SubjectId) (tentativeDueAction: LifeAction) : Task<Result<Option<LifeAction>, GrainTriggerTimerError<OpError, LifeAction>>> =
            hostedOrReferencedLifeCycleAdapter.TriggerTimerActionOnGrain grainProvider grainPartition dedupInfo subjectId tentativeDueAction

        member this.TryDeleteSelfOnGrain (grainProvider: IBiosphereGrainProvider) (grainPartition: GrainPartition) (subjectId: SubjectId) (sideEffectId: GrainSideEffectId) (requiredVersion: uint64) (requiredNextSideEffectSequenceNumber: uint64) (retryAttempt: byte) : Task =
            hostedOrReferencedLifeCycleAdapter.TryDeleteSelfOnGrain grainProvider grainPartition subjectId sideEffectId requiredVersion requiredNextSideEffectSequenceNumber retryAttempt

        member _.RunPrepareActionOnGrain (grainProvider: IBiosphereGrainProvider) (grainPartition: GrainPartition) (subjectId: SubjectId) (action: LifeAction) (transactionId: SubjectTransactionId) : Task<Result<unit, SubjectFailure<GrainPrepareTransitionError<OpError>>>> =
            hostedOrReferencedLifeCycleAdapter.RunPrepareActionOnGrain grainProvider grainPartition subjectId action transactionId

        member _.RunCommitPreparedOnGrain (grainProvider: IBiosphereGrainProvider) (grainPartition: GrainPartition) (subjectId: SubjectId) (transactionId: SubjectTransactionId) : Task =
            hostedOrReferencedLifeCycleAdapter.RunCommitPreparedOnGrain grainProvider grainPartition subjectId transactionId

        member _.RunRollbackPreparedOnGrain (grainProvider: IBiosphereGrainProvider) (grainPartition: GrainPartition) (subjectId: SubjectId) (transactionId: SubjectTransactionId) : Task =
            hostedOrReferencedLifeCycleAdapter.RunRollbackPreparedOnGrain grainProvider grainPartition subjectId transactionId

        member _.RunCheckPhantomPreparedOnGrain (grainProvider: IBiosphereGrainProvider) (grainPartition: GrainPartition) (subjectId: SubjectId) (transactionId: SubjectTransactionId) : Task =
            hostedOrReferencedLifeCycleAdapter.RunCheckPhantomPreparedOnGrain grainProvider grainPartition subjectId transactionId

        member this.RunActionMaybeConstructOnGrain (grainProvider: IBiosphereGrainProvider) (grainPartition: GrainPartition) (maybeDedupInfo: Option<SideEffectDedupInfo>) (subjectId: SubjectId) (action: LifeAction) (ctor: Constructor) (maybeConstructSubscriptions: Option<ConstructSubscriptions>) : Task<Result<unit, SubjectFailure<GrainOperationError<OpError>>>> =
            hostedOrReferencedLifeCycleAdapter.RunActionMaybeConstructOnGrain grainProvider grainPartition maybeDedupInfo subjectId action ctor maybeConstructSubscriptions

        member this.InitializeGrain (grainProvider: IBiosphereGrainProvider) (grainPartition: GrainPartition) (okIfAlreadyInitialized: bool) (maybeDedupInfo: Option<SideEffectDedupInfo>) (subjectId: SubjectId) (ctor: Constructor) (maybeConstructSubscriptions: Option<ConstructSubscriptions>) : Task<Result<unit, SubjectFailure<GrainConstructionError<OpError>>>> =
            hostedOrReferencedLifeCycleAdapter.InitializeGrain grainProvider grainPartition okIfAlreadyInitialized maybeDedupInfo subjectId ctor maybeConstructSubscriptions

        member _.PrepareInitializeGrain (grainProvider: IBiosphereGrainProvider) (grainPartition: GrainPartition) (subjectId: SubjectId) (ctor: Constructor) (transactionId: SubjectTransactionId) : Task<Result<unit, SubjectFailure<GrainPrepareConstructionError<OpError>>>> =
            hostedOrReferencedLifeCycleAdapter.PrepareInitializeGrain grainProvider grainPartition subjectId ctor transactionId

        member _.SubscribeToGrain (grainProvider: IBiosphereGrainProvider) (grainPartition: GrainPartition) (subjectId: SubjectId) (subscriptions: Map<SubscriptionName,LifeEvent>) (subscriberRef: SubjectPKeyReference): Task<Result<unit, SubjectFailure<GrainSubscriptionError>>> =
            hostedOrReferencedLifeCycleAdapter.SubscribeToGrain grainProvider grainPartition subjectId subscriptions subscriberRef

        member _.UnsubscribeFromGrain (grainProvider: IBiosphereGrainProvider) (grainPartition: GrainPartition) (subjectId: SubjectId) (subscriptions: Set<SubscriptionName>) (subscriberRef: SubjectPKeyReference): Task =
            hostedOrReferencedLifeCycleAdapter.UnsubscribeFromGrain grainProvider grainPartition subjectId subscriptions subscriberRef

        member _.TriggerSubscriptionOnGrain (grainProvider: IBiosphereGrainProvider) (grainPartition: GrainPartition) (maybeDedupInfo: Option<SideEffectDedupInfo>) (subjectIdStr: string) (subscriptionTriggerType: SubscriptionTriggerType) (triggeredEvent: LifeEvent): Task<Result<unit, SubjectFailure<GrainTriggerSubscriptionError<OpError>>>> =
            hostedOrReferencedLifeCycleAdapter.TriggerSubscriptionOnGrain grainProvider grainPartition maybeDedupInfo subjectIdStr subscriptionTriggerType triggeredEvent

        member this.GenerateId (grainScopedServiceProvider: IServiceProvider) (callOrigin: CallOrigin) (ctor: Constructor) : Task<Result<SubjectId, OpError>> =
            let typedCtor = ctor :?> 'Constructor
            backgroundTask {
                let (IdGenerationResult idGenTask) = this.LifeCycle.GenerateId callOrigin grainScopedServiceProvider typedCtor
                match! idGenTask with
                | Ok id -> return id :> SubjectId |> Ok
                | Error err -> return err :> OpError |> Error
            }

        member this.TestingOnlyTriggerReminder (hostEcosystemGrainFactory: IGrainFactory) (GrainPartition grainPartition) (subjectId: SubjectId) : Task =
            fun () -> backgroundTask {
                let typedId = subjectId :?> 'SubjectId
                let typedGrain = hostEcosystemGrainFactory.GetGrain<ISubjectGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>>(grainPartition, (getIdString typedId))
                do! typedGrain.InternalOrTestingOnlyTriggerReminder()
            }
            |> OrleansTransientErrorDetection.wrapTransientExceptions
            |> Task.Ignore

        member _.GetGrainStorageHandler (serviceProvider: IServiceProvider) : IGrainStorageHandler =
            serviceProvider.GetRequiredService<IGrainStorageHandler<'Subject, 'LifeAction, 'Constructor, 'LifeEvent, 'SubjectId, 'OpError>>()
            :> IGrainStorageHandler

        member this.ForceAwake (hostEcosystemGrainFactory: IGrainFactory) (GrainPartition grainPartition) (subjectIdStr: string) : Task =
            fun () -> backgroundTask {
                if not (this.IsMetaOfMeta subjectIdStr) then
                    let grain = hostEcosystemGrainFactory.GetGrain<ISubjectGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>>(grainPartition, subjectIdStr)
                    do! grain.ForceAwake ()
            }
            |> OrleansTransientErrorDetection.wrapTransientExceptions
            |> Task.Ignore

        member this.ShouldSendTelemetry (telemetryFor: ShouldSendTelemetryFor<LifeAction, Constructor>) : bool =
            match lifeCycle.ShouldSendTelemetry with
            | Some shouldSendTelemetry ->
                match telemetryFor with
                | ShouldSendTelemetryFor.LifeAction action ->
                    ShouldSendTelemetryFor.LifeAction (action :?> 'LifeAction)
                | ShouldSendTelemetryFor.Constructor constructor ->
                    ShouldSendTelemetryFor.Constructor (constructor :?> 'Constructor)
                | ShouldSendTelemetryFor.LifeEvent event ->
                    ShouldSendTelemetryFor.LifeEvent event
                |> shouldSendTelemetry
            | None ->
                true

        member this.RebuildTimersSubsBatch (serviceProvider: IServiceProvider) (GrainPartition grainPartition) (batchCursor: Option<string>) (batchSize: uint16) (parallelism: uint16) (repairGrainIdHash: bool) : Task<RebuildTimersSubsBatchResult> =
            fun () -> backgroundTask {
                let hostEcosystemGrainFactory = serviceProvider.GetRequiredService<IGrainFactory>()
                let storage = serviceProvider.GetRequiredService<IGrainStorageHandler<'Subject, 'LifeAction, 'Constructor, 'LifeEvent, 'SubjectId, 'OpError>>()
                let! subjectWithTickStateAndOurSubscriptionsList = storage.GetBatchOfSubjectsWithTickStateAndOurSubscriptions batchCursor batchSize

                // We retrieve full subject bodies instead of Ids to double check if there any reason to activate (potentially inactive) grain,
                // It can save a lot of resources because most of the subjects don't have a timer or subscriptions,
                // and even when they do many don't need to be rebuilt

                let subjectIdsThatNeedRefresh =
                    subjectWithTickStateAndOurSubscriptionsList
                    // skip Meta of Meta
                    |> List.filter (
                        fun (subject, subjectLastUpdatedOn, currentTickState, currentOurSubscriptions) ->
                            match
                                this.IsMetaOfMeta (subject.SubjectId :> SubjectId).IdString,
                                updateTimersAndSubscriptions
                                    lifeCycle
                                    subjectLastUpdatedOn
                                    emptyTraceContext // context doesn't matter, we just want to check if anything changed
                                    subject
                                    currentTickState
                                    currentOurSubscriptions
                                    repairGrainIdHash with
                            | false, Some _ ->
                                true
                            | _, None
                            | true, _ ->
                                false)
                    |> Seq.map (fun (subject, _, _, _) -> subject.SubjectId.IdString)

                do! subjectIdsThatNeedRefresh
                    |> Seq.map (fun subjectId ->
                        (fun () ->
                            let grain = hostEcosystemGrainFactory.GetGrain<ISubjectGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>>(grainPartition, subjectId)
                            grain.RefreshTimersAndSubs()
                        )
                    )
                    |> Task.batched (int (max 1us parallelism))
                    |> Task.Ignore

                return
                    match subjectWithTickStateAndOurSubscriptionsList.IsEmpty with
                    | true -> RebuildTimersSubsBatchResult.CompletedBatchNoMoreBatchesPending
                    | false ->
                        let lastRebuiltKey =
                            subjectWithTickStateAndOurSubscriptionsList
                            |> Seq.map (fun (subject, _, _, _) -> subject.SubjectId.IdString)
                            |> Seq.max
                        RebuildTimersSubsBatchResult.CompletedBatch lastRebuiltKey
            }
            |> OrleansTransientErrorDetection.wrapTransientExceptions

        member this.UpdatePermanentFailures (serviceProvider: IServiceProvider) (hostEcosystemGrainFactory: IGrainFactory) (GrainPartition grainPartition) (scope: UpdatePermanentFailuresScope) (filters: Set<UpdatePermanentFailuresFilter>) (operation: UpdatePermanentFailuresOperation) : Task<Result<LastUpdatePermanentFailuresResult, PermanentFailuresUpdateError>> =
            fun () -> backgroundTask {
                let storage = serviceProvider.GetRequiredService<IGrainStorageHandler<'Subject, 'LifeAction, 'Constructor, 'LifeEvent, 'SubjectId, 'OpError>>()
                match! storage.ProcessPermanentFailures scope filters operation with
                | None ->
                    return Error PermanentFailuresUpdateError.NoPermanentFailuresFound
                | Some result ->
                    // ask grains to retry side effect groups
                    do!
                        result.SideEffectIdsToRetry
                        |> Map.toSeq
                        |> Seq.filter (fun (idStr, _) -> not (this.IsMetaOfMeta idStr))
                        |> Seq.map (fun (idStr, sideEffectIdsToRetry) ->
                            hostEcosystemGrainFactory
                                .GetGrain<ISubjectGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>>(grainPartition, idStr)
                                .RetryPermanentFailures sideEffectIdsToRetry)
                        |> Task.WhenAll

                    return Ok result.Last
            }
            |> OrleansTransientErrorDetection.wrapTransientExceptions

    interface IHostedLifeCycleAdapter<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId> with
        member this.LifeCycle = lifeCycle
        member this.ReferencedLifeCycle = hostedOrReferencedLifeCycleAdapter.ReferencedLifeCycle
        member this.ShouldSendTelemetry (telemetryFor: ShouldSendTelemetryFor<'LifeAction, 'Constructor>) : bool =
            match lifeCycle.ShouldSendTelemetry with
            | Some shouldSendTelemetry ->
                shouldSendTelemetry telemetryFor
            | None ->
                true
        member this.ShouldRecordHistory (historyFor: ShouldRecordHistoryFor<'LifeAction, 'Constructor>) : bool =
            match lifeCycle.ShouldRecordHistory with
            | Some shouldRecordHistory ->
                shouldRecordHistory historyFor
            | None ->
                true

type HostedOrReferencedLifeCycleAdapterRegistry = HostedOrReferencedLifeCycleAdapterRegistry of System.Collections.Generic.IDictionary<LifeCycleKey, IHostedOrReferencedLifeCycleAdapter>
    with
        member this.GetLifeCycleBiosphereAdapterByKey key : Option<IHostedOrReferencedLifeCycleAdapter> =
            match this with
            | HostedOrReferencedLifeCycleAdapterRegistry dictionary ->
                match dictionary.TryGetValue key with
                | true, adapter -> Some adapter
                | false, _ -> None

type HostedLifeCycleAdapterCollection = HostedLifeCycleAdapterCollection of HostEcosystemName: string * Map<LifeCycleKey, IHostedLifeCycleAdapter>
    with
        interface System.Collections.Generic.IEnumerable<IHostedLifeCycleAdapter> with
            member this.GetEnumerator(): Collections.Generic.IEnumerator<IHostedLifeCycleAdapter> =
                let (HostedLifeCycleAdapterCollection (_, dictionary)) = this
                dictionary.Values.GetEnumerator()

            member this.GetEnumerator(): Collections.IEnumerator =
                let (HostedLifeCycleAdapterCollection (_, dictionary)) = this
                dictionary.Values.GetEnumerator() :> Collections.IEnumerator

        member this.GetLifeCycleAdapterByKey key : Option<IHostedLifeCycleAdapter> =
            match this with
            | HostedLifeCycleAdapterCollection (_, dictionary) ->
                match dictionary.TryGetValue key with
                | true, adapter -> Some adapter
                | false, _ -> None

        member this.GetLifeCycleAdapterByLocalName name =
            let hostEcosystemName =
                match this with
                | HostedLifeCycleAdapterCollection (hostEcosystemName, _) ->
                    hostEcosystemName
            this.GetLifeCycleAdapterByKey (LifeCycleKey (name, hostEcosystemName))