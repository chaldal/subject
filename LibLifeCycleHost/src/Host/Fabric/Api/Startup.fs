namespace LibLifeCycleHost.Host.Fabric.Api

#nowarn "0044" // Suppress deprecation Needed for IHostingEnvironment

open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Microsoft.AspNetCore.Cors.Infrastructure
open System
open Microsoft.Extensions.Configuration
open LibLifeCycleHost
open LibLifeCycleHost.Web
open Microsoft.Extensions.Primitives
open Orleans
open Orleans.Configuration
open Orleans.Hosting
open LibLifeCycleHost.Storage.SqlServer
open LibLifeCycleHost.OrleansEx.SiloBuilder
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open System.Threading.Tasks
open Microsoft.AspNetCore.HttpOverrides
open System.Net
open Microsoft.AspNetCore.DataProtection
open LibLifeCycleHost.Web.Config
open LibLifeCycleHost.Web.HttpHandler
open LibLifeCycleHost.Web.RealTimeHandler
open LibLifeCycleHost.Certificates
open Microsoft.AspNetCore.Server.Kestrel.Https
open LibLifeCycle
open LibLifeCycle.Config
open LibLifeCycleCore
open LibLifeCycleCore.Certificates
open System.Net.Http
open System.Threading
open Microsoft.ServiceFabric.Services.Runtime
open LibLifeCycleHost.ApplicationInsights
open LibLifeCycleHost.Host.Fabric.Api.Health
open LibLifeCycleHost.Host
open System.Reflection
open LibLifeCycleHost.TelemetryModel

type BuildAssembly = {
    Assembly: Assembly
}

