module AppEggShellGallery.Components.Content.Input.DateRender

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

open AppEggShellGallery.Components.Content.Input.Date
open System


let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.Input.Date.Props, estate: AppEggShellGallery.Components.Content.Input.Date.Estate, pstate: AppEggShellGallery.Components.Content.Input.Date.Pstate, actions: AppEggShellGallery.Components.Content.Input.Date.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
    AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
        props = (AppEggShellGallery.Components.ComponentContent.ForFullyQualifiedName "LC.Input.Date"),
        displayName = ("Input.Date"),
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
                    TODO
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.Date"
                                                    LibClient.Components.Constructors.LC.Input.Date(
                                                        requestFocusOnMount = (true),
                                                        validity = (Valid),
                                                        onChange = (actions.OnChange "A"),
                                                        value = (estate.Values.Item "A"),
                                                        label = ("Date")
                                                    )
                                                |])
                                    )
                                |])
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("Date Restrictions"),
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
                    TODO
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.Date"
                                                    LibClient.Components.Constructors.LC.Input.Date(
                                                        validity = (Valid),
                                                        canSelectDate = (canSelectDate),
                                                        maxDate = (DateOnly.FromDateTime(DateTime.Now).AddMonths(1)),
                                                        minDate = (DateOnly.FromDateTime(DateTime.Now).AddMonths(-3)),
                                                        onChange = (actions.OnChange "B"),
                                                        value = (estate.Values.Item "B")
                                                    )
                                                |])
                                    )
                                |])
                    )
                    (* <ComponentSample>
            <rt-prop name='Visuals'>
                <LC.Input.Date
                 Value='estate.Values.Item "B"'
                 OnChange='actions.OnChange "B"'
                 Validity='Valid'>
                </LC.Input.Date>

            </rt-prop>
            <rt-prop name='Code |> ~singleBlock ~Render'><![CDATA[
                    TODO
            ]]></rt-prop>
        </ComponentSample> *)
                |])
    )
