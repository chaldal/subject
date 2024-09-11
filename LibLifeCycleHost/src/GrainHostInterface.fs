namespace LibLifeCycleHost

open Orleans
open System.Threading.Tasks
open LibLifeCycle
open LibLifeCycleCore
open System

type ISubjectGrainObserver<'Subject, 'SubjectId
        when 'Subject   :> Subject<'SubjectId>
        and  'SubjectId :> SubjectId
        and 'SubjectId : comparison> =
    inherit IGrainObserver

    abstract OnUpdate: SubjectChange<'Subject, 'SubjectId> -> unit

type IViewGrain<'Input, 'Output, 'OpError
    when 'Input :> ViewInput<'Input>
    and  'Output :> ViewOutput<'Output>
    and  'OpError :> ViewOpError<'OpError>
    and  'OpError :> OpError> =
    // inherits all client methods
    inherit IViewClientGrain<'Input, 'Output, 'OpError>

type ISubjectGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId
                    when 'Subject              :> Subject<'SubjectId>
                    and  'LifeAction           :> LifeAction
                    and  'OpError              :> OpError
                    and  'Constructor          :> Constructor
                    and  'LifeEvent            :> LifeEvent
                    and  'LifeEvent            :  comparison
                    and  'SubjectId            :> SubjectId
                    and  'SubjectId            :  comparison> =

    // inherits all client methods
    inherit ISubjectClientGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>

    abstract member Subscribe: Map<SubscriptionName, LifeEvent> -> SubjectPKeyReference -> Task<Result<unit, SubjectFailure<GrainSubscriptionError>>>
    abstract member Unsubscribe: Set<SubscriptionName> -> SubjectPKeyReference -> Task
    abstract member TriggerSubscription: maybeDedupInfo: Option<SideEffectDedupInfo> -> SubscriptionTriggerType -> LifeEvent -> Task<Result<unit, SubjectFailure<GrainTriggerSubscriptionError<'OpError>>>>
    abstract member InternalOnlyAwaitTimerExpired: lifeEvent: 'LifeEvent -> Task
    abstract member InternalOnlyClearExpiredObservers: unit -> Task
    abstract member InternalOrTestingOnlyTriggerReminder: unit -> Task
    abstract member TestingOnlyInitializeDirectlyToValue: subjectId: 'SubjectId -> initialValue: 'Subject -> someRandomCtor: 'Constructor -> Task<Result<'Subject, GrainConstructionError<'OpError>>>
    abstract member RefreshTimersAndSubs: unit -> Task<Result<unit, GrainRefreshTimersAndSubsError>> // TODO: SubjectFailure?
    abstract member EnqueueActions: dedupInfo: SideEffectDedupInfo -> actions: NonemptyList<'LifeAction> -> Task<Result<unit, SubjectFailure<GrainEnqueueActionError>>>
    abstract member TriggerTimer: dedupInfo: SideEffectDedupInfo -> tentativeDueAction: 'LifeAction -> Task<Result<Option<'LifeAction>, GrainTriggerTimerError<'OpError, 'LifeAction>>>

    // Need to interleave because it waits until pending side effects are processed before clearing the state, and some side effects may
    // call back into the same grain (e.g. EnqueueActions) which would lead to a deadlock if not interleaved.
    [<Orleans.Concurrency.AlwaysInterleave>]
    abstract member SessionOnlyClearState: unit -> Task

    // Versions of client methods for inter-grain messaging (to save on network traffic and serialization costs, deduplicate retries, wrap life cycle exceptions etc.)
    [<Obsolete("ActInterGrain is obsolete, use ActInterGrainV2, this method will be deleted when whole Biosphere upgraded")>]
    abstract member ActInterGrain: maybeDedupInfo: Option<SideEffectDedupInfo> -> includeResponse: bool -> action: 'LifeAction -> Task<Result<Option<'Subject>, SubjectFailure<GrainTransitionError<'OpError>>>>
    abstract member ActInterGrainV2: maybeDedupInfo: Option<SideEffectDedupInfo> -> action: 'LifeAction -> maybeBeforeActSubscriptions: Option<BeforeActSubscriptions> -> Task<Result<unit, SubjectFailure<GrainTransitionError<'OpError>>>>
    abstract member ActMaybeConstructInterGrain: maybeDedupInfo: Option<SideEffectDedupInfo> -> includeResponse: bool -> action: 'LifeAction -> ctor: 'Constructor -> maybeConstructSubscriptions: Option<ConstructSubscriptions> -> Task<Result<Option<'Subject>, SubjectFailure<GrainOperationError<'OpError>>>>
    abstract member ConstructInterGrain: okIfAlreadyInitialized: bool -> maybeDedupInfo: Option<SideEffectDedupInfo> -> includeResponse: bool -> subjectId: 'SubjectId -> ctor: 'Constructor -> maybeConstructSubscriptions: Option<ConstructSubscriptions> -> Task<Result<Option<'Subject>, SubjectFailure<GrainConstructionError<'OpError>>>>
    abstract member ForceAwake: unit -> Task  // NO-OP to wake up the grain
    abstract member TryDeleteSelf: sideEffectId: GrainSideEffectId -> requiredVersion: uint64 -> requiredNextSideEffectSequenceNumber: uint64 -> retryAttempt: byte -> Task

    // transaction internals
    abstract member RunPrepareAction: action: 'LifeAction -> SubjectTransactionId -> Task<Result<unit, SubjectFailure<GrainPrepareTransitionError<'OpError>>>>
    abstract member RunCommitPrepared: SubjectTransactionId -> Task
    abstract member RunRollbackPrepared: SubjectTransactionId -> Task
    abstract member RunCheckPhantomPrepared: SubjectTransactionId -> Task
    abstract member PrepareInitialize: subjectId: 'SubjectId -> ctor: 'Constructor -> SubjectTransactionId -> Task<Result<unit, SubjectFailure<GrainPrepareConstructionError<'OpError>>>>

    // side effect error handling
    abstract member RetryPermanentFailures: sideEffectIdsToRetry: NonemptySet<GrainSideEffectId> -> Task

    // observing
    abstract member Observe: address: string -> maybeClientVersion: Option<ComparableVersion> -> observer: ISubjectGrainObserver<'Subject, 'SubjectId> -> Task<unit>
    abstract member Unobserve: address: string -> Task

