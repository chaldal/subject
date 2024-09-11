module AppEggShellGallery.Components.Content.Section.PaddedRender

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

open AppEggShellGallery.Components.Content.Section.Padded



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.Section.Padded.Props, estate: AppEggShellGallery.Components.Content.Section.Padded.Estate, pstate: AppEggShellGallery.Components.Content.Section.Padded.Pstate, actions: AppEggShellGallery.Components.Content.Section.Padded.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
    AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
        props = (AppEggShellGallery.Components.ComponentContent.ForFullyQualifiedName "LibClient.Components.Section.Padded"),
        isResponsive = (true),
        displayName = ("Section.Padded"),
        samples =
                (castAsElementAckingKeysWarning [|
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                        code =
                            (
                                AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                    (
                                            (castAsElementAckingKeysWarning [|
                                                @"
                <LC.Section.Padded>
                    <uitext>Sample uitext block wrapped by Section.Padded to get extra padding.</uitext>
                </LC.Section.Padded>
                <uitext>Sample uitext outside Section.Padded</uitext>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.Section.Padded"
                                    LibClient.Components.Constructors.LC.Section.Padded(
                                        children =
                                            [|
                                                let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                                LibClient.Components.Constructors.LC.LegacyUiText(
                                                    children =
                                                        [|
                                                            makeTextNode2 __parentFQN "Sample uitext block wrapped by Section.Padded to get extra padding."
                                                        |]
                                                )
                                            |]
                                    )
                                    let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                    LibClient.Components.Constructors.LC.LegacyUiText(
                                        children =
                                            [|
                                                makeTextNode2 __parentFQN "Sample uitext outside Section.Padded"
                                            |]
                                    )
                                |])
                    )
                |])
    )
