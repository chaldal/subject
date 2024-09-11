[<AutoOpen>]
module SuiteJobs.LifeCycles.DispatcherLifeCycle

open System
open LibLifeCycle
open Microsoft.Extensions.Configuration
open SuiteJobs.LifeCycles.Services
open SuiteJobs.Types
open LibLifeCycle.LifeCycleAccessBuilder
open type AccessTo<DispatcherAction, DispatcherConstructor>

type DispatcherEnvironment = {
    Clock: Service<Clock>
    Random: Service<Random>
    AllJobs: Service<All<Job, JobId, JobIndex, JobOpError>>
    Configuration: IConfiguration
} with interface Env

type Dispatcher
with
    member private this.SpotsLeft = this.Settings.MaxProcessingJobs.Value - this.ProcessingJobs.Count

let private validateSettings (settings: DispatcherSettings) : OperationResult<unit, DispatcherAction, DispatcherOpError, DispatcherLifeEvent> =
    operation {
        if settings.HotPollIfJobsCountAtOrBelow.Value > settings.MaxProcessingJobs.Value then
            return DispatcherOpError.InvalidSettings "HotPollIfJobsCountAtOrBelow can't be greater than MaxProcessingJobs"
        else
            return ()
    }

let private transition (env: DispatcherEnvironment) (dispatcher: Dispatcher) (action: DispatcherAction) : TransitionResult<Dispatcher, DispatcherAction, DispatcherOpError, DispatcherLifeEvent, DispatcherConstructor> =
    transition {
        let! now = env.Clock.Query Now

        match action with
        | DispatcherAction.Poll (retry, actualTrigger) when dispatcher.PollInfo.ScheduledTrigger <> Some actualTrigger ->
            // ignore legit race conditions: retry of a timer poll after concurrency error can run into a different trigger although probability is very low (1 in millions)
            // Detailed scenario: Poll side effect retry=0 bounced back with JobAlreadyReserved and enqueues a (separate!) retry side effect retry=1,
            // it enqueued fine but SQL fails to respond (false negative), SE processor knows nothing better than attempt the retry=0 side effect from the very start and so it does,
            // this time around retry=0 succeeds and schedules new Poll trigger, however retry=1 still runs only to discover that scheduled trigger has changed.
            if retry > 0u then
                ()
            else
                failwith $"Unexpected poll. Is it a bug or legit race condition? Scheduled: %A{dispatcher.PollInfo.ScheduledTrigger}; Actual: %A{actualTrigger}"
            return Transition.Ignore

        | DispatcherAction.Poll (retryNo, _) ->
            if dispatcher.SpotsLeft <= 0 then
                // negative spots can happen if settings changed / capacity dropped
                // reset trigger to let OnProcessingCompleted trigger next poll
                return { dispatcher with PollInfo = { dispatcher.PollInfo with ScheduledTrigger = None } }
            else if retryNo > dispatcher.Settings.MaxContinuousPollRetries then
                // slow down polling after a few racy attempts, insert a delay
                let! afterMilliseconds = env.Random.Query RandomIntegerBetween 100 1000
                return { dispatcher with PollInfo = { dispatcher.PollInfo with ScheduledTrigger = Some (PollTrigger.Cold (now.AddMilliseconds afterMilliseconds)) } }
            else
                let pollService = PollService(env.Configuration, env.AllJobs)
                let! jobIds = pollService.Poll dispatcher.SpotsLeft dispatcher.Settings.Queues
                let newJobIds = Set.difference (jobIds |> Set.ofList) dispatcher.ProcessingJobs

                yield! newJobIds |> Seq.map (fun jobId -> jobsDef.LifeCycles.job.Act jobId (JobAction.Start dispatcher.Id))
                let lastJobAddedOn, streakCount =
                    if newJobIds.IsEmpty then
                        dispatcher.PollInfo.LastJobAddedOn, 0u
                    else
                        now, dispatcher.PollInfo.StreakCount + 1u

                let updatedProcessingJobs = Set.union dispatcher.ProcessingJobs newJobIds

                return
                    { dispatcher with
                        PollInfo =
                            { dispatcher.PollInfo with
                                StreakCount = streakCount
                                LastPolledOn = now
                                LastJobAddedOn = lastJobAddedOn
                                ScheduledTrigger =
                                    if newJobIds.IsEmpty then
                                        // if it's cold, exponential backoff timer from 1s to 10s
                                        (now - lastJobAddedOn)
                                        |> min (TimeSpan.FromSeconds 10.0)
                                        |> max (TimeSpan.FromSeconds 1.0)
                                        |> fun after -> PollTrigger.Cold (now.Add after)
                                        |> Some
                                    elif updatedProcessingJobs.Count < dispatcher.Settings.MaxProcessingJobs.Value then
                                        // hot but some capacity left, schedule timer in case if jobs are slow and completions don't hot-trigger next Poll
                                        PollTrigger.HotButSlow (now.Add dispatcher.Settings.HotPollIfTimeSinceLastPollAtLeast)
                                        |> Some
                                    else
                                        // hot & at capacity, wait for some completions to hot-trigger next Poll
                                        None }
                        ProcessingJobs = updatedProcessingJobs }

        | DispatcherAction.ChangeSettings newSettings ->
            if dispatcher.Settings = newSettings && dispatcher.HardDelete = false then
                return Transition.Ignore
            else
                do! validateSettings newSettings
                match dispatcher.PollInfo.ScheduledTrigger with
                | Some (PollTrigger.Hot _) ->
                    return { dispatcher with Settings = newSettings; HardDelete = false }
                | None
                | Some (PollTrigger.Cold _)
                | Some (PollTrigger.HotButSlow _) ->
                    let hotTrigger = PollTrigger.Hot now
                    yield DispatcherAction.Poll (0u, hotTrigger)
                    return { dispatcher with Settings = newSettings; HardDelete = false; PollInfo = { dispatcher.PollInfo with ScheduledTrigger = Some hotTrigger } }

        | DispatcherAction.OnProcessingCompleted jobId
        | DispatcherAction.OnJobCannotBeStarted jobId ->
            if dispatcher.ProcessingJobs.Contains jobId then
                let pollInfo = dispatcher.PollInfo
                let dispatcherMinusJob = { dispatcher with ProcessingJobs = dispatcher.ProcessingJobs.Remove jobId }

                let dispatcher = () // shadow to avoid accidental usage
                dispatcher

                match dispatcherMinusJob.PollInfo.ScheduledTrigger with
                | Some (PollTrigger.Hot _)
                | Some (PollTrigger.Cold _) ->
                    // next hot poll already scheduled or it's cold
                    return dispatcherMinusJob
                | None
                | Some (PollTrigger.HotButSlow _) ->
                    let hotButSlowTimerDueOn = dispatcherMinusJob.PollInfo.LastPolledOn.Add dispatcherMinusJob.Settings.HotPollIfTimeSinceLastPollAtLeast
                    // trigger a Poll only if spots available, and also processing jobs count dropped below threshold OR there was sufficient delay since last Poll (e.g. slow jobs)
                    if dispatcherMinusJob.SpotsLeft > 0 &&
                       (dispatcherMinusJob.ProcessingJobs.Count <= dispatcherMinusJob.Settings.HotPollIfJobsCountAtOrBelow.Value || hotButSlowTimerDueOn <= now)
                        then
                        // erase context sometimes to break endless nesting of telemetry
                        let actOptions = if pollInfo.StreakCount % 3u = 0u then { defaultActOptions with ResetTraceContext = true } else defaultActOptions
                        let hotTrigger = PollTrigger.Hot now
                        yield jobsDef.LifeCycles.dispatcher.ActWith actOptions dispatcherMinusJob.Id (DispatcherAction.Poll (0u, hotTrigger))
                        return
                            { dispatcherMinusJob with
                                PollInfo = { pollInfo with ScheduledTrigger = Some hotTrigger } }
                    // no timer was scheduled because was at capacity but has capacity now, must schedule a timer
                    elif dispatcherMinusJob.SpotsLeft > 0 && dispatcherMinusJob.PollInfo.ScheduledTrigger.IsNone then
                        return
                            { dispatcherMinusJob with
                                PollInfo = { pollInfo with ScheduledTrigger = Some (PollTrigger.HotButSlow hotButSlowTimerDueOn) } }
                    else
                        return dispatcherMinusJob
            else
                return Transition.Ignore

        | DispatcherAction.HardDelete ->
            return { dispatcher with HardDelete = true }
    }

