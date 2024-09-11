[<AutoOpen>]
module SuiteJobs.LifeCycles.BatchLifeCycle

open LibLifeCycle
open SuiteJobs.LifeCycles.Services
open SuiteJobs.Types
open LibLifeCycle.LifeCycleAccessBuilder
open type AccessTo<BatchAction, BatchConstructor>

type BatchEnvironment = {
    Clock: Service<Clock>
    Sequence: Service<Sequence>
} with interface Env

let private incrementBatchCounter (sequence: Service<Sequence>) (counter: BatchStatsCounter) =
    (createStatsService sequence).Query StatsRequest.IncrementBatchCounter counter


let private transitionBatchBody (env: BatchEnvironment) (body: BatchBody) (action: BatchAction) : TransitionResult<BatchBody, BatchAction, BatchOpError, BatchLifeEvent, BatchConstructor> =
    transition {
        match action with
        | BatchAction.OnParentBatchUpdate status ->
            match body.ActivationStatus with
            | BatchActivationStatus.Awaiting (BatchParent.Batch _ as parent) ->
                let! now = env.Clock.Query Now
                match status with
                | BatchStatus.Finished _ ->
                    // there's nothing else to do here only mark batch status Activated
                    // the jobs will receive their notification from Batch parent too, possibly even earlier
                    return { body with ActivationStatus = BatchActivationStatus.Activated (now, Some parent) }
                | BatchStatus.Unfinished ->
                    return Transition.Ignore
            | BatchActivationStatus.Activated _ ->
                return Transition.NotAllowed

        | BatchAction.Cancel ->
            // don't move to Activated state - yes batch can be Awaiting its parent yet Finished (by all jobs deleted) at the same time, it's fine
            // best effort cancellation: request to delete all jobs that can be deleted
            match body.CancelRequestedOn with
            | Some _ ->
                return Transition.Ignore
            | None ->
                yield!
                    body.JobsProgress.Values
                    |> Seq.choose (function
                        | JobProgress.Unfinished jobId -> jobsDef.LifeCycles.job.Act jobId JobAction.Delete |> Some
                        | JobProgress.Finished _ -> None)
                let! now = env.Clock.Query Now
                return { body with CancelRequestedOn = Some now }

        | BatchAction.OnJobStatusChanged (jobId, jobStatus) ->
            match body.JobsProgress.GetByKey jobId with
            | Some jobProgress ->
                match jobProgress, jobStatus with
                | JobProgress.Unfinished _, JobStatus.Unfinished ->
                    return Transition.Ignore
                | JobProgress.Finished (_, progressSucceeded, _), JobStatus.Finished succeeded when progressSucceeded = succeeded ->
                    return Transition.Ignore
                | JobProgress.Unfinished _, JobStatus.Finished succeeded
                | JobProgress.Finished _, JobStatus.Finished succeeded ->
                    let! now = env.Clock.Query Now
                    return { body with JobsProgress = body.JobsProgress.AddOrUpdate (JobProgress.Finished (jobId, succeeded, now)) }
                | JobProgress.Finished _, JobStatus.Unfinished ->
                    return { body with JobsProgress = body.JobsProgress.AddOrUpdate (JobProgress.Unfinished jobId) }
            | None ->
                return BatchOpError.JobNotInBatch jobId

        | BatchAction.OnNewSubscriber _ ->
            shouldNotReachHereBecause "OnNewSubscriber must be handled in outer transition"
            return Transition.NotAllowed

        // ignore because body already filled, likely coming from ActMaybeConstruct
        | BatchAction.FillPlaceholder _ ->
            return Transition.Ignore
    }

