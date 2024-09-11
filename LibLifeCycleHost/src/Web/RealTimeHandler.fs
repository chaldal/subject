module LibLifeCycleHost.Web.RealTimeHandler

open System.Runtime.CompilerServices
open System.Threading.Tasks
open Fable.SignalR
open FSharp.Control
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.FSharp.Core
open LibLifeCycle
open LibLifeCycleHost

type [<Extension>] RealTimeDataExtensions() =
    static let legacyRealTimeSettings: SignalR.Settings<LibLifeCycleTypes.LegacyRealTime.ClientApi, LibLifeCycleTypes.LegacyRealTime.ServerApi> =
        {
            Config =
                Some
                    {
                        SignalR.Config<LibLifeCycleTypes.LegacyRealTime.ClientApi, LibLifeCycleTypes.LegacyRealTime.ServerApi>.Default () with
                            LogLevel = Some LogLevel.Warning
                            HubOptions = Some (fun options -> options.EnableDetailedErrors <- true)
                    }
            Send = (fun _ _ -> Task.CompletedTask)
            Invoke = (fun _ _ -> () |> Task.FromResult)
            EndpointPattern = "/realTime"
        }

    static let v1RealTimeSettings: SignalR.Settings<LibLifeCycleTypes.Api.V1.RealTime.ClientApi, LibLifeCycleTypes.Api.V1.RealTime.ServerApi> =
        {
            Config =
                Some
                    {
                        SignalR.Config<LibLifeCycleTypes.Api.V1.RealTime.ClientApi, LibLifeCycleTypes.Api.V1.RealTime.ServerApi>.Default () with
                            LogLevel = Some LogLevel.Warning
                            HubOptions = Some (fun options -> options.EnableDetailedErrors <- true)
                    }
            Send = (fun _ _ -> Task.CompletedTask)
            Invoke = (fun _ _ -> () |> Task.FromResult)
            EndpointPattern = "/api/v1/realTime"
        }

    [<Extension>]
    static member AddRealTimeEndpoints
            (
                services: IServiceCollection,
                ecosystem: Ecosystem
            ) =
        let serviceProvider = services.BuildServiceProvider()
        let lifeCycleAdapters = serviceProvider.GetRequiredService<HostedLifeCycleAdapterCollection>()

        lifeCycleAdapters
        |> Seq.iter (fun lifeCycleAdapter ->
            LibLifeCycleHost.Web.LegacyGenericRealTimeHandler.registerRealTimeSubjectData services lifeCycleAdapter
            LibLifeCycleHost.Web.Api.V1.GenericRealTimeHandler.registerRealTimeSubjectData services lifeCycleAdapter
        )

        ecosystem.ReferencedEcosystems
        |> Map.values
        |> Seq.collect (fun referencedEcosystem -> referencedEcosystem.LifeCycles)
        |> Seq.iter (fun referencedLifeCycle ->
            referencedLifeCycle.Invoke
                { new FullyTypedReferencedLifeCycleFunction<_> with
                    member _.Invoke (referencedLifeCycle: ReferencedLifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role>) =
                        let lifeCycleAdapter: IHostedOrReferencedLifeCycleAdapter =
                            { ReferencedLifeCycle = referencedLifeCycle }
                        LibLifeCycleHost.Web.Api.V1.GenericRealTimeHandler.registerRealTimeSubjectData services lifeCycleAdapter
                        // Only keeping compiler happy (for reasons unknown).
                        lifeCycleAdapter
                }
            |> ignore
        )

        services
            .AddSignalR(legacyRealTimeSettings, LibLifeCycleHost.Web.LegacyGenericRealTimeHandler.legacyStreamToClient ecosystem)
            .AddSignalR(v1RealTimeSettings, LibLifeCycleHost.Web.Api.V1.GenericRealTimeHandler.v1StreamToClient ecosystem)
        |> ignore

        services

    [<Extension>]
    static member UseRealTimeEndpoints
            (
                builder: IApplicationBuilder,
                ecosystem: Ecosystem
            ) =

        builder
            .UseSignalR(legacyRealTimeSettings, LibLifeCycleHost.Web.LegacyGenericRealTimeHandler.legacyStreamToClient ecosystem)
            .UseSignalR(v1RealTimeSettings, LibLifeCycleHost.Web.Api.V1.GenericRealTimeHandler.v1StreamToClient ecosystem)
