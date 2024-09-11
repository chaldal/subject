[<AutoOpen>]
module LibLifeCycle.LifeCycles.Meta.Types

// A loosely-typed "Meta" LifeCycle, that helps with management operations on existing lifecycles

open System
open LibLifeCycleTypes


type IndexRebuildError =
| InvalidLifeCycle of LifeCycleName: string
| InvalidIndex of IndexKey

type IndexRebuildType =
| All
| Selected of NonemptySet<IndexKey>

[<RequireQualifiedAccess>]
type RebuildIndicesBatchResult =
| CompletedBatchNoMoreBatchesPending
| CompletedBatch of LastRebuiltKey: string
| SubjectUpdatedConcurrentlyTryAgain

type StalledSideEffectsError =
    InvalidLifeCycle of LifeCycleName: string

// anonymous types would do just fine but unfortunately it breaks Orleans codegen in LibLifeCycleHostBuild. Can we fix it somehow?
type StalledSideEffectsLastDetected = { On: DateTimeOffset; IdStr: string }
type StalledSideEffectsLastChecked = { On: DateTimeOffset; Error: Option<StalledSideEffectsError> }
type StalledSideEffects = { LastDetected: Option<StalledSideEffectsLastDetected>; LastChecked: Option<StalledSideEffectsLastChecked> }

[<RequireQualifiedAccess>]
type TimersSubsRebuildError =
| InvalidLifeCycle of LifeCycleName: string

[<RequireQualifiedAccess>]
type RebuildTimersSubsBatchResult =
| CompletedBatchNoMoreBatchesPending
| CompletedBatch of LastRebuiltKey: string

[<RequireQualifiedAccess>]
type ReEncodeSubjectsError =
| InvalidLifeCycle of LifeCycleName: string

[<RequireQualifiedAccess>]
type ReEncodeSubjectsBatchResult =
| CompletedBatchNoMoreBatchesPending
| CompletedBatch of LastReEncodedId: string
| SubjectUpdatedConcurrentlyTryAgain

[<RequireQualifiedAccess>]
type ReEncodeSubjectsHistoryBatchResult =
| CompletedBatchNoMoreBatchesPending
| CompletedBatch of LastReEncodedId: string * LastReEncodedVersion: uint64

[<RequireQualifiedAccess>]
type ClearExpiredSubjectsHistoryError =
| InvalidLifeCycle of LifeCycleName: string

type ClearExpiredSubjectsHistoryLastChecked = { On: DateTimeOffset; Error: Option<ClearExpiredSubjectsHistoryError> }
type ClearExpiredSubjectsHistory = {
    LastDetectedOn: Option<DateTimeOffset>
    LastChecked: Option<ClearExpiredSubjectsHistoryLastChecked>
}

type MetaId = MetaId of LifeCycleName: string
with
    interface SubjectId with
        member this.IdString =
            let (MetaId name) = this in name

type IndexRebuildOperation = {
    RebuildType:          IndexRebuildType
    StartedOn:            DateTimeOffset
    LastUpdatedOn:        DateTimeOffset
    LastSubjectIdRebuilt: Option<string>
    BatchSize:            uint16
}

type CompletedIndexRebuildData = {
    FinalBatchSize: uint16
    CompletedOn: DateTimeOffset
}

type LastIndexRebuildOperation = {
    RebuildType: IndexRebuildType
    StartedOn:   DateTimeOffset
    Result:      Result<CompletedIndexRebuildData, IndexRebuildError>
}

type TimersSubsRebuildOperation = {
    RepairGrainIdHash:    bool
    StartedOn:            DateTimeOffset
    LastUpdatedOn:        DateTimeOffset
    LastSubjectIdRebuilt: Option<string>
    LastError:            Option<TimersSubsRebuildError>
    BatchSize:            uint16 // how many subjects to load from repo at a time
    Parallelism:          uint16 // how many affected grains to activate at a time
}

type ReEncodeSubjectsOperation = {
    StartedOn:              DateTimeOffset
    LastUpdatedOn:          DateTimeOffset
    LastSubjectIdReEncoded: Option<string>
    LastError:              Option<ReEncodeSubjectsError>
    BatchSize:              uint16
}

type ReEncodeSubjectsHistoryOperation = {
    StartedOn:                     DateTimeOffset
    LastUpdatedOn:                 DateTimeOffset
    LastSubjectIdVersionReEncoded: Option<string * uint64>
    LastError:                     Option<ReEncodeSubjectsError>
    BatchSize:                     uint16
}

