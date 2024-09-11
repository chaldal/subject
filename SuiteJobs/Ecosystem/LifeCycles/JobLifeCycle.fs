[<AutoOpen>]
module SuiteJobs.LifeCycles.JobLifeCycle

open System.Threading.Tasks
open LibLifeCycle
open SuiteJobs.Types
open LibLifeCycle.LifeCycleAccessBuilder
open SuiteJobs.LifeCycles.Connectors
open SuiteJobs.LifeCycles.Services
open type AccessTo<JobAction, JobConstructor>

type JobEnvironment = {
    Clock: Service<Clock>
    Unique: Service<Unique>
    Sequence: Service<Sequence>
    Random: Service<Random>
    AllJobs: Service<All<Job, JobId, JobIndex, JobOpError>>
}
with
    interface Env
    member this.GetNextOrderInQueue() : Task<uint64> =
        this.Sequence.Query GetNext "OrderInQueue"

let private incrementJobCounter (sequence: Service<Sequence>) (counter: JobStatsCounter) (queueName: NonemptyString) =
    (createStatsService sequence).Query StatsRequest.IncrementJobCounter counter queueName

type private JobStateMaker (env: JobEnvironment) =
    member _.Scheduled (body: JobBody) on =
        { body with State = JobState.Scheduled on }

    member _.Enqueued (body: JobBody) =
        backgroundTask {
            let! orderInQueue = env.GetNextOrderInQueue()
            let! now = env.Clock.Query Now
            return { body with State = JobState.Enqueued (int64 orderInQueue, now) }
        }

    member _.Deleted (body: JobBody) : OperationResult<JobBody, JobAction, JobOpError, JobLifeEvent> =
        operation {
            do! incrementJobCounter env.Sequence JobStatsCounter.Deleted body.QueueName
            let! now = env.Clock.Query Now
            return { body with State = JobState.Deleted now }
        }

