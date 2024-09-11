[<AutoOpen>]
module LibPushNotification.Connectors.PushNotificationConnector

open System
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open PushNotificationConnectors
open dotAPNS
open Google.Apis.Auth.OAuth2
open LibLifeCycle
open System.Net.Http
open System.Threading.Tasks
open LibPushNotification.Types
open Microsoft.Extensions.Logging
open LibPushNotification.Connectors.Configs
open LibPushNotification.Connectors.IosApnsConnector
open LibPushNotification.Connectors.AndroidFcmConnector


type PushNotificationEnvironment = {
    Clock:            Service<Clock>
    HttpClient:       HttpClient
    Logger:           ILogger<PushNotificationEnvironment>
    Config:           IConfiguration
    ServiceProvider:  IServiceProvider
} with interface Env

type PushNotificationRequest =
| SendNotification of DeviceTokens: seq<ClientDeviceToken> * Notification: PushNotification * ResponseChannel<Set<ClientDeviceToken>>
with interface Request

let private processAllResults
    (logger:          ILogger<PushNotificationEnvironment>)
    (responseChannel: ResponseChannel<Set<ClientDeviceToken>>)
    (allResults:      List<Task<array<PushNotificationRequestResult>>>)
    : Task<ResponseVerificationToken> =
    allResults
    |> Task.WhenAll
    |> Task.map (fun results ->
        results
        |> Seq.collect id
        |> Seq.filterMap (function
            | Error (PushNotificationRequestError.NonActionable error) ->
                logger.LogError(error, "Push notification sending failed")
                None
            | Error (PushNotificationRequestError.InvalidToken (token, error)) ->
                logger.LogError(error, $"Push notification sending failed for token: {token}")
                Some token
            | Ok () ->
                None
        )
        |> Set.ofSeq
        |> responseChannel.Respond
    )

let private sendNotificationToAndroidDevices
        (googleCredential: GoogleCredential)
        (httpClient:       HttpClient)
        (recipients:       List<NonemptyString>)
        (notification:     PushNotification)
        (fcmApiUrl:        NonemptyString)
        : Task<array<PushNotificationRequestResult>> =
    let fcmConfig = {
        ApiUrl = fcmApiUrl
    }

    let message = {
        Title           = notification.Title
        Body            = notification.Body
        MaybeIconUrl    = notification.MaybeIconUrl
        MaybeImageUrl   = notification.MaybeLargeImageUrl
        MaybeOnClickUrl = notification.MaybeOnClickUrl
        MaybeSound      = notification.MaybeSound
        ChannelId       =
            notification.MaybeChannelId
            |> Option.getOrElse DefaultNotificationChannelId
    }

    recipients
    |> Seq.distinct
    |> Seq.map (AndroidFcmConnector.sendMessage googleCredential httpClient fcmConfig message)
    |> Task.WhenAll

let private sendNotificationToIosDevices
        (apnsClient:   ApnsClient)
        (recipients:   List<NonemptyString>)
        (notification: PushNotification)
        : Task<array<PushNotificationRequestResult>> =
    let iosMessage = {
        Title                  = notification.Title
        Body                   = notification.Body
        MaybeSound             = None // TODO We should use notification.MaybeSound here
        MaybeNotificationCount = None
        MaybeOnClickUrl        = notification.MaybeOnClickUrl
    }

    recipients
    |> List.map (IosApnsConnector.sendMessage apnsClient iosMessage)
    |> Task.WhenAll

let private sendNotificationToWebDevices
        (env:          PushNotificationEnvironment)
        (recipients:   List<NonemptyString>)
        (notification: PushNotification)
        : Task<array<PushNotificationRequestResult>> =

    recipients
    |> Seq.distinct
    |> Seq.map (WebPushConnector.sendMessage env.Config notification)
    |> Task.WhenAll

let private requestProcessor (config: PushNotificationConfig) (env: PushNotificationEnvironment) (request: PushNotificationRequest) : Task<ResponseVerificationToken> =
    match request with
    | SendNotification (tokens, notification, responseChannel) ->
        let fcmTokens, apnsTokens, webTokens =
            (([], [], []), tokens)
            ||> Seq.fold
                (fun (fcmTokens, apnsTokens, webTokens) deviceToken ->
                    match deviceToken with
                    | FcmToken            token -> (token :: fcmTokens, apnsTokens, webTokens)
                    | ApnsToken           token -> (fcmTokens, token :: apnsTokens, webTokens)
                    | WebPushSubscription token -> (fcmTokens, apnsTokens, token :: webTokens)
                )

        let googleCredential = env.ServiceProvider.GetService<GoogleCredential>()
        let apnsClient = env.ServiceProvider.GetService<ApnsClient>()

        let results = [
            if googleCredential <> null then
                sendNotificationToAndroidDevices googleCredential env.HttpClient fcmTokens  notification config.FcmApiUrl
            if apnsClient <> null then
                sendNotificationToIosDevices apnsClient apnsTokens notification
            sendNotificationToWebDevices env webTokens  notification
        ]

        processAllResults
            env.Logger
            responseChannel
            results

type PushNotificationConnector (config: PushNotificationConfig) =
    let pushNotificationConnector : Connector<PushNotificationRequest,PushNotificationEnvironment> =
        ConnectorBuilder.newConnector "PushNotification"
        |> ConnectorBuilder.withRequestProcessor (requestProcessor config)

    member this.Connector = pushNotificationConnector