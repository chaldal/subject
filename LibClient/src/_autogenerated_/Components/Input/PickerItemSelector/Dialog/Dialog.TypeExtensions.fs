namespace LibClient.Components

open LibLang
open LibClient
open LibClient.Dialogs
open LibClient.PickerItemSelector
open LibClient.Components.Input.PickerItemSelector.Dialog

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Input_PickerItemSelector_DialogTypeExtensions =
    type LibClient.Components.TypeExtensions.TEs with
        end