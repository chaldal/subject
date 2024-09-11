module LibRouter.Components.LogRouteTransitionsRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard


open LibClient.Components
open LibRouter.Components
open ReactXP.Components
open LibRouter.Components

open LibClient
open LibClient.RenderHelpers

open LibRouter.Components.LogRouteTransitions



let render(children: array<ReactElement>, props: LibRouter.Components.LogRouteTransitions.Props, estate: LibRouter.Components.LogRouteTransitions.Estate, pstate: LibRouter.Components.LogRouteTransitions.Pstate, actions: LibRouter.Components.LogRouteTransitions.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    makeTextNode2 __parentFQN "not used, providing a render function directly"