[<RequireQualifiedAccess>]
type UpdatePermanentFailuresScope =
| SeqNum       of SubjectIdStr: string * SeqNum: uint64
| Subject      of SubjectIdStr: string
| Single       of SubjectIdStr: string * SideEffectId: Guid
| NextSeqBatch of BatchSize: uint8  // Why not just "All" ? if too many failed subjects then Retry All can lead to disaster.
                                    // Could be a batched job similar to index rebuild, but it goes against idea of manual failure resolution.

[<RequireQualifiedAccess>]
type UpdatePermanentFailuresFilter =
| OnlyWarnings
| ReasonContains of string

[<RequireQualifiedAccess>]
type UpdatePermanentFailuresOperation =
| Ack    of UntilFromNow: TimeSpan
| Retry
| Delete

type LastUpdatePermanentFailuresResult = {
    LastSubjectIdStr: string
    LastSideEffectId: Guid
    SideEffectSeqNum: uint64
    LastOperation:    UpdatePermanentFailuresOperation
}

type Meta = {
    Id:                          MetaId
    CreatedOn:                   DateTimeOffset
    IndexRebuildOp:              Option<IndexRebuildOperation>
    LastIndexRebuildOp:          Option<LastIndexRebuildOperation>
    TimersSubsRebuildOp:         Option<TimersSubsRebuildOperation>
    ReEncodeSubjectsOp:          Option<ReEncodeSubjectsOperation>
    ReEncodeSubjectsHistoryOp:   Option<ReEncodeSubjectsHistoryOperation>
    ClearExpiredSubjectsHistory: ClearExpiredSubjectsHistory
    StalledSideEffects:          StalledSideEffects
    LastMetricsReportOn:         Option<DateTimeOffset>
    LastUpdatePermanentFailures: Option<DateTimeOffset * LastUpdatePermanentFailuresResult>
} with
    interface Subject<MetaId> with
        member this.SubjectId = this.Id
        member this.SubjectCreatedOn = this.CreatedOn

type MetaConstructor =
| New of MetaId
with
    interface Constructor

type MetaAction =
| StartIndexRebuild                   of IndexRebuildType * SkipToIdInclusive: Option<string> * BatchSize: Option<uint16>
| RunNextIndexRebuildBatch
| ForceStopIndexRebuild
| StartTimersSubsRebuild              of RepairGrainIdHash: bool * SkipToIdInclusive: Option<string> * BatchSize: Option<uint16> * Parallelism: Option<uint16>
| RunNextTimersSubsRebuildBatch
| ForceStopTimersSubsRebuild
| StartReEncodeSubjects               of SkipToIdInclusive: Option<string> * BatchSize: Option<uint16>
| RunNextReEncodeSubjectsBatch
| ForceStopReEncodeSubjects
| StartReEncodeSubjectsHistory        of SkipToIdVersionInclusive: Option<string * uint64> * BatchSize: Option<uint16>
| RunNextReEncodeSubjectsHistoryBatch
| ForceStopReEncodeSubjectsHistory
| StalledSideEffectsCheck
| RunClearExpiredSubjectsHistoryBatch
| ReportMetrics
| UpdatePermanentFailure              of UpdatePermanentFailuresScope * Set<UpdatePermanentFailuresFilter> * UpdatePermanentFailuresOperation
with
    interface LifeAction

type MetaLifeEvent = private NoLifeEvent of unit
    with interface LifeEvent

type MetaOpError =
| IndexRebuildAlreadyInProgress
| TimersSubsRebuildAlreadyInProgress
| ReEncodeAlreadyInProgress
| InvalidLifeCycle of LifeCycleName: string
| NoPermanentFailuresFound
with interface OpError

type MetaNumericIndex = NoNumericIndex<MetaOpError>

[<RequireQualifiedAccess>]
type MetaStringIndex =
| RebuildingTimersAndSubs of LastRebuiltSubjectId: string // this index used by AllStalledTimers view
| RebuildingIndices of LastRebuiltSubjectId: string
| ReEncodingSubjects of LastReEncodedSubjectId: string
| ReEncodingSubjectsHistory of LastReEncodedSubjectId: string * LastReEncodedVersion: uint64
with
    interface SubjectStringIndex<MetaOpError> with
        member this.Primitive =
            match this with
            | RebuildingTimersAndSubs subjectId
            | RebuildingIndices subjectId
            | ReEncodingSubjects subjectId ->
                IndexedString subjectId
            | ReEncodingSubjectsHistory (subjectId, version) ->
                IndexedString $"%s{subjectId}/%d{version}"

