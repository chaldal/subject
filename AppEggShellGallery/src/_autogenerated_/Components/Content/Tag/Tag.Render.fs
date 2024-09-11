module AppEggShellGallery.Components.Content.TagRender

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

open AppEggShellGallery.Components.Content.Tag
open LibClient


let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.Tag.Props, estate: AppEggShellGallery.Components.Content.Tag.Estate, pstate: AppEggShellGallery.Components.Content.Tag.Pstate, actions: AppEggShellGallery.Components.Content.Tag.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
    AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
        props = (AppEggShellGallery.Components.ComponentContent.ForFullyQualifiedName "LibClient.Components.Tag"),
        isResponsive = (true),
        displayName = ("Tag"),
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
                <LC.Tag rt-fs='true'
                 Text='""Sweets""'
                 State='LC.Tag.ViewOnly'/>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.Tag"
                                    LibClient.Components.Constructors.LC.Tag(
                                        state = (LC.Tag.ViewOnly),
                                        text = ("Sweets")
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
                <LC.Tag rt-fs='true'
                 rt-map='tag := [""Apple""; ""Orange""; ""Banana""]'
                 Text='tag'
                 State='LC.Tag.ViewOnly'/>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    (
                                        (["Apple"; "Orange"; "Banana"])
                                        |> Seq.map
                                            (fun tag ->
                                                let __parentFQN = Some "LibClient.Components.Tag"
                                                LibClient.Components.Constructors.LC.Tag(
                                                    state = (LC.Tag.ViewOnly),
                                                    text = (tag)
                                                )
                                            )
                                        |> Array.ofSeq |> castAsElement
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
                <LC.Tags>
                    <LC.Tag rt-fs='true'
                     rt-map='tag := [""Apple""; ""Orange""; ""Banana""]'
                     Text='tag'
                     State='LC.Tag.ViewOnly'/>
                </LC.Tags>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.Tags"
                                    LibClient.Components.Constructors.LC.Tags(
                                        children =
                                            [|
                                                (
                                                    (["Apple"; "Orange"; "Banana"])
                                                    |> Seq.map
                                                        (fun tag ->
                                                            let __parentFQN = Some "LibClient.Components.Tag"
                                                            LibClient.Components.Constructors.LC.Tag(
                                                                state = (LC.Tag.ViewOnly),
                                                                text = (tag)
                                                            )
                                                        )
                                                    |> Array.ofSeq |> castAsElement
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
                <LC.Tags>
                    <LC.Tag rt-fs='true' Text='""View Only""'  State='LC.Tag.ViewOnly'/>
                    <LC.Tag rt-fs='true' Text='""Selected""'   State='LC.Tag.ViewOnly'   IsSelected='true'/>
                    <LC.Tag rt-fs='true' Text='""Actionable""' State='LC.Tag.Actionable (fun _ -> Action.alert ""You pressed a tag"")'/>
                    <LC.Tag rt-fs='true' Text='""InProgress""' State='LC.Tag.InProgress'/>
                    <LC.Tag rt-fs='true' Text='""Disabled""'   State='LC.Tag.Disabled'/>
                </LC.Tags>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.Tags"
                                    LibClient.Components.Constructors.LC.Tags(
                                        children =
                                            [|
                                                let __parentFQN = Some "LibClient.Components.Tag"
                                                LibClient.Components.Constructors.LC.Tag(
                                                    state = (LC.Tag.ViewOnly),
                                                    text = ("View Only")
                                                )
                                                let __parentFQN = Some "LibClient.Components.Tag"
                                                LibClient.Components.Constructors.LC.Tag(
                                                    isSelected = (true),
                                                    state = (LC.Tag.ViewOnly),
                                                    text = ("Selected")
                                                )
                                                let __parentFQN = Some "LibClient.Components.Tag"
                                                LibClient.Components.Constructors.LC.Tag(
                                                    state = (LC.Tag.Actionable (fun _ -> Action.alert "You pressed a tag")),
                                                    text = ("Actionable")
                                                )
                                                let __parentFQN = Some "LibClient.Components.Tag"
                                                LibClient.Components.Constructors.LC.Tag(
                                                    state = (LC.Tag.InProgress),
                                                    text = ("InProgress")
                                                )
                                                let __parentFQN = Some "LibClient.Components.Tag"
                                                LibClient.Components.Constructors.LC.Tag(
                                                    state = (LC.Tag.Disabled),
                                                    text = ("Disabled")
                                                )
                                            |]
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
                    <LC.Tags>
                        <LC.Tag rt-fs='true' theme='TagStyles.cautionTheme' Text='""View Only""'  State='LC.Tag.ViewOnly'/>
                        <LC.Tag rt-fs='true' theme='TagStyles.cautionTheme' Text='""Selected""'   State='LC.Tag.ViewOnly'   IsSelected='true'/>
                        <LC.Tag rt-fs='true' theme='TagStyles.cautionTheme' Text='""Actionable""' State='LC.Tag.Actionable (fun _ -> Action.alert ""You pressed a themed tag"")'/>
                        <LC.Tag rt-fs='true' theme='TagStyles.cautionTheme' Text='""InProgress""' State='LC.Tag.InProgress'/>
                        <LC.Tag rt-fs='true' theme='TagStyles.cautionTheme' Text='""Disabled""'   State='LC.Tag.Disabled'/>
                    </LC.Tags>
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
                    ""caution"" ==>
                        LibClient.Components.TagStyles.Theme.One (
                            theBackgroundColor         = Color.DevOrange,
                            theTextColor               = Color.White,
                            theSelectedBackgroundColor = Color.DevRed,
                            theSelectedTextColor       = Color.White
                        )
                "
                                                            |> makeTextNode2 __parentFQN
                                                        |]
                                                )
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.Tags"
                                    LibClient.Components.Constructors.LC.Tags(
                                        children =
                                            [|
                                                let __parentFQN = Some "LibClient.Components.Tag"
                                                LibClient.Components.Constructors.LC.Tag(
                                                    state = (LC.Tag.ViewOnly),
                                                    text = ("View Only"),
                                                    theme = (TagStyles.cautionTheme)
                                                )
                                                let __parentFQN = Some "LibClient.Components.Tag"
                                                LibClient.Components.Constructors.LC.Tag(
                                                    isSelected = (true),
                                                    state = (LC.Tag.ViewOnly),
                                                    text = ("Selected"),
                                                    theme = (TagStyles.cautionTheme)
                                                )
                                                let __parentFQN = Some "LibClient.Components.Tag"
                                                LibClient.Components.Constructors.LC.Tag(
                                                    state = (LC.Tag.Actionable (fun _ -> Action.alert "You pressed a themed tag")),
                                                    text = ("Actionable"),
                                                    theme = (TagStyles.cautionTheme)
                                                )
                                                let __parentFQN = Some "LibClient.Components.Tag"
                                                LibClient.Components.Constructors.LC.Tag(
                                                    state = (LC.Tag.InProgress),
                                                    text = ("InProgress"),
                                                    theme = (TagStyles.cautionTheme)
                                                )
                                                let __parentFQN = Some "LibClient.Components.Tag"
                                                LibClient.Components.Constructors.LC.Tag(
                                                    state = (LC.Tag.Disabled),
                                                    text = ("Disabled"),
                                                    theme = (TagStyles.cautionTheme)
                                                )
                                            |]
                                    )
                                |])
                    )
                |])
    )
