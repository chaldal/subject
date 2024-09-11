namespace LibLifeCycle

open System.Threading.Tasks

[<AutoOpen>]
module ConstructionBuilder =

    let internal noConstructionSideEffects : ConstructionSideEffects<_, _> = {
        ExternalActions = []
        LifeEvents      = []
        LifeActions     = []
    }

    type ConstructionBuilder() =
        member _.Bind(ConstructionResult res: ConstructionResult<'Subject1, 'LifeAction, 'OpError, 'LifeEvent>, binder: 'Subject1 -> ConstructionResult<'Subject2, 'LifeAction, 'OpError, 'LifeEvent>) : ConstructionResult<'Subject2, 'LifeAction, 'OpError, 'LifeEvent> =
            backgroundTask {
                match! res with
                | Ok (subj, blobActions1, sideEffects1) ->
                    let (ConstructionResult binderTask) = binder subj
                    match! binderTask with
                    | Ok (subj2, blobActions2, sideEffects2) ->
                        return Ok(subj2, blobActions1 @ blobActions2, sideEffects1 + sideEffects2)
                    | Error err ->
                        return Error err
                | Error err ->
                    return Error err
            }
            |> ConstructionResult

        member _.Bind(asyncTasks: Map<'Key, Task<'Value>>, binder: Map<'Key, 'Value> -> ConstructionResult<'Subject1, 'LifeAction, 'OpError, 'LifeEvent>) : ConstructionResult<'Subject1, 'LifeAction, 'OpError, 'LifeEvent> =
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

                let (ConstructionResult binderTask) =
                    Map.ofArray all
                    |> binder
                return! binderTask
            }
            |> ConstructionResult

        member _.Return(value: 'Subject when 'Subject :> Subject<'SubjectId>): ConstructionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent> =
            Ok (value, noBlobActions, noConstructionSideEffects)
            |> Task.FromResult
            |> ConstructionResult

        member _.Return<'Subject, 'OpError, 'LifeEvent, 'Constructor, 'LifeAction
                         when 'OpError     :> OpError
                         and  'LifeAction  :> LifeAction
                         and  'Constructor :> Constructor
                         and  'LifeEvent   :> LifeEvent> (error: 'OpError): ConstructionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent> =
            error
            |> LifeCycleCtorError
            |> Error
            |> Task.FromResult
            |> ConstructionResult

        [<CompilerMessage("QQQ Not implemented yet", 10666, IsError=false)>]
        member _.Return(_todo: TODO): ConstructionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent> =
            raise (System.NotImplementedException "QQQ Not implemented yet")

        member _.ReturnFrom (res: ConstructionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent>) : ConstructionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent> =
            res

        member _.ReturnFrom (res: InfallibleOperationResult<'Subject, 'LifeAction, 'LifeEvent>) : ConstructionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent> =
            backgroundTask {
                let (InfallibleOperationResult binderTask) = res
                let! subj, blobActions1, sideEffects1 = binderTask
                return (subj, blobActions1, sideEffects1 + noConstructionSideEffects) |> Ok
            }
            |> ConstructionResult

        member _.ReturnFrom (res: OperationResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent>) : ConstructionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent> =
            backgroundTask {
                let (OperationResult binderTask) = res
                match! binderTask with
                | Ok (subj, blobActions1, sideEffects1) ->
                    return (subj, blobActions1, sideEffects1 + noConstructionSideEffects) |> Ok
                | Error (OperationBuilderError.LifeCycleOperationError err) ->
                    return Error (ConstructionBuilderError.LifeCycleCtorError err)
                | Error (OperationBuilderError.LifeCycleOperationException exn) ->
                    return Error (ConstructionBuilderError.LifeCycleCtorException exn)
            }
            |> ConstructionResult

        member _.ReturnFrom(result: Result<'Subject, 'OpError>): ConstructionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent> =
            match result with
            | Ok okVal ->
                Ok (okVal, noBlobActions, noConstructionSideEffects)
            | Error err ->
                err |> LifeCycleCtorError |> Error
            |> Task.FromResult
            |> ConstructionResult

        member _.Yield<'Constructor, 'LifeEvent, 'LifeAction
                        when 'Constructor :> Constructor
                        and  'LifeAction  :> LifeAction
                        and  'LifeEvent   :> LifeEvent>
                (externalAction: IExternalLifeCycleOperation) : ConstructionSideEffects<'LifeEvent, 'LifeAction> =
            { noConstructionSideEffects with ExternalActions = [ExternalOperation.ExternalLifeCycleOperation externalAction] }

        member _.YieldFrom<'Constructor, 'LifeEvent, 'LifeAction
                        when 'Constructor :> Constructor
                        and  'LifeAction  :> LifeAction
                        and  'LifeEvent   :> LifeEvent> (sideEffects: seq<IExternalLifeCycleOperation>) : ConstructionSideEffects<'LifeEvent, 'LifeAction> =
            { noConstructionSideEffects with ExternalActions = sideEffects |> Seq.map (ExternalOperation.ExternalLifeCycleOperation) |> Seq.toList }

        member _.Yield<'Constructor, 'LifeEvent, 'LifeAction
                        when 'Constructor :> Constructor
                        and  'LifeAction  :> LifeAction
                        and  'LifeEvent   :> LifeEvent>
                (ingestOperation: IIngestTimeSeriesDataPointsOperation) : ConstructionSideEffects<'LifeEvent, 'LifeAction> =
            { noConstructionSideEffects with ExternalActions = [ExternalOperation.ExternalTimeSeriesOperation ingestOperation] }

        member _.Yield<'Constructor, 'LifeEvent, 'LifeAction
                        when 'Constructor :> Constructor
                        and  'LifeAction  :> LifeAction
                        and  'LifeEvent   :> LifeEvent>
                (externalAction: ExternalOperation<'LifeAction>) : ConstructionSideEffects<'LifeEvent, 'LifeAction> =
            { noConstructionSideEffects with ExternalActions = [externalAction] }

        member _.YieldFrom<'Constructor, 'LifeEvent, 'LifeAction
                        when 'Constructor :> Constructor
                        and  'LifeAction  :> LifeAction
                        and  'LifeEvent   :> LifeEvent> (sideEffects: seq<ExternalOperation<'LifeAction>>) : ConstructionSideEffects<'LifeEvent, 'LifeAction> =
            { noConstructionSideEffects with ExternalActions = sideEffects |> Seq.toList }

        member _.Yield<'Constructor, 'LifeEvent, 'LifeAction
                            when 'Constructor :> Constructor
                            and  'LifeAction  :> LifeAction
                            and  'LifeEvent   :> LifeEvent> (lifeEvent: 'LifeEvent when 'LifeEvent :> LifeEvent) : ConstructionSideEffects<'LifeEvent, 'LifeAction> =
            { noConstructionSideEffects with LifeEvents = [lifeEvent] }

        member _.Yield<'Constructor, 'LifeEvent, 'LifeAction
                            when 'Constructor :> Constructor
                            and  'LifeAction  :> LifeAction
                            and  'LifeEvent   :> LifeEvent> (lifeAction: 'LifeAction when 'LifeAction :> LifeAction) : ConstructionSideEffects<'LifeEvent, 'LifeAction> =
            { noConstructionSideEffects with LifeActions = [lifeAction] }

        member _.Yield<'Constructor, 'LifeEvent, 'LifeAction
                            when 'Constructor :> Constructor
                            and  'LifeAction  :> LifeAction
                            and  'LifeEvent   :> LifeEvent> (maybeLifeAction: Option<'LifeAction> when 'LifeAction :> LifeAction) : ConstructionSideEffects<'LifeEvent, 'LifeAction> =
            { noConstructionSideEffects with LifeActions = maybeLifeAction |> Option.toList }

        member _.YieldFrom<'Constructor, 'LifeEvent, 'LifeAction
                            when 'Constructor :> Constructor
                            and  'LifeAction  :> LifeAction
                            and  'LifeEvent   :> LifeEvent> (maybeLifeEvent: Option<'LifeEvent> when 'LifeEvent :> LifeEvent) : ConstructionSideEffects<'LifeEvent, 'LifeAction> =
            { noConstructionSideEffects with LifeEvents = maybeLifeEvent |> Option.toList }

        member _.YieldFrom<'Constructor, 'LifeEvent, 'LifeAction
                            when 'Constructor :> Constructor
                            and  'LifeAction  :> LifeAction
                            and  'LifeEvent   :> LifeEvent> (lifeEvents: seq<'LifeEvent>) : ConstructionSideEffects<'LifeEvent, 'LifeAction> =
            { noConstructionSideEffects with LifeEvents = lifeEvents |> Seq.toList }

        member _.YieldFrom<'Constructor, 'LifeEvent, 'LifeAction
                            when 'Constructor :> Constructor
                            and  'LifeAction  :> LifeAction
                            and  'LifeEvent   :> LifeEvent> (lifeActions: seq<'LifeAction>) : ConstructionSideEffects<'LifeEvent, 'LifeAction> =
            { noConstructionSideEffects with LifeActions = Seq.toList lifeActions }

        member _.Combine (sideEffects: ConstructionSideEffects<'LifeEvent, 'LifeAction>, res: unit -> ConstructionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent>) : ConstructionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent> =
            backgroundTask {
                let (ConstructionResult resTask) = res()
                match! resTask with
                | Ok (subj, blobActions, okSideEffects) ->
                    return Ok(subj, blobActions, (sideEffects + okSideEffects))
                | Error err ->
                    return Error err
            }
            |> ConstructionResult

        member _.Combine (lifeEvents: seq<'LifeEvent>, res: unit -> ConstructionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent>) : ConstructionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent> =
            backgroundTask {
                let (ConstructionResult resTask) = res()
                match! resTask with
                | Ok (subj, blobActions, okSideEffects) ->
                    return Ok(subj, blobActions, { okSideEffects with LifeEvents = okSideEffects.LifeEvents @ (lifeEvents |> Seq.toList) })
                | Error err ->
                    return Error err
            }
            |> ConstructionResult

        member _.Delay (value: unit -> ConstructionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent>) : unit -> ConstructionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent> =
            value

        member _.Run (value: unit -> ConstructionResult<'Subject, 'LifeAction,'OpError,'LifeEvent>) : ConstructionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent> =
            value()

        member _.Zero<'Constructor, 'LifeEvent, 'LifeAction
                       when 'Constructor :> Constructor
                       and  'LifeAction  :> LifeAction
                       and  'LifeEvent   :> LifeEvent> () : ConstructionSideEffects<'LifeEvent, 'LifeAction> =
            noConstructionSideEffects

        member this.TryFinally(body: unit -> ConstructionResult<'Subject, 'LifeAction,'OpError,'LifeEvent>, compensation: (unit -> unit)) : ConstructionResult<'Subject, 'LifeAction,'OpError,'LifeEvent> =
            try
                this.ReturnFrom(body())
            finally
                compensation()

        member this.Using<'T, 'LifeEvent, 'LifeAction, 'OpError, 'U
                           when 'T :> System.IDisposable
                            and  'LifeAction  :> LifeAction
                            and  'LifeEvent   :> LifeEvent
                            and  'OpError     :> OpError>
                (disposable: 'T, body: ('T -> ConstructionResult<'U, 'LifeAction, 'OpError, 'LifeEvent>))
                : ConstructionResult<'U, 'LifeAction, 'OpError, 'LifeEvent> =
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

    let construction = ConstructionBuilder()

