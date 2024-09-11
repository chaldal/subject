[<AutoOpen>]
module LibClient.AsyncSerialExecutor

open System.Text
open LibClient.JsInterop

type private Queue<'T>() =
    let mutable items = List.empty<'T>

    member _.Items = items

    member _.Enqueue(item: 'T) =
        items <- items @ [item]

    member _.TryDequeue() =
        match items with
        | head :: tail ->
            items <- tail
            Some head
        | [] -> None

let private log = Log.WithCategory("AsyncSerialExecutor")

// Used to ensure independent Async<'T> instances execute serially, where 'T can vary between instances. This is useful
// when execution overlap of those asyncs would violate invariants.
type AsyncSerialExecutor(name: string) =
    let mutable queue = Queue<string * (unit -> Async<unit>)>()
    let mutable isDraining = false

    member this.Enqueue<'T> (operationName: string) (callback: unit -> Async<'T>): Async<'T> =
        Async.FromContinuations
            (fun (successCont, exceptionCont, _cancellationCont) ->
                // Use wrapping lambda for homogeneity of the underlying queue.
                let executeCallback =
                    fun () ->
                        async {
                            try
                                let timeoutBeforeWarning = TimeSpan.FromSeconds(30)
                                let warningDisposable =
                                    runLaterDisposable
                                        timeoutBeforeWarning
                                        (fun () ->
                                            let operationsInQueue =
                                                queue.Items
                                                |> List.fold
                                                    (fun (sb: StringBuilder) (nextOperationName, _) ->
                                                        if sb.Length > 0 then
                                                            sb.Append(", ") |> ignore

                                                        sb.Append(nextOperationName)
                                                    )
                                                    (StringBuilder())
                                                |> string
                                            log.Warn(
                                                "Execution of a queued async {OperationName} for serial executor {Name} has seemingly stalled - it has failed to finish execution after {Timeout}. The current operations in the queue (next to execute is first) are {OperationsInQueue}",
                                                operationName,
                                                name,
                                                timeoutBeforeWarning,
                                                operationsInQueue
                                            )
                                        )
                                let! result = callback ()
                                warningDisposable.Dispose()

                                successCont(result)
                            with
                            | ex ->
                                exceptionCont(ex)
                        }

                // Queue the callback so that it is processed after any already-queued asyncs.
                queue.Enqueue(operationName, executeCallback)

                // Instigate draining if we need to.
                if not isDraining then
                    this.DrainPendingCallbacks()
                    |> startSafely
            )

    member private this.DrainPendingCallbacks(): Async<unit> =
        async {
            isDraining <- true

            let rec consume (): Async<unit> =
                async {
                    match queue.TryDequeue() with
                    | Some (_operationName, executeCallback) ->
                        // Execution is already safe due to it being wrapped above, so no need to catch exceptions here.
                        do! executeCallback ()

                        // Recurse until done.
                        return! consume ()
                    | None ->
                        Noop
                }

            try
                do! consume ()
            finally
                isDraining <- false
        }
