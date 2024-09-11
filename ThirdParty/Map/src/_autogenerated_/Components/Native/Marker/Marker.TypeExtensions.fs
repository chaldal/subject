namespace ThirdParty.Map.Components

open LibClient
open ThirdParty.Map.Types
open Fable.Core.JsInterop
open LibClient.Services.ImageService
open ThirdParty.Map.Components.Native.Marker
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Native_MarkerTypeExtensions =
    type ThirdParty.Map.Components.Constructors.Map.Native with
        static member Marker(coordinate: LatLng, draggable: bool, image: ImageSource, ?children: ReactChildrenProp, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Coordinate = coordinate
                    Draggable = draggable
                    Image = image
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            ThirdParty.Map.Components.Native.Marker.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            