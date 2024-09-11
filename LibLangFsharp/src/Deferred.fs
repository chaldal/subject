namespace LibLangFsharp

type ContinuationTuple<'T> = ('T -> unit) * (exn -> unit) * (System.OperationCanceledException -> unit)

type Deferred<'T>() =
    let mutable maybeResolvedValue: Option<Result<'T, exn>>     = None
    let mutable continuations:      List<ContinuationTuple<'T>> = []

    let resolve (wrappedValue: Result<'T, exn>) (continueFn: ContinuationTuple<'T> -> unit) : unit =
        match maybeResolvedValue with
        | None ->
            maybeResolvedValue <- Some (wrappedValue)
            continuations
            |> List.iter continueFn
            continuations <- []
        | Some _ -> failwith "This deferred has already been resolved"

    member _.Resolve (value: 'T) : unit =
        resolve (Ok value) (fun (cont, _, _) -> cont value)

    member this.ResolveIfPending (value: 'T) : unit =
        if this.IsPending then this.Resolve value

    member _.Reject (error: exn) : unit =
        resolve (Error error) (fun (_, econt, _) -> econt error)

    member _.Value : Async<'T> =
        match maybeResolvedValue with
        | Some (Ok value) -> async { return value }
        | Some (Error e)  -> async { return raise e }
        | None ->
            Async.FromContinuations (fun (cont, econt, ccont) ->
                continuations <- (cont, econt, ccont) :: continuations
            )

    member _.IsPending : bool =
        maybeResolvedValue.IsNone