module LibClient.Components.DateSelector

open System
open LibClient
open LibClient.ComponentDataSubscription
open LibClient.ServiceInstances

type DateOnly = System.DateOnly

type Props = (* GenerateMakeFunction *) {
    OnChange:      DateOnly -> unit
    MaybeSelected: Option<DateOnly>
    MinDate:       DateOnly option           // defaultWithAutoWrap None
    MaxDate:       DateOnly option           // defaultWithAutoWrap None
    CanSelectDate: (DateOnly -> bool) option // defaultWithAutoWrap None
}

type Estate = {
    Year:                 int
    Month:                int
    Rows:                 List<List<DateOnly>>
    CurrentMonthFirstDay: DateOnly
    CurrentDate:          DateOnly // we want only the current Date not the time.
}

let isPreviousMonthOutsideSelectableRange (firstDayOfCurrentMonth: DateOnly) (maybeMinDate: Option<DateOnly>) : bool =
    match maybeMinDate with
    | Some minDate ->
        let lastDayOfPreviousMonth = firstDayOfCurrentMonth.AddDays(-1)
        lastDayOfPreviousMonth < minDate
    | None -> false

let isNextMonthOutsideSelectableRange (firstDayOfCurrentMonth: DateOnly) (maybeMaxDate: Option<DateOnly>) : bool =
    match maybeMaxDate with
    | Some maxDate ->
        let firstDayOfNextMonth = firstDayOfCurrentMonth.AddMonths(1)
        firstDayOfNextMonth > maxDate
    | None -> false

let isOutsideSelectableRange (day: DateOnly) (maybeMinDate: Option<DateOnly>) (maybeMaxDate: Option<DateOnly>) : bool =
    match maybeMinDate, maybeMaxDate with
    | Some minDate, Some maxDate -> day < minDate || day > maxDate
    | Some minDate, None         -> day < minDate
    | None, Some maxDate         -> day > maxDate
    | None, None                 -> false

let canSelectDate (date: DateOnly) (maybeCanSelectDate: Option<DateOnly -> bool>) : bool =
    match maybeCanSelectDate with
    | Some canSelectDate -> canSelectDate date
    | None -> true

let buildRows (firstOfMonth: DateOnly) : List<List<DateOnly>> =
    let lastOfMonth    = firstOfMonth.AddMonths(1).AddDays(-1)
    let firstWeekStart = firstOfMonth.AddDays(- (int firstOfMonth.DayOfWeek))
    let lastWeekEnd    = lastOfMonth.AddDays(6 - (int lastOfMonth.DayOfWeek))
    let dayCount       = int (lastWeekEnd.ToDateTimeOffset(offset = DateTimeOffset.Now.Offset).Subtract(firstWeekStart.ToDateTimeOffset(offset = DateTimeOffset.Now.Offset)).TotalDays)
    [0..dayCount]
    // NOTE List.chunkBySize is not available in Fable, we get "export 'chunkBySize' was not found in '.fable/fable-library.2.3.3/List.js'
    // so using the Seq version with ugly conversions
    |> Seq.map firstWeekStart.AddDays
    |> Seq.chunkBySize 7
    |> Seq.map Seq.toList
    |> Seq.toList

type DateSelector(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, DateSelector>("LibClient.Components.DateSelector", _initialProps, Actions, hasStyles = true)

    let buildStateForMonth (firstOfMonth: DateOnly) : Estate =
        {
            Rows                 = buildRows firstOfMonth
            Year                 = firstOfMonth.Year
            Month                = firstOfMonth.Month
            CurrentMonthFirstDay = firstOfMonth
            CurrentDate          = services().Date.GetToday.ToDateOnly()
        }

    override _.GetInitialEstate (initialProps: Props) =
        let date = initialProps.MaybeSelected |> Option.getOrElse (DateTimeOffset.Now.ToDateOnly())
        let firstOfMonth = date.AddDays(1 - date.Day)
        buildStateForMonth firstOfMonth

    override this.ComponentDidMount() : unit =
        wireUpAdHocSubscriptionToComponent this
            (services().Date.SubscribeToToday
                (fun newToday -> this.SetEstate(fun estate _ -> { estate with CurrentDate = newToday.ToDateOnly() })))

    member this.UpdateStateForMonthIncrement (increment: int) : unit =
        this.SetEstate(fun estate _ ->
            buildStateForMonth (estate.CurrentMonthFirstDay.AddMonths(increment))
        )

and Actions(this: DateSelector) =
    member _.NextMonth (_e: ReactEvent.Action) : unit =
        this.UpdateStateForMonthIncrement 1

    member _.PrevMonth (_e: ReactEvent.Action) : unit =
        this.UpdateStateForMonthIncrement -1

let Make = makeConstructor<DateSelector,_,_>

// Unfortunately necessary boilerplate
type Pstate = NoPstate