let private transitionJobBody
    (jobRunnerConnector: JobRunnerConnector)
    (env: JobEnvironment) (jobId: JobId) (body: JobBody) (action: JobAction) : TransitionResult<JobBody, JobAction, JobOpError, JobLifeEvent, JobConstructor> =
    transition {
        let makeState = JobStateMaker(env)

        // state transitions are rather loose, like they were in Hangfire
        match body.State, action with
        | JobState.Scheduled scheduledOn, JobAction.Schedule on ->
            if (scheduledOn = on) then
                return Transition.Ignore
            else
                return makeState.Scheduled body on

        | JobState.Scheduled _, JobAction.Enqueue ->
            return! makeState.Enqueued body

        | JobState.Scheduled _, JobAction.Start _ ->
            return Transition.NotAllowed

        | JobState.Scheduled _, JobAction.Delete ->
            return! makeState.Deleted body

        | JobState.Awaiting _, JobAction.Schedule _
        | JobState.Awaiting _, JobAction.Enqueue
        | JobState.Awaiting _, JobAction.Start _ ->
            return Transition.NotAllowed

        | JobState.Awaiting (JobParent.Job (_, condition)), JobAction.OnParentJobUpdate status ->
            match condition, status with
            | AwaitingForJobStatus.OnlySucceeded, JobStatus.Finished (* succeeded *) true
            | AwaitingForJobStatus.AnyFinishedState, JobStatus.Finished _ ->
                return! makeState.Enqueued body
            | AwaitingForJobStatus.OnlySucceeded, JobStatus.Finished (* succeeded *) false ->
                // Hangfire deletes awaiting job if finish condition not met, so do we
                return! makeState.Deleted body
            | _, JobStatus.Unfinished ->
                return Transition.Ignore

        | JobState.Awaiting (JobParent.Job _), JobAction.OnParentBatchUpdate _ ->
            return Transition.NotAllowed

        | JobState.Awaiting (JobParent.Batch _), JobAction.OnParentJobUpdate _ ->
            return Transition.NotAllowed

        | JobState.Awaiting (JobParent.Batch (_, condition)), JobAction.OnParentBatchUpdate status ->
            match condition, status with
            | AwaitingForBatchStatus.OnlySucceeded, BatchStatus.Finished (* succeeded *) true
            | AwaitingForBatchStatus.AnyFinishedState, BatchStatus.Finished _ ->
                return! makeState.Enqueued body
            | AwaitingForBatchStatus.OnlySucceeded, BatchStatus.Finished (* succeeded *) false ->
                // Hangfire deletes awaiting job if finish condition not met, so do we
                return! makeState.Deleted body
            | _, BatchStatus.Unfinished ->
                return Transition.Ignore

        | JobState.Awaiting _, JobAction.Delete ->
            return! makeState.Deleted body

        | JobState.Awaiting _, JobAction.DeleteAwaitingJobsBackwards ->
            let! continuations =
                IndexPredicate.EqualToString (JobStringIndex.ParentJob jobId)
                |> JobIndex.PrepareQuery { Page = { Size = System.UInt16.MaxValue; Offset = 0UL }; OrderBy = OrderBy.FastestOrSingleSearchScoreIfAvailable }
                |> env.AllJobs.Query FilterFetchSubjects
            let maybeContinuation =
                continuations |> Seq.tryFind (fun continuationJob ->
                    match continuationJob.Body with
                    | JobBodyVariant.Proper { State = JobState.Awaiting (JobParent.Job _) } -> true
                    | _ -> false)
            match maybeContinuation with
            | Some continuation ->
                // this job is not last continuation, keep looking
                yield jobsDef.LifeCycles.job.Act continuation.Id JobAction.DeleteAwaitingJobsBackwards
                return body
            | None ->
                // this job is last continuation, let's delete backwards
                match body.State with
                | JobState.Awaiting (JobParent.Job (parentJobId, _)) ->
                    yield jobsDef.LifeCycles.job.Act parentJobId JobAction.DeleteAwaitingJobsBackwards
                | _ -> ()
                return! makeState.Deleted body

        | JobState.Enqueued _, JobAction.Schedule on ->
            return makeState.Scheduled body on

        | JobState.Enqueued _, JobAction.Enqueue ->
            return Transition.Ignore

        | JobState.Enqueued (_, enqueuedOn), JobAction.Start dequeuedBy ->
            let! ticket = env.Unique.Query NewUuid
            let! now = env.Clock.Query Now
            let latency = now - enqueuedOn
            yield
                jobRunnerConnector.Request
                    JobRunnerRequest.RunJob
                        { Ticket = ticket
                          JobId = jobId
                          IsAtMostOnce = body.FailurePolicy.IsAtMostOnce
                          Payload = body.Payload; }
                    (fun (ticket, update) ->
                        match update with
                        | ProcessingJobUpdate.Heartbeat ->
                            JobAction.OnHeartbeat ticket
                        | ProcessingJobUpdate.Completed result ->
                            JobAction.OnProcessingComplete (ticket, result))
            return { body with State = JobState.Processing (ticket, { StartedBy = dequeuedBy; StartedOn = now; Latency = latency; LastHeartbeatOn = None }) }

        | JobState.Enqueued _, JobAction.Delete ->
            return! makeState.Deleted body

        | JobState.Processing _, JobAction.Schedule _
        | JobState.Processing _, JobAction.Enqueue ->
            return Transition.NotAllowed

        | JobState.Processing _, JobAction.Delete ->
            return! makeState.Deleted body

        | JobState.Processing _, JobAction.Start _ ->
            return Transition.Ignore

        | JobState.Processing (requestTicket, unfinishedRun), JobAction.OnHeartbeat responseTicket ->
            if requestTicket = responseTicket then
                let! now = env.Clock.Query Now
                return { body with State = JobState.Processing (requestTicket, { unfinishedRun with LastHeartbeatOn = Some now }) }
            else
                return Transition.Ignore

        | JobState.Processing (requestTicket, unfinishedRun), JobAction.OnProcessingComplete (responseTicket, result) ->
            if requestTicket = responseTicket then
                let! now = env.Clock.Query Now
                let finishedRun: FinishedJobRun = {
                        FinishedOn = now
                        StartedBy = unfinishedRun.StartedBy
                        Latency = unfinishedRun.Latency
                        TotalDuration = result.TotalDuration
                        PureDuration = result.PureDuration
                    }

                match result.Result with
                | Ok () ->
                    do! incrementJobCounter env.Sequence JobStatsCounter.SuccessfulRuns body.QueueName
                    return { body with State = JobState.Succeeded finishedRun }

                | Error (retryForFree, failedReason) ->
                    do! incrementJobCounter env.Sequence JobStatsCounter.FailedRuns body.QueueName

                    let autoRetriesPolicy =
                        body.FailurePolicy.MaybeAutoRetries
                        |> Option.defaultValue
                               { MaxAutoRetries = 0uy
                                 DelayPolicy = JobAutoRetryDelayPolicy.Hangfire
                                 DeleteIfExceeded = false }

                    let attempt =
                        let incrementAttempt = if retryForFree then 0uy else 1uy
                        body.Retry |> Option.map (fun r -> r.Attempt + incrementAttempt) |> Option.defaultValue incrementAttempt

                    let failedState = JobState.Failed (failedReason, finishedRun)
                    if attempt <= autoRetriesPolicy.MaxAutoRetries then

                        let! randomVariance = env.Random.Query RandomIntegerBetween 0 30
                        let secondsToDelay =
                            if retryForFree then
                                60. // hardcode "free retries" at 1 min interval
                            else
                                match autoRetriesPolicy.DelayPolicy with
                                | JobAutoRetryDelayPolicy.Hangfire ->
                                    // TODO: better exponential delay formula? Here I simply repeat what Hangfire's AutomaticRetryAttribute does
                                    System.Math.Round(System.Math.Pow(float attempt - 1., 4.) + 15. + (float randomVariance * float attempt))
                                | JobAutoRetryDelayPolicy.LinearIncrease initialDelaySeconds ->
                                    initialDelaySeconds * (uint32 attempt) |> float

                        // cap max delay at 2h
                        let scheduleRetryAfter = min (System.TimeSpan.FromHours 2.) (System.TimeSpan.FromSeconds secondsToDelay)

                        yield JobAction.Schedule (now.Add scheduleRetryAfter)

                        let retry =
                            { Attempt = attempt
                              LastFailureMessage =
                              match failedReason with
                              | JobFailedReason.Exception (message, _, _) -> message
                              | JobFailedReason.ConnectorTimeout -> "Connector Timeout" }
                        return { body with State = failedState; Retry = Some retry }
                    else
                        if autoRetriesPolicy.DeleteIfExceeded then
                            yield JobAction.Delete
                        return  { body with State = failedState }
            else
                return Transition.Ignore // phantom

        | JobState.Succeeded _, JobAction.Schedule on ->
            return makeState.Scheduled body on

        | JobState.Succeeded _, JobAction.Enqueue ->
            return! makeState.Enqueued body

        | JobState.Succeeded _, JobAction.Start _ ->
            return Transition.NotAllowed

        // Beware if you decide to allow delete from Succeeded state, it has many implications
        // at very least must capture whether previous state was a Success and make it Finished true status
        // for now simply imitate Hangfire rules (delete from Succeeded not allowed)
        | JobState.Succeeded _, JobAction.Delete ->
            return JobOpError.CannotDeleteSuccessfulJob


        | JobState.Failed _, JobAction.Schedule on ->
            return makeState.Scheduled body on

        | JobState.Failed _, JobAction.Enqueue ->
            return! makeState.Enqueued body

        | JobState.Failed _, JobAction.Start _ ->
            return Transition.NotAllowed

        | JobState.Failed _, JobAction.Delete ->
            return! makeState.Deleted body

        | JobState.Deleted _, JobAction.Schedule on ->
            return makeState.Scheduled body on

        | JobState.Deleted _, JobAction.Enqueue ->
            return! makeState.Enqueued body

        | JobState.Deleted _, JobAction.Start _ ->
            return Transition.NotAllowed

        | JobState.Deleted _, JobAction.Delete ->
            return Transition.Ignore

        | _, JobAction.OnNewSubscriber _ ->
            shouldNotReachHereBecause "OnNewSubscriber must be handled in outer transition"
            return Transition.NotAllowed

        // parent update expected strictly in Awaiting state
        | _, JobAction.OnParentJobUpdate _
        | _, JobAction.OnParentBatchUpdate _ ->
            return Transition.NotAllowed

        // phantom connector call
        | _, JobAction.OnHeartbeat _
        | _, JobAction.OnProcessingComplete _
        // ignore because body already filled, likely coming from ActMaybeConstruct
        | _, JobAction.FillPlaceholder _
        // ignore, probably root af awaiting chain, should be deleted manually
        | _, JobAction.DeleteAwaitingJobsBackwards ->
            return Transition.Ignore
    }

