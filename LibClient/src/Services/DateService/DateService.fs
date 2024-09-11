module LibClient.Services.DateService

type DateTimeOffset = System.DateTimeOffset
type DateOnly       = System.DateOnly
type TimeSpan       = System.TimeSpan

open System
open LibClient
open LibClient.JsInterop

type UniDateTime =
    private
    | UnixTime       of int64
    // What is missing is sometimes more important than what is present.
    // We specifically do not want to allow `DateTime`. It is evil, and should
    // never be part of our applications' regular modelling. Instead, if we are
    // unlucky enough to be handed one by the backend of a third party library,
    // we should sanitize them out and convert to `DateTimeOffset` at the earliest
    // possible point in the code.
    // | DateTime       of DateTime
    | DateTimeOffset of DateTimeOffset
    | DateOnly       of DateOnly
    | StringDate     of string
    with
        static member Of (raw: int64)          : UniDateTime = UnixTime       raw
        static member Of (raw: DateTimeOffset) : UniDateTime = DateTimeOffset raw
        static member Of (raw: DateOnly)       : UniDateTime = DateOnly       raw
        static member Of (raw: string)         : UniDateTime = StringDate     raw
        static member Of (raw: Date)           : UniDateTime = DateTimeOffset (DateTimeOffset.Parse(sprintf "%i-%02i-%02i" raw.Year raw.Month raw.Day))

        member this.ToDateTimeOffset : DateTimeOffset =
            match this with
            | UnixTime       millis         -> System.DateTimeOffset.FromUnixTimeMilliseconds(millis)
            | DateTimeOffset dateTimeOffset -> dateTimeOffset
            | DateOnly       dateOnly       -> dateOnly.ToDateTimeOffset(offset = DateTimeOffset.Now.Offset)
            | StringDate     sd             -> DateTimeOffset.Parse(sd)

type UniDateTimePropFactory =
        static member Make (raw: int64)          = UniDateTime.Of raw
        static member Make (raw: DateTimeOffset) = UniDateTime.Of raw
        static member Make (raw: string)         = UniDateTime.Of raw
        static member Make (raw: Date)           = UniDateTime.Of raw

let formatDateWithOffset (format: string) (date: UniDateTime) (offset: TimeSpan) : string =
    // NOTE when we do localization, we can externalize this engishUS business
    Date.Format.localFormat Date.Local.englishUS format ((date.ToDateTimeOffset.ToOffset offset)).DateTime

let formatDate (format: string) (date: UniDateTime) : string =
    // NOTE when we do localization, we can externalize this engishUS business
    Date.Format.localFormat Date.Local.englishUS format (date.ToDateTimeOffset.UtcDateTime)


type DateService() =
    let subscriptionImplementationToday = Subscription.AdHocSubscriptionImplementation<DateTimeOffset>(Some DateTimeOffset.Now, None)
    let subscriptionImplementationNow   = Subscription.AdHocSubscriptionImplementation<DateTimeOffset>(Some DateTimeOffset.Now, None)

    let rec cycleToday () : unit =
        let now = DateTimeOffset.Now
        if Some now <> subscriptionImplementationToday.LatestValue then
            subscriptionImplementationToday.Update now
        runLater (TimeSpan.FromMinutes 1.) cycleToday

    let rec cycleNow () : unit =
        let now = DateTimeOffset.Now
        if Some now <> subscriptionImplementationNow.LatestValue then
            subscriptionImplementationNow.Update now
        runLater (TimeSpan.FromSeconds 1.) cycleNow

    do
        cycleToday()
        cycleNow()

    member _.GetToday : DateTimeOffset =
        subscriptionImplementationToday.LatestValue
        |> Option.getOrElse DateTimeOffset.Now

    member _.SubscribeToToday (subscriber: (DateTimeOffset) -> unit) : Subscription.SubscribeResult =
        subscriptionImplementationToday.Subscribe subscriber

    member _.GetNow : DateTimeOffset =
        subscriptionImplementationNow.LatestValue
        |> Option.getOrElse DateTimeOffset.Now

    member _.SubscribeToNow (subscriber: (DateTimeOffset) -> unit) : Subscription.SubscribeResult =
        subscriptionImplementationNow.Subscribe subscriber

    member _.TestingOnlySetToday (today: DateTimeOffset) : unit =
        subscriptionImplementationToday.Update today
