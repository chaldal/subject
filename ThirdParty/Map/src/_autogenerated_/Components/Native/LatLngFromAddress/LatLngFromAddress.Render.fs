module ThirdParty.Map.Components.Native.LatLngFromAddressRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard


open LibClient.Components
open ReactXP.Components
open ThirdParty.Map.Components

open LibClient
open LibClient.RenderHelpers
open ThirdParty.Map.LocalImages

open ThirdParty.Map.Components.Native.LatLngFromAddress



let render(children: array<ReactElement>, props: ThirdParty.Map.Components.Native.LatLngFromAddress.Props, estate: ThirdParty.Map.Components.Native.LatLngFromAddress.Estate, pstate: ThirdParty.Map.Components.Native.LatLngFromAddress.Pstate, actions: ThirdParty.Map.Components.Native.LatLngFromAddress.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "LibClient.Components.Subscribe"
    LibClient.Components.Constructors.LC.Subscribe(
        ``with`` = (LibClient.Components.Subscribe.Raw props.With),
        subscription = (subscriptionImplementationValue.Subscribe)
    )
