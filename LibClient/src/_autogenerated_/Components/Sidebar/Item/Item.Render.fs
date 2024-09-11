module LibClient.Components.Sidebar.ItemRender

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

open LibClient.Components.Sidebar.Item



let render(children: array<ReactElement>, props: LibClient.Components.Sidebar.Item.Props, estate: LibClient.Components.Sidebar.Item.Estate, pstate: LibClient.Components.Sidebar.Item.Pstate, actions: LibClient.Components.Sidebar.Item.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "LibClient.Components.Pointer.State"
    LibClient.Components.Constructors.LC.Pointer.State(
        content =
            (fun (pointerState: LC.Pointer.State.PointerState) ->
                    (castAsElementAckingKeysWarning [|
                        (
                            let stateClass = "state-" + props.State.Name
                            let isSelected = match props.State with | Selected -> true | _ -> false
                            let selectedClass = if isSelected then "selected" else ""
                            let hoveredClass = if pointerState.IsHovered && (not pointerState.IsDepressed) then "hovered" else ""
                            let depressedClass = if pointerState.IsDepressed then "depressed" else ""
                            let sharedClassSet = (
                                sprintf "%s %s %s %s" stateClass selectedClass hoveredClass depressedClass;
                                    
                            )
                            (castAsElementAckingKeysWarning [|
                                let __parentFQN = Some "ReactXP.Components.View"
                                let __currClass = (System.String.Format("{0}{1}{2}{3}{4}", "item ", (sharedClassSet), " ", (TopLevelBlockClass), ""))
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
                                                (props.LeftIcon)
                                                |> Option.map
                                                    (fun icon ->
                                                        let __parentFQN = Some "ReactXP.Components.View"
                                                        let __currClass = "left"
                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                        ReactXP.Components.Constructors.RX.View(
                                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                            children =
                                                                [|
                                                                    let __parentFQN = Some "LibClient.Components.Icon"
                                                                    let __currClass = (System.String.Format("{0}{1}{2}", "icon-left ", (sharedClassSet), ""))
                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                    LibClient.Components.Constructors.LC.Icon(
                                                                        icon = (icon),
                                                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                                    )
                                                                |]
                                                        )
                                                    )
                                                |> Option.getOrElse noElement
                                            )
                                            let __parentFQN = Some "ReactXP.Components.View"
                                            let __currClass = "middle"
                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                            ReactXP.Components.Constructors.RX.View(
                                                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                children =
                                                    [|
                                                        let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                                        let __currClass = (System.String.Format("{0}{1}{2}", "label ", (sharedClassSet), ""))
                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                        LibClient.Components.Constructors.LC.LegacyUiText(
                                                            selectable = (true),
                                                            ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                            children =
                                                                [|
                                                                    makeTextNode2 __parentFQN (System.String.Format("{0}", props.Label))
                                                                |]
                                                        )
                                                    |]
                                            )
                                            match (props.State) with
                                            | InProgress ->
                                                [|
                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                    let __currClass = "right"
                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                    ReactXP.Components.Constructors.RX.View(
                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "ReactXP.Components.ActivityIndicator"
                                                                ReactXP.Components.Constructors.RX.ActivityIndicator(
                                                                    size = (ReactXP.Components.ActivityIndicator.Small),
                                                                    color = ("#aaaaaa")
                                                                )
                                                            |]
                                                    )
                                                |]
                                            | _ ->
                                                [|
                                                    (
                                                        (props.Right)
                                                        |> Option.map
                                                            (fun right ->
                                                                let __parentFQN = Some "ReactXP.Components.View"
                                                                let __currClass = "right"
                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                ReactXP.Components.Constructors.RX.View(
                                                                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                    children =
                                                                        [|
                                                                            match (right) with
                                                                            | Right.Badge (PositiveInteger count) ->
                                                                                [|
                                                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                                                    let __currClass = (System.String.Format("{0}{1}{2}", "badge ", (sharedClassSet), ""))
                                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                    ReactXP.Components.Constructors.RX.View(
                                                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                                        children =
                                                                                            [|
                                                                                                let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                                                                                let __currClass = (System.String.Format("{0}{1}{2}", "badge-text ", (sharedClassSet), ""))
                                                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                LibClient.Components.Constructors.LC.LegacyUiText(
                                                                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                                                    children =
                                                                                                        [|
                                                                                                            makeTextNode2 __parentFQN (System.String.Format("{0}", if count <= 99 then count.ToString() else "99+"))
                                                                                                        |]
                                                                                                )
                                                                                            |]
                                                                                    )
                                                                                |]
                                                                            | Right.Icon icon ->
                                                                                [|
                                                                                    let __parentFQN = Some "LibClient.Components.Icon"
                                                                                    let __currClass = (System.String.Format("{0}{1}{2}", "icon-right ", (sharedClassSet), ""))
                                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                    LibClient.Components.Constructors.LC.Icon(
                                                                                        icon = (icon),
                                                                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                                                    )
                                                                                |]
                                                                            | _ ->
                                                                                [|
                                                                                    noElement
                                                                                |]
                                                                            |> castAsElementAckingKeysWarning
                                                                        |]
                                                                )
                                                            )
                                                        |> Option.getOrElse noElement
                                                    )
                                                |]
                                            |> castAsElementAckingKeysWarning
                                            (
                                                (match props.State with | State.Actionable onPress -> Some onPress | _ -> None)
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
