[<AutoOpen>]
module LibLifeCycleHost.HostExtensions

open System.Reflection
open System.Runtime.CompilerServices
open System.Threading
open System.Threading.Tasks
open LibLifeCycle.Connectors.ViewConnector
open LibLifeCycle.LifeCycles.Meta
open LibLifeCycle.LifeCycles.Startup
open LibLifeCycleHost
open LibLifeCycleHost.Storage.SqlServer.SqlServerTransferBlobHandler
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open FSharp.Reflection
open LibLifeCycle
open LibLifeCycle.Caching
open LibLifeCycleCore
open LibLifeCycleHost.Storage.Volatile
open LibLifeCycleHost.Storage.Test
open LibLifeCycleHost.Storage.SqlServer
open Orleans
open Orleans.Runtime
open System
open Microsoft.AspNetCore.DataProtection
open Microsoft.Extensions.Logging
open LibLifeCycleHost.Storage.SqlServer.SqlServerGrainStorageHandler
open LibLifeCycleHost.Storage.SqlServer.SqlServerTimeSeriesStorageHandler
open LibLifeCycle.MetaServices
open LibLifeCycleHost.MetaServices
open LibLifeCycleHost.Web
open System.Collections.Concurrent

type private SequenceGenerator (storageHandler: IGrainStorageHandler) =
    let validateSequenceName (name: string) =
        if not (String.IsNullOrEmpty name) && (name |> Seq.forall (fun c -> Char.IsLetterOrDigit c || c = '_')) then
            ()
        else
            failwithf "Sequence name allows only letters, digits and underscores: %s" name

    interface ISequence with
        member _.GetNext name =
            validateSequenceName name
            storageHandler.GetNextSequenceNumber name

        member _.PeekCurrent name =
            validateSequenceName name
            storageHandler.PeekCurrentSequenceNumber name

type private ActionContextImpl() =
    interface IActionContext with
        member _.GetUserId() : string =
            // TODO: can pass explicitly like CurrentSubjectRef
            let userId, _ = OrleansRequestContext.getTelemetryUserIdAndSessionId ()
            userId

        member _.GetSessionId() : Option<string> =
            OrleansRequestContext.getTelemetryUserIdAndSessionId ()
            |> snd

type private BiosphereBlobRepo (grainProvider: IBiosphereGrainProvider, grainPartition: GrainPartition) =
    member private _.MakeGrainConnector (ecosystemName: string) =
        backgroundTask {
            let! grainFactory = grainProvider.GetGrainFactory ecosystemName
            return GrainConnector (grainFactory, grainPartition, SessionHandle.NoSession, CallOrigin.Internal)
        }

    member private this.FetchBlobData (ecosystemName: string) (subjectRef: LocalSubjectPKeyReference) (blobId: Guid) : Task<Option<BlobData>> =
        fun () -> backgroundTask {
            let! grainConnector = this.MakeGrainConnector ecosystemName
            return! grainConnector.BlobData subjectRef blobId
        }
        |> OrleansTransientErrorDetection.wrapTransientExceptions

    interface IBlobRepo with
        member this.GetBlobData (ecosystemName: string) (subjectRef: LocalSubjectPKeyReference) (blobId: Guid) : Task<Option<BlobData>> =
            this.FetchBlobData ecosystemName subjectRef blobId

        // TODO: Implement proper blob streaming for biosphere
        member this.GetBlobDataStream (ecosystemName: string) (subjectRef: LocalSubjectPKeyReference) (blobId: Guid) (readBlobDataStream: Option<BlobDataStream> -> Task) : Task = backgroundTask {
            match! this.FetchBlobData ecosystemName subjectRef blobId with
            | None ->
                return! readBlobDataStream None
            | Some blobData ->
                use memoryStream = new System.IO.MemoryStream(blobData.Data, writable = false)
                return! readBlobDataStream (Some { TotalBytes = blobData.Data.Length; Stream = memoryStream; MimeType = blobData.MimeType })
        }

type private RouterBlobRepo (
    biosphereGrainProvider: IBiosphereGrainProvider,
    hostedLifeCyclesStorageType: Map<LifeCycleKey, StorageType>,
    sqlBlobRepo: SqlServerSubjectBlobRepo,
    volatileBlobRepo: VolatileSubjectBlobRepo,
    biosphereBlobRepo: BiosphereBlobRepo) =
    member private _.SelectBlobRepo (lcKey: LifeCycleKey) : IBlobRepo =
        if biosphereGrainProvider.IsHostedLifeCycle lcKey then
            match hostedLifeCyclesStorageType.TryFind lcKey with
            | Some (StorageType.Persistent _) -> sqlBlobRepo      :> IBlobRepo
            | Some StorageType.Volatile       -> volatileBlobRepo :> IBlobRepo
            | Some (StorageType.Custom _) ->
                raise (NotImplementedException(sprintf "Blob custom storage not available yet: %A" lcKey))
            | None ->
                raise (InvalidOperationException(sprintf "BlobId of unknown lifecycle: %A" lcKey))
        else
            biosphereBlobRepo :> IBlobRepo

    interface IBlobRepo with
        member this.GetBlobData (ecosystemName: string) (subjectRef: LocalSubjectPKeyReference) (blobId: Guid) : Task<Option<BlobData>> =
            let lcKey = LifeCycleKey.LifeCycleKey(subjectRef.LifeCycleName, ecosystemName)
            let repo = this.SelectBlobRepo lcKey
            repo.GetBlobData ecosystemName subjectRef blobId

        member this.GetBlobDataStream (ecosystemName: string) (subjectRef: LocalSubjectPKeyReference) (blobId: Guid) (readBlobDataStream: Option<BlobDataStream> -> Task) : Task =
            let lcKey = LifeCycleKey.LifeCycleKey(subjectRef.LifeCycleName, ecosystemName)
            let repo = this.SelectBlobRepo lcKey
            repo.GetBlobDataStream ecosystemName subjectRef blobId readBlobDataStream

