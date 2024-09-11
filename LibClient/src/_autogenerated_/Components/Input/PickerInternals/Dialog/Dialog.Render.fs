module LibClient.Components.Input.PickerInternals.DialogRender

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

open LibClient.Components.Input.PickerInternals.Dialog
open LibClient.Components.Input.PickerModel


let render(children: array<ReactElement>, props: LibClient.Components.Input.PickerInternals.Dialog.Props<'Item>, estate: LibClient.Components.Input.PickerInternals.Dialog.Estate<'Item>, pstate: LibClient.Components.Input.PickerInternals.Dialog.Pstate, actions: LibClient.Components.Input.PickerInternals.Dialog.Actions<'Item>, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
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
                        let __parentFQN = Some "LibClient.Components.Dialog.Shell.WhiteRounded.Raw"
                        LibClient.Components.Constructors.LC.Dialog.Shell.WhiteRounded.Raw(
                            canClose = (LibClient.Components.Dialog.Shell.WhiteRounded.Raw.When ([LibClient.Components.Dialog.Shell.WhiteRounded.Raw.OnEscape; LibClient.Components.Dialog.Shell.WhiteRounded.Raw.OnBackground; LibClient.Components.Dialog.Shell.WhiteRounded.Raw.OnCloseButton], actions.TryCancel)),
                            position = (LibClient.Components.Dialog.Shell.WhiteRounded.Raw.Top),
                            children =
                                [|
                                    let __parentFQN = Some "LibClient.Components.With.Ref"
                                    LibClient.Components.Constructors.LC.With.Ref(
                                        onInitialize = (fun (input: TextInput.ITextInputRef) -> input.requestFocus()),
                                        ``with`` =
                                            (fun (bindInput, _: Option<TextInput.ITextInputRef>) ->
                                                    (castAsElementAckingKeysWarning [|
                                                        let __parentFQN = Some "ReactXP.Components.View"
                                                        ReactXP.Components.Constructors.RX.View(
                                                            onPress = (fun e -> e.stopPropagation()),
                                                            children =
                                                                [|
                                                                    (
                                                                        if (props.Parameters.ShowSearchBar) then
                                                                            let __parentFQN = Some "ReactXP.Components.TextInput"
                                                                            let __currClass = (System.String.Format("{0}{1}{2}", "text-input ", (screenSize.Class), ""))
                                                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                            ReactXP.Components.Constructors.RX.TextInput(
                                                                                ?placeholder = (props.Parameters.Placeholder),
                                                                                onChangeText = (NonemptyString.ofString >> actions.OnQueryChange),
                                                                                value = (estate.ModelState.MaybeQuery |> NonemptyString.optionToString),
                                                                                ref = (bindInput),
                                                                                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.TextInput" __currStyles |> Some) else None)
                                                                            )
                                                                        else noElement
                                                                    )
                                                                |]
                                                        )
                                                    |])
                                            )
                                    )
                                    let __parentFQN = Some "ReactXP.Components.ScrollView"
                                    let __currClass = "scroll-view"
                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                    ReactXP.Components.Constructors.RX.ScrollView(
                                        vertical = (true),
                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.ScrollView" __currStyles |> Some) else None),
                                        children =
                                            [|
                                                let whenAvailable(items: List<'Item>) = (
                                                        (castAsElementAckingKeysWarning [|
                                                            let __parentFQN = Some "LibClient.Components.ItemList"
                                                            LibClient.Components.Constructors.LC.ItemList(
                                                                style = (LibClient.Components.ItemList.Raw),
                                                                items = (items),
                                                                styles = ([| DialogStyles.Styles.itemList |]),
                                                                whenNonempty =
                                                                    (fun (items) ->
                                                                            (castAsElementAckingKeysWarning [|
                                                                                (
                                                                                    (items)
                                                                                    |> Seq.mapi
                                                                                        (fun index item ->
                                                                                            let __parentFQN = Some "ReactXP.Components.View"
                                                                                            let __currClass = "item"
                                                                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                            ReactXP.Components.Constructors.RX.View(
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
                                                                                                                    match (props.Parameters.ItemView) with
                                                                                                                    | PickerItemView.Default toItemInfo ->
                                                                                                                        [|
                                                                                                                            let __parentFQN = Some "LibClient.Components.LegacyText"
                                                                                                                            LibClient.Components.Constructors.LC.LegacyText(
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
                                                                                                        let __parentFQN = Some "LibClient.Components.TapCapture"
                                                                                                        LibClient.Components.Constructors.LC.TapCapture(
                                                                                                            onPress = (actions.Toggle index item)
                                                                                                        )
                                                                                                    |]
                                                                                            )
                                                                                        )
                                                                                    |> Array.ofSeq |> castAsElement
                                                                                )
                                                                            |])
                                                                    )
                                                            )
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
                                    (
                                        if (estate.ModelState.Value.CanSelectMultiple) then
                                            let __parentFQN = Some "LibClient.Components.Buttons"
                                            LibClient.Components.Constructors.LC.Buttons(
                                                children =
                                                    [|
                                                        let __parentFQN = Some "LibClient.Components.Button"
                                                        LibClient.Components.Constructors.LC.Button(
                                                            state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (LibClient.Components.Button.Actionable actions.TryCancel)),
                                                            label = ("Done")
                                                        )
                                                    |]
                                            )
                                        else noElement
                                    )
                                |]
                        )
                    |])
            )
    )
