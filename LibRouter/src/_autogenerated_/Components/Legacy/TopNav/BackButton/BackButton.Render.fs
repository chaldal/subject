module LibRouter.Components.Legacy.TopNav.BackButtonRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard


open LibClient.Components
open LibRouter.Components
open ReactXP.Components
open LibRouter.Components

open LibClient
open LibClient.RenderHelpers

open LibRouter.Components.Legacy.TopNav.BackButton



let render(children: array<ReactElement>, props: LibRouter.Components.Legacy.TopNav.BackButton.Props, estate: LibRouter.Components.Legacy.TopNav.BackButton.Estate, pstate: LibRouter.Components.Legacy.TopNav.BackButton.Pstate, actions: LibRouter.Components.Legacy.TopNav.BackButton.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    (
        let navigate = LibRouter.Components.Router.useNavigate()
        let __parentFQN = Some "LibClient.Components.Legacy.TopNav.IconButton"
        let __currClass = "icon-button"
        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
        LibClient.Components.Constructors.LC.Legacy.TopNav.IconButton(
            state = (LibClient.Components.Legacy.TopNav.IconButton.Actionable (fun _ -> navigate.GoBack())),
            icon = (LibClient.Icons.Icon.Back),
            ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
        )
    )
