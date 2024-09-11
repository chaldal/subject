namespace LibLifeCycleHost.Host.Fabric.SubjectHost

open System.Net
open System.Net.NetworkInformation
open System.Fabric
open LibLifeCycleHost.ApplicationInsights
open LibLifeCycleHost.TelemetryModel
open Microsoft.ServiceFabric.Services.Runtime
open Microsoft.ServiceFabric.Services.Communication.Runtime
open Microsoft.ServiceFabric.AspNetCore.Configuration
open LibLifeCycleHost
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options
open Microsoft.Extensions.DependencyInjection
open LibLifeCycleHost.OrleansEx.SiloBuilder
open Orleans
open Orleans.Hosting
open Orleans.Configuration
open Microsoft.Extensions.Configuration
open LibLifeCycle.Config
open LibLifeCycleCore.Certificates
open LibLifeCycleHost.Storage.SqlServer
open Microsoft.AspNetCore.DataProtection
open Microsoft.AspNetCore.DataProtection.KeyManagement
open Orleans.Connections.Security
open LibLifeCycleHost.Certificates
open LibLifeCycle
open System.Net.Http
open System
open System.Threading
open System.Diagnostics
open LibLifeCycleHost.Host.Fabric
open System.Reflection
open LibLifeCycleHost.Host

