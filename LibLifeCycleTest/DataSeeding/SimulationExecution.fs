[<AutoOpen>]
module internal LibLifeCycleTest.DataSeeding.SimulationExecution

open System
open System.Threading.Tasks
open LibLifeCycleCore
open LibLifeCycleHost.TelemetryModel
open LibLifeCycleTest
open Microsoft.Extensions.DependencyInjection

[<RequireQualifiedAccess>]
type SimulationExecutionResult =
| Failed of exn
| Succeeded of FinishedSimulatedOn: DateTimeOffset

let executeSimulation (simulation: Simulation) =
    backgroundTask {
        try
            do! testRunnerCluster.Init ignore
            let rootOperationTracker = testRunnerCluster.OperationTracker

            // seeding can do real biosphere calls to referenced ecosystems' dev hosts, so must run on default partition
            let partitionId = defaultGrainPartition
            let! res =
              rootOperationTracker.TrackOperation
                { Partition = partitionId
                  Type = OperationType.TestSimulation
                  Name = simulation.SimulationName
                  MaybeParentActivityId = None
                  MakeItNewParentActivityId = true
                  BeforeRunProperties = [
                    "Module", simulation.ModuleName
                    "Simulation", simulation.SimulationName
                    "DataSeeding", "true"
                  ] |> Map.ofList }
                (fun () ->
                    backgroundTask {
                        let testPartition   = {
                            EcosystemDef         = TestCluster.getEcosystemDefUnderTest()
                            GrainPartition       = partitionId
                            CapturedInteractions = System.Collections.Concurrent.ConcurrentDictionary<SubjectId, Subject>()
                            InitState            = InitializationState.Uninitialized
                            NamedValues          = System.Collections.Concurrent.ConcurrentDictionary<string, obj>()
                            StasisWaitFor        = defaultStasisWaitFor
                            ConfigOverrides      = Map.empty
                            UserId               = ""
                        }

                        let taskOnPartition = simulation.Invoke ()
                        // Unfortunately, all we can know about the simulation method is that it will return an object that has an Invoke method that
                        // itself returns a Task. This is due to the use of CEs and F# generating types conforming to said interface.
                        do! taskOnPartition.GetType().GetMethod("Invoke").Invoke(taskOnPartition, [| testPartition |]) :?> Task

                        let clock = LibLifeCycle.Services.createService "System.Clock" (testClockHandler defaultGrainPartition)
                        let! finishedSimulatedOn = clock.Query LibLifeCycle.DefaultServices.Now

                        return { ReturnValue = SimulationExecutionResult.Succeeded finishedSimulatedOn; IsSuccess = Some true; AfterRunProperties = Map.empty }
                    })


            do! testRunnerCluster.DisposeAsync()

            return res
        with
        | ex ->
            return SimulationExecutionResult.Failed ex
    }