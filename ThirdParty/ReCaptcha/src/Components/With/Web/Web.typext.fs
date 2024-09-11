module ThirdParty.ReCaptcha.Components.With.Web

open LibClient

open Fable.Core
open Fable.Core.JsInterop
open Fable.Core.JS


type ReCaptchaInstance =
    abstract execute: string -> Promise<string>

type Props = (* GenerateMakeFunction *) {
    With:    (string -> Async<Result<NonemptyString, exn>>) -> ReactElement
    SiteKey: string
    key:     string  option // defaultWithAutoWrap JsUndefined
}

#if EGGSHELL_PLATFORM_IS_WEB
let private ReCaptchaLoader (_: string) (_: obj): Promise<ReCaptchaInstance> = import "load" "recaptcha-v3"


let private load (siteKey: string) : Async<ReCaptchaInstance> =
    async {
        return! ReCaptchaLoader siteKey (createObj ["autoHideBadge" ==> true]) |> Async.AwaitPromise
    }

let private execute (siteKey: string) (action: string) =
    async {
        let! reCaptcha = load siteKey
        return! reCaptcha.execute action |> Async.AwaitPromise
    }

type Estate = {
    dummyStateSoThatICanUseComponentDidMount: Option<string>
}

type Web(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, Web>("ThirdParty.ReCaptcha.Components.With.Web", _initialProps, Actions, hasStyles = false)

    override _.GetInitialEstate(_initialProps: Props) : Estate = {
        dummyStateSoThatICanUseComponentDidMount = None
    }

    override this.ComponentDidMount() : unit =
        load this.props.SiteKey |> Async.Ignore |> startSafely

and Actions(this: Web) =
    member _.getToken (action: string): Async<Result<NonemptyString, exn>> =
        async {
            try
                let! token = execute this.props.SiteKey action
                return
                    match NonemptyString.ofString token with
                    | Some nonemptyTokenString -> Ok nonemptyTokenString
                    | None -> Error (failwith "empty token")
            with e ->
                return Error e
        }
let Make = makeConstructor<Web, _, _>
#else
type Web(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Web>("ThirdParty.ReCaptcha.Components.With.Web", _initialProps, Actions, hasStyles = false)

and Actions(_this: Web) =
    member _.getToken (_: string): Async<Result<NonemptyString, exn>> =
        failwith "This shouldn't be called from native code"

let Make = makeConstructor<Web, _, _>
type Estate = NoEstate
#endif

// Unfortunately necessary boilerplate
type Pstate = NoPstate
