module PushNotificationConnectors.WebPushConnector
open System
open System.Net
open LibPushNotification.Connectors.Configs
open LibPushNotification.Types

open Microsoft.Extensions.Configuration
open Newtonsoft.Json
open WebPush
open LibLifeCycle.Config

type PushEncryptionKeys  = {
    auth: string
    p256dh: string
}

type WebPushSubscription = {
    endpoint:       string
    expirationTime: Option<double>
    keys:           PushEncryptionKeys
}

[<CLIMutable>]
type WebPushNotificationConfig = {
    Subject:    string
    PublicKey:  string
    PrivateKey: string
}
with
    interface IValidatable with
        member this.Validate(): unit =
            if String.IsNullOrWhiteSpace(this.Subject) then
                ConfigurationValidationException("PushNotification.Web.[Subject] not specified", this.Subject)       |> raise
            elif String.IsNullOrWhiteSpace(this.PublicKey) then
                ConfigurationValidationException("PushNotification.Web.[PublicKey] not specified", this.PublicKey)   |> raise
            elif String.IsNullOrWhiteSpace(this.PrivateKey) then
                ConfigurationValidationException("PushNotification.Web.[PrivateKey] not specified", this.PrivateKey) |> raise


let createNotificationPayload notification =
    JsonConvert.SerializeObject(
        {|
            title          = notification.Title.Value
            body           = notification.Body.Value
            onClickUrl     = notification.MaybeOnClickUrl |> NonemptyString.optionToString
            expirationTime = (DateTimeOffset.Now + (TimeSpan.FromDays 1)).ToUnixTimeMilliseconds()
            tag            = Guid.NewGuid().ToString()
        |}
    )

let sendMessage
        (configService: IConfiguration)
        (notification:  PushNotification)
        (token:         NonemptyString)
        : System.Threading.Tasks.Task<PushNotificationRequestResult> =
    backgroundTask {
        try
            let config = configService.GetSection("PushNotification.Web").GetAndValidate<WebPushNotificationConfig>()
            let vapidDetails = VapidDetails(config.Subject, config.PublicKey, config.PrivateKey)
            let webPushSubscription = JsonConvert.DeserializeObject<WebPushSubscription> (token.Value.Substring (token.Value.IndexOf('{')))
            let subscription = PushSubscription(webPushSubscription.endpoint, webPushSubscription.keys.p256dh, webPushSubscription.keys.auth)
            let client = new WebPushClient()
            do! client.SendNotificationAsync (subscription, createNotificationPayload notification, vapidDetails)
            return () |> Ok
        with
        | ConfigurationMissingException    _
        | ConfigurationValidationException _ ->
            return PushNotificationRequestError.NonActionable (Exception "Configuration missing or invalid for PushNotification.Web") |> Error
        | :? WebPushException as ex when ex.StatusCode = HttpStatusCode.TooManyRequests || ex.StatusCode = HttpStatusCode.RequestEntityTooLarge ->
            return PushNotificationRequestError.NonActionable (Exception $"Unexpected StatusCode: {ex.StatusCode}") |> Error
        | exn ->
            return PushNotificationRequestError.InvalidToken (ClientDeviceToken.WebPushSubscription token, exn) |> Error
    }