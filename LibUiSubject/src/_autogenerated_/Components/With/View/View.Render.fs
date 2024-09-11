module LibUiSubject.Components.With.ViewRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard


open LibClient.Components
open ReactXP.Components
open LibUiSubject.Components
open LibUiSubject.Components

open LibClient
open LibClient.RenderHelpers

open LibUiSubject.Components.With.View



let render(children: array<ReactElement>, props: LibUiSubject.Components.With.View.Props<'Input, 'Output, 'OpError>, estate: LibUiSubject.Components.With.View.Estate<'Input, 'Output, 'OpError>, pstate: LibUiSubject.Components.With.View.Pstate, actions: LibUiSubject.Components.With.View.Actions<'Input, 'Output, 'OpError>, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    match (props.With) with
    | WithView.WhenAvailable render ->
        [|
            let __parentFQN = Some "LibClient.Components.AsyncData"
            LibClient.Components.Constructors.LC.AsyncData(
                whenAvailable = (render),
                data = (estate.Value)
            )
        |]
    | WithView.Raw render ->
        [|
            render estate.Value
        |]
    |> castAsElementAckingKeysWarning
