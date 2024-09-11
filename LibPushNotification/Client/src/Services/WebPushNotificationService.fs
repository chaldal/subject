module LibPushNotification.Client.Web.Services

open LibClient
open Fable.Core
open Fable.Core.JsInterop
open LibPushNotification.Types
open Browser.Types
open System
open LibClient.Services.LocalStorageService

type iSubscribeOption =
    abstract userVisibleOnly: bool with get, set
    abstract applicationServerKey: array<string> with get, set

type subscribeOption = {
    userVisibleOnly: bool
    applicationServerKey: array<byte>
} with
    member this.toObj: iSubscribeOption =
        !!createObj [
            "userVisibleOnly"      ==> this.userVisibleOnly
            "applicationServerKey" ==> this.applicationServerKey
        ]

type iPushManager =
    abstract subscribe: iSubscribeOption -> JS.Promise<iSubscription>

type ServiceWorkerRegistration with
    [<Emit("$0.pushManager")>]
    member _.pushManager: iPushManager = jsNative

type WebNotification =
    abstract permission: string
    abstract requestPermission: unit -> JS.Promise<string>

[<Emit("Notification")>]
let Notification: WebNotification = jsNative

type WebPushConfig = {
    ServiceWorkerPath:    string
    ApplicationServerKey: string
}

type NotificationPermissionState =
| Default
| Granted
| Denied
with
    member this.IsDefaultState : bool =
        match this with
        | Default -> true
        | _       -> false

type WebDeviceId = WebDeviceId of Guid
    with
        static member newDeviceId (): WebDeviceId =
            WebDeviceId (Guid.NewGuid())

        member this.toNonemptyString (): NonemptyString =
            match this with
            | WebDeviceId guid -> guid.ToString() |> NonemptyString.ofStringUnsafe

[<RequireQualifiedAccess>]
type WebPushNotificationInitializationConfig<'Route, 'ResultlessDialog, 'ResultfulDialog> when 'Route: equality =
| PermissionAlreadyGranted of OnSubscription: (WebDeviceId * NonemptyString -> unit)
| WithDialog               of OnSubscription: (WebDeviceId * NonemptyString -> unit) * Nav: LibRouter.Components.With.Navigation.Navigation<'Route, 'ResultlessDialog, 'ResultfulDialog> * PromptMessage: string

