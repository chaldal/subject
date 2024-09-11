namespace LibClient.Components

open LibClient
open ReactXP.Styles
open ReactXP.Components
open LibClient.Components.Input.ParsedText
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Input_ParsedTextTypeExtensions =
    type LibClient.Components.Constructors.LC.Input with
        static member ParsedText(parse: Option<NonemptyString> -> Result<Option<'T>, string>, value: Value<'T>, validity: InputValidity, requestFocusOnMount: bool, onChange: Value<'T> -> unit, ?children: ReactChildrenProp, ?editable: bool, ?keyboardType: KeyboardType, ?returnKeyType: ReturnKeyType, ?shouldShowValidationErrors: bool, ?label: string, ?placeholder: string, ?prefix: string, ?suffix: InputSuffix, ?tabIndex: int, ?onKeyPress: (Browser.Types.KeyboardEvent -> unit), ?onEnterKeyPress: (ReactEvent.Keyboard                -> unit), ?styles: array<ViewStyles>, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Parse = parse
                    Value = value
                    Validity = validity
                    RequestFocusOnMount = requestFocusOnMount
                    OnChange = onChange
                    Editable = defaultArg editable (true)
                    KeyboardType = defaultArg keyboardType (KeyboardType.Default)
                    ReturnKeyType = defaultArg returnKeyType (ReturnKeyType.Done)
                    ShouldShowValidationErrors = defaultArg shouldShowValidationErrors (true)
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
            LibClient.Components.Input.ParsedText.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            