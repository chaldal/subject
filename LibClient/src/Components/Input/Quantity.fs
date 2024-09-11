[<AutoOpen>]
module LibClient.Components.Input_Quantity

open Fable.React
open LibClient
open LibClient.Components
open LibClient.Icons
open ReactXP.Components
open ReactXP.Styles

module LC =
    module Input =
        module QuantityTypes =
            type OnChange =
            | CanRemove    of (Option<PositiveInteger> -> ReactEvent.Action -> unit)
            | CannotRemove of (PositiveInteger         -> ReactEvent.Action -> unit)
            with
                member this.Call (value: PositiveInteger) : ReactEvent.Action -> unit =
                    match this with
                    | CanRemove    callback -> value |> Some |> callback
                    | CannotRemove callback -> value         |> callback

            type ThemeColors = {
                Border: Color
                Value:  Color
                Icons:  Color
            }

            type Theme = {
                NormalColors: ThemeColors
                InvalidColors: ThemeColors
                InvalidMessageColor: Color
            }

open LC.Input.QuantityTypes

[<RequireQualifiedAccess>]
module private Styles =
    let field =
        ViewStyles.Memoize(
            fun (theme: Theme) (isInvalid: bool) ->
                makeViewStyles {
                    FlexDirection.Row
                    AlignItems.Stretch
                    borderWidth  1
                    borderRadius 4
                    size         96 32

                    borderColor
                        (if isInvalid then theme.InvalidColors.Border else theme.NormalColors.Border)
                }
        )


    let side =
        makeViewStyles {
            flex  0
            width 26
            JustifyContent.Center
            AlignItems.Center
        }

    let center =
        makeViewStyles {
            JustifyContent.Center
            AlignItems.Center
            flex 1
        }

    let centerText =
        TextStyles.Memoize(
            fun (theme: Theme) (isInvalid: bool) ->
                makeTextStyles {
                    fontSize 16

                    color
                        (if isInvalid then theme.InvalidColors.Value else theme.NormalColors.Value)
                }
        )

    let invalidReason =
        TextStyles.Memoize(
            fun (theme: Theme) ->
                makeTextStyles {
                    fontSize 12
                    color theme.InvalidMessageColor
                }
        )

    let iconButtonTheme (theTheme: Theme) (isInvalid: bool) (isQuantityNone: bool) (theme: LC.IconButton.Theme): LC.IconButton.Theme =
        let iconSize =
            if isQuantityNone then 18 else 16
        let themeColors =
            if isInvalid then theTheme.InvalidColors else theTheme.NormalColors

        { theme with
            Actionable =
                { theme.Actionable with
                    IconColor = themeColors.Icons
                    IconSize = iconSize
                }
        }

type LibClient.Components.Constructors.LC.Input with
    [<Component>]
    static member Quantity(
        value: Option<PositiveInteger>,
        validity: InputValidity,
        onChange: OnChange,
        ?max: PositiveInteger,
        ?theme: Theme -> Theme,
        ?styles: array<ViewStyles>,
        ?key: string
    ) : ReactElement =
        key |> ignore

        let theTheme = Themes.GetMaybeUpdatedWith theme
        let isInvalid = validity.IsInvalid

        RX.View(
            children =
                elements {
                    RX.View(
                        styles = [|
                            Styles.field theTheme isInvalid
                            yield! styles |> Option.defaultValue [||]
                        |],
                        children =
                            elements {
                                RX.View(
                                    styles = [| Styles.side |],
                                    children =
                                        elements {
                                            match value with
                                            | Some quantity ->
                                                match (quantity - 1, onChange) with
                                                | (None, CanRemove onChange) ->
                                                    LC.IconButton(
                                                        theme = Styles.iconButtonTheme theTheme isInvalid true,
                                                        icon = Icon.GarbageBin,
                                                        state = ButtonHighLevelState.LowLevel (ButtonLowLevelState.Actionable (onChange None))
                                                    )
                                                | (None, CannotRemove _) ->
                                                    noElement
                                                | (Some decremented, onChange) ->
                                                    LC.IconButton(
                                                        theme = Styles.iconButtonTheme theTheme isInvalid false,
                                                        icon = Icon.Minus,
                                                        state = ButtonHighLevelState.LowLevel (ButtonLowLevelState.Actionable (onChange.Call decremented))
                                                    )
                                            | None ->
                                                noElement
                                        }
                                )

                                RX.View(
                                    styles = [| Styles.center |],
                                    children =
                                        elements {
                                            match value with
                                            | Some value ->
                                                LC.Text(
                                                    value = (string value.Value),
                                                    styles = [| Styles.centerText theTheme isInvalid |]
                                                )
                                            | None ->
                                                noElement
                                        }
                                )

                                let incrementedUnchecked =
                                    value
                                    |> Option.mapOrElse PositiveInteger.One (fun q -> q + 1u)

                                RX.View(
                                    styles = [| Styles.side |],
                                    children =
                                        elements {
                                            let maybeIncremented =
                                                match max with
                                                | Some max ->
                                                    if incrementedUnchecked <= max then
                                                        Some incrementedUnchecked
                                                    else
                                                        None
                                                | None ->
                                                    Some incrementedUnchecked

                                            match maybeIncremented with
                                            | Some incremented ->
                                                LC.IconButton(
                                                    theme = Styles.iconButtonTheme theTheme isInvalid false,
                                                    icon = Icon.Plus,
                                                    state = ButtonHighLevelState.LowLevel (ButtonLowLevelState.Actionable (onChange.Call incremented))
                                                )
                                            | None ->
                                                noElement
                                        }
                                )
                            }
                    )

                    match validity.InvalidReason with
                    | Some reason ->
                        RX.View(
                            children =
                                elements {
                                    LC.Text(
                                        reason,
                                        styles = [| Styles.invalidReason theTheme |]
                                    )
                                }
                        )
                    | None ->
                        noElement
                }
        )