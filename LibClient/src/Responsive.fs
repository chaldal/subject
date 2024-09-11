module LibClient.Responsive

open Fable.React
open Fable.Core.JsInterop

[<RequireQualifiedAccess>]
type ScreenSize =
| Handheld
| Desktop
with
    member this.Class =
        match this with
        | Handheld -> "screen-size-handheld"
        | Desktop  -> "screen-size-desktop"

    member this.Name =
        match this with
        | Handheld -> "handheld"
        | Desktop  -> "desktop"

let mutable private onScreenSizeUpdatedListeners: List<System.Action> = []

let private screenSizeOfWidth (width: int) : ScreenSize =
    // we are using this arbitrary value to determine whether we are on native or web
    // however for larger tablets (width > 900px) we might fall back to "desktop" mode
    // in such case, DOM elements (i.e. not ReactXP components) will fail
    match width < 900 with
    | true  -> ScreenSize.Handheld
    | false -> ScreenSize.Desktop

let mutable private latestScreenSize: ScreenSize = screenSizeOfWidth (ReactXP.UserInterface.windowLayoutInfo()).width

let getLatestScreenSize () : ScreenSize =
    latestScreenSize

let screenSizeContext = Fable.React.ReactBindings.React.createContext latestScreenSize
screenSizeContext?displayName <- "ScreenSizeContext"

let screenSizeContextProvider: ScreenSize -> ReactElements -> ReactElement = contextProvider screenSizeContext

let screenSizeOnLayout (e: ReactXP.Types.ViewOnLayoutEvent) : unit =
    let newScreenSize = screenSizeOfWidth e.width

    if newScreenSize <> latestScreenSize then
        latestScreenSize <- newScreenSize
        onScreenSizeUpdatedListeners
        |> List.iter (fun listener -> listener.Invoke())

let addOnScreenSizeUpdatedListener (listener: System.Action) : {| Off: unit -> unit |} =
    onScreenSizeUpdatedListeners <- listener :: onScreenSizeUpdatedListeners
    {|
        Off = fun () -> onScreenSizeUpdatedListeners <- onScreenSizeUpdatedListeners |> List.without listener
    |}
