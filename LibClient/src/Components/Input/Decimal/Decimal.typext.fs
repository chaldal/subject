module LibClient.Components.Input.Decimal

open LibClient
open System.Text.RegularExpressions
open ReactXP.Styles

let private onlyValidCharacters = Regex "^[\-0-9.]*$"

type PropSuffixFactory = InputSuffixFactory

let parseProp (raw: Option<NonemptyString>): Result<Option<decimal>, string> =
    match raw with
    | None -> Ok None
    | Some (NonemptyString nonemptyRaw) ->
        let hasOnlyValidCharacters : bool = onlyValidCharacters.IsMatch nonemptyRaw
        let parseResult = System.Decimal.ParseOption nonemptyRaw
        match (parseResult, hasOnlyValidCharacters) with
        | (None, _)
        | (_, false) -> Error "Only valid decimal values allowed"
        | (Some value, true) ->
            value |> Some |> Ok

type Value = LibClient.Components.Input.ParsedText.Value<decimal>

let parse (raw: Option<NonemptyString>) : Value =
    Value.OfRaw parseProp raw

let wrap (value: decimal) : Value = Value.Wrap (value, value.ToString())

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
    styles:              array<ViewStyles> option // defaultWithAutoWrap None

    key: string option // defaultWithAutoWrap JsUndefined
}

type Decimal(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Decimal>("LibClient.Components.Input.Decimal", _initialProps, Actions, hasStyles = false)

and Actions(_this: Decimal) =
    class end

let Make = makeConstructor<Decimal, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate