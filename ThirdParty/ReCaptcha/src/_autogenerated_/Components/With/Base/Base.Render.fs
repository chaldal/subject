module ThirdParty.ReCaptcha.Components.With.BaseRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard


open LibClient.Components
open ReactXP.Components
open ThirdParty.ReCaptcha.Components

open LibClient

open ThirdParty.ReCaptcha.Components.With.Base
open Fable.Core.JsInterop
open LibClient.EggShellReact


let render(children: array<ReactElement>, props: ThirdParty.ReCaptcha.Components.With.Base.Props, estate: ThirdParty.ReCaptcha.Components.With.Base.Estate, pstate: ThirdParty.ReCaptcha.Components.With.Base.Pstate, actions: ThirdParty.ReCaptcha.Components.With.Base.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    (castAsElementAckingKeysWarning [|
        #if EGGSHELL_PLATFORM_IS_WEB
        let __parentFQN = Some "ThirdParty.ReCaptcha.Components.With.Web"
        ThirdParty.ReCaptcha.Components.Constructors.ReCaptcha.With.Web(
            siteKey = (props.SiteKey),
            ``with`` =
                (fun (reCaptchaTokenGetter) ->
                        (castAsElementAckingKeysWarning [|
                            props.With reCaptchaTokenGetter
                        |])
                )
        )
        #else
        let __parentFQN = Some "ThirdParty.ReCaptcha.Components.With.Native"
        ThirdParty.ReCaptcha.Components.Constructors.ReCaptcha.With.Native(
            baseUrl = (props.BaseUrl),
            siteKey = (props.SiteKey),
            ``with`` =
                (fun (reCaptchaTokenGetter) ->
                        (castAsElementAckingKeysWarning [|
                            props.With reCaptchaTokenGetter
                        |])
                )
        )
        #endif
    |])
