namespace ThirdParty.GoogleAnalytics.Components

open LibClient
open ThirdParty.GoogleAnalytics.Components.Base.ViewContent
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Base_ViewContentTypeExtensions =
    type ThirdParty.GoogleAnalytics.Components.Constructors.GoogleAnalytics.Base with
        static member ViewContent(id: string, name: string, price: UnsignedDecimal, currency: string, ?children: ReactChildrenProp, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Id = id
                    Name = name
                    Price = price
                    Currency = currency
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            ThirdParty.GoogleAnalytics.Components.Base.ViewContent.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            