let private constructJobBody (env: JobEnvironment) (jobId: JobId) (ctor: ProperJobConstructor) (placeholderFilledOn: Option<System.DateTimeOffset>) : OperationResult<JobBody, JobAction, JobOpError, JobLifeEvent> =
    operation {
        let makeState = JobStateMaker(env)

        let newJobBodyWithDummyState (common: JobConstructorCommonData) (scope: JobScope) (parent: Option<JobParent>)
            : OperationResult<JobBody, JobAction, JobOpError, JobLifeEvent> =
            operation {
                do! incrementJobCounter env.Sequence JobStatsCounter.Created common.QueueName
                return
                    { Scope = scope
                      Parent = parent
                      Payload = common.Payload
                      QueueName = common.QueueName
                      QueueSortOrder = common.QueueSortOrder
                      FailurePolicy = common.FailurePolicy
                      SentOn = common.SentOn
                      CorrelationId = common.CorrelationId
                      PlaceholderFilledOn = placeholderFilledOn
                      Retry = None
                      State = JobState.Deleted System.DateTimeOffset.UnixEpoch }
            }

        match ctor with
        | ProperJobConstructor.Enqueued (common, scope) ->
            let! jobBody = newJobBodyWithDummyState common scope None
            return! makeState.Enqueued jobBody

        | ProperJobConstructor.Scheduled (common, scheduledOn) ->
            let! jobBody = newJobBodyWithDummyState common JobScope.Other None
            return makeState.Scheduled jobBody scheduledOn

        | ProperJobConstructor.Awaiting (common, scope, parent) ->
            match parent with
            | JobParent.Job (parentId, _) when parentId = jobId ->
                return JobOpError.UnableToConstructBody "Job cannot await for itself"
            | JobParent.Job _
            | JobParent.Batch _ ->
                let! jobBody = newJobBodyWithDummyState common scope (Some parent)
                match parent with
                // if this job constructed before its parent, the ActMaybeConstruct will create a placeholder parent to subscribe to,
                // it also combines with Subscribe signal so it's very cheap price for flexibility
                | JobParent.Job (parentId, _) ->
                    yield jobsDef.LifeCycles.job.ActMaybeConstruct parentId (JobAction.OnNewSubscriber (JobSubscriber.Job jobId)) (JobConstructor.NewPlaceholder (parentId, PlacedBy.Job jobId))
                | JobParent.Batch (parentId, _) ->
                    yield jobsDef.LifeCycles.batch.ActMaybeConstruct parentId (BatchAction.OnNewSubscriber (BatchSubscriber.Job jobId)) (BatchConstructor.NewPlaceholder (parentId, PlacedBy.Job jobId))
                return { jobBody with State = JobState.Awaiting parent }

        | ProperJobConstructor.Deleted (common, scope) ->
            let! jobBody = newJobBodyWithDummyState common scope None
            return! makeState.Deleted jobBody
    }

