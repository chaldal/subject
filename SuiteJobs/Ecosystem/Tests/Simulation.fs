[<AutoOpen>]
module Simulation

[<assembly: LibLifeCycleTest.TestRunner.SimulationTestFramework>]
do ()

open System.Reflection
open LibLifeCycleTest

let private createdTestJobsEcosystem =
    SuiteJobs.LifeCycles.AllLifeCycles.createJobsEcosystem (fun _ _ -> failwith "RunJob callback must be intercepted")

let jobRunnerConnector = createdTestJobsEcosystem.JobRunnerConnector
let jobLifeCycle = createdTestJobsEcosystem.JobLifeCycle

let private simulation_without_initializer = SimulationBuilder(createdTestJobsEcosystem.Ecosystem, Assembly.GetExecutingAssembly())

let initializer =
    simulation_without_initializer {
        return ()
    }

let simulation =
    simulation_without_initializer.WithInitializer initializer

open SuiteJobs.Types

type Job with
    member this.ProperBody =
        match this.Body with
        | JobBodyVariant.Placeholder _ -> failwith "the job has no body"
        | JobBodyVariant.Proper body -> body
    member this.ProperState =
        this.ProperBody.State

type Batch with
    member this.ProperBody =
        match this.Body with
        | BatchBodyVariant.Placeholder _ -> failwith "the batch has no body"
        | BatchBodyVariant.Proper body -> body