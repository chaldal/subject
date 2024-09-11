module AppEggShellGallery.Components.Content.CarouselRender

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

open AppEggShellGallery.Components.Content.Carousel



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.Carousel.Props, estate: AppEggShellGallery.Components.Content.Carousel.Estate, pstate: AppEggShellGallery.Components.Content.Carousel.Pstate, actions: AppEggShellGallery.Components.Content.Carousel.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
    AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
        props = (AppEggShellGallery.Components.ComponentContent.ForFullyQualifiedName "LibClient.Components.Carousel"),
        displayName = ("Carousel"),
        samples =
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
                    <LC.Carousel
                     styles='[| CarouselStyles.Styles.carousel |]'
                     Count='4 |> PositiveInteger.ofLiteral'
                     RequestFocusOnMount='true'
                     rt-prop-children='Slide(index: int)'>
                        <LC.With.Layout rt-fs='true' InitialOnly='true' rt-with='onLayoutOption, maybeLayout'>
                            <div onLayoutOption='onLayoutOption'>
                                <RX.Image
                                 class='image'
                                 Source='localImage (sprintf ""/images/wlop%i.jpg"" (index + 1))'
                                 ResizeMode='~Cover'
                                 Size='~FromParentLayout maybeLayout'/>
                            </div>
                        </LC.With.Layout>
                    </LC.Carousel>
                "
                                                            |> makeTextNode2 __parentFQN
                                                        |]
                                                )
                                                let __parentFQN = Some "AppEggShellGallery.Components.Code"
                                                AppEggShellGallery.Components.Constructors.Ui.Code(
                                                    heading = ("Styles"),
                                                    language = (AppEggShellGallery.Components.Code.Fsharp),
                                                    children =
                                                        [|
                                                            @"
                    ""carousel"" => [
                        height 200
                    ]
                    ""image"" => [
                        height 200
                    ]
                "
                                                            |> makeTextNode2 __parentFQN
                                                        |]
                                                )
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.Carousel"
                                    LibClient.Components.Constructors.LC.Carousel(
                                        requestFocusOnMount = (true),
                                        count = (4 |> PositiveInteger.ofLiteral),
                                        styles = ([| CarouselStyles.Styles.carousel |]),
                                        slide =
                                            (fun (index: int) ->
                                                    (castAsElementAckingKeysWarning [|
                                                        let __parentFQN = Some "LibClient.Components.With.Layout"
                                                        LibClient.Components.Constructors.LC.With.Layout(
                                                            initialOnly = (true),
                                                            ``with`` =
                                                                (fun (onLayoutOption, maybeLayout) ->
                                                                        (castAsElementAckingKeysWarning [|
                                                                            let __parentFQN = Some "ReactXP.Components.View"
                                                                            ReactXP.Components.Constructors.RX.View(
                                                                                ?onLayout = (onLayoutOption),
                                                                                children =
                                                                                    [|
                                                                                        let __parentFQN = Some "ReactXP.Components.Image"
                                                                                        let __currClass = "image"
                                                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                        ReactXP.Components.Constructors.RX.Image(
                                                                                            size = (ReactXP.Components.Image.FromParentLayout maybeLayout),
                                                                                            resizeMode = (ReactXP.Components.Image.Cover),
                                                                                            source = (localImage (sprintf "/images/wlop%i.jpg" (index + 1))),
                                                                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.Image" __currStyles |> Some) else None)
                                                                                        )
                                                                                    |]
                                                                            )
                                                                        |])
                                                                )
                                                        )
                                                    |])
                                            )
                                    )
                                |])
                    )
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
                <LC.Carousel
                 styles='[| CarouselStyles.Styles.carousel |]'
                 rt-let='urls := [
                    ""https://example.com/Picture?pictureId=64292""
                    ""https://example.com/Picture?pictureId=23157""
                    ""https://example.com/Picture?pictureId=31723""
                    ""https://example.com/Picture?pictureId=28589""
                    ""https://example.com/Picture?pictureId=26171""
                 ]'
                 Count='urls.Length |> PositiveInteger.ofIntUnsafe (* ok because hardcoded list *)'
                 RequestFocusOnMount='true'
                 rt-prop-children='Slide(index: int)'>
                    <LC.With.Layout rt-fs='true' InitialOnly='true' rt-with='onLayoutOption, maybeLayout'>
                        <div onLayoutOption='onLayoutOption'>
                            <RX.Image
                             class='image'
                             Source='~ofUrl (urls.[index])'
                             ResizeMode='~Cover'
                             Size='~FromParentLayout maybeLayout'/>
                        </div>
                    </LC.With.Layout>
                </LC.Carousel>
                "
                                                            |> makeTextNode2 __parentFQN
                                                        |]
                                                )
                                                let __parentFQN = Some "AppEggShellGallery.Components.Code"
                                                AppEggShellGallery.Components.Constructors.Ui.Code(
                                                    heading = ("Styles"),
                                                    language = (AppEggShellGallery.Components.Code.Fsharp),
                                                    children =
                                                        [|
                                                            @"
                    ""carousel"" => [
                        height 200
                    ]
                    ""image"" => [
                        height 200
                    ]
                "
                                                            |> makeTextNode2 __parentFQN
                                                        |]
                                                )
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    (
                                        let urls = (
                                            [
                                                                "https://example.com/Picture?pictureId=64292"
                                                                "https://example.com/Picture?pictureId=23157"
                                                                "https://example.com/Picture?pictureId=31723"
                                                                "https://example.com/Picture?pictureId=28589"
                                                                "https://example.com/Picture?pictureId=26171"
                                                             ]
                                        )
                                        let __parentFQN = Some "LibClient.Components.Carousel"
                                        LibClient.Components.Constructors.LC.Carousel(
                                            requestFocusOnMount = (true),
                                            count = (urls.Length |> PositiveInteger.ofIntUnsafe (* ok because hardcoded list *)),
                                            styles = ([| CarouselStyles.Styles.carousel |]),
                                            slide =
                                                (fun (index: int) ->
                                                        (castAsElementAckingKeysWarning [|
                                                            let __parentFQN = Some "LibClient.Components.With.Layout"
                                                            LibClient.Components.Constructors.LC.With.Layout(
                                                                initialOnly = (true),
                                                                ``with`` =
                                                                    (fun (onLayoutOption, maybeLayout) ->
                                                                            (castAsElementAckingKeysWarning [|
                                                                                let __parentFQN = Some "ReactXP.Components.View"
                                                                                ReactXP.Components.Constructors.RX.View(
                                                                                    ?onLayout = (onLayoutOption),
                                                                                    children =
                                                                                        [|
                                                                                            let __parentFQN = Some "ReactXP.Components.Image"
                                                                                            let __currClass = "image"
                                                                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                            ReactXP.Components.Constructors.RX.Image(
                                                                                                size = (ReactXP.Components.Image.FromParentLayout maybeLayout),
                                                                                                resizeMode = (ReactXP.Components.Image.Cover),
                                                                                                source = (ReactXP.Components.Image.ofUrl (urls.[index])),
                                                                                                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.Image" __currStyles |> Some) else None)
                                                                                            )
                                                                                        |]
                                                                                )
                                                                            |])
                                                                    )
                                                            )
                                                        |])
                                                )
                                        )
                                    )
                                |])
                    )
                |])
    )
