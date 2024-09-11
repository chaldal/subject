module LibClient.Components.Input.PickerItemSelector.PopupRender

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

open LibClient.Components.Input.PickerItemSelector.Popup
open LibClient.PickerItemSelector


let render(props: LibClient.Components.Input.PickerItemSelector.Popup.Props<'T>, estate: LibClient.Components.Input.PickerItemSelector.Popup.Estate<'T>, pstate: LibClient.Components.Input.PickerItemSelector.Popup.Pstate, actions: LibClient.Components.Input.PickerItemSelector.Popup.Actions<'T>, __componentStyles: ReactXP.Styles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.Styles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
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
                    (fun (__sibI, __sibC, __pFQN) ->
                        let __parentFQN = Some "ReactXP.Components.View"
                        ReactXP.Components.View.Make
                            (
                                let __currExplicitProps = (ReactXP.Components.TypeExtensions.TEs.MakeViewProps())
                                let __currClass = "view" + (nthChildClasses __sibI __sibC)
                                let __currStyles = (ReactXP.Styles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                let __implicitProps = [
                                    if __currClass <> "" then ("ClassName", __currClass :> obj)
                                    if (not __currStyles.IsEmpty) then ("style", ReactXP.Styles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles)
                                ]
                                (ReactXP.Styles.Runtime.injectImplicitProps __implicitProps __currExplicitProps)
                            )
                            (
                                let __list = [
                                    (
                                        if (props.Items.IsEmpty) then
                                            (fun (__sibI, __sibC, __pFQN) ->
                                                let __parentFQN = Some "ReactXP.Components.View"
                                                ReactXP.Components.View.Make
                                                    (
                                                        let __currExplicitProps = (ReactXP.Components.TypeExtensions.TEs.MakeViewProps())
                                                        let __currClass = "no-items-message" + (nthChildClasses __sibI __sibC)
                                                        let __currStyles = (ReactXP.Styles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                        let __implicitProps = [
                                                            if __currClass <> "" then ("ClassName", __currClass :> obj)
                                                            if (not __currStyles.IsEmpty) then ("style", ReactXP.Styles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles)
                                                        ]
                                                        (ReactXP.Styles.Runtime.injectImplicitProps __implicitProps __currExplicitProps)
                                                    )
                                                    (
                                                        let __list = [
                                                            ((makeTextNode "No items") |> RenderResult.Make)
                                                        ]
                                                        __list |> RenderResult.ToRawElements __parentFQN
                                                    )
                                            ) |> RenderResult.Make
                                        else RenderResult.Nothing
                                    )
                                    (
                                        match (props.ItemView) with
                                        | PickerItemView.Default toItemInfo ->
                                            (
                                                let __list = [
                                                    (
                                                        (props.Items |> OrderedSet.toSeq)
                                                        |> Seq.mapi
                                                            (fun index (item, metadata) ->
                                                                (
                                                                    let __list = [
                                                                        (fun (__sibI, __sibC, __pFQN) ->
                                                                            let __parentFQN = Some "ReactXP.Components.View"
                                                                            ReactXP.Components.View.Make
                                                                                (
                                                                                    let __currExplicitProps =
                                                                                        (ReactXP.Components.TypeExtensions.TEs.MakeViewProps(
                                                                                            ponPress = (fun e -> (props.Hide(); ReactEvent.Action.OfBrowserEvent e |> props.OnPress item))
                                                                                        ))
                                                                                    let __currClass = "button" + System.String.Format(" {0} {1}", (if (index = 0) then "first" else ""), (if (metadata.IsHighlighted) then "highlighted" else "")) + (nthChildClasses __sibI __sibC)
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
                                                                                            let __parentFQN = Some "LibClient.Components.UiText"
                                                                                            LibClient.Components.UiText.Make
                                                                                                (
                                                                                                    let __currExplicitProps = (LibClient.Components.TypeExtensions.TEs.MakeUiTextProps())
                                                                                                    let __currClass = "button-label" + System.String.Format(" {0} {1}", (if (metadata.IsSelected) then "selected" else ""), (if (metadata.IsHighlighted) then "highlighted" else "")) + (nthChildClasses __sibI __sibC)
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
                                                                                    ]
                                                                                    __list |> RenderResult.ToRawElements __parentFQN
                                                                                )
                                                                        ) |> RenderResult.Make
                                                                    ]
                                                                    __list |> RenderResult.ToLeaves
                                                                )
                                                                |> RenderResult.Make
                                                            )
                                                        |> List.ofSeq
                                                        |> RenderResult.Make
                                                    )
                                                ]
                                                __list |> RenderResult.ToRawElements __parentFQN
                                            )
                                            |> RenderResult.Make
                                        | PickerItemView.Custom render ->
                                            (
                                                let __list = [
                                                    (
                                                        (props.Items |> OrderedSet.toSeq)
                                                        |> Seq.mapi
                                                            (fun index (item, metadata) ->
                                                                (
                                                                    let __list = [
                                                                        (fun (__sibI, __sibC, __pFQN) ->
                                                                            let __parentFQN = Some "ReactXP.Components.View"
                                                                            ReactXP.Components.View.Make
                                                                                (
                                                                                    let __currExplicitProps =
                                                                                        (ReactXP.Components.TypeExtensions.TEs.MakeViewProps(
                                                                                            ponPress = (fun e -> (props.Hide(); ReactEvent.Action.OfBrowserEvent e |> props.OnPress item))
                                                                                        ))
                                                                                    let __currClass = "button" + System.String.Format(" {0} {1}", (if (index = 0) then "first" else ""), (if (metadata.IsHighlighted) then "highlighted" else "")) + (nthChildClasses __sibI __sibC)
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
                                                                                    ]
                                                                                    __list |> RenderResult.ToRawElements __parentFQN
                                                                                )
                                                                        ) |> RenderResult.Make
                                                                    ]
                                                                    __list |> RenderResult.ToLeaves
                                                                )
                                                                |> RenderResult.Make
                                                            )
                                                        |> List.ofSeq
                                                        |> RenderResult.Make
                                                    )
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
    |> RenderResult.ToRawElementsSingle __parentFQN |> RenderResult.ToSingleElement