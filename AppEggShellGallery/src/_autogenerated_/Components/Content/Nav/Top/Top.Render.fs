module AppEggShellGallery.Components.Content.Nav.TopRender

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

open AppEggShellGallery.Components.Content.Nav.Top



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.Nav.Top.Props, estate: AppEggShellGallery.Components.Content.Nav.Top.Estate, pstate: AppEggShellGallery.Components.Content.Nav.Top.Pstate, actions: AppEggShellGallery.Components.Content.Nav.Top.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
    AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
        isResponsive = (true),
        displayName = ("Nav.Top"),
        props =
            (
                AppEggShellGallery.Components.ComponentContent.Manual
                    (
                            (castAsElementAckingKeysWarning [|
                                let __parentFQN = Some "AppEggShellGallery.Components.ScrapedComponentProps"
                                AppEggShellGallery.Components.Constructors.Ui.ScrapedComponentProps(
                                    fullyQualifiedName = ("LibClient.Components.Nav.Top.Base"),
                                    heading = ("Nav.Top.Base")
                                )
                                let __parentFQN = Some "AppEggShellGallery.Components.ScrapedComponentProps"
                                AppEggShellGallery.Components.Constructors.Ui.ScrapedComponentProps(
                                    fullyQualifiedName = ("LibClient.Components.Nav.Top.Item"),
                                    heading = ("Nav.Top.Item")
                                )
                                let __parentFQN = Some "AppEggShellGallery.Components.ScrapedComponentProps"
                                AppEggShellGallery.Components.Constructors.Ui.ScrapedComponentProps(
                                    fullyQualifiedName = ("LibClient.Components.Nav.Top.Heading"),
                                    heading = ("Nav.Top.Heading")
                                )
                                let __parentFQN = Some "AppEggShellGallery.Components.ScrapedComponentProps"
                                AppEggShellGallery.Components.Constructors.Ui.ScrapedComponentProps(
                                    fullyQualifiedName = ("LibClient.Components.Nav.Top.Image"),
                                    heading = ("Nav.Top.Image")
                                )
                                let __parentFQN = Some "AppEggShellGallery.Components.ScrapedComponentProps"
                                AppEggShellGallery.Components.Constructors.Ui.ScrapedComponentProps(
                                    fullyQualifiedName = ("LibClient.Components.Nav.Top.Filler"),
                                    heading = ("Nav.Top.Filler")
                                )
                                let __parentFQN = Some "AppEggShellGallery.Components.ScrapedComponentProps"
                                AppEggShellGallery.Components.Constructors.Ui.ScrapedComponentProps(
                                    fullyQualifiedName = ("LibClient.Components.Nav.Top.ShowSidebarButton"),
                                    heading = ("Nav.Top.ShowSidebarButton")
                                )
                            |])
                    )
            ),
        samples =
                (castAsElementAckingKeysWarning [|
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("Basics"),
                        samples =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                                        layout = (AppEggShellGallery.Components.ComponentSample.CodeBelowSamples),
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
                        <LC.Nav.Top.Base>
                            <rt-prop name='Desktop(_)'>
                                <LC.Nav.Top.Image Source='localImage ""/images/avatar.png""' class='image'/>
                                <LC.Nav.Top.Heading Text='""MATERIAL DESIGN""'/>
                                <LC.Nav.Top.Item Style='~labelOnly ""Dummy Inprogress""'                                           State='~InProgress'       />
                                <LC.Nav.Top.Item Style='~labelOnly ""Design""'                                                     State='~Actionable ignore'/>
                                <LC.Nav.Top.Item Style='~labelOnly ""Components""'                                                 State='~Selected'         />
                                <LC.Nav.Top.Item Style='~labelOnly ""Develop""'                                                    State='~Actionable ignore'/>
                                <LC.Nav.Top.Item Style='~labelOnly ""Blog""'                                                       State='~Disabled'         />
                                <LC.Nav.Top.Item Style='~iconOnly  Icon.MagnifyingGlass'                                         State='~Actionable ignore'/>
                                <LC.Nav.Top.Item Style='~Style.With(label = ""Cart"", icon = Icon.ShoppingCart, badge = ~Count 3)' State='~Actionable ignore'/>
                                <!-- There is also SelectedActionable for cases when an item represents a group of
                                subpages, and even though it's selected, you still want to give the user to navigate
                                to the top subpage for the group, or perhaps to force a refresh (like in an email inbox). -->
                            </rt-prop>
                            <rt-prop name='Handheld(_)'>
                                <LR.Nav.Top.BackButton/>
                                <LC.Nav.Top.Heading Text='""MATERIAL DESIGN""'/>
                                <LC.Nav.Top.Item Style='~labelOnly ""Home""'    State='~Selected'         />
                                <LC.Nav.Top.Item Style='~iconOnly  Icon.Menu' State='~Actionable ignore'/>
                            </rt-prop>
                        </LC.Nav.Top.Base>
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
                        ""image"" => [
                            height       32
                            width        32
                            borderRadius 16
                            marginRight  10
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
                                                    let __parentFQN = Some "LibClient.Components.With.Context"
                                                    LibClient.Components.Constructors.LC.With.Context(
                                                        context = (AppEggShellGallery.SampleVisualsScreenSize.sampleVisualsScreenSizeContext),
                                                        ``with`` =
                                                            (fun (sampleVisualsScreenSize: ScreenSize) ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        let __parentFQN = Some "LibClient.Components.ForceContext"
                                                                        LibClient.Components.Constructors.LC.ForceContext(
                                                                            value = (sampleVisualsScreenSize),
                                                                            context = (screenSizeContext),
                                                                            children =
                                                                                [|
                                                                                    let __parentFQN = Some "LibClient.Components.Nav.Top.Base"
                                                                                    LibClient.Components.Constructors.LC.Nav.Top.Base(
                                                                                        desktop =
                                                                                            (fun (_) ->
                                                                                                    (castAsElementAckingKeysWarning [|
                                                                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Image"
                                                                                                        let __currClass = "image"
                                                                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                        LibClient.Components.Constructors.LC.Nav.Top.Image(
                                                                                                            source = (localImage "/images/avatar.png"),
                                                                                                            ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                                                                        )
                                                                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Heading"
                                                                                                        LibClient.Components.Constructors.LC.Nav.Top.Heading(
                                                                                                            text = ("MATERIAL DESIGN")
                                                                                                        )
                                                                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Item"
                                                                                                        LibClient.Components.Constructors.LC.Nav.Top.Item(
                                                                                                            state = (LibClient.Components.Nav.Top.Item.InProgress),
                                                                                                            style = (LibClient.Components.Nav.Top.Item.labelOnly "Dummy Inprogress")
                                                                                                        )
                                                                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Item"
                                                                                                        LibClient.Components.Constructors.LC.Nav.Top.Item(
                                                                                                            state = (LibClient.Components.Nav.Top.Item.Actionable ignore),
                                                                                                            style = (LibClient.Components.Nav.Top.Item.labelOnly "Design")
                                                                                                        )
                                                                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Item"
                                                                                                        LibClient.Components.Constructors.LC.Nav.Top.Item(
                                                                                                            state = (LibClient.Components.Nav.Top.Item.Selected),
                                                                                                            style = (LibClient.Components.Nav.Top.Item.labelOnly "Components")
                                                                                                        )
                                                                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Item"
                                                                                                        LibClient.Components.Constructors.LC.Nav.Top.Item(
                                                                                                            state = (LibClient.Components.Nav.Top.Item.Actionable ignore),
                                                                                                            style = (LibClient.Components.Nav.Top.Item.labelOnly "Develop")
                                                                                                        )
                                                                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Item"
                                                                                                        LibClient.Components.Constructors.LC.Nav.Top.Item(
                                                                                                            state = (LibClient.Components.Nav.Top.Item.Disabled),
                                                                                                            style = (LibClient.Components.Nav.Top.Item.labelOnly "Blog")
                                                                                                        )
                                                                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Item"
                                                                                                        LibClient.Components.Constructors.LC.Nav.Top.Item(
                                                                                                            state = (LibClient.Components.Nav.Top.Item.Actionable ignore),
                                                                                                            style = (LibClient.Components.Nav.Top.Item.iconOnly  Icon.MagnifyingGlass)
                                                                                                        )
                                                                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Item"
                                                                                                        LibClient.Components.Constructors.LC.Nav.Top.Item(
                                                                                                            state = (LibClient.Components.Nav.Top.Item.Actionable ignore),
                                                                                                            style = (LibClient.Components.Nav.Top.Item.Style.With(label = "Cart", icon = Icon.ShoppingCart, badge = LibClient.Components.Nav.Top.Item.Count 3))
                                                                                                        )
                                                                                                    |])
                                                                                            ),
                                                                                        handheld =
                                                                                            (fun (_) ->
                                                                                                    (castAsElementAckingKeysWarning [|
                                                                                                        let __parentFQN = Some "LibRouter.Components.Nav.Top.BackButton"
                                                                                                        LibRouter.Components.Constructors.LR.Nav.Top.BackButton()
                                                                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Heading"
                                                                                                        LibClient.Components.Constructors.LC.Nav.Top.Heading(
                                                                                                            text = ("MATERIAL DESIGN")
                                                                                                        )
                                                                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Item"
                                                                                                        LibClient.Components.Constructors.LC.Nav.Top.Item(
                                                                                                            state = (LibClient.Components.Nav.Top.Item.Selected),
                                                                                                            style = (LibClient.Components.Nav.Top.Item.labelOnly "Home")
                                                                                                        )
                                                                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Item"
                                                                                                        LibClient.Components.Constructors.LC.Nav.Top.Item(
                                                                                                            state = (LibClient.Components.Nav.Top.Item.Actionable ignore),
                                                                                                            style = (LibClient.Components.Nav.Top.Item.iconOnly  Icon.Menu)
                                                                                                        )
                                                                                                    |])
                                                                                            )
                                                                                    )
                                                                                |]
                                                                        )
                                                                    |])
                                                            )
                                                    )
                                                |])
                                    )
                                |])
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("Without Heading"),
                        samples =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                                        layout = (AppEggShellGallery.Components.ComponentSample.CodeBelowSamples),
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
                        <LC.Nav.Top.Base>
                            <rt-prop name='Desktop(_)'>
                                <LC.Nav.Top.Image Source='localImage ""/images/avatar.png""' class='image'/>
                                <LC.Nav.Top.Filler/>
                                <LC.Nav.Top.Item Style='~labelOnly ""Dummy Inprogress""'   State='~InProgress'       />
                                <LC.Nav.Top.Item Style='~labelOnly ""Design""'             State='~Actionable ignore'/>
                                <LC.Nav.Top.Item Style='~labelOnly ""Components""'         State='~Selected'         />
                                <LC.Nav.Top.Item Style='~labelOnly ""Develop""'            State='~Actionable ignore'/>
                                <LC.Nav.Top.Item Style='~labelOnly ""Blog""'               State='~Disabled'         />
                                <LC.Nav.Top.Item Style='~iconOnly  Icon.MagnifyingGlass' State='~Actionable ignore'/>
                                <!-- There is also SelectedActionable for cases when an item represents a group of
                                subpages, and even though it's selected, you still want to give the user to navigate
                                to the top subpage for the group, or perhaps to force a refresh (like in an email inbox). -->
                            </rt-prop>
                            <rt-prop name='Handheld(_)'>
                                <LR.Nav.Top.BackButton/>
                                <LC.Nav.Top.Filler/>
                                <LC.Nav.Top.Item Style='~labelOnly ""Home""'    State='~Selected'         />
                                <LC.Nav.Top.Item Style='~iconOnly  Icon.Menu' State='~Actionable ignore'/>
                            </rt-prop>
                        </LC.Nav.Top.Base>
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
                        ""image"" => [
                            height       32
                            width        32
                            borderRadius 16
                            marginRight  10
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
                                                    let __parentFQN = Some "LibClient.Components.With.Context"
                                                    LibClient.Components.Constructors.LC.With.Context(
                                                        context = (AppEggShellGallery.SampleVisualsScreenSize.sampleVisualsScreenSizeContext),
                                                        ``with`` =
                                                            (fun (sampleVisualsScreenSize: ScreenSize) ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        let __parentFQN = Some "LibClient.Components.ForceContext"
                                                                        LibClient.Components.Constructors.LC.ForceContext(
                                                                            value = (sampleVisualsScreenSize),
                                                                            context = (screenSizeContext),
                                                                            children =
                                                                                [|
                                                                                    let __parentFQN = Some "LibClient.Components.Nav.Top.Base"
                                                                                    LibClient.Components.Constructors.LC.Nav.Top.Base(
                                                                                        desktop =
                                                                                            (fun (_) ->
                                                                                                    (castAsElementAckingKeysWarning [|
                                                                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Image"
                                                                                                        let __currClass = "image"
                                                                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                        LibClient.Components.Constructors.LC.Nav.Top.Image(
                                                                                                            source = (localImage "/images/avatar.png"),
                                                                                                            ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                                                                        )
                                                                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Filler"
                                                                                                        LibClient.Components.Constructors.LC.Nav.Top.Filler()
                                                                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Item"
                                                                                                        LibClient.Components.Constructors.LC.Nav.Top.Item(
                                                                                                            state = (LibClient.Components.Nav.Top.Item.InProgress),
                                                                                                            style = (LibClient.Components.Nav.Top.Item.labelOnly "Dummy Inprogress")
                                                                                                        )
                                                                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Item"
                                                                                                        LibClient.Components.Constructors.LC.Nav.Top.Item(
                                                                                                            state = (LibClient.Components.Nav.Top.Item.Actionable ignore),
                                                                                                            style = (LibClient.Components.Nav.Top.Item.labelOnly "Design")
                                                                                                        )
                                                                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Item"
                                                                                                        LibClient.Components.Constructors.LC.Nav.Top.Item(
                                                                                                            state = (LibClient.Components.Nav.Top.Item.Selected),
                                                                                                            style = (LibClient.Components.Nav.Top.Item.labelOnly "Components")
                                                                                                        )
                                                                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Item"
                                                                                                        LibClient.Components.Constructors.LC.Nav.Top.Item(
                                                                                                            state = (LibClient.Components.Nav.Top.Item.Actionable ignore),
                                                                                                            style = (LibClient.Components.Nav.Top.Item.labelOnly "Develop")
                                                                                                        )
                                                                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Item"
                                                                                                        LibClient.Components.Constructors.LC.Nav.Top.Item(
                                                                                                            state = (LibClient.Components.Nav.Top.Item.Disabled),
                                                                                                            style = (LibClient.Components.Nav.Top.Item.labelOnly "Blog")
                                                                                                        )
                                                                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Item"
                                                                                                        LibClient.Components.Constructors.LC.Nav.Top.Item(
                                                                                                            state = (LibClient.Components.Nav.Top.Item.Actionable ignore),
                                                                                                            style = (LibClient.Components.Nav.Top.Item.iconOnly  Icon.MagnifyingGlass)
                                                                                                        )
                                                                                                    |])
                                                                                            ),
                                                                                        handheld =
                                                                                            (fun (_) ->
                                                                                                    (castAsElementAckingKeysWarning [|
                                                                                                        let __parentFQN = Some "LibRouter.Components.Nav.Top.BackButton"
                                                                                                        LibRouter.Components.Constructors.LR.Nav.Top.BackButton()
                                                                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Filler"
                                                                                                        LibClient.Components.Constructors.LC.Nav.Top.Filler()
                                                                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Item"
                                                                                                        LibClient.Components.Constructors.LC.Nav.Top.Item(
                                                                                                            state = (LibClient.Components.Nav.Top.Item.Selected),
                                                                                                            style = (LibClient.Components.Nav.Top.Item.labelOnly "Home")
                                                                                                        )
                                                                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Item"
                                                                                                        LibClient.Components.Constructors.LC.Nav.Top.Item(
                                                                                                            state = (LibClient.Components.Nav.Top.Item.Actionable ignore),
                                                                                                            style = (LibClient.Components.Nav.Top.Item.iconOnly  Icon.Menu)
                                                                                                        )
                                                                                                    |])
                                                                                            )
                                                                                    )
                                                                                |]
                                                                        )
                                                                    |])
                                                            )
                                                    )
                                                |])
                                    )
                                |])
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("Style Sample"),
                        samples =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                                        layout = (AppEggShellGallery.Components.ComponentSample.CodeBelowSamples),
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
                        <LC.Nav.Top.Base>
                            <rt-prop name='Desktop(_)'>
                                <LC.Nav.Top.Item Style='~Style.With(label = ""Store"", badge = ~Text ""Summer Sale"")'               State='~Actionable ignore'                    />
                                <LC.Nav.Top.Item Style='~Style.With(icon  = Icon.Bell, badge = ~Count 2)'                        State='~Actionable ignore'                    />
                                <LC.Nav.Top.Item Style='~labelOnly ""Blog""'                                                       State='~Disabled'                             />
                                <LC.Nav.Top.Item Style='~iconOnly  Icon.MagnifyingGlass'                                         State='~Actionable ignore'                    />
                                <LC.Nav.Top.Item Style='~Style.With(label = ""Cart"", icon = Icon.ShoppingCart, badge = ~Count 3)' State='~Actionable ignore'                    />
                                <LC.Nav.Top.Item Style='~iconOnly Icon.X'                                                        State='~Actionable ignore' class='adjust-icon'/>
                            </rt-prop>
                            <rt-prop name='Handheld(_)'>
                                <LC.Nav.Top.Item Style='~Style.With(label = ""Store"", badge = ~Text ""Summer Sale"")'               State='~Actionable ignore'/>
                                <LC.Nav.Top.Item Style='~Style.With(icon  = Icon.Bell, badge = ~Count 2)'                        State='~Actionable ignore'/>
                                <LC.Nav.Top.Item Style='~labelOnly ""Blog""'                                                       State='~Disabled'         />
                                <LC.Nav.Top.Item Style='~iconOnly  Icon.MagnifyingGlass'                                         State='~Actionable ignore'/>
                                <LC.Nav.Top.Item Style='~Style.With(label = ""Cart"", icon = Icon.ShoppingCart, badge = ~Count 3)' State='~Actionable ignore'/>
                            </rt-prop>
                        </LC.Nav.Top.Base>
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
                        // sometimes specific icon glyphs are not well positioned with respect to the baseline,
                        // either absolutely, or in terms of perception (e.g. a visually top-heavy icon). In these
                        // cases it's necessary to tweak their vertical positioning to make them appear to be
                        // vertically aligned with the rest of the icons.

                        ""adjust-icon"" ==> LibClient.Components.Nav.Top.ItemStyles.Theme.IconVerticalPositionAdjustment 10
                    "
                                                                            |> makeTextNode2 __parentFQN
                                                                        |]
                                                                )
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.With.Context"
                                                    LibClient.Components.Constructors.LC.With.Context(
                                                        context = (AppEggShellGallery.SampleVisualsScreenSize.sampleVisualsScreenSizeContext),
                                                        ``with`` =
                                                            (fun (sampleVisualsScreenSize: ScreenSize) ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        let __parentFQN = Some "LibClient.Components.ForceContext"
                                                                        LibClient.Components.Constructors.LC.ForceContext(
                                                                            value = (sampleVisualsScreenSize),
                                                                            context = (screenSizeContext),
                                                                            children =
                                                                                [|
                                                                                    let __parentFQN = Some "LibClient.Components.Nav.Top.Base"
                                                                                    LibClient.Components.Constructors.LC.Nav.Top.Base(
                                                                                        desktop =
                                                                                            (fun (_) ->
                                                                                                    (castAsElementAckingKeysWarning [|
                                                                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Item"
                                                                                                        LibClient.Components.Constructors.LC.Nav.Top.Item(
                                                                                                            state = (LibClient.Components.Nav.Top.Item.Actionable ignore),
                                                                                                            style = (LibClient.Components.Nav.Top.Item.Style.With(label = "Store", badge = LibClient.Components.Nav.Top.Item.Text "Summer Sale"))
                                                                                                        )
                                                                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Item"
                                                                                                        LibClient.Components.Constructors.LC.Nav.Top.Item(
                                                                                                            state = (LibClient.Components.Nav.Top.Item.Actionable ignore),
                                                                                                            style = (LibClient.Components.Nav.Top.Item.Style.With(icon  = Icon.Bell, badge = LibClient.Components.Nav.Top.Item.Count 2))
                                                                                                        )
                                                                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Item"
                                                                                                        LibClient.Components.Constructors.LC.Nav.Top.Item(
                                                                                                            state = (LibClient.Components.Nav.Top.Item.Disabled),
                                                                                                            style = (LibClient.Components.Nav.Top.Item.labelOnly "Blog")
                                                                                                        )
                                                                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Item"
                                                                                                        LibClient.Components.Constructors.LC.Nav.Top.Item(
                                                                                                            state = (LibClient.Components.Nav.Top.Item.Actionable ignore),
                                                                                                            style = (LibClient.Components.Nav.Top.Item.iconOnly  Icon.MagnifyingGlass)
                                                                                                        )
                                                                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Item"
                                                                                                        LibClient.Components.Constructors.LC.Nav.Top.Item(
                                                                                                            state = (LibClient.Components.Nav.Top.Item.Actionable ignore),
                                                                                                            style = (LibClient.Components.Nav.Top.Item.Style.With(label = "Cart", icon = Icon.ShoppingCart, badge = LibClient.Components.Nav.Top.Item.Count 3))
                                                                                                        )
                                                                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Item"
                                                                                                        let __currClass = "adjust-icon"
                                                                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                        LibClient.Components.Constructors.LC.Nav.Top.Item(
                                                                                                            state = (LibClient.Components.Nav.Top.Item.Actionable ignore),
                                                                                                            style = (LibClient.Components.Nav.Top.Item.iconOnly Icon.X),
                                                                                                            ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                                                                        )
                                                                                                    |])
                                                                                            ),
                                                                                        handheld =
                                                                                            (fun (_) ->
                                                                                                    (castAsElementAckingKeysWarning [|
                                                                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Item"
                                                                                                        LibClient.Components.Constructors.LC.Nav.Top.Item(
                                                                                                            state = (LibClient.Components.Nav.Top.Item.Actionable ignore),
                                                                                                            style = (LibClient.Components.Nav.Top.Item.Style.With(label = "Store", badge = LibClient.Components.Nav.Top.Item.Text "Summer Sale"))
                                                                                                        )
                                                                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Item"
                                                                                                        LibClient.Components.Constructors.LC.Nav.Top.Item(
                                                                                                            state = (LibClient.Components.Nav.Top.Item.Actionable ignore),
                                                                                                            style = (LibClient.Components.Nav.Top.Item.Style.With(icon  = Icon.Bell, badge = LibClient.Components.Nav.Top.Item.Count 2))
                                                                                                        )
                                                                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Item"
                                                                                                        LibClient.Components.Constructors.LC.Nav.Top.Item(
                                                                                                            state = (LibClient.Components.Nav.Top.Item.Disabled),
                                                                                                            style = (LibClient.Components.Nav.Top.Item.labelOnly "Blog")
                                                                                                        )
                                                                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Item"
                                                                                                        LibClient.Components.Constructors.LC.Nav.Top.Item(
                                                                                                            state = (LibClient.Components.Nav.Top.Item.Actionable ignore),
                                                                                                            style = (LibClient.Components.Nav.Top.Item.iconOnly  Icon.MagnifyingGlass)
                                                                                                        )
                                                                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Item"
                                                                                                        LibClient.Components.Constructors.LC.Nav.Top.Item(
                                                                                                            state = (LibClient.Components.Nav.Top.Item.Actionable ignore),
                                                                                                            style = (LibClient.Components.Nav.Top.Item.Style.With(label = "Cart", icon = Icon.ShoppingCart, badge = LibClient.Components.Nav.Top.Item.Count 3))
                                                                                                        )
                                                                                                    |])
                                                                                            )
                                                                                    )
                                                                                |]
                                                                        )
                                                                    |])
                                                            )
                                                    )
                                                |])
                                    )
                                |])
                    )
                |])
    )