[<AutoOpen>]
module ConstructionBuilderExtensions =

    type ConstructionBuilder with
        member _.Bind(InfallibleOperationResult res: InfallibleOperationResult<'Subject1, 'LifeAction, 'LifeEvent>, binder: 'Subject1 -> ConstructionResult<'Subject2, 'LifeAction, 'OpError, 'LifeEvent>) : ConstructionResult<'Subject2, 'LifeAction, 'OpError, 'LifeEvent> =
            backgroundTask {
                let! subj, blobActions1, sideEffects1 = res
                let (ConstructionResult binderTask) = binder subj
                match! binderTask with
                | Ok (subj2, blobActions2, sideEffects2) ->
                    return (subj2, blobActions1 @ blobActions2, sideEffects1 + sideEffects2) |> Ok
                | Error err ->
                    return Error err
            }
            |> ConstructionResult
        
        member _.Bind(OperationResult res: OperationResult<'Subject1, 'LifeAction, 'OpError, 'LifeEvent>, binder: 'Subject1 -> ConstructionResult<'Subject2, 'LifeAction, 'OpError, 'LifeEvent>) : ConstructionResult<'Subject2, 'LifeAction, 'OpError, 'LifeEvent> =
            backgroundTask {
                match! res with
                | Ok (subj, blobActions1, sideEffects1) ->
                    let (ConstructionResult binderTask) = binder subj
                    match! binderTask with
                    | Ok (subj2, blobActions2, sideEffects2) ->
                        return (subj2, blobActions1 @ blobActions2, sideEffects1 + sideEffects2) |> Ok
                    | Error err ->
                        return Error err
                | Error (OperationBuilderError.LifeCycleOperationError err) ->
                    return Error (ConstructionBuilderError.LifeCycleCtorError err)
                | Error (OperationBuilderError.LifeCycleOperationException exn) ->
                    return Error (ConstructionBuilderError.LifeCycleCtorException exn)
            }
            |> ConstructionResult

        member _.Bind(asyncTask: Task<'Subject1>, binder: 'Subject1 -> ConstructionResult<'Subject2, 'LifeAction, 'OpError, 'LifeEvent>) : ConstructionResult<'Subject2, 'LifeAction, 'OpError, 'LifeEvent> =
            backgroundTask {
                let! resolved = asyncTask
                let (ConstructionResult binderTask) = binder resolved
                return! binderTask
            }
            |> ConstructionResult

        member _.ReturnFrom (x: Task<'Subject>) : ConstructionResult<'Subject, _, _, _> =
            backgroundTask {
                let! resolved = x
                return Ok (resolved, noBlobActions, noConstructionSideEffects)
            }
            |> ConstructionResult

        member _.ReturnFrom (x: Async<'Subject>) : ConstructionResult<'Subject, _, _, _> =
            backgroundTask {
                let! resolved = x
                return Ok (resolved, noBlobActions, noConstructionSideEffects)
            }
            |> ConstructionResult

        member _.Return(value: 'Subject): ConstructionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent> =
            Ok (value, noBlobActions, noConstructionSideEffects)
            |> Task.FromResult
            |> ConstructionResult

        member _.Bind (Blob.Create (context, unique, mimeType, bytes), binder: BlobId -> ConstructionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent>) : ConstructionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent> =
            backgroundTask {
                let! blobId = createBlobId context unique
                let (ConstructionResult binderTask) = binder blobId
                match! binderTask with
                | Ok (subject, blobActions, sideEffects) ->
                    return Ok (subject, blobActions @ [ BlobAction.Create (blobId, mimeType, bytes) ], sideEffects)
                | Error err ->
                    return Error err
            }
            |> ConstructionResult

        member this.Bind (action: Blob.MaybeCreateAction, binder: Option<BlobId> -> ConstructionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent>) : ConstructionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent> =
            match action with
            | Blob.MaybeCreate (context, unique, Some (maybeMimeType, bytes)) ->
                this.Bind (Blob.Create (context, unique, maybeMimeType, bytes), Some >> binder)
            | Blob.MaybeCreate (_, _, None) ->
                binder None

        member _.Bind (Blobs.Create (context, unique, sources), binder: List<BlobId> -> ConstructionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent>) : ConstructionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent> =
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

                let (ConstructionResult binderTask) = binder blobIds
                match! binderTask with
                | Ok (subject, blobActions, sideEffects) ->
                    return Ok (subject, blobActions @ actions, sideEffects)
                | Error err ->
                    return Error err
            }
            |> ConstructionResult