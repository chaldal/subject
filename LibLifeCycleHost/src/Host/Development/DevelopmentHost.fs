module LibLifeCycleHost.Host.DevelopmentHost

open System.Net
open System.Threading.Tasks
open LibLifeCycleHost.ApplicationInsights
open LibLifeCycleHost.TelemetryModel
open LibLifeCycleHost.Web
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Hosting
open LibLifeCycleCore
open LibLifeCycleCore.Certificates
open LibLifeCycleHost
open LibLifeCycleHost.Web.Config
open LibLifeCycleHost.Web.HttpHandler
open LibLifeCycleHost.Web.RealTimeHandler
open LibLifeCycleHost.OrleansEx.SiloBuilder
open LibLifeCycleHost.Storage.SqlServer
open Microsoft.Extensions.Configuration
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Options
open Microsoft.AspNetCore.DataProtection
open Microsoft.AspNetCore.DataProtection.KeyManagement
open System
open LibLifeCycle
open LibLifeCycle.Config
open System.Net.Http
open System.Reflection
open Orleans
open Orleans.Connections.Security

let private applyDevHostConfigToEcosystem (ecosystem: Ecosystem) (config: IConfiguration) =
    match config.GetSection("DevHost.UnrestrictedApiAccess").Get<bool>() with
    | false ->
        ecosystem
    | true ->
        { Name = ecosystem.Name
          Def = ecosystem.Def
          Connectors = ecosystem.Connectors
          EnforceRateLimits = ecosystem.EnforceRateLimits
          LifeCycles =
              ecosystem.LifeCycles
              |> Map.map (fun _ lc ->
                  lc.Invoke
                      { new FullyTypedLifeCycleFunction<_> with
                          member _.Invoke lifeCycle =
                              { lifeCycle with
                                  MaybeApiAccess =
                                      { AccessRules = [{ Input = MatchAny; Roles = MatchAny; EventTypes = MatchAny; Decision = Grant }]
                                        AccessPredicate = fun _ _ _ _ -> true
                                        RateLimitsPredicate = fun _ -> Some [] // no api limits, should be a separate config key?
                                        AnonymousCanReadTotalCount = true }
                                      |> Some }
                              :> ILifeCycle })
          Views =
              ecosystem.Views
              |> Map.map (fun _ v ->
                  v.Invoke
                      { new FullyTypedViewFunction<_> with
                          member _.Invoke view =
                              { view with
                                  MaybeApiAccess =
                                      { ViewApiAccess.AccessRules = [{ Input = MatchAny; Roles = MatchAny; Decision = Grant }]
                                        AccessPredicate = fun _ _ _ _ -> true }
                                      |> Some }
                              :> IView })

          TimeSeries =
              ecosystem.TimeSeries
              |> Map.map (fun _ timeSeries ->
                  timeSeries.Invoke
                    { new FullyTypedTimeSeriesFunction<_> with
                        member _.Invoke timeSeries =
                           { timeSeries with
                                MaybeApiAccess =
                                    { TimeSeriesApiAccess.AccessRules = [{ Input = MatchAny; EventTypes = MatchAny; Roles = MatchAny; Decision = Grant }]
                                      AccessPredicate = fun _ _ _ _ -> true }
                                    |> Some }
                           :> ITimeSeries })

          ReferencedEcosystems =
              ecosystem.ReferencedEcosystems
              |> Map.map (fun _ referencedEcosystem ->
                  { Def = referencedEcosystem.Def
                    LifeCycles =
                        referencedEcosystem.LifeCycles
                        |> List.map (fun lc ->
                            lc.Invoke
                                { new FullyTypedReferencedLifeCycleFunction<_> with
                                    member _.Invoke lifeCycle =
                                        { lifeCycle with
                                            MaybeApiAccess =
                                                { AccessRules = [{ Input = MatchAny; Roles = MatchAny; EventTypes = MatchAny; Decision = Grant }]
                                                  AccessPredicate = fun _ _ _ _ -> true
                                                  RateLimitsPredicate = fun _ -> Some [] // no api limits, should be a separate config key?
                                                  AnonymousCanReadTotalCount = true }
                                                |> Some }
                                        :> IReferencedLifeCycle })
                    Views =
                        referencedEcosystem.Views
                        |> List.map (fun v ->
                            v.Invoke
                                { new FullyTypedReferencedViewFunction<_> with
                                    member _.Invoke view =
                                        { view with
                                            MaybeApiAccess =
                                                { AccessRules = [{ Input = MatchAny; Roles = MatchAny; Decision = Grant }]
                                                  AccessPredicate = fun _ _ _ _ -> true }
                                                |> Some }
                                        :> IReferencedView }) }) }

