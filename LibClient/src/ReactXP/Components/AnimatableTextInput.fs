[<AutoOpen>]
module ReactXP.Components.AnimatableTextInput

open ReactXP.Helpers
open ReactXP.Styles

open Fable.Core
open Fable.Core.JsInterop
open Browser.Types

module RX =
    module AnimatableTextInput =
        [<StringEnum>]
        type KeyboardType =
        | Default
        | Numeric
        | [<CompiledName("email-address")>] EmailAddress
        | [<CompiledName("number-pad")>] NumberPad

        [<StringEnum>]
        type AutoCapitalize =
        | [<CompiledName("none")>]Never
        | Sentences
        | Words
        | Characters

        [<StringEnum>]
        type KeyboardAppearance =
        | Default
        | Light
        | Dark

        [<StringEnum>]
        type ReturnKeyType =
        | Done
        | Go
        | Next
        | Search
        | Send

        [<StringEnum>]
        type ClearButtonMode =
        | Never
        | [<CompiledName("while-editing")>] WhileEditing
        | [<CompiledName("unless-editing")>] UnlessEditing
        | Always

        type ITextInputRef =
            abstract member selectAll:    unit -> unit;
            abstract member requestFocus: unit -> unit;

open RX.AnimatableTextInput

type ReactXP.Components.Constructors.RX with
    static member AnimatableTextInput(
        ?autoCapitalize:           AutoCapitalize,
        ?autoCorrect:              bool,
        ?autoFocus:                bool,
        ?blurOnSubmit:             bool,
        ?defaultValue:             string,
        ?editable:                 bool,
        ?keyboardType:             KeyboardType,
        ?maxLength:                float,
        ?multiline:                bool,
        ?placeholder:              string,
        ?placeholderTextColor:     string,
        ?secureTextEntry:          bool,
        ?value:                    string,
        ?title:                    string,
        ?allowFontScaling:         bool,
        ?maxContentSizeMultiplier: float,
        ?keyboardAppearance:       KeyboardAppearance,
        ?returnKeyType:            ReturnKeyType,
        ?disableFullscreenUI:      bool,
        ?spellCheck:               bool,
        ?selectionColor:           string,
        ?styles:                   array<AnimatableTextInputStyles>,
        // windows and iOS only
        ?clearButtonMode:          ClearButtonMode,
        ?onKeyPress:               KeyboardEvent -> unit,
        ?onFocus:                  FocusEvent -> unit,
        ?onBlur:                   FocusEvent -> unit,
        ?onPaste:                  ClipboardEvent -> unit,
        ?onChangeText:             string -> unit,
        ?onSelectionChange:        float -> float -> unit,
        ?onSubmitEditing:          unit -> unit,
        ?onScroll:                 float -> float -> unit,
        ?ref:                      LibClient.JsInterop.JsNullable<ITextInputRef> -> unit
    ) =
        let __props = createEmpty

        __props?autoCapitalize           <- autoCapitalize
        __props?autoCorrect              <- autoCorrect
        __props?autoFocus                <- autoFocus
        __props?blurOnSubmit             <- blurOnSubmit
        __props?defaultValue             <- defaultValue
        __props?editable                 <- editable |> Option.orElse (Some true)
        __props?keyboardType             <- keyboardType
        __props?maxLength                <- maxLength
        __props?multiline                <- multiline
        __props?placeholder              <- placeholder
        __props?placeholderTextColor     <- placeholderTextColor
        __props?secureTextEntry          <- secureTextEntry
        __props?value                    <- value
        __props?title                    <- title
        __props?allowFontScaling         <- allowFontScaling
        __props?maxContentSizeMultiplier <- maxContentSizeMultiplier
        __props?keyboardAppearance       <- keyboardAppearance
        __props?returnKeyType            <- returnKeyType
        __props?disableFullscreenUI      <- disableFullscreenUI
        __props?spellCheck               <- spellCheck
        __props?selectionColor           <- selectionColor
        __props?clearButtonMode          <- clearButtonMode
        __props?onKeyPress               <- onKeyPress
        __props?onFocus                  <- onFocus
        __props?onBlur                   <- onBlur
        __props?onPaste                  <- onPaste
        __props?onChangeText             <- onChangeText
        __props?onSelectionChange        <- onSelectionChange
        __props?onSubmitEditing          <- onSubmitEditing
        __props?onScroll                 <- onScroll
        __props?ref                      <- ref
        __props?style                    <- styles

        Fable.React.ReactBindings.React.createElement(
            ReactXPRaw?Animated?TextInput,
            __props,
            [||]
        )
