module Jobs.``Dispatcher Tests``

open System
open SuiteJobs.Types
open SuiteJobs.LifeCycles
open SuiteJobs.LifeCycles.Connectors

[<Simulation>]
let ``Dispatcher processes jobs only from nominated queues`` () =
    simulation {
        let queueName = NonemptyString.ofStringUnsafe "Queue-A"
        let otherQueueName = NonemptyString.ofStringUnsafe "Queue-B"

        let settings: DispatcherSettings =
            { Queues = NonemptySet.ofOneItem queueName
              MaxProcessingJobs = UnsignedInteger.One
              HotPollIfJobsCountAtOrBelow = UnsignedInteger.Zero
              HotPollIfTimeSinceLastPollAtLeast = TimeSpan.FromSeconds 10.
              MaxContinuousPollRetries = 0u }

        use! __ =
            Ecosystem.useConnector jobRunnerConnector (function
                JobRunnerRequest.RunJob (data, responseChannel) ->
                    responseChannel.RespondNext (data.Ticket, { Result = Ok (); TotalDuration = TimeSpan.Zero; PureDuration = None } |> ProcessingJobUpdate.Completed)
                    responseChannel.Complete() |> Some)

        let! jobA = ``Job Tests``.``Create enqueued job`` (Some (fun ctorData -> { ctorData with QueueName = queueName }))
        let! jobB = ``Job Tests``.``Create enqueued job`` (Some (fun ctorData -> { ctorData with QueueName = otherQueueName }))

        let! dispatcher =
            DispatcherConstructor.New(NonemptyString.ofStringUnsafe "Dispatcher-A", settings)
            |> Ecosystem.construct dispatcherLifeCycle
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenAwaitSystemStasis
            |> Ecosystem.thenGetLatest dispatcherLifeCycle

        do! jobA.Id
            |> Ecosystem.get jobLifeCycle
            |> Ecosystem.thenAssertSome
            |> Ecosystem.thenAssertEventually jobLifeCycle (fun j -> match j.ProperState with JobState.Succeeded _ -> true | _ -> false)
            |> Ecosystem.thenIgnore

        do! jobB.Id
            |> Ecosystem.get jobLifeCycle
            |> Ecosystem.thenAssertSome
            |> Ecosystem.thenAssertEventually jobLifeCycle (fun j -> match j.ProperState with JobState.Enqueued _ -> true | _ -> false)
            |> Ecosystem.thenIgnore

        return dispatcher, queueName
    }

[<Simulation>]
let ``Dispatcher keeps processing jobs until queue is empty`` () =
    simulation {
        let queueName = NonemptyString.ofStringUnsafe "Queue-A"

        let settings: DispatcherSettings =
            { Queues = NonemptySet.ofOneItem queueName
              MaxProcessingJobs = UnsignedInteger.One
              HotPollIfJobsCountAtOrBelow = UnsignedInteger.Zero
              HotPollIfTimeSinceLastPollAtLeast = TimeSpan.FromSeconds 10.
              MaxContinuousPollRetries = 0u }

        use! __ =
            Ecosystem.useConnector jobRunnerConnector (function
                JobRunnerRequest.RunJob (data, responseChannel) ->
                    responseChannel.RespondNext (data.Ticket, { Result = Ok (); TotalDuration = TimeSpan.Zero; PureDuration = None } |> ProcessingJobUpdate.Completed)
                    responseChannel.Complete() |> Some)

        let createEnqueuedJob () =
            ``Job Tests``.``Create enqueued job`` (Some (fun ctorData -> { ctorData with QueueName = queueName }))
            |> Ecosystem.simulate
        let! bunchOfJobsBefore =
            [1 .. 10] |> Seq.map (fun _ -> createEnqueuedJob ()) |> Ecosystem.runInParallel

        let! dispatcher =
            DispatcherConstructor.New(NonemptyString.ofStringUnsafe "Dispatcher-A", settings)
            |> Ecosystem.construct dispatcherLifeCycle
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenAwaitSystemStasis
            |> Ecosystem.thenGetLatest dispatcherLifeCycle

        let! bunchOfJobsAfter = Ecosystem.getMultiple jobLifeCycle (bunchOfJobsBefore |> Seq.map (fun j -> j.Id) |> Set.ofSeq)
        if bunchOfJobsAfter |> Seq.exists (fun j -> match j.ProperState with JobState.Succeeded _ -> false | _ -> true) then
            failwith "Expected all jobs to succeed"

        return dispatcher
    }

[<Simulation>]
let ``Dispatcher polls empty queue for more jobs with defined max sleep time`` () =
    simulation {
        let! _, queueName = ``Dispatcher processes jobs only from nominated queues``()
        // imitate long idle time
        do! Ecosystem.moveTimeForwardAndRunReminders (TimeSpan.FromMinutes 30.)

        use! __ =
            Ecosystem.useConnector jobRunnerConnector (function
                JobRunnerRequest.RunJob (data, responseChannel) ->
                    responseChannel.RespondNext (data.Ticket, { Result = Ok (); TotalDuration = TimeSpan.Zero; PureDuration = None } |> ProcessingJobUpdate.Completed)
                    responseChannel.Complete() |> Some)

        do!
            ``Job Tests``.``Create enqueued job`` (Some (fun ctorData -> { ctorData with QueueName = queueName }))
            |> Ecosystem.simulate
            |> Ecosystem.thenAssertEventually jobLifeCycle (fun j -> match j.ProperState with JobState.Enqueued _ -> true | _ -> false)
            |> Ecosystem.thenMoveTimeForwardAndRunReminders (TimeSpan.FromMinutes 1.) // at most one minute of waiting time
            |> Ecosystem.thenAssertEventually jobLifeCycle (fun j -> match j.ProperState with JobState.Succeeded _ -> true | _ -> false)
            |> Ecosystem.thenIgnore
    }

[<Simulation>]
let ``Dispatcher can be hard deleted`` () =
    simulation {
        let! dispatcher = ``Dispatcher keeps processing jobs until queue is empty`` ()
        do!
            dispatcher
            |> Ecosystem.act dispatcherLifeCycle DispatcherAction.HardDelete
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenMap (fun d -> d.Id)
            |> Ecosystem.thenAssertEventuallyDeleted dispatcherLifeCycle
            |> Ecosystem.thenIgnore
    }