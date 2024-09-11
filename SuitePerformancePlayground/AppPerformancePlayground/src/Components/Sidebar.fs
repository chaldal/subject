[<AutoOpen>]
module AppPerformancePlayground.Components.Sidebar

open Fable.React
open LibClient
open LibClient.Components
open AppPerformancePlayground
open AppPerformancePlayground.Components
open AppPerformancePlayground.Navigation
open Sidebar.Item
open ReactXP.Styles
open ReactXP.Components

type Ui with
    [<Component>]
    static member Sidebar(_maybeRoute: Route option) =
        Ui.With.Session.Authenticated (fun authenticated -> [|
            LC.Sidebar.WithClose (fun close ->
                let closeAndGo (route: Route) e =
                    nav.Go route e
                    close e

                let logOut e =
                    AppPerformancePlayground.Actions.logOut ()
                    |> Async.Map (Result.mapError (fun e -> nav.Go (Alert (None, e))))
                    |> Async.Ignore
                    |> startSafely
                    close e

                LC.Sidebar.Base(
                    scrollableMiddle = asFragment (elements {
                        LC.Sidebar.Heading ("Various routes")
                        LC.Sidebar.Item(label = i18n.t.Bananas, state = Actionable(closeAndGo Bananas))
                        LC.Sidebar.Item(label = i18n.t.Mangoes, state = Actionable(closeAndGo Mangoes))
                    }),
                    fixedBottom = element {
                        RX.View (styles = [|Styles.LoggedInAs|], children = [|
                            LC.Text $"Logged in as {authenticated.Who.Name.Value}"
                        |])
                        LC.Sidebar.Item(label = "Logout", state = Actionable logOut)
                    }
                )
            )
        |])

and private Styles() =
    static member val LoggedInAs = makeViewStyles {
        margin 10
    }