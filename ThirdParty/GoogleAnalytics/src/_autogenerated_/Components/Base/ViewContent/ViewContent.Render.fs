module ThirdParty.GoogleAnalytics.Components.Base.ViewContentRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard


open LibClient.Components
open ReactXP.Components
open ThirdParty.GoogleAnalytics.Components

open LibClient

open ThirdParty.GoogleAnalytics.Components.Base.ViewContent



let render(children: array<ReactElement>, props: ThirdParty.GoogleAnalytics.Components.Base.ViewContent.Props, estate: ThirdParty.GoogleAnalytics.Components.Base.ViewContent.Estate, pstate: ThirdParty.GoogleAnalytics.Components.Base.ViewContent.Pstate, actions: ThirdParty.GoogleAnalytics.Components.Base.ViewContent.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "ReactXP.Components.View"
    ReactXP.Components.Constructors.RX.View()
