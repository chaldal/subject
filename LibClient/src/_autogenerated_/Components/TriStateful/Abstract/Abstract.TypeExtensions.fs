namespace LibClient.Components

open LibClient
open Fable.React
open LibClient.Components.TriStateful.Abstract
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module TriStateful_AbstractTypeExtensions =
    type LibClient.Components.Constructors.LC.TriStateful with
        static member Abstract(content: (Mode * (* runAction *) RunAction * (* reset *) (ReactEvent.Action -> unit)) -> ReactElement, ?children: ReactChildrenProp, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Content = content
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.TriStateful.Abstract.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            