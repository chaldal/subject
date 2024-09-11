module LibPushNotification.Connectors.AndroidFcmConnector

open System
open System.Collections.Generic
open System.Net
open System.Net.Http
open Google.Apis.Auth.OAuth2
open LibPushNotification.Connectors.Configs
open Newtonsoft.Json
open System.Threading.Tasks
open LibPushNotification.Types

let expiryTime = TimeSpan.FromMinutes(30)
let mutable globalTokenWithExpiry = NonemptyString.ofLiteral "N/A", DateTimeOffset.MinValue

// NB: We're currently using a deprecated API, as we use Android notifications in other
//     projects, and will update everything at once so as to minimise potential issues.
//     It may be worth removing all this common code into a shared Lib to ease future
//     updates.

// NB: I can't make this private as `Newtonsoft` is then
//     unable to find the type in order to serialize it
type AndroidNotificationPayload = {
    [<JsonProperty("channel_id")>]
    ChannelId: string
    
    [<JsonProperty("icon", Required = Required.AllowNull, NullValueHandling=NullValueHandling.Ignore)>]
    Icon: string
    
    [<JsonProperty("sound", Required = Required.AllowNull, NullValueHandling=NullValueHandling.Ignore)>]
    Sound: string
    
    [<JsonProperty("image", Required = Required.AllowNull, NullValueHandling=NullValueHandling.Ignore)>]
    Image: string
}

type AndroidPayload = {
    [<JsonProperty("notification")>] Notification: AndroidNotificationPayload
}

type (* private *) MessagePayload = {
    [<JsonProperty "token">]        
    RecipientFcmToken: string
    
    [<JsonProperty "notification">]
    Notification: System.Collections.Generic.IDictionary<string, string>
    
    [<JsonProperty "data">]
    Data: System.Collections.Generic.IDictionary<string, string>
    
    [<JsonProperty "android">]
    Android: AndroidPayload
}

type (* private *) JsonPayload = {
    [<JsonProperty "message">] Message: MessagePayload
}

type SendPushNotificationResponse = {
    [<JsonProperty("failure")>]
    Failure: int

    [<JsonProperty("success")>]
    Success: int

    [<JsonProperty("results")>]
    Results: Dictionary<string, string> array
}

type AndroidFcmConfiguration = {
    ApiUrl: NonemptyString
}

let maybeAddKey (key: string) (opt: Option<'T>) : Option<string * 'T> =
    opt |> Option.map (fun t -> (key, t))

let private getAccessToken (googleCredential: GoogleCredential) : Task<Result<NonemptyString, Exception>> =
    backgroundTask {
        let! maybeToken = googleCredential.UnderlyingCredential.GetAccessTokenForRequestAsync() |> Task.map NonemptyString.ofString
        return
            match maybeToken  with
            | Some token -> token |> Ok
            | None -> Error (Exception "No Token Found")
    }


type AndroidFcmMessage =
    {
        Title:           NonemptyString
        Body:            NonemptyString
        MaybeIconUrl:    Option<NonemptyString>
        MaybeImageUrl:   Option<NonemptyString>
        MaybeOnClickUrl: Option<NonemptyString>
        MaybeSound:      Option<NonemptyString>
        ChannelId:       NotificationChannelId
    }

let private makeHttpRequestMessage
        (config:      AndroidFcmConfiguration)
        (accessToken: NonemptyString)
        (message:     AndroidFcmMessage)
        (recipient:   NonemptyString)
        : HttpRequestMessage =
    let notification =
        [
            ("title", message.Title.Value)
            ("body",  message.Body.Value)
        ]
        |> Map.ofList

    let data =
        let baseData =
            [
                ("guid", Guid.NewGuid().ToString())
            ]
            |> Map.ofList

        [
            message.MaybeOnClickUrl |> Option.map (fun v -> v.Value) |> maybeAddKey Configs.onClickUrlKeyName
        ]
        |> Seq.fold
            (fun acc maybeKvp -> Map.addOption maybeKvp acc)
            baseData
        |> Map.toSeq
        |> dict

    let content =
        JsonConvert.SerializeObject(
            {
                Message = {
                    RecipientFcmToken = recipient.Value
                    Notification      = notification
                    Data              = data
                    Android           = {
                        Notification = {
                            ChannelId = message.ChannelId.Value.Value
                            Sound     = message.MaybeSound    |> Option.mapOrElse null (fun v -> v.Value) 
                            Image     = message.MaybeImageUrl |> Option.mapOrElse null (fun v -> v.Value) 
                            Icon      = message.MaybeIconUrl  |> Option.mapOrElse null (fun v -> v.Value) 
                        }
                    }
                }
            }
        )

    let requestMessage = new HttpRequestMessage(HttpMethod.Post, config.ApiUrl.Value)
    requestMessage.Content <-
        new StringContent(
            content,
            System.Text.Encoding.UTF8,
            "application/json"
        )

    requestMessage.Headers.TryAddWithoutValidation(
        HttpRequestHeader.Authorization.ToString(),
        $"Bearer {accessToken.Value}"
    )
    |> ignore
    requestMessage

let private sendHttpMessage
        (httpClient:         HttpClient)
        (recipient:          NonemptyString)
        (httpRequestMessage: HttpRequestMessage)
        : Task<PushNotificationRequestResult> =
    backgroundTask {
        try
            match! httpClient.SendAsync httpRequestMessage with
            | response when response.IsSuccessStatusCode ->
                let! responseContent = response.Content.ReadAsStringAsync()
                try
                    let requestNotificationJsonResponse = JsonConvert.DeserializeObject<SendPushNotificationResponse>(responseContent)

                    match requestNotificationJsonResponse.Failure > 0, requestNotificationJsonResponse.Results[0].Get("error") with
                    | true, Some "NotRegistered" ->
                        let exn = Exception $"FCM token NotRegistered. {recipient}"
                        return PushNotificationRequestError.InvalidToken (recipient |> FcmToken, exn) |> Error
                    | _ ->
                        return () |> Ok
                with
                | _ ->
                    return () |> Ok
            | failedResponse ->
                let exn = Exception $"Request to {httpRequestMessage.RequestUri} failed with HTTP status code: {failedResponse.StatusCode}"

                return PushNotificationRequestError.InvalidToken (recipient |> FcmToken, exn) |> Error
        with
        | exn ->
            return PushNotificationRequestError.InvalidToken (recipient |> FcmToken, exn) |> Error
    }

let sendMessage
        (googleCredential: GoogleCredential)
        (httpClient: HttpClient)
        (config:     AndroidFcmConfiguration)
        (message:    AndroidFcmMessage)
        (recipient:  NonemptyString)
        : Task<PushNotificationRequestResult> =
    backgroundTask {
        let! tokenResult = getAccessToken googleCredential

        match tokenResult with
        | Ok accessToken ->
            return!
                recipient
                |> makeHttpRequestMessage config accessToken message
                |> sendHttpMessage httpClient recipient

        | Error exn ->
            return PushNotificationRequestError.InvalidToken (recipient |> FcmToken, exn) |> Error
    }