let private constructBatchBody
    (isFillPlaceholder: bool)
    (env: BatchEnvironment)
    (batchId: BatchId)
    (properBatchCtor: ProperBatchConstructor)
    (cancelRequestedOn: Option<System.DateTimeOffset>)
    : OperationResult<BatchBody, BatchAction, BatchOpError, BatchLifeEvent> =
    operation {
        let! now = env.Clock.Query Now

        let initialBatchBody (jobIds: List<JobId>) =
            let initialJobsProgress = jobIds |> Seq.map JobProgress.Unfinished |> KeyedSet.ofSeq
            { Description = properBatchCtor.Description
              JobsProgress = initialJobsProgress
              CancelRequestedOn = cancelRequestedOn
              ActivationStatus =
                  match properBatchCtor.Parent with
                  | None ->
                      BatchActivationStatus.Activated (now, None)
                  | Some parent ->
                      BatchActivationStatus.Awaiting parent
              SentOn = properBatchCtor.SentOn
              PlaceholderFilledOn = if isFillPlaceholder then Some now else None }

        match properBatchCtor.Parent with
        | Some (BatchParent.Batch (parentId, _)) ->
            // if this batch constructed before its parent, the ActMaybeConstruct will create a placeholder parent to subscribe to,
            // it also combines with Subscribe signal so it's very cheap price for flexibility
            yield jobsDef.LifeCycles.batch.ActMaybeConstruct parentId (BatchAction.OnNewSubscriber (BatchSubscriber.Batch batchId)) (BatchConstructor.NewPlaceholder (parentId, PlacedBy.Batch batchId))
        | None -> ()

        match properBatchCtor.JobsToConstruct with
        | BatchJobsToConstruct.Parallel jobsData ->
            let sideEffects =
                BatchedJobsHelper.parallelBatchedJobsProperCtors batchId cancelRequestedOn properBatchCtor.Parent jobsData
                |> List.map (JobConstructor.NewProper >> jobsDef.LifeCycles.job.Construct)
            yield! sideEffects
            do! incrementBatchCounter env.Sequence BatchStatsCounter.Created
            return initialBatchBody (jobsData |> List.map fst)

        | BatchJobsToConstruct.Sequential (jobsData, sequentialParams) ->
            let sideEffects =
               BatchedJobsHelper.sequentialBatchedJobsProperCtors batchId cancelRequestedOn properBatchCtor.Parent jobsData sequentialParams
               |> List.map (
                    fun (jobId, properCtor) ->
                        // jobs can be constructed out of order, so either construct or fill possible placeholder
                        jobsDef.LifeCycles.job.ActMaybeConstruct jobId (JobAction.FillPlaceholder properCtor) (JobConstructor.NewProper (jobId, properCtor))
                    )

            yield! sideEffects
            do! incrementBatchCounter env.Sequence BatchStatsCounter.Created
            return initialBatchBody (jobsData |> List.map fst)

        | BatchJobsToConstruct.Placeholders placeholderJobIds ->
            let sideEffects =
                placeholderJobIds
                |> List.map (fun jobId ->
                    jobsDef.LifeCycles.job.ActMaybeConstruct jobId (JobAction.OnNewSubscriber (JobSubscriber.Batch batchId)) (JobConstructor.NewPlaceholder (jobId, PlacedBy.Batch batchId)))

            yield! sideEffects
            do! incrementBatchCounter env.Sequence BatchStatsCounter.Created
            return initialBatchBody placeholderJobIds
    }

let private transition' (env: BatchEnvironment) (batch: Batch) (action: BatchAction) : TransitionResult<Batch, BatchAction, BatchOpError, BatchLifeEvent, BatchConstructor> =
    transition {
        match action, batch.Body with
        | BatchAction.OnNewSubscriber subscriber, BatchBodyVariant.Proper _ ->
            yield BatchLifeEvent.OnBatchStatusChanged (batch.Status, Some subscriber)
            return batch

        | _, BatchBodyVariant.Proper body ->
            let! updatedBody = transitionBatchBody env body action
            return { batch with Body = BatchBodyVariant.Proper updatedBody }

        | _, BatchBodyVariant.Placeholder placeholder ->
            match action with
            | BatchAction.FillPlaceholder ctor ->
                let! body = constructBatchBody (* isFillPlaceholder *) true env batch.Id ctor placeholder.CancelRequestedOn
                return { batch with Body = BatchBodyVariant.Proper body }

            | BatchAction.Cancel ->
                match placeholder.CancelRequestedOn with
                | Some _ ->
                    return Transition.Ignore
                | None ->
                    let! now = env.Clock.Query Now
                    return { batch with Body = BatchBodyVariant.Placeholder { placeholder with CancelRequestedOn = Some now } }

            | BatchAction.OnNewSubscriber subscriber ->
                let placedAndSubscribedBySameThing =
                    match placeholder.PlacedBy, subscriber with
                    | PlacedBy.Batch placedByBatchId, BatchSubscriber.Batch subscriberBatchId ->
                        placedByBatchId = subscriberBatchId
                    | PlacedBy.Job placedByJobId, BatchSubscriber.Job subscriberJobId ->
                        placedByJobId = subscriberJobId
                    | PlacedBy.Client, _
                    | PlacedBy.Batch _, _
                    | PlacedBy.Job _, _ -> false

                if placedAndSubscribedBySameThing then
                    return Transition.Ignore
                else
                    yield BatchLifeEvent.OnBatchStatusChanged (batch.Status, Some subscriber)
                    return batch

            | BatchAction.OnJobStatusChanged _
            | BatchAction.OnParentBatchUpdate _ ->
                return Transition.NotAllowed
    }

