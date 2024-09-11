module LibLifeCycleCore.Transaction.Distributed

open System
open System.Threading.Tasks
open LibLifeCycle.LifeCycles.Transaction
open LibLifeCycleCore

/// State of outer transaction scope when it resets e.g. when ran to completion or on exception
[<RequireQualifiedAccess>]
type OuterTxnOutcome =
    | Committed
    | Aborted
    | Inconclusive

/// Implemented by ultimate consumer of the ecosystem that needs a distributed transaction.
/// Outer transaction manager is responsible for committing outer transaction and taking care
/// of orphan ecosystem transactions (in case of partial failures).
type IOuterTxnManager =

    /// Indicates whether outer transaction started. Ecosystem transaction can't start
    /// earlier than outer transaction.
    abstract member InTransaction : bool

    /// Outer transaction timeout, used as the timeout for preparing ecosystem transaction
    abstract member Timeout : TimeSpan

    /// Tell outer transaction that ecosystem transaction is about to initiate.
    /// Outer transaction must write-ahead transaction token to some persistent storage,
    /// that will survive in event of transaction crash.
    abstract member EnlistEcosystemTransaction :           ecosystemName: string -> transactionToken: Guid -> Task

    /// Notify outer transaction when ecosystem transaction prepared, so outer transaction can continue.
    /// Outer transaction must mark its token as prepared
    abstract member NotifyEcosystemTransactionPrepared :   ecosystemName: string -> transactionToken: Guid -> Task

    /// Notify outer transaction when previously prepared ecosystem transaction continues with more operations.
    /// Outer transaction manager must mark its previously prepared token as not prepared again.
    abstract member NotifyEcosystemTransactionContinuing : ecosystemName: string -> transactionToken: Guid -> Task

    /// Tell outer transaction manager to delete its ecosystem transaction token.
    /// This happens when both inner and outer transaction are 100% completed (successfully or not),
    /// i.e. right after outer transaction completes or fails (most of the time), or later in orphan cleanup job
    abstract member ForgetEcosystemTransaction :           ecosystemName: string -> transactionToken: Guid -> Task

    abstract member SetOuterTxnResetHandler:               handler: (OuterTxnOutcome -> Task) -> unit

// Public functions to allow external client deal with orphaned ecosystem transactions

