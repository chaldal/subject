namespace ThirdParty.Map.Components

open LibClient
open ThirdParty.Map.Types
open Fable.Core.JsInterop
open LibClient.Services.ImageService
open ThirdParty.Map.Components.Native.Polyline
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Native_PolylineTypeExtensions =
    type ThirdParty.Map.Components.Constructors.Map.Native with
        static member Polyline(value: Polyline, ?children: ReactChildrenProp, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Value = value
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            ThirdParty.Map.Components.Native.Polyline.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            