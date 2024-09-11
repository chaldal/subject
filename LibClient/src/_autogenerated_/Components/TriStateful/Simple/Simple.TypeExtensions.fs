namespace LibClient.Components

open LibClient
open Fable.React
open LibClient.Components.TriStateful.Simple
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module TriStateful_SimpleTypeExtensions =
    type LibClient.Components.Constructors.LC.TriStateful with
        static member Simple(content: (* runAction *) (Async<Result<unit, string>> -> unit) -> ReactElement, ?children: ReactChildrenProp, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Content = content
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.TriStateful.Simple.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            