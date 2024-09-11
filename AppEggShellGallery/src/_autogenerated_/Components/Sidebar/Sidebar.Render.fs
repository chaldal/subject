module AppEggShellGallery.Components.SidebarRender

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

open AppEggShellGallery.Components.Sidebar



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Sidebar.Props, estate: AppEggShellGallery.Components.Sidebar.Estate, pstate: AppEggShellGallery.Components.Sidebar.Pstate, actions: AppEggShellGallery.Components.Sidebar.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "LibRouter.Components.With.CurrentRoute"
    LibRouter.Components.Constructors.LR.With.CurrentRoute(
        spec = (routesSpec()),
        fn =
            (fun (maybeCurrentRoute) ->
                    (castAsElementAckingKeysWarning [|
                        match (maybeCurrentRoute) with
                        | Some { SampleVisualsScreenSize = _; ActualRoute = currentRoute } ->
                            [|
                                let __parentFQN = Some "LibClient.Components.Sidebar.WithClose"
                                LibClient.Components.Constructors.LC.Sidebar.WithClose(
                                    ``with`` =
                                        (fun (close) ->
                                                (castAsElementAckingKeysWarning [|
                                                    let maybeFixedTop = (
                                                            (castAsElementAckingKeysWarning [|
                                                                (
                                                                    let show = fun (route: ActualRoute) e -> nav.Go (maybeCurrentRoute, route) e; close e
                                                                    let itemState = (
                                                                        fun route -> if route = currentRoute then LibClient.Components.Sidebar.Item.Selected else LibClient.Components.Sidebar.Item.Actionable (show route);

                                                                    )
                                                                    let __parentFQN = Some "LibClient.Components.Responsive"
                                                                    LibClient.Components.Constructors.LC.Responsive(
                                                                        desktop =
                                                                            (fun (_screenSize) ->
                                                                                    (castAsElementAckingKeysWarning [|
                                                                                        noElement
                                                                                    |])
                                                                            ),
                                                                        handheld =
                                                                            (fun (_screenSize) ->
                                                                                    (castAsElementAckingKeysWarning [|
                                                                                        let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                        LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                            state = (itemState (Docs "index.md")                               ),
                                                                                            label = ("Docs")
                                                                                        )
                                                                                        let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                        LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                            state = (itemState (Tools "tools/index.md")                        ),
                                                                                            label = ("Tools")
                                                                                        )
                                                                                        let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                        LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                            state = (itemState (Components Index)                              ),
                                                                                            label = ("Components")
                                                                                        )
                                                                                        let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                        LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                            state = (itemState (HowTo (HowToItem.Markdown "how-to/index.md"))  ),
                                                                                            label = ("How To")
                                                                                        )
                                                                                        let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                        LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                            state = (itemState (Design (DesignItem.Markdown "design/index.md"))),
                                                                                            label = ("Design")
                                                                                        )
                                                                                    |])
                                                                            )
                                                                    )
                                                                )
                                                            |])
                                                    )
                                                    match (currentRoute) with
                                                    | Home | TinyGuid ->
                                                        [|
                                                            let __parentFQN = Some "LibClient.Components.Responsive"
                                                            LibClient.Components.Constructors.LC.Responsive(
                                                                desktop =
                                                                    (fun (_screenSize) ->
                                                                            (castAsElementAckingKeysWarning [|
                                                                                noElement
                                                                            |])
                                                                    ),
                                                                handheld =
                                                                    (fun (_screenSize) ->
                                                                            (castAsElementAckingKeysWarning [|
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Base"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Base(
                                                                                    fixedTop =
                                                                                            (castAsElementAckingKeysWarning [|
                                                                                                maybeFixedTop
                                                                                            |])
                                                                                )
                                                                            |])
                                                                    )
                                                            )
                                                        |]
                                                    | Docs url ->
                                                        [|
                                                            (
                                                                let show = fun url e -> nav.Go (maybeCurrentRoute, Docs url) e; close e
                                                                let itemState = (
                                                                    fun itemUrl -> if url = itemUrl then LibClient.Components.Sidebar.Item.Selected else LibClient.Components.Sidebar.Item.Actionable (show itemUrl);

                                                                )
                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Base"
                                                                LibClient.Components.Constructors.LC.Sidebar.Base(
                                                                    fixedTop =
                                                                            (castAsElementAckingKeysWarning [|
                                                                                maybeFixedTop
                                                                            |]),
                                                                    scrollableMiddle =
                                                                            (castAsElementAckingKeysWarning [|
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState "index.md"),
                                                                                    label = ("EggShell Introduction")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Divider"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Divider()
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState "basics/getting-started.md"),
                                                                                    label = ("Getting Started")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState "basics/dev-experience.md"),
                                                                                    label = ("Dev Experience")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState "fsharp/component.md"),
                                                                                    label = ("Components")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState "fsharp/styling.md"),
                                                                                    label = ("Styling")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState "fsharp/themeing.md"),
                                                                                    label = ("Themeing")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState "fsharp/legacy.md"),
                                                                                    label = ("Legacy Interop")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState "basics/libraries.md"),
                                                                                    label = ("Libraries")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Divider"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Divider()
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Heading"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Heading(
                                                                                    text = ("Native")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState "native/getting-started.md"),
                                                                                    label = ("Getting Started")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState "native/dev-experience.md"),
                                                                                    label = ("Dev Experience")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState "native/link-native-library.md"),
                                                                                    label = ("Link Native Libray")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState "native/release-app.md"),
                                                                                    label = ("Release Native App")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Divider"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Divider()
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Heading"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Heading(
                                                                                    text = ("Housekeeping")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState "basics/changelog.md"),
                                                                                    label = ("Changelog")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState "basics/roadmap.md"),
                                                                                    label = ("Roadmap")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Divider"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Divider()
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Heading"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Heading(
                                                                                    text = ("Unsorted")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState "unsorted/background.md"),
                                                                                    label = ("Background")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState "unsorted/icons.md"),
                                                                                    label = ("Icons infra")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState "unsorted/component-design.md"),
                                                                                    label = ("Component types")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState "unsorted/eggshell-specific-fsharp-good-practices.md"),
                                                                                    label = ("EggShell-specific F# Good Coding Practices")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState "unsorted/directory-structure.md"),
                                                                                    label = ("Directory structure")
                                                                                )
                                                                            |])
                                                                )
                                                            )
                                                        |]
                                                    | Tools url ->
                                                        [|
                                                            (
                                                                let show = fun url e -> nav.Go (maybeCurrentRoute, Tools url) e; close e
                                                                let itemState = (
                                                                    fun itemUrl -> if url = itemUrl then LibClient.Components.Sidebar.Item.Selected else LibClient.Components.Sidebar.Item.Actionable (show itemUrl);

                                                                )
                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Base"
                                                                LibClient.Components.Constructors.LC.Sidebar.Base(
                                                                    fixedTop =
                                                                            (castAsElementAckingKeysWarning [|
                                                                                maybeFixedTop
                                                                            |]),
                                                                    scrollableMiddle =
                                                                            (castAsElementAckingKeysWarning [|
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState "tools/index.md"),
                                                                                    label = ("Tools Introduction")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Divider"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Divider()
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState "tools/cli.md"),
                                                                                    label = ("eggshell CLI")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState "tools/snippets.md"),
                                                                                    label = ("Snippets")
                                                                                )
                                                                            |])
                                                                )
                                                            )
                                                        |]
                                                    | HowTo currItem ->
                                                        [|
                                                            (
                                                                let show = fun item e -> nav.Go (maybeCurrentRoute, HowTo item) e; close e
                                                                let itemState = fun item -> if item = currItem then LibClient.Components.Sidebar.Item.Selected else LibClient.Components.Sidebar.Item.Actionable (show item)
                                                                let itemStateMarkdown = (
                                                                    fun url -> itemState (HowToItem.Markdown url);

                                                                )
                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Base"
                                                                LibClient.Components.Constructors.LC.Sidebar.Base(
                                                                    fixedTop =
                                                                            (castAsElementAckingKeysWarning [|
                                                                                maybeFixedTop
                                                                            |]),
                                                                    scrollableMiddle =
                                                                            (castAsElementAckingKeysWarning [|
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemStateMarkdown "how-to/index.md"),
                                                                                    label = ("How To Introduction")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Divider"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Divider()
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemStateMarkdown "how-to/faq.md"),
                                                                                    label = ("FAQ")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemStateMarkdown "how-to/projects.md"),
                                                                                    label = ("Where to find examples")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemStateMarkdown "how-to/tap-capture.md"),
                                                                                    label = ("Taps, Clicks, Hovers, etc")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemStateMarkdown "how-to/executors.md"),
                                                                                    label = ("Executors")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemStateMarkdown "how-to/responsive.md"),
                                                                                    label = ("Responsive Components")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemStateMarkdown "how-to/scrolling.md"),
                                                                                    label = ("Scrolling in ReactXP")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemStateMarkdown "how-to/refs.md"),
                                                                                    label = ("React Refs")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemStateMarkdown "how-to/spinners.md"),
                                                                                    label = ("Dealing with Spinners")
                                                                                )
                                                                            |])
                                                                )
                                                            )
                                                        |]
                                                    | Subject url ->
                                                        [|
                                                            (
                                                                let show = fun url e -> nav.Go (maybeCurrentRoute, Subject url) e; close e
                                                                let itemState = (
                                                                    fun itemUrl -> if url = itemUrl then LibClient.Components.Sidebar.Item.Selected else LibClient.Components.Sidebar.Item.Actionable (show itemUrl);

                                                                )
                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Base"
                                                                LibClient.Components.Constructors.LC.Sidebar.Base(
                                                                    fixedTop =
                                                                            (castAsElementAckingKeysWarning [|
                                                                                maybeFixedTop
                                                                            |]),
                                                                    scrollableMiddle =
                                                                            (castAsElementAckingKeysWarning [|
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState "subject/index.md"),
                                                                                    label = ("Introduction")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Divider"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Divider()
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState "subject/actions-and-transitions.md"),
                                                                                    label = ("Actions and transitions")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState "subject/events-and-subscriptions.md"),
                                                                                    label = ("Events and subscriptions")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState "subject/construction-and-id-generation.md"),
                                                                                    label = ("Construction and id generation")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState "subject/indexing-and-querying.md"),
                                                                                    label = ("Indexing and querying")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Divider"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Divider()
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState "subject/testing.md"),
                                                                                    label = ("Testing")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState "subject/dev-host-simulator.md"),
                                                                                    label = ("Dev Host Simulations")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Divider"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Divider()
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState "subject/views.md"),
                                                                                    label = ("Views")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState "subject/access-control.md"),
                                                                                    label = ("Access control")
                                                                                )
                                                                            |])
                                                                )
                                                            )
                                                        |]
                                                    | Design currItem ->
                                                        [|
                                                            (
                                                                let show = fun item e -> nav.Go (maybeCurrentRoute, Design item) e; close e
                                                                let itemState = (
                                                                    fun item -> if currItem = item then LibClient.Components.Sidebar.Item.Selected else LibClient.Components.Sidebar.Item.Actionable (show item);

                                                                )
                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Base"
                                                                LibClient.Components.Constructors.LC.Sidebar.Base(
                                                                    fixedTop =
                                                                            (castAsElementAckingKeysWarning [|
                                                                                maybeFixedTop
                                                                            |]),
                                                                    scrollableMiddle =
                                                                            (castAsElementAckingKeysWarning [|
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState (DesignItem.Markdown "design/index.md")),
                                                                                    label = ("Design Introduction")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Divider"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Divider()
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState DesignItem.ColorVariants),
                                                                                    label = ("Colors")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState DesignItem.Icons),
                                                                                    label = ("Icons")
                                                                                )
                                                                            |])
                                                                )
                                                            )
                                                        |]
                                                    | Legacy currItem ->
                                                        [|
                                                            (
                                                                let show = fun item e -> nav.Go (maybeCurrentRoute, Legacy item) e; close e
                                                                let itemState = (
                                                                    fun item -> if currItem = item then LibClient.Components.Sidebar.Item.Selected else LibClient.Components.Sidebar.Item.Actionable (show item);

                                                                )
                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Base"
                                                                LibClient.Components.Constructors.LC.Sidebar.Base(
                                                                    fixedTop =
                                                                            (castAsElementAckingKeysWarning [|
                                                                                maybeFixedTop
                                                                            |]),
                                                                    scrollableMiddle =
                                                                            (castAsElementAckingKeysWarning [|
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState (LegacyItem.Markdown "design/index.md")),
                                                                                    label = ("Legacy Introduction")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Divider"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Divider()
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Heading"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Heading(
                                                                                    text = ("Render DSL")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState (LegacyItem.Markdown "renderDsl/index.md")),
                                                                                    label = ("Language Description")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState (LegacyItem.Markdown "renderDsl/style-guide.md")),
                                                                                    label = ("Style Guide")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState (LegacyItem.Markdown "fsharp/background.md")),
                                                                                    label = ("Sunsetting")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Divider"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Divider()
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Heading"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Heading(
                                                                                    text = ("Styles DSL")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState (LegacyItem.Markdown "stylesDsl/index.md")),
                                                                                    label = ("Language Description")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState (LegacyItem.Markdown "stylesDsl/style-guide.md")),
                                                                                    label = ("Style Guide")
                                                                                )
                                                                            |])
                                                                )
                                                            )
                                                        |]
                                                    | Components content ->
                                                        [|
                                                            (
                                                                let show = fun content e -> nav.Go (maybeCurrentRoute, Components content) e; close e
                                                                let itemState = (
                                                                    fun itemContent -> if content = itemContent then LibClient.Components.Sidebar.Item.Selected else LibClient.Components.Sidebar.Item.Actionable (show itemContent);

                                                                )
                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Base"
                                                                LibClient.Components.Constructors.LC.Sidebar.Base(
                                                                    fixedTop =
                                                                            (castAsElementAckingKeysWarning [|
                                                                                maybeFixedTop
                                                                            |]),
                                                                    scrollableMiddle =
                                                                            (castAsElementAckingKeysWarning [|
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Index),
                                                                                    label = ("Components Introduction")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Divider"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Divider()
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Heading"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Heading(
                                                                                    text = ("Layout")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Layout_Row),
                                                                                    label = ("Row")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Layout_Column),
                                                                                    label = ("Column")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Layout_Sized),
                                                                                    label = ("Sized")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Layout_Constrained),
                                                                                    label = ("Constrained")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Divider"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Divider()
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Heading"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Heading(
                                                                                    text = ("Buttons")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState ComponentItem.Buttons),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Buttons")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState ComponentItem.Button),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Button")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState IconButton),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("IconButton")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState FloatingActionButton),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("FloatingActionButton")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState TextButton),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("TextButton")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState ToggleButtons),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("ToggleButtons")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Divider"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Divider()
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Heading"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Heading(
                                                                                    text = ("Input")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Forms),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Forms")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Input_Checkbox),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Input.Checkbox")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Input_ChoiceList),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Input.ChoiceList")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Input_Date),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Input.Date")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Input_DayOfTheWeek),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Input.DayOfTheWeek")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Input_Decimal),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Input.Decimal")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Input_Duration),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Input.Duration")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Input_EmailAddress),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Input.EmailAddress")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Input_LocalTime),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Input.LocalTime")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Input_File),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Input.File")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Input_Image),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Input.Image")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Input_Picker),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Input.Picker")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Input_PhoneNumber),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Input.PhoneNumber")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Input_PositiveInteger),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Input.PositiveInteger")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Input_PositiveDecimal),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Input.PositiveDecimal")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Input_Quantity),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Input.Quantity")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Input_Text),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Input.Text")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Input_UnsignedInteger),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Input.UnsignedInteger")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Input_UnsignedDecimal),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Input.UnsignedDecimal")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Divider"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Divider()
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Heading"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Heading(
                                                                                    text = ("Content Blocks")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Card),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Card")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Carousel),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Carousel")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Dialogs),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Dialogs")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Draggable),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Draggable")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState ImageCard),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("ImageCard")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState InfoMessage),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("InfoMessage")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState ItemList),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("ItemList")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Section_Padded),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Section.Padded")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Tabs),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Tabs")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Divider"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Divider()
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Heading"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Heading(
                                                                                    text = ("Animation")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState AnimatableImage),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("AnimatableImage")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState AnimatableText),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("AnimatableText")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState AnimatableTextInput),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("AnimatableTextInput")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState AnimatableView),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("AnimatableView")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Divider"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Divider()
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Heading"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Heading(
                                                                                    text = ("Admin Panels")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Grid),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Grid")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState QueryGrid),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("QueryGrid")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (LibClient.Components.Sidebar.Item.Disabled),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("WithSortAndFilter")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Divider"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Divider()
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Heading"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Heading(
                                                                                    text = ("Text & Formatting")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState ComponentItem.Heading),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Heading")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Pre),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Pre")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Tag),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Tag")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState TimeSpan),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("TimeSpan")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Timestamp),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Timestamp")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Divider"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Divider()
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Heading"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Heading(
                                                                                    text = ("Graphic")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Avatar),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Avatar")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState ComponentItem.Icon),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Icon")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState IconWithBadge),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("IconWithBadge")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Thumb),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Thumb")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Thumbs),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Thumbs")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Scrim),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Scrim")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Stars),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Stars")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Divider"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Divider()
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Heading"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Heading(
                                                                                    text = ("Navigation")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState ComponentItem.ContextMenu),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Context Menu")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState ComponentItem.Sidebar),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Sidebar")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Nav_Top),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Nav.Top")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Nav_Bottom),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Nav.Bottom")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Divider"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Divider()
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Heading"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Heading(
                                                                                    text = ("Higher Order Components")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState ErrorBoundary),
                                                                                    label = ("ErrorBoundary")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Executor_AlertErrors),
                                                                                    label = ("AlertErrors")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState AsyncData),
                                                                                    label = ("AsyncData")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState WithContext),
                                                                                    label = ("WithDataFlowControl")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState TriStateful),
                                                                                    label = ("TriStateful")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState QuadStateful),
                                                                                    label = ("QuadStateful")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState Responsive),
                                                                                    label = ("Responsive")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState InProgress),
                                                                                    label = ("InProgress")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState WithExecutor),
                                                                                    label = ("With.Executor")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Divider"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Divider()
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Heading"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Heading(
                                                                                    text = ("With")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState WithContext),
                                                                                    label = ("WithContext")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState WithDataFlowControl),
                                                                                    label = ("DataFlowControl")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Divider"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Divider()
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Heading"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Heading(
                                                                                    text = ("Third Party")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState ThirdParty_Map),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Map")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState ThirdParty_Recharts),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Recharts")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Divider"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Divider()
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Heading"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Heading(
                                                                                    text = ("Unsorted")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState DateSelector),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("DateSelector")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (itemState TouchableOpacity),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("TouchableOpacity")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (LibClient.Components.Sidebar.Item.Disabled),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Dialog.Confirm")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (LibClient.Components.Sidebar.Item.Disabled),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Dialog.Shell.WhiteRounded.Base")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (LibClient.Components.Sidebar.Item.Disabled),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Dialog.Shell.WhiteRounded.Standard")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (LibClient.Components.Sidebar.Item.Disabled),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Dialog.Shell.FullScren")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (LibClient.Components.Sidebar.Item.Disabled),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Dialog.Shell.FromBottom")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (LibClient.Components.Sidebar.Item.Disabled),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("DueDateTag")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (LibClient.Components.Sidebar.Item.Disabled),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("FormFieldsDivider")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (LibClient.Components.Sidebar.Item.Disabled),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("HandheldListItem")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (LibClient.Components.Sidebar.Item.Disabled),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("HeaderCell")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (LibClient.Components.Sidebar.Item.Disabled),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("LabelledFormField")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (LibClient.Components.Sidebar.Item.Disabled),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("LabelledValue")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (LibClient.Components.Sidebar.Item.Disabled),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Popup")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (LibClient.Components.Sidebar.Item.Disabled),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("Route")
                                                                                )
                                                                                let __parentFQN = Some "LibClient.Components.Sidebar.Item"
                                                                                LibClient.Components.Constructors.LC.Sidebar.Item(
                                                                                    state = (LibClient.Components.Sidebar.Item.Disabled),
                                                                                    right = (LibClient.Components.Sidebar.Item.Right.Icon Icon.EggShell),
                                                                                    label = ("TwoWayScrollable")
                                                                                )
                                                                            |])
                                                                )
                                                            )
                                                        |]
                                                    |> castAsElementAckingKeysWarning
                                                |])
                                        )
                                )
                            |]
                        | None ->
                            [|
                                makeTextNode2 __parentFQN "no sidebar"
                            |]
                        |> castAsElementAckingKeysWarning
                    |])
            )
    )
