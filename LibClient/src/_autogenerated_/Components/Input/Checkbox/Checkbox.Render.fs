module LibClient.Components.Input.CheckboxRender

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

open LibClient.Components.Input.Checkbox



let render(children: array<ReactElement>, props: LibClient.Components.Input.Checkbox.Props, estate: LibClient.Components.Input.Checkbox.Estate, pstate: LibClient.Components.Input.Checkbox.Pstate, actions: LibClient.Components.Input.Checkbox.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "ReactXP.Components.View"
    let __currClass = (System.String.Format("{0}{1}{2}", "", (TopLevelBlockClass), " top-level-div"))
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
                let __parentFQN = Some "ReactXP.Components.View"
                let __currClass = "div"
                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                ReactXP.Components.Constructors.RX.View(
                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                    children =
                        [|
                            match (props.Value) with
                            | Some true ->
                                [|
                                    let __parentFQN = Some "LibClient.Components.Icon"
                                    let __currClass = "icon" + System.String.Format(" {0} {1}", (if (props.Validity.IsInvalid) then "checkbox-invalid" else ""), (if (not props.Validity.IsInvalid) then "checked" else ""))
                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                    LibClient.Components.Constructors.LC.Icon(
                                        icon = (Icon.CheckboxChecked),
                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                    )
                                |]
                            | Some false ->
                                [|
                                    let __parentFQN = Some "LibClient.Components.Icon"
                                    let __currClass = "icon" + System.String.Format(" {0} {1}", (if (props.Validity.IsInvalid) then "checkbox-invalid" else ""), (if (not props.Validity.IsInvalid) then "unchecked" else ""))
                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                    LibClient.Components.Constructors.LC.Icon(
                                        icon = (Icon.CheckboxEmpty),
                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                    )
                                |]
                            | None ->
                                [|
                                    let __parentFQN = Some "LibClient.Components.Icon"
                                    let __currClass = "icon" + System.String.Format(" {0} {1}", (if (props.Validity.IsInvalid) then "checkbox-invalid" else ""), (if (not props.Validity.IsInvalid) then "unchecked" else ""))
                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                    LibClient.Components.Constructors.LC.Icon(
                                        icon = (Icon.CheckboxUnknown),
                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                    )
                                |]
                            |> castAsElementAckingKeysWarning
                            match (props.Label) with
                            | Children ->
                                [|
                                    castAsElement children
                                |]
                            | String label ->
                                [|
                                    let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                    let __currClass = "label"
                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                    LibClient.Components.Constructors.LC.LegacyUiText(
                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                        children =
                                            [|
                                                makeTextNode2 __parentFQN (System.String.Format("{0}", label))
                                            |]
                                    )
                                |]
                            |> castAsElementAckingKeysWarning
                            let __parentFQN = Some "LibClient.Components.TapCapture"
                            LibClient.Components.Constructors.LC.TapCapture(
                                onPress = (actions.OnPress),
                                styles = ([| CheckboxStyles.Styles.tapCapture |])
                            )
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
            |]
    )