let startDevelopmentHost (hostConfiguration: HostConfiguration) (ecosystem': Ecosystem) (buildAssembly: Assembly) args =

    Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development")
    try
        Host.CreateDefaultBuilder(args)
            .ConfigureLogging(
                fun context logging ->
                    let loggingBuilder =
                        logging
                            .ClearProviders()
                            .AddSimpleConsole(fun options ->
                                    options.IncludeScopes <- true
                                    options.TimestampFormat <- $"________________________________________________________________{Environment.NewLine}HH:mm:ss "
                                )

                    loggingBuilder.AddSeq() |> ignore

                    context.Configuration.GetSection("AppInsights").TryGetAndTryValidate<AppInsightsConfiguration>()
                    |> Option.iter (fun config -> loggingBuilder.AddApplicationInsights(config.InstrumentationKey) |> ignore)
            )
            .ConfigureWebHost(
                fun webBuilder ->
                    webBuilder
                        .UseKestrel(
                            fun options ->
                                options.Limits.MaxRequestBodySize <- int64 LibLifeCycleHost.Web.Http.MaxDeflatedBodySize
                                options.Limits.MaxResponseBufferSize <- int64 (256 * 1024) // 256 KB
                        )
                        .Configure(
                            fun context app ->
                                let ecosystem = applyDevHostConfigToEcosystem ecosystem' context.Configuration
                                app
                                    .UseDeveloperExceptionPage()
                                    .UseCors(fun cors ->
                                        cors.AllowAnyMethod().AllowAnyHeader().WithExposedHeaders("X-Subject-Id", "X-Total-Count", "X-Login-Url")
                                            .SetIsOriginAllowed(fun _ -> true).AllowCredentials() |> ignore)
                                    .UseRouting()
                                    .UseRealTimeEndpoints(ecosystem)
                                    .UseHttpEndpoints((* suppressExceptionDetails *) false)

                                    |> hostConfiguration.Configure
                            )
                    |> ignore)
            .ConfigureServices(
                fun context services ->
                    let ecosystem = applyDevHostConfigToEcosystem ecosystem' context.Configuration

                    match context.Configuration.GetSection("AppInsights").TryGetAndTryValidate<AppInsightsConfiguration>() with
                    | Some config ->
                        addTelemetry config.InstrumentationKey (sprintf "%s.DevHost" ecosystem.Name) services (* developerMode *) false (* isWorkerService *) false
                    | None ->
                        services.AddSingleton<OperationTracker>(noopOperationTracker) |> ignore

                    context.Configuration.GetSection("Http").TryGetAndValidate<HttpCookieConfiguration>()
                    |> Option.defaultWith (fun _ -> { DefaultAppCookieDomain = ""; HostNameSuffixToAppCookieDomainQueryString = "" })
                    |> services.AddSingleton<HttpCookieConfiguration>
                    |> ignore

                    services.AddSingleton<HttpHandlerSettings>(HttpHandlerSettings((* isDevHost *) true))
                    |> ignore

                    let orleansClusteringConfig =
                        context.Configuration
                            .GetSection("Orleans.Clustering")
                            .GetAndValidate<OrleansClusteringConfiguration>()

                    let sqlServerConfig =
                        context.Configuration
                            .GetSection("Storage.SqlServer")
                            .GetAndValidate<SqlServerConfiguration>()
                    let connStrings = { ByEcosystemName = NonemptyMap.ofOneItem (ecosystem.Name, sqlServerConfig.ConnectionString) }

                    // We need to setup the schema here, as a lot of other processes are
                    // dependent on the schema having been already created
                    SqlServerSetup.createSchema connStrings

                    services
                        .AddSingleton<HttpClient>(new HttpClient(new HttpClientHandler(UseCookies = false), Timeout = TimeSpan.FromMinutes 5.0))
                        .AddSingleton<SqlServerConnectionStrings>(
                            fun serviceProvider ->
                                let lifeCycleAdapterCollection = serviceProvider.GetRequiredService<HostedLifeCycleAdapterCollection>()
                                let timeSeriesAdapterCollection = serviceProvider.GetRequiredService<TimeSeriesAdapterCollection>()
                                SqlServerSetup.doSetup connStrings lifeCycleAdapterCollection timeSeriesAdapterCollection
                                connStrings
                        )
                        .AddCors()
                        .AddSingleton<TlsOptions>(
                                fun serviceProvider ->
                                    let (tlsCertificate, hostName) =
                                        getOrleansTlsCertificateAndHostName ecosystem.Name (* useDevelopmentCertificate *) true

                                    let options = TlsOptions(LocalCertificate = tlsCertificate)
                                    options.OnAuthenticateAsClient <-
                                        fun _connection sslOptions ->
                                            // Actual value doesn't matter, just required for SSL validation
                                            sslOptions.TargetHost <- hostName

                                    options
                            )
                        .AddEcosystem(ecosystem, EcosystemStorageSetup.Proper)
                        .AddSingleton<IBiosphereGrainProvider, BiosphereGrainProvider>(fun serviceProvider ->
                            let hostEcosystemGrainFactory = serviceProvider.GetRequiredService<IGrainFactory>()
                            let operationTracker = serviceProvider.GetRequiredService<OperationTracker>()
                            BiosphereGrainProvider(ecosystem, hostEcosystemGrainFactory, orleansClusteringConfig.MembershipConnectionString, operationTracker, (* useDevelopmentCertificate *) true, buildAssembly))
                        .AddRealTimeEndpoints(ecosystem)
                        .AddSingleton<ApiSessionCryptographer>(fun serviceProvider ->
                            let dataProtectionProvider = serviceProvider.GetRequiredService<IDataProtectionProvider>()
                            ApiSessionCryptographer(dataProtectionProvider))
                        // Override GrainPartition service to always use the default
                        .AddScoped<GrainPartition>(fun _ -> defaultGrainPartition)
                        .AddDataProtection()
                        .SetApplicationName(null)
                        .Services
                        .AddSingleton<IConfigureOptions<KeyManagementOptions>>(
                            fun serviceProvider ->
                                ConfigureOptions<KeyManagementOptions>(
                                    fun options ->
                                        options.XmlRepository <- SqlServerDataProtectionXmlRepository(sqlServerConfig, ecosystem.Name)
                                )
                                :> IConfigureOptions<KeyManagementOptions>
                        )
                        .AddHttpContextAccessor()


                        |> fun services -> hostConfiguration.ConfigureServices (context.Configuration, services)
            )
            .UseOrleans(
                fun context siloBuilder ->
                    let ecosystem = applyDevHostConfigToEcosystem ecosystem' context.Configuration
                    let orleansClusteringConfig =
                        context.Configuration
                            .GetSection("Orleans.Clustering")
                            .GetAndValidate<OrleansClusteringConfiguration>()
                    siloBuilder
                        .ConfigureSiloForEcosystem(
                            ecosystem,
                            buildAssembly,
                            EcosystemSiloSetup.Proper (
                                ProperSiloDev (* ShouldResetMembershipTable *) true,
                                orleansClusteringConfig.MembershipConnectionString,
                                IPAddress.Loopback,
                                orleansClusteringConfig.DevHostGatewayPort,
                                orleansClusteringConfig.DevHostSiloPort,
                                (* ListenOnAnyHostAddress *) false, (* MaybeDashboardPort *) None))
            )
            .RunConsoleAsync()
            .Wait()
        with
        | :? AggregateException as exn when exn.InnerExceptions.Count = 1 && exn.InnerExceptions.[0].GetType() = typeof<TaskCanceledException> ->
            printfn "Shutting Down .."
            () // NO-OP

        | :? TaskCanceledException ->
            printfn "Shutting Down .."
            () // NO-OP
    0

