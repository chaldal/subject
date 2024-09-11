namespace LibRouter.Components

open LibClient
open Fable.Core
open Fable.Core.JsInterop
open LibRouter.Components.Router
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module RouterTypeExtensions =
    type LibRouter.Components.Constructors.LR with
        static member Router(?children: ReactChildrenProp, ?future: obj, ?key: string, ?initialEntries: array<string>, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    future = defaultArg future (Some (createObj ["v7_startTransition" ==> true]))
                    key = key |> Option.orElse (LibClient.JsInterop.Undefined)
                    initialEntries = initialEntries |> Option.orElse (LibClient.JsInterop.Undefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibRouter.Components.Router.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            