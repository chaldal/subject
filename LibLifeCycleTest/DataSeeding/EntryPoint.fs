[<AutoOpen>]
module LibLifeCycleTest.DataSeeding.EntryPoint

open System
open System.Reflection
open System.Text.RegularExpressions
open LibLifeCycleTest
open Microsoft.Extensions.DependencyInjection

let private filterSimulations testAssembly filter flags =
    {
        Assembly = testAssembly
        Module = None
        Simulation = Some (Regex filter)
        IncludeNonSeedingSimulations = flags |> List.contains "--all"
    }
    |> discoverSimulations

let private writeToConsole (text: string) = System.Console.WriteLine text

let private filterAndPrint testAssembly filter flags =
    filterSimulations testAssembly filter flags
    |> List.groupBy (fun s -> s.ModuleName)
    |> List.iter (
        fun (moduleName, ss) ->
            writeToConsole $"\r\nModule \"{moduleName}\":"
            ss |> List.iter (fun s -> writeToConsole $"    {s.SimulationName}"))

let dataSeedingMain (testAssembly: Assembly) (configureCustomStorage: IServiceCollection -> unit) args =

    let filterAndPrint = filterAndPrint testAssembly

    match List.ofArray args with
    | "list" :: [] ->
        filterAndPrint ".*" []
        0

    | "list" :: (["--all"] as flags) ->
        filterAndPrint ".*" flags
        0

    | "list" :: filter :: flags ->
        filterAndPrint filter flags
        0

    | "seed" :: filter :: flags ->
        match filterSimulations testAssembly filter flags with
        | [s] ->
            writeToConsole $"Seeding simulation: {s.SimulationName}"
            TestCluster.enableTestDataSeedingMode configureCustomStorage

            let result = (executeSimulation s).ConfigureAwait(false).GetAwaiter().GetResult()
            writeToConsole "\r\n\r\n"
            match result with
            | SimulationExecutionResult.Succeeded finishedSimulatedOn ->
                writeToConsole "Seeding succeeded!"
                if finishedSimulatedOn > DateTimeOffset.Now then
                    writeToConsole $"WARNING: Simulation finished with virtual time in future: {finishedSimulatedOn}"
                0
            | SimulationExecutionResult.Failed ex ->
                writeToConsole $"Seeding failed :( \r\n ${ex}"
                -1

        | filteredSimulations ->
            writeToConsole $"Should filter exactly one simulation but found {filteredSimulations.Length}:"
            filterAndPrint filter flags
            -1

    | _ ->
        writeToConsole
            @"
Welcome to Test Data Seeding tool!

Disclaimer:
    * if simulation fails it can leave storage in dirty state
    * do NOT use if you care a lot about data in target database. Or back it up if you do.
    * do NOT run Dev Host / other hosts on top of the same SQL database while seeding. We've warned you.
        * also do NOT run hosts for referenced biosphere ecosystems if seeding simulation uses them.
    * time-travel simulations supported however
        * you may end up with future things persisted.
        * seeding both this and referenced ecosystems can produce corrupted data if clock simulated
    * some tests may be green with Test Storage but fail with real backend e.g. if test subject already exists
    * use custom storage providers at your own risk. Must be RemindersImplementation.TestNotPersistedManuallyTriggered upon seed.

Prerequisites to do once:
    * run and stop a Dev Host or other host to initialize database schema
    * configure storage / connection settings in appsettings.TestDataSeeding.json

Command line options:
    * list [regex_filter] [optional_flags]
        Output names of all available simulations by text filter.
        Skip filter to list all.

    * seed [regex_filter] [optional_flags]
        Runs a filtered simulation. Filter is required and must return only one simulation.
        Just because it seems safer to seed one thing at a time.

optional_flags:
    * --all - include Simulations without `IsForDataSeedingOnly = true` property"
        0