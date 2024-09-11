module LibClient.Components.Input.PickerInternals.PopupRender

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

open LibClient.Components.Input.PickerInternals.Popup
open LibClient.Components.Input.PickerModel
open ReactXP.LegacyStyles


let render(children: array<ReactElement>, props: LibClient.Components.Input.PickerInternals.Popup.Props<'Item>, estate: LibClient.Components.Input.PickerInternals.Popup.Estate<'Item>, pstate: LibClient.Components.Input.PickerInternals.Popup.Pstate, actions: LibClient.Components.Input.PickerInternals.Popup.Actions<'Item>, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "ReactXP.Components.ScrollView"
    let __currClass = "scroll-view"
    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
    let __dynamicStyles: List<ReactXP.LegacyStyles.RuleFunctionReturnedStyleRules> = estate.ModelState.MaybeFieldWidth |> Option.map (fun value -> [width (value - 4)]) |> Option.getOrElse []
    let __currStyles = __currStyles @ (ReactXP.LegacyStyles.Designtime.processDynamicStyles __dynamicStyles)
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
                            let whenAvailable(items: List<'Item>) = (
                                    (castAsElementAckingKeysWarning [|
                                        match (items) with
                                        | [] ->
                                            [|
                                                let __parentFQN = Some "ReactXP.Components.View"
                                                let __currClass = "no-items-message"
                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                ReactXP.Components.Constructors.RX.View(
                                                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                    children =
                                                        [|
                                                            let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                                            let __currClass = "no-items-message-text"
                                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                            LibClient.Components.Constructors.LC.LegacyUiText(
                                                                ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                children =
                                                                    [|
                                                                        makeTextNode2 __parentFQN "No items"
                                                                    |]
                                                            )
                                                        |]
                                                )
                                            |]
                                        | nonemptyItems ->
                                            [|
                                                (
                                                    (nonemptyItems)
                                                    |> Seq.mapi
                                                        (fun index item ->
                                                            (
                                                                let isHighlighted = Some index = estate.ModelState.MaybeHighlightedItemIndex
                                                                (castAsElementAckingKeysWarning [|
                                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                                    let __currClass = "item" + System.String.Format(" {0} {1}", (if (index = 0) then "first" else ""), (if (isHighlighted) then "highlighted" else ""))
                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                    ReactXP.Components.Constructors.RX.View(
                                                                        onPress = (actions.Select index item),
                                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                        children =
                                                                            [|
                                                                                let __parentFQN = Some "ReactXP.Components.View"
                                                                                let __currClass = "item-selectedness"
                                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                ReactXP.Components.Constructors.RX.View(
                                                                                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                                    children =
                                                                                        [|
                                                                                            (
                                                                                                if (estate.ModelState.Value.IsSelected item) then
                                                                                                    let __parentFQN = Some "LibClient.Components.Icon"
                                                                                                    let __currClass = "item-selected-icon"
                                                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                    LibClient.Components.Constructors.LC.Icon(
                                                                                                        icon = (Icon.CheckMark),
                                                                                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                                                                    )
                                                                                                else noElement
                                                                                            )
                                                                                        |]
                                                                                )
                                                                                let __parentFQN = Some "ReactXP.Components.View"
                                                                                let __currClass = "item-body"
                                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                ReactXP.Components.Constructors.RX.View(
                                                                                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                                    children =
                                                                                        [|
                                                                                            match (props.ItemView) with
                                                                                            | PickerItemView.Default toItemInfo ->
                                                                                                [|
                                                                                                    let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                                                                                    let __currClass = "item-label" + System.String.Format(" {0}", (if (isHighlighted) then "highlighted" else ""))
                                                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                    LibClient.Components.Constructors.LC.LegacyUiText(
                                                                                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                                                        children =
                                                                                                            [|
                                                                                                                makeTextNode2 __parentFQN (System.String.Format("{0}", (toItemInfo item).Label))
                                                                                                            |]
                                                                                                    )
                                                                                                |]
                                                                                            | PickerItemView.Custom render ->
                                                                                                [|
                                                                                                    render item
                                                                                                |]
                                                                                            |> castAsElementAckingKeysWarning
                                                                                        |]
                                                                                )
                                                                            |]
                                                                    )
                                                                |])
                                                            )
                                                        )
                                                    |> Array.ofSeq |> castAsElement
                                                )
                                            |]
                                        |> castAsElementAckingKeysWarning
                                    |])
                            )
                            let __parentFQN = Some "LibClient.Components.AsyncData"
                            LibClient.Components.Constructors.LC.AsyncData(
                                data = (estate.ModelState.SelectableItems),
                                whenAvailable =
                                    (fun (items) ->
                                            (castAsElementAckingKeysWarning [|
                                                whenAvailable items
                                            |])
                                    ),
                                whenFetching =
                                    (fun (maybeOldData) ->
                                            (castAsElementAckingKeysWarning [|
                                                match (maybeOldData) with
                                                | None ->
                                                    [|
                                                        let __parentFQN = Some "ReactXP.Components.View"
                                                        let __currClass = "activity-indicator-block"
                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                        ReactXP.Components.Constructors.RX.View(
                                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                            children =
                                                                [|
                                                                    let __parentFQN = Some "ReactXP.Components.ActivityIndicator"
                                                                    ReactXP.Components.Constructors.RX.ActivityIndicator(
                                                                        color = ("#aaaaaa"),
                                                                        size = (ReactXP.Components.ActivityIndicator.Medium)
                                                                    )
                                                                |]
                                                        )
                                                    |]
                                                | Some oldData ->
                                                    [|
                                                        whenAvailable oldData
                                                        let __parentFQN = Some "ReactXP.Components.View"
                                                        let __currClass = "activity-indicator-overlay"
                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                        ReactXP.Components.Constructors.RX.View(
                                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                            children =
                                                                [|
                                                                    let __parentFQN = Some "ReactXP.Components.ActivityIndicator"
                                                                    ReactXP.Components.Constructors.RX.ActivityIndicator(
                                                                        color = ("#aaaaaa"),
                                                                        size = (ReactXP.Components.ActivityIndicator.Medium)
                                                                    )
                                                                |]
                                                        )
                                                    |]
                                                |> castAsElementAckingKeysWarning
                                            |])
                                    )
                            )
                        |]
                )
            |]
    )
