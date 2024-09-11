module LibUiSubject.Services.RealTimeService

open System
open LibClient
open LibClient.NetworkState
open Fable.SignalR
open LibLifeCycleTypes.Api.V1
open Fable.Core
open Fable.Core.JsInterop

type OnReconnectedCallback = unit -> unit
type OnForceReconnectCallback = unit -> unit

// Each real-time connection's session (from backend's perspective) is based on cookie state when first connecting to the real-time endpoint. There could be
// no cookie if a session subject has not been created for this client. The backend applies ACLs with respect to the session, so it's important that we
// reconnect the real-time connection if the session identity changes, which only really happens when going from no session to some session.
//
// Note that we're using the term "established" here rather than "created" because we simply don't know if the session needed to be created or not. All we
// can do is ask the server to XxxOrConstruct the session (done by the SessionService), but we don't get notified whether construction was required or not,
// and therefore whether we already had a cookie or not.
let sessionEstablishedEventQueue: LibClient.EventBus.Queue<unit> = LibClient.EventBus.Queue "sessionEstablished"

[<Global("navigator")>]
let private navigator: obj = jsNative

let private takeBrowserTabLock (lockId: string) =
    if navigator <> JsUndefined && navigator?locks <> JsUndefined && navigator?locks?request <> JsUndefined then
        let mutable maybeReleaseLock: Option<unit -> unit> = None

        let promise = Fable.Core.JS.Constructors.Promise.Create(
            fun resolve _reject ->
                maybeReleaseLock <- Some resolve
        )

        let lockRequestPromise: JS.Promise<unit> =
            navigator?locks?request(
                lockId,
                fun () -> promise
            )

        match maybeReleaseLock with
        | Some resolveLock ->
            Ok (lockRequestPromise, resolveLock)
        | None ->
            Error "Lock could not be obtained"
    else
        Error "Tab locks are not a supported API of execution environment"

type RealTimeService(eventBus: LibClient.EventBus.EventBus, backendUrl: string, ?keepAlive: bool) as this =
    let log =
        Log
            .WithCategory("RealTimeService")

    let keepAlive =
        defaultArg
            keepAlive
#if EGGSHELL_PLATFORM_IS_WEB
            true
#else
            false
