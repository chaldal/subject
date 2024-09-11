module LibLifeCycleHost.OrleansEx.SiloBuilder

#nowarn "0044" // Suppress deprecation Needed for IHostingEnvironment

open System.Net
open System.Security.Cryptography.X509Certificates
open LibLifeCycleHost.ApplicationInsights
open LibLifeCycleHost.Storage.SqlServer
open LibLifeCycleHost.SubjectReminderTable
open LibLifeCycleHost.TelemetryModel
open LibLifeCycleHost.TraceContextGrainCallFilter
open Orleans
open Orleans.Connections.Security
open Orleans.Hosting
open Orleans.Configuration
open Orleans.Runtime
open LibLifeCycleCore.OrleansEx
open LibLifeCycleHost
open System
open System.Runtime.CompilerServices
open LibLifeCycle
open System.Reflection
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options

let private configureSiloEcosystemDefSerializers
    (siloBuilder: ISiloBuilder)
    (lifeCycleDefs: List<LifeCycleDef>)
    (viewDefs: List<IViewDef>)
    (buildAssembly: Assembly)
    : ISiloBuilder =
        siloBuilder
            .ConfigureServices(Serialization.registerSerializers lifeCycleDefs viewDefs)
            .ConfigureApplicationParts(fun parts -> Serialization.configureApplicationParts lifeCycleDefs viewDefs parts buildAssembly)

let private configureSiloClientSerializers
    (clientBuilder: IClientBuilder)
    (lifeCycleDefs: List<LifeCycleDef>)
    (viewDefs: List<IViewDef>)
    (buildAssembly: Assembly)
    : IClientBuilder =
        clientBuilder
            .ConfigureServices(Serialization.registerSerializers lifeCycleDefs viewDefs)
            .ConfigureApplicationParts(fun parts -> Serialization.configureApplicationParts lifeCycleDefs viewDefs parts buildAssembly)

type EcosystemProperSiloSetup =
| ProperSiloProd
| ProperSiloDev of ShouldResetMembershipTable: bool

[<RequireQualifiedAccess>]
type EcosystemSiloSetup =
| Proper of
    EcosystemProperSiloSetup *
    MembershipConnectionString: string *
    AdvertisedIP: IPAddress *
    SiloPort: int *
    GatewayPort: int *
    ListenOnAnyHostAddress: bool *
    MaybeDashboardPort: Option<int>
| Test
| TestDataSeeding


[<RequireQualifiedAccess>]
type EcosystemSiloClientSetup =
| TestCluster
| TestClusterDataSeeding
// can't use public anonymous payload types because LibLifeCycleHostBuild C# codegen freaks out. Why do we even have these types generated??
| ApiToHost of EcosystemSiloClientSetupApiToHostPayload
| HostToRemoteHost of EcosystemSiloClientSetupHostToRemoteHostPayload

and EcosystemSiloClientSetupApiToHostPayload =
    {
       OrleansTlsCertificate: X509Certificate2
       OrleansHostName: string
       MembershipConnectionString: string
       HostingEnvironment: Microsoft.Extensions.Hosting.IHostingEnvironment // if no longer required, remove #nowarn at the top of the file
       OperationTracker: OperationTracker
       MaybeAppInsightsConfig: Option<AppInsightsConfiguration>
    }
and EcosystemSiloClientSetupHostToRemoteHostPayload =
    {
       OrleansTlsCertificate: X509Certificate2
       OrleansHostName: string
       MembershipConnectionString: string
       OperationTracker: OperationTracker
       RemoteEcosystemName: string
    }

