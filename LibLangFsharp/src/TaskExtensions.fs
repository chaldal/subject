[<AutoOpen>]
module TaskExtensions

open System.Threading
open System.Threading.Tasks

#if !FABLE_COMPILER

[<RequireQualifiedAccess>]
module Task =
    let map (f: 'T -> 'U) (t: Task<'T>) : Task<'U> =
        backgroundTask {
            let! value = t
            return f value
        }

    let iter (f: 'T -> unit) (t: Task<'T>) : Task<unit> =
        map f t

    let fireAndForget (_: Task<_>) : unit = ()

    let unwrap (task: Task<Task<'T>>) : Task<'T> =
        task.Unwrap()

    let map2 (f: 'T1 -> 'T2 -> 'U) (t1: Task<'T1>) (t2: Task<'T2>) : Task<'U> =
        backgroundTask {
            let! value1 = t1
            let! value2 = t2
            return f value1 value2
        }

    let asUnit (task: Task) : Task<unit> =
        backgroundTask {
            do! task
            return ()
        }

    let ignoreHandleError (handleError: 'Error -> unit) (task: Task<Result<'T, 'Error>>) : Task =
        backgroundTask {
            match! task with
            | Ok _ ->
                Noop
            | Error err ->
                handleError err
                Noop
        }

    let startOnScheduler (taskScheduler: TaskScheduler) (taskFactory: unit -> Task<'T>) : Task<'T> =
        Task.Factory
            .StartNew(taskFactory, CancellationToken.None, TaskCreationOptions.DenyChildAttach, taskScheduler)
            .Unwrap()

    let startOnThreadPool (taskFactory: unit -> Task<'T>) : Task<'T> =
        startOnScheduler TaskScheduler.Default taskFactory

    let batched (batchSize: int) (taskFactories: seq<unit -> Task<'T>>) : Task<list<'T>> =
        taskFactories
        |> Seq.chunkBySize batchSize
        |> Seq.fold (
            fun (prevTask: Task<list<'T>>) chunk ->
                backgroundTask {
                    let! prevTaskResult = prevTask
                    let! thisTaskResult =
                        chunk
                        |> Seq.map (fun factory -> factory())
                        |> Task.WhenAll

                    return prevTaskResult @ (Array.toList thisTaskResult)
                }) (Task.FromResult [])

type Task with
    // Too many existing references to move to Task.ignore, so going to leave this as-is
    static member Ignore (task: Task<'T>) : Task =
        task :> Task

#endif
