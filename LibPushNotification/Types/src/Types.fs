[<CodecLib.CodecAutoGenerate>]
module LibPushNotification.Types

type iSubscription =
    abstract endpoint: string with get, set
    abstract expirationTime: Option<double> with get, set
    abstract toJSON: unit -> obj

type ClientDeviceToken =
| FcmToken  of NonemptyString
| ApnsToken of NonemptyString
| WebPushSubscription of NonemptyString


type NotificationChannelId = NotificationChannelId of NonemptyString
with
    member this.Value : NonemptyString =
        let (NotificationChannelId value ) = this
        value


type PushNotification = {
    Title:              NonemptyString
    Body:               NonemptyString
    MaybeIconUrl:       Option<NonemptyString>
    MaybeLargeImageUrl: Option<NonemptyString>
    MaybeSound:         Option<NonemptyString>
    MaybeChannelId:     Option<NotificationChannelId>
    MaybeOnClickUrl:    Option<NonemptyString>
}

[<RequireQualifiedAccess>]
type NotificationImportance =
| DEFAULT     = 3
| HIGH        = 4
| LOW         = 2
| MIN         = 1
| NONE        = 0
| UNSPECIFIED = -1000

type NotificationChannel = {
    Id:             NotificationChannelId
    Name:           NonemptyString
    Description:    NonemptyString
    MaybeSoundName: Option<NonemptyString>
    Importance:     NotificationImportance
}

let DefaultNotificationChannelId = NotificationChannelId(NonemptyString.ofStringUnsafe "Default")
let DefaultNotificationChannel = {
    Id = DefaultNotificationChannelId
    Name = (NonemptyString.ofStringUnsafe "Default")
    Description = (NonemptyString.ofStringUnsafe "Default")
    MaybeSoundName = None
    Importance = NotificationImportance.DEFAULT
}

////////////////////////////////
// Generated code starts here //
////////////////////////////////


#if !FABLE_COMPILER

open CodecLib

type ClientDeviceToken with
    static member private get_ObjCodec_V1 () =
        function
        | FcmToken _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function FcmToken _ -> Some 0 | _ -> None)
                and! payload = reqWith codecFor<_, NonemptyString> "FcmToken" (function (FcmToken x) -> Some x | _ -> None)
                return FcmToken payload
            }
        | ApnsToken _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function ApnsToken _ -> Some 0 | _ -> None)
                and! payload = reqWith codecFor<_, NonemptyString> "ApnsToken" (function (ApnsToken x) -> Some x | _ -> None)
                return ApnsToken payload
            }
        | WebPushSubscription _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function WebPushSubscription _ -> Some 0 | _ -> None)
                and! payload = reqWith codecFor<_, NonemptyString> "WebPushSubscription" (function (WebPushSubscription x) -> Some x | _ -> None)
                return WebPushSubscription payload
            }
        |> mergeUnionCases
    static member get_Codec () = ofObjCodec (ClientDeviceToken.get_ObjCodec_V1 ())

#endif // !FABLE_COMPILER