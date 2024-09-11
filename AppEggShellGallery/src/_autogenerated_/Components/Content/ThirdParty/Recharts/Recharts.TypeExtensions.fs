namespace AppEggShellGallery.Components

open LibClient
open LibLang
open AppEggShellGallery.Components.Content.ThirdParty.Recharts
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Content_ThirdParty_RechartsTypeExtensions =
    type AppEggShellGallery.Components.Constructors.Ui.Content.ThirdParty with
        static member Recharts(?children: ReactChildrenProp, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            AppEggShellGallery.Components.Content.ThirdParty.Recharts.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            