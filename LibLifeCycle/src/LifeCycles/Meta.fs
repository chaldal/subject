[<AutoOpen>]
module LibLifeCycle.LifeCycles.Meta.LifeCycle

open System
open System.Threading.Tasks
open LibLifeCycle
open LibLifeCycle.MetaServices

type MetaEnvironment = {
    Clock: Service<Clock>
    DataMaintenanceManager: Service<SubjectDataMaintenanceManager>
    EcosystemHealthManager: Service<EcosystemHealthManager>
    MetricsReporter:             Service<MetricsReporter>
} with interface Env

let private transition (env: MetaEnvironment) (meta: Meta) (action: MetaAction) : TransitionResult<Meta, MetaAction, MetaOpError, MetaLifeEvent, MetaConstructor> =
    transition {
        match action with
        | StartIndexRebuild (indexRebuildType, skipToIdInclusive, batchSize) ->
            match meta.IndexRebuildOp with
            | None ->
                let! now = env.Clock.Query Now

                yield RunNextIndexRebuildBatch

                return {
                    meta with
                        IndexRebuildOp = Some {
                            RebuildType    = indexRebuildType
                            StartedOn      = now
                            LastUpdatedOn  = now
                            LastSubjectIdRebuilt = skipToIdInclusive
                            BatchSize      = batchSize |> Option.defaultValue 50us
                        }
                }

            | Some _ ->
                return MetaOpError.IndexRebuildAlreadyInProgress

        | RunNextIndexRebuildBatch ->
            match meta.IndexRebuildOp with
            | None ->
                return Transition.Ignore

            | Some rebuildOp ->
                let (MetaId lifeCycleName) = meta.Id

                let! resNextRebuildOp = env.DataMaintenanceManager.Query RebuildIndicesBatch lifeCycleName rebuildOp.LastSubjectIdRebuilt rebuildOp.RebuildType rebuildOp.BatchSize

                let! now = env.Clock.Query Now

                let (maybeNextRebuildOp, maybeLastRebuildOp) =
                    match resNextRebuildOp with
                    | Ok (RebuildIndicesBatchResult.CompletedBatch lastRebuiltKey) ->
                        let nextRebuildOp = {
                            rebuildOp with
                                LastSubjectIdRebuilt = Some lastRebuiltKey
                                LastUpdatedOn  = now
                        }
                        ((Some nextRebuildOp), meta.LastIndexRebuildOp)

                    | Ok RebuildIndicesBatchResult.SubjectUpdatedConcurrentlyTryAgain ->
                        let nextRebuildOp = {
                            rebuildOp with
                                LastSubjectIdRebuilt = rebuildOp.LastSubjectIdRebuilt
                                LastUpdatedOn  = now
                        }
                        ((Some nextRebuildOp), meta.LastIndexRebuildOp)

                    | Ok RebuildIndicesBatchResult.CompletedBatchNoMoreBatchesPending ->
                        let completedData = {
                            FinalBatchSize = rebuildOp.BatchSize
                            CompletedOn    = now
                        }

                        (None, Some {
                            RebuildType = rebuildOp.RebuildType
                            StartedOn   = now
                            Result      = Ok completedData
                        })

                    | Error rebuildErr ->
                        (None, Some {
                            RebuildType = rebuildOp.RebuildType
                            StartedOn   = now
                            Result      = Error rebuildErr
                        })

                yield maybeNextRebuildOp |> Option.map (fun _ -> RunNextIndexRebuildBatch)

                return {
                    meta with
                        IndexRebuildOp     = maybeNextRebuildOp
                        LastIndexRebuildOp = maybeLastRebuildOp
                }

        | ForceStopIndexRebuild ->
            match meta.IndexRebuildOp with
            | None ->
                return Transition.Ignore
            | Some _ ->
                return { meta with IndexRebuildOp = None }

        | StartTimersSubsRebuild (repairGrainIdHash, skipToIdInclusive, batchSize, parallelism) ->
            match meta.TimersSubsRebuildOp with
            | None ->
                let! now = env.Clock.Query Now

                yield RunNextTimersSubsRebuildBatch

                return {
                    meta with
                        TimersSubsRebuildOp = Some {
                            RepairGrainIdHash    = repairGrainIdHash
                            StartedOn            = now
                            LastUpdatedOn        = now
                            LastSubjectIdRebuilt = skipToIdInclusive
                            LastError            = None
                            BatchSize            = batchSize |> Option.defaultValue 10us // potentially must block until all batch grains are awoken, so conservative BatchSize
                            Parallelism          = parallelism |> Option.defaultValue 1us
                        }
                }

            | Some _ ->
                return MetaOpError.TimersSubsRebuildAlreadyInProgress

        | RunNextTimersSubsRebuildBatch ->
            match meta.TimersSubsRebuildOp with
            | None ->
                return Transition.Ignore

            | Some rebuildOp ->
                let (MetaId lifeCycleName) = meta.Id

                let! result = env.DataMaintenanceManager.Query RebuildTimersSubsBatch lifeCycleName rebuildOp.LastSubjectIdRebuilt rebuildOp.BatchSize rebuildOp.Parallelism rebuildOp.RepairGrainIdHash

                let! now = env.Clock.Query Now

                let updatedRebuildOp =
                    match result with
                    | Error err ->
                        Some {
                            rebuildOp with
                                LastUpdatedOn = now
                                LastError = Some err
                        }
                    | Ok result ->
                        match result with
                        | RebuildTimersSubsBatchResult.CompletedBatchNoMoreBatchesPending ->
                            None // no more Id's, rebuild all done :)
                        | RebuildTimersSubsBatchResult.CompletedBatch lastRebuiltKey ->
                            Some {
                                rebuildOp with
                                    LastUpdatedOn = now
                                    LastSubjectIdRebuilt = Some lastRebuiltKey
                                    LastError = None
                            }

                match meta.TimersSubsRebuildOp with
                | Some { LastError = _ } ->
                    yield RunNextTimersSubsRebuildBatch
                | _ -> ()

                return {
                    meta with
                        TimersSubsRebuildOp = updatedRebuildOp
                }

        | ForceStopTimersSubsRebuild ->
            match meta.TimersSubsRebuildOp with
            | None ->
                return Transition.Ignore
            | Some _ ->
                return { meta with TimersSubsRebuildOp = None }

        | StartReEncodeSubjects (skipToIdInclusive, batchSize) ->
            match meta.ReEncodeSubjectsOp with
            | None ->
                let! now = env.Clock.Query Now

                yield RunNextReEncodeSubjectsBatch

                return {
                    meta with
                        ReEncodeSubjectsOp = Some {
                            StartedOn = now
                            LastUpdatedOn = now
                            LastSubjectIdReEncoded = skipToIdInclusive
                            LastError = None
                            BatchSize = batchSize |> Option.defaultValue 10us
                        }
                }

            | Some _ ->
                return MetaOpError.ReEncodeAlreadyInProgress

        | RunNextReEncodeSubjectsBatch ->
            match meta.ReEncodeSubjectsOp with
            | None ->
                return Transition.Ignore

            | Some reEncodeOp ->
                let (MetaId lifeCycleName) = meta.Id

                let! resNextReEncodeOp = env.DataMaintenanceManager.Query ReEncodeSubjectsBatch lifeCycleName reEncodeOp.LastSubjectIdReEncoded reEncodeOp.BatchSize

                let! now = env.Clock.Query Now

                let maybeNextReEncodeOp =
                    match resNextReEncodeOp with
                    | Ok (ReEncodeSubjectsBatchResult.CompletedBatch lastReEncodedId) ->
                        Some { reEncodeOp with LastSubjectIdReEncoded = Some lastReEncodedId; LastUpdatedOn = now }
                    | Ok ReEncodeSubjectsBatchResult.SubjectUpdatedConcurrentlyTryAgain ->
                        Some { reEncodeOp with LastSubjectIdReEncoded = reEncodeOp.LastSubjectIdReEncoded; LastUpdatedOn = now }
                    | Ok ReEncodeSubjectsBatchResult.CompletedBatchNoMoreBatchesPending ->
                        None
                    | Error err ->
                        Some { reEncodeOp with LastUpdatedOn = now; LastError = Some err }

                match maybeNextReEncodeOp with
                | Some { LastError = None } ->
                    yield RunNextReEncodeSubjectsBatch
                | _ -> ()

                return { meta with ReEncodeSubjectsOp = maybeNextReEncodeOp }

        | ForceStopReEncodeSubjects ->
            match meta.ReEncodeSubjectsOp with
            | None ->
                return Transition.Ignore
            | Some _ ->
                return { meta with ReEncodeSubjectsOp = None }

        | StartReEncodeSubjectsHistory (skipToIdVersionInclusive, batchSize) ->
            match meta.ReEncodeSubjectsHistoryOp with
            | None ->
                let! now = env.Clock.Query Now

                yield RunNextReEncodeSubjectsHistoryBatch

                return {
                    meta with
                        ReEncodeSubjectsHistoryOp = Some {
                            StartedOn = now
                            LastUpdatedOn = now
                            LastSubjectIdVersionReEncoded = skipToIdVersionInclusive
                            LastError = None
                            BatchSize = batchSize |> Option.defaultValue 20us
                        }
                }

            | Some _ ->
                return MetaOpError.ReEncodeAlreadyInProgress

        | RunNextReEncodeSubjectsHistoryBatch ->
            match meta.ReEncodeSubjectsHistoryOp with
            | None ->
                return Transition.Ignore

            | Some reEncodeOp ->
                let (MetaId lifeCycleName) = meta.Id

                let! resNextReEncodeOp = env.DataMaintenanceManager.Query ReEncodeSubjectsHistoryBatch lifeCycleName reEncodeOp.LastSubjectIdVersionReEncoded reEncodeOp.BatchSize

                let! now = env.Clock.Query Now

                let maybeNextReEncodeOp =
                    match resNextReEncodeOp with
                    | Ok (ReEncodeSubjectsHistoryBatchResult.CompletedBatch (lastReEncodedId, lastReEncodedVersion)) ->
                        Some { reEncodeOp with LastSubjectIdVersionReEncoded = Some (lastReEncodedId, lastReEncodedVersion); LastUpdatedOn = now }
                    | Ok ReEncodeSubjectsHistoryBatchResult.CompletedBatchNoMoreBatchesPending ->
                        None
                    | Error err ->
                        Some { reEncodeOp with LastUpdatedOn = now; LastError = Some err }

                match maybeNextReEncodeOp with
                | Some { LastError = None } ->
                    yield RunNextReEncodeSubjectsHistoryBatch
                | _ -> ()

                return { meta with ReEncodeSubjectsHistoryOp = maybeNextReEncodeOp }

        | ForceStopReEncodeSubjectsHistory ->
            match meta.ReEncodeSubjectsHistoryOp with
            | None ->
                return Transition.Ignore
            | Some _ ->
                return { meta with ReEncodeSubjectsHistoryOp = None }

        | StalledSideEffectsCheck ->
            let (MetaId lifeCycleName) = meta.Id

            let! result = env.EcosystemHealthManager.Query ProcessStalledSideEffectsBatch lifeCycleName 50us

            let! now = env.Clock.Query Now

            let updateLastChecked error (meta: Meta) =
                { meta with StalledSideEffects = { meta.StalledSideEffects with LastChecked = Some { On = now; Error = error } } }

            let updateLastDetected idStr (meta: Meta) =
                { meta with StalledSideEffects = { meta.StalledSideEffects with LastDetected = Some { On = now; IdStr = idStr } } }

            match result, meta.StalledSideEffects.LastDetected with
            // nothing new detected
            | Ok None, _ ->
                return updateLastChecked None meta

            // detected same stalled subject Id within a minute since last check: leave it alone, must be a lot of backpressure
            | Ok (Some lastIdStr), Some lastDetected when lastIdStr = lastDetected.IdStr && now < lastDetected.On.Add (TimeSpan.FromMinutes 1.) ->
                return updateLastChecked None meta

            | Ok (Some lastIdStr), _ ->
                return (meta |> updateLastChecked None |> updateLastDetected lastIdStr)

            | Error error, _ ->
                return (meta |> updateLastChecked (Some error))

        | RunClearExpiredSubjectsHistoryBatch ->
            let (MetaId lifeCycleName) = meta.Id

            let! now = env.Clock.Query Now
            // adjust batch size if results in too much of slow SQL telemetry > 1 sec
            let batchSize = 1000us
            let! result = env.DataMaintenanceManager.Query ClearExpiredSubjectsHistoryBatch lifeCycleName now batchSize

            let! now = env.Clock.Query Now
            let updateLastChecked error (meta: Meta) =
                { meta with ClearExpiredSubjectsHistory = { meta.ClearExpiredSubjectsHistory with LastChecked = Some { On = now; Error = error } } }
            let updateLastDetected (meta: Meta) =
                { meta with ClearExpiredSubjectsHistory = { meta.ClearExpiredSubjectsHistory with LastDetectedOn = Some now } }

            match result, meta.ClearExpiredSubjectsHistory.LastDetectedOn with
            | Ok clearedCount, _ ->
                if clearedCount = 0us then
                    return updateLastChecked None meta
                else
                    yield RunClearExpiredSubjectsHistoryBatch
                    return (meta |> updateLastChecked None |> updateLastDetected)

            | Error error, _ ->
                return (meta |> updateLastChecked (Some error))

        | ReportMetrics ->
            let (MetaId lifeCycleName) = meta.Id

            do! [
                    env.MetricsReporter.Query ReportSideEffectMetrics lifeCycleName |> Task.Ignore
                    env.MetricsReporter.Query ReportTimerMetrics lifeCycleName  |> Task.Ignore
                ]
                |> Task.WhenAll
                |> Task.asUnit

            let! now = env.Clock.Query Now
            return {
                meta with LastMetricsReportOn = Some now
            }

        | UpdatePermanentFailure (scope, filters, operation) ->
            let (MetaId lifeCycleName) = meta.Id
            match! env.EcosystemHealthManager.Query UpdatePermanentFailures lifeCycleName scope filters operation with
            | Ok lastRes ->
                let! now = env.Clock.Query Now
                return { meta with LastUpdatePermanentFailures = Some (now, lastRes) }
            | Error (PermanentFailuresUpdateError.InvalidLifeCycle invalidLifeCycleName) ->
                return MetaOpError.InvalidLifeCycle invalidLifeCycleName
            | Error PermanentFailuresUpdateError.NoPermanentFailuresFound ->
                return MetaOpError.NoPermanentFailuresFound
    }

