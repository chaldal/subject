module Jobs.``Batch Tests``

open System
open SuiteJobs.Types
open SuiteJobs.LifeCycles
open SuiteJobs.LifeCycles.Connectors

let dummyDispatcherId = "DummyDispatcher" |> NonemptyString.ofStringUnsafe |> DispatcherId

let ``Create activated parallel batch``() =
    simulation {
        let! batchId = genBatchId
        let! batchJobsToConstruct = genParallelBatchJobsToConstruct

        let! now = Ecosystem.now
        let! batch =
            BatchConstructor.NewProper (batchId, { Description = None; JobsToConstruct = batchJobsToConstruct; Parent = None; SentOn = now })
            |> Ecosystem.construct batchLifeCycle
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenAssertEventually batchLifeCycle (fun b ->
                match b.ProperBody.ActivationStatus with
                | BatchActivationStatus.Activated _ ->
                    b.ProperBody.JobsProgress.Values |> Seq.forall (function JobProgress.Unfinished _ -> true | JobProgress.Finished _ -> false)
                | BatchActivationStatus.Awaiting _ -> false)

        return batch
    }

let ``Create awaiting parallel batch``
        (awaitForBatchId: BatchId)
        (overrideAwaitForStatus: Option<AwaitingForBatchStatus>) =
    simulation {
        let! batchId = genBatchId
        let! batchJobsToConstruct = genParallelBatchJobsToConstruct

        let! now = Ecosystem.now
        let! batch =
            BatchConstructor.NewProper (batchId,
                { Description = None
                  JobsToConstruct = batchJobsToConstruct
                  Parent = BatchParent.Batch (awaitForBatchId, overrideAwaitForStatus |> Option.defaultValue AwaitingForBatchStatus.OnlySucceeded) |> Some
                  SentOn = now })

            |> Ecosystem.construct batchLifeCycle
            |> Ecosystem.thenAssertOk
            // can't do assertEventually because it may get activated straight away if parent batch already finished
            |> Ecosystem.thenAssert (fun b -> match b.ProperBody.ActivationStatus with BatchActivationStatus.Awaiting _ -> true | _ -> false)
        return batch
    }

let ``Create simple sequential batch of two jobs`` (awaitingForJobStatus: AwaitingForJobStatus) =
    simulation {
        let! batchId = genBatchId
        let! parentJobId = genJobId
        let! jobData1 = genTypicalJobConstructorCommonData
        let! childJobId = genJobId
        let! jobData2 = genTypicalJobConstructorCommonData
        let sequentialParams =
            { AwaitingForJobStatus = awaitingForJobStatus
              NumberOfThreads = PositiveInteger.One }
        let batchJobsToConstruct = BatchJobsToConstruct.Sequential ([parentJobId, jobData1; childJobId, jobData2], sequentialParams)

        let! now = Ecosystem.now
        let! batch =
            BatchConstructor.NewProper(batchId, { Description = None; JobsToConstruct = batchJobsToConstruct; Parent = None; SentOn = now })
            |> Ecosystem.construct batchLifeCycle
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenAwaitSystemStasis
            |> Ecosystem.thenGetLatest batchLifeCycle

        do! childJobId
            |> Ecosystem.get jobLifeCycle
            |> Ecosystem.thenAssertSome
            |> Ecosystem.thenAssert (fun j -> match j.ProperState with JobState.Awaiting (JobParent.Job (forJobId, _)) -> forJobId = parentJobId | _ -> false)
            |> Ecosystem.thenIgnore

        return {| Batch = batch; ParentJobId = parentJobId; ChildJobId = childJobId |}
    }

let ``Start parallel batch jobs and run successfully`` (activatedBatch: Batch) =
    simulation {
        let jobIds = activatedBatch.ProperBody.JobsProgress.Keys

        use! __ =
            Ecosystem.useConnector jobRunnerConnector (function
                JobRunnerRequest.RunJob (data, responseChannel) ->
                    responseChannel.RespondNext (data.Ticket, { Result = Ok (); TotalDuration = TimeSpan.Zero; PureDuration = None } |> ProcessingJobUpdate.Completed)
                    responseChannel.Complete() |> Some)

        let! jobs =
            jobIds
            |> Seq.map (fun jobId ->
                jobId
                |> Ecosystem.actOnId jobLifeCycle (JobAction.Start dummyDispatcherId)
                |> Ecosystem.thenAssertOk
                |> Ecosystem.thenAwaitSystemStasis
                |> Ecosystem.thenGetLatest jobLifeCycle)
            |> Ecosystem.runInParallel
        if jobs |> List.forall (fun j -> match j.Status with JobStatus.Finished (* succeeded *) true -> true | _ -> false) then
            ()
        else
            failwith "Some jobs did not finish successfully"
    }

