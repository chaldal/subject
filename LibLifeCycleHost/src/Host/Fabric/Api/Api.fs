namespace LibLifeCycleHost.Host.Fabric.Api

open System.Fabric
open Microsoft.ServiceFabric.Services.Runtime
open Microsoft.ServiceFabric.Services.Communication.AspNetCore
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open System.Fabric.Description
open Microsoft.ServiceFabric.Services.Communication.Runtime
open System.Net
open Microsoft.ServiceFabric.AspNetCore.Configuration
open Microsoft.AspNetCore.Server.Kestrel.Https
open LibLifeCycle
open LibLifeCycleHost.Host
open System.Threading
open LibLifeCycleHost.Host.Fabric
open System.Diagnostics
open System.Reflection
open Microsoft.Extensions.Configuration

type ApiStatelessService(argv: string[], ecosystem: Ecosystem, buildAssembly: Assembly, hostConfiguration: HostConfiguration, context: StatelessServiceContext) =
    inherit StatelessService(context)

    override this.CreateServiceInstanceListeners() :ServiceInstanceListener seq =
        this.Context.CodePackageActivationContext.GetEndpoints()
        |> Seq.where(fun endPoint -> endPoint.Protocol = EndpointProtocol.Http || endPoint.Protocol = EndpointProtocol.Https)
        |> Seq.map(fun endPoint ->
            new ServiceInstanceListener(
                (fun serviceContext ->
                    KestrelCommunicationListener(serviceContext, endPoint.Name, fun url listener ->
                        WebHostBuilder()
                            .ConfigureAppConfiguration(
                                fun _hostContext config ->
                                    config
                                        .AddServiceFabricConfiguration(context.CodePackageActivationContext,
                                            fun opts ->
                                                opts.IncludePackageName <- false
                                                opts.DecryptValue       <- true
                                            )
                                        .AddCommandLine(argv)
                                    |> ignore
                            )
                            .ConfigureServices(fun services ->
                                services
                                    .AddSingleton<StatelessService>(this :> StatelessService)
                                    .AddSingleton<Ecosystem>(ecosystem)
                                    .AddSingleton<BuildAssembly>({Assembly = buildAssembly})
                                    .AddSingleton<HostConfiguration>(hostConfiguration)
                                |> ignore
                            )
                            .UseStartup<Startup>()
                            .UseKestrel(
                                fun kestrelServerOptions ->
                                    kestrelServerOptions.AddServerHeader <- false
                                    kestrelServerOptions.Limits.MaxResponseBufferSize <- int64 (256 * 1024) // 256 KB
                                    kestrelServerOptions.Limits.MaxRequestBodySize <- int64 LibLifeCycleHost.Web.Http.MaxDeflatedBodySize

                                    if endPoint.Protocol = EndpointProtocol.Http then
                                        kestrelServerOptions.Listen(IPAddress.IPv6Any, endPoint.Port)
                                    else
                                        kestrelServerOptions.Listen(IPAddress.IPv6Any, endPoint.Port,
                                            fun listenOptions ->
                                                let httpsOptions = kestrelServerOptions.ApplicationServices.GetRequiredService<HttpsConnectionAdapterOptions>()
                                                listenOptions.UseHttps(httpsOptions)
                                                |> ignore)
                            )
                            .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                            .Build()
                        ) :> ICommunicationListener), endPoint.Name)
            )

type Bootstrap() =
    static member startOnFabricEx(argv: string[]) (hostConfiguration: HostConfiguration) (buildAssembly: Assembly) (ecosystem: Ecosystem) (serviceTypeName: string) =
        try
            ServiceRuntime.RegisterServiceAsync(serviceTypeName, (fun context -> new ApiStatelessService(argv, ecosystem, buildAssembly, hostConfiguration, context) :> StatelessService)).GetAwaiter().GetResult()
            System.Threading.Thread.Sleep(Timeout.Infinite)
            0
        with
        | ex ->
            reraise()

    static member startOnFabric(argv: string[]) (hostConfiguration: HostConfiguration) (buildAssembly: Assembly) (ecosystem: Ecosystem) =
        let serviceTypeName = sprintf "Egg.%s.ApiType" ecosystem.Name
        Bootstrap.startOnFabricEx argv hostConfiguration buildAssembly ecosystem serviceTypeName