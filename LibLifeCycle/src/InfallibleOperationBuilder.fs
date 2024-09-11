namespace LibLifeCycle

open System.Threading.Tasks
open LibLifeCycleTypes.File

[<AutoOpen>]
module TransitionWorkflow =

    type TODO = TODO // Placeholder to "return TODO" that will throw an exception

    /// Special predefined transition results
    [<RequireQualifiedAccess>]
    type Transition =
    /// Short-circuits transition to a no-op. Unlike `return subject` with unchanged subject, it's a true no-op
    /// that doesn't write subject or side effects to storage and keeps old LastUpdated timestamp.
    /// Will fail with a domain error if attempted after some side-effects were yielded in TransitionBuilder.
    | Ignore
    | NotAllowed // for standard reflection of action not allowed for a given subject state
    // TODO: add more transition short-circuits:
    // | DeleteSelf - to supersede hacky TimerAction.DeleteSelf

    // BLOB actions (see also Bind extensions below)
    module Blob =
        type CreateAction =
            /// Request to create a blob owned by current subject.
            /// Note that blob is not created immediately but at the end of transition.
            Create of Context: Service<ActionContext> * Service<Unique> * MimeType: Option<MimeType> * FileData

        type MaybeCreateAction =
            /// Request to create a blob owned by current subject, if source data is provided.
            /// Note that blob is not created immediately but at the end of transition.
            MaybeCreate of Context: Service<ActionContext> * Service<Unique> * MaybeBlobSource: Option<Option<MimeType> * FileData>

        type MaybeCreateOrReplaceAction =
            /// Request to create or replace a blob, if MaybeNewData is Some
            MaybeCreateOrReplace of Context: Service<ActionContext> * Service<Unique> * MaybeOriginalBlobId: Option<BlobId> * MaybeNewData: Option<Option<MimeType> * FileData>

        type AppendAction =
            /// Request to append data to a blob. The blob must be owned by current subject.
            /// Note that data is not appended to blob immediately but at the end of transition.
            /// Append is not idempotent. Action which appends to a blob (or clients that invoke the action) should
            /// implement its own deduplication mechanism.
            Append of Context: Service<ActionContext> * BlobId * byte[]

        type DeleteAction =
            /// Request to delete blob. The blob must be owned by current subject.
            /// Note that blob is not deleted immediately but at the end of transition.
            Delete of Context: Service<ActionContext> * BlobId

        type MaybeDeleteAction =
            /// Request to delete blob if the id is Some. The blob must be owned by current subject.
            /// Note that blob is not deleted immediately but at the end of transition.
            MaybeDelete of Context: Service<ActionContext> * Option<BlobId>

    module Blobs =
        type CreateAction =
            /// Request to create blobs owned by current subject.
            /// Note that blobs are not created immediately but at the end of transition.
            Create of Context: Service<ActionContext> * Service<Unique> * BlobSources: list<Option<MimeType> * FileData>

        type DeleteAction =
            /// Request to delete blobs. The blobs must be owned by current subject.
            /// Note that blobs are not deleted immediately but at the end of transition.
            Delete of Context: Service<ActionContext> * list<BlobId>

