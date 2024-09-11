namespace LibClient.Components

open LibClient
open LibClient.Components.Input.PickerModel
open ReactXP.Styles
open ReactXP.LegacyStyles
open LibClient.Components.Input.PickerInternals.Field
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Input_PickerInternals_FieldTypeExtensions =
    type LibClient.Components.Constructors.LC.Input.PickerInternals with
        static member Field(model: PickerModel<'Item>, value: SelectableValue<'Item>, validity: InputValidity, itemView: PickerItemView<'Item>, ?children: ReactChildrenProp, ?label: string, ?placeholder: string, ?styles: array<ViewStyles>, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Model = model
                    Value = value
                    Validity = validity
                    ItemView = itemView
                    Label = label |> Option.orElse (None)
                    Placeholder = placeholder |> Option.orElse (None)
                    styles = styles |> Option.orElse (None)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Input.PickerInternals.Field.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            