module Suitejobs.TestDataSeeding

open LibLifeCycleTest.DataSeeding

[<EntryPoint>]
let main args =
    dataSeedingMain
        simulation.TestAssembly
        ignore
        args