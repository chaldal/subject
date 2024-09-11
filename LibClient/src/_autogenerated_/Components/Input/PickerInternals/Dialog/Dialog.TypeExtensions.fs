namespace LibClient.Components

open LibClient
open LibLang
open LibClient.Dialogs
open LibClient.Components.Input.PickerModel
open LibLangFsharp
open LibClient.Components.Input.PickerInternals.Dialog
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Input_PickerInternals_DialogTypeExtensions =
    type LibClient.Components.Constructors.LC.Input.PickerInternals with
        end