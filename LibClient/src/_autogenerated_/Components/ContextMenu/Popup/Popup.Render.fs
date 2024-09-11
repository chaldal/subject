module LibClient.Components.ContextMenu.PopupRender

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

open LibClient.Components.ContextMenu.Popup
open LibClient.ContextMenus.Types


let render(children: array<ReactElement>, props: LibClient.Components.ContextMenu.Popup.Props, estate: LibClient.Components.ContextMenu.Popup.Estate, pstate: LibClient.Components.ContextMenu.Popup.Pstate, actions: LibClient.Components.ContextMenu.Popup.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "ReactXP.Components.ScrollView"
    let __currClass = "scroll-view"
    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
    ReactXP.Components.Constructors.RX.ScrollView(
        vertical = (true),
        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.ScrollView" __currStyles |> Some) else None),
        children =
            [|
                let __parentFQN = Some "ReactXP.Components.View"
                let __currClass = "view"
                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                ReactXP.Components.Constructors.RX.View(
                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                    children =
                        [|
                            (
                                (props.Items)
                                |> Seq.mapi
                                    (fun index item ->
                                        (castAsElementAckingKeysWarning [|
                                            match (item) with
                                            | Divider ->
                                                [|
                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                    let __currClass = "divider"
                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                    ReactXP.Components.Constructors.RX.View(
                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None)
                                                    )
                                                |]
                                            | Heading text ->
                                                [|
                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                    let __currClass = "heading"
                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                    ReactXP.Components.Constructors.RX.View(
                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "LibClient.Components.LegacyText"
                                                                LibClient.Components.Constructors.LC.LegacyText(
                                                                    children =
                                                                        [|
                                                                            makeTextNode2 __parentFQN (System.String.Format("{0}", text))
                                                                        |]
                                                                )
                                                            |]
                                                    )
                                                |]
                                            | InternalButton (label, isSelected, onPress) ->
                                                [|
                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                    let __currClass = "button" + System.String.Format(" {0} {1}", (if (index = 0) then "first" else ""), (if (isSelected) then "selected" else ""))
                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                    ReactXP.Components.Constructors.RX.View(
                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                                                let __currClass = "button-text" + System.String.Format(" {0}", (if (isSelected) then "selected" else ""))
                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                LibClient.Components.Constructors.LC.LegacyUiText(
                                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                    children =
                                                                        [|
                                                                            makeTextNode2 __parentFQN (System.String.Format("{0}", label))
                                                                        |]
                                                                )
                                                                let __parentFQN = Some "LibClient.Components.TapCapture"
                                                                LibClient.Components.Constructors.LC.TapCapture(
                                                                    onPress = (fun _ -> props.Hide(); onPress props.OpeningEvent)
                                                                )
                                                            |]
                                                    )
                                                |]
                                            | ButtonCautionary (label, onPress) ->
                                                [|
                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                    let __currClass = "button button-cautionary" + System.String.Format(" {0}", (if (index = 0) then "first" else ""))
                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                    ReactXP.Components.Constructors.RX.View(
                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                                                let __currClass = "button-text button-cautionary"
                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                LibClient.Components.Constructors.LC.LegacyUiText(
                                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                    children =
                                                                        [|
                                                                            makeTextNode2 __parentFQN (System.String.Format("{0}", label))
                                                                        |]
                                                                )
                                                                let __parentFQN = Some "LibClient.Components.TapCapture"
                                                                LibClient.Components.Constructors.LC.TapCapture(
                                                                    onPress = (fun _ -> props.Hide(); onPress props.OpeningEvent)
                                                                )
                                                            |]
                                                    )
                                                |]
                                            |> castAsElementAckingKeysWarning
                                        |])
                                    )
                                |> Array.ofSeq |> castAsElement
                            )
                        |]
                )
            |]
    )
