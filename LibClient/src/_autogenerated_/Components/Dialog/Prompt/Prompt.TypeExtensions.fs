namespace LibClient.Components

open LibClient
open LibLang
open LibClient.Dialogs
open LibClient.Components.Form.Base.Types
open LibClient.Components.Dialog.Prompt
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Dialog_PromptTypeExtensions =
    type LibClient.Components.Constructors.LC.Dialog with
        end