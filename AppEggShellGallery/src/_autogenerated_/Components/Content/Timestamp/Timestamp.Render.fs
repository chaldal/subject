module AppEggShellGallery.Components.Content.TimestampRender

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

open AppEggShellGallery.Components.Content.Timestamp



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.Timestamp.Props, estate: AppEggShellGallery.Components.Content.Timestamp.Estate, pstate: AppEggShellGallery.Components.Content.Timestamp.Pstate, actions: AppEggShellGallery.Components.Content.Timestamp.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
    AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
        props = (AppEggShellGallery.Components.ComponentContent.ForFullyQualifiedName "LibClient.Components.Timestamp"),
        displayName = ("Timestamp"),
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
                <LC.Timestamp rt-fs='true' Value='^(System.DateTimeOffset.Now)'/>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.Timestamp"
                                    LibClient.Components.Constructors.LC.Timestamp(
                                        value = (LibClient.Components.Timestamp.PropValueFactory.Make(System.DateTimeOffset.Now))
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
                <LC.Timestamp rt-fs='true' Value='^(1603261700701L)'/>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.Timestamp"
                                    LibClient.Components.Constructors.LC.Timestamp(
                                        value = (LibClient.Components.Timestamp.PropValueFactory.Make(1603261700701L))
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
                <LC.Timestamp rt-fs='true' Value='^(""2011-03-15"")' Format='""dddd, MMMM dd, yyyy""'/>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.Timestamp"
                                    LibClient.Components.Constructors.LC.Timestamp(
                                        format = ("dddd, MMMM dd, yyyy"),
                                        value = (LibClient.Components.Timestamp.PropValueFactory.Make("2011-03-15"))
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
                <LC.Timestamp rt-fs='true' Value='^(System.DateTimeOffset.Now)' Format='""dddd, MMMM dd, yyyy""'/>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.Timestamp"
                                    LibClient.Components.Constructors.LC.Timestamp(
                                        format = ("dddd, MMMM dd, yyyy"),
                                        value = (LibClient.Components.Timestamp.PropValueFactory.Make(System.DateTimeOffset.Now))
                                    )
                                |])
                    )
                |])
    )
