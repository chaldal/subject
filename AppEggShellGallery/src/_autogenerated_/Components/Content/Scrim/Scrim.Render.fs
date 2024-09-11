module AppEggShellGallery.Components.Content.ScrimRender

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

open AppEggShellGallery.Components.Content.Scrim



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.Scrim.Props, estate: AppEggShellGallery.Components.Content.Scrim.Estate, pstate: AppEggShellGallery.Components.Content.Scrim.Pstate, actions: AppEggShellGallery.Components.Content.Scrim.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
    AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
        props = (AppEggShellGallery.Components.ComponentContent.ForFullyQualifiedName "LibClient.Components.Scrim"),
        displayName = ("Scrim"),
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
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "ReactXP.Components.View"
                                    let __currClass = "sample-block"
                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                    ReactXP.Components.Constructors.RX.View(
                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                        children =
                                            [|
                                                let __parentFQN = Some "LibClient.Components.Buttons"
                                                LibClient.Components.Constructors.LC.Buttons(
                                                    children =
                                                        [|
                                                            let __parentFQN = Some "LibClient.Components.Button"
                                                            LibClient.Components.Constructors.LC.Button(
                                                                state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (LibClient.Components.Button.Actionable Actions.greet)),
                                                                label = ("Greet")
                                                            )
                                                        |]
                                                )
                                                let __parentFQN = Some "LibClient.Components.Scrim"
                                                LibClient.Components.Constructors.LC.Scrim(
                                                    isVisible = (estate.IsScrimVisible),
                                                    styles = ([| ScrimStyles.Styles.scrim |])
                                                )
                                            |]
                                    )
                                    let __parentFQN = Some "LibClient.Components.Buttons"
                                    LibClient.Components.Constructors.LC.Buttons(
                                        children =
                                            [|
                                                let __parentFQN = Some "LibClient.Components.Button"
                                                LibClient.Components.Constructors.LC.Button(
                                                    state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (LibClient.Components.Button.Actionable actions.Toggle)),
                                                    label = ("Toggle")
                                                )
                                            |]
                                    )
                                |])
                    )
                |])
    )
