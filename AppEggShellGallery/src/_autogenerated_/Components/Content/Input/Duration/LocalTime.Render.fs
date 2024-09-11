module AppEggShellGallery.Components.Content.Input.DurationRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard

open LibClient.RenderResultModule

open LibUiAdmin.Components
open LibClient.Components
open LibRouter.Components
open ThirdParty.Map.Components
open ReactXP.Components
open ThirdParty.Recharts.Components
open ThirdParty.Showdown.Components
open ThirdParty.SyntaxHighlighter.Components
open AppEggShellGallery.Components

open LibLang
open LibClient
open LibClient.Services.Subscription
open LibClient.RenderHelpers
open LibClient.Chars
open LibClient.ColorModule
open LibClient.Responsive
open AppEggShellGallery.RenderHelpers
open AppEggShellGallery.Navigation
open AppEggShellGallery.LocalImages
open AppEggShellGallery.Icons
open AppEggShellGallery.AppServices
open AppEggShellGallery

open AppEggShellGallery.Components.Content.Input.Duration



let render(props: AppEggShellGallery.Components.Content.Input.Duration.Props, estate: AppEggShellGallery.Components.Content.Input.Duration.Estate, pstate: AppEggShellGallery.Components.Content.Input.Duration.Pstate, actions: AppEggShellGallery.Components.Content.Input.Duration.Actions, __componentStyles: ReactXP.Styles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.Styles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    (fun (__sibI, __sibC, __pFQN) ->
        let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
        AppEggShellGallery.Components.ComponentContent.Make
            (
                let __currExplicitProps =
                    (AppEggShellGallery.Components.TypeExtensions.TEs.MakeComponentContentProps(
                        pProps = (AppEggShellGallery.Components.ComponentContent.ForFullyQualifiedName "LibClient.Components.Input.LocalTime"),
                        pDisplayName = ("Input.LocalTime"),
                        pSamples =
                            FRH.fragment []
                                (
                                    let __list = [
                                        (fun (__sibI, __sibC, __pFQN) ->
                                            let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                                            AppEggShellGallery.Components.ComponentSampleGroup.Make
                                                (
                                                    let __currExplicitProps =
                                                        (AppEggShellGallery.Components.TypeExtensions.TEs.MakeComponentSampleGroupProps(
                                                            pHeading = ("Modes"),
                                                            pSamples =
                                                                FRH.fragment []
                                                                    (
                                                                        let __list = [
                                                                            (fun (__sibI, __sibC, __pFQN) ->
                                                                                let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                                                                                AppEggShellGallery.Components.ComponentSample.Make
                                                                                    (
                                                                                        let __currExplicitProps =
                                                                                            (AppEggShellGallery.Components.TypeExtensions.TEs.MakeComponentSampleProps(
                                                                                                pCode =
                                                                                                    (
                                                                                                        AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                                                                                            (
                                                                                                                FRH.fragment []
                                                                                                                    (
                                                                                                                        let __list = [
                                                                                                                            @"
                    <LC.Input.LocalTime
                     Label='""Start Time""'
                     Value='estate.Values.Item ""A""'
                     OnChange='actions.OnChange ""A""'
                     Validity='Valid'/>
                "
                                                                                                                            |> makeTextNode |> RenderResult.Make
                                                                                                                        ]
                                                                                                                        __list |> RenderResult.ToRawElements __parentFQN
                                                                                                                    )
                                                                                                            )
                                                                                                    ),
                                                                                                pVisuals =
                                                                                                    FRH.fragment []
                                                                                                        (
                                                                                                            let __list = [
                                                                                                                (fun (__sibI, __sibC, __pFQN) ->
                                                                                                                    let __parentFQN = Some "LibClient.Components.Input.LocalTime"
                                                                                                                    LibClient.Components.Input.LocalTime.Make
                                                                                                                        (
                                                                                                                            let __currExplicitProps =
                                                                                                                                (LibClient.Components.TypeExtensions.TEs.MakeInput_LocalTimeProps(
                                                                                                                                    pValidity = (Valid),
                                                                                                                                    pOnChange = (actions.OnChange "A"),
                                                                                                                                    pValue = (estate.Values.Item "A"),
                                                                                                                                    pLabel = ("Start Time")
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
                                            let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                                            AppEggShellGallery.Components.ComponentSampleGroup.Make
                                                (
                                                    let __currExplicitProps =
                                                        (AppEggShellGallery.Components.TypeExtensions.TEs.MakeComponentSampleGroupProps(
                                                            pHeading = ("Validation (hardcoded, not dynamic)"),
                                                            pSamples =
                                                                FRH.fragment []
                                                                    (
                                                                        let __list = [
                                                                            (fun (__sibI, __sibC, __pFQN) ->
                                                                                let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                                                                                AppEggShellGallery.Components.ComponentSample.Make
                                                                                    (
                                                                                        let __currExplicitProps =
                                                                                            (AppEggShellGallery.Components.TypeExtensions.TEs.MakeComponentSampleProps(
                                                                                                pCode =
                                                                                                    (
                                                                                                        AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                                                                                            (
                                                                                                                FRH.fragment []
                                                                                                                    (
                                                                                                                        let __list = [
                                                                                                                            @"
                    <LC.Input.LocalTime
                     Label='""Start Time""'
                     Value='LibClient.Components.Input.LocalTime.empty'
                     OnChange='ignore'
                     Validity='Missing'/>
                "
                                                                                                                            |> makeTextNode |> RenderResult.Make
                                                                                                                        ]
                                                                                                                        __list |> RenderResult.ToRawElements __parentFQN
                                                                                                                    )
                                                                                                            )
                                                                                                    ),
                                                                                                pVisuals =
                                                                                                    FRH.fragment []
                                                                                                        (
                                                                                                            let __list = [
                                                                                                                (fun (__sibI, __sibC, __pFQN) ->
                                                                                                                    let __parentFQN = Some "LibClient.Components.Input.LocalTime"
                                                                                                                    LibClient.Components.Input.LocalTime.Make
                                                                                                                        (
                                                                                                                            let __currExplicitProps =
                                                                                                                                (LibClient.Components.TypeExtensions.TEs.MakeInput_LocalTimeProps(
                                                                                                                                    pValidity = (Missing),
                                                                                                                                    pOnChange = (ignore),
                                                                                                                                    pValue = (LibClient.Components.Input.LocalTime.empty),
                                                                                                                                    pLabel = ("Start Time")
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
                                                                                let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                                                                                AppEggShellGallery.Components.ComponentSample.Make
                                                                                    (
                                                                                        let __currExplicitProps =
                                                                                            (AppEggShellGallery.Components.TypeExtensions.TEs.MakeComponentSampleProps(
                                                                                                pCode =
                                                                                                    (
                                                                                                        AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                                                                                            (
                                                                                                                FRH.fragment []
                                                                                                                    (
                                                                                                                        let __list = [
                                                                                                                            @"
                    <LC.Input.LocalTime
                     Label='""Start Time""'
                     Value='LibClient.Components.Input.LocalTime.empty'
                     OnChange='ignore'
                     Validity='Invalid ""That is just a bad time""'/>
                "
                                                                                                                            |> makeTextNode |> RenderResult.Make
                                                                                                                        ]
                                                                                                                        __list |> RenderResult.ToRawElements __parentFQN
                                                                                                                    )
                                                                                                            )
                                                                                                    ),
                                                                                                pVisuals =
                                                                                                    FRH.fragment []
                                                                                                        (
                                                                                                            let __list = [
                                                                                                                (fun (__sibI, __sibC, __pFQN) ->
                                                                                                                    let __parentFQN = Some "LibClient.Components.Input.LocalTime"
                                                                                                                    LibClient.Components.Input.LocalTime.Make
                                                                                                                        (
                                                                                                                            let __currExplicitProps =
                                                                                                                                (LibClient.Components.TypeExtensions.TEs.MakeInput_LocalTimeProps(
                                                                                                                                    pValidity = (Invalid "That is just a bad time"),
                                                                                                                                    pOnChange = (ignore),
                                                                                                                                    pValue = (LibClient.Components.Input.LocalTime.empty),
                                                                                                                                    pLabel = ("Start Time")
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