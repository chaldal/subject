namespace LibClient.Components

open LibClient
open LibLang
open LibClient.Dialogs
open LibClient.Components.Dialog.Confirm
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Dialog_ConfirmTypeExtensions =
    type LibClient.Components.Constructors.LC.Dialog with
        end