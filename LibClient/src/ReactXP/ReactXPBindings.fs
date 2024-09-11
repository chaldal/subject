namespace ReactXP

open Fable.Core.JsInterop
open Browser.Types

module Helpers =
    let ReactXPRaw: obj = import "*" "@chaldal/reactxp"

    let extractProp<'T when 'T : null> (key: string) (props: obj) : Option<'T> =
        let value: 'T = props?(key)
        match isNull value with
        | true  -> None
        | false -> Some value

type NativePlatform =
| Android
| IOS

type OS =
| Windows
| Linux
| Mac
| Android
| IOS
| Other

type Platform =
| Web    of OS
| Native of NativePlatform

module Runtime =
    let platform : Platform =
        match Helpers.ReactXPRaw?Platform?getType() with
        | "web" ->
            let isWindows: bool = import "windows" "platform-detect"
            let isLinux:   bool = import "linux"   "platform-detect"
            let isMac:     bool = import "macos"   "platform-detect"
            let isAndroid: bool = import "android" "platform-detect"
            let isIOS:     bool = import "ios"     "platform-detect"

            match (isWindows, isLinux, isMac, isAndroid, isIOS) with
            | (true, _, _, _, _) -> Web OS.Windows
            | (_, true, _, _, _) -> Web OS.Linux
            | (_, _, true, _, _) -> Web OS.Mac
            | (_, _, _, true, _) -> Web OS.Android
            | (_, _, _, _, true) -> Web OS.IOS
            | _                  -> Web OS.Other

        | "android" -> Native NativePlatform.Android
        | "ios"     -> Native NativePlatform.IOS
        | _         -> failwithf "Unsupported platform %s" (Helpers.ReactXPRaw?Platform?getType())

    let ifWeb (f: Document -> unit) : unit =
        match platform with
        | Web _ -> f Browser.Dom.document
        | _     -> Noop

    let isWeb () : bool =
        match platform with
        | Web _ -> true
        | _     -> false

    let isDesktopWeb () : bool =
        match platform with
        | Web OS.Windows
        | Web OS.Linux
        | Web OS.Mac -> true
        | _          -> false

    let isNative () : bool =
        match platform with
        | Native _ -> true
        | _        -> false


module UserInterface =
    let windowLayoutInfo () : ReactXP.Types.ViewOnLayoutEvent =
        Helpers.ReactXPRaw?UserInterface?measureWindow()

    let pixelDensity () : float =
        if Helpers.ReactXPRaw?UserInterface?isHighPixelDensityScreen() then
            // it's pretty lame that all we get is a boolean... oh well
            2.0
        else
            1.0

    let dismissKeyboard () : unit =
        Helpers.ReactXPRaw?UserInterface?dismissKeyboard()


module Linking =
    open Fable.Core

    let getInitialUrl () : Async<Option<string>> = async {
        let! maybeRawUrl = Helpers.ReactXPRaw?Linking?getInitialUrl() |> Async.AwaitPromise
        return maybeRawUrl |> Option.map string
    }

    let openUrl (url: string) : unit =
        Helpers.ReactXPRaw?Linking?openUrl(url)

    let deepLinkRequestEvent (callback: string -> unit) : unit =
        Helpers.ReactXPRaw?Linking?deepLinkRequestEvent?subscribe (fun maybeUrl -> maybeUrl |> Option.map (fun url -> url |> callback))


module Clipboard =
    let setText (text: string) : unit =
        Helpers.ReactXPRaw?Clipboard?setText(text)