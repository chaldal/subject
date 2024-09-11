namespace LibClient.Components

open LibClient
open LibLangFsharp
open LibClient.Responsive
open LibClient.Components.Input.PickerModel
open Fable.Core.JsInterop
open ReactXP.Styles
open LibClient.Components.Input.PickerInternals.Base
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Input_PickerInternals_BaseTypeExtensions =
    type LibClient.Components.Constructors.LC.Input.PickerInternals with
        static member Base(items: Items<'Item>, itemView: PickerItemView<'Item>, value: SelectableValue<'Item>, validity: InputValidity, screenSize: ScreenSize, showSearchBar: bool, ?children: ReactChildrenProp, ?label: string, ?placeholder: string, ?pickerId: string, ?styles: array<ViewStyles>, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Items = items
                    ItemView = itemView
                    Value = value
                    Validity = validity
                    ScreenSize = screenSize
                    ShowSearchBar = showSearchBar
                    Label = label |> Option.orElse (None)
                    Placeholder = placeholder |> Option.orElse (None)
                    PickerId = pickerId |> Option.orElse (JsUndefined)
                    styles = styles |> Option.orElse (None)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Input.PickerInternals.Base.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            