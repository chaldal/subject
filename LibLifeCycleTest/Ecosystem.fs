[<AutoOpen>]
module EcosystemModule

open System
open System.Runtime.CompilerServices
open FSharpPlus
open LibLifeCycleHost.TelemetryModel
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open LibLifeCycle
open LibLifeCycleCore
open LibLifeCycleHost
open System.Threading.Tasks
open FsCheck
open LibLifeCycleTest

type private CallerInfo = {
    Context: string
    Line: int
}

[<RequireQualifiedAccess>]
module Ecosystem =
    let internal getStasisWaitFor cluster =
        if System.Diagnostics.Debugger.IsAttached then
            TimeSpan.FromDays 1.
        else
            cluster.Partition.StasisWaitFor

    let overrideStasisWaitFor (waitFor: TimeSpan) =
        fun cluster ->
            cluster.Partition.StasisWaitFor <- waitFor
            Task.FromResult ()

    let private createPartitionScopedLogger (serviceProvider: IServiceProvider) partition =
        let unscopedLogger = serviceProvider.GetRequiredService<ILogger<ClockSimulator>>()
        let valueSummarizers = serviceProvider.GetRequiredService<ValueSummarizers>()
        let logger = newPartitionScopedLogger valueSummarizers unscopedLogger partition
        logger

    let private getWaitDelayMs (totalWaitTime: TimeSpan) =
        min (totalWaitTime.TotalMilliseconds / 25.) 2500.
        |> int

    let internal registerInteractedSubject (cluster: LogicalCluster) (subject: 'Subject) =
        let id = getId subject
        cluster.Partition.CapturedInteractions.[id] <- subject

    let private registerInteractedSubjectRes (cluster: LogicalCluster) (subjectRes: Result<'Subject, _>) =
        match subjectRes with
        | Ok subj ->
            registerInteractedSubject cluster subj
        | Error _ ->
            ()
        subjectRes

    let private registerInteractedSubjectResTask (cluster: LogicalCluster) (subjectResTask: Task<Result<'Subject, _>>) =
        backgroundTask {
            let! subjRes = subjectResTask
            return registerInteractedSubjectRes cluster subjRes
        }

    let private registerInteractedSubjectResTaskActAndWait (cluster: LogicalCluster) (subjectResTask: Task<Result<ActOrConstructAndWaitOnLifeEventResult<'Subject, _, _>, _>>) =
        backgroundTask {
            let! subjRes = subjectResTask
            match subjRes with
            | Ok (LifeEventTriggered (versionedSubject, _))
            | Ok (WaitOnLifeEventTimedOut versionedSubject) ->
                registerInteractedSubject cluster versionedSubject.Subject
            | _ ->
                ()
            return subjRes
        }

    let filterOneFromInteractedSubjects (predicate: Subject -> Option<'Subject>) : ClusterOperation<Option<'Subject>> =
        fun cluster ->
            backgroundTask {
                return
                    cluster.Partition.CapturedInteractions
                    |> Seq.map (fun kv -> kv.Value)
                    |> Seq.choose predicate
                    |> Seq.tryHead
            }

    let setNamedValue (namedValue: NamedValue<'T>) (value: 'T) : ClusterOperation<'T> =
        fun cluster ->
            backgroundTask {
                cluster.Partition.NamedValues.AddOrUpdate(namedValue.Name, (fun _ -> box value), (fun _ _ -> box value)) |> ignore
                return value
            }

    let configure (key: string) (value: string) : ClusterOperation<unit> =
        fun cluster ->
            backgroundTask {
                cluster.Partition.ConfigOverrides <- cluster.Partition.ConfigOverrides.AddOrUpdate(key, value)
                return ()
            }

    let thenSetNamedValue (namedValue: NamedValue<'T>) (clusterOp: ClusterOperation<'T>) : ClusterOperation<'T> =
        fun cluster ->
            backgroundTask {
                let! value = clusterOp cluster
                cluster.Partition.NamedValues.AddOrUpdate(namedValue.Name, (fun _ -> box value), (fun _ _ -> box value)) |> ignore
                return value
            }

    let getNamedValue<'T> (namedValue: NamedValue<'T>) : ClusterOperation<Option<'T>> =
        fun cluster ->
            backgroundTask {
                match cluster.Partition.NamedValues.TryGetValue namedValue.Name with
                | true, someObj ->
                    match someObj with
                    | :? 'T as value ->
                        return Some value
                    | x ->
                        return failwithf "Expecting a named value of type %s, but instead found %A" typeof<'T>.FullName x
                | false, _ ->
                    return None
            }

    let whenAll
        (operations: list<ClusterOperation<'T>>)
        : ClusterOperation<list<'T>> =
            fun cluster ->
                backgroundTask {
                    let! res = Task.WhenAll (operations |> List.map (fun op -> op cluster))
                    return res |> List.ofArray
                }

    module Biosphere =
        let private biosphereAdapter
            (scope: IClusterScope)
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>) =
            let adapters = scope.ServiceProvider.GetRequiredService<HostedOrReferencedLifeCycleAdapterRegistry> ()
            match adapters.GetLifeCycleBiosphereAdapterByKey lifeCycleDef.Key with
            | Some adapter -> adapter
            | None -> failwithf "LC Adapter not found for: %A" lifeCycleDef.Key

        let private biosphereGrainConnector
            (scope: IClusterScope)
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            : ClusterOperation<GrainConnector> =
                fun cluster ->
                backgroundTask {
                    let biosphereGrainProvider = scope.ServiceProvider.GetRequiredService<IBiosphereGrainProvider>()
                    let! grainFactory = biosphereGrainProvider.GetGrainFactory lifeCycleDef.Key.EcosystemName
                    return GrainConnector(grainFactory, cluster.Partition.GrainPartition, SessionHandle.NoSession, CallOrigin.Internal)
                }

        let generateId
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (ctor: 'Constructor)
            : ClusterOperation<Result<'SubjectId, GrainIdGenerationError<'OpError>>> =
                fun cluster ->
                    backgroundTask {
                        use scope = cluster.NewScope()
                        match! (biosphereAdapter scope lifeCycleDef).GenerateId scope.ServiceProvider CallOrigin.Internal ctor with
                        | Ok id ->
                            return (id :?> 'SubjectId) |> Ok
                        | Error err ->
                            return (err :?> 'OpError) |> GrainIdGenerationError.IdGenerationError |> Error
                    }

        let construct
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (ctor: 'Constructor)
            : ClusterOperation<Result<'Subject, GrainConstructionError<'OpError>>> =
                fun cluster ->
                    backgroundTask {
                        use scope = cluster.NewScope()
                        let adapter = biosphereAdapter scope lifeCycleDef
                        match! adapter.GenerateId scope.ServiceProvider CallOrigin.Internal ctor with
                        | Ok id ->
                            let! grainConnector = biosphereGrainConnector scope lifeCycleDef cluster
                            return!
                                grainConnector.Construct lifeCycleDef (id :?> 'SubjectId) ctor
                                    |> Task.map (Result.map (fun vs -> vs.Subject))
                                    |> registerInteractedSubjectResTask cluster
                        | Error err ->
                            return (err :?> 'OpError) |> GrainConstructionError.ConstructionError |> Error
                    }

        let hackDbSeedingOnlyInitializeDirectlyToValue
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (id: 'SubjectId)
            (initialValue: 'Subject)
            (randomCtor: 'Constructor)
            : ClusterOperation<Result<'Subject, GrainConstructionError<'OpError>>> =
                fun cluster ->
                    backgroundTask {
                        use scope = cluster.NewScope()
                        let biosphereGrainProvider = scope.ServiceProvider.GetRequiredService<IBiosphereGrainProvider>()
                        let! grainFactory = biosphereGrainProvider.GetGrainFactory lifeCycleDef.Key.EcosystemName
                        let grain =
                            let (GrainPartition partitionGuid) = cluster.Partition.GrainPartition
                            let idStr = (id :> SubjectId).IdString
                            grainFactory.GetGrain<ISubjectGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>>(partitionGuid, idStr)
                        return!
                            grain.TestingOnlyInitializeDirectlyToValue id initialValue randomCtor
                            |> registerInteractedSubjectResTask cluster
                    }

        let maybeConstruct
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (ctor: 'Constructor)
            : ClusterOperation<Result<'Subject, GrainMaybeConstructionError<'OpError>>> =
                fun cluster ->
                    backgroundTask {
                        use scope = cluster.NewScope()
                        let adapter = biosphereAdapter scope lifeCycleDef
                        match! adapter.GenerateId scope.ServiceProvider CallOrigin.Internal ctor with
                        | Ok id ->
                            let! grainConnector = biosphereGrainConnector scope lifeCycleDef cluster
                            return!
                                grainConnector.GetMaybeConstruct lifeCycleDef (id :?> 'SubjectId) ctor
                                |> Task.map (Result.map (fun vs -> vs.Subject))
                                |> registerInteractedSubjectResTask cluster
                        | Error err ->
                            return (err :?> 'OpError) |> GrainMaybeConstructionError.ConstructionError |> Error
                    }

        let thenConstruct
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (ctor: 'Constructor) (unitTask: ClusterOperation<unit>)
            : ClusterOperation<Result<'Subject, GrainConstructionError<'OpError>>> =
                fun cluster ->
                    backgroundTask {
                        do! unitTask cluster
                        use scope = cluster.NewScope()
                        let adapter = biosphereAdapter scope lifeCycleDef
                        match! adapter.GenerateId scope.ServiceProvider CallOrigin.Internal ctor with
                        | Ok id ->
                            let! grainConnector = biosphereGrainConnector scope lifeCycleDef cluster
                            return!
                                grainConnector.Construct lifeCycleDef (id :?> 'SubjectId) ctor
                                |> Task.map (Result.map (fun vs -> vs.Subject))
                                |> registerInteractedSubjectResTask cluster
                        | Error err ->
                            return (err :?> 'OpError) |> GrainConstructionError.ConstructionError |> Error
                    }

        let constructWaitAtMost (waitFor: TimeSpan)
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (ctor: 'Constructor) (lifeEvent: 'LifeEvent)
            : ClusterOperation<Result<ActOrConstructAndWaitOnLifeEventResult<'Subject, 'SubjectId, 'LifeEvent>, GrainConstructionError<'OpError>>> =
                fun cluster ->
                    backgroundTask {
                        use scope = cluster.NewScope()
                        let adapter = biosphereAdapter scope lifeCycleDef
                        match! adapter.GenerateId scope.ServiceProvider CallOrigin.Internal ctor with
                        | Ok id ->
                            let! grainConnector = biosphereGrainConnector scope lifeCycleDef cluster
                            return!
                                grainConnector.ConstructAndWait lifeCycleDef (id :?> 'SubjectId) ctor lifeEvent waitFor
                                |> registerInteractedSubjectResTaskActAndWait cluster
                        | Error err ->
                            return (err :?> 'OpError) |> GrainConstructionError.ConstructionError |> Error
                    }

        let constructWait lifeCycle ctor lifeEvent =
            fun cluster ->
                constructWaitAtMost (getStasisWaitFor cluster) lifeCycle ctor lifeEvent cluster

        let genConstruct
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (genCtor: Gen<'Constructor>) : ClusterOperation<Result<'Subject, GrainConstructionError<'OpError>>> =
                fun cluster ->
                    let ctor = evalGen genCtor
                    construct lifeCycleDef ctor cluster

        let actOnId
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (action: 'LifeAction) (subjectId: 'SubjectId)
            : ClusterOperation<Result<'Subject, GrainTransitionError<'OpError>>> =
                fun cluster ->
                    backgroundTask {
                        use scope = cluster.NewScope()
                        let! grainConnector = biosphereGrainConnector scope lifeCycleDef cluster
                        return!
                            grainConnector.Act lifeCycleDef subjectId.IdString action
                            |> Task.map (Result.map (fun vs -> vs.Subject))
                            |> registerInteractedSubjectResTask cluster
                    }

        let actOnIdWaitAtMost
                (waitFor:      TimeSpan)
                (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
                (action:       'LifeAction)
                (lifeEvent:    'LifeEvent)
                (subjectId:    'SubjectId)
                : ClusterOperation<Result<ActOrConstructAndWaitOnLifeEventResult<'Subject, 'SubjectId, 'LifeEvent>, GrainTransitionError<'OpError>>> =
            fun cluster ->
                backgroundTask {
                    use scope = cluster.NewScope()
                    let! grainConnector = biosphereGrainConnector scope lifeCycleDef cluster

                    return! grainConnector.ActAndWait lifeCycleDef subjectId.IdString action lifeEvent waitFor
                    |> registerInteractedSubjectResTaskActAndWait cluster
                }

        let actOnIdWait
                (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
                (action:       'LifeAction)
                (lifeEvent:    'LifeEvent)
                (subjectId:    'SubjectId)
                : ClusterOperation<Result<ActOrConstructAndWaitOnLifeEventResult<'Subject, 'SubjectId, 'LifeEvent>, GrainTransitionError<'OpError>>> =
            fun cluster -> actOnIdWaitAtMost (getStasisWaitFor cluster) lifeCycleDef action lifeEvent subjectId cluster

        let act
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (action: 'LifeAction) (subject: 'Subject)
            : ClusterOperation<Result<'Subject, GrainTransitionError<'OpError>>> =
                actOnId lifeCycleDef action subject.SubjectId

        let thenAct
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (action: 'LifeAction) (subjectTask: ClusterOperation<'Subject>) : ClusterOperation<Result<'Subject, GrainTransitionError<'OpError>>> =
                fun cluster ->
                    backgroundTask {
                        let! subject = subjectTask cluster
                        use scope = cluster.NewScope()
                        let! grainConnector = biosphereGrainConnector scope lifeCycleDef cluster
                        return!
                            grainConnector.Act lifeCycleDef subject.SubjectId.IdString action
                            |> Task.map (Result.map (fun vs -> vs.Subject))
                            |> registerInteractedSubjectResTask cluster
                    }

        let thenActWaitAtMost (waitFor: TimeSpan)
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (action: 'LifeAction) (lifeEvent: 'LifeEvent) (subjectTask: ClusterOperation<'Subject>) : ClusterOperation<Result<ActOrConstructAndWaitOnLifeEventResult<'Subject, 'SubjectId, 'LifeEvent>, GrainTransitionError<'OpError>>> =
                fun cluster ->
                    backgroundTask {
                        let! subject = subjectTask cluster
                        use scope = cluster.NewScope()
                        let! grainConnector = biosphereGrainConnector scope lifeCycleDef cluster
                        return!
                            grainConnector.ActAndWait lifeCycleDef subject.SubjectId.IdString action lifeEvent waitFor
                            |> registerInteractedSubjectResTaskActAndWait cluster
                    }

        let thenActWait lifeCycleDef action lifeEvent subjectTask =
            fun cluster ->
                thenActWaitAtMost (getStasisWaitFor cluster) lifeCycleDef action lifeEvent subjectTask cluster

        let  actWaitAtMost (waitFor: TimeSpan)
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (action: 'LifeAction) (lifeEvent: 'LifeEvent) (subject: 'Subject) : ClusterOperation<Result<ActOrConstructAndWaitOnLifeEventResult<'Subject, 'SubjectId, 'LifeEvent>, GrainTransitionError<'OpError>>> =
                fun cluster ->
                    backgroundTask {
                        use scope = cluster.NewScope()
                        let! grainConnector = biosphereGrainConnector scope lifeCycleDef cluster
                        return!
                            grainConnector.ActAndWait lifeCycleDef subject.SubjectId.IdString action lifeEvent waitFor
                            |> registerInteractedSubjectResTaskActAndWait cluster
                    }

        let actWait lifeCycle action lifeEvent subject =
            fun cluster ->
                actWaitAtMost (getStasisWaitFor cluster) lifeCycle action lifeEvent subject cluster

        let thenActBuildAction
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (actionBuilder: 'Context -> 'LifeAction) (subjectTaskWithContext: ClusterOperation<'Subject * 'Context>) : ClusterOperation<Result<'Subject, GrainTransitionError<'OpError>>> =
                fun cluster ->
                    backgroundTask {
                        let! (subject, context) = subjectTaskWithContext cluster
                        let action = actionBuilder context
                        use scope = cluster.NewScope()
                        let! grainConnector = biosphereGrainConnector scope lifeCycleDef cluster
                        return!
                            grainConnector.Act lifeCycleDef subject.SubjectId.IdString action
                            |> Task.map (Result.map (fun vs -> vs.Subject))
                    }

        let get
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (subjectId: 'SubjectId) : ClusterOperation<Option<'Subject>> =
                fun cluster ->
                    backgroundTask {
                        use scope = cluster.NewScope()
                        let! grainConnector = biosphereGrainConnector scope lifeCycleDef cluster
                        let! subject = grainConnector.GetSubjectFromGrain lifeCycleDef subjectId.IdString
                        return
                            match subject with
                            | Ok maybeVersionedSubject ->
                                maybeVersionedSubject
                                |> Option.map (fun vs -> vs.Subject)
                                |> Option.iter (registerInteractedSubject cluster)
                                maybeVersionedSubject
                                |> Option.map (fun vs -> vs.Subject)
                            | Error GrainGetError.AccessDenied -> failwith "Unexpected access denial"
                    }

        let thenGet
                (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
                (clusterOp: ClusterOperation<'SubjectId>)
                : ClusterOperation<Option<'Subject>> =
            fun cluster ->
                backgroundTask {
                    let! subjectId = clusterOp cluster
                    return! get lifeCycleDef subjectId cluster
                }

        let thenGetLatest
                (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
                (clusterOp: ClusterOperation<'Subject>)
                : ClusterOperation<'Subject> =
            fun cluster ->
                backgroundTask {
                    let! subject = clusterOp cluster
                    match! get lifeCycleDef subject.SubjectId cluster with
                    | Some latestSubject ->
                        return latestSubject
                    | None ->
                        return failwithf "Get-latest-subject-Assertion Failed for %s" typeof<'Subject>.FullName
                }

        let getMultiple
                (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
                (subjectIds: Set<'SubjectId>) : ClusterOperation<list<'Subject>> =
            fun cluster ->
                backgroundTask {
                    use scope = cluster.NewScope()
                    let! grainConnector = biosphereGrainConnector scope lifeCycleDef cluster
                    let! subjects = grainConnector.GetByIds lifeCycleDef subjectIds
                    // TODO: return actual versioned subject?
                    return (subjects |> List.map VersionedSubject.subject)
                }

        let filterFetch
                (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
                (query: IndexQuery<'SubjectIndex>)
                : ClusterOperation<List<'Subject>> =
            fun cluster ->
                backgroundTask {
                    use scope = cluster.NewScope()
                    let! grainConnector = biosphereGrainConnector scope lifeCycleDef cluster
                    let! subjects = grainConnector.FilterFetchSubjects lifeCycleDef query
                    // TODO: return actual versioned subject?
                    return (subjects |> List.map VersionedSubject.subject)
                }

        let filterFetchAll
                (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
                (resultSetOptions: ResultSetOptions<'SubjectIndex>)
                : ClusterOperation<List<'Subject>> =
            fun cluster ->
                backgroundTask {
                    use scope = cluster.NewScope()
                    let! grainConnector = biosphereGrainConnector scope lifeCycleDef cluster
                    let! subjects = grainConnector.FilterFetchAllSubjects lifeCycleDef resultSetOptions
                    // TODO: return actual versioned subject?
                    return (subjects |> List.map VersionedSubject.subject)
                }

        let getPrepared
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (subjectId: 'SubjectId) (transactionId: SubjectTransactionId) : ClusterOperation<Option<'Subject>> =
                fun cluster ->
                    backgroundTask {
                        use scope = cluster.NewScope()
                        let! grainConnector = biosphereGrainConnector scope lifeCycleDef cluster
                        match! grainConnector.GetPreparedSubjectFromGrain lifeCycleDef subjectId.IdString transactionId with
                        | Ok prepared -> return prepared
                        | Error GrainGetError.AccessDenied -> return failwith "Unexpected access denial"
                    }

        let refresh
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (subject: 'Subject) : ClusterOperation<'Subject> =
                fun cluster ->
                    backgroundTask {
                        use scope = cluster.NewScope()
                        let! grainConnector = biosphereGrainConnector scope lifeCycleDef cluster
                        match! grainConnector.GetSubjectFromGrain lifeCycleDef subject.SubjectId.IdString with
                        | Ok (Some vs) ->
                            registerInteractedSubject cluster vs.Subject
                            return vs.Subject
                        | Ok None ->
                            return failwith "Option.Some-Assertion Failed during refresh"
                        | Error GrainGetError.AccessDenied -> return failwith "Unexpected access denial"
                    }

        let thenRefresh
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (res: ClusterOperation<'Subject>) : ClusterOperation<'Subject> =
                fun cluster ->
                    backgroundTask {
                        let! subject = res cluster
                        return! refresh lifeCycleDef subject cluster
                    }

        let allowedActionsForSubject
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (subject: 'Subject)
            : ClusterOperation<list<string>> =
            fun cluster ->
                backgroundTask {
                    use scope = cluster.NewScope()
                    let! grainConnector = biosphereGrainConnector scope lifeCycleDef cluster
                    return! grainConnector.AllowedActionsForSubject lifeCycleDef subject
                }

    let generateId
            (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>)
            (ctor: 'Constructor)
            : ClusterOperation<Result<'SubjectId, GrainIdGenerationError<'OpError>>> =
        Biosphere.generateId lifeCycle.Definition ctor

    let construct
        (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>)
        (ctor: 'Constructor)
        : ClusterOperation<Result<'Subject, GrainConstructionError<'OpError>>> =
            Biosphere.construct lifeCycle.Definition ctor

    let hackDbSeedingOnlyInitializeDirectlyToValue
        (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>)
        (id: 'SubjectId)
        (initialValue: 'Subject)
        (randomCtor: 'Constructor)
        : ClusterOperation<Result<'Subject, GrainConstructionError<'OpError>>> =
            Biosphere.hackDbSeedingOnlyInitializeDirectlyToValue lifeCycle.Definition id initialValue randomCtor

    let maybeConstruct
        (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>)
        (ctor: 'Constructor)
        : ClusterOperation<Result<'Subject, GrainMaybeConstructionError<'OpError>>> =
            Biosphere.maybeConstruct lifeCycle.Definition ctor

    let thenConstruct
        (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>)
        (ctor: 'Constructor) (unitTask: ClusterOperation<unit>)
        : ClusterOperation<Result<'Subject, GrainConstructionError<'OpError>>> =
            Biosphere.thenConstruct lifeCycle.Definition ctor unitTask

    let constructWaitAtMost (waitFor: TimeSpan)
        (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>)
        (ctor: 'Constructor) (lifeEvent: 'LifeEvent)
        : ClusterOperation<Result<ActOrConstructAndWaitOnLifeEventResult<'Subject, 'SubjectId, 'LifeEvent>, GrainConstructionError<'OpError>>> =
            Biosphere.constructWaitAtMost waitFor lifeCycle.Definition ctor lifeEvent

    let constructWait
        (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>)
        ctor lifeEvent =
        Biosphere.constructWait lifeCycle.Definition ctor lifeEvent

    let genConstruct
        (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>)
        (genCtor: Gen<'Constructor>) : ClusterOperation<Result<'Subject, GrainConstructionError<'OpError>>> =
            fun cluster ->
                let ctor = evalGen genCtor
                construct lifeCycle ctor cluster

    let actOnId
        (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>)
        (action: 'LifeAction) (subjectId: 'SubjectId)
        : ClusterOperation<Result<'Subject, GrainTransitionError<'OpError>>> =
            Biosphere.actOnId lifeCycle.Definition action subjectId

    let actOnIdWaitAtMost
            (waitFor: TimeSpan)
            (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>)
            (action:    'LifeAction)
            (subjectId: 'SubjectId)
            (lifeEvent: 'LifeEvent)
            : ClusterOperation<Result<ActOrConstructAndWaitOnLifeEventResult<'Subject, 'SubjectId, 'LifeEvent>, GrainTransitionError<'OpError>>> =
        Biosphere.actOnIdWaitAtMost waitFor lifeCycle.Definition action lifeEvent subjectId

    let actOnIdWait
            (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>)
            (action:    'LifeAction)
            (subjectId: 'SubjectId)
            (lifeEvent: 'LifeEvent)
            : ClusterOperation<Result<ActOrConstructAndWaitOnLifeEventResult<'Subject, 'SubjectId, 'LifeEvent>, GrainTransitionError<'OpError>>> =
        Biosphere.actOnIdWait lifeCycle.Definition action lifeEvent subjectId

    let act
        (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>)
        (action: 'LifeAction) (subject: 'Subject)
        : ClusterOperation<Result<'Subject, GrainTransitionError<'OpError>>> =
            Biosphere.act lifeCycle.Definition action subject

    let thenAct
        (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>)
        (action: 'LifeAction) (subjectTask: ClusterOperation<'Subject>) : ClusterOperation<Result<'Subject, GrainTransitionError<'OpError>>> =
            Biosphere.thenAct lifeCycle.Definition action subjectTask

    let thenIgnore (subjectTask: ClusterOperation<'T>) : ClusterOperation<unit> =
        fun cluster ->
            backgroundTask {
                do! subjectTask cluster
                    |> Task.Ignore
            }

    let thenActWaitAtMost (waitFor: TimeSpan)
        (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>)
        (action: 'LifeAction) (lifeEvent: 'LifeEvent) (subjectTask: ClusterOperation<'Subject>) : ClusterOperation<Result<ActOrConstructAndWaitOnLifeEventResult<'Subject, 'SubjectId, 'LifeEvent>, GrainTransitionError<'OpError>>> =
            Biosphere.thenActWaitAtMost waitFor lifeCycle.Definition action lifeEvent subjectTask

    let thenActWait
        (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>)
        action lifeEvent subjectTask =
            Biosphere.thenActWait lifeCycle.Definition action lifeEvent subjectTask

    let actWaitAtMost (waitFor: TimeSpan)
        (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>)
        (action: 'LifeAction) (lifeEvent: 'LifeEvent) (subject: 'Subject) : ClusterOperation<Result<ActOrConstructAndWaitOnLifeEventResult<'Subject, 'SubjectId, 'LifeEvent>, GrainTransitionError<'OpError>>> =
            Biosphere.actWaitAtMost waitFor lifeCycle.Definition action lifeEvent subject

    let actWait
        (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>)
        action lifeEvent subject =
            Biosphere.actWait lifeCycle.Definition action lifeEvent subject



    let thenActBuildAction
        (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>)
        (actionBuilder: 'Context -> 'LifeAction) (subjectTaskWithContext: ClusterOperation<'Subject * 'Context>) : ClusterOperation<Result<'Subject, GrainTransitionError<'OpError>>> =
            Biosphere.thenActBuildAction lifeCycle.Definition actionBuilder subjectTaskWithContext

    let get
        (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>)
        (subjectId: 'SubjectId) : ClusterOperation<Option<'Subject>> =
            Biosphere.get lifeCycle.Definition subjectId

    let thenGet
            (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>)
            (clusterOp: ClusterOperation<'SubjectId>)
            : ClusterOperation<Option<'Subject>> =
        Biosphere.thenGet lifeCycle.Definition clusterOp

    let thenGetLatest
            (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>)
            (clusterOp: ClusterOperation<'Subject>)
            : ClusterOperation<'Subject> =
        Biosphere.thenGetLatest lifeCycle.Definition clusterOp

    let getMultiple
            (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>)
            (subjectIds: Set<'SubjectId>) : ClusterOperation<list<'Subject>> =
        Biosphere.getMultiple lifeCycle.Definition subjectIds

    let filterFetch
            (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>)
            (query: IndexQuery<'SubjectIndex>)
            : ClusterOperation<List<'Subject>> =
        Biosphere.filterFetch lifeCycle.Definition query

    let filterFetchAll
            (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>)
            (resultSetOptions: ResultSetOptions<'SubjectIndex>)
            : ClusterOperation<List<'Subject>> =
        Biosphere.filterFetchAll lifeCycle.Definition resultSetOptions

    let readView // TODO: read views in Biosphere? requires replacing view name with a ViewKey similar to LifeCycleKey
        (view: View<'Input, 'Output, 'OpError, 'AccessPredicateInput, 'Session, 'Role, 'Env>)
        (input: 'Input) : ClusterOperation<Result<'Output, GrainExecutionError<'OpError>>> =
            fun cluster ->
                backgroundTask {
                    // blunt copy-and-paste from ViewGrain, because ViewGrain supports only codec-enabled views
                    use scope = cluster.NewScope()
                    let (ViewResult viewTask) = (view :> IView<'Input, 'Output, 'OpError>).Read CallOrigin.Internal scope.ServiceProvider input
                    match! viewTask with
                    | Ok output -> return Ok output
                    | Error err -> return err |> GrainExecutionError.ExecutionError |> Error
                }

    let private setNewTimeAndTriggerReminders (scope: IClusterScope) (cluster: LogicalCluster) (newTime: DateTimeOffset) : Task =
        backgroundTask {
            let grainPartition = cluster.Partition.GrainPartition
            let logger = createPartitionScopedLogger scope.ServiceProvider grainPartition
            let lifeCycleAdapterCollection = scope.ServiceProvider.GetRequiredService<HostedLifeCycleAdapterCollection>()
            let operationTracker = scope.ServiceProvider.GetRequiredService<OperationTracker>()
            let virtualTimeBefore = simulatedNowNoIncrement grainPartition
            do! operationTracker.TrackOperation
                    { Partition = grainPartition
                      Type = OperationType.TestTimeForward
                      Name = "SetNewTime"
                      MaybeParentActivityId = None
                      MakeItNewParentActivityId = true
                      BeforeRunProperties = Map.ofOneItem ("SimulatedTimeBefore", virtualTimeBefore.ToString("yyyy-MM-dd HH:mm:ss.fffff")) }
                    (fun () ->
                        backgroundTask {
                            do! LibLifeCycleTest.ClockSimulation.setNewTimeAndTriggerReminders logger grainPartition newTime lifeCycleAdapterCollection (scope.DefaultGrainFactory())
                            let virtualTimeAfter = simulatedNowNoIncrement grainPartition
                            return { ReturnValue = (); IsSuccess = Some true; AfterRunProperties = Map.ofOneItem ("SimulatedTimeAfter", virtualTimeAfter.ToString("yyyy-MM-dd HH:mm:ss.fffff")) }
                        })
        }

    let internal waitForSystemStasis (scope: IClusterScope) maxWaitFor : ClusterOperation<unit> = // TODO: can we do stasis detection in whole Biosphere?
        fun cluster ->
            backgroundTask {
                let lifeCycleAdapterCollection = scope.ServiceProvider.GetRequiredService<HostedLifeCycleAdapterCollection>()
                let sideEffectTrackerHook = scope.ServiceProvider.GetRequiredService<ISideEffectTrackerHook>()
                let grainPartition = cluster.Partition.GrainPartition
                let logger = createPartitionScopedLogger scope.ServiceProvider grainPartition

                let loopUntil = DateTimeOffset.Now.Add maxWaitFor
                let mutable stasisReached = false
                let mutable lastProcessedSideEffectsVersion = -1

                while (DateTimeOffset.Now < loopUntil && (not stasisReached)) do

                    let remainingMaxWait = max TimeSpan.Zero (loopUntil - DateTimeOffset.Now)

                    match! sideEffectTrackerHook.WaitForAllSideEffectsProcessed remainingMaxWait with
                    | Ok processedSideEffectVersion ->
                        if processedSideEffectVersion = lastProcessedSideEffectsVersion then
                            //logger.Info "Stasis detected at side effects version %d" lastProcessedSideEffectsVersion
                            stasisReached <- true

                        if not stasisReached then
                            lastProcessedSideEffectsVersion <- processedSideEffectVersion

                            let now = simulatedNow grainPartition
                            let hasOverdueTimer =
                                (reminderHook :> IReminderHook).FirstReminderOn grainPartition
                                |> Option.map (fun on -> on < now)
                                |> Option.defaultValue false

                            if hasOverdueTimer then
                                logger.Info "Side effects processed but overdue timers still scheduled => trigger again"
                                do! LibLifeCycleTest.ClockSimulation.setNewTimeAndTriggerReminders logger grainPartition now lifeCycleAdapterCollection (scope.DefaultGrainFactory())
                    | Error probablyUnprocessedSideEffects ->
                        return
                            failwithf "Stasis not reached, %d side effects not processed within %O : %O"
                                probablyUnprocessedSideEffects.Length
                                maxWaitFor
                                probablyUnprocessedSideEffects

                if not stasisReached then
                    return failwithf "Stasis not reached within %O, read logs to find potential endless loops" maxWaitFor
                else
                    return ()
            }

    let private getEventuallyWithin // TODO: can we do stasis detection in whole Biosphere?
        (maxWaitFor: TimeSpan)
        (lifeCycle:  LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>)
        (subjectId:  'SubjectId) : ClusterOperation<Option<'Subject>> =
            fun cluster ->
                backgroundTask {
                    // Task CEs, by design don't do tail recursion, so we'll use a mutable + while loop
                    let mutable matchedValue = None
                    use scope = cluster.NewScope()
                    do! waitForSystemStasis scope maxWaitFor cluster
                    match! scope.DefaultGrainConnector(cluster.Partition.GrainPartition).GetSubjectFromGrain lifeCycle.Definition subjectId.IdString with
                    | Ok (Some res) ->
                        registerInteractedSubject cluster res.Subject
                        matchedValue <- Some res.Subject
                    | Ok None ->
                        ()
                    | Error GrainGetError.AccessDenied -> failwith "Unexpected access denial"

                    return matchedValue
                }

    let private thenGetEventuallyWithin // TODO: can we do stasis detection in whole Biosphere?
            (maxWaitFor: TimeSpan)
            (lifeCycle:  LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>)
            (clusterOp:  ClusterOperation<'SubjectId>) : ClusterOperation<Option<'Subject>> =
        fun cluster ->
            backgroundTask {
                let! subjectId = clusterOp cluster
                return! getEventuallyWithin maxWaitFor lifeCycle subjectId cluster
            }

    let getEventually  // TODO: can we do stasis detection in whole Biosphere?
        (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>)
        (subjectId: 'SubjectId) : ClusterOperation<Option<'Subject>> =
            fun cluster ->
                getEventuallyWithin (getStasisWaitFor cluster) lifeCycle subjectId cluster

    let thenGetEventually // TODO: can we do stasis detection in whole Biosphere?
        (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>)
        (clusterOp: ClusterOperation<'SubjectId>)
        : ClusterOperation<Option<'Subject>> =
            fun cluster ->
                thenGetEventuallyWithin (getStasisWaitFor cluster) lifeCycle clusterOp cluster

    let getPrepared
        (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>)
        (subjectId: 'SubjectId) (transactionId: SubjectTransactionId) : ClusterOperation<Option<'Subject>> =
            Biosphere.getPrepared lifeCycle.Definition subjectId transactionId





    let runInParallel (operations: seq<ClusterOperation<'T>>) : ClusterOperation<list<'T>> =
        fun cluster ->
            backgroundTask {
                let! result =
                    operations
                    |> Seq.map (fun op -> op cluster)
                    |> Task.WhenAll

                return result |> List.ofArray
            }

    let runSequentially (operations: seq<ClusterOperation<'T>>) : ClusterOperation<list<'T>> =
        fun cluster ->
            backgroundTask {
                let mutable result = []
                for op in operations do
                    let! res = op cluster
                    result <- List.append result [res]

                return result
            }









    let getBlob
        (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>)
        (blobIdFactory: 'Subject -> BlobId) (subject: 'Subject) : ClusterOperation<Option<BlobData>> =
                fun cluster ->
                    backgroundTask {
                        use scope = cluster.NewScope()
                        let blobRepo = scope.ServiceProvider.GetRequiredService<IBlobRepo>()
                        let lcKey = lifeCycle.Definition.Key
                        return! blobRepo.GetBlobData lcKey.EcosystemName { LifeCycleName = lcKey.LocalLifeCycleName; SubjectIdStr = subject.SubjectId.IdString } (blobIdFactory subject).Id
                    }

    let thenGetBlob
        (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>)
        (blobIdFactory: 'Subject -> BlobId) (subjectTask: ClusterOperation<'Subject>) : ClusterOperation<Option<BlobData>> =
                fun cluster ->
                    backgroundTask {
                        let! subject = subjectTask cluster
                        return! getBlob lifeCycle blobIdFactory subject cluster
                    }

    let value (value: 'T) : ClusterOperation<'T> =
        fun _cluster ->
            backgroundTask {
                return value
            }



    let refresh
        (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>)
        (subject: 'Subject) : ClusterOperation<'Subject> =
            Biosphere.refresh lifeCycle.Definition subject

    let thenRefresh
        (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>)
        (res: ClusterOperation<'Subject>) : ClusterOperation<'Subject> =
            Biosphere.thenRefresh lifeCycle.Definition res

    let private assertDeletedWithin // TODO: biosphere-wide stasis detection needed
        (waitFor: TimeSpan)
        (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>)
        (subjectId: 'SubjectId) : ClusterOperation<unit> =
            fun cluster ->
                backgroundTask {
                    // Task CEs, by design don't do tail recursion, so we'll use a mutable + while loop
                    let mutable doesExist = true
                    use scope = cluster.NewScope()
                    do! waitForSystemStasis scope waitFor cluster
                    match! scope.DefaultGrainConnector(cluster.Partition.GrainPartition).DoesSubjectExist lifeCycle.Definition subjectId with
                    | true ->
                        ()
                    | false ->
                        doesExist <- false

                    if not doesExist then
                        return ()
                    else
                        return failwith "Deleted Assertion Failed"
                }

    let private thenAssertDeletedWithin
            (waitFor:   TimeSpan)
            (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>)
            (clusterOp: ClusterOperation<'SubjectId>)
            : ClusterOperation<unit> =
        fun cluster ->
            backgroundTask {
                let! subjectId = clusterOp cluster
                return! assertDeletedWithin waitFor lifeCycle subjectId cluster
            }



    let awaitSystemStasisWithin (waitFor: TimeSpan) : ClusterOperation<unit> =
        fun cluster ->
            backgroundTask {
                use scope = cluster.NewScope()
                do! waitForSystemStasis scope waitFor cluster
            }

    let awaitSystemStasis () : ClusterOperation<unit> =
        fun cluster ->
            awaitSystemStasisWithin (getStasisWaitFor cluster) cluster

    let thenAwaitSystemStasisWithin (waitFor: TimeSpan) (subjectTask: ClusterOperation<'Subject>) : ClusterOperation<'Subject>  =
        fun cluster ->
            backgroundTask {
                let! subject = subjectTask cluster
                do! awaitSystemStasisWithin waitFor cluster
                return subject
            }

    let thenAwaitSystemStasis (subjectTask: ClusterOperation<'Subject>) : ClusterOperation<'Subject> =
        fun cluster ->
            backgroundTask {
                let! subject = subjectTask cluster
                do! awaitSystemStasis () cluster
                return subject
            }

    // TODO: get rid of usages and remove this func
    let thenWaitOnRefAtMost (waitFor: TimeSpan) (refToWaitOn: Ref<Option<'Ref>>) (clusterOp: ClusterOperation<'T>) : ClusterOperation<'T * 'Ref> =
        fun cluster ->
            let loopUntil = DateTimeOffset.Now.Add(waitFor)
            backgroundTask {
                let! resT = clusterOp cluster

                // Task CEs, by design don't do tail recursion, so we'll use a mutable + while loop
                while DateTimeOffset.Now < loopUntil && (refToWaitOn.Value).IsNone do
                    do! Task.Delay(getWaitDelayMs waitFor)

                match refToWaitOn.Value with
                | Some t ->
                    return (resT, t)
                | None ->
                    return failwithf "Eventually-Predicate Assertion on Ref Failed"
            }

    let thenWaitOnRef (refToWaitOn: Ref<Option<'Ref>>) (clusterOp: ClusterOperation<'T>) : ClusterOperation<'T * 'Ref> =
        fun cluster ->
            thenWaitOnRefAtMost (getStasisWaitFor cluster) refToWaitOn clusterOp cluster

    let thenMap (mapper: 'T -> 'U) (clusterOp: ClusterOperation<'T>) : ClusterOperation<'U> =
        fun cluster ->
            backgroundTask {
                let! t = clusterOp cluster
                return mapper t
            }

    let defaultValue (defaultValue: ClusterOperation<'T>) (clusterOp: ClusterOperation<Option<'T>>) : ClusterOperation<'T> =
        fun cluster ->
            backgroundTask {
                let! t = clusterOp cluster
                match t with
                | Some v ->
                    return v
                | None ->
                    return! defaultValue cluster
            }

    let setCurrentUserId (userId: string): ClusterOperation<unit> =
        fun cluster ->
            backgroundTask {
                cluster.Partition.UserId <- userId
                return ()
            }

    let setNewTimeAndRunReminders (newTime: DateTimeOffset) : ClusterOperation<unit> =
        fun cluster ->
            backgroundTask {
                use scope = cluster.NewScope()
                do! waitForSystemStasis scope (getStasisWaitFor cluster) cluster
                do! setNewTimeAndTriggerReminders scope cluster newTime
                do! waitForSystemStasis scope (getStasisWaitFor cluster) cluster
            }

    let moveTimeForwardAndRunReminders (addDelta: TimeSpan) : ClusterOperation<unit> =
        fun cluster ->
            backgroundTask {
                use scope = cluster.NewScope()
                do! waitForSystemStasis scope (getStasisWaitFor cluster) cluster
                do! setNewTimeAndTriggerReminders scope cluster ((simulatedNow cluster.Partition.GrainPartition).Add addDelta)
                do! waitForSystemStasis scope (getStasisWaitFor cluster) cluster
            }

    let moveToTimeAndRunReminders (newTime: DateTimeOffset) : ClusterOperation<unit> =
        fun cluster ->
            backgroundTask {
                use scope = cluster.NewScope()
                do! waitForSystemStasis scope (getStasisWaitFor cluster) cluster
                do! setNewTimeAndTriggerReminders scope cluster newTime
                do! waitForSystemStasis scope (getStasisWaitFor cluster) cluster
            }

    let now : ClusterOperation<DateTimeOffset> =
        fun cluster ->
            simulatedNow cluster.Partition.GrainPartition
            |> Task.FromResult

    let overrideInitialSimulatedTime initialTime : ClusterOperation<unit> =
        fun cluster ->
            overrideInitialSimulatedTime initialTime cluster.Partition.GrainPartition
            |> Task.FromResult

    let thenSetNewTimeAndRunReminders (newTime: DateTimeOffset) (pipeline: ClusterOperation<'T>) : ClusterOperation<'T> =
        fun cluster ->
            backgroundTask {
                use scope = cluster.NewScope()
                let! t = pipeline cluster
                do! waitForSystemStasis scope (getStasisWaitFor cluster) cluster
                do! setNewTimeAndTriggerReminders scope cluster newTime
                do! waitForSystemStasis scope (getStasisWaitFor cluster) cluster
                return t
            }

    let thenMoveTimeForwardAndRunReminders (addDelta: TimeSpan) (pipeline: ClusterOperation<'T>) : ClusterOperation<'T> =
        fun cluster ->
            backgroundTask {
                use scope = cluster.NewScope()
                let! t = pipeline cluster
                do! waitForSystemStasis scope (getStasisWaitFor cluster) cluster
                do! setNewTimeAndTriggerReminders scope cluster ((simulatedNow cluster.Partition.GrainPartition).Add addDelta)
                do! waitForSystemStasis scope (getStasisWaitFor cluster) cluster
                return t
            }

    let useConnector (connector: Connector<'Request, 'Env>) (interceptor: 'Request -> Option<ResponseVerificationToken>) : ClusterOperation<IDisposable> =
        fun cluster ->
            let interceptorId =
                interceptConnector cluster.Partition connector (
                    fun serviceProvider request next ->
                        match interceptor request with
                        | Some token ->
                            Task.FromResult token
                        | None ->
                            next serviceProvider request
                )

            { new IDisposable with
                  member _.Dispose() =
                    removeConnectorInterception cluster.Partition.GrainPartition interceptorId
            }
            |> Task.FromResult

    let useConnectorAsync (connector: Connector<'Request, 'Env>) (interceptor: 'Request -> Option<Task<ResponseVerificationToken>>) : ClusterOperation<IDisposable> =
        fun cluster ->
            let interceptorId =
                interceptConnector cluster.Partition connector (
                    fun serviceProvider request next ->
                        match interceptor request with
                        | Some token ->
                            token
                        | None ->
                            next serviceProvider request
                )

            { new IDisposable with
                  member _.Dispose() =
                    removeConnectorInterception cluster.Partition.GrainPartition interceptorId
            }
            |> Task.FromResult

    let interceptConnector (connector: Connector<'Request, 'Env>) (interceptor: 'Request -> Option<ResponseVerificationToken>) : ClusterOperation<unit> =
        fun cluster ->
            backgroundTask {
                let! _ = useConnector connector interceptor cluster
                return ()
            }

    let interceptConnectorAsync (connector: Connector<'Request, 'Env>) (interceptor: 'Request -> Option<Task<ResponseVerificationToken>>) : ClusterOperation<unit> =
        fun cluster ->
            backgroundTask {
                let! _ = useConnectorAsync connector interceptor cluster
                return ()
            }

    let simulate (simulation: PartitionOperation<'T>) : ClusterOperation<'T> =
        fun cluster ->
            backgroundTask {
                return! simulation cluster.Partition
            }



    let thenClearAllBadLogs (pipeline: ClusterOperation<'T>) : ClusterOperation<'T> =
        fun cluster ->
            backgroundTask {
                let! res = pipeline cluster
                match partitionIdToTestOutputHelper.TryGetValue cluster.Partition.GrainPartition with
                | (true, (_, counters)) ->
                    counters.Reset()
                | _ ->
                    Noop
                return res
            }

    let getLogger () : ClusterOperation<IFsLogger> =
        fun cluster ->
            backgroundTask {
                use scope = cluster.NewScope()
                let logger = createPartitionScopedLogger scope.ServiceProvider cluster.Partition.GrainPartition
                return logger
            }

    let allowedActionsForSubject
        (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>)
        (subject: 'Subject)
        : ClusterOperation<list<string>> =
            Biosphere.allowedActionsForSubject lifeCycle.Definition subject

    // This is an interim measure to handle cases where either the testing framework or
    // the subject stack itself has some race conditions that need to be addressed.
    // Ideally we should remove all calls to this function, and this function itself, as the
    // framework matures.
    // Until then, this allows us to carry on
    let hackDelay (_whyIsThisHackNeeded: WhyIsThisHackNeeded) : ClusterOperation<unit> =
        fun _cluster ->
            backgroundTask {
                do! Task.Delay 200
            }

    let blobDataViaIBlobRepoGrain (blobId: BlobId)
        : ClusterOperation<Option<BlobData>> =
        fun cluster ->
            backgroundTask {
                use scope = cluster.NewScope()
                let grainConnector = scope.DefaultGrainConnector cluster.Partition.GrainPartition
                return! grainConnector.BlobData blobId.Owner blobId.Id
            }

type Ecosystem = class end
with
    static member private asyncAssertEventuallyWithinWithCaller
            (callerInfo: CallerInfo)
            (waitFor: TimeSpan)
            (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _,
            _, _, _>)
            (eventuallyPredicate: 'Subject -> PartitionOperation<bool>)
            (subject: 'Subject)
        : ClusterOperation<'Subject> =
            fun cluster ->
                backgroundTask {
                    // Task CEs, by design don't do tail recursion, so we'll use a mutable + while loop
                    let mutable matchedValue = None
                    let mutable lastObservedValue = None
                    use scope = cluster.NewScope()
                    do! Ecosystem.waitForSystemStasis scope waitFor cluster
                    match! scope.DefaultGrainConnector(cluster.Partition.GrainPartition).GetSubjectFromGrain lifeCycle.Definition subject.SubjectId.IdString with
                    | Ok (Some vs) ->
                        Ecosystem.registerInteractedSubject cluster vs.Subject
                        match! eventuallyPredicate vs.Subject cluster.Partition with
                        | true ->
                            matchedValue <- Some vs.Subject
                        | false ->
                            lastObservedValue <- Some vs.Subject
                    | Ok None ->
                        ()
                    | Error GrainGetError.AccessDenied ->
                        failwith $"%s{callerInfo.Context}: %i{callerInfo.Line} - Unexpected access denial"

                    match matchedValue with
                    | Some t ->
                        return t
                    | None ->
                        return failwithf $"%s{callerInfo.Context}: %i{callerInfo.Line} - Eventually-Predicate Assertion Failed, last observed value: %A{lastObservedValue}"
                }
                
    static member private assertEventuallyWithCaller
            (callerInfo: CallerInfo)
            lifeCycle
            eventuallyPredicate
            subject =
        fun cluster ->
            Ecosystem.asyncAssertEventuallyWithinWithCaller
                callerInfo (Ecosystem.getStasisWaitFor cluster) lifeCycle (eventuallyPredicate >> partitionOperationOfValue) subject cluster

    static member private thenAssertEventuallyWithCaller
            (callerInfo: CallerInfo)
            lifeCycle
            eventuallyPredicate
            subjectTask
        : ClusterOperation<'Subject> =
        fun cluster ->
            backgroundTask {
                let! subject = subjectTask cluster
                return! Ecosystem.assertEventuallyWithCaller callerInfo lifeCycle eventuallyPredicate subject cluster
            }
        
    static member private assertEventuallyWithinWithCaller
            (callerInfo: CallerInfo)
            (waitFor: TimeSpan)
            lifeCycle eventuallyPredicate
            subject=
        Ecosystem.asyncAssertEventuallyWithinWithCaller callerInfo waitFor lifeCycle (eventuallyPredicate >> partitionOperationOfValue) subject

    static member private asyncAssertEventuallyWithCaller
            (callerInfo: CallerInfo)
            lifeCycle
            eventuallyPredicate
            subject =
        fun cluster ->
            Ecosystem.asyncAssertEventuallyWithinWithCaller
                callerInfo
                (Ecosystem.getStasisWaitFor cluster)
                lifeCycle
                eventuallyPredicate
                subject
                cluster

    static member private thenAssertEventuallyWithinWithCaller callerInfo waitFor lifeCycle eventuallyPredicate (subjectTask: ClusterOperation<'Subject>) =
        fun cluster ->
            backgroundTask {
                let! subject = subjectTask cluster
                return! Ecosystem.asyncAssertEventuallyWithinWithCaller callerInfo waitFor lifeCycle (eventuallyPredicate >> partitionOperationOfValue) subject cluster
            }

    static member private thenAsyncAssertEventuallyWithinWithCaller callerInfo waitFor lifeCycle eventuallyPredicate (subjectTask: ClusterOperation<'Subject>) =
        fun cluster ->
            backgroundTask {
                let! subject = subjectTask cluster
                return! Ecosystem.asyncAssertEventuallyWithinWithCaller callerInfo waitFor lifeCycle eventuallyPredicate subject cluster
            }

    static member private thenAsyncAssertEventuallyWithCaller (callerInfo: CallerInfo) lifeCycle eventuallyPredicate (subjectTask: ClusterOperation<'Subject>) =
        fun cluster ->
            backgroundTask {
                let! subject = subjectTask cluster
                return! Ecosystem.asyncAssertEventuallyWithCaller callerInfo lifeCycle eventuallyPredicate subject cluster
            }

    static member private thenAssertOkWithCaller
            (callerInfo: CallerInfo)
            (res: ClusterOperation<Result<'T, 'Err>>) : ClusterOperation<'T> =
        fun cluster ->
            backgroundTask {
                match! res cluster with
                | Ok t ->
                    return t
                | Error err ->
                    return failwithf $"%s{callerInfo.Context}: %i{callerInfo.Line} - OK-Assertion Failed with Error: %A{err} (type: %s{typeof<'Err>.FullName})"
            }

    static member private thenAssertAllOkWithCaller
            (callerInfo: CallerInfo)
            (clusterOp: ClusterOperation<list<Result<'T, 'Err>>>) : ClusterOperation<list<'T>> =
        fun cluster ->
            backgroundTask {
                let! results = clusterOp cluster
                return
                    results
                    |> List.map (function
                        | Ok t ->
                            t
                        | Error err ->
                            failwithf $"%s{callerInfo.Context}: %i{callerInfo.Line} - OK-AssertAll Failed with Error: %A{err} (type: %s{typeof<'Err>.FullName})")
            }

    static member private thenAssertConstructionOpErrorWithCaller
            (callerInfo: CallerInfo)
            (res: ClusterOperation<Result<'T, GrainConstructionError<'Err>>>) : ClusterOperation<'Err> =
        fun cluster ->
            backgroundTask {
                match! res cluster with
                | Ok t ->
                    return failwithf $"%s{callerInfo.Context}: %i{callerInfo.Line} - Construction OpError-Assertion Failed with OK: %A{t}"
                | Error(GrainConstructionError.ConstructionError err) ->
                    return err
                | Error(GrainConstructionError.SubjectAlreadyInitialized pKey) ->
                    return failwithf $"%s{callerInfo.Context}: %i{callerInfo.Line} - Construction OpError-Assertion Failed with SubjectAlreadyInitialized for pKey: %s{pKey}"
                | Error(GrainConstructionError.AccessDenied) ->
                    return failwithf $"%s{callerInfo.Context}: %i{callerInfo.Line} - Construction OpError-Assertion Failed with AccessDenied"
            }

    static member private thenAssertTransitionOpErrorWithCaller
            (callerInfo: CallerInfo)
            (res: ClusterOperation<Result<'T, GrainTransitionError<'Err>>>) : ClusterOperation<'Err> =
        fun cluster ->
            backgroundTask {
                match! res cluster with
                | Ok t ->
                    return failwithf $"%s{callerInfo.Context}: %i{callerInfo.Line} - Transition OpError-Assertion Failed with OK: %A{t}"
                | Error(GrainTransitionError.TransitionError err) ->
                    return err
                | Error(GrainTransitionError.TransitionNotAllowed) ->
                    return failwithf $"%s{callerInfo.Context}: %i{callerInfo.Line} - Transition OpError-Assertion Failed with TransitionNotAllowed"
                | Error(GrainTransitionError.SubjectNotInitialized pKey) ->
                    return failwithf $"%s{callerInfo.Context}: %i{callerInfo.Line} - Transition OpError-Assertion Failed with SubjectNotInitialized for pKey: %s{pKey}"
                | Error(GrainTransitionError.AccessDenied) ->
                    return failwithf $"%s{callerInfo.Context}: %i{callerInfo.Line} - Transition OpError-Assertion Failed with AccessDenied"
                | Error(GrainTransitionError.LockedInTransaction) ->
                    return failwithf $"%s{callerInfo.Context}: %i{callerInfo.Line} - Transition OpError-Assertion Failed with LockedInTransaction"
            }

    static member private thenAssertTransitionNotAllowedWithCaller
            (callerInfo: CallerInfo)
            (res: ClusterOperation<Result<'T, GrainTransitionError<'Err>>>) : ClusterOperation<unit> =
        fun cluster ->
            backgroundTask {
                let! r = res cluster
                match r with
                | Ok t ->
                    return failwithf $"%s{callerInfo.Context}: %i{callerInfo.Line} - Transition OpError-Assertion Failed with OK: %A{t}"
                | Error(GrainTransitionError.TransitionNotAllowed) ->
                    return ()
                | Error(GrainTransitionError.TransitionError err) ->
                    return failwithf $"%s{callerInfo.Context}: %i{callerInfo.Line} - Transition OpError-Assertion Failed with TransitionError: %A{err}"
                | Error(GrainTransitionError.SubjectNotInitialized pKey) ->
                    return failwithf $"%s{callerInfo.Context}: %i{callerInfo.Line} - Transition OpError-Assertion Failed with SubjectNotInitialized for pKey: %s{pKey}"
                | Error(GrainTransitionError.AccessDenied) ->
                    return failwithf $"%s{callerInfo.Context}: %i{callerInfo.Line} - Transition OpError-Assertion Failed with AccessDenied"
                | Error(GrainTransitionError.LockedInTransaction) ->
                    return failwithf $"%s{callerInfo.Context}: %i{callerInfo.Line} - Transition OpError-Assertion Failed with LockedInTransaction"
            }

    static member private thenAssertWithCaller
            (callerInfo: CallerInfo)
            (predicate: 'T -> bool)
            : ClusterOperation<'T> -> ClusterOperation<'T> =
        fun res ->
            fun cluster ->
                backgroundTask {
                    let! t= res cluster
                    if predicate t then
                        return t
                    else
                        return failwithf $"%s{callerInfo.Context}: %i{callerInfo.Line} - Assertion Failed for value %A{t}"
                }

    static member private thenAssertSomeWithCaller
            (callerInfo: CallerInfo)
            (res: ClusterOperation<Option<'T>>) : ClusterOperation<'T> =
        fun cluster ->
            backgroundTask {
                match! res cluster with
                | Some t ->
                    return t
                | None ->
                    return failwithf $"%s{callerInfo.Context}: %i{callerInfo.Line} - Option.Some-Assertion Failed for %s{typeof<'T>.FullName}"
            }

    static member private assertEventuallyDeletedWithCaller
            (callerInfo: CallerInfo)
            (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>)
            : 'SubjectId -> ClusterOperation<unit> =
        fun subjectId ->
            fun cluster ->
                backgroundTask {
                    // Task CEs, by design don't do tail recursion, so we'll use a mutable + while loop
                    let mutable doesExist = true
                    use scope = cluster.NewScope()
                    do! Ecosystem.waitForSystemStasis scope (Ecosystem.getStasisWaitFor cluster) cluster
                    match! scope.DefaultGrainConnector(cluster.Partition.GrainPartition).DoesSubjectExist lifeCycle.Definition subjectId with
                    | true ->
                        ()
                    | false ->
                        doesExist <- false

                    if not doesExist then
                        return ()
                    else
                        return failwithf $"%s{callerInfo.Context}: %i{callerInfo.Line} - Deleted Assertion Failed"
                }

    static member private thenAssertEventuallyDeletedWithCaller
            (callerInfo: CallerInfo)
            (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>)
            (clusterOp: ClusterOperation<'SubjectId>)
            : ClusterOperation<unit> =
        fun cluster ->
            backgroundTask {
                let! subjectId = clusterOp cluster
                return! Ecosystem.assertEventuallyDeletedWithCaller callerInfo lifeCycle subjectId cluster
            }

    static member private assertNoBadLogsWithCaller
            (callerInfo: CallerInfo)
            : ClusterOperation<unit> =
        fun cluster ->
            backgroundTask {
                match partitionIdToTestOutputHelper.TryGetValue cluster.Partition.GrainPartition with
                | (true, (_, counters)) ->
                    match counters.GetFailureAggregateStringIfNonZero() with
                    | Some str ->
                        return failwithf $"%s{callerInfo.Context}: %i{callerInfo.Line} - No Bad Logs assertion failed: %s{str}"
                    | None ->
                        Noop
                | _ ->
                    Noop
            }

    static member private thenAssertNoBadLogsWithCaller
            (callerInfo: CallerInfo)
            (pipeline: ClusterOperation<'T>) : ClusterOperation<'T> =
        fun cluster ->
            backgroundTask {
                let! res = pipeline cluster
                match partitionIdToTestOutputHelper.TryGetValue cluster.Partition.GrainPartition with
                | (true, (_, counters)) ->
                    match counters.GetFailureAggregateStringIfNonZero() with
                    | Some str ->
                        failwithf $"%s{callerInfo.Context}: %i{callerInfo.Line} - No Bad Logs assertion failed: %s{str}"
                    | None ->
                        Noop
                | _ ->
                    Noop
                return res
            }

    static member private thenAssertEventTriggeredWithCaller
            (callerInfo: CallerInfo)
            (actWaitResult: ClusterOperation<ActOrConstructAndWaitOnLifeEventResult<'Subject, 'SubjectId, 'LifeEvent>>)
            : ClusterOperation<'Subject * 'LifeEvent> =
        fun cluster ->
            backgroundTask {
                match! actWaitResult cluster with
                | LifeEventTriggered (finalVersionedSubj, lifeEvent) ->
                        return (finalVersionedSubj.Subject, lifeEvent)
                | WaitOnLifeEventTimedOut _ ->
                    return failwithf $"%s{callerInfo.Context}: %i{callerInfo.Line} - Wait on Life Event timed out"
            }

    static member private thenAssertEventWithCaller
            (callerInfo: CallerInfo)
            (mapEvent: 'LifeEvent -> Option<'T>)
            : ClusterOperation<ActOrConstructAndWaitOnLifeEventResult<'Subject, 'SubjectId, 'LifeEvent>> -> ClusterOperation<'Subject * 'T> =
        fun actWaitResult ->
        fun cluster ->
            backgroundTask {
                match! actWaitResult cluster with
                | LifeEventTriggered (finalVersionedSubj, lifeEvent) ->
                    match mapEvent lifeEvent with
                    | Some mapped ->
                        return (finalVersionedSubj.Subject, mapped)
                    | None ->
                        return failwithf $"%s{callerInfo.Context}: %i{callerInfo.Line} - Raised life event didn't match input event"
                | WaitOnLifeEventTimedOut _ ->
                    return failwithf $"%s{callerInfo.Context}: %i{callerInfo.Line} - Wait on Life Event timed out"
            }

    

    
    static member asyncAssertEventuallyWithin (waitFor, [<CallerMemberName>] ?context: string, [<CallerLineNumber>] ?line: int) =
        Ecosystem.asyncAssertEventuallyWithinWithCaller { Context = context.Value; Line = line.Value } waitFor
    
    static member assertEventually (lifeCycle, [<CallerMemberName>] ?context: string, [<CallerLineNumber>] ?line: int) =
        Ecosystem.assertEventuallyWithCaller { Context = context.Value; Line = line.Value } lifeCycle
            
    static member thenAssertEventually (lifeCycle, [<CallerMemberName>] ?context: string, [<CallerLineNumber>] ?line: int) =
        Ecosystem.thenAssertEventuallyWithCaller { Context = context.Value; Line = line.Value } lifeCycle

    static member assertEventuallyWithin (waitFor, [<CallerMemberName>] ?context: string, [<CallerLineNumber>] ?line: int) =
        Ecosystem.assertEventuallyWithinWithCaller { Context = context.Value; Line = line.Value } waitFor
                
    static member asyncAssertEventually (lifeCycle, [<CallerMemberName>] ?context: string, [<CallerLineNumber>] ?line: int) =
        Ecosystem.asyncAssertEventuallyWithCaller { Context = context.Value; Line = line.Value } lifeCycle

    static member thenAssertEventuallyWithin (waitFor: TimeSpan, [<CallerMemberName>] ?context: string, [<CallerLineNumber>] ?line: int) =
        Ecosystem.thenAssertEventuallyWithinWithCaller { Context = context.Value; Line = line.Value } waitFor

    static member thenAsyncAssertEventuallyWithin (waitFor: TimeSpan, [<CallerMemberName>] ?context: string, [<CallerLineNumber>] ?line: int) =
        Ecosystem.thenAsyncAssertEventuallyWithinWithCaller { Context = context.Value; Line = line.Value } waitFor

    static member thenAsyncAssertEventually (lifeCycle, [<CallerMemberName>] ?context: string, [<CallerLineNumber>] ?line: int) =
        Ecosystem.thenAsyncAssertEventuallyWithCaller { Context = context.Value; Line = line.Value } lifeCycle

    static member thenAssertOk (res, [<CallerMemberName>] ?context: string, [<CallerLineNumber>] ?line: int) =
        Ecosystem.thenAssertOkWithCaller { Context = context.Value; Line = line.Value } res

    static member thenAssertAllOk (clusterOp, [<CallerMemberName>] ?context: string, [<CallerLineNumber>] ?line: int) =
        Ecosystem.thenAssertAllOkWithCaller { Context = context.Value; Line = line.Value } clusterOp

    static member thenAssertConstructionOpError (res, [<CallerMemberName>] ?context: string, [<CallerLineNumber>] ?line: int) =
        Ecosystem.thenAssertConstructionOpErrorWithCaller { Context = context.Value; Line = line.Value } res

    static member thenAssertTransitionOpError (res, [<CallerMemberName>] ?context: string, [<CallerLineNumber>] ?line: int) =
        Ecosystem.thenAssertTransitionOpErrorWithCaller { Context = context.Value; Line = line.Value } res

    static member thenAssertTransitionNotAllowed (res, [<CallerMemberName>] ?context: string, [<CallerLineNumber>] ?line: int) =
        Ecosystem.thenAssertTransitionNotAllowedWithCaller { Context = context.Value; Line = line.Value } res

    static member thenAssert (predicate, [<CallerMemberName>] ?context: string, [<CallerLineNumber>] ?line: int) =
        Ecosystem.thenAssertWithCaller { Context = context.Value; Line = line.Value } predicate

    static member thenAssertSome (res, [<CallerMemberName>] ?context: string, [<CallerLineNumber>] ?line: int) =
        Ecosystem.thenAssertSomeWithCaller { Context = context.Value; Line = line.Value } res

    static member assertEventuallyDeleted (lifeCycle, [<CallerMemberName>] ?context: string, [<CallerLineNumber>] ?line: int) =
        Ecosystem.assertEventuallyDeletedWithCaller { Context = context.Value; Line = line.Value } lifeCycle

    static member thenAssertEventuallyDeleted (lifeCycle, [<CallerMemberName>] ?context: string, [<CallerLineNumber>] ?line: int) =
        Ecosystem.thenAssertEventuallyDeletedWithCaller { Context = context.Value; Line = line.Value } lifeCycle

    static member assertNoBadLogs ([<CallerMemberName>] ?context: string, [<CallerLineNumber>] ?line: int) =
        Ecosystem.assertNoBadLogsWithCaller { Context = context.Value; Line = line.Value }

    static member thenAssertNoBadLogs (pipeline, [<CallerMemberName>] ?context: string, [<CallerLineNumber>] ?line: int) =
        Ecosystem.thenAssertNoBadLogsWithCaller { Context = context.Value; Line = line.Value } pipeline

    static member thenAssertEventTriggered (actWaitResult, [<CallerMemberName>] ?context: string, [<CallerLineNumber>] ?line: int) =
        Ecosystem.thenAssertEventTriggeredWithCaller { Context = context.Value; Line = line.Value } actWaitResult

    static member thenAssertEvent (mapEvent, [<CallerMemberName>] ?context: string, [<CallerLineNumber>] ?line: int) =
        Ecosystem.thenAssertEventWithCaller { Context = context.Value; Line = line.Value } mapEvent