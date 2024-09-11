namespace LibClient.Components

open LibClient
open Fable.Core.JsInterop
open LibClient.Input
open ReactXP.Styles
open ReactXP.Components
open ReactXP.LegacyStyles
open LibClient.Components.Input.Text
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Input_TextTypeExtensions =
    type LibClient.Components.Constructors.LC.Input with
        static member Text(value: Option<NonemptyString>, onChange: Option<NonemptyString>       -> unit, validity: InputValidity, ?children: ReactChildrenProp, ?editable: bool, ?blurOnSubmit: bool, ?multiline: bool, ?requestFocusOnMount: bool, ?secureTextEntry: bool, ?keyboardType: KeyboardType, ?returnKeyType: ReturnKeyType, ?autoCapitalize: AutoCapitalize, ?label: string, ?onKeyPress: (Browser.Types.KeyboardEvent -> unit), ?onEnterKeyPress: (ReactEvent.Keyboard         -> unit), ?onFocus: (Browser.Types.FocusEvent    -> unit), ?onBlur: (Browser.Types.FocusEvent    -> unit), ?placeholder: string, ?prefix: string, ?suffix: InputSuffix, ?maxLength: int, ?tabIndex: int, ?styles: array<ViewStyles>, ?key: string, ?ref: (LibClient.JsInterop.JsNullable<ITextRef> -> unit), ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Value = value
                    OnChange = onChange
                    Validity = validity
                    Editable = defaultArg editable (true)
                    BlurOnSubmit = defaultArg blurOnSubmit (false)
                    Multiline = defaultArg multiline (false)
                    RequestFocusOnMount = defaultArg requestFocusOnMount (false)
                    SecureTextEntry = defaultArg secureTextEntry (false)
                    KeyboardType = defaultArg keyboardType (KeyboardType.Default)
                    ReturnKeyType = defaultArg returnKeyType (ReturnKeyType.Done)
                    AutoCapitalize = defaultArg autoCapitalize (AutoCapitalize.Never)
                    Label = label |> Option.orElse (None)
                    OnKeyPress = onKeyPress |> Option.orElse (None)
                    OnEnterKeyPress = onEnterKeyPress |> Option.orElse (None)
                    OnFocus = onFocus |> Option.orElse (None)
                    OnBlur = onBlur |> Option.orElse (None)
                    Placeholder = placeholder |> Option.orElse (None)
                    Prefix = prefix |> Option.orElse (None)
                    Suffix = suffix |> Option.orElse (None)
                    MaxLength = maxLength |> Option.orElse (None)
                    TabIndex = tabIndex |> Option.orElse (None)
                    styles = styles |> Option.orElse (None)
                    key = key |> Option.orElse (JsUndefined)
                    ref = ref |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Input.Text.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            