namespace AppEggShellGallery.Components

open LibClient
open AppEggShellGallery.Components.Content.Scrim
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Content_ScrimTypeExtensions =
    type AppEggShellGallery.Components.Constructors.Ui.Content with
        static member Scrim(?children: ReactChildrenProp, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            AppEggShellGallery.Components.Content.Scrim.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            