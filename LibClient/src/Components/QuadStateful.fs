[<AutoOpen>]
module LibClient.Components.QuadStateful

open Fable.React

open LibClient

open ReactXP.Components

module LC =
    module QuadStateful =
        type Mode<'InputAcc, 'Input> =
        | Initial
        | Input of 'InputAcc
        | InProgress
        | Error of string

        type InitialInputAcc<'T> =
        | Sync  of 'T
        | Async of Async<Result<'T, string>>

open LC.QuadStateful

[<RequireQualifiedAccess>]
module private Actions =
    let edit (modeHook: IStateHook<Mode<'InputAcc, 'Input>>) (initialInputAcc: InitialInputAcc<'InputAcc>) (_e: ReactEvent.Action) : unit =
        match initialInputAcc with
        | Sync value ->
            modeHook.update (Input value)
        | Async asyncResult ->
            modeHook.update InProgress

            async {
                match! asyncResult with
                | Ok value ->
                    modeHook.update (Input value)
                | Result.Error message ->
                    modeHook.update (Error message)
            } |> startSafely

    let reset (modeHook: IStateHook<Mode<'InputAcc, 'Input>>) (_e: ReactEvent.Action) : unit =
        modeHook.update Initial

    let setInput (modeHook: IStateHook<Mode<'InputAcc, 'Input>>) (value: 'InputAcc) : unit =
        modeHook.update (Input value)

    let act (modeHook: IStateHook<Mode<'InputAcc, 'Input>>) (act: 'Input -> Async<Result<unit, string>>) (input: 'Input) (_e: ReactEvent.Action) : unit =
        modeHook.update InProgress

        async {
            match! act input with
            | Ok () ->
                modeHook.update Initial
            | Result.Error message ->
                modeHook.update (Error message)
        } |> startSafely


type LibClient.Components.Constructors.LC with
    [<Component>]
    static member QuadStateful<'InputAcc, 'Input>(
            initialInputAcc: InitialInputAcc<'InputAcc>,
            act: 'Input -> Async<Result<unit, string>>,
            validate: 'InputAcc -> Option<'Input>,
            initial: ((* Edit *) ReactEvent.Action -> unit) -> ReactElement,
            input: ('InputAcc * ((* SetInput *) 'InputAcc -> unit) * (* MaybeAct *) Option<ReactEvent.Action -> unit> * (* Cancel *) (ReactEvent.Action -> unit)) -> ReactElement
        ) : ReactElement =
        let modeHook = Hooks.useState Mode<'InputAcc, 'Input>.Initial

        match modeHook.current with
        | Initial ->
            initial (Actions.edit modeHook initialInputAcc)

        | Input acc ->
            let maybeAct = validate acc |> Option.map (fun input -> Actions.act modeHook act input)

            element {
                input (acc, Actions.setInput modeHook, maybeAct, Actions.reset modeHook)

                LC.Button(
                    label = "Save",
                    state = ButtonHighLevelState.LowLevel (match maybeAct with | Some act -> ButtonLowLevelState.Actionable act | _ -> ButtonLowLevelState.Disabled)
                )
                LC.Button(
                    label = "Cancel",
                    state = ButtonHighLevelState.LowLevel (ButtonLowLevelState.Actionable (Actions.reset modeHook))
                )
            }

        | InProgress ->
            RX.ActivityIndicator(
                color = "#aaaaaa"
            )

        | Error message ->
            element {
                RX.View(
                    elements {
                        LC.Text $"Error: {message}"
                    }
                )
                LC.Button(
                    label = "OK",
                    state = ButtonHighLevelState.LowLevel (ButtonLowLevelState.Actionable (Actions.reset modeHook))
                )
            }