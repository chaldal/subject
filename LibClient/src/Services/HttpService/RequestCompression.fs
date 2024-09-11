module LibClient.Services.HttpService.RequestCompression

open Fable.Core.JsInterop
open LibClient.Services.HttpService.HttpService

let makeInterceptor (resultUrlRegexSource: string) (minPayloadSizeToEncodeBytes: int) : RequestInterceptor =
    let pako: obj = importDefault "pako"

    let regex = System.Text.RegularExpressions.Regex resultUrlRegexSource

    fun (rawRequestParams: ReactXPRawRequestParams) ->
        let existingOptions =
            match rawRequestParams.MaybeOptions with
            | None         -> createEmpty
            | Some options -> options

        let existingSendData = existingOptions?sendData

        let result =
            if regex.IsMatch rawRequestParams.Url && not (isNullOrUndefined existingSendData) && existingSendData?length >= minPayloadSizeToEncodeBytes then
                let existingAugmentHeaders =
                    if isNullOrUndefined existingOptions?augmentHeaders then
                        createEmpty
                    else
                        existingOptions?augmentHeaders

                let updatedAugmentHeaders =
                    Fable.Core.JS.Constructors.Object.assign (
                        createEmpty,
                        existingAugmentHeaders,
                        createObj [
                            "X-Content-Encoding" ==> "deflate"
                        ]
                    )

                let compressedSendData = pako?deflateRaw (existingOptions?sendData)

                let updatedOptions =
                    Fable.Core.JS.Constructors.Object.assign (
                        createEmpty,
                        existingOptions,
                        createObj [
                            "augmentHeaders" ==> updatedAugmentHeaders
                            "sendData"       ==> compressedSendData
                            "contentType"    ==> "application/deflatedJson"
                        ]
                    )

                { rawRequestParams with
                    MaybeOptions = Some updatedOptions
                }

            else
                rawRequestParams

        Async.Of result
