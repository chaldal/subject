namespace ThirdParty.Map.Components

open LibClient
open ThirdParty.Map.Components.With.LatLng
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module With_LatLngTypeExtensions =
    type ThirdParty.Map.Components.Constructors.Map.With with
        static member LatLng(address: string, ``with``: AsyncData<LatLngType> -> ReactElement, apiKey: string, ?children: ReactChildrenProp, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Address = address
                    With = ``with``
                    ApiKey = apiKey
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            ThirdParty.Map.Components.With.LatLng.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            