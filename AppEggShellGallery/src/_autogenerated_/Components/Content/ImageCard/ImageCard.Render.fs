module AppEggShellGallery.Components.Content.ImageCardRender

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

open AppEggShellGallery.Components.Content.ImageCard



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.ImageCard.Props, estate: AppEggShellGallery.Components.Content.ImageCard.Estate, pstate: AppEggShellGallery.Components.Content.ImageCard.Pstate, actions: AppEggShellGallery.Components.Content.ImageCard.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
    AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
        props = (AppEggShellGallery.Components.ComponentContent.ForFullyQualifiedName "LibClient.Components.ImageCard"),
        displayName = ("ImageCard"),
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
                <LC.ImageCard
                 Source='localImage ""/images/wlop4.jpg""'/>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.ImageCard"
                                    LibClient.Components.Constructors.LC.ImageCard(
                                        source = (localImage "/images/wlop4.jpg")
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
                <LC.ImageCard
                 Source='localImage ""/images/wlop4.jpg""'
                 Label='~Text (""Painting"", ~UseScrim.No)'/>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.ImageCard"
                                    LibClient.Components.Constructors.LC.ImageCard(
                                        label = (LibClient.Components.ImageCard.Text ("Painting", LibClient.Components.ImageCard.UseScrim.No)),
                                        source = (localImage "/images/wlop4.jpg")
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
                <LC.ImageCard
                 Source='localImage ""/images/wlop4.jpg""'
                 Label='~Text (""Painting"", ~UseScrim.Yes)'/>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.ImageCard"
                                    LibClient.Components.Constructors.LC.ImageCard(
                                        label = (LibClient.Components.ImageCard.Text ("Painting", LibClient.Components.ImageCard.UseScrim.Yes)),
                                        source = (localImage "/images/wlop4.jpg")
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
                <LC.ImageCard
                 Source='localImage ""/images/wlop4.jpg""'
                 Label='~Children ~UseScrim.Yes'>
                    <div class='metadata'>
                        <text class='title'>Painting</text>
                        <text class='author'>by WLOP</text>
                    </div>
                </LC.ImageCard>
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
                    ""metadata"" => [
                        FlexDirection.Row
                        JustifyContent.SpaceBetween
                        paddingHV 20 10
                    ]

                    ""title"" => [
                        FontWeight.Bold
                        fontSize 18
                        color    Color.White
                    ]

                    ""author"" => [
                        fontSize 14
                        color    Color.White
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
                                    let __parentFQN = Some "LibClient.Components.ImageCard"
                                    LibClient.Components.Constructors.LC.ImageCard(
                                        label = (LibClient.Components.ImageCard.Children LibClient.Components.ImageCard.UseScrim.Yes),
                                        source = (localImage "/images/wlop4.jpg"),
                                        children =
                                            [|
                                                let __parentFQN = Some "ReactXP.Components.View"
                                                let __currClass = "metadata"
                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                ReactXP.Components.Constructors.RX.View(
                                                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                    children =
                                                        [|
                                                            let __parentFQN = Some "LibClient.Components.LegacyText"
                                                            let __currClass = "title"
                                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                            LibClient.Components.Constructors.LC.LegacyText(
                                                                ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                children =
                                                                    [|
                                                                        makeTextNode2 __parentFQN "Painting"
                                                                    |]
                                                            )
                                                            let __parentFQN = Some "LibClient.Components.LegacyText"
                                                            let __currClass = "author"
                                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                            LibClient.Components.Constructors.LC.LegacyText(
                                                                ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                children =
                                                                    [|
                                                                        makeTextNode2 __parentFQN "by WLOP"
                                                                    |]
                                                            )
                                                        |]
                                                )
                                            |]
                                    )
                                |])
                    )
                |])
    )