type private BiosphereSubjectRepo<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId
                   when 'Subject              :> Subject<'SubjectId>
                   and  'LifeAction           :> LifeAction
                   and  'OpError              :> OpError
                   and  'Constructor          :> Constructor
                   and  'LifeEvent            :> LifeEvent
                   and  'LifeEvent            : comparison
                   and  'SubjectIndex         :> SubjectIndex<'OpError>
                   and  'SubjectId            :> SubjectId
                   and  'SubjectId            : comparison>
    (
        lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>,
        grainProvider: IBiosphereGrainProvider,
        grainPartition: GrainPartition
    ) =
    let ecosystemName = lifeCycleDef.Key.EcosystemName

    member private _.MakeGrainConnector () =
        backgroundTask {
            let! grainFactory = grainProvider.GetGrainFactory ecosystemName
            return GrainConnector (grainFactory, grainPartition, SessionHandle.NoSession, CallOrigin.Internal)
        }

    interface ISubjectRepo<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'SubjectIndex, 'OpError> with

        member this.DoesExistById (id: 'SubjectId): Task<bool> =
            fun () -> backgroundTask {
                let! grainConnector = this.MakeGrainConnector ()
                return! grainConnector.DoesSubjectExist lifeCycleDef id }
            |> OrleansTransientErrorDetection.wrapTransientExceptions

        member this.GetById (id: 'SubjectId): Task<Option<VersionedSubject<'Subject, 'SubjectId>>> =
            fun () -> backgroundTask {
                let! grainConnector = this.MakeGrainConnector ()
                match! grainConnector.GetById lifeCycleDef id with
                | Ok res ->
                    return res
                | Error err ->
                    return failwith $"Biosphere repo GetById failed with: {err}" }
            |> OrleansTransientErrorDetection.wrapTransientExceptions

        member this.GetByIdStr (idStr: string): Task<Option<VersionedSubject<'Subject, 'SubjectId>>> =
            fun () -> backgroundTask {
                let! grainConnector = this.MakeGrainConnector ()
                match! grainConnector.GetByIdStr lifeCycleDef idStr with
                | Ok res ->
                    return res
                | Error err ->
                    return failwith $"Biosphere repo GetByIdStr failed with: {err}" }
            |> OrleansTransientErrorDetection.wrapTransientExceptions

        member this.GetByIds (ids: Set<'SubjectId>): Task<List<VersionedSubject<'Subject, 'SubjectId>>> =
            fun () -> backgroundTask {
                let! grainConnector = this.MakeGrainConnector ()
                return! grainConnector.GetByIds lifeCycleDef ids }
            |> OrleansTransientErrorDetection.wrapTransientExceptions

        member this.GetByIdsStr (ids: Set<string>): Task<List<VersionedSubject<'Subject, 'SubjectId>>> =
            fun () -> backgroundTask {
                let! grainConnector = this.MakeGrainConnector ()
                return! grainConnector.GetByIdsStr lifeCycleDef ids }
            |> OrleansTransientErrorDetection.wrapTransientExceptions

        member this.Any(predicate: PreparedIndexPredicate<'SubjectIndex>): Task<bool> =
            fun () -> backgroundTask {
                let! grainConnector = this.MakeGrainConnector ()
                return! grainConnector.Any lifeCycleDef predicate }
            |> OrleansTransientErrorDetection.wrapTransientExceptions

        member this.FilterFetchIds (query: IndexQuery<'SubjectIndex>) : Task<List<'SubjectId>> =
            fun () -> backgroundTask {
                let! grainConnector = this.MakeGrainConnector ()
                return! grainConnector.FilterFetchIds lifeCycleDef query }
            |> OrleansTransientErrorDetection.wrapTransientExceptions

        member this.FilterFetchSubjects (query: IndexQuery<'SubjectIndex>) : Task<List<VersionedSubject<'Subject, 'SubjectId>>> =
            fun () -> backgroundTask {
                let! grainConnector = this.MakeGrainConnector ()
                return! grainConnector.FilterFetchSubjects lifeCycleDef query }
            |> OrleansTransientErrorDetection.wrapTransientExceptions

        member this.FilterFetchSubjectsWithTotalCount(query: IndexQuery<'SubjectIndex>) : Task<List<VersionedSubject<'Subject, 'SubjectId>> * uint64> =
            fun () -> backgroundTask {
                let! grainConnector = this.MakeGrainConnector ()
                return! grainConnector.FilterFetchSubjectsWithTotalCount lifeCycleDef query }
            |> OrleansTransientErrorDetection.wrapTransientExceptions

        member this.FilterCountSubjects (predicate: PreparedIndexPredicate<'SubjectIndex>) : Task<uint64> =
            fun () -> backgroundTask {
                let! grainConnector = this.MakeGrainConnector ()
                return! grainConnector.FilterCountSubjects lifeCycleDef predicate }
            |> OrleansTransientErrorDetection.wrapTransientExceptions

        member this.FetchAllSubjects (resultSetOptions: ResultSetOptions<'SubjectIndex>): Task<List<VersionedSubject<'Subject, 'SubjectId>>> =
            fun () -> backgroundTask {
                let! grainConnector = this.MakeGrainConnector ()
                return! grainConnector.FilterFetchAllSubjects lifeCycleDef resultSetOptions }
            |> OrleansTransientErrorDetection.wrapTransientExceptions

        member this.CountAllSubjects (): Task<uint64> =
            fun () -> backgroundTask {
                let! grainConnector = this.MakeGrainConnector ()
                return! grainConnector.CountAllSubjects lifeCycleDef }
            |> OrleansTransientErrorDetection.wrapTransientExceptions

        member this.FetchAllSubjectsWithTotalCount (resultSetOptions: ResultSetOptions<'SubjectIndex>): Task<List<VersionedSubject<'Subject, 'SubjectId>> * uint64> =
            fun () -> backgroundTask {
                let! grainConnector = this.MakeGrainConnector ()
                return! grainConnector.FetchAllSubjectsWithTotalCount lifeCycleDef resultSetOptions }
            |> OrleansTransientErrorDetection.wrapTransientExceptions

        member this.GetVersionSnapshotByIdStr (idStr: string) (ofVersion: GetSnapshotOfVersion) : Task<Option<TemporalSnapshot<'Subject, 'LifeAction, 'Constructor, 'SubjectId>>> =
            fun () -> backgroundTask {
                let! grainConnector = this.MakeGrainConnector ()
                return! grainConnector.GetSubjectVersionSnapshot lifeCycleDef idStr ofVersion }
            |> OrleansTransientErrorDetection.wrapTransientExceptions

        member this.GetVersionSnapshotById (id: 'SubjectId) (ofVersion: GetSnapshotOfVersion) : Task<Option<TemporalSnapshot<'Subject, 'LifeAction, 'Constructor, 'SubjectId>>> =
            fun () -> backgroundTask {
                let! grainConnector = this.MakeGrainConnector ()
                return! grainConnector.GetSubjectVersionSnapshotById lifeCycleDef id ofVersion }
            |> OrleansTransientErrorDetection.wrapTransientExceptions

        member this.FetchWithHistoryById (id: 'SubjectId) (fromLastUpdatedOn: Option<DateTimeOffset>) (toLastUpdatedOn: Option<DateTimeOffset>) (page: ResultPage): Task<List<TemporalSnapshot<'Subject, 'LifeAction, 'Constructor, 'SubjectId>>> =
            fun () -> backgroundTask {
                let! grainConnector = this.MakeGrainConnector ()
                return! grainConnector.FetchWithHistoryByIdStr lifeCycleDef id.IdString fromLastUpdatedOn toLastUpdatedOn page }
            |> OrleansTransientErrorDetection.wrapTransientExceptions

        member this.FetchWithHistoryByIdStr (idStr: string) (fromLastUpdatedOn: Option<DateTimeOffset>) (toLastUpdatedOn: Option<DateTimeOffset>) (page: ResultPage): Task<List<TemporalSnapshot<'Subject, 'LifeAction, 'Constructor, 'SubjectId>>> =
            fun () -> backgroundTask {
                let! grainConnector = this.MakeGrainConnector ()
                return! grainConnector.FetchWithHistoryByIdStr lifeCycleDef idStr fromLastUpdatedOn toLastUpdatedOn page }
            |> OrleansTransientErrorDetection.wrapTransientExceptions

        member _.GetSideEffectPermanentFailures (_scope: UpdatePermanentFailuresScope): Task<List<SideEffectPermanentFailure>> =
            NotImplementedException "Biosphere repo doesn't implement maintenance of side effect failures" |> raise

        member this.FetchAuditTrail (idStr: string) (page: ResultPage) : Task<List<SubjectAuditData<'LifeAction, 'Constructor>>> =
            fun () -> backgroundTask {
                let! grainConnector = this.MakeGrainConnector ()
                return! grainConnector.FetchAuditTrail lifeCycleDef idStr page }
            |> OrleansTransientErrorDetection.wrapTransientExceptions

let private overrideConfigurationCache = ConcurrentDictionary<Map<string, string>, IConfiguration>() // OverrideConfig => Global Config + Overriden Config

[<Literal>]
let ConfigOverrideRequestContextKey = "ConfigOverrides"

let private registerEnvironmentFactoryGeneric<'Env when 'Env :> Env> (services: IServiceCollection) =
    let envType = typeof<'Env>

    if envType = typeof<NoEnvironment> then
        let factory: EnvironmentFactory<NoEnvironment> = Func<CallScopedEnvDependencies, NoEnvironment>(fun _ -> noEnvironment)
        services.AddSingleton(factory) |> ignore
    else
        let envPropertyInfos =
            FSharpType.GetRecordFields (envType, BindingFlags.NonPublic ||| BindingFlags.Public)
        let environmentConstructor =
            FSharpValue.PreComputeRecordConstructor (envType, BindingFlags.NonPublic ||| BindingFlags.Public)

        // Register environment dependencies
        envPropertyInfos
        |> Seq.iter (fun propertyInfo ->
            match propertyInfo.PropertyType.GetCustomAttribute<RegisterScopedDependencyAttribute>() |> Option.ofObj with
            | None ->
                match propertyInfo.PropertyType.GetCustomAttribute<RegisterSingletonDependencyAttribute>() |> Option.ofObj with
                | None ->
                    ()
                | Some _ ->
                    services.AddSingleton(propertyInfo.PropertyType) |> ignore
            | Some _ ->
                services.AddScoped(propertyInfo.PropertyType) |> ignore
        )

        let createFactory (serviceProvider: IServiceProvider): EnvironmentFactory<'Env> =
            EnvironmentFactory<'Env>(
                fun callScopedEnvDependencies ->
                    envPropertyInfos
                    |> Seq.map (
                        fun propertyInfo ->
                            match propertyInfo.PropertyType with
                            | t when t = typeof<CallOrigin> ->
                                let callOrigin: CallOrigin = callScopedEnvDependencies.CallOrigin
                                box callOrigin

                            | t when t = typeof<Service<ActionContext>> ->
                                let service: Service<ActionContext> =
                                    createService "System.ActionContext" (actionContextHandler (ActionContextImpl() :> IActionContext) callScopedEnvDependencies.LocalSubjectRef)
                                box service

                            | t when t.IsAssignableTo typeof<Env> ->
                                typeof<LibLifeCycle.LifeCycle.AnchorTypeForModule>.DeclaringType.GetMethod(nameof createEnvImpl, BindingFlags.Static ||| BindingFlags.NonPublic)
                                    .MakeGenericMethod([| t |])
                                    .Invoke(null, [| callScopedEnvDependencies; serviceProvider |])

                            | t when t = typeof<IConfiguration> ->
                                let globalConfig = serviceProvider.GetRequiredService<IConfiguration>()

                                // override config within grains, currently only used for testing
                                RequestContext.Get ConfigOverrideRequestContextKey
                                |> Option.ofObj
                                |> Option.map (fun obj -> obj :?> Map<string, string>)
                                |> Option.map (fun overrides ->
                                    overrideConfigurationCache.GetOrAdd(overrides, fun _ ->
                                        (new ConfigurationManager())
                                            .AddConfiguration(globalConfig)
                                            .AddInMemoryCollection(
                                                overrides
                                                |> Map.toSeq
                                                |> Seq.map (System.Collections.Generic.KeyValuePair)
                                                |> Seq.toArray)
                                            .Build() :> IConfiguration
                                        ))
                                |> Option.defaultValue globalConfig
                                |> box

                            | t -> serviceProvider.GetRequiredService t
                        )
                    |> Seq.toArray
                    |> environmentConstructor
                     :?> 'Env
            )

        services.AddScoped<EnvironmentFactory<'Env>>(Func<IServiceProvider, EnvironmentFactory<'Env>>(createFactory))
        |> ignore

[<RequireQualifiedAccess>]
type EcosystemStorageSetup =
| Proper
| Test
| TestDataSeeding

let private getAllOfTService (serviceNameQualifier: string) (repo: ISubjectRepo<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'SubjectIndex, 'OpError>) (cache: InMemoryCache): Service<All<'Subject, 'SubjectId, 'SubjectIndex, 'OpError>> =
    createService (sprintf "All<%s>" serviceNameQualifier) (allSubjectsServiceHandler repo cache)

let private getAllTimeSeriesOfTService (serviceNameQualifier: string) (repo: ITimeSeriesRepo<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'TimeSeriesIndex>): Service<AllTimeSeries<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'TimeSeriesIndex>> =
    createService (sprintf "AllTimeSeries<%s>" serviceNameQualifier) (allTimeSeriesServiceHandler repo)

let getInMemoryCache (storageSetup: EcosystemStorageSetup) (serviceProvider: IServiceProvider) : InMemoryCache =
    match storageSetup with
    | EcosystemStorageSetup.Proper ->
        serviceProvider.GetRequiredService<DotnetInMemoryCache>() :> InMemoryCache
    | EcosystemStorageSetup.TestDataSeeding
    | EcosystemStorageSetup.Test ->
        serviceProvider.GetRequiredService<NonCachingInMemoryCache>() :> InMemoryCache

let private registerLifeCycle
    (services: IServiceCollection)
    (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env>)
    (storageSetup: EcosystemStorageSetup)
    : HostedLifeCycleAdapter<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId> =

    let lifeCycleAdapter =
        HostedLifeCycleAdapter<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId> lifeCycle

    services.AddSingleton(lifeCycleAdapter)
    |> ignore

    // FIXME, we need a better framework for pluggability of storage providers
    // We should be able to configure from the outside what the persistent and volatile providers are, and be able to
    // override Custom providers for Test. Once we have a better framework, we can move the Test providers to the Test library

    let subjectGrainRepoImplementationFactory (serviceProvider: IServiceProvider) : ISubjectRepo<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'SubjectIndex, 'OpError> =
        match lifeCycleAdapter.LifeCycle.Storage.Type, storageSetup with
        | StorageType.Persistent _, EcosystemStorageSetup.Proper
        | StorageType.Persistent _, EcosystemStorageSetup.TestDataSeeding ->
            let config = serviceProvider.GetRequiredService<SqlServerConnectionStrings>()
            SqlServerSubjectRepo<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>(
                lifeCycleAdapter, config)
            :> ISubjectRepo<_, _, _, _, _, _>

        | StorageType.Persistent _, EcosystemStorageSetup.Test ->
            let grainPartition = serviceProvider.GetRequiredService<GrainPartition>()
            TestSubjectRepo<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>(lifeCycle.Indices, grainPartition)
            :> ISubjectRepo<_, _, _, _, _, _>

        | StorageType.Volatile, _ ->
            let hostEcosystemGrainFactory = serviceProvider.GetRequiredService<IGrainFactory>()
            let grainPartition = serviceProvider.GetRequiredService<GrainPartition>()
            VolatileSubjectRepo<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>(hostEcosystemGrainFactory, grainPartition)
            :> ISubjectRepo<_, _, _, _, _, _>

        | StorageType.Custom key, EcosystemStorageSetup.Proper
        | StorageType.Custom key, EcosystemStorageSetup.TestDataSeeding ->
            let subjectRepos = serviceProvider.GetRequiredService<CustomStorageHandlers>()
            match subjectRepos.GetSubjectRepo<ISubjectRepo<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'SubjectIndex, 'OpError>> key serviceProvider with
            | None -> failwithf "Subject repo not found: %s" key
            | Some s -> s

        | StorageType.Custom _, EcosystemStorageSetup.Test ->
            let grainPartition = serviceProvider.GetRequiredService<GrainPartition>()
            TestSubjectRepo<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>(lifeCycle.Indices, grainPartition)
            :> ISubjectRepo<_, _, _, _, _, _>

    let subjectGrainStorageImplementationFactory (serviceProvider: IServiceProvider) : IGrainStorageHandler<'Subject, 'LifeAction, 'Constructor, 'LifeEvent, 'SubjectId, 'OpError> =
        match lifeCycleAdapter.LifeCycle.Storage.Type, storageSetup with
        | StorageType.Persistent _, EcosystemStorageSetup.Proper
        | StorageType.Persistent _, EcosystemStorageSetup.TestDataSeeding ->
            let config = serviceProvider.GetRequiredService<SqlServerConnectionStrings>()
            let allTransferBlobHandlers = serviceProvider.GetServices<ITransferBlobHandler>()
            let timeSeriesAdapters = serviceProvider.GetRequiredService<TimeSeriesAdapterCollection>()
            let sqlLogger = serviceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<SqlServerGrainStorageHandler<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>>>()
            let remindersTriggeredManually = (storageSetup = EcosystemStorageSetup.TestDataSeeding)
            SqlServerGrainStorageHandler<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>(
                (Seq.toList allTransferBlobHandlers), lifeCycleAdapter, timeSeriesAdapters, config, sqlLogger, remindersTriggeredManually
            )
            :> IGrainStorageHandler<'Subject, 'LifeAction, 'Constructor, 'LifeEvent, 'SubjectId, 'OpError>

        | StorageType.Persistent _, EcosystemStorageSetup.Test ->
                let logger = serviceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<TestGrainStorageHandler<'Subject, 'LifeAction, 'Constructor, 'LifeEvent, 'SubjectId, 'OpError>>>()
                let grainPartition = serviceProvider.GetRequiredService<GrainPartition>()
                let blobStorage = serviceProvider.GetRequiredService<TestBlobStorage>()
                let timeSeriesAdapters = serviceProvider.GetRequiredService<TimeSeriesAdapterCollection>()
                TestGrainStorageHandler<'Subject, 'LifeAction, 'Constructor, 'LifeEvent, 'SubjectId, 'OpError>(logger, blobStorage, timeSeriesAdapters, grainPartition)
                :> IGrainStorageHandler<'Subject, 'LifeAction, 'Constructor, 'LifeEvent, 'SubjectId, 'OpError>

        | StorageType.Volatile, EcosystemStorageSetup.Test
        | StorageType.Volatile, EcosystemStorageSetup.TestDataSeeding ->
            VolatileGrainStorageHandler<'Subject, 'LifeAction, 'Constructor, 'LifeEvent, 'SubjectId, 'OpError>((* remindersTriggeredManually *) true)
            :> IGrainStorageHandler<'Subject, 'LifeAction, 'Constructor, 'LifeEvent, 'SubjectId, 'OpError>

        | StorageType.Volatile, EcosystemStorageSetup.Proper ->
            VolatileGrainStorageHandler<'Subject, 'LifeAction, 'Constructor, 'LifeEvent, 'SubjectId, 'OpError>((* remindersTriggeredManually *) false)
            :> IGrainStorageHandler<'Subject, 'LifeAction, 'Constructor, 'LifeEvent, 'SubjectId, 'OpError>

        | StorageType.Custom key, EcosystemStorageSetup.Proper
        | StorageType.Custom key, EcosystemStorageSetup.TestDataSeeding -> // assume subject with custom storage doesn't have reminders
            let storageHandlers = serviceProvider.GetRequiredService<CustomStorageHandlers>()
            match storageHandlers.GetCustomStorage<IGrainStorageHandler<'Subject, 'LifeAction, 'Constructor, 'LifeEvent, 'SubjectId, 'OpError>> key serviceProvider with
            | None -> failwithf "Storage handler not found: %s" key
            | Some s -> s

        | StorageType.Custom _, EcosystemStorageSetup.Test ->
            let logger = serviceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<TestGrainStorageHandler<'Subject, 'LifeAction, 'Constructor, 'LifeEvent, 'SubjectId, 'OpError>>>()
            let grainPartition = serviceProvider.GetRequiredService<GrainPartition>()
            let blobStorage = serviceProvider.GetRequiredService<TestBlobStorage>()
            let timeSeriesAdapters = serviceProvider.GetRequiredService<TimeSeriesAdapterCollection>()
            TestGrainStorageHandler<'Subject, 'LifeAction, 'Constructor, 'LifeEvent, 'SubjectId, 'OpError>(logger, blobStorage, timeSeriesAdapters, grainPartition)
            :> IGrainStorageHandler<'Subject, 'LifeAction, 'Constructor, 'LifeEvent, 'SubjectId, 'OpError>

    let allServiceType = typeof<Service<All<'Subject, 'SubjectId, 'SubjectIndex, 'OpError>>>
    if services |> Seq.exists (fun s -> s.ServiceType = allServiceType) then
        failwith $"Ambiguous All<> service can't be registered. Do you have similar/ambiguous life cycle definitions in hosted and referenced ecosystems? {allServiceType.FullName}"

    services
        // Register the adapter itself as singleton
        .AddSingleton(lifeCycleAdapter)
        .AddSingleton(DotnetInMemoryCache())
        .AddSingleton(NonCachingInMemoryCache())
        // Register the Grain Repo for this subject
        .AddScoped<ISubjectRepo<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'SubjectIndex, 'OpError>>(fun serviceProvider -> subjectGrainRepoImplementationFactory serviceProvider)
        .AddScoped<IGrainStorageHandler<'Subject, 'LifeAction, 'Constructor, 'LifeEvent, 'SubjectId, 'OpError>>(fun serviceProvider -> subjectGrainStorageImplementationFactory serviceProvider)
        // Register a service for All<'Subject>
        .AddScoped<Service<All<'Subject, 'SubjectId, 'SubjectIndex, 'OpError>>>(
            fun serviceProvider ->
                let subjectRepo = serviceProvider.GetRequiredService<ISubjectRepo<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'SubjectIndex, 'OpError>>()
                let cache =       getInMemoryCache storageSetup serviceProvider

                getAllOfTService (* serviceNameQualifier *) lifeCycleAdapter.LifeCycle.Name subjectRepo cache
        )
        .AddScoped<Service<Sequence>>(
            fun serviceProvider ->
                let storageHandler =
                    serviceProvider.GetRequiredService<IGrainStorageHandler<'Subject, 'LifeAction, 'Constructor, 'LifeEvent, 'SubjectId, 'OpError>>()
                    :> IGrainStorageHandler
                createService "System.Sequence" (sequenceHandler (SequenceGenerator storageHandler :> ISequence)))
        |> registerEnvironmentFactoryGeneric<'Env>

    lifeCycleAdapter


let private registerUntypedLifeCycle (services: IServiceCollection) (lifeCycle: ILifeCycle) (storageSetup: EcosystemStorageSetup) =
    lifeCycle.Invoke
        { new FullyTypedLifeCycleFunction<_> with
            member _.Invoke lifeCycle = registerLifeCycle services lifeCycle storageSetup :> IHostedLifeCycleAdapter }

let private registerReferencedLifeCycle
        (services: IServiceCollection)
        (referencedLifeCycle: IReferencedLifeCycle<_, _, _, _, _, _>)
        (storageSetup: EcosystemStorageSetup)
        : IHostedOrReferencedLifeCycleAdapter =
    referencedLifeCycle.Invoke
        { new FullyTypedReferencedLifeCycleFunction<_, _, _, _, _, _, _> with
            member _.Invoke (referencedLifeCycle: ReferencedLifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role>) =
                // Register a service for All<'Subject>
                let allServiceType = typeof<Service<All<'Subject, 'SubjectId, 'SubjectIndex, 'OpError>>>
                if services |> Seq.exists (fun s -> s.ServiceType = allServiceType) then
                    failwith $"Ambiguous All<> service can't be registered. Do you have similar/ambiguous life cycle definitions in hosted and referenced ecosystems? {allServiceType.FullName}"

                services
                    .AddScoped<ISubjectRepo<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'SubjectIndex, 'OpError>>(
                        fun serviceProvider ->
                            let grainProvider = serviceProvider.GetRequiredService<IBiosphereGrainProvider>()
                            let grainPartition = serviceProvider.GetRequiredService<GrainPartition>()
                            BiosphereSubjectRepo<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>(referencedLifeCycle.Def, grainProvider, grainPartition)
                        )
                    .AddScoped<Service<All<'Subject, 'SubjectId, 'SubjectIndex, 'OpError>>>(
                        fun serviceProvider ->
                            let subjectRepo = serviceProvider.GetService<ISubjectRepo<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'SubjectIndex, 'OpError>>()
                            let cache = getInMemoryCache storageSetup serviceProvider

                            let serviceNameQualifier = sprintf "%s.%s" referencedLifeCycle.Def.Key.EcosystemName referencedLifeCycle.Def.Key.LocalLifeCycleName
                            getAllOfTService serviceNameQualifier subjectRepo cache
                    )
        }
    |> ignore

    { ReferencedLifeCycle = referencedLifeCycle }

let private registerUntypedReferencedLifeCycle (services: IServiceCollection) (referencedLifeCycle: IReferencedLifeCycle) (storageSetup: EcosystemStorageSetup) =
    referencedLifeCycle.Invoke
        { new FullyTypedReferencedLifeCycleFunction<_> with
            member _.Invoke referencedLifeCycle =
                registerReferencedLifeCycle services referencedLifeCycle storageSetup
        }

let private viewConnectorHandler (view: View<'Input, 'Output, 'OpError, 'AccessPredicateInput, 'Session, 'Role, 'Env>) (serviceProvider: IServiceProvider) (request: ViewConnectorRequest<'Input, 'Output, 'OpError>) : Task<ResponseVerificationToken> =
    match request with
    | ReadView (input, responseChannel) ->
        backgroundTask {
            let viewEnv = view.CreateEnv CallOrigin.Internal serviceProvider
            let (ViewResult viewTask) = view.Read viewEnv input
            let! res = viewTask
            return responseChannel.Respond res
        }

let private registerView
    (services: IServiceCollection)
    (view: View<'Input, 'Output, 'OpError, 'AccessPredicateInput, 'Session, 'Role, 'Env>)
    : ViewAdapter<'Input, 'Output, 'OpError> =

    let viewAdapter: ViewAdapter<'Input, 'Output, 'OpError> = {
        View = view
    }

    // Register a service for ViewConnector<'Subject>
    let viewConnectorServiceType = typeof<Service<ViewConnectorRequest<'Input, 'Output, 'OpError>>>
    if services |> Seq.exists (fun s -> s.ServiceType = viewConnectorServiceType) then
        failwith $"Ambiguous ViewConnector<> service can't be registered. Do you have similar/ambiguous view definitions in hosted and referenced ecosystems? {viewConnectorServiceType.FullName}"

    services
        // Register the adapter itself as singleton
        .AddSingleton(viewAdapter)
        .AddScoped<Service<ViewConnectorRequest<'Input, 'Output, 'OpError>>>(fun serviceProvider ->
            createService (sprintf "%sConnector" view.Name) (viewConnectorHandler view serviceProvider))
        |> registerEnvironmentFactoryGeneric<'Env>

    viewAdapter

let private registerUntypedView (services: IServiceCollection) (view: IView) =
    view.Invoke
        { new FullyTypedViewFunction<_> with
            member _.Invoke view = registerView services view :> IViewAdapter }

let private registerTimeSeries
    (services: IServiceCollection)
    (timeSeries: TimeSeries<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex, 'AccessPredicateInput, 'Session, 'Role>)
    (storageSetup: EcosystemStorageSetup)
    : TimeSeriesAdapter<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex> =

    let timeSeriesAdapter: TimeSeriesAdapter<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex> = {
        TimeSeries = timeSeries
    }

    let timeSeriesStorageImplementationFactory (serviceProvider: IServiceProvider) : ITimeSeriesStorageHandler<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'TimeSeriesIndex> =
        match storageSetup with
        | EcosystemStorageSetup.Proper
        | EcosystemStorageSetup.TestDataSeeding ->
            let config = serviceProvider.GetRequiredService<SqlServerConnectionStrings>()
            let logger = serviceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<SqlServerTimeSeriesStorageHandler<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex>>>()
            SqlServerTimeSeriesStorageHandler<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex>(timeSeries, config, logger)
            :> ITimeSeriesStorageHandler<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'TimeSeriesIndex>

        | EcosystemStorageSetup.Test ->
            let grainPartition = serviceProvider.GetRequiredService<GrainPartition>()
            TestTimeSeriesStorageHandler<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'TimeSeriesIndex>(grainPartition)
            :> ITimeSeriesStorageHandler<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'TimeSeriesIndex>

    let timeSeriesRepoImplementationFactory (serviceProvider: IServiceProvider) : ITimeSeriesRepo<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'TimeSeriesIndex> =
        match storageSetup with
        | EcosystemStorageSetup.Proper
        | EcosystemStorageSetup.TestDataSeeding ->
            let config = serviceProvider.GetRequiredService<SqlServerConnectionStrings>()
            SqlServerTimeSeriesRepo<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex>(timeSeries, config)
            :> ITimeSeriesRepo<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'TimeSeriesIndex>

        | EcosystemStorageSetup.Test ->
            let grainPartition = serviceProvider.GetRequiredService<GrainPartition>()
            TestTimeSeriesRepo<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex>(timeSeries, grainPartition)
            :> ITimeSeriesRepo<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'TimeSeriesIndex>

    services
        // Register the adapter itself as singleton  // TODO: do we actually need it if there's no grain that uses typed TimeSeriesAdapter<> ?
        .AddSingleton(timeSeriesAdapter)
        .AddScoped<ITimeSeriesStorageHandler<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'TimeSeriesIndex>>(timeSeriesStorageImplementationFactory)
        .AddScoped<ITimeSeriesRepo<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'TimeSeriesIndex>>(timeSeriesRepoImplementationFactory)
        .AddScoped<Service<AllTimeSeries<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'TimeSeriesIndex>>>(
            fun serviceProvider ->
                //let timeSeriesRepo = serviceProvider.GetRequiredService<ITimeSeriesRepo<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'TimeSeriesIndex>>()
                let timeSeriesRepo =
                        serviceProvider.GetService<ITimeSeriesRepo<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'TimeSeriesIndex>>()
                getAllTimeSeriesOfTService (* serviceNameQualifier *) timeSeriesAdapter.TimeSeries.Name timeSeriesRepo
        )

        |> ignore

    timeSeriesAdapter

let private registerUntypedTimeSeries
    (services: IServiceCollection)
    (timeSeries: ITimeSeries)
    (storageSetup: EcosystemStorageSetup) =
    timeSeries.Invoke
        { new FullyTypedTimeSeriesFunction<_> with
            member _.Invoke timeSeries = registerTimeSeries services timeSeries storageSetup :> ITimeSeriesAdapter }

let internal getCryptographer (rootPurpose: string) (dataProtectionProvider: IDataProtectionProvider) : ICryptographer =
    let rootProtector = dataProtectionProvider.CreateProtector rootPurpose
    { new ICryptographer with
        member _.Encrypt(decrypted: byte []) (purpose: string): byte [] =
            let protector = rootProtector.CreateProtector purpose
            protector.Protect decrypted

        member _.Decrypt(encrypted: byte []) (purpose: string): Result<byte[], DecryptionFailure> =
            let protector = rootProtector.CreateProtector purpose
            try
                protector.Unprotect encrypted
                |> Ok
            with
            | :? System.Security.Cryptography.CryptographicException ->
                Error DecryptionFailure
    }

type private TransientSideEffectFailureHook () =
    let random = System.Random()
    interface ITransientSideEffectFailureHook with
        member this.ShouldInjectOddRetry retryNo =
            // 10% chance of up to one retry
            retryNo = 0u && random.Next() % 10 = 0


type SqlServerSetupStartupTask(
    lifeCycleAdapterCollection: HostedLifeCycleAdapterCollection,
    timeSeriesAdapterCollection: TimeSeriesAdapterCollection,
    sqlServerConnections: SqlServerConnectionStrings,
    logger: ILogger<SqlServerSetupStartupTask>) =

    interface IStartupTask with
        member _.Execute(_cancellationToken: CancellationToken): Task =
            logger.Info("SqlServerSetupStartupTask Begin")

            // FIXME -- this probably needs to be pulled out of the orleans lifecycle, as it looks
            // like all startup tasks are initiated in parallel (however initiatied one after another)
            // For now, I've made the SQL Server blocking sync, so the setup is done before the other
            // tasks are started, but we're also implicitly relying on non-guaranteed ordering of tasks
            // and the fact that Orleans is currently creating the tasks one at a time, both of which could
            // change in the future.
            try
                SqlServerSetup.doSetup sqlServerConnections lifeCycleAdapterCollection timeSeriesAdapterCollection
                logger.Info("SqlServerSetupStartupTask End")
                Task.CompletedTask
            with
            | ex ->
                logger.LogError (ex, "SqlServerSetupStartupTask Exception")
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw()
                shouldNotReachHereBecause "line above throws"

type LifeCycleHostStartupTask(serviceProvider: IServiceProvider, lifeCycleAdapterCollection: HostedLifeCycleAdapterCollection,
    logger: ILogger<LifeCycleHostStartupTask>) =

    interface IStartupTask with

        member _.Execute(_cancellationToken: CancellationToken): Task =
            logger.Info("LifeCycleHostStartupTask Begin")

            task {
                try
                    use scopedServiceProvider = serviceProvider.CreateScope()
                    let grainProvider = scopedServiceProvider.ServiceProvider.GetRequiredService<IBiosphereGrainProvider>()

                    let siloDetails : ILocalSiloDetails = serviceProvider.GetRequiredService<ILocalSiloDetails>()
                    let startupId = NonemptyString.ofStringUnsafe siloDetails.Name

                    let startupLifeCycleAdapter = lifeCycleAdapterCollection.GetLifeCycleAdapterByLocalName "_Startup" |> Option.get
                    let! res = startupLifeCycleAdapter.RunActionMaybeConstructOnGrain grainProvider defaultGrainPartition None (StartupId startupId) StartupAction.PerformStartup (StartupConstructor.NewForSilo startupId) None

                    logger.Info("LifeCycleHostStartupTask End")

                    match res with
                    | Ok _ ->
                        return Ok Nothing
                    | Error err ->
                        return Error (sprintf "_Startup PerformStartup action threw an error: %A" err)
                with
                | ex ->
                    logger.LogError (ex, "LifeCycleHostStartupTask Exception")
                    System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw()
                    return shouldNotReachHereBecause "line above throws"

            } |> Task.Ignore

type CustomStorageInitStartupTask(serviceProvider: IServiceProvider, logger: ILogger<CustomStorageInitStartupTask>) =

    interface IStartupTask with

        member _.Execute(cancellationToken: CancellationToken): Task =
            logger.Info("CustomStorageInitStartupTask Begin")

            backgroundTask {
                try
                    do!
                        serviceProvider.GetServices<ICustomStorageInit>()
                        |> Seq.map (fun service -> service.Execute(cancellationToken))
                        |> Task.WhenAll

                    logger.Info("CustomStorageInitStartupTask End")
                with
                | ex ->
                    logger.LogError (ex, "CustomStorageInitStartupTask Exception")
                    System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw()
                    return shouldNotReachHereBecause "line above throws"
            }

// Temporary hack, for child D.I scopes to be able to get the current IGrainActivationContext.
// To get rid of this hack, we need to move to a more feature-rich D.I system like Autofac
// that allows us to override registrations in child scopes
type ContainerForIGrainActivationContext() =
    member val Value : Option<IGrainActivationContext> = None with get, set

[<Extension>]
type ServiceCollectionExtensions =

    [<Extension>]
    static member AddEcosystem (services: IServiceCollection, ecosystem: Ecosystem, storageSetup: EcosystemStorageSetup) : IServiceCollection =
        services
            .AddSingleton(ecosystem)
            .AddScoped<ContainerForIGrainActivationContext>(fun _ -> new ContainerForIGrainActivationContext())
            .AddScoped<GrainPartition>(
                fun serviceProvider ->
                    // This will only work from within the context of a grain. All external callers need an override
                    // (e.g. in ASP.NET Core, or in testing framework)

                        let grainActivationContext =
                            serviceProvider.GetRequiredService<ContainerForIGrainActivationContext>().Value
                            |> Option.defaultWith (fun () -> serviceProvider.GetRequiredService<IGrainActivationContext>())

                        let (grainPartition, _) = grainActivationContext.GrainIdentity.GetPrimaryKey()
                        GrainPartition grainPartition
                )
                // Add System Services
                .AddSingleton(createService "System.Clock" clockHandler)
                .AddScoped<Service<SubjectDataMaintenanceManager>>(fun serviceProvider ->
                    let lifeCycleAdapterCollection = serviceProvider.GetRequiredService<HostedLifeCycleAdapterCollection>()
                    createService "System.SubjectDataMaintenanceManager" (subjectDataMaintenanceManagerHandler serviceProvider lifeCycleAdapterCollection))
                .AddScoped<Service<EcosystemHealthManager>>(fun serviceProvider ->
                    let config = serviceProvider.GetRequiredService<SqlServerConnectionStrings>()
                    let grainPartition = serviceProvider.GetRequiredService<GrainPartition>()
                    let lifeCycleAdapterCollection = serviceProvider.GetRequiredService<HostedLifeCycleAdapterCollection>()
                    createService "System.EcosystemHealthManager" (ecosystemHealthManagerHandler serviceProvider lifeCycleAdapterCollection config grainPartition))
                .AddScoped<Service<MetricsReporter>>(fun serviceProvider ->
                    let lifeCycleAdapterCollection = serviceProvider.GetRequiredService<HostedLifeCycleAdapterCollection>()
                    createService "System.MetricsReporter" (metricsReporterHandler serviceProvider ecosystem.Name lifeCycleAdapterCollection))
                .AddSingleton(createService "System.Random"        randomHandler)
                .AddSingleton(createService "System.Unique"        uniqueHandler)
                .AddScoped<ITransferBlobHandler>(fun serviceProvider ->
                    let config = serviceProvider.GetRequiredService<SqlServerConnectionStrings>()
                    let clock = serviceProvider.GetRequiredService<Service<Clock>>()
                    let logger = serviceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<SqlServerTransferBlobHandler>>()
                    SqlServerTransferBlobHandler(config, ecosystem.Name, clock, logger) :> ITransferBlobHandler)
                .AddSingleton<Service<Cryptographer>>(
                    fun serviceProvider ->
                        let dataProtectionProvider = serviceProvider.GetRequiredService<IDataProtectionProvider>()
                        // TODO: fix encryption for co-hosted ecosystems, they need to use ecosystemName of LifeCycleKey
                        let cryptographer = getCryptographer ecosystem.Name dataProtectionProvider
                        createService "System.Cryptographer" (cryptographerHandler cryptographer))
                .AddSingleton<Service<AllBlobs>>(
                    fun serviceProvider ->
                        let repo = serviceProvider.GetRequiredService<IBlobRepo>()
                        createService "System.AllBlobs" (allBlobsServiceHandler repo))
        |> ignore

        match storageSetup with
        | EcosystemStorageSetup.Test ->
            services.AddSingleton<TestBlobStorage> (fun _ -> TestBlobStorage ()) |> ignore
            // inject chance transient failures in Test mode
            services.AddSingleton<ITransientSideEffectFailureHook> (TransientSideEffectFailureHook ()) |> ignore
        | EcosystemStorageSetup.Proper
        | EcosystemStorageSetup.TestDataSeeding ->
            ()

        // Add lifecycles
        ecosystem.LifeCycles
        |> Map.toSeq
        |> Seq.map (
            fun (lifeCycleName, untypedLifeCycle) ->
                // Sneaky side-effect
                // We're registering the LifeCycle into the D.I container
                lifeCycleName, (registerUntypedLifeCycle services untypedLifeCycle storageSetup))
        |> Map.ofSeq
        |> fun adapters ->
            HostedLifeCycleAdapterCollection (ecosystem.Name, adapters)
            |> services.AddSingleton
            |> ignore

            HostedOrReferencedLifeCycleAdapterRegistry (
                // register hosted LC adapters
                adapters
                |> Seq.map (fun kvp -> kvp.Key, kvp.Value :> IHostedOrReferencedLifeCycleAdapter)
                |> Seq.append (
                    // register referenced LC adapters
                    ecosystem.ReferencedEcosystems.Values
                    |> Seq.collect (fun e -> e.LifeCycles)
                    |> Seq.map (fun lc -> lc.Def.LifeCycleKey, registerUntypedReferencedLifeCycle services lc storageSetup)
                )
                |> Map.ofSeq)
            |> services.AddSingleton
            |> ignore

        // add blob repo when all life cycles registered
        services.AddSingleton<IBlobRepo>(
            fun serviceProvider ->
                match storageSetup with
                | EcosystemStorageSetup.Test ->
                    // hack for test env, because Sql dependencies won't resolve
                    let blobStorage = serviceProvider.GetRequiredService<TestBlobStorage>()
                    TestSubjectBlobRepo(blobStorage) :> IBlobRepo
                | EcosystemStorageSetup.Proper
                | EcosystemStorageSetup.TestDataSeeding ->
                    let grainProvider = serviceProvider.GetRequiredService<IBiosphereGrainProvider>()
                    let volatileBlobRepo = VolatileSubjectBlobRepo ()
                    let lifeCycleAdapterCollection = serviceProvider.GetRequiredService<HostedLifeCycleAdapterCollection>()
                    let hostedLifeCyclesStorageType = lifeCycleAdapterCollection |> Seq.map (fun a -> a.LifeCycle.Def.LifeCycleKey, a.Storage) |> Map.ofSeq
                    let config = serviceProvider.GetRequiredService<SqlServerConnectionStrings>()
                    let sqlBlobRepo = SqlServerSubjectBlobRepo config
                    // The grain partition only differs in test scenarios, so we hard-code it here because RouterBlobRepo not used during testing.
                    let grainPartition = defaultGrainPartition
                    let biosphereBlobRepo = BiosphereBlobRepo (grainProvider, grainPartition)
                    RouterBlobRepo (grainProvider, hostedLifeCyclesStorageType, sqlBlobRepo, volatileBlobRepo, biosphereBlobRepo) :> IBlobRepo)
        |> ignore

        // Add views
        ecosystem.Views
        |> Map.toSeq
        |> Seq.map (
            fun (viewName, untypedView) ->
                // Sneaky side-effect
                // We're registering the View into the D.I container
                viewName, (registerUntypedView services untypedView))
        |> Map.ofSeq
        |> ViewAdapterCollection
        |> services.AddSingleton
        |> ignore

        // Add time series
        ecosystem.TimeSeries
        |> Map.toSeq
        |> Seq.map (
            fun (timeSeriesKey, untypedTimeSeries) ->
                // Sneaky side-effect
                // We're registering the TimeSeries into the D.I. container
                timeSeriesKey, (registerUntypedTimeSeries services untypedTimeSeries storageSetup))
        |> Map.ofSeq
        |> TimeSeriesAdapterCollection
        |> services.AddSingleton
        |> ignore

        // Register summarizers for all relevant types
        // use fastestButNotInformativeSummarizersForEcosystem instead if codecs fail for some reason
        printfn "Compiling value summarizers... %A" System.DateTimeOffset.Now
        mixedCodecBasedAndFastSummarizersForEcosystem ecosystem None
        |> services.AddSingleton
        |> ignore
        printfn "Compiled value summarizers... %A" System.DateTimeOffset.Now

        ecosystem.Connectors
        |> Map.toSeq
        |> Seq.map (
            fun (connectorName, connector) ->
                let adapter = makeConnectorAdapter connector

                connector.Invoke
                    { new FullyTypedConnectorFunction<_> with
                        member _.Invoke (_: Connector<'Request, 'Env>) =
                            services.AddSingleton(adapter :?> ConnectorAdapter<'Request, 'Env>) |> ignore
                            registerEnvironmentFactoryGeneric<'Env> services
                            1 // can't be unit, don't ask me why, ask F# compiler
                    }
                    |> ignore

                connectorName, adapter
            )
        |> Map.ofSeq
        |> fun adapters -> ConnectorAdapterCollection adapters
        |> services.AddSingleton
        |> ignore

        // Register mutable map that contains custom storage handler factories
        let customStorageHandlers = CustomStorageHandlers (System.Collections.Generic.Dictionary<string, Func<IServiceProvider, (obj * obj)>>())
        services.AddSingleton<CustomStorageHandlers>(customStorageHandlers)