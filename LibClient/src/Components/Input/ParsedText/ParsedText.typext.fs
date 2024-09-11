module LibClient.Components.Input.ParsedText

open LibClient
open ReactXP.Styles
open ReactXP.Components

type PropSuffixFactory = InputSuffixFactory

type Value<'T> =
    private {
        PrivateRaw:    Option<NonemptyString>
        PrivateResult: Result<Option<'T>, string>
    }

    member this.Result : Result<Option<'T>, string> = this.PrivateResult

    static member OfRaw (parse: Option<NonemptyString> -> Result<Option<'T>, string>) (raw: Option<NonemptyString>) : Value<'T> =
        {
            PrivateRaw    = raw
            PrivateResult = parse raw
        }

    static member Wrap (value: 'T, stringRepresentation: string) : Value<'T> = {
        PrivateRaw    = NonemptyString.ofString stringRepresentation
        PrivateResult = Ok (Some value)
    }

let getRawInputValueForFieldInternalProcessing (source: Value<'T>) : Option<NonemptyString> =
    source.PrivateRaw


type KeyboardType  = TextInput.KeyboardType
type ReturnKeyType = TextInput.ReturnKeyType

type Props<'T> = (* GenerateMakeFunction *) {
    Parse:                      Option<NonemptyString> -> Result<Option<'T>, string>
    Label:                      string                                option        // defaultWithAutoWrap None
    Value:                      Value<'T>
    Validity:                   InputValidity
    Placeholder:                Option<string>                                      // defaultWithAutoWrap None
    Prefix:                     string                                option        // defaultWithAutoWrap None
    Suffix:                     InputSuffix                           option        // defaultWithAutoWrap None
    Editable:                   bool                                                // default             true
    RequestFocusOnMount:        bool
    TabIndex:                   int                                   option        // defaultWithAutoWrap None
    OnChange:                   Value<'T> -> unit
    OnKeyPress:                 (Browser.Types.KeyboardEvent -> unit) option        // defaultWithAutoWrap None
    OnEnterKeyPress:            (ReactEvent.Keyboard                -> unit) option // defaultWithAutoWrap None
    KeyboardType:               KeyboardType                                        // default             KeyboardType.Default
    ReturnKeyType:              ReturnKeyType                                       // default             ReturnKeyType.Done
    ShouldShowValidationErrors: bool                                                // default             true

    styles: array<ViewStyles> option // defaultWithAutoWrap None
    key: string option // defaultWithAutoWrap JsUndefined
}

type ParsedText<'T>(_initialProps) =
    inherit PureStatelessComponent<Props<'T>, Actions<'T>, ParsedText<'T>>("LibClient.Components.Input.ParsedText", _initialProps, Actions<'T>, hasStyles = true)

and Actions<'T>(_this: ParsedText<'T>) =
    class end

let Make = makeConstructor<ParsedText<'T>, _, _>

// Unfortunately necessary boilerplate
type Estate<'T> = NoEstate1<'T>
type Pstate = NoPstate