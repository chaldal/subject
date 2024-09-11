[<AutoOpen>]
module LibClient.Components.Input_PhoneNumber

open Fable.React
open LibClient
open LibClient.Components.Input
open ReactXP.Components
open ReactXP.Styles
open LibClient.Components

let parseProp (raw: Option<NonemptyString>): Result<Option<PhoneNumber>, string> =
    match raw with
    | None -> Ok None
    | Some (NonemptyString nonemptyRaw) ->
        PhoneNumber.tryOfString nonemptyRaw
        |> Result.mapBoth Some (fun e -> e.ToDisplayString)

module LC =
    module Input =
        module PhoneNumberTypes =
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
            type Value = LibClient.Components.Input.ParsedText.Value<PhoneNumber>

            let parse (raw: Option<NonemptyString>) : Value =
                Value.OfRaw parseProp raw

            let wrap (value: PhoneNumber) : Value = Value.Wrap (value, value.Value.ToString())

            let empty = parse None

open LC.Input.PhoneNumberTypes

module private Styles =
    let legacyParsedText (theme: Theme) =
        ParsedTextStyles.Theme.One (theme.BorderLabelBlurredColor, theme.BorderLabelFocusedColor, theme.BorderLabelInvalidColor, theme.TextColor, theme.NoneditableTextColor, theme.NoneditableBackgroundColor, theme.InvalidReasonColor, theme.PlaceholderColor, theme.TheVerticalPadding)
        |> legacyTheme

type LibClient.Components.Constructors.LC.Input with
    [<Component>]
    static member PhoneNumber(
        value:                Value,
        validity:             InputValidity,
        onChange:             Value -> unit,
        ?label:               string,
        ?placeholder:         string,
        ?prefix:              string,
        ?suffix:              InputSuffix,
        ?requestFocusOnMount: bool,
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
            Fable.Core.JS.console.warn "LC.Input.PhoneNumber is being used with legacy styles. Please update all usages to use styles rather than classes."

        LC.Input.ParsedText(
            xLegacyStyles = (Styles.legacyParsedText theTheme |> List.append xLegacyStyles),
            keyboardType = ParsedText.KeyboardType.NumberPad,
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