module LibClient.Components.Input.PickerInternals.FieldRender

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

open LibClient.Components.Input.PickerInternals.Field
open LibClient.Components.Input.PickerModel


let render(children: array<ReactElement>, props: LibClient.Components.Input.PickerInternals.Field.Props<'Item>, estate: LibClient.Components.Input.PickerInternals.Field.Estate<'Item>, pstate: LibClient.Components.Input.PickerInternals.Field.Pstate, actions: LibClient.Components.Input.PickerInternals.Field.Actions<'Item>, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    (castAsElementAckingKeysWarning [|
        (*  onLayout='ignore' hack. Check LibClient/src/Components/TapCapture/TapCapture.render  *)
        (*  we now use a real onLayout callback because we needed, but if it's ever to be removed,
         it HAS to be replaced with ignore because of the above comment  *)
        let __parentFQN = Some "ReactXP.Components.View"
        let __currClass = (System.String.Format("{0}{1}{2}", "view ", (TopLevelBlockClass), ""))
        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
        ReactXP.Components.Constructors.RX.View(
            onLayout = (actions.OnLayout),
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
                    let __currClass = "border" + System.String.Format(" {0} {1}", (if (props.Validity.IsInvalid) then "border-invalid" else ""), (if (estate.IsFocused) then "border-focused" else ""))
                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                    ReactXP.Components.Constructors.RX.View(
                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                        children =
                            [|
                                let __parentFQN = Some "ReactXP.Components.View"
                                let __currClass = "picker-actions"
                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                ReactXP.Components.Constructors.RX.View(
                                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                    children =
                                        [|
                                            let __parentFQN = Some "ReactXP.Components.View"
                                            ReactXP.Components.Constructors.RX.View(
                                                children =
                                                    [|
                                                        match (props.Value) with
                                                        | AtMostOne (maybeSelectedValues, _) ->
                                                            [|
                                                                (
                                                                    if (maybeSelectedValues <> None) then
                                                                        (castAsElementAckingKeysWarning [|
                                                                            let __parentFQN = Some "LibClient.Components.Icon"
                                                                            let __currClass = "icon"
                                                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                            LibClient.Components.Constructors.LC.Icon(
                                                                                icon = (Icon.X),
                                                                                ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                                            )
                                                                            let __parentFQN = Some "LibClient.Components.TapCapture"
                                                                            LibClient.Components.Constructors.LC.TapCapture(
                                                                                onPress = (actions.Clear)
                                                                            )
                                                                        |])
                                                                    else noElement
                                                                )
                                                            |]
                                                        | Any (maybeSelectedValues, _) ->
                                                            [|
                                                                (
                                                                    (maybeSelectedValues)
                                                                    |> Option.map
                                                                        (fun selectedValues ->
                                                                            (castAsElementAckingKeysWarning [|
                                                                                (
                                                                                    if (not selectedValues.IsEmpty) then
                                                                                        (castAsElementAckingKeysWarning [|
                                                                                            let __parentFQN = Some "LibClient.Components.Icon"
                                                                                            let __currClass = "icon"
                                                                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                            LibClient.Components.Constructors.LC.Icon(
                                                                                                icon = (Icon.X),
                                                                                                ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                                                            )
                                                                                            let __parentFQN = Some "LibClient.Components.TapCapture"
                                                                                            LibClient.Components.Constructors.LC.TapCapture(
                                                                                                onPress = (actions.Clear)
                                                                                            )
                                                                                        |])
                                                                                    else noElement
                                                                                )
                                                                            |])
                                                                        )
                                                                    |> Option.getOrElse noElement
                                                                )
                                                            |]
                                                        | _ ->
                                                            [||]
                                                        |> castAsElementAckingKeysWarning
                                                    |]
                                            )
                                            let __parentFQN = Some "ReactXP.Components.View"
                                            ReactXP.Components.Constructors.RX.View(
                                                children =
                                                    [|
                                                        let __parentFQN = Some "LibClient.Components.Icon"
                                                        let __currClass = "icon"
                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                        LibClient.Components.Constructors.LC.Icon(
                                                            icon = (Icon.ChevronDown),
                                                            ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                        )
                                                        let __parentFQN = Some "LibClient.Components.TapCapture"
                                                        LibClient.Components.Constructors.LC.TapCapture(
                                                            onPress = (actions.ShowItemSelector),
                                                            styles = ([| FieldStyles.Styles.tapCapture |])
                                                        )
                                                    |]
                                            )
                                        |]
                                )
                                let __parentFQN = Some "LibClient.Components.Responsive"
                                LibClient.Components.Constructors.LC.Responsive(
                                    desktop =
                                        (fun (_) ->
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "ReactXP.Components.TextInput"
                                                    let __currClass = "text-input"
                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                    ReactXP.Components.Constructors.RX.TextInput(
                                                        onKeyPress = (actions.OnKeyPress),
                                                        onChangeText = (NonemptyString.ofString >> actions.OnChangeQuery),
                                                        onBlur = (actions.OnBlur),
                                                        onFocus = (actions.OnFocus),
                                                        placeholderTextColor = ((extractPlaceholderColor __mergedStyles).ToReactXPString),
                                                        placeholder = (match (props.Value.IsEmpty, props.Placeholder) with (true, Some value) -> value | _ -> ""),
                                                        value = (estate.MaybeQuery |> NonemptyString.optionToString),
                                                        ref = (actions.RefTextInput),
                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.TextInput" __currStyles |> Some) else None)
                                                    )
                                                |])
                                        ),
                                    handheld =
                                        (fun (_) ->
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                    let __currClass = "handheld-full-width-tap-area"
                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                    ReactXP.Components.Constructors.RX.View(
                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                        children =
                                                            [|
                                                                (
                                                                    (if estate.ModelState.Value.IsEmpty then props.Placeholder else None)
                                                                    |> Option.map
                                                                        (fun value ->
                                                                            let __parentFQN = Some "ReactXP.Components.TextInput"
                                                                            let __currClass = "text-input"
                                                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                            ReactXP.Components.Constructors.RX.TextInput(
                                                                                placeholderTextColor = ((extractPlaceholderColor __mergedStyles).ToReactXPString),
                                                                                placeholder = (value),
                                                                                editable = (false),
                                                                                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.TextInput" __currStyles |> Some) else None)
                                                                            )
                                                                        )
                                                                    |> Option.getOrElse noElement
                                                                )
                                                                let __parentFQN = Some "LibClient.Components.TapCapture"
                                                                LibClient.Components.Constructors.LC.TapCapture(
                                                                    onPress = (actions.ShowItemSelector),
                                                                    styles = ([| FieldStyles.Styles.tapCapture |])
                                                                )
                                                            |]
                                                    )
                                                |])
                                        )
                                )
                                (
                                    if (actions.ShouldShowSelectedValue()) then
                                        let __parentFQN = Some "ReactXP.Components.View"
                                        let __currClass = "picker-values"
                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                        ReactXP.Components.Constructors.RX.View(
                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                            children =
                                                [|
                                                    match (props.Value) with
                                                    | AtMostOne (maybeSelectedValue, _) ->
                                                        [|
                                                            (
                                                                (maybeSelectedValue)
                                                                |> Option.map
                                                                    (fun item ->
                                                                        (castAsElementAckingKeysWarning [|
                                                                            match (props.ItemView) with
                                                                            | PickerItemView.Default toItemInfo ->
                                                                                [|
                                                                                    let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                                                                    let __currClass = "selected-item"
                                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                    LibClient.Components.Constructors.LC.LegacyUiText(
                                                                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                                        children =
                                                                                            [|
                                                                                                makeTextNode2 __parentFQN (System.String.Format("{0}", (toItemInfo item).Label))
                                                                                            |]
                                                                                    )
                                                                                |]
                                                                            | PickerItemView.Custom renderItem ->
                                                                                [|
                                                                                    renderItem item
                                                                                |]
                                                                            |> castAsElementAckingKeysWarning
                                                                        |])
                                                                    )
                                                                |> Option.getOrElse noElement
                                                            )
                                                        |]
                                                    | ExactlyOne (maybeSelectedValue, _) ->
                                                        [|
                                                            (
                                                                (maybeSelectedValue)
                                                                |> Option.map
                                                                    (fun item ->
                                                                        (castAsElementAckingKeysWarning [|
                                                                            match (props.ItemView) with
                                                                            | PickerItemView.Default toItemInfo ->
                                                                                [|
                                                                                    let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                                                                    let __currClass = "selected-item"
                                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                    LibClient.Components.Constructors.LC.LegacyUiText(
                                                                                        numberOfLines = (1),
                                                                                        ellipsizeMode = (EllipsizeMode.Tail),
                                                                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                                        children =
                                                                                            [|
                                                                                                makeTextNode2 __parentFQN (System.String.Format("{0}", (toItemInfo item).Label))
                                                                                            |]
                                                                                    )
                                                                                |]
                                                                            | PickerItemView.Custom renderItem ->
                                                                                [|
                                                                                    renderItem item
                                                                                |]
                                                                            |> castAsElementAckingKeysWarning
                                                                        |])
                                                                    )
                                                                |> Option.getOrElse noElement
                                                            )
                                                        |]
                                                    | AtLeastOne (maybeSelectedValues, _) | Any (maybeSelectedValues, _) ->
                                                        [|
                                                            (
                                                                (maybeSelectedValues)
                                                                |> Option.map
                                                                    (fun selectedValues ->
                                                                        (castAsElementAckingKeysWarning [|
                                                                            (
                                                                                (selectedValues.ToSeq)
                                                                                |> Seq.map
                                                                                    (fun item ->
                                                                                        let __parentFQN = Some "ReactXP.Components.View"
                                                                                        let __currClass = "tag" + System.String.Format(" {0}", (if (estate.ModelState.DeleteState = DeleteState.Selected item) then "highlighted" else ""))
                                                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                        ReactXP.Components.Constructors.RX.View(
                                                                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                                            children =
                                                                                                [|
                                                                                                    match (props.ItemView) with
                                                                                                    | PickerItemView.Default toItemInfo ->
                                                                                                        [|
                                                                                                            let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                                                                                            let __currClass = "selected-item tag-text"
                                                                                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                            LibClient.Components.Constructors.LC.LegacyUiText(
                                                                                                                ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                                                                children =
                                                                                                                    [|
                                                                                                                        makeTextNode2 __parentFQN (System.String.Format("{0}", (toItemInfo item).Label))
                                                                                                                    |]
                                                                                                            )
                                                                                                        |]
                                                                                                    | PickerItemView.Custom renderItem ->
                                                                                                        [|
                                                                                                            renderItem item
                                                                                                        |]
                                                                                                    |> castAsElementAckingKeysWarning
                                                                                                    (
                                                                                                        if (selectedValues.Count > 1 || (match props.Value with | Any _ -> true | _ -> false)) then
                                                                                                            let __parentFQN = Some "ReactXP.Components.View"
                                                                                                            ReactXP.Components.Constructors.RX.View(
                                                                                                                children =
                                                                                                                    [|
                                                                                                                        let __parentFQN = Some "LibClient.Components.Icon"
                                                                                                                        let __currClass = "icon"
                                                                                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                                        LibClient.Components.Constructors.LC.Icon(
                                                                                                                            icon = (Icon.X),
                                                                                                                            ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                                                                                        )
                                                                                                                        let __parentFQN = Some "LibClient.Components.TapCapture"
                                                                                                                        LibClient.Components.Constructors.LC.TapCapture(
                                                                                                                            onPress = (actions.Unselect item)
                                                                                                                        )
                                                                                                                    |]
                                                                                                            )
                                                                                                        else noElement
                                                                                                    )
                                                                                                |]
                                                                                        )
                                                                                    )
                                                                                |> Array.ofSeq |> castAsElement
                                                                            )
                                                                        |])
                                                                    )
                                                                |> Option.getOrElse noElement
                                                            )
                                                        |]
                                                    |> castAsElementAckingKeysWarning
                                                    let __parentFQN = Some "LibClient.Components.TapCapture"
                                                    LibClient.Components.Constructors.LC.TapCapture(
                                                        onPress = (actions.RequestFocus)
                                                    )
                                                |]
                                        )
                                    else noElement
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
                                            let __currClass = "label-text" + System.String.Format(" {0} {1}", (if (props.Validity.IsInvalid) then "invalid" else ""), (if (estate.IsFocused) then "focused" else ""))
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
    |])
