module AppEggShellGallery.Components.Route.DesignRender

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

open AppEggShellGallery.Components.Route.Design



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Route.Design.Props, estate: AppEggShellGallery.Components.Route.Design.Estate, pstate: AppEggShellGallery.Components.Route.Design.Pstate, actions: AppEggShellGallery.Components.Route.Design.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    (castAsElementAckingKeysWarning [|
        let __parentFQN = Some "LibClient.Components.SetPageMetadata"
        LibClient.Components.Constructors.LC.SetPageMetadata(
            title = ("Design")
        )
        let __parentFQN = Some "LibRouter.Components.Route"
        LibRouter.Components.Constructors.LR.Route(
            scroll = (LibRouter.Components.Route.Vertical),
            children =
                [|
                    let __parentFQN = Some "LibClient.Components.Section.Padded"
                    LibClient.Components.Constructors.LC.Section.Padded(
                        children =
                            [|
                                match (props.Item) with
                                | DesignItem.Markdown url ->
                                    [|
                                        let __parentFQN = Some "ThirdParty.Showdown.Components.MarkdownViewer"
                                        ThirdParty.Showdown.Components.Constructors.Showdown.MarkdownViewer(
                                            showdownConverter = (showdownConverterWithSyntaxHighlighting),
                                            globalLinkHandler = ("globalMarkdownLinkHandler"),
                                            source = (ThirdParty.Showdown.Components.MarkdownViewer.Url ("/docs/" + url |> services().Http.PrepareInBundleResourceUrl))
                                        )
                                    |]
                                | ColorVariants ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.ColorVariants"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.ColorVariants()
                                    |]
                                | Icons ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Icons"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Icons()
                                    |]
                                |> castAsElementAckingKeysWarning
                            |]
                    )
                |]
        )
    |])