let private idGeneration (_: MetaEnvironment) (ctor: MetaConstructor) : IdGenerationResult<MetaId, MetaOpError> =
    idgen {
        match ctor with
        | New metaId -> return metaId
    }

let private construction<'Subject, 'SubjectId when 'Subject :> Subject<'SubjectId> and 'SubjectId :> SubjectId and 'SubjectId : comparison> (env: MetaEnvironment) (id: MetaId) (ctor: MetaConstructor) : ConstructionResult<Meta, MetaAction, MetaOpError, MetaLifeEvent> =
    construction {
        match ctor with
        | New _ ->
            let! now = env.Clock.Query Now
            return {
                Id                          = id
                CreatedOn                   = now
                IndexRebuildOp              = None
                LastIndexRebuildOp          = None
                TimersSubsRebuildOp         = None
                ReEncodeSubjectsOp          = None
                ReEncodeSubjectsHistoryOp   = None
                ClearExpiredSubjectsHistory = { LastDetectedOn = None; LastChecked = None }
                StalledSideEffects          = { LastDetected = None; LastChecked = None }
                LastMetricsReportOn         = None
                LastUpdatePermanentFailures = None
            }
    }

let private timers (meta: Meta) : list<Timer<MetaAction>> =
    [
        {
            TimerAction = TimerAction.RunAction MetaAction.StalledSideEffectsCheck
            Schedule =
                match
                    meta.StalledSideEffects.LastDetected, meta.StalledSideEffects.LastChecked with
                | _, None ->
                    // never checked, do asap
                    Schedule.Now
                | Some detected, Some checked' when detected.On = checked'.On ->
                     // just detected, try again asap
                     Schedule.Now
                 | maybeDetected, Some checked' ->
                     // nothing new detected, exponential backoff
                     maybeDetected
                     |> Option.map (fun detected -> checked'.On.Subtract detected.On)
                     |> Option.defaultValue (TimeSpan.FromMinutes 4.0)
                     |> min (TimeSpan.FromMinutes 4.0)
                     |> max (TimeSpan.FromSeconds 10.0)
                     |> checked'.On.Add
                     |> Schedule.On
        }

        let clearExpiredSubjectsTimer schedule =
            { TimerAction = TimerAction.RunAction MetaAction.RunClearExpiredSubjectsHistoryBatch; Schedule = schedule }

        match meta.ClearExpiredSubjectsHistory.LastDetectedOn, meta.ClearExpiredSubjectsHistory.LastChecked with
        | _, None ->
            // never checked, do asap
            clearExpiredSubjectsTimer Schedule.Now
        | Some detectedOn, Some checked' when detectedOn = checked'.On ->
             // just detected, do nothing, previous action will continue itself
             ()
        | maybeDetected, Some checked' ->
            // nothing new detected, exponential backoff 1 min to 1h
            maybeDetected
            |> Option.map (fun detectedOn -> checked'.On.Subtract detectedOn)
            |> Option.defaultValue (TimeSpan.FromHours 1.0)
            |> min (TimeSpan.FromHours 1.0)
            |> max (TimeSpan.FromMinutes 1.0)
            |> checked'.On.Add
            |> Schedule.On
            |> clearExpiredSubjectsTimer

        let reportPeriod = TimeSpan.FromMinutes 1.
        {
            TimerAction = TimerAction.RunAction MetaAction.ReportMetrics
            Schedule =
                match meta.LastMetricsReportOn with
                | None -> Schedule.Now
                | Some last -> Schedule.On (last.Add reportPeriod)
        }

        // piggyback on stalled side effect check to detect Meta of removed life cycles and delete them
        match meta.StalledSideEffects.LastChecked |> Option.bind (fun last -> last.Error) with
        | Some (StalledSideEffectsError.InvalidLifeCycle _) ->
            { TimerAction = TimerAction.DeleteSelf; Schedule = Schedule.Now }
        | None ->
            ()

        match meta.ClearExpiredSubjectsHistory.LastChecked |> Option.bind (fun last -> last.Error) with
        | Some (ClearExpiredSubjectsHistoryError.InvalidLifeCycle _) ->
            { TimerAction = TimerAction.DeleteSelf; Schedule = Schedule.Now }
        | None ->
            ()
    ]

let private indices (meta: Meta) =
    indices {
        meta.IndexRebuildOp
        |> Option.map (fun op -> op.LastSubjectIdRebuilt |> Option.defaultValue "")
        |> Option.map MetaStringIndex.RebuildingIndices

        meta.TimersSubsRebuildOp
        |> Option.map (fun op -> op.LastSubjectIdRebuilt |> Option.defaultValue "")
        |> Option.map MetaStringIndex.RebuildingTimersAndSubs

        meta.ReEncodeSubjectsOp
        |> Option.map (fun op -> op.LastSubjectIdReEncoded |> Option.defaultValue "")
        |> Option.map MetaStringIndex.ReEncodingSubjects

        meta.ReEncodeSubjectsHistoryOp
        |> Option.map (fun op -> op.LastSubjectIdVersionReEncoded |> Option.defaultValue ("", 0UL))
        |> Option.map MetaStringIndex.ReEncodingSubjectsHistory
    }

let private shouldSendTelemetry =
    function
    | ShouldSendTelemetryFor.LifeAction action ->
        match action with
        | MetaAction.ReportMetrics
        | MetaAction.RunNextIndexRebuildBatch
        | MetaAction.RunNextTimersSubsRebuildBatch
        | MetaAction.RunNextReEncodeSubjectsBatch
        | MetaAction.StalledSideEffectsCheck
        | MetaAction.RunClearExpiredSubjectsHistoryBatch
        | MetaAction.RunNextReEncodeSubjectsHistoryBatch ->
            false
        | MetaAction.StartIndexRebuild _
        | MetaAction.ForceStopIndexRebuild
        | MetaAction.StartTimersSubsRebuild _
        | MetaAction.ForceStopTimersSubsRebuild
        | MetaAction.StartReEncodeSubjects _
        | MetaAction.ForceStopReEncodeSubjects
        | MetaAction.StartReEncodeSubjectsHistory _
        | MetaAction.ForceStopReEncodeSubjectsHistory
        | MetaAction.UpdatePermanentFailure _ ->
            true
    | ShouldSendTelemetryFor.Constructor _
    | ShouldSendTelemetryFor.LifeEvent _ ->
        true

type MetaLifeCycle<'Session, 'Role when 'Role : comparison> =
        LifeCycle<Meta, MetaAction, MetaOpError, MetaConstructor, MetaLifeEvent, MetaIndex, MetaId, AccessPredicateInput, 'Session, 'Role, MetaEnvironment>

let metaLifeCycle<'Session, 'Role when 'Role :  comparison>
    (metaLifeCycleDef: MetaLifeCycleDef)
    : MetaLifeCycle<'Session, 'Role> =
    LifeCycleBuilder.newLifeCycle          metaLifeCycleDef
    |> LifeCycleBuilder.withApiAccessRestrictedToRootOnly
    |> LifeCycleBuilder.withTransition     transition
    |> LifeCycleBuilder.withIdGeneration   idGeneration
    |> LifeCycleBuilder.withConstruction   construction
    |> LifeCycleBuilder.withIndices        indices
    |> LifeCycleBuilder.withTimers         timers
    |> LifeCycleBuilder.withTelemetryRules shouldSendTelemetry
    |> LifeCycleBuilder.withStorage
           (StorageType.Persistent
                (PromotedIndicesConfig.Empty,
                System.TimeSpan.FromDays 120. // expire history of _Meta after a couple of months
                |> PersistentHistoryExpiration.AfterSubjectChange |> Some
                |> PersistentHistoryRetention.FilteredByTelemetryRules))
    |> LifeCycleBuilder.build