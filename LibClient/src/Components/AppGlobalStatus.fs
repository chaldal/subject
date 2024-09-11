[<AutoOpen>]
module LibClient.Components.AppGlobalStatus

open System
open Fable.React
open LibClient
open LibClient.Components
open LibClient.NetworkState
open ReactXP.Components
open ReactXP.Styles

module LC =
    module AppGlobalStatus =
        [<RequireQualifiedAccess>]
        type ConnectionStatus =
        | NetworkUnavailable
        | NetworkAvailableButBackendUnreachable of MaybeRetryingAt: Option<DateTimeOffset>
        | NetworkAvailableAndBackendReachable

        type Theme = {
            BackgroundColor: Color
        }

open LC.AppGlobalStatus

[<RequireQualifiedAccess>]
module private Styles =
    let view =
        ViewStyles.Memoize(
            fun (theme: Theme) ->
                makeViewStyles {
                    FlexDirection.Column
                    JustifyContent.Center
                    Overflow.VisibleForDropShadow
                    AlignItems.Center
                    padding 8
                    backgroundColor theme.BackgroundColor
                }
        )

    let bubble =
        makeViewStyles {
            backgroundColor Color.White
            paddingHV 12 6
            borderRadius 20
            shadow (Color.BlackAlpha 0.3) 8 (0, 2)
        }

    let message =
        makeTextStyles {
            fontSize 12
        }

module private Helpers =
    [<RequireQualifiedAccess>]
    type LastKnownBackendState =
    | Available
    | Unavailable of MaybeRetryingAt: Option<DateTimeOffset>

    let connectionStatus (isConnected: bool) (lastKnownBackendState: LastKnownBackendState) =
        match isConnected, lastKnownBackendState with
        | false, _ -> ConnectionStatus.NetworkUnavailable
        | true, LastKnownBackendState.Unavailable maybeRetryingAt -> ConnectionStatus.NetworkAvailableButBackendUnreachable maybeRetryingAt
        | true, LastKnownBackendState.Available -> ConnectionStatus.NetworkAvailableAndBackendReachable

    let retryingAtDeltaDisplay (now: DateTimeOffset) (retryingAt: DateTimeOffset): string =
        let delta = max TimeSpan.Zero (retryingAt - now)
        let seconds = int delta.TotalSeconds

        if seconds = 0 then
            "now"
        else if seconds = 1 then
            "in 1 second"
        else
            $"in %i{seconds} seconds"

type LibClient.Components.Constructors.LC with
    [<Component>]
    static member AppGlobalStatus(
                ?bottom: ConnectionStatus -> ReactElement,
                ?styles: array<ViewStyles>,
                ?theme: Theme -> Theme,
                ?key: string
            ) : ReactElement =
        key |> ignore

        let isConnectedHook = Hooks.useState true
        let lastKnownBackendStateHook = Hooks.useState Helpers.LastKnownBackendState.Available

        Hooks.useEffectDisposable(
            (fun () ->
                async {
                    let! isConnected = ReactXP.NetInfo.isConnected ()
                    isConnectedHook.update isConnected
                    ReactXP.NetInfo.onIsConnectedChange isConnectedHook.update
                } |> startSafely

                let onResult =
                    LibClient.ServiceInstances.services().EventBus.On
                        networkStateEventQueue
                        (fun event ->
                            match event with
                            | NetworkStateEvent.NonBackendConnectivitySucceeded
                            | NetworkStateEvent.NonBackendConnectivityFailed ->
                                // We don't display any messaging specific to non-backend connectivity, so no need to track any state here.
                                ()
                            | NetworkStateEvent.BackendConnectivitySucceeded ->
                                lastKnownBackendStateHook.update Helpers.LastKnownBackendState.Available
                            | NetworkStateEvent.BackendConnectivityFailed maybeRetryingAt ->
                                lastKnownBackendStateHook.update (Helpers.LastKnownBackendState.Unavailable maybeRetryingAt)
                        )
                { new IDisposable with
                    member _.Dispose() =
                        onResult.Off()
                }
            ),
            [||]
        )

        let bottom = defaultArg bottom (fun _ -> noElement)
        let theTheme = Themes.GetMaybeUpdatedWith theme

        let connectionStatus =
            Helpers.connectionStatus isConnectedHook.current lastKnownBackendStateHook.current

        match connectionStatus with
        | ConnectionStatus.NetworkUnavailable
        | ConnectionStatus.NetworkAvailableButBackendUnreachable _ ->
            RX.View(
                styles =
                    [|
                        Styles.view theTheme
                        yield! styles |> Option.defaultValue [||]
                    |],
                children =
                    elements {
                        RX.View(
                            styles = [| Styles.bubble |],
                            children =
                                elements {
                                    match connectionStatus with
                                    | ConnectionStatus.NetworkUnavailable ->
                                        LC.Text(
                                            "It looks like you are not connected to the Internet",
                                            styles = [| Styles.message |]
                                        )
                                    | ConnectionStatus.NetworkAvailableButBackendUnreachable maybeRetryingAt ->
                                        match maybeRetryingAt with
                                        | Some retryingAt ->
                                            LC.With.Now(
                                                fun now ->
                                                    LC.Text(
                                                        $"Disconnected from server - trying again {Helpers.retryingAtDeltaDisplay now retryingAt}",
                                                        styles = [| Styles.message |]
                                                    )
                                            )
                                        | None ->
                                            LC.Text(
                                                "Disconnected from server",
                                                styles = [| Styles.message |]
                                            )
                                    | ConnectionStatus.NetworkAvailableAndBackendReachable ->
                                        // We cannot get here due to top-level check
                                        LC.Text(
                                            "You are connected",
                                            styles = [| Styles.message |]
                                        )
                                }
                        )
                        bottom connectionStatus
                    }
            )
        | ConnectionStatus.NetworkAvailableAndBackendReachable ->
            noElement
