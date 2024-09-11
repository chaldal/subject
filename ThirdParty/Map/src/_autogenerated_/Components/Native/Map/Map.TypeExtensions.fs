namespace ThirdParty.Map.Components

open LibClient
open ReactXP.Styles
open ThirdParty.Map.Types
open LibClient.Services.ImageService
open Fable.Core.JsInterop
open LibClient.LocalImages
open ThirdParty.Map.Components.Native.Map
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Native_MapTypeExtensions =
    type ThirdParty.Map.Components.Constructors.Map.Native with
        static member Map(apiKey: string, value: Option<LatLng>, zoom: Option<int>, onChange: Option<LatLng> -> unit, fullScreen: bool, markers: Option<List<Marker>>, shapes: Option<List<Shape>>, ref: IRefReactNativeMapView -> unit, ?children: ReactChildrenProp, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    ApiKey = apiKey
                    Value = value
                    Zoom = zoom
                    OnChange = onChange
                    FullScreen = fullScreen
                    Markers = markers
                    Shapes = shapes
                    Ref = ref
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            ThirdParty.Map.Components.Native.Map.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            