namespace LibLifeCycleHost.Storage.Volatile

open LibLifeCycle.LifeCycles.Meta
open LibLifeCycleCore
open LibLifeCycleHost
open LibLifeCycleHost.GrainStorageModel
open System.Threading.Tasks

type VolatileGrainStorageHandler<'Subject, 'LifeAction, 'Constructor, 'LifeEvent, 'SubjectId, 'OpError
                                  when 'Subject      :> Subject<'SubjectId>
                                  and  'LifeAction   :> LifeAction
                                  and  'OpError      :> OpError
                                  and  'Constructor  :> Constructor
                                  and  'LifeEvent    :> LifeEvent
                                  and  'LifeEvent    : comparison
                                  and  'SubjectId    :> SubjectId
                                  and  'SubjectId    : comparison> (remindersTriggeredManually: bool) =

    let assertNoBlobs (writeData: SubjectWriteData<_, _, _, _, _>) =
        if not writeData.BlobActions.IsEmpty then
            failwith "Volatile subjects don't support blobs"

    let sequences = System.Collections.Concurrent.ConcurrentDictionary<string, Ref<uint64>>()

    let reminderImplementation =
        if remindersTriggeredManually then
            RemindersImplementation.TestNotPersistedManuallyTriggered
        else
            RemindersImplementation.NotPersisted


    interface IGrainStorageHandler<'Subject, 'LifeAction, 'Constructor, 'LifeEvent, 'SubjectId, 'OpError> with

        member _.TryReadState(_pKey: string) =
            Task.FromResult None

        member _.RemindersImplementation = reminderImplementation

        member _.InitializeSubject (_pKey: string) (_dedupInfo: Option<SideEffectDedupInfo>) (insertData: SubjectInsertData<'Subject, 'Constructor, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>) : Task<Result<InitializeSubjectSuccessResult, 'OpError>> =
            assertNoBlobs insertData.DataToInsert

            { ETag = System.Guid.NewGuid().ToString()
              Version = 0UL
              SkipHistoryOnNextOp = false }

            |> Ok
            |> Task.FromResult

        member _.PrepareInitializeSubject (_pKey: string) (_preparedSubjectInsertData: PreparedSubjectInsertData<'Subject, 'Constructor, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>) (_uniqueIndicesToReserve: list<UniqueIndexToReserveOnPrepare<'OpError>>) (_subjectTransactionId: SubjectTransactionId) : Task<Result<(* ETag *) string, 'OpError>> =
            failwith "Volatile subjects can't participate in transactions"

        member _.CommitInitializeSubject (_pKey: string) (_eTag: string) (_subjectTransactionId: SubjectTransactionId) (_insertData: SubjectInsertData<'Subject, 'Constructor, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>) : Task<InitializeSubjectSuccessResult> =
            failwith "Volatile subjects can't participate in transactions"

        member _.RollbackInitializeSubject (_pKey: string) (_uniqueIndicesToRelease: list<UniqueIndexToReleaseOnRollback>) (_subjectTransactionId: SubjectTransactionId) (_eTag: string) : Task =
            failwith "Volatile subjects can't participate in transactions"

        member _.ClearState(_pKey: string) _expectedETag _skipHistory _sideEffectId =
            Task.CompletedTask

        member _.AssertStateConsistency (_pKey: string) (_expectedETagIfCreated: Option<string>) : Task =
            Task.CompletedTask

        member _.ClearExpiredSubjectsHistory _now _batchSize =
            Task.FromResult 0us

        member _.UpdateSideEffectStatus _pKey _sideEffectId _sideEffectResult =
            Task.CompletedTask

        // FIXME -- Volatile needs real indices
        member this.RebuildIndices (_lifeCycleName: string) (_rebuildType: IndexRebuildType) (_maybeLastSubjectIdRebuilt: Option<string>) (_batchSize: uint16) : Task<RebuildIndicesBatchResult> =
            Task.FromResult RebuildIndicesBatchResult.CompletedBatchNoMoreBatchesPending

        member this.ReEncodeSubjects (_lifeCycleName: string) (_maybeLastSubjectIdReEncoded: Option<string>) (_batchSize: uint16) : Task<ReEncodeSubjectsBatchResult> =
            Task.FromResult ReEncodeSubjectsBatchResult.CompletedBatchNoMoreBatchesPending

        member this.ReEncodeSubjectsHistory (_lifeCycleName: string) (_maybeLastSubjectIdVersionReEncoded: Option<string * uint64>) (_batchSize: uint16) : Task<ReEncodeSubjectsHistoryBatchResult> =
            Task.FromResult ReEncodeSubjectsHistoryBatchResult.CompletedBatchNoMoreBatchesPending

        member this.UpdateSubject _pKey _dedupData updateData =
            { NewETag = System.Guid.NewGuid().ToString()
              NewVersion = updateData.CurrentVersion + 1UL
              SkipHistoryOnNextOp = false }
            |> Ok
            |> Task.FromResult

        member this.PrepareUpdateSubject _pKey _expectedETag _preparedUpdateData _uniqueIndicesToReserve _subjectTransactionId =
            failwith "Volatile subjects can't participate in transactions"

        member this.RollbackUpdateSubject _pKey _expectedETag _uniqueIndicesToReserve _subjectTransactionId =
            failwith "Volatile subjects can't participate in transactions"

        member _.CommitUpdateSubject (_pKey: string) (_subjectTransactionId: SubjectTransactionId) (_updateData: SubjectUpdateData<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>) : Task<UpdateSubjectSuccessResult> =
            failwith "Volatile subjects can't participate in transactions"

        member _.EnqueueSideEffects (_pKey: string) (_now: System.DateTimeOffset) (_dedupData: Option<UpsertDedupData>) (_expectedETag: string) (_nextSideEffectSeqNum: GrainSideEffectSequenceNumber) (_sideEffectGroups: NonemptyKeyedSet<GrainSideEffectSequenceNumber, SideEffectGroup<'LifeAction, 'OpError>>) : Task<(* newETag *) string> =
            System.Guid.NewGuid().ToString()
            |> Task.FromResult

        member _.SetTickState (_pKey: string) (_expectedETag: string) (_nextSideEffectSeqNum: GrainSideEffectSequenceNumber) (_tickState: TickState) (_sideEffectGroup: Option<SideEffectGroup<'LifeAction, 'OpError>>) : Task<(* newETag *) string> =
            System.Guid.NewGuid().ToString()
            |> Task.FromResult

        member this.SetTickStateAndSubscriptions _pKey _expectedETag _nextSideEffectSeqNum _tickState _subscriptions _sideEffects =
            System.Guid.NewGuid().ToString()
            |> Task.FromResult

        member this.ReadFailedSideEffects _pKey _failedSideEffectIds =
            failwith "Volatile storage doesn't support retrying of failed side effects"

        member this.RetrySideEffects _pKey _expectedETag _nextSideEffectSeqNum _retryingSideEffectGroups =
            failwith "Volatile storage doesn't support retrying of failed side effects"

        member this.AddSubscriptions _pKey _expectedETag _subscriptionsToAdd =
            System.Guid.NewGuid().ToString() |> Task.FromResult

        member this.RemoveSubscriptions _pKey _expectedETag _subscriberPKeyRef _subscriptionNames =
            System.Guid.NewGuid().ToString() |> Task.FromResult

        member _.GetIdsOfSubjectsWithStalledSideEffects _ =
            Task.FromResult Set.empty

        member _.GetBatchOfSubjectsWithTickStateAndOurSubscriptions _ _ =
            Task.FromResult List.empty

        member _.ProcessPermanentFailures _ _ _ =
            Task.FromResult None

        member _.GetSideEffectMetrics () =
            Task.FromResult None

        member _.GetTimerMetrics () =
            Task.FromResult None

        member _.GetNextSequenceNumber (sequenceName: string) : Task<uint64> =
            let refCell = sequences.GetOrAdd(sequenceName, fun _ -> (ref 0UL))
            System.Threading.Interlocked.Increment refCell
            |> Task.FromResult

        member _.PeekCurrentSequenceNumber (sequenceName: string) : Task<uint64> =
            let refCell = sequences.GetOrAdd(sequenceName, fun _ -> (ref 0UL))
            refCell.Value
            |> Task.FromResult