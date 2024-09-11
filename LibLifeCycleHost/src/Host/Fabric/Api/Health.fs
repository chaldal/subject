module LibLifeCycleHost.Host.Fabric.Api.Health

open System.Fabric.Health
open LibLifeCycle
open Microsoft.ServiceFabric.Services.Runtime
open System

type ClusterConnectionStatus =
| Connected
| NotYetConnected of exn * ElapsedSinceFirstAttempt: TimeSpan * CancellationRequested: bool * Attempt: int
| ConnectionLost

let reportHealth (ecosystem: Ecosystem) (statelessService: StatelessService) (connectionStatus: ClusterConnectionStatus) =
    let healthSourceId = sprintf "Egg.%s.Api.ClusterClientService" ecosystem.Name

    match connectionStatus with
    | Connected ->
        ("Cluster connection is active", HealthState.Ok)
    | ConnectionLost ->
        ("Cluster connection was lost", HealthState.Error)
    | NotYetConnected (exn, ts, cancellationRequested, attempt) ->
        if ts < TimeSpan.FromSeconds 30.0 then
            sprintf "Waiting for cluster connection for %d seconds. Attempt = %d. CancellationRequested = %b. Exception details: %s",
            HealthState.Warning
        else
            sprintf "Unable to acquire cluster connection after %d seconds. Attempt = %d. CancellationRequested = %b. Exception details: %s",
             HealthState.Error
        |> fun (formatter, healthState) ->
            formatter
                (int ts.TotalSeconds)
                 attempt
                 cancellationRequested
                 (exn.ToString()),
            healthState
    |> fun (description, healthState) ->
        HealthInformation(
            healthSourceId,
            "ClusterConnection",
            healthState,
            Description = description)
        |> statelessService.Context.CodePackageActivationContext.ReportDeployedServicePackageHealth