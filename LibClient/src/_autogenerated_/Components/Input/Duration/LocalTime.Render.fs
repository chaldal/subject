module LibClient.Components.Input.DurationRender

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

open LibClient.Components.Input.Duration



let render(props: LibClient.Components.Input.Duration.Props, estate: LibClient.Components.Input.Duration.Estate, pstate: LibClient.Components.Input.Duration.Pstate, actions: LibClient.Components.Input.Duration.Actions, __componentStyles: ReactXP.Styles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.Styles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    (
        let externalValidityForFields = match props.Validity with Valid -> Valid | _ -> Missing
        let (rawHours, rawMinutes, rawPeriodOffset) = (
            props.Value.Raw;
             
        )
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
                            (props.Label)
                            |> Option.map
                                (fun label -> RenderResult.Make [
                                    (fun (__sibI, __sibC, __pFQN) ->
                                        let __parentFQN = Some "ReactXP.Components.View"
                                        ReactXP.Components.View.Make
                                            (
                                                let __currExplicitProps =
                                                    (ReactXP.Components.TypeExtensions.TEs.MakeViewProps(
                                                        ponPress = (actions.Focus)
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
                                                        let __parentFQN = Some "LibClient.Components.Text"
                                                        LibClient.Components.Text.Make
                                                            (
                                                                let __currExplicitProps = (LibClient.Components.TypeExtensions.TEs.MakeTextProps())
                                                                let __currClass = "label" + System.String.Format(" {0} {1}", (if ((props.Value.InternalValidity.Or props.Validity).IsInvalid) then "invalid" else ""), (if (estate.IsFocused) then "focused" else "")) + (nthChildClasses __sibI __sibC)
                                                                let __currStyles = (ReactXP.Styles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                let __implicitProps = [
                                                                    if __currClass <> "" then ("ClassName", __currClass :> obj)
                                                                    if (not __currStyles.IsEmpty) then ("__style", __currStyles :> obj)
                                                                ]
                                                                (ReactXP.Styles.Runtime.injectImplicitProps __implicitProps __currExplicitProps)
                                                            )
                                                            (
                                                                let __list = [
                                                                    ((makeTextNode (System.String.Format("{0}", label))) |> RenderResult.Make)
                                                                ]
                                                                __list |> RenderResult.ToRawElements __parentFQN
                                                            )
                                                    ) |> RenderResult.Make
                                                ]
                                                __list |> RenderResult.ToRawElements __parentFQN
                                            )
                                    ) |> RenderResult.Make
                                ])
                            |> Option.getOrElse RenderResult.Nothing
                        )
                        (fun (__sibI, __sibC, __pFQN) ->
                            let __parentFQN = Some "ReactXP.Components.View"
                            ReactXP.Components.View.Make
                                (
                                    let __currExplicitProps = (ReactXP.Components.TypeExtensions.TEs.MakeViewProps())
                                    let __currClass = "fields" + (nthChildClasses __sibI __sibC)
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
                                            let __parentFQN = Some "LibClient.Components.Input.Text"
                                            LibClient.Components.Input.Text.Make
                                                (
                                                    let __currExplicitProps =
                                                        (LibClient.Components.TypeExtensions.TEs.MakeInput_TextProps(
                                                            pref = (actions.RefHoursInput),
                                                            pOnEnterKeyPressOption = (props.OnEnterKeyPress),
                                                            pOnBlur = (actions.OnBlur),
                                                            pOnFocus = (actions.OnFocus),
                                                            pOnChange = (props.Value.SetHours >> props.OnChange),
                                                            pRequestFocusOnMount = (props.RequestFocusOnMount),
                                                            pPlaceholder = ("00"),
                                                            pValidity = ((props.Value.InternalFieldValidity Hours).Or externalValidityForFields),
                                                            pMaxLength = (2),
                                                            pValue = (rawHours)
                                                        ))
                                                    let __currClass = "field" + (nthChildClasses __sibI __sibC)
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
                                            let __parentFQN = Some "LibClient.Components.Text"
                                            LibClient.Components.Text.Make
                                                (
                                                    let __currExplicitProps = (LibClient.Components.TypeExtensions.TEs.MakeTextProps())
                                                    let __currClass = "colon" + (nthChildClasses __sibI __sibC)
                                                    let __currStyles = (ReactXP.Styles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                    let __implicitProps = [
                                                        if __currClass <> "" then ("ClassName", __currClass :> obj)
                                                        if (not __currStyles.IsEmpty) then ("__style", __currStyles :> obj)
                                                    ]
                                                    (ReactXP.Styles.Runtime.injectImplicitProps __implicitProps __currExplicitProps)
                                                )
                                                (
                                                    let __list = [
                                                        ((makeTextNode ":") |> RenderResult.Make)
                                                    ]
                                                    __list |> RenderResult.ToRawElements __parentFQN
                                                )
                                        ) |> RenderResult.Make
                                        (fun (__sibI, __sibC, __pFQN) ->
                                            let __parentFQN = Some "LibClient.Components.Input.Text"
                                            LibClient.Components.Input.Text.Make
                                                (
                                                    let __currExplicitProps =
                                                        (LibClient.Components.TypeExtensions.TEs.MakeInput_TextProps(
                                                            pOnEnterKeyPressOption = (props.OnEnterKeyPress),
                                                            pOnBlur = (actions.OnBlur),
                                                            pOnFocus = (actions.OnFocus),
                                                            pOnChange = (props.Value.SetMinutes >> props.OnChange),
                                                            pPlaceholder = ("00"),
                                                            pValidity = ((props.Value.InternalFieldValidity Minutes).Or externalValidityForFields),
                                                            pMaxLength = (2),
                                                            pValue = (rawMinutes)
                                                        ))
                                                    let __currClass = "field" + (nthChildClasses __sibI __sibC)
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
                                            let __parentFQN = Some "LibClient.Components.Legacy.Input.Picker"
                                            LibClient.Components.Legacy.Input.Picker.Make
                                                (
                                                    let __currExplicitProps =
                                                        (LibClient.Components.TypeExtensions.TEs.MakeLegacy_Input_PickerProps(
                                                            pValidity = (externalValidityForFields),
                                                            pOnChange = (LibClient.Components.Legacy.Input.Picker.CannotUnselect (snd >> props.Value.SetPeriod >> props.OnChange)),
                                                            pValue = (LibClient.Components.Legacy.Input.Picker.ByItem rawPeriodOffset |> Some),
                                                            pItems = (periodPickerItems)
                                                        ))
                                                    let __currClass = "picker" + (nthChildClasses __sibI __sibC)
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
                        (
                            ((props.Value.InternalValidity.Or props.Validity).InvalidReason)
                            |> Option.map
                                (fun reason -> RenderResult.Make [
                                    (fun (__sibI, __sibC, __pFQN) ->
                                        let __parentFQN = Some "ReactXP.Components.View"
                                        ReactXP.Components.View.Make
                                            (
                                                let __currExplicitProps = (ReactXP.Components.TypeExtensions.TEs.MakeViewProps())
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
                                                        let __parentFQN = Some "LibClient.Components.Text"
                                                        LibClient.Components.Text.Make
                                                            (
                                                                let __currExplicitProps = (LibClient.Components.TypeExtensions.TEs.MakeTextProps())
                                                                let __currClass = "invalid-reason" + (nthChildClasses __sibI __sibC)
                                                                let __currStyles = (ReactXP.Styles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                let __implicitProps = [
                                                                    if __currClass <> "" then ("ClassName", __currClass :> obj)
                                                                    if (not __currStyles.IsEmpty) then ("__style", __currStyles :> obj)
                                                                ]
                                                                (ReactXP.Styles.Runtime.injectImplicitProps __implicitProps __currExplicitProps)
                                                            )
                                                            (
                                                                let __list = [
                                                                    ((makeTextNode (System.String.Format("{0}", reason))) |> RenderResult.Make)
                                                                ]
                                                                __list |> RenderResult.ToRawElements __parentFQN
                                                            )
                                                    ) |> RenderResult.Make
                                                ]
                                                __list |> RenderResult.ToRawElements __parentFQN
                                            )
                                    ) |> RenderResult.Make
                                ])
                            |> Option.getOrElse RenderResult.Nothing
                        )
                    ]
                    __list |> RenderResult.ToRawElements __parentFQN
                )
        ) |> RenderResult.Make
    )
    |> RenderResult.ToRawElementsSingle __parentFQN |> RenderResult.ToSingleElement