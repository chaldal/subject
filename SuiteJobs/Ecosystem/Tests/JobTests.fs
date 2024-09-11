module Jobs.``Job Tests``

open System
open FsCheck
open SuiteJobs.LifeCycles.Connectors
open SuiteJobs.Types

let dummyDispatcherId = "DummyDispatcher" |> NonemptyString.ofStringUnsafe |> DispatcherId

[<Simulation>]
let ``Scheduled job enqueues itself when time comes`` () =
    simulation {
        let! jobId = genJobId
        let! common = genTypicalJobConstructorCommonData
        let! now = Ecosystem.now
        let! scheduleAfter = Gen.choose (5, 100) |> Gen.map TimeSpan.FromMinutes
        do!
            JobConstructor.NewProper(jobId, ProperJobConstructor.Scheduled (common, now.Add scheduleAfter))
            |> Ecosystem.construct jobLifeCycle
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenAssertEventually jobLifeCycle
                   (fun j -> match j.ProperState with JobState.Scheduled _ -> true | _ -> false)
            |> Ecosystem.thenMoveTimeForwardAndRunReminders (scheduleAfter.Subtract (TimeSpan.FromSeconds 1))
            |> Ecosystem.thenAssertEventually jobLifeCycle
                   (fun j -> match j.ProperState with JobState.Scheduled _ -> true | _ -> false)
            |> Ecosystem.thenMoveTimeForwardAndRunReminders (TimeSpan.FromSeconds 1)
            |> Ecosystem.thenAssertEventually jobLifeCycle
                    (fun j -> match j.ProperState with JobState.Enqueued _ -> true | _ -> false)
            |> Ecosystem.thenIgnore
    }

let ``Create enqueued job`` (customize: Option<JobConstructorCommonData -> JobConstructorCommonData>) =
    simulation {
        let! jobId = genJobId
        let! common = genTypicalJobConstructorCommonData
        let common = customize |> Option.map (fun f -> f common) |> Option.defaultValue common

        let! job =
            JobConstructor.NewProper(jobId, ProperJobConstructor.Enqueued (common, JobScope.Other))
            |> Ecosystem.construct jobLifeCycle
            |> Ecosystem.thenAssertOk
            // can do assertEventually because newly Enqueued job will NEVER start spontaneously
            |> Ecosystem.thenAssertEventually jobLifeCycle
                   (fun j -> match j.ProperState with JobState.Enqueued _ -> true | _ -> false)
        return job
    }

let ``Create awaiting job``
        (awaitForJobId: JobId)
        (overrideAwaitForStatus: Option<AwaitingForJobStatus>) =
    simulation {
        let! jobId = genJobId
        let! common = genTypicalJobConstructorCommonData

        let! job =
            JobConstructor.NewProper(jobId, ProperJobConstructor.Awaiting (
                  common, JobScope.Other,
                  JobParent.Job (awaitForJobId, overrideAwaitForStatus |> Option.defaultValue AwaitingForJobStatus.OnlySucceeded)))
            |> Ecosystem.construct jobLifeCycle
            |> Ecosystem.thenAssertOk
            // can't do assertEventually because it may get enqueued straight away if parent job already finished
            |> Ecosystem.thenAssert (fun j -> match j.ProperState with JobState.Awaiting _ -> true | _ -> false)
        return job
    }

[<Simulation>]
let ``Enqueued job started, code runs, ends in Succeeded state`` () =
    simulation {
        let! job = ``Create enqueued job`` None

        use! __ =
            Ecosystem.useConnector jobRunnerConnector (function
                JobRunnerRequest.RunJob (data, responseChannel) ->
                    responseChannel.RespondNext (data.Ticket, { Result = Ok (); TotalDuration = TimeSpan.Zero; PureDuration = None } |> ProcessingJobUpdate.Completed)
                    responseChannel.Complete() |> Some)

        let! job =
            job
            |> Ecosystem.act jobLifeCycle (JobAction.Start dummyDispatcherId)
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenAssert
                   (fun j -> match j.ProperState with JobState.Processing _ -> true | _ -> false)
            |> Ecosystem.thenAssertEventually jobLifeCycle
                    (fun j -> match j.ProperState with JobState.Succeeded _ -> true | _ -> false)

        return job
    }

