namespace AppEggShellGallery.Components

open LibClient
open LibClient.Components.Form.Base.Types
open AppEggShellGallery.Components.Content.QueryGrid
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Content_QueryGridTypeExtensions =
    type AppEggShellGallery.Components.Constructors.Ui.Content with
        static member QueryGrid(?children: ReactChildrenProp, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            AppEggShellGallery.Components.Content.QueryGrid.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            