type IDynamicSubscriptionDispatcherGrain =
    inherit IGrainWithGuidCompoundKey
    abstract member TriggerSubscription: maybeDedupInfo: Option<SideEffectDedupInfo> -> target: LocalSubjectPKeyReference -> SubscriptionTriggerType -> LifeEvent -> Task<Result<unit, SubjectFailure<GrainTriggerDynamicSubscriptionError>>>

type IConnectorGrain<'Request, 'Env> =
    inherit IGrainWithGuidKey
    abstract member SendRequest: buildRequest: (ResponseChannel<'Reply> -> 'Request) -> buildAction: ('Reply -> LifeAction) -> requestor: SubjectPKeyReference -> sideEffectId: GrainSideEffectId -> Task
    abstract member SendRequestMultiResponse: buildRequest: (MultiResponseChannel<'Reply> -> 'Request) -> buildAction: ('Reply -> LifeAction) -> requestor: SubjectPKeyReference -> sideEffectId: GrainSideEffectId -> Task

// grains should implement this interface to report telemetry
type ITrackedGrain =
    abstract member GetTelemetryData: methodInfo: System.Reflection.MethodInfo -> args: obj[] -> Option<TrackedGrainTelemetryData>
and TrackedGrainTelemetryData = {
    Type: TelemetryModel.OperationType
    Name: string
    Scope: LoggerScopeDictionary
    Partition: GrainPartition
}


/// Used for testing, so we can re-activate reminders after fudging the clock
[<AllowNullLiteral>]
type IReminderHook =
    abstract member RegisterReminder: logger: IFsLogger -> lifeCycleKey: LifeCycleKey -> grainPartiton: GrainPartition -> subjectId: SubjectId -> on: DateTimeOffset -> unit
    abstract member ClearReminder:    logger: IFsLogger -> lifeCycleKey: LifeCycleKey -> grainPartiton: GrainPartition -> subjectId: SubjectId -> unit
    abstract member FirstReminderOn:  grainPartition: GrainPartition -> Option<DateTimeOffset>


/// Used for testing and local Dev Host to inject random retries side effect processor.
/// This helps to check whether ecosystem can sustain transient errors and process crashes in the wild.
[<AllowNullLiteral>]
type ITransientSideEffectFailureHook =
    abstract member ShouldInjectOddRetry : retryNo: uint32 -> bool
    // Ideally hook should throw exceptions but it's slower and leads to red tests due to captured bad logs.
    // Consider throwing exceptions in Dev env only

/// Used for testing to detect system stasis
[<AllowNullLiteral>]
type ISideEffectTrackerHook =
    abstract member OnNewSideEffects : newSideEffectIds: seq<GrainSideEffectId * GrainSideEffect<LifeAction, OpError>> -> unit
    abstract member OnSideEffectProcessed : processedSideEffectId: GrainSideEffectId -> unit
    abstract member WaitForAllSideEffectsProcessed : waitFor: TimeSpan -> Task<Result<(* version *)int, list<GrainSideEffect<LifeAction, OpError>>>>

type IBiosphereGrainProvider =
    abstract member GetGrainFactory : ecosystemName: string -> Task<IGrainFactory>
    abstract member IsHostedLifeCycle: lcKey: LifeCycleKey -> bool
    abstract member Close : unit -> Task