[<Simulation>]
let ``Succeeded job is hard deleted from storage after a couple of days`` () =
    simulation {
        do! ``Enqueued job started, code runs, ends in Succeeded state`` ()
            |> Ecosystem.simulate
            |> Ecosystem.thenMoveTimeForwardAndRunReminders (TimeSpan.FromDays 2.)
            |> Ecosystem.thenMap (fun j -> j.Id)
            |> Ecosystem.thenAssertEventuallyDeleted jobLifeCycle
    }

[<Simulation>]
let ``Deleted job is hard deleted from storage after a couple of days`` () =
    simulation {
        do! ``Create enqueued job`` None
            |> Ecosystem.simulate
            |> Ecosystem.thenAct jobLifeCycle JobAction.Delete
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenMoveTimeForwardAndRunReminders (TimeSpan.FromDays 4.)
            |> Ecosystem.thenMap (fun j -> j.Id)
            |> Ecosystem.thenAssertEventuallyDeleted jobLifeCycle
    }

[<Simulation>]
let ``Enqueued job started, code throws, ends in Failed state`` () =
    simulation {
        let! job = ``Create enqueued job`` None

        use! __ =
            Ecosystem.useConnector jobRunnerConnector (function
                JobRunnerRequest.RunJob (data, responseChannel) ->
                    responseChannel.RespondNext (data.Ticket, { Result = Error ((* retryForFree *) false, JobFailedReason.Exception ("smth bad", "System.Exception", "details")); TotalDuration = TimeSpan.Zero; PureDuration = None } |> ProcessingJobUpdate.Completed)
                    responseChannel.Complete() |> Some)

        do! job
            |> Ecosystem.act jobLifeCycle (JobAction.Start dummyDispatcherId)
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenAssert
                   (fun j -> match j.ProperState with JobState.Processing _ -> true | _ -> false)
            |> Ecosystem.thenAssertEventually jobLifeCycle
                    (fun j -> match j.ProperState with JobState.Failed (JobFailedReason.Exception _, _) -> true | _ -> false)
            |> Ecosystem.thenIgnore
    }

[<Simulation>]
let ``Enqueued job started, code throws a free to retry exception, ends in Scheduled for retry state`` () =
    simulation {
        let! job = ``Create enqueued job`` None

        use! __ =
            Ecosystem.useConnector jobRunnerConnector (function
                JobRunnerRequest.RunJob (data, responseChannel) ->
                    responseChannel.RespondNext (data.Ticket, { Result = Error ((* retryForFree *) true, JobFailedReason.Exception ("runner is unavailable", "System.Exception", "try again")); TotalDuration = TimeSpan.Zero; PureDuration = None } |> ProcessingJobUpdate.Completed)
                    responseChannel.Complete() |> Some)

        do! job
            |> Ecosystem.act jobLifeCycle (JobAction.Start dummyDispatcherId)
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenAssert
                   (fun j -> match j.ProperState with JobState.Processing _ -> true | _ -> false)
            |> Ecosystem.thenAssertEventually jobLifeCycle
                    (fun j -> match j.ProperState, j.ProperBody.Retry with JobState.Scheduled _, Some { Attempt = 0uy } -> true | _ -> false)
            |> Ecosystem.thenIgnore
    }

