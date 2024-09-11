namespace AppEggShellGallery.Components

open LibClient
open AppEggShellGallery.Components.Content.Input.Text
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Content_Input_TextTypeExtensions =
    type AppEggShellGallery.Components.Constructors.Ui.Content.Input with
        static member Text(?children: ReactChildrenProp, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            AppEggShellGallery.Components.Content.Input.Text.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            