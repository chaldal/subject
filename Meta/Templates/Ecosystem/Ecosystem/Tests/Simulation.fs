[<AutoOpen>]
module Simulation

[<assembly: LibLifeCycleTest.TestRunner.SimulationTestFramework>]
do()

open System.Reflection
open LibLifeCycleTest

let private simulation_without_initializer = SimulationBuilder(SuiteT__EC__T.LifeCycles.AllLifeCycles.T__ECI__TEcosystem, Assembly.GetExecutingAssembly())

let initializer =
    simulation_without_initializer {
        // Perform any initialization that should run with every simulation.
        return ()
    }

let simulation =
    simulation_without_initializer.WithInitializer initializer