[<Simulation>]
let ``All batch jobs started and run successfully, batch ends in successfully Finished status`` () =
    simulation {
        let! batch = ``Create activated parallel batch`` ()

        do! ``Start parallel batch jobs and run successfully`` batch

        let! batch =
            batch.Id
            |> Ecosystem.get batchLifeCycle
            |> Ecosystem.thenAssertSome
            |> Ecosystem.thenAssert (fun b ->
                match b.Status with | BatchStatus.Finished (* success *) true -> true | _ -> false)

        return batch
    }

[<Simulation>]
let ``Finished batch is hard deleted after a couple of days`` () =
    simulation {
        do! ``All batch jobs started and run successfully, batch ends in successfully Finished status`` ()
            |> Ecosystem.simulate
            |> Ecosystem.thenMoveTimeForwardAndRunReminders (TimeSpan.FromDays 2.)
            |> Ecosystem.thenMap (fun b -> b.Id)
            |> Ecosystem.thenAssertEventuallyDeleted batchLifeCycle
    }

[<Simulation>]
let ``All batch jobs started, one failed then deleted, others successful, batch ends in unsuccessfully Finished status`` () =
    simulation {
        let! batch = ``Create activated parallel batch`` ()
        let jobIds = batch.ProperBody.JobsProgress.Keys |> Set.toList
        let unluckyJobId = jobIds.Head

        use! __ =
            Ecosystem.useConnector jobRunnerConnector (function
                JobRunnerRequest.RunJob (data, responseChannel) ->
                    let res = if data.JobId = unluckyJobId then Error ((* retryForFree *) false, JobFailedReason.Exception ("bad luck", "System.Exception", "details")) else Ok ()
                    responseChannel.RespondNext (data.Ticket, { Result = res; TotalDuration = TimeSpan.Zero; PureDuration = None } |> ProcessingJobUpdate.Completed)
                    responseChannel.Complete() |> Some)

        let! jobs =
            jobIds
            |> Seq.map (fun jobId ->
                jobId
                |> Ecosystem.actOnId jobLifeCycle (JobAction.Start dummyDispatcherId)
                |> Ecosystem.thenAssertOk
                |> Ecosystem.thenAwaitSystemStasis
                |> Ecosystem.thenGetLatest jobLifeCycle)
            |> Ecosystem.runInParallel
        if jobs |> List.forall (fun j ->
            //
            match j.Status with
            | JobStatus.Finished true -> j.Id <> unluckyJobId
            | JobStatus.Unfinished -> j.Id = unluckyJobId
            | _ -> false) then
            ()
        else
            failwith "Some jobs in the batch have unexpected status"

        do! batch.Id
            |> Ecosystem.get batchLifeCycle
            |> Ecosystem.thenAssertSome
            // batch not finished until failed job is deleted
            |> Ecosystem.thenAssert (fun b ->
                match b.Status with | BatchStatus.Unfinished -> true | _ -> false)
            |> Ecosystem.thenIgnore

        do! unluckyJobId
            |> Ecosystem.actOnId jobLifeCycle JobAction.Delete
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenIgnore

        do! batch.Id
            |> Ecosystem.get batchLifeCycle
            |> Ecosystem.thenAssertSome
            |> Ecosystem.thenAssertEventually batchLifeCycle (fun b ->
                match b.Status with | BatchStatus.Finished (* success *) false -> true | _ -> false)
            |> Ecosystem.thenIgnore
    }

[<Simulation>]
let ``Batch cancelled before jobs started, ends in unsuccessfully Finished status`` () =
    simulation {
        do!
            ``Create activated parallel batch`` ()
            |> Ecosystem.simulate
            |> Ecosystem.thenAct batchLifeCycle BatchAction.Cancel
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenAssertEventually batchLifeCycle (fun b ->
                match b.ProperBody.ActivationStatus, b.Status with
                | BatchActivationStatus.Activated _, BatchStatus.Finished (* succeeded *) false -> true
                | _ -> false)
            |> Ecosystem.thenIgnore
    }