[<Extension>]
type SiloConfigurationExtensions =
    [<Extension>]
    static member ConfigureSiloForEcosystem (
        siloBuilder: ISiloBuilder,
        ecosystem: Ecosystem,
        buildAssembly: Assembly,
        siloSetup: EcosystemSiloSetup)
        : unit =

        siloBuilder
            .Configure<ClusterOptions>(
                fun (opts: ClusterOptions) ->
                    opts.ClusterId <- ecosystem.Name
                    opts.ServiceId <- ecosystem.Name
            )
            .Configure<SchedulingOptions>(
                fun (opts: SchedulingOptions) ->
                    opts.AllowCallChainReentrancy <- false // TODO: maybe set true, to allow _Meta of _Meta stuff?
            )
            .AddOutgoingGrainCallFilter<TraceContextOutgoingGrainCallFilter>()
            .AddIncomingGrainCallFilter<TraceContextIncomingGrainCallFilter>()

        |> fun siloBuilder ->
            // register serializer for both host & referenced ecosystems.
            // Referenced serializers required  to deserialize life events in subscriptions
            // TODO: to minimize chance of collisions register absolute minimum of referenced types.
            Seq.fold
                (fun (siloBuilder: ISiloBuilder) (referencedEcosystem: ReferencedEcosystem) ->
                    let referencedLifeCycleDefs =
                        referencedEcosystem.LifeCycles
                        |> List.map (fun rlc -> rlc.Def)
                    let referencedViewDefs =
                        referencedEcosystem.Views
                        |> List.map (fun rv -> rv.Def)
                    configureSiloEcosystemDefSerializers siloBuilder referencedLifeCycleDefs referencedViewDefs buildAssembly)
                (configureSiloEcosystemDefSerializers siloBuilder ecosystem.LifeCycleDefs ecosystem.ViewDefs buildAssembly)
                ecosystem.ReferencedEcosystems.Values

        |> fun siloBuilder ->
            match siloSetup with
            | EcosystemSiloSetup.Test ->
                siloBuilder
                    .UseInMemoryReminderService()
                    .ConfigureLogging(fun context logging ->
                        let config = context.Configuration.GetSection("Logging")
                        logging
                            .ClearProviders()
                            .AddConfiguration(config)
                            .AddConsole() |> ignore)

                    .Configure<ClientMessagingOptions>(
                        fun(opts: ClientMessagingOptions) ->
                            opts.ResponseTimeout <- TimeSpan.FromSeconds 60.
                            opts.ResponseTimeoutWithDebugger <- TimeSpan.FromDays 1.
                    )
                    .Configure<SiloMessagingOptions>(
                        fun(opts: SiloMessagingOptions) ->
                             opts.ResponseTimeout <- TimeSpan.FromSeconds 60.
                             opts.ResponseTimeoutWithDebugger <- TimeSpan.FromDays 1.
                    )


            | EcosystemSiloSetup.TestDataSeeding ->
                siloBuilder
                    .AddStartupTask<SqlServerSetupStartupTask>(ServiceLifecycleStage.First)
                    .AddStartupTask<CustomStorageInitStartupTask>(ServiceLifecycleStage.First)
                    .UseInMemoryReminderService()
                    .ConfigureLogging(fun context logging ->
                        let config = context.Configuration.GetSection("Logging")
                        logging
                            .ClearProviders()
                            .AddConfiguration(config)
                            .AddConsole() |> ignore)

                    .Configure<ClientMessagingOptions>(
                        fun(opts: ClientMessagingOptions) ->
                            opts.ResponseTimeout <- TimeSpan.FromSeconds 60.
                            opts.ResponseTimeoutWithDebugger <- TimeSpan.FromDays 1.
                    )
                    .Configure<SiloMessagingOptions>(
                        fun(opts: SiloMessagingOptions) ->
                             opts.ResponseTimeout <- TimeSpan.FromSeconds 60.
                             opts.ResponseTimeoutWithDebugger <- TimeSpan.FromDays 1.
                    )


            | EcosystemSiloSetup.Proper (properSiloSetup, membershipConnectionString, advertisedIP, siloPort, gatewayPort, listenOnAnyHostAddress, maybeDashboardPort) ->
                let shouldResetMembershipTable =
                    match properSiloSetup with
                    | ProperSiloDev isSingleProcess ->
                        isSingleProcess
                    | ProperSiloProd ->
                        false

                siloBuilder
                    .ConfigureEndpoints(advertisedIP, siloPort, gatewayPort, listenOnAnyHostAddress)
                    // Ideally we want to just be able to say .UseTls(x509), but unfortunately there's no way to inject in configuration
                    .Configure<SiloConnectionOptions>(
                        fun (options: SiloConnectionOptions) ->
                            options.ConfigureSiloInboundConnection(
                                fun connectionBuilder ->
                                    connectionBuilder.ApplicationServices.GetRequiredService<TlsOptions>()
                                    |> connectionBuilder.UseServerTls
                            )

                            options.ConfigureGatewayInboundConnection(
                                fun connectionBuilder ->
                                    connectionBuilder.ApplicationServices.GetRequiredService<TlsOptions>()
                                    |> connectionBuilder.UseServerTls
                            )

                            options.ConfigureSiloOutboundConnection(
                                fun connectionBuilder ->
                                    connectionBuilder.ApplicationServices.GetRequiredService<TlsOptions>()
                                    |> connectionBuilder.UseClientTls
                            )
                    )
                    .AddStartupTask<SqlServerSetupStartupTask>(ServiceLifecycleStage.First)
                    .AddStartupTask<CustomStorageInitStartupTask>(ServiceLifecycleStage.First)
                    .AddStartupTask<LifeCycleHostStartupTask>(ServiceLifecycleStage.Active)
                    .Configure<ConnectionOptions>(fun (options: ConnectionOptions) ->
                        options.ProtocolVersion <- Orleans.Runtime.Messaging.NetworkProtocolVersion.Version2)
                    .ConfigureServices(fun services ->
                        services.AddSingleton<IReminderTable, SubjectReminderTable>(fun serviceProvider ->
                            let isDevHost = match properSiloSetup with | ProperSiloDev _ -> true | ProperSiloProd -> false
                            SubjectReminderTable(
                                serviceProvider.GetRequiredService<SqlServerConnectionStrings>(),
                                serviceProvider.GetRequiredService<Ecosystem>(),
                                serviceProvider.GetRequiredService<IGrainReferenceConverter>(),
                                serviceProvider.GetRequiredService<HostedLifeCycleAdapterCollection>(),
                                serviceProvider.GetRequiredService<ILogger<SubjectReminderTable>>(),
                                serviceProvider.GetRequiredService<Service<Clock>>(),
                                serviceProvider.GetRequiredService<IOptions<ConsistentRingOptions>>(),
                                isDevHost))
                        |> ignore)
                    .UseAdoNetClustering(fun (adoNetOptions: AdoNetClusteringSiloOptions) ->
                        if shouldResetMembershipTable then
                            // This is needed on dev environments where the process may not have closed cleanly
                            // and there's no other server to mark this one as dead
                            SqlServerSetup.runOrleansSetup membershipConnectionString
                            SqlServerSetup.resetMembershipTable membershipConnectionString ecosystem.Name

                        adoNetOptions.ConnectionString <- membershipConnectionString
                        adoNetOptions.Invariant        <- "Microsoft.Data.SqlClient"
                    )
                    |> fun siloBuilder ->
                            match maybeDashboardPort with
                            | Some dashboardEndpointPort ->
                                siloBuilder
                                    .UseDashboard(
                                        fun dashboardOptions ->
                                            dashboardOptions.Port <- dashboardEndpointPort
                                    )
                            | None ->
                                siloBuilder
        |> ignore

    [<Extension>]
    static member ConfigureSiloClientForEcosystem (
        clientBuilder: IClientBuilder,
        hostEcosystem: Ecosystem,
        buildAssembly: Assembly,
        clientSetup: EcosystemSiloClientSetup)
        : IClientBuilder =

        let ecosystemName =
            match clientSetup with
            | EcosystemSiloClientSetup.TestCluster
            | EcosystemSiloClientSetup.TestClusterDataSeeding
            | EcosystemSiloClientSetup.ApiToHost _ ->
                hostEcosystem.Name
            | EcosystemSiloClientSetup.HostToRemoteHost x ->
                x.RemoteEcosystemName

        clientBuilder
            .Configure<ClusterOptions>(
                fun (opts: ClusterOptions) ->
                    opts.ClusterId <- ecosystemName
                    opts.ServiceId <- ecosystemName
            )
            .AddOutgoingGrainCallFilter<TraceContextOutgoingGrainCallFilter>()

        |> fun clientBuilder ->
            match clientSetup with
            | EcosystemSiloClientSetup.TestCluster
            | EcosystemSiloClientSetup.TestClusterDataSeeding ->
                clientBuilder
                    // Sadly ClientBuilder and the host container don't share the service container
                    // so we have to explicitly bind services that we expect the Orleans client
                    // to use
                    // https://github.com/dotnet/orleans/issues/4744
                    .ConfigureServices(fun services ->
                        services
                            .AddSingleton<OperationTracker>(noopOperationTracker)
                        |> ignore
                    )

            | EcosystemSiloClientSetup.ApiToHost x ->
                (configureSiloClientSerializers clientBuilder hostEcosystem.LifeCycleDefs hostEcosystem.ViewDefs buildAssembly)
                    // Sadly ClientBuilder and ASP.NET Core don't share the service container
                    // so we have to explicitly bind services that we expect the Orleans client
                    // to use
                    // https://github.com/dotnet/orleans/issues/4744
                    .ConfigureServices(fun services ->
                        services
                            // This is needed by clientBuilder.AddApplicationInsightsTelemetryConsumer below
                            .AddSingleton<Microsoft.Extensions.Hosting.IHostingEnvironment>(x.HostingEnvironment) // if no longer required, remove #nowarn at the top of the file
                            .AddSingleton<OperationTracker>(x.OperationTracker)
                            |> ignore
                    )
                    .UseTls(x.OrleansTlsCertificate,
                        fun options ->
                            options.OnAuthenticateAsClient <-
                                fun _connection sslOptions ->
                                    // Actual value doesn't matter, just required for SSL validation
                                    sslOptions.TargetHost <- x.OrleansHostName
                    )
                    .UseAdoNetClustering(
                        fun (adoNetOptions: AdoNetClusteringClientOptions) ->
                            adoNetOptions.ConnectionString <- x.MembershipConnectionString
                            adoNetOptions.Invariant        <- "Microsoft.Data.SqlClient"
                    )
                    |> fun clientBuilder ->
                        match x.MaybeAppInsightsConfig with
                        | Some appInsightsConfig ->
                            clientBuilder.AddApplicationInsightsTelemetryConsumer(appInsightsConfig.InstrumentationKey)
                        | None ->
                            clientBuilder

            | EcosystemSiloClientSetup.HostToRemoteHost x ->
                match hostEcosystem.ReferencedEcosystems.TryFind x.RemoteEcosystemName with
                | None ->
                    // technically for dynamic ecosystems we need only subscription dispatcher interface, but registering whole ecosystem will do anyway
                    configureSiloClientSerializers clientBuilder hostEcosystem.LifeCycleDefs hostEcosystem.ViewDefs buildAssembly
                | Some referencedEcosystem ->
                    let referencedLifeCyclesDefs =
                        referencedEcosystem.LifeCycles
                        |> List.map (fun rlc -> rlc.Def)
                    let referencedViewDefs =
                        referencedEcosystem.Views
                        |> List.map (fun rv -> rv.Def)
                    configureSiloClientSerializers clientBuilder referencedLifeCyclesDefs referencedViewDefs buildAssembly

                |> fun clientBuilder ->
                    clientBuilder
                        .ConfigureServices(fun services ->
                            services
                                .AddSingleton<OperationTracker>(x.OperationTracker)
                                |> ignore
                        )
                        .UseTls(x.OrleansTlsCertificate,
                            fun options ->
                                options.OnAuthenticateAsClient <-
                                    fun _connection sslOptions ->
                                        // Actual value doesn't matter, just required for SSL validation
                                        sslOptions.TargetHost <- x.OrleansHostName
                        )
                        .UseAdoNetClustering(
                            fun (adoNetOptions: AdoNetClusteringClientOptions) ->
                                adoNetOptions.ConnectionString <- x.MembershipConnectionString
                                adoNetOptions.Invariant        <- "Microsoft.Data.SqlClient"
                        )