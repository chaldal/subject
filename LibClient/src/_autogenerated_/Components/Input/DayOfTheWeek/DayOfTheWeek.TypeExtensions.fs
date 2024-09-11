namespace LibClient.Components

open LibClient
open LibClient.Components.Input.DayOfTheWeek
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Input_DayOfTheWeekTypeExtensions =
    type LibClient.Components.Constructors.LC.Input with
        static member DayOfTheWeek(mode: Mode, validity: InputValidity, ?children: ReactChildrenProp, ?label: string, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Mode = mode
                    Validity = validity
                    Label = label |> Option.orElse (None)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Input.DayOfTheWeek.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            