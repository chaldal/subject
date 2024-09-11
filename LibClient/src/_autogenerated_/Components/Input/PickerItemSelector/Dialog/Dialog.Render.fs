module LibClient.Components.Input.PickerItemSelector.DialogRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard

open LibClient.RenderResultModule

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

open LibClient.Components.Input.PickerItemSelector.Dialog
open LibClient.PickerItemSelector


let render(props: LibClient.Components.Input.PickerItemSelector.Dialog.Props<'T>, estate: LibClient.Components.Input.PickerItemSelector.Dialog.Estate<'T>, pstate: LibClient.Components.Input.PickerItemSelector.Dialog.Pstate, actions: LibClient.Components.Input.PickerItemSelector.Dialog.Actions<'T>, __componentStyles: ReactXP.Styles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.Styles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    (fun (__sibI, __sibC, __pFQN) ->
        let __parentFQN = Some "LibClient.Components.With.ScreenSize"
        LibClient.Components.With.ScreenSize.Make
            (
                let __currExplicitProps =
                    (LibClient.Components.TypeExtensions.TEs.MakeWith_ScreenSizeProps(
                        pWith =
                            (fun (screenSize) ->
                                FRH.fragment []
                                    (
                                        let __list = [
                                            (fun (__sibI, __sibC, __pFQN) ->
                                                let __parentFQN = Some "LibClient.Components.Dialog.Shell.WhiteRounded.Raw"
                                                LibClient.Components.Dialog.Shell.WhiteRounded.Raw.Make
                                                    (
                                                        let __currExplicitProps =
                                                            (LibClient.Components.TypeExtensions.TEs.MakeDialog_Shell_WhiteRounded_RawProps(
                                                                pCanClose = (LibClient.Components.Dialog.Shell.WhiteRounded.Raw.When ([LibClient.Components.Dialog.Shell.WhiteRounded.Raw.OnEscape; LibClient.Components.Dialog.Shell.WhiteRounded.Raw.OnBackground], actions.TryCancel)),
                                                                pPosition = (LibClient.Components.Dialog.Shell.WhiteRounded.Raw.Top)
                                                            ))
                                                        let __currClass = (nthChildClasses __sibI __sibC)
                                                        let __currStyles = (ReactXP.Styles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                        let __implicitProps = [
                                                            if __currClass <> "" then ("ClassName", __currClass :> obj)
                                                            if (not __currStyles.IsEmpty) then ("__style", __currStyles :> obj)
                                                        ]
                                                        (ReactXP.Styles.Runtime.injectImplicitProps __implicitProps __currExplicitProps)
                                                    )
                                                    (
                                                        let __list = [
                                                            (fun (__sibI, __sibC, __pFQN) ->
                                                                let __parentFQN = Some "LibClient.Components.With.Ref"
                                                                LibClient.Components.With.Ref.Make
                                                                    (
                                                                        let __currExplicitProps =
                                                                            (LibClient.Components.TypeExtensions.TEs.MakeWith_RefProps(
                                                                                pOnInitialize = (fun (input: TextInput.ITextInputRef) -> input.requestFocus()),
                                                                                pWith =
                                                                                    (fun (bindInput, _: Option<TextInput.ITextInputRef>) ->
                                                                                        FRH.fragment []
                                                                                            (
                                                                                                let __list = [
                                                                                                    (fun (__sibI, __sibC, __pFQN) ->
                                                                                                        let __parentFQN = Some "ReactXP.Components.View"
                                                                                                        ReactXP.Components.View.Make
                                                                                                            (
                                                                                                                let __currExplicitProps =
                                                                                                                    (ReactXP.Components.TypeExtensions.TEs.MakeViewProps(
                                                                                                                        ponPress = (fun e -> e.stopPropagation())
                                                                                                                    ))
                                                                                                                let __currClass = (nthChildClasses __sibI __sibC)
                                                                                                                let __currStyles = (ReactXP.Styles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                                let __implicitProps = [
                                                                                                                    if __currClass <> "" then ("ClassName", __currClass :> obj)
                                                                                                                    if (not __currStyles.IsEmpty) then ("style", ReactXP.Styles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles)
                                                                                                                ]
                                                                                                                (ReactXP.Styles.Runtime.injectImplicitProps __implicitProps __currExplicitProps)
                                                                                                            )
                                                                                                            (
                                                                                                                let __list = [
                                                                                                                    (fun (__sibI, __sibC, __pFQN) ->
                                                                                                                        let __parentFQN = Some "ReactXP.Components.TextInput"
                                                                                                                        ReactXP.Components.TextInput.Make
                                                                                                                            (
                                                                                                                                let __currExplicitProps =
                                                                                                                                    (ReactXP.Components.TypeExtensions.TEs.MakeTextInputProps(
                                                                                                                                        pplaceholder = ("Type to filter"),
                                                                                                                                        ponChangeText = (actions.Filter),
                                                                                                                                        pref = (bindInput)
                                                                                                                                    ))
                                                                                                                                let __currClass = (System.String.Format("{0}{1}{2}", "text-input ", (screenSize.Class), "")) + (nthChildClasses __sibI __sibC)
                                                                                                                                let __currStyles = (ReactXP.Styles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                                                let __implicitProps = [
                                                                                                                                    if __currClass <> "" then ("ClassName", __currClass :> obj)
                                                                                                                                    if (not __currStyles.IsEmpty) then ("style", ReactXP.Styles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.TextInput" __currStyles)
                                                                                                                                ]
                                                                                                                                (ReactXP.Styles.Runtime.injectImplicitProps __implicitProps __currExplicitProps)
                                                                                                                            )
                                                                                                                            []
                                                                                                                    ) |> RenderResult.Make
                                                                                                                ]
                                                                                                                __list |> RenderResult.ToRawElements __parentFQN
                                                                                                            )
                                                                                                    ) |> RenderResult.Make
                                                                                                ]
                                                                                                __list |> RenderResult.ToRawElements __parentFQN
                                                                                            )
                                                                                    )
                                                                            ))
                                                                        let __currClass = (nthChildClasses __sibI __sibC)
                                                                        let __currStyles = (ReactXP.Styles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                        let __implicitProps = [
                                                                            if __currClass <> "" then ("ClassName", __currClass :> obj)
                                                                            if (not __currStyles.IsEmpty) then ("__style", __currStyles :> obj)
                                                                        ]
                                                                        (ReactXP.Styles.Runtime.injectImplicitProps __implicitProps __currExplicitProps)
                                                                    )
                                                                    []
                                                            ) |> RenderResult.Make
                                                            (fun (__sibI, __sibC, __pFQN) ->
                                                                let __parentFQN = Some "ReactXP.Components.ScrollView"
                                                                ReactXP.Components.ScrollView.Make
                                                                    (
                                                                        let __currExplicitProps =
                                                                            (ReactXP.Components.TypeExtensions.TEs.MakeScrollViewProps(
                                                                                pvertical = (true)
                                                                            ))
                                                                        let __currClass = "scroll-view" + (nthChildClasses __sibI __sibC)
                                                                        let __currStyles = (ReactXP.Styles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                        let __implicitProps = [
                                                                            if __currClass <> "" then ("ClassName", __currClass :> obj)
                                                                            if (not __currStyles.IsEmpty) then ("style", ReactXP.Styles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.ScrollView" __currStyles)
                                                                        ]
                                                                        (ReactXP.Styles.Runtime.injectImplicitProps __implicitProps __currExplicitProps)
                                                                    )
                                                                    (
                                                                        let __list = [
                                                                            (
                                                                                match (props.Parameters.ItemView) with
                                                                                | PickerItemView.Default toItemInfo ->
                                                                                    (
                                                                                        let __list = [
                                                                                            (fun (__sibI, __sibC, __pFQN) ->
                                                                                                let __parentFQN = Some "LibClient.Components.ItemList"
                                                                                                LibClient.Components.ItemList.Make
                                                                                                    (
                                                                                                        let __currExplicitProps =
                                                                                                            (LibClient.Components.TypeExtensions.TEs.MakeItemListProps(
                                                                                                                pStyle = (LibClient.Components.ItemList.Raw),
                                                                                                                pItems = (estate.Items |> OrderedSet.toSeq),
                                                                                                                pWhenNonempty =
                                                                                                                    (fun (items) ->
                                                                                                                        FRH.fragment []
                                                                                                                            (
                                                                                                                                let __list = [
                                                                                                                                    (
                                                                                                                                        (items)
                                                                                                                                        |> Seq.map
                                                                                                                                            (fun (item, _metadata) ->
                                                                                                                                                (fun (__sibI, __sibC, __pFQN) ->
                                                                                                                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                                                                                                                    ReactXP.Components.View.Make
                                                                                                                                                        (
                                                                                                                                                            let __currExplicitProps = (ReactXP.Components.TypeExtensions.TEs.MakeViewProps())
                                                                                                                                                            let __currClass = "item" + (nthChildClasses __sibI __sibC)
                                                                                                                                                            let __currStyles = (ReactXP.Styles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                                                                            let __implicitProps = [
                                                                                                                                                                if __currClass <> "" then ("ClassName", __currClass :> obj)
                                                                                                                                                                if (not __currStyles.IsEmpty) then ("style", ReactXP.Styles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles)
                                                                                                                                                            ]
                                                                                                                                                            (ReactXP.Styles.Runtime.injectImplicitProps __implicitProps __currExplicitProps)
                                                                                                                                                        )
                                                                                                                                                        (
                                                                                                                                                            let __list = [
                                                                                                                                                                (fun (__sibI, __sibC, __pFQN) ->
                                                                                                                                                                    let __parentFQN = Some "LibClient.Components.Text"
                                                                                                                                                                    LibClient.Components.Text.Make
                                                                                                                                                                        (
                                                                                                                                                                            let __currExplicitProps = (LibClient.Components.TypeExtensions.TEs.MakeTextProps())
                                                                                                                                                                            let __currClass = (nthChildClasses __sibI __sibC)
                                                                                                                                                                            let __currStyles = (ReactXP.Styles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                                                                                            let __implicitProps = [
                                                                                                                                                                                if __currClass <> "" then ("ClassName", __currClass :> obj)
                                                                                                                                                                                if (not __currStyles.IsEmpty) then ("__style", __currStyles :> obj)
                                                                                                                                                                            ]
                                                                                                                                                                            (ReactXP.Styles.Runtime.injectImplicitProps __implicitProps __currExplicitProps)
                                                                                                                                                                        )
                                                                                                                                                                        (
                                                                                                                                                                            let __list = [
                                                                                                                                                                                ((makeTextNode (System.String.Format("{0}", (toItemInfo item).Label))) |> RenderResult.Make)
                                                                                                                                                                            ]
                                                                                                                                                                            __list |> RenderResult.ToRawElements __parentFQN
                                                                                                                                                                        )
                                                                                                                                                                ) |> RenderResult.Make
                                                                                                                                                                (fun (__sibI, __sibC, __pFQN) ->
                                                                                                                                                                    let __parentFQN = Some "LibClient.Components.TapCapture"
                                                                                                                                                                    LibClient.Components.TapCapture.Make
                                                                                                                                                                        (
                                                                                                                                                                            let __currExplicitProps =
                                                                                                                                                                                (LibClient.Components.TypeExtensions.TEs.MakeTapCaptureProps(
                                                                                                                                                                                    pOnPress = ((fun e -> (actions.TryCancel e; props.Parameters.OnPress item e)))
                                                                                                                                                                                ))
                                                                                                                                                                            let __currClass = (nthChildClasses __sibI __sibC)
                                                                                                                                                                            let __currStyles = (ReactXP.Styles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                                                                                            let __implicitProps = [
                                                                                                                                                                                if __currClass <> "" then ("ClassName", __currClass :> obj)
                                                                                                                                                                                if (not __currStyles.IsEmpty) then ("__style", __currStyles :> obj)
                                                                                                                                                                            ]
                                                                                                                                                                            (ReactXP.Styles.Runtime.injectImplicitProps __implicitProps __currExplicitProps)
                                                                                                                                                                        )
                                                                                                                                                                        []
                                                                                                                                                                ) |> RenderResult.Make
                                                                                                                                                            ]
                                                                                                                                                            __list |> RenderResult.ToRawElements __parentFQN
                                                                                                                                                        )
                                                                                                                                                ) |> RenderResult.Make
                                                                                                                                            )
                                                                                                                                        |> List.ofSeq
                                                                                                                                        |> RenderResult.Make
                                                                                                                                    )
                                                                                                                                ]
                                                                                                                                __list |> RenderResult.ToRawElements __parentFQN
                                                                                                                            )
                                                                                                                    )
                                                                                                            ))
                                                                                                        let __currClass = "item-list" + (nthChildClasses __sibI __sibC)
                                                                                                        let __currStyles = (ReactXP.Styles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                        let __implicitProps = [
                                                                                                            if __currClass <> "" then ("ClassName", __currClass :> obj)
                                                                                                            if (not __currStyles.IsEmpty) then ("__style", __currStyles :> obj)
                                                                                                        ]
                                                                                                        (ReactXP.Styles.Runtime.injectImplicitProps __implicitProps __currExplicitProps)
                                                                                                    )
                                                                                                    []
                                                                                            ) |> RenderResult.Make
                                                                                        ]
                                                                                        __list |> RenderResult.ToRawElements __parentFQN
                                                                                    )
                                                                                    |> RenderResult.Make
                                                                                | PickerItemView.Custom render ->
                                                                                    (
                                                                                        let __list = [
                                                                                            (fun (__sibI, __sibC, __pFQN) ->
                                                                                                let __parentFQN = Some "LibClient.Components.ItemList"
                                                                                                LibClient.Components.ItemList.Make
                                                                                                    (
                                                                                                        let __currExplicitProps =
                                                                                                            (LibClient.Components.TypeExtensions.TEs.MakeItemListProps(
                                                                                                                pStyle = (LibClient.Components.ItemList.Raw),
                                                                                                                pItems = (estate.Items |> OrderedSet.toSeq),
                                                                                                                pWhenNonempty =
                                                                                                                    (fun (items) ->
                                                                                                                        FRH.fragment []
                                                                                                                            (
                                                                                                                                let __list = [
                                                                                                                                    (
                                                                                                                                        (items)
                                                                                                                                        |> Seq.map
                                                                                                                                            (fun (item, _metadata) ->
                                                                                                                                                (fun (__sibI, __sibC, __pFQN) ->
                                                                                                                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                                                                                                                    ReactXP.Components.View.Make
                                                                                                                                                        (
                                                                                                                                                            let __currExplicitProps = (ReactXP.Components.TypeExtensions.TEs.MakeViewProps())
                                                                                                                                                            let __currClass = "item" + (nthChildClasses __sibI __sibC)
                                                                                                                                                            let __currStyles = (ReactXP.Styles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                                                                            let __implicitProps = [
                                                                                                                                                                if __currClass <> "" then ("ClassName", __currClass :> obj)
                                                                                                                                                                if (not __currStyles.IsEmpty) then ("style", ReactXP.Styles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles)
                                                                                                                                                            ]
                                                                                                                                                            (ReactXP.Styles.Runtime.injectImplicitProps __implicitProps __currExplicitProps)
                                                                                                                                                        )
                                                                                                                                                        (
                                                                                                                                                            let __list = [
                                                                                                                                                                ((render item) |> RenderResult.Make)
                                                                                                                                                                (fun (__sibI, __sibC, __pFQN) ->
                                                                                                                                                                    let __parentFQN = Some "LibClient.Components.TapCapture"
                                                                                                                                                                    LibClient.Components.TapCapture.Make
                                                                                                                                                                        (
                                                                                                                                                                            let __currExplicitProps =
                                                                                                                                                                                (LibClient.Components.TypeExtensions.TEs.MakeTapCaptureProps(
                                                                                                                                                                                    pOnPress = ((fun e -> (actions.TryCancel e; props.Parameters.OnPress item e)))
                                                                                                                                                                                ))
                                                                                                                                                                            let __currClass = (nthChildClasses __sibI __sibC)
                                                                                                                                                                            let __currStyles = (ReactXP.Styles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                                                                                            let __implicitProps = [
                                                                                                                                                                                if __currClass <> "" then ("ClassName", __currClass :> obj)
                                                                                                                                                                                if (not __currStyles.IsEmpty) then ("__style", __currStyles :> obj)
                                                                                                                                                                            ]
                                                                                                                                                                            (ReactXP.Styles.Runtime.injectImplicitProps __implicitProps __currExplicitProps)
                                                                                                                                                                        )
                                                                                                                                                                        []
                                                                                                                                                                ) |> RenderResult.Make
                                                                                                                                                            ]
                                                                                                                                                            __list |> RenderResult.ToRawElements __parentFQN
                                                                                                                                                        )
                                                                                                                                                ) |> RenderResult.Make
                                                                                                                                            )
                                                                                                                                        |> List.ofSeq
                                                                                                                                        |> RenderResult.Make
                                                                                                                                    )
                                                                                                                                ]
                                                                                                                                __list |> RenderResult.ToRawElements __parentFQN
                                                                                                                            )
                                                                                                                    )
                                                                                                            ))
                                                                                                        let __currClass = "item-list" + (nthChildClasses __sibI __sibC)
                                                                                                        let __currStyles = (ReactXP.Styles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                        let __implicitProps = [
                                                                                                            if __currClass <> "" then ("ClassName", __currClass :> obj)
                                                                                                            if (not __currStyles.IsEmpty) then ("__style", __currStyles :> obj)
                                                                                                        ]
                                                                                                        (ReactXP.Styles.Runtime.injectImplicitProps __implicitProps __currExplicitProps)
                                                                                                    )
                                                                                                    []
                                                                                            ) |> RenderResult.Make
                                                                                        ]
                                                                                        __list |> RenderResult.ToRawElements __parentFQN
                                                                                    )
                                                                                    |> RenderResult.Make
                                                                            )
                                                                        ]
                                                                        __list |> RenderResult.ToRawElements __parentFQN
                                                                    )
                                                            ) |> RenderResult.Make
                                                        ]
                                                        __list |> RenderResult.ToRawElements __parentFQN
                                                    )
                                            ) |> RenderResult.Make
                                        ]
                                        __list |> RenderResult.ToRawElements __parentFQN
                                    )
                            )
                    ))
                let __currClass = (nthChildClasses __sibI __sibC)
                let __currStyles = (ReactXP.Styles.Runtime.findApplicableStyles __mergedStyles __currClass)
                let __implicitProps = [
                    if __currClass <> "" then ("ClassName", __currClass :> obj)
                    if (not __currStyles.IsEmpty) then ("__style", __currStyles :> obj)
                ]
                (ReactXP.Styles.Runtime.injectImplicitProps __implicitProps __currExplicitProps)
            )
            []
    ) |> RenderResult.Make
    |> RenderResult.ToRawElementsSingle __parentFQN |> RenderResult.ToSingleElement