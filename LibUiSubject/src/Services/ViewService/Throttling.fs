namespace LibUiSubject.Services.ViewService

open System
open LibLangFsharp
open LibClient
open LibClient.Services.HttpService.Types
open LibClient.Services.HttpService.ThothEncodedHttpService

type ViewEndpoints<'Input, 'Output, 'OpError> = {
    Get: ApiEndpoint<unit, 'Input, 'Output>
} with
    static member inline Make<'Input, 'Output, 'OpError> (viewName: string, viewUrlBase: string) : ViewEndpoints<'Input, 'Output, 'OpError> =
        {
            Get = makeEndpoint<unit, 'Input, 'Input, 'Output> Post (fun () -> sprintf "%s/%s/" viewUrlBase viewName) id
        }

// "Throttle" means if there is an ongoing request for a given key, then use it rather than starting another.
type internal Throttling<'Input, 'Output, 'OpError when 'Input : comparison>(
        thothEncodedHttpService: ThothEncodedHttpService,
        apiEndpoints: ViewEndpoints<'Input, 'Output, 'OpError>
    ) =
    let mutable ongoingRequestsOne: Map<'Input, Deferred<AsyncData<'Output>>> = Map.empty

    member private _.Throttled<'T, 'K when 'K : comparison> (ongoingRequests: Map<'K, Deferred<'T>>) (updateOngoingRequests: (Map<'K, Deferred<'T>> -> Map<'K, Deferred<'T>>) -> unit) (key: 'K) (rawFetch: unit -> Async<'T>) : Async<'T> =
        match ongoingRequests.TryFind key with
        | Some ongoingRequestDeferred -> ongoingRequestDeferred.Value
        | None ->
            let newRequestDeferred = Deferred<'T>()
            updateOngoingRequests (fun ongoingRequests -> ongoingRequests.Add (key, newRequestDeferred))

            async {
                try
                    let! result = rawFetch ()
                    updateOngoingRequests (fun ongoingRequests -> ongoingRequests.Remove key)
                    newRequestDeferred.Resolve result
                with
                | exn ->
                    updateOngoingRequests (fun ongoingRequests -> ongoingRequests.Remove key)
                    newRequestDeferred.Reject exn
            } |> startSafely

            newRequestDeferred.Value

    member this.FetchOne (input: 'Input) : Async<AsyncData<'Output>> =
        this.Throttled
            ongoingRequestsOne
            (fun updater -> ongoingRequestsOne <- updater ongoingRequestsOne)
            input
            (fun () -> this.RawFetchOne input)

    member private _.RawFetchOne (input: 'Input) : Async<AsyncData<'Output>> = async {
        match! thothEncodedHttpService.Request apiEndpoints.Get () input with
        | Ok output                   -> return Available output
        | Error (Non200Code (422, webResponse)) ->
            match Json.FromString<String>(webResponse.body :?> string) with
            | Ok decodedError -> return Failed (RequestFailure (ClientError (422, decodedError)))
            | Error _         -> return Failed (UnknownFailure "Decode Error")
        | Error (Non200Code (404, _)) -> return Unavailable
        | Error (Non200Code (0,   _)) -> return Failed AsyncDataFailure.NetworkFailure
        | Error (Non200Code (responseCode, response)) -> return Failed (AsyncDataFailure.RequestFailure (RequestFailure.ofStatusCode (responseCode, jsonStringify(response))))
        | Error error                 -> return Failed (UnknownFailure (error.ToString()))
    }
