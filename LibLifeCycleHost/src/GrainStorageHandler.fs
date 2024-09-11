[<AutoOpen>]
module LibLifeCycleHost.GrainStorageHandler

open System
open System.Threading.Tasks
open LibLifeCycle.LifeCycles.Meta
open LibLifeCycle.MetaServices
open LibLifeCycleCore
open LibLifeCycleHost
open LibLifeCycleHost.GrainStorageModel

[<RequireQualifiedAccess>]
type RemindersImplementation =
| Persisted
| NotPersisted
| TestNotPersistedManuallyTriggered

type IGrainStorageHandler =
    abstract member RemindersImplementation : RemindersImplementation

    abstract member RollbackInitializeSubject
                        :  pKey: string
                        -> uniqueIndicesToRelease: list<UniqueIndexToReleaseOnRollback>
                        -> subjectTransactionId: SubjectTransactionId
                        -> eTag: string
                        -> Task

    abstract member RollbackUpdateSubject
                        : pKey: string
                        -> expectedETag: string
                        -> uniqueIndicesToRelease: list<UniqueIndexToReleaseOnRollback>
                        -> subjectTransactionId: SubjectTransactionId
                        -> Task<(* newETag *) string>

    abstract member RebuildIndices
                        :  lifeCycleName: string
                        -> rebuildType: IndexRebuildType
                        -> maybeLastSubjectIdRebuilt: Option<string>
                        -> batchSize: uint16
                        -> Task<RebuildIndicesBatchResult>

    abstract member ReEncodeSubjects
                        :  lifeCycleName: string
                        -> maybeLastSubjectIdReEncoded: Option<string>
                        -> batchSize: uint16
                        -> Task<ReEncodeSubjectsBatchResult>

    abstract member ReEncodeSubjectsHistory
                        :  lifeCycleName: string
                        -> maybeLastSubjectIdVersionReEncoded: Option<string * uint64>
                        -> batchSize: uint16
                        -> Task<ReEncodeSubjectsHistoryBatchResult>

    abstract member ClearExpiredSubjectsHistory
                        :  now: DateTimeOffset
                        -> batchSize: uint16
                        -> Task<uint16>

    abstract member UpdateSideEffectStatus
                        :  pKey: string
                        -> sideEffectId: GrainSideEffectId
                        -> sideEffectResult: GrainSideEffectResult
                        -> Task

    abstract member RemoveSubscriptions
                        :  pKey: string
                        -> expectedETag: string
                        -> subscriberPKeyRef: SubjectPKeyReference
                        -> subscriptionNames: Set<SubscriptionName>
                        -> Task<(* newETag *) string>

    abstract member GetIdsOfSubjectsWithStalledSideEffects
                        : batchSize: uint16
                         -> Task<Set<string>>

    // if an individual storage handler supplies metrics, it should ALWAYS do so, so that monitoring can detect recovery
    abstract member GetSideEffectMetrics
                        : unit
                        -> Task<Option<GetSideEffectMetricsResult>>

    abstract member GetTimerMetrics
                        : unit
                        -> Task<Option<GetTimerMetricsResult>>

    abstract member GetNextSequenceNumber: sequenceName: string -> Task<uint64>

    abstract member PeekCurrentSequenceNumber: sequenceName: string -> Task<uint64>

    abstract member ClearState
                        :  pKey: string
                        -> expectedETag: string
                        -> skipHistory: bool
                        -> sideEffectId: Option<GrainSideEffectId>
                        -> Task

    // throws InconsistentStateException if backing storage different from supplied arguments
    abstract member AssertStateConsistency
                :  pKey: string
                -> expectedETagIfCreated: Option<string>
                -> Task

type IGrainStorageHandler<'Subject, 'LifeAction, 'Constructor, 'LifeEvent, 'SubjectId, 'OpError
                           when 'Subject               :> Subject<'SubjectId>
                           and  'LifeAction            :> LifeAction
                           and  'Constructor           :> Constructor
                           and  'LifeEvent             :> LifeEvent
                           and  'OpError               :> OpError
                           and  'LifeEvent             : comparison
                           and  'SubjectId             :> SubjectId
                           and  'SubjectId             : comparison> =

    inherit IGrainStorageHandler

    abstract member TryReadState
                        :  pKey: string
                        -> Task<Option<ReadStateResult<'Subject, 'Constructor, 'LifeAction, 'LifeEvent, 'OpError, 'SubjectId>>>

    abstract member InitializeSubject
                        :  pKey: string
                        -> dedupInfo: Option<SideEffectDedupInfo>
                        -> data: SubjectInsertData<'Subject, 'Constructor, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>
                        -> Task<Result<InitializeSubjectSuccessResult, 'OpError>>

    abstract member PrepareInitializeSubject
                        :  pKey: string
                        -> preparedSubjectInsertData: PreparedSubjectInsertData<'Subject, 'Constructor, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>
                        -> uniqueIndicesToReserve: list<UniqueIndexToReserveOnPrepare<'OpError>>
                        -> subjectTransactionId: SubjectTransactionId
                        -> Task<Result<(* ETag *) string, 'OpError>>

    abstract member CommitInitializeSubject
                        :  pKey: string
                        -> expectedETag: string
                        -> subjectTransactionId: SubjectTransactionId
                        -> insertData: SubjectInsertData<'Subject, 'Constructor, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>
                        -> Task<InitializeSubjectSuccessResult>

    abstract member UpdateSubject
                        : pKey: string
                        -> dedupData: Option<UpsertDedupData>
                        -> updateData: SubjectUpdateData<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>
                        -> Task<Result<UpdateSubjectSuccessResult, 'OpError>>

    abstract member PrepareUpdateSubject
                        : pKey: string
                        -> expectedETag: string
                        -> preparedUpdateData: PreparedSubjectUpdateData<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>
                        -> uniqueIndicesToReserve: list<UniqueIndexToReserveOnPrepare<'OpError>>
                        -> subjectTransactionId: SubjectTransactionId
                        -> Task<Result<(* newETag *) string, 'OpError>>

    abstract member CommitUpdateSubject
                        : pKey: string
                        -> subjectTransactionId: SubjectTransactionId
                        -> updateData: SubjectUpdateData<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>
                        -> Task<UpdateSubjectSuccessResult>

    abstract member EnqueueSideEffects
                        : pKey: string
                        -> now: DateTimeOffset
                        -> dedupData: Option<UpsertDedupData>
                        -> expectedETag: string
                        -> nextSideEffectSeqNum: GrainSideEffectSequenceNumber
                        -> sideEffectGroups: NonemptyKeyedSet<GrainSideEffectSequenceNumber, SideEffectGroup<'LifeAction, 'OpError>>
                        -> Task<(* newETag *) string>

    abstract member SetTickState
                        : pKey: string
                        -> expectedETag: string
                        -> nextSideEffectSeqNum: GrainSideEffectSequenceNumber
                        -> tickState: TickState
                        -> sideEffectGroup: Option<SideEffectGroup<'LifeAction, 'OpError>>
                        -> Task<(* newETag *) string>

    abstract member SetTickStateAndSubscriptions
                        : pKey: string
                        -> expectedETag: string
                        -> nextSideEffectSeqNum: GrainSideEffectSequenceNumber
                        -> tickState: TickState
                        -> subscriptions: Map<SubscriptionName, SubjectReference>
                        -> sideEffectGroup: Option<SideEffectGroup<'LifeAction, 'OpError>>
                        -> Task<(* newETag *) string>

    abstract member ReadFailedSideEffects
                    : pKey: string
                    -> failedSideEffectIds: NonemptySet<GrainSideEffectId>
                    -> Task<KeyedSet<GrainSideEffectSequenceNumber, SideEffectGroup<'LifeAction, 'OpError>>>

    abstract member RetrySideEffects
                        : pKey: string
                        -> expectedETag: string
                        -> nextSideEffectSeqNum: GrainSideEffectSequenceNumber
                        -> retryingSideEffectGroups: NonemptyKeyedSet<GrainSideEffectSequenceNumber, SideEffectGroup<'LifeAction, 'OpError>>
                        -> Task<(* newETag *) string>

    abstract member AddSubscriptions
                        :  pKey: string
                        -> expectedETag: string
                        -> subscriptionsToAdd: SubscriptionsToAdd<'LifeEvent>
                        -> Task<(* newETag *) string>

    abstract member GetBatchOfSubjectsWithTickStateAndOurSubscriptions
                    : cursor: Option<string>
                    -> batchSize: uint16
                    -> Task<list<'Subject * DateTimeOffset * TickState * Map<SubscriptionName, SubjectReference>>>

    abstract member ProcessPermanentFailures
                : scope: UpdatePermanentFailuresScope
                -> filters: Set<UpdatePermanentFailuresFilter>
                -> operation: UpdatePermanentFailuresOperation
                -> Task<Option<UpdatePermanentFailuresResult<'LifeAction>>>
