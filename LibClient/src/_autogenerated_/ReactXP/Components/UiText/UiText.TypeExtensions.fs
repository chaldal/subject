namespace ReactXP.Components

open LibClient
open ReactXP.Helpers
open Fable.Core.JsInterop
open Fable.Core
open Browser.Types
open ReactXP.Components.UiText
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module UiTextTypeExtensions =
    type ReactXP.Components.Constructors.RX with
        end