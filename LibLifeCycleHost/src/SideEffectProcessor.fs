[<AutoOpen>]
module LibLifeCycleHost.SideEffectProcessor

open LibLifeCycle.LifeCycles.Transaction
open LibLifeCycleHost
open LibLifeCycleHost.GrainStorageModel
open LibLifeCycle
open LibLifeCycleCore
open LibLifeCycleCore.OrleansEx.TraceContextGrainCallFilter
open System.Threading.Tasks
open LibLifeCycleHost.Web
open LibLifeCycleHost.TelemetryModel
open Orleans
open Orleans.Runtime
open System
open System.Reflection
open Microsoft.Extensions.DependencyInjection
open LibLangFsharp.SafeMailboxProcessor

type AnchorTypeForModule = private AnchorTypeForModule of unit

[<RequireQualifiedAccess>]
type SideEffectProcessorMessage<'LifeAction, 'OpError when 'LifeAction :> LifeAction and 'OpError :> OpError> =
| ProcessSideEffectGroup of SideEffectGroup<'LifeAction, 'OpError>
| ShutdownSideEffectProcessor of AsyncReplyChannel<unit>
| NoOpAndContinue of AsyncReplyChannel<unit> // this is used to wait until side effects flushed, but without the shutdown

[<RequireQualifiedAccess>]
type private DispatchGrainRpc<'LifeAction, 'OpError when 'LifeAction :> LifeAction and 'OpError :> OpError> =
| Rpc of GrainRpcOperation * SubjectReference
| TriggerSubscription of GrainTriggerSubscriptionRpc
| TriggerTimerActionOnSelf of TentativeDueAction: 'LifeAction
| HandleSubscriptionResponseOnSelf of TriggerSubscriptionResponse<'LifeAction, 'OpError>
| TryDeleteSelf of GrainSideEffectId * RequiredVersion: uint64 * RequiredNextSideEffectSequenceNumber: uint64 * RetryAttempt: byte

type private SideEffectTracing =
| DoNotSendTelemetry of TraceContext
| SendTelemetry      of TraceContext * OperationType * TelemetryName: string * CustomProperties: Map<string, string>

type private DispatchSideEffectNormalResult =
| DispatchSuccess
| DispatchTransientFailure of string * MaxDelayBeforeRetry: TimeSpan
| DispatchPermanentFailure of string * SideEffectFailureSeverity

type private DispatchSideEffectResult =
| Normal         of DispatchSideEffectNormalResult
| FatalTransient of string

type private ProcessedSideEffectResult =
| ProcessedNormal of RetryNo: uint32 * Success: bool
| ProcessedFatalTransient of RetryNo: uint32 // fatal error signals SE processor to shut down

type private SideEffectProcessingStatus =
| NotDispatched
| Dispatched of GrainSideEffectResult // dispatched but side effect status not updated in storage
| Processed of ProcessedSideEffectResult // dispatched and side effect status updated in storage

[<RequireQualifiedAccess>]
type private RpcArgsAndRetValToHandle =
| Act                           of LifeAction * Result<unit, SubjectFailure<GrainTransitionError<OpError>>>
| TriggerTimer                  of Result<Option<LifeAction>, GrainTriggerTimerError<OpError, LifeAction>>
| ActAndSubscribe               of LifeAction * Map<SubscriptionName, LifeEvent> * Result<unit, SubjectFailure<GrainTransitionError<OpError>>>
| Construct                     of Constructor * OkIfAlreadyInitialized: bool * Result<unit, SubjectFailure<GrainConstructionError<OpError>>>
| ConstructAndSubscribe         of Constructor * OkIfAlreadyInitialized: bool * Map<SubscriptionName, LifeEvent> * Result<unit, SubjectFailure<GrainConstructionError<OpError>>>
| Subscribe                     of Map<SubscriptionName, LifeEvent> * Result<unit, SubjectFailure<GrainSubscriptionError>>
| ActMaybeConstruct             of Constructor * LifeAction * Result<unit, SubjectFailure<GrainOperationError<OpError>>>
| ActMaybeConstructAndSubscribe of Constructor * LifeAction * Map<SubscriptionName, LifeEvent> * Result<unit, SubjectFailure<GrainOperationError<OpError>>>

