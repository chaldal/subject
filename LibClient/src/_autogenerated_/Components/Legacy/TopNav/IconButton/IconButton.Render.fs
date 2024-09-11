module LibClient.Components.Legacy.TopNav.IconButtonRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard


open LibClient.Components
open ReactXP.Components
open LibClient.Components

open LibClient
open LibClient.RenderHelpers
open LibClient.Icons
open LibClient.Chars
open LibClient.ColorModule
open LibClient.LocalImages
open LibClient.Responsive

open LibClient.Components.Legacy.TopNav.IconButton



let render(children: array<ReactElement>, props: LibClient.Components.Legacy.TopNav.IconButton.Props, estate: LibClient.Components.Legacy.TopNav.IconButton.Estate, pstate: LibClient.Components.Legacy.TopNav.IconButton.Pstate, actions: LibClient.Components.Legacy.TopNav.IconButton.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "LibClient.Components.IconButton"
    let __currClass = (System.String.Format("{0}{1}{2}", "icon-button ", (TopLevelBlockClass), ""))
    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
    LibClient.Components.Constructors.LC.IconButton(
        state = (LibClient.Components.IconButton.PropStateFactory.MakeLowLevel(props.State)),
        icon = (props.Icon),
        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
    )
