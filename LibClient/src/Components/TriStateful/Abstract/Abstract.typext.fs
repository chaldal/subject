module LibClient.Components.TriStateful.Abstract

open LibClient
open Fable.React

type RunAction = Async<Result<unit, string>> -> unit

type [<RequireQualifiedAccess>] [<DefaultAugmentation(false)>] Mode =
| Initial
| InProgress
| Error of string
with
    member this.MaybeError : Option<string> =
        match this with
        | Error e -> Some e
        | _       -> None

    member this.IsInProgress : bool =
        match this with
        | InProgress -> true
        | _          -> false

type Props = (* GenerateMakeFunction *) {
    Content: (Mode * (* runAction *) RunAction * (* reset *) (ReactEvent.Action -> unit)) -> ReactElement
}

type Estate = {
    Mode: Mode
}

type Abstract(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, Abstract>("LibClient.Components.TriStateful.Abstract", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(_initialProps: Props) : Estate = {
        Mode = Mode.Initial
    }

and Actions(this: Abstract) =
    member _.Reset (_e: ReactEvent.Action) : unit =
        this.SetEstate (fun estate _ -> { estate with Mode = Mode.Initial })

    member _.RunAction (action: Async<Result<unit, string>>) : unit =
        this.SetEstate (fun estate _ -> { estate with Mode = Mode.InProgress })
        async {
            let! actionResult = action
            let nextMode =
                match actionResult with
                | Ok _          -> Mode.Initial
                | Error message -> Mode.Error message

            this.SetEstate (fun estate _ -> { estate with Mode = nextMode })
        } |> startSafely

let Make = makeConstructor<Abstract, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate

let toButtonState (mode: Mode) (runAction: Async<Result<unit, string>> -> unit) (makeTask: unit -> Async<Result<unit, string>>) : ButtonLowLevelState =
    match mode with
    | Mode.Initial ->
        ButtonLowLevelState.Actionable
            (fun _ ->
                makeTask ()
                |> runAction
            )

    | Mode.InProgress ->
        ButtonLowLevelState.InProgress

    | _ ->
        ButtonLowLevelState.Disabled
