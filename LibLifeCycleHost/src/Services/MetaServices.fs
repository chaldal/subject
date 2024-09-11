module LibLifeCycleHost.MetaServices

open Microsoft.Data.SqlClient
open LibLifeCycle
open LibLifeCycle.LifeCycles.Meta
open LibLifeCycle.MetaServices
open LibLifeCycle.Views.Healthcheck
open LibLifeCycleCore
open LibLifeCycleHost
open LibLifeCycleHost.Storage.SqlServer
open LibLifeCycleHost.TelemetryModel
open Microsoft.Extensions.DependencyInjection
open Orleans
open System
open System.Threading.Tasks

let subjectDataMaintenanceManagerHandler (serviceProvider: IServiceProvider) (lifeCycleAdapterCollection: HostedLifeCycleAdapterCollection) (request: SubjectDataMaintenanceManager) : Task<ResponseVerificationToken> =
    match request with
    | RebuildIndicesBatch(lifeCycleName, maybeLastSubjectIdRebuilt, rebuildType, batchSize, responseChannel) ->
        backgroundTask {
            match lifeCycleAdapterCollection.GetLifeCycleAdapterByLocalName lifeCycleName with
            | None ->
                return IndexRebuildError.InvalidLifeCycle lifeCycleName |> Error |> responseChannel.Respond

            | Some lifeCycleAdapter ->
                let storageHandler = lifeCycleAdapter.GetGrainStorageHandler serviceProvider
                let! result = storageHandler.RebuildIndices lifeCycleName rebuildType maybeLastSubjectIdRebuilt batchSize
                return responseChannel.Respond (Ok result)
        }

    | ReEncodeSubjectsBatch (lifeCycleName, maybeLastSubjectIdReEncoded, batchSize, responseChannel) ->
        backgroundTask {
            match lifeCycleAdapterCollection.GetLifeCycleAdapterByLocalName lifeCycleName with
            | None ->
                return ReEncodeSubjectsError.InvalidLifeCycle lifeCycleName |> Error |> responseChannel.Respond
            | Some lifeCycleAdapter ->
                let storageHandler = lifeCycleAdapter.GetGrainStorageHandler serviceProvider
                let! result = storageHandler.ReEncodeSubjects lifeCycleName maybeLastSubjectIdReEncoded batchSize
                return responseChannel.Respond (Ok result)
        }

    | ReEncodeSubjectsHistoryBatch (lifeCycleName, maybeLastSubjectIdVersionReEncoded, batchSize, responseChannel) ->
        backgroundTask {
            match lifeCycleAdapterCollection.GetLifeCycleAdapterByLocalName lifeCycleName with
            | None ->
                return ReEncodeSubjectsError.InvalidLifeCycle lifeCycleName |> Error |> responseChannel.Respond
            | Some lifeCycleAdapter ->
                let storageHandler = lifeCycleAdapter.GetGrainStorageHandler serviceProvider
                let! result = storageHandler.ReEncodeSubjectsHistory lifeCycleName maybeLastSubjectIdVersionReEncoded batchSize
                return responseChannel.Respond (Ok result)
        }

    | RebuildTimersSubsBatch (lifeCycleName, cursor, batchSize, parallelism, repairGrainIdHash, responseChannel) ->
        backgroundTask {
            match lifeCycleAdapterCollection.GetLifeCycleAdapterByLocalName lifeCycleName with
            | None ->
                return TimersSubsRebuildError.InvalidLifeCycle lifeCycleName |> Error |> responseChannel.Respond
            | Some lifeCycleAdapter ->
                let! result = lifeCycleAdapter.RebuildTimersSubsBatch serviceProvider defaultGrainPartition cursor batchSize parallelism repairGrainIdHash
                return responseChannel.Respond (Ok result)
        }

    | ClearExpiredSubjectsHistoryBatch (lifeCycleName, now, batchSize, responseChannel) ->
        backgroundTask {
            match lifeCycleAdapterCollection.GetLifeCycleAdapterByLocalName lifeCycleName with
            | None ->
                return ClearExpiredSubjectsHistoryError.InvalidLifeCycle lifeCycleName |> Error |> responseChannel.Respond
            | Some lifeCycleAdapter ->
                let storageHandler = lifeCycleAdapter.GetGrainStorageHandler serviceProvider
                let! result = storageHandler.ClearExpiredSubjectsHistory now batchSize
                return responseChannel.Respond (Ok result)
        }

