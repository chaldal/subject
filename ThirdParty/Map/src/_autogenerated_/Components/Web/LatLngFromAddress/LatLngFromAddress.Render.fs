module ThirdParty.Map.Components.Web.LatLngFromAddressRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard


open LibClient.Components
open ReactXP.Components
open ThirdParty.Map.Components

open LibClient
open LibClient.RenderHelpers
open ThirdParty.Map.LocalImages

open ThirdParty.Map.Components.Web.LatLngFromAddress



let render(children: array<ReactElement>, props: ThirdParty.Map.Components.Web.LatLngFromAddress.Props, estate: ThirdParty.Map.Components.Web.LatLngFromAddress.Estate, pstate: ThirdParty.Map.Components.Web.LatLngFromAddress.Pstate, actions: ThirdParty.Map.Components.Web.LatLngFromAddress.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "LibClient.Components.With.RefDom"
    LibClient.Components.Constructors.LC.With.RefDom(
        onInitialize = (actions.OnMapAnchorLoaded),
        ``with`` =
            (fun (bindDivRef, _) ->
                    (castAsElementAckingKeysWarning [|
                        FRS.div
                            [(FRP.Ref ((bindDivRef)))]
                            [||]
                        let __parentFQN = Some "LibClient.Components.Subscribe"
                        LibClient.Components.Constructors.LC.Subscribe(
                            ``with`` = (LibClient.Components.Subscribe.Raw props.With),
                            subscription = (subscriptionImplementationValue.Subscribe)
                        )
                    |])
            )
    )
