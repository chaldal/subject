module LibClient.Components.Sidebar.BaseRender

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

open LibClient.Components.Sidebar.Base



let render(children: array<ReactElement>, props: LibClient.Components.Sidebar.Base.Props, estate: LibClient.Components.Sidebar.Base.Estate, pstate: LibClient.Components.Sidebar.Base.Pstate, actions: LibClient.Components.Sidebar.Base.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "LibClient.Components.VerticallyScrollable"
    let __currClass = "view"
    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
    LibClient.Components.Constructors.LC.VerticallyScrollable(
        scrollableMiddle = (props.ScrollableMiddle),
        fixedBottom = (props.FixedBottom),
        fixedTop = (props.FixedTop),
        ?styles = (props.styles),
        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
    )
