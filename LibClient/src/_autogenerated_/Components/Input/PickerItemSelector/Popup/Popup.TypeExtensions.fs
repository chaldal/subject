namespace LibClient.Components

open LibClient
open LibClient.PickerItemSelector
open LibClient.Components.Input.PickerItemSelector.Popup

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Input_PickerItemSelector_PopupTypeExtensions =
    type LibClient.Components.TypeExtensions.TEs with
        static member MakeInput_PickerItemSelector_PopupProps<'T when 'T : comparison>(pItemView: PickerItemView<'T>, pItems: OrderedSet<'T * PickerItemMetadata>, pOnPress: 'T -> ReactEvent.Action -> unit, pHide: unit -> unit) =
            {
                ItemView = pItemView
                Items = pItems
                OnPress = pOnPress
                Hide = pHide
            }