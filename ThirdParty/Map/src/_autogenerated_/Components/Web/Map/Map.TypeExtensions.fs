namespace ThirdParty.Map.Components

open LibClient
open LibClient.JsInterop
open ThirdParty.Map
open Fable.Core
open Fable.Core.JsInterop
open Fable.React
open ThirdParty.Map.Components.Web.Map
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Web_MapTypeExtensions =
    type ThirdParty.Map.Components.Constructors.Map.Web with
        static member Map(apiKey: string, position: MapPosition, onPositionChanged: Option<MapPosition -> unit>, zoom: Option<int>, markers: Option<List<Marker>>, shapes: Option<List<Shape>>, directions: Option<List<Directions>>, backgroundColor: Option<string>, clickableIcons: Option<bool>, disableDefaultUI: Option<bool>, minZoom: Option<float>, maxZoom: Option<float>, fullScreen: Option<bool>, mapStyles: Option<List<MapStyle>>, mapTypeId: Option<MapTypeId>, ref: IWebMapViewRef -> unit, ?children: ReactChildrenProp, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    ApiKey = apiKey
                    Position = position
                    OnPositionChanged = onPositionChanged
                    Zoom = zoom
                    Markers = markers
                    Shapes = shapes
                    Directions = directions
                    BackgroundColor = backgroundColor
                    ClickableIcons = clickableIcons
                    DisableDefaultUI = disableDefaultUI
                    MinZoom = minZoom
                    MaxZoom = maxZoom
                    FullScreen = fullScreen
                    MapStyles = mapStyles
                    MapTypeId = mapTypeId
                    Ref = ref
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            ThirdParty.Map.Components.Web.Map.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            