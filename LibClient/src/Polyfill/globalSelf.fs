module LibClient.Polyfill.GlobalSelf

open ReactXP
open Fable.Core
open Fable.Core.JsInterop
open LibClient.JsInterop

[<Global>]
let private self: obj = jsNative


[<Emit("global.self = global")>]
let private setGlobalSelf () : unit = jsNative

let initialize () : unit =
    if isUndefined self && Runtime.isNative() then
        setGlobalSelf()