module LibPushNotification.Connectors.IosApnsConnector

open System
open LibPushNotification.Connectors.Configs
open dotAPNS
open System.Net.Http
open System.Threading.Tasks
open LibPushNotification.Types
open System.Security.Cryptography.X509Certificates

type IosApnsConfiguration = {
    CertificatePrivateKey: byte[]
    CertificatePassword:   NonemptyString
}

type IosApnsMessage =
    {
        Title:                  NonemptyString
        Body:                   NonemptyString
        MaybeSound:             Option<NonemptyString>
        MaybeNotificationCount: Option<int>
        MaybeOnClickUrl:        Option<NonemptyString>
    }

let sendMessage
        (apnsClient: ApnsClient)
        (message:    IosApnsMessage)
        (iosToken:   NonemptyString)
        : Task<PushNotificationRequestResult> =
    backgroundTask {
        let notification =
            ApplePush(ApplePushType.Alert)
                .AddAlert(message.Title.Value, message.Body.Value)
                .AddToken(iosToken.Value)
            |> fun applePush -> message.MaybeSound             |> Option.mapOrElse (applePush.AddSound()) (fun sound             -> applePush.AddSound(sound.Value))
            |> fun applePush -> message.MaybeNotificationCount |> Option.mapOrElse applePush              (fun notificationCount -> applePush.AddBadge(notificationCount))
            |> fun applePush -> message.MaybeOnClickUrl        |> Option.mapOrElse applePush              (fun onClickUrl        -> applePush.AddCustomProperty(Configs.onClickUrlKeyName, onClickUrl.Value))

        try
            match! apnsClient.SendAsync notification with
            | response when response.IsSuccessful ->
                return () |> Ok
            | response ->
                return PushNotificationRequestError.NonActionable (Exception $"Failed to send a push, APNs reported an error: {response.ReasonString}") |> Error
        with
        | :? ApnsCertificateExpiredException ->
            return PushNotificationRequestError.InvalidToken (ApnsToken iosToken, Exception "APNs certificate has expired.") |> Error
        | exn ->
            return PushNotificationRequestError.InvalidToken (ApnsToken iosToken, exn) |> Error

    }

// Make client using APNS certificate which doesn't work for us. TODO remove it in future.
let makeSandboxApnsClient (config: IosApnsConfiguration) : ApnsClient =
    let certificate = new X509Certificate2(config.CertificatePrivateKey, config.CertificatePassword.Value)
    ApnsClient.CreateUsingCert(certificate).UseSandbox()

let makeProductionApnsClient (config: IosApnsConfiguration) : ApnsClient =
    let certificate = new X509Certificate2(config.CertificatePrivateKey, config.CertificatePassword.Value)
    ApnsClient.CreateUsingCert(certificate)

// Make client using APNS token 
let makeSandboxApnsClientUsingJwt (jwtOptions: ApnsJwtOptions) (httpClient: HttpClient) : ApnsClient =
    ApnsClient.CreateUsingJwt(httpClient, jwtOptions).UseSandbox()
    
let makeProductionApnsClientUsingJwt (jwtOptions: ApnsJwtOptions) (httpClient: HttpClient) : ApnsClient =
    ApnsClient.CreateUsingJwt(httpClient, jwtOptions)    