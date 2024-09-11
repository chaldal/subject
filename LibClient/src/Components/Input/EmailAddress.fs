[<AutoOpen>]
module LibClient.Components.Input_EmailAddress

open Fable.React
open LibClient
open LibClient.Components.Input
open ReactXP.Components
open ReactXP.Styles
open LibClient.Components

let parseProp (raw: Option<NonemptyString>): Result<Option<EmailAddress>, string> =
    match raw with
    | None -> Ok None
    | Some (NonemptyString nonemptyRaw) ->
        EmailAddress.tryOfString nonemptyRaw
        |> Result.mapBoth Some (fun e ->
            match e with
            | EmailAddressValidationError.EmptyString -> "Cannot be empty"
            | NoAtSymbol                              -> "Must contain an '@' symbol"
            | MultipleAtSymbols                       -> "Cannot contain multiple '@' symbols"
            | AtSymbolAtStart                         -> "'@' symbol cannot appear first"
            | AtSymbolAtEnd                           -> "'@' symbol cannot appear last"
            | NotAValidEmail                          -> "Not a valid email address")

module LC =
    module Input =
        module EmailAddressTypes =
            type Theme = {
                BorderLabelBlurredColor: Color
                BorderLabelFocusedColor: Color
                BorderLabelInvalidColor: Color
                TextColor: Color
                NoneditableTextColor: Color
                NoneditableBackgroundColor: Color
                InvalidReasonColor: Color
                PlaceholderColor: Color
                TheVerticalPadding: int
            }
            type Value = ParsedText.Value<EmailAddress>
            let parse (raw: Option<NonemptyString>) : Value =
                Value.OfRaw parseProp raw

            let wrap (value: EmailAddress) : Value = Value.Wrap (value, value.Value.ToString())

            let empty = parse None

open LC.Input.EmailAddressTypes

module private Styles =
    let legacyParsedText (theme: Theme) =
        ParsedTextStyles.Theme.One (theme.BorderLabelBlurredColor, theme.BorderLabelFocusedColor, theme.BorderLabelInvalidColor, theme.TextColor, theme.NoneditableTextColor, theme.NoneditableBackgroundColor, theme.InvalidReasonColor, theme.PlaceholderColor, theme.TheVerticalPadding)
        |> legacyTheme

type LibClient.Components.Constructors.LC.Input with
    [<Component>]
    static member EmailAddress(
        value:                Value,
        validity:             InputValidity,
        onChange:             Value -> unit,
        ?requestFocusOnMount: bool,
        ?label:               string,
        ?placeholder:         string,
        ?prefix:              string,
        ?suffix:              InputSuffix,
        ?tabIndex:            int,
        ?onKeyPress:          Browser.Types.KeyboardEvent -> unit,
        ?onEnterKeyPress:     ReactEvent.Keyboard -> unit,
        ?styles:              array<ViewStyles>,
        ?theme:               Theme -> Theme,
        ?key:                 string,
        ?xLegacyStyles:       List<ReactXP.LegacyStyles.RuntimeStyles>
    ): ReactElement =
        key |> ignore

        let requestFocusOnMount = defaultArg requestFocusOnMount false
        let theTheme = Themes.GetMaybeUpdatedWith theme
        let xLegacyStyles = defaultArg xLegacyStyles []

        if xLegacyStyles.IsNonempty then
            Fable.Core.JS.console.warn "LC.Input.EmailAddress is being used with legacy styles. Please update all usages to use styles rather than classes."

        LC.Input.ParsedText(
            xLegacyStyles = (Styles.legacyParsedText theTheme |> List.append xLegacyStyles),
            keyboardType = ParsedText.KeyboardType.EmailAddress,
            onChange = onChange,
            requestFocusOnMount = requestFocusOnMount,
            validity = validity,
            value = value,
            parse = parseProp,
            ?onEnterKeyPress = onEnterKeyPress,
            ?onKeyPress = onKeyPress,
            ?tabIndex = tabIndex,
            ?suffix = suffix,
            ?prefix = prefix,
            ?placeholder = placeholder,
            ?label = label,
            ?styles = styles
        )