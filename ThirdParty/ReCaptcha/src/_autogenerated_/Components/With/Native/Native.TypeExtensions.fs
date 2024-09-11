namespace ThirdParty.ReCaptcha.Components

open LibClient
open ReactXP.Components.WebView
open ThirdParty.ReCaptcha.Components.With.Native
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module With_NativeTypeExtensions =
    type ThirdParty.ReCaptcha.Components.Constructors.ReCaptcha.With with
        static member Native(siteKey: string, baseUrl: string, ``with``: (string -> Async<Result<NonemptyString, exn>>) -> ReactElement, ?children: ReactChildrenProp, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    SiteKey = siteKey
                    BaseUrl = baseUrl
                    With = ``with``
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            ThirdParty.ReCaptcha.Components.With.Native.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            