module AppEggShellGallery.Components.Content.TabsRender

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

open AppEggShellGallery.Components.Content.Tabs



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.Tabs.Props, estate: AppEggShellGallery.Components.Content.Tabs.Estate, pstate: AppEggShellGallery.Components.Content.Tabs.Pstate, actions: AppEggShellGallery.Components.Content.Tabs.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
    AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
        displayName = ("Tabs"),
        props =
            (
                AppEggShellGallery.Components.ComponentContent.Manual
                    (
                            (castAsElementAckingKeysWarning [|
                                let __parentFQN = Some "AppEggShellGallery.Components.ScrapedComponentProps"
                                AppEggShellGallery.Components.Constructors.Ui.ScrapedComponentProps(
                                    fullyQualifiedName = ("LibClient.Components.Tabs"),
                                    heading = ("Tabs")
                                )
                                let __parentFQN = Some "AppEggShellGallery.Components.ScrapedComponentProps"
                                AppEggShellGallery.Components.Constructors.Ui.ScrapedComponentProps(
                                    fullyQualifiedName = ("LibClient.Components.Tab"),
                                    heading = ("Tab")
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
                                                    language = (AppEggShellGallery.Components.Code.Fsharp),
                                                    children =
                                                        [|
                                                            @"
                    type TabItem =
                    | Home
                    | Profile
                    | Contact

                    type Estate = {
                        SelectedTab: TabItem
                    }

                    type Tabs(_initialProps) =
                        inherit EstatefulComponent<Props, Estate, Actions, Tabs>(""AppEggShellGallery.Components.Content.Tabs"", _initialProps, Actions, hasStyles = true)

                        // Initialize Estate with default selected tab
                        override _.GetInitialEstate(_initialProps: Props) : Estate = {
                            SelectedTab = Home
                        }

                    and Actions(this: Tabs) =

                        // Update selected tab in estate when a new tab is selected
                        member _.SelectTab (tab: TabItem) (_e: ReactEvent.Pointer) : unit =
                            this.SetEstate (fun estate _ -> { estate with SelectedTab = tab })
                "
                                                            |> makeTextNode2 __parentFQN
                                                        |]
                                                )
                                                let __parentFQN = Some "AppEggShellGallery.Components.Code"
                                                AppEggShellGallery.Components.Constructors.Ui.Code(
                                                    language = (AppEggShellGallery.Components.Code.Render),
                                                    children =
                                                        [|
                                                            @"
                    <LC.Tabs
                    class='tabs'
                    rt-let='tabState := fun tab -> if estate.SelectedTab = tab then ~Selected else (~Unselected (actions.SelectTab tab))'>
                        <LC.Tab rt-fs='true' Label='""Home""'    State='tabState Home   '/>
                        <LC.Tab rt-fs='true' Label='""Profile""' State='tabState Profile'/>
                        <LC.Tab rt-fs='true' Label='""Contact""' State='tabState Contact'/>
                    </LC.Tabs>
                    <rt-match what='estate.SelectedTab'>
                        <rt-case is='TabItem.Home   '><div>This is the HOME tab</div></rt-case>
                        <rt-case is='TabItem.Profile'><div>This is the PROFILE tab</div></rt-case>
                        <rt-case is='TabItem.Contact'><div>This is the CONTACT tab</div></rt-case>
                    </rt-match>
                "
                                                            |> makeTextNode2 __parentFQN
                                                        |]
                                                )
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    (
                                        let tabState = fun tab -> if estate.SelectedTab = tab then LibClient.Components.Tabs.Selected else (LibClient.Components.Tabs.Unselected (actions.SelectTab tab))
                                        let __parentFQN = Some "LibClient.Components.Tabs"
                                        let __currClass = "tabs"
                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                        LibClient.Components.Constructors.LC.Tabs(
                                            ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                            children =
                                                [|
                                                    let __parentFQN = Some "LibClient.Components.Tab"
                                                    LibClient.Components.Constructors.LC.Tab(
                                                        state = (tabState Home   ),
                                                        label = ("Home")
                                                    )
                                                    let __parentFQN = Some "LibClient.Components.Tab"
                                                    LibClient.Components.Constructors.LC.Tab(
                                                        state = (tabState Profile),
                                                        label = ("Profile")
                                                    )
                                                    let __parentFQN = Some "LibClient.Components.Tab"
                                                    LibClient.Components.Constructors.LC.Tab(
                                                        state = (tabState Contact),
                                                        label = ("Contact")
                                                    )
                                                |]
                                        )
                                    )
                                    match (estate.SelectedTab) with
                                    | TabItem.Home    ->
                                        [|
                                            let __parentFQN = Some "ReactXP.Components.View"
                                            ReactXP.Components.Constructors.RX.View(
                                                children =
                                                    [|
                                                        makeTextNode2 __parentFQN "This is the HOME tab"
                                                    |]
                                            )
                                        |]
                                    | TabItem.Profile ->
                                        [|
                                            let __parentFQN = Some "ReactXP.Components.View"
                                            ReactXP.Components.Constructors.RX.View(
                                                children =
                                                    [|
                                                        makeTextNode2 __parentFQN "This is the PROFILE tab"
                                                    |]
                                            )
                                        |]
                                    | TabItem.Contact ->
                                        [|
                                            let __parentFQN = Some "ReactXP.Components.View"
                                            ReactXP.Components.Constructors.RX.View(
                                                children =
                                                    [|
                                                        makeTextNode2 __parentFQN "This is the CONTACT tab"
                                                    |]
                                            )
                                        |]
                                    |> castAsElementAckingKeysWarning
                                |])
                    )
                |])
    )