type MetaSearchIndex = NoSearchIndex
type MetaGeographyIndex = NoGeographyIndex

type MetaIndex () = inherit SubjectIndex<MetaIndex, MetaNumericIndex, MetaStringIndex, MetaSearchIndex, MetaGeographyIndex, MetaOpError>()

// CODECS

#if !FABLE_COMPILER
open CodecLib

type IndexRebuildError with
    static member get_Codec () =
        function
        | InvalidLifeCycle _ ->
            codec {
                let! payload = reqWith Codecs.string "InvalidLifeCycle" (function InvalidLifeCycle x -> Some x | _ -> None)
                return InvalidLifeCycle payload
            }
        | InvalidIndex _ ->
            codec {
                let! payload = reqWith (IndexKey.get_Codec ()) "InvalidIndex" (function InvalidIndex x     -> Some x | _ -> None)
                return InvalidIndex payload
            }
        |> mergeUnionCases
        |> ofObjCodec

type IndexRebuildType with
    static member get_Codec () =
        function
        | All ->
            codec {
                let! _ = reqWith Codecs.unit "All" (function All -> Some () | _ -> None)
                return All
            }
        | Selected _ ->
            codec {
                let! payload = reqWith (NonemptySet.codec codecFor<_, IndexKey>) "Selected" (function Selected x -> Some x  | _ -> None)
                return Selected payload
            }
        |> mergeUnionCases
        |> ofObjCodec

type StalledSideEffectsError with
    static member get_Codec () =
        function
        | InvalidLifeCycle _ ->
            codec {
                let! payload = reqWith Codecs.string "InvalidLifeCycle" (function InvalidLifeCycle x -> Some x)
                return InvalidLifeCycle payload
            }
        |> mergeUnionCases
        |> ofObjCodec

type StalledSideEffectsLastDetected
with
    static member get_Codec () = ofObjCodec <| codec {
        let! on = reqWith Codecs.dateTimeOffset "On" (fun x -> Some x.On)
        and! idStr = reqWith Codecs.string "IdStr" (fun x -> Some x.IdStr)
        return { On = on; IdStr = idStr } }

type StalledSideEffectsLastChecked
with
    static member get_Codec () = ofObjCodec <| codec {
        let! on = reqWith Codecs.dateTimeOffset "On" (fun x -> Some x.On)
        and! error = optWith (StalledSideEffectsError.get_Codec ()) "Error" (fun x -> x.Error)
        return { On = on; Error = error } }

type StalledSideEffects
with
    static member get_Codec () = ofObjCodec <| codec {
        let! lastDetected = optWith (StalledSideEffectsLastDetected.get_Codec ()) "LastDetected" (fun x -> x.LastDetected)
        and! lastChecked = optWith (StalledSideEffectsLastChecked.get_Codec ()) "LastChecked" (fun x -> x.LastChecked)
        return { LastDetected = lastDetected; LastChecked = lastChecked } }

type TimersSubsRebuildError with
    static member get_Codec () =
        function
        | InvalidLifeCycle _ ->
            codec {
                let! payload = reqWith Codecs.string "InvalidLifeCycle" (function InvalidLifeCycle x -> Some x)
                return InvalidLifeCycle payload
            }
        |> mergeUnionCases
        |> ofObjCodec

type ReEncodeSubjectsError with
    static member get_Codec () =
        function
        | InvalidLifeCycle _ ->
            codec {
                let! payload = reqWith Codecs.string "InvalidLifeCycle" (function InvalidLifeCycle x -> Some x)
                return InvalidLifeCycle payload
            }
        |> mergeUnionCases
        |> ofObjCodec

type ClearExpiredSubjectsHistoryError with
    static member get_Codec () =
        function
        | InvalidLifeCycle _ ->
            codec {
                let! payload = reqWith Codecs.string "InvalidLifeCycle" (function InvalidLifeCycle x -> Some x)
                return InvalidLifeCycle payload
            }
        |> mergeUnionCases
        |> ofObjCodec

type ClearExpiredSubjectsHistoryLastChecked
with
    static member get_Codec () = ofObjCodec <| codec {
        let! on = reqWith Codecs.dateTimeOffset "On" (fun x -> Some x.On)
        and! error = optWith (ClearExpiredSubjectsHistoryError.get_Codec ()) "Error" (fun x -> x.Error)
        return { On = on; Error = error } }

type ClearExpiredSubjectsHistory
with
    static member get_Codec () = ofObjCodec <| codec {
        let! lastDetectedOn = optWith Codecs.dateTimeOffset "LastDetectedOn" (fun x -> x.LastDetectedOn)
        and! lastChecked = optWith (ClearExpiredSubjectsHistoryLastChecked.get_Codec ()) "LastChecked" (fun x -> x.LastChecked)
        return { LastDetectedOn = lastDetectedOn; LastChecked = lastChecked } }

type MetaId
with
    static member TypeLabel () = "MetaId"

    static member get_ObjCodec () =
        function
        | MetaId _ ->
            codec {
                let! payload = reqWith Codecs.string "MetaId" (fun (MetaId x) -> Some x)
                return MetaId payload
            }
        |> mergeUnionCases

    static member get_Codec () = ofObjCodec (MetaId.get_ObjCodec ())

    static member Init (typeLabel: string , _typeParams: _) = initializeInterfaceImplementation<SubjectId, MetaId> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| MetaId.get_ObjCodec ())


type IndexRebuildOperation with
    static member get_Codec () = ofObjCodec <| codec {
        let! rebuildType          = reqWith (IndexRebuildType.get_Codec ()) "RebuildType" (fun x -> Some x.RebuildType)
        and! startedOn            = reqWith Codecs.dateTimeOffset "StartedOn" (fun x -> Some x.StartedOn)
        and! lastUpdatedOn        = reqWith Codecs.dateTimeOffset "LastUpdatedOn" (fun x -> Some x.LastUpdatedOn)
        and! lastSubjectIdRebuilt = optWith Codecs.string "LastSubjectIdRebuilt" (fun x -> x.LastSubjectIdRebuilt)
        and! batchSize            = reqWith Codecs.uint16 "BatchSize" (fun x -> Some x.BatchSize)
        return { RebuildType = rebuildType; StartedOn = startedOn; LastUpdatedOn = lastUpdatedOn; LastSubjectIdRebuilt = lastSubjectIdRebuilt; BatchSize = batchSize }}

type CompletedIndexRebuildData with
    static member get_Codec () = ofObjCodec <| codec {
        let! finalBatchSize = reqWith Codecs.uint16 "FinalBatchSize" (fun x -> Some x.FinalBatchSize)
        and! completedOn = reqWith Codecs.dateTimeOffset "CompletedOn" (fun x -> Some x.CompletedOn)
        return { FinalBatchSize = finalBatchSize; CompletedOn = completedOn }}

type LastIndexRebuildOperation with
    static member get_Codec () = ofObjCodec <| codec {
        let! rebuildType = reqWith (IndexRebuildType.get_Codec ()) "RebuildType" (fun x -> Some x.RebuildType)
        and! startedOn = reqWith Codecs.dateTimeOffset  "StartedOn" (fun x -> Some x.StartedOn)
        and! result = reqWith (Codecs.result (CompletedIndexRebuildData.get_Codec ()) (IndexRebuildError.get_Codec ())) "Result" (fun x -> Some x.Result)
        return { RebuildType = rebuildType; StartedOn = startedOn; Result = result }}

type TimersSubsRebuildOperation with
    static member get_Codec () = ofObjCodec <| codec {
        let! startedOn              = reqWith Codecs.dateTimeOffset "StartedOn" (fun x -> Some x.StartedOn)
        and! lastUpdatedOn          = reqWith Codecs.dateTimeOffset "LastUpdatedOn" (fun x -> Some x.LastUpdatedOn)
        and! lastSubjectIdRebuilt   = optWith Codecs.string "LastSubjectIdRebuilt" (fun x -> x.LastSubjectIdRebuilt)
        and! lastError              = optWith (TimersSubsRebuildError.get_Codec ()) "LastError" (fun x -> x.LastError)
        and! batchSize              = reqWith Codecs.uint16 "BatchSize" (fun x -> Some x.BatchSize)
        and! maybeParallelism       = optWith Codecs.uint16 "Parallelism" (fun x -> Some x.Parallelism)
        and! maybeRepairGrainIdHash = optWith Codecs.boolean "RepairGrainIdHash" (fun x -> Some x.RepairGrainIdHash)
        return
            { RepairGrainIdHash = maybeRepairGrainIdHash |> Option.defaultValue false
              StartedOn = startedOn
              LastUpdatedOn = lastUpdatedOn
              LastSubjectIdRebuilt = lastSubjectIdRebuilt
              LastError = lastError
              BatchSize = batchSize
              Parallelism = maybeParallelism |> Option.defaultValue 1us }}