[<Simulation>]
let ``Batch cancelled after it finished successfully, remains in successfully Finished status`` () =
    simulation {
        do!
            ``All batch jobs started and run successfully, batch ends in successfully Finished status`` ()
            |> Ecosystem.simulate
            |> Ecosystem.thenAct batchLifeCycle BatchAction.Cancel
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenAssertEventually batchLifeCycle (fun b ->
                match b.ProperBody.ActivationStatus, b.Status with
                | BatchActivationStatus.Activated _, BatchStatus.Finished (* succeeded *) true -> true
                | _ -> false)
            |> Ecosystem.thenIgnore
    }

[<Simulation>]
let ``Sequential batch jobs run successfully in order, batch ends in successfully Finished status`` () =
    simulation {
        let! x = ``Create simple sequential batch of two jobs`` AwaitingForJobStatus.OnlySucceeded

        use! __ =
            Ecosystem.useConnector jobRunnerConnector (function
                JobRunnerRequest.RunJob (data, responseChannel) ->
                    responseChannel.RespondNext (data.Ticket, { Result = Ok (); TotalDuration = TimeSpan.Zero; PureDuration = None } |> ProcessingJobUpdate.Completed)
                    responseChannel.Complete() |> Some)
        do!
            x.ParentJobId
            |> Ecosystem.actOnId jobLifeCycle (JobAction.Start dummyDispatcherId)
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenAwaitSystemStasis
            |> Ecosystem.thenIgnore

        do! x.Batch.Id
            |> Ecosystem.get batchLifeCycle
            |> Ecosystem.thenAssertSome
            |> Ecosystem.thenAssertEventually batchLifeCycle (fun b ->
                match b.Status with | BatchStatus.Unfinished _ -> true | _ -> false)
            |> Ecosystem.thenIgnore

        do! x.ChildJobId
            |> Ecosystem.get jobLifeCycle
            |> Ecosystem.thenAssertSome
            |> Ecosystem.thenAssertEventually jobLifeCycle (fun j ->
                match j.ProperState with JobState.Enqueued _ -> true | _ -> false)
            |> Ecosystem.thenAct jobLifeCycle (JobAction.Start dummyDispatcherId)
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenIgnore

        do! x.Batch.Id
            |> Ecosystem.get batchLifeCycle
            |> Ecosystem.thenAssertSome
            |> Ecosystem.thenAssertEventually batchLifeCycle (fun b ->
                match b.Status with | BatchStatus.Finished (* succeeded *) true -> true | _ -> false)
            |> Ecosystem.thenIgnore
    }

[<Simulation>]
let ``Sequential batch parent job deleted, batch ends in unsuccessfully Finished status`` () =
    simulation {
        let! x = ``Create simple sequential batch of two jobs`` AwaitingForJobStatus.OnlySucceeded

        do! x.Batch.Id
            |> Ecosystem.get batchLifeCycle
            |> Ecosystem.thenAssertSome
            |> Ecosystem.thenAssertEventually batchLifeCycle (fun b ->
                match b.Status with | BatchStatus.Unfinished _ -> true | _ -> false)
            |> Ecosystem.thenIgnore

        do! x.ParentJobId
            |> Ecosystem.actOnId jobLifeCycle JobAction.Delete
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenIgnore

        do! x.Batch.Id
            |> Ecosystem.get batchLifeCycle
            |> Ecosystem.thenAssertSome
            |> Ecosystem.thenAssertEventually batchLifeCycle (fun b ->
                match b.Status with | BatchStatus.Finished (* succeeded *) false -> true | _ -> false)
            |> Ecosystem.thenIgnore
    }

[<Simulation>]
let ``Sequential batch await for finished job state, parent job deleted, child job enqueues, batch still Unfinished`` () =
    simulation {
        let! x = ``Create simple sequential batch of two jobs`` AwaitingForJobStatus.AnyFinishedState

        do! x.ParentJobId
            |> Ecosystem.actOnId jobLifeCycle JobAction.Delete
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenIgnore

        do! x.ChildJobId
            |> Ecosystem.get jobLifeCycle
            |> Ecosystem.thenAssertSome
            |> Ecosystem.thenAssertEventually jobLifeCycle (fun j ->
                match j.ProperState with JobState.Enqueued _ -> true | _ -> false)
            |> Ecosystem.thenIgnore

        do! x.Batch.Id
            |> Ecosystem.get batchLifeCycle
            |> Ecosystem.thenAssertSome
            |> Ecosystem.thenAssertEventually batchLifeCycle (fun b ->
                match b.Status with | BatchStatus.Unfinished _ -> true | _ -> false)
            |> Ecosystem.thenIgnore
    }