#endif

    do
        // If keepAlive is true, we make a best effort to never allow the real-time connection to go down.
        if keepAlive then
            let lockId = $"RealTime_KeepAliveLock_{Guid.NewGuid().ToString()}"
            log.Debug("Attempt to take browser tab lock with ID {LockId}", lockId)

            match takeBrowserTabLock lockId with
            | Ok (lockRequestPromise, _releaseLock) ->
                // We currently have no need to release the lock, since the real-time connection remains open wherever possible. Therefore, we never expect
                // to see these messages, but add them for diagnostic purposes.
                lockRequestPromise
                    .``then``(
                        onfulfilled = (fun _ -> log.Debug("Lock with ID {LockId} was released", lockId)),
                        onrejected = (fun _ -> log.Debug("Lock with ID {LockId} was rejected", lockId))
                    )
                |> ignore

                log.Debug("Browser tab lock with ID {LockId} successfully taken", lockId)
            | Error msg ->
                log.Error("Failed to take browser tab lock with ID {LockId}: {ErrorMessage}", lockId, msg)

        eventBus.On
            sessionEstablishedEventQueue
            (fun _ ->
                log.Debug("Forcing reconnection due to session being established")
                this.ForceReconnect() |> startSafely)
        |> ignore

        ReactXP.NetInfo.onIsConnectedChange (fun isConnected ->
            // In this scenario, we have detected that Internet connectivity has been re-established on the client. In such a case, we can preempt the retry
            // policy established below and force reconnection now.
            if isConnected then
                log.Debug("Forcing reconnection due connectivity change")
                this.ForceReconnect() |> startSafely
        )

    let realTimeEndpointUrl = $"{backendUrl}/api/v1/realTime"
    let mutable onReconnectedCallbacks: Map<Guid, OnReconnectedCallback> = Map.empty
    let mutable onForceReconnectCallbacks: Map<Guid, OnForceReconnectCallback> = Map.empty

    let notifyReconnected () =
        eventBus.Broadcast networkStateEventQueue NetworkStateEvent.BackendConnectivitySucceeded

        onReconnectedCallbacks
        |> Map.values
        |> Seq.iter (fun callback -> callback ())

    let notifyForceReconnect () =
        onForceReconnectCallbacks
        |> Map.values
        |> Seq.iter (fun callback -> callback ())

    let canErrorBeIgnored (message: string): bool =
        // We really don't care if start fails due to stop being called. See https://github.com/dotnet/aspnetcore/issues/38893
        // TODO: later versions of SignalR make it safer to trap this particular problem via an exception type rather than message.
        message = "Failed to start the HttpConnection before stop() was called."

    let startHubConnectionAndIgnoreIrrelevantErrors (hubConnection: HubConnection<_, _, _, _, _>): Async<unit> =
        async {
            try
                return! hubConnection.start ()
            with
            | e when canErrorBeIgnored e.Message ->
                return ()
        }

    // SignalR does not take ownership of retrying the initial connection - that's on us. See the docs for details (it's hidden away in the automatic
    // reconnection section: https://docs.microsoft.com/en-us/aspnet/core/signalr/javascript-client?view=aspnetcore-6.0&tabs=visual-studio#automatically-reconnect)
    let startConnectionWithRetry (hubConnection: HubConnection<_,_,_,_,_>) =
        let rec startWithRetry (hubConnection: HubConnection<_,_,_,_,_>) (attempt: int): Async<unit> =
            async {
                try
                    do! startHubConnectionAndIgnoreIrrelevantErrors hubConnection
                with
                | _ ->
                    let waitPeriod =
                        2.0 ** (float attempt)
                        |> min 15.0
                        |> int
                        |> TimeSpan.FromSeconds

                    let retryingAt = DateTimeOffset.UtcNow.Add(waitPeriod)
                    eventBus.Broadcast networkStateEventQueue (NetworkStateEvent.BackendConnectivityFailed (Some retryingAt))
                    do! Async.Sleep waitPeriod
                    do! startWithRetry hubConnection (attempt + 1)
            }

        startWithRetry hubConnection 0

    let hubConnection = lazy(
        // This retry policy is targeting the scenario where the client still has Internet connectivity, but the server becomes unavailable. In such cases,
        // we exponentially back of re-connection attempts so as to avoid flooding the server when it comes back up.
        let retryPolicy = {
            nextRetryDelayInMilliseconds = (fun context ->
                // Mitigate the thundering herd problem by staggering client reconnection attempts.
                let jitter =
                    match context.previousRetryCount with
                    | 0 -> System.Random().NextDouble() * 19.0
                    | _ -> 0.0

                // A simple exponential back-off.
                let nextRetry =
                    2.0 ** (float context.previousRetryCount)
                    |> (+) jitter
                    |> min 60.0
                    |> TimeSpan.FromSeconds

                let retryingAt = DateTimeOffset.UtcNow.Add(nextRetry)
                eventBus.Broadcast networkStateEventQueue (NetworkStateEvent.BackendConnectivityFailed (Some retryingAt))

                nextRetry.TotalMilliseconds |> int |> Some
            )
        }

        let logger =
            { new ILogger with
                member _.log (logLevel: LogLevel) (message: string) =
                    if logLevel < LogLevel.Warning then
                        ()
                    else
                        match logLevel with
                        | LogLevel.Debug ->
                            Browser.Dom.console.debug message
                        | LogLevel.Information ->
                            Browser.Dom.console.info message
                        | LogLevel.Warning ->
                            Browser.Dom.console.warn message
                        | LogLevel.Error
                        | LogLevel.Critical ->
                            // Even though we swallow ignorable exceptions, SignalR still logs them as errors internally. We need to ensure that log message
                            // does not propagate up.
                            if not (canErrorBeIgnored message) then
                                Browser.Dom.console.error message
                        | _ ->
                            Browser.Dom.console.trace message
                    ()
            }

        let hubConnection =
            SignalR.connect<ClientApi, ClientStreamApi, unit, ServerApi, ServerStreamApi>(
                fun hubConnectionBuilder ->
                    hubConnectionBuilder.withUrl(
                        realTimeEndpointUrl,
                        (
                            fun builder ->
                                builder
                                    // TODO: We currently force the use of WebSockets because otherwise negotiation fails
                                    //       when the server is running under Fabric. This is because we don't have Traefik
                                    //       set up with sticky sessions, so the negotiate and subsequent requests hit
                                    //       different nodes, and the connection fails.
                                    .transport(TransportType.WebSockets)
                                    .skipNegotiation(true)
                                    // Only required because leaving as None is not correctly translated to a JS undefined.
                                    .withCredentials(true)
                            )
                        )
                        .withAutomaticReconnect(retryPolicy)
                        .configureLogging(logger)
                        .onReconnected(fun _ -> notifyReconnected ())
            )

        // It's very important we don't return an Async<'T> from the Lazy because it will be re-evaluated on every access, causing the connection
        // logic to re-execute. So we first off the initial connection independently. It will keep trying to connect until that initial connection
        // is established, after which SignalR itself takes care of re-connecting upon any connection failure.
        startConnectionWithRetry hubConnection
        |> startSafely

        hubConnection
    )

    member private _.ForceReconnect () : Async<unit> =
        async {
            notifyForceReconnect ()

            let hubConnection = hubConnection.Value

            // Stopping the connection is required in order for us to start it again. This will also force the connection out of reconnecting mode.
            do! hubConnection.stop ()

            // Now we can start it again...
            do! startConnectionWithRetry hubConnection

            if hubConnection.state = ConnectionState.Connected then
                // ...and notify all interested parties (so that streams can be restarted, in particular).
                notifyReconnected ()
        }

    member _.OnReconnected (callback: OnReconnectedCallback): IDisposable =
        let callbackId = Guid.NewGuid()
        onReconnectedCallbacks <-
            onReconnectedCallbacks
            |> Map.add callbackId callback

        { new IDisposable with
            member _.Dispose() =
                onReconnectedCallbacks <-
                    onReconnectedCallbacks
                    |> Map.remove callbackId
        }

    member _.OnForceReconnect (callback: OnForceReconnectCallback): IDisposable =
        let callbackId = Guid.NewGuid()
        onForceReconnectCallbacks <-
            onForceReconnectCallbacks
            |> Map.add callbackId callback

        { new IDisposable with
            member _.Dispose() =
                onForceReconnectCallbacks <-
                    onForceReconnectCallbacks
                    |> Map.remove callbackId
        }

    member _.StreamFrom (msg: ClientStreamApi) (subscriber: StreamSubscriber<ServerStreamApi>) : Async<IDisposable> =
        async {
            return! hubConnection.Value.stream msg subscriber (fun _ -> Noop)
        }
