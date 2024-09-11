module ThirdParty.Map.Components.BaseRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard


open LibClient.Components
open ReactXP.Components
open ThirdParty.Map.Components

open LibClient
open LibClient.RenderHelpers
open ThirdParty.Map.LocalImages

open ThirdParty.Map.Components.Base



let render(children: array<ReactElement>, props: ThirdParty.Map.Components.Base.Props, estate: ThirdParty.Map.Components.Base.Estate, pstate: ThirdParty.Map.Components.Base.Pstate, actions: ThirdParty.Map.Components.Base.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "ReactXP.Components.View"
    let __currClass = (System.String.Format("{0}{1}{2}", "view ", (TopLevelBlockClass), ""))
    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
    ReactXP.Components.Constructors.RX.View(
        ?styles =
            (
                let __currProcessedStyles = if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "%s" __currStyles) else [||]
                match props.styles with
                | Some styles ->
                    Array.append __currProcessedStyles styles |> Some
                | None -> Some __currProcessedStyles
            ),
        children =
            [|
                #if EGGSHELL_PLATFORM_IS_WEB
                let __parentFQN = Some "ThirdParty.Map.Components.Web.Map"
                ThirdParty.Map.Components.Constructors.Map.Web.Map(
                    ref = (actions.WebRef),
                    mapTypeId = (props.MapTypeId),
                    mapStyles = (props.MapStyles),
                    fullScreen = (props.FullScreen),
                    maxZoom = (props.MaxZoom),
                    minZoom = (props.MinZoom),
                    disableDefaultUI = (props.DisableDefaultUI),
                    clickableIcons = (props.ClickableIcons),
                    backgroundColor = (props.BackgroundColor),
                    directions = (props.Directions),
                    shapes = (props.Shapes),
                    markers = (props.Markers),
                    zoom = (props.Zoom),
                    onPositionChanged = (props.OnPositionChanged),
                    position = (props.Position),
                    apiKey = (props.ApiKey)
                )
                #else
                (
                    let isFullScreen = props.FullScreen |> Option.defaultValue false
                    let __parentFQN = Some "ThirdParty.Map.Components.Native.Map"
                    ThirdParty.Map.Components.Constructors.Map.Native.Map(
                        onChange = (fun maybeLatLng -> match props.OnPositionChanged, maybeLatLng with | Some onPositionChanged, Some latLng -> latLng |> MapPosition.LatLng |> onPositionChanged | _ -> ()),
                        ref = (actions.NativeRef),
                        shapes = (props.Shapes),
                        fullScreen = (isFullScreen),
                        markers = (props.Markers),
                        value = (match props.Position with | MapPosition.LatLng latLng -> Some latLng | MapPosition.Auto -> failwith "MapPosition.Auto not supported on native"),
                        zoom = (props.Zoom),
                        apiKey = (props.ApiKey)
                    )
                )
                #endif
                (
                    if (props.OnLocatePress.IsSome) then
                        (castAsElementAckingKeysWarning [|
                            let __parentFQN = Some "ThirdParty.Map.Components.Locate.LocateButtonWrapper"
                            ThirdParty.Map.Components.Constructors.Map.Locate.LocateButtonWrapper(
                                children =
                                    [|
                                        let __parentFQN = Some "ThirdParty.Map.Components.Locate.LocateButton"
                                        ThirdParty.Map.Components.Constructors.Map.Locate.LocateButton(
                                            onPress = (actions.OnLocatePress)
                                        )
                                    |]
                            )
                        |])
                    else noElement
                )
            |]
    )
