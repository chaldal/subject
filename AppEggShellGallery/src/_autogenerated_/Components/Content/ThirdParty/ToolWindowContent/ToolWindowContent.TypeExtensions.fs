namespace AppEggShellGallery.Components

open LibClient
open ThirdParty.Map
open AppEggShellGallery.Components.Content.ThirdParty.ToolWindowContent
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Content_ThirdParty_ToolWindowContentTypeExtensions =
    type AppEggShellGallery.Components.Constructors.Ui.Content.ThirdParty with
        static member ToolWindowContent(handle: InfoWindowHandle, ?children: ReactChildrenProp, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Handle = handle
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            AppEggShellGallery.Components.Content.ThirdParty.ToolWindowContent.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            