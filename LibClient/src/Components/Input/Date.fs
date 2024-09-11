[<AutoOpen>]
module LibClient.Components.Input_Date

open System
open System.Text.RegularExpressions
open Fable.React
open LibClient
open LibClient.Components
open LibClient.Icons
open ReactXP.Components
open ReactXP.Styles

// TODO: delete after RenderDSL migration
type PropSuffixFactory = InputSuffixFactory

[<RequireQualifiedAccess>]
module private Helpers =
    // Fable does not yet support DateTime parsing of any sort, so we need to translate the provided format
    // into a regular expression for parsing user input.
    let private regexForFormat (_format: string) : Regex =
        // TODO TODO TODO
        // Supports: d, M, y

        // TODO: construct appropriate regex from format, allow whitespace etc.
        Regex("^(\\d{1,2})/(\\d{1,2})/(\\d{2,4})$")

    let private parseDateOnly (input: string) (format: string) : Option<DateOnly> =
        let regex = regexForFormat format
        let m = regex.Match(input)

        if m.Success then
            let day = int m.Groups.[1].Value
            let month = int m.Groups.[2].Value
            let year = int m.Groups.[3].Value

            let result = DateOnly(year, month, day)

            // Detect overflow (e.g. entering 13 for month will +1 the year)
            if result.Day = day && result.Month = month && result.Year = year then
                result
                |> Some
            else
                None
        else
            None

    let parseProp
            (maybeMinDate: Option<DateOnly>)
            (maybeMaxDate: Option<DateOnly>)
            (maybeCanSelectDate: Option<DateOnly -> bool>)
            (raw: Option<NonemptyString>) : Result<Option<DateOnly>, string> =
        match raw with
        | None -> Ok None
        | Some (NonemptyString nonemptyRaw) ->
            let parsedValue = parseDateOnly nonemptyRaw "TODO"

            match parsedValue with
            | Some value ->
                let isOverMinDate =
                    maybeMinDate
                    |> Option.map (fun minDate -> value >= minDate)
                    |> Option.defaultValue true
                let isUnderMaxDate =
                    maybeMaxDate
                    |> Option.map (fun maxDate -> value <= maxDate)
                    |> Option.defaultValue true
                let canSelectDate =
                    maybeCanSelectDate
                    |> Option.map (fun canSelectDate -> canSelectDate value)
                    |> Option.defaultValue true

                if not isOverMinDate then
                    Error "Date falls before minimum allowed"
                else if not isUnderMaxDate then
                    Error "Date falls after maximum allowed"
                else if not canSelectDate then
                    Error "Date cannot be selected"
                else
                    value |> Some |> Ok
            | None -> Error "Invalid date"

module LC =
    module Input =
        module DateTypes =
            type DateOnly = System.DateOnly

            type Value = LibClient.Components.Input.ParsedText.Value<DateOnly>

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
                CalendarButtonColor: Color
                CalendarButtonBackgroundColor: Color
                CalendarButtonIconSize: int
            }

            let parse (raw: Option<NonemptyString>) : Value =
                Value.OfRaw (Helpers.parseProp None None None) raw

            let wrap (format: string) (value: DateOnly) : Value =
                Value.Wrap (
                    value,
                    value.ToDateTimeOffset(offset = DateTimeOffset.Now.Offset).ToString(format)
                )

            let empty = parse None

open LC.Input.DateTypes