type ReEncodeSubjectsOperation with
    static member get_Codec () = ofObjCodec <| codec {
        let! startedOn              = reqWith Codecs.dateTimeOffset "StartedOn" (fun x -> Some x.StartedOn)
        and! lastUpdatedOn          = reqWith Codecs.dateTimeOffset "LastUpdatedOn" (fun x -> Some x.LastUpdatedOn)
        and! lastSubjectIdReEncoded = optWith Codecs.string "LastSubjectIdReEncoded" (fun x -> x.LastSubjectIdReEncoded)
        and! lastError              = optWith (ReEncodeSubjectsError.get_Codec ()) "LastError" (fun x -> x.LastError)
        and! batchSize              = reqWith Codecs.uint16 "BatchSize" (fun x -> Some x.BatchSize)
        return
            { StartedOn = startedOn
              LastUpdatedOn = lastUpdatedOn
              LastSubjectIdReEncoded = lastSubjectIdReEncoded
              LastError = lastError
              BatchSize = batchSize }}

type ReEncodeSubjectsHistoryOperation with
    static member get_Codec () = ofObjCodec <| codec {
        let! startedOn                     = reqWith Codecs.dateTimeOffset "StartedOn" (fun x -> Some x.StartedOn)
        and! lastUpdatedOn                 = reqWith Codecs.dateTimeOffset "LastUpdatedOn" (fun x -> Some x.LastUpdatedOn)
        and! lastSubjectIdVersionReEncoded = optWith (Codecs.tuple2 Codecs.string Codecs.uint64) "LastSubjectIdVersionReEncoded" (fun x -> x.LastSubjectIdVersionReEncoded)
        and! lastError                     = optWith (ReEncodeSubjectsError.get_Codec ()) "LastError" (fun x -> x.LastError)
        and! batchSize                     = reqWith Codecs.uint16 "BatchSize" (fun x -> Some x.BatchSize)
        return
            { StartedOn = startedOn
              LastUpdatedOn = lastUpdatedOn
              LastSubjectIdVersionReEncoded = lastSubjectIdVersionReEncoded
              LastError = lastError
              BatchSize = batchSize }}

type UpdatePermanentFailuresScope with
    static member get_Codec () =
        function
        | SeqNum _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 Codecs.string Codecs.uint64) "SeqNum" (function | SeqNum (x1, x2) -> Some (x1, x2) | _ -> None)
                return SeqNum payload
            }
        | Subject _ ->
            codec {
                let! payload = reqWith Codecs.string "Subject" (function | Subject x -> Some x | _ -> None)
                return Subject payload
            }
        | NextSeqBatch _ ->
            codec {
                let! payload = reqWith Codecs.byte "NextSeqBatch" (function | NextSeqBatch x -> Some x | _ -> None)
                return NextSeqBatch payload
            }
        | Single _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 Codecs.string Codecs.guid) "Single" (function | Single (x1, x2) -> Some (x1, x2) | _ -> None)
                return Single payload
            }
        |> mergeUnionCases
        |> ofObjCodec

type UpdatePermanentFailuresFilter with
    static member get_Codec () =
        function
        | OnlyWarnings ->
            codec {
                let! _ = reqWith Codecs.unit "OnlyWarnings" (function | OnlyWarnings -> Some () | _ -> None)
                return OnlyWarnings
            }
        | ReasonContains _ ->
            codec {
                let! payload = reqWith Codecs.string "ReasonContains" (function | ReasonContains x -> Some x | _ -> None)
                return ReasonContains payload
            }
        |> mergeUnionCases
        |> ofObjCodec

type UpdatePermanentFailuresOperation with
    static member get_Codec () =
        function
        | Ack _ ->
            codec {
                let! payload = reqWith Codecs.timeSpan "Ack" (function | Ack x -> Some x | _ -> None)
                return Ack payload
            }
        | Delete ->
            codec {
                let! _ = reqWith Codecs.unit "Delete" (function Delete -> Some () | _ -> None)
                return Delete
            }
        | Retry ->
            codec {
                let! _ = reqWith Codecs.unit "Retry" (function Retry -> Some () | _ -> None)
                return Retry
            }
        |> mergeUnionCases
        |> ofObjCodec

