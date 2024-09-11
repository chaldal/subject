[<AutoOpen>]
module AsyncExtensions

type Microsoft.FSharp.Control.Async with
    static member Sleep (delay: System.TimeSpan) : Async<unit> =
        Async.Sleep (int delay.TotalMilliseconds)

    static member RunLater (delay: System.TimeSpan) (body: unit -> unit) : unit =
        async {
            do! Async.Sleep (int delay.TotalMilliseconds)
            body()
        }

// TODO this should be using `startSafely` or something similar (dependencies may
// not let it be used here), lest errors get swallowed
#if FABLE_COMPILER
        |> Async.StartImmediate
#else
        |> Async.Start
#endif

    static member TryCatch (raw: Async<'T>) : Async<Result<'T, exn>> = async {
        let! choice = raw |> Async.Catch
        match choice with
        | Choice1Of2 result -> return (Ok result)
        | Choice2Of2 e      -> return (Error e)
    }

    static member TryStart (exceptionHandler: exn -> unit) (raw: Async<unit>) : unit =
        async {
            match! raw |> Async.TryCatch with
            | Ok ()   -> return ()
            | Error e -> exceptionHandler e
        }
#if FABLE_COMPILER
        |> Async.StartImmediate
#else
        |> Async.Start
#endif

    static member StartLoggingOnError (logFunction: exn -> Printf.StringFormat<unit, unit> -> unit) (raw: Async<unit>) : unit =
        Async.TryStart (function e -> logFunction e "Caught exception in Async.StartLoggingOnError") raw

    static member Map (func: 'T -> 'U) (input: Async<'T>) : Async<'U> =
        async {
            let! t = input
            return func t
        }

    static member Of (value: 'T) : Async<'T> =
        async.Return value

    static member FoldOf (fns: List<'T -> Async<'T>>) (initialValue: 'T) : Async<'T> =
        Async.Fold fns (Async.Of initialValue)

    static member Fold (fns: List<'T -> Async<'T>>) (initialValueAsync: Async<'T>) : Async<'T> =
        fns
        |> List.fold
            (fun currValueAsync fn ->
                async {
                    let! currValue = currValueAsync
                    return! fn currValue
                }
            )
            initialValueAsync

    static member Never<'T> () : Async<'T> =
        Async.FromContinuations (fun _ -> Noop)
