namespace ThirdParty.Map.Components

open LibClient
open LibClient.Services.Subscription
open ThirdParty.Map.Types
open Fable.Core
open Fable.Core.JsInterop
open LibClient.Services.HttpService.HttpService
open LibClient.Services.HttpService.Types
open ThirdParty.Map.Components.Native.LatLngFromAddress
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Native_LatLngFromAddressTypeExtensions =
    type ThirdParty.Map.Components.Constructors.Map.Native with
        static member LatLngFromAddress(address: string, ``with``: AsyncData<LatLng> -> ReactElement, apiKey: string, ?children: ReactChildrenProp, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
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
            ThirdParty.Map.Components.Native.LatLngFromAddress.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            