[<Simulation>]
let ``Awaiting batch activates only when parent batch succeeds`` () =
    simulation {
        let! parentBatch = ``Create activated parallel batch`` ()
        let! childBatch = ``Create awaiting parallel batch`` parentBatch.Id None

        do! ``Start parallel batch jobs and run successfully`` parentBatch

        do! childBatch.Id
            |> Ecosystem.get batchLifeCycle
            |> Ecosystem.thenAssertSome
            |> Ecosystem.thenAssert (fun b -> match b.ProperBody.ActivationStatus with BatchActivationStatus.Activated _ -> true | _ -> false)
            |> Ecosystem.thenIgnore
    }

[<Simulation>]
let ``Awaiting batch constructed before parent creates a parent batch placeholder`` () =
    simulation {
        let! parentBatchId = genBatchId
        let! childBatch =
            ``Create awaiting parallel batch`` parentBatchId None
            |> Ecosystem.simulate
            |> Ecosystem.thenAssert (fun b -> match b.ProperBody.ActivationStatus with | BatchActivationStatus.Awaiting _ -> true | _ -> false)

        do! Ecosystem.awaitSystemStasis() // let awaiting jobs construct

        let! parentBatchPlaceholder =
            parentBatchId
            |> Ecosystem.get batchLifeCycle
            |> Ecosystem.thenAssertSome
            |> Ecosystem.thenAssert (fun pb -> match pb.Body with | BatchBodyVariant.Placeholder _ -> true | _ -> false)

        return childBatch, parentBatchPlaceholder
    }

[<Simulation>]
let ``Awaiting batch constructed before parent, parent placeholder filled, child activates when parent succeeds`` () =
    simulation {
        let! childBatch, parentBatchPlaceholder = ``Awaiting batch constructed before parent creates a parent batch placeholder`` ()

        let! batchJobsToConstruct = genParallelBatchJobsToConstruct

        let! now = Ecosystem.now
        let! parentBatch =
            parentBatchPlaceholder
            |> Ecosystem.act batchLifeCycle (BatchAction.FillPlaceholder { Description = None; JobsToConstruct = batchJobsToConstruct; Parent = None; SentOn = now })
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenAssertEventually batchLifeCycle (fun b ->
                match b.ProperBody.ActivationStatus with | BatchActivationStatus.Activated _ -> true | _ -> false)

        do! ``Start parallel batch jobs and run successfully`` parentBatch

        do! childBatch.Id
            |> Ecosystem.get batchLifeCycle
            |> Ecosystem.thenAssertSome
            |> Ecosystem.thenAssert (fun b -> match b.ProperBody.ActivationStatus with BatchActivationStatus.Activated _ -> true | _ -> false)
            |> Ecosystem.thenIgnore
    }

[<Simulation>]
let ``Awaiting batch constructed before parent, parent placeholder filled with empty batch, child activates`` () =
    simulation {
        let! childBatch, parentBatchPlaceholder = ``Awaiting batch constructed before parent creates a parent batch placeholder`` ()

        let! now = Ecosystem.now
        do! parentBatchPlaceholder
            |> Ecosystem.act batchLifeCycle (BatchAction.FillPlaceholder { Description = None; JobsToConstruct = BatchJobsToConstruct.Parallel []; Parent = None; SentOn = now })
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenAssertEventually batchLifeCycle (fun b ->
                match b.ProperBody.ActivationStatus with | BatchActivationStatus.Activated _ -> true | _ -> false)
            |> Ecosystem.thenIgnore

        do! childBatch.Id
            |> Ecosystem.get batchLifeCycle
            |> Ecosystem.thenAssertSome
            |> Ecosystem.thenAssert (fun b -> match b.ProperBody.ActivationStatus with BatchActivationStatus.Activated _ -> true | _ -> false)
            |> Ecosystem.thenIgnore
    }

