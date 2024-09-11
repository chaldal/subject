namespace AppEggShellGallery.Components

open LibClient
open AppEggShellGallery.LocalImages
open AppEggShellGallery.Components.Content.Dialogs
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Content_DialogsTypeExtensions =
    type AppEggShellGallery.Components.Constructors.Ui.Content with
        static member Dialogs(?children: ReactChildrenProp, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            AppEggShellGallery.Components.Content.Dialogs.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            