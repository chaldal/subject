namespace ThirdParty.Map.Components

open LibClient
open ReactXP.Styles
open ThirdParty.Map.Components.Base
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module BaseTypeExtensions =
    type ThirdParty.Map.Components.Constructors.Map with
        static member Base(apiKey: string, ?children: ReactChildrenProp, ?position: MapPosition, ?onPositionChanged: MapPosition -> unit, ?zoom: int, ?markers: List<Marker>, ?shapes: List<Shape>, ?fullScreen: bool, ?backgroundColor: string, ?clickableIcons: bool, ?disableDefaultUI: bool, ?minZoom: float, ?maxZoom: float, ?mapStyles: List<MapStyle>, ?mapTypeId: MapTypeId, ?directions: List<Directions>, ?onLocatePress: ReactEvent.Action -> LocateToConfig, ?styles: array<ViewStyles>, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    ApiKey = apiKey
                    Position = defaultArg position (MapPosition.Auto)
                    OnPositionChanged = onPositionChanged |> Option.orElse (JsUndefined)
                    Zoom = zoom |> Option.orElse (JsUndefined)
                    Markers = markers |> Option.orElse (JsUndefined)
                    Shapes = shapes |> Option.orElse (JsUndefined)
                    FullScreen = fullScreen |> Option.orElse (JsUndefined)
                    BackgroundColor = backgroundColor |> Option.orElse (JsUndefined)
                    ClickableIcons = clickableIcons |> Option.orElse (JsUndefined)
                    DisableDefaultUI = disableDefaultUI |> Option.orElse (JsUndefined)
                    MinZoom = minZoom |> Option.orElse (JsUndefined)
                    MaxZoom = maxZoom |> Option.orElse (JsUndefined)
                    MapStyles = mapStyles |> Option.orElse (JsUndefined)
                    MapTypeId = mapTypeId |> Option.orElse (JsUndefined)
                    Directions = directions |> Option.orElse (JsUndefined)
                    OnLocatePress = onLocatePress |> Option.orElse (None)
                    styles = styles |> Option.orElse (None)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            ThirdParty.Map.Components.Base.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            