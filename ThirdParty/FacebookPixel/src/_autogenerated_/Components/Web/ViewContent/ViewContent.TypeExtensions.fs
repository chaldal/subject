namespace ThirdParty.FacebookPixel.Components

open LibClient
open ThirdParty.FacebookPixel.Components.Web.ViewContent
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Web_ViewContentTypeExtensions =
    type ThirdParty.FacebookPixel.Components.Constructors.FacebookPixel.Web with
        static member ViewContent(id: string, price: UnsignedDecimal, ?children: ReactChildrenProp, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Id = id
                    Price = price
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            ThirdParty.FacebookPixel.Components.Web.ViewContent.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            