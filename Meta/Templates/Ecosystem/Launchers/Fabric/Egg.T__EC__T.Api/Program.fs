module Egg.T__EC__T.Api.Program

open LibLifeCycleHost.Host

let ecosystem = SuiteT__EC__T.LifeCycles.AllLifeCycles.T__ECI__TEcosystem

[<EntryPoint>]
let main argv =
    let hostConfiguration = {
        ConfigureServices = ignore
        Configure         = ignore
    }

    Fabric.Api.Bootstrap.startOnFabric argv hostConfiguration typeof<LibLifeCycleHostBuild.Build>.Assembly ecosystem