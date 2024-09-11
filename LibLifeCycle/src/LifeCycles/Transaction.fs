[<AutoOpen>]
module LibLifeCycle.LifeCycles.Transaction.LifeCycle

open LibLifeCycle
open System
open LibLifeCycle.LifeCycles.Transaction
open Microsoft.Extensions.Logging

(*
Generic poor man's implementation of multi-subject transactions.

Limitations:

- supports only flat list of constructors and actions, at most one operation per entity;
  - in theory it can be extended to many operations per entity
- has poor throughput: locks all affected entities, executes actions on the side-effect mailbox i.e. sequentially
- actions with transient side effects not supported / will error out

Features:

- allows client code to decide on commit or rollback (for bigger outer transactions)
- transaction can time out only while running or awaiting for commit or rollback command,
  once commit or rollback initiated, it is guaranteed to finalize sooner or later.

We also considered Orleans Transaction but put them aside for now for following reasons:
- they don't give explicit control of commit or rollback on client side
- force short timeouts that can't be changed
- dictate very specific data access patterns that require another type of grain to share same underlying storage with SubjectGrain
Having said that we still potentially can use Orleans Transaction, they were simply not very suitable for the task at hand.
*)


type SubjectTransactionEnvironment = {
    Clock:  Service<Clock>
    Logger: ILogger<SubjectTransactionEnvironment>
} with interface Env

type TransactionOpMapping<'Op when 'Op : comparison> =
    'Op -> SubjectTransactionId -> SubjectTransactionStep -> TxnBatchOpNo -> ExternalOperation<SubjectTransactionAction<'Op>>

let private createPrepareOperationSideEffect transactionId operationMapping (txnBatchOpNo: uint16 * uint16) op =
    operationMapping op transactionId SubjectTransactionStep.Prepare txnBatchOpNo

