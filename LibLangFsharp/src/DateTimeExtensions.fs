[<AutoOpen>]
module DateTimeExtensions

open System
open System.Globalization

type TimeSpan with
    member this.TruncateMillis : TimeSpan =
        this.Subtract (TimeSpan.FromMilliseconds (float this.Milliseconds))

    member this.Multiply (factor: float) : TimeSpan =
        (float this.Ticks * factor)
        |> int64
        |> TimeSpan.FromTicks

    static member ToDisplayString (value: System.TimeSpan) : string =
        let (hours, period) =
            match value.Hours with
            | hours when hours < 12 -> (hours,      "AM")
            | hours                 -> (hours % 12, "PM")

        let twelveAdjustedHours =
            match hours with
            | 0 -> 12
            | _ -> hours

        sprintf "%i:%02i %s" twelveAdjustedHours value.Minutes period

type DateTimeOffset with
    member this.SubtractDays (days: float) : DateTimeOffset =
        this.AddDays -days

    member this.EndOfDay : DateTimeOffset =
        // AddTicks -1 should actually be the right answer
        // but JS doesn't support resolution below ms
        this.BeginningOfDay.AddDays(1.).AddMilliseconds(-1.)

    member this.BeginningOfDay : DateTimeOffset =
        DateTimeOffset(this.Date, this.Offset)

    /// Converts this `DateTimeOffset` to a `DateOnly`. The offset of this `DateTimeOffset` is applied when determining the date.
    member this.ToDateOnly() =
        DateOnly.FromDateTime(this.DateTime)

    /// Creates `DateTimeOffset` from `DateOnly` and given timezone offset
    static member ofDateOnly (date: DateOnly) (offset: TimeSpan) : DateTimeOffset =
        DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, offset)

    static member Split (value: DateTimeOffset) : DateOnly * TimeSpan =
        (DateOnly.FromDateTime value.Date, value.TimeOfDay)

type DateOnly with
    /// Convert this `DateOnly` to a `DateTimeOffset` using the specified `time` (defaults to midnight) and `offset` (defaults to UTC).
    member this.ToDateTimeOffset(?time: TimeOnly, ?offset: TimeSpan) =
        let time =
            time
            |> Option.defaultValue (TimeOnly(0, 0))

        let offset =
            offset
            |> Option.defaultValue (TimeSpan.Zero)

        DateTimeOffset(this.ToDateTime(time), offset)

type DayOfTheWeek =
| Monday
| Tuesday
| Wednesday
| Thursday
| Friday
| Saturday
| Sunday
with
    static member ofEnum (source: DayOfWeek) =
        match source with
        | DayOfWeek.Monday    -> Monday
        | DayOfWeek.Tuesday   -> Tuesday
        | DayOfWeek.Wednesday -> Wednesday
        | DayOfWeek.Thursday  -> Thursday
        | DayOfWeek.Friday    -> Friday
        | DayOfWeek.Saturday  -> Saturday
        | DayOfWeek.Sunday    -> Sunday
        | _                   -> failwith "Invalid day of week"

    static member toEnum (source: DayOfTheWeek) =
        match source with
        | Monday    -> DayOfWeek.Monday
        | Tuesday   -> DayOfWeek.Tuesday
        | Wednesday -> DayOfWeek.Wednesday
        | Thursday  -> DayOfWeek.Thursday
        | Friday    -> DayOfWeek.Friday
        | Saturday  -> DayOfWeek.Saturday
        | Sunday    -> DayOfWeek.Sunday

module DayOfTheWeek =
    let toShortDayString (dayOfTheWeek: DayOfTheWeek) =
        // FIXME this doesn't take into account culture
        // But at this point Fable appears to have issues with culture
        match dayOfTheWeek with
        | Monday ->    "Mon"
        | Tuesday ->   "Tue"
        | Wednesday -> "Wed"
        | Thursday ->  "Thu"
        | Friday ->    "Fri"
        | Saturday ->  "Sat"
        | Sunday ->    "Sun"

    let tryOfShortDayString (value: string) : DayOfTheWeek option =
        match value with
        | "Mon" -> Some DayOfTheWeek.Monday
        | "Tue" -> Some DayOfTheWeek.Tuesday
        | "Wed" -> Some DayOfTheWeek.Wednesday
        | "Thu" -> Some DayOfTheWeek.Thursday
        | "Fri" -> Some DayOfTheWeek.Friday
        | "Sat" -> Some DayOfTheWeek.Saturday
        | "Sun" -> Some DayOfTheWeek.Sunday
        | _     -> None

[<Struct>]
[<StructuralEquality>]
[<StructuralComparison>]
type Date = private {
    // WARNING order of fields is important for correct structural comparison
    Year_:  int
    Month_: int
    Day_:   int
} with
    member this.Year  : int = this.Year_
    member this.Month : int = this.Month_
    member this.Day   : int = this.Day_

