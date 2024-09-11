module LibPushNotification.Client.Services

open Fable.Core
open LibClient
open Fable.Core.JsInterop
open LibPushNotification.Types
open ReactXP

type IConfig =
    abstract onRegister:             string -> unit
    abstract onNotification:         obj -> unit
    abstract onAction:               obj -> unit
    abstract onRegistrationError:    obj -> unit
    abstract popInitialNotification: bool with get, set
    abstract requestPermissions:     bool with get, set

type IToken =
    abstract token: string with get, set
    abstract os:    string with get, set

type IData =
    abstract onClickUrl: Option<string> with get, set

type INotification =
    abstract foreground:      bool   with get, set
    abstract userInteraction: bool   with get, set
    abstract message:         string with get, set
    abstract data:            IData  with get, set
    abstract finish:          obj -> unit
    abstract channelId:       string

type PermissionStatus =
| Granted
| Denied
| NeverAskAgain
    static member OfString (statusString: string): PermissionStatus =
        match statusString with
        | "granted"         -> Granted
        | "denied"          -> Denied
        | "never_ask_again" -> NeverAskAgain
        | _                 -> Denied // Should keep an eye on this in-case of issues

type IPermission =
    abstract request: obj  -> JS.Promise<string>
    abstract check: string -> JS.Promise<bool>
    
type IPlatform =
    abstract Version: int
    
let androidNotificationPermissionNamespace = "android.permission.POST_NOTIFICATIONS"
exception PushNotificationPermissionError of string

// The push notification library we are using, does handle iOS permission request internally with
// requestPermissions param. But doesn't support android 13 permission request.
//
// We are currently utilizing the `requestPermissions` for the iOS and a manual permission request
// For the Android.
type PushNotificationService () =
    let pushNotificationIOS: obj        = import "default" "@react-native-community/push-notification-ios"
    let pushNotification: obj           = importDefault "react-native-push-notification"
    let permissionsAndroid: IPermission = import "PermissionsAndroid" "react-native"
    let platform: IPlatform             = import "Platform" "react-native"

    let deferredToken = LibLangFsharp.Deferred<NonemptyString>()

    member this.Configure (callback: INotification -> unit) : unit =
        let config : IConfig =
            !!{|
                onRegister =
                    fun (theToken: IToken) ->
                        if deferredToken.IsPending then
                            deferredToken.Resolve (NonemptyString.ofStringUnsafe theToken.token) // safe because there will be token
                onNotification =
                   fun (notification: INotification) ->
                        callback notification

                        notification.finish(pushNotificationIOS?FetchResult?NoData) // required

                onRegistrationError =
                    fun error ->
                        Log.Error("Failed to register device for push notification: {error}", error)
                        deferredToken.Reject
                popInitialNotification = true
                requestPermissions     = true
            |}
            
        async {
            // on Android 13 - we need to manually request notification permission
            // for iOS - the push-notification library internally handle the permission request. 
            match Runtime.platform with
            | Platform.Native NativePlatform.Android ->
                this.CreateChannel DefaultNotificationChannel
                return! this.AndroidRequestPermissionAndRegisterPushNotification config
            | _ -> 
                return! this.IOSRegisterPushNotification config
        } |> AsyncHelpers.startSafely
        
    member private this.ConfigureNotification (config: IConfig) =
        pushNotification?configure(config)
        
    member private this.IOSRegisterPushNotification (config: IConfig) : Async<unit> =
        this.ConfigureNotification config
        |> Async.Of
        
    member private this.AndroidRequestPermissionAndRegisterPushNotification (config: IConfig): Async<unit> =
        async {
            let! hasNotificationPermission = this.HasAndroidNotificationPermission ()
            
            let! shouldConfigureNotification =
                match hasNotificationPermission with
                | true  -> Async.Of true
                | false ->
                    async {
                        let! permission = this.Android13RequestNotificationPermission ()
        
                        match permission with
                        | Granted ->
                            return true
                        | Denied
                        | NeverAskAgain ->
                            Log.Error("Failed to register device for push notification: {error}", permission)
                            deferredToken.Reject (PushNotificationPermissionError $"Permission is not granted {permission}")
                            return false
                    }

            if shouldConfigureNotification then
                this.ConfigureNotification config
        }
        
    member private this.HasAndroidNotificationPermission() : Async<bool> =
        if platform.Version >= 33 then // android 13 = platform version 33
            async {
                return! this.Android13CheckNotificationPermission ()
            }
        else
            Async.Of true
        
    member private this.Android13CheckNotificationPermission (): Async<bool> =
        promise {
            return! permissionsAndroid.check androidNotificationPermissionNamespace
        }
        |> Async.AwaitPromise
         
    member private this.Android13RequestNotificationPermission  (): Async<PermissionStatus> =
        promise {
            let! permission = permissionsAndroid.request(androidNotificationPermissionNamespace)
            return PermissionStatus.OfString permission
        }
        |> Async.AwaitPromise

    member this.getToken () : Async<NonemptyString> =
        deferredToken.Value

    // Complete my implementation.
    member this.GetChannels () =
        pushNotification?getChannels (fun channel_ids ->
            Fable.Core.JS.console.log "Checking channel"
            Fable.Core.JS.console.log channel_ids
        )

    member this.CreateChannel (channelDetails: NotificationChannel) =
        let (NotificationChannelId channelId) = channelDetails.Id
        let soundName =
            match channelDetails.MaybeSoundName with
            | Some sound -> sound.Value
            | None       -> "default"

        let channelAttr = createObj [
            "channelId"          ==> channelId.Value
            "channelName"        ==> channelDetails.Name.Value
            "channelDescription" ==> channelDetails.Description.Value
            "playSound"          ==> true
            "soundName"          ==> soundName
            "importance"         ==> channelDetails.Importance
            "vibrate"            ==> true
        ]

        pushNotification?createChannel (channelAttr, ( fun _ -> ()))

    member this.DeleteChannel (channelId: NonemptyString) =
        pushNotification?deleteChannel(channelId.Value);
