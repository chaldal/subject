[<AutoOpen>]
module ReactXP.Components.WebView

open LibClient.JsInterop

open ReactXP.Types

open Fable.Core.JsInterop
open Browser.Types

type WebViewSource = {
    html:    string
    baseUrl: string option
}

type WebViewNavigationState = {
    canGoBack:    bool
    canGoForward: bool
    loading:      bool
    url:          string
    title:        string
}

type WebViewShouldStartLoadEvent = {
    url: string
}

type WebViewMessageEvent (* extends Event *) = {
    data:   string
    origin: string
}

// The TypeScript definitions used the bit shift syntax that's
// in the comments on every line. Can't use it in F#.
type WebViewSandboxMode =
| None                               =    0 // 0
| AllowForms                         =    1 // 1 << 0
| AllowModals                        =    2 // 1 << 1
| AllowOrientationLock               =    4 // 1 << 2
| AllowPointerLock                   =    8 // 1 << 3
| AllowPopups                        =   16 // 1 << 4
| AllowPopupsToEscapeSandbox         =   32 // 1 << 5
| AllowPresentation                  =   64 // 1 << 6
| AllowSameOrigin                    =  128 // 1 << 7
| AllowScripts                       =  256 // 1 << 8
| AllowTopNavigation                 =  512 // 1 << 9
| AllowMixedContentAlways            = 1024 // 1 << 10
| AllowMixedContentCompatibilityMode = 2048 // 1 << 11

module WebViewSandboxMode =
    let ofList (modes: List<WebViewSandboxMode>) : WebViewSandboxMode =
        let combined =
            modes
            |> List.reduce (fun a b -> a ||| b)
        combined :> obj :?> WebViewSandboxMode

type WebViewMethods = {
    goBack:      unit     -> unit
    goForward:   unit     -> unit
    reload:      unit     -> unit
    postMessage: (string) -> unit
}

let ReactXPWebView: obj = Fable.Core.JsInterop.importDefault "@chaldal/reactxp-webview"

type ReactXP.Components.Constructors.RX with
    static member WebView(
        ?url:                             string,
        ?source:                          WebViewSource,
        ?headers:                         Headers,
        ?onLoad:                          Event -> unit,
        ?onNavigationStateChange:         WebViewNavigationState -> unit,
        ?scalesPageToFit:                 bool,
        ?injectedJavaScript:              string,
        ?javaScriptEnabled:               bool,
        ?mediaPlaybackRequiresUserAction: bool,
        ?allowsInlineMediaPlayback:       bool,
        ?startInLoadingState:             bool,
        ?domStorageEnabled:               bool,
        ?onShouldStartLoadWithRequest:    WebViewShouldStartLoadEvent -> bool,
        ?onLoadStart:                     Event -> unit,
        ?onError:                         Event -> unit,
        ?onMessage:                       WebViewMessageEvent -> unit,
        ?sandbox:                         WebViewSandboxMode,
        ?ref:                             JsNullable<WebViewMethods> -> unit,
        ?styles:                          array<ReactXP.Styles.FSharpDialect.ViewStyles>,
        ?xLegacyStyles:                   List<ReactXP.LegacyStyles.RuntimeStyles>
    ) =
        let __props = createEmpty

        __props?url                             <- url
        __props?source                          <- source
        __props?headers                         <- headers
        __props?onLoad                          <- onLoad
        __props?onNavigationStateChange         <- onNavigationStateChange
        __props?scalesPageToFit                 <- scalesPageToFit
        __props?injectedJavaScript              <- injectedJavaScript
        __props?javaScriptEnabled               <- javaScriptEnabled
        __props?mediaPlaybackRequiresUserAction <- mediaPlaybackRequiresUserAction
        __props?allowsInlineMediaPlayback       <- allowsInlineMediaPlayback
        __props?startInLoadingState             <- startInLoadingState
        __props?domStorageEnabled               <- domStorageEnabled
        __props?onShouldStartLoadWithRequest    <- onShouldStartLoadWithRequest
        __props?onLoadStart                     <- onLoadStart
        __props?onError                         <- onError
        __props?onMessage                       <- onMessage
        __props?sandbox                         <- sandbox
        __props?ref                             <- ref
        __props?style                           <- styles

        match xLegacyStyles with
        | Option.None | Option.Some [] -> ()
        | Option.Some styles -> __props?__style <- styles

        Fable.React.ReactBindings.React.createElement(
            ReactXPWebView,
            __props,
            [||]
        )
