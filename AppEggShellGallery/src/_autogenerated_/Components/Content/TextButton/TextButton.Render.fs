module AppEggShellGallery.Components.Content.TextButtonRender

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

open AppEggShellGallery.Components.Content.TextButton



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.TextButton.Props, estate: AppEggShellGallery.Components.Content.TextButton.Estate, pstate: AppEggShellGallery.Components.Content.TextButton.Pstate, actions: AppEggShellGallery.Components.Content.TextButton.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
    AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
        props = (AppEggShellGallery.Components.ComponentContent.ForFullyQualifiedName "LibClient.Components.TextButton"),
        displayName = ("TextButton"),
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
                <LC.TextButton rt-fs='true'
                 Label='""Add to Cart""'
                 State='^LowLevel(LC.TextButton.Actionable Actions.demoPointerEventAction)'/>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.TextButton"
                                    LibClient.Components.Constructors.LC.TextButton(
                                        state = (LibClient.Components.TextButton.PropStateFactory.MakeLowLevel(LC.TextButton.Actionable Actions.demoPointerEventAction)),
                                        label = ("Add to Cart")
                                    )
                                |])
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                        code =
                            (
                                AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                    (
                                            (castAsElementAckingKeysWarning [|
                                                @"
                <LC.TextButton rt-fs='true'
                 Label='""Add to Cart""'
                 State='^LowLevel(LC.TextButton.InProgress)'/>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.TextButton"
                                    LibClient.Components.Constructors.LC.TextButton(
                                        state = (LibClient.Components.TextButton.PropStateFactory.MakeLowLevel(LC.TextButton.InProgress)),
                                        label = ("Add to Cart")
                                    )
                                |])
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                        code =
                            (
                                AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                    (
                                            (castAsElementAckingKeysWarning [|
                                                @"
                <LC.TextButton rt-fs='true'
                 Label='""Add to Cart""'
                 State='^Disabled'/>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.TextButton"
                                    LibClient.Components.Constructors.LC.TextButton(
                                        state = (LibClient.Components.TextButton.PropStateFactory.MakeDisabled),
                                        label = ("Add to Cart")
                                    )
                                |])
                    )
                |]),
        themeSamples =
                (castAsElementAckingKeysWarning [|
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                        code =
                            (
                                AppEggShellGallery.Components.ComponentSample.Children
                                    (
                                            (castAsElementAckingKeysWarning [|
                                                let __parentFQN = Some "AppEggShellGallery.Components.Code"
                                                AppEggShellGallery.Components.Constructors.Ui.Code(
                                                    language = (AppEggShellGallery.Components.Code.Render),
                                                    children =
                                                        [|
                                                            @"
                <LC.TextButton rt-fs='true'
                 theme='fun theme -> { theme with Primary = { theme.Primary with Actionable = { theme.Primary.Actionable with TextColor = Color.DevOrange } } }'
                 Label='""Special Add to Cart""'
                 State='^LowLevel(LC.TextButton.Actionable Actions.demoPointerEventAction)'/>
                "
                                                            |> makeTextNode2 __parentFQN
                                                        |]
                                                )
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.TextButton"
                                    LibClient.Components.Constructors.LC.TextButton(
                                        state = (LibClient.Components.TextButton.PropStateFactory.MakeLowLevel(LC.TextButton.Actionable Actions.demoPointerEventAction)),
                                        label = ("Special Add to Cart"),
                                        theme = (fun theme -> { theme with Primary = { theme.Primary with Actionable = { theme.Primary.Actionable with TextColor = Color.DevOrange } } })
                                    )
                                |])
                    )
                |])
    )
