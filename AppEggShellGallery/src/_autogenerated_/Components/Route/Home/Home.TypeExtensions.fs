namespace AppEggShellGallery.Components

open LibClient
open LibClient.Responsive
open LibLang
open AppEggShellGallery.Navigation
open AppEggShellGallery.Components.Route.Home
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Route_HomeTypeExtensions =
    type AppEggShellGallery.Components.Constructors.Ui.Route with
        static member Home(pstoreKey: string, ?children: ReactChildrenProp, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    PstoreKey = pstoreKey
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            AppEggShellGallery.Components.Route.Home.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            