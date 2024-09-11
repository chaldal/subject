namespace LibLifeCycle

open System.Threading.Tasks
open LibLifeCycleTypes.File

[<AutoOpen>]
module OperationBuilder =

    let internal noOperationSideEffects : OperationSideEffects<_, _> = {
        ExternalActions = []
        LifeEvents      = []
        LifeActions     = []
    }


    type OperationBuilder() =
        member _.Bind(OperationResult res: OperationResult<'Subject1, 'LifeAction, 'OpError, 'LifeEvent>, binder: 'Subject1 -> OperationResult<'Subject2, 'LifeAction, 'OpError, 'LifeEvent>) : OperationResult<'Subject2, 'LifeAction, 'OpError, 'LifeEvent> =
            backgroundTask {
                match! res with
                | Ok (subj, blobActions1, sideEffects1) ->
                    let (OperationResult binderTask) = binder subj
                    match! binderTask with
                    | Ok (subj2, blobActions2, sideEffects2) ->
                        return Ok(subj2, blobActions1 @ blobActions2, sideEffects1 + sideEffects2)
                    | Error err ->
                        return Error err
                | Error err ->
                    return Error err
            }
            |> OperationResult

        member _.Bind(asyncTasks: Map<'Key, Task<'Value>>, binder: Map<'Key, 'Value> -> OperationResult<'Subject1, 'LifeAction, 'OpError, 'LifeEvent>) : OperationResult<'Subject1, 'LifeAction, 'OpError, 'LifeEvent> =
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

                let (OperationResult binderTask) =
                    Map.ofArray all
                    |> binder

                return! binderTask
            }
            |> OperationResult

        member _.Return(value: 'Subject when 'Subject :> Subject<'SubjectId>): OperationResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent> =
            (value, noBlobActions, noOperationSideEffects)
            |> Ok
            |> Task.FromResult
            |> OperationResult

        member _.Return<'Subject, 'OpError, 'LifeEvent, 'LifeAction
                         when 'OpError     :> OpError
                         and  'LifeAction  :> LifeAction
                         and  'LifeEvent   :> LifeEvent> (error: 'OpError): OperationResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent> =
            error
            |> LifeCycleOperationError
            |> Error
            |> Task.FromResult
            |> OperationResult

        [<CompilerMessage("QQQ Not implemented yet", 10666, IsError=false)>]
        member _.Return(_todo: TODO): OperationResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent> =
            raise (System.NotImplementedException "QQQ Not implemented yet")

        member _.ReturnFrom (res: OperationResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent>) : OperationResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent> =
            res
            
        member _.ReturnFrom (res: InfallibleOperationResult<'Subject, 'LifeAction, 'LifeEvent>) : OperationResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent> =
            backgroundTask {
                let (InfallibleOperationResult binderTask) = res
                let! subj, blobActions1, sideEffects1 = binderTask
                return (subj, blobActions1, sideEffects1 + noOperationSideEffects) |> Ok
            }
            |> OperationResult

        member _.ReturnFrom(result: Result<'Subject, 'OpError>): OperationResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent> =
            match result with
            | Ok okVal ->
                Ok (okVal, noBlobActions, noOperationSideEffects)
            | Error err ->
                err |> LifeCycleOperationError |> Error
            |> Task.FromResult
            |> OperationResult

        member _.Yield<'LifeEvent, 'LifeAction
                        when 'LifeAction  :> LifeAction
                        and  'LifeEvent   :> LifeEvent>
                (externalAction: IExternalLifeCycleOperation) : OperationSideEffects<'LifeEvent, 'LifeAction> =
            { noOperationSideEffects with ExternalActions = [ExternalOperation.ExternalLifeCycleOperation externalAction] }

        member _.YieldFrom<'LifeEvent, 'LifeAction
                        when 'LifeAction  :> LifeAction
                        and  'LifeEvent   :> LifeEvent> (sideEffects: seq<IExternalLifeCycleOperation>) : OperationSideEffects<'LifeEvent, 'LifeAction> =
            { noOperationSideEffects with ExternalActions = sideEffects |> Seq.map (ExternalOperation.ExternalLifeCycleOperation) |> Seq.toList }

        member _.Yield<'LifeEvent, 'LifeAction
                        when 'LifeAction  :> LifeAction
                        and  'LifeEvent   :> LifeEvent>
                (ingestOperation: IIngestTimeSeriesDataPointsOperation) : OperationSideEffects<'LifeEvent, 'LifeAction> =
            { noOperationSideEffects with ExternalActions = [ExternalOperation.ExternalTimeSeriesOperation ingestOperation] }

        member _.Yield<'LifeEvent, 'LifeAction
                        when 'LifeAction  :> LifeAction
                        and  'LifeEvent   :> LifeEvent>
                (externalAction: ExternalOperation<'LifeAction>) : OperationSideEffects<'LifeEvent, 'LifeAction> =
            { noOperationSideEffects with ExternalActions = [externalAction] }

        member _.YieldFrom<'LifeEvent, 'LifeAction
                        when 'LifeAction  :> LifeAction
                        and  'LifeEvent   :> LifeEvent> (sideEffects: seq<ExternalOperation<'LifeAction>>) : OperationSideEffects<'LifeEvent, 'LifeAction> =
            { noOperationSideEffects with ExternalActions = sideEffects |> Seq.toList }

        member _.Yield<'LifeEvent, 'LifeAction
                            when 'LifeAction  :> LifeAction
                            and  'LifeEvent   :> LifeEvent> (lifeEvent: 'LifeEvent when 'LifeEvent :> LifeEvent) : OperationSideEffects<'LifeEvent, 'LifeAction> =
            { noOperationSideEffects with LifeEvents = [lifeEvent] }

        member _.Yield<'LifeEvent, 'LifeAction
                            when 'LifeAction  :> LifeAction
                            and  'LifeEvent   :> LifeEvent> (lifeAction: 'LifeAction when 'LifeAction :> LifeAction) : OperationSideEffects<'LifeEvent, 'LifeAction> =
            { noOperationSideEffects with LifeActions = [lifeAction] }

        member _.Yield<'LifeEvent, 'LifeAction
                            when 'LifeAction  :> LifeAction
                            and  'LifeEvent   :> LifeEvent> (maybeLifeAction: Option<'LifeAction> when 'LifeAction :> LifeAction) : OperationSideEffects<'LifeEvent, 'LifeAction> =
            { noOperationSideEffects with LifeActions = maybeLifeAction |> Option.toList }

        member _.YieldFrom<'LifeEvent, 'LifeAction
                            when 'LifeAction  :> LifeAction
                            and  'LifeEvent   :> LifeEvent> (maybeLifeEvent: Option<'LifeEvent> when 'LifeEvent :> LifeEvent) : OperationSideEffects<'LifeEvent, 'LifeAction> =
            { noOperationSideEffects with LifeEvents = maybeLifeEvent |> Option.toList }

        member _.YieldFrom<'LifeEvent, 'LifeAction
                            when 'LifeAction  :> LifeAction
                            and  'LifeEvent   :> LifeEvent> (lifeEvents: seq<'LifeEvent>) : OperationSideEffects<'LifeEvent, 'LifeAction> =
            { noOperationSideEffects with LifeEvents = lifeEvents |> Seq.toList }

        member _.YieldFrom<'LifeEvent, 'LifeAction
                            when 'LifeAction  :> LifeAction
                            and  'LifeEvent   :> LifeEvent> (lifeActions: seq<'LifeAction>) : OperationSideEffects<'LifeEvent, 'LifeAction> =
            { noOperationSideEffects with LifeActions = Seq.toList lifeActions }

        member _.Combine (sideEffects: OperationSideEffects<'LifeEvent, 'LifeAction>, res: unit -> OperationResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent>) : OperationResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent> =
            backgroundTask {
                let (OperationResult resTask) = res()
                match! resTask with
                | Ok (subj, blobActions, okSideEffects) ->
                    return Ok(subj, blobActions, (sideEffects + okSideEffects))
                | Error err ->
                    return Error err
            }
            |> OperationResult

        member _.Combine (lifeEvents: seq<'LifeEvent>, res: unit -> OperationResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent>) : OperationResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent> =
            backgroundTask {
                let (OperationResult resTask) = res()
                match! resTask with
                | Ok (subj, blobActions, okSideEffects) ->
                    return Ok(subj, blobActions, { okSideEffects with LifeEvents = okSideEffects.LifeEvents @ (lifeEvents |> Seq.toList) })
                | Error err ->
                    return Error err
            }
            |> OperationResult

        member _.Delay (value: unit -> OperationResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent>) : unit -> OperationResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent> =
            value

        member _.Run (value: unit -> OperationResult<'Subject, 'LifeAction,'OpError,'LifeEvent>) : OperationResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent> =
            value()

        member _.Zero<'LifeEvent, 'LifeAction
                       when 'LifeAction  :> LifeAction
                       and  'LifeEvent   :> LifeEvent> () : OperationSideEffects<'LifeEvent, 'LifeAction> =
            noOperationSideEffects

        member this.TryFinally(body: unit -> OperationResult<'Subject, 'LifeAction,'OpError,'LifeEvent>, compensation: (unit -> unit)) : OperationResult<'Subject, 'LifeAction,'OpError,'LifeEvent> =
            try
                this.ReturnFrom(body())
            finally
                compensation()

        member this.Using<'T, 'LifeEvent, 'LifeAction, 'OpError, 'U
                           when 'T :> System.IDisposable
                            and  'LifeAction  :> LifeAction
                            and  'LifeEvent   :> LifeEvent
                            and  'OpError     :> OpError>
                (disposable: 'T, body: ('T -> OperationResult<'U, 'LifeAction, 'OpError, 'LifeEvent>))
                : OperationResult<'U, 'LifeAction, 'OpError, 'LifeEvent> =
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

    let operation = OperationBuilder()

