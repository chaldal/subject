namespace LibLifeCycleHost
// Grains can't be in Modules, due to a bug in the way Orleans builds up Grain identity

open System
open LibLifeCycle
open LibLifeCycleCore
open Orleans
open System.Threading.Tasks
open Orleans.Concurrency
open Microsoft.Extensions.Logging

[<StatelessWorker>]
[<Reentrant>]
type SubjectBlobRepoGrain (
    repo: IBlobRepo,
    ecosystem: Ecosystem,
    logger: Microsoft.Extensions.Logging.ILogger<SubjectBlobRepoGrain>
    ) =

    inherit Grain()

    let wrapExceptions (methodName: string) (createTask: unit -> Task<'T>) : Task<'T> =
        task {
            try
                let! res = createTask ()
                return res
            with
            | :? TransientSubjectException as ex ->
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw()
                return shouldNotReachHereBecause "line above throws"
            | :? PermanentSubjectException as ex ->
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw()
                return shouldNotReachHereBecause "line above throws"
            | ex ->
                logger.LogError(ex, $"Exception in SubjectBlobRepoGrain.${methodName}")
                return TransientSubjectException ($"Blob repo %s{methodName}", ex.ToString()) |> raise
        }

    interface IBlobRepoGrain with
        member _.GetBlobData (subjectRef: LocalSubjectPKeyReference) (blobId: Guid) : Task<Option<BlobData>> =
            fun () -> repo.GetBlobData ecosystem.Name subjectRef blobId
            |> wrapExceptions (nameof(repo.GetBlobData))

    interface ITrackedGrain with
        member _.GetTelemetryData (_methodInfo: System.Reflection.MethodInfo) (_: obj[]) : Option<TrackedGrainTelemetryData> = None