namespace LibClient.Components

open LibClient
open ReactXP.Styles
open Fable.React
open LibClient.Components.Input.Checkbox
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Input_CheckboxTypeExtensions =
    type LibClient.Components.Constructors.LC.Input with
        static member Checkbox(onChange: bool -> unit, value: Option<bool>, validity: InputValidity, ?children: ReactChildrenProp, ?label: Label, ?styles: array<ViewStyles>, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    OnChange = onChange
                    Value = value
                    Validity = validity
                    Label = defaultArg label (Children)
                    styles = styles |> Option.orElse (None)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Input.Checkbox.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            