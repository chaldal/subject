module AppEggShellGallery.Components.Content.ThumbsRender

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

open AppEggShellGallery.Components.Content.Thumbs



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.Thumbs.Props, estate: AppEggShellGallery.Components.Content.Thumbs.Estate, pstate: AppEggShellGallery.Components.Content.Thumbs.Pstate, actions: AppEggShellGallery.Components.Content.Thumbs.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
    AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
        props = (AppEggShellGallery.Components.ComponentContent.ForFullyQualifiedName "LibClient.Components.Thumbs"),
        displayName = ("Thumbs"),
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
                <LC.Thumbs For='^[]'/>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.Thumbs"
                                    LibClient.Components.Constructors.LC.Thumbs(
                                        ``for`` = (LibClient.Components.Thumbs.PropForFactory.Make[])
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
                <LC.Thumbs For='^([""/images/yuumei1.jpg""; ""/images/yuumei2.jpg""; ""/images/yuumei3.jpg""; ""/images/yuumei4.jpg""] |> List.map localImage)'/>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.Thumbs"
                                    LibClient.Components.Constructors.LC.Thumbs(
                                        ``for`` = (LibClient.Components.Thumbs.PropForFactory.Make(["/images/yuumei1.jpg"; "/images/yuumei2.jpg"; "/images/yuumei3.jpg"; "/images/yuumei4.jpg"] |> List.map localImage))
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
                <LC.Thumbs For='^([4; 3; 2; 1], fun i -> localImage (sprintf ""/images/yuumei%i.jpg"" i))'/>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.Thumbs"
                                    LibClient.Components.Constructors.LC.Thumbs(
                                        ``for`` = (LibClient.Components.Thumbs.PropForFactory.Make([4; 3; 2; 1], fun i -> localImage (sprintf "/images/yuumei%i.jpg" i)))
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
                <LC.Thumbs
                 For='^([4; 3; 2; 1], fun i -> localImage (sprintf ""/images/yuumei%i.jpg"" i))'
                 Selected='estate.Selected'
                 OnPress='actions.OnPress'/>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.Thumbs"
                                    LibClient.Components.Constructors.LC.Thumbs(
                                        onPress = (actions.OnPress),
                                        selected = (estate.Selected),
                                        ``for`` = (LibClient.Components.Thumbs.PropForFactory.Make([4; 3; 2; 1], fun i -> localImage (sprintf "/images/yuumei%i.jpg" i)))
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
                <With.Navigation rt-with='nav'>
                    <LC.Thumbs
                     rt-let='items := [4; 3; 2; 1]; itemToImage := fun i -> localImage (sprintf ""/images/yuumei%i.jpg"" i)'
                     For='^(items, itemToImage)'
                     OnPress='fun _ index -> nav.Go (ImageViewer (items |> List.map itemToImage, index))'/>
                </With.Navigation>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "AppEggShellGallery.Components.With.Navigation"
                                    AppEggShellGallery.Components.Constructors.Ui.With.Navigation(
                                        ``with`` =
                                            (fun (nav) ->
                                                    (castAsElementAckingKeysWarning [|
                                                        (
                                                            let items = [4; 3; 2; 1]
                                                            let itemToImage = fun i -> localImage (sprintf "/images/yuumei%i.jpg" i)
                                                            let __parentFQN = Some "LibClient.Components.Thumbs"
                                                            LibClient.Components.Constructors.LC.Thumbs(
                                                                onPress = (fun _ index -> nav.Go (ImageViewer (items |> List.map itemToImage, index))),
                                                                ``for`` = (LibClient.Components.Thumbs.PropForFactory.Make(items, itemToImage))
                                                            )
                                                        )
                                                    |])
                                            )
                                    )
                                |])
                    )
                |])
    )
