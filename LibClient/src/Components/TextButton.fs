[<AutoOpen>]
module LibClient.Components.TextButton

open Fable.React

open LibClient

open ReactXP.Components
open ReactXP.Styles

module LC =
    module TextButton =
        type PropStateFactory = ButtonHighLevelStateFactory
        let Actionable = ButtonLowLevelState.Actionable
        let InProgress = ButtonLowLevelState.InProgress
        let Disabled   = ButtonLowLevelState.Disabled

        type Level =
        | Primary
        | Secondary

        type StateTheme = {
            TextColor: Color
            FontSize: int
            Opacity: float
        }

        type StatesTheme = {
            Actionable: StateTheme
            Disabled: StateTheme
            InProgress: StateTheme
        }

        type Theme = {
            Primary: StatesTheme
            Secondary: StatesTheme
        }
        with
            member this.StateTheme (level: Level) (state: ButtonLowLevelState) =
                let statesTheme =
                    match level with
                    | Primary -> this.Primary
                    | Secondary -> this.Secondary

                match state with
                | Actionable _ -> statesTheme.Actionable
                | InProgress -> statesTheme.InProgress
                | Disabled -> statesTheme.Disabled

open LC.TextButton

// TODO: delete after RenderDSL migration
type PropStateFactory = LC.TextButton.PropStateFactory

[<RequireQualifiedAccess>]
module private Styles =
    let tapCapture =
        makeViewStyles {
            trbl -4 -12 -4 -12
        }

    let view =
        ViewStyles.Memoize(
            fun (state: ButtonLowLevelState) ->
                makeViewStyles {
                    Overflow.VisibleForTapCapture

                    match state with
                    | Actionable _ ->
                        Cursor.Pointer
                    | _ ->
                        Noop
                }
        )

    let textTheme =
        TextStyles.Memoize(
            fun (theme: Theme) (level: Level) (state: ButtonLowLevelState) ->
                let stateTheme = theme.StateTheme level state

                makeTextStyles {
                    color stateTheme.TextColor
                    fontSize stateTheme.FontSize
                    opacity stateTheme.Opacity
                }
        )

    let spinnerBlock =
        makeViewStyles {
            Position.Absolute
            trbl 0 0 0 0
            AlignItems.Center
            JustifyContent.Center
            backgroundColor (Color.WhiteAlpha 0.5)
        }

type LibClient.Components.Constructors.LC with
    [<Component>]
    static member TextButton(
            state: ButtonHighLevelState,
            label: string,
            ?level: Level,
            ?numberOfLines: int,
            ?styles: array<TextStyles>,
            ?theme:   Theme -> Theme,
            ?key: string
        ) : ReactElement =
        key |> ignore

        let level = defaultArg level Primary
        let theTheme = Themes.GetMaybeUpdatedWith theme
        let lowLevelState = state.ToLowLevel

        RX.View(
            styles = [| Styles.view lowLevelState |],
            children =
                elements {
                    LC.Text(
                        value = label,
                        styles =
                            [|
                                Styles.textTheme theTheme level lowLevelState
                                yield! (styles |> Option.defaultValue [||])
                            |],
                        ?numberOfLines = numberOfLines
                    )

                    match lowLevelState with
                    | InProgress ->
                        RX.View(
                            styles = [| Styles.spinnerBlock |],
                            children =
                                elements {
                                    RX.ActivityIndicator(
                                        color = "#aaaaaa",
                                        size = Size.Tiny
                                    )
                                }
                        )
                    | _ ->
                        noElement

                    match lowLevelState with
                    | Actionable onPress ->
                        LC.TapCapture(
                            styles = [| Styles.tapCapture |],
                            onPress = onPress
                        )
                    | _ ->
                        noElement
                }
        )