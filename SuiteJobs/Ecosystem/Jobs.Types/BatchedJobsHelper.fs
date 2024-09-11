/// Shared helper functions that help implement convenient cases of BatchJobsToConstruct (such as Sequential and Parallel) on host side,
/// or achieve similar behavior via BatchJobsToConstruct.Placeholders with jobs creation driven on the client side
/// (latter can be useful if batch is too large and size of BatchConstructor becomes a concern)
module SuiteJobs.Types.BatchedJobsHelper

let private firstInChainProperJobCtor
    (batchId: BatchId)
    (maybeCancelRequestedOn: Option<System.DateTimeOffset>)
    (maybeAwaiting: Option<BatchParent>)
    (common: JobConstructorCommonData) =
    match maybeCancelRequestedOn, maybeAwaiting with
    | None, None ->
        // first job in chain of jobs in non-cancelled batch without a parent constructed as Enqueued
        ProperJobConstructor.Enqueued (common, JobScope.Batch batchId)
    | None, Some (BatchParent.Batch (parentId, status)) ->
        // first job in chain of jobs in non-cancelled Enqueued batch constructed as Enqueued
        ProperJobConstructor.Awaiting (common, JobScope.Batch batchId, JobParent.Batch (parentId, status))
    | Some _, _ ->
        // first job in chain of jobs in batch that was cancelled before it was constructed will be constructed as Deleted
        ProperJobConstructor.Deleted (common, JobScope.Batch batchId)


let private secondOrFurtherInChainProperJobCtor
    (batchId: BatchId)
    (maybeCancelRequestedOn: Option<System.DateTimeOffset>)
    (awaitingForJobStatus: AwaitingForJobStatus)
    (prevJobId: JobId)
    (common: JobConstructorCommonData) =
    match maybeCancelRequestedOn with
    | None ->
        // second or further job in chain of jobs in non-cancelled batch constructed as Awaiting previous job
        ProperJobConstructor.Awaiting (common, JobScope.Batch batchId, JobParent.Job (prevJobId, awaitingForJobStatus))
    | Some _ ->
        // second or further job in chain of jobs in batch that was cancelled while it was a placeholder will be constructed as Deleted
        ProperJobConstructor.Deleted (common, JobScope.Batch batchId)


let parallelBatchedJobsProperCtors
    (batchId: BatchId)
    (maybeCancelRequestedOn: Option<System.DateTimeOffset>)
    (maybeAwaiting: Option<BatchParent>)
    (jobsData: List<JobId * JobConstructorCommonData>) =
    jobsData |> List.map (fun (jobId, common) -> jobId, firstInChainProperJobCtor batchId maybeCancelRequestedOn maybeAwaiting common)


let sequentialBatchedJobsProperCtors
    (batchId: BatchId)
    (maybeCancelRequestedOn: Option<System.DateTimeOffset>)
    (maybeAwaiting: Option<BatchParent>)
    (jobsData: List<JobId * JobConstructorCommonData>)
    (sequentialParams: SequentialJobsParams)=
    jobsData
    |> List.mapi (fun i (jobId, data) -> {| GroupNo = i % sequentialParams.NumberOfThreads.Value; JobId = jobId; Data = data |})
    |> List.groupBy (fun x -> x.GroupNo)
    |> List.collect (fun (_, group) ->
        match group with
        | [] -> []
        | firstInChain :: rest ->
            [
                yield firstInChain.JobId, firstInChainProperJobCtor batchId maybeCancelRequestedOn maybeAwaiting firstInChain.Data
                yield!
                    Seq.zip rest group
                    |> Seq.map (fun (child, parent) ->
                        child.JobId, secondOrFurtherInChainProperJobCtor batchId maybeCancelRequestedOn sequentialParams.AwaitingForJobStatus parent.JobId child.Data)
            ]
        )
