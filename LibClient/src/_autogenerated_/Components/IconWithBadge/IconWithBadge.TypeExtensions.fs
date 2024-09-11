namespace LibClient.Components

open LibClient
open LibClient.Components.IconWithBadge
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module IconWithBadgeTypeExtensions =
    type LibClient.Components.Constructors.LC with
        static member IconWithBadge(icon: LibClient.Icons.IconConstructor, badge: Badge, ?children: ReactChildrenProp, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Icon = icon
                    Badge = badge
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.IconWithBadge.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            