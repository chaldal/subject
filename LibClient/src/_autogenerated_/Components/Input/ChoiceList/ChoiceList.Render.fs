module LibClient.Components.Input.ChoiceListRender

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

open LibClient.Components.Input.ChoiceList



let render(children: array<ReactElement>, props: LibClient.Components.Input.ChoiceList.Props<'T>, estate: LibClient.Components.Input.ChoiceList.Estate<'T>, pstate: LibClient.Components.Input.ChoiceList.Pstate, actions: LibClient.Components.Input.ChoiceList.Actions<'T>, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    (castAsElementAckingKeysWarning [|
        let __parentFQN = Some "ReactXP.Components.View"
        let __currClass = (System.String.Format("{0}", TopLevelBlockClass))
        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
        ReactXP.Components.Constructors.RX.View(
            ?styles =
                (
                    let __currProcessedStyles = if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "%s" __currStyles) else [||]
                    match props.styles with
                    | Some styles ->
                        Array.append __currProcessedStyles styles |> Some
                    | None -> Some __currProcessedStyles
                ),
            children =
                [|
                    props.Items estate.Group
                |]
        )
        (
            (props.Validity.InvalidReason)
            |> Option.map
                (fun reason ->
                    let __parentFQN = Some "ReactXP.Components.View"
                    ReactXP.Components.Constructors.RX.View(
                        children =
                            [|
                                let __parentFQN = Some "LibClient.Components.LegacyText"
                                let __currClass = "invalid-reason"
                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                LibClient.Components.Constructors.LC.LegacyText(
                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                    children =
                                        [|
                                            makeTextNode2 __parentFQN (System.String.Format("{0}", reason))
                                        |]
                                )
                            |]
                    )
                )
            |> Option.getOrElse noElement
        )
    |])