let commitOrphanedTransaction (grainConnector: GrainConnector) (txnLifeCycleDef: SubjectTransactionLifeCycleDef<'Op>) (transactionId: SubjectTransactionId)
    : Task<Result<unit, GrainTransitionError<SubjectTransactionOpError<'Op>>>> =
    backgroundTask {
        let action = SubjectTransactionAction.Commit
        match! grainConnector.Act txnLifeCycleDef (transactionId :> SubjectId).IdString action with
        | Ok _ ->
            return Ok ()
        | Error err ->
            return Error err
    }

let rollbackOrphanTransaction (grainConnector: GrainConnector) (txnLifeCycleDef: SubjectTransactionLifeCycleDef<'Op>) (transactionId: SubjectTransactionId)
    : Task<Result<unit, GrainOperationError<SubjectTransactionOpError<'Op>>>> =
    // orphan rollback is more subtle than commit, because transaction may be deemed orphaned before it had a chance to be created!
    // If that's the case then create dummy finalized rolled back transaction to prevent further late attempts to prepare it
    backgroundTask {
        let action = SubjectTransactionAction.Rollback (* timedOut *) false
        let ctor = SubjectTransactionConstructor.NewOrphanedBeforePrepare transactionId
        match! grainConnector.ActMaybeConstruct txnLifeCycleDef (transactionId :> SubjectId).IdString action ctor with
        | Ok _ ->
            return Ok ()
        | Error err ->
            return Error err
    }


[<RequireQualifiedAccess>]
type private LastKnownEcosystemTransaction<'Op when 'Op : comparison> =
| Preparing                  of SubjectTransactionId
| Prepared                   of SubjectTransaction<'Op>
| RollingBackOrNeverPrepared of SubjectTransactionId

/// Encapsulates complexity of distributed transaction with some outer transaction (e.g. plain SQL) and
/// inner ecosystem transaction.
/// Provides best-effort commit or rollback orchestration when outer transaction completes,
/// however it's up to the outer client to keep track of possible orhan ecosystem transactions
/// and finalize them accordingly.
/// This provides strong (eventual) consistency guarantee but can limit availability: in case of orphan
/// ecosystem txn its prepared subjects remain locked until rolled back or committed.
type OuterTxnEcosystemClient<'Op when 'Op : comparison>
    (
        grainConnector: GrainConnector,
        outerTxnManager: IOuterTxnManager,
        txnLifeCycleDef: SubjectTransactionLifeCycleDef<'Op>,
        ecosystemName: string
    ) as this =
    let mutable maybeLastKnownEcosystemTxn: Option<LastKnownEcosystemTransaction<'Op>> = None
    do
        outerTxnManager.SetOuterTxnResetHandler this.OnTransactionReset

    let mapAwaitPrepareResult awaitResult (SubjectTransactionId transactionIdGuid) : Task<Result<SubjectTransaction<_>, _>> =
        backgroundTask {
            match awaitResult with
            | Ok (LifeEventTriggered (transaction, SubjectTransactionLifeEvent.OnPreparedOrFailed (* prepared *) true)) ->
                // transaction prepare fine - Ok
                return transaction.Subject |> Ok

            | Ok (LifeEventTriggered (transaction, SubjectTransactionLifeEvent.OnPreparedOrFailed (* prepared *) false)) ->
                let rollbackReasonToError =
                    function
                    | TransactionRollbackReason.Timeout ->
                        "transaction timed out"
                    | TransactionRollbackReason.Failure err ->
                        err.Value
                    | TransactionRollbackReason.Request ->
                        "transaction rolled back by request"
                    | TransactionRollbackReason.OrphanedBeforePrepare ->
                        "transaction classified as orphan by client before it prepared"

                let error =
                    match transaction.Subject.State with
                    | SubjectTransactionState.Running (_, maybePendingRollbackReason) ->
                        maybePendingRollbackReason
                        |> Option.map rollbackReasonToError
                        |> Option.get
                    | SubjectTransactionState.Finalizing (outcome, _)
                    | SubjectTransactionState.Finalized (outcome, _, _, _) ->
                        match outcome with
                        | TransactionOutcome.RolledBack rollbackReason ->
                            rollbackReasonToError rollbackReason
                        | TransactionOutcome.Committed ->
                            failwith "unexpected committed"
                    | SubjectTransactionState.Prepared _ ->
                        failwith "unexpected prepared"

                // transaction tried to prepare but failed - domain error
                return Error {| Message = error; RollingBackOrNeverPrepared = true |}

             | Ok (LifeEventTriggered (_, lifeEvent)) ->
                 // unexpected event detected, escalate to exception
                 return
                     sprintf "Ecosystem Transaction %s received unexpected life event: %A" (transactionIdGuid.ToTinyUuid()) lifeEvent
                     |> InvalidOperationException
                     |> raise

             | Error err ->
                 // also a legit Error
                 let message = sprintf "Can't prepare Ecosystem Transaction %s: %A" (transactionIdGuid.ToTinyUuid()) err
                 return Error {| Message = message; RollingBackOrNeverPrepared = false |}

             | Ok (WaitOnLifeEventTimedOut _) ->
                 // transient timeouts escalate to exceptions
                 return
                     sprintf "Ecosystem Transaction %s timed out" (transactionIdGuid.ToTinyUuid())
                     |> InvalidOperationException
                     |> raise
        }

    let prepareNewTransaction transactionIdGuid ops prepareTimeout =
        backgroundTask {
            // some ridiculous timeout to keep ecosystem transaction alive, we'd rather have locked subjects then inconsistent state
            // it is outer client responsibility to check and heal any failed or inconclusive transactions

            let lifeTimeout = TimeSpan.FromDays 30.;
            let ctor = SubjectTransactionConstructor.New (SubjectTransactionId transactionIdGuid, ops, Some lifeTimeout)

            match! grainConnector.GenerateId txnLifeCycleDef ctor with
            | Ok transactionId ->
                let! initAwaitResult =
                    grainConnector.ConstructAndWait
                        txnLifeCycleDef transactionId ctor
                        (SubjectTransactionLifeEvent.OnPreparedOrFailed true)
                        prepareTimeout
                return! mapAwaitPrepareResult initAwaitResult transactionId

            | Error err ->
                return sprintf "Can't create Ecosystem transaction Id: %A" err |> InvalidOperationException |> raise
        }

    let continueTransaction (transaction: SubjectTransaction<'Op>) ops prepareTimeout =
        backgroundTask {
            let action = SubjectTransactionAction.Continue ops
            // TODO: session?
            let! actAwaitResult =
                grainConnector.ActAndWait
                    txnLifeCycleDef (transaction.TransactionId :> SubjectId).IdString action
                    (SubjectTransactionLifeEvent.OnPreparedOrFailed true)
                    prepareTimeout
            return! mapAwaitPrepareResult actAwaitResult transaction.TransactionId
        }

    member this.GetEcosystemTransactionId() : SubjectTransactionId =
        match maybeLastKnownEcosystemTxn with
        | Some (LastKnownEcosystemTransaction.Prepared { TransactionId = transactionId }) ->
            transactionId
        | _ ->
            InvalidOperationException("Can't get ecosystem transaction Id before it's prepared") |> raise

    member this.PrepareAsync(ops: NonemptySet<'Op>) : Task<Result<unit, string>> =
        if not <| outerTxnManager.InTransaction then
            InvalidOperationException "Can't prepare Ecosystem transaction outside outer transaction scope" |> raise
        else
            // don't wait for full outer transaction timeout while preparing. Even if it helps ecosystem to prepare
            // the outer transaction is likely to timeout anyway
            let prepareTimeout = max (outerTxnManager.Timeout.TotalSeconds / 1.5 |> TimeSpan.FromSeconds) (TimeSpan.FromSeconds 2.)

            backgroundTask {
                match maybeLastKnownEcosystemTxn with
                | None ->
                    // create and enlist transaction
                    let transactionIdGuid = Guid.NewGuid()
                    maybeLastKnownEcosystemTxn <- LastKnownEcosystemTransaction.Preparing (SubjectTransactionId transactionIdGuid) |> Some
                    do! outerTxnManager.EnlistEcosystemTransaction ecosystemName transactionIdGuid

                    match! prepareNewTransaction transactionIdGuid ops prepareTimeout with
                    | Ok ecosystemTransaction ->
                        do! outerTxnManager.NotifyEcosystemTransactionPrepared ecosystemName transactionIdGuid
                        maybeLastKnownEcosystemTxn <- ecosystemTransaction |> LastKnownEcosystemTransaction.Prepared |> Some
                        return Ok ()

                    | Error err ->
                        maybeLastKnownEcosystemTxn <- LastKnownEcosystemTransaction.RollingBackOrNeverPrepared (SubjectTransactionId transactionIdGuid) |> Some
                        return Error err.Message

                | Some (LastKnownEcosystemTransaction.Prepared ({ TransactionId = (SubjectTransactionId transactionIdGuid) } as ecosystemTransaction)) ->
                    // continue existing transaction
                    maybeLastKnownEcosystemTxn <- LastKnownEcosystemTransaction.Preparing (SubjectTransactionId transactionIdGuid) |> Some
                    do! outerTxnManager.NotifyEcosystemTransactionContinuing ecosystemName transactionIdGuid

                    match! continueTransaction ecosystemTransaction ops prepareTimeout with
                    | Ok ecosystemTransaction ->
                        do! outerTxnManager.NotifyEcosystemTransactionPrepared ecosystemName transactionIdGuid
                        maybeLastKnownEcosystemTxn <- ecosystemTransaction |> LastKnownEcosystemTransaction.Prepared |> Some
                        return Ok ()

                    | Error err ->
                        maybeLastKnownEcosystemTxn <-
                            if err.RollingBackOrNeverPrepared then
                                // ecosystem agreed to continue but failed further, now rolling back automatically
                                LastKnownEcosystemTransaction.RollingBackOrNeverPrepared (SubjectTransactionId transactionIdGuid)
                            else
                                // ecosystem did not agree to continue, keep old prepared state
                                LastKnownEcosystemTransaction.Prepared ecosystemTransaction
                            |> Some
                        return Error err.Message

                | Some _ ->
                    return InvalidOperationException("Can't continue ecosystem transaction that is not prepared") |> raise
            }

    member private this.OnTransactionReset (outcome: OuterTxnOutcome) : Task =

        let prevMaybeLastKnownEcosystemTxn = maybeLastKnownEcosystemTxn
        maybeLastKnownEcosystemTxn <- None

        match prevMaybeLastKnownEcosystemTxn, outcome with
        | None, _ ->
            // was never used within outer scope
            Task.CompletedTask

        | Some (LastKnownEcosystemTransaction.RollingBackOrNeverPrepared _), OuterTxnOutcome.Committed ->
            InvalidOperationException "PANIC: Outside scope committed when ecosystem failed to prepare (certain failure). Must be a bug" |> raise

        | Some (LastKnownEcosystemTransaction.RollingBackOrNeverPrepared (SubjectTransactionId transactionIdGuid)), OuterTxnOutcome.Aborted
        | Some (LastKnownEcosystemTransaction.RollingBackOrNeverPrepared (SubjectTransactionId transactionIdGuid)), OuterTxnOutcome.Inconclusive ->
            // ecosystem was never prepared or already rolls back automatically, just tell outer scope to forget it
            outerTxnManager.ForgetEcosystemTransaction ecosystemName transactionIdGuid

        | Some (LastKnownEcosystemTransaction.Preparing _), OuterTxnOutcome.Committed ->
            InvalidOperationException "PANIC: Outside scope committed when ecosystem failed transiently. Must be a bug" |> raise

        | Some (LastKnownEcosystemTransaction.Preparing (SubjectTransactionId transactionIdGuid)), OuterTxnOutcome.Aborted
        | Some (LastKnownEcosystemTransaction.Preparing (SubjectTransactionId transactionIdGuid)), OuterTxnOutcome.Inconclusive
        | Some (LastKnownEcosystemTransaction.Prepared { TransactionId = (SubjectTransactionId transactionIdGuid) }), OuterTxnOutcome.Aborted ->
            backgroundTask {
                // Ecosystem is either 100% prepared or not 100% failed to prepare (think transient error), so tell to rollback first
                let action = SubjectTransactionAction.Rollback (* timedOut *) false
                match! grainConnector.Act txnLifeCycleDef ((SubjectTransactionId transactionIdGuid) :> SubjectId).IdString action with
                | Ok _ ->
                    // and tell outer scope to forget transaction token if rollback was successful
                    do! outerTxnManager.ForgetEcosystemTransaction ecosystemName transactionIdGuid
                | Error _ ->
                    // otherwise keep the txn token around so it can be picked up by orphan detection job
                    ()
            }

        | Some (LastKnownEcosystemTransaction.Prepared { TransactionId = (SubjectTransactionId transactionIdGuid) }), OuterTxnOutcome.Committed ->
            backgroundTask {
                // ecosystem is 100% prepared and so is outer scope. try to commit it now
                let action = SubjectTransactionAction.Commit
                match! grainConnector.Act txnLifeCycleDef ((SubjectTransactionId transactionIdGuid) :> SubjectId).IdString action with
                | Ok _ ->
                    // and tell outer scope to forget transaction token if commit was successful
                    do! outerTxnManager.ForgetEcosystemTransaction ecosystemName transactionIdGuid
                | Error _ ->
                    // otherwise keep the txn token around so it can be picked up by orphan detection job
                    ()
            }

        | Some (LastKnownEcosystemTransaction.Prepared _), OuterTxnOutcome.Inconclusive ->
            // orphaned prepared ecosystem transactions are handled in background job in outer consumer
            Task.CompletedTask