let private transition'
    (jobRunnerConnector: JobRunnerConnector)
    (env: JobEnvironment) (job: Job) (action: JobAction) : TransitionResult<Job, JobAction, JobOpError, JobLifeEvent, JobConstructor> =
    transition {
        match action, job.Body with
        | JobAction.OnNewSubscriber newAwaitingJobId, JobBodyVariant.Proper _ ->
            yield JobLifeEvent.OnJobStatusChanged (job.Status, Some newAwaitingJobId)
            return job

        | _, JobBodyVariant.Proper body ->
            let! updatedBody = transitionJobBody jobRunnerConnector env job.Id body action
            return { job with Body = JobBodyVariant.Proper updatedBody }

        | _, JobBodyVariant.Placeholder placeholder ->
            match action with
            | JobAction.FillPlaceholder ctor ->
                let! now = env.Clock.Query Now
                let! jobBody = constructJobBody env job.Id ctor (Some now)
                match placeholder.DeleteRequestedOn with
                | None ->
                    return { job with Body = JobBodyVariant.Proper jobBody }
                | Some deleteRequestedOn ->
                    // Q: why not construct job in original state and just `yield JobAction.Delete`? It'd look neater in history
                    // A: it'd give the job an unwanted chance to run
                    return { job with Body = JobBodyVariant.Proper { jobBody with State = JobState.Deleted deleteRequestedOn } }

            | JobAction.Delete ->
                match placeholder.DeleteRequestedOn with
                | Some _ ->
                    return Transition.Ignore
                | None ->
                    let! now = env.Clock.Query Now
                    return { job with Body = JobBodyVariant.Placeholder { placeholder with DeleteRequestedOn = Some now } }

            | JobAction.OnNewSubscriber subscriber ->
                let placedAndSubscribedBySameThing =
                    match placeholder.PlacedBy, subscriber with
                    | PlacedBy.Batch placedByBatchId, JobSubscriber.Batch subscriberBatchId ->
                        placedByBatchId = subscriberBatchId
                    | PlacedBy.Job placedByJobId, JobSubscriber.Job subscriberJobId ->
                        placedByJobId = subscriberJobId
                    | PlacedBy.Client, _
                    | PlacedBy.Batch _, _
                    | PlacedBy.Job _, _ -> false

                if placedAndSubscribedBySameThing then
                    return Transition.Ignore
                else
                    yield JobLifeEvent.OnJobStatusChanged (job.Status, Some subscriber)
                    return job

            | JobAction.Schedule _
            | JobAction.Enqueue
            | JobAction.Start _
            | JobAction.OnHeartbeat _
            | JobAction.OnProcessingComplete _
            | JobAction.OnParentJobUpdate _
            | JobAction.OnParentBatchUpdate _ ->
                return Transition.NotAllowed

            | JobAction.DeleteAwaitingJobsBackwards ->
                return Transition.Ignore
    }

