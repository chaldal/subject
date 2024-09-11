module LibClient.NetworkState

open System

[<RequireQualifiedAccess>]
type NetworkStateEvent =
| BackendConnectivitySucceeded
| BackendConnectivityFailed of MaybeRetryingAt: Option<DateTimeOffset>
| NonBackendConnectivitySucceeded
| NonBackendConnectivityFailed

let networkStateEventQueue: LibClient.EventBus.Queue<NetworkStateEvent> = LibClient.EventBus.Queue "networkState"

