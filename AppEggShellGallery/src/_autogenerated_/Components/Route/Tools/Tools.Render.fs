module AppEggShellGallery.Components.Route.ToolsRender

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

open AppEggShellGallery.Components.Route.Tools



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Route.Tools.Props, estate: AppEggShellGallery.Components.Route.Tools.Estate, pstate: AppEggShellGallery.Components.Route.Tools.Pstate, actions: AppEggShellGallery.Components.Route.Tools.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    (castAsElementAckingKeysWarning [|
        let __parentFQN = Some "LibClient.Components.SetPageMetadata"
        LibClient.Components.Constructors.LC.SetPageMetadata(
            title = ("Tools")
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
                                let __parentFQN = Some "ThirdParty.Showdown.Components.MarkdownViewer"
                                ThirdParty.Showdown.Components.Constructors.Showdown.MarkdownViewer(
                                    showdownConverter = (showdownConverterWithSyntaxHighlighting),
                                    globalLinkHandler = ("globalMarkdownLinkHandler"),
                                    source = (ThirdParty.Showdown.Components.MarkdownViewer.Url ("/docs/" + props.MarkdownUrl |> services().Http.PrepareInBundleResourceUrl))
                                )
                                (
                                    if (props.MarkdownUrl = "tools/snippets.md") then
                                        (castAsElementAckingKeysWarning [|
                                            let __parentFQN = Some "ThirdParty.Showdown.Components.MarkdownViewer"
                                            ThirdParty.Showdown.Components.Constructors.Showdown.MarkdownViewer(
                                                source = (ThirdParty.Showdown.Components.MarkdownViewer.Code "## RenderDSL scope snippets")
                                            )
                                            let __parentFQN = Some "AppEggShellGallery.Components.Snippets"
                                            AppEggShellGallery.Components.Constructors.Ui.Snippets(
                                                scope = (AppEggShellGallery.Components.Snippets.One "renderdsl")
                                            )
                                            let __parentFQN = Some "ThirdParty.Showdown.Components.MarkdownViewer"
                                            ThirdParty.Showdown.Components.Constructors.Showdown.MarkdownViewer(
                                                source = (ThirdParty.Showdown.Components.MarkdownViewer.Code "## F# scope snippets")
                                            )
                                            let __parentFQN = Some "AppEggShellGallery.Components.Snippets"
                                            AppEggShellGallery.Components.Constructors.Ui.Snippets(
                                                scope = (AppEggShellGallery.Components.Snippets.One "fsharp")
                                            )
                                        |])
                                    else noElement
                                )
                            |]
                    )
                |]
        )
    |])