type Startup(ecosystem: Ecosystem, buildAssembly: BuildAssembly, hostConfiguration: HostConfiguration, configuration: IConfiguration) =

    // Please excuse the mutable. Unfortunately required due to the way orleans structures its APIs
    // (i.e. we have no direct access to a field that contains the connected gateway count, and instead
    // we get notified of connection status via events
    [<VolatileField>]
    let mutable isClusterClientConnected = false

    let sqlServerConfig         = configuration.GetSection("Storage.SqlServer").GetAndValidate<SqlServerConfiguration>()
    let orleansClusteringConfig = configuration.GetSection("Orleans.Clustering").GetAndValidate<OrleansClusteringConfiguration>()
    let certificateConfig       = configuration.GetSection("Certificate").Get<CertificateConfiguration>()
    let maybeAppInsightsConfig = configuration.GetSection("AppInsights").TryGetAndTryValidate<AppInsightsConfiguration>()

    let (orleansTlsCertificate, orleansHostName) =
        getOrleansTlsCertificateAndHostName ecosystem.Name certificateConfig.UseDevelopmentCertificate

    let configureCors (builder: CorsPolicyBuilder) =
        let lifeCycleNameLower = ecosystem.Name.ToLowerInvariant()
        let customCorsHostNames =
            configuration.GetSection("Application").TryGetAndValidate<ApplicationConfiguration>()
            |> Option.map (fun config -> config.CorsHostNamesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries) |> Array.toList)
            |> Option.defaultValue []
            |> List.map (sprintf "https://%s")

        builder
            .WithOrigins(
                [
                    (sprintf "https://%s.dev.subject.dev" lifeCycleNameLower)
                    (sprintf "https://%sadmin.dev.subject.dev" lifeCycleNameLower)
                    (sprintf "https://%s.subject.dev" lifeCycleNameLower)
                    (sprintf "https://%sadmin.subject.dev" lifeCycleNameLower)
                    (sprintf "https://%s.dev.subject.app" lifeCycleNameLower)
                    (sprintf "https://%sadmin.dev.subject.app" lifeCycleNameLower)
                    (sprintf "https://%s.subject.app" lifeCycleNameLower)
                    (sprintf "https://%sadmin.subject.app" lifeCycleNameLower)
                ]
                |> List.append customCorsHostNames
                |> List.toArray
            )
            .SetIsOriginAllowedToAllowWildcardSubdomains()
            .WithMethods("GET", "POST", "PUT", "OPTIONS")
            .AllowAnyHeader()
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromHours 0.5)
            .WithExposedHeaders("X-Subject-Id", "X-Total-Count")
        |> ignore

    let webApp =
        choose  [
            GET >=> choose [
                route "/hello" >=> text "world"

                // This is monitored by traefik and the node is blocked from serving traffic,
                // if we lose connectivity to the cluster
                route "/healthcheck" >=> (fun _next ctx ->
                    if isClusterClientConnected then
                        ctx.SetStatusCode 200
                        ctx.WriteStringAsync "OK"
                    else
                        ctx.SetStatusCode 503
                        ctx.WriteStringAsync "No cluster connection")
            ]
        ]

    member _.ConfigureServices(services: IServiceCollection) : unit =
        match maybeAppInsightsConfig with
        | Some config ->
            addTelemetry config.InstrumentationKey (sprintf "%s.Api" ecosystem.Name) services (* developerMode *) false (* isWorkerService *) false
        | None ->
            services.AddSingleton<OperationTracker>(noopOperationTracker) |> ignore

        configuration.GetSection("Http").TryGetAndValidate<HttpCookieConfiguration>()
        |> Option.defaultWith (fun _ -> { DefaultAppCookieDomain = ""; HostNameSuffixToAppCookieDomainQueryString = "" })
        |> services.AddSingleton<HttpCookieConfiguration>
        |> ignore

        services
            .AddSingleton<HttpClient>(new HttpClient(new HttpClientHandler(UseCookies = false), Timeout = TimeSpan.FromMinutes 5.0))
            .AddGiraffe()
            .AddCors()
            .AddEcosystem(ecosystem, EcosystemStorageSetup.Proper)
            .AddSingleton<IBiosphereGrainProvider, BiosphereGrainProvider>(fun serviceProvider ->
                let hostEcosystemGrainFactory = serviceProvider.GetRequiredService<IGrainFactory>()
                let operationTracker = serviceProvider.GetRequiredService<OperationTracker>()
                BiosphereGrainProvider(ecosystem, hostEcosystemGrainFactory, orleansClusteringConfig.MembershipConnectionString, operationTracker, certificateConfig.UseDevelopmentCertificate, buildAssembly.Assembly))
            .AddRealTimeEndpoints(ecosystem)
            .AddSingleton<ApiSessionCryptographer>(fun serviceProvider ->
                let dataProtectionProvider = serviceProvider.GetRequiredService<IDataProtectionProvider>()
                ApiSessionCryptographer(dataProtectionProvider))
            // Override GrainPartition service to always use the default
            .AddScoped<GrainPartition>(fun _ -> defaultGrainPartition)
            .AddSingleton<SqlServerConnectionStrings>({ ByEcosystemName = NonemptyMap.ofOneItem (ecosystem.Name, sqlServerConfig.ConnectionString) })
            .AddSingleton<IGrainFactory>(
                fun serviceProvider ->
                    let statelessService = serviceProvider.GetRequiredService<StatelessService>()
                    let hostingEnv = serviceProvider.GetRequiredService<IHostingEnvironment>() // if no longer required, remove #nowarn at the top of the file
                    let operationTracker = serviceProvider.GetRequiredService<OperationTracker>()

                    ClientBuilder()
                        .AddGatewayCountChangedHandler(fun _ eventArgs ->
                            if eventArgs.ConnectionRecovered then
                                isClusterClientConnected <- true
                                reportHealth ecosystem statelessService ClusterConnectionStatus.Connected
                        )
                        .AddClusterConnectionLostHandler(fun _ _ ->
                            isClusterClientConnected <- false
                            reportHealth ecosystem statelessService ClusterConnectionStatus.ConnectionLost
                        )
                        .ConfigureSiloClientForEcosystem(
                            ecosystem,
                            buildAssembly.Assembly,
                            EcosystemSiloClientSetup.ApiToHost
                                { OrleansTlsCertificate = orleansTlsCertificate
                                  OrleansHostName = orleansHostName
                                  MembershipConnectionString = orleansClusteringConfig.MembershipConnectionString
                                  HostingEnvironment = hostingEnv
                                  OperationTracker = operationTracker
                                  MaybeAppInsightsConfig = maybeAppInsightsConfig })
                        |> fun clientBuilder -> clientBuilder.Build()
                    :> IGrainFactory
            )
            .AddSingleton<HttpsConnectionAdapterOptions>(
                    let defaultCertificate =
                        if certificateConfig.UseDevelopmentCertificate then
                            starDotDevDotSubjectDotAppTlsCertificate
                        else
                            allSubjectDotAppTlsCertificates.Value
                            |> tryGetPreferredSubjectDotAppTlsCertificate DateTimeOffset.Now
                            |> Option.get

                    let sniSelector =
                        starDotDevDotSubjectDotAppTlsCertificate :: allSubjectDotAppTlsCertificates.Value
                        |> prepareSniCertificateSelector

                    let httpsOptions = HttpsConnectionAdapterOptions()
                    httpsOptions.ServerCertificateSelector <-
                        fun _ctx sniName ->
                            if String.IsNullOrWhiteSpace sniName then
                                defaultCertificate
                            else
                                sniSelector DateTimeOffset.Now sniName
                                |> Option.defaultValue defaultCertificate

                    httpsOptions
            )
            .AddHostedService(fun serviceProvider -> InitializeClusterClientService(serviceProvider, ecosystem))
            .Configure<ForwardedHeadersOptions>(
                fun (options: ForwardedHeadersOptions) ->
                    options.ForwardedHeaders <- ForwardedHeaders.XForwardedFor ||| ForwardedHeaders.XForwardedProto
                    options.ForwardLimit <- Nullable()
                    // Trust X-F-F from any host on an internal non-routable network
                    // This policy requires re-consideration once we get employees to use a VPN
                    options.KnownNetworks.Add(IPNetwork(IPAddress.Loopback, 8))
                    options.KnownNetworks.Add(IPNetwork(IPAddress.Parse("10.0.0.0"), 8))
                    options.KnownNetworks.Add(IPNetwork(IPAddress.Parse("172.16.0.0"), 12))
                    options.KnownNetworks.Add(IPNetwork(IPAddress.Parse("192.168.0.0"), 16))

                    // Cloudflare IPs
                    options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("173.245.48.0"), 20));
                    options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("103.21.244.0"), 22));
                    options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("103.22.200.0"), 22));
                    options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("103.31.4.0"), 22));
                    options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("141.101.64.0"), 18));
                    options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("108.162.192.0"), 18));
                    options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("190.93.240.0"), 20));
                    options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("188.114.96.0"), 20));
                    options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("197.234.240.0"), 22));
                    options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("198.41.128.0"), 17));
                    options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("162.158.0.0"), 15));
                    options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("104.16.0.0"), 13));
                    options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("104.24.0.0"), 14));
                    options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("172.64.0.0"), 13));
                    options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("131.0.72.0"), 22));

                    // BD Cluster
                    options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("103.234.164.128"), 29)); // CA
                    options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("116.206.62.160"), 29)); // AA
                    options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("103.163.171.160"), 29)); // CC
                    options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("103.185.176.0"), 23)); // Own IPs

                    options.KnownProxies.Clear()
                    options.KnownProxies.Add(IPAddress.IPv6Loopback)
            )
            .AddDataProtection()
            .SetApplicationName(null)
            .AddKeyManagementOptions(
                fun options ->
                    options.XmlRepository <- SqlServerDataProtectionXmlRepository(sqlServerConfig, ecosystem.Name)
            )
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
                dataProtectionBuilder.Services
            |> fun services ->
                services.AddHttpContextAccessor()
            |> fun services -> hostConfiguration.ConfigureServices (configuration, services)

    member _.Configure(app: IApplicationBuilder, _env: IWebHostEnvironment) :unit =
        // Save original X-F-F Header and RemoteIP Information for logging
        // The Forwarded Middleware below processes it
        UseExtensions.Use(app,
            fun context (next: Func<Task>) ->
                let existingXff = context.Request.Headers["X-Forwarded-For"]
                let remoteIp = context.Connection.RemoteIpAddress |> fun ip -> (if ip.IsIPv4MappedToIPv6 then ip.MapToIPv4() else ip).ToString()
                context.Request.Headers.Add("X-Egg-Original-IP-Chain", StringValues(Seq.append existingXff [remoteIp] |> Array.ofSeq));
                // context.Items["X-Egg-Original-IP-Chain"] <- existingXff
                next.Invoke())

            .UseForwardedHeaders()

            // Ensure IP is not IPV6-wrapped
            |> fun app ->
                UseExtensions.Use(app,
                    fun context (next: Func<Task>) ->
                        if context.Connection.RemoteIpAddress.IsIPv4MappedToIPv6 then
                            context.Connection.RemoteIpAddress <- context.Connection.RemoteIpAddress.MapToIPv4()
                        next.Invoke())

                    .UseDeveloperExceptionPage()
                    .UseCors(configureCors)
                    .UseCookiePolicy()
                    .UseRouting()
                    .UseRealTimeEndpoints(ecosystem)
                    .UseHttpEndpoints((* suppressExceptionDetails *) true)
        |> hostConfiguration.Configure

        app
            .UseGiraffe(webApp)



