namespace LibClient.Components

open LibClient
open LibLang
open LibClient.Dialogs
open LibClient.ContextMenus.Types
open LibClient.Components.ContextMenu.Dialog
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module ContextMenu_DialogTypeExtensions =
    type LibClient.Components.Constructors.LC.ContextMenu with
        end