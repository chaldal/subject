module Jobs.``RecurringJob Tests``

open System
open SuiteJobs.Types
open SuiteJobs.LifeCycles
open SuiteJobs.LifeCycles.Connectors

let dummyDispatcherId = "DummyDispatcher" |> NonemptyString.ofStringUnsafe |> DispatcherId

[<Simulation>]
let ``Recurring job scheduled by cron expression and enqueues a new job at specified intervals``() =
    simulation {
        use! __ =
            Ecosystem.useConnector jobRunnerConnector (function
                JobRunnerRequest.RunJob (data, responseChannel) ->
                    responseChannel.RespondNext (data.Ticket, { Result = Ok (); TotalDuration = TimeSpan.Zero; PureDuration = None } |> ProcessingJobUpdate.Completed)
                    responseChannel.Complete() |> Some)

        let! jobData = genTypicalJobConstructorCommonData
        let! simulationStartedOn = Ecosystem.now
        let interval = TimeSpan.FromMinutes 5

        let! recurringJob =
             Ecosystem.construct recurringJobLifeCycle
                (RecurringJobConstructor.New (
                    NonemptyString.ofStringUnsafe "Test",
                    "*/5 * * * *",
                    "Bangladesh Standard Time",
                    jobData))
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenAssertEventually recurringJobLifeCycle (fun rj ->
                match rj.State with
                | RecurringJobState.Scheduled (dueOn, _) ->
                   let dueIn = dueOn - simulationStartedOn
                   dueIn >= TimeSpan.Zero && dueIn <= interval
                | _ -> false)
            |> Ecosystem.thenMoveTimeForwardAndRunReminders interval
            |> Ecosystem.thenGetLatest recurringJobLifeCycle

        let firstJobId =
            match recurringJob.State with
            | RecurringJobState.Fired (_, execution) -> execution.Id | _ -> failwith "unexpected"

        do! Ecosystem.actOnId jobLifeCycle (JobAction.Start dummyDispatcherId) firstJobId
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenIgnore

        let! recurringJob =
            recurringJob
            |> Ecosystem.assertEventually recurringJobLifeCycle (fun rj ->
                match rj.State with
                | RecurringJobState.Scheduled (dueOn, _) ->
                   let dueIn = dueOn - simulationStartedOn
                   dueIn >= interval && dueIn <= interval * 2.
                | _ -> false)
            |> Ecosystem.thenMoveTimeForwardAndRunReminders interval
            |> Ecosystem.thenGetLatest recurringJobLifeCycle

        let secondJobId =
            match recurringJob.State with
            | RecurringJobState.Fired (_, execution) -> execution.Id | _ -> failwith "unexpected"

        do! Ecosystem.actOnId jobLifeCycle (JobAction.Start dummyDispatcherId) secondJobId
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenIgnore

        let! recurringJob =
            recurringJob
            |> Ecosystem.assertEventually recurringJobLifeCycle (fun rj ->
                match rj.State with
                | RecurringJobState.Scheduled (dueOn, _) ->
                   let dueIn = dueOn - simulationStartedOn
                   dueIn >= interval * 2. && dueIn <= interval * 3.
                | _ -> false)
        return recurringJob
    }

[<Simulation>]
let ``Recurring job fired much later because system was offline, ignore missed past intervals``() =
    simulation {
        let! jobData = genTypicalJobConstructorCommonData
        let! simulationStartedOn = Ecosystem.now
        let interval = TimeSpan.FromMinutes 5

        let! recurringJob =
             Ecosystem.construct recurringJobLifeCycle
                (RecurringJobConstructor.New (
                    NonemptyString.ofStringUnsafe "Test",
                    "*/5 * * * *",
                    "Bangladesh Standard Time",
                    jobData))
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenAssertEventually recurringJobLifeCycle (fun rj ->
                match rj.State with
                | RecurringJobState.Scheduled (dueOn, _) ->
                   let dueIn = dueOn - simulationStartedOn
                   dueIn >= TimeSpan.Zero && dueIn <= interval
                | _ -> false)
            // Go Offline and wake up after a week (typical for a dev host)
            |> Ecosystem.thenMoveTimeForwardAndRunReminders (TimeSpan.FromDays 7.)
            |> Ecosystem.thenGetLatest recurringJobLifeCycle

        let! simulatedOnlineAgainOn = Ecosystem.now

        let firstJobId =
            match recurringJob.State with
            | RecurringJobState.Fired (_, execution) -> execution.Id | _ -> failwith "unexpected"

        do! Ecosystem.actOnId jobLifeCycle JobAction.Delete firstJobId
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenIgnore

        do! recurringJob
            |> Ecosystem.assertEventually recurringJobLifeCycle (fun rj ->
                match rj.State with
                | RecurringJobState.Scheduled (dueOn, _) ->
                   let dueIn = dueOn - simulatedOnlineAgainOn
                   dueIn >= TimeSpan.Zero && dueIn <= interval
                | _ -> false)
            |> Ecosystem.thenIgnore
    }

[<Simulation>]
let ``Recurring job fired but job not started, it times out eventually and schedules next job instance``() =
    simulation {
        let! jobData = genTypicalJobConstructorCommonData
        let! simulationStartedOn = Ecosystem.now
        let interval = TimeSpan.FromMinutes 7

        let! recurringJob =
             Ecosystem.construct recurringJobLifeCycle
                (RecurringJobConstructor.New (
                    NonemptyString.ofStringUnsafe "Test",
                    "*/7 * * * *",
                    "Bangladesh Standard Time",
                    jobData))
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenAssertEventually recurringJobLifeCycle (fun rj ->
                match rj.State with
                | RecurringJobState.Scheduled (dueOn, _) ->
                   let dueIn = dueOn - simulationStartedOn
                   dueIn >= TimeSpan.Zero && dueIn <= interval
                | _ -> false)
            |> Ecosystem.thenMoveTimeForwardAndRunReminders interval
            |> Ecosystem.thenGetLatest recurringJobLifeCycle

        let nextDueOn =
            match recurringJob.State with
            | RecurringJobState.Fired (nextDueOn, _) -> nextDueOn | _ -> failwith "unexpected"

        do! Ecosystem.moveToTimeAndRunReminders nextDueOn

        do!
            recurringJob
            |> Ecosystem.assertEventually recurringJobLifeCycle (fun rj ->
                match rj.State with // still fired, giving a chance to fired job to finish
                | RecurringJobState.Fired (nextDueOn', _) ->
                    nextDueOn' = nextDueOn
                | _ -> false)
            |> Ecosystem.thenMoveTimeForwardAndRunReminders (TimeSpan.FromMinutes 4.)
            |> Ecosystem.thenAssertEventually recurringJobLifeCycle (fun rj ->
                match rj.State with // fired and rescheduled to next interval
                | RecurringJobState.Fired (nextDueOn'', _) ->
                    nextDueOn'' > nextDueOn && nextDueOn'' <= nextDueOn + interval
                | _ -> false)
            |> Ecosystem.thenIgnore
    }

[<Simulation>]
let ``Recurring job can be hard deleted`` () =
    simulation {
        let! recurringJob = ``Recurring job scheduled by cron expression and enqueues a new job at specified intervals`` ()
        do!
            recurringJob
            |> Ecosystem.act recurringJobLifeCycle RecurringJobAction.HardDelete
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenMap (fun d -> d.Id)
            |> Ecosystem.thenAssertEventuallyDeleted recurringJobLifeCycle
            |> Ecosystem.thenIgnore
    }