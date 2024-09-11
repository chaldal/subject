namespace LibClient.Components

open LibClient
open ReactXP.Styles
open LibClient.Components.Section.Padded
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Section_PaddedTypeExtensions =
    type LibClient.Components.Constructors.LC.Section with
        static member Padded(?children: ReactChildrenProp, ?styles: array<ViewStyles>, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    styles = styles |> Option.orElse (None)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Section.Padded.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            