namespace LibRouter.Components

open LibClient
open Fable.Core.JsInterop
open LibRouter.Components.Link
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module LinkTypeExtensions =
    type LibRouter.Components.Constructors.LR with
        static member Link(``to``: string, ?children: ReactChildrenProp, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    To = ``to``
                    key = key |> Option.orElse (LibClient.JsInterop.Undefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibRouter.Components.Link.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            