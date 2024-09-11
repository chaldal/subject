namespace LibClient.Components

open LibClient
open System
open LibClient.Components.Input.LocalTime
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Input_LocalTimeTypeExtensions =
    type LibClient.Components.Constructors.LC.Input with
        static member LocalTime(value: Value, validity: InputValidity, onChange: Value -> unit, ?children: ReactChildrenProp, ?requestFocusOnMount: bool, ?label: string, ?onEnterKeyPress: (ReactEvent.Keyboard -> unit), ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Value = value
                    Validity = validity
                    OnChange = onChange
                    RequestFocusOnMount = defaultArg requestFocusOnMount (false)
                    Label = label |> Option.orElse (None)
                    OnEnterKeyPress = onEnterKeyPress |> Option.orElse (None)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Input.LocalTime.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            