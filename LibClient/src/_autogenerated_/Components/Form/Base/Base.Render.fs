module LibClient.Components.Form.BaseRender

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

open LibClient.Components.Form.Base



let render(children: array<ReactElement>, props: LibClient.Components.Form.Base.Props<'Fields, 'Accumulator, 'Accumulated>, estate: LibClient.Components.Form.Base.Estate<'Fields, 'Accumulator, 'Accumulated>, pstate: LibClient.Components.Form.Base.Pstate, actions: LibClient.Components.Form.Base.Actions<'Fields, 'Accumulator, 'Accumulated>, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    match (props.Executor) with
    | Some executor ->
        [|
            props.Content (makeFormHandle executor estate actions)
        |]
    | None ->
        [|
            let __parentFQN = Some "LibClient.Components.Executor.AlertErrors"
            LibClient.Components.Constructors.LC.Executor.AlertErrors(
                ``with`` = (fun executor -> props.Content (makeFormHandle executor estate actions))
            )
        |]
    |> castAsElementAckingKeysWarning
