module AppEggShellGallery.Components.Content.Input.TextRender

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

open AppEggShellGallery.Components.Content.Input.Text
open LibClient


let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.Input.Text.Props, estate: AppEggShellGallery.Components.Content.Input.Text.Estate, pstate: AppEggShellGallery.Components.Content.Input.Text.Pstate, actions: AppEggShellGallery.Components.Content.Input.Text.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
    AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
        props = (AppEggShellGallery.Components.ComponentContent.ForFullyQualifiedName "LibClient.Components.Input.Text"),
        displayName = ("Input.Text"),
        notes =
                (castAsElementAckingKeysWarning [|
                    makeTextNode2 __parentFQN "This component is a work in progress. If there's a feature that's not already\n        supported, check if RX.TextInput supports it, and if so, plumb it through."
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
                    <LC.Input.Text
                     Label='""Name""'
                     Value='estate.Values.Item ""A""'
                     Validity='Valid'
                     RequestFocusOnMount='true'
                     OnChange='actions.OnChange ""A""'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.Text"
                                                    LibClient.Components.Constructors.LC.Input.Text(
                                                        onChange = (actions.OnChange "A"),
                                                        requestFocusOnMount = (true),
                                                        validity = (Valid),
                                                        value = (estate.Values.Item "A"),
                                                        label = ("Name")
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
                    <LC.Input.Text
                     Label='""Fruit""'
                     Value='estate.Values.Item ""B""'
                     Validity='Valid'
                     OnChange='actions.OnChange ""B""'
                     Multiline='true'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.Text"
                                                    LibClient.Components.Constructors.LC.Input.Text(
                                                        multiline = (true),
                                                        onChange = (actions.OnChange "B"),
                                                        validity = (Valid),
                                                        value = (estate.Values.Item "B"),
                                                        label = ("Fruit")
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
                    <LC.Input.Text
                     Label='""Title""'
                     Value='estate.Values.Item ""C""'
                     Validity='Valid'
                     OnChange='actions.OnChange ""C""'
                     Placeholder='""e.g. Kafka on the Shore""'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.Text"
                                                    LibClient.Components.Constructors.LC.Input.Text(
                                                        placeholder = ("e.g. Kafka on the Shore"),
                                                        onChange = (actions.OnChange "C"),
                                                        validity = (Valid),
                                                        value = (estate.Values.Item "C"),
                                                        label = ("Title")
                                                    )
                                                |])
                                    )
                                |])
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("Prefix"),
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
                    <LC.Input.Text
                     Label='""Price""'
                     Prefix='""$""'
                     Value='estate.Values.Item ""F1""'
                     Validity='Valid'
                     OnChange='actions.OnChange ""F1""'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.Text"
                                                    LibClient.Components.Constructors.LC.Input.Text(
                                                        onChange = (actions.OnChange "F1"),
                                                        validity = (Valid),
                                                        value = (estate.Values.Item "F1"),
                                                        prefix = ("$"),
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
                    <LC.Input.Text
                     Label='""Price""'
                     Prefix='""$""'
                     Value='estate.Values.Item ""F2""'
                     Validity='Valid'
                     OnChange='actions.OnChange ""F2""'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.Text"
                                                    LibClient.Components.Constructors.LC.Input.Text(
                                                        onChange = (actions.OnChange "F2"),
                                                        validity = (Valid),
                                                        value = (estate.Values.Item "F2"),
                                                        prefix = ("$"),
                                                        label = ("Price")
                                                    )
                                                |])
                                    )
                                |])
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("Suffix"),
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
                    <LC.Input.Text
                     Label='""Portion Size""'
                     Suffix='^ ""Persons""'
                     Value='estate.Values.Item ""G1""'
                     Validity='Valid'
                     OnChange='actions.OnChange ""G1""'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.Text"
                                                    LibClient.Components.Constructors.LC.Input.Text(
                                                        onChange = (actions.OnChange "G1"),
                                                        validity = (Valid),
                                                        value = (estate.Values.Item "G1"),
                                                        suffix = (LibClient.Components.Input.Text.PropSuffixFactory.Make "Persons"),
                                                        label = ("Portion Size")
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
                    <LC.Input.Text
                     Label='""Portion Size""'
                     Suffix='^ ""Persons""'
                     Value='estate.Values.Item ""G2""'
                     Validity='Valid'
                     OnChange='actions.OnChange ""G2""'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.Text"
                                                    LibClient.Components.Constructors.LC.Input.Text(
                                                        onChange = (actions.OnChange "G2"),
                                                        validity = (Valid),
                                                        value = (estate.Values.Item "G2"),
                                                        suffix = (LibClient.Components.Input.Text.PropSuffixFactory.Make "Persons"),
                                                        label = ("Portion Size")
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
                    <LC.Input.Text
                     Label='""Portion Size""'
                     Suffix='^ Icon.MagnifyingGlass'
                     Value='estate.Values.Item ""G2""'
                     Validity='Valid'
                     OnChange='actions.OnChange ""G2""'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.Text"
                                                    LibClient.Components.Constructors.LC.Input.Text(
                                                        onChange = (actions.OnChange "G2"),
                                                        validity = (Valid),
                                                        value = (estate.Values.Item "G2"),
                                                        suffix = (LibClient.Components.Input.Text.PropSuffixFactory.Make Icon.MagnifyingGlass),
                                                        label = ("Portion Size")
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
                    <LC.Input.Text
                     Label='""Name""'
                     Value='estate.Values.Item ""D""'
                     Validity='Missing'
                     OnChange='actions.OnChange ""D""'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.Text"
                                                    LibClient.Components.Constructors.LC.Input.Text(
                                                        onChange = (actions.OnChange "D"),
                                                        validity = (Missing),
                                                        value = (estate.Values.Item "D"),
                                                        label = ("Name")
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
                    <LC.Input.Text
                     Label='""Name""'
                     Value='estate.Values.Item ""E""'
                     Validity='Invalid ""Something is wrong with this value""'
                     OnChange='actions.OnChange ""D""'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.Text"
                                                    LibClient.Components.Constructors.LC.Input.Text(
                                                        onChange = (actions.OnChange "D"),
                                                        validity = (Invalid "Something is wrong with this value"),
                                                        value = (estate.Values.Item "E"),
                                                        label = ("Name")
                                                    )
                                                |])
                                    )
                                |])
                    )
                |])
    )