[<Simulation>]
let ``Awaiting batch constructed before parent, parent placeholder cancelled then filled, both batches are cancelled in the end`` () =
    simulation {
        let! childBatch, parentBatchPlaceholder = ``Awaiting batch constructed before parent creates a parent batch placeholder`` ()

        let! batchJobsToConstruct = genParallelBatchJobsToConstruct

        let! now = Ecosystem.now
        do! parentBatchPlaceholder
            |> Ecosystem.act batchLifeCycle BatchAction.Cancel
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenAct batchLifeCycle (BatchAction.FillPlaceholder { Description = None; JobsToConstruct = batchJobsToConstruct; Parent = None; SentOn = now })
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenAssertEventually batchLifeCycle (fun b ->
                match b.Status with | BatchStatus.Finished (* success *) false -> true | _ -> false)
            |> Ecosystem.thenIgnore

        do! Ecosystem.awaitSystemStasis()

        do! childBatch.Id
            |> Ecosystem.get batchLifeCycle
            |> Ecosystem.thenAssertSome
            |> Ecosystem.thenAssert (fun b -> match b.Status with BatchStatus.Finished (* success *) false -> true | _ -> false)
            |> Ecosystem.thenIgnore
    }

[<Simulation>]
let ``Awaiting batch activates asap after construction if parent batch already succeeded`` () =
    simulation {
        let! parentBatch = ``All batch jobs started and run successfully, batch ends in successfully Finished status`` ()
        do!
            ``Create awaiting parallel batch`` parentBatch.Id None
            |> Ecosystem.simulate
            |> Ecosystem.thenAssertEventually batchLifeCycle (fun b -> match b.ProperBody.ActivationStatus with BatchActivationStatus.Activated _ -> true | _ -> false)
            |> Ecosystem.thenIgnore
    }

[<Simulation>]
let ``Awaiting batch activates asap after construction if parent batch is empty (and therefore succeeded)`` () =
    simulation {
        let! now = Ecosystem.now
        let! parentBatchId = genBatchId
        let! parentBatch =
            BatchConstructor.NewProper (parentBatchId, { Description = None; JobsToConstruct = BatchJobsToConstruct.Parallel []; Parent = None; SentOn = now })
            |> Ecosystem.construct batchLifeCycle
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenAssert (fun b ->
                match b.Status with | BatchStatus.Finished (* success *) true -> true | _ -> false)

        do!
            ``Create awaiting parallel batch`` parentBatch.Id None
            |> Ecosystem.simulate
            |> Ecosystem.thenAssertEventually batchLifeCycle (fun b -> match b.ProperBody.ActivationStatus with BatchActivationStatus.Activated _ -> true | _ -> false)
            |> Ecosystem.thenIgnore
    }

[<Simulation>]
let ``Awaiting batch jobs deleted in cascade if parent batch completed but not fully succeeded`` () =
    simulation {
        let! parentBatch = ``Create activated parallel batch`` ()
        let! childBatch = ``Create awaiting parallel batch`` parentBatch.Id None

        let parentJobIds = parentBatch.ProperBody.JobsProgress.Keys |> List.ofSeq
        let firstParentJobId = parentJobIds.Head
        let otherParentJobIds = parentJobIds.Tail

        do! firstParentJobId
            |> Ecosystem.actOnId jobLifeCycle JobAction.Delete
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenIgnore

        use! __ =
            Ecosystem.useConnector jobRunnerConnector (function
                JobRunnerRequest.RunJob (data, responseChannel) ->
                    responseChannel.RespondNext (data.Ticket, { Result = Ok (); TotalDuration = TimeSpan.Zero; PureDuration = None } |> ProcessingJobUpdate.Completed)
                    responseChannel.Complete() |> Some)

        do!
            otherParentJobIds
            |> Seq.map (fun jobId ->
                jobId
                |> Ecosystem.actOnId jobLifeCycle JobAction.Enqueue
                |> Ecosystem.thenAssertOk
                |> Ecosystem.thenAct jobLifeCycle (JobAction.Start dummyDispatcherId)
                |> Ecosystem.thenAssertOk)
            |> Ecosystem.runInParallel
            |> Ecosystem.thenIgnore

        do! Ecosystem.awaitSystemStasis ()

        do! childBatch.Id
            |> Ecosystem.get batchLifeCycle
            |> Ecosystem.thenAssertSome
            |> Ecosystem.thenAssert (fun b ->
                match b.Status with BatchStatus.Finished (* succeeded *) false -> true | _ -> false)
            |> Ecosystem.thenIgnore
    }