let private transition (env: BatchEnvironment) (batch: Batch) (action: BatchAction) : TransitionResult<Batch, BatchAction, BatchOpError, BatchLifeEvent, BatchConstructor> =
    transition {
        let! batchAfter = transition' env batch action
        let statusAfter = batchAfter.Status
        if batch.Status <> statusAfter then
            yield BatchLifeEvent.OnBatchStatusChanged (statusAfter, None)

        let indexStateAfter = batchAfter.IndexBatchState
        do!
            backgroundTask {
                if (batch.IndexBatchState <> indexStateAfter) then
                    match indexStateAfter with
                    | None
                    | Some IndexBatchState.AwaitingBatch
                    | Some IndexBatchState.AwaitingJob
                    | Some IndexBatchState.Started ->
                        ()
                    | Some IndexBatchState.Cancelled ->
                        do! incrementBatchCounter env.Sequence BatchStatsCounter.Cancelled
                    | Some (IndexBatchState.Finished (* succeeded *) true) ->
                        do! incrementBatchCounter env.Sequence BatchStatsCounter.Successful
                    | Some (IndexBatchState.Finished (* succeeded *) false) ->
                        do!  incrementBatchCounter env.Sequence BatchStatsCounter.Completed
            }

        return batchAfter
    }

let private construction (env: BatchEnvironment) (id: BatchId) (ctor: BatchConstructor) : ConstructionResult<Batch, BatchAction, BatchOpError, BatchLifeEvent> =
    construction {
        let! now = env.Clock.Query Now
        let! initialBatch =
            construction {
                match ctor with
                | BatchConstructor.NewPlaceholder (_, placedBy) ->
                    return ({ Id = id; CreatedOn = now; Body = BatchBodyVariant.Placeholder { CancelRequestedOn = None; PlacedBy = placedBy } } : Batch)
                | BatchConstructor.NewProper (_, properCtor) ->
                    let! body = constructBatchBody (* isFillPlaceholder *) false env id properCtor (* cancelRequestedOn *) None
                    return ({ Id = id; CreatedOn = now; Body = BatchBodyVariant.Proper body } : Batch)
            }
        match initialBatch.Status with
        | BatchStatus.Unfinished _ -> ()
        | BatchStatus.Finished succeeded ->
            // creator-subscribers may need to know that if batch was finished right upon construction
            yield BatchLifeEvent.OnBatchStatusChanged (BatchStatus.Finished succeeded, None)
        return initialBatch
    }

let private idGeneration (_env: BatchEnvironment) (ctor: BatchConstructor) : IdGenerationResult<BatchId, BatchOpError> =
    idgen {
        match ctor with
        | BatchConstructor.NewPlaceholder (id, _)
        | BatchConstructor.NewProper (id, _) ->
            return id
    }

let private lifeEventSatisfies (input: LifeEventSatisfiesInput<BatchLifeEvent>) =
    match input.Subscribed, input.Raised with
    | BatchLifeEvent.OnBatchStatusChanged (_, maybeSubscribedToAllPlusExclusiveFor), BatchLifeEvent.OnBatchStatusChanged (_, maybePublishedExclusivelyFor) ->
        match maybePublishedExclusivelyFor, maybeSubscribedToAllPlusExclusiveFor with
        | None, _ -> true // event not exclusive, send it to any subscriber
        | Some publishedExclusivelyFor, Some subscribedToAllPlusExclusiveFor ->
            publishedExclusivelyFor = subscribedToAllPlusExclusiveFor
        | Some _, None ->
            false

let private subscriptions (batch: Batch) =
    match batch.Body with
    | BatchBodyVariant.Placeholder _ -> Map.empty
    | BatchBodyVariant.Proper body ->
        [
            match batch.Status with
            | BatchStatus.Unfinished _ ->
                let dummyStatus = JobStatus.Unfinished
                yield!
                    body.JobsProgress.Keys
                    |> Seq.map (fun (JobId key as jobId) ->
                        $"Job-OnUpdated-%s{key}",
                                jobsDef.LifeCycles.job.SubscribeToSubjectAndMapLifeEvent
                                    jobId
                                    (JobLifeEvent.OnJobStatusChanged (dummyStatus, Some (JobSubscriber.Batch batch.Id)))
                                    (function JobLifeEvent.OnJobStatusChanged (status, _) -> Some <| BatchAction.OnJobStatusChanged (jobId, status) | _ -> None)
                        )
            | BatchStatus.Finished _ ->
                ()

            match body.ActivationStatus with
            | BatchActivationStatus.Awaiting parent ->
                match parent with
                | BatchParent.Batch (BatchId key as parentBatchId, _) ->
                    let dummyStatus = BatchStatus.Unfinished
                    (
                        $"BatchParent-OnUpdated-%s{key}",
                            jobsDef.LifeCycles.batch.SubscribeToSubjectAndMapLifeEvent
                                parentBatchId (BatchLifeEvent.OnBatchStatusChanged (dummyStatus, batch.Id |> BatchSubscriber.Batch |> Some))
                                (function BatchLifeEvent.OnBatchStatusChanged (realStatus, _) -> Some <| BatchAction.OnParentBatchUpdate realStatus)
                    )
            | BatchActivationStatus.Activated _ ->
                ()
        ]
        |> Map.ofList

let private timers (batch: Batch) =
    match batch.Body with
    | BatchBodyVariant.Placeholder _ -> []
    | BatchBodyVariant.Proper body ->
        [
            match body.ActivationStatus, batch.Status with
            | BatchActivationStatus.Activated _,  BatchStatus.Finished (_: bool) ->
                body.JobsProgress.Values
                |> Seq.choose (function JobProgress.Unfinished _ -> None | JobProgress.Finished (_, _, on) -> Some on)
                |> Seq.append [batch.CreatedOn] // for empty batches
                |> Seq.max
                |> fun lastJobFinishedOn ->
                    { TimerAction = TimerAction.DeleteSelf
                      Schedule = Schedule.On (lastJobFinishedOn.AddDays 2) }
            | BatchActivationStatus.Awaiting _ , _
            | _, BatchStatus.Unfinished _ ->
                ()
        ]

let private responseHandler (response: SideEffectResponse) =
    seq {
        match jobsDef.LifeCycles.job.OnResponse response with
        // job might be succeeded or physically gone by the time batch cancellation requested, it's fine
        | ActNotInitialized (_, JobAction.Delete, next)
        | ActError (_, JobAction.Delete, JobOpError.CannotDeleteSuccessfulJob, next) ->
            next.Dismiss()
        | _ ->
            ()
    }

let private indices (batch: Batch) : seq<BatchIndex> =
    IndicesWorkflow.indices {
        BatchNumericIndex.CreatedOn batch.CreatedOn
        batch.IndexBatchState |> Option.map BatchNumericIndex.State

        match batch.Body with
        | BatchBodyVariant.Placeholder _ ->
            BatchNumericIndex.Placeholder batch.CreatedOn

        | BatchBodyVariant.Proper body ->
            BatchNumericIndex.SentOn body.SentOn
            body.PlaceholderFilledOn |> Option.map BatchNumericIndex.PlaceholderFilledOn

            // state-specific indices
            match batch.Status with
            | BatchStatus.Unfinished ->
                match body.ActivationStatus with
                | BatchActivationStatus.Awaiting _ -> ()
                | BatchActivationStatus.Activated (activatedOn, _) ->
                    BatchNumericIndex.StartedOn activatedOn
            | BatchStatus.Finished _ ->
                body.JobsProgress.Values
                |> Seq.choose (function JobProgress.Unfinished _ -> None | JobProgress.Finished (_, _, on) -> Some on)
                |> Seq.append [batch.CreatedOn]
                |> Seq.max
                |> BatchNumericIndex.FinishedOn
                body.CancelRequestedOn |> Option.map BatchNumericIndex.CancelledOn

            body.Parent |> Option.map (function
                | BatchParent.Batch (parentId, _) -> BatchStringIndex.ParentBatch parentId)
    }

open type PromotedIndex<BatchNumericIndex, BatchStringIndex>
let private promotedIndicesConfig =
    promotedIndices {
        PromoteIndex BatchNumericIndex.State
    }

let private shouldSendTelemetry =
    function
    | ShouldSendTelemetryFor.Constructor _ -> false
    | ShouldSendTelemetryFor.LifeAction action ->
        match action with
        | BatchAction.Cancel -> true
        | BatchAction.OnParentBatchUpdate status ->
            match status with
            | BatchStatus.Unfinished | BatchStatus.Finished false -> true
            | BatchStatus.Finished true -> false
        | BatchAction.OnJobStatusChanged _
        | BatchAction.OnNewSubscriber _
        | BatchAction.FillPlaceholder _ -> false
    | ShouldSendTelemetryFor.LifeEvent _ -> false

let batchLifeCycle =
    newJobsLifeCycle                           jobsDef.LifeCycles.batch
    |> LifeCycleBuilder.withTransition         transition
    |> LifeCycleBuilder.withIdGeneration       idGeneration
    |> LifeCycleBuilder.withConstruction       construction
    |> LifeCycleBuilder.withIndices            indices
    |> LifeCycleBuilder.withTimers             timers
    |> LifeCycleBuilder.withLifeEventSatisfies lifeEventSatisfies
    |> LifeCycleBuilder.withSubscriptions      subscriptions
    |> LifeCycleBuilder.withResponseHandler    responseHandler
    |> LifeCycleBuilder.withStorage
           (StorageType.Persistent
                (promotedIndicesConfig,
                System.TimeSpan.FromDays 5.
                |> PersistentHistoryExpiration.AfterSubjectDeletion |> Some
                |> PersistentHistoryRetention.Unfiltered))
    |> LifeCycleBuilder.withTelemetryRules     shouldSendTelemetry
    |> LifeCycleBuilder.build