let private transition
    (jobRunnerConnector: JobRunnerConnector)
    (env: JobEnvironment) (job: Job) (action: JobAction) : TransitionResult<Job, JobAction, JobOpError, JobLifeEvent, JobConstructor> =
    transition {
        let! jobAfter = transition' jobRunnerConnector env job action
        let statusBefore = job.Status
        let statusAfter = jobAfter.Status
        if statusBefore <> statusAfter then
            yield JobLifeEvent.OnJobStatusChanged (statusAfter, None)

        let isProcessingState (j: Job) =
            match j.Body with
            | JobBodyVariant.Placeholder _ -> false
            | JobBodyVariant.Proper body ->
                match body.State with
                | JobState.Processing _ -> true
                | JobState.Awaiting _ | JobState.Deleted _ | JobState.Enqueued _ | JobState.Failed _ | JobState.Scheduled _ | JobState.Succeeded _ -> false
        if isProcessingState job && not (isProcessingState jobAfter) then
            yield JobLifeEvent.OnProcessingCompleted

        return jobAfter
    }

let private construction (env: JobEnvironment) (id: JobId) (ctor: JobConstructor) : ConstructionResult<Job, JobAction, JobOpError, JobLifeEvent> =
    construction {
        let! now = env.Clock.Query Now
        let! initialJob =
            construction {
                match ctor with
                | JobConstructor.NewPlaceholder (_, placedBy) ->
                    return { Id = id; CreatedOn = now; Body = JobBodyVariant.Placeholder { DeleteRequestedOn = None; PlacedBy = placedBy } }
                | JobConstructor.NewProper (_, properCtor) ->
                    let! jobBody = constructJobBody env id properCtor None
                    return { Id = id; CreatedOn = now; Body = JobBodyVariant.Proper jobBody }
            }
        match initialJob.Status with
        | JobStatus.Unfinished _ -> ()
        | JobStatus.Finished succeeded ->
            // creator-subscribers such as Batch may need to know that if job was finished right upon construction
            yield JobLifeEvent.OnJobStatusChanged (JobStatus.Finished succeeded, None)
        return initialJob
    }

