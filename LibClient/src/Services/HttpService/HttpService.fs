module LibClient.Services.HttpService.HttpService

open Fable.Core
open Fable.Core.JsInterop
open LibClient.NetworkState

open Types
open LibClient

let private mapToReactXPHttpAction (action: HttpAction) : ReactXPHttp.HttpAction =
    match action with
    | Get    -> ReactXPHttp.HttpAction.Get
    | Post   -> ReactXPHttp.HttpAction.Post
    | Put    -> ReactXPHttp.HttpAction.Put
    | Delete -> ReactXPHttp.HttpAction.Delete

type ApiEndpoint<'Params, 'Payload, 'Result> = {
    Action:  HttpAction
    Url:     'Params -> string
    Payload: 'Payload -> obj
    Result:  obj -> 'Result
}

type ReactXPRawRequestParams = {
    Url:          string
    Action:       HttpAction
    MaybeOptions: Option<obj>
}

type RequestInterceptor  = ReactXPRawRequestParams -> Async<ReactXPRawRequestParams>
type ResponseInterceptor = ReactXPHttp.WebResponse -> Async<ReactXPHttp.WebResponse>

[<RequireQualifiedAccess>]
type StaticResourceUrlTransformSettings =
| None
| Pattern     of MaybeInBundlePattern: Option<string> * MaybeExternalPattern: Option<string>
| Transformer of (string -> string)

type HttpService(
        eventBus: LibClient.EventBus.EventBus,
        staticResourceUrlTransformSettings: StaticResourceUrlTransformSettings,
        isBackendUrl: string -> bool,
        maybeRelativeResourceUrlPrefix: Option<string>) =
    let mutable requestInterceptors:  List<RequestInterceptor>  = []
    let mutable responseInterceptors: List<ResponseInterceptor> = []

    member this.AddRequestInterceptor (interceptor: RequestInterceptor) : unit =
        requestInterceptors <- requestInterceptors @ [interceptor]

    member this.AddResponseInterceptor (interceptor: ResponseInterceptor) : unit =
        responseInterceptors <- responseInterceptors @ [interceptor]

    member this.Request<'Params, 'Payload, 'Result> (endpoint: ApiEndpoint<'Params, 'Payload, 'Result>) (parameters: 'Params) (payload: 'Payload) : Async<'Result> =
        this.RequestRaw (endpoint.Url parameters) endpoint.Action payload None None

    member this.PrepareStaticResourceUrl (rawUrl: string) : string =
        match staticResourceUrlTransformSettings with
        | StaticResourceUrlTransformSettings.None -> rawUrl
        | StaticResourceUrlTransformSettings.Transformer fn -> fn rawUrl
        | StaticResourceUrlTransformSettings.Pattern (maybeInBundlePattern, maybeExternalPattern) ->
            match (HttpService.IsRelativeUrl rawUrl, maybeInBundlePattern, maybeExternalPattern) with
            | (true, Some pattern, _) ->
                let preparedUrl = this.PrepareInBundleResourceUrl rawUrl
                pattern
                    .Replace("###-URL-RAW-###",     preparedUrl)
                    .Replace("###-URL-ENCODED-###", LibClient.JsInterop.encodeURIComponent preparedUrl)
            | (false, _, Some pattern) ->
                pattern
                    .Replace("###-URL-RAW-###",     rawUrl)
                    .Replace("###-URL-ENCODED-###", LibClient.JsInterop.encodeURIComponent rawUrl)
            | _ -> rawUrl

    member _.PrepareInBundleResourceUrl (relativeUrl: string) : string =
        match maybeRelativeResourceUrlPrefix with
        | None        -> relativeUrl
        | Some prefix -> prefix + relativeUrl

    static member IsRelativeUrl (url: string) : bool =
        (url.StartsWith "http://" || url.StartsWith "https://")
        |> not

    member this.RequestRaw<'Result, 'Payload> (url: string) (action: HttpAction) (payload: 'Payload) (maybeContentType: Option<string>) (maybeHeaders: Option<Map<string, string>>): Async<'Result> = async {
        let optionsHeaders =
            match maybeHeaders with
            | None -> []
            | Some headers ->
                let headersObj = headers |> Map.map (fun _ v -> v :> obj) |> Map.toList |> createObj
                [("overrideGetHeaders" ==> headersObj)]

        let optionsPayload =
            match (payload :> obj) = (() :> obj) with
            | true  -> []
            | false -> [("sendData" ==> payload)]

        let optionsContentType =
            match maybeContentType with
            | Some contentType -> ["contentType" ==> contentType]
            | None -> []

        let options = createObj (optionsPayload @ optionsHeaders @ optionsContentType)

        let! rawResult = this.RequestReactXPRaw url action (Some options)

        return rawResult.body :?> 'Result
    }

    member _.RequestReactXPRaw (rawUrl: string) (rawAction: HttpAction) (rawMaybeOptions: Option<obj>) : Async<ReactXPHttp.WebResponse> = async {
        let requestParamsAsync =
            {
                Url          = rawUrl
                Action       = rawAction
                MaybeOptions = rawMaybeOptions
            }
            |> Async.FoldOf requestInterceptors

        let! { Url = url; Action = action; MaybeOptions = maybeOptions } = requestParamsAsync

        let options =
            maybeOptions
            |> Option.getOrElseLazy (fun () -> createEmpty)

        let request: obj = createNew ReactXPHttp.ReactXPSimpleWebRequest (mapToReactXPHttpAction action, url, options)

        // we need this because ReactXPHttp throws a promise
        // exception for a non-200 error code
        let rawResponseAsync = async {
            let! rawResponseResult =
                request?start()
                |> Async.AwaitPromise
                |> Async.TryCatch

            match rawResponseResult with
            | Ok okResponse -> return okResponse
            | Error exn     -> return exn :> obj :?> ReactXPHttp.WebResponse
        }

        let! response = Async.Fold responseInterceptors rawResponseAsync

        let (|Informational|Successful|Redirection|ClientError|ServerError|Invalid|) (statusCode: int) =
            if statusCode >= 100 && statusCode < 200 then
                Informational
            else if statusCode >= 200 && statusCode < 300 then
                Successful
            else if statusCode >= 300 && statusCode < 400 then
                Redirection
            else if statusCode >= 400 && statusCode < 500 then
                ClientError
            else if statusCode >= 500 && statusCode < 600 then
                ServerError
            else
                Invalid

        match response.statusCode, isBackendUrl rawUrl with
        | Informational, true
        | Successful, true
        | Redirection, true
        | ClientError, true ->
            NetworkStateEvent.BackendConnectivitySucceeded
        | ServerError, true
        | Invalid, true ->
            NetworkStateEvent.BackendConnectivityFailed None
        | Informational, false
        | Successful, false
        | Redirection, false
        | ClientError, false ->
            NetworkStateEvent.NonBackendConnectivitySucceeded
        | ServerError, false
        | Invalid, false ->
            NetworkStateEvent.NonBackendConnectivityFailed
        |> eventBus.Broadcast networkStateEventQueue

        return response
    }

