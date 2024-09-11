[<AutoOpen>]
module LibRouter.Components.Nav_Top_BackButton

open Fable.React
open LibClient
open LibClient.Components
open LibClient.Icons
open ReactXP.Styles
open LibClient.Components.Nav.Top.ItemStyles

module LR =
    module Nav =
        module Top =
            module BackButtonTypes =
                type StateTheme = {
                    BaseColors: Colors
                    HoveredColors: Colors
                    DepressedColors: Colors
                }

                type Theme = {
                    Actionable: StateTheme
                    InProgress: StateTheme
                    Disabled: StateTheme
                    Selected: StateTheme
                    SelectedActionable: StateTheme
                }

open LR.Nav.Top.BackButtonTypes

[<RequireQualifiedAccess>]
module private Styles =
    let viewThemeLegacy (theTheme: Theme) =
        [
            LibClient.Components.Nav.Top.ItemStyles.Theme.Part(State.Actionable ignore, theTheme.Actionable.BaseColors, theTheme.Actionable.HoveredColors, theTheme.Actionable.DepressedColors)
            LibClient.Components.Nav.Top.ItemStyles.Theme.Part(State.InProgress, theTheme.InProgress.BaseColors, theTheme.InProgress.HoveredColors, theTheme.InProgress.DepressedColors)
            LibClient.Components.Nav.Top.ItemStyles.Theme.Part(State.Disabled, theTheme.Disabled.BaseColors, theTheme.Disabled.HoveredColors, theTheme.Disabled.DepressedColors)
            LibClient.Components.Nav.Top.ItemStyles.Theme.Part(State.Selected, theTheme.Selected.BaseColors, theTheme.Selected.HoveredColors, theTheme.Selected.DepressedColors)
            LibClient.Components.Nav.Top.ItemStyles.Theme.Part(State.SelectedActionable ignore, theTheme.SelectedActionable.BaseColors, theTheme.SelectedActionable.HoveredColors, theTheme.SelectedActionable.DepressedColors)
        ]
        |> List.concat
        |> ReactXP.LegacyStyles.Designtime.makeSheet
        |> legacyTheme

type LibRouter.Components.Constructors.LR.Nav.Top with
    [<Component>]
    static member BackButton(
            ?theme: Theme -> Theme,
            ?styles: array<ViewStyles>,
            ?key: string) : ReactElement =
        key |> ignore

        let theTheme = Themes.GetMaybeUpdatedWith theme
        let navigate = LibRouter.Components.Router.useNavigate()

        let goBack (_: ReactEvent.Action) =
            navigate.GoBack()

        LC.Nav.Top.Item(
            styles = (styles |> Option.defaultValue [||]),
            style = Nav.Top.Item.iconOnly Icon.Back,
            state = Nav.Top.Item.State.Actionable goBack,
            xLegacyStyles = Styles.viewThemeLegacy theTheme
        )
