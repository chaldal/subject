module LibClient.Components.Input.LocalTime

open System
open LibClient

type Field =
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

let private validateHours (hours: UnsignedInteger) : Result<UnsignedInteger, Field * string> =
    if 0 < hours.Value && hours.Value < 13 then
        Ok hours
    else
        Error (Hours, "Hours should be between 1 and 12")

let private validateMinutes (minutes: UnsignedInteger) : Result<UnsignedInteger, Field * string> =
    if minutes.Value < 60 then
        Ok minutes
    else
        Error (Minutes, "Minutes should be between 0 and 59")

let private computeResult (rawHours: Option<NonemptyString>, rawMinutes: Option<NonemptyString>, periodOffset: int) : Result<Option<TimeSpan>, Field * string> = resultful {
    let! maybeHours   = parseField Hours   rawHours
    let! maybeMinutes = parseField Minutes rawMinutes

    match (maybeHours, maybeMinutes) with
    | (Some hours, Some minutes) ->
        let! validHours   = validateHours   hours
        let! validMinutes = validateMinutes minutes

        return
            (TimeSpan.FromHours (float ((validHours.Value % 12) + periodOffset))) + (TimeSpan.FromMinutes (float validMinutes.Value))
            |> Some

    | (Some hours, None) ->
        let! _validHours = validateHours hours
        return None

    | (None, Some minutes) ->
        let! _validMinutes = validateMinutes minutes
        return None

    | _ -> return None
}

let periodPickerItems: List<LibClient.Components.Legacy.Input.Picker.PickerItem<int>> = [
    { Label = "AM"; Item = 0  }
    { Label = "PM"; Item = 12 }
]

type Period = {
    Label: string 
    Item:  int
}
let periodPickerItemsList: List<Period> = [
    { Label = "AM"; Item = 0  }
    { Label = "PM"; Item = 12 }
]

type Value = {
    Raw:                      Option<NonemptyString> * Option<NonemptyString> * int
    InternalValidationResult: Option<Field * string>
    Result:                   Option<TimeSpan>
} with
    member this.InternalFieldValidity (field: Field) : InputValidity =
        match this.InternalValidationResult with
        | Some (errField, _message) when errField = field -> Missing
        | _                                              -> Valid

    member this.InternalValidity : InputValidity =
        match this.InternalValidationResult with
        | Some (_, message) -> Invalid message
        | _                 -> Valid

    static member FromRaw (raw: Option<NonemptyString> * Option<NonemptyString> * int) : Value =
        let result = computeResult raw
        {
            Raw                      = raw
            InternalValidationResult = result |> Result.invert |> Result.toOption
            Result                   = result |> Result.toOption |> Option.getOrElse None
        }

    member this.SetPeriod (value: int) : Value =
        let (hours, minutes, _) = this.Raw
        Value.FromRaw (hours, minutes, value)

    member this.SetHours (value: Option<NonemptyString>) : Value =
        let (_, minutes, periodOffset) = this.Raw
        Value.FromRaw (value, minutes, periodOffset)

    member this.SetMinutes (value: Option<NonemptyString>) : Value =
        let (hours, _, periodOffset) = this.Raw
        Value.FromRaw (hours, value, periodOffset)

    override this.ToString () : string =
        let (hours, minutes, periodOffset) = this.Raw
        let hours   = hours   |> Option.mapOrElse "00" (fun h -> h.Value)
        let minutes = minutes |> Option.mapOrElse "00" (fun m -> m.Value)
        let periodOffset = if periodOffset = 0 then "AM" else "PM"
        sprintf "%s:%s %s" hours minutes periodOffset

let wrap (value: TimeSpan) : Value =
    let (hours, periodOffset) =
        match value.Hours with
        | hours when hours < 12 -> (hours,      0)
        | hours                 -> (hours % 12, 12)

    let twelveAdjustedHours =
        match hours with
        | 0 -> 12
        | _ -> hours

    Value.FromRaw (
        twelveAdjustedHours.ToString() |> NonemptyString.ofString,
        sprintf "%02i" value.Minutes   |> NonemptyString.ofString,
        periodOffset
    )

let empty : Value = {
    Raw                      = (None, None, 0)
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

    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate = {
    IsFocused: bool
}

type LocalTime(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, LocalTime>("LibClient.Components.Input.LocalTime", _initialProps, Actions, hasStyles = true)

    let mutable maybeHoursInput: Option<LibClient.Components.Input.Text.ITextRef> = None
    member _.MaybeHoursInput
        with get () = maybeHoursInput
        and  set (value: Option<LibClient.Components.Input.Text.ITextRef>) : unit =
             maybeHoursInput <- value

    override _.GetInitialEstate(_initialProps: Props) : Estate = {
        IsFocused = false
    }

and Actions(this: LocalTime) =
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

let Make = makeConstructor<LocalTime, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate