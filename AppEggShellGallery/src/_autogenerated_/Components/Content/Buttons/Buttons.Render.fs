module AppEggShellGallery.Components.Content.ButtonsRender

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

open AppEggShellGallery.Components.Content.Buttons



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.Buttons.Props, estate: AppEggShellGallery.Components.Content.Buttons.Estate, pstate: AppEggShellGallery.Components.Content.Buttons.Pstate, actions: AppEggShellGallery.Components.Content.Buttons.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
    AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
        props = (AppEggShellGallery.Components.ComponentContent.ForFullyQualifiedName "LibClient.Components.Buttons"),
        displayName = ("Buttons"),
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
                <LC.Buttons rt-fs='true' Align='~Left'>
                    <LC.Button
                        Icon='~Left Icon.Home'
                        Label='""Home""'
                        State='^LowLevel (~Actionable Actions.greet)' />
                    <LC.Button
                        Icon='~Left Icon.Submit'
                        Label='""Submit""'
                        State='^LowLevel (~Actionable Actions.greet)' />
                </LC.Buttons>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.Buttons"
                                    LibClient.Components.Constructors.LC.Buttons(
                                        align = (LibClient.Components.Buttons.Left),
                                        children =
                                            [|
                                                let __parentFQN = Some "LibClient.Components.Button"
                                                LibClient.Components.Constructors.LC.Button(
                                                    state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (LibClient.Components.Button.Actionable Actions.greet)),
                                                    label = ("Home"),
                                                    icon = (LibClient.Components.Button.Left Icon.Home)
                                                )
                                                let __parentFQN = Some "LibClient.Components.Button"
                                                LibClient.Components.Constructors.LC.Button(
                                                    state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (LibClient.Components.Button.Actionable Actions.greet)),
                                                    label = ("Submit"),
                                                    icon = (LibClient.Components.Button.Left Icon.Submit)
                                                )
                                            |]
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
                <LC.Buttons rt-fs='true'>
                    <LC.Button
                     Icon='~Left Icon.Home'
                     Label='""Home""'
                     State='^LowLevel (~Actionable Actions.greet)' />
                    <LC.Button
                     Icon='~Left Icon.Submit'
                     Label='""Submit""'
                     State='^LowLevel (~Actionable Actions.greet)' />
                </LC.Buttons>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.Buttons"
                                    LibClient.Components.Constructors.LC.Buttons(
                                        children =
                                            [|
                                                let __parentFQN = Some "LibClient.Components.Button"
                                                LibClient.Components.Constructors.LC.Button(
                                                    state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (LibClient.Components.Button.Actionable Actions.greet)),
                                                    label = ("Home"),
                                                    icon = (LibClient.Components.Button.Left Icon.Home)
                                                )
                                                let __parentFQN = Some "LibClient.Components.Button"
                                                LibClient.Components.Constructors.LC.Button(
                                                    state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (LibClient.Components.Button.Actionable Actions.greet)),
                                                    label = ("Submit"),
                                                    icon = (LibClient.Components.Button.Left Icon.Submit)
                                                )
                                            |]
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
                <LC.Buttons rt-fs='true' Align='~Right'>
                    <LC.Button
                     Icon='~Left Icon.Home'
                     Label='""Home""'
                     State='^LowLevel (~Actionable Actions.greet)' />
                    <LC.Button
                     Icon='~Left Icon.Submit'
                     Label='""Submit""'
                     State='^LowLevel (~Actionable Actions.greet)' />
                </LC.Buttons>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.Buttons"
                                    LibClient.Components.Constructors.LC.Buttons(
                                        align = (LibClient.Components.Buttons.Right),
                                        children =
                                            [|
                                                let __parentFQN = Some "LibClient.Components.Button"
                                                LibClient.Components.Constructors.LC.Button(
                                                    state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (LibClient.Components.Button.Actionable Actions.greet)),
                                                    label = ("Home"),
                                                    icon = (LibClient.Components.Button.Left Icon.Home)
                                                )
                                                let __parentFQN = Some "LibClient.Components.Button"
                                                LibClient.Components.Constructors.LC.Button(
                                                    state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (LibClient.Components.Button.Actionable Actions.greet)),
                                                    label = ("Submit"),
                                                    icon = (LibClient.Components.Button.Left Icon.Submit)
                                                )
                                            |]
                                    )
                                |])
                    )
                |])
    )
