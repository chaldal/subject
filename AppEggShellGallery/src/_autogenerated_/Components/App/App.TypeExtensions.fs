namespace AppEggShellGallery.Components

open LibClient
open LibLang
open LibRouter.RoutesSpec
open Fable.Core.JsInterop
open AppEggShellGallery.Navigation
open System.Text.RegularExpressions
open AppEggShellGallery.Components.App
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module AppTypeExtensions =
    type AppEggShellGallery.Components.Constructors.Ui with
        end