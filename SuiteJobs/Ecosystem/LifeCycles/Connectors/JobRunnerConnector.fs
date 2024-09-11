[<AutoOpen>]
module SuiteJobs.LifeCycles.Connectors.JobRunnerConnector

open System
open System.Threading.Tasks
open Microsoft.Extensions.Configuration
open SuiteJobs.Types
open LibLifeCycle


type JobRunnerConnectorEnvironment = {
    Clock: Service<Clock>
    Configuration: IConfiguration
}
with interface Env

type RunJobRequestData = {
    Ticket: Guid
    JobId: JobId // jobId is unused but is helpful for testing
    IsAtMostOnce: bool
    Payload: JobPayload
}

[<RequireQualifiedAccess>]
type ProcessingJobUpdate =
| Heartbeat
| Completed of JobProcessingResult

[<RequireQualifiedAccess>]
type JobRunnerRequest =
| RunJob of RunJobRequestData * MultiResponseChannel<Guid * ProcessingJobUpdate>
with interface Request

type RunJobDelegate = JobRunnerConnectorEnvironment -> RunJobRequestData -> Task<JobProcessingResult>

let private requestProcessor
    (runJob: RunJobDelegate)
    (env: JobRunnerConnectorEnvironment)
    (request: JobRunnerRequest)
    : Task<ResponseVerificationToken> =
    match request with
    | JobRunnerRequest.RunJob (data, responseChannel) ->
        backgroundTask {
            let stopwatch = System.Diagnostics.Stopwatch()
            stopwatch.Start()
            try
                let mutable jobTaskCompleted = false
                let runJobTask = runJob env data

                while not jobTaskCompleted do
                    // heartbeats every 30 seconds
                    let! taskThatFinishedFirst = Task.WhenAny(runJobTask, Task.Delay (TimeSpan.FromSeconds 30.))
                    if taskThatFinishedFirst = runJobTask then
                        let! jobResult = runJobTask
                        responseChannel.RespondNext (data.Ticket, ProcessingJobUpdate.Completed jobResult)
                        jobTaskCompleted <- true
                    else
                        responseChannel.RespondNext (data.Ticket, ProcessingJobUpdate.Heartbeat)

                return responseChannel.Complete()
            with
            ex ->
                stopwatch.Stop()
                let retryForFree = not data.IsAtMostOnce
                responseChannel.RespondNext (
                    data.Ticket,
                    { TotalDuration = stopwatch.Elapsed; PureDuration = None; Result = Error (retryForFree, JobFailedReason.Exception (ex.Message, ex.GetType().FullName, ex.ToString())) }
                    |> ProcessingJobUpdate.Completed)
                return responseChannel.Complete()
        }

type JobRunnerConnector = Connector<JobRunnerRequest, JobRunnerConnectorEnvironment>

let createJobRunnerConnector (runJob: RunJobDelegate) : JobRunnerConnector =
    ConnectorBuilder.newConnector "JobRunner"
    |> ConnectorBuilder.withRequestProcessor (requestProcessor runJob)
    |> ConnectorBuilder.withDisabledTelemetry
