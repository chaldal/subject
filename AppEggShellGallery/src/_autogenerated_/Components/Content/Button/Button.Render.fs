module AppEggShellGallery.Components.Content.ButtonRender

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

open AppEggShellGallery.Components.Content.Button



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.Button.Props, estate: AppEggShellGallery.Components.Content.Button.Estate, pstate: AppEggShellGallery.Components.Content.Button.Pstate, actions: AppEggShellGallery.Components.Content.Button.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
    AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
        props = (AppEggShellGallery.Components.ComponentContent.ForFullyQualifiedName "LibClient.Components.Button"),
        isResponsive = (true),
        displayName = ("Button"),
        notes =
                (castAsElementAckingKeysWarning [|
                    makeTextNode2 __parentFQN "Every LC.Button component below is wrapped with LC.Buttons to prevent it from expanding to full width.\n        This is not shown in code samples for simplicity."
                |]),
        samples =
                (castAsElementAckingKeysWarning [|
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("Icons"),
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
                        <LC.Button
                         Icon='~Icon.Left Icon.Home'
                         Label='""Submit""'
                         State='^LowLevel (~Actionable Actions.greet)' />
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Buttons"
                                                    LibClient.Components.Constructors.LC.Buttons(
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "LibClient.Components.Button"
                                                                LibClient.Components.Constructors.LC.Button(
                                                                    state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (LibClient.Components.Button.Actionable Actions.greet)),
                                                                    label = ("Submit"),
                                                                    icon = (LibClient.Components.Button.Icon.Left Icon.Home)
                                                                )
                                                            |]
                                                    )
                                                |])
                                    )
                                |])
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("Badge"),
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
                        <LC.Button
                         Icon='~Icon.Left Icon.Cart'
                         Label='""Cart""'
                         Badge='~Count 3'
                         State='^LowLevel (~Actionable Actions.greet)' />
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Buttons"
                                                    LibClient.Components.Constructors.LC.Buttons(
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "LibClient.Components.Button"
                                                                let __currClass = "button-with-badge"
                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                LibClient.Components.Constructors.LC.Button(
                                                                    state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (LibClient.Components.Button.Actionable Actions.greet)),
                                                                    badge = (LibClient.Components.Button.Count 3),
                                                                    label = ("Cart"),
                                                                    icon = (LibClient.Components.Button.Icon.Left Icon.ShoppingCart),
                                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                                )
                                                            |]
                                                    )
                                                |])
                                    )
                                |])
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("Primary"),
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
                        <LC.Button
                         Label='""Submit""'
                         Level='~Primary'
                         State='^LowLevel (~Actionable Actions.greet)' />
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Buttons"
                                                    LibClient.Components.Constructors.LC.Buttons(
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "LibClient.Components.Button"
                                                                LibClient.Components.Constructors.LC.Button(
                                                                    state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (LibClient.Components.Button.Actionable Actions.greet)),
                                                                    level = (LibClient.Components.Button.Primary),
                                                                    label = ("Submit")
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
                        <LC.Button
                         Label='""Submit""'
                         Level='~Primary'
                         State='^LowLevel ~InProgress' />
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Buttons"
                                                    LibClient.Components.Constructors.LC.Buttons(
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "LibClient.Components.Button"
                                                                LibClient.Components.Constructors.LC.Button(
                                                                    state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel LibClient.Components.Button.InProgress),
                                                                    level = (LibClient.Components.Button.Primary),
                                                                    label = ("Submit")
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
                        <LC.Button
                         Label='""Submit""'
                         Level='~Primary'
                         State='^Disabled' />
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Buttons"
                                                    LibClient.Components.Constructors.LC.Buttons(
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "LibClient.Components.Button"
                                                                LibClient.Components.Constructors.LC.Button(
                                                                    state = (LibClient.Components.Button.PropStateFactory.MakeDisabled),
                                                                    level = (LibClient.Components.Button.Primary),
                                                                    label = ("Submit")
                                                                )
                                                            |]
                                                    )
                                                |])
                                    )
                                |])
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("Secondary"),
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
                        <LC.Button
                         Label='""Submit""'
                         Level='~Secondary'
                         State='^LowLevel (~Actionable Actions.greet)' />
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Buttons"
                                                    LibClient.Components.Constructors.LC.Buttons(
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "LibClient.Components.Button"
                                                                LibClient.Components.Constructors.LC.Button(
                                                                    state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (LibClient.Components.Button.Actionable Actions.greet)),
                                                                    level = (LibClient.Components.Button.Secondary),
                                                                    label = ("Submit")
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
                        <LC.Button
                         Label='""Submit""'
                         Level='~Secondary'
                         State='^LowLevel ~InProgress' />
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Buttons"
                                                    LibClient.Components.Constructors.LC.Buttons(
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "LibClient.Components.Button"
                                                                LibClient.Components.Constructors.LC.Button(
                                                                    state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel LibClient.Components.Button.InProgress),
                                                                    level = (LibClient.Components.Button.Secondary),
                                                                    label = ("Submit")
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
                        <LC.Button
                         Label='""Submit""'
                         Level='~Secondary'
                         State='^Disabled' />
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Buttons"
                                                    LibClient.Components.Constructors.LC.Buttons(
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "LibClient.Components.Button"
                                                                LibClient.Components.Constructors.LC.Button(
                                                                    state = (LibClient.Components.Button.PropStateFactory.MakeDisabled),
                                                                    level = (LibClient.Components.Button.Secondary),
                                                                    label = ("Submit")
                                                                )
                                                            |]
                                                    )
                                                |])
                                    )
                                |])
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("Tertiary"),
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
                        <LC.Button
                         Label='""Submit""'
                         Level='~Tertiary'
                         State='^LowLevel (~Actionable Actions.greet)' />
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Buttons"
                                                    LibClient.Components.Constructors.LC.Buttons(
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "LibClient.Components.Button"
                                                                LibClient.Components.Constructors.LC.Button(
                                                                    state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (LibClient.Components.Button.Actionable Actions.greet)),
                                                                    level = (LibClient.Components.Button.Tertiary),
                                                                    label = ("Submit")
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
                        <LC.Button
                         Label='""Submit""'
                         Level='~Tertiary'
                         State='^LowLevel ~InProgress' />
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Buttons"
                                                    LibClient.Components.Constructors.LC.Buttons(
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "LibClient.Components.Button"
                                                                LibClient.Components.Constructors.LC.Button(
                                                                    state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel LibClient.Components.Button.InProgress),
                                                                    level = (LibClient.Components.Button.Tertiary),
                                                                    label = ("Submit")
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
                        <LC.Button
                         Label='""Submit""'
                         Level='~Tertiary'
                         State='^Disabled' />
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Buttons"
                                                    LibClient.Components.Constructors.LC.Buttons(
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "LibClient.Components.Button"
                                                                LibClient.Components.Constructors.LC.Button(
                                                                    state = (LibClient.Components.Button.PropStateFactory.MakeDisabled),
                                                                    level = (LibClient.Components.Button.Tertiary),
                                                                    label = ("Submit")
                                                                )
                                                            |]
                                                    )
                                                |])
                                    )
                                |])
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("PrimaryB"),
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
                        <LC.Button
                         Label='""Submit""'
                         Level='~PrimaryB'
                         State='^LowLevel (~Actionable Actions.greet)' />
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Buttons"
                                                    LibClient.Components.Constructors.LC.Buttons(
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "LibClient.Components.Button"
                                                                LibClient.Components.Constructors.LC.Button(
                                                                    state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (LibClient.Components.Button.Actionable Actions.greet)),
                                                                    level = (LibClient.Components.Button.PrimaryB),
                                                                    label = ("Submit")
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
                        <LC.Button
                         Label='""Submit""'
                         Level='~PrimaryB'
                         State='^LowLevel ~InProgress' />
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Buttons"
                                                    LibClient.Components.Constructors.LC.Buttons(
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "LibClient.Components.Button"
                                                                LibClient.Components.Constructors.LC.Button(
                                                                    state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel LibClient.Components.Button.InProgress),
                                                                    level = (LibClient.Components.Button.PrimaryB),
                                                                    label = ("Submit")
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
                        <LC.Button
                         Label='""Submit""'
                         Level='~PrimaryB'
                         State='^Disabled' />
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Buttons"
                                                    LibClient.Components.Constructors.LC.Buttons(
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "LibClient.Components.Button"
                                                                LibClient.Components.Constructors.LC.Button(
                                                                    state = (LibClient.Components.Button.PropStateFactory.MakeDisabled),
                                                                    level = (LibClient.Components.Button.PrimaryB),
                                                                    label = ("Submit")
                                                                )
                                                            |]
                                                    )
                                                |])
                                    )
                                |])
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("SecondaryB"),
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
                        <LC.Button
                         Label='""Submit""'
                         Level='~SecondaryB'
                         State='^LowLevel (~Actionable Actions.greet)' />
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Buttons"
                                                    LibClient.Components.Constructors.LC.Buttons(
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "LibClient.Components.Button"
                                                                LibClient.Components.Constructors.LC.Button(
                                                                    state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (LibClient.Components.Button.Actionable Actions.greet)),
                                                                    level = (LibClient.Components.Button.SecondaryB),
                                                                    label = ("Submit")
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
                        <LC.Button
                         Label='""Submit""'
                         Level='~SecondaryB'
                         State='^LowLevel ~InProgress' />
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Buttons"
                                                    LibClient.Components.Constructors.LC.Buttons(
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "LibClient.Components.Button"
                                                                LibClient.Components.Constructors.LC.Button(
                                                                    state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel LibClient.Components.Button.InProgress),
                                                                    level = (LibClient.Components.Button.SecondaryB),
                                                                    label = ("Submit")
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
                        <LC.Button
                         Label='""Submit""'
                         Level='~SecondaryB'
                         State='^Disabled' />
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Buttons"
                                                    LibClient.Components.Constructors.LC.Buttons(
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "LibClient.Components.Button"
                                                                LibClient.Components.Constructors.LC.Button(
                                                                    state = (LibClient.Components.Button.PropStateFactory.MakeDisabled),
                                                                    level = (LibClient.Components.Button.SecondaryB),
                                                                    label = ("Submit")
                                                                )
                                                            |]
                                                    )
                                                |])
                                    )
                                |])
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("Cautionary"),
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
                        <LC.Button
                         Label='""Submit""'
                         Level='~Cautionary'
                         State='^LowLevel (~Actionable Actions.greet)' />
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Buttons"
                                                    LibClient.Components.Constructors.LC.Buttons(
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "LibClient.Components.Button"
                                                                LibClient.Components.Constructors.LC.Button(
                                                                    state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (LibClient.Components.Button.Actionable Actions.greet)),
                                                                    level = (LibClient.Components.Button.Cautionary),
                                                                    label = ("Submit")
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
                        <LC.Button
                         Label='""Submit""'
                         Level='~Cautionary'
                         State='^LowLevel ~InProgress' />
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Buttons"
                                                    LibClient.Components.Constructors.LC.Buttons(
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "LibClient.Components.Button"
                                                                LibClient.Components.Constructors.LC.Button(
                                                                    state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel LibClient.Components.Button.InProgress),
                                                                    level = (LibClient.Components.Button.Cautionary),
                                                                    label = ("Submit")
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
                        <LC.Button
                         Label='""Submit""'
                         Level='~Cautionary'
                         State='^Disabled' />
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Buttons"
                                                    LibClient.Components.Constructors.LC.Buttons(
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "LibClient.Components.Button"
                                                                LibClient.Components.Constructors.LC.Button(
                                                                    state = (LibClient.Components.Button.PropStateFactory.MakeDisabled),
                                                                    level = (LibClient.Components.Button.Cautionary),
                                                                    label = ("Submit")
                                                                )
                                                            |]
                                                    )
                                                |])
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
                    <LC.Button
                     class='caution'
                     Label='""Submit""'
                     Level='~Cautionary'
                     State='^LowLevel (~Actionable Actions.greet)'/>
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
                    LibClient.Components.ButtonStyles.Theme.One (
                        level              = LibClient.Components.Button.Cautionary,
                        state              = LibClient.Components.Button.State.Actionable ignore,
                        theTextColor       = Color.Black,
                        theBorderColor     = Color.White,
                        theBackgroundColor = Color.DevOrange
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
                                    let __parentFQN = Some "LibClient.Components.Buttons"
                                    LibClient.Components.Constructors.LC.Buttons(
                                        children =
                                            [|
                                                let __parentFQN = Some "LibClient.Components.Button"
                                                let __currClass = "caution"
                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                LibClient.Components.Constructors.LC.Button(
                                                    state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (LibClient.Components.Button.Actionable Actions.greet)),
                                                    level = (LibClient.Components.Button.Cautionary),
                                                    label = ("Submit"),
                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                )
                                            |]
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
                    <LC.Button
                     class='btn-small'
                     Icon='~Icon.Right Icon.Home'
                     Label='""Submit""'
                     State='^LowLevel (~Actionable Actions.greet)'/>
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
                    ""btn-small"" ==> LibClient.Components.ButtonStyles.Theme.IconSize 15
                "
                                                            |> makeTextNode2 __parentFQN
                                                        |]
                                                )
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.Buttons"
                                    LibClient.Components.Constructors.LC.Buttons(
                                        children =
                                            [|
                                                let __parentFQN = Some "LibClient.Components.Button"
                                                let __currClass = "btn-small"
                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                LibClient.Components.Constructors.LC.Button(
                                                    state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (LibClient.Components.Button.Actionable Actions.greet)),
                                                    label = ("Submit"),
                                                    icon = (LibClient.Components.Button.Icon.Right Icon.Home),
                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                )
                                            |]
                                    )
                                |])
                    )
                |])
    )
