namespace LibLifeCycleCore

open System
open System.Reflection
open System.Threading
open System.Threading.Tasks
open System.Transactions
open LibLifeCycleCore.OrleansEx.TraceContextGrainCallFilter
open Orleans
open Orleans.Configuration
open Orleans.Messaging
open Orleans.Runtime.Membership
open LibLifeCycleCore
open LibLifeCycleCore.Certificates
open Microsoft.Extensions.DependencyInjection
open LibLifeCycleCore.OrleansEx

// need this for external native clients, to avoid escalation to distributed transaction
// No need to use within host because it doesn't create a TransactionScope
type private AdoNetGatewayListProviderWithSuppressedTransaction (adoNetProvider: AdoNetGatewayListProvider) =
    interface IGatewayListProvider with
        member _.GetGateways() : Task<_> =
            backgroundTask {
                use _ = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled)
                return! adoNetProvider.GetGateways()
            }

        member _.InitializeGatewayListProvider() : Task =
            backgroundTask {
                use _ = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled)
                return! adoNetProvider.InitializeGatewayListProvider()
            }

        member _.IsUpdatable : bool = adoNetProvider.IsUpdatable

        member _.MaxStaleness = adoNetProvider.MaxStaleness

module private Common =
    let createGrainConnector (serviceProvider: IServiceProvider) =
        let grainFactory = serviceProvider.GetRequiredService<IGrainFactory>()
        let grainPartition = serviceProvider.GetRequiredService<GrainPartition>()
        GrainConnector(grainFactory, grainPartition, SessionHandle.NoSession, CallOrigin.Internal)


type GrainConnectorProvider =
    inherit IDisposable
    abstract member CreateGrainConnector : unit -> GrainConnector


type AdoNetClusteringGrainConnectorProvider
    (ecosystemDef: EcosystemDef,
        membershipConnectionString: string,
        retryFilter: exn -> Task<bool>,
        useDevelopmentCertificate: bool,
        buildAssembly: Assembly) =

    let cancellationTokenSource = new CancellationTokenSource()

    let serviceProvider =
        ServiceCollection()
            .AddSingleton<IGrainFactory>(
                fun _ ->
                    let orleansTlsCertificate, orleansHostName =
                        getOrleansTlsCertificateAndHostName ecosystemDef.Name useDevelopmentCertificate
                    let client =
                      (ClientBuilder() :> IClientBuilder)
                        .UseTls(orleansTlsCertificate,
                            fun options ->
                                options.OnAuthenticateAsClient <-
                                    fun _connection sslOptions ->
                                        // Actual value doesn't matter, just required for SSL validation
                                        sslOptions.TargetHost <- orleansHostName)
                        .ConfigureServices(fun services ->
                            // decompiled UseAdoNetClustering with wrapped gateway lister
                            services.Configure(
                                fun (adoNetOptions: AdoNetClusteringClientOptions) ->
                                    adoNetOptions.ConnectionString <- membershipConnectionString
                                    adoNetOptions.Invariant        <- "Microsoft.Data.SqlClient")
                            |> ignore
                            services.AddSingleton<AdoNetGatewayListProvider, AdoNetGatewayListProvider>() |> ignore
                            services.AddSingleton<IGatewayListProvider, AdoNetGatewayListProviderWithSuppressedTransaction>() |> ignore
                            services.AddSingleton<IConfigurationValidator, AdoNetClusteringClientOptionsValidator>() |> ignore)
                        .Configure<ClusterOptions>(
                            fun (opts: ClusterOptions) ->
                                opts.ClusterId <- ecosystemDef.Name
                                opts.ServiceId <- ecosystemDef.Name
                        )
                        .ConfigureServices(Serialization.registerSerializers ecosystemDef.LifeCycleDefs ecosystemDef.ViewDefs)
                        .ConfigureApplicationParts(fun parts -> Serialization.configureApplicationParts ecosystemDef.LifeCycleDefs ecosystemDef.ViewDefs parts buildAssembly)
                        .AddOutgoingGrainCallFilter<TraceContextOutgoingGrainCallFilter>()
                        .Build()
                    client
                        .Connect(
                            fun exnBeforeRetry ->
                                if cancellationTokenSource.Token.IsCancellationRequested then
                                    Task.FromResult false
                                else
                                    retryFilter exnBeforeRetry)
                        .ConfigureAwait(false)
                        .GetAwaiter()
                        .GetResult()

                    client :> IGrainFactory)
            .AddScoped<GrainPartition>(fun _ -> defaultGrainPartition)
            .BuildServiceProvider()
            :> IServiceProvider

    interface IDisposable with
        member this.Dispose() =
            cancellationTokenSource.Cancel()

    interface GrainConnectorProvider with
        member _.CreateGrainConnector() = Common.createGrainConnector serviceProvider