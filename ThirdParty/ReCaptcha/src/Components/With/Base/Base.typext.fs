module ThirdParty.ReCaptcha.Components.With.Base

open LibClient

type Props = (* GenerateMakeFunction *) {
    With:    (string -> Async<Result<NonemptyString, exn>>) -> ReactElement
    SiteKey: string
    BaseUrl: string
    key:     string option // defaultWithAutoWrap JsUndefined
}

type Base(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Base>("ThirdParty.ReCaptcha.Components.With.Base", _initialProps, Actions, hasStyles = false)

and Actions(_this: Base) =
    class end

let Make = makeConstructor<Base, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate
type Estate = NoEstate