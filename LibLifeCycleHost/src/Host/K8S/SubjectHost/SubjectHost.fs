module LibLifeCycleHost.Host.K8S.SubjectHost

open LibLifeCycle
open LibLifeCycle.Config
open LibLifeCycleCore.Certificates
open LibLifeCycleHost.Certificates
open LibLifeCycleHost
open LibLifeCycleHost.ApplicationInsights
open LibLifeCycleHost.Host
open LibLifeCycleHost.OrleansEx.SiloBuilder
open LibLifeCycleHost.Storage.SqlServer
open LibLifeCycleHost.TelemetryModel
open LibLifeCycleHost.Host.K8S
open Microsoft.Extensions.Configuration
open Microsoft.AspNetCore.DataProtection
open Microsoft.AspNetCore.DataProtection.KeyManagement
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options
open Orleans
open Orleans.Connections.Security
open System
open System.Net
open System.Net.Http
open System.Net.NetworkInformation
open System.Reflection
open System.Threading.Tasks

// TODO: Untangle and separate Dev and Prod configurations. It's spaghetti at the moment, can't tell one from another
// e.g. in Dev it misses Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development")
// TODO End goal: same SubjectHost setup code for all environments, no duplication

let startSubjectHost
        (hostConfiguration: HostConfiguration)
        (ecosystem':         Ecosystem)
        (buildAssembly:     Assembly)
        (args:              array<string>)
        : int =
    try
        let ecosystemName = ecosystem'.Name

        Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(
                fun _hostContext config ->
                    // even when reloadOnChange = false, default file provider still creates a file system watcher, which can exhaust system limits in linux
                    let fileProviderThatDoesNotWatchFileSystem =
                        let fileProvider = config.GetFileProvider()
                        { new Microsoft.Extensions.FileProviders.IFileProvider with
                            member _.GetFileInfo (subpath: string) = fileProvider.GetFileInfo subpath
                            member this.GetDirectoryContents (subpath: string) = fileProvider.GetDirectoryContents subpath
                            member this.Watch (_filter: string) = Microsoft.Extensions.FileProviders.NullChangeToken.Singleton }
                    config
                        .SetFileProvider(fileProviderThatDoesNotWatchFileSystem)
                        .AddJsonFile(
                            "appsettings.json",
                            optional       = false,
                            reloadOnChange = false
                        )
                        .AddCommandLine(args)
                        .AddEnvironmentVariables()
                    |> ignore
            )
            .ConfigureLogging(
                fun context logging ->
                    let loggingBuilder = logging.ClearProviders().AddConsole()
                    context.Configuration.GetSection("AppInsights").TryGetAndTryValidate<AppInsightsConfiguration>()
                    |> Option.iter (fun config -> loggingBuilder.AddApplicationInsights(config.InstrumentationKey) |> ignore)
            )
            .ConfigureServices(
                fun context services ->
                    match context.Configuration.GetSection("AppInsights").TryGetAndTryValidate<AppInsightsConfiguration>() with
                    | Some config ->
                        addTelemetry config.InstrumentationKey (sprintf "%s.SubjectHost" ecosystemName) services (* developerMode *) false (* isWorkerService *) true
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
                    let connStrings = { ByEcosystemName = NonemptyMap.ofOneItem (ecosystemName, sqlServerConfig.ConnectionString) }

                    let devEnvEnabled = DevEnv.isDevEnvEnabled()
                    if devEnvEnabled then
                        // In dev env we usually have permissions to automatically create ecosystem db schema so developer doesn't have to
                        SqlServerSetup.createSchema connStrings

                    let ecosystem = DevEnv.applyDevEnvConfigToEcosystem ecosystem' context.Configuration

                    services
                        .AddSingleton<HttpClient>(new HttpClient(new HttpClientHandler(UseCookies = false), Timeout = TimeSpan.FromMinutes 5.0))
                        .AddSingleton<SqlServerConnectionStrings>(connStrings)
                        .AddSingleton<TlsOptions>(
                            fun _serviceProvider ->
                                let (tlsCertificate, hostName) =
                                    getOrleansTlsCertificateAndHostName ecosystemName certificateConfig.UseDevelopmentCertificate

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
                            let allMatchingSecretsCertificates = getSecretsCertificatesFromStore ecosystemName
                            let preferredSecretsCertificate =
                                getPreferredSecretsCertificate System.DateTimeOffset.Now allMatchingSecretsCertificates
                            dataProtectionBuilder
                                .ProtectKeysWithCertificate(preferredSecretsCertificate)
                                .UnprotectKeysWithAnyCertificate(List.toArray allMatchingSecretsCertificates)
                    |> fun dataProtectionBuilder ->
                        dataProtectionBuilder
                            .Services
                            .AddSingleton<IConfigureOptions<KeyManagementOptions>>(
                                fun _serviceProvider ->
                                    ConfigureOptions<KeyManagementOptions>(
                                        fun options ->
                                            options.XmlRepository <- SqlServerDataProtectionXmlRepository(sqlServerConfig, ecosystemName)
                                    )
                                    :> IConfigureOptions<KeyManagementOptions>
                            )
                    |> fun services -> hostConfiguration.ConfigureServices (context.Configuration, services)
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

                    let maybeIpToAdvertise =
                        NetworkInterface.GetAllNetworkInterfaces()
                        |> Seq.where   (fun networkInterface -> networkInterface.OperationalStatus = OperationalStatus.Up)
                        |> Seq.map     (fun networkInterface -> networkInterface.GetIPProperties())
                        |> Seq.where   (fun ipProperties -> ipProperties.GatewayAddresses.Count > 0 && ipProperties.UnicastAddresses.Count > 0)
                        |> Seq.collect (fun ipProperties -> ipProperties.UnicastAddresses |> Seq.map (fun ip -> ip.Address))
                        |> Seq.groupBy (fun addr -> addr.AddressFamily)
                        |> Seq.sortBy  (fun (addrFamily, _) -> match addrFamily with | Sockets.AddressFamily.InterNetwork -> 0 | _ -> 1)
                        |> Seq.collect snd
                        |> Seq.sortBy  (fun addr -> addr.GetAddressBytes())
                        |> Seq.tryHead

                    let ipToAdvertise =
                        match maybeIpToAdvertise with
                        | Some ip -> ip
                        | None ->
                            match context.Configuration["Orleans.Clustering:AdvertisedIP"] with
                            | null -> IPAddress.Loopback
                            | s    -> IPAddress.Parse s

                    let siloEndpointPort           = context.Configuration.GetValue<int>("Orleans.Clustering:SiloPort")
                    let gatewayEndpointPort        = context.Configuration.GetValue<int>("Orleans.Clustering:GatewayPort")
                    let maybeDashboardEndpointPort =
                        match context.Configuration["Orleans.Clustering:DashboardPort"] with
                        | null -> None
                        | s    -> Some (int s)

                    let ecosystem = DevEnv.applyDevEnvConfigToEcosystem ecosystem' context.Configuration

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
                                true, (* ListenOnAnyHostAddress *)
                                maybeDashboardEndpointPort
                            )
                        )
            )
            .Build()
            .RunAsync()
            .Wait()
        with
        | :? AggregateException as exn when exn.InnerExceptions.Count = 1 && exn.InnerExceptions.[0].GetType() = typeof<TaskCanceledException> ->
            printfn "Shutting Down .."
            ()

        | :? TaskCanceledException ->
            printfn "Shutting Down .."
            ()
    0
