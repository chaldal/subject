namespace LibClient.Components

open LibClient
open LibClient.Components.Legacy.TopNav.Filler
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Legacy_TopNav_FillerTypeExtensions =
    type LibClient.Components.Constructors.LC.Legacy.TopNav with
        static member Filler(?children: ReactChildrenProp, ?count: int, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Count = defaultArg count (1)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Legacy.TopNav.Filler.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            