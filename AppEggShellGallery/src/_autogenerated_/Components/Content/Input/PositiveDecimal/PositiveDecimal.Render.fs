module AppEggShellGallery.Components.Content.Input.PositiveDecimalRender

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

open AppEggShellGallery.Components.Content.Input.PositiveDecimal
open LibClient


let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.Input.PositiveDecimal.Props, estate: AppEggShellGallery.Components.Content.Input.PositiveDecimal.Estate, pstate: AppEggShellGallery.Components.Content.Input.PositiveDecimal.Pstate, actions: AppEggShellGallery.Components.Content.Input.PositiveDecimal.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
    AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
        props = (AppEggShellGallery.Components.ComponentContent.ForFullyQualifiedName "LibClient.Components.Input.PositiveDecimal"),
        displayName = ("Input.PositiveDecimal"),
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
                    <LC.Input.PositiveDecimal
                     Label='""Price""'
                     Value='estate.Values.Item ""A""'
                     OnChange='actions.OnChange ""A""'
                     Validity='Valid'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.PositiveDecimal"
                                                    LibClient.Components.Constructors.LC.Input.PositiveDecimal(
                                                        validity = (Valid),
                                                        onChange = (actions.OnChange "A"),
                                                        value = (estate.Values.Item "A"),
                                                        label = ("Price")
                                                    )
                                                |])
                                    )
                                |])
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("Internal Validation"),
                        notes =
                                (castAsElementAckingKeysWarning [|
                                    makeTextNode2 __parentFQN "This component performs internal validation of the input, which has higher precedence\n                for display that externally supplied validation, e.g. if the input is non-numeric, and\n                the caller is also (for some reason) passing a InputValidity.Missing, then it's the\n                non-numeric error message that'll be displayed, and only once that's fixed will the\n                externally provided Validity be shown."
                                |]),
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
                    <LC.Input.PositiveDecimal
                     Label='""Price""'
                     Value='estate.Values.Item ""B""'
                     OnChange='actions.OnChange ""B""'
                     Validity='Valid'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.PositiveDecimal"
                                                    LibClient.Components.Constructors.LC.Input.PositiveDecimal(
                                                        validity = (Valid),
                                                        onChange = (actions.OnChange "B"),
                                                        value = (estate.Values.Item "B"),
                                                        label = ("Price")
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
                    <LC.Input.PositiveDecimal
                     Label='""Price""'
                     Value='estate.Values.Item ""C""'
                     OnChange='actions.OnChange ""C""'
                     Validity='Valid'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.PositiveDecimal"
                                                    LibClient.Components.Constructors.LC.Input.PositiveDecimal(
                                                        validity = (Valid),
                                                        onChange = (actions.OnChange "C"),
                                                        value = (estate.Values.Item "C"),
                                                        label = ("Price")
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
                    <LC.Input.PositiveDecimal
                     Label='""Price""'
                     Value='estate.Values.Item ""D""'
                     OnChange='actions.OnChange ""D""'
                     Validity='Valid'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        notes =
                                                (castAsElementAckingKeysWarning [|
                                                    makeTextNode2 __parentFQN "Note that an empty field is not considered internally invalid. Doing so would\n                    result in a tonne of red fields when the form is empty, which would a horrible\n                    user experience. So instead, the component returns an `Ok None`\n                    in this case, and exernally providing `InputValidity.Missing` when\n                    a form submission was attempted is the right way to deal with missing values."
                                                |]),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.PositiveDecimal"
                                                    LibClient.Components.Constructors.LC.Input.PositiveDecimal(
                                                        validity = (Valid),
                                                        onChange = (actions.OnChange "D"),
                                                        value = (estate.Values.Item "D"),
                                                        label = ("Price")
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
                    <LC.Input.PositiveDecimal
                     Label='""Price""'
                     Value='estate.Values.Item ""G""'
                     OnChange='actions.OnChange ""G""'
                     Validity='Valid'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.PositiveDecimal"
                                                    LibClient.Components.Constructors.LC.Input.PositiveDecimal(
                                                        validity = (Valid),
                                                        onChange = (actions.OnChange "G"),
                                                        value = (estate.Values.Item "G"),
                                                        label = ("Price")
                                                    )
                                                |])
                                    )
                                |])
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("External Validation"),
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
                    <LC.Input.PositiveDecimal
                     Label='""Price""'
                     Value='estate.Values.Item ""E""'
                     Validity='Missing'
                     OnChange='actions.OnChange ""E""'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.PositiveDecimal"
                                                    LibClient.Components.Constructors.LC.Input.PositiveDecimal(
                                                        onChange = (actions.OnChange "E"),
                                                        validity = (Missing),
                                                        value = (estate.Values.Item "E"),
                                                        label = ("Price")
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
                    <LC.Input.PositiveDecimal
                     Label='""Price""'
                     Value='estate.Values.Item ""F""'
                     Validity='Invalid ""This input is just bad""'
                     OnChange='actions.OnChange ""F""'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.PositiveDecimal"
                                                    LibClient.Components.Constructors.LC.Input.PositiveDecimal(
                                                        onChange = (actions.OnChange "F"),
                                                        validity = (Invalid "This input is just bad"),
                                                        value = (estate.Values.Item "F"),
                                                        label = ("Price")
                                                    )
                                                |])
                                    )
                                |])
                    )
                |])
    )
