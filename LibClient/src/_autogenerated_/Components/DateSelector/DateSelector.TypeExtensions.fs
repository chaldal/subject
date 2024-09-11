namespace LibClient.Components

open LibClient
open System
open LibClient.ComponentDataSubscription
open LibClient.ServiceInstances
open LibClient.Components.DateSelector
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module DateSelectorTypeExtensions =
    type LibClient.Components.Constructors.LC with
        static member DateSelector(onChange: DateOnly -> unit, maybeSelected: Option<DateOnly>, ?children: ReactChildrenProp, ?minDate: DateOnly, ?maxDate: DateOnly, ?canSelectDate: (DateOnly -> bool), ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    OnChange = onChange
                    MaybeSelected = maybeSelected
                    MinDate = minDate |> Option.orElse (None)
                    MaxDate = maxDate |> Option.orElse (None)
                    CanSelectDate = canSelectDate |> Option.orElse (None)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.DateSelector.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            