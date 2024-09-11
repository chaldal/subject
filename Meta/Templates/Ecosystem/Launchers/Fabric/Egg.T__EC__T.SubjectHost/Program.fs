module Egg.T__EC__T.SubjectHost.Program

open LibLifeCycleHost.Host

[<EntryPoint>]
let main argv =
    let hostConfiguration = {
        ConfigureServices = ignore
        Configure         = ignore
    }

    SuiteT__EC__T.LifeCycles.AllLifeCycles.T__ECI__TEcosystem
    |> LibLifeCycleHost.Host.Fabric.SubjectHost.Bootstrap.startOnFabric argv hostConfiguration typeof<LibLifeCycleHostBuild.Build>.Assembly