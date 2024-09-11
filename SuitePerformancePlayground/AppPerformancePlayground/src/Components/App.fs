[<AutoOpen>]
module AppPerformancePlayground.Components.App

open Fable.React
open LibRouter.RoutesSpec
open LibClient
open ReactXP.Styles
open ReactXP.Components
open LibClient.Components
open LibRouter.Components
open AppPerformancePlayground.Navigation
open AppPerformancePlayground.Components

type Ui.App with
    [<Component>]
    static member Root () : ReactElement =
        let isVisible = Hooks.useState false

        RX.View [|
            LC.Button (
                label = "Toggle",
                state = ButtonHighLevelState.LowLevel (ButtonLowLevelState.Actionable (fun _ -> isVisible.update (fun curr -> not curr)))
            )
            if isVisible.current then
                [1 .. 100]
                |> Seq.map (fun i ->
                    RX.View (
                        key = $"{i}",
                        styles = [|Styles.Card|],
                        children = [|
                            LC.Text (
                                styles = [|Styles.Text|],
                                value = "6 CDEF"
                            )
                        |]
                    )
                )
                |> Array.ofSeq |> castAsElementAckingKeysWarning
        |]

    [<Component>]
    static member Authenticated (_pstoreKey: string) : ReactElement =
        let maybeNavigationFrame = LR.UseRoute (routesSpec())
        let maybeRoute = maybeNavigationFrame |> Option.map NavigationFrame.route

        LC.AppShell.Content (
            desktopSidebarStyle = AppShell.Content.DesktopSidebarStyle.Fixed,
            sidebar             = Ui.Sidebar maybeRoute,
            topNav              = Ui.Nav.Top maybeRoute,
            onError             = LC.AppShell.TopLevelErrorMessage,
            dialogs = (
                LR.Dialogs (
                    nav          = nav,
                    dialogsState = navigationState.DialogsState,
                    dialogs =
                        (maybeNavigationFrame
                         |> Option.map NavigationFrame.dialogs
                         |> Option.getOrElse []),
                    makeResultless = (fun (resultlessDialog, close) ->
                        match resultlessDialog with
                        | DoSomething param -> AppPerformancePlayground.Components.Dialog.DoSomething.Open param close
                    ),
                    makeResultful = (fun (resultfulDialog, close) ->
                        match resultfulDialog with
                        | Sentinel -> noElement
                    )
                )
            ),
            content = (
                 match maybeRoute with
                 | None -> LC.InfoMessage(level = InfoMessage.Level.Attention, message = "Route Not Found")
                 | Some route ->
                     match route with
                     | Landing -> Ui.Route.Landing ()
                     | Bananas -> Ui.Route.Bananas ()
                     | Mangoes -> Ui.Route.Mangoes ()
            )
        )

and private Styles() =
    static member val Card = makeViewStyles {
        margin          8
        padding         8
        backgroundColor Color.White
        borderRadius 2
        elevation    10
        shadow       (Color.BlackAlpha 0.3) 4 (0, 1)
        size 200 30
    }

    static member val Text = makeTextStyles {
        fontSize 30
        color Color.DevPink
        FontWeight.Bold
    }
