// Contains implementations of useful IDisposable implementations, inspired by Rx. These are for usage within a Fable context, so
// no thread-safety is required (and we'd just use Rx if we needed these outside of Fable, hence them being in the LibClient project).

[<AutoOpen>]
module LibClient.Disposables

open System

// Does nothing when disposed.
type EmptyDisposable private() =
    static let mutable instance = new EmptyDisposable()

    static member Instance = instance

    interface IDisposable with
        member _.Dispose() =
            ()

/// Many disposables under the guise of one.
type CompositeDisposable(disposables: List<IDisposable>) =
    let mutable disposed = false

    interface IDisposable with
        member _.Dispose() =
            match disposed with
            | true -> ()
            | false ->
                disposables
                |> List.iter (fun d -> d.Dispose())

                disposed <- true

/// A disposable with an inner, replaceable disposable, with automatic disposal of any predecessor.
type SerialDisposable() =
    let mutable maybeInnerDisposable: Option<IDisposable> = None
    let mutable disposed = false

    member _.ReplaceInnerDisposable (innerDisposable: IDisposable) =
        if disposed then
            ObjectDisposedException("SerialDisposable already disposed")
            |> raise

        match maybeInnerDisposable with
        | Some innerDisposable -> innerDisposable.Dispose()
        | None -> ()

        maybeInnerDisposable <- Some innerDisposable

    interface IDisposable with
        member _.Dispose() =
            match disposed with
            | true -> ()
            | false ->
                match maybeInnerDisposable with
                | Some disposable -> disposable.Dispose()
                | None -> ()

                disposed <- true
