[<AutoOpen>]
module LibRouter.Components.Legacy_TopNav_BackButton

open Fable.React
open LibClient
open LibClient.Components
open ReactXP.Styles

module LR =
    module Legacy =
        module TopNav =
            module BackButtonTypes =
                type Theme = {
                    IconColor: Color
                }

open LR.Legacy.TopNav.BackButtonTypes

[<RequireQualifiedAccess>]
module private Styles =
    let iconButtonTheme (theTheme: Theme) (theme: LC.Legacy.TopNav.IconButtonTypes.Theme): LC.Legacy.TopNav.IconButtonTypes.Theme =
        { theme with IconColor = theTheme.IconColor }

type LibRouter.Components.Constructors.LR.Legacy.TopNav with
    [<Component>]
    static member BackButton(
            ?theme: Theme -> Theme,
            ?key: string) : ReactElement =
        key |> ignore

        let theTheme = Themes.GetMaybeUpdatedWith theme
        let navigate = LibRouter.Components.Router.useNavigate()
        let location = LibRouter.Components.Router.useLocation()

        let goBack (_: ReactEvent.Action) =
            if location.key = "default" then
                navigate.Replace "/"
            else
                navigate.GoBack()

        LC.Legacy.TopNav.IconButton(
            theme = Styles.iconButtonTheme theTheme,
            icon = LibClient.Icons.Icon.Back,
            state = LC.Legacy.TopNav.IconButtonTypes.State.Actionable goBack
        )
