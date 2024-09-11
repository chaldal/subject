namespace ThirdParty.ReCaptcha.Components

open LibClient
open Fable.Core
open Fable.Core.JsInterop
open Fable.Core.JS
open ThirdParty.ReCaptcha.Components.With.Web
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module With_WebTypeExtensions =
    type ThirdParty.ReCaptcha.Components.Constructors.ReCaptcha.With with
        static member Web(``with``: (string -> Async<Result<NonemptyString, exn>>) -> ReactElement, siteKey: string, ?children: ReactChildrenProp, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    With = ``with``
                    SiteKey = siteKey
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            ThirdParty.ReCaptcha.Components.With.Web.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            