type PushNotificationService (localStorageService: LocalStorageService, config: WebPushConfig) =
    let deferredSubscription = LibLangFsharp.Deferred<WebDeviceId * iSubscription>()

    member this.GetPermission () : NotificationPermissionState =
        Notification.permission
        |> this.MatchPermission

    member this.RequestPermission (): JS.Promise<NotificationPermissionState> =
        let notificationPermission =
            Notification.permission
            |> this.MatchPermission

        match notificationPermission with
        | Default ->
            Notification.requestPermission()
            |> Promise.map this.MatchPermission
        | _ ->
            notificationPermission
            |> JS.Constructors.Promise.resolve

    member this.RequestPermissionOnClick (onSubscription: WebDeviceId * NonemptyString -> unit, onDenied: unit -> unit) : unit =
        match this.GetPermission() with
        | Granted ->
            Noop
        | Denied ->
            onDenied()
        | Default ->
            this.Initialize true onSubscription
            |> startSafely

    member private this.ConfigureWithDialog (onSubscription: WebDeviceId * NonemptyString -> unit, nav: LibRouter.Components.With.Navigation.Navigation<'A,'B,'C>, promptMessage: string) = async {
        let! showDialog = this.ShouldShowDialog()
        if showDialog then
            let systemDialog: SystemDialog =
                ((None,
                  promptMessage,
                  "No",
                  "Yes",
                  (fun confirmed ->
                    if confirmed then
                        this.Initialize true onSubscription
                    else
                        this.SetShouldShowDialog false
                    |> startSafely
                  ))
                 |> SystemDialog.Confirm)

            nav.Go systemDialog ReactEvent.Action.NonUserOriginatingAction
        else
            this.Initialize false onSubscription |> startSafely
    }

    member this.Configure (config: WebPushNotificationInitializationConfig<'Route, 'ResultlessDialog, 'ResultfulDialog>) : Async<unit> =
        match config with
        | WebPushNotificationInitializationConfig.PermissionAlreadyGranted onSubscription ->
            this.Initialize false onSubscription
        | WebPushNotificationInitializationConfig.WithDialog (onSubscription, nav, promptMessage) ->
            this.ConfigureWithDialog (onSubscription, nav, promptMessage)

    member private this.Initialize (requestPermission: bool) (onInitialize: WebDeviceId * NonemptyString -> unit) : Async<unit> =
        let permissionDeniedErrorMessage = "Permission Denied"
        let mapResult (result: Result<WebDeviceId * iSubscription, string>) : unit =
            match result with
            | Error message when message = permissionDeniedErrorMessage ->
                ()
            | Error message ->
                Log.Error $"Failed to configure web push notification, error: {message}"
            | Ok (deviceId, token) ->
                let maybeTokenString = token.toJSON () |> jsonStringify |> NonemptyString.ofString
                match maybeTokenString with
                | None                    -> Log.Error "Failed to configure web push notification, subscription string is empty"
                | Some subscriptionString -> onInitialize (deviceId, subscriptionString)

        if not deferredSubscription.IsPending then
            deferredSubscription.Value
            |> Async.Map Result.Ok
            |> Async.Map mapResult
        else
            match this.RegisterWorker() with
            | None                 -> failwith "Service worker is not registered. You may register it your bootstrap"
            | Some registrationReq ->
                promise {
                    let! registration = registrationReq
                    let! permission   =
                        if requestPermission then
                            this.RequestPermission()
                        else
                            this.GetPermission()
                            |> promise.Return
                    return!
                        promise {
                            match permission with
                            | Granted ->
                                let! subscription = this.Subscribe registration
                                let! deviceId     = this.GetOrCreateDeviceId () |> Async.StartAsPromise
                                deferredSubscription.ResolveIfPending(deviceId, subscription)
                                return Ok (deviceId, subscription)
                            | Default
                            | Denied ->
                                return Error permissionDeniedErrorMessage
                            }
                            |> Promise.catch (fun error -> (
                                deferredSubscription.Reject(error)
                                Error error.Message
                            ))
                        }
                        |> Promise.map mapResult
                        |> Async.AwaitPromise

    member private this.GetShouldShowDialog () : Async<Option<bool>> =
        localStorageService.Get "ShouldShowNotificationDialog" Json.FromString

    member private this.SetShouldShowDialog (shouldShowNotificationDialog: bool) : Async<unit> =
        localStorageService.Put "ShouldShowNotificationDialog" shouldShowNotificationDialog Json.ToString

    member private this.ShouldShowDialog () = async {
        if this.GetPermission().IsDefaultState |> not then
            return false
        else
            let! shouldShowNotificationDialog = this.GetShouldShowDialog()
            return shouldShowNotificationDialog |> Option.getOrElse true
    }

    member private this.GetMaybeDeviceId () : Async<Option<WebDeviceId>> =
        localStorageService.Get "WebDeviceId" Json.FromString

    member this.SaveDeviceId (id: WebDeviceId) : Async<unit> =
        localStorageService.Put "WebDeviceId" id Json.ToString

    member this.GetOrCreateDeviceId (): Async<WebDeviceId> =
        async {
            let! maybeDeviceId = this.GetMaybeDeviceId()
            return!
                match maybeDeviceId with
                | Some deviceId -> async.Return deviceId
                | None ->
                    async {
                        let newWebDeviceId = WebDeviceId.newDeviceId ()
                        this.SaveDeviceId(newWebDeviceId) |> startSafely
                        return newWebDeviceId
                    }
        }

    member this.GetSubscription() =
        deferredSubscription.Value

    member private this.Subscribe (swRegistration: ServiceWorkerRegistration) : JS.Promise<iSubscription> =
        let applicationServerKey = this.UrlB64ToUint8Array(config.ApplicationServerKey);
        let subscribeOption: iSubscribeOption =
            {
                userVisibleOnly      = true
                applicationServerKey = applicationServerKey
            }.toObj

        swRegistration.pushManager.subscribe(subscribeOption)

    member this.RegisterWorker (): Option<JS.Promise<ServiceWorkerRegistration>> =
        Browser.Navigator.navigator.serviceWorker
        |> Option.map (fun worker -> worker.register config.ServiceWorkerPath)

    member private this.UrlB64ToUint8Array (base64String : string) : byte[] =
        let padding = String.replicate ((4 - base64String.Length % 4) % 4) "="
        let base64 =
            (base64String + padding)
                .Replace("-", "+")
                .Replace("_", "/")
        let rawData = Convert.FromBase64String(base64)
        Array.map int rawData |> Array.map byte

    member private this.MatchPermission (permissionString: string): NotificationPermissionState =
        match permissionString with
        | "granted" -> Granted
        | "denied"  -> Denied
        | "default"
        | _         -> Default