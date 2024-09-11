namespace LibClient.Components

open LibClient
open LibClient.Components
open Fable.React
open ReactXP.Components
open ReactXP.Styles
open LibClient.Components.AppShell.Context
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module AppShell_ContextTypeExtensions =
    type LibClient.Components.Constructors.LC.AppShell with
        static member Context(?children: ReactChildrenProp, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.AppShell.Context.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            