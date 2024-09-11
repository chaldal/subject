module LibClient.Components.ContextMenu.DialogRender

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

open LibClient.Components.ContextMenu.Dialog
open LibClient
open LibClient.ContextMenus.Types


let render(children: array<ReactElement>, props: LibClient.Components.ContextMenu.Dialog.Props, estate: LibClient.Components.ContextMenu.Dialog.Estate, pstate: LibClient.Components.ContextMenu.Dialog.Pstate, actions: LibClient.Components.ContextMenu.Dialog.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "LibClient.Components.Dialog.Base"
    LibClient.Components.Constructors.LC.Dialog.Base(
        canClose = (LibClient.Components.Dialog.Base.When ([LibClient.Components.Dialog.Base.OnEscape; LibClient.Components.Dialog.Base.OnBackground], actions.TryCancel)),
        contentPosition = (LibClient.Components.Dialog.Base.Free),
        children =
            [|
                let __parentFQN = Some "ReactXP.Components.View"
                let __currClass = "dialog-contents"
                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                ReactXP.Components.Constructors.RX.View(
                    onPress = (fun e -> (e.stopPropagation(); actions.TryCancel (ReactEvent.Action.OfBrowserEvent e))),
                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                    children =
                        [|
                            let __parentFQN = Some "ReactXP.Components.ScrollView"
                            let __currClass = "scroll-view"
                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                            ReactXP.Components.Constructors.RX.ScrollView(
                                vertical = (true),
                                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.ScrollView" __currStyles |> Some) else None),
                                children =
                                    [|
                                        (
                                            (props.Parameters.Items)
                                            |> Seq.map
                                                (fun item ->
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
                                                                ReactXP.Components.Constructors.RX.View(
                                                                    children =
                                                                        [|
                                                                            let __parentFQN = Some "LibClient.Components.LegacyText"
                                                                            let __currClass = "heading"
                                                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                            LibClient.Components.Constructors.LC.LegacyText(
                                                                                ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
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
                                                                let __parentFQN = Some "LibClient.Components.Button"
                                                                let __currClass = "button button-normal" + System.String.Format(" {0}", (if (isSelected) then "selected" else ""))
                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                LibClient.Components.Constructors.LC.Button(
                                                                    state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (LibClient.Components.Button.Actionable (fun e -> (actions.TryCancel e; onPress e)))),
                                                                    label = (label),
                                                                    level = (LibClient.Components.Button.Primary),
                                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                                )
                                                            |]
                                                        | ButtonCautionary (label, onPress) ->
                                                            [|
                                                                let __parentFQN = Some "LibClient.Components.Button"
                                                                let __currClass = "button button-cautionary"
                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                LibClient.Components.Constructors.LC.Button(
                                                                    state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (LibClient.Components.Button.Actionable (fun e -> (actions.TryCancel e; onPress e)))),
                                                                    label = (label),
                                                                    level = (LibClient.Components.Button.Cautionary),
                                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
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
            |]
    )
