module AppEggShellGallery.Components.ComponentContentRender

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

open AppEggShellGallery.Components.ComponentContent



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.ComponentContent.Props, estate: AppEggShellGallery.Components.ComponentContent.Estate, pstate: AppEggShellGallery.Components.ComponentContent.Pstate, actions: AppEggShellGallery.Components.ComponentContent.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "ReactXP.Components.View"
    let __currClass = "view"
    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
    ReactXP.Components.Constructors.RX.View(
        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
        children =
            [|
                let __parentFQN = Some "AppEggShellGallery.Components.ComponentContentHeading"
                AppEggShellGallery.Components.Constructors.Ui.ComponentContentHeading(
                    isResponsive = (props.IsResponsive),
                    displayName = (props.DisplayName)
                )
                (
                    if (not (props.Notes = noElement)) then
                        (castAsElementAckingKeysWarning [|
                            let __parentFQN = Some "LibClient.Components.Heading"
                            let __currClass = "heading-secondary"
                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                            LibClient.Components.Constructors.LC.Heading(
                                level = (LibClient.Components.Heading.Secondary),
                                ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                children =
                                    [|
                                        let __parentFQN = Some "LibClient.Components.LegacyText"
                                        let __currClass = "heading-secondary-text"
                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                        LibClient.Components.Constructors.LC.LegacyText(
                                            ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                            children =
                                                [|
                                                    makeTextNode2 __parentFQN "Notes"
                                                |]
                                        )
                                    |]
                            )
                            let __parentFQN = Some "ReactXP.Components.View"
                            let __currClass = "notes"
                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                            ReactXP.Components.Constructors.RX.View(
                                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                children =
                                    [|
                                        props.Notes
                                    |]
                            )
                        |])
                    else noElement
                )
                (
                    (props.Props)
                    |> Option.map
                        (fun propsConfig ->
                            (castAsElementAckingKeysWarning [|
                                let __parentFQN = Some "LibClient.Components.Heading"
                                let __currClass = "heading-secondary"
                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                LibClient.Components.Constructors.LC.Heading(
                                    level = (LibClient.Components.Heading.Secondary),
                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                    children =
                                        [|
                                            let __parentFQN = Some "LibClient.Components.LegacyText"
                                            let __currClass = "heading-secondary-text"
                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                            LibClient.Components.Constructors.LC.LegacyText(
                                                ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                children =
                                                    [|
                                                        makeTextNode2 __parentFQN "Props"
                                                    |]
                                            )
                                        |]
                                )
                                match (propsConfig) with
                                | Manual children ->
                                    [|
                                        children
                                    |]
                                | ForFullyQualifiedName fullyQualifiedName ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.ScrapedComponentProps"
                                        AppEggShellGallery.Components.Constructors.Ui.ScrapedComponentProps(
                                            fullyQualifiedName = (fullyQualifiedName)
                                        )
                                    |]
                                |> castAsElementAckingKeysWarning
                            |])
                        )
                    |> Option.getOrElse noElement
                )
                let __parentFQN = Some "LibClient.Components.Heading"
                let __currClass = "heading-secondary"
                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                LibClient.Components.Constructors.LC.Heading(
                    level = (LibClient.Components.Heading.Secondary),
                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                    children =
                        [|
                            let __parentFQN = Some "LibClient.Components.LegacyText"
                            let __currClass = "heading-secondary-text"
                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                            LibClient.Components.Constructors.LC.LegacyText(
                                ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                children =
                                    [|
                                        makeTextNode2 __parentFQN "Samples"
                                    |]
                            )
                        |]
                )
                let __parentFQN = Some "ReactXP.Components.ScrollView"
                ReactXP.Components.Constructors.RX.ScrollView(
                    horizontal = (true),
                    children =
                        [|
                            #if EGGSHELL_PLATFORM_IS_WEB
                            FRS.table
                                [(FRP.ClassName ("aesg-ContentComponent-table"))]
                                ([|
                                    FRS.tbody
                                        []
                                        ([|
                                            props.Samples
                                        |])
                                |])
                            #else
                            let __parentFQN = Some "ReactXP.Components.View"
                            ReactXP.Components.Constructors.RX.View(
                                children =
                                    [|
                                        props.Samples
                                    |]
                            )
                            #endif
                            (
                                if (not (props.ThemeSamples = noElement)) then
                                    (castAsElementAckingKeysWarning [|
                                        let __parentFQN = Some "LibClient.Components.Heading"
                                        let __currClass = "heading-secondary"
                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                        LibClient.Components.Constructors.LC.Heading(
                                            level = (LibClient.Components.Heading.Secondary),
                                            ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                            children =
                                                [|
                                                    let __parentFQN = Some "LibClient.Components.LegacyText"
                                                    let __currClass = "heading-secondary-text"
                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                    LibClient.Components.Constructors.LC.LegacyText(
                                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                        children =
                                                            [|
                                                                makeTextNode2 __parentFQN "Theme"
                                                            |]
                                                    )
                                                |]
                                        )
                                        #if EGGSHELL_PLATFORM_IS_WEB
                                        FRS.table
                                            [(FRP.ClassName ("aesg-ContentComponent-table"))]
                                            ([|
                                                FRS.tbody
                                                    []
                                                    ([|
                                                        props.ThemeSamples
                                                    |])
                                            |])
                                        #else
                                        let __parentFQN = Some "ReactXP.Components.View"
                                        ReactXP.Components.Constructors.RX.View(
                                            children =
                                                [|
                                                    props.ThemeSamples
                                                |]
                                        )
                                        #endif
                                    |])
                                else noElement
                            )
                        |]
                )
            |]
    )
