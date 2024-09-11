namespace AppEggShellGallery.Components

open LibClient
open AppEggShellGallery.Components.ColorVariant.Base
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module ColorVariant_BaseTypeExtensions =
    type AppEggShellGallery.Components.Constructors.Ui.ColorVariant with
        static member Base(color: Color, name: string, isMain: bool, ?children: ReactChildrenProp, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Color = color
                    Name = name
                    IsMain = isMain
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            AppEggShellGallery.Components.ColorVariant.Base.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            