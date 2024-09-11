module ThirdParty.Showdown.Components.MarkdownViewerRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard


open LibClient.Components
open ReactXP.Components
open ThirdParty.Showdown.Components

open LibClient
open LibClient.RenderHelpers

open ThirdParty.Showdown.Components.MarkdownViewer



let render(children: array<ReactElement>, props: ThirdParty.Showdown.Components.MarkdownViewer.Props, estate: ThirdParty.Showdown.Components.MarkdownViewer.Estate, pstate: ThirdParty.Showdown.Components.MarkdownViewer.Pstate, actions: ThirdParty.Showdown.Components.MarkdownViewer.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "LibClient.Components.AsyncData"
    LibClient.Components.Constructors.LC.AsyncData(
        data = (estate.SourceCode),
        whenAvailable =
            (fun (sourceCode: string) ->
                    (castAsElementAckingKeysWarning [|
                        #if EGGSHELL_PLATFORM_IS_WEB
                         makeHtml props.ShowdownConverter props.GlobalLinkHandler (props.ImageUrlTransformer |> Option.map (fun f -> f props.Source)) sourceCode
                        #endif
                    |])
            )
    )
