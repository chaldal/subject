[<AutoOpen>]
module LibLifeCycle.LifeCycles.Transaction.Extensions

open LibLifeCycle
open LibLifeCycle.LifeCycles.Transaction

type LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env
                when 'Subject              :> Subject<'SubjectId>
                and  'LifeAction           :> LifeAction
                and  'OpError              :> OpError
                and  'Constructor          :> Constructor
                and  'LifeEvent            :> LifeEvent
                and  'LifeEvent            :  comparison
                and  'SubjectIndex         :> SubjectIndex<'OpError>
                and  'SubjectId            :> SubjectId
                and  'AccessPredicateInput :> AccessPredicateInput
                and  'Role                 :  comparison
                and  'Env                  :> Env>
with
        member this.ActInTransaction<'Op when 'Op : comparison>
            (id: 'SubjectId)
            (action: 'LifeAction)
            (transactionId: SubjectTransactionId)
            (step: SubjectTransactionStep)
            ((batchNo, opNo): TxnBatchOpNo)
            : ExternalOperation<SubjectTransactionAction<'Op>> =

            match step with
            | SubjectTransactionStep.Prepare ->
                createLifeCycleTxnOp this.Definition (LifeCycleTxnOp.PrepareAct (id, action, transactionId))

            | SubjectTransactionStep.Commit ->
                createLifeCycleTxnOp this.Definition (LifeCycleTxnOp.Commit (id, transactionId))

            | SubjectTransactionStep.Rollback ->
                createLifeCycleTxnOp this.Definition (LifeCycleTxnOp.Rollback (id, transactionId))

            | SubjectTransactionStep.CheckPhantom ->
                createLifeCycleTxnOp this.Definition (LifeCycleTxnOp.CheckPhantom (id, transactionId))

            |> fun externalOperation ->
                ExternalOperation.ExternalSubjectTransactionOperation (externalOperation, batchNo, opNo)

        member this.ConstructInTransaction<'Op when 'Op : comparison>
            (id: 'SubjectId)
            (constructor: 'Constructor)
            (transactionId: SubjectTransactionId)
            (step: SubjectTransactionStep)
            ((batchNo, opNo): TxnBatchOpNo)
            : ExternalOperation<SubjectTransactionAction<'Op>> =

            match step with
            | SubjectTransactionStep.Prepare ->
                createLifeCycleTxnOp this.Definition (LifeCycleTxnOp.PrepareInitialize (id, constructor, transactionId))

            | SubjectTransactionStep.Commit ->
                createLifeCycleTxnOp this.Definition (LifeCycleTxnOp.Commit (id, transactionId))

            | SubjectTransactionStep.Rollback ->
                createLifeCycleTxnOp this.Definition (LifeCycleTxnOp.Rollback (id, transactionId))

            | SubjectTransactionStep.CheckPhantom ->
                createLifeCycleTxnOp this.Definition (LifeCycleTxnOp.CheckPhantom (id, transactionId))

            |> fun externalOperation ->
                ExternalOperation.ExternalSubjectTransactionOperation (externalOperation, batchNo, opNo)