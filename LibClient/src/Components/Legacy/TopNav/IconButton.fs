[<AutoOpen>]
module LibClient.Components.Legacy_TopNav_IconButton

open Fable.React
open LibClient
open ReactXP.Styles

module LC =
    module Legacy =
        module TopNav =
            module IconButtonTypes =
                type Theme = {
                    IconColor: Color
                    IconSize: int
                }

                type State = LibClient.Input.ButtonLowLevelState
                let Actionable = State.Actionable
                let InProgress = State.InProgress
                let Disabled   = State.Disabled

open LC.Legacy.TopNav.IconButtonTypes

[<RequireQualifiedAccess>]
module private Styles =
    let iconButtonTheme (theTheme: Theme) (theme: LC.IconButton.Theme): LC.IconButton.Theme =
        { theme with
            Actionable =
                { theme.Actionable with
                    IconColor = theTheme.IconColor
                    IconSize = theTheme.IconSize
                }
        }

type LibClient.Components.Constructors.LC.Legacy.TopNav with
    [<Component>]
    static member IconButton(
            state: State,
            icon: LibClient.Icons.IconConstructor,
            ?theme: Theme -> Theme,
            ?styles: array<ViewStyles>,
            ?key: string) : ReactElement =
        key |> ignore

        let theTheme = Themes.GetMaybeUpdatedWith theme

        LC.IconButton(
            styles = (styles |> Option.defaultValue [||]),
            icon = icon,
            state = ButtonHighLevelState.LowLevel state,
            theme = Styles.iconButtonTheme theTheme
        )