let private construction (env: DispatcherEnvironment) (_id: DispatcherId) (ctor: DispatcherConstructor) : ConstructionResult<Dispatcher, DispatcherAction, DispatcherOpError, DispatcherLifeEvent> =
    construction {
        match ctor with
        | DispatcherConstructor.New (name, settings) ->
            do! validateSettings settings
            let! now = env.Clock.Query Now

            let hotTrigger = PollTrigger.Hot now
            yield DispatcherAction.Poll (0u, hotTrigger)

            return {
                Name = name
                CreatedOn = now
                Settings = settings
                PollInfo = { ScheduledTrigger = Some hotTrigger; StreakCount = 0u; LastPolledOn = now; LastJobAddedOn = now }
                ProcessingJobs = Set.empty
                HardDelete = false
            }
    }

let private idGeneration (_env: DispatcherEnvironment) (ctor: DispatcherConstructor) : IdGenerationResult<DispatcherId, DispatcherOpError> =
    idgen {
        match ctor with
        | DispatcherConstructor.New (name, _) ->
            return DispatcherId name
    }

let private timers (dispatcher: Dispatcher) =
    [
        if dispatcher.HardDelete then
            if dispatcher.ProcessingJobs.IsEmpty then
                { TimerAction = TimerAction.DeleteSelf; Schedule = Schedule.Now }
            else
                ()
        else
            match dispatcher.PollInfo.ScheduledTrigger with
            | Some (PollTrigger.Cold timerScheduledOn as trigger)
            | Some (PollTrigger.HotButSlow timerScheduledOn as trigger) ->
                { TimerAction = TimerAction.RunAction (DispatcherAction.Poll (0u, trigger)); Schedule = Schedule.On timerScheduledOn }
            | Some (PollTrigger.Hot _)
            | None ->
                ()
    ]

