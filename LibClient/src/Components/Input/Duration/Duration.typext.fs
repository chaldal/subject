module LibClient.Components.Input.Duration

open System
open LibClient

type Field =
| Days
| Hours
| Minutes

let private parseField (field: Field) (value: Option<NonemptyString>) : Result<Option<UnsignedInteger>, Field * string> =
    match value with
    | None -> Ok None
    | Some (NonemptyString nonemptyRaw) ->
        match System.Int32.ParseOption nonemptyRaw with
        | None -> Error (field, "Allowed numeric characters only")
        | Some value ->
            match Unsigned.UnsignedInteger.ofInt value with
            | None                 -> Error (field, "Allowed only positive numbers")
            | Some unsignedDecimal -> unsignedDecimal |> Some |> Ok
            
let private validateDays (days: UnsignedInteger) : Result<UnsignedInteger, Field * string> =
    Ok days

let private validateHours (hours: UnsignedInteger) : Result<UnsignedInteger, Field * string> =
    if hours.Value < 24 then
        Ok hours
    else
        Error (Hours, "Hours should be between 0 and 23")

let private validateMinutes (minutes: UnsignedInteger) : Result<UnsignedInteger, Field * string> =
    if minutes.Value < 60 then
        Ok minutes
    else
        Error (Minutes, "Minutes should be between 0 and 59")

let private computeResult (rawDays: Option<NonemptyString>, rawHours: Option<NonemptyString>, rawMinutes: Option<NonemptyString>) : Result<Option<TimeSpan>, Field * string> = resultful {
    let! maybeDays    = parseField Days    rawDays
    let! maybeHours   = parseField Hours   rawHours
    let! maybeMinutes = parseField Minutes rawMinutes

    match (maybeDays, maybeHours, maybeMinutes) with
    | (Some days, Some hours, Some minutes) ->
        let! validDays    = validateDays    days
        let! validHours   = validateHours   hours
        let! validMinutes = validateMinutes minutes

        return
            (TimeSpan.FromDays (float validDays.Value)) + (TimeSpan.FromHours (float validHours.Value)) + (TimeSpan.FromMinutes (float validMinutes.Value))
            |> Some

    | (None, Some hours, Some minutes) ->
        let! validHours   = validateHours   hours
        let! validMinutes = validateMinutes minutes

        return
            (TimeSpan.FromHours (float validHours.Value)) + (TimeSpan.FromMinutes (float validMinutes.Value))
            |> Some

    | (None, Some hours, None) ->
        let! _validHours = validateHours hours
        return None

    | (None, None, Some minutes) ->
        let! _validMinutes = validateMinutes minutes
        return None

    | _ -> return None
}

type Value = {
    Raw:                      Option<NonemptyString> * Option<NonemptyString> * Option<NonemptyString>
    InternalValidationResult: Option<Field * string>
    Result:                   Option<TimeSpan>
} with
    member this.InternalFieldValidity (field: Field) : InputValidity =
        match this.InternalValidationResult with
        | Some (errField, _message) when errField = field -> Missing
        | _                                               -> Valid

    member this.InternalValidity : InputValidity =
        match this.InternalValidationResult with
        | Some (_, message) -> Invalid message
        | _                 -> Valid

    static member FromRaw (raw: Option<NonemptyString> * Option<NonemptyString> * Option<NonemptyString>) : Value =
        let result = computeResult raw
        {
            Raw                      = raw
            InternalValidationResult = result |> Result.invert |> Result.toOption
            Result                   = result |> Result.toOption |> Option.getOrElse None
        }

    member this.SetDays (value: Option<NonemptyString>) : Value =
        let (_, hours, minutes) = this.Raw
        Value.FromRaw (value, hours, minutes)

    member this.SetHours (value: Option<NonemptyString>) : Value =
        let (days, _, minutes) = this.Raw
        Value.FromRaw (days, value, minutes)

    member this.SetMinutes (value: Option<NonemptyString>) : Value =
        let (days, hours, _) = this.Raw
        Value.FromRaw (days, hours, value)

    override this.ToString () : string =
        let (maybeDays, hours, minutes) = this.Raw
        let hours   = hours   |> Option.mapOrElse "00" (fun h -> h.Value)
        let minutes = minutes |> Option.mapOrElse "00" (fun m -> m.Value)

        match maybeDays with
        | Some days ->
            sprintf "%s:%s:%s" days.Value hours minutes
        | None ->
            sprintf "%s:%s" hours minutes

let wrap (value: TimeSpan) : Value =
    Value.FromRaw (
        value.Days.ToString() |> NonemptyString.ofString,
        value.Hours.ToString() |> NonemptyString.ofString,
        sprintf "%02i" value.Minutes |> NonemptyString.ofString
    )

let empty : Value = {
    Raw                      = (None, None, None)
    InternalValidationResult = None
    Result                   = None
}

type Props = (* GenerateMakeFunction *) {
    Label:               string                        option // defaultWithAutoWrap None
    Value:               Value
    Validity:            InputValidity
    OnChange:            Value -> unit
    OnEnterKeyPress:     (ReactEvent.Keyboard -> unit) option // defaultWithAutoWrap None
    RequestFocusOnMount: bool                                 // default             false
    ShouldDisplayDays:   bool                                 // default             false

    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate = {
    IsFocused: bool
}

type Duration(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, Duration>("LibClient.Components.Input.Duration", _initialProps, Actions, hasStyles = true)

    let mutable maybeDaysInput:  Option<LibClient.Components.Input.Text.ITextRef> = None
    let mutable maybeHoursInput: Option<LibClient.Components.Input.Text.ITextRef> = None

    member _.MaybeDaysInput
        with get () = maybeDaysInput
        and  set (value: Option<LibClient.Components.Input.Text.ITextRef>) : unit =
             maybeDaysInput <- value

    member _.MaybeHoursInput
        with get () = maybeHoursInput
        and  set (value: Option<LibClient.Components.Input.Text.ITextRef>) : unit =
             maybeHoursInput <- value

    override _.GetInitialEstate(_initialProps: Props) : Estate = {
        IsFocused = false
    }

and Actions(this: Duration) =
    member _.RefDaysInput (nullableInstance: LibClient.JsInterop.JsNullable<LibClient.Components.Input.Text.ITextRef>) =
        this.MaybeDaysInput <- nullableInstance.ToOption

    member _.RefHoursInput (nullableInstance: LibClient.JsInterop.JsNullable<LibClient.Components.Input.Text.ITextRef>) =
        this.MaybeHoursInput <- nullableInstance.ToOption

    member _.Focus (_e: Browser.Types.Event) : unit =
        this.MaybeHoursInput |> Option.sideEffect (fun input ->
            input.RequestFocus()
        )

    member _.OnFocus (_e: Browser.Types.FocusEvent) : unit =
        this.SetEstate (fun estate _ -> { estate with IsFocused = true })

    member _.OnBlur (_e: Browser.Types.FocusEvent) : unit =
        this.SetEstate (fun estate _ -> { estate with IsFocused = false })

let Make = makeConstructor<Duration, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate