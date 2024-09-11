module AppEggShellGallery.Components.Content.Input.CheckboxRender

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

open AppEggShellGallery.Components.Content.Input.Checkbox



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.Input.Checkbox.Props, estate: AppEggShellGallery.Components.Content.Input.Checkbox.Estate, pstate: AppEggShellGallery.Components.Content.Input.Checkbox.Pstate, actions: AppEggShellGallery.Components.Content.Input.Checkbox.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
    AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
        props = (AppEggShellGallery.Components.ComponentContent.ForFullyQualifiedName "LibClient.Components.Input.Checkbox"),
        displayName = ("Input.Checkbox"),
        notes =
                (castAsElementAckingKeysWarning [|
                    makeTextNode2 __parentFQN "Label defaulting to ~Children is for backwards compatibility."
                |]),
        samples =
                (castAsElementAckingKeysWarning [|
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("Basics"),
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
                    <LC.Input.Checkbox
                     Value='estate.B'
                     OnChange='actions.OnChangeB'
                     Validity='Valid'/>
                        <uitext>Children-based Label</uitext>
                    </LC.Input.Checkbox>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.Checkbox"
                                                    LibClient.Components.Constructors.LC.Input.Checkbox(
                                                        validity = (Valid),
                                                        onChange = (actions.OnChangeB),
                                                        value = (estate.B),
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                                                LibClient.Components.Constructors.LC.LegacyUiText(
                                                                    children =
                                                                        [|
                                                                            makeTextNode2 __parentFQN "Children-based Label"
                                                                        |]
                                                                )
                                                            |]
                                                    )
                                                |])
                                    )
                                |])
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("Validation"),
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
                    <LC.Input.Checkbox
                     Label='~String ""I want fries with that""'
                     Value='estate.A'
                     OnChange='actions.OnChangeA'
                     Validity='Invalid ""Required""'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.Checkbox"
                                                    LibClient.Components.Constructors.LC.Input.Checkbox(
                                                        validity = (Invalid "Required"),
                                                        onChange = (actions.OnChangeA),
                                                        value = (estate.A),
                                                        label = (LibClient.Components.Input.Checkbox.String "I want fries with that")
                                                    )
                                                |])
                                    )
                                |])
                    )
                |])
    )
