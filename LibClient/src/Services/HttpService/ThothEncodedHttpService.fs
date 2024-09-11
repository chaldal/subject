module LibClient.Services.HttpService.ThothEncodedHttpService

module JS = Fable.Core.JS
open Fable.Core.JsInterop
open LibClient.Json

open Types

// F# does not allow us to use inheritance for record types,
// want: extends HttpService.ApiEndpoint
type ApiEndpoint<'UrlParams, 'Request, 'Response> = {
    Action:  HttpAction
    Url:     'UrlParams -> string
    Payload: 'Request -> obj
    Result:  obj -> 'Response
    Decoder: string -> Result<'Response, string>
}

// I tried making this a static member on ApiEndpoint, but for some strange reason,
// even when specifying the 'Response type explicitly, the resulting ApiEndpoint ends
// up having the third type parameter as type obj, not the specified type
let inline makeEndpoint<'UrlParams, 'Request, 'RequestTweaked, 'Response>
    (action: HttpAction)
    (urlBuilder: 'UrlParams -> string)
    // this is here to fix stack overflow for <'Index>-embellished types, see https://github.com/fable-compiler/Fable/issues/3607
    (tweakRequestTypeBeforeJsonEncoding: 'Request -> 'RequestTweaked)
    : ApiEndpoint<'UrlParams, 'Request, 'Response> =
    {
        Action  = action
        Url     = urlBuilder
        Payload = fun (request: 'Request) -> Json.ToString<'RequestTweaked> (tweakRequestTypeBeforeJsonEncoding request) :> obj
        Result  = mapHttpResult<'Response>
        Decoder = fun (source: string) -> Json.FromString<'Response> source
    }

type RequestError =
| Non200Code    of ErrorCode: int * Response: ReactXPHttp.WebResponse
| DecodingError of ErrorMessage: string * Body: obj * Response: ReactXPHttp.WebResponse

type ThothEncodedHttpService(httpService: HttpService.HttpService) =
    member this.Request<'Params, 'Payload, 'Result> (endpoint: ApiEndpoint<'Params, 'Payload, 'Result>) (parameters: 'Params) (payload: 'Payload) : Async<Result<'Result, RequestError>> =
        this.RequestWithMaybeHeaders endpoint parameters payload None

    member this.RequestLegacyRaisingOnError<'Params, 'Payload, 'Result> (endpoint: ApiEndpoint<'Params, 'Payload, 'Result>) (parameters: 'Params) (payload: 'Payload) : Async<'Result> = async {
        match! this.RequestWithMaybeHeaders endpoint parameters payload None with
        | Error e   -> return failwith (sprintf "%O" e)
        | Ok result -> return result
    }

    member _.RequestWithMaybeHeaders<'Params, 'Payload, 'Result> (endpoint: ApiEndpoint<'Params, 'Payload, 'Result>) (parameters: 'Params) (payload: 'Payload) (maybeHeaders: Option<Map<string, string>>) : Async<Result<'Result, RequestError>> = async {
        let url = (endpoint.Url parameters)
        let! result =
            let action = endpoint.Action

            let optionsSourceReturnRawData = [
                "returnRawData"   ==> true
                // HACK blanket settings, make more configurable if necessary later
                "withCredentials" ==> true
            ]

            let optionsHeaders =
                match maybeHeaders with
                | None -> []
                | Some headers ->
                    let headersObj = headers |> Map.map (fun _ v -> v :> obj) |> Map.toList |> createObj
                    [("overrideGetHeaders" ==> headersObj)]

            let optionsPayload =
                match (payload :> obj) = (() :> obj) with
                | true  -> []
                | false ->
                    let encodedPayload = endpoint.Payload payload
                    [("sendData" ==> encodedPayload)]

            let options = createObj (optionsPayload @ optionsHeaders @ optionsSourceReturnRawData)

            httpService.RequestReactXPRaw url action (Some options)

        match result.statusCode with
        | 200 | 201 | 202 | 204 ->
            let rawData = result.body :?> string
            return
                endpoint.Decoder rawData
                |> Result.mapError (fun message ->
                    DecodingError (message, rawData, result)
                )
        | statusCode ->
            return Non200Code (statusCode, result) |> Error
    }