[<AutoOpen>]
module InfallibleOperationBuilder =

    let internal noInfallibleOperationSideEffects : InfallibleOperationSideEffects<_, _> = {
        ExternalActions = []
        LifeEvents      = []
        LifeActions     = []
    }

    type InfallibleOperationBuilder() =
        member _.Bind(InfallibleOperationResult res: InfallibleOperationResult<'Subject1, 'LifeAction, 'LifeEvent>, binder: 'Subject1 -> InfallibleOperationResult<'Subject2, 'LifeAction, 'LifeEvent>) : InfallibleOperationResult<'Subject2, 'LifeAction, 'LifeEvent> =
            backgroundTask {
                let! subj, blobActions1, sideEffects1 = res

                let (InfallibleOperationResult binderTask) = binder subj
                let! (subj2, blobActions2, sideEffects2) = binderTask
                return subj2, blobActions1 @ blobActions2, sideEffects1 + sideEffects2
            }
            |> InfallibleOperationResult

        member _.Bind(asyncTasks: Map<'Key, Task<'Value>>, binder: Map<'Key, 'Value> -> InfallibleOperationResult<'Subject1, 'LifeAction, 'LifeEvent>) : InfallibleOperationResult<'Subject1, 'LifeAction, 'LifeEvent> =
            backgroundTask {
                let! all =
                    asyncTasks
                    |> Map.toSeq
                    |> Seq.map (
                        fun (key, value) ->
                            backgroundTask {
                                let! res = value
                                return (key, res)
                            }
                        )
                    |> Task.WhenAll

                let (InfallibleOperationResult binderTask) =
                    Map.ofArray all
                    |> binder

                return! binderTask
            }
            |> InfallibleOperationResult

        member _.Return(value: 'Subject when 'Subject :> Subject<'SubjectId>): InfallibleOperationResult<'Subject, 'LifeAction, 'LifeEvent> =
            (value, noBlobActions, noInfallibleOperationSideEffects)
            |> Task.FromResult
            |> InfallibleOperationResult

        [<CompilerMessage("QQQ Not implemented yet", 10666, IsError=false)>]
        member _.Return(_todo: TODO): InfallibleOperationResult<'Subject, 'LifeAction, 'LifeEvent> =
            raise (System.NotImplementedException "QQQ Not implemented yet")

        member _.ReturnFrom (res: InfallibleOperationResult<'Subject, 'LifeAction, 'LifeEvent>) : InfallibleOperationResult<'Subject, 'LifeAction, 'LifeEvent> =
            res

        member this.TryFinally(body: unit -> InfallibleOperationResult<'Subject, 'LifeAction, 'LifeEvent>, compensation: unit -> unit) : InfallibleOperationResult<'Subject, 'LifeAction ,'LifeEvent> =
            try
                this.ReturnFrom(body())
            finally
                compensation()

        member _.Yield<'LifeEvent, 'LifeAction
                        when 'LifeAction  :> LifeAction
                        and  'LifeEvent   :> LifeEvent>
                (externalAction: IExternalLifeCycleOperation) : InfallibleOperationSideEffects<'LifeEvent, 'LifeAction> =
            { noInfallibleOperationSideEffects with ExternalActions = [ExternalOperation.ExternalLifeCycleOperation externalAction] }

        member _.YieldFrom<'LifeEvent, 'LifeAction
                        when 'LifeAction  :> LifeAction
                        and  'LifeEvent   :> LifeEvent> (sideEffects: seq<IExternalLifeCycleOperation>) : InfallibleOperationSideEffects<'LifeEvent, 'LifeAction> =
            { noInfallibleOperationSideEffects with ExternalActions = sideEffects |> Seq.map (ExternalOperation.ExternalLifeCycleOperation) |> Seq.toList }

        member _.Yield<'LifeEvent, 'LifeAction
                        when 'LifeAction  :> LifeAction
                        and  'LifeEvent   :> LifeEvent>
                (ingestOperation: IIngestTimeSeriesDataPointsOperation) : InfallibleOperationSideEffects<'LifeEvent, 'LifeAction> =
            { noInfallibleOperationSideEffects with ExternalActions = [ExternalOperation.ExternalTimeSeriesOperation ingestOperation] }

        member _.Yield<'LifeEvent, 'LifeAction
                        when 'LifeAction  :> LifeAction
                        and  'LifeEvent   :> LifeEvent>
                (externalAction: ExternalOperation<'LifeAction>) : InfallibleOperationSideEffects<'LifeEvent, 'LifeAction> =
            { noInfallibleOperationSideEffects with ExternalActions = [externalAction] }

        member _.YieldFrom<'LifeEvent, 'LifeAction
                        when 'LifeAction  :> LifeAction
                        and  'LifeEvent   :> LifeEvent> (sideEffects: seq<ExternalOperation<'LifeAction>>) : InfallibleOperationSideEffects<'LifeEvent, 'LifeAction> =
            { noInfallibleOperationSideEffects with ExternalActions = sideEffects |> Seq.toList }

        member _.Yield<'LifeEvent, 'LifeAction
                            when 'LifeAction  :> LifeAction
                            and  'LifeEvent   :> LifeEvent> (lifeEvent: 'LifeEvent when 'LifeEvent :> LifeEvent) : InfallibleOperationSideEffects<'LifeEvent, 'LifeAction> =
            { noInfallibleOperationSideEffects with LifeEvents = [lifeEvent] }

        member _.Yield<'LifeEvent, 'LifeAction
                            when 'LifeAction  :> LifeAction
                            and  'LifeEvent   :> LifeEvent> (lifeAction: 'LifeAction when 'LifeAction :> LifeAction) : InfallibleOperationSideEffects<'LifeEvent, 'LifeAction> =
            { noInfallibleOperationSideEffects with LifeActions = [lifeAction] }

        member _.Yield<'LifeEvent, 'LifeAction
                            when 'LifeAction  :> LifeAction
                            and  'LifeEvent   :> LifeEvent> (maybeLifeAction: Option<'LifeAction> when 'LifeAction :> LifeAction) : InfallibleOperationSideEffects<'LifeEvent, 'LifeAction> =
            { noInfallibleOperationSideEffects with LifeActions = maybeLifeAction |> Option.toList }

        member _.YieldFrom<'LifeEvent, 'LifeAction
                            when 'LifeAction  :> LifeAction
                            and  'LifeEvent   :> LifeEvent> (maybeLifeEvent: Option<'LifeEvent> when 'LifeEvent :> LifeEvent) : InfallibleOperationSideEffects<'LifeEvent, 'LifeAction> =
            { noInfallibleOperationSideEffects with LifeEvents = maybeLifeEvent |> Option.toList }

        member _.YieldFrom<'LifeEvent, 'LifeAction
                            when 'LifeAction  :> LifeAction
                            and  'LifeEvent   :> LifeEvent> (lifeEvents: seq<'LifeEvent>) : InfallibleOperationSideEffects<'LifeEvent, 'LifeAction> =
            { noInfallibleOperationSideEffects with LifeEvents = lifeEvents |> Seq.toList }

        member _.YieldFrom<'LifeEvent, 'LifeAction
                            when 'LifeAction  :> LifeAction
                            and  'LifeEvent   :> LifeEvent> (lifeActions: seq<'LifeAction>) : InfallibleOperationSideEffects<'LifeEvent, 'LifeAction> =
            { noInfallibleOperationSideEffects with LifeActions = Seq.toList lifeActions }

        member _.Combine (sideEffects: InfallibleOperationSideEffects<'LifeEvent, 'LifeAction>, res: unit -> InfallibleOperationResult<'Subject, 'LifeAction, 'LifeEvent>) : InfallibleOperationResult<'Subject, 'LifeAction, 'LifeEvent> =
            backgroundTask {
                let (InfallibleOperationResult resTask) = res()
                let! subj, blobActions, okSideEffects = resTask
                return subj, blobActions, (sideEffects + okSideEffects)
            }
            |> InfallibleOperationResult

        member _.Combine (lifeEvents: seq<'LifeEvent>, res: unit -> InfallibleOperationResult<'Subject, 'LifeAction, 'LifeEvent>) : InfallibleOperationResult<'Subject, 'LifeAction, 'LifeEvent> =
            backgroundTask {
                let (InfallibleOperationResult resTask) = res()
                let! subj, blobActions, okSideEffects = resTask
                return subj, blobActions, { okSideEffects with LifeEvents = okSideEffects.LifeEvents @ (lifeEvents |> Seq.toList) }
            }
            |> InfallibleOperationResult

        member _.Delay (value: unit -> InfallibleOperationResult<'Subject, 'LifeAction, 'LifeEvent>) : unit -> InfallibleOperationResult<'Subject, 'LifeAction, 'LifeEvent> =
            value

        member _.Run (value: unit -> InfallibleOperationResult<'Subject, 'LifeAction,'LifeEvent>) : InfallibleOperationResult<'Subject, 'LifeAction, 'LifeEvent> =
            value()

        member _.Zero<'LifeEvent, 'LifeAction
                       when 'LifeAction  :> LifeAction
                       and  'LifeEvent   :> LifeEvent> () : InfallibleOperationSideEffects<'LifeEvent, 'LifeAction> =
            noInfallibleOperationSideEffects

        member this.Using<'T, 'LifeEvent, 'LifeAction, 'U
                           when 'T :> System.IDisposable
                            and  'LifeAction  :> LifeAction
                            and  'LifeEvent   :> LifeEvent>
                (disposable: 'T, body: ('T -> InfallibleOperationResult<'U, 'LifeAction, 'LifeEvent>))
                : InfallibleOperationResult<'U, 'LifeAction, 'LifeEvent> =
            let body' = fun () -> body disposable
            this.TryFinally(body', fun () -> disposable.Dispose())

        member inline _.While (guard: unit -> bool, body: unit -> 'T) : 'U =
            let rec loop guard body =
                if guard () then
                    body () + loop guard body
                else
                    LanguagePrimitives.GenericZero
            loop guard body

        member inline this.For (p: seq<'T>, rest: 'T -> _) : 'U =
            using (p.GetEnumerator ()) (fun enum ->
                this.While (enum.MoveNext, fun () -> rest enum.Current))

    let infallibleOperation = InfallibleOperationBuilder()

[<AutoOpen>]
module InfallibleOperationBuilderExtensions =
    let internal assertBlobOwner (blobId: BlobId) (subjectRef: LocalSubjectPKeyReference) =
        if subjectRef <> blobId.Owner then
            // it is fine to fail with panic because it must never be attempted.
            // Even if somewhere it's a possible scenario then life cycle must check the owner and return a custom error if they don't match
            failwithf "Can't modify blob that belongs to a different subject. Caller: %A; Owner: %A" subjectRef blobId.Owner

    let internal createBlobId (context: Service<ActionContext>) (unique: Service<Unique>) : Task<BlobId> =
        backgroundTask {
            let! subjectRef = context.Query CurrentSubjectRef
            let! guid = unique.Query NewUuid
            return
                { Id_ = guid
                  Revision_ = 0u
                  Owner_ = subjectRef }
        }

    type InfallibleOperationBuilder with
        member _.ReturnFrom(result: 'Subject): InfallibleOperationResult<'Subject, 'LifeAction, 'LifeEvent> =
            (result, noBlobActions, noInfallibleOperationSideEffects)
            |> Task.FromResult
            |> InfallibleOperationResult

        member _.Bind(asyncTask: Task<'Subject1>, binder: 'Subject1 -> InfallibleOperationResult<'Subject2, 'LifeAction, 'LifeEvent>) : InfallibleOperationResult<'Subject2, 'LifeAction, 'LifeEvent> =
            backgroundTask {
                let! resolved = asyncTask
                let (InfallibleOperationResult binderTask) = binder resolved
                return! binderTask
            }
            |> InfallibleOperationResult

        member _.ReturnFrom (x: Task<'Subject>) : InfallibleOperationResult<'Subject, _, _> =
            backgroundTask {
                let! resolved = x
                return resolved, noBlobActions, noInfallibleOperationSideEffects
            }
            |> InfallibleOperationResult

        member _.ReturnFrom (x: Async<'Subject>) : InfallibleOperationResult<'Subject, _, _> =
            backgroundTask {
                let! resolved = x
                return resolved, noBlobActions, noInfallibleOperationSideEffects
            }
            |> InfallibleOperationResult

        member _.Return(value: 'Subject): InfallibleOperationResult<'Subject, 'LifeAction, 'LifeEvent> =
            (value, noBlobActions, noInfallibleOperationSideEffects)
            |> Task.FromResult
            |> InfallibleOperationResult

        member _.Bind (Blob.Create (context, unique, mimeType, bytes), binder: BlobId -> InfallibleOperationResult<'Subject, 'LifeAction, 'LifeEvent>) : InfallibleOperationResult<'Subject, 'LifeAction, 'LifeEvent> =
            backgroundTask {
                let! blobId = createBlobId context unique
                let (InfallibleOperationResult binderTask) = binder blobId
                let! subject, blobActions, sideEffects = binderTask
                return subject, blobActions @ [ BlobAction.Create (blobId, mimeType, bytes) ], sideEffects
            }
            |> InfallibleOperationResult

        member this.Bind (action: Blob.MaybeCreateAction, binder: Option<BlobId> -> InfallibleOperationResult<'Subject, 'LifeAction, 'LifeEvent>) : InfallibleOperationResult<'Subject, 'LifeAction, 'LifeEvent> =
            match action with
            | Blob.MaybeCreate (context, unique, Some (maybeMimeType, bytes)) ->
                this.Bind (Blob.Create (context, unique, maybeMimeType, bytes), Some >> binder)
            | Blob.MaybeCreate (_, _, None) ->
                binder None

        member _.Bind (Blobs.Create (context, unique, sources), binder: List<BlobId> -> InfallibleOperationResult<'Subject, 'LifeAction, 'LifeEvent>) : InfallibleOperationResult<'Subject, 'LifeAction, 'LifeEvent> =
            let tasks =
                sources
                |> Seq.map (fun (maybeMimeType, bytes) -> backgroundTask {
                    let! blobId = createBlobId context unique
                    return (blobId, BlobAction.Create (blobId, maybeMimeType, bytes))
                })

            backgroundTask {
                let! results = Task.WhenAll (tasks |> Array.ofSeq)
                let (blobIds, actions) =
                    Array.foldBack
                        (fun (blobId, action) (blobIds, actions) ->
                            (blobId :: blobIds, action :: actions)
                        )
                        results
                        ([], [])

                let (InfallibleOperationResult binderTask) = binder blobIds
                let! subject, blobActions, sideEffects = binderTask
                return subject, blobActions @ actions, sideEffects
            }
            |> InfallibleOperationResult