let private idGeneration (_env: JobEnvironment) (ctor: JobConstructor) : IdGenerationResult<JobId, JobOpError> =
    idgen {
        match ctor with
        | JobConstructor.NewPlaceholder (id, _)
        | JobConstructor.NewProper (id, _) ->
            return id
    }

let private timers (job: Job) =
    match job.Body with
    | JobBodyVariant.Placeholder _ -> []
    | JobBodyVariant.Proper body ->
        [
            match body.State with
            | JobState.Scheduled scheduledOn ->
                { TimerAction = TimerAction.RunAction JobAction.Enqueue
                  Schedule = Schedule.On scheduledOn }

            | JobState.Processing (ticket, unfinishedRun) ->
                let timeoutOn = (unfinishedRun.LastHeartbeatOn |> Option.defaultValue unfinishedRun.StartedOn).AddMinutes 1.
                // to be safe, don't retry at-most-once jobs after connector timeout, there's a chance that they ran once already.
                let retryForFree = not body.FailurePolicy.IsAtMostOnce
                { TimerAction = TimerAction.RunAction (
                    JobAction.OnProcessingComplete (ticket,
                            { TotalDuration = timeoutOn - unfinishedRun.StartedOn; PureDuration = None; Result = Error ( retryForFree, JobFailedReason.ConnectorTimeout) }))
                  Schedule = Schedule.On timeoutOn }

            | JobState.Succeeded finishedRun ->
                let finishedJobSelfDeletesAfter =
                    match body.Scope with
                    | JobScope.Batch _ -> System.TimeSpan.FromDays 2. // in sync with Batch self-deletion
                    | JobScope.Other | JobScope.Recurring _ -> System.TimeSpan.FromDays 1.
                { TimerAction = TimerAction.DeleteSelf
                  Schedule = Schedule.On (finishedRun.FinishedOn.Add finishedJobSelfDeletesAfter) }

            | JobState.Deleted finishedOn ->
                // delete jobs are rare and often were deleted manually, keep them around for longer
                { TimerAction = TimerAction.DeleteSelf
                  Schedule = Schedule.On (finishedOn.AddDays 4) }

            | JobState.Awaiting _
            | JobState.Enqueued _
            | JobState.Failed _ ->
                ()
        ]

let private subscriptions (job: Job) =
    match job.Body with
    | JobBodyVariant.Placeholder _ -> Map.empty
    | JobBodyVariant.Proper body ->
        [
            match body.State with
            | JobState.Awaiting jobParent ->
                match jobParent with
                | JobParent.Job (JobId key as parentId, _) ->
                    let dummyStatus = JobStatus.Unfinished
                    $"JobParent-OnUpdated-%s{key}",
                        jobsDef.LifeCycles.job.SubscribeToSubjectAndMapLifeEvent parentId
                            (JobLifeEvent.OnJobStatusChanged (dummyStatus, Some (JobSubscriber.Job job.Id)))
                            (function | JobLifeEvent.OnJobStatusChanged (realStatus, _) -> Some (JobAction.OnParentJobUpdate realStatus) | _ -> None)
                | JobParent.Batch (BatchId key as parentId, _) ->
                    let dummyStatus = BatchStatus.Unfinished
                    $"JobParent-OnUpdated-%s{key}",
                        jobsDef.LifeCycles.batch.SubscribeToSubjectAndMapLifeEvent parentId
                            (BatchLifeEvent.OnBatchStatusChanged (dummyStatus, job.Id |> BatchSubscriber.Job |> Some))
                            (function | BatchLifeEvent.OnBatchStatusChanged (realStatus, _) -> Some (JobAction.OnParentBatchUpdate realStatus))
            | _ ->
                ()
        ]
        |> Map.ofList

