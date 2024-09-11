module LibClient.Services.HttpService.ReactXPHttp

open Fable.Core
open Fable.Core.JsInterop

open LibClient.JsInterop

// I feel comfortable leaving this untyped, we're wrapping over it with our HttpService,
// so only maintainers of the library ever need to interact with this untyped blob
let ReactXPSimpleWebRequest: obj = Fable.Core.JsInterop.import "SimpleWebRequest" "simplerestclients"

[<StringEnum>]
type HttpAction =
| [<CompiledName("POST")>]   Post
| [<CompiledName("GET")>]    Get
| [<CompiledName("PUT")>]    Put
| [<CompiledName("DELETE")>] Delete
| [<CompiledName("PATCH")>]  Patch

type WebRequestPriority =
| DontCare = 0
| Low      = 1
| Normal   = 2
| High     = 3
| Critical = 4

// export type SendDataType = Params | string | NativeFileData;
type SendDataType = unit

type XMLHttpRequestResponseType = unit

//export interface XMLHttpRequestProgressEvent extends ProgressEvent {
type XMLHttpRequestProgressEvent = {
    lengthComputable: bool
    loaded:           int
    path:             seq<string>
    percent:          int
    position:         int
    total:            int
    totalSize:        int
}

// export interface Headers extends Dictionary<string> {}
type Headers = obj // JS map<string, string>

type SimpleWebRequestBase = obj

// hack derived this from an interface hierarchy; if some necessary
// runtime fields are missing, feel free to add
type WebErrorResponse = {
    body:     obj
    canceled: bool
    timedOut: bool
}

// One of those TS numeric enums with no explicit start value, be careful.
type ErrorHandlingType =
// Ignore retry policy, if any, and fail immediately
| DoNotRetry = 0

// Retry immediately, without counting it as a failure (used when you've made some sort of change to the )
| RetryUncountedImmediately = 1

// Retry with exponential backoff, but don't count it as a failure (for 429 handling)
| RetryUncountedWithBackoff = 2

// Use standard retry policy (count it as a failure, exponential backoff as policy dictates)
| RetryCountedWithBackoff = 3

// Return this if you need to satisfy some condition before this request will retry (then call .resumeRetrying()).
| PauseUntilResumed = 4

type WebRequestOptions = {
    withCredentials:    bool                          option // defaultWithAutoWrap JsUndefined
    retries:            int                           option // defaultWithAutoWrap JsUndefined
    priority:           WebRequestPriority            option // defaultWithAutoWrap JsUndefined
    timeout:            int                           option // defaultWithAutoWrap JsUndefined
    acceptType:         string                        option // defaultWithAutoWrap JsUndefined
    customResponseType: XMLHttpRequestResponseType    option // defaultWithAutoWrap JsUndefined
    contentType:        string                        option // defaultWithAutoWrap JsUndefined
    sendData:           SendDataType                  option // defaultWithAutoWrap JsUndefined

    // Used instead of calling getHeaders.
    overrideGetHeaders: Headers                       option // defaultWithAutoWrap JsUndefined
    // Overrides all other headers.
    augmentHeaders:     Headers                       option // defaultWithAutoWrap JsUndefined

    streamingDownloadProgress: (string -> unit)                                                 option // defaultWithAutoWrap JsUndefined
    onProgress:                (XMLHttpRequestProgressEvent -> unit)                            option // defaultWithAutoWrap JsUndefined
    customErrorHandler:        (SimpleWebRequestBase -> WebErrorResponse -> ErrorHandlingType)  option // defaultWithAutoWrap JsUndefined
    augmentErrorResponse:      (WebErrorResponse -> unit)                                       option // defaultWithAutoWrap JsUndefined
}

type WebResponse = {
    body:           obj
    headers:        Headers
    requestHeaders: Headers
    statusCode:     int
    statusText:     string
    url:            string
    method:         string
}
