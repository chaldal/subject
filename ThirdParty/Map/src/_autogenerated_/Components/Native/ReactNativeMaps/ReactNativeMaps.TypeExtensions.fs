namespace ThirdParty.Map.Components

open LibClient
open System
open Fable.Core.JsInterop
open ThirdParty.Map.Types
open ThirdParty.Map.Components.Native.ReactNativeMaps
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Native_ReactNativeMapsTypeExtensions =
    type ThirdParty.Map.Components.Constructors.Map.Native with
        static member ReactNativeMaps(size: Option<(* width *) int * (* height *) int>, zoom: Option<int>, value: LatLng, onChange: Option<LatLng> -> unit, ref: IRefReactNativeMapView -> unit, ?children: ReactChildrenProp, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Size = size
                    Zoom = zoom
                    Value = value
                    OnChange = onChange
                    Ref = ref
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            ThirdParty.Map.Components.Native.ReactNativeMaps.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            