let private lifeEventSatisfies (input: LifeEventSatisfiesInput<JobLifeEvent>) =
    match input.Subscribed, input.Raised with
    | JobLifeEvent.OnJobStatusChanged (_, maybeSubscribedToAllPlusExclusiveFor), JobLifeEvent.OnJobStatusChanged (_, maybePublishedExclusivelyFor) ->
        match maybePublishedExclusivelyFor, maybeSubscribedToAllPlusExclusiveFor with
        | None, _ -> true // event not exclusive, send it to any subscriber
        | Some publishedExclusivelyFor, Some subscribedToAllPlusExclusiveFor ->
            publishedExclusivelyFor = subscribedToAllPlusExclusiveFor
        | Some _, None ->
            false
    | JobLifeEvent.OnProcessingCompleted, JobLifeEvent.OnProcessingCompleted -> true
    | JobLifeEvent.OnJobStatusChanged _, _
    | JobLifeEvent.OnProcessingCompleted, _ -> false

let private shouldSendTelemetry =
    function
    | ShouldSendTelemetryFor.Constructor _ -> false
    | ShouldSendTelemetryFor.LifeAction action ->
        match action with
        | JobAction.Schedule _
        | JobAction.OnHeartbeat _
        | JobAction.Delete _
        | JobAction.DeleteAwaitingJobsBackwards _ -> true
        | JobAction.OnProcessingComplete (_, res) ->
            match res.Result with
            | Error ((* retryForFree *) true, _) -> false
            | Error ((* retryForFree *) false, reason) ->
                match reason with
                | JobFailedReason.ConnectorTimeout _ -> false
                | JobFailedReason.Exception _ -> true
            | Ok _ -> false
        | JobAction.OnParentJobUpdate status ->
            match status with
            | JobStatus.Finished false | JobStatus.Unfinished -> true
            | JobStatus.Finished true -> false
        | JobAction.OnParentBatchUpdate status ->
            match status with
            | BatchStatus.Finished false | BatchStatus.Unfinished -> true
            | BatchStatus.Finished true -> false
        | JobAction.OnNewSubscriber _
        | JobAction.FillPlaceholder _
        | JobAction.Enqueue
        | JobAction.Start _ ->
            false

    | ShouldSendTelemetryFor.LifeEvent _ -> true

