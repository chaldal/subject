namespace LibRouter.Components

open LibClient
open Fable.Core.JsInterop
open LibRouter.Components.NativeBackButton
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module NativeBackButtonTypeExtensions =
    type LibRouter.Components.Constructors.LR with
        static member NativeBackButton(goBack: unit -> unit, ?children: ReactChildrenProp, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    goBack = goBack
                    key = key |> Option.orElse (LibClient.JsInterop.Undefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibRouter.Components.NativeBackButton.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            