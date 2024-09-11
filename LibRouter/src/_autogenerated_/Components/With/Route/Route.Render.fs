module LibRouter.Components.With.RouteRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard


open LibClient.Components
open LibRouter.Components
open ReactXP.Components
open LibRouter.Components

open LibClient
open LibClient.RenderHelpers

open LibRouter.Components.With.Route



let render(children: array<ReactElement>, props: LibRouter.Components.With.Route.Props<'Route, 'ResultlessDialog, 'ResultfulDialog>, estate: LibRouter.Components.With.Route.Estate<'Route, 'ResultlessDialog, 'ResultfulDialog>, pstate: LibRouter.Components.With.Route.Pstate, actions: LibRouter.Components.With.Route.Actions<'Route, 'ResultlessDialog, 'ResultfulDialog>, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    props.With (decodeLocation props.Spec (LibRouter.Components.Router.useLocation()))
