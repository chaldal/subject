module LibClient.Components.Input.UnsignedInteger

open LibClient
open LibClient.Input
open System.Text.RegularExpressions
open ReactXP.Styles

let private onlyValidCharacters = Regex "^[0-9]*$"

type PropSuffixFactory = InputSuffixFactory

let parseProp (raw: Option<NonemptyString>): Result<Option<Unsigned.UnsignedInteger>, string> =
    match raw with
    | None -> Ok None
    | Some (NonemptyString nonemptyRaw) ->
        let hasOnlyValidCharacters : bool = onlyValidCharacters.IsMatch nonemptyRaw
        let parseResult = System.Int32.ParseOption nonemptyRaw
        match (parseResult, hasOnlyValidCharacters) with
        | (None, _)
        | (_, false) -> Error "Only numbers allowed"
        | (Some value, true) ->
            match Unsigned.UnsignedInteger.ofInt value with
            | None                 -> Error "Allowed only nonnegative numbers"
            | Some unsignedInteger -> unsignedInteger |> Some |> Ok

type Value = LibClient.Components.Input.ParsedText.Value<Unsigned.UnsignedInteger>

let parse (raw: Option<NonemptyString>) : Value =
    Value.OfRaw parseProp raw

let wrap (value: Unsigned.UnsignedInteger) : Value = Value.Wrap (value, value.Value.ToString())

let empty : Value = parse None

type Props = (* GenerateMakeFunction *) {
    Label:               string                                option // defaultWithAutoWrap None
    Value:               Value
    Validity:            InputValidity
    Placeholder:         Option<string>                               // defaultWithAutoWrap None
    Prefix:              string                                option // defaultWithAutoWrap None
    Suffix:              InputSuffix                           option // defaultWithAutoWrap None
    RequestFocusOnMount: bool                                         // default             false
    TabIndex:            int                                   option // defaultWithAutoWrap None
    OnChange:            Value -> unit
    OnKeyPress:          (Browser.Types.KeyboardEvent -> unit) option // defaultWithAutoWrap None
    OnEnterKeyPress:     (ReactEvent.Keyboard         -> unit) option // defaultWithAutoWrap None

    styles: array<ViewStyles> option // defaultWithAutoWrap None
    key: string option // defaultWithAutoWrap JsUndefined
}

type UnsignedInteger(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, UnsignedInteger>("LibClient.Components.Input.UnsignedInteger", _initialProps, Actions, hasStyles = true)

and Actions(_this: UnsignedInteger) =
    class end

let Make = makeConstructor<UnsignedInteger, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate