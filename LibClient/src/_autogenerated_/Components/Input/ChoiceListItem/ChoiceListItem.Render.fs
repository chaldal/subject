module LibClient.Components.Input.ChoiceListItemRender

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

open LibClient.Components.Input.ChoiceListItem



let render(children: array<ReactElement>, props: LibClient.Components.Input.ChoiceListItem.Props<'T>, estate: LibClient.Components.Input.ChoiceListItem.Estate<'T>, pstate: LibClient.Components.Input.ChoiceListItem.Pstate, actions: LibClient.Components.Input.ChoiceListItem.Actions<'T>, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    (castAsElementAckingKeysWarning [|
        let icon = (
                (castAsElementAckingKeysWarning [|
                    match (props.Group.IsSelected props.Value) with
                    | true ->
                        [|
                            let __parentFQN = Some "LibClient.Components.Icon"
                            let __currClass = "icon checked"
                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                            LibClient.Components.Constructors.LC.Icon(
                                icon = (match props.Group.Value with AtLeastOne _ | Any _ -> Icon.CheckboxChecked | AtMostOne _ | ExactlyOne _ -> Icon.RadioButtonFilled),
                                ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                            )
                        |]
                    | false ->
                        [|
                            let __parentFQN = Some "LibClient.Components.Icon"
                            let __currClass = "icon unchecked"
                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                            LibClient.Components.Constructors.LC.Icon(
                                icon = (match props.Group.Value with AtLeastOne _ | Any _ -> Icon.CheckboxEmpty | AtMostOne _ | ExactlyOne _ -> Icon.RadioButtonEmpty),
                                ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                            )
                        |]
                    |> castAsElementAckingKeysWarning
                |])
        )
        let __parentFQN = Some "ReactXP.Components.View"
        let __currClass = (System.String.Format("{0}{1}{2}", "view ", (TopLevelBlockClass), ""))
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
                    (
                        if (props.IconPosition = IconPosition.Left) then
                            (castAsElementAckingKeysWarning [|
                                icon
                            |])
                        else noElement
                    )
                    match (props.Label) with
                    | Children ->
                        [|
                            let __parentFQN = Some "ReactXP.Components.View"
                            let __currClass = (System.String.Format("{0}{1}{2}", "label icon-position-", (props.IconPosition), ""))
                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                            ReactXP.Components.Constructors.RX.View(
                                children = (children),
                                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None)
                            )
                        |]
                    | String label ->
                        [|
                            let __parentFQN = Some "LibClient.Components.LegacyUiText"
                            let __currClass = (System.String.Format("{0}{1}{2}", "label icon-position-", (props.IconPosition), ""))
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
                    (
                        if (props.IconPosition = IconPosition.Right) then
                            (castAsElementAckingKeysWarning [|
                                icon
                            |])
                        else noElement
                    )
                    let __parentFQN = Some "LibClient.Components.TapCapture"
                    LibClient.Components.Constructors.LC.TapCapture(
                        onPress = (props.Group.Toggle props.Value)
                    )
                |]
        )
    |])