let private subscriptions (dispatcher: Dispatcher) =
    dispatcher.ProcessingJobs
    |> Seq.map (fun (JobId key as jobId) ->
        $"Job-OnUpdated-%s{key}",
                jobsDef.LifeCycles.job.SubscribeToSubject
                    jobId
                    JobLifeEvent.OnProcessingCompleted
                    (DispatcherAction.OnProcessingCompleted jobId))
    |> Map.ofSeq

let private responseHandler (response: SideEffectResponse) =
    seq {
        match jobsDef.LifeCycles.job.OnResponse response with
        | SubscribeNotInitialized (poisonJobId, _, next)
        | ActNotInitialized (poisonJobId, JobAction.Start _, next)
        | ActNotAllowed (poisonJobId, JobAction.Start _, next)
        | ActError (poisonJobId, JobAction.Start _, _, next) ->
            // this error is possible in a rare situation when one dispatcher runs faster than the other and job was very quick, consider this:
            // A slowpoke Dispatcher #1 polls a job and decides to reserve it by unique index, but did not save its new state yet
            // In the meantime Dispatcher #2 polls same job and does the full cycle:
            //   successfully saves state / reserves unique index, starts job, receives job processed confirmation, and releases unique index
            // Then slowpoke Dispatcher #1 reserves the same unique index and tries to Start the job which is already completed - and gets the error
            next.Compensate (DispatcherAction.OnJobCannotBeStarted poisonJobId)
        | _ -> ()

        match jobsDef.LifeCycles.dispatcher.OnResponse response with
        | ActError (_, DispatcherAction.Poll (retry, trigger), DispatcherOpError.JobAlreadyReserved _, next) ->
            // keep polling if job was reserved concurrently
            next.Compensate (DispatcherAction.Poll (retry + 1u, trigger))
        | _ -> ()
    }

let private shouldSendTelemetry =
    function
    | ShouldSendTelemetryFor.Constructor ctor ->
        match ctor with
        | DispatcherConstructor.New _ -> true
    | ShouldSendTelemetryFor.LifeAction action ->
        match action with
        | DispatcherAction.OnProcessingCompleted _ ->
            false // reduce telemetry aggressively
        | DispatcherAction.OnJobCannotBeStarted _
        | DispatcherAction.ChangeSettings _
        | DispatcherAction.HardDelete _ -> true
        | DispatcherAction.Poll (_, trigger) ->
            match trigger with
            | PollTrigger.Cold _
            | PollTrigger.Hot _ -> false // remove frequent poll noise from telemetry
            | PollTrigger.HotButSlow _ -> true
    | ShouldSendTelemetryFor.LifeEvent _ -> false

let private indices (dispatcher: Dispatcher) : seq<DispatcherIndex> =
    IndicesWorkflow.indices {
        yield!
            dispatcher.Settings.Queues.ToSet
            |> Seq.map DispatcherStringIndex.Queue

        yield!
            dispatcher.ProcessingJobs
            |> Seq.map DispatcherStringIndex.Job
    }

let dispatcherLifeCycle =
    newJobsLifeCycle                        jobsDef.LifeCycles.dispatcher
    |> LifeCycleBuilder.withTransition      transition
    |> LifeCycleBuilder.withIdGeneration    idGeneration
    |> LifeCycleBuilder.withConstruction    construction
    |> LifeCycleBuilder.withTimers timers
    |> LifeCycleBuilder.withSubscriptions   subscriptions
    |> LifeCycleBuilder.withResponseHandler responseHandler
    |> LifeCycleBuilder.withIndices indices
    |> LifeCycleBuilder.withStorageEx
           (StorageType.Persistent
                (PromotedIndicesConfig.Empty,
                System.TimeSpan.FromDays 3.
                |> PersistentHistoryExpiration.AfterSubjectChange |> Some
                |> PersistentHistoryRetention.Unfiltered)
                |> Some)
           // generous dedup cache size so OnProcessingCompleted notifications don't interfere with Poll during a brownout
           (* maxDedupCacheSizeOverride *) (Some 50us)
    |> LifeCycleBuilder.withTelemetryRules  shouldSendTelemetry
    |> LifeCycleBuilder.build
