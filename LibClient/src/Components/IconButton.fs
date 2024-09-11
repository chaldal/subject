[<AutoOpen>]
module LibClient.Components.IconButton

open Fable.React

open LibClient
open LibClient.Icons

open ReactXP.Components
open ReactXP.Styles

module LC =
    module IconButton =
        type PropStateFactory = ButtonHighLevelStateFactory
        let Actionable = ButtonLowLevelState.Actionable
        let InProgress = ButtonLowLevelState.InProgress
        let Disabled   = ButtonLowLevelState.Disabled

        type StateTheme = {
            IconColor: Color
            IconSize: int
            TapTargetMargin: int * int * int * int
        }

        type Theme = {
            Actionable: StateTheme
            Disabled: StateTheme
            InProgress: StateTheme
        }
        with
            member this.StateTheme (lowLevelState: ButtonLowLevelState) =
                match lowLevelState with
                | Actionable _ -> this.Actionable
                | InProgress -> this.InProgress
                | Disabled -> this.Disabled

open LC.IconButton

// TODO: delete after RenderDSL migration
type PropStateFactory = LC.IconButton.PropStateFactory

[<RequireQualifiedAccess>]
module private Styles =
    let spinnerBlock =
        makeViewStyles {
            Position.Absolute
            trbl 0 0 0 0
            AlignItems.Center
            JustifyContent.Center
            backgroundColor (Color.WhiteAlpha 0.5)
        }

    let viewTheme =
        ViewStyles.Memoize(
            fun (theme: Theme) (lowLevelState: ButtonLowLevelState) (isDepressed: bool) ->
                let stateTheme = theme.StateTheme(lowLevelState)

                makeViewStyles {
                    width stateTheme.IconSize
                    height stateTheme.IconSize

                    FlexDirection.Row
                    JustifyContent.Center
                    AlignItems.Center
                    Overflow.VisibleForTapCapture

                    match lowLevelState with
                    | ButtonLowLevelState.Disabled ->
                        opacity 0.5
                    | ButtonLowLevelState.Actionable _ ->
                        Cursor.Pointer
                    | ButtonLowLevelState.InProgress ->
                        Noop

                    if isDepressed then
                        opacity 0.5
                }
        )

    let iconTheme =
        TextStyles.Memoize(
            fun (theme: Theme) (lowLevelState: ButtonLowLevelState) ->
                let stateTheme = theme.StateTheme(lowLevelState)

                makeTextStyles {
                    color stateTheme.IconColor
                    fontSize stateTheme.IconSize
                }
        )

    let tapCaptureTheme =
        ViewStyles.Memoize(
            fun (theme: Theme) (lowLevelState: ButtonLowLevelState) ->
                let stateTheme = theme.StateTheme(lowLevelState)
                let (t, r, b, l) = stateTheme.TapTargetMargin

                makeViewStyles {
                    trbl t r b l
                }
        )

type LibClient.Components.Constructors.LC with
    [<Component>]
    static member IconButton(
            state: ButtonHighLevelState,
            icon: IconConstructor,
            ?styles: array<ViewStyles>,
            ?theme: Theme -> Theme,
            ?key: string
        ) : ReactElement =
        key |> ignore

        let theTheme = Themes.GetMaybeUpdatedWith theme
        let lowLevelState = state.ToLowLevel

        LC.Pointer.State(
            fun pointerState ->
                let isDepressed = pointerState.IsDepressed

                element {
                    RX.View(
                        styles =
                            [|
                                Styles.viewTheme theTheme lowLevelState isDepressed
                                yield! (styles |> Option.defaultValue [||])
                            |],
                        children =
                            elements {
                                LC.Icon(
                                    styles = [| Styles.iconTheme theTheme lowLevelState |],
                                    icon = icon
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
                                        styles = [| Styles.tapCaptureTheme theTheme lowLevelState |],
                                        onPress = onPress,
                                        pointerState = pointerState
                                    )
                                | _ ->
                                    noElement
                            }
                    )
                }
        )