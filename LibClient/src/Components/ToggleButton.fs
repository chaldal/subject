[<AutoOpen>]
module LibClient.Components.ToggleButton

open Fable.React

open LibClient
open LibClient.Icons

open ReactXP.Components
open ReactXP.Styles

module LC =
    module ToggleButton =
        type Style =
        | Label        of string
        | Icon         of IconConstructor
        | LabelAndIcon of string * IconConstructor
        with
            member this.Parts : Option<string> * Option<IconConstructor> =
                match this with
                | Label        label         -> (Some label, None     )
                | Icon         icon          -> (None,       Some icon)
                | LabelAndIcon (label, icon) -> (Some label, Some icon)

        [<RequireQualifiedAccess>]
        type Position =
        | First
        | Inner
        | Last

        type ColorTheme = {
            TextColor: Color
            BorderColor: Color
            BackgroundColor: Color
        }

        type Theme = {
            Selected: ColorTheme
            Unselected: ColorTheme
        }
        with
            member this.ColorTheme (isSelected: bool): ColorTheme =
                if isSelected then
                    this.Selected
                else
                    this.Unselected

open LC.ToggleButton

[<RequireQualifiedAccess>]
module private Styles =
    let viewTheme =
        ViewStyles.Memoize(
            fun (theme: Theme) (isSelected: bool) ->
                let colorTheme = theme.ColorTheme isSelected

                makeViewStyles {
                    paddingHV 12 4
                    borderWidth 1
                    marginLeft -1
                    Cursor.Pointer
                    borderColor colorTheme.BorderColor
                    backgroundColor colorTheme.BackgroundColor
                }
        )

    let firstView =
        makeViewStyles {
            marginLeft 0
            borderTopLeftRadius 5
            borderBottomLeftRadius 5
        }

    let lastView =
        makeViewStyles {
            borderTopRightRadius 5
            borderBottomRightRadius 5
        }

    let iconTheme =
        TextStyles.Memoize(
            fun (theme: Theme) (isSelected: bool) ->
                let colorTheme = theme.ColorTheme isSelected

                makeTextStyles {
                    fontSize 20
                    color colorTheme.TextColor
                }
        )

    let leftIcon =
        makeViewStyles {
            marginHorizontal 2
        }

    let labelBlock =
        makeViewStyles {
            FlexDirection.Row
            JustifyContent.Center
            AlignItems.Center
        }

    let labelTheme =
        TextStyles.Memoize(
            fun (theme: Theme) (isSelected: bool) ->
                let colorTheme = theme.ColorTheme isSelected

                makeTextStyles {
                    color colorTheme.TextColor
                }
        )

type LibClient.Components.Constructors.LC with
    [<Component>]
    static member ToggleButton<'T>(
            style: Style,
            value: 'T,
            group: ToggleButtons.Group<'T>,
            ?position: Position,
            ?pointerState: LC.Pointer.State.PointerState,
            ?theme: Theme -> Theme,
            ?styles : array<ViewStyles>,
            ?key: string
        ) : ReactElement =
        key |> ignore

        let position = defaultArg position Position.Inner
        let theTheme = Themes.GetMaybeUpdatedWith theme

        let isSelected = group.IsSelected value
        let (maybeLabel, maybeIcon) = style.Parts

        RX.View(
            styles =
                [|
                    Styles.viewTheme theTheme isSelected

                    match position with
                    | Position.First -> Styles.firstView
                    | Position.Last -> Styles.lastView
                    | Position.Inner -> Noop

                    yield! (styles |> Option.defaultValue [||])
                |],
            children =
                elements {
                    RX.View(
                        styles = [| Styles.labelBlock |],
                        children =
                            elements {
                                RX.View(
                                    styles = [| Styles.leftIcon |],
                                    children =
                                        elements {
                                            match maybeIcon with
                                            | Some icon ->
                                                LC.Icon(
                                                    styles = [| Styles.iconTheme theTheme isSelected |],
                                                    icon = icon
                                                )
                                            | None ->
                                                noElement
                                        }
                                )

                                match maybeLabel with
                                | Some label ->
                                    LC.UiText(
                                        value = label,
                                        styles = [| Styles.labelTheme theTheme isSelected |]
                                    )
                                | None ->
                                    noElement
                            }
                    )

                    LC.TapCapture(
                        onPress = group.Toggle value,
                        ?pointerState = pointerState
                    )
                }
        )
