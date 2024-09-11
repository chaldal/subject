namespace LibClient.Components

open LibClient
open LibClient.Input
open ReactXP.Styles
open System.Text.RegularExpressions
open LibClient.Components.Input.PositiveInteger
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Input_PositiveIntegerTypeExtensions =
    type LibClient.Components.Constructors.LC.Input with
        static member PositiveInteger(value: Value, validity: InputValidity, onChange: Value -> unit, ?children: ReactChildrenProp, ?editable: bool, ?requestFocusOnMount: bool, ?label: string, ?placeholder: string, ?prefix: string, ?suffix: InputSuffix, ?tabIndex: int, ?onKeyPress: (Browser.Types.KeyboardEvent -> unit), ?onEnterKeyPress: (ReactEvent.Keyboard         -> unit), ?styles: array<ViewStyles>, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Value = value
                    Validity = validity
                    OnChange = onChange
                    Editable = defaultArg editable (true)
                    RequestFocusOnMount = defaultArg requestFocusOnMount (false)
                    Label = label |> Option.orElse (None)
                    Placeholder = placeholder |> Option.orElse (None)
                    Prefix = prefix |> Option.orElse (None)
                    Suffix = suffix |> Option.orElse (None)
                    TabIndex = tabIndex |> Option.orElse (None)
                    OnKeyPress = onKeyPress |> Option.orElse (None)
                    OnEnterKeyPress = onEnterKeyPress |> Option.orElse (None)
                    styles = styles |> Option.orElse (None)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Input.PositiveInteger.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            