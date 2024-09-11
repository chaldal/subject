namespace ThirdParty.GoogleAnalytics.Components

open LibClient
open ThirdParty.GoogleAnalytics.Components.Web.ViewContent

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Web_ViewContentTypeExtensions =
    type ThirdParty.GoogleAnalytics.Components.TypeExtensions.TEs with
        static member MakeWeb_ViewContentProps(pId: string, pName: string, pPrice: UnsignedDecimal, pCurrency: string, ?pkey: string, ?pkeyOption: Option<string>) =
            {
                Id = pId
                Name = pName
                Price = pPrice
                Currency = pCurrency
                key = defaultArg (pkeyOption) (defaultArg (pkey |> Option.map Some) (JsUndefined))
            }