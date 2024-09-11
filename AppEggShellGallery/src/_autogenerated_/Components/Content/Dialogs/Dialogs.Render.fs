module AppEggShellGallery.Components.Content.DialogsRender

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

open AppEggShellGallery.Components.Content.Dialogs



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.Dialogs.Props, estate: AppEggShellGallery.Components.Content.Dialogs.Estate, pstate: AppEggShellGallery.Components.Content.Dialogs.Pstate, actions: AppEggShellGallery.Components.Content.Dialogs.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.With.Navigation"
    AppEggShellGallery.Components.Constructors.Ui.With.Navigation(
        ``with`` =
            (fun (nav) ->
                    (castAsElementAckingKeysWarning [|
                        let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
                        AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
                            displayName = ("Dialogs"),
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
                    <LC.Button
                     Label='""Alert""'
                     State='^Go (Alert (None, ""Something happened and you should probably do something about it.""), nav)'/>
                "
                                                                    |> makeTextNode2 __parentFQN
                                                                |])
                                                        )
                                                ),
                                            visuals =
                                                    (castAsElementAckingKeysWarning [|
                                                        let __parentFQN = Some "LibClient.Components.Button"
                                                        LibClient.Components.Constructors.LC.Button(
                                                            state = (LibClient.Components.Button.PropStateFactory.MakeGo (Alert (None, "Something happened and you should probably do something about it."), nav)),
                                                            label = ("Alert")
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
                    <LC.Button
                     Label='""Confirm""'
                     State='^Go (Confirm ((Some ""Confirm""), ""Okay to delete all the things?"", ""No"", ""Yes"", ignore), nav)'/>
                "
                                                                    |> makeTextNode2 __parentFQN
                                                                |])
                                                        )
                                                ),
                                            visuals =
                                                    (castAsElementAckingKeysWarning [|
                                                        let __parentFQN = Some "LibClient.Components.Button"
                                                        LibClient.Components.Constructors.LC.Button(
                                                            state = (LibClient.Components.Button.PropStateFactory.MakeGo (Confirm ((Some "Confirm"), "Okay to delete all the things?", "No", "Yes", ignore), nav)),
                                                            label = ("Confirm")
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
                    <LC.Button
                     Label='""Confirm""'
                     State='^Go (ConfirmCustom ((Some ""Confirm""), ""Send the dark forest deterrence broadcast?"", [LibClient.Components.Dialog.Confirm.Cancel (""No"", LibClient.Components.Button.Level.Primary, ignore); LibClient.Components.Dialog.Confirm.Confirm (""Yes"", LibClient.Components.Button.Secondary, ignore)]), nav)'/>
                "
                                                                    |> makeTextNode2 __parentFQN
                                                                |])
                                                        )
                                                ),
                                            visuals =
                                                    (castAsElementAckingKeysWarning [|
                                                        let __parentFQN = Some "LibClient.Components.Button"
                                                        LibClient.Components.Constructors.LC.Button(
                                                            state = (LibClient.Components.Button.PropStateFactory.MakeGo (ConfirmCustom ((Some "Confirm"), "Send the dark forest deterrence broadcast?", [LibClient.Components.Dialog.Confirm.Cancel ("No", LibClient.Components.Button.Level.Primary, ignore); LibClient.Components.Dialog.Confirm.Confirm ("Yes", LibClient.Components.Button.Secondary, ignore)]), nav)),
                                                            label = ("Custom Confirm")
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
                    <LC.Button
                     Label='""Carousel""'
                     State='^Go (ImageViewer (imageSources, 0u), nav)'/>
                "
                                                                    |> makeTextNode2 __parentFQN
                                                                |])
                                                        )
                                                ),
                                            visuals =
                                                    (castAsElementAckingKeysWarning [|
                                                        let __parentFQN = Some "LibClient.Components.Button"
                                                        LibClient.Components.Constructors.LC.Button(
                                                            state = (LibClient.Components.Button.PropStateFactory.MakeGo (ImageViewer (imageSources, 0u), nav)),
                                                            label = ("Image Viewer")
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
                "
                                                                    |> makeTextNode2 __parentFQN
                                                                |])
                                                        )
                                                ),
                                            visuals =
                                                    (castAsElementAckingKeysWarning [|
                                                        makeTextNode2 __parentFQN "TODO add contents about WhiteRounded, WhiteRoundedStandard, FullScreen and other dialogs here"
                                                    |])
                                        )
                                    |])
                        )
                    |])
            )
    )
