module LibClient.Polyfill.AbortController

open ReactXP
open Fable.Core
open Fable.Core.JsInterop
open LibClient.JsInterop

[<Global>]
let private AbortController: obj = jsNative

let initialize () : unit =
    if isUndefined AbortController && Runtime.isNative() then
        importDefault "abortcontroller-polyfill/dist/polyfill-patch-fetch"