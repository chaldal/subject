namespace LibLifeCycle

open System.Threading.Tasks
open LibLifeCycle

[<AutoOpen>]
module TransitionBuilder =
    let noTransitionSideEffects = {
        ExternalActions = []
        Constructors    = []
        LifeActions     = []
        LifeEvents      = []
    }


    type TransitionBuilder() =
        member _.Bind(TransitionResult res: TransitionResult<'Subject1, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor>, binder: 'Subject1 -> TransitionResult<'Subject2, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor>) : TransitionResult<'Subject2, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor> =
            backgroundTask {
                match! res with
                | Ok (TransitionOk (subj, blobActions1, sideEffects1)) ->
                    let (TransitionResult binderTask) = binder subj
                    match! binderTask with
                    | Ok (TransitionOk (subj2, blobActions2, sideEffects2)) ->
                        return (subj2, blobActions1 @ blobActions2, sideEffects1 + sideEffects2) |> TransitionOk |> Ok
                    | Ok (TransitionIgnored (ignoredBlobActions, ignoredSideEffects))->
                        return (blobActions1 @ ignoredBlobActions, sideEffects1 + ignoredSideEffects) |> TransitionIgnored |> Ok
                    | Error err ->
                        return Error err
                | Ok (TransitionIgnored (ignoredBlobActions, ignoredSidEffects))->
                    return (ignoredBlobActions, ignoredSidEffects) |> TransitionIgnored |> Ok
                | Error err ->
                    return Error err
            }
            |> TransitionResult

        member _.Bind(asyncTasks: Map<'Key, Task<'Value>>, binder: Map<'Key, 'Value> -> TransitionResult<'Subject1, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor>) : TransitionResult<'Subject1, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor> =
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

                let (TransitionResult task) =
                    Map.ofArray all
                    |> binder
                return! task
            }
            |> TransitionResult

        member _.Return(value: 'Subject when 'Subject :> Subject<'SubjectId>): TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor> =
            TransitionOk (value, noBlobActions, noTransitionSideEffects)
            |> Ok
            |> Task.FromResult
            |> TransitionResult

        member _.Return<'Subject, 'OpError, 'LifeEvent, 'Constructor, 'LifeAction
                         when 'OpError     :> OpError
                         and  'LifeAction  :> LifeAction
                         and  'Constructor :> Constructor
                         and  'LifeEvent   :> LifeEvent> (error: 'OpError): TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor> =
            LifeCycleError error
            |> Error
            |> Task.FromResult
            |> TransitionResult

        member _.Return(transition: Transition): TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor> =
            match transition with
            | Transition.Ignore ->
                (noBlobActions, noTransitionSideEffects) |> TransitionIgnored |> Ok
            | Transition.NotAllowed ->
                TransitionNotAllowed |> Error
            |> Task.FromResult
            |> TransitionResult

        [<CompilerMessage("QQQ Not implemented yet", 10666, IsError=false)>]
        member _.Return(_todo: TODO): TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor> =
            raise (System.NotImplementedException "QQQ Not implemented yet")

        member _.ReturnFrom (res: TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor>) : TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor> =
            res

        member _.ReturnFrom (res: InfallibleOperationResult<'Subject, 'LifeAction, 'LifeEvent>) : TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor> =
            backgroundTask {
                let (InfallibleOperationResult binderTask) = res
                let! subj, blobActions1, sideEffects1 = binderTask
                return (subj, blobActions1, sideEffects1 + noTransitionSideEffects) |> TransitionOk |> Ok
            }
            |> TransitionResult

        member _.ReturnFrom (res: OperationResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent>) : TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor> =
            backgroundTask {
                let (OperationResult binderTask) = res
                match! binderTask with
                | Ok (subj, blobActions1, sideEffects1) ->
                    return (subj, blobActions1, sideEffects1 + noTransitionSideEffects) |> TransitionOk |> Ok
                | Error (OperationBuilderError.LifeCycleOperationError err) ->
                    return Error (TransitionBuilderError.LifeCycleError err)
                | Error (OperationBuilderError.LifeCycleOperationException exn) ->
                    return Error (TransitionBuilderError.LifeCycleException exn)
            }
            |> TransitionResult

        member _.ReturnFrom(result: Result<'Subject, 'OpError>): TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor> =
            match result with
            | Ok okVal ->
                (okVal, noBlobActions, noTransitionSideEffects) |> TransitionOk |> Ok
            | Error err ->
                LifeCycleError err |> Error
            |> Task.FromResult
            |> TransitionResult

        member _.Yield<'Constructor, 'LifeEvent, 'LifeAction
                        when 'Constructor :> Constructor
                        and  'LifeAction  :> LifeAction
                        and  'LifeEvent   :> LifeEvent>
                (externalAction: IExternalLifeCycleOperation) : TransitionSideEffects<'Constructor, 'LifeEvent, 'LifeAction> =
            { noTransitionSideEffects with ExternalActions = [ ExternalOperation.ExternalLifeCycleOperation externalAction ] }

        member _.YieldFrom<'Constructor, 'LifeEvent, 'LifeAction
                            when 'Constructor :> Constructor
                            and  'LifeAction  :> LifeAction
                            and  'LifeEvent   :> LifeEvent> (sideEffects: seq<IExternalLifeCycleOperation>) : TransitionSideEffects<'Constructor, 'LifeEvent, 'LifeAction> =
            { noTransitionSideEffects with ExternalActions = sideEffects |> Seq.map (ExternalOperation.ExternalLifeCycleOperation) |> Seq.toList }

        member _.Yield<'Constructor, 'LifeEvent, 'LifeAction
                        when 'Constructor :> Constructor
                        and  'LifeAction  :> LifeAction
                        and  'LifeEvent   :> LifeEvent>
                (ingestOperation: IIngestTimeSeriesDataPointsOperation) : TransitionSideEffects<'Constructor, 'LifeEvent, 'LifeAction> =
            { noTransitionSideEffects with ExternalActions = [ ExternalOperation.ExternalTimeSeriesOperation ingestOperation ] }

        member _.YieldFrom<'Constructor, 'LifeEvent, 'LifeAction
                        when 'Constructor :> Constructor
                        and  'LifeAction  :> LifeAction
                        and  'LifeEvent   :> LifeEvent>
                    (sideEffects: seq<IIngestTimeSeriesDataPointsOperation>) : TransitionSideEffects<'Constructor, 'LifeEvent, 'LifeAction> =
            { noTransitionSideEffects with ExternalActions = sideEffects |> Seq.map (ExternalOperation.ExternalTimeSeriesOperation) |> Seq.toList }

        member _.Yield<'Constructor, 'LifeEvent, 'LifeAction
                        when 'Constructor :> Constructor
                        and  'LifeAction  :> LifeAction
                        and  'LifeEvent   :> LifeEvent>
                (externalAction: ExternalOperation<'LifeAction>) : TransitionSideEffects<'Constructor, 'LifeEvent, 'LifeAction> =
            { noTransitionSideEffects with ExternalActions = [externalAction] }

        member _.YieldFrom<'Constructor, 'LifeEvent, 'LifeAction
                        when 'Constructor :> Constructor
                        and  'LifeAction  :> LifeAction
                        and  'LifeEvent   :> LifeEvent> (sideEffects: seq<ExternalOperation<'LifeAction>>) : TransitionSideEffects<'Constructor, 'LifeEvent, 'LifeAction> =
            { noTransitionSideEffects with ExternalActions = Seq.toList sideEffects }

        member _.Yield<'Constructor, 'LifeEvent, 'LifeAction
                            when 'Constructor :> Constructor
                            and  'LifeAction  :> LifeAction
                            and  'LifeEvent   :> LifeEvent> (lifeEvent: 'LifeEvent when 'LifeEvent :> LifeEvent) : TransitionSideEffects<'Constructor, 'LifeEvent, 'LifeAction> =
            { noTransitionSideEffects with LifeEvents = [lifeEvent] }

        member _.Yield<'Constructor, 'LifeEvent, 'LifeAction
                            when 'Constructor :> Constructor
                            and  'LifeAction  :> LifeAction
                            and  'LifeEvent   :> LifeEvent> (lifeAction: 'LifeAction when 'LifeAction :> LifeAction) : TransitionSideEffects<'Constructor, 'LifeEvent, 'LifeAction> =
            { noTransitionSideEffects with LifeActions = [lifeAction] }

        member _.Yield<'Constructor, 'LifeEvent, 'LifeAction
                            when 'Constructor :> Constructor
                            and  'LifeAction  :> LifeAction
                            and  'LifeEvent   :> LifeEvent> (maybeLifeAction: Option<'LifeAction> when 'LifeAction :> LifeAction) : TransitionSideEffects<'Constructor, 'LifeEvent, 'LifeAction> =
            { noTransitionSideEffects with LifeActions = maybeLifeAction |> Option.toList }

        member _.YieldFrom<'Constructor, 'LifeEvent, 'LifeAction
                            when 'Constructor :> Constructor
                            and  'LifeAction  :> LifeAction
                            and  'LifeEvent   :> LifeEvent> (maybeLifeEvent: Option<'LifeEvent> when 'LifeEvent :> LifeEvent) : TransitionSideEffects<'Constructor, 'LifeEvent, 'LifeAction> =
            { noTransitionSideEffects with LifeEvents = (maybeLifeEvent |> Option.toList) }

        member _.YieldFrom<'Constructor, 'LifeEvent, 'LifeAction
                            when 'Constructor :> Constructor
                            and  'LifeAction  :> LifeAction
                            and  'LifeEvent   :> LifeEvent> (lifeEvents: seq<'LifeEvent>) : TransitionSideEffects<'Constructor, 'LifeEvent, 'LifeAction> =
            { noTransitionSideEffects with LifeEvents = Seq.toList lifeEvents }

        member _.YieldFrom<'Constructor, 'LifeEvent, 'LifeAction
                            when 'Constructor :> Constructor
                            and  'LifeAction  :> LifeAction
                            and  'LifeEvent   :> LifeEvent> (lifeActions: seq<'LifeAction>) : TransitionSideEffects<'Constructor, 'LifeEvent, 'LifeAction> =
            { noTransitionSideEffects with LifeActions = Seq.toList lifeActions }

        member _.Yield<'Constructor, 'LifeEvent, 'LifeAction
                        when 'Constructor :> Constructor
                        and  'LifeAction  :> LifeAction
                        and  'LifeEvent   :> LifeEvent>
                                (constructor: 'Constructor) : TransitionSideEffects<'Constructor, 'LifeEvent, 'LifeAction> =
            { noTransitionSideEffects with Constructors = [constructor] }

        member _.YieldFrom<'Constructor, 'LifeEvent, 'LifeAction
                            when 'Constructor :> Constructor
                            and  'LifeAction  :> LifeAction
                            and  'LifeEvent   :> LifeEvent> (constructors: seq<'Constructor>) : TransitionSideEffects<'Constructor, 'LifeEvent, 'LifeAction> =
            { noTransitionSideEffects with Constructors = Seq.toList constructors }

        member _.Combine (sideEffects: TransitionSideEffects<'Constructor, 'LifeEvent, 'LifeAction>, res: unit -> TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor>) : TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor> =
            backgroundTask {
                let (TransitionResult resTask) = res()
                match! resTask with
                | Ok (TransitionOk (subj, okBlobActions, okSideEffects)) ->
                    return (subj, okBlobActions, sideEffects + okSideEffects) |> TransitionOk |> Ok
                | Ok (TransitionIgnored (ignoredBlobActions, ignoredSideEffects)) ->
                    return (ignoredBlobActions, sideEffects + ignoredSideEffects) |> TransitionIgnored |> Ok
                | Error err ->
                    return Error err
            }
            |> TransitionResult

        member _.Delay (value: unit -> TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor>) : unit -> TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor> =
            value

        member _.Run (value: unit -> TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor>) : TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor> =
            value()

        member _.Zero<'Constructor, 'LifeEvent, 'LifeAction
                       when 'Constructor :> Constructor
                       and  'LifeAction  :> LifeAction
                       and  'LifeEvent   :> LifeEvent> () : TransitionSideEffects<'Constructor, 'LifeEvent, 'LifeAction> =
            noTransitionSideEffects

        member this.TryWith (body: unit -> TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor>, handler: exn -> TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor>) : TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor> =
            try
                this.ReturnFrom(body())
            with
            | ex -> handler ex

        member this.TryFinally(body: unit -> TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor>, compensation: unit -> unit) : TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor> =
            try
                this.ReturnFrom(body())
            finally
                compensation()

        member this.Using<'T, 'Constructor, 'LifeEvent, 'LifeAction, 'OpError, 'U
                           when 'T :> System.IDisposable
                            and  'Constructor :> Constructor
                            and  'LifeAction  :> LifeAction
                            and  'LifeEvent   :> LifeEvent
                            and  'OpError     :> OpError>
                (disposable: 'T, body: ('T -> TransitionResult<'U, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor>))
                : TransitionResult<'U, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor> =
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

    let transition = TransitionBuilder()

[<AutoOpen>]
module TransitionBuilderExtensions =
    type TransitionBuilder with

        member _.ReturnFrom (x: Task<'Subject>) : TransitionResult<'Subject, 'LifeAction, _, _, 'Constructor> =
            backgroundTask {
                let! resolved = x
                return (resolved, noBlobActions, noTransitionSideEffects) |> TransitionOk |> Ok
            }
            |> TransitionResult

        member _.ReturnFrom (x: Async<'Subject>) : TransitionResult<'Subject, 'LifeAction, _, _, 'Constructor> =
            backgroundTask {
                let! resolved = x
                return (resolved, noBlobActions, noTransitionSideEffects) |> TransitionOk |> Ok
            }
            |> TransitionResult

        member _.Return(value: 'Subject): TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor> =
            (value, noBlobActions, noTransitionSideEffects)
            |> TransitionOk
            |> Ok
            |> Task.FromResult
            |> TransitionResult

        member _.Bind(InfallibleOperationResult res: InfallibleOperationResult<'Subject1, 'LifeAction, 'LifeEvent>, binder: 'Subject1 -> TransitionResult<'Subject2, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor>) : TransitionResult<'Subject2, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor> =
            backgroundTask {
                let! subj, blobActions1, sideEffects1 = res
                let (TransitionResult binderTask) = binder subj
                match! binderTask with
                | Ok (TransitionOk (subj2, blobActions2, sideEffects2)) ->
                    return (subj2, blobActions1 @ blobActions2, sideEffects1 + sideEffects2) |> TransitionOk |> Ok
                | Ok (TransitionIgnored (ignoredBlobActions, ignoredSideEffects)) ->
                    return (blobActions1 @ ignoredBlobActions, sideEffects1 + ignoredSideEffects) |> TransitionIgnored |> Ok
                | Error err ->
                    return Error err
            }
            |> TransitionResult
        
        member _.Bind(OperationResult res: OperationResult<'Subject1, 'LifeAction, 'OpError, 'LifeEvent>, binder: 'Subject1 -> TransitionResult<'Subject2, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor>) : TransitionResult<'Subject2, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor> =
            backgroundTask {
                match! res with
                | Ok (subj, blobActions1, sideEffects1) ->
                    let (TransitionResult binderTask) = binder subj
                    match! binderTask with
                    | Ok (TransitionOk (subj2, blobActions2, sideEffects2)) ->
                        return (subj2, blobActions1 @ blobActions2, sideEffects1 + sideEffects2) |> TransitionOk |> Ok
                    | Ok (TransitionIgnored (ignoredBlobActions, ignoredSideEffects)) ->
                        return (blobActions1 @ ignoredBlobActions, sideEffects1 + ignoredSideEffects) |> TransitionIgnored |> Ok
                    | Error err ->
                        return Error err
                | Error (OperationBuilderError.LifeCycleOperationError err) ->
                    return Error (TransitionBuilderError.LifeCycleError err)
                | Error (OperationBuilderError.LifeCycleOperationException exn) ->
                    return Error (TransitionBuilderError.LifeCycleException exn)
            }
            |> TransitionResult

        member _.Bind(asyncTask: Task<'Subject1>, binder: 'Subject1 -> TransitionResult<'Subject2, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor>) : TransitionResult<'Subject2, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor> =
            backgroundTask {
                let! resolved = asyncTask
                let (TransitionResult binderTask) = binder resolved
                return! binderTask
            }
            |> TransitionResult

        member _.Bind(tasks: seq<TransitionResult<'Subject1, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor>>, binder: seq<'Subject1> -> TransitionResult<'Subject2, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor>) : TransitionResult<'Subject2, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor> =
            backgroundTask {
                let! tasksResults = Task.WhenAll (tasks |> Array.ofSeq |> Seq.map (function TransitionResult task -> task))

                let processedResults =
                    Array.foldBack
                        (fun taskResult acc ->
                            match acc with
                            | Ok (subjects, actions, sideEffects) ->
                                match taskResult with
                                | Ok (TransitionOk (currSubject, currActions, currSideEffects)) -> Ok (currSubject :: subjects, currActions :: actions, currSideEffects + sideEffects)
                                | Ok (TransitionIgnored (blobActions, sideEffects))             -> Error (Choice1Of2 (blobActions, sideEffects))
                                | Error error                                                   -> Error (Choice2Of2 error)
                            | Error err ->
                                Error err
                        )
                        tasksResults
                        (Ok ([], [], TransitionSideEffects<_, _, _>.get_Zero()))

                match processedResults with
                | Ok (subjects, blobActions, sideEffects) ->
                    let (TransitionResult binderTask) = binder subjects
                    match! binderTask with
                    | Ok (TransitionOk (nextSubject, nextActions, nextSideEffects)) ->
                        return (nextSubject, nextActions @ (List.flatten blobActions), nextSideEffects + sideEffects) |> TransitionOk |> Ok
                    | Ok (TransitionIgnored (ignoredActions, ignoredSideEffects)) ->
                        return (ignoredActions @ (List.flatten blobActions), ignoredSideEffects + sideEffects) |> TransitionIgnored |> Ok
                    | Error error -> return Error error
                | Error (Choice1Of2 (blobActions, sideEffects)) -> return Ok (TransitionIgnored (blobActions, sideEffects))
                | Error (Choice2Of2 error) -> return Error error
            }
            |> TransitionResult

        member _.Bind (Blob.Create (context, unique, maybeMimeType, fileData), binder: BlobId -> TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor>) : TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor> =
            backgroundTask {
                let! blobId = createBlobId context unique
                let blobAction = [ BlobAction.Create (blobId, maybeMimeType, fileData) ]
                let (TransitionResult binderTask) = binder blobId
                match! binderTask with
                | Ok (TransitionOk (subject, blobActions, sideEffects)) ->
                    return (subject, blobActions @ blobAction, sideEffects) |> TransitionOk |> Ok
                | Ok (TransitionIgnored (ignoredBlobActions, ignoredSideEffects)) ->
                    return (ignoredBlobActions @ blobAction, ignoredSideEffects) |> TransitionIgnored |> Ok
                | Error err ->
                    return Error err
            }
            |> TransitionResult

        member _.Bind (Blobs.Create (context, unique, sources), binder: List<BlobId> -> TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor>) : TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor> =
            let tasks =
                sources
                |> Seq.map (fun (maybeMimeType, fileDatas) -> backgroundTask {
                    let! blobId = createBlobId context unique
                    return (blobId, BlobAction.Create (blobId, maybeMimeType, fileDatas))
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

                let (TransitionResult binderTask) = binder blobIds
                match! binderTask with
                | Ok (TransitionOk (subject, blobActions, sideEffects)) ->
                    return (subject, blobActions @ actions, sideEffects) |> TransitionOk |> Ok
                | Ok (TransitionIgnored (ignoredBlobActions, ignoredSideEffects)) ->
                    return (ignoredBlobActions @ actions, ignoredSideEffects) |> TransitionIgnored |> Ok
                | Error err ->
                    return Error err
            }
            |> TransitionResult

        member this.Bind (action: Blob.MaybeCreateAction, binder: Option<BlobId> -> TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor>) : TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor> =
            match action with
            | Blob.MaybeCreate (context, unique, Some (maybeMimeType, fileData)) ->
                this.Bind (Blob.Create (context, unique, maybeMimeType, fileData), Some >> binder)
            | Blob.MaybeCreate (_, _, None) ->
                binder None

        member this.Bind (Blob.MaybeCreateOrReplace (context, unique, maybeOriginalBlobId, maybeNewData), binder: Option<BlobId> -> TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor>) : TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor> =
            match maybeOriginalBlobId, maybeNewData with
            | None, Some (newMaybeMimeType, fileData) ->
                // new blob created
                this.Bind (Blob.Create (context, unique, newMaybeMimeType, fileData), Some >> binder)
            | Some originalBlobId, Some (maybeMimeType, fileData) ->
                // existing blob replaced

                backgroundTask {
                    let! updatedBlobId = createBlobId context unique

                    let deleteAction = BlobAction.Delete originalBlobId
                    let createAction = BlobAction.Create (updatedBlobId, maybeMimeType, fileData)
                    let (TransitionResult binderTask) = (Some >> binder) updatedBlobId
                    match! binderTask with
                    | Ok (TransitionOk (subject, blobActions, sideEffects)) ->
                        return (subject, blobActions @ [deleteAction; createAction], sideEffects) |> TransitionOk |> Ok
                    | Ok (TransitionIgnored (ignoredBlobActions, ignoredSideEffects)) ->
                        return (ignoredBlobActions @ [deleteAction; createAction], ignoredSideEffects) |> TransitionIgnored |> Ok
                    | Error err ->
                        return Error err
                }
                |> TransitionResult
            | Some oldBlobId, None ->
                // blob unchanged
                binder <| Some oldBlobId
            | None, None ->
                binder <| None

        member _.Bind (Blob.Append (actionContext, originalBlobId, bytesToAppend), binder: BlobId -> TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor>) : TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor> =
            backgroundTask {
                let! subjectRef = actionContext.Query CurrentSubjectRef
                assertBlobOwner originalBlobId subjectRef

                let updatedBlobId =
                    { originalBlobId with Revision_ = originalBlobId.Revision_ + 1u }

                let (TransitionResult binderTask) = binder updatedBlobId
                match! binderTask with
                | Ok (TransitionOk (subject, blobActions, sideEffects)) ->
                    return (subject, blobActions @ [ (BlobAction.Append (originalBlobId, updatedBlobId, bytesToAppend)) ], sideEffects) |> TransitionOk |> Ok
                | Ok (TransitionIgnored (ignoredBlobActions, ignoredSideEffects)) ->
                    return (ignoredBlobActions @ [ (BlobAction.Append (originalBlobId, updatedBlobId, bytesToAppend)) ], ignoredSideEffects) |> TransitionIgnored |> Ok
                | Error err ->
                    return Error err
            }
            |> TransitionResult

        member _.Bind (Blob.Delete (actionContext, blobId), binder: unit -> TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor>) : TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor> =
            backgroundTask {
                let! subjectRef = actionContext.Query CurrentSubjectRef
                assertBlobOwner blobId subjectRef

                let (TransitionResult binderTask) = binder ()
                match! binderTask with
                | Ok (TransitionOk (subject, blobActions, sideEffects)) ->
                    return (subject,  blobActions @ [ (BlobAction.Delete blobId) ], sideEffects) |> TransitionOk |> Ok
                | Ok (TransitionIgnored (ignoredBlobActions, ignoredSideEffects)) ->
                    return (ignoredBlobActions @ [ (BlobAction.Delete blobId) ], ignoredSideEffects) |> TransitionIgnored |> Ok
                | Error err ->
                    return Error err
            }
            |> TransitionResult

        member this.Bind (action: Blob.MaybeDeleteAction, binder: unit -> TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor>) : TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor> =
            match action with
            | Blob.MaybeDelete (context, Some blobId) ->
                this.Bind (Blob.Delete (context, blobId), binder)
            | Blob.MaybeDelete (_, None) ->
                backgroundTask {
                    let (TransitionResult binderTask) = binder ()
                    match! binderTask with
                    | Ok (TransitionOk (subject, blobActions, sideEffects)) ->
                        return (subject, blobActions, sideEffects) |> TransitionOk |> Ok
                    | Ok (TransitionIgnored (ignoredBlobActions, ignoredSideEffects)) ->
                        return (ignoredBlobActions, ignoredSideEffects) |> TransitionIgnored |> Ok
                    | Error err ->
                        return Error err
                }
                |> TransitionResult

        member _.Bind (Blobs.Delete (actionContext, blobIds), binder: unit -> TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor>) : TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor> =
            backgroundTask {
                let! subjectRef = actionContext.Query CurrentSubjectRef
                blobIds |> List.iter (fun blobId -> assertBlobOwner blobId subjectRef)
                let deleteActions = blobIds |> List.map BlobAction.Delete

                let (TransitionResult binderTask) = binder ()
                match! binderTask with
                | Ok (TransitionOk (subject, blobActions, sideEffects)) ->
                    return (subject, blobActions @ deleteActions, sideEffects) |> TransitionOk |> Ok
                | Ok (TransitionIgnored (ignoredBlobActions, ignoredSideEffects)) ->
                    return (ignoredBlobActions @ deleteActions, ignoredSideEffects) |> TransitionIgnored |> Ok
                | Error err ->
                    return Error err
            }
            |> TransitionResult
