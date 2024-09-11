module ThirdParty.ReCaptcha.Components.With.NativeRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard


open LibClient.Components
open ReactXP.Components
open ThirdParty.ReCaptcha.Components

open LibClient

open ThirdParty.ReCaptcha.Components.With.Native
open ReactXP.Components.WebView


let render(children: array<ReactElement>, props: ThirdParty.ReCaptcha.Components.With.Native.Props, estate: ThirdParty.ReCaptcha.Components.With.Native.Estate, pstate: ThirdParty.ReCaptcha.Components.With.Native.Pstate, actions: ThirdParty.ReCaptcha.Components.With.Native.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    (castAsElementAckingKeysWarning [|
        let __parentFQN = Some "ReactXP.Components.WebView"
        ReactXP.Components.Constructors.RX.WebView(
            onMessage = (actions.onMessageHandler),
            source = ({ html = actions.source (props.SiteKey); baseUrl = Some props.BaseUrl }),
            ref = (actions.Bound.RefWebview),
            javaScriptEnabled = (true)
        )
        props.With (actions.getToken)
    |])
