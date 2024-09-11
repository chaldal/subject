namespace LibUiSubject.Services.ViewService

open System
open LibClient
open LibClient.Services.HttpService.ThothEncodedHttpService
open LibUiSubject.Types

type IViewService<'Input, 'Output, 'OpError> =
    abstract member GetOne:     useCache: UseCache -> input: 'Input -> Async<AsyncData<'Output>>
    abstract member MakeGetUrl: input: 'Input -> string

type ViewService<'Input, 'Output, 'OpError when 'Input : comparison>
    (
        reasonablyFreshTTL:      TimeSpan,
        thothEncodedHttpService: ThothEncodedHttpService,
        apiEndpoints:            ViewEndpoints<'Input, 'Output, 'OpError>
    ) =

    let inMemoryCache = InMemoryCache<'Input, 'Output>(reasonablyFreshTTL)
    let throttling = Throttling<'Input, 'Output, 'OpError>(thothEncodedHttpService, apiEndpoints)

    member this.GetOne (useCache: UseCache) (input: 'Input) : Async<AsyncData<'Output>> = async {
        match inMemoryCache.GetCachedOutputForInput useCache input with
        | Some (_, outputAD) -> return outputAD
        | None               -> return! this.FetchOneAndCache input
    }

    member _.MakeGetUrl (input: 'Input) : string =
        (apiEndpoints.Get.Url ()) + "?i=" + ((apiEndpoints.Get.Payload input) :?> string |> LibClient.JsInterop.encodeURIComponent)

    member private this.FetchOneAndCache (input: 'Input) : Async<AsyncData<'Output>> = async {
        let! result = throttling.FetchOne input
        inMemoryCache.CacheOne input result
        return result
    }

    interface IViewService<'Input, 'Output, 'OpError> with
        member this.GetOne (useCache: UseCache) (input: 'Input) : Async<AsyncData<'Output>> =
            this.GetOne useCache input

        member this.MakeGetUrl (input: 'Input) : string =
            this.MakeGetUrl input

type ViewService private() =
    static member inline Create<'Input, 'Output, 'OpError when 'Input : comparison>
            (name:                    string)
            (reasonablyFreshTTL:      TimeSpan)
            (thothEncodedHttpService: ThothEncodedHttpService)
            (backendUrl:              string)
            : ViewService<'Input, 'Output, 'OpError> =
        let endpoints = ViewEndpoints.Make<'Input, 'Output, 'OpError> (name, backendUrl + "/api/v1/view")
        ViewService<'Input, 'Output, 'OpError> (reasonablyFreshTTL, thothEncodedHttpService, endpoints)
