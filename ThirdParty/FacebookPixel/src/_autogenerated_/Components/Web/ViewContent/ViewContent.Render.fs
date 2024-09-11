module ThirdParty.FacebookPixel.Components.Web.ViewContentRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard


open LibClient.Components
open ReactXP.Components
open ThirdParty.FacebookPixel.Components

open LibClient

open ThirdParty.FacebookPixel.Components.Web.ViewContent



let render(children: array<ReactElement>, props: ThirdParty.FacebookPixel.Components.Web.ViewContent.Props, estate: ThirdParty.FacebookPixel.Components.Web.ViewContent.Estate, pstate: ThirdParty.FacebookPixel.Components.Web.ViewContent.Pstate, actions: ThirdParty.FacebookPixel.Components.Web.ViewContent.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "ReactXP.Components.View"
    ReactXP.Components.Constructors.RX.View()
