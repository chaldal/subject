namespace AppEggShellGallery.Components

open LibClient
open LibLang
open AppEggShellGallery.Navigation
open AppEggShellGallery.Components.Route.HowTo
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Route_HowToTypeExtensions =
    type AppEggShellGallery.Components.Constructors.Ui.Route with
        static member HowTo(pstoreKey: string, item: HowToItem, ?children: ReactChildrenProp, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    PstoreKey = pstoreKey
                    Item = item
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            AppEggShellGallery.Components.Route.HowTo.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            