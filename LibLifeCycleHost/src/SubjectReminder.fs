module LibLifeCycleHost.SubjectReminder

open System
open Orleans
open Orleans.Runtime

[<Literal>]
let SubjectReminderName = "nextTick"

[<Literal>]
let MetaKeepAliveReminderName = "metaKeepAliveTick"

// Technically we don't care for the period value (i.e. the time after which the reminder fires again) because we need only
// first reminder tick but instead of setting it to some arbitrarily large value, we need to set it to a small value
// This is because in some scenarios (listed below) reminder misses the initial tick when it is supposed to fire at, and instead fire at the time + period value.
// To mitigate this possibility, we can set the period to a small value, so even if the reminder time is missed, the reminder + period time isn't
// too far off from the original. Generally, once a timer has fired, we can expect the reminder to get deleted or rescheduled,
// so this period shouldn't be an issue. We'll set the value to the minimum value that Orleans allows (1 minute).
// Here's the list of known scenarios when initial tick can be missed, all mitigated:
// Scenario 1: initial tick delivered earlier than requested
// Mitigation: reminder delayed by subjectReminderImplicitDelayToReduceEarlyTicks, but if early tick still happens it is rescheduled for the same time
// Scenario 2: SubjectGrain schedules live reminder too early / immediately due
// Mitigation: liveSubjectReminderMinDelay applies for any new live reminder so it's not immediate
// Scenario 3: SubjectReminderTable loads a reminder that is overdue or immediately due
// Mitigation: initial reminder time is overridden to be now + subjectReminderTableReminderMinDelay
// Scenario 4: system was offline / there was outage
// Mitigation: it's handled by SubjectReminderTable as described above
let defaultReminderPeriod = TimeSpan.FromMinutes 1. // This is the minimum allowed value

// As name suggests, each reminder is implicitly delayed by at least this amount, even if it's time is far in the future,
// this is to minimize incidence of ticks that fire early (and hence reduce reschedule churn).
// One example why tick might be delivered earlier than requested is clock difference between silo that owns the reminder
// and silo that owns the subject grain activation (they can be different), or just odd system clock drift
let subjectReminderImplicitDelayToReduceEarlyTicks = TimeSpan.FromMilliseconds 50.

// Look ahead limit is set to be Orleans 3.x's hardcode of Orleans.Runtime.Constants.RefreshReminderList = TimeSpan.FromMinutes(5),
// plus padding of 2 minutes to be safer in case if refresh is delayed for some reason.
let subjectReminderTableLookAheadLimit = TimeSpan.FromMinutes 7

// When reading new due reminder from SubjectReminderTable we override it to be "Asap" to avoid being delayed by up to 60 seconds before it actually fires,
// but it will be at least 2 sec into future so we don't stress Orleans too much and not miss the overridden tick too
let subjectReminderTableNewDueReminderMinDelay = TimeSpan.FromSeconds 2.0

let subjectReminderTableIsNewDueReminderBestGuess (initialBucketQuery: bool) (now: DateTimeOffset) (nextTickOn: DateTimeOffset) =
    if initialBucketQuery then
        nextTickOn < now + subjectReminderTableNewDueReminderMinDelay
    else
        // due reminder tolerance is higher for refresh bucket query, chances are this reminder is already live / not new and will fire in a moment, no need to delay it further
        // however if its time passed or nearly passed then it's actually did not / may not fire on time, so assume it's new
        // TODO: review 100ms padding based on actual "tick latency" traces in telemetry. Previous was 0ms and it did result in one 60 sec delay in 24h for a timer-heavy system
        nextTickOn < now + TimeSpan.FromMilliseconds 100

// Reminder table can overlook a reminder set for up to look ahead limit + Orleans.Runtime.Constants.RefreshReminderList = TimeSpan.FromMinutes(5) into the future.
// It's not clear why look ahead alone is not enough since it is already greater than refresh interval, but tick latency telemetry fits the theory
let liveSubjectReminderScheduleAheadLimit = subjectReminderTableLookAheadLimit + TimeSpan.FromMinutes 5.

// When scheduling a live reminder, do it at least 1 sec into future so we don't stress Orleans too much and not miss the initial tick
let liveSubjectReminderMinDelay = TimeSpan.FromSeconds 1.0

// This will apply only for volatile subjects, persisted live reminder delay is cut off at liveReminderScheduleAheadLimit
let liveSubjectReminderMaxDelay = TimeSpan.FromDays 21.0

let newReminderEntry (reminderName: string) (grainRef: GrainReference) (startAt: DateTimeOffset) =
    ReminderEntry(
        GrainRef     = grainRef,
        StartAt      = startAt.UtcDateTime,
        Period       = defaultReminderPeriod,
        ReminderName = reminderName,
        ETag         = Guid.NewGuid().ToString()
    )

let getSubjectGrainReminderToUnregister (grain: Grain) : IGrainReminder =
    let arbitraryStartAt = DateTimeOffset.UnixEpoch // it's not passed to IGrainReminder anyway, can be anything
    let reminderEntry = newReminderEntry SubjectReminderName grain.GrainReference arbitraryStartAt
    // This call to an internal orleans methods will allow us to avoid one DB lookup, each time we clear the timer
    // Of course, this makes us more brittle in case Orleans changes internals, that's manageable.
    // Worst case, we can always do grain.GetReminder from within the grain
    typeof<ReminderEntry>.GetMethod("ToIGrainReminder", Reflection.BindingFlags.NonPublic ||| Reflection.BindingFlags.Instance)
        .Invoke(reminderEntry, Array.empty)
        :?> IGrainReminder
