module LibClient.Components.Legacy.Input.PickerRender

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

open LibClient.Components.Legacy.Input.Picker
open LibClient.ContextMenus


let render(children: array<ReactElement>, props: LibClient.Components.Legacy.Input.Picker.Props<'T>, estate: LibClient.Components.Legacy.Input.Picker.Estate<'T>, pstate: LibClient.Components.Legacy.Input.Picker.Pstate, actions: LibClient.Components.Legacy.Input.Picker.Actions<'T>, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "LibClient.Components.With.ScreenSize"
    LibClient.Components.Constructors.LC.With.ScreenSize(
        ``with`` =
            (fun (screenSize) ->
                    (castAsElementAckingKeysWarning [|
                        (
                            let menuItems = estate.ContextMenuItems
                            let __parentFQN = Some "ReactXP.Components.View"
                            let __currClass = (System.String.Format("{0}{1}{2}", "view ", (TopLevelBlockClass), "")) + System.String.Format(" {0}", (if (props.Label.IsSome) then "withLabel" else ""))
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
                                        let __currClass = "border" + System.String.Format(" {0} {1}", (if (props.Validity.IsInvalid) then "is-invalid" else ""), (if (estate.IsFocused) then "focused" else ""))
                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                        ReactXP.Components.Constructors.RX.View(
                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                            children =
                                                [|
                                                    let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                                    let __currClass = "selected-item"
                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                    LibClient.Components.Constructors.LC.LegacyUiText(
                                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                        children =
                                                            [|
                                                                makeTextNode2 __parentFQN (System.String.Format("{0}", estate.SelectedItemLabel))
                                                            |]
                                                    )
                                                    let __parentFQN = Some "LibClient.Components.Icon"
                                                    let __currClass = "icon"
                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                    LibClient.Components.Constructors.LC.Icon(
                                                        icon = (Icon.ChevronDown),
                                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                    )
                                                    let __parentFQN = Some "LibClient.Components.TapCapture"
                                                    LibClient.Components.Constructors.LC.TapCapture(
                                                        onPress = (fun e -> ContextMenu.Open menuItems screenSize e.MaybeSource NoopFn e),
                                                        onPressOut = (actions.OnPressOut),
                                                        onPressIn = (actions.OnPressIn)
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
                                        (
                                            (props.Label)
                                            |> Option.map
                                                (fun label ->
                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                    let __currClass = "label"
                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                    ReactXP.Components.Constructors.RX.View(
                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                                                let __currClass = "label-text" + System.String.Format(" {0}", (if (props.Validity.IsInvalid) then "is-invalid" else ""))
                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                LibClient.Components.Constructors.LC.LegacyUiText(
                                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                    children =
                                                                        [|
                                                                            makeTextNode2 __parentFQN (System.String.Format("{0}", label))
                                                                        |]
                                                                )
                                                            |]
                                                    )
                                                )
                                            |> Option.getOrElse noElement
                                        )
                                    |]
                            )
                        )
                    |])
            )
    )
