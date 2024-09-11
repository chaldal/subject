namespace LibClient.Components

open LibClient
open LibClient.Components.Sidebar.WithClose
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Sidebar_WithCloseTypeExtensions =
    type LibClient.Components.Constructors.LC.Sidebar with
        static member WithClose(``with``: (ReactEvent.Action -> unit) -> ReactElement, ?children: ReactChildrenProp, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    With = ``with``
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Sidebar.WithClose.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            