let metricsReporterHandler (serviceProvider: IServiceProvider) (ecosystemName: string) (lifeCycleAdapterCollection: HostedLifeCycleAdapterCollection) (request: MetricsReporter) : Task<ResponseVerificationToken> =
    let operationTracker = serviceProvider.GetRequiredService<OperationTracker>()

    let metricName       metric lifeCycleName    = sprintf "%s_%s_%s"    metric ecosystemName lifeCycleName
    let metricNameWithId metric lifeCycleName id = sprintf "%s_%s_%s_%s" metric ecosystemName lifeCycleName id

    match request with
    | ReportSideEffectMetrics(lifeCycleName, responseChannel) ->
        backgroundTask {
            match lifeCycleAdapterCollection.GetLifeCycleAdapterByLocalName lifeCycleName with
            | None ->
                return ReportMetricsError.InvalidLifeCycle lifeCycleName |> Error |> responseChannel.Respond

            | Some lifeCycleAdapter ->
                let storageHandler = lifeCycleAdapter.GetGrainStorageHandler serviceProvider

                match! storageHandler.GetSideEffectMetrics() with
                | None -> ()
                | Some metrics ->
                    operationTracker.SendMetric (metricName "SideEffectOldestAgeSecs" lifeCycleName) metrics.OldestAgeOfNonFailed.TotalSeconds
                    operationTracker.SendMetric (metricName "SideEffectQueueLength"   lifeCycleName) (float metrics.QueueLength)
                    operationTracker.SendMetric (metricName "SideEffectErrorCount"    lifeCycleName) (float metrics.FailureErrorCount)
                    operationTracker.SendMetric (metricName "SideEffectWarningCount"  lifeCycleName) (float metrics.FailureWarningCount)

                return responseChannel.Respond(Ok ())
        }

    | ReportTimerMetrics(lifeCycleName, responseChannel) ->
        backgroundTask {
            match lifeCycleAdapterCollection.GetLifeCycleAdapterByLocalName lifeCycleName with
            | None ->
                return ReportMetricsError.InvalidLifeCycle lifeCycleName |> Error |> responseChannel.Respond

            | Some lifeCycleAdapter ->
                let storageHandler = lifeCycleAdapter.GetGrainStorageHandler serviceProvider

                match! storageHandler.GetTimerMetrics() with
                | None -> ()
                | Some {OldestAge = oldestAge; ExpiredCount = expiredCount} ->
                    operationTracker.SendMetric (metricName "TimerOldestAgeSecs" lifeCycleName) oldestAge.TotalSeconds
                    operationTracker.SendMetric (metricName "TimerExpiredCount"  lifeCycleName) (float expiredCount)

                return responseChannel.Respond(Ok ())
        }

    // TODO should this handler be moved out of MetaServices as other lifecycles will use ReportCustomMetrics?
    | ReportCustomMetrics(lifeCycleName, idStr, customMetrics, responseChannel) ->
        backgroundTask {
            customMetrics
            |> Map.iter ( fun name value -> operationTracker.SendMetric (metricNameWithId name lifeCycleName idStr) value )
            return responseChannel.Respond(Ok ())
        }


