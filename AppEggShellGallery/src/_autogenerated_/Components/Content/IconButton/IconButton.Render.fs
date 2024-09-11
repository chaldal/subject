module AppEggShellGallery.Components.Content.IconButtonRender

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

open AppEggShellGallery.Components.Content.IconButton



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.IconButton.Props, estate: AppEggShellGallery.Components.Content.IconButton.Estate, pstate: AppEggShellGallery.Components.Content.IconButton.Pstate, actions: AppEggShellGallery.Components.Content.IconButton.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
    AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
        props = (AppEggShellGallery.Components.ComponentContent.ForFullyQualifiedName "LibClient.Components.IconButton"),
        displayName = ("IconButton"),
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
                <LC.IconButton
                 Icon='Icon.Megaphone'
                 State='^LowLevel(~Actionable Actions.greet)'/>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.IconButton"
                                    LibClient.Components.Constructors.LC.IconButton(
                                        state = (LibClient.Components.IconButton.PropStateFactory.MakeLowLevel(LC.IconButton.Actionable Actions.greet)),
                                        icon = (Icon.Megaphone)
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
                <LC.IconButton
                 Icon='Icon.Megaphone'
                 State='^LowLevel(~InProgress)'/>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.IconButton"
                                    LibClient.Components.Constructors.LC.IconButton(
                                        state = (LibClient.Components.IconButton.PropStateFactory.MakeLowLevel(LC.IconButton.InProgress)),
                                        icon = (Icon.Megaphone)
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
                <LC.IconButton
                 Icon='Icon.Home'
                 State='^Disabled'/>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.IconButton"
                                    LibClient.Components.Constructors.LC.IconButton(
                                        state = (LibClient.Components.IconButton.PropStateFactory.MakeDisabled),
                                        icon = (Icon.Home)
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
                    <LC.IconButton
                     Icon='Icon.Send'
                     State='^LowLevel(~Actionable Actions.greet)'
                     theme='IconButtonStyles.Styles.specialTheme'/>
                "
                                                            |> makeTextNode2 __parentFQN
                                                        |]
                                                )
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.IconButton"
                                    LibClient.Components.Constructors.LC.IconButton(
                                        theme = (IconButtonStyles.Styles.specialTheme),
                                        state = (LibClient.Components.IconButton.PropStateFactory.MakeLowLevel(LC.IconButton.Actionable Actions.greet)),
                                        icon = (Icon.Send)
                                    )
                                |])
                    )
                |])
    )