let private indices (job: Job) : seq<JobIndex> =
    IndicesWorkflow.indices {
        JobNumericIndex.CreatedOn job.CreatedOn

        match job.Body with
        | JobBodyVariant.Placeholder placeholder ->
            JobNumericIndex.Placeholder job.CreatedOn
            JobNumericIndex.State IndexJobState.Placeholder
            match placeholder.PlacedBy with
            | PlacedBy.Batch batchId ->
                JobStringIndex.Batch batchId
            | PlacedBy.Job _
            | PlacedBy.Client ->
                ()

        | JobBodyVariant.Proper body ->
            JobNumericIndex.SentOn body.SentOn
            body.CorrelationId |> Option.map JobStringIndex.CorrelationId
            body.PlaceholderFilledOn |> Option.map JobNumericIndex.PlaceholderFilledOn
            JobStringIndex.Queue body.QueueName
            // state specific-indices
            match body.State with
            | JobState.Enqueued (orderInQueue, on) ->
                JobNumericIndex.EnqueuedOn on
                JobStringIndex.EnqueuedTo body.QueueName
                // sortable concatenated value such as 001f_00000000ffffffff
                let sort = $"%04x{body.QueueSortOrder}_%016x{orderInQueue}"
                JobStringIndex.DequeueSort sort
            | JobState.Awaiting _ ->
                ()
            | JobState.Scheduled on ->
                JobNumericIndex.ScheduledOn on
            | JobState.Processing (_, unfinishedRun) ->
                JobNumericIndex.ProcessingFrom unfinishedRun.StartedOn
                JobNumericIndex.HeartbeatOn (unfinishedRun.LastHeartbeatOn |> Option.defaultValue unfinishedRun.StartedOn)
                JobNumericIndex.Latency unfinishedRun.Latency
                JobStringIndex.StartedBy unfinishedRun.StartedBy
            | JobState.Failed (_, finishedRun) ->
                JobNumericIndex.FailedOn finishedRun.FinishedOn
                JobNumericIndex.TotalDuration finishedRun.TotalDuration
                finishedRun.PureDuration |> Option.map JobNumericIndex.PureDuration
                JobNumericIndex.Latency finishedRun.Latency
                JobStringIndex.StartedBy finishedRun.StartedBy
            | JobState.Succeeded finishedRun ->
                JobNumericIndex.SucceededOn finishedRun.FinishedOn
                JobNumericIndex.TotalDuration finishedRun.TotalDuration
                finishedRun.PureDuration |> Option.map JobNumericIndex.PureDuration
                JobNumericIndex.Latency finishedRun.Latency
                JobStringIndex.StartedBy finishedRun.StartedBy
                body.Retry |> Option.map (fun r -> r.Attempt |> JobNumericIndex.SucceededRetryAttempt)
            | JobState.Deleted on ->
                JobNumericIndex.DeletedOn on

            match body.State with
            | JobState.Awaiting (JobParent.Job _) -> IndexJobState.AwaitingJob
            | JobState.Awaiting (JobParent.Batch _) -> IndexJobState.AwaitingBatch
            | JobState.Deleted _ -> IndexJobState.Deleted
            | JobState.Enqueued _ -> IndexJobState.Enqueued
            | JobState.Failed _ -> IndexJobState.Failed
            | JobState.Processing _ -> IndexJobState.Processing
            | JobState.Scheduled _ -> IndexJobState.Scheduled
            | JobState.Succeeded _ -> IndexJobState.Succeeded
            |> JobNumericIndex.State

            match body.Retry, job.Status with
            | Some _, JobStatus.Unfinished ->
                match body.State with
                | JobState.Failed  _ -> () // hide failed jobs  from Retries tab, it's highlighted separately
                | _ -> JobNumericIndex.Retry
            | None, _
            | _, JobStatus.Finished _ ->
                    ()

            match body.Scope with
            | JobScope.Batch batchId ->
                JobStringIndex.Batch batchId
            | JobScope.Recurring recurringJobId ->
                JobStringIndex.Recurring recurringJobId
            | JobScope.Other ->
                ()

            body.Parent |> Option.map (function
                | JobParent.Job (parentId, _) -> JobStringIndex.ParentJob parentId
                | JobParent.Batch (parentId, _) -> JobStringIndex.ParentBatch parentId)

            let truncateTo500Chars (s: string) =
                let truncatedPrefix = "TRUNCATED!"
                if s = null then "(NULL)" elif s.Length <= 500 then s else $"{truncatedPrefix}{s.Substring(0,500-truncatedPrefix.Length)}"
            let payload = body.Payload
            JobStringIndex.Type (truncateTo500Chars payload.Type)
            JobStringIndex.Method (truncateTo500Chars payload.Method)
            JobStringIndex.Args (truncateTo500Chars payload.Arguments)
    }

open type PromotedIndex<JobNumericIndex, JobStringIndex>
let private promotedIndicesConfig =
    promotedIndices {
        // TODO: what we need in framework is dynamic index keys, not promotions
        // for efficient Poll
        PromoteIndex JobStringIndex.EnqueuedTo
        // for faster dashboard
        PromoteIndex JobStringIndex.Batch
    }

let createJobLifeCycle (jobRunnerConnector: JobRunnerConnector) =
    newJobsLifeCycle                           jobsDef.LifeCycles.job
    |> LifeCycleBuilder.withTransition         (transition jobRunnerConnector)
    |> LifeCycleBuilder.withIdGeneration       idGeneration
    |> LifeCycleBuilder.withConstruction       construction
    |> LifeCycleBuilder.withIndices            indices
    |> LifeCycleBuilder.withTimers             timers
    |> LifeCycleBuilder.withLifeEventSatisfies lifeEventSatisfies
    |> LifeCycleBuilder.withSubscriptions      subscriptions
    |> LifeCycleBuilder.withStorage
           (StorageType.Persistent
                (promotedIndicesConfig,
                System.TimeSpan.FromDays 5.
                |> PersistentHistoryExpiration.AfterSubjectDeletion |> Some
                |> PersistentHistoryRetention.Unfiltered))
    |> LifeCycleBuilder.withTelemetryRules     shouldSendTelemetry
    |> LifeCycleBuilder.build
