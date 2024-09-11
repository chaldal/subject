namespace AppEggShellGallery.Components

open LibClient
open AppEggShellGallery.Components.Content
open System
open LibClient.Components.Input.PickerModel
open AppEggShellGallery.Components.Content.Input.Picker
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Content_Input_PickerTypeExtensions =
    type AppEggShellGallery.Components.Constructors.Ui.Content.Input with
        static member Picker(?children: ReactChildrenProp, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            AppEggShellGallery.Components.Content.Input.Picker.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            