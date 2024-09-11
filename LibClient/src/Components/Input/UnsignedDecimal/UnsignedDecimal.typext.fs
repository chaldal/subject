module LibClient.Components.Input.UnsignedDecimal

open LibClient
open LibClient.Input
open System.Text.RegularExpressions
open ReactXP.Styles

let private onlyValidCharacters = Regex "^[0-9.]*$"

type PropSuffixFactory = InputSuffixFactory

let parseProp (raw: Option<NonemptyString>): Result<Option<Unsigned.UnsignedDecimal>, string> =
    match raw with
    | None -> Ok None
    | Some (NonemptyString nonemptyRaw) ->
        let hasOnlyValidCharacters : bool = onlyValidCharacters.IsMatch nonemptyRaw
        let parseResult = System.Decimal.ParseOption nonemptyRaw
        match (parseResult, hasOnlyValidCharacters) with
        | (None, _)
        | (_, false) -> Error "Only numbers and period allowed"
        | (Some value, true) ->
            match Unsigned.UnsignedDecimal.ofDecimal value with
            | None                 -> Error "Allowed only nonnegative numbers"
            | Some unsignedDecimal -> unsignedDecimal |> Some |> Ok

type Value = LibClient.Components.Input.ParsedText.Value<Unsigned.UnsignedDecimal>

let parse (raw: Option<NonemptyString>) : Value =
    Value.OfRaw parseProp raw

let wrap (value: Unsigned.UnsignedDecimal) : Value = Value.Wrap (value, value.Value.ToString())

let empty = parse None

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
    OnEnterKeyPress:     (ReactEvent.Keyboard                -> unit) option // defaultWithAutoWrap None

    styles: array<ViewStyles> option // defaultWithAutoWrap None
    key: string option // defaultWithAutoWrap JsUndefined
}

type UnsignedDecimal(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, UnsignedDecimal>("LibClient.Components.Input.UnsignedDecimal", _initialProps, Actions, hasStyles = false)

and Actions(_this: UnsignedDecimal) =
    class end

let Make = makeConstructor<UnsignedDecimal, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate