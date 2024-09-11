namespace LibClient.Components

open LibClient
open LibClient.Components.Legacy.TopNav.ShowSidebarButton
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Legacy_TopNav_ShowSidebarButtonTypeExtensions =
    type LibClient.Components.Constructors.LC.Legacy.TopNav with
        static member ShowSidebarButton(onlyOnHandheld: bool, ?children: ReactChildrenProp, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    OnlyOnHandheld = onlyOnHandheld
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Legacy.TopNav.ShowSidebarButton.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            