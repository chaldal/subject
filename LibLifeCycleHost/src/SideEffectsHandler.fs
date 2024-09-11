module internal LibLifeCycleHost.SideEffectsHandler

open System
open LibLifeCycle
open System.Threading.Tasks

let private performExternalActionOnGrain<'LifeAction, 'Constructor, 'SubjectId, 'SourceLifeAction, 'SourceOpError
                                          when 'LifeAction           :> LifeAction
                                          and  'SourceLifeAction     :> LifeAction
                                          and  'Constructor          :> Constructor
                                          and  'SubjectId            :> SubjectId
                                          and  'SubjectId            : comparison
                                          and  'SourceOpError        :> OpError>
        (adapters: HostedOrReferencedLifeCycleAdapterRegistry)
        (grainScopedServiceProvider: IServiceProvider)
        (externalAction: ExternalLifeCycleOperation<'LifeAction, 'Constructor, 'SubjectId>)
        (traceContext: TraceContext)
        : Task<GrainPersistedSideEffect<'SourceLifeAction, 'SourceOpError>> =

    let initializeGrainRpc ctor okIfAlreadyInitialized =
        backgroundTask {
            match adapters.GetLifeCycleBiosphereAdapterByKey externalAction.LifeCycleKey with
            | Some targetAdapter ->
                match! targetAdapter.GenerateId grainScopedServiceProvider CallOrigin.Internal ctor with
                | Ok subjectId ->
                    return
                        {
                            SubjectReference = {
                                LifeCycleKey = externalAction.LifeCycleKey
                                SubjectId    = subjectId
                            }
                            RpcOperation  = GrainRpcOperation.InitializeGrain (ctor, okIfAlreadyInitialized)
                            TraceContext  = traceContext
                        } |> GrainPersistedSideEffect.Rpc

                | Error _ ->
                    // FIXME.  Also treat IdGen exceptions as permanent errors
                    return failwith "FIXME"
            | _ ->
                // FIXME
                return failwith "FIXME"
        }


    backgroundTask {
        let subjectReference id : SubjectReference = {
            LifeCycleKey = externalAction.LifeCycleKey
            SubjectId    = id
        }

        match externalAction.Op with
        | LifeCycleOp.Act(id, action, options) ->
            return
                {
                    SubjectReference = subjectReference id
                    RpcOperation  = GrainRpcOperation.RunActionOnGrain (action, options.Deduplicate)
                    TraceContext = if options.ResetTraceContext then emptyTraceContext else traceContext
                } |> GrainPersistedSideEffect.Rpc

        | LifeCycleOp.Construct ctor ->
            return! initializeGrainRpc ctor (* okIfAlreadyInitialized *) false

        | LifeCycleOp.MaybeConstruct ctor ->
            return! initializeGrainRpc ctor (* okIfAlreadyInitialized *) true

        | LifeCycleOp.ActMaybeConstruct (id, action, ctor, options) ->
            return
                {
                    SubjectReference = subjectReference id
                    RpcOperation  = GrainRpcOperation.RunActionMaybeConstructOnGrain (action, ctor, options.Deduplicate)
                    TraceContext  = if options.ResetTraceContext then emptyTraceContext else traceContext
                } |> GrainPersistedSideEffect.Rpc
    }

let private performExternalSubjectTransactionOperationOnGrain<'LifeAction, 'Constructor, 'SubjectId, 'SourceLifeAction, 'SourceOpError
                                          when 'LifeAction           :> LifeAction
                                          and  'SourceLifeAction     :> LifeAction
                                          and  'Constructor          :> Constructor
                                          and  'SubjectId            :> SubjectId
                                          and  'SubjectId            : comparison
                                          and  'SourceOpError        :> OpError>
        (externalAction: ExternalLifeCycleTxnOperation<'LifeAction, 'Constructor, 'SubjectId>)
        (batchNo: uint16)
        (opNo: uint16)
        (traceContext: TraceContext)
        : GrainPersistedSideEffect<'SourceLifeAction, 'SourceOpError> =

    let transactionStep (id, transactionId, operation) =
        let rpc =
            {
                SubjectReference = {
                    LifeCycleKey = externalAction.LifeCycleKey
                    SubjectId     = id
                }
                TransactionId = transactionId
                BatchNo       = batchNo
                OpNo          = opNo
                RpcOperation  = operation
                TraceContext  = traceContext
            }
        GrainPersistedSideEffect.RpcTransactionStep rpc

    match externalAction.Op with
    | LifeCycleTxnOp.PrepareAct (id, action, transactionId) ->
        (id, transactionId, GrainRpcTransactionStepOperation.PrepareActionOnGrain action)
        |> transactionStep

    | LifeCycleTxnOp.PrepareInitialize (id, ctor, transactionId) ->
        (id, transactionId, GrainRpcTransactionStepOperation.PrepareInitializeGrain ctor)
        |> transactionStep

    | LifeCycleTxnOp.Commit (id, transactionId) ->
        (id, transactionId, GrainRpcTransactionStepOperation.CommitPreparedOnGrain)
        |> transactionStep

    | LifeCycleTxnOp.Rollback (id, transactionId) ->
        (id, transactionId, GrainRpcTransactionStepOperation.RollbackPreparedOnGrain)
        |> transactionStep

    | LifeCycleTxnOp.CheckPhantom (id, transactionId) ->
        (id, transactionId, GrainRpcTransactionStepOperation.CheckPhantomPreparedOnGrain)
        |> transactionStep

let private processExternalActionsAndSelfConstructorsOnGrains
        (adapters: HostedOrReferencedLifeCycleAdapterRegistry)
        (grainScopedServiceProvider: IServiceProvider)
        (sourceLifeCycle: ILifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>)
        (sourceSubjectId: 'SubjectId)
        (selfConstructors: List<'Constructor>)
        (externalActions: List<ExternalOperation<'LifeAction>>)
        (selfActions: List<'LifeAction>)
        (traceContext: TraceContext)
        : Task<List<GrainSideEffect<'LifeAction, 'OpError>>> =
    sourceLifeCycle.Invoke
        { new FullyTypedLifeCycleFunction<_, _, _, _, _, _, _> with
            member _.Invoke lifeCycle =
                externalActions
                |> Seq.append(
                        selfConstructors
                        |> Seq.map (lifeCycle.Definition.Construct >> ExternalOperation.ExternalLifeCycleOperation)
                    )
                |> Seq.append(
                        selfActions
                        |> Seq.map ((lifeCycle.Definition.Act sourceSubjectId) >> ExternalOperation.ExternalLifeCycleOperation)
                    )
                |> Seq.map(
                    fun externalAction ->
                        fun () ->
                            backgroundTask {
                                match externalAction with
                                | ExternalOperation.ExternalLifeCycleOperation externalLifeCycleOperation ->
                                    // Non-Generic to Generic method jump
                                    let! persistedSideEffect =
                                        externalLifeCycleOperation.Invoke
                                            { new FullyTypedExternalLifeCycleOperationFunction<_> with
                                                member _.Invoke op = performExternalActionOnGrain adapters grainScopedServiceProvider op traceContext }
                                    return persistedSideEffect |> GrainSideEffect.Persisted

                                | ExternalOperation.ExternalSubjectTransactionOperation (externalLifeCycleOperation, batchNo, opNo) ->
                                    // Non-Generic to Generic method jump
                                    return
                                        externalLifeCycleOperation.Invoke
                                            { new FullyTypedExternalLifeCycleTxnOperationFunction<_> with
                                                member _.Invoke op =
                                                    performExternalSubjectTransactionOperationOnGrain op batchNo opNo traceContext
                                                    |> GrainSideEffect.Persisted }

                                | ExternalOperation.ExternalTimeSeriesOperation ingestTimeSeriesDataPointsOp ->
                                    return
                                        ingestTimeSeriesDataPointsOp.Invoke
                                            { new FullyTypedIngestTimeSeriesDataPointsOperationFunction<_> with
                                                member _.Invoke op =
                                                    GrainPersistedSideEffect.IngestTimeSeries (op.TimeSeriesKey, box op.Points, traceContext)
                                                    |> GrainSideEffect.Persisted }

                                | ExternalOperation.ExternalConnectorOperation (connectorName, buildRequest, buildResponse, responseType) ->
                                    return
                                        GrainTransientSideEffect.ConnectorRequest(traceContext, responseType, connectorName, buildRequest, buildResponse)
                                        |> GrainSideEffect.Transient

                                | ExternalOperation.ExternalConnectorMultiResponseOperation (connectorName, buildRequest, buildResponse, responseType) ->
                                    return
                                        GrainTransientSideEffect.ConnectorRequestMultiResponse(traceContext, responseType, connectorName, buildRequest, buildResponse)
                                        |> GrainSideEffect.Transient
                            }
                    )
                |> Task.batched 1 }

let processWorkflowSideEffectsOnGrains
        (adapters: HostedOrReferencedLifeCycleAdapterRegistry)
        (grainScopedServiceProvider: IServiceProvider)
        (sourceLifeCycle: ILifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>)
        (sourceSubjectId: 'SubjectId)
        (sideEffects: TransitionSideEffects<'Constructor, 'LifeEvent, 'LifeAction>)
        (traceContext: TraceContext)
        : Task<WorkflowSideEffectProcessingResult<'LifeAction, 'LifeEvent, 'OpError>> =
        backgroundTask {
            let! grainActivities = processExternalActionsAndSelfConstructorsOnGrains adapters grainScopedServiceProvider sourceLifeCycle sourceSubjectId sideEffects.Constructors sideEffects.ExternalActions sideEffects.LifeActions traceContext
            return
                {
                    RaisedLifeEvents = sideEffects.LifeEvents
                    GrainSideEffects = grainActivities
                }
        }