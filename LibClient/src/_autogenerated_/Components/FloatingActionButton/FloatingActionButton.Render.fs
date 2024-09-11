module LibClient.Components.FloatingActionButtonRender

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

open LibClient.Components.FloatingActionButton



let render(children: array<ReactElement>, props: LibClient.Components.FloatingActionButton.Props, estate: LibClient.Components.FloatingActionButton.Estate, pstate: LibClient.Components.FloatingActionButton.Pstate, actions: LibClient.Components.FloatingActionButton.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    (
        let lowLevelState = props.State.ToLowLevel
        let __parentFQN = Some "LibClient.Components.Pointer.State"
        LibClient.Components.Constructors.LC.Pointer.State(
            content =
                (fun (pointerState: LC.Pointer.State.PointerState) ->
                        (castAsElementAckingKeysWarning [|
                            (
                                let isInProgress = match lowLevelState with InProgress -> true | _ -> false
                                let __parentFQN = Some "ReactXP.Components.View"
                                let __currClass = (System.String.Format("{0}{1}{2}{3}{4}", "view ", (TopLevelBlockClass), " state-", (lowLevelState.GetName), "")) + System.String.Format(" {0} {1} {2}", (if (pointerState.IsHovered && (not pointerState.IsDepressed)) then "is-hovered" else ""), (if (pointerState.IsDepressed) then "is-depressed" else ""), (if (props.Label.IsSome) then "with-label" else ""))
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
                                            let __parentFQN = Some "LibClient.Components.Icon"
                                            let __currClass = (System.String.Format("{0}{1}{2}", "icon state-", (lowLevelState.GetName), ""))
                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                            LibClient.Components.Constructors.LC.Icon(
                                                icon = (props.Icon),
                                                ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
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
                                                                    let __parentFQN = Some "LibClient.Components.LegacyText"
                                                                    let __currClass = (System.String.Format("{0}{1}{2}", "label-text state-", (lowLevelState.GetName), ""))
                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                    LibClient.Components.Constructors.LC.LegacyText(
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
                                            (
                                                if (isInProgress) then
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
                                                                    color = ("#ffffff")
                                                                )
                                                            |]
                                                    )
                                                else noElement
                                            )
                                            (
                                                (match lowLevelState with Actionable onPress -> Some onPress | _ -> None)
                                                |> Option.map
                                                    (fun onPress ->
                                                        let __parentFQN = Some "LibClient.Components.TapCapture"
                                                        LibClient.Components.Constructors.LC.TapCapture(
                                                            styles = ([| FloatingActionButtonStyles.Styles.tapCapture |]),
                                                            pointerState = (pointerState),
                                                            onPress = (onPress)
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
    )
