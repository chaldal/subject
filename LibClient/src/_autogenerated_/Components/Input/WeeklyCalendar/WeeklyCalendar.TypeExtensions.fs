namespace LibClient.Components

open LibClient
open System
open LibClient.Components.Input.WeeklyCalendar
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Input_WeeklyCalendarTypeExtensions =
    type LibClient.Components.Constructors.LC.Input with
        static member WeeklyCalendar(value: Set<Date>, onChange: Set<Date> -> unit, validity: InputValidity, ?children: ReactChildrenProp, ?label: string, ?startDate: Date, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Value = value
                    OnChange = onChange
                    Validity = validity
                    Label = label |> Option.orElse (None)
                    StartDate = startDate |> Option.orElse (None)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Input.WeeklyCalendar.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            