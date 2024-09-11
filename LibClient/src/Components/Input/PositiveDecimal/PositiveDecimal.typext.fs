module LibClient.Components.Input.PositiveDecimal

open LibClient
open ReactXP.Styles
open LibClient.Input
open System.Text.RegularExpressions

let private onlyValidCharacters = Regex "^[0-9.]*$"

type PropSuffixFactory = InputSuffixFactory

let parseProp (raw: Option<NonemptyString>): Result<Option<Positive.PositiveDecimal>, string> =
    match raw with
    | None -> Ok None
    | Some (NonemptyString nonemptyRaw) ->
        let hasOnlyValidCharacters : bool = onlyValidCharacters.IsMatch nonemptyRaw
        let parseResult = System.Decimal.ParseOption nonemptyRaw
        match (parseResult, hasOnlyValidCharacters) with
        | (None, _)
        | (_, false) -> Error "Only numbers and period allowed"
        | (Some value, true) ->
            match Positive.PositiveDecimal.ofDecimal value with
            | None                 -> Error "Allowed only positive numbers"
            | Some positiveDecimal -> positiveDecimal |> Some |> Ok

type Value = LibClient.Components.Input.ParsedText.Value<Positive.PositiveDecimal>

let parse (raw: Option<NonemptyString>) : Value =
    Value.OfRaw parseProp raw

let wrap (value: Positive.PositiveDecimal) : Value = Value.Wrap (value, value.Value.ToString())

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

type PositiveDecimal(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, PositiveDecimal>("LibClient.Components.Input.PositiveDecimal", _initialProps, Actions, hasStyles = false)

and Actions(_this: PositiveDecimal) =
    class end

let Make = makeConstructor<PositiveDecimal, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate