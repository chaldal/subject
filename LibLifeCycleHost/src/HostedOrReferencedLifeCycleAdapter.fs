[<AutoOpen>]
module LibLifeCycleHost.HostedOrReferencedLifeCycleAdapter

open System
open LibLifeCycle
open LibLifeCycleCore
open Microsoft.Extensions.DependencyInjection
open System.Threading.Tasks

// Helps to use a life cycle from a non-generic context when only LifeCycleDef is available, potentially in external ecosystem
type IHostedOrReferencedLifeCycleAdapter =
    abstract member ReferencedLifeCycle:            IReferencedLifeCycle
    abstract member IsSessionLifeCycle:             bool
    abstract member RunActionOnGrain:               grainProvider: IBiosphereGrainProvider -> grainPartition: GrainPartition -> maybeDedupInfo: Option<SideEffectDedupInfo> -> subjectIdStr: string -> action: LifeAction -> maybeBeforeActSubscriptions: Option<BeforeActSubscriptions> -> Task<Result<unit, SubjectFailure<GrainTransitionError<OpError>>>>
    abstract member TriggerTimerActionOnGrain:      grainProvider: IBiosphereGrainProvider -> grainPartition: GrainPartition -> dedupInfo: SideEffectDedupInfo -> subjectId: SubjectId -> tentativeDueAction: LifeAction -> Task<Result<Option<LifeAction>, GrainTriggerTimerError<OpError, LifeAction>>>
    abstract member TryDeleteSelfOnGrain:           grainProvider: IBiosphereGrainProvider -> grainPartition: GrainPartition -> subjectId: SubjectId -> sideEffectId: GrainSideEffectId -> requiredVersion: uint64 -> requiredNextSideEffectSequenceNumber: uint64 -> retryAttempt: byte -> Task
    abstract member RunPrepareActionOnGrain:        grainProvider: IBiosphereGrainProvider -> grainPartition: GrainPartition -> subjectId: SubjectId -> action: LifeAction -> SubjectTransactionId -> Task<Result<unit, SubjectFailure<GrainPrepareTransitionError<OpError>>>>
    abstract member RunCommitPreparedOnGrain:       grainProvider: IBiosphereGrainProvider -> grainPartition: GrainPartition -> subjectId: SubjectId -> SubjectTransactionId -> Task
    abstract member RunRollbackPreparedOnGrain:     grainProvider: IBiosphereGrainProvider -> grainPartition: GrainPartition -> subjectId: SubjectId -> SubjectTransactionId -> Task
    abstract member RunCheckPhantomPreparedOnGrain: grainProvider: IBiosphereGrainProvider -> grainPartition: GrainPartition -> subjectId: SubjectId -> SubjectTransactionId -> Task
    abstract member RunActionMaybeConstructOnGrain: grainProvider: IBiosphereGrainProvider -> grainPartition: GrainPartition -> maybeDedupInfo: Option<SideEffectDedupInfo> -> subjectId: SubjectId -> action: LifeAction -> ctor: Constructor -> maybeConstructSubscriptions: Option<ConstructSubscriptions> -> Task<Result<unit, SubjectFailure<GrainOperationError<OpError>>>>
    abstract member InitializeGrain:                grainProvider: IBiosphereGrainProvider -> grainPartition: GrainPartition -> okIfAlreadyInitialized: bool -> maybeDedupInfo: Option<SideEffectDedupInfo> -> subjectId: SubjectId -> constructor: Constructor -> maybeConstructSubscriptions: Option<ConstructSubscriptions> -> Task<Result<unit, SubjectFailure<GrainConstructionError<OpError>>>>
    abstract member PrepareInitializeGrain:         grainProvider: IBiosphereGrainProvider -> grainPartition: GrainPartition -> subjectId: SubjectId -> constructor: Constructor -> SubjectTransactionId -> Task<Result<unit, SubjectFailure<GrainPrepareConstructionError<OpError>>>>
    abstract member SubscribeToGrain:               grainProvider: IBiosphereGrainProvider -> grainPartition: GrainPartition -> subjectId: SubjectId -> subscriptions: Map<SubscriptionName, LifeEvent> -> subscriberRef: SubjectPKeyReference -> Task<Result<unit, SubjectFailure<GrainSubscriptionError>>>
    abstract member UnsubscribeFromGrain:           grainProvider: IBiosphereGrainProvider -> grainPartition: GrainPartition -> subjectId: SubjectId -> subscriptions: Set<SubscriptionName> -> subscriberRef: SubjectPKeyReference -> Task
    abstract member TriggerSubscriptionOnGrain:     grainProvider: IBiosphereGrainProvider -> grainPartition: GrainPartition -> maybeDedupInfo: Option<SideEffectDedupInfo> -> subjectIdStr: string -> subscriptionTriggerType: SubscriptionTriggerType -> triggeredEvent: LifeEvent -> Task<Result<unit, SubjectFailure<GrainTriggerSubscriptionError<OpError>>>>
    abstract member GenerateId:                     grainScopedServiceProvider: IServiceProvider -> callOrigin: CallOrigin -> constructor: Constructor -> Task<Result<SubjectId, OpError>>
    abstract member ShouldSendTelemetry:            telemetryFor: ShouldSendTelemetryFor<LifeAction, Constructor> -> bool


type IHostedOrReferencedLifeCycleAdapter<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId
                          when 'Subject              :> Subject<'SubjectId>
                          and  'LifeAction           :> LifeAction
                          and  'OpError              :> OpError
                          and  'Constructor          :> Constructor
                          and  'LifeEvent            :> LifeEvent
                          and  'LifeEvent            : comparison
                          and  'SubjectId            :> SubjectId
                          and  'SubjectId            : comparison> =
    inherit IHostedOrReferencedLifeCycleAdapter
    abstract member ReferencedLifeCycle: IReferencedLifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>
    abstract member ShouldSendTelemetry: telemetryFor: ShouldSendTelemetryFor<'LifeAction, 'Constructor> -> bool
    abstract member ShouldRecordHistory: historyFor:   ShouldRecordHistoryFor<'LifeAction, 'Constructor> -> bool

type HostedOrReferencedLifeCycleAdapter<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId
                          when 'Subject              :> Subject<'SubjectId>
                          and  'LifeAction           :> LifeAction
                          and  'OpError              :> OpError
                          and  'Constructor          :> Constructor
                          and  'LifeEvent            :> LifeEvent
                          and  'LifeEvent            : comparison
                          and  'SubjectId            :> SubjectId
                          and  'SubjectId            : comparison> = {
    ReferencedLifeCycle: IReferencedLifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>
}
with
    member private this.GetFactory (grainProvider: IBiosphereGrainProvider) =
        grainProvider.GetGrainFactory this.ReferencedLifeCycle.Def.LifeCycleKey.EcosystemName

    interface IHostedOrReferencedLifeCycleAdapter with
        member this.ReferencedLifeCycle = this.ReferencedLifeCycle

        member this.IsSessionLifeCycle = this.ReferencedLifeCycle.IsSessionLifeCycle

        member this.RunActionOnGrain (grainProvider: IBiosphereGrainProvider) (GrainPartition grainPartition) (maybeDedupInfo: Option<SideEffectDedupInfo>) (subjectIdStr: string) (action: LifeAction) (maybeBeforeActSubscriptions: Option<BeforeActSubscriptions>) : Task<Result<unit, SubjectFailure<GrainTransitionError<OpError>>>> =
            let typedAction = action :?> 'LifeAction
            fun () -> backgroundTask {
                let! grainFactory = this.GetFactory grainProvider
                let typedGrain = grainFactory.GetGrain<ISubjectGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>>(grainPartition, subjectIdStr)
                match maybeBeforeActSubscriptions with
                | None _ ->
                    // choose old action for compatibility reasons. // TODO: always use V2 when whole biosphere is updated
                    let! result = typedGrain.ActInterGrain maybeDedupInfo (* includeResponse *) false typedAction
                    return result |> Result.mapBoth (fun _ -> ()) (SubjectFailure.CastUnsafe GrainTransitionError<OpError>.CastUnsafe)
                | Some _ ->
                    let! result = typedGrain.ActInterGrainV2 maybeDedupInfo typedAction maybeBeforeActSubscriptions
                    return result |> Result.mapError (SubjectFailure.CastUnsafe GrainTransitionError<OpError>.CastUnsafe)
            }
            |> OrleansTransientErrorDetection.wrapTransientExceptions

        member this.TriggerTimerActionOnGrain (grainProvider: IBiosphereGrainProvider) (GrainPartition grainPartition) (dedupInfo: SideEffectDedupInfo) (subjectId: SubjectId) (tentativeDueAction: LifeAction) : Task<Result<Option<LifeAction>, GrainTriggerTimerError<OpError, LifeAction>>> =
            let typedId = subjectId :?> 'SubjectId
            let typedTentativeDueAction = tentativeDueAction :?> 'LifeAction
            fun () -> backgroundTask {
                let! grainFactory = this.GetFactory grainProvider
                let typedGrain = grainFactory.GetGrain<ISubjectGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>>(grainPartition, (getIdString typedId))
                let! result = typedGrain.TriggerTimer dedupInfo typedTentativeDueAction
                return result |> Result.mapBoth (fun maybeAction -> maybeAction |> Option.map (fun action -> action :> LifeAction)) GrainTriggerTimerError<OpError, LifeAction>.CastUnsafe
            }
            |> OrleansTransientErrorDetection.wrapTransientExceptions

        member this.TryDeleteSelfOnGrain (grainProvider: IBiosphereGrainProvider) (GrainPartition grainPartition) (subjectId: SubjectId) (sideEffectId: GrainSideEffectId) (requiredVersion: uint64) (requiredNextSideEffectSequenceNumber: uint64) (retryAttempt: byte) : Task =
            let typedId = subjectId :?> 'SubjectId
            backgroundTask {
                let! grainFactory = this.GetFactory grainProvider
                let typedGrain = grainFactory.GetGrain<ISubjectGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>>(grainPartition, (getIdString typedId))
                do! typedGrain.TryDeleteSelf sideEffectId requiredVersion requiredNextSideEffectSequenceNumber retryAttempt
            }

        member this.RunPrepareActionOnGrain (grainProvider: IBiosphereGrainProvider) (GrainPartition grainPartition) (subjectId: SubjectId) (action: LifeAction) (transactionId: SubjectTransactionId) : Task<Result<unit, SubjectFailure<GrainPrepareTransitionError<OpError>>>> =
            let typedId = subjectId :?> 'SubjectId

            let typedAction = action :?> 'LifeAction
            fun () -> backgroundTask {
                let! grainFactory = this.GetFactory grainProvider
                let typedGrain = grainFactory.GetGrain<ISubjectGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>>(grainPartition, (getIdString typedId))
                let! result = typedGrain.RunPrepareAction typedAction transactionId
                return result |> Result.mapError (SubjectFailure.CastUnsafe GrainPrepareTransitionError<OpError>.CastUnsafe)
            }
            |> OrleansTransientErrorDetection.wrapTransientExceptions

        member this.RunCommitPreparedOnGrain (grainProvider: IBiosphereGrainProvider) (GrainPartition grainPartition) (subjectId: SubjectId) (transactionId: SubjectTransactionId) : Task =
            let typedId = subjectId :?> 'SubjectId
            fun () -> backgroundTask {
                let! grainFactory = this.GetFactory grainProvider
                let typedGrain = grainFactory.GetGrain<ISubjectGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>>(grainPartition, (getIdString typedId))
                return! typedGrain.RunCommitPrepared transactionId
            }
            |> OrleansTransientErrorDetection.wrapTransientExceptions
            |> Task.Ignore

        member this.RunRollbackPreparedOnGrain (grainProvider: IBiosphereGrainProvider) (GrainPartition grainPartition) (subjectId: SubjectId) (transactionId: SubjectTransactionId) : Task =
            let typedId = subjectId :?> 'SubjectId
            fun () -> backgroundTask {
                let! grainFactory = this.GetFactory grainProvider
                let typedGrain = grainFactory.GetGrain<ISubjectGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>>(grainPartition, (getIdString typedId))
                return! typedGrain.RunRollbackPrepared transactionId
            }
            |> OrleansTransientErrorDetection.wrapTransientExceptions
            |> Task.Ignore

        member this.RunCheckPhantomPreparedOnGrain (grainProvider: IBiosphereGrainProvider) (GrainPartition grainPartition) (subjectId: SubjectId) (transactionId: SubjectTransactionId) : Task =
            let typedId = subjectId :?> 'SubjectId
            fun () -> backgroundTask {
                let! grainFactory = this.GetFactory grainProvider
                let typedGrain = grainFactory.GetGrain<ISubjectGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>>(grainPartition, (getIdString typedId))
                return! typedGrain.RunCheckPhantomPrepared transactionId
            }
            |> OrleansTransientErrorDetection.wrapTransientExceptions
            |> Task.Ignore

        member this.RunActionMaybeConstructOnGrain (grainProvider: IBiosphereGrainProvider) (GrainPartition grainPartition) (maybeDedupInfo: Option<SideEffectDedupInfo>) (subjectId: SubjectId) (action: LifeAction) (ctor: Constructor) (maybeConstructSubscriptions: Option<ConstructSubscriptions>) : Task<Result<unit, SubjectFailure<GrainOperationError<OpError>>>> =
            let typedId = subjectId :?> 'SubjectId
            let typedAction = action :?> 'LifeAction
            let typedCtor = ctor :?> 'Constructor
            fun () -> backgroundTask {
                let! grainFactory = this.GetFactory grainProvider
                let typedGrain = grainFactory.GetGrain<ISubjectGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>>(grainPartition, (getIdString typedId))
                let! result = typedGrain.ActMaybeConstructInterGrain maybeDedupInfo (* includeResponse *) false typedAction typedCtor maybeConstructSubscriptions
                return result |> Result.mapBoth (fun _ -> ()) (SubjectFailure.CastUnsafe GrainOperationError<OpError>.CastUnsafe)
            }
            |> OrleansTransientErrorDetection.wrapTransientExceptions

        member this.InitializeGrain (grainProvider: IBiosphereGrainProvider) (GrainPartition grainPartition) (okIfAlreadyInitialized: bool) (maybeDedupInfo: Option<SideEffectDedupInfo>) (subjectId: SubjectId) (ctor: Constructor) (maybeConstructSubscriptions: Option<ConstructSubscriptions>) : Task<Result<unit, SubjectFailure<GrainConstructionError<OpError>>>> =
            let typedCtor = ctor :?> 'Constructor
            let typedId = subjectId :?> 'SubjectId
            fun () -> backgroundTask {
                let! grainFactory = this.GetFactory grainProvider
                let typedGrain = grainFactory.GetGrain<ISubjectGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>>(grainPartition, (getIdString typedId))
                let! result = typedGrain.ConstructInterGrain okIfAlreadyInitialized maybeDedupInfo (* includeResponse *) false typedId typedCtor maybeConstructSubscriptions
                return result |> Result.mapBoth (fun _ -> ()) (SubjectFailure.CastUnsafe GrainConstructionError<OpError>.CastUnsafe)
            }
            |> OrleansTransientErrorDetection.wrapTransientExceptions

        member this.PrepareInitializeGrain (grainProvider: IBiosphereGrainProvider) (GrainPartition grainPartition) (subjectId: SubjectId) (ctor: Constructor) (transactionId: SubjectTransactionId) : Task<Result<unit, SubjectFailure<GrainPrepareConstructionError<OpError>>>> =
            let typedCtor = ctor :?> 'Constructor
            let typedId = subjectId :?> 'SubjectId
            fun () -> backgroundTask {
                let! grainFactory = this.GetFactory grainProvider
                let typedGrain = grainFactory.GetGrain<ISubjectGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>>(grainPartition, (getIdString typedId))
                let! result = typedGrain.PrepareInitialize typedId typedCtor transactionId
                return result |> Result.mapError (SubjectFailure.CastUnsafe GrainPrepareConstructionError<OpError>.CastUnsafe)
            }
            |> OrleansTransientErrorDetection.wrapTransientExceptions

        member this.SubscribeToGrain (grainProvider: IBiosphereGrainProvider) (GrainPartition grainPartition) (subjectId: SubjectId) (subscriptions: Map<SubscriptionName,LifeEvent>) (subscriberRef: SubjectPKeyReference): Task<Result<unit, SubjectFailure<GrainSubscriptionError>>> =
            let typedId = subjectId :?> 'SubjectId
            fun () -> backgroundTask {
                let! grainFactory = this.GetFactory grainProvider
                let typedGrain = grainFactory.GetGrain<ISubjectGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>>(grainPartition, (getIdString typedId))
                return! typedGrain.Subscribe subscriptions subscriberRef
            }
            |> OrleansTransientErrorDetection.wrapTransientExceptions

        member this.UnsubscribeFromGrain (grainProvider: IBiosphereGrainProvider) (GrainPartition grainPartition) (subjectId: SubjectId) (subscriptions: Set<SubscriptionName>) (subscriberRef: SubjectPKeyReference): Task =
            let typedId = subjectId :?> 'SubjectId
            fun () -> backgroundTask {
                let! grainFactory = this.GetFactory grainProvider
                let typedGrain = grainFactory.GetGrain<ISubjectGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>>(grainPartition, (getIdString typedId))
                return! typedGrain.Unsubscribe subscriptions subscriberRef
            }
            |> OrleansTransientErrorDetection.wrapTransientExceptions
            |> Task.Ignore

        member this.TriggerSubscriptionOnGrain (grainProvider: IBiosphereGrainProvider) (GrainPartition grainPartition) (maybeDedupInfo: Option<SideEffectDedupInfo>) (subjectIdStr: string) (subscriptionTriggerType: SubscriptionTriggerType) (triggeredEvent: LifeEvent): Task<Result<unit, SubjectFailure<GrainTriggerSubscriptionError<OpError>>>> =
            fun () -> backgroundTask {
                let! grainFactory = this.GetFactory grainProvider
                let typedGrain = grainFactory.GetGrain<ISubjectGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>>(grainPartition, subjectIdStr)
                let! result = typedGrain.TriggerSubscription maybeDedupInfo subscriptionTriggerType triggeredEvent
                return result |> Result.mapError (SubjectFailure.CastUnsafe GrainTriggerSubscriptionError<OpError>.CastUnsafe)
            }
            |> OrleansTransientErrorDetection.wrapTransientExceptions

        member this.GenerateId (grainScopedServiceProvider: IServiceProvider) (callOrigin: CallOrigin) (ctor: Constructor) : Task<Result<SubjectId, OpError>> =
            let (GrainPartition grainPartition) = grainScopedServiceProvider.GetRequiredService<GrainPartition>()
            let grainProvider = grainScopedServiceProvider.GetRequiredService<IBiosphereGrainProvider>()
            let idGenIdStr = "IdGen" // idStr is a constant, combination of generic param must point to correct grain unambiguously
            let typedCtor = ctor :?> 'Constructor
            fun () -> backgroundTask {
                let! grainFactory = this.GetFactory grainProvider
                let idGenerationGrain = grainFactory.GetGrain<ISubjectIdGenerationGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>>(grainPartition, idGenIdStr)
                match! idGenerationGrain.GenerateIdV2 callOrigin typedCtor with
                | Ok id ->
                    return id :> SubjectId |> Ok
                | Error (GrainIdGenerationError.IdGenerationError err) ->
                    return err :> OpError |> Error
            }
            |> OrleansTransientErrorDetection.wrapTransientExceptions

        member this.ShouldSendTelemetry (telemetryFor: ShouldSendTelemetryFor<LifeAction, Constructor>) : bool =
            match this.ReferencedLifeCycle.ShouldSendTelemetry with
            | Some shouldSendTelemetry ->
                match telemetryFor with
                | ShouldSendTelemetryFor.LifeAction action ->
                    ShouldSendTelemetryFor.LifeAction (action :?> 'LifeAction)
                | ShouldSendTelemetryFor.Constructor constructor ->
                    ShouldSendTelemetryFor.Constructor (constructor :?> 'Constructor)
                | ShouldSendTelemetryFor.LifeEvent event ->
                    ShouldSendTelemetryFor.LifeEvent event
                |> shouldSendTelemetry
            | None ->
                true

    interface IHostedOrReferencedLifeCycleAdapter<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId> with
        member this.ReferencedLifeCycle = this.ReferencedLifeCycle
        member this.ShouldSendTelemetry (telemetryFor: ShouldSendTelemetryFor<'LifeAction, 'Constructor>) : bool =
            match this.ReferencedLifeCycle.ShouldSendTelemetry with
            | Some shouldSendTelemetry ->
                shouldSendTelemetry telemetryFor
            | None ->
                true
        member this.ShouldRecordHistory (historyFor: ShouldRecordHistoryFor<'LifeAction, 'Constructor>) : bool =
            match this.ReferencedLifeCycle.ShouldRecordHistory with
            | Some shouldRecordHistory ->
                shouldRecordHistory historyFor
            | None ->
                true