[<AutoOpen>]
module OperationBuilderExtensions =

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

    type OperationBuilder with
        member _.Bind(InfallibleOperationResult res: InfallibleOperationResult<'Subject1, 'LifeAction, 'LifeEvent>, binder: 'Subject1 -> OperationResult<'Subject2, 'LifeAction, 'OpError, 'LifeEvent>) : OperationResult<'Subject2, 'LifeAction, 'OpError, 'LifeEvent> =
            backgroundTask {
                let! subj, blobActions1, sideEffects1 = res
                let (OperationResult binderTask) = binder subj
                match! binderTask with
                | Ok (subj2, blobActions2, sideEffects2) ->
                    return (subj2, blobActions1 @ blobActions2, sideEffects1 + sideEffects2) |> Ok
                | Error err ->
                    return Error err
            }
            |> OperationResult
        
        member _.Bind(asyncTask: Task<'Subject1>, binder: 'Subject1 -> OperationResult<'Subject2, 'LifeAction, 'OpError, 'LifeEvent>) : OperationResult<'Subject2, 'LifeAction, 'OpError, 'LifeEvent> =
            backgroundTask {
                let! resolved = asyncTask
                let (OperationResult binderTask) = binder resolved
                return! binderTask
            }
            |> OperationResult

        member _.ReturnFrom (x: Task<'Subject>) : OperationResult<'Subject, _, _, _> =
            backgroundTask {
                let! resolved = x
                return Ok (resolved, noBlobActions, noOperationSideEffects)
            }
            |> OperationResult

        member _.ReturnFrom (x: Async<'Subject>) : OperationResult<'Subject, _, _, _> =
            backgroundTask {
                let! resolved = x
                return Ok (resolved, noBlobActions, noOperationSideEffects)
            }
            |> OperationResult

        member _.Return(value: 'Subject): OperationResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent> =
            Ok (value, noBlobActions, noOperationSideEffects)
            |> Task.FromResult
            |> OperationResult

        member _.Bind (Blob.Create (context, unique, mimeType, bytes), binder: BlobId -> OperationResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent>) : OperationResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent> =
            backgroundTask {
                let! blobId = createBlobId context unique
                let (OperationResult binderTask) = binder blobId
                match! binderTask with
                | Ok (subject, blobActions, sideEffects) ->
                    return Ok (subject, blobActions @ [ BlobAction.Create (blobId, mimeType, bytes) ], sideEffects)
                | Error err ->
                    return Error err
            }
            |> OperationResult

        member this.Bind (action: Blob.MaybeCreateAction, binder: Option<BlobId> -> OperationResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent>) : OperationResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent> =
            match action with
            | Blob.MaybeCreate (context, unique, Some (maybeMimeType, bytes)) ->
                this.Bind (Blob.Create (context, unique, maybeMimeType, bytes), Some >> binder)
            | Blob.MaybeCreate (_, _, None) ->
                binder None

        member _.Bind (Blobs.Create (context, unique, sources), binder: List<BlobId> -> OperationResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent>) : OperationResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent> =
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

                let (OperationResult binderTask) = binder blobIds
                match! binderTask with
                | Ok (subject, blobActions, sideEffects) ->
                    return Ok (subject, blobActions @ actions, sideEffects)
                | Error err ->
                    return Error err
            }
            |> OperationResult