namespace AppEggShellGallery.Components

open LibClient
open Fable.Core.JsInterop
open AppEggShellGallery.Components.ColorVariant.ColorBlock
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module ColorVariant_ColorBlockTypeExtensions =
    type AppEggShellGallery.Components.Constructors.Ui.ColorVariant with
        static member ColorBlock(color: Color, ?children: ReactChildrenProp, ?isMain: bool, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Color = color
                    IsMain = defaultArg isMain (false)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            AppEggShellGallery.Components.ColorVariant.ColorBlock.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            