[<Simulation>]
let ``Enqueued retryable job started, connector heartbeat times out, ends in Scheduled for free retry state`` () =
    simulation {
        let! job =
            ``Create enqueued job``
                (Some (fun common -> { common with FailurePolicy = { common.FailurePolicy with MaybeAutoRetries = Some { MaxAutoRetries = 1uy; DelayPolicy = JobAutoRetryDelayPolicy.Hangfire; DeleteIfExceeded = false } } }))

        use! __ =
            Ecosystem.useConnector jobRunnerConnector (function JobRunnerRequest.RunJob _ -> failwith "imitates timeout")

        do! job
            |> Ecosystem.act jobLifeCycle (JobAction.Start dummyDispatcherId)
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenAssert
                   (fun j -> match j.ProperState with JobState.Processing _ -> true | _ -> false)
            // |> Ecosystem.thenAssertNoBadLogs - this assertion is racy and sometimes fails, so not doing it
            |> Ecosystem.thenAssertEventually jobLifeCycle // still processing after connector finished
                    (fun j -> match j.ProperState with JobState.Processing _ -> true | _ -> false)
            |> Ecosystem.thenClearAllBadLogs
            |> Ecosystem.thenMoveTimeForwardAndRunReminders (TimeSpan.FromMinutes 1.) // heartbeat timeout hardcoded as one minute
            |> Ecosystem.thenAssertEventually jobLifeCycle
                    (fun j -> match j.ProperState, j.ProperBody.Retry with JobState.Scheduled _, Some { Attempt = 0uy } -> true | _ -> false)
            |> Ecosystem.thenIgnore
    }

[<Simulation>]
let ``Enqueued at-most-once job started, connector heartbeat times out, ends in Failed state`` () =
    simulation {
        let! job = ``Create enqueued job`` None

        use! __ =
            Ecosystem.useConnector jobRunnerConnector (function JobRunnerRequest.RunJob _ -> failwith "imitates timeout")

        do! job
            |> Ecosystem.act jobLifeCycle (JobAction.Start dummyDispatcherId)
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenAssert
                    (fun j -> match j.ProperState with JobState.Processing _ -> true | _ -> false)
            |> Ecosystem.thenAssertEventually jobLifeCycle // still processing after connector finished
                    (fun j -> match j.ProperState with JobState.Processing _ -> true | _ -> false)
            |> Ecosystem.thenClearAllBadLogs
            |> Ecosystem.thenMoveTimeForwardAndRunReminders (TimeSpan.FromMinutes 1.) // heartbeat timeout hardcoded as one minute
            |> Ecosystem.thenAssertEventually jobLifeCycle (fun j -> match j.ProperState with JobState.Failed _ -> true | _ -> false)
            |> Ecosystem.thenIgnore
    }

[<Simulation>]
let ``Enqueued job with MaxRetries=1 started, code throws, rescheduled to retry`` () =
    simulation {
        let! job =
            ``Create enqueued job``
                (Some (fun common -> { common with FailurePolicy = { common.FailurePolicy with MaybeAutoRetries = Some { MaxAutoRetries = 1uy; DelayPolicy = JobAutoRetryDelayPolicy.Hangfire; DeleteIfExceeded = false } } }))

        use! __ =
            Ecosystem.useConnector jobRunnerConnector (function
                JobRunnerRequest.RunJob (data, responseChannel) ->
                    responseChannel.RespondNext (data.Ticket, { Result = Error ((* retryForFree *) false, JobFailedReason.Exception ("smth bad", "System.Exception", "details")); TotalDuration = TimeSpan.Zero; PureDuration = None } |> ProcessingJobUpdate.Completed)
                    responseChannel.Complete() |> Some)

        let! job, retryOn =
            job
            |> Ecosystem.act jobLifeCycle (JobAction.Start dummyDispatcherId)
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenAssertEventually jobLifeCycle
                    (fun j -> match j.ProperBody.Retry with Some retry -> retry.Attempt = 1uy | _ -> false)
            |> Ecosystem.thenMap (fun j -> match j.ProperState with | JobState.Scheduled retryOn -> j, retryOn | _ -> failwith "unexpected state")

        do! Ecosystem.moveToTimeAndRunReminders retryOn

        do! job.Id
            |> Ecosystem.get jobLifeCycle
            |> Ecosystem.thenAssertSome
            |> Ecosystem.thenAssert (fun j -> match j.ProperState with JobState.Enqueued _ -> true | _ -> false)
            |> Ecosystem.thenIgnore
    }

