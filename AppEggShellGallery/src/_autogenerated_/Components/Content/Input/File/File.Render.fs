module AppEggShellGallery.Components.Content.Input.FileRender

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

open AppEggShellGallery.Components.Content.Input.File



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.Input.File.Props, estate: AppEggShellGallery.Components.Content.Input.File.Estate, pstate: AppEggShellGallery.Components.Content.Input.File.Pstate, actions: AppEggShellGallery.Components.Content.Input.File.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
    AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
        props = (AppEggShellGallery.Components.ComponentContent.ForFullyQualifiedName "LibClient.Components.Input.File"),
        displayName = ("Input.File"),
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
                <LC.Input.File OnChange='actions.OnChange'/>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.Input.File"
                                    LibClient.Components.Constructors.LC.Input.File(
                                        validity = (estate.MaybeFile |> Result.map (fun _ -> Valid) |> Result.recover (fun m -> Invalid m)),
                                        value = (estate.MaybeFile |> Result.recover (fun _ -> [])),
                                        onChange = (actions.OnChange)
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
                <LC.Input.File
                 OnChange='actions.OnChange'
                 MaxFileCount='PositiveInteger.ofLiteral 1'
                 MaxFileSize='mBToKB 5<MB>'/>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.Input.File"
                                    LibClient.Components.Constructors.LC.Input.File(
                                        maxFileSize = (LibLifeCycleTypes.File.mBToKB 5<LibLifeCycleTypes.File.MB>),
                                        maxFileCount = (Positive.PositiveInteger.ofLiteral 1),
                                        validity = (estate.MaybeFile |> Result.map (fun _ -> Valid) |> Result.recover (fun m -> Invalid m)),
                                        value = (estate.MaybeFile |> Result.recover (fun _ -> [])),
                                        onChange = (actions.OnChange)
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
                <LC.Input.File
                 OnChange='actions.OnChange'
                 MaxFileCount='PositiveInteger.ofLiteral 4'
                 MaxFileSize='mBToKB 5<MB>'/>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.Input.File"
                                    LibClient.Components.Constructors.LC.Input.File(
                                        maxFileSize = (LibLifeCycleTypes.File.mBToKB 5<LibLifeCycleTypes.File.MB>),
                                        maxFileCount = (Positive.PositiveInteger.ofLiteral 4),
                                        validity = (estate.MaybeFile |> Result.map (fun _ -> Valid) |> Result.recover (fun m -> Invalid m)),
                                        value = (estate.MaybeFile |> Result.recover (fun _ -> [])),
                                        onChange = (actions.OnChange)
                                    )
                                |])
                    )
                |])
    )