type SubjectHostStatelessService(argv: string[], ecosystem: Ecosystem, buildAssembly: Assembly, hostConfiguration: HostConfiguration, context: StatelessServiceContext) =
    inherit StatelessService(context)

    let ipToAdvertise =
        NetworkInterface.GetAllNetworkInterfaces()
        |> Seq.where   (fun networkInterface -> networkInterface.OperationalStatus = OperationalStatus.Up)
        |> Seq.map     (fun networkInterface -> networkInterface.GetIPProperties())
        |> Seq.where   (fun ipProperties -> ipProperties.GatewayAddresses.Count > 0 && ipProperties.UnicastAddresses.Count > 0)
        |> Seq.collect (fun ipProperties -> ipProperties.UnicastAddresses |> Seq.map (fun ip -> ip.Address))
        |> Seq.groupBy (fun addr -> addr.AddressFamily)
        |> Seq.sortBy  (fun (addrFamily, _) -> match addrFamily with | Sockets.AddressFamily.InterNetwork -> 0 | _ -> 1) // Prefer IPv4
        |> Seq.collect snd
        |> Seq.sortBy  (fun addr -> addr.GetAddressBytes())
        |> Seq.tryHead
        |> Option.defaultWith (fun _ -> failwith "Unable to get a usable IP Address to advertise for node")

    let tryGetPortForEndpoint name =
        match context.CodePackageActivationContext.GetEndpoints().TryGetValue name with
        | true, endpoint ->
            // Let's (for now) ignore the configured IP
            endpoint.Port |> Some
        | false, _ ->
            None

    let getPortForEndpoint name =
        tryGetPortForEndpoint name |> Option.defaultWith (fun () -> failwithf "%s is not defined in ServiceManifest.xml" name)

    override _.CreateServiceInstanceListeners() :ServiceInstanceListener seq =
        let siloEndpointPort           = getPortForEndpoint    "SiloEndpoint"
        let gatewayEndpointPort        = getPortForEndpoint    "GatewayEndpoint"
        let maybeDashboardEndpointPort = tryGetPortForEndpoint "DashboardEndpoint"

        let host =
            Host.CreateDefaultBuilder()
                .ConfigureLogging(
                    fun context logging ->
                        let loggingBuilder =
                            logging
                                .ClearProviders()
                                .AddConsole()
                        context.Configuration.GetSection("AppInsights").TryGetAndTryValidate<AppInsightsConfiguration>()
                        |> Option.iter (fun config -> loggingBuilder.AddApplicationInsights(config.InstrumentationKey) |> ignore)
                )
                .ConfigureAppConfiguration(
                    fun _hostContext config ->
                        config
                            // NB: the order of these calls is important. Parameters set by the
                            //     later function calls overrides the earlier ones.
                            .AddServiceFabricConfiguration(
                                context.CodePackageActivationContext,
                                    fun opts ->
                                        opts.IncludePackageName <- false
                                        opts.DecryptValue       <- true
                            )
                            .AddCommandLine(argv)
                        |> ignore
                )
                .ConfigureServices(
                    fun context services ->
                        match context.Configuration.GetSection("AppInsights").TryGetAndTryValidate<AppInsightsConfiguration>() with
                        | Some config ->
                            addTelemetry config.InstrumentationKey (sprintf "%s.SubjectHost" ecosystem.Name) services (* developerMode *) false (* isWorkerService *) true
                        | None ->
                            services.AddSingleton<OperationTracker>(noopOperationTracker) |> ignore

                        let certificateConfig = context.Configuration.GetSection("Certificate").Get<CertificateConfiguration>()

                        let orleansClusteringConfig =
                            context.Configuration
                                .GetSection("Orleans.Clustering")
                                .GetAndValidate<OrleansClusteringConfiguration>()

                        let sqlServerConfig =
                            context.Configuration
                                .GetSection("Storage.SqlServer")
                                .GetAndValidate<SqlServerConfiguration>()

                        services
                            .AddSingleton<HttpClient>(new HttpClient(new HttpClientHandler(UseCookies = false), Timeout = TimeSpan.FromMinutes 5.0))
                            .AddSingleton<SqlServerConnectionStrings>({ ByEcosystemName = NonemptyMap.ofOneItem (ecosystem.Name, sqlServerConfig.ConnectionString) })
                            .AddSingleton<TlsOptions>(
                                fun serviceProvider ->
                                    let (tlsCertificate, hostName) =
                                        getOrleansTlsCertificateAndHostName ecosystem.Name certificateConfig.UseDevelopmentCertificate

                                    let options = TlsOptions(LocalCertificate = tlsCertificate)
                                    options.OnAuthenticateAsClient <-
                                        fun _connection sslOptions ->
                                            // Actual value doesn't matter, just required for SSL validation
                                            sslOptions.TargetHost <- hostName

                                    options
                            )
                            .AddSingleton<HostConfiguration>(hostConfiguration)
                            .AddEcosystem(ecosystem, EcosystemStorageSetup.Proper)
                            .AddSingleton<IBiosphereGrainProvider, BiosphereGrainProvider>(fun serviceProvider ->
                                let hostEcosystemGrainFactory = serviceProvider.GetRequiredService<IGrainFactory>()
                                let operationTracker = serviceProvider.GetRequiredService<OperationTracker>()
                                BiosphereGrainProvider(ecosystem, hostEcosystemGrainFactory, orleansClusteringConfig.MembershipConnectionString, operationTracker, certificateConfig.UseDevelopmentCertificate, buildAssembly))
                            .AddDataProtection()
                            .SetApplicationName(null)
                            |> fun dataProtectionBuilder ->
                                if certificateConfig.UseDevelopmentCertificate then
                                    // don't protect key in dev environment to make Dev Fabric similar to Dev Host
                                    dataProtectionBuilder
                                else
                                    let allMatchingSecretsCertificates =
                                        getSecretsCertificatesFromStore ecosystem.Name
                                    let preferredSecretsCertificate =
                                        getPreferredSecretsCertificate System.DateTimeOffset.Now allMatchingSecretsCertificates
                                    dataProtectionBuilder
                                        .ProtectKeysWithCertificate(preferredSecretsCertificate)
                                        .UnprotectKeysWithAnyCertificate(List.toArray allMatchingSecretsCertificates)
                            |> fun dataProtectionBuilder ->
                                dataProtectionBuilder
                                    .Services
                                    .AddSingleton<IConfigureOptions<KeyManagementOptions>>(
                                        fun serviceProvider ->
                                            ConfigureOptions<KeyManagementOptions>(
                                                fun options ->
                                                    options.XmlRepository <- SqlServerDataProtectionXmlRepository(sqlServerConfig, ecosystem.Name)
                                            )
                                            :> IConfigureOptions<KeyManagementOptions>
                                    )
                            |> fun services -> hostConfiguration.ConfigureServices (context.Configuration, services)
                            |> ignore
                )
                .UseOrleans(
                    fun context siloBuilder ->
                        let orleansClusteringConfig =
                            context.Configuration
                                .GetSection("Orleans.Clustering")
                                .GetAndValidate<OrleansClusteringConfiguration>()
                        let certificateConfiguration =
                            context.Configuration
                                .GetSection("Certificate")
                                .Get<CertificateConfiguration>()

                        let properSiloSetup =
                            if certificateConfiguration.UseDevelopmentCertificate then
                                ProperSiloDev (* ShouldResetMembershipTable *) orleansClusteringConfig.ShouldInitializeForLocal1NodeEnvironment
                            else
                                ProperSiloProd

                        siloBuilder
                            .ConfigureSiloForEcosystem(
                                ecosystem,
                                buildAssembly,
                                EcosystemSiloSetup.Proper (
                                    properSiloSetup,
                                    orleansClusteringConfig.MembershipConnectionString,
                                    ipToAdvertise,
                                    gatewayEndpointPort,
                                    siloEndpointPort,
                                    (* ListenOnAnyHostAddress *) true, maybeDashboardEndpointPort))
                )
                .Build()

        ServiceInstanceListener(
            (fun _serviceContext -> OrleansCommunicationListener(host, ipToAdvertise, siloEndpointPort, gatewayEndpointPort, maybeDashboardEndpointPort) :> ICommunicationListener))
        |> Seq.singleton

type Bootstrap() =
    static member startOnFabricEx (argv: string[]) (hostConfiguration: HostConfiguration) (buildAssembly: Assembly) (ecosystem: Ecosystem) (serviceTypeName: string) =
        try
            ServiceRuntime.RegisterServiceAsync(serviceTypeName, (fun context -> new SubjectHostStatelessService(argv, ecosystem, buildAssembly, hostConfiguration, context) :> StatelessService)).GetAwaiter().GetResult()
            Thread.Sleep Timeout.Infinite
            0
        with
        | ex ->
            reraise()

    static member startOnFabric (argv: string[]) (hostConfiguration: HostConfiguration) (buildAssembly: Assembly) (ecosystem: Ecosystem) =
        Bootstrap.startOnFabricEx argv hostConfiguration buildAssembly ecosystem $"Egg.{ecosystem.Name}.SubjectHostType"