module LibClient.Components.Nav.Bottom.ItemRender

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

open LibClient.Components.Nav.Bottom.Item



let render(children: array<ReactElement>, props: LibClient.Components.Nav.Bottom.Item.Props, estate: LibClient.Components.Nav.Bottom.Item.Estate, pstate: LibClient.Components.Nav.Bottom.Item.Pstate, actions: LibClient.Components.Nav.Bottom.Item.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
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
                        let __parentFQN = Some "LibClient.Components.Pointer.State"
                        LibClient.Components.Constructors.LC.Pointer.State(
                            content =
                                (fun (pointerState: LC.Pointer.State.PointerState) ->
                                        (castAsElementAckingKeysWarning [|
                                            (
                                                let stateClass = "state-" + props.State.Name
                                                let isSelected = match props.State with Selected | SelectedActionable _ -> true | _ -> false
                                                let selectedClass = if isSelected then "selected" else ""
                                                let hoveredClass = if pointerState.IsHovered && (not pointerState.IsDepressed) then "hovered" else ""
                                                let depressedClass = if pointerState.IsDepressed then "depressed" else ""
                                                let sharedClassSet = (
                                                    sprintf "%s %s %s %s" stateClass selectedClass hoveredClass depressedClass;
                                                            
                                                )
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                    let __currClass = (System.String.Format("{0}{1}{2}{3}{4}{5}{6}", "item ", (sharedClassSet), " ", (TopLevelBlockClass), " ", (screenSize.Class), ""))
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
                                                                match (props.Style) with
                                                                | Style.Internal (Some label, Some icon, labelPosition, Some badge) ->
                                                                    [|
                                                                        let __parentFQN = Some "ReactXP.Components.View"
                                                                        let __currClass = (System.String.Format("{0}{1}{2}", "item-content-container item-content-container-with-badge ", (labelPosition), ""))
                                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                        ReactXP.Components.Constructors.RX.View(
                                                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                            children =
                                                                                [|
                                                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                                                    let __currClass = "adjust-icon-vertical-position"
                                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                    ReactXP.Components.Constructors.RX.View(
                                                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                                        children =
                                                                                            [|
                                                                                                let __parentFQN = Some "LibClient.Components.Icon"
                                                                                                let __currClass = (System.String.Format("{0}{1}{2}{3}{4}", "icon ", (sharedClassSet), " ", (screenSize.Class), ""))
                                                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                LibClient.Components.Constructors.LC.Icon(
                                                                                                    icon = (icon),
                                                                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                                                                )
                                                                                            |]
                                                                                    )
                                                                                    let __parentFQN = Some "LibClient.Components.Badge"
                                                                                    let __currClass = (System.String.Format("{0}{1}{2}", "badge ", (screenSize.Class), ""))
                                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                    LibClient.Components.Constructors.LC.Badge(
                                                                                        badge = (badge),
                                                                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                                                    )
                                                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                                                    let __currClass = "label-content label-content-with-icon-badge"
                                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                    ReactXP.Components.Constructors.RX.View(
                                                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                                        children =
                                                                                            [|
                                                                                                let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                                                                                let __currClass = (System.String.Format("{0}{1}{2}{3}{4}", "", (sharedClassSet), " ", (screenSize.Class), " label"))
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
                                                                                |]
                                                                        )
                                                                    |]
                                                                | Style.Internal (Some label, Some icon, labelPosition, None) ->
                                                                    [|
                                                                        let __parentFQN = Some "ReactXP.Components.View"
                                                                        let __currClass = (System.String.Format("{0}{1}{2}", "item-content-container ", (labelPosition), ""))
                                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                        ReactXP.Components.Constructors.RX.View(
                                                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                            children =
                                                                                [|
                                                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                                                    let __currClass = "adjust-icon-vertical-position"
                                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                    ReactXP.Components.Constructors.RX.View(
                                                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                                        children =
                                                                                            [|
                                                                                                let __parentFQN = Some "LibClient.Components.Icon"
                                                                                                let __currClass = (System.String.Format("{0}{1}{2}{3}{4}", "icon ", (sharedClassSet), " ", (screenSize.Class), ""))
                                                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                LibClient.Components.Constructors.LC.Icon(
                                                                                                    icon = (icon),
                                                                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                                                                )
                                                                                            |]
                                                                                    )
                                                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                                                    let __currClass = "label-content"
                                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                    ReactXP.Components.Constructors.RX.View(
                                                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                                        children =
                                                                                            [|
                                                                                                let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                                                                                let __currClass = (System.String.Format("{0}{1}{2}{3}{4}", "", (sharedClassSet), " ", (screenSize.Class), " label"))
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
                                                                                |]
                                                                        )
                                                                    |]
                                                                | Style.Internal (None, Some icon, _, Some badge) ->
                                                                    [|
                                                                        let __parentFQN = Some "ReactXP.Components.View"
                                                                        let __currClass = "item-content-container item-content-container-with-badge"
                                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                        ReactXP.Components.Constructors.RX.View(
                                                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                            children =
                                                                                [|
                                                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                                                    let __currClass = "adjust-icon-vertical-position"
                                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                    ReactXP.Components.Constructors.RX.View(
                                                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                                        children =
                                                                                            [|
                                                                                                let __parentFQN = Some "LibClient.Components.Icon"
                                                                                                let __currClass = (System.String.Format("{0}{1}{2}{3}{4}", "icon ", (sharedClassSet), " ", (screenSize.Class), ""))
                                                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                LibClient.Components.Constructors.LC.Icon(
                                                                                                    icon = (icon),
                                                                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                                                                )
                                                                                            |]
                                                                                    )
                                                                                    let __parentFQN = Some "LibClient.Components.Badge"
                                                                                    let __currClass = (System.String.Format("{0}{1}{2}", "badge ", (screenSize.Class), ""))
                                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                    LibClient.Components.Constructors.LC.Badge(
                                                                                        badge = (badge),
                                                                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                                                    )
                                                                                |]
                                                                        )
                                                                    |]
                                                                | Style.Internal (Some label, None, _, Some badge) ->
                                                                    [|
                                                                        let __parentFQN = Some "ReactXP.Components.View"
                                                                        let __currClass = "item-content-container item-content-container-with-badge"
                                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                        ReactXP.Components.Constructors.RX.View(
                                                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                            children =
                                                                                [|
                                                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                                                    let __currClass = "label-content"
                                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                    ReactXP.Components.Constructors.RX.View(
                                                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                                        children =
                                                                                            [|
                                                                                                let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                                                                                let __currClass = (System.String.Format("{0}{1}{2}{3}{4}", "", (sharedClassSet), " ", (screenSize.Class), " label"))
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
                                                                                    let __parentFQN = Some "LibClient.Components.Badge"
                                                                                    let __currClass = (System.String.Format("{0}{1}{2}", "badge ", (screenSize.Class), ""))
                                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                    LibClient.Components.Constructors.LC.Badge(
                                                                                        badge = (badge),
                                                                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                                                    )
                                                                                |]
                                                                        )
                                                                    |]
                                                                | Style.Internal (Some label, None, _, None) ->
                                                                    [|
                                                                        let __parentFQN = Some "ReactXP.Components.View"
                                                                        let __currClass = "item-content-container"
                                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                        ReactXP.Components.Constructors.RX.View(
                                                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                            children =
                                                                                [|
                                                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                                                    let __currClass = "label-content"
                                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                    ReactXP.Components.Constructors.RX.View(
                                                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                                        children =
                                                                                            [|
                                                                                                let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                                                                                let __currClass = (System.String.Format("{0}{1}{2}{3}{4}", "", (sharedClassSet), " ", (screenSize.Class), " label"))
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
                                                                                |]
                                                                        )
                                                                    |]
                                                                | Style.Internal (None, Some icon, _, None) ->
                                                                    [|
                                                                        let __parentFQN = Some "ReactXP.Components.View"
                                                                        let __currClass = "item-content-container"
                                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                        ReactXP.Components.Constructors.RX.View(
                                                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                            children =
                                                                                [|
                                                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                                                    let __currClass = "adjust-icon-vertical-position"
                                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                    ReactXP.Components.Constructors.RX.View(
                                                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                                        children =
                                                                                            [|
                                                                                                let __parentFQN = Some "LibClient.Components.Icon"
                                                                                                let __currClass = (System.String.Format("{0}{1}{2}{3}{4}", "icon ", (sharedClassSet), " ", (screenSize.Class), ""))
                                                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                LibClient.Components.Constructors.LC.Icon(
                                                                                                    icon = (icon),
                                                                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                                                                )
                                                                                            |]
                                                                                    )
                                                                                |]
                                                                        )
                                                                    |]
                                                                | _ ->
                                                                    [|
                                                                        let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                                                        LibClient.Components.Constructors.LC.LegacyUiText(
                                                                            children =
                                                                                [|
                                                                                    makeTextNode2 __parentFQN "combination not supported"
                                                                                |]
                                                                        )
                                                                    |]
                                                                |> castAsElementAckingKeysWarning
                                                                (
                                                                    (match props.State with | State.Actionable onPress | State.SelectedActionable onPress -> Some onPress | _ -> None)
                                                                    |> Option.map
                                                                        (fun onPress ->
                                                                            let __parentFQN = Some "LibClient.Components.TapCapture"
                                                                            LibClient.Components.Constructors.LC.TapCapture(
                                                                                pointerState = (pointerState),
                                                                                onPress = (onPress)
                                                                            )
                                                                        )
                                                                    |> Option.getOrElse noElement
                                                                )
                                                            |]
                                                    )
                                                |])
                                            )
                                        |])
                                )
                        )
                    |])
            )
    )
