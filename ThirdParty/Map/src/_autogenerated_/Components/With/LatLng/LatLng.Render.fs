module ThirdParty.Map.Components.With.LatLngRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard


open LibClient.Components
open ReactXP.Components
open ThirdParty.Map.Components

open LibClient
open LibClient.RenderHelpers
open ThirdParty.Map.LocalImages

open ThirdParty.Map.Components.With.LatLng



let render(children: array<ReactElement>, props: ThirdParty.Map.Components.With.LatLng.Props, estate: ThirdParty.Map.Components.With.LatLng.Estate, pstate: ThirdParty.Map.Components.With.LatLng.Pstate, actions: ThirdParty.Map.Components.With.LatLng.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "ReactXP.Components.View"
    let __currClass = "view"
    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
    ReactXP.Components.Constructors.RX.View(
        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
        children =
            [|
                #if EGGSHELL_PLATFORM_IS_WEB
                let __parentFQN = Some "ThirdParty.Map.Components.Web.LatLngFromAddress"
                ThirdParty.Map.Components.Constructors.Map.Web.LatLngFromAddress(
                    ``with`` = (props.With),
                    apiKey = (props.ApiKey),
                    address = (props.Address)
                )
                #else
                let __parentFQN = Some "ThirdParty.Map.Components.Native.LatLngFromAddress"
                ThirdParty.Map.Components.Constructors.Map.Native.LatLngFromAddress(
                    ``with`` = (props.With),
                    apiKey = (props.ApiKey),
                    address = (props.Address)
                )
                #endif
            |]
    )
