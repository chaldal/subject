[<AutoOpen>]
module LibLifeCycleHost.SubjectGrainModel

open System

type TraceContext = {
    // as name suggests, user and session are for telemetry/audit only and should NOT be used for authorization.
    TelemetryUserId: string
    TelemetrySessionId: Option<string>
    // parent Id of diagnostics activity for distributed tracing
    ParentId: string
}

let emptyTraceContext : TraceContext = { TelemetryUserId = ""; TelemetrySessionId = None; ParentId = "" }

[<RequireQualifiedAccess>]
type GrainSideEffect<'LifeAction, 'OpError when 'LifeAction :> LifeAction and 'OpError :> OpError> =
| Persisted of GrainPersistedSideEffect<'LifeAction, 'OpError>
| Transient of GrainTransientSideEffect<'LifeAction>

and GrainTransientSideEffect<'LifeAction when 'LifeAction :> LifeAction> =
| ConnectorRequest of TraceContext * ResponseType: Type * ConnectorName: string * BuildRequest: obj (* ResponseChannel<'Response> -> Request  *) * Action: (obj (* 'Request *) -> 'LifeAction)
| ConnectorRequestMultiResponse of TraceContext * ResponseType: Type * ConnectorName: string * BuildRequest: obj (* MultiResponseChannel<'Response> -> Request  *) * Action: (obj (* 'Request *) -> 'LifeAction)

and GrainPersistedSideEffect<'LifeAction, 'OpError when 'LifeAction :> LifeAction and 'OpError :> OpError> =
| RunActionOnSelf               of 'LifeAction * Option<TraceContext>
| Rpc                           of GrainRpc
| TriggerTimerActionOnSelf      of TentativeDueAction: 'LifeAction * Option<TraceContext>
| HandleSubscriptionResponseOnSelf of TriggerSubscriptionResponse<'LifeAction, 'OpError> * SubscriptionTriggerType * LifeEvent * TraceContext
| TryDeleteSelf                 of RequiredVersion: uint64 * RequiredNextSideEffectSequenceNumber: uint64 * RetryAttempt: byte

// TODO: try use responseHandler pattern instead, to distinguish permanent and transient txn errors (ConflictingPrepare), and retry latter until timed out
| RpcTransactionStep            of GrainRpcTransactionStep

| RpcTriggerSubscriptionOnGrain of GrainTriggerSubscriptionRpc
| IngestTimeSeries              of TimeSeriesKey * ``list<'TimeSeriesDataPoint>``: obj * TraceContext
| ObsoleteNoop                  of Why: string // obsolete side effects still need to noop-run to be cleared from storage

and GrainRpc = {
    SubjectReference: SubjectReference
    RpcOperation:     GrainRpcOperation
    TraceContext:     TraceContext
}

and [<RequireQualifiedAccess>] TriggerSubscriptionResponse<'LifeAction, 'OpError when 'OpError :> OpError and 'LifeAction :> LifeAction> =
| ActOk of 'LifeAction
| ActError of 'OpError * 'LifeAction
| ActNotAllowed of 'LifeAction
| Exn of ExceptionDetails: string * Option<'LifeAction>

and GrainRpcTransactionStep = {
    SubjectReference: SubjectReference
    TransactionId:    SubjectTransactionId
    BatchNo:          uint16
    OpNo:             uint16
    RpcOperation:     GrainRpcTransactionStepOperation
    TraceContext:     TraceContext
}

and GrainTriggerSubscriptionRpc = {
    SubjectPKeyReference: SubjectPKeyReference
    SubscriptionTriggerType: SubscriptionTriggerType
    LifeEvent: LifeEvent
    TraceContext: TraceContext
    Deduplicate: bool
}

and GrainRpcOperation =
    | RunActionOnGrain                           of LifeAction * Deduplicate: bool
    | RunActionOnGrainAndSubscribe               of LifeAction * Deduplicate: bool * Map<SubscriptionName, LifeEvent>
    | InitializeGrain                            of Constructor * OkIfAlreadyInitialized: bool
    | InitializeGrainAndSubscribe                of Constructor * OkIfAlreadyInitialized: bool * Map<SubscriptionName, LifeEvent>
    | SubscribeToGrain                           of Map<SubscriptionName, LifeEvent>
    | UnsubscribeFromGrain                       of Set<SubscriptionName>
    // TODO: consider removing ActMaybeConstruct throughout the stack - same can be achieved via MaybeConstruct and Act (although with two grain calls)
    | RunActionMaybeConstructOnGrain             of LifeAction * Constructor * Deduplicate: bool
    | RunActionMaybeConstructAndSubscribeOnGrain of LifeAction * Constructor * Deduplicate: bool * Map<SubscriptionName, LifeEvent>

and GrainRpcTransactionStepOperation =
    | PrepareActionOnGrain    of LifeAction
    | PrepareInitializeGrain  of Constructor
    | CommitPreparedOnGrain
    | RollbackPreparedOnGrain
    | CheckPhantomPreparedOnGrain

