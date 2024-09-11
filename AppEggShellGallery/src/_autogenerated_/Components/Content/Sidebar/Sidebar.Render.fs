module AppEggShellGallery.Components.Content.SidebarRender

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

open AppEggShellGallery.Components.Content.Sidebar



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.Sidebar.Props, estate: AppEggShellGallery.Components.Content.Sidebar.Estate, pstate: AppEggShellGallery.Components.Content.Sidebar.Pstate, actions: AppEggShellGallery.Components.Content.Sidebar.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
    AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
        displayName = ("Sidebar"),
        props =
            (
                AppEggShellGallery.Components.ComponentContent.Manual
                    (
                            (castAsElementAckingKeysWarning [|
                                let __parentFQN = Some "AppEggShellGallery.Components.ScrapedComponentProps"
                                AppEggShellGallery.Components.Constructors.Ui.ScrapedComponentProps(
                                    fullyQualifiedName = ("LibClient.Components.Sidebar.Base"),
                                    heading = ("Sidebar.Base")
                                )
                                let __parentFQN = Some "AppEggShellGallery.Components.ScrapedComponentProps"
                                AppEggShellGallery.Components.Constructors.Ui.ScrapedComponentProps(
                                    fullyQualifiedName = ("LibClient.Components.Sidebar.Item"),
                                    heading = ("Sidebar.Item")
                                )
                                let __parentFQN = Some "AppEggShellGallery.Components.ScrapedComponentProps"
                                AppEggShellGallery.Components.Constructors.Ui.ScrapedComponentProps(
                                    fullyQualifiedName = ("LibClient.Components.Sidebar.Heading"),
                                    heading = ("Sidebar.Heading")
                                )
                                let __parentFQN = Some "AppEggShellGallery.Components.ScrapedComponentProps"
                                AppEggShellGallery.Components.Constructors.Ui.ScrapedComponentProps(
                                    fullyQualifiedName = ("LibClient.Components.Sidebar.Divider"),
                                    heading = ("Sidebar.Divider")
                                )
                            |])
                    )
            ),
        samples =
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
                    <LC.Sidebar.Base class='sidebar'>
                        <rt-prop name='FixedTop'>
                            <div class='profile'>
                                <LC.Avatar rt-fs='true' Source='localImage ""/images/tifa.jpg""'/>
                                <uitext class='name'>Tifa Lockhart</uitext>
                                <uitext class='email'>tifa@ffvii.world</uitext>
                            </div>
                        </rt-prop>

                        <rt-prop name='ScrollableMiddle'>
                            <LC.Sidebar.Heading Text='""With Left Icons""'/>
                            <LC.Sidebar.Item Label='""Inbox""'    LeftIcon='Icon.TwoSheets' State='~Actionable ignore'/>
                            <LC.Sidebar.Item Label='""Calendar""' LeftIcon='Icon.Calendar'  State='~Actionable ignore'/>
                            <LC.Sidebar.Item Label='""Starred""'  LeftIcon='Icon.Star'      State='~Actionable ignore'/>
                            <LC.Sidebar.Item Label='""Tags""'     LeftIcon='Icon.Tags'      State='~Actionable ignore'/>

                            <LC.Sidebar.Divider/>
                            <LC.Sidebar.Heading Text='""Without Left Icons""'/>
                            <LC.Sidebar.Item Label='""Settings &amp; Account""' State='~Actionable ignore'/>
                            <LC.Sidebar.Item Label='""Help &amp; Feedback""'    State='~Actionable ignore'/>

                            <LC.Sidebar.Divider/>
                            <LC.Sidebar.Heading Text='""Right Icon/Badge""'/>
                            <LC.Sidebar.Item Label='""Notifications""' Right='~Right.Badge (PositiveInteger.ofLiteral 3)' State='~Actionable ignore'/>
                            <LC.Sidebar.Item Label='""Orders""'        Right='~Right.Icon Icon.Bell'                      State='~Actionable ignore'/>

                            <LC.Sidebar.Divider/>
                            <LC.Sidebar.Heading Text='""States""'/>
                            <LC.Sidebar.Item Label='""Disabled""'    State='~Disabled'/>
                            <LC.Sidebar.Item Label='""Selected""'    State='~Selected'/>
                            <LC.Sidebar.Item Label='""In Progress""' State='~InProgress'/>
                        </rt-prop>

                        <rt-prop name='FixedBottom'>
                            <LC.Sidebar.Item Label='""Log Out""' LeftIcon='Icon.Power' State='~Actionable ignore'/>
                        </rt-prop>
                    </LC.Sidebar.Base>
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
                    ""sidebar"" => [
                        height 600
                    ]
                "
                                                            |> makeTextNode2 __parentFQN
                                                        |]
                                                )
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.Sidebar.Base"
                                    let __currClass = "sidebar"
                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                    LibClient.Components.Constructors.LC.Sidebar.Base(
                                        fixedBottom =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                    LibClient.Components.Constructors.LC.Sidebar.Item(
                                                        state = (LibClient.Components.Sidebar.Item.Actionable ignore),
                                                        leftIcon = (Icon.Power),
                                                        label = ("Log Out")
                                                    )
                                                |]),
                                        fixedTop =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                    let __currClass = "profile"
                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                    ReactXP.Components.Constructors.RX.View(
                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "LibClient.Components.Avatar"
                                                                LibClient.Components.Constructors.LC.Avatar(
                                                                    source = (localImage "/images/tifa.jpg")
                                                                )
                                                                let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                                                let __currClass = "name"
                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                LibClient.Components.Constructors.LC.LegacyUiText(
                                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                    children =
                                                                        [|
                                                                            makeTextNode2 __parentFQN "Tifa Lockhart"
                                                                        |]
                                                                )
                                                                let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                                                let __currClass = "email"
                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                LibClient.Components.Constructors.LC.LegacyUiText(
                                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                    children =
                                                                        [|
                                                                            makeTextNode2 __parentFQN "tifa@ffvii.world"
                                                                        |]
                                                                )
                                                            |]
                                                    )
                                                |]),
                                        scrollableMiddle =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Sidebar.Heading"
                                                    LibClient.Components.Constructors.LC.Sidebar.Heading(
                                                        text = ("With Left Icons")
                                                    )
                                                    let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                    LibClient.Components.Constructors.LC.Sidebar.Item(
                                                        state = (LibClient.Components.Sidebar.Item.Actionable ignore),
                                                        leftIcon = (Icon.TwoSheets),
                                                        label = ("Inbox")
                                                    )
                                                    let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                    LibClient.Components.Constructors.LC.Sidebar.Item(
                                                        state = (LibClient.Components.Sidebar.Item.Actionable ignore),
                                                        leftIcon = (Icon.Calendar),
                                                        label = ("Calendar")
                                                    )
                                                    let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                    LibClient.Components.Constructors.LC.Sidebar.Item(
                                                        state = (LibClient.Components.Sidebar.Item.Actionable ignore),
                                                        leftIcon = (Icon.Star),
                                                        label = ("Starred")
                                                    )
                                                    let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                    LibClient.Components.Constructors.LC.Sidebar.Item(
                                                        state = (LibClient.Components.Sidebar.Item.Actionable ignore),
                                                        leftIcon = (Icon.Tags),
                                                        label = ("Tags")
                                                    )
                                                    let __parentFQN = Some "LibClient.Components.Sidebar.Divider"
                                                    LibClient.Components.Constructors.LC.Sidebar.Divider()
                                                    let __parentFQN = Some "LibClient.Components.Sidebar.Heading"
                                                    LibClient.Components.Constructors.LC.Sidebar.Heading(
                                                        text = ("Without Left Icons")
                                                    )
                                                    let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                    LibClient.Components.Constructors.LC.Sidebar.Item(
                                                        state = (LibClient.Components.Sidebar.Item.Actionable ignore),
                                                        label = ("Settings & Account")
                                                    )
                                                    let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                    LibClient.Components.Constructors.LC.Sidebar.Item(
                                                        state = (LibClient.Components.Sidebar.Item.Actionable ignore),
                                                        label = ("Help & Feedback")
                                                    )
                                                    let __parentFQN = Some "LibClient.Components.Sidebar.Divider"
                                                    LibClient.Components.Constructors.LC.Sidebar.Divider()
                                                    let __parentFQN = Some "LibClient.Components.Sidebar.Heading"
                                                    LibClient.Components.Constructors.LC.Sidebar.Heading(
                                                        text = ("Right Icon/Badge")
                                                    )
                                                    let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                    LibClient.Components.Constructors.LC.Sidebar.Item(
                                                        state = (LibClient.Components.Sidebar.Item.Actionable ignore),
                                                        right = (LibClient.Components.Sidebar.Item.Right.Badge (PositiveInteger.ofLiteral 3)),
                                                        label = ("Notifications")
                                                    )
                                                    let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                    LibClient.Components.Constructors.LC.Sidebar.Item(
                                                        state = (LibClient.Components.Sidebar.Item.Actionable ignore),
                                                        right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.Bell),
                                                        label = ("Orders")
                                                    )
                                                    let __parentFQN = Some "LibClient.Components.Sidebar.Divider"
                                                    LibClient.Components.Constructors.LC.Sidebar.Divider()
                                                    let __parentFQN = Some "LibClient.Components.Sidebar.Heading"
                                                    LibClient.Components.Constructors.LC.Sidebar.Heading(
                                                        text = ("States")
                                                    )
                                                    let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                    LibClient.Components.Constructors.LC.Sidebar.Item(
                                                        state = (LibClient.Components.Sidebar.Item.Disabled),
                                                        label = ("Disabled")
                                                    )
                                                    let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                    LibClient.Components.Constructors.LC.Sidebar.Item(
                                                        state = (LibClient.Components.Sidebar.Item.Selected),
                                                        label = ("Selected")
                                                    )
                                                    let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                    LibClient.Components.Constructors.LC.Sidebar.Item(
                                                        state = (LibClient.Components.Sidebar.Item.InProgress),
                                                        label = ("In Progress")
                                                    )
                                                |]),
                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                    )
                                |])
                    )
                |])
    )
