[<AutoOpen>]
module LibClient.Components.Tab

open Fable.React

open LibClient

open ReactXP.Components
open ReactXP.Styles

module LC =
    module Tab =
        type State =
        | Selected
        | Unselected of OnPress: (ReactEvent.Action -> unit)

        type Theme = {
            SelectedColor: Color
            UnselectedColor: Color
        }

open LC.Tab

[<RequireQualifiedAccess>]
module private Styles =
    let label =
        makeViewStyles {
            paddingHV 6 8
        }

    let viewTheme =
        ViewStyles.Memoize(
            fun (theme: Theme) (state: State) ->
                makeViewStyles {
                    FlexDirection.Row
                    AlignItems.Stretch
                    borderBottomWidth 3
                    marginHorizontal  11
                    marginBottom      -1 // to overlap the LC.Tabs bottom border
                    Cursor.Pointer

                    match state with
                    | Selected ->
                        borderColor theme.SelectedColor
                    | Unselected _ ->
                        borderColor Color.Transparent
                }
        )

    let labelTextTheme =
        TextStyles.Memoize(
            fun (theme: Theme) (state: State) ->
                makeTextStyles {
                    fontSize          16

                    color
                        (match state with
                        | Selected -> theme.SelectedColor
                        | Unselected _ -> theme.UnselectedColor)
                }
        )

type LibClient.Components.Constructors.LC with
    [<Component>]
    static member Tab(
            label: string,
            state: State,
            ?styles: array<ViewStyles>,
            ?theme: Theme -> Theme,
            ?key: string
        ) : ReactElement =
        key |> ignore

        let theTheme = Themes.GetMaybeUpdatedWith theme

        RX.View(
            styles =
                [|
                    Styles.viewTheme theTheme state
                    yield! styles |> Option.defaultValue [||]
                |],
            children =
                elements {
                    RX.View(
                        styles = [| Styles.label |],
                        children =
                            elements {
                                LC.UiText(
                                    label,
                                    styles =
                                        [|
                                            Styles.labelTextTheme theTheme state
                                        |]
                                )
                            }
                    )

                    match state with
                    | Unselected onPress ->
                        LC.TapCapture(
                            onPress = onPress
                        )
                    | _ ->
                        noElement
                }
        )
