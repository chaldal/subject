namespace AppEggShellGallery.Components

open LibClient
open Scraping.Types
open LibRenderDSL.Types
open AppEggShellGallery.Components.ComponentProps
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module ComponentPropsTypeExtensions =
    type AppEggShellGallery.Components.Constructors.Ui with
        static member ComponentProps(data: Data, ?children: ReactChildrenProp, ?heading: string, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Data = data
                    Heading = heading |> Option.orElse (None)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            AppEggShellGallery.Components.ComponentProps.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            