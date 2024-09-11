namespace AppEggShellGallery.Components

open LibClient
open AppEggShellGallery.Navigation
open AppEggShellGallery.Components.TopNav
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module TopNavTypeExtensions =
    type AppEggShellGallery.Components.Constructors.Ui with
        static member TopNav(maybeRoute: Option<Route>, ?children: ReactChildrenProp, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    MaybeRoute = maybeRoute
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            AppEggShellGallery.Components.TopNav.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            