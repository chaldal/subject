module AppEggShellGallery.Components.Content.Input.DurationRender

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

open AppEggShellGallery.Components.Content.Input.Duration



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.Input.Duration.Props, estate: AppEggShellGallery.Components.Content.Input.Duration.Estate, pstate: AppEggShellGallery.Components.Content.Input.Duration.Pstate, actions: AppEggShellGallery.Components.Content.Input.Duration.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
    AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
        props = (AppEggShellGallery.Components.ComponentContent.ForFullyQualifiedName "LibClient.Components.Input.Duration"),
        displayName = ("Input.Duration"),
        samples =
                (castAsElementAckingKeysWarning [|
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("Modes"),
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
                    <LC.Input.Duration
                     Label='""Duration""'
                     Value='estate.Values.Item ""A""'
                     OnChange='actions.OnChange ""A""'
                     Validity='Valid'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.Duration"
                                                    LibClient.Components.Constructors.LC.Input.Duration(
                                                        validity = (Valid),
                                                        onChange = (actions.OnChange "A"),
                                                        value = (estate.Values.Item "A"),
                                                        label = ("Duration")
                                                    )
                                                |])
                                    )
                                |])
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("Validation (hardcoded, not dynamic)"),
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
                    <LC.Input.Duration
                     Label='""Duration""'
                     Value='LibClient.Components.Input.Duration.empty'
                     OnChange='ignore'
                     Validity='Missing'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.Duration"
                                                    LibClient.Components.Constructors.LC.Input.Duration(
                                                        validity = (Missing),
                                                        onChange = (ignore),
                                                        value = (LibClient.Components.Input.Duration.empty),
                                                        label = ("Duration")
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
                    <LC.Input.Duration
                     Label='""Duration""'
                     Value='LibClient.Components.Input.Duration.empty'
                     OnChange='ignore'
                     Validity='Invalid ""That is just a bad duration""'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.Duration"
                                                    LibClient.Components.Constructors.LC.Input.Duration(
                                                        validity = (Invalid "That is just a bad duration"),
                                                        onChange = (ignore),
                                                        value = (LibClient.Components.Input.Duration.empty),
                                                        label = ("Duration")
                                                    )
                                                |])
                                    )
                                |])
                    )
                |])
    )