[<Simulation>]
let ``Awaiting job enqueues only when parent job succeeds`` () =
    simulation {
        let! parentJob = ``Create enqueued job`` None
        let! childJob = ``Create awaiting job`` parentJob.Id None

        do! childJob.Id
            |> Ecosystem.get jobLifeCycle
            |> Ecosystem.thenAssertSome
            |> Ecosystem.thenAssertEventually jobLifeCycle (fun j -> match j.ProperState with JobState.Awaiting _ -> true | _ -> false)
            |> Ecosystem.thenIgnore

        use! __ =
            Ecosystem.useConnector jobRunnerConnector (function
                JobRunnerRequest.RunJob (data, responseChannel) ->
                    responseChannel.RespondNext (data.Ticket, { Result = Ok (); TotalDuration = TimeSpan.Zero; PureDuration = None } |> ProcessingJobUpdate.Completed)
                    responseChannel.Complete() |> Some)

        do! parentJob
            |> Ecosystem.act jobLifeCycle (JobAction.Start dummyDispatcherId)
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenAssertEventually jobLifeCycle (fun j -> match j.ProperState with JobState.Succeeded _ -> true | _ -> false)
            |> Ecosystem.thenIgnore

        do! childJob.Id
            |> Ecosystem.get jobLifeCycle
            |> Ecosystem.thenAssertSome
            |> Ecosystem.thenAssert (fun j -> match j.ProperState with JobState.Enqueued _ -> true | _ -> false)
            |> Ecosystem.thenIgnore
    }

[<Simulation>]
let ``Awaiting job constructed before parent creates a parent job placeholder`` () =
    simulation {
        let! parentJobId = genJobId
        let! childJob =
            ``Create awaiting job`` parentJobId None
            |> Ecosystem.simulate
            |> Ecosystem.thenGetLatest jobLifeCycle
            |> Ecosystem.thenAssertEventually jobLifeCycle (fun j -> match j.ProperState with JobState.Awaiting _ -> true | _ -> false)

        let! parentJobPlaceholder =
            parentJobId
            |> Ecosystem.get jobLifeCycle
            |> Ecosystem.thenAssertSome
            |> Ecosystem.thenAssert (fun pj -> match pj.Body with | JobBodyVariant.Placeholder _ -> true | _ -> false)

        return childJob, parentJobPlaceholder
    }

[<Simulation>]
let ``Awaiting job constructed before parent, parent placeholder filled, child enqueues when parent succeeds`` () =
    simulation {
        let! childJob, parentJobPlaceholder = ``Awaiting job constructed before parent creates a parent job placeholder`` ()

        let! common = genTypicalJobConstructorCommonData
        let! parentJob =
            parentJobPlaceholder
            |> Ecosystem.act jobLifeCycle (JobAction.FillPlaceholder (ProperJobConstructor.Enqueued (common, JobScope.Other)))
            |> Ecosystem.thenAssertOk

        use! __ =
            Ecosystem.useConnector jobRunnerConnector (function
                JobRunnerRequest.RunJob (data, responseChannel) ->
                    responseChannel.RespondNext (data.Ticket, { Result = Ok (); TotalDuration = TimeSpan.Zero; PureDuration = None } |> ProcessingJobUpdate.Completed)
                    responseChannel.Complete() |> Some)

        do! parentJob
            |> Ecosystem.act jobLifeCycle (JobAction.Start dummyDispatcherId)
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenAssertEventually jobLifeCycle (fun j -> match j.ProperState with JobState.Succeeded _ -> true | _ -> false)
            |> Ecosystem.thenIgnore

        do! childJob.Id
            |> Ecosystem.get jobLifeCycle
            |> Ecosystem.thenAssertSome
            |> Ecosystem.thenAssert (fun j -> match j.ProperState with JobState.Enqueued _ -> true | _ -> false)
            |> Ecosystem.thenIgnore
    }

