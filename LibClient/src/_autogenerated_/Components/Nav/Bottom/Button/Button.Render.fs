module LibClient.Components.Nav.Bottom.ButtonRender

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

open LibClient.Components.Nav.Bottom.Button



let render(children: array<ReactElement>, props: LibClient.Components.Nav.Bottom.Button.Props, estate: LibClient.Components.Nav.Bottom.Button.Estate, pstate: LibClient.Components.Nav.Bottom.Button.Pstate, actions: LibClient.Components.Nav.Bottom.Button.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    match (props.Badge) with
    | Some badge ->
        [|
            let __parentFQN = Some "LibClient.Components.Button"
            let __currClass = (System.String.Format("{0}{1}{2}", "button ", (TopLevelBlockClass), ""))
            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
            LibClient.Components.Constructors.LC.Button(
                state = (props.State),
                badge = (badge),
                icon = (props.Icon),
                level = (props.Level),
                label = (props.Label),
                ?contentContainerStyles = (props.contentContainerStyles),
                ?styles = (props.styles),
                ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
            )
        |]
    | None ->
        [|
            let __parentFQN = Some "LibClient.Components.Button"
            let __currClass = (System.String.Format("{0}{1}{2}", "button ", (TopLevelBlockClass), ""))
            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
            LibClient.Components.Constructors.LC.Button(
                state = (props.State),
                icon = (props.Icon),
                level = (props.Level),
                label = (props.Label),
                ?contentContainerStyles = (props.contentContainerStyles),
                ?styles = (props.styles),
                ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
            )
        |]
    |> castAsElementAckingKeysWarning
