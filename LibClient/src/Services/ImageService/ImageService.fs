module LibClient.Services.ImageService

open LibClient
open LibLangFsharp

open LibClient.Services.HttpService.HttpService

module WebpSupport =
    open Fable.Core.JsInterop

    [<RequireQualifiedAccess>]
    type Feature =
    | Lossy
    | Lossless
    | Alpha
    | Animation

    let private testImages = Map.ofList [
        (Feature.Lossy,     "UklGRiIAAABXRUJQVlA4IBYAAAAwAQCdASoBAAEADsD+JaQAA3AAAAAA")
        (Feature.Lossless,  "UklGRhoAAABXRUJQVlA4TA0AAAAvAAAAEAcQERGIiP4HAA==")
        (Feature.Alpha,     "UklGRkoAAABXRUJQVlA4WAoAAAAQAAAAAAAAAAAAQUxQSAwAAAARBxAR/Q9ERP8DAABWUDggGAAAABQBAJ0BKgEAAQAAAP4AAA3AAP7mtQAAAA==")
        (Feature.Animation, "UklGRlIAAABXRUJQVlA4WAoAAAASAAAAAAAAAAAAQU5JTQYAAAD/////AABBTk1GJgAAAAAAAAAAAAAAAAAAAGQAAABWUDhMDQAAAC8AAAAQBxAREYiI/gcA")
    ]

    let isSupported (feature: Feature) : Async<bool> =
        #if EGGSHELL_PLATFORM_IS_WEB

        let source = testImages.TryFind feature |> Option.get // okay becuase we have one per feature
        let deferred = Deferred()
        let img = Browser.Dom.document.createElement("img") :?> Browser.Types.HTMLImageElement
        img.onload <- (fun _e ->
            deferred.Resolve (img.width > 0. && img.height > 0.)
        )
        img.onerror <- (fun _e ->
            deferred.Resolve false
        )
        img.src <- "data:image/webp;base64," + source
        deferred.Value

        #else
        // All native platforms we target support webp, so no check is necessary
        feature |> ignore
        Async.Of true

        #endif

type OptimizationSettings = {
    PixelDensityMultiplier:           float
    IsWebpSupported:                  bool
    RoundUpToBucket:                  int -> int
    MaybeInBundleImageServiceBaseUrl: Option<string>
    MaybeExternalImageServiceBaseUrl: Option<string>
}

[<RequireQualifiedAccess>]
type ImageSource =
    private
    | LocalWeb    of RelativePath: string
    | LocalNative of RelativePath: string
    | Global      of Url: string
    | Data        of DataUri: string
    with
        member this.Url : string =
            match this with
            | LocalWeb    value
            | LocalNative value
            | Global      value -> value
            | Data        value -> value

module ImageSource =
    let private isDataUri (url: string) : bool =
        url.StartsWith "data:"

    let restricted_ofNativeImport (value: obj) : ImageSource =
        ImageSource.LocalNative (value :?> string)

    let restricted_ofWebRelativePath (path: string) : ImageSource =
        ImageSource.LocalWeb path

    let ofUrl (url: string) : ImageSource =
        if isDataUri url then
            ImageSource.Data url
        else if not (HttpService.IsRelativeUrl url) then
            ImageSource.Global url
        else
            failwith (sprintf "Relative URLs not supported directly, you need to use `localImage \"%s\"` instead." url)

    let ofPossiblyRelativeUrl (localImageFn: string -> ImageSource) (possiblyRelativeUrl: string) : ImageSource =
        match HttpService.IsRelativeUrl possiblyRelativeUrl with
        | true  -> localImageFn possiblyRelativeUrl
        | false -> ofUrl possiblyRelativeUrl

type ImageService (httpService: HttpService, maybeInBundleImageServiceBaseUrl: Option<string>, maybeExternalImageServiceBaseUrl: Option<string>, makeOptimizedUrl: HttpService -> OptimizationSettings -> ImageSource -> Option<int> -> string, sizeBuckets: list<int>) =
    let roundUpToBucket (dimension: int) : int =
        sizeBuckets
        |> List.tryFind (fun candidate -> candidate >= dimension)
        |> Option.getOrElse dimension

    let mutable optimizationSettings = {
        PixelDensityMultiplier           = ReactXP.UserInterface.pixelDensity()
        IsWebpSupported                  = false
        RoundUpToBucket                  = roundUpToBucket
        MaybeInBundleImageServiceBaseUrl = maybeInBundleImageServiceBaseUrl
        MaybeExternalImageServiceBaseUrl = maybeExternalImageServiceBaseUrl
    }

    let mutable webpSupportDeterminedDeferred = Deferred()

    do
        async {
            let! isSupportedLossy    = WebpSupport.isSupported WebpSupport.Feature.Lossy
            let! isSupportedLossless = WebpSupport.isSupported WebpSupport.Feature.Lossless
            let! isSupportedAlpha    = WebpSupport.isSupported WebpSupport.Feature.Alpha
            optimizationSettings <- { optimizationSettings with IsWebpSupported = isSupportedLossy && isSupportedLossless && isSupportedAlpha }
            webpSupportDeterminedDeferred.Resolve ()
        } |> startSafely

    static member WithoutOptimizations (httpService: HttpService) : ImageService =
        let makeOptimizedUrl _ _ source _ =
            match source with
            | ImageSource.LocalNative relativePath -> relativePath
            | ImageSource.LocalWeb    relativePath -> relativePath
            | ImageSource.Global      url          -> url
            | ImageSource.Data        dataUri      -> dataUri

        ImageService (httpService, None, None, makeOptimizedUrl, [])

    member _.WhenInitialized () : Async<unit> =
        webpSupportDeterminedDeferred.Value

    member _.MakeOptimizedUrl (source: ImageSource) (maybeWidth: Option<int>) : string =
        makeOptimizedUrl httpService optimizationSettings source maybeWidth