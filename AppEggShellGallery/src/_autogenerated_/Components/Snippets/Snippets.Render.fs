module AppEggShellGallery.Components.SnippetsRender

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

open AppEggShellGallery.Components.Snippets



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Snippets.Props, estate: AppEggShellGallery.Components.Snippets.Estate, pstate: AppEggShellGallery.Components.Snippets.Pstate, actions: AppEggShellGallery.Components.Snippets.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "ReactXP.Components.View"
    ReactXP.Components.Constructors.RX.View(
        children =
            [|
                match (AppEggShellGallery.ScrapedData.RenderDslSnippets.renderDslSnippetDataResult) with
                | Ok renderDslSnippetData ->
                    [|
                        match (renderDslSnippetData) with
                        | Ok snippets ->
                            [|
                                (
                                    let filteredSnippets = snippets |> List.filter props.Scope.Filter
                                    FRS.table
                                        [(FRP.ClassName ("aesg-Snippets-table dom-user-select-text"))]
                                        ([|
                                            FRS.tbody
                                                []
                                                ([|
                                                    FRS.tr
                                                        []
                                                        ([|
                                                            FRS.th
                                                                []
                                                                ([|
                                                                    makeTextNode2 __parentFQN "Name"
                                                                |])
                                                            FRS.th
                                                                []
                                                                ([|
                                                                    makeTextNode2 __parentFQN "Prefix"
                                                                |])
                                                            FRS.th
                                                                []
                                                                ([|
                                                                    makeTextNode2 __parentFQN "Description"
                                                                |])
                                                            (
                                                                if (props.Scope = All) then
                                                                    FRS.th
                                                                        []
                                                                        ([|
                                                                            makeTextNode2 __parentFQN "Scope"
                                                                        |])
                                                                else noElement
                                                            )
                                                        |])
                                                    (
                                                        (filteredSnippets)
                                                        |> Seq.map
                                                            (fun snippet ->
                                                                FRS.tr
                                                                    []
                                                                    ([|
                                                                        FRS.td
                                                                            [(FRP.ClassName ("nowrap"))]
                                                                            ([|
                                                                                makeTextNode2 __parentFQN (System.String.Format("{0}", snippet.Key))
                                                                            |])
                                                                        FRS.td
                                                                            [(FRP.ClassName ("nowrap"))]
                                                                            ([|
                                                                                makeTextNode2 __parentFQN (System.String.Format("{0}", snippet.Prefix))
                                                                            |])
                                                                        FRS.td
                                                                            [(FRP.ClassName ("description"))]
                                                                            ([|
                                                                                let __parentFQN = Some "ThirdParty.Showdown.Components.MarkdownViewer"
                                                                                ThirdParty.Showdown.Components.Constructors.Showdown.MarkdownViewer(
                                                                                    globalLinkHandler = ("globalMarkdownLinkHandler"),
                                                                                    source = (ThirdParty.Showdown.Components.MarkdownViewer.Code snippet.Description)
                                                                                )
                                                                            |])
                                                                        (
                                                                            if (props.Scope = All) then
                                                                                FRS.td
                                                                                    []
                                                                                    ([|
                                                                                        makeTextNode2 __parentFQN (System.String.Format("{0}", snippet.Scope))
                                                                                    |])
                                                                            else noElement
                                                                        )
                                                                    |])
                                                            )
                                                        |> Array.ofSeq |> castAsElement
                                                    )
                                                |])
                                        |])
                                )
                            |]
                        | Error error ->
                            [|
                                let __parentFQN = Some "ReactXP.Components.View"
                                let __currClass = "error"
                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                ReactXP.Components.Constructors.RX.View(
                                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                    children =
                                        [|
                                            let __parentFQN = Some "LibClient.Components.LegacyText"
                                            let __currClass = "error-text"
                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                            LibClient.Components.Constructors.LC.LegacyText(
                                                ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                children =
                                                    [|
                                                        makeTextNode2 __parentFQN (System.String.Format("{0}", error))
                                                    |]
                                            )
                                        |]
                                )
                            |]
                        |> castAsElementAckingKeysWarning
                    |]
                | Error error ->
                    [|
                        let __parentFQN = Some "ReactXP.Components.View"
                        let __currClass = "error"
                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                        ReactXP.Components.Constructors.RX.View(
                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                            children =
                                [|
                                    let __parentFQN = Some "LibClient.Components.LegacyText"
                                    let __currClass = "error-text"
                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                    LibClient.Components.Constructors.LC.LegacyText(
                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                        children =
                                            [|
                                                makeTextNode2 __parentFQN (System.String.Format("{0}", error))
                                            |]
                                    )
                                |]
                        )
                    |]
                |> castAsElementAckingKeysWarning
            |]
    )
