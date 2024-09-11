[<AutoOpen>]
module SuiteJobs.LifeCycles.RecurringJobLifeCycle

open System
open LibLifeCycle
open SuiteJobs.Types
open LibLifeCycle.LifeCycleAccessBuilder
open type AccessTo<RecurringJobAction, RecurringJobConstructor>

type RecurringJobEnvironment = {
    Clock: Service<Clock>
    Unique: Service<Unique>
} with interface Env

let private nextDueBdt (cronExpression: string) (timeZoneId: string) (after: DateTimeOffset) : OperationResult<DateTimeOffset, RecurringJobAction, RecurringJobOpError, RecurringJobLifeEvent> =
    operation {
        let result =
            try
                let timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById timeZoneId
                let after =
                    let after = after.UtcDateTime
                    let afterUtcDateTime = DateTime(after.Year, after.Month, after.Day, after.Hour, after.Minute, after.Second)
                    TimeZoneInfo.ConvertTimeFromUtc(afterUtcDateTime, timeZoneInfo)

                let nextUtcDt =
                    let schedule = NCrontab.CrontabSchedule.Parse cronExpression
                    let next = schedule.GetNextOccurrence after
                    let nextUtc = TimeZoneInfo.ConvertTimeToUtc(next, timeZoneInfo)
                    DateTime.SpecifyKind (nextUtc, DateTimeKind.Utc) |> DateTimeOffset

                Ok nextUtcDt
            with
            | e ->
                e.ToString() |> Error
        match result with
        | Ok x -> return x
        | Error err ->
            return RecurringJobOpError.ScheduleError err
    }

let private transition (env: RecurringJobEnvironment) (recurringJob: RecurringJob) (action: RecurringJobAction) : TransitionResult<RecurringJob, RecurringJobAction, RecurringJobOpError, RecurringJobLifeEvent, RecurringJobConstructor> =
    transition {
        match recurringJob.State, action with
        | RecurringJobState.Scheduled (dueOn, _), RecurringJobAction.TriggerJob ->
            let! guid = env.Unique.Query NewUuid

            // is id should be generated on client somehow?
            let jobId = guid.ToTinyUuid() |> JobId

            let! now = env.Clock.Query Now
            let! nextDueOn = nextDueBdt recurringJob.CronExpression recurringJob.TimeZoneId (max dueOn now)

            // SentOn is redundant on JobInstanceData, ideally the type should be forked and the field removed
            let common = { recurringJob.JobInstanceData with SentOn = now }

            yield jobsDef.LifeCycles.job.Construct (JobConstructor.NewProper (jobId, ProperJobConstructor.Enqueued (common, JobScope.Recurring recurringJob.Id)))
            return { recurringJob with State = RecurringJobState.Fired (nextDueOn, { Id = jobId; FiredOn = now }) }

        | RecurringJobState.Scheduled _, RecurringJobAction.OnFiredJobTimeout _
        | RecurringJobState.Scheduled _, RecurringJobAction.OnJobStatusChanged _ ->
            return Transition.NotAllowed

        | RecurringJobState.Scheduled (_, maybeLastExecution), RecurringJobAction.Update (cronExpression, timeZoneId, jobInstanceData) ->
            if recurringJob.CronExpression = cronExpression &&
               recurringJob.TimeZoneId = timeZoneId &&
               recurringJob.JobInstanceData = jobInstanceData &&
               recurringJob.HardDelete = false then
                return Transition.Ignore
            else
                let! now = env.Clock.Query Now
                let! dueOn = nextDueBdt cronExpression timeZoneId now
                return
                    { recurringJob with
                        CronExpression = cronExpression
                        TimeZoneId = timeZoneId
                        JobInstanceData = jobInstanceData
                        HardDelete = false
                        State = RecurringJobState.Scheduled (dueOn, maybeLastExecution) }

        | RecurringJobState.Scheduled _, RecurringJobAction.HardDelete ->
            return { recurringJob with HardDelete = true }

        | RecurringJobState.Fired _, RecurringJobAction.TriggerJob ->
            return Transition.Ignore

        | RecurringJobState.Fired (nextDueOn, execution), RecurringJobAction.OnJobStatusChanged jobStatus ->
            match jobStatus with
            | JobStatus.Unfinished _ ->
                return Transition.Ignore
            | JobStatus.Finished _ ->
                return { recurringJob with State = RecurringJobState.Scheduled (nextDueOn, Some execution) }

        | RecurringJobState.Fired (nextDueOn, execution), RecurringJobAction.OnFiredJobTimeout _ ->
            return { recurringJob with State = RecurringJobState.Scheduled (nextDueOn, Some execution) }

        | RecurringJobState.Fired (_, execution), RecurringJobAction.Update (cronExpression, timeZoneId, jobInstanceData) ->
            if recurringJob.CronExpression = cronExpression &&
               recurringJob.TimeZoneId = timeZoneId &&
               recurringJob.JobInstanceData = jobInstanceData &&
               recurringJob.HardDelete = false then
                return Transition.Ignore
            else
                let! nextDueOn = nextDueBdt cronExpression timeZoneId execution.FiredOn
                return
                    { recurringJob with
                        CronExpression = cronExpression
                        TimeZoneId = timeZoneId
                        JobInstanceData = jobInstanceData
                        HardDelete = false
                        State = RecurringJobState.Fired (nextDueOn, execution) }

        | RecurringJobState.Fired _, RecurringJobAction.HardDelete ->
            return { recurringJob with HardDelete = true }
    }