let ecosystemHealthManagerHandler
    (serviceProvider: IServiceProvider) (lifeCycleAdapterCollection: HostedLifeCycleAdapterCollection) (config: SqlServerConnectionStrings) (grainPartition: GrainPartition)
    (request: EcosystemHealthManager)
    : Task<ResponseVerificationToken> =
    match request with
    | ProcessStalledSideEffectsBatch (lifeCycleName, batchSize, responseChannel) ->
        backgroundTask {
            match lifeCycleAdapterCollection.GetLifeCycleAdapterByLocalName lifeCycleName with
            | None ->
                return StalledSideEffectsError.InvalidLifeCycle lifeCycleName |> Error |> responseChannel.Respond

            | Some lifeCycleAdapter ->
                let storage = lifeCycleAdapter.GetGrainStorageHandler serviceProvider
                let hostEcosystemGrainFactory = serviceProvider.GetRequiredService<IGrainFactory>()

                let! ids = storage.GetIdsOfSubjectsWithStalledSideEffects batchSize

                do! ids
                    |> Seq.map (lifeCycleAdapter.ForceAwake hostEcosystemGrainFactory defaultGrainPartition)
                    |> Task.WhenAll

                return
                    ids
                    |> Seq.sort
                    |> Seq.tryLast
                    |> Ok
                    |> responseChannel.Respond
        }

    | UpdatePermanentFailures (lifeCycleName, scope, filters, operation, responseChannel) ->
        backgroundTask {
            match lifeCycleAdapterCollection.GetLifeCycleAdapterByLocalName lifeCycleName with
            | None ->
                return PermanentFailuresUpdateError.InvalidLifeCycle lifeCycleName |> Error |> responseChannel.Respond

            | Some lifeCycleAdapter ->
                let hostEcosystemGrainFactory = serviceProvider.GetRequiredService<IGrainFactory>()
                let! result = lifeCycleAdapter.UpdatePermanentFailures serviceProvider hostEcosystemGrainFactory grainPartition scope filters operation
                return responseChannel.Respond result
        }

    | PerformEcosystemHealthcheck (now, responseHandler) ->
        let (HostedLifeCycleAdapterCollection (ecosystemName, _)) = lifeCycleAdapterCollection
        let notManyErrorsThreshold = 10u
        let notManyOverdueTimersThreshold = 500u
        let criticallyOldOverdueTimerThreshold = TimeSpan.FromMinutes 30.
        backgroundTask {
            try
                use connection = new SqlConnection(config.ForEcosystem ecosystemName)
                do! connection.OpenAsync()
                use command = connection.CreateCommand()
                let sql =
                    $"SELECT ISNULL(SUM(StalledCount), 0), ISNULL(STRING_AGG(Subj, ';'), '') FROM
                    (
                        SELECT Subj, COUNT(*) AS StalledCount
                        FROM [{ecosystemName}].[AllStalledPrepared]
                        GROUP BY Subj
                    ) x

                    SELECT ISNULL(SUM(WarningCount), 0), ISNULL(STRING_AGG(Subj, ';'), '') FROM
                    (
                        SELECT Subj, COUNT(*) AS WarningCount
                        FROM [{ecosystemName}].[AllFailedSideEffects]
                        WHERE FailureSeverity = 1 AND (FailureAckedUntil IS NULL OR [FailureAckedUntil] < GETUTCDATE())
                        GROUP BY Subj
                    ) x

                    SELECT ISNULL(SUM(ErrorCount), 0), ISNULL(STRING_AGG(Subj, ';'), '') FROM
                    (
                        SELECT Subj, COUNT(*) AS ErrorCount
                        FROM [{ecosystemName}].[AllFailedSideEffects]
                        WHERE FailureSeverity = 2 AND (FailureAckedUntil IS NULL OR [FailureAckedUntil] < GETUTCDATE())
                        GROUP BY Subj
                    ) x

                    SELECT ISNULL(SUM(StalledCount), 0), ISNULL(STRING_AGG(Subj, ';'), '') FROM
                    (
                        SELECT Subj, COUNT(*) AS StalledCount
                        FROM [{ecosystemName}].[AllStalledSideEffects]
                        GROUP BY Subj
                    ) x

                    SELECT ISNULL(SUM(StalledCount), 0), ISNULL(STRING_AGG(Subj, ';'), ''), MIN(OldestNextTickOn) AS OldestNextTickOn FROM
                    (
                        SELECT Subj, COUNT(*) AS StalledCount, MIN(NextTickOn) AS OldestNextTickOn
                        FROM [{ecosystemName}].[AllStalledTimers]
                        GROUP BY Subj
                    ) x"

                command.CommandText <- sql
                use! reader = command.ExecuteReaderAsync()
                let! _ = reader.ReadAsync()

                let subjectTransactionStalledPreparedCount = reader.GetInt32 0 |> uint32
                let subjectTransactionStalledPreparedSubjects = reader.GetString 1

                let! _ = reader.NextResultAsync()
                let! _ = reader.ReadAsync()
                let sideEffectsWarningsCount = reader.GetInt32 0 |> uint32
                let sideEffectWarningsSubjects = reader.GetString 1

                let! _ = reader.NextResultAsync()
                let! _ = reader.ReadAsync()
                let sideEffectsErrorCount = reader.GetInt32 0 |> uint32
                let sideEffectErrorsSubjects = reader.GetString 1

                let! _ = reader.NextResultAsync()
                let! _ = reader.ReadAsync()
                let sideEffectsStalledCount = reader.GetInt32 0 |> uint32
                let sideEffectStalledSubjects = reader.GetString 1

                let! _ = reader.NextResultAsync()
                let! _ = reader.ReadAsync()
                let timersStalledCount = reader.GetInt32 0 |> uint32
                let timersStalledSubjects = reader.GetString 1
                let timersStalledOldestNextTickOn = if reader.IsDBNull 2 then DateTimeOffset.MinValue else reader.GetDateTimeOffset 2

                let allIssues =
                    [
                        if sideEffectsWarningsCount > 0u then
                            EcosystemHealthcheckProductionIssue.SideEffectsWarnings (sideEffectsWarningsCount, sideEffectWarningsSubjects) |> Choice1Of2

                        if sideEffectsErrorCount = 0u then
                            ()
                        elif sideEffectsErrorCount < notManyErrorsThreshold then
                            EcosystemHealthcheckProductionIssue.SideEffectsNotTooManyFailures (byte sideEffectsErrorCount, sideEffectErrorsSubjects) |> Choice1Of2
                        else
                            EcosystemHealthcheckCriticalAlarm.SideEffectsManyFailures (sideEffectsErrorCount, sideEffectErrorsSubjects) |> Choice2Of2

                        if sideEffectsStalledCount > 0u then
                            EcosystemHealthcheckCriticalAlarm.SideEffectsStalled (sideEffectsStalledCount, sideEffectStalledSubjects) |> Choice2Of2

                        if subjectTransactionStalledPreparedCount > 0u then
                            EcosystemHealthcheckCriticalAlarm.SubjectTransactionStalledPrepared (subjectTransactionStalledPreparedCount, subjectTransactionStalledPreparedSubjects) |> Choice2Of2

                        if timersStalledCount = 0u then
                            ()
                        elif timersStalledCount < notManyOverdueTimersThreshold && (timersStalledOldestNextTickOn.Add criticallyOldOverdueTimerThreshold) > now then
                            EcosystemHealthcheckProductionIssue.TimersOverdueNotSeverely (uint16 timersStalledCount, timersStalledOldestNextTickOn, timersStalledSubjects) |> Choice1Of2
                        else
                            EcosystemHealthcheckCriticalAlarm.TimersOverdueSeverely (timersStalledCount, timersStalledOldestNextTickOn, timersStalledSubjects) |> Choice2Of2
                    ]

                let productionIssues = allIssues |> List.choose (function | Choice1Of2 x -> Some x | Choice2Of2 _ -> None)
                let criticalAlarms   = allIssues |> List.choose (function | Choice2Of2 x -> Some x | Choice1Of2 _ -> None)

                let result =
                    match criticalAlarms, productionIssues with
                    | [], [] ->
                        EcosystemHealthcheckResult.AllClear
                    | _ :: _, _ ->
                        EcosystemHealthcheckResult.CriticalAlarms (criticalAlarms, productionIssues)
                    | [], _ :: _ ->
                        EcosystemHealthcheckResult.ProductionIssues productionIssues

                return responseHandler.Respond (Ok result)
            with
            | ex ->
                return responseHandler.Respond (Error (ex.ToString()))
        }