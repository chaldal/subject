module AppEggShellGallery.Components.Content.FloatingActionButtonRender

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

open AppEggShellGallery.Components.Content.FloatingActionButton



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.FloatingActionButton.Props, estate: AppEggShellGallery.Components.Content.FloatingActionButton.Estate, pstate: AppEggShellGallery.Components.Content.FloatingActionButton.Pstate, actions: AppEggShellGallery.Components.Content.FloatingActionButton.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
    AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
        props = (AppEggShellGallery.Components.ComponentContent.ForFullyQualifiedName "LibClient.Components.FloatingActionButton"),
        displayName = ("FloatingActionButton"),
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
                <LC.FloatingActionButton
                 Icon='Icon.Home'
                 State='^LowLevel(~Actionable Actions.greet)'/>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.FloatingActionButton"
                                    LibClient.Components.Constructors.LC.FloatingActionButton(
                                        state = (LibClient.Components.FloatingActionButton.PropStateFactory.MakeLowLevel(LibClient.Components.FloatingActionButton.Actionable Actions.greet)),
                                        icon = (Icon.Home)
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
                <LC.FloatingActionButton
                 Icon='Icon.Plus'
                 Label='""Add Items""'
                 State='^LowLevel(~Actionable ignore)'/>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.FloatingActionButton"
                                    LibClient.Components.Constructors.LC.FloatingActionButton(
                                        state = (LibClient.Components.FloatingActionButton.PropStateFactory.MakeLowLevel(LibClient.Components.FloatingActionButton.Actionable ignore)),
                                        label = ("Add Items"),
                                        icon = (Icon.Plus)
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
                <LC.FloatingActionButton
                 Icon='Icon.Home'
                 State='^LowLevel(~InProgress)'/>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.FloatingActionButton"
                                    LibClient.Components.Constructors.LC.FloatingActionButton(
                                        state = (LibClient.Components.FloatingActionButton.PropStateFactory.MakeLowLevel(LibClient.Components.FloatingActionButton.InProgress)),
                                        icon = (Icon.Home)
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
                <LC.FloatingActionButton
                 Icon='Icon.Plus'
                 Label='""Add Items""'
                 State='^LowLevel(~InProgress)'/>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.FloatingActionButton"
                                    LibClient.Components.Constructors.LC.FloatingActionButton(
                                        state = (LibClient.Components.FloatingActionButton.PropStateFactory.MakeLowLevel(LibClient.Components.FloatingActionButton.InProgress)),
                                        label = ("Add Items"),
                                        icon = (Icon.Plus)
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
                <LC.FloatingActionButton
                 Icon='Icon.Home'
                 State='^Disabled'/>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.FloatingActionButton"
                                    LibClient.Components.Constructors.LC.FloatingActionButton(
                                        state = (LibClient.Components.FloatingActionButton.PropStateFactory.MakeDisabled),
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
                    <LC.FloatingActionButton
                     class='special'
                     Icon='Icon.Report'
                     State='^LowLevel(~Actionable Actions.greet)'/>
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
                    ""special"" ==> LibClient.Components.FloatingActionButtonStyles.Theme.One (LibClient.Input.ButtonLikeState.Actionable ignore, theBackgroundColor = Color.White, iconColor = Color.DevBlue,  iconSize = 32)
                "
                                                            |> makeTextNode2 __parentFQN
                                                        |]
                                                )
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.FloatingActionButton"
                                    let __currClass = "special"
                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                    LibClient.Components.Constructors.LC.FloatingActionButton(
                                        state = (LibClient.Components.FloatingActionButton.PropStateFactory.MakeLowLevel(LibClient.Components.FloatingActionButton.Actionable Actions.greet)),
                                        icon = (Icon.Report),
                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                    )
                                |])
                    )
                |])
    )
