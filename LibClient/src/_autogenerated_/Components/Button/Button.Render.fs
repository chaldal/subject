module LibClient.Components.ButtonRender

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

open LibClient.Components.Button



let render(children: array<ReactElement>, props: LibClient.Components.Button.Props, estate: LibClient.Components.Button.Estate, pstate: LibClient.Components.Button.Pstate, actions: LibClient.Components.Button.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    (
        let state = props.State.ToLowLevel
        let levelAndStateClass = (
            "level-" + props.Level.ToString() + " state-" + state.GetName
             
        )
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
                                                let __parentFQN = Some "ReactXP.Components.View"
                                                let __currClass = (System.String.Format("{0}{1}{2}{3}{4}{5}{6}", "view ", (TopLevelBlockClass), " ", (levelAndStateClass), " ", (screenSize.Class), "")) + System.String.Format(" {0} {1} {2}", (if (props.Level <> Tertiary) then "non-tertiary" else ""), (if (pointerState.IsHovered && (not pointerState.IsDepressed)) then "is-hovered" else ""), (if (pointerState.IsDepressed) then "is-depressed" else ""))
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
                                                            let __currClass = "label-block"
                                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                            ReactXP.Components.Constructors.RX.View(
                                                                ?styles =
                                                                    (
                                                                        let __currProcessedStyles = if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "%s" __currStyles) else [||]
                                                                        match props.contentContainerStyles with
                                                                        | Some styles ->
                                                                            Array.append __currProcessedStyles styles |> Some
                                                                        | None -> Some __currProcessedStyles
                                                                    ),
                                                                children =
                                                                    [|
                                                                        (
                                                                            (props.Icon.LeftOption)
                                                                            |> Option.map
                                                                                (fun (leftIcon) ->
                                                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                                                    let __currClass = "left-icon"
                                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                    ReactXP.Components.Constructors.RX.View(
                                                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                                        children =
                                                                                            [|
                                                                                                let __parentFQN = Some "LibClient.Components.Icon"
                                                                                                let __currClass = (System.String.Format("{0}{1}{2}", "icon ", (levelAndStateClass), ""))
                                                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                LibClient.Components.Constructors.LC.Icon(
                                                                                                    icon = (leftIcon),
                                                                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                                                                )
                                                                                            |]
                                                                                    )
                                                                                )
                                                                            |> Option.getOrElse noElement
                                                                        )
                                                                        let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                                                        let __currClass = (System.String.Format("{0}{1}{2}{3}{4}", "label-text ", (levelAndStateClass), " ", (screenSize.Class), ""))
                                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                        LibClient.Components.Constructors.LC.LegacyUiText(
                                                                            numberOfLines = (1),
                                                                            ellipsizeMode = (EllipsizeMode.Tail),
                                                                            ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                            children =
                                                                                [|
                                                                                    makeTextNode2 __parentFQN (System.String.Format("{0}", props.Label))
                                                                                |]
                                                                        )
                                                                        (
                                                                            (props.Icon.RightOption)
                                                                            |> Option.map
                                                                                (fun (rightIcon) ->
                                                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                                                    let __currClass = "right-icon"
                                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                    ReactXP.Components.Constructors.RX.View(
                                                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                                        children =
                                                                                            [|
                                                                                                let __parentFQN = Some "LibClient.Components.Icon"
                                                                                                let __currClass = (System.String.Format("{0}{1}{2}", "icon ", (levelAndStateClass), ""))
                                                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                LibClient.Components.Constructors.LC.Icon(
                                                                                                    icon = (rightIcon),
                                                                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                                                                )
                                                                                            |]
                                                                                    )
                                                                                )
                                                                            |> Option.getOrElse noElement
                                                                        )
                                                                        (
                                                                            (props.Badge)
                                                                            |> Option.map
                                                                                (fun badge ->
                                                                                    let __parentFQN = Some "LibClient.Components.Badge"
                                                                                    let __currClass = (System.String.Format("{0}{1}{2}", "badge ", (screenSize.Class), ""))
                                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                    LibClient.Components.Constructors.LC.Badge(
                                                                                        badge = (badge),
                                                                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                                                    )
                                                                                )
                                                                            |> Option.getOrElse noElement
                                                                        )
                                                                    |]
                                                            )
                                                            (
                                                                if (match state with | InProgress -> true | _ -> false) then
                                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                                    let __currClass = "spinner-block"
                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                    ReactXP.Components.Constructors.RX.View(
                                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                        children =
                                                                            [|
                                                                                let __parentFQN = Some "ReactXP.Components.ActivityIndicator"
                                                                                ReactXP.Components.Constructors.RX.ActivityIndicator(
                                                                                    size = (ReactXP.Components.ActivityIndicator.Tiny),
                                                                                    color = ("#aaaaaa")
                                                                                )
                                                                            |]
                                                                    )
                                                                else noElement
                                                            )
                                                            (
                                                                (match state with | Actionable onPress -> Some onPress | _ -> None)
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
                            )
                        |])
                )
        )
    )
