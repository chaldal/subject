module ThirdParty.Map.Components.Web.MapRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard


open LibClient.Components
open ReactXP.Components
open ThirdParty.Map.Components

open LibClient
open LibClient.RenderHelpers
open ThirdParty.Map.LocalImages

open ThirdParty.Map.Components.Web.Map



let render(children: array<ReactElement>, props: ThirdParty.Map.Components.Web.Map.Props, estate: ThirdParty.Map.Components.Web.Map.Estate, pstate: ThirdParty.Map.Components.Web.Map.Pstate, actions: ThirdParty.Map.Components.Web.Map.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "LibClient.Components.With.ScreenSize"
    LibClient.Components.Constructors.LC.With.ScreenSize(
        ``with`` =
            (fun (screenSize) ->
                    (castAsElementAckingKeysWarning [|
                        #if EGGSHELL_PLATFORM_IS_WEB
                        let __parentFQN = Some "ReactXP.Components.View"
                        let __currClass = (System.String.Format("{0}{1}{2}", "view ", (TopLevelBlockClass), ""))
                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                        ReactXP.Components.Constructors.RX.View(
                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                            children =
                                [|
                                    let __parentFQN = Some "LibClient.Components.With.RefDom"
                                    LibClient.Components.Constructors.LC.With.RefDom(
                                        onInitialize = (actions.OnMapAnchorLoaded),
                                        ``with`` =
                                            (fun (bindDivRef, _) ->
                                                    (castAsElementAckingKeysWarning [|
                                                        (
                                                            let isFullScreen = match props.FullScreen with | Some f -> f | None -> false
                                                            FRS.div
                                                                [(FRP.Ref ((bindDivRef))); (FRP.ClassName ((System.String.Format("{0}{1}{2}", "map-anchor ", (screenSize.Class), "")) + System.String.Format(" {0}", (if (not isFullScreen) then "cookups-hack-fixed-size" else ""))))]
                                                                [||]
                                                        )
                                                        (*  TODO handle error cases: error while loading map, delay in map loading etc  *)
                                                    |])
                                            )
                                    )
                                |]
                        )
                        #else
                        let __parentFQN = Some "ReactXP.Components.View"
                        ReactXP.Components.Constructors.RX.View()
                        #endif
                    |])
            )
    )