type LastUpdatePermanentFailuresResult with
    static member get_Codec () = ofObjCodec <| codec {
        let! lastSubjectIdStr = reqWith Codecs.string "IdStr" (fun x -> Some x.LastSubjectIdStr)
        and! lastSideEffectId = reqWith Codecs.guid "SideEffectId" (fun x -> Some x.LastSideEffectId)
        and! sideEffectSeqNum = reqWith Codecs.uint64 "SeqNum" (fun x -> Some x.SideEffectSeqNum)
        and! lastOperation = reqWith (UpdatePermanentFailuresOperation.get_Codec ()) "Op" (fun x -> Some x.LastOperation)
        return {
            LastSubjectIdStr = lastSubjectIdStr
            LastSideEffectId = lastSideEffectId
            SideEffectSeqNum = sideEffectSeqNum
            LastOperation    = lastOperation
        }}

type Meta with

    static member TypeLabel () = "Meta"

    static member get_ObjCodec () = codec {
        let! id = reqWith (MetaId.get_Codec ()) "Id" (fun x -> Some x.Id)
        and! createdOn = reqWith Codecs.dateTimeOffset "CreatedOn" (fun x -> Some x.CreatedOn)
        and! indexRebuildOp = optWith (IndexRebuildOperation.get_Codec ()) "IndexRebuildOp" (fun x -> x.IndexRebuildOp)
        and! lastIndexRebuildOp = optWith (LastIndexRebuildOperation.get_Codec ()) "LastIndexRebuildOp" (fun x -> x.LastIndexRebuildOp)
        and! timersSubsRebuildOp = optWith (TimersSubsRebuildOperation.get_Codec ()) "TimersSubsRebuildOp" (fun x -> x.TimersSubsRebuildOp)
        and! reEncodeSubjectsOp = optWith (ReEncodeSubjectsOperation.get_Codec ()) "ReEncodeSubjectsOp" (fun x -> x.ReEncodeSubjectsOp)
        and! reEncodeSubjectsHistoryOp = optWith (ReEncodeSubjectsHistoryOperation.get_Codec ()) "ReEncodeSubjectsHistoryOp" (fun x -> x.ReEncodeSubjectsHistoryOp)
        and! maybeClearExpiredSubjectsHistory = optWith (ClearExpiredSubjectsHistory.get_Codec ()) "ClearExpiredSubjectsHistory" (fun x -> Some x.ClearExpiredSubjectsHistory)
        and! maybeStalledSideEffects = optWith (StalledSideEffects.get_Codec ()) "StalledSideEffects" (fun x -> Some x.StalledSideEffects)
        and! lastMetricsReportOn = optWith Codecs.dateTimeOffset "LastMetricsReportOn" (fun x -> x.LastMetricsReportOn)
        and! lastUpdatePermanentFailures = optWith (Codecs.tuple2 Codecs.dateTimeOffset (LastUpdatePermanentFailuresResult.get_Codec ())) "LastUpdatePermanentFailures" (fun x -> x.LastUpdatePermanentFailures)
        let stalledSideEffects = maybeStalledSideEffects |> Option.defaultValue { LastDetected = None; LastChecked = None }
        let clearExpiredSubjectsHistory = maybeClearExpiredSubjectsHistory |> Option.defaultValue { LastDetectedOn = None; LastChecked = None }
        return {
            Id                          = id
            CreatedOn                   = createdOn
            IndexRebuildOp              = indexRebuildOp
            LastIndexRebuildOp          = lastIndexRebuildOp
            TimersSubsRebuildOp         = timersSubsRebuildOp
            ReEncodeSubjectsOp          = reEncodeSubjectsOp
            ReEncodeSubjectsHistoryOp   = reEncodeSubjectsHistoryOp
            ClearExpiredSubjectsHistory = clearExpiredSubjectsHistory
            StalledSideEffects          = stalledSideEffects
            LastMetricsReportOn         = lastMetricsReportOn
            LastUpdatePermanentFailures = lastUpdatePermanentFailures
        }}
    static member get_Codec () = ofObjCodec (Meta.get_ObjCodec ())

    static member Init (typeLabel: string , _typeParams: _) = initializeInterfaceImplementation<Subject<MetaId>, Meta> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| Meta.get_ObjCodec ())

