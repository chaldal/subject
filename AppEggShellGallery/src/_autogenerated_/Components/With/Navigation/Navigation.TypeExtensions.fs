namespace AppEggShellGallery.Components

open LibClient
open AppEggShellGallery.Components.With.Navigation
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module With_NavigationTypeExtensions =
    type AppEggShellGallery.Components.Constructors.Ui.With with
        static member Navigation(``with``: AppEggShellGallery.Navigation.Navigation -> ReactElement, ?children: ReactChildrenProp, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    With = ``with``
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            AppEggShellGallery.Components.With.Navigation.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            