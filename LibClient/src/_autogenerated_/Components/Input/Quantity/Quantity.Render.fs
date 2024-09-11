module LibClient.Components.Input.QuantityRender

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

open LibClient.Components.Input.Quantity



let render(children: array<ReactElement>, props: LibClient.Components.Input.Quantity.Props, estate: LibClient.Components.Input.Quantity.Estate, pstate: LibClient.Components.Input.Quantity.Pstate, actions: LibClient.Components.Input.Quantity.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    (
        let isInvalid = props.Validity.IsInvalid
        let __parentFQN = Some "ReactXP.Components.View"
        let __currClass = "view"
        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
        ReactXP.Components.Constructors.RX.View(
            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
            children =
                [|
                    let __parentFQN = Some "ReactXP.Components.View"
                    let __currClass = "field" + System.String.Format(" {0} {1}", (if (isInvalid) then "invalid" else ""), (if (not isInvalid) then "valid" else ""))
                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                    ReactXP.Components.Constructors.RX.View(
                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                        children =
                            [|
                                let __parentFQN = Some "ReactXP.Components.View"
                                let __currClass = "side"
                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                ReactXP.Components.Constructors.RX.View(
                                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                    children =
                                        [|
                                            (
                                                (props.Value)
                                                |> Option.map
                                                    (fun quantity ->
                                                        (castAsElementAckingKeysWarning [|
                                                            match ((quantity - 1, props.OnChange)) with
                                                            | (None, CanRemove onChange) ->
                                                                [|
                                                                    let __parentFQN = Some "LibClient.Components.IconButton"
                                                                    let __currClass = "icon-button quantity-none" + System.String.Format(" {0} {1}", (if (isInvalid) then "invalid" else ""), (if (not isInvalid) then "valid" else ""))
                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                    LibClient.Components.Constructors.LC.IconButton(
                                                                        state = (LibClient.Components.IconButton.PropStateFactory.MakeLowLevel(LibClient.Components.IconButton.Actionable (onChange None))),
                                                                        icon = (Icon.GarbageBin),
                                                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                                    )
                                                                |]
                                                            | (None, CannotRemove _) ->
                                                                [||]
                                                            | (Some decremented, onChange) ->
                                                                [|
                                                                    let __parentFQN = Some "LibClient.Components.IconButton"
                                                                    let __currClass = "icon-button" + System.String.Format(" {0} {1}", (if (isInvalid) then "invalid" else ""), (if (not isInvalid) then "valid" else ""))
                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                    LibClient.Components.Constructors.LC.IconButton(
                                                                        state = (LibClient.Components.IconButton.PropStateFactory.MakeLowLevel(LibClient.Components.IconButton.Actionable (onChange.Call decremented))),
                                                                        icon = (Icon.Minus),
                                                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                                    )
                                                                |]
                                                            |> castAsElementAckingKeysWarning
                                                        |])
                                                    )
                                                |> Option.getOrElse noElement
                                            )
                                        |]
                                )
                                let __parentFQN = Some "ReactXP.Components.View"
                                let __currClass = "center"
                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                ReactXP.Components.Constructors.RX.View(
                                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                    children =
                                        [|
                                            (
                                                (props.Value)
                                                |> Option.map
                                                    (fun value ->
                                                        let __parentFQN = Some "LibClient.Components.LegacyText"
                                                        let __currClass = "center-text" + System.String.Format(" {0} {1}", (if (isInvalid) then "invalid" else ""), (if (not isInvalid) then "valid" else ""))
                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                        LibClient.Components.Constructors.LC.LegacyText(
                                                            ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                            children =
                                                                [|
                                                                    makeTextNode2 __parentFQN (System.String.Format("{0}", value.Value))
                                                                |]
                                                        )
                                                    )
                                                |> Option.getOrElse noElement
                                            )
                                        |]
                                )
                                (
                                    let incrementedUnchecked = props.Value |> Option.mapOrElse PositiveInteger.One (fun q -> q + 1u)
                                    let __parentFQN = Some "ReactXP.Components.View"
                                    let __currClass = "side"
                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                    ReactXP.Components.Constructors.RX.View(
                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                        children =
                                            [|
                                                (
                                                    (match props.Max with | Some max -> (if incrementedUnchecked <= max then Some incrementedUnchecked else None) | None -> Some incrementedUnchecked)
                                                    |> Option.map
                                                        (fun incremented ->
                                                            let __parentFQN = Some "LibClient.Components.IconButton"
                                                            let __currClass = "icon-button" + System.String.Format(" {0} {1}", (if (isInvalid) then "invalid" else ""), (if (not isInvalid) then "valid" else ""))
                                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                            LibClient.Components.Constructors.LC.IconButton(
                                                                state = (LibClient.Components.IconButton.PropStateFactory.MakeLowLevel(LibClient.Components.IconButton.Actionable (props.OnChange.Call incremented))),
                                                                icon = (Icon.Plus),
                                                                ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                            )
                                                        )
                                                    |> Option.getOrElse noElement
                                                )
                                            |]
                                    )
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
    )
