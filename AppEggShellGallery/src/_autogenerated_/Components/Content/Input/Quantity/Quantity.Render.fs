module AppEggShellGallery.Components.Content.Input.QuantityRender

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

open AppEggShellGallery.Components.Content.Input.Quantity
open LibClient


let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.Input.Quantity.Props, estate: AppEggShellGallery.Components.Content.Input.Quantity.Estate, pstate: AppEggShellGallery.Components.Content.Input.Quantity.Pstate, actions: AppEggShellGallery.Components.Content.Input.Quantity.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
    AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
        props = (AppEggShellGallery.Components.ComponentContent.ForFullyQualifiedName "LibClient.Components.Input.Quantity"),
        displayName = ("Input.Quantity"),
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
                    <LC.Input.Quantity
                     Value='estate.Values.Item ""A""'
                     OnChange='~CanRemove (actions.OnChangeOption ""A"")'
                     Validity='Valid'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.Quantity"
                                                    LibClient.Components.Constructors.LC.Input.Quantity(
                                                        validity = (Valid),
                                                        onChange = (LC.Input.QuantityTypes.CanRemove (actions.OnChangeOption "A")),
                                                        value = (estate.Values.Item "A")
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
                    <LC.Input.Quantity
                     Value='estate.Values.Item ""B""'
                     OnChange='~CannotRemove (actions.OnChange ""B"")'
                     Validity='Valid'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.Quantity"
                                                    LibClient.Components.Constructors.LC.Input.Quantity(
                                                        validity = (Valid),
                                                        onChange = (LC.Input.QuantityTypes.CannotRemove (actions.OnChange "B")),
                                                        value = (estate.Values.Item "B")
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
                    <LC.Input.Quantity
                     Value='estate.Values.Item ""C""'
                     Max='5 |> PositiveInteger.ofLiteral'
                     OnChange='~CannotRemove (actions.OnChange ""C"")'
                     Validity='Valid'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        notes =
                                                (castAsElementAckingKeysWarning [|
                                                    makeTextNode2 __parentFQN "The Max prop can only enforce that the plus button is not visible\n                    and that OnChange is never called with a value greater than Max,\n                    but it cannot prevent Value from containing a value greater\n                    than Max, since that is managed externally. So external validation\n                    is necessary."
                                                |]),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.Quantity"
                                                    LibClient.Components.Constructors.LC.Input.Quantity(
                                                        validity = (Valid),
                                                        onChange = (LC.Input.QuantityTypes.CannotRemove (actions.OnChange "C")),
                                                        max = (5 |> PositiveInteger.ofLiteral),
                                                        value = (estate.Values.Item "C")
                                                    )
                                                |])
                                    )
                                |])
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("Validation (hardcoded, not dynamic)"),
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
                    <LC.Input.Quantity
                     Value='estate.Values.Item ""D""'
                     Validity='Missing'
                     OnChange='~CanRemove (actions.OnChangeOption ""D"")'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.Quantity"
                                                    LibClient.Components.Constructors.LC.Input.Quantity(
                                                        onChange = (LC.Input.QuantityTypes.CanRemove (actions.OnChangeOption "D")),
                                                        validity = (Missing),
                                                        value = (estate.Values.Item "D")
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
                    <LC.Input.Quantity
                     Value='estate.Values.Item ""E""'
                     Validity='Invalid ""Not allowed so many""'
                     Max='5 |> PositiveInteger.ofLiteral'
                     OnChange='~CanRemove (actions.OnChangeOption ""E"")'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.Quantity"
                                                    LibClient.Components.Constructors.LC.Input.Quantity(
                                                        onChange = (LC.Input.QuantityTypes.CanRemove (actions.OnChangeOption "E")),
                                                        max = (5 |> PositiveInteger.ofLiteral),
                                                        validity = (Invalid "Not allowed so many"),
                                                        value = (estate.Values.Item "E")
                                                    )
                                                |])
                                    )
                                |])
                    )
                |])
    )