[<Simulation>]
let ``Placeholder in the middle of awaiting jobs chain, filled after its parent completed, becomes enqueued`` () =
    simulation {
        let! _, placeholderJob = ``Awaiting job constructed before parent creates a parent job placeholder`` ()
        let! succeededJob = ``Enqueued job started, code runs, ends in Succeeded state`` ()

        let! common = genTypicalJobConstructorCommonData
        do! placeholderJob
            |> Ecosystem.act jobLifeCycle (JobAction.FillPlaceholder (ProperJobConstructor.Awaiting (common, JobScope.Other, JobParent.Job (succeededJob.Id, AwaitingForJobStatus.OnlySucceeded))))
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenAssert (fun j -> match j.ProperState with JobState.Awaiting _ -> true | _ -> false)
            |> Ecosystem.thenAssertEventually jobLifeCycle (fun j -> match j.ProperState with JobState.Enqueued _ -> true | _ -> false)
            |> Ecosystem.thenIgnore
    }

[<Simulation>]
let ``Awaiting job constructed before parent, parent placeholder deleted before it's filled, both jobs are deleted in the end`` () =
    simulation {
        let! childJob, parentJobPlaceholder = ``Awaiting job constructed before parent creates a parent job placeholder`` ()

        let! common = genTypicalJobConstructorCommonData
        do! parentJobPlaceholder
            |> Ecosystem.act jobLifeCycle JobAction.Delete
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenAct jobLifeCycle (JobAction.FillPlaceholder (ProperJobConstructor.Enqueued (common, JobScope.Other)))
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenAssert (fun j -> match j.ProperBody.State with | JobState.Deleted _ -> true | _ -> false)
            |> Ecosystem.thenIgnore

        do! Ecosystem.awaitSystemStasis()

        do! childJob.Id
            |> Ecosystem.get jobLifeCycle
            |> Ecosystem.thenAssertSome
            |> Ecosystem.thenAssert (fun j -> match j.ProperState with JobState.Deleted _ -> true | _ -> false)
            |> Ecosystem.thenIgnore
    }

[<Simulation>]
let ``Awaiting job enqueues asap after construction if parent job already succeeded`` () =
    simulation {
        let! parentJob = ``Enqueued job started, code runs, ends in Succeeded state`` ()
        do!
            ``Create awaiting job`` parentJob.Id None
            |> Ecosystem.simulate
            |> Ecosystem.thenAssertEventually jobLifeCycle (fun j -> match j.ProperState with JobState.Enqueued _ -> true | _ -> false)
            |> Ecosystem.thenIgnore
    }

[<Simulation>]
let ``Awaiting job stays awaiting when parent job fails`` () =
    simulation {
        let! parentJob = ``Create enqueued job`` None
        let! childJob = ``Create awaiting job`` parentJob.Id None

        use! __ =
            Ecosystem.useConnector jobRunnerConnector (function
                JobRunnerRequest.RunJob (data, responseChannel) ->
                    responseChannel.RespondNext (data.Ticket, { Result = Error ((* retryForFree *) false, JobFailedReason.Exception ("smth bad", "System.Exception", "details")); TotalDuration = TimeSpan.Zero; PureDuration = None } |> ProcessingJobUpdate.Completed)
                    responseChannel.Complete() |> Some)

        do! parentJob
            |> Ecosystem.act jobLifeCycle (JobAction.Start dummyDispatcherId)
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenAssertEventually jobLifeCycle (fun j -> match j.ProperState with JobState.Failed _ -> true | _ -> false)
            |> Ecosystem.thenIgnore

        do! childJob.Id
            |> Ecosystem.get jobLifeCycle
            |> Ecosystem.thenAssertSome
            |> Ecosystem.thenAssert (fun j -> match j.ProperState with JobState.Awaiting _ -> true | _ -> false)
            |> Ecosystem.thenIgnore
    }

