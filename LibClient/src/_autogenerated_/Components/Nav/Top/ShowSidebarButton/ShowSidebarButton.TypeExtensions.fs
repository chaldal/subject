namespace LibClient.Components

open LibClient
open LibClient.Icons
open LibClient.Components.Nav.Top.ShowSidebarButton
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Nav_Top_ShowSidebarButtonTypeExtensions =
    type LibClient.Components.Constructors.LC.Nav.Top with
        static member ShowSidebarButton(?children: ReactChildrenProp, ?badge: Badge, ?menuIcon: IconConstructor, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Badge = badge |> Option.orElse (None)
                    MenuIcon = menuIcon |> Option.orElse (None)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Nav.Top.ShowSidebarButton.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            