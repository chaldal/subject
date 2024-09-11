module LibRouter.Components.Router

open Fable.Core
open Fable.Core.JsInterop
open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap LibClient.JsInterop.Undefined
    initialEntries: array<string> option // defaultWithAutoWrap LibClient.JsInterop.Undefined
    future: obj // default Some (createObj ["v7_startTransition" ==> true])
}

type Location = {
    pathname: string
    search:   string
    hash:     string
    key:      string
} with
    member this.Url : string =
        this.pathname + this.search

// I couldn't find a typing for the navigate object to force Fable to call it
// with two separate parameters — it would always try to wrap them in an array.
// Using ".call" also didn't work, because it's apparently not a straight-up
// function, but some complexly wrapped hook function.
[<Emit("$0($1, {replace: true})")>]
let private rawReplace (_navigate: obj, _url: string): unit = jsNative

type Navigate(navigate: obj) =
    member _.Push (url: string): unit =
        (navigate :?> string -> unit) url

    member this.Replace (url: string): unit =
        rawReplace (navigate, url)

    member _.GoBack (): unit =
        (navigate :?> int -> unit) -1


let (Router, useLocation, useNavigate): obj * (unit -> Location) * (unit -> Navigate) =
    (
        let (router, useLocation, useNavigateRaw) =
            #if EGGSHELL_PLATFORM_IS_WEB
            import "BrowserRouter" "react-router-dom",
            import "useLocation"   "react-router-dom",
            import "useNavigate"   "react-router-dom"
            #else
            import "NativeRouter"  "react-router-native",
            import "useLocation"   "react-router-native",
            import "useNavigate"   "react-router-native"
            #endif

        (router, useLocation, fun () -> Navigate(useNavigateRaw()))
    )

let Make = LibClient.ThirdParty.wrapComponent<Props>(Router)
