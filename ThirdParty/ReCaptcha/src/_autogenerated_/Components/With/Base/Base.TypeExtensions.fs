namespace ThirdParty.ReCaptcha.Components

open LibClient
open ThirdParty.ReCaptcha.Components.With.Base
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module With_BaseTypeExtensions =
    type ThirdParty.ReCaptcha.Components.Constructors.ReCaptcha.With with
        static member Base(``with``: (string -> Async<Result<NonemptyString, exn>>) -> ReactElement, siteKey: string, baseUrl: string, ?children: ReactChildrenProp, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    With = ``with``
                    SiteKey = siteKey
                    BaseUrl = baseUrl
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            ThirdParty.ReCaptcha.Components.With.Base.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            