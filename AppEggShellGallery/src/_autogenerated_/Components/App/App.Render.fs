module AppEggShellGallery.Components.AppRender

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

open AppEggShellGallery.Components.App
open LibRouter.RoutesSpec


let render(children: array<ReactElement>, props: AppEggShellGallery.Components.App.Props, estate: AppEggShellGallery.Components.App.Estate, pstate: AppEggShellGallery.Components.App.Pstate, actions: AppEggShellGallery.Components.App.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "LibRouter.Components.With.Route"
    LibRouter.Components.Constructors.LR.With.Route(
        spec = (routesSpec()),
        ``with`` =
            (fun (maybeNavigationFrame) ->
                    (castAsElementAckingKeysWarning [|
                        let __parentFQN = Some "LibRouter.Components.LogRouteTransitions"
                        LibRouter.Components.Constructors.LR.LogRouteTransitions()
                        let __parentFQN = Some "AppEggShellGallery.Components.With.Navigation"
                        AppEggShellGallery.Components.Constructors.Ui.With.Navigation(
                            ``with`` =
                                (fun (nav) ->
                                        (castAsElementAckingKeysWarning [|
                                            registerGlobalMarkdownLinkHandler nav
                                        |])
                                )
                        )
                        (
                            let maybeRoute = maybeNavigationFrame |> Option.map NavigationFrame.route
                            let __parentFQN = Some "LibClient.Components.AppShell.Content"
                            LibClient.Components.Constructors.LC.AppShell.Content(
                                desktopSidebarStyle = (LibClient.Components.AppShell.Content.Fixed),
                                content =
                                        (castAsElementAckingKeysWarning [|
                                            match (maybeRoute) with
                                            | Some { SampleVisualsScreenSize = sampleVisualsScreenSize; ActualRoute = route } ->
                                                [|
                                                    match (route) with
                                                    | Home ->
                                                        [|
                                                            let __parentFQN = Some "AppEggShellGallery.Components.Route.Home"
                                                            AppEggShellGallery.Components.Constructors.Ui.Route.Home(
                                                                pstoreKey = (props.PstoreKey + "-Route-Home")
                                                            )
                                                        |]
                                                    | Docs       url ->
                                                        [|
                                                            let __parentFQN = Some "AppEggShellGallery.Components.Route.Docs"
                                                            AppEggShellGallery.Components.Constructors.Ui.Route.Docs(
                                                                markdownUrl = (url),
                                                                pstoreKey = (props.PstoreKey + "-Route-Docs")
                                                            )
                                                        |]
                                                    | Components content ->
                                                        [|
                                                            let __parentFQN = Some "AppEggShellGallery.Components.Route.Components"
                                                            AppEggShellGallery.Components.Constructors.Ui.Route.Components(
                                                                content = (content),
                                                                sampleVisualsScreenSize = (sampleVisualsScreenSize),
                                                                pstoreKey = (props.PstoreKey + "-Route-Components")
                                                            )
                                                        |]
                                                    | Tools      url ->
                                                        [|
                                                            let __parentFQN = Some "AppEggShellGallery.Components.Route.Tools"
                                                            AppEggShellGallery.Components.Constructors.Ui.Route.Tools(
                                                                markdownUrl = (url),
                                                                pstoreKey = (props.PstoreKey + "-Route-Tools")
                                                            )
                                                        |]
                                                    | HowTo      item ->
                                                        [|
                                                            let __parentFQN = Some "AppEggShellGallery.Components.Route.HowTo"
                                                            AppEggShellGallery.Components.Constructors.Ui.Route.HowTo(
                                                                item = (item),
                                                                pstoreKey = (props.PstoreKey + "-Route-HowTo")
                                                            )
                                                        |]
                                                    | Subject    url ->
                                                        [|
                                                            let __parentFQN = Some "AppEggShellGallery.Components.Route.Subject"
                                                            AppEggShellGallery.Components.Constructors.Ui.Route.Subject(
                                                                markdownUrl = (url),
                                                                pstoreKey = (props.PstoreKey + "-Route-Subject")
                                                            )
                                                        |]
                                                    | Design     item ->
                                                        [|
                                                            let __parentFQN = Some "AppEggShellGallery.Components.Route.Design"
                                                            AppEggShellGallery.Components.Constructors.Ui.Route.Design(
                                                                item = (item),
                                                                pstoreKey = (props.PstoreKey + "-Route-Design")
                                                            )
                                                        |]
                                                    | Legacy     item ->
                                                        [|
                                                            let __parentFQN = Some "AppEggShellGallery.Components.Route.Legacy"
                                                            AppEggShellGallery.Components.Constructors.Ui.Route.Legacy(
                                                                item = (item),
                                                                pstoreKey = (props.PstoreKey + "-Route-Legacy")
                                                            )
                                                        |]
                                                    | TinyGuid ->
                                                        [|
                                                            let __parentFQN = Some "AppEggShellGallery.Components.Route.TinyGuid"
                                                            AppEggShellGallery.Components.Constructors.Ui.Route.TinyGuid()
                                                        |]
                                                    |> castAsElementAckingKeysWarning
                                                |]
                                            | None ->
                                                [|
                                                    let __parentFQN = Some "LibClient.Components.InfoMessage"
                                                    LibClient.Components.Constructors.LC.InfoMessage(
                                                        message = ("Route Not Found"),
                                                        level = (LibClient.Components.InfoMessage.Attention)
                                                    )
                                                |]
                                            |> castAsElementAckingKeysWarning
                                        |]),
                                dialogs =
                                        (castAsElementAckingKeysWarning [|
                                            let __parentFQN = Some "AppEggShellGallery.Components.With.Navigation"
                                            AppEggShellGallery.Components.Constructors.Ui.With.Navigation(
                                                ``with`` =
                                                    (fun (nav) ->
                                                            (castAsElementAckingKeysWarning [|
                                                                let __parentFQN = Some "LibRouter.Components.Dialogs"
                                                                LibRouter.Components.Constructors.LR.Dialogs(
                                                                    nav = (nav),
                                                                    dialogsState = (navigationState.DialogsState),
                                                                    dialogs = (maybeNavigationFrame |> Option.map NavigationFrame.dialogs |> Option.getOrElse []),
                                                                    makeResultful =
                                                                        (fun (resultfulDialog, close) ->
                                                                                (castAsElementAckingKeysWarning [|
                                                                                    match (resultfulDialog) with
                                                                                    | ResultfulDialog.Sentinel ->
                                                                                        [|
                                                                                            noElement
                                                                                        |]
                                                                                    |> castAsElementAckingKeysWarning
                                                                                |])
                                                                        ),
                                                                    makeResultless =
                                                                        (fun (resultlessDialog, close) ->
                                                                                (castAsElementAckingKeysWarning [|
                                                                                    match (resultlessDialog) with
                                                                                    | ResultlessDialog.Sentinel ->
                                                                                        [|
                                                                                            noElement
                                                                                        |]
                                                                                    |> castAsElementAckingKeysWarning
                                                                                |])
                                                                        )
                                                                )
                                                            |])
                                                    )
                                            )
                                        |]),
                                onError =
                                    (fun (error, retry) ->
                                            (castAsElementAckingKeysWarning [|
                                                let __parentFQN = Some "LibClient.Components.AppShell.TopLevelErrorMessage"
                                                LibClient.Components.Constructors.LC.AppShell.TopLevelErrorMessage(
                                                    retry = (retry),
                                                    error = (error)
                                                )
                                            |])
                                    ),
                                sidebar =
                                        (castAsElementAckingKeysWarning [|
                                            let __parentFQN = Some "AppEggShellGallery.Components.Sidebar"
                                            AppEggShellGallery.Components.Constructors.Ui.Sidebar(
                                                maybeRoute = (maybeRoute)
                                            )
                                        |]),
                                topNav =
                                        (castAsElementAckingKeysWarning [|
                                            let __parentFQN = Some "AppEggShellGallery.Components.TopNav"
                                            AppEggShellGallery.Components.Constructors.Ui.TopNav(
                                                maybeRoute = (maybeRoute)
                                            )
                                        |])
                            )
                        )
                    |])
            )
    )