let private construction (env: RecurringJobEnvironment) (id: RecurringJobId) (ctor: RecurringJobConstructor) : ConstructionResult<RecurringJob, RecurringJobAction, RecurringJobOpError, RecurringJobLifeEvent> =
    construction {
        match ctor with
        | RecurringJobConstructor.New (name, cronExpression, timeZoneId, jobInstanceData) ->
            let! now = env.Clock.Query Now
            let! dueOn = nextDueBdt cronExpression timeZoneId now
            return {
                Id = id
                Name = name
                CreatedOn = now
                CronExpression = cronExpression
                TimeZoneId = timeZoneId
                JobInstanceData = jobInstanceData
                State = RecurringJobState.Scheduled (dueOn, None)
                HardDelete = false
            }
    }

let private idGeneration (env: RecurringJobEnvironment) (ctor: RecurringJobConstructor) : IdGenerationResult<RecurringJobId, RecurringJobOpError> =
    idgen {
        match ctor with
        | RecurringJobConstructor.New _ ->
            let! guid = env.Unique.Query NewUuid
            return guid.ToString("D") |> RecurringJobId
    }

let private timers (recurringJob: RecurringJob) =
    [
        if recurringJob.HardDelete then
            match recurringJob.State with
            | RecurringJobState.Scheduled _ ->
                { TimerAction = TimerAction.DeleteSelf; Schedule = Schedule.Now }
            | RecurringJobState.Fired _ -> ()
        else
            match recurringJob.State with
            | RecurringJobState.Scheduled (dueOn, _) ->
                { TimerAction = TimerAction.RunAction RecurringJobAction.TriggerJob; Schedule = Schedule.On dueOn }
            | RecurringJobState.Fired (nextDueOn, execution) ->
                // try to avoid overlapping job instances but give after three minutes when next tick is due
                let timeoutOn = (max nextDueOn execution.FiredOn).Add (TimeSpan.FromMinutes 3.)
                { TimerAction = TimerAction.RunAction RecurringJobAction.OnFiredJobTimeout; Schedule = Schedule.On timeoutOn }
    ]

let private subscriptions (job: RecurringJob) =
    [
        match job.State with
        | RecurringJobState.Fired (_, { Id = (JobId key as jobId) }) ->
            let dummyStatus = JobStatus.Unfinished
            $"Job-OnUpdated-%s{key}",
                jobsDef.LifeCycles.job.SubscribeToSubjectAndMapLifeEvent
                    jobId
                    (JobLifeEvent.OnJobStatusChanged (dummyStatus, None))
                    (function (JobLifeEvent.OnJobStatusChanged (realStatus, _)) -> Some <| RecurringJobAction.OnJobStatusChanged realStatus | _ -> None)
        | RecurringJobState.Scheduled _ ->
            ()
    ]
    |> Map.ofList

let private indices (job: RecurringJob) : seq<RecurringJobIndex> =
    IndicesWorkflow.indices {
        RecurringJobStringIndex.Name job.Name
    }

let private shouldSendTelemetry =
    function
    | ShouldSendTelemetryFor.Constructor ctor ->
        match ctor with
        | RecurringJobConstructor.New _ -> true
    | ShouldSendTelemetryFor.LifeAction action ->
        match action with
        | RecurringJobAction.TriggerJob -> false
        | RecurringJobAction.OnFiredJobTimeout
        | RecurringJobAction.Update _
        | RecurringJobAction.HardDelete -> true
        | RecurringJobAction.OnJobStatusChanged status ->
            match status with
            | JobStatus.Finished false | JobStatus.Unfinished -> true
            | JobStatus.Finished true -> false
    | ShouldSendTelemetryFor.LifeEvent _ -> false

let recurringJobLifeCycle =
    newJobsLifeCycle                      jobsDef.LifeCycles.recurringJob
    |> LifeCycleBuilder.withTransition    transition
    |> LifeCycleBuilder.withIdGeneration  idGeneration
    |> LifeCycleBuilder.withConstruction  construction
    |> LifeCycleBuilder.withTimers        timers
    |> LifeCycleBuilder.withSubscriptions subscriptions
    |> LifeCycleBuilder.withIndices       indices
    |> LifeCycleBuilder.withStorage
           (StorageType.Persistent
                (PromotedIndicesConfig.Empty,
                System.TimeSpan.FromDays 60.
                |> PersistentHistoryExpiration.AfterSubjectChange |> Some
                |> PersistentHistoryRetention.Unfiltered))
    |> LifeCycleBuilder.withTelemetryRules shouldSendTelemetry
    |> LifeCycleBuilder.build