[<Simulation>]
let ``Awaiting batch that awaits for any Finished status initializes if parent batch jobs deleted`` () =
    simulation {
        let! parentBatch = ``Create activated parallel batch`` ()
        let! childBatch = ``Create awaiting parallel batch`` parentBatch.Id (Some AwaitingForBatchStatus.AnyFinishedState)

        let parentJobIds = parentBatch.ProperBody.JobsProgress.Keys
        do!
            parentJobIds
            |> Seq.map (fun jobId ->
                jobId
                |> Ecosystem.actOnId jobLifeCycle JobAction.Delete
                |> Ecosystem.thenAssertOk)
            |> Ecosystem.runInParallel
            |> Ecosystem.thenIgnore

        do! Ecosystem.awaitSystemStasis ()

        do! childBatch.Id
            |> Ecosystem.get batchLifeCycle
            |> Ecosystem.thenAssertSome
            |> Ecosystem.thenAssert (fun b ->
                match b.Status with | BatchStatus.Unfinished -> true | _ -> false)
            |> Ecosystem.thenIgnore
    }

[<Simulation>]
let ``Batch with job placeholders constructed before proper jobs, creates placeholder jobs`` () =
    simulation {
        let! batchId = genBatchId
        let! job1Id = genJobId
        let! job2Id = genJobId
        let batchJobsToConstruct = BatchJobsToConstruct.Placeholders [job1Id; job2Id]

        let! now = Ecosystem.now
        let! batch =
            BatchConstructor.NewProper (batchId, { Description = None; JobsToConstruct = batchJobsToConstruct; Parent = None; SentOn = now })
            |> Ecosystem.construct batchLifeCycle
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenAssertEventually batchLifeCycle (fun b ->
                match b.ProperBody.ActivationStatus with
                | BatchActivationStatus.Activated _ ->
                    b.ProperBody.JobsProgress.Values |> Seq.forall (function JobProgress.Unfinished _ -> true | JobProgress.Finished _ -> false)
                | BatchActivationStatus.Awaiting _ -> false)

        do! Ecosystem.get jobLifeCycle job1Id
            |> Ecosystem.thenAssertSome
            |> Ecosystem.thenAssert (fun j -> match j.Body with JobBodyVariant.Placeholder _ -> true | _ -> false)
            |> Ecosystem.thenIgnore

        do! Ecosystem.get jobLifeCycle job2Id
            |> Ecosystem.thenAssertSome
            |> Ecosystem.thenAssert (fun j -> match j.Body with JobBodyVariant.Placeholder _ -> true | _ -> false)
            |> Ecosystem.thenIgnore

        return batch, job1Id, job2Id
    }

[<Simulation>]
let ``Batch with job placeholders, jobs filled and started and run successfully, batch ends in successfully Finished status`` () =
    simulation {
        let! batch, job1Id, job2Id = ``Batch with job placeholders constructed before proper jobs, creates placeholder jobs`` ()

        let! job1Common = genTypicalJobConstructorCommonData
        do! job1Id
            |> Ecosystem.actOnId jobLifeCycle (JobAction.FillPlaceholder (ProperJobConstructor.Enqueued (job1Common, JobScope.Batch batch.Id)))
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenIgnore

        let! job2Common = genTypicalJobConstructorCommonData
        do! job2Id
            |> Ecosystem.actOnId jobLifeCycle (JobAction.FillPlaceholder (ProperJobConstructor.Enqueued (job2Common, JobScope.Batch batch.Id)))
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenIgnore

        do! ``Start parallel batch jobs and run successfully`` batch

        let! batch =
            batch.Id
            |> Ecosystem.get batchLifeCycle
            |> Ecosystem.thenAssertSome
            |> Ecosystem.thenAssert (fun b ->
                match b.Status with | BatchStatus.Finished (* success *) true -> true | _ -> false)

        return batch
    }

[<Simulation>]
let ``Batch with job placeholders constructed after proper jobs Succeeded, ends in successfully Finished status`` () =
    simulation {
        let! job1 = ``Job Tests``.``Enqueued job started, code runs, ends in Succeeded state``()
        let! job2 = ``Job Tests``.``Enqueued job started, code runs, ends in Succeeded state``()

        let batchJobsToConstruct = BatchJobsToConstruct.Placeholders [job1.Id; job2.Id]
        let! now = Ecosystem.now
        let! batchId = genBatchId

        do! BatchConstructor.NewProper (batchId, { Description = None; JobsToConstruct = batchJobsToConstruct; Parent = None; SentOn = now })
            |> Ecosystem.construct batchLifeCycle
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenAssertEventually batchLifeCycle (fun b ->
                match b.Status with | BatchStatus.Finished (* success *) true -> true | _ -> false)
            |> Ecosystem.thenIgnore
    }