let private mapTxnOperationResultToReplyAction<'Op when 'Op : comparison> (result: DispatchSideEffectNormalResult) (step: GrainRpcTransactionStep)
    : Result<Option<LifeAction>, DispatchSideEffectNormalResult> =
    match result, step.RpcOperation with
    // inconclusive, let upstream code decide what to do
    | DispatchTransientFailure _, _ ->
        Error result

    // successful prepare
    | DispatchSuccess, (GrainRpcTransactionStepOperation.PrepareActionOnGrain _ | GrainRpcTransactionStepOperation.PrepareInitializeGrain _) ->
        (SubjectTransactionAction<'Op>.OnOperationPrepared step.OpNo) :> LifeAction |> Some |> Ok

    // failed prepare
    | DispatchPermanentFailure (err, _), (GrainRpcTransactionStepOperation.PrepareActionOnGrain _ | GrainRpcTransactionStepOperation.PrepareInitializeGrain _) ->
        (SubjectTransactionAction<'Op>.OnOperationFailed (step.OpNo, NonemptyString.ofStringUnsafe err)) :> LifeAction |> Some |> Ok

    // any definitive (non-transient) response for commit / rollback treated as finalized
    | (DispatchSuccess | DispatchPermanentFailure _), GrainRpcTransactionStepOperation.CommitPreparedOnGrain
    | (DispatchSuccess | DispatchPermanentFailure _), GrainRpcTransactionStepOperation.RollbackPreparedOnGrain ->
        (SubjectTransactionAction<'Op>.OnOperationFinalized step.OpNo) :> LifeAction |> Some |> Ok

    | (DispatchSuccess | DispatchPermanentFailure _), GrainRpcTransactionStepOperation.CheckPhantomPreparedOnGrain ->
        None |> Ok

let getSideEffectMailboxProcessor<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId
                when 'Subject              :> Subject<'SubjectId>
                and  'LifeAction           :> LifeAction
                and  'OpError              :> OpError
                and  'Constructor          :> Constructor
                and  'LifeEvent            :> LifeEvent
                and  'LifeEvent            :  comparison
                and  'SubjectId            :> SubjectId
                and  'SubjectId            :  comparison>
    (grainScopedServiceProvider: IServiceProvider)
    (grainPartition: GrainPartition)
    (thisSubjectId: 'SubjectId)
    (updateGrainSideEffect: GrainSideEffectId -> GrainSideEffectResult -> Task)
    (isHostGrainDeactivated: unit -> bool)
    (logger: IFsLogger)
    (valueSummarizers: ValueSummarizers)
    (operationTracker: OperationTracker)
    : MailboxProcessor<SideEffectProcessorMessage<'LifeAction, 'OpError>> =

    let connectorAdapterCollection = grainScopedServiceProvider.GetRequiredService<ConnectorAdapterCollection>()
    let adapters =                   grainScopedServiceProvider.GetRequiredService<HostedOrReferencedLifeCycleAdapterRegistry>()
    let timeSeriesAdapters =         grainScopedServiceProvider.GetRequiredService<TimeSeriesAdapterCollection>()
    let thisLifeCycleAdapter =       grainScopedServiceProvider.GetRequiredService<HostedLifeCycleAdapter<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>>()
    let hostEcosystemGrainFactory =  grainScopedServiceProvider.GetRequiredService<IGrainFactory>()
    let grainProvider =              grainScopedServiceProvider.GetRequiredService<IBiosphereGrainProvider>()
    let clock =                      grainScopedServiceProvider.GetRequiredService<Service<Clock>>()
    let transientFailureHook =       grainScopedServiceProvider.GetService<ITransientSideEffectFailureHook>() |> Option.ofObj
    let thisLifeCycleKey =           thisLifeCycleAdapter.LifeCycle.Def.LifeCycleKey
    let thisEcosystemName =          thisLifeCycleKey.EcosystemName
    let thisSubjectRef: SubjectReference = { LifeCycleKey = thisLifeCycleKey; SubjectId = thisSubjectId }
    let thisSubjectPKeyRef: SubjectPKeyReference = thisSubjectRef.SubjectPKeyReference

    let targetRefStr (lifeCycleKey: LifeCycleKey) =
        let qualifiedLifeCycleName =
            match lifeCycleKey with
            | LifeCycleKey (lifeCycleName, ecosystemName) ->
                sprintf "%s/%s" ecosystemName lifeCycleName
            | OBSOLETE_LocalLifeCycleKey _ ->
                failwith "unexpected obsolete local LC key in SE processor"
        sprintf "LifeCycle:%s/Id:%s" qualifiedLifeCycleName

    let targetRefStr' (subjectRef: SubjectReference) =
        targetRefStr subjectRef.LifeCycleKey subjectRef.SubjectId.IdString

    let waitForBeforeRetry (maxDelayBeforeRetry: TimeSpan) (numRetriesSoFar: uint32) : TimeSpan =
        match transientFailureHook with
        | None ->
            if numRetriesSoFar > 30u then // avoid arithmetic overflow
                maxDelayBeforeRetry
            else
                min maxDelayBeforeRetry (pown 2 (int numRetriesSoFar) |> TimeSpan.FromSeconds)
        | Some _ ->
            TimeSpan.Zero // don't wait before retry in test mode


    let handleSideEffectResponses (sideEffectId: GrainSideEffectId) (subjectReference: SubjectReference) (orderedResponses: list<SideEffectResponse>) : Task<DispatchSideEffectNormalResult> =
        let decisionOrResponseHandlerException response =
            try
                thisLifeCycleAdapter.LifeCycle.ResponseHandler response
                |> Seq.tryHead
                |> Option.map (fun decision -> decision.Value)
                |> Option.defaultWith (fun () ->
                    match response with
                    | SideEffectResponse.Success _ -> SideEffectSuccessDecision.RogerThat |> Choice1Of2
                    | SideEffectResponse.Failure f ->
                        match f with
                        | SideEffectFailure.IngestTimeSeriesError _ ->
                            // time series usually not critical so can be just a Warning by default
                            SideEffectFailureSeverity.Warning
                        | _ ->
                            SideEffectFailureSeverity.Error
                        |> SideEffectFailureDecision.Escalate
                        |> Choice2Of2)
                |> Ok
            with
            | exn ->
                $"Exception in response handler for response: %A{response}; %A{exn.ToString()}" |> Error

        // remember decision-or-exception for each response in list.
        let decisionsOrResponseHandlerExceptions =
            orderedResponses |> List.map decisionOrResponseHandlerException

        let responseAndDecisionMessage (response, decisionOrException) =
            let responseDecisionText =
                match decisionOrException with
                | Error exn -> $"EXCEPTION: %s{exn}"
                | Ok (Choice1Of2 successDecision) ->
                    match successDecision with
                    | SideEffectSuccessDecision.RogerThat -> "RogerThat"
                    | SideEffectSuccessDecision.Continue action -> $"Continue with action %A{action}"
                | Ok (Choice2Of2 failureDecision) ->
                    match failureDecision with
                    | SideEffectFailureDecision.Dismiss -> "Dismiss"
                    | SideEffectFailureDecision.Escalate severity -> $"Escalate to %A{severity}"
                    | SideEffectFailureDecision.Compensate action -> $"Compensate with action %A{action}"
            match response with
            | SideEffectResponse.Failure failure ->
                match failure with
                | SideEffectFailure.ActError (subjectRef, action, err) ->
                    $"[RPC to %s{targetRefStr' subjectRef} Error] Transition Error %A{err} when running Action %A{action} "
                | SideEffectFailure.ActNotAllowed (subjectRef, action) ->
                    $"[RPC to %s{targetRefStr' subjectRef} Error] Transition Not Allowed when running Action %A{action}"
                | SideEffectFailure.ActNotInitialized (subjectRef, action) ->
                    $"[RPC to %s{targetRefStr' subjectRef} Error] Subject not initialized when running Action %A{action}"
                | SideEffectFailure.ConstructError (subjectRef, ctor, err) ->
                    $"[RPC to %s{targetRefStr' subjectRef} Error] Construction Error %A{err} when running Ctor %A{ctor}"
                | SideEffectFailure.ConstructAlreadyInitialized (subjectRef, ctor) ->
                    $"[RPC to %s{targetRefStr' subjectRef} Error] Subject already initialized when running Ctor %A{ctor}"
                | SideEffectFailure.SubscribeNotInitialized (subjectRef, subscriptions) ->
                    $"[RPC to %s{targetRefStr' subjectRef} Error] Subject is not initialized running SubscribeToGrain with value %A{subscriptions}"
                | SideEffectFailure.IngestTimeSeriesError (timeSeriesKey, _, err) ->
                    $"[Ingest %s{timeSeriesKey.EcosystemName}/%s{timeSeriesKey.LocalTimeSeriesName} Error] : %A{err}"
            | SideEffectResponse.Success success ->
                match success with
                | SideEffectSuccess.ConstructOk (subjectRef, ctor) ->
                    $"[RPC to %s{targetRefStr' subjectRef} OK] Construct OK. Ctor %A{ctor}"
                | SideEffectSuccess.ActOk (subjectRef, action) ->
                    $"[RPC to %s{targetRefStr' subjectRef} OK] Act OK. Action %A{action}"
                | SideEffectSuccess.SubscribeOk (subjectRef, subscriptions) ->
                    $"[RPC to %s{targetRefStr' subjectRef} OK] Subscribe OK. Subscriptions: %A{subscriptions}"
            |> fun rpcResponseMessage ->
                $"%s{rpcResponseMessage} => Response handler decision: %s{responseDecisionText}"

        let preliminaryWorstResult =
            decisionsOrResponseHandlerExceptions
            |> List.fold (fun worstResultSoFar decisionOrException ->
                match worstResultSoFar, decisionOrException with
                // already as bad as it gets
                | Error SideEffectFailureSeverity.Error, _ ->
                    worstResultSoFar
                // can't be made worse by a success or proper error handling, keep as is
                | _, Ok (Choice1Of2 (SideEffectSuccessDecision.Continue _))
                | _, Ok (Choice1Of2 SideEffectSuccessDecision.RogerThat)
                | _, Ok (Choice2Of2 (SideEffectFailureDecision.Compensate _))
                | _, Ok (Choice2Of2 SideEffectFailureDecision.Dismiss) ->
                    worstResultSoFar
                // ran into a permanent error
                | _, Ok (Choice2Of2 (SideEffectFailureDecision.Escalate SideEffectFailureSeverity.Error))
                | _, Error _ ->
                    Error SideEffectFailureSeverity.Error
                // was Ok but now error of some severity
                | Ok (), Ok (Choice2Of2 (SideEffectFailureDecision.Escalate severity)) ->
                    Error severity
                // was Warning and still is
                | Error SideEffectFailureSeverity.Warning, Ok (Choice2Of2 (SideEffectFailureDecision.Escalate SideEffectFailureSeverity.Warning)) ->
                    worstResultSoFar
              ) (Ok ())

        let maybeActionsToEnqueue =
            decisionsOrResponseHandlerExceptions
            |> Seq.choose (function
                | Ok (Choice1Of2 (SideEffectSuccessDecision.Continue action))
                | Ok (Choice2Of2 (SideEffectFailureDecision.Compensate action)) ->
                    Some action
                | Ok (Choice1Of2 SideEffectSuccessDecision.RogerThat)
                | Ok (Choice2Of2 SideEffectFailureDecision.Dismiss)
                | Ok (Choice2Of2 (SideEffectFailureDecision.Escalate _))
                | Error _ ->
                    None)
            |> NonemptyList.ofSeq

        backgroundTask {
            let! finalResult =
                match maybeActionsToEnqueue with
                | None ->
                    preliminaryWorstResult |> Result.mapError (fun x -> Choice1Of2 (x, None)) |> Task.FromResult
                | Some actionsToEnqueue ->
                    let (GrainPartition grainPartition) = grainPartition
                    let grain = hostEcosystemGrainFactory.GetGrain<ISubjectGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>>(grainPartition, thisSubjectPKeyRef.SubjectIdStr)
                    // TODO: should we move it all inside grain code and make one interlocked call? Should we do the same for two-step Txn SE retrials?
                    let dedupInfo =
                        // We need unique Caller to dedup response handling continue/compensate actions.
                        // Pretend there's a special channel on target subject side that enqueues response action back to "this" subject.
                        // Why not just use "this" SE subject as Caller ? Because SE can have multiple threads that will compete for a single Dedup memory slot, effectively cancelling each other.
                        // Then why not just use Target subj as Caller? For the same reason: target grain can call "this" grain concurrently with yet another side effect, also competing for the same Dedup slot as EnqueueActions.
                        // It means we need a separate "doppleganger" Target Caller to enqueue response continuations.
                        // TODO: Can we do better?
                        //      In theory, we could've used Target subj as Caller, if target grain itself was responsible for capturing and delivering responses to the caller
                        //      via its own side effect queue (this would have some other reliability benefits but is tricky to implement).
                        let fakeResponseCaller =
                            { subjectReference.SubjectPKeyReference with
                                LifeCycleKey =
                                    match subjectReference.SubjectPKeyReference.LifeCycleKey with
                                    | LifeCycleKey (lifeCycleName, ecosystemName) ->
                                        LifeCycleKey ($"_RESPONSE@%s{lifeCycleName}", ecosystemName)
                                    | OBSOLETE_LocalLifeCycleKey _ ->
                                        failwith "unexpected obsolete local LC key in SE processor" }

                        { Id = sideEffectId // simply use side-effect Id as it's already persisted
                          Caller = fakeResponseCaller }

                    backgroundTask {
                        match!
                            (fun () -> grain.EnqueueActions dedupInfo actionsToEnqueue
                             |> OrleansTransientErrorDetection.wrapTransientExceptions) with
                        | Ok () ->
                            return preliminaryWorstResult |> Result.mapError (fun x -> Choice1Of2 (x, Some "ENQUEUE response handler actions => OK"))
                        | Error err ->
                            match err with
                            | SubjectFailure.Err GrainEnqueueActionError.LockedInTransaction ->
                                return (TimeSpan.FromSeconds 15., "Target grain locked in transaction") |> Choice2Of2 |> Error
                            | SubjectFailure.Err (GrainEnqueueActionError.SubjectNotInitialized _)
                            | SubjectFailure.Exn _ ->
                                let enqueueErrorMessage = $"ENQUEUE response handler actions => ERROR: %A{err}"
                                return (SideEffectFailureSeverity.Error, Some enqueueErrorMessage) |> Choice1Of2 |> Error
                    }

            match finalResult with
            | Ok () ->
                return DispatchSuccess
            | Error (Choice1Of2 (severity, maybeEnqueueActionsMessage)) ->
                let fullFailureMessage =
                    Seq.zip orderedResponses decisionsOrResponseHandlerExceptions
                    |> Seq.map responseAndDecisionMessage
                    |> fun messages ->
                        maybeEnqueueActionsMessage
                        |> Option.map (fun enqueueMessage -> Seq.append messages [enqueueMessage])
                        |> Option.defaultValue messages
                    |> fun messages -> String.Join ("\n", messages)
                return DispatchPermanentFailure (fullFailureMessage, severity)
            | Error (Choice2Of2 (delayBeforeRetry, transientErrorMessage)) ->
                return DispatchTransientFailure (transientErrorMessage, delayBeforeRetry)
        }

    let handleRpcGrainCallResult (sideEffectId: GrainSideEffectId) (subjectRef: SubjectReference) (argsAndRetVal: RpcArgsAndRetValToHandle) : Task<DispatchSideEffectNormalResult> =
        let handleSideEffectResponses =
            handleSideEffectResponses sideEffectId subjectRef

        let handleSideEffectFailure failure =
            handleSideEffectResponses [SideEffectResponse.Failure failure]

        let handleSideEffectSuccess success =
            handleSideEffectResponses [SideEffectResponse.Success success]

        backgroundTask {
            match argsAndRetVal with
            | RpcArgsAndRetValToHandle.Act (action, result) ->
                match result with
                | Ok () ->
                    return! handleSideEffectSuccess (SideEffectSuccess.ActOk (subjectRef, action))
                | Error (SubjectFailure.Err err) ->
                    match err with
                    | GrainTransitionError.SubjectNotInitialized _ ->
                        return! handleSideEffectFailure (SideEffectFailure.ActNotInitialized (subjectRef, action))
                    | GrainTransitionError.TransitionError terr ->
                        return! handleSideEffectFailure (SideEffectFailure.ActError (subjectRef, action, terr))
                    | GrainTransitionError.TransitionNotAllowed ->
                        return! handleSideEffectFailure (SideEffectFailure.ActNotAllowed (subjectRef, action))
                    | GrainTransitionError.LockedInTransaction ->
                        return DispatchTransientFailure ("Target grain locked in transaction", TimeSpan.FromSeconds 15.)
                    | GrainTransitionError.AccessDenied ->
                        // access denied should never really happen inter-grain, if it does - keep retrying
                        return DispatchTransientFailure ("Access to the target grain is denied", TimeSpan.FromMinutes 1.)
                | Error (SubjectFailure.Exn exnDetails) ->
                    return DispatchPermanentFailure (exnDetails, SideEffectFailureSeverity.Error)

            | RpcArgsAndRetValToHandle.TriggerTimer result ->
                match result with
                | Ok None ->
                    return DispatchSuccess
                | Ok (Some timerAction) ->
                    return! handleSideEffectSuccess (SideEffectSuccess.ActOk (subjectRef, timerAction))
                | Error (GrainTriggerTimerError.SubjectNotInitialized _) ->
                    let message = sprintf "[RPC to %s Error] Subject not initialized when triggering timer action" (targetRefStr' thisSubjectRef)
                    return DispatchPermanentFailure (message, SideEffectFailureSeverity.Error)
                | Error (GrainTriggerTimerError.TransitionError (terr, timerAction)) ->
                    return! handleSideEffectFailure (SideEffectFailure.ActError (subjectRef, timerAction, terr))
                | Error (GrainTriggerTimerError.TransitionNotAllowed timerAction) ->
                    return! handleSideEffectFailure (SideEffectFailure.ActNotAllowed (subjectRef, timerAction))
                | Error GrainTriggerTimerError.LockedInTransaction ->
                    return DispatchTransientFailure ("Target grain locked in transaction", TimeSpan.FromSeconds 15.)
                | Error (GrainTriggerTimerError.Exn (exnDetails, _)) ->
                    return DispatchPermanentFailure (exnDetails, SideEffectFailureSeverity.Error)

            | RpcArgsAndRetValToHandle.ActAndSubscribe (action, subscriptions, result) ->
                match result with
                | Ok () ->
                    let responses = [
                        SideEffectSuccess.SubscribeOk (subjectRef, subscriptions) |> SideEffectResponse.Success
                        SideEffectSuccess.ActOk (subjectRef, action) |> SideEffectResponse.Success
                    ]
                    return! handleSideEffectResponses responses
                | Error (SubjectFailure.Err err) ->
                    match err with
                    | GrainTransitionError.SubjectNotInitialized _ ->
                        let responses = [
                            SideEffectFailure.SubscribeNotInitialized (subjectRef, subscriptions) |> SideEffectResponse.Failure
                            SideEffectFailure.ActNotInitialized (subjectRef, action) |> SideEffectResponse.Failure
                        ]
                        return! handleSideEffectResponses responses
                    | GrainTransitionError.TransitionError terr ->
                        let responses = [
                            SideEffectSuccess.SubscribeOk (subjectRef, subscriptions) |> SideEffectResponse.Success
                            SideEffectFailure.ActError (subjectRef, action, terr) |> SideEffectResponse.Failure
                        ]
                        return! handleSideEffectResponses responses
                    | GrainTransitionError.TransitionNotAllowed ->
                        let responses = [
                            SideEffectSuccess.SubscribeOk (subjectRef, subscriptions) |> SideEffectResponse.Success
                            SideEffectFailure.ActNotAllowed (subjectRef, action) |> SideEffectResponse.Failure
                        ]
                        return! handleSideEffectResponses responses
                    | GrainTransitionError.LockedInTransaction ->
                        return DispatchTransientFailure ("Target grain locked in transaction", TimeSpan.FromSeconds 15.)
                    | GrainTransitionError.AccessDenied ->
                        // access denied should never really happen inter-grain, if it does - keep retrying
                        return DispatchTransientFailure ("Access to the target grain is denied", TimeSpan.FromMinutes 1.)
                | Error (SubjectFailure.Exn exnDetails) ->
                    return DispatchPermanentFailure (exnDetails, SideEffectFailureSeverity.Error)

            | RpcArgsAndRetValToHandle.ActMaybeConstruct (ctor, action, result) ->
                match result with
                | Ok _ ->
                    return! handleSideEffectSuccess (SideEffectSuccess.ActOk (subjectRef, action))
                | Error (SubjectFailure.Err err) ->
                    match err with
                    | GrainOperationError.TransitionError terr ->
                        return! handleSideEffectFailure (SideEffectFailure.ActError (subjectRef, action, terr))
                    | GrainOperationError.ConstructionError cerr ->
                        return! handleSideEffectFailure (SideEffectFailure.ConstructError (subjectRef, ctor, cerr))
                    | GrainOperationError.TransitionNotAllowed ->
                        return! handleSideEffectFailure (SideEffectFailure.ActNotAllowed (subjectRef, action))
                    | GrainOperationError.LockedInTransaction ->
                        return DispatchTransientFailure ("Target grain locked in transaction", TimeSpan.FromSeconds 15.)
                    | GrainOperationError.AccessDenied ->
                        // access denied should never really happen inter-grain, if it does - keep retrying
                        return DispatchTransientFailure ("Access to the target grain is denied", TimeSpan.FromMinutes 1.)
                | Error (SubjectFailure.Exn exnDetails) ->
                    return DispatchPermanentFailure (exnDetails, SideEffectFailureSeverity.Error)

            | RpcArgsAndRetValToHandle.ActMaybeConstructAndSubscribe (ctor, action, subscriptions, result) ->
                match result with
                | Ok _ ->
                    let responses = [
                        SideEffectSuccess.SubscribeOk (subjectRef, subscriptions) |> SideEffectResponse.Success
                        SideEffectSuccess.ActOk (subjectRef, action) |> SideEffectResponse.Success
                    ]
                    return! handleSideEffectResponses responses
                | Error (SubjectFailure.Err err) ->
                    match err with
                    | GrainOperationError.TransitionError terr ->
                        let responses = [
                            SideEffectSuccess.SubscribeOk (subjectRef, subscriptions) |> SideEffectResponse.Success
                            SideEffectFailure.ActError (subjectRef, action, terr) |> SideEffectResponse.Failure
                        ]
                        return! handleSideEffectResponses responses
                    | GrainOperationError.ConstructionError cerr ->
                        // subscriptions not created, do we need to send SubscribeNotInitialized?
                        return! handleSideEffectFailure (SideEffectFailure.ConstructError (subjectRef, ctor, cerr))
                    | GrainOperationError.TransitionNotAllowed ->
                        let responses = [
                            SideEffectSuccess.SubscribeOk (subjectRef, subscriptions) |> SideEffectResponse.Success
                            SideEffectFailure.ActNotAllowed (subjectRef, action) |> SideEffectResponse.Failure
                        ]
                        return! handleSideEffectResponses responses
                    | GrainOperationError.LockedInTransaction ->
                        return DispatchTransientFailure ("Target grain locked in transaction", TimeSpan.FromSeconds 15.)
                    | GrainOperationError.AccessDenied ->
                        // access denied should never really happen inter-grain, if it does - keep retrying
                        return DispatchTransientFailure ("Access to the target grain is denied", TimeSpan.FromMinutes 1.)
                | Error (SubjectFailure.Exn exnDetails) ->
                    return DispatchPermanentFailure (exnDetails, SideEffectFailureSeverity.Error)

            | RpcArgsAndRetValToHandle.Construct (ctor, okIfAlreadyInitialized, result) ->
                match result with
                | Ok _ ->
                    return! handleSideEffectSuccess (SideEffectSuccess.ConstructOk (subjectRef, ctor))
                | Error (SubjectFailure.Err err) ->
                    match err, okIfAlreadyInitialized with
                    | GrainConstructionError.SubjectAlreadyInitialized _, true ->
                        // .MaybeConstruct will invoke responseHandler only on actual construction
                        return DispatchSuccess
                    | GrainConstructionError.SubjectAlreadyInitialized _, false ->
                        return! handleSideEffectFailure (SideEffectFailure.ConstructAlreadyInitialized (subjectRef, ctor))
                    | GrainConstructionError.ConstructionError cerr, _ ->
                        return! handleSideEffectFailure (SideEffectFailure.ConstructError (subjectRef, ctor, cerr))
                    | GrainConstructionError.AccessDenied, _ ->
                        // access denied should never really happen inter-grain, if it does - keep retrying
                        return DispatchTransientFailure ("Access to the target grain is denied", TimeSpan.FromMinutes 1.)
                | Error (SubjectFailure.Exn exnDetails) ->
                    return DispatchPermanentFailure (exnDetails, SideEffectFailureSeverity.Error)

            | RpcArgsAndRetValToHandle.ConstructAndSubscribe(ctor, okIfAlreadyInitialized, subscriptions, result) ->
                match result with
                | Ok _ ->
                    let subscribeResponse = SideEffectSuccess.SubscribeOk (subjectRef, subscriptions) |> SideEffectResponse.Success
                    let responses =
                        if okIfAlreadyInitialized then
                            [subscribeResponse] // no ConstructOk for MaybeConstruct
                        else
                            [subscribeResponse; SideEffectSuccess.ConstructOk (subjectRef, ctor) |> SideEffectResponse.Success]
                    return! handleSideEffectResponses responses
                | Error (SubjectFailure.Err err) ->
                    match err, okIfAlreadyInitialized with
                    | GrainConstructionError.SubjectAlreadyInitialized _, true ->
                        // .MaybeConstruct will invoke responseHandler only on actual construction TODO: add separate MaybeConstruct response handler pattern?
                        let responses = [SideEffectSuccess.SubscribeOk (subjectRef, subscriptions) |> SideEffectResponse.Success]
                        return! handleSideEffectResponses responses
                    | GrainConstructionError.SubjectAlreadyInitialized _, false ->
                        let responses = [
                            SideEffectSuccess.SubscribeOk (subjectRef, subscriptions) |> SideEffectResponse.Success
                            SideEffectFailure.ConstructAlreadyInitialized (subjectRef, ctor) |> SideEffectResponse.Failure
                        ]
                        return! handleSideEffectResponses responses
                    | GrainConstructionError.ConstructionError cerr, _ ->
                        let responses = [
                            SideEffectSuccess.SubscribeOk (subjectRef, subscriptions) |> SideEffectResponse.Success
                            SideEffectFailure.ConstructError (subjectRef, ctor, cerr) |> SideEffectResponse.Failure
                        ]
                        return! handleSideEffectResponses responses
                    | GrainConstructionError.AccessDenied, _ ->
                        // access denied should never really happen inter-grain, if it does - keep retrying
                        return DispatchTransientFailure ("Access to the target grain is denied", TimeSpan.FromMinutes 1.)
                | Error (SubjectFailure.Exn exnDetails) ->
                    return DispatchPermanentFailure (exnDetails, SideEffectFailureSeverity.Error)

            | RpcArgsAndRetValToHandle.Subscribe (subscriptions, result) ->
                match result with
                | Ok _ ->
                    return! handleSideEffectSuccess (SideEffectSuccess.SubscribeOk (subjectRef, subscriptions))
                | Error err ->
                    match err with
                    | SubjectFailure.Err (GrainSubscriptionError.SubjectNotInitialized _) ->
                        return! handleSideEffectFailure (SideEffectFailure.SubscribeNotInitialized (subjectRef, subscriptions))
                    | SubjectFailure.Exn exnDetails ->
                        return DispatchPermanentFailure (exnDetails, SideEffectFailureSeverity.Error)
        }

    let dedupInfo sideEffectId =
        { Id = sideEffectId // simply use side-effect Id as it's already persisted
          Caller = thisSubjectPKeyRef }

    let dispatchGrainRpc
        (grainRpcChoice: DispatchGrainRpc<'LifeAction, 'OpError>)
        (sideEffectId: GrainSideEffectId)
        (_retryNo: uint32)
        : Task<DispatchSideEffectResult> =
        let lifeCycleKey, subjectIdStr =
            match grainRpcChoice with
            | DispatchGrainRpc.Rpc (_, ref) -> ref.LifeCycleKey, ref.SubjectId.IdString
            | DispatchGrainRpc.TriggerSubscription rpc -> rpc.SubjectPKeyReference.LifeCycleKey, rpc.SubjectPKeyReference.SubjectIdStr
            | DispatchGrainRpc.TriggerTimerActionOnSelf _
            | DispatchGrainRpc.HandleSubscriptionResponseOnSelf _
            | DispatchGrainRpc.TryDeleteSelf _ -> thisLifeCycleKey, thisSubjectId.IdString

        let targetRefStr = targetRefStr lifeCycleKey subjectIdStr

        let maybeDedupInfo deduplicate =
            if deduplicate then
                Some (dedupInfo sideEffectId)
            else
                None

        let maybeTargetLifeCycleAdapter = adapters.GetLifeCycleBiosphereAdapterByKey lifeCycleKey

        Task.map Normal <|
        match grainRpcChoice with
        | DispatchGrainRpc.Rpc (rpc, subjectRef) ->
            match maybeTargetLifeCycleAdapter with
            | Some targetLifeCycleAdapter ->
                match rpc with
                | RunActionOnGrain (action, deduplicate) ->
                    backgroundTask {
                        let! result = targetLifeCycleAdapter.RunActionOnGrain grainProvider grainPartition (maybeDedupInfo deduplicate) subjectRef.SubjectId.IdString action (* maybeBeforeActSubscriptions *) None
                        return! handleRpcGrainCallResult sideEffectId subjectRef (RpcArgsAndRetValToHandle.Act (action, result))
                    }
                | RunActionOnGrainAndSubscribe (action, deduplicate, subscriptions) ->
                    backgroundTask {
                        let beforeActSubscriptions = {
                            Subscriptions = subscriptions
                            Subscriber = thisSubjectPKeyRef
                        }
                        let! result = targetLifeCycleAdapter.RunActionOnGrain grainProvider grainPartition (maybeDedupInfo deduplicate) subjectRef.SubjectId.IdString action (Some beforeActSubscriptions)
                        return! handleRpcGrainCallResult sideEffectId subjectRef (RpcArgsAndRetValToHandle.ActAndSubscribe (action, beforeActSubscriptions.Subscriptions, result))
                    }
                | RunActionMaybeConstructOnGrain (action, ctor, deduplicate) ->
                    backgroundTask {
                        let! result = targetLifeCycleAdapter.RunActionMaybeConstructOnGrain grainProvider grainPartition (maybeDedupInfo deduplicate) subjectRef.SubjectId action ctor (* maybeConstructSubscriptions *) None
                        return! handleRpcGrainCallResult sideEffectId subjectRef (RpcArgsAndRetValToHandle.ActMaybeConstruct (ctor, action, result))
                    }
                | RunActionMaybeConstructAndSubscribeOnGrain (action, ctor, deduplicate, subscriptions) ->
                    let constructSubscriptions = {
                        Subscriptions = subscriptions
                        Subscriber    = thisSubjectPKeyRef
                    }
                    backgroundTask {
                        let! result = targetLifeCycleAdapter.RunActionMaybeConstructOnGrain grainProvider grainPartition (maybeDedupInfo deduplicate) subjectRef.SubjectId action ctor (Some constructSubscriptions)
                        return! handleRpcGrainCallResult sideEffectId subjectRef (RpcArgsAndRetValToHandle.ActMaybeConstructAndSubscribe (ctor, action, constructSubscriptions.Subscriptions, result))
                    }
                | InitializeGrain (ctor, okIfAlreadyInitialized) ->
                    backgroundTask {
                        let! result = targetLifeCycleAdapter.InitializeGrain grainProvider grainPartition okIfAlreadyInitialized (maybeDedupInfo (not okIfAlreadyInitialized)) subjectRef.SubjectId ctor (* maybeConstructSubscriptions *) None
                        return! handleRpcGrainCallResult sideEffectId subjectRef (RpcArgsAndRetValToHandle.Construct (ctor, okIfAlreadyInitialized, result))
                    }
                | InitializeGrainAndSubscribe (ctor, okIfAlreadyInitialized, subscriptions) ->
                    let constructSubscriptions = {
                        Subscriptions = subscriptions
                        Subscriber    = thisSubjectPKeyRef
                    }
                    backgroundTask {
                        let! result = targetLifeCycleAdapter.InitializeGrain grainProvider grainPartition okIfAlreadyInitialized (maybeDedupInfo (not okIfAlreadyInitialized)) subjectRef.SubjectId ctor (Some constructSubscriptions)
                        return! handleRpcGrainCallResult sideEffectId subjectRef (RpcArgsAndRetValToHandle.ConstructAndSubscribe (ctor, okIfAlreadyInitialized, constructSubscriptions.Subscriptions, result))
                    }
                | SubscribeToGrain subscriptions ->
                    backgroundTask {
                        let! result = targetLifeCycleAdapter.SubscribeToGrain grainProvider grainPartition subjectRef.SubjectId subscriptions thisSubjectPKeyRef
                        return! handleRpcGrainCallResult sideEffectId subjectRef (RpcArgsAndRetValToHandle.Subscribe (subscriptions, result))
                    }
                | UnsubscribeFromGrain (subscriptions) ->
                    backgroundTask {
                        do! targetLifeCycleAdapter.UnsubscribeFromGrain grainProvider grainPartition subjectRef.SubjectId subscriptions thisSubjectPKeyRef
                        return DispatchSuccess
                }

            | None ->
                (sprintf "[RPC to %s Error] LifeCycle with key %A not found" targetRefStr lifeCycleKey, SideEffectFailureSeverity.Error)
                |> DispatchPermanentFailure
                |> Task.FromResult

        | DispatchGrainRpc.TriggerSubscription rpc ->
            backgroundTask {
                let { SubscriptionTriggerType = subscriptionTriggerType; LifeEvent = lifeEvent; } = rpc

                match maybeTargetLifeCycleAdapter with
                | Some targetLifeCycleAdapter ->
                    match! targetLifeCycleAdapter.TriggerSubscriptionOnGrain grainProvider grainPartition (maybeDedupInfo rpc.Deduplicate) subjectIdStr subscriptionTriggerType lifeEvent with
                    | Ok () ->
                        return DispatchSuccess
                    | Error (SubjectFailure.Err err) ->
                        match err with
                        | GrainTriggerSubscriptionError.SubjectNotInitialized _ ->
                            // Subscriber doesn't exist when event published, how should we treat it?
                            // It's a valid scenario for volatile subscriber that can disappear any moment,
                            // otherwise it must be error in framework so should be escalated

                            match targetLifeCycleAdapter with
                            | :? IHostedLifeCycleAdapter as targetAdapter when targetAdapter.Storage = StorageType.Volatile ->
                                logger.Info "TRIGGER %a ==> Volatile Subscriber Is Gone: %a"
                                    (logger.P "subscription") subscriptionTriggerType
                                    (logger.P "subscriber") targetRefStr

                                // remove orphan subscription from yourself
                                match subscriptionTriggerType with
                                | SubscriptionTriggerType.Named name ->
                                    do! (thisLifeCycleAdapter :> IHostedOrReferencedLifeCycleAdapter).UnsubscribeFromGrain
                                            grainProvider grainPartition thisSubjectId (Set.singleton name) rpc.SubjectPKeyReference
                                return DispatchSuccess

                            | _ ->
                                // TODO: why not remove begone subscriber with Persistent storage just like from Volatile after first error? We should.
                                return
                                    (sprintf "[RPC to %s Error] Subject is not initialized running TriggerSubscriptionOnGrain for subscription %A and lifeEvent %A"
                                        targetRefStr subscriptionTriggerType lifeEvent,
                                        SideEffectFailureSeverity.Error)
                                    |> DispatchPermanentFailure

                        | GrainTriggerSubscriptionError.TransitionError terr ->
                            return
                                (sprintf "[RPC to %s Error] Transition Error %A when running triggering subscription %A for LifeEvent %A" targetRefStr terr subscriptionTriggerType lifeEvent,
                                 SideEffectFailureSeverity.Error)
                                |> DispatchPermanentFailure

                        | GrainTriggerSubscriptionError.TransitionNotAllowed ->
                            return
                                (sprintf "[RPC to %s Error] Not Allowed transition, triggering subscription %A for LifeEvent %A" targetRefStr subscriptionTriggerType lifeEvent,
                                 SideEffectFailureSeverity.Error)
                                |> DispatchPermanentFailure

                        | GrainTriggerSubscriptionError.LockedInTransaction ->
                            return DispatchTransientFailure ("Target grain locked in transaction", TimeSpan.FromSeconds 15.)

                    | Error (SubjectFailure.Exn exnDetails) ->
                        return DispatchPermanentFailure (exnDetails, SideEffectFailureSeverity.Error)

                | None ->
                    // no known life cycle adapter found, try to trigger it dynamically
                    let grainId = "$" // it's a singleton stateless reentrant grain
                    let (GrainPartition partitionGuid) = grainPartition

                    let! grainFactory = grainProvider.GetGrainFactory lifeCycleKey.EcosystemName
                    let grain = grainFactory.GetGrain<IDynamicSubscriptionDispatcherGrain>(partitionGuid, grainId)
                    let target: LocalSubjectPKeyReference = { LifeCycleName = lifeCycleKey.LocalLifeCycleName; SubjectIdStr = subjectIdStr }
                    match! grain.TriggerSubscription (maybeDedupInfo rpc.Deduplicate) target subscriptionTriggerType lifeEvent with
                    | Ok () ->
                        return DispatchSuccess

                    | Error (SubjectFailure.Err err) ->
                        // we are not responsible for external subscriber errors, classify them as warnings
                        match err with
                        | GrainTriggerDynamicSubscriptionError.SubjectNotInitialized _ ->
                            return
                                (sprintf "[RPC to %s Error] Subject is not initialized running TriggerSubscriptionOnGrain for subscription %A and lifeEvent %A"
                                    targetRefStr subscriptionTriggerType lifeEvent,
                                    SideEffectFailureSeverity.Warning)
                                |> DispatchPermanentFailure

                        | GrainTriggerDynamicSubscriptionError.TransitionError terr ->
                            return
                                (sprintf "[RPC to %s Error] Transition Error %A when running triggering subscription %A for LifeEvent %A"
                                    targetRefStr terr subscriptionTriggerType lifeEvent,
                                    SideEffectFailureSeverity.Warning)
                                |> DispatchPermanentFailure

                        | GrainTriggerDynamicSubscriptionError.TransitionNotAllowed ->
                            return
                                (sprintf "[RPC to %s Error] Not Allowed transition, triggering subscription %A for LifeEvent %A" targetRefStr subscriptionTriggerType lifeEvent,
                                 SideEffectFailureSeverity.Warning)
                                |> DispatchPermanentFailure

                        | GrainTriggerDynamicSubscriptionError.LockedInTransaction ->
                            return DispatchTransientFailure ("Target grain locked in transaction", TimeSpan.FromSeconds 15.)

                        | GrainTriggerDynamicSubscriptionError.LifeCycleNotFound lifeCycleName ->
                            return
                                (sprintf "[RPC to %s Error] LifeCycle with name %s not found, triggering subscription %A for LifeEvent %A" targetRefStr lifeCycleName subscriptionTriggerType lifeEvent,
                                 SideEffectFailureSeverity.Warning)
                                |> DispatchPermanentFailure

                    | Error (SubjectFailure.Exn exnDetails) ->
                        return DispatchPermanentFailure (exnDetails, SideEffectFailureSeverity.Warning)
            }

        | DispatchGrainRpc.TriggerTimerActionOnSelf tentativeDueAction ->
            backgroundTask {
                let! result = (thisLifeCycleAdapter :> IHostedOrReferencedLifeCycleAdapter).TriggerTimerActionOnGrain grainProvider grainPartition (dedupInfo sideEffectId) thisSubjectId tentativeDueAction
                return! handleRpcGrainCallResult sideEffectId thisSubjectRef (RpcArgsAndRetValToHandle.TriggerTimer result)
            }

        | DispatchGrainRpc.HandleSubscriptionResponseOnSelf response ->
            match response with
            | TriggerSubscriptionResponse.ActOk triggeredAction ->
                handleRpcGrainCallResult sideEffectId thisSubjectRef (RpcArgsAndRetValToHandle.Act (triggeredAction, Ok ()))
            | TriggerSubscriptionResponse.ActError (err, triggeredAction) ->
                handleRpcGrainCallResult sideEffectId thisSubjectRef (RpcArgsAndRetValToHandle.Act (triggeredAction, GrainTransitionError.TransitionError (err :> OpError) |> SubjectFailure.Err |> Error))
            | TriggerSubscriptionResponse.ActNotAllowed triggeredAction ->
                handleRpcGrainCallResult sideEffectId thisSubjectRef (RpcArgsAndRetValToHandle.Act (triggeredAction, GrainTransitionError.TransitionNotAllowed |> SubjectFailure.Err |> Error))
            | TriggerSubscriptionResponse.Exn (details, Some triggeredAction) ->
                handleRpcGrainCallResult sideEffectId thisSubjectRef (RpcArgsAndRetValToHandle.Act (triggeredAction, details |> SubjectFailure.Exn |> Error))
            | TriggerSubscriptionResponse.Exn (details, None) ->
                DispatchPermanentFailure (details, SideEffectFailureSeverity.Error)
                |> Task.FromResult

        | DispatchGrainRpc.TryDeleteSelf(sideEffectId, requiredVersion, requiredNextSideEffectSequenceNumber, retryAttempt) ->
            let maxRetryAttemptsToDeleteSelf = 10uy
            if retryAttempt <= maxRetryAttemptsToDeleteSelf then
                backgroundTask {
                    do! (thisLifeCycleAdapter :> IHostedOrReferencedLifeCycleAdapter).TryDeleteSelfOnGrain grainProvider grainPartition thisSubjectId sideEffectId requiredVersion requiredNextSideEffectSequenceNumber retryAttempt
                    return DispatchSuccess
                }
            else
                DispatchPermanentFailure
                    ($"TryDeleteSelf did not succeed after %d{maxRetryAttemptsToDeleteSelf} attempts, aborted to avoid risk of endless loop. Is this a bug in framework?",
                        SideEffectFailureSeverity.Warning)
                |> Task.FromResult

    let dispatchGrainRpcTransactionStep (grainRpcTxnStep: GrainRpcTransactionStep) (retryNo: uint32) : Task<DispatchSideEffectNormalResult> =
        let lifeCycleKey = grainRpcTxnStep.SubjectReference.LifeCycleKey
        let permanentFailure err = DispatchPermanentFailure (err, SideEffectFailureSeverity.Error)

        match adapters.GetLifeCycleBiosphereAdapterByKey lifeCycleKey with
        | Some targetLifeCycleAdapter ->
            let { SubjectReference = subjectRef; RpcOperation = operation; TransactionId = transactionId } = grainRpcTxnStep

            // TODO: ideally retry after conflicting prepares should be driven by events and configured txn timeouts but it'll do for now
            // too dangerous to retry infinitely because conflicting orphaned transaction can remain prepared for minutes
            let maxNumberOfRetriesForConflictingPrepare = 4u
            let maxRetryDelayForConflictingPrepare = TimeSpan.FromSeconds 2.

            backgroundTask {
                match operation with
                | PrepareActionOnGrain action ->
                    match! targetLifeCycleAdapter.RunPrepareActionOnGrain grainProvider grainPartition subjectRef.SubjectId action transactionId with
                    | Ok _ ->
                        return DispatchSuccess
                    | Error (SubjectFailure.Err err) ->
                        return
                            match err with
                            | GrainPrepareTransitionError.TransitionError opError ->
                                // TODO: need interface to convert error to readable string
                                sprintf "%O" opError
                                // apply max length
                                |> fun s -> if s.Length > 100 then s.Substring (0, 100) else s
                                |> permanentFailure

                            | GrainPrepareTransitionError.TransitionNotAllowed ->
                                permanentFailure "Transition not allowed"

                            | GrainPrepareTransitionError.SubjectNotInitialized primaryKey ->
                                sprintf "Subject not initialized: %s" primaryKey |> permanentFailure

                            | GrainPrepareTransitionError.ConflictingPrepare (SubjectTransactionId subjectTransactionGuid) ->
                                if retryNo <= maxNumberOfRetriesForConflictingPrepare then
                                    DispatchTransientFailure (sprintf "Another transaction prepared: %O, retry # %d" subjectTransactionGuid retryNo, maxRetryDelayForConflictingPrepare)
                                else
                                    permanentFailure (sprintf "Persistently locked by another transaction: %O. Make sure you are not doing more than one action on the same subject Id in a transaction." subjectTransactionGuid)

                    | Error (SubjectFailure.Exn message) ->
                        return permanentFailure message

                | PrepareInitializeGrain ctor ->
                    match! targetLifeCycleAdapter.PrepareInitializeGrain grainProvider grainPartition subjectRef.SubjectId ctor transactionId with
                    | Ok _ ->
                        return DispatchSuccess
                    | Error (SubjectFailure.Err err) ->
                        return
                            match err with
                            | GrainPrepareConstructionError.SubjectAlreadyInitialized primaryKey ->
                                sprintf "Subject already initialized: %s" primaryKey
                                |> permanentFailure

                            | GrainPrepareConstructionError.ConstructionError opError ->
                                // TODO: need interface to convert error to readable string
                                sprintf "%O" opError
                                // apply max length
                                |> fun s -> if s.Length > 100 then s.Substring (0, 100) else s
                                |> permanentFailure

                            | GrainPrepareConstructionError.ConflictingPrepare (SubjectTransactionId subjectTransactionGuid) ->
                                if retryNo <= maxNumberOfRetriesForConflictingPrepare then
                                    DispatchTransientFailure (sprintf "Another transaction prepared: %O, retry # %d" subjectTransactionGuid retryNo, maxRetryDelayForConflictingPrepare)
                                else
                                    permanentFailure (sprintf "Persistently locked by another transaction: %O. Make sure you are not doing more than one action on the same subject Id in a transaction." subjectTransactionGuid)

                    | Error (SubjectFailure.Exn message) ->
                        return permanentFailure message

                | CommitPreparedOnGrain ->
                    do! targetLifeCycleAdapter.RunCommitPreparedOnGrain grainProvider grainPartition subjectRef.SubjectId transactionId
                    return DispatchSuccess

                | RollbackPreparedOnGrain ->
                    do! targetLifeCycleAdapter.RunRollbackPreparedOnGrain grainProvider grainPartition subjectRef.SubjectId transactionId
                    return DispatchSuccess

                | CheckPhantomPreparedOnGrain ->
                    do! targetLifeCycleAdapter.RunCheckPhantomPreparedOnGrain grainProvider grainPartition subjectRef.SubjectId transactionId
                    return DispatchSuccess
            }
        | None ->
            sprintf "Unknown lifecycle: %A" lifeCycleKey |> permanentFailure |> Task.FromResult

    let lifeCycleKeyToTelemetryString (lcKey: LifeCycleKey) =
        if lcKey.EcosystemName = thisEcosystemName then
            lcKey.LocalLifeCycleName
        else
            $"{lcKey.EcosystemName}/{lcKey.LocalLifeCycleName}"

    // Need another level of indirection to obtain telemetry data before executing the side effect
    let getPersistedSideEffectTelemetryDataAndDispatchFunc
            (sideEffectId: GrainSideEffectId)
            (grainSideEffect: GrainPersistedSideEffect<'LifeAction, 'OpError>)
            // consider adding more telemetry data e.g. some payload
            : Choice<TraceContext, {| OperationType: OperationType; TelemetryName: string; Target: SubjectPKeyReference; TraceContext: TraceContext |}> *
              (GrainSideEffectId -> (* retryNo *) uint32 -> Task<DispatchSideEffectResult>) =

        match grainSideEffect with
        | GrainPersistedSideEffect.IngestTimeSeries (timeSeriesKey, ``list<'TimeSeriesDataPoint>``, traceContext) ->
            Choice1Of2 traceContext, // don't send telemetry for Ingest
            fun sideEffectId _ ->
                backgroundTask {
                    match timeSeriesAdapters.GetTimeSeriesAdapterByKey timeSeriesKey with
                    | Some adapter ->
                        match! adapter.Ingest grainScopedServiceProvider clock CallOrigin.Internal ``list<'TimeSeriesDataPoint>`` with
                        | Ok () ->
                            return DispatchSideEffectResult.Normal DispatchSideEffectNormalResult.DispatchSuccess
                        | Error err ->
                            let response = SideEffectFailure.IngestTimeSeriesError (timeSeriesKey, ``list<'TimeSeriesDataPoint>``, err) |> SideEffectResponse.Failure
                            // used only for EnqueueActions not available in TimeSeries response handling, can pass garbage
                            let invalidSubjectRef = Unchecked.defaultof<SubjectReference>
                            let! res = handleSideEffectResponses sideEffectId invalidSubjectRef [response]
                            return DispatchSideEffectResult.Normal res
                    | None ->
                        return
                            (sprintf "TimeSeries with key %A not found" timeSeriesKey,
                             SideEffectFailureSeverity.Error)
                            |> DispatchPermanentFailure
                            |> DispatchSideEffectResult.Normal
                }

        | GrainPersistedSideEffect.RunActionOnSelf (action, maybeTraceContext) ->
            (
                let traceContext = maybeTraceContext |> Option.defaultValue emptyTraceContext
                if (thisLifeCycleAdapter :> IHostedOrReferencedLifeCycleAdapter).ShouldSendTelemetry (ShouldSendTelemetryFor.LifeAction action) then
                    {|
                        OperationType = OperationType.SideEffectVanilla
                        TelemetryName = $"%s{lifeCycleKeyToTelemetryString thisLifeCycleKey} ActOnSelf"
                        Target = thisSubjectPKeyRef
                        TraceContext = traceContext
                    |}
                    |> Choice2Of2
                else
                    Choice1Of2 traceContext
            ),
            // always deduplicate actions on self (timer action & response handler compensate action)
            // cheap to maintain because caller is always the same
            (GrainRpcOperation.RunActionOnGrain (action, (* deduplicate *) true), thisSubjectRef)
            |> DispatchGrainRpc.Rpc
            |> dispatchGrainRpc

        | GrainPersistedSideEffect.TriggerTimerActionOnSelf (tentativeDueAction, maybeTraceContext) ->
            (
                let traceContext = maybeTraceContext |> Option.defaultValue emptyTraceContext
                if (thisLifeCycleAdapter :> IHostedOrReferencedLifeCycleAdapter).ShouldSendTelemetry (ShouldSendTelemetryFor.LifeAction tentativeDueAction) then
                    {|
                        OperationType = OperationType.SideEffectVanilla
                        TelemetryName = $"%s{lifeCycleKeyToTelemetryString thisLifeCycleKey} Timer"
                        Target = thisSubjectPKeyRef
                        TraceContext = traceContext
                    |}
                    |> Choice2Of2
                else
                    Choice1Of2 traceContext
            ),
            DispatchGrainRpc.TriggerTimerActionOnSelf tentativeDueAction
            |> dispatchGrainRpc

        | GrainPersistedSideEffect.HandleSubscriptionResponseOnSelf (response, _subscriptionTriggerType, _event, traceContext) ->
            (
                match response with
                | TriggerSubscriptionResponse.ActOk triggeredAction
                | TriggerSubscriptionResponse.ActNotAllowed triggeredAction
                | TriggerSubscriptionResponse.ActError (_, triggeredAction)
                | TriggerSubscriptionResponse.Exn (_, Some triggeredAction) ->
                    if (thisLifeCycleAdapter :> IHostedOrReferencedLifeCycleAdapter).ShouldSendTelemetry (ShouldSendTelemetryFor.LifeAction triggeredAction) then
                        {|
                            OperationType = OperationType.SideEffectVanilla
                            TelemetryName = $"%s{lifeCycleKeyToTelemetryString thisLifeCycleKey} TriggerSub"
                            Target = thisSubjectPKeyRef
                            TraceContext = traceContext
                        |}
                        |> Choice2Of2
                    else
                        Choice1Of2 traceContext
                | TriggerSubscriptionResponse.Exn (_, None) ->
                    Choice1Of2 traceContext
            ),
            DispatchGrainRpc.HandleSubscriptionResponseOnSelf response
            |> dispatchGrainRpc

        | GrainPersistedSideEffect.TryDeleteSelf(requiredVersion, requiredNextSideEffectSequenceNumber, retryAttempt) ->
            if (retryAttempt = 0uy) then
                Choice1Of2 emptyTraceContext
            else
                {|
                    OperationType = OperationType.SideEffectDeleteSelf
                    TelemetryName = $"%s{lifeCycleKeyToTelemetryString thisLifeCycleKey} TryDeleteSelf"
                    Target = thisSubjectPKeyRef
                    TraceContext = emptyTraceContext // TODO: pass context? who cares
                |}
                |> Choice2Of2
            , // send telemetry for TryDeleteSelf only if it's a retry
            DispatchGrainRpc.TryDeleteSelf (sideEffectId, requiredVersion, requiredNextSideEffectSequenceNumber, retryAttempt)
            |> dispatchGrainRpc

        | GrainPersistedSideEffect.Rpc rpc ->
            rpc.SubjectReference.LifeCycleKey
            |> adapters.GetLifeCycleBiosphereAdapterByKey
            |> Option.map (
                fun targetLifeCycleAdapter ->
                    match rpc.RpcOperation with
                    | GrainRpcOperation.InitializeGrain (ctor, (* okIfAlreadyInitialized *) false) ->
                        ("Construct", targetLifeCycleAdapter.ShouldSendTelemetry (ShouldSendTelemetryFor.Constructor ctor))
                    | GrainRpcOperation.InitializeGrain (ctor, (* okIfAlreadyInitialized *) true) ->
                        ("MaybeConstruct", targetLifeCycleAdapter.ShouldSendTelemetry (ShouldSendTelemetryFor.Constructor ctor))
                    | GrainRpcOperation.InitializeGrainAndSubscribe (ctor, _, subs) ->
                        ("ConstructAndSubscribe",
                         targetLifeCycleAdapter.ShouldSendTelemetry (ShouldSendTelemetryFor.Constructor ctor) ||
                            subs.Values |> Seq.exists (ShouldSendTelemetryFor.LifeEvent >> targetLifeCycleAdapter.ShouldSendTelemetry))
                    | GrainRpcOperation.SubscribeToGrain subs ->
                        ("Subscribe", subs.Values |> Seq.exists (ShouldSendTelemetryFor.LifeEvent >> targetLifeCycleAdapter.ShouldSendTelemetry))
                    | GrainRpcOperation.UnsubscribeFromGrain _ ->
                        // set to true, probably all of it will be filtered out anyway, like any successful vanilla side effect telemetry
                        ("Unsubscribe", true) // TODO: should be symmetrical to subscribe but have no LifeEvent instance around.
                    | GrainRpcOperation.RunActionMaybeConstructOnGrain (action, _ctor, _) ->
                        ("ActMaybeConstruct",
                           // check only action in ActMaybeConstruct
                           targetLifeCycleAdapter.ShouldSendTelemetry (ShouldSendTelemetryFor.LifeAction action))
                    | GrainRpcOperation.RunActionMaybeConstructAndSubscribeOnGrain (action, _ctor, _, _) ->
                        ("ActionMaybeConstructAndSubscribe",
                           // check only action in ActMaybeConstruct
                           targetLifeCycleAdapter.ShouldSendTelemetry (ShouldSendTelemetryFor.LifeAction action))
                    | GrainRpcOperation.RunActionOnGrain (action, _) ->
                        ("Act", targetLifeCycleAdapter.ShouldSendTelemetry (ShouldSendTelemetryFor.LifeAction action))
                    | GrainRpcOperation.RunActionOnGrainAndSubscribe (action, _, _) ->
                        ("ActAndSubscribe", targetLifeCycleAdapter.ShouldSendTelemetry (ShouldSendTelemetryFor.LifeAction action)))
            |> Option.filter snd
            |> Option.map (
                fun (opName, _) ->
                    {|
                        OperationType = OperationType.SideEffectVanilla
                        TelemetryName = $"{lifeCycleKeyToTelemetryString rpc.SubjectReference.LifeCycleKey} %s{opName}"
                        Target = rpc.SubjectReference.SubjectPKeyReference
                        TraceContext = rpc.TraceContext
                    |}
                    |> Choice2Of2)
            |> Option.defaultWith (fun () -> Choice1Of2 rpc.TraceContext),
            (rpc.RpcOperation, rpc.SubjectReference) |> DispatchGrainRpc.Rpc |> dispatchGrainRpc

        | GrainPersistedSideEffect.RpcTriggerSubscriptionOnGrain rpc ->
            rpc.SubjectPKeyReference.LifeCycleKey
            |> adapters.GetLifeCycleBiosphereAdapterByKey
            |> Option.map (fun lifeCycleAdapter -> lifeCycleAdapter.ShouldSendTelemetry (ShouldSendTelemetryFor.LifeEvent rpc.LifeEvent))
            |> Option.filter id
            |> Option.map (
                fun _ ->
            {|
                OperationType = OperationType.SideEffectVanilla
                TelemetryName = $"{lifeCycleKeyToTelemetryString rpc.SubjectPKeyReference.LifeCycleKey} TriggerSub"
                Target = rpc.SubjectPKeyReference
                TraceContext = rpc.TraceContext
            |}
                    |> Choice2Of2)
            |> Option.defaultWith (fun () -> Choice1Of2 rpc.TraceContext)
            |> fun telemetryChoice -> telemetryChoice, (rpc |> DispatchGrainRpc.TriggerSubscription |> dispatchGrainRpc)

        | GrainPersistedSideEffect.RpcTransactionStep step ->
            match step.RpcOperation with
            | GrainRpcTransactionStepOperation.PrepareInitializeGrain ctor ->
                step.SubjectReference.LifeCycleKey
                |> adapters.GetLifeCycleBiosphereAdapterByKey
                |> Option.map (fun lifeCycleAdapter -> lifeCycleAdapter.ShouldSendTelemetry (ShouldSendTelemetryFor.Constructor ctor))
                |> Option.filter id
                |> Option.map (fun _ -> "PrepareInitialize")
            | GrainRpcTransactionStepOperation.PrepareActionOnGrain action ->
                step.SubjectReference.LifeCycleKey
                |> adapters.GetLifeCycleBiosphereAdapterByKey
                |> Option.map (fun lifeCycleAdapter -> lifeCycleAdapter.ShouldSendTelemetry (ShouldSendTelemetryFor.LifeAction action))
                |> Option.filter id
                |> Option.map (fun _ -> "PrepareAct")
            | GrainRpcTransactionStepOperation.CommitPreparedOnGrain ->
                Some "Commit"
            | GrainRpcTransactionStepOperation.RollbackPreparedOnGrain ->
                Some "Rollback"
            | GrainRpcTransactionStepOperation.CheckPhantomPreparedOnGrain ->
                Some "CheckPhantom"
            |> Option.map (
                fun stepOpName ->
                    {|
                        OperationType = OperationType.SideEffectTransaction
                        TelemetryName = $"{lifeCycleKeyToTelemetryString step.SubjectReference.LifeCycleKey} %s{stepOpName}"
                        // report txn target in telemetry although technically reply fed back to self
                        Target = step.SubjectReference.SubjectPKeyReference
                        TraceContext = step.TraceContext
                    |}
                    |> Choice2Of2)
            |> Option.defaultWith (fun () -> Choice1Of2 step.TraceContext),
            (fun (sideEffectId: GrainSideEffectId) (retryNo: uint32) ->
                backgroundTask {
                    // in case of transient replySuccess failure there will be a retry,
                    // however target grain will remain in Prepared state which will be a Noop on retry
                    // i.e. no extra dedup is needed

                    let! result = dispatchGrainRpcTransactionStep step retryNo

                    let replyActionResult =
                        typeof<AnchorTypeForModule>.DeclaringType.GetMethod(nameof(mapTxnOperationResultToReplyAction), BindingFlags.Static ||| BindingFlags.NonPublic)
                                // 'Op type
                                .MakeGenericMethod(typeof<'LifeAction>.GenericTypeArguments.[0])
                                .Invoke(null, [| box result; box step |])
                            :?> Result<Option<LifeAction>, DispatchSideEffectNormalResult>

                    match replyActionResult with
                    | Ok (Some replyAction) ->
                        // Txn life cycle takes care of deduplication, no need to support at system level
                        let replyRpcOperation = GrainRpcOperation.RunActionOnGrain (replyAction, (* deduplicate *) false)
                        return! dispatchGrainRpc (DispatchGrainRpc.Rpc (replyRpcOperation, thisSubjectRef)) sideEffectId retryNo

                    | Ok None ->
                        return Normal DispatchSuccess

                    | Error transientFailureResult ->
                        return Normal transientFailureResult
                })

        | GrainPersistedSideEffect.ObsoleteNoop _ ->
            Choice1Of2 emptyTraceContext,
            (fun _ _ -> Task.FromResult (DispatchSideEffectResult.Normal DispatchSideEffectNormalResult.DispatchSuccess))

    let processGrainPersistedSideEffect
            (dispatchFunc: GrainSideEffectId -> (* retryNo *) uint32 -> Task<DispatchSideEffectResult>)
            (grainSideEffect: GrainPersistedSideEffect<'LifeAction, 'OpError>)
            (sideEffectId: GrainSideEffectId)
            : Task<ProcessedSideEffectResult> =

        backgroundTask {
            // Persisted side effects have at-least-once guarantee
            // Attempt to process side-effect, continuously retrying on any transient failures (i.e. exceptions)

            // Normally we'd use a tail-recursive pattern to do retries, but the task builder explicitly doesn't support them
            // so we're forced to use a mutable + while loop here, as recommended
            let mutable sideEffectRunStatus = NotDispatched
            let mutable retryNo = 0u

            while (match sideEffectRunStatus with | Processed _ -> false | _ -> true) do
                try
                    match isHostGrainDeactivated(), sideEffectRunStatus with
                    | (* host grain deactivated *) false, NotDispatched ->

                        // TODO: when retryNo > 0, optimize to avoid empty attempts after a false-negative transient failure.
                        // e.g. it's possible that updateGrainSideEffect threw timeout but in fact succeeded, can query the side effects storage and if
                        // side effect is gone then consider it Dispatched Success.
                        // Makes sense to do such check only if exception was thrown by updateGrainSideEffect or handleSideEffectResponses/EnqueueActions

                        match! dispatchFunc sideEffectId retryNo with
                        | Normal DispatchSuccess ->
                            if transientFailureHook
                               |> Option.map (fun hook ->
                                   match grainSideEffect with
                                   | GrainPersistedSideEffect.TryDeleteSelf _ ->
                                       false // never imitate retry of TryDeleteSelf, because on second attempt grain will be already deactivated and it will result in FatalTransient
                                   | _ -> hook.ShouldInjectOddRetry retryNo)
                               |> Option.defaultValue false then
                                retryNo <- retryNo + 1u
                                sideEffectRunStatus <- NotDispatched
                            else
                                sideEffectRunStatus <- Dispatched GrainSideEffectResult.Success

                        | Normal (DispatchTransientFailure(err, maxDelayBeforeRetry)) ->
                            // similar to transient exception handling but with custom max delay
                            // Exponential back-off, up to 1 minute
                            let waitFor = waitForBeforeRetry maxDelayBeforeRetry retryNo

                            logger.Warn
                                "Transient failure dispatching side-effect %a with ID %a on Subject %a after %a retries, will retry in %a seconds. Message: %a"
                                    (logger.P "side effect") grainSideEffect
                                    (logger.P "side effect ID") sideEffectId
                                    (logger.P "subject ID") thisSubjectId
                                    (logger.P "retries") retryNo
                                    (logger.P "retry again in") (int waitFor.TotalSeconds)
                                    (logger.P "message") err

                            do! Task.Delay waitFor
                            sideEffectRunStatus <- NotDispatched
                            retryNo <- retryNo + 1u

                        | Normal (DispatchPermanentFailure (err, severity)) ->
                            let logIt =
                                match severity with
                                | SideEffectFailureSeverity.Error   -> logger.Error
                                | SideEffectFailureSeverity.Warning -> logger.Warn
                            logIt
                                "Permanent failure dispatching side-effect %a with ID %a on Subject %a: %a"
                                    (logger.P "side effect") grainSideEffect
                                    (logger.P "side effect ID") sideEffectId
                                    (logger.P "subject ID") thisSubjectId
                                    (logger.P "err") err

                            let grainSideEffectResult = GrainSideEffectResult.PermanentFailure (err, severity)
                            sideEffectRunStatus <- Dispatched grainSideEffectResult

                            do! updateGrainSideEffect sideEffectId grainSideEffectResult
                            sideEffectRunStatus <- ProcessedNormal (retryNo, (* success *) false) |> Processed

                        | FatalTransient err ->
                            logger.Error
                                "Fatal failure dispatching side-effect %a with ID %a on Subject %a: %a. Side effect processor will shut down asap."
                                (logger.P "side effect") grainSideEffect
                                (logger.P "side effect ID") sideEffectId
                                (logger.P "subject ID") thisSubjectId
                                (logger.P "err") err
                            // do not invoke updateGrainSideEffect because it's a transient error (although a fatal one)
                            // it will be retried by new healthy instance of SE processor
                            sideEffectRunStatus <- Processed (ProcessedFatalTransient retryNo)

                    | (* host grain deactivated *) false, Dispatched grainSideEffectResult ->
                        do! updateGrainSideEffect sideEffectId grainSideEffectResult
                        let success = match grainSideEffectResult with | GrainSideEffectResult.Success _ -> true | GrainSideEffectResult.PermanentFailure _ -> false
                        sideEffectRunStatus <- ProcessedNormal (retryNo, success) |> Processed

                    | (* host grain deactivated *) true, (NotDispatched | Dispatched _) ->
                        logger.Warn "Host grain is deactivated, skip processing the side-effect %a with ID %a on Subject %a"
                            (logger.P "side effect") grainSideEffect
                            (logger.P "side effect ID") sideEffectId
                            (logger.P "subject ID") thisSubjectId

                        sideEffectRunStatus <- Processed (ProcessedFatalTransient retryNo)

                    | (* host grain deactivated *) true, Processed _
                    | _, Processed _ -> // Unexpected
                        raise <| InvalidOperationException(sprintf "Unexpected retry loop after side effect marked processed: %O" sideEffectRunStatus)
                with
                | :? PermanentSubjectException as ex ->
                    let resSideEffect = GrainSideEffectResult.PermanentFailure (ex.ToString(), SideEffectFailureSeverity.Error)
                    sideEffectRunStatus <- Dispatched resSideEffect

                | ex ->
                    // Exponential back-off, up to 1 minute
                    let waitFor = waitForBeforeRetry (TimeSpan.FromSeconds 60.) retryNo

                    match sideEffectRunStatus with
                    | Dispatched result ->
                        // was already dispatched, no need to retry
                        logger.WarnExn ex
                            "Transient failure updating status of dispatched side-effect %a with ID %a and dispatch result %a on Subject %a after %a retries, will retry in %a seconds"
                                (logger.P "side effect") grainSideEffect
                                (logger.P "side effect ID") sideEffectId
                                (logger.P "dispatch status") result
                                (logger.P "subject ID") thisSubjectId
                                (logger.P "retries") retryNo
                                (logger.P "retry again in") (int waitFor.TotalSeconds)

                        do! Task.Delay waitFor
                        // no need to change dispatched status

                    | NotDispatched ->
                        logger.WarnExn ex
                            "Transient failure dispatching side-effect %a with ID %a on Subject %a after %a retries, will retry in %a seconds"
                                (logger.P "side effect") grainSideEffect
                                (logger.P "side effect ID") sideEffectId
                                (logger.P "subject ID") thisSubjectId
                                (logger.P "retries") retryNo
                                (logger.P "retry again in") (int waitFor.TotalSeconds)

                        do! Task.Delay waitFor
                        sideEffectRunStatus <- NotDispatched
                        retryNo <- retryNo + 1u

                    | Processed _ ->
                        raise <| InvalidOperationException(sprintf "Unexpected exception after side effect marked processed: %O" sideEffectRunStatus, ex)

            match sideEffectRunStatus with
            | Processed processedResult ->
                return processedResult
            | _ ->
                // shouldn't happen
                return ProcessedNormal (retryNo, false)
        }

    let getTransientSideEffectTelemetryDataAndDispatchFunc
            (sideEffectId: GrainSideEffectId)
            (grainSideEffect: GrainTransientSideEffect<'LifeAction>)
            : SideEffectTracing * (GrainSideEffectId -> Task) =

        match grainSideEffect with
        | GrainTransientSideEffect.ConnectorRequest(traceContext, responseType, connectorName, buildRequest, buildAction) ->
            connectorAdapterCollection.GetConnectorAdapterByName connectorName
            |> Option.map (fun connectorAdapter -> connectorAdapter.ShouldSendTelemetry)
            |> Option.filter id
            |> Option.map (fun _ -> SendTelemetry (traceContext, OperationType.SideEffectVanilla, $"Connector %s{connectorName}", Map.ofOneItem ("ConnectorName", connectorName)))
            |> Option.defaultWith (fun () -> DoNotSendTelemetry traceContext),
            (fun _ ->
                backgroundTask {
                    match connectorAdapterCollection.GetConnectorAdapterByName connectorName with
                    | Some connector ->
                        let untypedBuildAction = buildAction >> (fun action -> action :> LifeAction)
                        try
                            connector.RequestOnGrain hostEcosystemGrainFactory responseType buildRequest untypedBuildAction grainPartition thisSubjectPKeyRef sideEffectId
                        with
                        | ex ->
                            logger.WarnExn ex "Connector %a threw exception"
                                (logger.P "connector") connectorName
                    | None ->
                        logger.Error "Connector %a not found"
                            (logger.P "connector") connectorName
                } |> Task.Ignore)
        | GrainTransientSideEffect.ConnectorRequestMultiResponse(traceContext, responseType, connectorName, buildRequest, buildAction) ->
            connectorAdapterCollection.GetConnectorAdapterByName connectorName
            |> Option.map (fun connectorAdapter -> connectorAdapter.ShouldSendTelemetry)
            |> Option.filter id
            |> Option.map (fun _ -> SendTelemetry (traceContext, OperationType.SideEffectVanilla, $"Connector %s{connectorName}", Map.ofOneItem ("ConnectorName", connectorName)))
            |> Option.defaultWith (fun () -> DoNotSendTelemetry traceContext),
            (fun _ ->
                backgroundTask {
                    match connectorAdapterCollection.GetConnectorAdapterByName connectorName with
                    | Some connector ->
                        let untypedBuildAction = buildAction >> (fun action -> action :> LifeAction)
                        try
                            connector.RequestMultiResponseOnGrain hostEcosystemGrainFactory responseType buildRequest untypedBuildAction grainPartition thisSubjectPKeyRef sideEffectId
                        with
                        | ex ->
                            logger.WarnExn ex "Connector %a threw exception"
                                (logger.P "connector") connectorName
                    | None ->
                        logger.Error "Connector %a not found"
                            (logger.P "connector") connectorName
                } |> Task.Ignore)

    let processGrainTransientSideEffect
            (dispatchFunc: GrainSideEffectId -> Task)
            (grainSideEffect: GrainTransientSideEffect<'LifeAction>)
            (sideEffectId: GrainSideEffectId)
            : Task<ProcessedSideEffectResult> =
        backgroundTask {
            try
                // Transient side effects have at-most-once guarantee i.e. no retries
                // Review this assumption if we there's more transient side effects than just Connectors
                do! dispatchFunc sideEffectId
                return ProcessedNormal (0u, true)
            with
            | ex ->
                logger.WarnExn ex "Failure dispatching transient side-effect %a with ID %a on Subject %a"
                    (logger.P "grain side effect") grainSideEffect
                    (logger.P "side effect id") sideEffectId
                    (logger.P "this subject id") thisSubjectId

                return ProcessedNormal (0u, (* success *) false)
        }

    let sideEffectGroupPriority =
        // Ordering is important; for example, we want to make sure our subscriptions are complete before we run
        // any external action that might trigger an event to be caught by that subscription
        function
        | GrainSideEffect.Persisted persistedSideEffect ->
            match persistedSideEffect with
            | GrainPersistedSideEffect.ObsoleteNoop _  ->             (0, 0us)
            | GrainPersistedSideEffect.IngestTimeSeries _  ->         (10, 0us)
            | GrainPersistedSideEffect.Rpc { RpcOperation = rpc } ->
                match rpc with
                | GrainRpcOperation.InitializeGrain _                            -> (20, 0us)
                | GrainRpcOperation.InitializeGrainAndSubscribe _                -> (20, 0us)
                | GrainRpcOperation.RunActionMaybeConstructAndSubscribeOnGrain _ -> (25, 0us)
                | GrainRpcOperation.RunActionOnGrainAndSubscribe _               -> (27, 0us)
                | GrainRpcOperation.SubscribeToGrain _                           -> (30, 0us)
                | GrainRpcOperation.UnsubscribeFromGrain _                       -> (40, 0us)
                | GrainRpcOperation.RunActionMaybeConstructOnGrain _             -> (50, 0us)
                | GrainRpcOperation.RunActionOnGrain _                           -> (60, 0us)
            | GrainPersistedSideEffect.RpcTransactionStep { RpcOperation = rpc; BatchNo = batchNo } ->
                // group by batchNo to allow individual prepare / commit batches run in parallel
                // it's important to commit transaction actions in the order of prepare so clients have more control
                match rpc with
                | GrainRpcTransactionStepOperation.PrepareInitializeGrain _    -> (20, batchNo)
                | GrainRpcTransactionStepOperation.PrepareActionOnGrain _      -> (60, batchNo)
                | GrainRpcTransactionStepOperation.CommitPreparedOnGrain       -> (60, batchNo)
                | GrainRpcTransactionStepOperation.RollbackPreparedOnGrain     -> (60, batchNo)
                | GrainRpcTransactionStepOperation.CheckPhantomPreparedOnGrain -> (60, batchNo)
            | GrainPersistedSideEffect.RpcTriggerSubscriptionOnGrain {SubscriptionTriggerType = SubscriptionTriggerType.Named _ }
            | GrainPersistedSideEffect.RunActionOnSelf _  ->                    (70, 0us)
            | GrainPersistedSideEffect.TriggerTimerActionOnSelf _  ->           (70, 0us)
            | GrainPersistedSideEffect.HandleSubscriptionResponseOnSelf _  ->   (70, 0us)
            | GrainPersistedSideEffect.TryDeleteSelf _                     ->   (Int32.MaxValue, 0us)
        | GrainSideEffect.Transient transientSideEffect   ->
            match transientSideEffect with
            | GrainTransientSideEffect.ConnectorRequest _
            | GrainTransientSideEffect.ConnectorRequestMultiResponse _ -> (70, 0us)

    let persistedSideEffectTarget =
        function
        | GrainPersistedSideEffect.RunActionOnSelf _
        | GrainPersistedSideEffect.TriggerTimerActionOnSelf _
        | GrainPersistedSideEffect.HandleSubscriptionResponseOnSelf _
        | GrainPersistedSideEffect.TryDeleteSelf _ ->
            Choice1Of3 thisSubjectPKeyRef
        | GrainPersistedSideEffect.RpcTriggerSubscriptionOnGrain { SubjectPKeyReference = subscriber } ->
            Choice1Of3 subscriber
        | GrainPersistedSideEffect.Rpc { SubjectReference = target }
        | GrainPersistedSideEffect.RpcTransactionStep { SubjectReference = target } ->
            Choice1Of3 target.SubjectPKeyReference
        | GrainPersistedSideEffect.IngestTimeSeries (timeSeriesKey, _, _) ->
            Choice2Of3 timeSeriesKey
        | GrainPersistedSideEffect.ObsoleteNoop _ ->
            Choice3Of3 () // no-ops have same dummy target (run sequentially but it's OK because rare)

    let processPriorityGroupsStopOnFatalError (processPriorityGroupsFactories: seq<unit -> Task<ProcessedSideEffectResult[]>>) : Task<bool> =
        let mutable continueOrShutdown = true
        let mutable i = 0
        let factories = processPriorityGroupsFactories |> Seq.toArray

        backgroundTask {
            while continueOrShutdown && i < factories.Length do
                let! results = factories.[i] ()
                // shutdown if fatal error
                continueOrShutdown <- results |> Seq.forall (function ProcessedNormal _ -> true | ProcessedFatalTransient _ -> false)
                i <- i + 1
            return continueOrShutdown
        }

    let processGrainSideEffectGroup (sideEffectGroup: SideEffectGroup<'LifeAction, 'OpError>) : Task<bool> =

        sideEffectGroup.SideEffects.ToMap
        |> Map.toList

        // group side effects into groups by priority, side effects within one group run in parallel, groups run one after another
        |> List.map (
            fun (id, sideEffect) ->
                let priority = sideEffectGroupPriority sideEffect
                priority, id, sideEffect
            )
        |> List.groupBy (fun (priority, _, _) -> priority)
        |> List.sortBy fst
        |> List.map (snd >> List.map (fun (_, sideEffectId, sideEffect) -> (sideEffectId, sideEffect)))

        // 2+ side effects within one group targeting same subject will break built-in deduplication.
        // To avoid it make sure each subgroup has unique targets.
        // Example: group with non-unique targets [SE1: subj1; SE2: subj2; SE3: subj1; SE4: subj2; SE5: subj3]
        //  will split in two groups: [SE1: subj1; SE2: subj2; SE5: subj3] and [SE3: subj1; SE4: subj2]
        |> List.collect (fun sideEffectsGroup ->

            sideEffectsGroup
            |> List.groupBy (fun (sideEffectId, sideEffect) ->
                match sideEffect with
                | GrainSideEffect.Persisted persistedSideEffect ->
                    Choice1Of2 (persistedSideEffectTarget persistedSideEffect)
                | GrainSideEffect.Transient _ ->
                    // consider all transient SE targets unique
                    Choice2Of2 sideEffectId)
            |> List.collect (fun (_, sideEffectsWithSameTarget) ->
                sideEffectsWithSameTarget
                // need repeatable sort for reliable deduplication, let it be SE guid
                |> List.sortBy (fun (sideEffectId: GrainSideEffectId, _) -> sideEffectId)
                |> List.mapi (fun i data -> i, data))

            |> List.groupBy (fun (order: int, _) -> order)
            |> List.sortBy fst
            |> List.map (snd >> List.map snd))

        |> List.map (fun sideEffectsGroup ->
            fun () ->
                sideEffectsGroup
                |> Seq.map (fun (sideEffectId, sideEffect) ->
                    match sideEffect with

                    | GrainSideEffect.Persisted persistedSideEffect ->
                        let telemetryChoice, dispatchFunc =
                            getPersistedSideEffectTelemetryDataAndDispatchFunc sideEffectId persistedSideEffect
                        let tracing =
                            match telemetryChoice with
                            | Choice1Of2 traceContext -> DoNotSendTelemetry traceContext
                            | Choice2Of2 telemetryData ->
                                let customProperties =
                                    [
                                        ("TargetSubjectId", telemetryData.Target.SubjectIdStr)
                                    ]
                                    |> Map.ofSeq
                                SendTelemetry (telemetryData.TraceContext, telemetryData.OperationType, telemetryData.TelemetryName, customProperties)
                        tracing, sideEffectId, processGrainPersistedSideEffect dispatchFunc persistedSideEffect

                    | GrainSideEffect.Transient transientSideEffect ->
                        let tracing, dispatchFunc =
                            getTransientSideEffectTelemetryDataAndDispatchFunc sideEffectId transientSideEffect
                        tracing, sideEffectId, processGrainTransientSideEffect dispatchFunc transientSideEffect)
                |> Seq.map (
                    fun (tracing, sideEffectId, createSideEffectTask) ->

                        match tracing with
                        | DoNotSendTelemetry traceContext
                        | SendTelemetry (traceContext, _, _, _) ->
                            traceContext
                        |> fun traceContext ->
                            // override telemetry user & session
                            OrleansRequestContext.setTelemetryUserIdAndSessionId (traceContext.TelemetryUserId, traceContext.TelemetrySessionId)

                        match tracing with
                        | DoNotSendTelemetry traceContext ->
                            // leave ParentActivityIdKey for untracked call so the inter-grain "Grain" dependency appears below last known parent
                            RequestContext.Set (ParentActivityIdKey, traceContext.ParentId)
                            createSideEffectTask sideEffectId

                        | SendTelemetry (traceContext, sideEffectOperationType, sideEffectTelemetryName, customProperties) ->
                            let beforeRunProperties =
                                seq {
                                    yield!
                                        logger.Scope
                                        |> Seq.filter (fun kvp -> kvp.Key <> "Partition") // partition always the same, don't waste space
                                        |> Seq.map (fun kvp -> kvp.Key, kvp.Value)
                                    "SideEffectId", valueSummarizers.FormatValue sideEffectId
                                    "SeqNum", $"%d{sideEffectGroup.SequenceNumber}"
                                    if sideEffectGroup.RehydratedFromStorage then
                                        "Rehydrated", "true"
                                }
                                |> Map.ofSeq
                                |> Map.merge customProperties

                            let makeItNewParentActivityId =
                                match sideEffectOperationType with
                                | OperationType.SideEffectVanilla ->
                                    // Note: to maintain proper parent-child hierarchy in Telemetry, keep it in sync with DependencyTelemetryProcessor
                                    // Unfortunately App Insights / Activity design makes it hard to filter out already created telemetry while keeping proper parent-child relations
                                    // Here we mark Vanilla side effects as siblings of their underlying grain calls, so they can be safely eliminated without affecting hierarchy (most of them are eliminated)
                                    false
                                | OperationType.SideEffectTransaction
                                | OperationType.SideEffectDeleteSelf
                                | OperationType.GrainCallVanilla
                                | OperationType.GrainCallTriggerTimer
                                | OperationType.GrainCallTriggerSubscription
                                | OperationType.GrainCallConnector
                                | OperationType.TestSimulation
                                | OperationType.TestTimeForward ->
                                    // create new parent Id at this tracked level of nesting so the inter-grain "Grain" dependency appears below this "Side Effect"
                                    true

                            // ParentId can be absent e.g. if timer tick has empty context, or it runs in test code, or restored by legacy codec
                            let maybeParentActivityId =
                                if String.IsNullOrEmpty traceContext.ParentId then None else  Some traceContext.ParentId

                            operationTracker.TrackOperation<_>
                                { Partition = grainPartition
                                  Type = sideEffectOperationType
                                  Name = sideEffectTelemetryName
                                  MaybeParentActivityId = maybeParentActivityId
                                  MakeItNewParentActivityId = makeItNewParentActivityId
                                  BeforeRunProperties = beforeRunProperties }
                                (fun () ->
                                    task {
                                        let! result = createSideEffectTask sideEffectId
                                        let isSuccess, retryNo, isFatalTransient =
                                            match result with
                                            | ProcessedNormal (retryNo, success) ->
                                                success, retryNo, false
                                            | ProcessedFatalTransient retryNo ->
                                                false, retryNo, true

                                        let afterRunProperties =
                                            seq {
                                                if isFatalTransient then "HadFatalTransientError", "true"
                                                if retryNo > 0u then "MultipleAttempts", $"%d{retryNo + 1u}"
                                            }
                                            |> Map.ofSeq

                                        return { ReturnValue =  result; IsSuccess = Some isSuccess; AfterRunProperties = afterRunProperties }
                                    })
                )
                |> Task.WhenAll
        )
        |> processPriorityGroupsStopOnFatalError


    let messageProcessor (message: SideEffectProcessorMessage<'LifeAction, 'OpError>) : Async<StatelessNextStep> =
        backgroundTask {
            try
                match message with
                | SideEffectProcessorMessage.ShutdownSideEffectProcessor replyChannel ->
                    replyChannel.Reply ()
                    return StatelessNextStep.Shutdown

                | SideEffectProcessorMessage.NoOpAndContinue replyChannel ->
                    replyChannel.Reply ()
                    return StatelessNextStep.Continue

                | SideEffectProcessorMessage.ProcessSideEffectGroup sideEffectGroup ->
                    let! continueOrShutdown = processGrainSideEffectGroup sideEffectGroup
                    let nextStep = if continueOrShutdown then StatelessNextStep.Continue else StatelessNextStep.Shutdown
                    return nextStep
            with
            | ex ->
                // Side effect processor only handles exceptions from very processing of each side effect, there are few more outer layers of unprotected code
                // that in theory can throw but we don't expect them to do it. If it happens we need to find and fix the root cause.
                logger.ErrorExn ex "Uncaught exception in side effect processor - it's probably unusable now! If you read this message then find and fix the root cause."
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw()
                return shouldNotReachHereBecause "line above throws"
        }
        |> Async.AwaitTask

    let errorHandler _ _ : Async<StatelessNextStep> =
        // Errors are handled and retried in the message processor
        // so this code should never get hit
        async {
            return invalidOp "Unreachable code"
        }

    makeSafeStatelessMailboxProcessor messageProcessor errorHandler