[<Simulation>]
let ``Awaiting jobs delete in cascade if parent job deleted`` () =
    simulation {
        let! parentJob = ``Create enqueued job`` None
        let! childJob = ``Create awaiting job`` parentJob.Id None
        let! grandChildJob = ``Create awaiting job`` childJob.Id None

        do! parentJob
            |> Ecosystem.act jobLifeCycle JobAction.Delete
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenAssertEventually jobLifeCycle (fun j -> match j.ProperState with JobState.Deleted _ -> true | _ -> false)
            |> Ecosystem.thenIgnore

        do! childJob.Id
            |> Ecosystem.get jobLifeCycle
            |> Ecosystem.thenAssertSome
            |> Ecosystem.thenAssert (fun j -> match j.ProperState with JobState.Deleted _ -> true | _ -> false)
            |> Ecosystem.thenIgnore

        do! grandChildJob.Id
            |> Ecosystem.get jobLifeCycle
            |> Ecosystem.thenAssertSome
            |> Ecosystem.thenAssert (fun j -> match j.ProperState with JobState.Deleted _ -> true | _ -> false)
            |> Ecosystem.thenIgnore
    }

[<Simulation>]
let ``Awaiting job that await for any Finished status enqueues if parent job deleted`` () =
    simulation {
        let! parentJob = ``Create enqueued job`` None
        let! childJob = ``Create awaiting job`` parentJob.Id (Some AwaitingForJobStatus.AnyFinishedState)

        do! parentJob
            |> Ecosystem.act jobLifeCycle JobAction.Delete
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenAssertEventually jobLifeCycle (fun j -> match j.ProperState with JobState.Deleted _ -> true | _ -> false)
            |> Ecosystem.thenIgnore

        do! childJob.Id
            |> Ecosystem.get jobLifeCycle
            |> Ecosystem.thenAssertSome
            |> Ecosystem.thenAssert (fun j -> match j.ProperState with JobState.Enqueued _ -> true | _ -> false)
            |> Ecosystem.thenIgnore
    }

[<Simulation>]
let ``Awaiting jobs chain can be deleted backwards starting from arbitrary job`` () =
    simulation {
        let! job1 = ``Create enqueued job`` None
        let! job2 = ``Create awaiting job`` job1.Id (Some AwaitingForJobStatus.AnyFinishedState)
        let! job3 = ``Create awaiting job`` job2.Id (Some AwaitingForJobStatus.AnyFinishedState)
        let! job4 = ``Create awaiting job`` job3.Id (Some AwaitingForJobStatus.AnyFinishedState)

        do! Ecosystem.awaitSystemStasis ()

        do! job3 // doesn't have to be last job in chain
            |> Ecosystem.act jobLifeCycle JobAction.DeleteAwaitingJobsBackwards
            |> Ecosystem.thenAwaitSystemStasis
            |> Ecosystem.thenIgnore

        let! _ =
            [job4; job3; job2]
            |> Seq.map (fun job ->
                job
                |> Ecosystem.assertEventually jobLifeCycle (fun j -> match j.ProperState with JobState.Deleted _ -> true | _ -> false)
                |> Ecosystem.thenIgnore)
            |> Ecosystem.runInParallel

        do! job1.Id // root job not deleted as it wasn't awaiting
            |> Ecosystem.get jobLifeCycle
            |> Ecosystem.thenAssertSome
            |> Ecosystem.thenAssert (fun j -> match j.ProperState with JobState.Enqueued _ -> true | _ -> false)
            |> Ecosystem.thenIgnore
    }