let construction
    (operationMapping: TransactionOpMapping<'Op>)
    (env: SubjectTransactionEnvironment) (transactionId: SubjectTransactionId) (ctor: SubjectTransactionConstructor<'Op>)
    : ConstructionResult<SubjectTransaction<'Op>, SubjectTransactionAction<'Op>, SubjectTransactionOpError<'Op>, SubjectTransactionLifeEvent> =

    construction {
        let! now = env.Clock.Query Now

        match ctor with
        | SubjectTransactionConstructor.New (_, operations, timeout) ->

            let runningOperations =
                operations.ToSet
                |> Seq.mapi (fun i op -> { Op = op; No = (1us, uint16 (i + 1)); Finished = false })
                |> NonemptyKeyedSet.ofSeq
                |> Option.get

            let transaction = {
                TransactionId = transactionId
                StartedOn     = now
                // extremely generous default timeout because commit and rollback can come from external clients
                Timeout       = timeout |> Option.defaultValue (TimeSpan.FromMinutes 20.)
                NextNo        = (2us, uint16 (runningOperations.Count.Value + 1))
                State         = SubjectTransactionState.Running (runningOperations, None)
            }

            // start the operations in transaction
            yield!
                runningOperations.Values
                |> Seq.map (fun x -> createPrepareOperationSideEffect transactionId operationMapping x.No x.Op)

            return transaction

        | SubjectTransactionConstructor.NewOrphanedBeforePrepare _ ->
            let transaction = {
                TransactionId = transactionId
                StartedOn     = now
                Timeout       = TimeSpan.Zero
                NextNo        = (1us, 1us)
                State         = SubjectTransactionState.Finalized (TransactionOutcome.RolledBack TransactionRollbackReason.OrphanedBeforePrepare, KeyedSet.empty, now, (* checkedPhantoms *) true)
            }
            return transaction
    }

let transition<'Op when 'Op : comparison>
    (operationMapping: TransactionOpMapping<'Op>)
    (env: SubjectTransactionEnvironment)
    (subjectTransaction: SubjectTransaction<'Op>)
    (action: SubjectTransactionAction<'Op>)
    : TransitionResult<SubjectTransaction<'Op>, SubjectTransactionAction<'Op>, SubjectTransactionOpError<'Op>, SubjectTransactionLifeEvent, SubjectTransactionConstructor<'Op>> =

    let startRollback rollbackReason (preparedOps: seq<IdleTransactionOp<'Op>>) =
        transition {
            let outcome = TransactionOutcome.RolledBack rollbackReason
            yield!
                preparedOps
                |> Seq.map (fun op -> operationMapping op.Op subjectTransaction.TransactionId SubjectTransactionStep.Rollback op.No)
            let finalizingOps =
                preparedOps
                |> Seq.map (fun op -> { Op = op.Op; No = op.No; Finished = false })
                |> NonemptyKeyedSet.ofSeq
                |> Option.get
            return
                { subjectTransaction with
                    State = SubjectTransactionState.Finalizing (outcome, finalizingOps) }
        }

    let selectTransactionStateBasedOnOpStatuses (maybePendingRollbackReason: Option<TransactionRollbackReason>) (updatedRunningOps: NonemptyKeyedSet<'Op, RunningTransactionOp<'Op>>) =
        transition {
            let distinctFinished =
                updatedRunningOps.Values
                |> Seq.map (fun op -> op.Finished)
                |> Seq.distinct
                |> Seq.sortBy (function | false  -> 0 | true -> 1)
                |> List.ofSeq

            match distinctFinished, maybePendingRollbackReason with
            | (* finished *) false :: _, _ ->
                // something still running
                return { subjectTransaction with State = SubjectTransactionState.Running (updatedRunningOps, maybePendingRollbackReason) }

            // all prepared but rollback is pending
            | (* finished *) true :: _, Some rollbackReason ->
                yield SubjectTransactionLifeEvent.OnPreparedOrFailed (* prepared *) false
                // Why rolling back all operations and not just prepared? Because there's a possibility of
                // false failures as a result of faulty non-idempotent retry of already prepared state.
                return! startRollback rollbackReason (updatedRunningOps.Values |> Seq.map (fun op -> { Op = op.Op; No = op.No }))

            | (* finished *) true :: _, None ->
                // all finished and prepared, no rollback requested
                yield SubjectTransactionLifeEvent.OnPreparedOrFailed (* prepared *) true
                let preparedOps = updatedRunningOps |> NonemptyKeyedSet.map (fun op -> { Op = op.Op; No = op.No })
                return { subjectTransaction with State = SubjectTransactionState.Prepared preparedOps }

            | [], _ ->
                shouldNotReachHereBecause "transaction must have at least one operation"
                return subjectTransaction
        }

    transition {

        match subjectTransaction.State with
        | SubjectTransactionState.Running (runningOps, maybePendingRollbackReason) ->

            match action with
            | SubjectTransactionAction.OnOperationPrepared preparedOpNo ->
                match runningOps.Values |> Seq.tryFind (fun { No = (_, opNo) } -> opNo = preparedOpNo) |> Option.map (fun op -> op.Finished, op.Op, op.No) with
                | Some ((* finished *) false, op, no) ->
                    // was running, now prepared
                    return! selectTransactionStateBasedOnOpStatuses maybePendingRollbackReason (runningOps.AddOrUpdate { Op = op; No = no; Finished = true })
                | Some ((* finished *) true, _, _) ->
                    // already prepared
                    return Transition.Ignore
                | None ->
                    return SubjectTransactionOpError.InvalidTransition (subjectTransaction.State, action)

            | SubjectTransactionAction.OnOperationFailed (failedOpNo, error) ->
                match runningOps.Values |> Seq.tryFind (fun { No = (_, opNo) } -> opNo = failedOpNo) |> Option.map (fun op -> op.Finished, op.Op, op.No) with
                | Some ((* finished *) false, op, no) ->
                    // was running, now failed
                    return! selectTransactionStateBasedOnOpStatuses (error |> TransactionRollbackReason.Failure |> Some) (runningOps.AddOrUpdate { Op = op; No = no; Finished = true })
                | Some ((* finished *) true, _, _) ->
                    // already failed
                    return Transition.Ignore
                | None ->
                    // subject not found in this transaction - too bad
                    return SubjectTransactionOpError.InvalidTransition (subjectTransaction.State, action)

            | SubjectTransactionAction.Rollback timedOut ->
                // can schedule roll back before got replies from all operations,
                // but can't start finalizing immediately to avoid risk of orphan prepares
                let pendingRollbackReason =
                    maybePendingRollbackReason
                    |> Option.defaultValue (if timedOut then TransactionRollbackReason.Timeout else TransactionRollbackReason.Request)
                return { subjectTransaction with State = SubjectTransactionState.Running (runningOps, Some pendingRollbackReason) }

            | SubjectTransactionAction.Continue extraOps ->
                if Set.isSubset extraOps.ToSet runningOps.Keys.ToSet then
                    // idempotent continue
                    return Transition.Ignore
                else
                    return SubjectTransactionOpError.InvalidTransition (subjectTransaction.State, action)

            | SubjectTransactionAction.Commit
            | SubjectTransactionAction.OnOperationFinalized _
            | SubjectTransactionAction.CheckForPhantoms ->
                return SubjectTransactionOpError.InvalidTransition (subjectTransaction.State, action)

        | SubjectTransactionState.Prepared preparedOps ->
            match action with
            | SubjectTransactionAction.OnOperationPrepared preparedOpNo ->
                if preparedOps.Values |> Seq.exists (fun { No = (_, opNo) } -> opNo = preparedOpNo)
                then
                    let (SubjectTransactionId txnId) = subjectTransaction.TransactionId
                    env.Logger.LogWarning("Idempotent prepare in Running State, OpNo {opNo}, txn Id {txnId}", preparedOpNo, txnId)
                    // idempotent prepare
                    return Transition.Ignore
                else
                    // unknown prepare
                    return SubjectTransactionOpError.InvalidTransition (subjectTransaction.State, action)

            | SubjectTransactionAction.Continue extraOps ->

                // extra Ops must be either totally different from prepared ops (new continuation) ...
                let sameOps = Set.intersect extraOps.ToSet preparedOps.Keys.ToSet
                if sameOps.IsEmpty then
                    let nextBatchNo, nextOpNo = subjectTransaction.NextNo
                    let extraRunningOps =
                        extraOps.ToSet
                        |> Seq.mapi (fun i op -> { Op = op; No = (nextBatchNo, nextOpNo + uint16 i); Finished = false })
                        |> List.ofSeq

                    let statuses =
                        preparedOps.Values
                        |> Seq.map (fun op -> { Op = op.Op; No = op.No; Finished = true })
                        |> Seq.append extraRunningOps
                        |> NonemptyKeyedSet.ofSeq
                        |> Option.get

                    // start the extra operations in transaction
                    yield!
                        extraRunningOps
                        |> Seq.map (fun op -> createPrepareOperationSideEffect subjectTransaction.TransactionId operationMapping op.No op.Op)

                    return
                        { subjectTransaction with
                            NextNo = (nextBatchNo + 1us, nextOpNo + (uint16 extraOps.Count.Value) + 1us)
                            State = SubjectTransactionState.Running (statuses, None) }

                // ... or be a subset of prepared ops (idempotent retry)
                elif sameOps.Count = extraOps.Count.Value then
                    return Transition.Ignore

                // ... partial subset is the error
                else
                    return SubjectTransactionOpError.InvalidTransition (subjectTransaction.State, action)

            | SubjectTransactionAction.Commit ->
                // side-effects will commit in the order of prepare, see SideEffectProcessor sideEffectPriority
                // txn contract implies that operations in _one set_ can be prepared out of order
                // (ideally in parallel but framework doesn't support it atm), but clients have right to expect
                // that operations from 1st set (constructor) will commit before 2nd set (Continue).
                yield!
                    preparedOps.Values
                    |> Seq.map (fun op -> operationMapping op.Op subjectTransaction.TransactionId SubjectTransactionStep.Commit op.No)
                let finalizingOps =
                    preparedOps
                    |> NonemptyKeyedSet.map (fun op -> { Op = op.Op; No = op.No; Finished = false })
                return
                    { subjectTransaction with
                        State = SubjectTransactionState.Finalizing (TransactionOutcome.Committed, finalizingOps) }

            | SubjectTransactionAction.Rollback timedOut ->
                // rollback side-effects
                let reason =
                    match timedOut with
                    | true  -> TransactionRollbackReason.Timeout
                    | false -> TransactionRollbackReason.Request
                return! startRollback reason preparedOps.Values

            | SubjectTransactionAction.OnOperationFailed _
            | SubjectTransactionAction.OnOperationFinalized _
            | SubjectTransactionAction.CheckForPhantoms ->
                return SubjectTransactionOpError.InvalidTransition (subjectTransaction.State, action)

        | SubjectTransactionState.Finalizing (outcome, finalizingOps) ->
            match (action, outcome) with
            | SubjectTransactionAction.Commit, TransactionOutcome.Committed
            | SubjectTransactionAction.Rollback _, TransactionOutcome.RolledBack _ ->
                // idempotent commit or rollback
                return Transition.Ignore

            | SubjectTransactionAction.OnOperationFinalized finalizedOpNo, _ ->
                let newFinalizingOps =
                    match finalizingOps.Values |> Seq.tryFind (fun { No = (_, opNo) } -> opNo = finalizedOpNo) with
                    | Some op ->
                        finalizingOps.AddOrUpdate { op with Finished = true }
                    | None ->
                        finalizingOps

                if newFinalizingOps.Values |> Seq.exists (fun op -> op.Finished = false) then
                    return { subjectTransaction with State = SubjectTransactionState.Finalizing (outcome, newFinalizingOps) }
                else
                    let! now = env.Clock.Query Now
                    yield SubjectTransactionLifeEvent.OnFinalized outcome
                    let finalizedOps = newFinalizingOps |> NonemptyKeyedSet.map (fun op -> { Op = op.Op; No = op.No }) |> NonemptyKeyedSet.toKeyedSet
                    return { subjectTransaction with State = SubjectTransactionState.Finalized (outcome, finalizedOps, now, (* checkedPhantoms *) false) }

            | SubjectTransactionAction.OnOperationPrepared preparedOpNo, _ ->
                match finalizingOps.Values |> Seq.tryFind (fun { No = (_, opNo) } -> opNo = preparedOpNo), outcome with
                | Some _, TransactionOutcome.Committed ->
                    let (SubjectTransactionId txnId) = subjectTransaction.TransactionId
                    env.Logger.LogWarning("Phantom prepare in Finalizing (Commit), OpNo {opNo}, txn Id {txnId}", preparedOpNo, txnId)
                    // state prepared for something that is already committed or being committed, must be idempotent retry
                    return Transition.Ignore

                | Some op, TransactionOutcome.RolledBack _ ->
                    let (SubjectTransactionId txnId) = subjectTransaction.TransactionId
                    env.Logger.LogWarning("Phantom prepare in Finalizing (Rollback), OpNo {opNo}, txn Id {txnId}", preparedOpNo, txnId)
                    // phantom prepared while rolling back? See comment in Finalized state + OnOperationPrepared action.
                    // Same thing here just in case, although we didn't see this transition in prod
                    yield operationMapping op.Op subjectTransaction.TransactionId SubjectTransactionStep.Rollback op.No
                    return subjectTransaction

                | None, _ ->
                    return SubjectTransactionOpError.InvalidTransition (subjectTransaction.State, action)

            | SubjectTransactionAction.OnOperationFailed (failedOpNo, _), _ ->
                match finalizingOps.Values |> Seq.tryFind (fun { No = (_, opNo) } -> opNo = failedOpNo), outcome with
                | Some _, TransactionOutcome.RolledBack _ ->
                    // state failed for something that is already rolling back, must be idempotent retry
                    return Transition.Ignore
                | Some _, TransactionOutcome.Committed
                | None, _ ->
                    // suspicious failed
                    return SubjectTransactionOpError.InvalidTransition (subjectTransaction.State, action)

            | SubjectTransactionAction.Continue _, _
            | SubjectTransactionAction.Commit, TransactionOutcome.RolledBack _
            | SubjectTransactionAction.Rollback _, TransactionOutcome.Committed
            | SubjectTransactionAction.CheckForPhantoms, _ ->
                 return SubjectTransactionOpError.InvalidTransition (subjectTransaction.State, action)

        | SubjectTransactionState.Finalized (outcome, finalizedOps, finalizedOn, checkedPhantoms) ->
            match (action, outcome) with
            | SubjectTransactionAction.Commit, TransactionOutcome.Committed
            | SubjectTransactionAction.Rollback _, TransactionOutcome.RolledBack _ ->
                // idempotent commit or rollback
                return Transition.Ignore

            | SubjectTransactionAction.OnOperationFinalized finalizedOpNo, _ ->
                match finalizedOps.Values |> Seq.tryFind (fun { No = (_, opNo) } -> opNo = finalizedOpNo) with
                | Some _ ->
                    // idempotent finalize
                    return Transition.Ignore
                | None ->
                    // suspicious finalize
                    return SubjectTransactionOpError.InvalidTransition (subjectTransaction.State, action)

            | SubjectTransactionAction.OnOperationPrepared preparedOpNo, TransactionOutcome.RolledBack _ ->
                match finalizedOps.Values |> Seq.tryFind (fun { No = (_, opNo) } -> opNo = preparedOpNo) with
                | Some op ->
                    let (SubjectTransactionId txnId) = subjectTransaction.TransactionId
                    env.Logger.LogWarning("Phantom prepare in Finalized (Rollback), OpNo {opNo}, txn Id {txnId}", preparedOpNo, txnId)
                    // Transaction was requested to roll back but state eventually prepared! How is it possible?
                    // This is what happened in prod during bad brownout:
                    // * Prepare side effect failed after few retries because target grain was locked by another transaction
                    // * it sent OnOperationFailed to this subject, and this subject received it BUT response to side effect processor timed out
                    // * side effect was retried from beginning - this time Prepare succeeded and locked target grain,
                    //     but transaction grain dismissed it which wasn't a right thing to do.
                    // Instead we must roll back this phantom prepare.
                    yield operationMapping op.Op subjectTransaction.TransactionId SubjectTransactionStep.Rollback op.No
                    // throw back Finalized one into Finalizing
                    let finalizingOps =
                        finalizedOps
                        |> NonemptyKeyedSet.ofKeyedSet
                        |> Option.get
                        |> NonemptyKeyedSet.map (fun ({ No = (_, opNo) } as op) -> { Op = op.Op; No = op.No; Finished = opNo <> preparedOpNo; })
                    return { subjectTransaction with State = SubjectTransactionState.Finalizing (outcome, finalizingOps) }
                | None ->
                    // suspicious prepared
                    return SubjectTransactionOpError.InvalidTransition (subjectTransaction.State, action)

            | SubjectTransactionAction.OnOperationFailed (failedOpNo, _), TransactionOutcome.RolledBack _ ->
                match finalizedOps.Values |> Seq.tryFind (fun { No = (_, opNo) } -> opNo = failedOpNo) with
                | Some _ ->
                    // idempotent failed
                    return Transition.Ignore
                | None ->
                    // suspicious failed
                    return SubjectTransactionOpError.InvalidTransition (subjectTransaction.State, action)

            | SubjectTransactionAction.CheckForPhantoms, _ ->
                if checkedPhantoms then
                    env.Logger.LogWarning("Check for phantom prepares already done. Ignore")
                    return Transition.Ignore
                elif finalizedOps.IsEmpty then
                    env.Logger.LogWarning("Check for phantom prepares invoked for empty finalized transaction. Ignore")
                    return Transition.Ignore
                else
                    yield! finalizedOps.Values |> Seq.map (fun op -> operationMapping op.Op subjectTransaction.TransactionId SubjectTransactionStep.CheckPhantom op.No)
                    return { subjectTransaction with State = SubjectTransactionState.Finalized (outcome, finalizedOps, finalizedOn, (* checkedPhantoms *) true) }

            | SubjectTransactionAction.OnOperationPrepared _, TransactionOutcome.Committed
            | SubjectTransactionAction.OnOperationFailed _, TransactionOutcome.Committed
            | SubjectTransactionAction.Continue _, _
            | SubjectTransactionAction.Commit, TransactionOutcome.RolledBack _
            | SubjectTransactionAction.Rollback _, TransactionOutcome.Committed ->
                return SubjectTransactionOpError.InvalidTransition (subjectTransaction.State, action)
    }

let idGeneration (_env: SubjectTransactionEnvironment) ctor : IdGenerationResult<SubjectTransactionId, SubjectTransactionOpError<'Op>> =
    idgen {
        match ctor with
        | SubjectTransactionConstructor.New (transactionId, _, _)
        | SubjectTransactionConstructor.NewOrphanedBeforePrepare transactionId ->
            return transactionId
    }

let private lifeEventSatisfies (input: LifeEventSatisfiesInput<SubjectTransactionLifeEvent>) =
    match input.Subscribed, input.Raised with
    | SubjectTransactionLifeEvent.OnFinalized _, SubjectTransactionLifeEvent.OnFinalized _ -> true
    | SubjectTransactionLifeEvent.OnPreparedOrFailed _, SubjectTransactionLifeEvent.OnPreparedOrFailed _ -> true
    | _ -> false

let timers<'Op when 'Op : comparison> (subjectTransaction: SubjectTransaction<'Op>) : List<Timer<SubjectTransactionAction<'Op>>> =
    [
        match subjectTransaction.State with
        | SubjectTransactionState.Running _
        | SubjectTransactionState.Prepared _ ->
            { TimerAction = TimerAction.RunAction (SubjectTransactionAction<'Op>.Rollback (* timedOut *) true)
              Schedule    = Schedule.On <| subjectTransaction.StartedOn.Add subjectTransaction.Timeout }

        | SubjectTransactionState.Finalizing _ ->
            ()

        | SubjectTransactionState.Finalized (_, _, _, (* checkedPhantoms *) false) ->
            // despite our best efforts a bad brownout still can produce a phantom prepare via complex scenario
            // of duplicate txn grain activation with rehydrated prepare side effects, it prepares target grain
            // but fails to notify txn grain so it can heal itself - because silo dies.
            // Until framework fixed it's best to stay pessimistic and double check for such phantoms
            { TimerAction = TimerAction.RunAction SubjectTransactionAction<'Op>.CheckForPhantoms
              Schedule    = Schedule.AfterLastTransition (TimeSpan.FromMinutes 10.) }

        | SubjectTransactionState.Finalized (_, _, _, (* checkedPhantoms *) true) ->
            // Delete fully finalized transaction after a while.  Do not delete too soon as it can confuse
            // resolution of orphaned transactions in external distributed transactions.
            // TODO: Delete history too
            { TimerAction = TimerAction.DeleteSelf
              Schedule    = Schedule.AfterLastTransition (TimeSpan.FromDays 7.) }
    ]

let private shouldSendTelemetry =
    function
    | ShouldSendTelemetryFor.LifeAction action ->
        match action with
        | SubjectTransactionAction.Continue _
        | SubjectTransactionAction.Commit
        | SubjectTransactionAction.Rollback _ -> true
        | SubjectTransactionAction.OnOperationPrepared _
        | SubjectTransactionAction.OnOperationFailed _
        | SubjectTransactionAction.OnOperationFinalized _
        | SubjectTransactionAction.CheckForPhantoms ->
            // TODO: must be false.  set true to investigate ongoing txn bugs
            true
    | ShouldSendTelemetryFor.Constructor _ ->
        // TODO: must be false.  set true to investigate ongoing txn bugs
        true
    | ShouldSendTelemetryFor.LifeEvent _ -> false

let createSubjectTransactionLifeCycle
    (lifeCycleDef: LifeCycleDef<_, _, _, _, _, _, _>)
    (operationMapping: TransactionOpMapping<'op>)
    (accessRules: List<AccessRule<_, _, _, _>>)
    : LifeCycle<SubjectTransaction<'op>, SubjectTransactionAction<'op>, SubjectTransactionOpError<'op>, SubjectTransactionConstructor<'op>, SubjectTransactionLifeEvent, SubjectTransactionIndex<'op>, SubjectTransactionId, AccessPredicateInput, 'Session, 'Role, SubjectTransactionEnvironment> =

    LifeCycleBuilder.newLifeCycle lifeCycleDef

    |> LifeCycleBuilder.withApiAccessRestrictedByRules        accessRules
    |> LifeCycleBuilder.withTransition         (transition operationMapping)
    |> LifeCycleBuilder.withIdGeneration       idGeneration
    |> LifeCycleBuilder.withConstruction       (construction operationMapping)
    |> LifeCycleBuilder.withLifeEventSatisfies lifeEventSatisfies
    |> LifeCycleBuilder.withTimers             timers
    |> LifeCycleBuilder.withTelemetryRules     shouldSendTelemetry
    |> LifeCycleBuilder.withStorage
           (StorageType.Persistent
                (PromotedIndicesConfig.Empty,
                System.TimeSpan.FromDays 365. // expire history records after a year to reclaim space
                // TODO: ideally should be AfterSubjectDeletion but need to populate tombstones in prod which is prohibitively expensive because tables are huge.
                // For now use AfterSubjectChange, after the period specified above (year?) tombstones will be up to date,
                // then we can add the index (see 20241021_00_AddHistoryTombstoneFlag.sql) and switch to AfterSubjectDeletion.
                |> PersistentHistoryExpiration.AfterSubjectChange |> Some
                |> PersistentHistoryRetention.Unfiltered))
    |> LifeCycleBuilder.build
