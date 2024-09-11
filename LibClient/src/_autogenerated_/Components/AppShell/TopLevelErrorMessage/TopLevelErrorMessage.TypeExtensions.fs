namespace LibClient.Components

open LibClient
open Fable.Core
open LibClient.Components.AppShell.TopLevelErrorMessage
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module AppShell_TopLevelErrorMessageTypeExtensions =
    type LibClient.Components.Constructors.LC.AppShell with
        static member TopLevelErrorMessage(error: System.Exception, retry: unit -> unit, ?children: ReactChildrenProp, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Error = error
                    Retry = retry
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.AppShell.TopLevelErrorMessage.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            