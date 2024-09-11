module LibClient.Components.Input.Text

open LibClient
open Fable.Core.JsInterop
open LibClient.Input
open ReactXP.Styles
open ReactXP.Components

type ITextRef =
    abstract member SelectAll:    unit -> unit;
    abstract member RequestFocus: unit -> unit;

type PropSuffixFactory = InputSuffixFactory

type KeyboardType  = TextInput.KeyboardType
type ReturnKeyType = TextInput.ReturnKeyType

type Props = (* GenerateMakeFunction *) {
    Value:               Option<NonemptyString>
    OnChange:            Option<NonemptyString>       -> unit
    Label:               string                                option // defaultWithAutoWrap None
    OnKeyPress:          (Browser.Types.KeyboardEvent -> unit) option // defaultWithAutoWrap None
    OnEnterKeyPress:     (ReactEvent.Keyboard         -> unit) option // defaultWithAutoWrap None
    OnFocus:             (Browser.Types.FocusEvent    -> unit) option // defaultWithAutoWrap None
    OnBlur:              (Browser.Types.FocusEvent    -> unit) option // defaultWithAutoWrap None
    Placeholder:         string                                option // defaultWithAutoWrap None
    Prefix:              string                                option // defaultWithAutoWrap None
    Suffix:              InputSuffix                           option // defaultWithAutoWrap None
    MaxLength:           int                                   option // defaultWithAutoWrap None
    TabIndex:            int                                   option // defaultWithAutoWrap None
    Editable:            bool                                         // default             true
    BlurOnSubmit:        bool                                         // default             false
    Multiline:           bool                                         // default             false
    RequestFocusOnMount: bool                                         // default             false
    SecureTextEntry:     bool                                         // default             false
    KeyboardType:        KeyboardType                                 // default             KeyboardType.Default
    ReturnKeyType:       ReturnKeyType                                // default             ReturnKeyType.Done
    AutoCapitalize:      AutoCapitalize                               // default             AutoCapitalize.Never
    Validity:            InputValidity

    styles: array<ViewStyles> option // defaultWithAutoWrap None
    key: string option                                             // defaultWithAutoWrap JsUndefined
    ref: (LibClient.JsInterop.JsNullable<ITextRef> -> unit) option // defaultWithAutoWrap JsUndefined
}

type Estate = {
    IsFocused: bool
}

type Text(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, Text>("LibClient.Components.Input.Text", _initialProps, Actions, hasStyles = true)

    let mutable maybeTextInput: Option<ReactXP.Components.TextInput.ITextInputRef> = None
    member _.MaybeTextInput
        with get () = maybeTextInput
        and  set (value: Option<ReactXP.Components.TextInput.ITextInputRef>) : unit =
             maybeTextInput <- value

    member this.HandleOnKeyPress (e: Browser.Types.KeyboardEvent) : unit =
        this.props.OnKeyPress |> Option.sideEffect (fun f -> f e)

        match (this.props.Multiline, e.key, this.props.OnEnterKeyPress) with
        | (false, KeyboardEvent.Key.Enter, Some onEnterKeyPress) -> onEnterKeyPress { Event = e; MaybeSource = Some (this :> ReactElement)}
        | (true,  KeyboardEvent.Key.Enter, _)                    -> Log.Error "`OnEnterKeyPress` does not fire when `Multiline='true'`"
        | _                                                      -> Noop

    override _.GetInitialEstate(_initialProps: Props) : Estate = {
        IsFocused = false
    }

    interface ITextRef with
        member _.SelectAll () : unit =
            maybeTextInput |> Option.sideEffect (fun input -> input.selectAll())

        member _.RequestFocus () : unit =
            maybeTextInput |> Option.sideEffect (fun input -> input.requestFocus())

and Actions(this: Text) =
    member _.RefTextInput (nullableInstance: LibClient.JsInterop.JsNullable<ReactXP.Components.TextInput.ITextInputRef>) =
        this.MaybeTextInput <- nullableInstance.ToOption

    member _.Focus (_e: ReactEvent.Action) : unit =
        this.MaybeTextInput |> Option.sideEffect (fun textInput ->
            textInput.requestFocus()
        )

    member _.OnFocus (e: Browser.Types.FocusEvent) : unit =
        this.SetEstate (fun estate _ -> { estate with IsFocused = true })
        this.props.OnFocus |> Option.sideEffect (fun f -> f e)

    member _.OnBlur (e: Browser.Types.FocusEvent) : unit =
        this.SetEstate (fun estate _ -> { estate with IsFocused = false })
        this.props.OnBlur |> Option.sideEffect (fun f -> f e)

    member _.OnKeyPressOption : Option<(Browser.Types.KeyboardEvent) -> unit> =
        match (this.props.OnKeyPress, this.props.OnEnterKeyPress) with
        | (None, None) -> None
        | _            -> Some this.HandleOnKeyPress


// Don't want to clobber && for the whole file
open ReactXP.LegacyStyles

let extractPlaceholderColor (mergedStyles: List<RuntimeStyles>) : Color =
    let maybeColor: Option<Color> =
        let placeholderStyles = Runtime.findApplicableStyles mergedStyles "SPECIAL-placeholder"
        placeholderStyles
        |> List.filterMap (function
            | RuntimeStyles.StaticRules lazyRulesObject ->
                lazyRulesObject.GetRawStyleRules
                |> Seq.findMap
                    (fun rawRule ->
                        let (key, value) = rawRule :> obj :?> (string * obj)
                        match key with
                        | "color"    -> Some (Color.InternalString (value :?> string))
                        | _          -> None
                    )
            | _ -> None
        )
        |> List.tryLast

    maybeColor |> Option.getOrElse Color.DevRed

let Make = makeConstructor<Text, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate