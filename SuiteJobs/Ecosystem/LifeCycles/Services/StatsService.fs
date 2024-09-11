[<AutoOpen>]
module SuiteJobs.LifeCycles.Services.StatsService

open System.Threading.Tasks
open LibLifeCycle


[<RequireQualifiedAccess>]
type JobStatsCounter =
| Created
| SuccessfulRuns
| FailedRuns
| Deleted

[<RequireQualifiedAccess>]
type BatchStatsCounter =
| Created
| Successful
| Completed
| Cancelled

[<RequireQualifiedAccess>]
type StatsRequest =
| IncrementJobCounter of JobStatsCounter * QueueName: NonemptyString * ResponseChannel<unit>
| IncrementBatchCounter of BatchStatsCounter * ResponseChannel<unit>
with interface Request

let private jobSequenceName (counter: JobStatsCounter) (maybeQueueName: Option<NonemptyString>) =
    match counter with
    | JobStatsCounter.Created -> "JobsCreated"
    | JobStatsCounter.SuccessfulRuns -> "JobsSuccessfulRuns"
    | JobStatsCounter.FailedRuns -> "JobsFailedRuns"
    | JobStatsCounter.Deleted -> "JobsDeletions"
    |> fun counterName ->
        match maybeQueueName with
        | None -> $"Total_%s{counterName}"
        | Some queueName -> $"Queue_%s{counterName}_%s{queueName.Value}"

let private batchSequenceName (counter: BatchStatsCounter) =
    match counter with
    | BatchStatsCounter.Created -> "BatchesCreated"
    | BatchStatsCounter.Successful -> "BatchesSuccessful"
    | BatchStatsCounter.Completed -> "BatchesCompleted"
    | BatchStatsCounter.Cancelled -> "BatchesCancelled"
    |> fun counterName ->
        $"Total_%s{counterName}"

let private statsServiceHandler
    (sequence: Service<Sequence>)
    (request: StatsRequest)
    : Task<ResponseVerificationToken> =
    match request with
    | StatsRequest.IncrementJobCounter (counter, queueName, responseChannel) ->
        backgroundTask {
            try
                let! _ = sequence.Query GetNext (jobSequenceName counter None)
                let! _ = sequence.Query GetNext (jobSequenceName counter (Some queueName))
                return responseChannel.Respond ()
            with
            | _ ->
                // yes, simply ignore stats errors
                return responseChannel.Respond ()
        }

    | StatsRequest.IncrementBatchCounter (counter, responseChannel) ->
        backgroundTask {
            try
                let! _ = sequence.Query GetNext (batchSequenceName counter)
                return responseChannel.Respond ()
            with
            | _ ->
                // yes, simply ignore stats errors
                return responseChannel.Respond ()
        }

let createStatsService (sequence: Service<Sequence>) =
    createService "StatsService" (statsServiceHandler sequence)