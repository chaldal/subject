module LibClient.Components.SubscribeRender

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

open LibClient.Components.Subscribe



let render(children: array<ReactElement>, props: LibClient.Components.Subscribe.Props<'T>, estate: LibClient.Components.Subscribe.Estate<'T>, pstate: LibClient.Components.Subscribe.Pstate, actions: LibClient.Components.Subscribe.Actions<'T>, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    match (props.With) with
    | With.WhenAvailable render ->
        [|
            let __parentFQN = Some "LibClient.Components.AsyncData"
            LibClient.Components.Constructors.LC.AsyncData(
                whenAvailable = (render),
                data = (estate.Value)
            )
        |]
    | With.Raw render ->
        [|
            render estate.Value
        |]
    |> castAsElementAckingKeysWarning
