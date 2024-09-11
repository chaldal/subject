namespace LibLifeCycleHost.Host.Fabric.SubjectHost

open System
open Microsoft.ServiceFabric.Services.Communication.Runtime
open Microsoft.Extensions.Hosting
open Orleans
open System.Net

type OrleansCommunicationListener(host: IHost, ipToAdvertise: IPAddress, siloEndpointPort: int, gatewayEndpointPort: int, maybeDashboardEndpointPort: Option<int>) =
    interface ICommunicationListener with

        member _.Abort(): unit =
            try
                printfn "OrleansCommunicationListener Abort Begin"

                host.StopAsync(TimeSpan.FromSeconds 5.).GetAwaiter().GetResult()
                host.Dispose()

                printfn "OrleansCommunicationListener Abort End"
            with
            | ex ->
                eprintfn "Exception thrown in OrleansCommunicationListener Abort: %A" ex

                try
                    host.Dispose()
                with
                | _ -> Noop

        member this.CloseAsync(cancellationToken: Threading.CancellationToken): Threading.Tasks.Task =
            task {
                try
                    printfn "OrleansCommunicationListener CloseAsync Begin"

                    do! host.StopAsync(cancellationToken)
                    host.Dispose()

                    printfn "OrleansCommunicationListener CloseAsync End"
                with
                | ex ->
                    eprintfn "Exception thrown in OrleansCommunicationListener CloseAsync: %A" ex
                    (this :> ICommunicationListener).Abort()
            } |> System.Threading.Tasks.Task.Ignore

        member this.OpenAsync(cancellationToken: Threading.CancellationToken): Threading.Tasks.Task<string> =
            try
                task {
                    do! host.StartAsync(cancellationToken)
                    return
                        seq {
                            ("GatewayEndpoint", sprintf "tcp://%s:%d" (ipToAdvertise.ToString()) gatewayEndpointPort)
                            ("SiloEndpoint"   , sprintf "tcp://%s:%d" (ipToAdvertise.ToString())    siloEndpointPort)
                            match maybeDashboardEndpointPort with
                            | Some dashboardEndpointPort ->
                                ("DashboardEndpoint", sprintf "http://%s:%d" (ipToAdvertise.ToString()) dashboardEndpointPort)
                            | None ->
                                Noop
                        }
                        |> dict
                        |> Newtonsoft.Json.JsonConvert.SerializeObject
                }
            with
            | _ ->
                (this :> ICommunicationListener).Abort()
                reraise()