module LibPushNotification.Connectors.Configs
open LibPushNotification.Types

type PushNotificationConfig = {
    FcmApiUrl: NonemptyString
}

let onClickUrlKeyName = "onClickUrl"

[<RequireQualifiedAccess>]
type PushNotificationRequestError =
| InvalidToken  of ClientDeviceToken * System.Exception
| NonActionable of System.Exception

type PushNotificationRequestResult = Result<unit, PushNotificationRequestError >