[<RequireQualifiedAccess>]
module private Styles =
    let popup =
        makeViewStyles {
            borderRadius    10
            shadow          (Color.BlackAlpha 0.3) 5 (0, 2)
            backgroundColor Color.White
            padding         10
        }

    let iconButton =
        ViewStyles.Memoize(
            fun (theme: Theme) ->
                makeViewStyles {
                    backgroundColor theme.CalendarButtonBackgroundColor
                    opacity         0.7
                }
        )

    let legacyParsedText (theme: Theme) =
        LibClient.Components.Input.ParsedTextStyles.Theme.One (theme.BorderLabelBlurredColor, theme.BorderLabelFocusedColor, theme.BorderLabelInvalidColor, theme.TextColor, theme.NoneditableTextColor, theme.NoneditableBackgroundColor, theme.InvalidReasonColor, theme.PlaceholderColor, theme.TheVerticalPadding)
        |> legacyTheme

    let iconButtonTheme (theTheme: Theme) (theme: LC.IconButton.Theme): LC.IconButton.Theme =
        { theme with
            Actionable =
                { theme.Actionable with
                    IconColor = theTheme.CalendarButtonColor
                    IconSize = theTheme.CalendarButtonIconSize
                }
        }

type LibClient.Components.Constructors.LC.Input with
    [<Component>]
    static member Date(
                value: Value,
                validity: InputValidity,
                onChange: Value -> unit,
                ?valueFormat: string,
                ?label: string,
                ?placeholder: string,
                ?requestFocusOnMount: bool,
                ?onKeyPress: (Browser.Types.KeyboardEvent -> unit),
                ?onEnterKeyPress: (ReactEvent.Keyboard -> unit),
                ?minDate: DateOnly,
                ?maxDate: DateOnly,
                ?canSelectDate: (DateOnly -> bool),
                ?styles: array<ViewStyles>,
                ?theme:   Theme -> Theme,
                ?key: string
            ) : ReactElement =
        key |> ignore

        let valueFormat = defaultArg valueFormat "dd/MM/yyyy"
        let placeholder = defaultArg placeholder valueFormat
        let requestFocusOnMount = defaultArg requestFocusOnMount false
        let theTheme = Themes.GetMaybeUpdatedWith theme

        let popupConnectorState = Hooks.useState (LibClient.Components.Popup.Connector())

        let toggle (a: ReactEvent.Action) : unit =
            a.MaybeSource
            |> Option.iter (fun source ->
                // TODO: this doesn't currently work right, likely because clicking the button immediately hides the popup
                match popupConnectorState.current.IsShown () with
                | false -> popupConnectorState.current.Show source
                | true -> popupConnectorState.current.Hide ()
            )

        let inputSuffix =
            LC.IconButton(
                theme = Styles.iconButtonTheme theTheme,
                styles = [| Styles.iconButton theTheme |],
                icon = Icon.Calendar,
                state = ButtonHighLevelState.LowLevel (ButtonLowLevelState.Actionable toggle)
            )

        RX.View(
            styles = (styles |> Option.defaultValue [||] ),
            children =
                elements {
                    LC.Input.ParsedText(
                        xLegacyStyles = Styles.legacyParsedText theTheme,
                        parse = Helpers.parseProp minDate maxDate canSelectDate,
                        value = value,
                        validity = validity,
                        placeholder = placeholder,
                        requestFocusOnMount = requestFocusOnMount,
                        onChange = onChange,
                        suffix = InputSuffix.Element inputSuffix,
                        ?label = label,
                        ?onKeyPress = onKeyPress,
                        ?onEnterKeyPress = onEnterKeyPress
                    )

                    LC.Popup(
                        connector = popupConnectorState.current,
                        render =
                            fun () ->
                                RX.View(
                                    styles = [| Styles.popup |],
                                    // XXX it looks like the component is rendered once and then cut
                                    // off from the world, i.e. updates to props.Value do not propagate
                                    // to it. I think that's how ReactXP's popups infrastructure is set up,
                                    // though it seems strange and unlikely.
                                    children =
                                        elements {
                                            LC.DateSelector(
                                                onChange =
                                                    (fun date ->
                                                        wrap valueFormat date |> onChange
                                                        popupConnectorState.current.Hide()
                                                    ),
                                                maybeSelected = (match value.Result with | Ok result -> result | Error _ -> None),
                                                ?minDate = minDate,
                                                ?maxDate = maxDate,
                                                ?canSelectDate = canSelectDate
                                            )
                                        }
                                )
                    )
                }
        )