and InitializeClusterClientService(serviceProvider: IServiceProvider, ecosystem: Ecosystem) =
    let stopCancellationTokenSource = new CancellationTokenSource()

    let statelessService = serviceProvider.GetRequiredService<StatelessService>()

    interface IHostedService with

        member _.StartAsync(cancellationToken: Threading.CancellationToken): Task =
            let anyCancelationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, stopCancellationTokenSource.Token)

            // Don't block the start-up of the API service on cluster client initializing
            // While it is desirable to wait until we get a cluster connection, this essentially means we're
            // treating start-up differently from ongoing loss of connection.
            // Also while this is blocked, we don't get a signal if the process is shutting down
            Task.Run(fun _ ->
                let startTime = DateTimeOffset.Now

                let anyCancellationToken = anyCancelationTokenSource.Token

                let mutable attempt = 1

                let hostEcosystemClusterClient =
                    serviceProvider
                        .GetRequiredService<IGrainFactory>()
                        :?> IClusterClient

                hostEcosystemClusterClient.Connect(fun exnBeforeRetry ->
                    let elapsed = DateTimeOffset.Now - startTime
                    reportHealth ecosystem statelessService
                        (ClusterConnectionStatus.NotYetConnected (exnBeforeRetry, elapsed, anyCancellationToken.IsCancellationRequested, attempt))

                    if anyCancellationToken.IsCancellationRequested then
                        Task.FromResult false
                    else
                        attempt <- attempt + 1
                        Task.Delay(TimeSpan.FromSeconds 2.0).ContinueWith(fun _ -> true))

            , anyCancelationTokenSource.Token)
            |> ignore

            Task.CompletedTask

        member _.StopAsync(forceShutdownRequested: Threading.CancellationToken): Task =
            stopCancellationTokenSource.Cancel()

            let biosphereGrainProvider =
                serviceProvider.GetRequiredService<IBiosphereGrainProvider>()

            if not forceShutdownRequested.IsCancellationRequested then
                biosphereGrainProvider.Close()
            else
                Task.CompletedTask
