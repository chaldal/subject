module SuiteT__EC__T.LifeCycleHost.Program

open LibLifeCycleHost.Host
open LibLifeCycleHost.Host.DevelopmentHost
open System.IO
open System.Reflection

let private ecosystem =
    SuiteT__EC__T.LifeCycles.AllLifeCycles.T__ECI__TEcosystem

[<EntryPoint>]
let main args =
    // Normalize current directory, so the settings file is found
    Assembly.GetEntryAssembly().Location
    |> Path.GetDirectoryName
    |> Directory.SetCurrentDirectory

    let hostConfiguration = {
        ConfigureServices = ignore
        Configure = ignore
    }

    startDevelopmentHost hostConfiguration ecosystem (typeof<LibLifeCycleHostBuild.Build>.Assembly) args