module Date =
    let ofYearMonthDay (year: int, monthOneBased: int, day: int) : Date =
        if monthOneBased < 1 || monthOneBased > 12 then failwith "Invalid month"
        if day < 1 || DateTime.DaysInMonth (year, monthOneBased) < day then failwith "Invalid day"
        {
            Year_  = year
            Month_ = monthOneBased
            Day_   = day
        }

    let ofDateTimeOffset (source: DateTimeOffset) : Date =
        {
            Year_  = source.Year
            Month_ = source.Month
            Day_   = source.Day
        }

    let toDateTimeOffset (offset: TimeSpan) (source: Date) : DateTimeOffset =
        DateTimeOffset(source.Year, source.Month, source.Day, 0, 0, 0, offset)

    let ofDateOnly (source: DateOnly) : Date =
        {
            Year_ = source.Year
            Month_ = source.Month
            Day_   = source.Day
        }

    let toDateOnly (source: Date) : DateOnly =
        DateOnly(source.Year, source.Month, source.Day)

    let toDateTimeOffsetWithTime (time: TimeOnly) (offset: TimeSpan) (source: Date) : DateTimeOffset =
        DateTimeOffset(source.Year, source.Month, source.Day, time.Hour, time.Minute, time.Second, offset)

    let addDays (daysToAdd: int) (date: Date) : Date =
        date
        |> toDateTimeOffset TimeSpan.Zero
        |> fun dt -> dt.AddDays(float daysToAdd)
        |> ofDateTimeOffset

    let dayOfTheWeek (date: Date) : DayOfTheWeek =
        date
        |> toDateTimeOffset TimeSpan.Zero
        |> fun dt -> dt.DayOfWeek
        |> DayOfTheWeek.ofEnum

    let generateNextWeek (date: Date) : seq<Date> =
        seq { for i in 0 .. 7 -> (addDays i date) }

    #if !FABLE_COMPILER
    let weekOfTheYear (date: Date) : uint32 =
        let cultureInfo = CultureInfo.InvariantCulture
        let dateTime =
            date
            |> toDateTimeOffset TimeSpan.Zero
            |> fun dt -> dt.DateTime

        cultureInfo.Calendar.GetWeekOfYear(
            dateTime,
            cultureInfo.DateTimeFormat.CalendarWeekRule,
            cultureInfo.DateTimeFormat.FirstDayOfWeek
        )
        |> uint32

    let toMonthName ({ Year_ = _year; Month_ = month; Day_ = _day }: Date) : string =
        CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(month)
    #endif

    let isLeapYear ({ Year_ = year; Month_ = _month; Day_ = _day }: Date) : bool =
      if year % 100 = 0 then year % 400 = 0 else year % 4 = 0

    let toComparableNumber ({ Year_ = year; Month_ = month; Day_ = day }: Date) : int64 =
        (int64 year) * 10000L + (int64 month) * 100L + (int64 day)

type Date with
    member this.DayOfTheWeek : DayOfTheWeek =
        this |> Date.dayOfTheWeek
    #if !FABLE_COMPILER
    member this.MonthName : string =
        this |> Date.toMonthName
    #endif

    static member ( - ) (left: Date, right: Date) : TimeSpan =
        ((Date.toDateTimeOffset TimeSpan.Zero left) - (Date.toDateTimeOffset TimeSpan.Zero right))
    static member op_GreaterThanOrEqual (left: Date, right: Date) : bool =
        struct(left.Year, left.Month, left.Day) >= struct(right.Year, right.Month, right.Day)
    static member op_LessThanOrEqual (left: Date, right: Date) : bool =
        struct(left.Year, left.Month, left.Day) <= struct(right.Year, right.Month, right.Day)

let bdTzOffset = TimeSpan(6, 0, 0)

let toBdt (value: DateTimeOffset) =
    if value.Offset = bdTzOffset then value else value.ToOffset(bdTzOffset)

let startOfDayBdt (value: DateTimeOffset) =
    DateTimeOffset((toBdt value).Date, bdTzOffset)

#if !FABLE_COMPILER

open CodecLib
open FSharpPlus

type DayOfTheWeek
with
    static member get_Codec () : Codec<'RawEncoding, DayOfTheWeek> =
        (
            Codec.create
                (tryParse >> Option.map DayOfTheWeek.ofEnum >> Result.ofOption (Uncategorized "Not a day of the week"))
                (DayOfTheWeek.toEnum >> string)
        )
        |> Codec.compose Codecs.string

type Date with
    static member get_Codec () : Codec<_, Date> =
        (
            Codec.create
                (trySscanf "%i-%i-%i" >=> Option.protect Date.ofYearMonthDay >> Result.ofOption (Uncategorized "Wrong date"))
                (fun (d: Date) -> sprintf "%i-%i-%i" d.Year d.Month d.Day)
        )
        |> Codec.compose Codecs.string


#endif