type WorkflowSideEffectProcessingResult<'LifeAction, 'LifeEvent, 'OpError
            when 'LifeAction :> LifeAction
            and 'LifeEvent :> LifeEvent
            and 'OpError :> OpError> = {
    GrainSideEffects: List<GrainSideEffect<'LifeAction, 'OpError>>
    RaisedLifeEvents: List<'LifeEvent>
}

[<RequireQualifiedAccess>]
type TickState =
    | NoTick
    | Scheduled of NextTickOn: DateTimeOffset * Option<TraceContext>
    | Fired     of OriginalNextTickOn: DateTimeOffset

type SubjectState<'Subject, 'SubjectId when 'Subject :> Subject<'SubjectId> and 'SubjectId :> SubjectId and 'SubjectId : comparison> = {
    Subject:           'Subject
    LastUpdatedOn:     DateTimeOffset
    TickState:         TickState
    // It's assumed that SubscriptionName unambiguously maps to a LifeEvent
    // Consider including LifeEvent explicitly (also will make unsubscription simpler)
    OurSubscriptions:  Map<SubscriptionName, SubjectReference>
}

type OthersSubscribing<'LifeEvent when 'LifeEvent :> LifeEvent and 'LifeEvent: comparison>
    = Map<'LifeEvent, Map<SubscriptionName, Set<SubjectPKeyReference>>>

type GrainSideEffectId = Guid

type [<RequireQualifiedAccess>] GrainSideEffectResult =
| Success
| PermanentFailure of string * SideEffectFailureSeverity

type RecursionLoopControlForTrackingUnboundedRecursions = {
    SourceStartingPoint: string
    Counter: byte
}

let newRecursionControl (format: Printf.StringFormat<'T,RecursionLoopControlForTrackingUnboundedRecursions>) =
    Printf.ksprintf (fun str -> { SourceStartingPoint = str; Counter = 1uy }) format

let doRecursionControl recControl counter =
    if recControl.Counter > counter then
        failwithf "Too many recursions when processing side-effects. Starting point: %s" recControl.SourceStartingPoint

[<RequireQualifiedAccess>]
type SideEffectTarget =
| TimeSeries of TimeSeriesKey
// TODO: migrate LifeCycle side effects to codecs too :  | LifeCycle of LifeCycleKey | Self

[<RequireQualifiedAccess>]
type TypedTimeSeriesSideEffect<'TimeSeriesDataPoint, 'TimeSeriesId, [<Measure>] 'UnitOfMeasure
    when 'TimeSeriesDataPoint :> TimeSeriesDataPoint<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure>
    and  'TimeSeriesId :> TimeSeriesId<'TimeSeriesId>> =
| Ingest of list<'TimeSeriesDataPoint> * TraceContext

type CodecFriendlyGrainTriggerSubscriptionRpc = {
    SubjectPKeyReference:    SubjectPKeyReference
    SubscriptionTriggerType: SubscriptionTriggerType
    // LifeEvent json serialized once and reused across subscribers to save some CPU as some event payloads are large and repeated serialization for many subscribers adds up
    LifeEventJson:           string
    TraceContext:            TraceContext
    Deduplicate:             bool
}

type GrainTriggerSubscriptionRpc with
    member this.AsCodecFriendlyData (cachedLifeEventJson: string) : CodecFriendlyGrainTriggerSubscriptionRpc =
        {
            SubjectPKeyReference = this.SubjectPKeyReference
            SubscriptionTriggerType = this.SubscriptionTriggerType
            LifeEventJson = cachedLifeEventJson
            TraceContext = this.TraceContext
            Deduplicate = this.Deduplicate
        }

// Reminder update is meant to be immediately applied to runtime with best-effort, unlike persisted TickState that is
// not always in sync with the runtime and is looked up by it only periodically (every few minutes).
// Reminder at runtime and persisted tick can be out of sync, either due to clever optimizations or best-effort reminder update failure.
type ReminderUpdate = {
    /// Best guess of what reminder is currently applied at runtime, as we can't look it up easily.
    AssumedCurrent: AssumedCurrentReminder
    On: Option<DateTimeOffset>
}
and [<RequireQualifiedAccess>] AssumedCurrentReminder =
| NotSet
| Set of DateTimeOffset
| Unknown

// this type is a temp measure to get rid of untyped/interface codecs that fail at runtime,
// so far it's done only for TimeSeries Ingest but will be retrofitted for other persisted side effects
[<RequireQualifiedAccess>]
type CodecFriendlyGrainPersistedSideEffect<'LifeAction, 'OpError when 'LifeAction :> LifeAction and 'OpError :> OpError> =
| RunActionOnSelf               of 'LifeAction * Option<TraceContext>
| Rpc                           of GrainRpc
| TriggerTimerActionOnSelf      of TentativeDueAction: 'LifeAction * Option<TraceContext>
| HandleSubscriptionResponseOnSelf of TriggerSubscriptionResponse<'LifeAction, 'OpError> * SubscriptionTriggerType * LifeEvent * TraceContext
| TryDeleteSelf                 of RequiredVersion: uint64 * RequiredNextSideEffectSequenceNumber: uint64 * RetryAttempt: byte
| RpcTransactionStep            of GrainRpcTransactionStep
| RpcTriggerSubscriptionOnGrain of CodecFriendlyGrainTriggerSubscriptionRpc
// TODO: remove these two when all persistted side effects flushed in all biosphere
| UpdateTimer                   of DateTimeOffset
| ClearTimer                    of NextTickOnToClear: DateTimeOffset
with
    member this.AsGrainPersistedSideEffect (maybeOverrideTraceContextParentId: Option<string>) =
        let maybeOverrideTraceContext (traceContext: TraceContext) =
            match maybeOverrideTraceContextParentId with
            | None -> traceContext
            | Some overrideTraceContextParentId -> { traceContext with ParentId = overrideTraceContextParentId }

        match this with
        | RunActionOnSelf (x1, x2) ->           GrainPersistedSideEffect.RunActionOnSelf(x1, x2 |> Option.map maybeOverrideTraceContext), None
        | Rpc x ->                              GrainPersistedSideEffect.Rpc { x with TraceContext = maybeOverrideTraceContext x.TraceContext }, None
        | TriggerTimerActionOnSelf (x1, x2) ->  GrainPersistedSideEffect.TriggerTimerActionOnSelf(x1, x2 |> Option.map maybeOverrideTraceContext), None
        | HandleSubscriptionResponseOnSelf (x1, x2, x3, x4) -> GrainPersistedSideEffect.HandleSubscriptionResponseOnSelf(x1, x2, x3, maybeOverrideTraceContext x4), None
        | TryDeleteSelf (x1, x2, x3) ->         GrainPersistedSideEffect.TryDeleteSelf (x1, x2, x3), None
        | RpcTransactionStep x ->               GrainPersistedSideEffect.RpcTransactionStep { x with TraceContext = maybeOverrideTraceContext x.TraceContext }, None
        | RpcTriggerSubscriptionOnGrain rpc ->
            {
                SubjectPKeyReference = rpc.SubjectPKeyReference
                SubscriptionTriggerType = rpc.SubscriptionTriggerType
                LifeEvent = CodecLib.StjCodecs.ofJsonText rpc.LifeEventJson |> function | Ok x -> x | Error e -> failwithf $"LifeEvent decode error: %A{e}"
                TraceContext = maybeOverrideTraceContext rpc.TraceContext
                Deduplicate = rpc.Deduplicate
            }
            |> GrainPersistedSideEffect.RpcTriggerSubscriptionOnGrain,
            None
        | UpdateTimer nextTickOn -> GrainPersistedSideEffect.ObsoleteNoop "UpdateTimer is not a side effect anymore", Some { AssumedCurrent = AssumedCurrentReminder.Unknown; On = Some nextTickOn }
        | ClearTimer _ -> GrainPersistedSideEffect.ObsoleteNoop "ClearTimer is not a side effect anymore", Some { AssumedCurrent = AssumedCurrentReminder.Unknown; On = None }


type GrainPersistedSideEffect<'LifeAction, 'OpError when 'LifeAction :> LifeAction and 'OpError :> OpError>
with
    member this.AsCodecFriendlyDataUnsafeForTriggerSubscriptionOnGrain : Choice<CodecFriendlyGrainPersistedSideEffect<'LifeAction, 'OpError>, TimeSeriesKey * (* ``list<'TimeSeriesDataPoint>``: *) obj * TraceContext> =
        match this with
        | RunActionOnSelf (x1, x2) ->           Choice1Of2 <| CodecFriendlyGrainPersistedSideEffect.RunActionOnSelf(x1, x2)
        | Rpc x ->                              Choice1Of2 <| CodecFriendlyGrainPersistedSideEffect.Rpc x
        | TriggerTimerActionOnSelf (x1, x2) ->  Choice1Of2 <| CodecFriendlyGrainPersistedSideEffect.TriggerTimerActionOnSelf(x1, x2)
        | HandleSubscriptionResponseOnSelf (x1, x2, x3, x4) -> Choice1Of2 <| CodecFriendlyGrainPersistedSideEffect.HandleSubscriptionResponseOnSelf (x1, x2, x3, x4)
        | TryDeleteSelf (x1, x2, x3) ->         Choice1Of2 <| CodecFriendlyGrainPersistedSideEffect.TryDeleteSelf (x1, x2, x3)
        | RpcTransactionStep x ->               Choice1Of2 <| CodecFriendlyGrainPersistedSideEffect.RpcTransactionStep x
        | RpcTriggerSubscriptionOnGrain _ ->    failwithf "Unexpected case RpcTriggerSubscriptionOnGrain from AsCodecFriendlyDataExceptTriggerSubscriptionOnGrain"
        | IngestTimeSeries (x1, x2, x3) ->      Choice2Of2 (x1, x2, x3)
        | ObsoleteNoop _ ->                     failwithf "Unexpected case ObsoleteNoop from AsCodecFriendlyDataExceptTriggerSubscriptionOnGrain"

    static member AsCodecFriendlyData (sideEffectIdAndPersistedSideEffects: List<GrainSideEffectId * GrainPersistedSideEffect<'LifeAction, 'OpError>>) : List<GrainSideEffectId * Choice<CodecFriendlyGrainPersistedSideEffect<'LifeAction, 'OpError>, TimeSeriesKey * (* ``list<'TimeSeriesDataPoint>``: *) obj * TraceContext>> =
        sideEffectIdAndPersistedSideEffects
        |> List.partition (fun (_sideEffectId, persistedSideEffect) ->
            match persistedSideEffect with
            | GrainPersistedSideEffect.RunActionOnSelf _
            | GrainPersistedSideEffect.Rpc _
            | GrainPersistedSideEffect.TriggerTimerActionOnSelf _
            | GrainPersistedSideEffect.HandleSubscriptionResponseOnSelf _
            | GrainPersistedSideEffect.TryDeleteSelf _
            | GrainPersistedSideEffect.RpcTransactionStep _
            | GrainPersistedSideEffect.IngestTimeSeries _
            | GrainPersistedSideEffect.ObsoleteNoop _ ->
                false
            | GrainPersistedSideEffect.RpcTriggerSubscriptionOnGrain _ ->
                true
        )
        |> fun (triggerSubSideEffects, otherPersistedSideEffects) ->
            triggerSubSideEffects
            |> List.choose (fun (sideEffectId, persistedSideEffect) ->
                match persistedSideEffect with
                | GrainPersistedSideEffect.RpcTriggerSubscriptionOnGrain rpc ->
                    Some (sideEffectId, rpc)
                | _ ->
                    None
            ), otherPersistedSideEffects
        |> fun (triggerSubSideEffects, otherPersistedSideEffects) ->
            triggerSubSideEffects
            |> List.groupBy (fun (_, rpc) -> rpc.LifeEvent)
            |> List.collect (fun (lifeEvent, sideEffects) ->
                let lifeEventJson = CodecLib.StjCodecs.Extensions.toJsonTextChecked lifeEvent
                sideEffects
                |> List.map (fun (sideEffectId, subscriptionRpc) -> sideEffectId, Choice1Of2 (CodecFriendlyGrainPersistedSideEffect.RpcTriggerSubscriptionOnGrain (subscriptionRpc.AsCodecFriendlyData lifeEventJson)))
            )
            |> List.append
                (otherPersistedSideEffects
                 |> List.map (fun (sideEffectId, persistedSideEffect) -> sideEffectId, persistedSideEffect.AsCodecFriendlyDataUnsafeForTriggerSubscriptionOnGrain)) // safe because we filtered out RpcTriggerSubscriptionOnGrain above


// CODECs

#if !FABLE_COMPILER

open CodecLib

type TraceContext with
    static member get_Codec () = ofObjCodec <| codec {
        let! telemetryUserId    = reqWith Codecs.string "UserId" (fun x -> Some x.TelemetryUserId)
        and! parentId           = reqWith Codecs.string "ParentId" (fun x -> Some x.ParentId)
        and! telemetrySessionId = optWith Codecs.string "SessionId" (fun x -> x.TelemetrySessionId)
        return { TelemetryUserId = telemetryUserId; TelemetrySessionId = telemetrySessionId; ParentId = parentId } }

type GrainRpcOperation with
   // Note: make this method inline if you want to see how build dies with OOM
   static member get_ObjCodec_V1 () =
       function
       | RunActionOnGrain _ ->
           codec {
               let! payload = reqWith (Codecs.tuple2 defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, LifeAction> Codecs.boolean) "RunActionOnGrain" (function (RunActionOnGrain (x1, x2)) -> Some (x1, x2) | _ -> None)
               return RunActionOnGrain payload
           }
           |> withDecoders [
               decoder {
                   let! x1, x2, _ = reqDecodeWithCodec (Codecs.tuple3 defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, LifeAction> Codecs.boolean Codecs.boolean) "RunActionOnGrain"
                   return RunActionOnGrain (x1, x2)
               }
           ]
       | RunActionOnGrainAndSubscribe _ ->
           codec {
               let! payload = reqWith (Codecs.tuple3 defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, LifeAction> Codecs.boolean (Codecs.gmap Codecs.string defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, LifeEvent>)) "RunActionOnGrainAndSubscribe" (function (RunActionOnGrainAndSubscribe (x1, x2, x3)) -> Some (x1, x2, x3) | _ -> None)
               return RunActionOnGrainAndSubscribe payload
           }
       | InitializeGrain _ ->
           codec {
               let! payload = reqWith (Codecs.tuple2 defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, Constructor> Codecs.boolean) "InitializeGrain" (function (InitializeGrain (x1, x2))  -> Some (x1, x2) | _ -> None)
               return InitializeGrain payload
           }
           |> withDecoders [
               decoder {
                   let! x1, x2, _ = reqDecodeWithCodec (Codecs.tuple3 defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, Constructor> Codecs.boolean Codecs.boolean) "InitializeGrain"
                   return InitializeGrain (x1, x2)
               }
            ]
       | InitializeGrainAndSubscribe _ ->
           codec {
               let! payload = reqWith (Codecs.tuple3 defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, Constructor> Codecs.boolean (Codecs.gmap Codecs.string defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, LifeEvent>)) "InitializeGrainAndSubscribe" (function (InitializeGrainAndSubscribe (x1, x2, x3)) -> Some (x1, x2, x3) | _ -> None)
               return InitializeGrainAndSubscribe payload
           }
           |> withDecoders [
               decoder {
                   let! x1, x2, x3, _ = reqDecodeWithCodec (Codecs.tuple4 defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, Constructor> Codecs.boolean (Codecs.gmap Codecs.string defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, LifeEvent>) Codecs.boolean) "InitializeGrainAndSubscribe"
                   return InitializeGrainAndSubscribe (x1, x2, x3)
               }
            ]
       | SubscribeToGrain _                           ->
           codec {
               let! payload = reqWith (Codecs.gmap Codecs.string defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, LifeEvent>) "SubscribeToGrain" (function SubscribeToGrain x -> Some x | _ -> None)
               return SubscribeToGrain payload
           }
           |> withDecoders [
               decoder {
                   let! x, _ = reqDecodeWithCodec (Codecs.tuple2 (Codecs.gmap Codecs.string defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, LifeEvent>) Codecs.boolean) "SubscribeToGrain"
                   return SubscribeToGrain x
               }
           ]
       | UnsubscribeFromGrain _ ->
           codec {
               let! payload = reqWith (Codecs.set Codecs.string) "UnsubscribeFromGrain" (function (UnsubscribeFromGrain x) -> Some x | _ -> None)
               return UnsubscribeFromGrain payload
           }
       | RunActionMaybeConstructOnGrain _ ->
           codec {
               let! payload = reqWith (Codecs.tuple3 defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, LifeAction> defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, Constructor> Codecs.boolean) "RunActionMaybeConstructOnGrain" (function (RunActionMaybeConstructOnGrain (x1, x2, x3)) -> Some (x1, x2, x3) | _ -> None)
               return RunActionMaybeConstructOnGrain payload
           }
           |> withDecoders [
               decoder {
                   let! x1, x2, x3, _ = reqDecodeWithCodec (Codecs.tuple4 defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, LifeAction> defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, Constructor> Codecs.boolean Codecs.boolean) "RunActionMaybeConstructOnGrain"
                   return RunActionMaybeConstructOnGrain (x1, x2, x3)
               }
            ]
       | RunActionMaybeConstructAndSubscribeOnGrain _ ->
           codec {
               let! payload = reqWith (Codecs.tuple4 defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, LifeAction> defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, Constructor> Codecs.boolean (Codecs.gmap Codecs.string defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, LifeEvent>)) "RunActionMaybeConstructAndSubscribeOnGrain" (function (RunActionMaybeConstructAndSubscribeOnGrain (x1, x2, x3, x4)) -> Some (x1, x2, x3, x4) | _ -> None)
               return RunActionMaybeConstructAndSubscribeOnGrain payload
           }
           |> withDecoders [
               decoder {
                   let! x1, x2, x3, x4, _ = reqDecodeWithCodec (Codecs.tuple5 defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, LifeAction> defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, Constructor> Codecs.boolean (Codecs.gmap Codecs.string defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, LifeEvent>) Codecs.boolean) "RunActionMaybeConstructAndSubscribeOnGrain"
                   return RunActionMaybeConstructAndSubscribeOnGrain (x1, x2, x3, x4)
               }
            ]
       |> mergeUnionCases

   static member get_ObjCodec () = GrainRpcOperation.get_ObjCodec_V1 ()
   static member get_Codec () = ofObjCodec <| GrainRpcOperation.get_ObjCodec ()

type GrainRpcTransactionStepOperation with
   // Note: make this method inline if you want to see how build dies with OOM
   static member get_Codec () =
       function
       | PrepareInitializeGrain _ ->
           codec {
               let! payload = reqWith defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, Constructor> "PrepareInitializeGrain" (function (PrepareInitializeGrain x) -> (Some x) | _ -> None)
               return PrepareInitializeGrain payload
           }
       | PrepareActionOnGrain _ ->
           codec {
               let! payload = reqWith defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, LifeAction> "PrepareActionOnGrain" (function (PrepareActionOnGrain x) -> (Some x) | _ -> None)
               return PrepareActionOnGrain payload
           }
       | CommitPreparedOnGrain ->
           codec {
               let! _ = reqWith Codecs.unit "CommitPreparedOnGrain" (function CommitPreparedOnGrain -> Some () | _ -> None)
               return CommitPreparedOnGrain
           }
       | RollbackPreparedOnGrain ->
           codec {
               let! _ = reqWith Codecs.unit "RollbackPreparedOnGrain" (function RollbackPreparedOnGrain -> Some () | _ -> None)
               return RollbackPreparedOnGrain
           }
       | CheckPhantomPreparedOnGrain ->
           codec {
               let! _ = reqWith Codecs.unit "CheckPhantomPreparedOnGrain" (function CheckPhantomPreparedOnGrain -> Some () | _ -> None)
               return CheckPhantomPreparedOnGrain
           }
       |> mergeUnionCases
       |> ofObjCodec

type GrainRpc with
    static member get_Codec () = ofObjCodec <| codec {
        let! subjectReference  = reqWith (SubjectReference.get_Codec ()) "SubjectReference" (fun x -> Some x.SubjectReference)
        and! rpcOperation      = reqWith (GrainRpcOperation.get_Codec ()) "RpcOperation" (fun x -> Some x.RpcOperation)
        and! maybeTraceContext = optWith (TraceContext.get_Codec ()) "TraceContext" (fun x -> Some x.TraceContext)
        // TODO: make field required when pre-existing side-effects flushed
        let traceContext = maybeTraceContext |> Option.defaultValue emptyTraceContext
        return { SubjectReference = subjectReference; RpcOperation = rpcOperation; TraceContext = traceContext } }

type GrainRpcTransactionStep with
    static member get_ObjCodec_V1 () = codec {
        let! subjectReference  = reqWith (SubjectReference.get_Codec ()) "SubjectReference" (fun x -> Some x.SubjectReference)
        and! transactionId     = reqWith (SubjectTransactionId.get_Codec ()) "TransactionId" (fun x -> Some x.TransactionId)
        and! batchNo           = reqWith Codecs.uint16 "BatchNo" (fun x -> Some x.BatchNo)
        and! opNo              = reqWith Codecs.uint16 "OpNo" (fun x -> Some x.OpNo)
        and! rpcOperation      = reqWith (GrainRpcTransactionStepOperation.get_Codec ()) "RpcOperation" (fun x -> Some x.RpcOperation)
        and! traceContext      = reqWith (TraceContext.get_Codec ()) "TraceContext" (fun x -> Some x.TraceContext)
        return { SubjectReference = subjectReference; TransactionId = transactionId; BatchNo = batchNo; OpNo = opNo; RpcOperation = rpcOperation; TraceContext = traceContext } }

    static member get_Codec () = ofObjCodec (GrainRpcTransactionStep.get_ObjCodec_V1())

type GrainTriggerSubscriptionRpc with
    static member get_Codec () = ofObjCodec <| codec {
        let! subjectPKeyReference    = reqWith (SubjectPKeyReference.get_Codec ()) "SubjectPKeyReference" (fun x -> Some x.SubjectPKeyReference)
        and! subscriptionTriggerType = reqWith (SubscriptionTriggerType.get_Codec ()) "SubscriptionTriggerType" (fun x -> Some x.SubscriptionTriggerType)
        and! lifeEvent               = reqWith defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, LifeEvent> "LifeEvent" (fun x -> Some x.LifeEvent)
        and! maybeTraceContext       = optWith (TraceContext.get_Codec ()) "TraceContext" (fun x -> Some x.TraceContext)
        and! maybeDeduplicate        = optWith Codecs.boolean "Deduplicate" (fun x -> Some x.Deduplicate)
        // TODO: make fields required when old side effects pumped
        let traceContext = maybeTraceContext |> Option.defaultValue emptyTraceContext
        let deduplicate = maybeDeduplicate |> Option.defaultValue false
        return { SubjectPKeyReference = subjectPKeyReference; SubscriptionTriggerType = subscriptionTriggerType ; LifeEvent = lifeEvent; TraceContext = traceContext; Deduplicate = deduplicate } }

type CodecFriendlyGrainTriggerSubscriptionRpc with
    static member get_Codec () = ofObjCodec <| codec {
        let! subjectPKeyReference    = reqWith (SubjectPKeyReference.get_Codec ()) "SubjectPKeyReference" (fun x -> Some x.SubjectPKeyReference)
        and! subscriptionTriggerType = reqWith (SubscriptionTriggerType.get_Codec ()) "SubscriptionTriggerType" (fun x -> Some x.SubscriptionTriggerType)
        and! lifeEventJson           = reqWith Codecs.string "LifeEventJson" (fun x -> Some x.LifeEventJson)
        and! maybeTraceContext       = optWith (TraceContext.get_Codec ()) "TraceContext" (fun x -> Some x.TraceContext)
        and! maybeDeduplicate        = optWith Codecs.boolean "Deduplicate" (fun x -> Some x.Deduplicate)
        // TODO: make fields required when old side effects pumped
        let traceContext = maybeTraceContext |> Option.defaultValue emptyTraceContext
        let deduplicate = maybeDeduplicate |> Option.defaultValue false
        return { SubjectPKeyReference = subjectPKeyReference; SubscriptionTriggerType = subscriptionTriggerType ; LifeEventJson = lifeEventJson; TraceContext = traceContext; Deduplicate = deduplicate } }

type TriggerSubscriptionResponse<'LifeAction, 'OpError when 'OpError :> OpError and 'LifeAction :> LifeAction>
with
    static member inline get_Codec () =
        function
        | ActOk _ ->
            codec {
                let! payload = reqWith defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'lifeAction> "ActOk" (function ActOk x -> Some x | _ -> None)
                return ActOk payload
            }
        | ActError _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'opError> defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'lifeAction>) "ActError" (function ActError (x1, x2) -> Some (x1, x2) | _ -> None)
                return ActError payload
            }
        | ActNotAllowed _ ->
            codec {
                let! payload = reqWith defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'lifeAction> "ActNotAllowed" (function ActNotAllowed x -> Some x | _ -> None)
                return ActNotAllowed payload
            }
        | Exn _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 Codecs.string (Codecs.option defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'lifeAction>)) "Exn" (function Exn (x1, x2) -> Some (x1, x2) | _ -> None)
                return Exn payload
            }
        |> mergeUnionCases
        |> ofObjCodec

    static member CastUnsafe(response: TriggerSubscriptionResponse<#LifeAction, #OpError>) : TriggerSubscriptionResponse<'LifeAction, 'OpError> =
        match response with
        | ActOk x ->
            ActOk (x |> box :?> 'LifeAction)
        | ActError (x1, x2) ->
            ActError (x1 |> box :?> 'OpError, x2 |> box :?> 'LifeAction)
        | ActNotAllowed x ->
            ActNotAllowed (x |> box :?> 'LifeAction)
        | Exn (x1, x2) ->
            Exn (x1, x2 |> Option.map (fun a -> a |> box :?> 'LifeAction))

type SideEffectTarget with
    static member get_Codec () =
        function
        | TimeSeries _ ->
            codec {
                let! payload = reqWith codecFor<_, TimeSeriesKey> "TS" (function | TimeSeries x -> Some x)
                return TimeSeries payload
            }
        |> mergeUnionCases
        |> ofObjCodec

type TypedTimeSeriesSideEffect<'TimeSeriesDataPoint, 'TimeSeriesId, [<Measure>] 'UnitOfMeasure
    when 'TimeSeriesDataPoint :> TimeSeriesDataPoint<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure>
    and  'TimeSeriesId :> TimeSeriesId<'TimeSeriesId>> with
    static member get_Codec () =
        function
        | (Ingest _ : TypedTimeSeriesSideEffect<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure>) ->
            codec {
                let! payload = reqWith (Codecs.tuple2 (Codecs.list ('TimeSeriesDataPoint.Codec())) codecFor<_, TraceContext>) "Ingest" (function | Ingest (x1, x2) -> Some (x1, x2))
                return Ingest payload
            }
        |> mergeUnionCases
        |> ofObjCodec

type CodecFriendlyGrainPersistedSideEffect<'LifeAction, 'OpError when 'LifeAction :> LifeAction and 'OpError :> OpError> with

    static member inline get_Codec () : Codec<_, CodecFriendlyGrainPersistedSideEffect<'lifeAction, 'opError>> =
        function
        | RunActionOnSelf _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'lifeAction> (Codecs.option codecFor<_, TraceContext>)) "RunActionOnSelf" (function (RunActionOnSelf (x1, x2)) -> Some (x1, x2) | _ -> None)
                return RunActionOnSelf payload
            }
        | Rpc _ ->
            codec {
                let! payload = reqWith (GrainRpc.get_Codec ()) "Rpc" (function (Rpc x) -> Some x | _ -> None)
                return Rpc payload
            }
        | RpcTransactionStep _ ->
            codec {
                let! payload = reqWith (GrainRpcTransactionStep.get_Codec ()) "RpcTransactionStep" (function (RpcTransactionStep x) -> Some x | _ -> None)
                return RpcTransactionStep payload
            }
        | RpcTriggerSubscriptionOnGrain _ ->
            codec {
                let! payload = reqWith (CodecFriendlyGrainTriggerSubscriptionRpc.get_Codec ()) "RpcTriggerSubscriptionOnGrain" (function (RpcTriggerSubscriptionOnGrain x) -> Some x | _ -> None)
                return RpcTriggerSubscriptionOnGrain payload
            }
            |> withDecoders [
                decoder {
                    let! payload = reqDecodeWithCodec (GrainTriggerSubscriptionRpc.get_Codec ()) "RpcTriggerSubscriptionOnGrain"
                    return RpcTriggerSubscriptionOnGrain (payload.AsCodecFriendlyData (CodecLib.StjCodecs.Extensions.toJsonTextChecked payload.LifeEvent))
                }
            ]
        | UpdateTimer _ ->
            codec {
                let! payload = reqWith Codecs.dateTimeOffset "UpdateTimer" (function (UpdateTimer x) -> Some x | _ -> None)
                return UpdateTimer payload
            }
        | ClearTimer _ ->
            codec {
                let! payload = reqWith Codecs.dateTimeOffset "ClearTimer" (function (ClearTimer x) -> Some x | _ -> None)
                return ClearTimer payload
            }
        | TriggerTimerActionOnSelf _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'lifeAction> (Codecs.option codecFor<_, TraceContext>)) "TriggerTimerActionOnSelf" (function (TriggerTimerActionOnSelf (x1, x2)) -> Some (x1, x2) | _ -> None)
                return TriggerTimerActionOnSelf payload
            }
        | HandleSubscriptionResponseOnSelf _ ->
            codec {
                let! payload = reqWith (Codecs.tuple4 codecFor<_, TriggerSubscriptionResponse<'lifeAction, 'opError>> codecFor<_, SubscriptionTriggerType> defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, LifeEvent> codecFor<_, TraceContext>) "HandleSubscriptionResponseOnSelf" (function (HandleSubscriptionResponseOnSelf (x1, x2, x3, x4)) -> Some (x1, x2, x3, x4) | _ -> None)
                return HandleSubscriptionResponseOnSelf payload
            }
        | TryDeleteSelf _ ->
            codec {
                let! payload = reqWith (Codecs.tuple3 Codecs.uint64 Codecs.uint64 Codecs.byte) "TryDeleteSelf" (function (TryDeleteSelf (x1, x2, x3)) -> Some (x1, x2, x3) | _ -> None)
                return TryDeleteSelf payload
            }
            |> withDecoders [
                decoder {
                    let! x1, x2 = reqDecodeWithCodec (Codecs.tuple2 Codecs.uint64 Codecs.uint64) "TryDeleteSelf"
                    return TryDeleteSelf (x1, x2, 0uy)
                }
            ]
        |> mergeUnionCases
        |> ofObjCodec

    static member CastUnsafe (sideEffect: CodecFriendlyGrainPersistedSideEffect<#LifeAction, #OpError>) : CodecFriendlyGrainPersistedSideEffect<'LifeAction, 'OpError> =
        match sideEffect with
        | RunActionOnSelf (x1, x2) ->
            RunActionOnSelf (x1 |> box :?> 'LifeAction, x2)
        | Rpc x ->
            Rpc x
        | RpcTransactionStep x ->
            RpcTransactionStep x
        | RpcTriggerSubscriptionOnGrain x ->
            RpcTriggerSubscriptionOnGrain x
        | UpdateTimer x ->
            UpdateTimer x
        | ClearTimer x ->
            ClearTimer x
        | TriggerTimerActionOnSelf (x1, x2) ->
            TriggerTimerActionOnSelf (x1 |> box :?> 'LifeAction, x2)
        | HandleSubscriptionResponseOnSelf (x1, x2, x3, x4) ->
            HandleSubscriptionResponseOnSelf (TriggerSubscriptionResponse<'LifeAction, 'OpError>.CastUnsafe x1, x2, x3, x4)
        | TryDeleteSelf (x1, x2, x3) ->
            TryDeleteSelf (x1, x2, x3)

type TickState with
    static member get_Codec () =
        function
        | NoTick ->
            codec {
                let! _ = reqWith Codecs.unit "NoTick" (function NoTick -> Some () | _ -> None)
                return NoTick
            }
        | Scheduled _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 (Codecs.dateTimeOffset) (Codecs.option (TraceContext.get_Codec ()))) "Scheduled" (function (Scheduled (x1, x2)) -> Some (x1, x2) | _ -> None)
                return Scheduled payload
            }
        | Fired _ ->
            codec {
                let! payload = reqWith Codecs.dateTimeOffset "Fired" (function (Fired x) -> Some x | _ -> None)
                return Fired payload
            }
        |> mergeUnionCases
        |> ofObjCodec

type AssumedCurrentReminder with
    static member get_Codec () =
        function
        | NotSet ->
            codec {
                let! _ = reqWith Codecs.unit "NotSet" (function NotSet -> Some () | _ -> None)
                return NotSet
            }
        | Set _ ->
            codec {
                let! payload = reqWith Codecs.dateTimeOffset "Set" (function (Set x) -> Some x | _ -> None)
                return Set payload
            }
        | Unknown ->
            codec {
                let! _ = reqWith Codecs.unit "Unknown" (function Unknown -> Some () | _ -> None)
                return Unknown
            }
        |> mergeUnionCases
        |> ofObjCodec

type ReminderUpdate with
    static member get_Codec () =
        codec {
            let! assumedCurrent = reqWith codecFor<_, AssumedCurrentReminder> "Assumed" (fun x -> Some x.AssumedCurrent)
            and! on = reqWith (Codecs.option Codecs.dateTimeOffset) "On" (fun x -> Some x.On)
            return { AssumedCurrent = assumedCurrent; On = on } }
        |> ofObjCodec

type SubjectState<'Subject, 'SubjectId when 'Subject :> Subject<'SubjectId> and 'SubjectId :> SubjectId> with
    // must be inline, or project won't compile
    static member inline get_Codec () : Codec<_, SubjectState<'subject, 'subjectId>> =
        ofObjCodec <|
        codec {
            let! subject          = reqWith defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'subject> "Subject" (fun x -> Some x.Subject)
            and! lastUpdatedOn    = reqWith Codecs.dateTimeOffset "LastUpdatedOn" (fun x -> Some x.LastUpdatedOn)
            and! tickState        = reqWith (TickState.get_Codec ()) "TickState" (fun x -> Some x.TickState)
            and! ourSubscriptions = reqWith (Codecs.gmap Codecs.string (SubjectReference.get_Codec ())) "OurSubscriptions" (fun x -> Some x.OurSubscriptions)
            return { Subject = subject; LastUpdatedOn = lastUpdatedOn; TickState = tickState; OurSubscriptions = ourSubscriptions }
        }

    static member CastUnsafe (data: SubjectState<#Subject<'SubjectId>, 'SubjectId>) : SubjectState<'Subject, 'SubjectId> =
        {
            Subject          = data.Subject |> box :?> 'Subject
            LastUpdatedOn    = data.LastUpdatedOn
            TickState       = data.TickState
            OurSubscriptions = data.OurSubscriptions
        }

#endif
