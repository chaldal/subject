module ThirdParty.ReCaptcha.Components.With.Native

open LibClient
open ReactXP.Components.WebView

type Props = (* GenerateMakeFunction *) {
    SiteKey: string
    BaseUrl: string
    With:    (string -> Async<Result<NonemptyString, exn>>) -> ReactElement
    key:     string option // defaultWithAutoWrap JsUndefined
}

type Estate = {
    MaybeBrowserRef: Option<ReactXP.Components.WebView.WebViewMethods>
}

type Native(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, Native>("ThirdParty.ReCaptcha.Components.With.Native", _initialProps, Actions, hasStyles = false)

    override _.GetInitialEstate(_initialProps: Props) : Estate = {
        MaybeBrowserRef = None
    }

and Actions(this: Native) =
    let bound = {|
        RefWebview = fun (nullableInstance: LibClient.JsInterop.JsNullable<ReactXP.Components.WebView.WebViewMethods>) ->
            this.SetEstate (fun estate _ -> { estate with MaybeBrowserRef = nullableInstance.ToOption })
        |}

    let mutable deferred = LibLangFsharp.Deferred<Result<NonemptyString, exn>>()

    member _.Bound = bound

    member _.getToken (action: string): Async<Result<NonemptyString, exn>> =
        this.state.MaybeBrowserRef |> Option.sideEffect (fun webviewRef -> webviewRef.postMessage action)
        deferred.Value

    member _.onMessageHandler (e: WebViewMessageEvent) =
        if deferred.IsPending then
            match NonemptyString.ofString e.data with
            | Some nonemptyToken ->
                Ok nonemptyToken
                |> deferred.Resolve
            | None -> (failwith "empty token") |> deferred.Reject

    member _.source (siteKey: string) =
        sprintf "<!DOCTYPE html><html><head>
        <script src=\"https://www.google.com/recaptcha/api.js?render=%s\"></script>
        <script>

            function getRecaptchaToken(siteKey, action) {
                window.grecaptcha.ready(function() {
                    window.grecaptcha.execute(siteKey, { action: action }).then(
                        function(args) {
                            window.ReactNativeWebView.postMessage(args);
                        }
                    )
                })
            }

            document.addEventListener(\"message\",(event)=>{
                getRecaptchaToken(\"%s\", event.data)
            })
        </script>
        </head></html>" siteKey siteKey

let Make = makeConstructor<Native, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate
