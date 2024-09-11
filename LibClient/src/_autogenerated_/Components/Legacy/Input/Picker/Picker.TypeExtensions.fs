namespace LibClient.Components

open LibClient
open LibClient.ContextMenus.Types
open ReactXP.Styles
open LibClient.Components.Legacy.Input.Picker
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Legacy_Input_PickerTypeExtensions =
    type LibClient.Components.Constructors.LC.Legacy.Input with
        static member Picker(items: List<PickerItem<'T>>, value: Option<SelectedItem<'T>>, onChange: OnChange<'T>, validity: InputValidity, ?children: ReactChildrenProp, ?label: string, ?styles: array<ViewStyles>, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Items = items
                    Value = value
                    OnChange = onChange
                    Validity = validity
                    Label = label |> Option.orElse (None)
                    styles = styles |> Option.orElse (None)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Legacy.Input.Picker.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            