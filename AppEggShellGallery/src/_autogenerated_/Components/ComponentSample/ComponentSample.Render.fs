module AppEggShellGallery.Components.ComponentSampleRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard


open LibClient.Components
open LibRouter.Components
open ThirdParty.Map.Components
open ReactXP.Components
open ThirdParty.Recharts.Components
open ThirdParty.Showdown.Components
open ThirdParty.SyntaxHighlighter.Components
open LibUiAdmin.Components
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

open AppEggShellGallery.Components.ComponentSample



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.ComponentSample.Props, estate: AppEggShellGallery.Components.ComponentSample.Estate, pstate: AppEggShellGallery.Components.ComponentSample.Pstate, actions: AppEggShellGallery.Components.ComponentSample.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    (castAsElementAckingKeysWarning [|
        #if EGGSHELL_PLATFORM_IS_WEB
        (
            (props.Heading)
            |> Option.map
                (fun heading ->
                    FRS.tr
                        [(FRP.ClassName ("cs-heading"))]
                        ([|
                            FRS.td
                                [(FRP.ColSpan ((2)))]
                                ([|
                                    let __parentFQN = Some "LibClient.Components.Heading"
                                    let __currClass = "heading"
                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                    LibClient.Components.Constructors.LC.Heading(
                                        level = (LibClient.Components.Heading.Tertiary),
                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                        children =
                                            [|
                                                makeTextNode2 __parentFQN (System.String.Format("{0}", heading))
                                            |]
                                    )
                                |])
                        |])
                )
            |> Option.getOrElse noElement
        )
        FRS.tr
            []
            ([|
                FRS.td
                    [(FRP.ClassName (System.String.Format(" {0} {1} {2}", (if (props.VerticalAlignment = VerticalAlignment.Middle) then "vertical-align-middle" else ""), (if (props.VerticalAlignment = VerticalAlignment.Top) then "vertical-align-top" else ""), (if (props.Layout            = Layout.CodeBelowSamples) then "layout-code-below-samples" else ""))))]
                    ([|
                        let __parentFQN = Some "ReactXP.Components.View"
                        let __currClass = "visuals"
                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                        ReactXP.Components.Constructors.RX.View(
                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                            children =
                                [|
                                    let __parentFQN = Some "LibClient.Components.With.Context"
                                    LibClient.Components.Constructors.LC.With.Context(
                                        context = (AppEggShellGallery.SampleVisualsScreenSize.sampleVisualsScreenSizeContext),
                                        ``with`` =
                                            (fun (sampleVisualsScreenSize: ScreenSize) ->
                                                    (castAsElementAckingKeysWarning [|
                                                        let __parentFQN = Some "LibClient.Components.ForceContext"
                                                        LibClient.Components.Constructors.LC.ForceContext(
                                                            value = (sampleVisualsScreenSize),
                                                            context = (screenSizeContext),
                                                            children =
                                                                [|
                                                                    props.Visuals
                                                                |]
                                                        )
                                                    |])
                                            )
                                    )
                                |]
                        )
                    |])
                FRS.td
                    [(FRP.ClassName (System.String.Format(" {0}", (if (props.Layout = Layout.CodeBelowSamples) then "layout-code-below-samples" else ""))))]
                    ([|
                        let __parentFQN = Some "ReactXP.Components.View"
                        let __currClass = "code-and-notes"
                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                        ReactXP.Components.Constructors.RX.View(
                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                            children =
                                [|
                                    let __parentFQN = Some "ReactXP.Components.View"
                                    let __currClass = "code"
                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                    ReactXP.Components.Constructors.RX.View(
                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                        children =
                                            [|
                                                match (props.Code) with
                                                | SingleBlock (language, code) ->
                                                    [|
                                                        let __parentFQN = Some "AppEggShellGallery.Components.Code"
                                                        AppEggShellGallery.Components.Constructors.Ui.Code(
                                                            language = (language),
                                                            children =
                                                                [|
                                                                    code
                                                                |]
                                                        )
                                                    |]
                                                | Children children ->
                                                    [|
                                                        children
                                                    |]
                                                |> castAsElementAckingKeysWarning
                                            |]
                                    )
                                    (
                                        if (not (props.Notes = noElement)) then
                                            let __parentFQN = Some "ReactXP.Components.View"
                                            ReactXP.Components.Constructors.RX.View(
                                                children =
                                                    [|
                                                        props.Notes
                                                    |]
                                            )
                                        else noElement
                                    )
                                |]
                        )
                    |])
            |])
        #else
        (
            (props.Heading)
            |> Option.map
                (fun heading ->
                    let __parentFQN = Some "LibClient.Components.Heading"
                    let __currClass = "heading"
                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                    LibClient.Components.Constructors.LC.Heading(
                        level = (LibClient.Components.Heading.Tertiary),
                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                        children =
                            [|
                                makeTextNode2 __parentFQN (System.String.Format("{0}", heading))
                            |]
                    )
                )
            |> Option.getOrElse noElement
        )
        props.Visuals
        let __parentFQN = Some "ReactXP.Components.View"
        let __currClass = "code-and-notes"
        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
        ReactXP.Components.Constructors.RX.View(
            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
            children =
                [|
                    let __parentFQN = Some "ReactXP.Components.View"
                    let __currClass = "code"
                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                    ReactXP.Components.Constructors.RX.View(
                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                        children =
                            [|
                                match (props.Code) with
                                | SingleBlock (language, code) ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Code"
                                        AppEggShellGallery.Components.Constructors.Ui.Code(
                                            language = (language),
                                            children =
                                                [|
                                                    code
                                                |]
                                        )
                                    |]
                                | Children children ->
                                    [|
                                        children
                                    |]
                                |> castAsElementAckingKeysWarning
                            |]
                    )
                    (
                        if (not (props.Notes = noElement)) then
                            let __parentFQN = Some "ReactXP.Components.View"
                            ReactXP.Components.Constructors.RX.View(
                                children =
                                    [|
                                        props.Notes
                                    |]
                            )
                        else noElement
                    )
                |]
        )
        #endif
    |])