type MetaConstructor with
    static member TypeLabel () = "MetaConstructor"

    static member get_ObjCodec () =
        function
        | New _ ->
            codec {
                let! payload = reqWith (MetaId.get_Codec ()) "New" (fun (New x) -> Some x)
                return New payload
            }
        |> mergeUnionCases

    static member get_Codec () = ofObjCodec (MetaConstructor.get_ObjCodec ())
    static member Init (typeLabel: string , _typeParams: _) = initializeInterfaceImplementation<Constructor, MetaConstructor> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| MetaConstructor.get_ObjCodec ())

type MetaAction with
    static member TypeLabel () = "MetaAction"
    static member get_ObjCodec_V1 () =
        function
        | StartIndexRebuild _ ->
            codec {
                let! payload = reqWith (Codecs.tuple3 (IndexRebuildType.get_Codec ()) (Codecs.option Codecs.string) (Codecs.option Codecs.uint16)) "StartIndexRebuild" (function StartIndexRebuild (x1, x2, x3) -> Some (x1, x2, x3) | _ -> None)
                return StartIndexRebuild payload
            }
        | RunNextIndexRebuildBatch ->
            codec {
                let! _ = reqWith Codecs.unit "RunNextIndexRebuildBatch" (function RunNextIndexRebuildBatch -> Some () | _ -> None)
                return RunNextIndexRebuildBatch
            }
        | ForceStopIndexRebuild ->
            codec {
                let! _ = reqWith Codecs.unit "ForceStopIndexRebuild" (function ForceStopIndexRebuild -> Some () | _ -> None)
                return ForceStopIndexRebuild
            }
        | StartTimersSubsRebuild _ ->
            codec {
                let! payload = reqWith (Codecs.tuple4 Codecs.boolean (Codecs.option Codecs.string) (Codecs.option Codecs.uint16) (Codecs.option Codecs.uint16)) "StartTimersSubsRebuild" (function StartTimersSubsRebuild (x1, x2, x3, x4) -> Some (x1, x2, x3, x4) | _ -> None)
                return StartTimersSubsRebuild payload
            }
        | RunNextTimersSubsRebuildBatch ->
            codec {
                let! _ = reqWith Codecs.unit "RunNextTimersSubsRebuildBatch" (function RunNextTimersSubsRebuildBatch -> Some () | _ -> None)
                return RunNextTimersSubsRebuildBatch
            }
        | ForceStopTimersSubsRebuild ->
            codec {
                let! _ = reqWith Codecs.unit "ForceStopTimersSubsRebuild" (function ForceStopTimersSubsRebuild -> Some () | _ -> None)
                return ForceStopTimersSubsRebuild
            }
        | StartReEncodeSubjects _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 (Codecs.option Codecs.string) (Codecs.option Codecs.uint16)) "StartReEncodeSubjects" (function StartReEncodeSubjects (x1, x2) -> Some (x1, x2) | _ -> None)
                return StartReEncodeSubjects payload
            }
        | RunNextReEncodeSubjectsBatch ->
            codec {
                let! _ = reqWith Codecs.unit "RunNextReEncodeSubjectsBatch" (function RunNextReEncodeSubjectsBatch -> Some () | _ -> None)
                return RunNextReEncodeSubjectsBatch
            }
        | ForceStopReEncodeSubjects ->
            codec {
                let! _ = reqWith Codecs.unit "ForceStopReEncodeSubjects" (function ForceStopReEncodeSubjects -> Some () | _ -> None)
                return ForceStopReEncodeSubjects
            }
        | StartReEncodeSubjectsHistory _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 (Codecs.option (Codecs.tuple2 Codecs.string Codecs.uint64)) (Codecs.option Codecs.uint16)) "StartReEncodeSubjectsHistory" (function StartReEncodeSubjectsHistory (x1, x2) -> Some (x1, x2) | _ -> None)
                return StartReEncodeSubjectsHistory payload
            }
        | RunNextReEncodeSubjectsHistoryBatch ->
            codec {
                let! _ = reqWith Codecs.unit "RunNextReEncodeSubjectsHistoryBatch" (function RunNextReEncodeSubjectsHistoryBatch -> Some () | _ -> None)
                return RunNextReEncodeSubjectsHistoryBatch
            }
        | ForceStopReEncodeSubjectsHistory ->
            codec {
                let! _ = reqWith Codecs.unit "ForceStopReEncodeSubjectsHistory" (function ForceStopReEncodeSubjectsHistory -> Some () | _ -> None)
                return ForceStopReEncodeSubjectsHistory
            }
        | StalledSideEffectsCheck ->
            codec {
                let! _ = reqWith Codecs.unit "StalledSideEffectsCheck" (function StalledSideEffectsCheck -> Some () | _ -> None)
                return StalledSideEffectsCheck
            }
        | RunClearExpiredSubjectsHistoryBatch ->
            codec {
                let! _ = reqWith Codecs.unit "RunClearExpiredSubjectsHistoryBatch" (function RunClearExpiredSubjectsHistoryBatch -> Some () | _ -> None)
                return RunClearExpiredSubjectsHistoryBatch
            }
        | ReportMetrics ->
            codec {
                let! _ = reqWith Codecs.unit "ReportMetrics" (function ReportMetrics -> Some () | _ -> None)
                return ReportMetrics
            }
        | UpdatePermanentFailure _ ->
            codec {
                let! payload = reqWith (Codecs.tuple3 (UpdatePermanentFailuresScope.get_Codec ()) (Codecs.set (UpdatePermanentFailuresFilter.get_Codec ())) (UpdatePermanentFailuresOperation.get_Codec ())) "UpdatePermanentFailure" (function UpdatePermanentFailure (x1, x2, x3) -> Some (x1, x2, x3) | _ -> None)
                return UpdatePermanentFailure payload
            }
        |> mergeUnionCases

    static member get_ObjCodec () = MetaAction.get_ObjCodec_V1 ()

    static member get_Codec () = ofObjCodec <| MetaAction.get_ObjCodec ()
    static member Init (typeLabel: string , _typeParams: _) = initializeInterfaceImplementation<LifeAction, MetaAction> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| MetaAction.get_ObjCodec ())

