module LibLifeCycle.MetaServices

open LibLifeCycle
open LibLifeCycle.LifeCycles.Meta
open LibLifeCycle.Views.Healthcheck
open System

type SubjectDataMaintenanceManager =
| RebuildIndicesBatch of LifeCycleName: string * MaybeLastSubjectIdRebuilt: Option<string> * RebuildType: IndexRebuildType * BatchSize: uint16 * ResponseChannel<Result<RebuildIndicesBatchResult, IndexRebuildError>>
| ReEncodeSubjectsBatch of LifeCycleName: string * LastSubjectIdProcessed: Option<string>               * BatchSize: uint16 * ResponseChannel<Result<ReEncodeSubjectsBatchResult, ReEncodeSubjectsError>>
| ReEncodeSubjectsHistoryBatch of LifeCycleName: string * LastSubjectIdVersionRebuilt: Option<string * uint64> * BatchSize: uint16 * ResponseChannel<Result<ReEncodeSubjectsHistoryBatchResult, ReEncodeSubjectsError>>
| RebuildTimersSubsBatch of LifeCycleName: string * LastSubjectIdRebuilt: Option<string> * BatchSize: uint16 * Parallelism: uint16 * RepairGrainIdHash: bool * ResponseChannel<Result<RebuildTimersSubsBatchResult, TimersSubsRebuildError>>
| ClearExpiredSubjectsHistoryBatch of LifeCycleName: string * Now: DateTimeOffset * BatchSize: uint16 * ResponseChannel<Result<uint16, ClearExpiredSubjectsHistoryError>>
with interface Request

[<RequireQualifiedAccess>]
type PermanentFailuresUpdateError =
| InvalidLifeCycle of LifeCycleName: string
| NoPermanentFailuresFound

type EcosystemHealthManager =
| PerformEcosystemHealthcheck of Now: DateTimeOffset * ResponseChannel<Result<EcosystemHealthcheckResult, (* errorMessage *) string>>
| UpdatePermanentFailures of LifeCycleName: string * UpdatePermanentFailuresScope * Set<UpdatePermanentFailuresFilter> * UpdatePermanentFailuresOperation * ResponseChannel<Result<LastUpdatePermanentFailuresResult, PermanentFailuresUpdateError>>
| ProcessStalledSideEffectsBatch of LifeCycleName: string * BatchSize: uint16 * ResponseChannel<Result<Option<string>, StalledSideEffectsError>>
with interface Request

type ReportMetricsError =
| InvalidLifeCycle of LifeCycleName: string

type GetSideEffectMetricsResult = {
    OldestAgeOfNonFailed: TimeSpan
    FailureWarningCount:  int
    FailureErrorCount:    int
    QueueLength:          int
}

type GetTimerMetricsResult = {
    OldestAge:    TimeSpan
    ExpiredCount: int
}

type MetricsReporter =
| ReportSideEffectMetrics of LifeCycleName: string * ResponseChannel<Result<unit, ReportMetricsError>>
| ReportTimerMetrics      of LifeCycleName: string * ResponseChannel<Result<unit, ReportMetricsError>>
| ReportCustomMetrics     of LifeCycleName: string * SubjectIdStr: string * Map<string, float> * ResponseChannel<Result<unit, ReportMetricsError>>
with interface Request
