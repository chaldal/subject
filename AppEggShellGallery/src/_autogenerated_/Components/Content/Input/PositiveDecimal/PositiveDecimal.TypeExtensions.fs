namespace AppEggShellGallery.Components

open LibClient
open AppEggShellGallery.Components.Content.Input.PositiveDecimal
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Content_Input_PositiveDecimalTypeExtensions =
    type AppEggShellGallery.Components.Constructors.Ui.Content.Input with
        static member PositiveDecimal(?children: ReactChildrenProp, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            AppEggShellGallery.Components.Content.Input.PositiveDecimal.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            