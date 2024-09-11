module LibClient.Components.Input.TextRender

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

open LibClient.Components.Input.Text
open LibClient.Input


let render(children: array<ReactElement>, props: LibClient.Components.Input.Text.Props, estate: LibClient.Components.Input.Text.Estate, pstate: LibClient.Components.Input.Text.Pstate, actions: LibClient.Components.Input.Text.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    (
        let isLabelSmall = props.Value.IsSome || estate.IsFocused || props.Placeholder.IsSome
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
                    let __currClass = "border" + System.String.Format(" {0} {1} {2}", (if (props.Validity.IsInvalid) then "border-invalid" else ""), (if (estate.IsFocused) then "border-focused" else ""), (if (not props.Editable) then "border-noneditable" else ""))
                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                    ReactXP.Components.Constructors.RX.View(
                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                        children =
                            [|
                                (*  switching between 1 element and multiple elements is treated by either React or
             our layers on top of react as a fundamental change, resulting in reconstruction
             (i.e. not just re-rendering) of components, and loss of focus in the input field.
             So we keep things as always multiple by having sentinel elements  *)
                                match ((isLabelSmall, props.Prefix)) with
                                | (true, Some prefix) ->
                                    [|
                                        let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                        let __currClass = "prefix"
                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                        LibClient.Components.Constructors.LC.LegacyUiText(
                                            ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                            children =
                                                [|
                                                    makeTextNode2 __parentFQN (System.String.Format("{0}", prefix))
                                                |]
                                        )
                                    |]
                                | _ ->
                                    [|
                                        let __parentFQN = Some "ReactXP.Components.View"
                                        let __currClass = "focus-preserving-sentinel"
                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                        ReactXP.Components.Constructors.RX.View(
                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None)
                                        )
                                    |]
                                |> castAsElementAckingKeysWarning
                                let __parentFQN = Some "ReactXP.Components.TextInput"
                                let __currClass = "text-input defaults" + System.String.Format(" {0} {1}", (if (not props.Editable) then "noneditable" else ""), (if (not props.Multiline) then "single-line" else ""))
                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                ReactXP.Components.Constructors.RX.TextInput(
                                    autoCapitalize = (props.AutoCapitalize),
                                    returnKeyType = (props.ReturnKeyType),
                                    keyboardType = (props.KeyboardType),
                                    ?tabIndex = (props.TabIndex),
                                    secureTextEntry = (props.SecureTextEntry),
                                    ref = (actions.RefTextInput),
                                    ?maxLength = (props.MaxLength |> Option.map float),
                                    placeholderTextColor = ((extractPlaceholderColor __mergedStyles).ToReactXPString),
                                    placeholder = (props.Placeholder |> Option.getOrElse ""),
                                    blurOnSubmit = (props.BlurOnSubmit),
                                    editable = (props.Editable),
                                    autoFocus = (props.RequestFocusOnMount),
                                    multiline = (props.Multiline),
                                    onBlur = (actions.OnBlur),
                                    onFocus = (actions.OnFocus),
                                    ?onKeyPress = (actions.OnKeyPressOption),
                                    onChangeText = (NonemptyString.ofString >> props.OnChange),
                                    value = (props.Value |> NonemptyString.optionToString),
                                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.TextInput" __currStyles |> Some) else None)
                                )
                                match ((isLabelSmall, props.Suffix)) with
                                | (true, Some (InputSuffix.Text text)) ->
                                    [|
                                        let __parentFQN = Some "LibClient.Components.LegacyText"
                                        let __currClass = "suffix-text"
                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                        LibClient.Components.Constructors.LC.LegacyText(
                                            ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                            children =
                                                [|
                                                    makeTextNode2 __parentFQN (System.String.Format("{0}", text))
                                                |]
                                        )
                                    |]
                                | (true, Some (InputSuffix.Icon icon)) ->
                                    [|
                                        let __parentFQN = Some "LibClient.Components.Icon"
                                        let __currClass = "suffix-icon"
                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                        LibClient.Components.Constructors.LC.Icon(
                                            icon = (icon),
                                            ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                        )
                                    |]
                                | (true, Some (InputSuffix.Element element)) ->
                                    [|
                                        element
                                    |]
                                | _ ->
                                    [|
                                        let __parentFQN = Some "ReactXP.Components.View"
                                        let __currClass = "focus-preserving-sentinel"
                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                        ReactXP.Components.Constructors.RX.View(
                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None)
                                        )
                                    |]
                                |> castAsElementAckingKeysWarning
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
                    match (props.Label) with
                    | Some label ->
                        [|
                            let __parentFQN = Some "ReactXP.Components.View"
                            let __currClass = "label" + System.String.Format(" {0}", (if (isLabelSmall) then "small" else ""))
                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                            ReactXP.Components.Constructors.RX.View(
                                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                children =
                                    [|
                                        let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                        let __currClass = "label-text" + System.String.Format(" {0} {1} {2}", (if (props.Validity.IsInvalid) then "invalid" else ""), (if (estate.IsFocused) then "focused" else ""), (if (isLabelSmall) then "small" else ""))
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
                                            onPress = (actions.Focus)
                                        )
                                    |]
                            )
                        |]
                    | None ->
                        [|
                            let __parentFQN = Some "ReactXP.Components.View"
                            let __currClass = "focus-preserving-sentinel"
                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                            ReactXP.Components.Constructors.RX.View(
                                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None)
                            )
                        |]
                    |> castAsElementAckingKeysWarning
                |]
        )
    )
