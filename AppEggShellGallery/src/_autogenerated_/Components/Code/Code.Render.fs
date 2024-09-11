module AppEggShellGallery.Components.CodeRender

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

open AppEggShellGallery.Components.Code



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Code.Props, estate: AppEggShellGallery.Components.Code.Estate, pstate: AppEggShellGallery.Components.Code.Pstate, actions: AppEggShellGallery.Components.Code.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
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
                (
                    (props.Heading)
                    |> Option.map
                        (fun heading ->
                            let __parentFQN = Some "ReactXP.Components.View"
                            let __currClass = "heading"
                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                            ReactXP.Components.Constructors.RX.View(
                                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                children =
                                    [|
                                        makeTextNode2 __parentFQN (System.String.Format("{0}", heading))
                                    |]
                            )
                        )
                    |> Option.getOrElse noElement
                )
                match (estate.ProcessedSource) with
                | Ok code ->
                    [|
                        #if EGGSHELL_PLATFORM_IS_WEB
                        FRS.div
                            [(FRP.ClassName ("dom-user-select-text"))]
                            ([|
                                let __parentFQN = Some "ThirdParty.SyntaxHighlighter.Components.SyntaxHighlighter"
                                ThirdParty.SyntaxHighlighter.Components.Constructors.SyntaxHighlighter.SyntaxHighlighter(
                                    source = (code),
                                    language = (props.Language)
                                )
                            |])
                        #else
                        let __parentFQN = Some "ReactXP.Components.View"
                        ReactXP.Components.Constructors.RX.View(
                            children =
                                [|
                                    makeTextNode2 __parentFQN (System.String.Format("{0}", code))
                                |]
                        )
                        #endif
                    |]
                | Error message ->
                    [|
                        let __parentFQN = Some "ReactXP.Components.View"
                        let __currClass = "error"
                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                        ReactXP.Components.Constructors.RX.View(
                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                            children =
                                [|
                                    makeTextNode2 __parentFQN (System.String.Format("{0}", message))
                                |]
                        )
                    |]
                |> castAsElementAckingKeysWarning
            |]
    )
