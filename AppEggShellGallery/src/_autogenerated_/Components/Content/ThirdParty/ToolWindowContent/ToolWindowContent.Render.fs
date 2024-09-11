module AppEggShellGallery.Components.Content.ThirdParty.ToolWindowContentRender

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

open AppEggShellGallery.Components.Content.ThirdParty.ToolWindowContent



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.ThirdParty.ToolWindowContent.Props, estate: AppEggShellGallery.Components.Content.ThirdParty.ToolWindowContent.Estate, pstate: AppEggShellGallery.Components.Content.ThirdParty.ToolWindowContent.Pstate, actions: AppEggShellGallery.Components.Content.ThirdParty.ToolWindowContent.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "ReactXP.Components.View"
    let __currClass = "content"
    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
    ReactXP.Components.Constructors.RX.View(
        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
        children =
            [|
                let __parentFQN = Some "LibClient.Components.Heading"
                LibClient.Components.Constructors.LC.Heading(
                    level = (LibClient.Components.Heading.Secondary),
                    children =
                        [|
                            makeTextNode2 __parentFQN "Info Window"
                        |]
                )
                let __parentFQN = Some "LibClient.Components.LegacyText"
                LibClient.Components.Constructors.LC.LegacyText(
                    children =
                        [|
                            makeTextNode2 __parentFQN "This is an example of an info window."
                        |]
                )
                let __parentFQN = Some "LibClient.Components.Button"
                LibClient.Components.Constructors.LC.Button(
                    state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (LibClient.Components.Button.Actionable (fun _ -> props.Handle.Close()))),
                    label = ("Close")
                )
            |]
    )
