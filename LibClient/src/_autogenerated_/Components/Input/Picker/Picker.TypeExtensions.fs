namespace LibClient.Components

open LibClient
open LibClient.Components.Input.PickerModel
open ReactXP.Styles
open LibClient.Components.Input.Picker
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Input_PickerTypeExtensions =
    type LibClient.Components.Constructors.LC.Input with
        static member Picker(items: Items<'T>, itemView: PickerItemView<'T>, value: SelectableValue<'T>, validity: InputValidity, ?children: ReactChildrenProp, ?showSearchBar: bool, ?label: string, ?placeholder: string, ?styles: array<ViewStyles>, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Items = items
                    ItemView = itemView
                    Value = value
                    Validity = validity
                    ShowSearchBar = defaultArg showSearchBar (true)
                    Label = label |> Option.orElse (None)
                    Placeholder = placeholder |> Option.orElse (None)
                    styles = styles |> Option.orElse (None)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Input.Picker.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            