namespace LibClient.Components

open LibClient
open LibClient.Components.Input.PickerModel
open LibClient.Components.Input.PickerInternals.Popup
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Input_PickerInternals_PopupTypeExtensions =
    type LibClient.Components.Constructors.LC.Input.PickerInternals with
        static member Popup(model: PickerModel<'Item>, itemView: PickerItemView<'Item>, ?children: ReactChildrenProp, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Model = model
                    ItemView = itemView
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Input.PickerInternals.Popup.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            