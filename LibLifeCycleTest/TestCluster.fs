[<AutoOpen>]
module LibLifeCycleTest.TestCluster

open System
open LibLifeCycleHost.ApplicationInsights
open LibLifeCycleHost.Storage.SqlServer
open LibLifeCycleHost.TelemetryModel
open Orleans
open FSharpPlus
open Orleans.Connections.Security
open Orleans.Hosting
open Orleans.TestingHost
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open LibLifeCycle
open LibLifeCycle.Config
open LibLifeCycleCore
open LibLifeCycleCore.Certificates
open LibLifeCycleHost
open Microsoft.Extensions.Configuration
open Microsoft.AspNetCore.DataProtection
open System.Net.Http
open LibLifeCycleHost.OrleansEx.SiloBuilder
open System.Threading.Tasks
open System.Collections.Concurrent
open Autofac
open Autofac.Extensions.DependencyInjection
open Xunit.Sdk
open System.Threading

type TestConnectorInterceptorId = TestConnectorInterceptorId of ConnectorName: string * Id: Guid

let private chainInterceptors
                (serviceProvider: IServiceProvider) (request: 'Request) (next: IServiceProvider -> 'Request -> Task<ResponseVerificationToken>)
                (interceptors: list<ConnectorInterceptor<'Request>>)
                : Task<ResponseVerificationToken> =
    let rec getNextInterceptor
        (middlewares: list<ConnectorInterceptor<'Request>>)
        (serviceProvider: IServiceProvider) (req: 'Request) : Task<ResponseVerificationToken> =
        match middlewares with
        | head::tail ->
            let next env req = getNextInterceptor tail env req
            head serviceProvider req next
        | [] ->
            next serviceProvider req

    getNextInterceptor interceptors serviceProvider request

let private allConnectorInterceptors = ConcurrentDictionary<(* connector name *) string, ConcurrentDictionary<(* grain partition *) Guid, list<(* interceptor guid *) Guid * obj>>>()

let interceptConnector (testPartition: TestPartition) (connector: Connector<'Request, 'Env>) (interceptor: ConnectorInterceptor<'Request>) =
    let (GrainPartition grainPartitionGuid) = testPartition.GrainPartition
    let interceptorGuid = Guid.NewGuid()
    let innerDict = allConnectorInterceptors.GetOrAdd(connector.Name, fun _ -> ConcurrentDictionary<Guid, list<Guid * obj>>())
    let interceptorId = TestConnectorInterceptorId(connector.Name, interceptorGuid)
    innerDict.AddOrUpdate(grainPartitionGuid,
        (fun _ -> [interceptorGuid, box interceptor]),
        (fun _ existing -> (interceptorGuid, (box interceptor))::existing))
    |> ignore
    interceptorId

let removeConnectorInterception (GrainPartition grainPartitionGuid) (TestConnectorInterceptorId(connectorName, interceptorGuid)) =
    match allConnectorInterceptors.TryGetValue connectorName with
    | true, innerDict ->
        innerDict.AddOrUpdate(grainPartitionGuid,
            (fun _ -> []),
            (fun _ interceptors -> interceptors |> List.where (fun (interceptorGuidI, _) -> interceptorGuidI <> interceptorGuid)))
        |> ignore
    | false, _ ->
        ()

let removeAllConnectorInterceptionForGrainParition (GrainPartition grainPartitionGuid) =
    allConnectorInterceptors
    |> Seq.iter (fun i -> i.Value.TryRemove grainPartitionGuid |> ignore)

let private addTestMasterInterceptor<'Request, 'Env when 'Request :> Request and 'Env :> Env> (ecosystem: Ecosystem) (connector: Connector<'Request, 'Env>) : Ecosystem =
    let masterInterceptor (serviceProvider: IServiceProvider) (request: 'Request) (_next: IServiceProvider -> 'Request -> Task<ResponseVerificationToken>) =
        let next: IServiceProvider -> 'Request -> Task<ResponseVerificationToken> =
            fun _sp req -> failwithf "Unintercepted Connector %s. All connector requests must be intercepted in tests using Ecosystem.interceptConnector. Request: %A" connector.Name req

        let (GrainPartition grainPartition) = serviceProvider.GetRequiredService<GrainPartition>()
        match allConnectorInterceptors.TryGetValue connector.Name with
        | (true, interceptorsByGuid) ->
            match interceptorsByGuid.TryGetValue grainPartition with
            | (true, interceptors) ->
                interceptors
                |> Seq.map snd
                |> Seq.cast<ConnectorInterceptor<'Request>>
                |> Seq.toList
                |> chainInterceptors serviceProvider request next
            | _ ->
                next serviceProvider request
        | _ ->
            next serviceProvider request

    Ecosystem.addConnectorInterceptor connector masterInterceptor ecosystem

let private interceptAllEcosystemConnectors (ecosystem: Ecosystem) : Ecosystem =
    ecosystem.Connectors
    |> Map.values
    |> Seq.fold (fun ecosystem connector ->
        connector.Invoke
            { new FullyTypedConnectorFunction<_> with
                member _.Invoke (connector: Connector<'Request, 'Env>) =
                    addTestMasterInterceptor<'Request, 'Env> ecosystem connector }
    ) ecosystem

let private implementReferencedLifeCycle
    (referencedLifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'ReferencedSession, 'ReferencedRole, 'Env>)
    (thisEcosystem: Ecosystem)
    : Ecosystem =
        { thisEcosystem with LifeCycles = thisEcosystem.LifeCycles.Add (referencedLifeCycle.Definition.Key, referencedLifeCycle) }

let implementReferencedEcosystem (thisEcosystem: Ecosystem) (referencedEcosystemImpl: Ecosystem) : Ecosystem =
    if thisEcosystem.Name = referencedEcosystemImpl.Name then
        failwith "Ecosystem can't implement itself as a referenced ecosystem"

    referencedEcosystemImpl.LifeCycles
    |> Map.values
    |> Seq.filter (fun lc -> lc.Name <> MetaLifeCycleName && lc.Name <> "_Startup" && lc.Name <> "_RequestRateCounter")
    |> Seq.fold (fun ecosystem lc ->
        lc.Invoke
            { new FullyTypedLifeCycleFunction<_> with
                member _.Invoke lifeCycle = implementReferencedLifeCycle lifeCycle ecosystem }
        // referenced life cycles now implemented & co-hosted, pretend that not referenced
        |> fun ecosystem -> { ecosystem with ReferencedEcosystems = ecosystem.ReferencedEcosystems.Remove referencedEcosystemImpl.Name }
    ) thisEcosystem

    // TODO: implement referenced time series


    |> fun ecosystem ->
        referencedEcosystemImpl.Connectors.Values
        |> Seq.fold (fun ecosystem referencedConnector ->
            // We need to rename referenced ecosystem connectors to avoid name clashes with the
            // current ecosystem's connectors.
            // We need to "in-place" edit the name of the referenced connector, as these objects
            // are already linked into the referenced ecosystem's lifecyes. Typical copy-on-edit operations
            // will not work
            let prefixedConnectorName = $"{referencedEcosystemImpl.Name}/{referencedConnector.Name}"
            referencedConnector.GetType()
                .GetProperty(nameof(referencedConnector.Name)).GetSetMethod()
                .Invoke(referencedConnector, [| prefixedConnectorName |])
                |> ignore

            { ecosystem with Connectors = ecosystem.Connectors.Add(referencedConnector.Name, referencedConnector) })
            ecosystem

// Unforunately there appears to be no way to be able to dynamically pass this onto TestHostConfigurator
let private ecosystemInitLockObj = obj()
let mutable private ecosystemDefUnderTest: Option<EcosystemDef> = None
let mutable private ecosystemUnderTest: Option<Ecosystem> = None
let mutable private configureTestServices: Option<IServiceCollection -> unit> = None

let getEcosystemDefUnderTest () =
    match ecosystemDefUnderTest with
    | Some ecosystemDef ->
        ecosystemDef
    | None ->
        failwith "Test Cluster hasn't been initialized yet"

type BadLogCounters = {
    Warning : Ref<int>
    Error   : Ref<int>
    Critical: Ref<int>
} with
    member this.Reset() =
        Interlocked.Exchange(this.Warning,  0) |> ignore
        Interlocked.Exchange(this.Error,    0) |> ignore
        Interlocked.Exchange(this.Critical, 0) |> ignore

    member this.GetFailureAggregateStringIfNonZero() =
        let (numWarnings, numErrors, numCritical) = (
            Thread.VolatileRead this.Warning,
            Thread.VolatileRead this.Error,
            Thread.VolatileRead this.Critical
        )
        if numWarnings + numErrors + numCritical > 0 then
            seq {
                (numWarnings, "Warnings")
                (numErrors,   "Errors")
                (numCritical, "Criticals")
            }
            |> Seq.where (fun (num, _) -> num > 0)
            |> Seq.map (fun t -> t ||> sprintf "%d %s")
            |> String.concat ","
            |> Some
        else
            None

let internal partitionIdToTestOutputHelper = ConcurrentDictionary<GrainPartition, TestOutputHelper * BadLogCounters>()

type private TestLogger(categoryName: string) =
    let lastScope = AsyncLocal<Option<LoggerScopeDictionary>>()
    let clearDispose =
        { new IDisposable with
            member _.Dispose() =
                lastScope.Value <- None
        }

    let noopDispose =
        { new IDisposable with
            member _.Dispose() = ()
        }

    interface ILogger with
        member this.BeginScope<'State>(state: 'State) =
            if typeof<'State>.IsAssignableFrom(typeof<LoggerScopeDictionary>) then
                lastScope.Value <- state |> box :?> LoggerScopeDictionary |> Some
                clearDispose
            else
                noopDispose

        member this.IsEnabled logLevel =
            logLevel >= LogLevel.Debug

        member this.Log(logLevel, _eventId, state, excn, formatter) =
            lastScope.Value
            |> Option.iter (fun lastScope ->
                IReadOnlyDictionary.tryGetValue PartitionScopeKey lastScope
                |> Option.bind (fun partitionIdGuidStr ->
                    match Guid.TryParse partitionIdGuidStr with
                    | true, partitionId ->
                        Some (GrainPartition partitionId)
                    | false, _ ->
                        None
                )
                |> Option.bind (fun partitionId ->
                    IReadOnlyDictionary.tryGetValue partitionId partitionIdToTestOutputHelper
                    |> Option.map (fun (testOutputHelper, badLogCouters) -> (partitionId, testOutputHelper, badLogCouters))
                )
                |> Option.iter (fun (partitionId, testOutputHelper, badLogCouters) ->
                    match logLevel with
                    | LogLevel.Warning ->
                        Interlocked.Increment badLogCouters.Warning  |> ignore
                    | LogLevel.Error ->
                        Interlocked.Increment badLogCouters.Error    |> ignore
                    | LogLevel.Critical ->
                        Interlocked.Increment badLogCouters.Critical |> ignore
                    | _ -> ()

                    let now = DateTimeOffset.Now
                    let title =
                        if categoryName.StartsWith "LibLifeCycleHost.SubjectGrain" ||
                           categoryName.StartsWith "LibLifeCycleHost.SubjectIdGenerationGrain" then
                            let grainIdStr =
                                IReadOnlyDictionary.tryGetValue IdStrScopeKey lastScope
                                |> Option.bind tryUnbox<string>
                                |> Option.defaultValue "ID-UNKNOWN"

                            let lifeCycleName =
                                IReadOnlyDictionary.tryGetValue LifeCycleNameScopeKey lastScope
                                |> Option.bind tryUnbox<string>
                                |> Option.defaultValue "LIFECYCLE-UNKNOWN"

                            sprintf "LifeCycle %s #%s" lifeCycleName grainIdStr

                        elif categoryName.StartsWith "LibLifeCycleHost.ConnectorGrain" then
                            let connectorName =
                                IReadOnlyDictionary.tryGetValue ConnectorNameScopeKey lastScope
                                |> Option.bind tryUnbox<string>
                                |> Option.defaultValue "CONNECTOR-UNKNOWN"

                            sprintf "Connector %s" connectorName
                        else
                            categoryName

                    let logLevelStr =
                        if logLevel > LogLevel.Information then
                            sprintf " [%O] " logLevel
                        else ""

                    formatter.Invoke(state, excn)
                    |> fun s -> s.Replace("==>", "\n\n==>").Replace(" ERROR ", " **ERROR** ", StringComparison.InvariantCultureIgnoreCase)
                    |> sprintf "# %s%s\n*On: %s*\n*Simulated On: %s*\n\n%s\n" logLevelStr title (now.ToString()) ((simulatedNow partitionId).ToString())
                    |> fun s ->
                        // why is the formatter not rendering the exception?
                        if excn <> null then
                            sprintf "%s\n\nException:\n%A" s excn
                        else
                            s
                    |> testOutputHelper.WriteLine
                )
            )

let mutable private maybeTestDataSeeding:
    Option<{| ConfigureCustomStorage: IServiceCollection -> unit |}> = None

let private storageSetup() =
    match maybeTestDataSeeding with
    | None -> EcosystemStorageSetup.Test
    | Some _ -> EcosystemStorageSetup.TestDataSeeding

let internal enableTestDataSeedingMode
    (configureCustomStorage: IServiceCollection -> unit) =
    maybeTestDataSeeding <- Some {| ConfigureCustomStorage = configureCustomStorage |}

// data protection provider must be shared between different silos (test cluster has 2 by default)
// to avoid intermittent errors when encrypting and decrypting code run on different silos
// TODO: in TestDataSeeding mode protection provider must match SubjectHost / DevHost configuration
let private globalSingletonDataProtectionProvider = EphemeralDataProtectionProvider()

type private TestHostConfigurator() =
    interface IHostConfigurator with
        member _.Configure (hostBuilder: IHostBuilder) : unit =
            hostBuilder
                // We are using Autofac here, instead of the built-in ServiceProvider, as it provides the ability
                // to override registrations on child scopes
                .UseServiceProviderFactory(AutofacServiceProviderFactory())
                .ConfigureLogging(
                    fun context logging ->
                        let config = context.Configuration.GetSection("Logging")
                        logging
                            .AddConfiguration(config)
                            // .AddConsole()
                            .AddProvider(
                                { new ILoggerProvider with
                                      member this.CreateLogger(categoryName) =
                                        new TestLogger(categoryName) :> ILogger

                                      member this.Dispose() = ()
                                }
                            )
                        |> ignore
                )
                .ConfigureAppConfiguration(
                    fun config ->
                        let config =
                            config
                                .AddEnvironmentVariables()
                                .AddJsonFile ("appsettings.json", (* optional *) true, (* reloadOnChange *) false)
                        match maybeTestDataSeeding with
                        | Some _ -> "appsettings.TestDataSeeding.json"
                        | None -> "appsettings.Tests.json"
                        |> fun testConfigFile -> config.AddJsonFile (testConfigFile, (* optional *) true, (* reloadOnChange *) false) |> ignore
                )

                .ConfigureServices(
                    fun context services ->
                        let ecosystem = ecosystemUnderTest.Value

                        match context.Configuration.GetSection("AppInsights").TryGetAndTryValidate<AppInsightsConfiguration>() with
                        | Some config ->
                            addTelemetryEx<TestAppInsightsOperationTracker> config.InstrumentationKey (sprintf "%s.Tests" ecosystem.Name) services (* developerMode *) true (* isWorkerService *) true
                        | None ->
                            services.AddSingleton<OperationTracker>(noopOperationTracker) |> ignore

                        services
                            .AddSingleton<HttpClient>(new HttpClient(new HttpClientHandler(UseCookies = false), Timeout = TimeSpan.FromSeconds 10.0))
                            .AddEcosystem(ecosystem, storageSetup ())
                            .AddSingleton<IBiosphereGrainProvider>(
                                fun serviceProvider ->
                                    let grainFactory = serviceProvider.GetRequiredService<IGrainFactory>()
                                    { new IBiosphereGrainProvider with
                                        member _.GetGrainFactory (ecosystemName: string) =
                                            // referenced life cycles must be co-hosted in tests
                                            Task.FromResult grainFactory
                                        member _.IsHostedLifeCycle (_lcKey: LifeCycleKey) = true
                                        member _.Close () = Task.CompletedTask })

                            .AddSingleton<IReminderHook>(reminderHook)
                            .AddSingleton(slowDebugValueSummarizers)
                            // .. or Mix of codec-based and slow summarizers
                            // .AddSingleton(mixedCodecBasedAndFastSummarizersForEcosystem ecosystem (Some defaultSlowSummarizer))
                            // .. or same as in prod if you need to debug it
                            // .AddSingleton(mixedCodecBasedAndFastSummarizersForEcosystem ecosystem None)
                            .AddScoped<Service<Clock>>(
                                fun serviceProvider ->
                                    let grainPartition = serviceProvider.GetRequiredService<GrainPartition>()
                                    createService "System.Clock" (testClockHandler grainPartition)
                            )
                            .AddScoped<ISideEffectTrackerHook>(
                                fun serviceProvider ->
                                    let grainPartition = serviceProvider.GetRequiredService<GrainPartition>()
                                    TestSideEffectTrackerHook grainPartition)
                            .AddSingleton<IDataProtectionProvider>(globalSingletonDataProtectionProvider)
                            .AddDataProtection()
                            .SetApplicationName(null)
                            .Services
                        |> ignore

                        maybeTestDataSeeding
                        |> Option.iter (
                            fun x ->
                                x.ConfigureCustomStorage services

                                let section = context.Configuration.GetSection "TestDataSeeding.SqlServer.ConnectionStrings"
                                let connectionStrings =
                                    section.GetChildren ()
                                    |> Seq.map (fun s -> s.Key, s.Value)
                                    |> NonemptyMap.ofSeq
                                    |> function
                                        | None -> failwith "no connection strings found for data seeding"
                                        | Some byEcosystemName ->
                                            { ByEcosystemName = byEcosystemName }

                                // We need to setup the schema here, as a lot of other processes are
                                // dependent on the schema having been already created
                                SqlServerSetup.createSchema connectionStrings

                                services
                                    .AddSingleton<SqlServerConnectionStrings>(connectionStrings)
                                    .AddSingleton<TlsOptions>(
                                        fun serviceProvider ->
                                            let (tlsCertificate, hostName) =
                                                getOrleansTlsCertificateAndHostName ecosystem.Name (* useDevelopmentCertificate *) true

                                            let options = TlsOptions(LocalCertificate = tlsCertificate)
                                            options.OnAuthenticateAsClient <-
                                                fun _connection sslOptions ->
                                                    // Actual value doesn't matter, just required for SSL validation
                                                    sslOptions.TargetHost <- hostName

                                            options)
                                |> ignore)

                        configureTestServices
                        |> Option.iter (fun f -> f services)
                )
                |> ignore

type private TestSiloConfigurator() =
    interface ISiloConfigurator with
        member _.Configure (siloBuilder: ISiloBuilder) : unit =
            siloBuilder
                .ConfigureSiloForEcosystem(
                    ecosystemUnderTest.Value,
                    typeof<LibLifeCycleHostBuild.Build>.Assembly,
                        match maybeTestDataSeeding with
                        | None -> EcosystemSiloSetup.Test
                        | Some _ -> EcosystemSiloSetup.TestDataSeeding)

type private TestSiloClientConfigurator() =
    interface IClientBuilderConfigurator with
        member _.Configure(_configuration, clientBuilder) =
            let ecosystem = ecosystemUnderTest.Value
            clientBuilder
                .ConfigureSiloClientForEcosystem(
                    ecosystem,
                    typeof<LibLifeCycleHostBuild.Build>.Assembly,
                        match maybeTestDataSeeding with
                        | None -> EcosystemSiloClientSetup.TestCluster
                        | Some _ -> EcosystemSiloClientSetup.TestClusterDataSeeding)
                |> ignore

type TestClusterScope(autofacServiceProvider: AutofacServiceProvider) =
    interface IClusterScope with
        member _.ServiceProvider = autofacServiceProvider :> IServiceProvider

        member _.Dispose() =
            autofacServiceProvider.Dispose()

let private deadCluster =
    { new ICluster with
         member _.DisposeAsync() = ValueTask.CompletedTask
         member _.Init _ = failwith "No Test Clusters have been created. A TestCluster is automatically initialized when a simulation is initialized"
         member _.NewScope _ = failwith "No Test Clusters have been created. A TestCluster is automatically initialized when a simulation is initialized"
         member _.OperationTracker = failwith "No Test Clusters have been created. A TestCluster is automatically initialized when a simulation is initialized"
         member _.ShouldExecutePartitionInitializer = true
    }

let mutable testRunnerCluster = deadCluster

type RequestContextData(configOverride: Map<string, string>, userId: string) =
    do
        if configOverride.IsNonempty then
            Orleans.Runtime.RequestContext.Set(LibLifeCycleHost.HostExtensions.ConfigOverrideRequestContextKey, configOverride)

        if not (String.IsNullOrWhiteSpace userId) then
            LibLifeCycleHost.Web.OrleansRequestContext.setTelemetryUserIdAndSessionId (userId, None)

    interface IDisposable with
        member _.Dispose() =
            if configOverride.IsNonempty then
                Orleans.Runtime.RequestContext.Remove LibLifeCycleHost.HostExtensions.ConfigOverrideRequestContextKey
                |> ignore

            if not (String.IsNullOrWhiteSpace userId) then
                LibLifeCycleHost.Web.OrleansRequestContext.setTelemetryUserIdAndSessionId ("", None)

type TestCluster(ecosystem: Ecosystem, configureServices: IServiceCollection -> unit) as this =

    // create lazily to give upstream code chance to init maybeTestDataSeeding
    let createTestCluster () : Orleans.TestingHost.TestCluster =
        let initialSilosCount =
            match maybeTestDataSeeding with None -> 2s | Some _ -> 1s
        TestClusterBuilder(initialSilosCount)
            .AddSiloBuilderConfigurator<TestHostConfigurator>()
            .AddSiloBuilderConfigurator<TestSiloConfigurator>()
            .AddClientBuilderConfigurator<TestSiloClientConfigurator>()
            .Build()

    let mutable clusterDeployTask: Option<Task> = None
    let mutable maybeCluster: Option<Orleans.TestingHost.TestCluster> = None

    do
        lock ecosystemInitLockObj (fun _ ->
            if testRunnerCluster <> deadCluster then
                failwith "Only one TestCluser can be initialized"
            else
                testRunnerCluster <- this :> ICluster
        )

    let siloHostServices (cluster: Orleans.TestingHost.TestCluster) =
        let siloHandle = cluster.Primary :?> Orleans.TestingHost.InProcessSiloHandle
        siloHandle.SiloHost.Services

    interface ICluster with
        member _.ShouldExecutePartitionInitializer = true

        member _.Init logOutput =
            lock ecosystemInitLockObj (fun _ ->
                if ecosystemUnderTest.IsSome || configureTestServices.IsSome then
                    task {
                        if (not clusterDeployTask.Value.IsCompleted) then
                            logOutput (sprintf "Cluster is already deploying, waiting ... %A" DateTimeOffset.Now)
                        do! clusterDeployTask.Value
                    }
                else
                    logOutput (sprintf "Creating & deploying cluster ... %A" DateTimeOffset.Now)
                    let cluster = createTestCluster ()
                    maybeCluster <- Some cluster
                    ecosystemDefUnderTest <- Some ecosystem.Def
                    ecosystemUnderTest <-
                        ecosystem
                        |> interceptAllEcosystemConnectors
                        |> Some

                    configureTestServices <- Some configureServices
                    clusterDeployTask <- Some (cluster.DeployAsync())
                    task {
                        do! clusterDeployTask.Value
                        logOutput (sprintf "Deployed cluster %A" DateTimeOffset.Now)
                    }
            )
            |> Task.asUnit

        member _.NewScope (testPartition: TestPartition) =
            match maybeCluster with
            | Some cluster ->
                if cluster.Primary = null then
                    failwith "TestCluster.Init has not been called"
                let lifetimeScope = (siloHostServices cluster).GetRequiredService<ILifetimeScope>()
                let childScope = lifetimeScope.BeginLifetimeScope(fun builder ->
                    builder.Register(fun _ -> new RequestContextData(testPartition.ConfigOverrides, testPartition.UserId))
                        .As<RequestContextData>()
                        .InstancePerLifetimeScope()
                        .AutoActivate() |> ignore
                    builder.Register(fun _ -> testPartition.GrainPartition).As<GrainPartition>().InstancePerLifetimeScope() |> ignore
                )

                new TestClusterScope(new AutofacServiceProvider(childScope))
                :> IClusterScope
            | None ->
                failwith "Test Cluster is not created"

        member this.OperationTracker =
            match maybeCluster with
            | Some cluster ->
                (siloHostServices cluster).GetRequiredService<OperationTracker>()
            | None ->
                failwith "Test Cluster is not created"

        member this.DisposeAsync () : ValueTask =
            match maybeCluster with
            | Some cluster ->
                backgroundTask {
                    let operationTracker = (this :> ICluster).OperationTracker
                    do! operationTracker.Shutdown()
                    do! cluster.StopAllSilosAsync()
                }
                |> ValueTask
            | None ->
                ValueTask.CompletedTask