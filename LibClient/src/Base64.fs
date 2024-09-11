module LibClient.Base64

open Fable.Core
open Fable.Core.JsInterop
open LibClient.JsInterop

[<Global>]
let private btoa : string -> string = jsNative
[<Global>]
let private atob : string -> string = jsNative

[<Emit("global[$0] = $1")>]
let private setGlobal (_key: string) (_value: string->string) : unit = jsNative

let initialize () : unit =
    if isUndefined btoa || isUndefined atob then
        setGlobal "btoa"     (import "encode" "base-64")
        setGlobal "atob"     (import "decode" "base-64")

let encode (binaryString: string) : string =
    btoa (encodeURIComponent binaryString)

let decode (base64String: string) : string =
    decodeURIComponent (atob base64String)

let encodeByteArray (source: byte[]) : string =
    System.Convert.ToBase64String source

let decodeByteArray (base64String: string) : byte[] =
    System.Convert.FromBase64String base64String

let private encodeUrlUnsafeCharacters (value: string) : string =
    value.Replace("/", "-").Replace("+", "_")

let private decodeUrlUnsafeCharacters (value: string) : string =
    value.Replace("-", "/").Replace("_", "+")

let encodeUrlSafe (binaryString: string) : string =
    binaryString
    |> encodeURIComponent
    |> btoa
    |> encodeUrlUnsafeCharacters

let decodeUrlSafe (base64String: string) : string =
    base64String
    |> decodeUrlUnsafeCharacters
    |> atob
    |> decodeURIComponent