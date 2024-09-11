[<AutoOpen>]
module LibClient.UniDirectionalDataFlow

open Fable.React
open LibLangFsharp

// NOTE we generally don't like abbreviations, but the types defined here
// will be heavily used across many projects, they will be the bread and
// butter of building apps, so the abbreviation will increase readability.

type UDActionKey = string

type UDActionErrorMessage = string

type UDActionResult = Async<Result<unit, UDActionErrorMessage>>

module UDActionResult =
    let ifOkSideEffect (f: 'T -> unit) (udActionResult: Async<Result<'T, UDActionErrorMessage>>) : Async<Result<'T, UDActionErrorMessage>> =
        udActionResult
        |> Async.Map (Result.map (fun value ->
            f value
            value
        ))

    let inParallel (asyncResults: seq<UDActionResult>) : UDActionResult =
        async {
            let! results = Async.Parallel asyncResults
            let errorMessages =
                results
                |> List.ofArray
                |> List.filterMap (function Error message -> Some message | _ -> None)
            match errorMessages with
            | [] -> return Ok ()
            | _  -> return Error (String.concat "\n\n" errorMessages)
        }

type UDAction = unit -> UDActionResult

module UDAction =
    let ofSyncErrorless (f: unit -> unit) : UDAction =
        fun () ->
            f()
            Ok () |> Async.Of

type UDActionError = {
    Message: UDActionErrorMessage
    Dismiss: unit -> unit
}

[<RequireQualifiedAccess>]
type Executor =
| Actionable of Execute: (UDAction -> Deferred<unit>)
| InProgress
| Error of UDActionError
with
    member this.MaybeExecute (action: UDAction) : unit =
        match this with
        | Actionable execute -> execute action |> ignore
        | _                  -> Noop

    member this.MaybeExecuteWithCompletion (action: UDAction) : Option<Deferred<unit>> =
        match this with
        | Actionable execute -> execute action |> Some
        | _                  -> None

    // have to add "Now" because F#
    member this.IsInProgressNow : bool =
        match this with
        | Executor.InProgress -> true
        | _ -> false

type MakeExecutor = UDActionKey -> Executor

// we want to pull out "nonemptiness" of the map one level above the actual
// map, since that makes for easier consumption
type ExecutorErrorsLazy = unit -> Option<NonemptyMap<UDActionKey, UDActionError>>

let NoopUDAction : UDAction = fun () -> () |> Ok |> Async.Of

let globalExecutorContext = Fable.React.ReactBindings.React.createContext (fun _ -> failwith "No value was provided for the global executor context")

let globalExecutorContextProvider: MakeExecutor -> array<ReactElement> -> ReactElement = contextProvider globalExecutorContext

let mutable private globalExecutorInstance: Option<MakeExecutor> = None

let globalExecutor () : MakeExecutor =
    globalExecutorInstance
    |> Option.getOrElseRaise (exn "Global executor was never set. Are you using AppShell.Context?")

let setGlobalExecutor (value: MakeExecutor) : unit =
    globalExecutorInstance <- Some value