type MetaLifeEvent
with
    static member get_ObjCodec () : Codec<MultiObj<'RawEncoding>, MetaLifeEvent> =
        function
        | NoLifeEvent _ ->
            codec {
                let! payload = reqWith Codecs.unit "NoLifeEvent" (function NoLifeEvent _ -> Some ())
                return NoLifeEvent payload
            }
        |> mergeUnionCases

    static member get_Codec () = ofObjCodec (MetaLifeEvent.get_ObjCodec ())
    static member Init (typeLabel: string, _typeParams: _) =
        initializeInterfaceImplementation<LifeEvent, MetaLifeEvent> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| MetaLifeEvent.get_ObjCodec ())
    static member TypeLabel () = "MetaLifeEvent" // it won't be ever encoded via interface but need to be configured anyway

type MetaOpError with
    static member TypeLabel () = "MetaOpError"

    static member get_ObjCodec () =
        function
        | IndexRebuildAlreadyInProgress ->
            codec {
                let! _ = reqWith Codecs.unit "IndexRebuildAlreadyInProgress" (function IndexRebuildAlreadyInProgress -> Some () | _ -> None)
                return IndexRebuildAlreadyInProgress
            }
        | TimersSubsRebuildAlreadyInProgress ->
            codec {
                let! _ = reqWith Codecs.unit "TimersSubsRebuildAlreadyInProgress" (function TimersSubsRebuildAlreadyInProgress -> Some () | _ -> None)
                return TimersSubsRebuildAlreadyInProgress
            }
        | ReEncodeAlreadyInProgress ->
            codec {
                let! _ = reqWith Codecs.unit "ReEncodeAlreadyInProgress" (function ReEncodeAlreadyInProgress -> Some () | _ -> None)
                return ReEncodeAlreadyInProgress
            }
        | InvalidLifeCycle _ ->
            codec {
                let! payload = reqWith Codecs.string "InvalidLifeCycle" (function InvalidLifeCycle x -> Some x | _ -> None)
                return InvalidLifeCycle payload
            }
        | NoPermanentFailuresFound ->
            codec {
                let! _ = reqWith Codecs.unit "NoPermanentFailuresFound" (function NoPermanentFailuresFound -> Some () | _ -> None)
                return NoPermanentFailuresFound
            }
        |> mergeUnionCases

    static member get_Codec () = ofObjCodec <| MetaOpError.get_ObjCodec ()
    static member Init (typeLabel: string , _typeParams: _) = initializeInterfaceImplementation<OpError, MetaOpError> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| MetaOpError.get_ObjCodec ())

#endif
