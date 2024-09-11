module LibRouter.Components.With.NavigationRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard

open LibClient.RenderResultModule

open LibClient.Components
open LibRouter.Components
open ReactXP.Components
open LibRouter.Components

open LibClient
open LibClient.RenderHelpers

open LibRouter.Components.With.Navigation



let render(props: LibRouter.Components.With.Navigation.Props<'Route, 'ResultlessDialog, 'ResultfulDialog>, estate: LibRouter.Components.With.Navigation.Estate<'Route, 'ResultlessDialog, 'ResultfulDialog>, pstate: LibRouter.Components.With.Navigation.Pstate, actions: LibRouter.Components.With.Navigation.Actions<'Route, 'ResultlessDialog, 'ResultfulDialog>, __componentStyles: ReactXP.Styles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.Styles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    ((makeTextNode "not used, providing a render function directly") |> RenderResult.Make)
    |> RenderResult.ToRawElementsSingle __parentFQN |> RenderResult.ToSingleElement