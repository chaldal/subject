module AppEggShellGallery.Components.Content.Input.EmailAddressRender

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

open AppEggShellGallery.Components.Content.Input.EmailAddress
open LibClient


let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.Input.EmailAddress.Props, estate: AppEggShellGallery.Components.Content.Input.EmailAddress.Estate, pstate: AppEggShellGallery.Components.Content.Input.EmailAddress.Pstate, actions: AppEggShellGallery.Components.Content.Input.EmailAddress.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
    AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
        props = (AppEggShellGallery.Components.ComponentContent.ForFullyQualifiedName "LibClient.Components.Input.EmailAddress"),
        displayName = ("Input.EmailAddress"),
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
                    <LC.Input.EmailAddress
                     rt-fs='true'
                     Label='""Email Address""'
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
                                                    let __parentFQN = Some "LibClient.Components.Input.EmailAddress"
                                                    LibClient.Components.Constructors.LC.Input.EmailAddress(
                                                        validity = (Valid),
                                                        onChange = (actions.OnChange "A"),
                                                        value = (estate.Values.Item "A"),
                                                        label = ("Email Address")
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
                                    makeTextNode2 __parentFQN "This component performs internal validation of the input, which has higher precedence\n                for display that externally supplied validation, e.g. if the input is not a valid\n                email address, and the caller is also (for some reason) passing a InputValidity.Missing, then\n                it's the invalid email address message that'll be displayed, and only once that's fixed will\n                the externally provided Validity be shown."
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
                    <LC.Input.EmailAddress
                     rt-fs='true'
                     Label='""Email Address""'
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
                                                    let __parentFQN = Some "LibClient.Components.Input.EmailAddress"
                                                    LibClient.Components.Constructors.LC.Input.EmailAddress(
                                                        validity = (Valid),
                                                        onChange = (actions.OnChange "B"),
                                                        value = (estate.Values.Item "B"),
                                                        label = ("Email Address")
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
                    <LC.Input.EmailAddress
                     rt-fs='true'
                     Label='""Email Address""'
                     Value='estate.Values.Item ""C""'
                     OnChange='actions.OnChange ""C""'
                     Validity='Missing'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.EmailAddress"
                                                    LibClient.Components.Constructors.LC.Input.EmailAddress(
                                                        validity = (Valid),
                                                        onChange = (actions.OnChange "C"),
                                                        value = (estate.Values.Item "C"),
                                                        label = ("Email Address")
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
                    <LC.Input.EmailAddress
                     rt-fs='true'
                     Label='""Email Address""'
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
                                                    let __parentFQN = Some "LibClient.Components.Input.EmailAddress"
                                                    LibClient.Components.Constructors.LC.Input.EmailAddress(
                                                        validity = (Valid),
                                                        onChange = (actions.OnChange "D"),
                                                        value = (estate.Values.Item "D"),
                                                        label = ("Email Address")
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
                    <LC.Input.EmailAddress
                     rt-fs='true'
                     Label='""Email Address""'
                     Value='estate.Values.Item ""A""'
                     OnChange='actions.OnChange ""A""'
                     Validity='Missing'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.EmailAddress"
                                                    LibClient.Components.Constructors.LC.Input.EmailAddress(
                                                        validity = (Missing),
                                                        onChange = (actions.OnChange "A"),
                                                        value = (estate.Values.Item "A"),
                                                        label = ("Email Address")
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
                    <LC.Input.EmailAddress
                     rt-fs='true'
                     Label='""Email Address""'
                     Value='estate.Values.Item ""A""'
                     OnChange='actions.OnChange ""A""'
                     Validity='Invalid ""This input is just bad""'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.EmailAddress"
                                                    LibClient.Components.Constructors.LC.Input.EmailAddress(
                                                        validity = (Invalid "This input is just bad"),
                                                        onChange = (actions.OnChange "A"),
                                                        value = (estate.Values.Item "A"),
                                                        label = ("Email Address")
                                                    )
                                                |])
                                    )
                                |])
                    )
                |])
    )
