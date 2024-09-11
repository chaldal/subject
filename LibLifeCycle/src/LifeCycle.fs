[<AutoOpen>]
module LibLifeCycle.LifeCycle
#nowarn "57" // TODO remove Experimental attribute when whole biosphere updated

open System
open System.Threading.Tasks
open System.Threading
open Microsoft.Extensions.DependencyInjection
open LibLifeCycleTypes.File
open LibLifeCycle

type AnchorTypeForModule = private AnchorTypeForModule of unit

type Env =
    inherit Record

type AccessPredicateInput =
    inherit Record

type CallScopedEnvDependencies = {
    CallOrigin: CallOrigin
    LocalSubjectRef: Option<LocalSubjectPKeyReference>
}

type EnvironmentFactory<'Env> = Func<CallScopedEnvDependencies, 'Env>

let internal createEnvImpl<'Env> (callScopedEnvDependencies: CallScopedEnvDependencies) (serviceProvider: IServiceProvider) : 'Env =
    let environmentFactory = serviceProvider.GetRequiredService<EnvironmentFactory<'Env>>()
    let environment = environmentFactory.Invoke(callScopedEnvDependencies)
    environment

// Helper for no environment
type NoEnvironment = private {
    NoEnvironmentDummyField: unit // F# doesn't support dummy fields
} with interface Env
let noEnvironment = { NoEnvironmentDummyField = () }

type NoSession = private NoSession of unit

type NoSessionAction = private NoSessionAction of unit
with
    interface LifeAction

type NoSessionLifeEvent = private NoSessionLifeEvent of unit
with
    interface LifeEvent

type NoRole = private NoRole of unit

// TODO: replace all reflection jumps with functions then review how many function interfaces we actually need, there's too many

type FullyTypedLifeCycleFunction<'Res> =
    abstract member Invoke: LifeCycle<_, _, _, _, _, _, _, _, _, _, _> -> 'Res

and ILifeCycle =
    abstract member Name: string
    abstract member Def: LifeCycleDef
    abstract member Invoke: FullyTypedLifeCycleFunction<'Res> -> 'Res

and [<RequireQualifiedAccess>] RevalidateCompleteResult =
| Success
| Failure

and SessionRevalidator<'Session> = {
    GetRevalidateOn:             'Session -> Option<DateTimeOffset>
    RevalidateAction:            LifeAction
    RevalidateCompleteEvent:     LifeEvent
    GetRevalidateCompleteResult: LifeEvent -> RevalidateCompleteResult
}

and EcosystemSessionHandler<'Session> = {
    LifeCycle:        ILifeCycle
    GetIdStr:         'Session -> string
    GetUserId:        'Session -> AccessUserId
    MaybeRevalidator: Option<SessionRevalidator<'Session>>
}

and EcosystemSessionHandling<'Session, 'Role when 'Role : comparison> = {
    Handler: EcosystemSessionHandler<'Session>
    GetRoles: ExternalCallOrigin -> Option<'Session> -> Set<'Role>
}

and FullyTypedLifeCycleFunction<'Res, 'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId
        when 'Subject              :> Subject<'SubjectId>
        and  'LifeAction           :> LifeAction
        and  'OpError              :> OpError
        and  'Constructor          :> Constructor
        and  'LifeEvent            :> LifeEvent
        and  'LifeEvent            :  comparison
        and  'SubjectId            :> SubjectId
        and  'SubjectId            : comparison> =
    abstract member Invoke: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env> -> 'Res

and ILifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId
                 when 'Subject              :> Subject<'SubjectId>
                 and  'LifeAction           :> LifeAction
                 and  'OpError              :> OpError
                 and  'Constructor          :> Constructor
                 and  'LifeEvent            :> LifeEvent
                 and  'LifeEvent            :  comparison
                 and  'SubjectId            :> SubjectId
                 and 'SubjectId : comparison> =
    inherit ILifeCycle
    abstract member Subscriptions:       'Subject -> Map<SubscriptionName, Subscription<'LifeAction>>
    abstract member Timers:              'Subject -> list<Timer<'LifeAction>>
    abstract member SingletonCtor:       Option<'Constructor>
    abstract member Storage:             LifeCycleStorage
    abstract member MetaData:            LifeCycleMetaData
    abstract member ResponseHandler:     SideEffectResponse -> seq<SideEffectResponseDecision<'LifeAction>>
    abstract member ShouldSendTelemetry: Option<ShouldSendTelemetryFor<'LifeAction, 'Constructor> -> bool>
    abstract member ShouldRecordHistory: Option<ShouldRecordHistoryFor<'LifeAction, 'Constructor> -> bool>
    abstract member LifeEventSatisfies:  Option<LifeEventSatisfiesInput<'LifeEvent> -> bool>
    abstract member Invoke:              FullyTypedLifeCycleFunction<'Res, 'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId> -> 'Res

    // TODO: these also can be supplanted with function invocations (Invoke calls)
    abstract member IsSessionLifeCycle: bool
    abstract member AutoIgnoreNoOpTransitions: bool
    abstract member GenerateId: CallOrigin -> IServiceProvider -> 'Constructor -> IdGenerationResult<'SubjectId, 'OpError>
    abstract member Construct: CallOrigin -> IServiceProvider -> 'SubjectId -> 'Constructor -> ConstructionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent>
    abstract member Act: CallOrigin -> IServiceProvider -> 'Subject -> 'LifeAction -> TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor>

and IdGeneration<'Constructor, 'OpError, 'SubjectId, 'Env
        when 'OpError              :> OpError
        and  'SubjectId            :> SubjectId
        and  'SubjectId            :  comparison
        and  'Env :> Env> =
    'Env -> 'Constructor -> IdGenerationResult<'SubjectId, 'OpError>

and Construction<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId, 'Env
                when 'Subject              :> Subject<'SubjectId>
                and  'LifeAction           :> LifeAction
                and  'OpError              :> OpError
                and  'Constructor          :> Constructor
                and  'LifeEvent            :> LifeEvent
                and  'LifeEvent            :  comparison
                and  'SubjectId            :> SubjectId
                and  'SubjectId            :  comparison
                and  'Env                  :> Env> =
    'Env -> 'SubjectId -> 'Constructor -> ConstructionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent>

and Transition<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId, 'Env
                when 'Subject              :> Subject<'SubjectId>
                and  'LifeAction           :> LifeAction
                and  'OpError              :> OpError
                and  'Constructor          :> Constructor
                and  'LifeEvent            :> LifeEvent
                and  'LifeEvent            :  comparison
                and  'SubjectId            :> SubjectId
                and  'SubjectId            :  comparison
                and  'SubjectId            :> SubjectId
                and  'SubjectId            :  comparison
                and  'Env                  :> Env> =
    'Env -> 'Subject -> 'LifeAction -> TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor>

and AccessPredicate<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'AccessPredicateInput, 'Session
                when 'Subject              :> Subject<'SubjectId>
                and  'LifeAction           :> LifeAction
                and  'Constructor          :> Constructor
                and  'SubjectId            :> SubjectId
                and  'SubjectId            :  comparison
                and  'AccessPredicateInput :> AccessPredicateInput> =
    'AccessPredicateInput -> AccessEvent<'Subject, 'LifeAction, 'Constructor, 'SubjectId> -> ExternalCallOrigin -> Option<'Session> -> bool

and LifeCycleApiAccess<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role
                when 'Subject              :> Subject<'SubjectId>
                and  'LifeAction           :> LifeAction
                and  'Constructor          :> Constructor
                and  'SubjectId            :> SubjectId
                and  'SubjectId            :  comparison
                and  'AccessPredicateInput :> AccessPredicateInput
                and  'Role                 :  comparison> = {
    AccessRules:                List<AccessRule<'AccessPredicateInput, 'Role, 'LifeAction, 'Constructor>>
    AccessPredicate:            AccessPredicate<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'AccessPredicateInput, 'Session>
    RateLimitsPredicate:        LifeCycleRateLimitPredicate<'LifeAction, 'Constructor>
    AnonymousCanReadTotalCount: bool
}

// A Subject's Life-Cycle defines some operations that governs its behavior, along with some metadata
and LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env
                when 'Subject              :> Subject<'SubjectId>
                and  'LifeAction           :> LifeAction
                and  'OpError              :> OpError
                and  'Constructor          :> Constructor
                and  'LifeEvent            :> LifeEvent
                and  'LifeEvent            :  comparison
                and  'SubjectIndex         :> SubjectIndex<'OpError>
                and  'SubjectId            :> SubjectId
                and  'SubjectId            :  comparison
                and  'AccessPredicateInput :> AccessPredicateInput
                and  'Role                 :  comparison
                and  'Env                  :> Env> = internal {
    Definition:          LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>
    IdGeneration:        IdGeneration<'Constructor, 'OpError, 'SubjectId, 'Env>
    Construction:        Construction<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId, 'Env>
    AutoIgnoreNoOpTransitions: bool
    Transition:          Transition<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId, 'Env>
    Subscriptions:       'Subject -> Map<SubscriptionName, Subscription<'LifeAction>>
    Timers:              'Subject -> list<Timer<'LifeAction>>
    Indices:             'Subject -> seq<'SubjectIndex>
    SingletonCtor:       Option<'Constructor>
    Storage:             LifeCycleStorage
    MetaData:            LifeCycleMetaData
    MaybeApiAccess:      Option<LifeCycleApiAccess<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role>>
    ResponseHandler:     SideEffectResponse -> seq<SideEffectResponseDecision<'LifeAction>>
    ShouldSendTelemetry: Option<ShouldSendTelemetryFor<'LifeAction, 'Constructor> -> bool>
    ShouldRecordHistory: Option<ShouldRecordHistoryFor<'LifeAction, 'Constructor> -> bool>
    LifeEventSatisfies:  Option<LifeEventSatisfiesInput<'LifeEvent> -> bool>
    SessionHandling:     Option<EcosystemSessionHandling<'Session, 'Role>>
}
with
    member this.Name = this.Definition.Key.LocalLifeCycleName
    member private this.CreateEnv (callOrigin: CallOrigin) (maybeSubjectId: Option<'SubjectId>) (serviceProvider: IServiceProvider) : 'Env =
        createEnvImpl<'Env>
            { CallOrigin = callOrigin
              LocalSubjectRef =
                  maybeSubjectId
                  |> Option.map (fun subjectId ->
                      { LifeCycleName = this.Name; SubjectIdStr = subjectId.IdString }) }
            serviceProvider
    member this.SessionHandler = this.SessionHandling |> Option.map(fun h -> h.Handler)

    interface ILifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId> with
        member this.Invoke (fn: FullyTypedLifeCycleFunction<_, _, _, _, _, _, _>) = fn.Invoke this
        member this.Invoke (fn: FullyTypedLifeCycleFunction<_>) = fn.Invoke this
        member this.Def = this.Definition
        member this.Name = this.Name
        member this.Subscriptions subj = this.Subscriptions subj
        member this.Timers subj = this.Timers subj
        member this.SingletonCtor = this.SingletonCtor
        member this.Storage = this.Storage
        member this.MetaData = this.MetaData
        member this.ResponseHandler response = this.ResponseHandler response
        member this.ShouldSendTelemetry = this.ShouldSendTelemetry
        member this.ShouldRecordHistory = this.ShouldRecordHistory
        member this.LifeEventSatisfies = this.LifeEventSatisfies

        member this.AutoIgnoreNoOpTransitions = this.AutoIgnoreNoOpTransitions

        member this.IsSessionLifeCycle =
            this.SessionHandling
            |> Option.map (fun h -> h.Handler.LifeCycle.Def.LifeCycleKey = this.Definition.Key)
            |> Option.defaultValue false

        member this.GenerateId callOrigin serviceProvider ctor =
            let env = this.CreateEnv callOrigin None serviceProvider
            this.IdGeneration env ctor

        member this.Construct callOrigin serviceProvider subjectId ctor =
            let env = this.CreateEnv callOrigin (Some subjectId) serviceProvider
            this.Construction env subjectId ctor

        member this.Act callOrigin serviceProvider subject action =
            let env = this.CreateEnv callOrigin (Some subject.SubjectId) serviceProvider
            this.Transition env subject action

and AccessRule<'AccessPredicateInput, 'Role, 'LifeAction, 'Constructor
        when 'AccessPredicateInput :> AccessPredicateInput
        and  'Role                 :  comparison
        and  'LifeAction           :> LifeAction
        and  'Constructor          :> Constructor> = {
    Input:      AccessMatch<'AccessPredicateInput>
    EventTypes: AccessMatch<NonemptySet<AccessEventType<'LifeAction, 'Constructor>>>
    Roles:      AccessMatch<NonemptySet<'Role>>
    Decision:   AccessDecision
}

and LifeCycleMetaData = internal {
    IndexKeys_:       Set<IndexKey>
}
with
    member this.IndexKeys = this.IndexKeys_

and Timer<'LifeAction when 'LifeAction :> LifeAction> = {
    TimerAction: TimerAction<'LifeAction>
    Schedule:    Schedule
}

and [<RequireQualifiedAccess>] TimerAction<'LifeAction when 'LifeAction :> LifeAction> =
| RunAction of 'LifeAction
| DeleteSelf

and [<RequireQualifiedAccess>] Schedule =
| Now
| On    of DateTimeOffset
| AfterLastTransition of TimeSpan

and [<RequireQualifiedAccess>] PersistentHistoryExpiration =
/// Subject history retains in _History table for unlimited time until subject is deleted, then history is hard deleted in background after a specified period of time
/// !!TODO!! if you apply this setting to big pre-existing history table, then TombstoneSysEnd SQL index must be created manually! See 20241021_00_AddHistoryTombstoneFlag.sql script
| AfterSubjectDeletion of KeepDeletedSubjectHistoryForAtLeast: TimeSpan
/// Subject history retains in _History table for specified time after history is inserted (think SysStart column), then history is hard deleted in background.
/// This includes history of subjects that are still alive.
/// !!TODO!! if you apply this setting to big pre-existing history table, then Version SQL index must be created manually! See 20241021_01_AlterHistoryVersionIndexIncludeSysStart.sql script
| AfterSubjectChange of KeepSubjectHistoryForAtLeast: TimeSpan

and [<RequireQualifiedAccess>] PersistentHistoryRetention =
| Unfiltered of Option<PersistentHistoryExpiration>
| FilteredByTelemetryRules of Option<PersistentHistoryExpiration>
| FilteredByHistoryRules   of Option<PersistentHistoryExpiration>
| NoHistory of Justification: string
with
    // for backwards compatibility
    static member FullHistory = PersistentHistoryRetention.Unfiltered None
    static member ByTelemetryRules = PersistentHistoryRetention.FilteredByTelemetryRules None

and [<RequireQualifiedAccess>] StorageType =
| Persistent of PromotedIndicesConfig * PersistentHistoryRetention
| Volatile
| Custom of Key: string

and LifeCycleStorage = {
    Type: StorageType
    MaxDedupCacheSize: uint16
}

and ICustomStorageInit =
    abstract member Execute: CancellationToken -> Task

and CustomStorageHandlers = CustomStorageHandlers of System.Collections.Generic.IDictionary<string, Func<IServiceProvider, (obj * obj)>>
with
    member this.GetSubjectRepo<'T> key serviceProvider : Option<'T> =
        match this with
        | CustomStorageHandlers (map) ->
            match map.TryGetValue key with
            | true, f ->
                let subjectRepo = fst(f.Invoke(serviceProvider))
                let castSubjectRepo = subjectRepo :?> 'T
                Some castSubjectRepo
            | false, _ -> None

    member this.GetCustomStorage<'T> key serviceProvider : Option<'T> =
        match this with
        | CustomStorageHandlers (map) ->
            match map.TryGetValue key with
            | true, f ->
                let storageHandler = snd(f.Invoke(serviceProvider))
                let castStorageHandler = storageHandler :?> 'T
                Some castStorageHandler
            | false, _ -> None

and LifeEventSatisfiesInput<'LifeEvent when 'LifeEvent :> LifeEvent and 'LifeEvent : comparison> = {
    Raised:  'LifeEvent
    Subscribed: 'LifeEvent
}

and [<RequireQualifiedAccess>] BlobAction =
| Create of BlobId * Option<MimeType> * FileData
| Append of BlobId * UpdatedBlobId: BlobId * byte[]
| Delete of BlobId

and TransitionBuilderError<'OpError when 'OpError :> OpError> =
| LifeCycleError of 'OpError
| TransitionNotAllowed
// why not just let it bubble up? Because exceptions during transition most likely means bugs in the app,
// in case if it's invoked via Side Effect it should be treated as permanent rather than transient failure.
| LifeCycleException of Exception

and TransitionOk<'Subject, 'LifeAction, 'LifeEvent, 'Constructor // Don't constrain Subjects, but constrain others (See foot-note ^1)
                      when 'LifeAction :> LifeAction
                      and  'LifeEvent  :> LifeEvent
                      and  'Constructor :> Constructor> =
| TransitionOk      of 'Subject * List<BlobAction> * TransitionSideEffects<'Constructor, 'LifeEvent, 'LifeAction>
                    // TransitionIgnored collects side effects & blob actions only to assert that there's none
| TransitionIgnored of List<BlobAction> * TransitionSideEffects<'Constructor, 'LifeEvent, 'LifeAction>

// Operations can be asynchronous ..
and TransitionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent, 'Constructor // Don't constrain Subjects, but constrain others (See foot-note ^1)
                      when 'OpError    :> OpError
                      and  'LifeAction :> LifeAction
                      and  'LifeEvent  :> LifeEvent
                      and  'Constructor :> Constructor> =
                      TransitionResult of Task<Result<TransitionOk<'Subject, 'LifeAction, 'LifeEvent, 'Constructor>, TransitionBuilderError<'OpError>>>

and IdGenerationResult<'SubjectId, 'OpError  // Don't constrain SubjectId, but constrain others (See foot-note ^1)
                        when 'OpError :> OpError> =
                        IdGenerationResult of Task<Result<'SubjectId, 'OpError>>

and ConstructionBuilderError<'OpError when 'OpError :> OpError> =
| LifeCycleCtorError of 'OpError
// why not just let it bubble up? Because exceptions during constrcution most likely means bugs in the app,
// in case if it's invoked via Side Effect it should be treated as permanent rather than transient failure.
| LifeCycleCtorException of Exception

and ConstructionResult<'Subject, 'LifeAction, 'OpError, 'LifeEvent // Don't constrain Constructor, but constrain others (See foot-note ^1)
                        when 'OpError    :> OpError
                        and  'LifeAction :> LifeAction
                        and  'LifeEvent  :> LifeEvent> =
                        ConstructionResult of Task<Result<'Subject * List<BlobAction> * ConstructionSideEffects<'LifeEvent, 'LifeAction>, ConstructionBuilderError<'OpError>>>

and OperationBuilderError<'OpError when 'OpError :> OpError> =
| LifeCycleOperationError of 'OpError
| LifeCycleOperationException of Exception

and OperationResult<'Res, 'LifeAction, 'OpError, 'LifeEvent
                     when 'OpError    :> OpError
                     and  'LifeAction :> LifeAction
                     and  'LifeEvent  :> LifeEvent> =
                     OperationResult of Task<Result<'Res * List<BlobAction> * OperationSideEffects<'LifeEvent, 'LifeAction>, OperationBuilderError<'OpError>>>

and InfallibleOperationResult<'Res, 'LifeAction, 'LifeEvent
                     when  'LifeAction :> LifeAction
                     and  'LifeEvent  :> LifeEvent> =
                     InfallibleOperationResult of Task<'Res * List<BlobAction> * InfallibleOperationSideEffects<'LifeEvent, 'LifeAction>>

and TransitionSideEffects<'Constructor, 'LifeEvent, 'LifeAction
                           when 'LifeAction :> LifeAction
                           and  'Constructor :> Constructor
                           and  'LifeEvent :> LifeEvent> = {
    Constructors:    List<'Constructor>
    LifeEvents:      List<'LifeEvent>
    LifeActions:     List<'LifeAction>
    ExternalActions: List<ExternalOperation<'LifeAction>>
}
with
    member this.IsEmpty =
        this.Constructors.IsEmpty &&
        this.LifeEvents.IsEmpty &&
        this.LifeActions.IsEmpty &&
        this.ExternalActions.IsEmpty

    static member (+) (sideEffect1: TransitionSideEffects<'Constructor, 'LifeEvent, 'LifeAction>, sideEffect2: TransitionSideEffects<'Constructor, 'LifeEvent, 'LifeAction>) =
        {
            ExternalActions = sideEffect1.ExternalActions @ sideEffect2.ExternalActions
            LifeEvents      = sideEffect1.LifeEvents      @ sideEffect2.LifeEvents
            LifeActions     = sideEffect1.LifeActions     @ sideEffect2.LifeActions
            Constructors    = sideEffect1.Constructors    @ sideEffect2.Constructors
        }

    static member get_Zero () = {
        ExternalActions = []
        LifeEvents      = []
        LifeActions     = []
        Constructors    = []
    }

and ConstructionSideEffects<'LifeEvent, 'LifeAction
                             when 'LifeAction :> LifeAction
                             and  'LifeEvent :> LifeEvent> = {
    LifeEvents:      List<'LifeEvent>
    LifeActions:     List<'LifeAction>
    ExternalActions: List<ExternalOperation<'LifeAction>>
}
with
    static member (+) (sideEffect1: ConstructionSideEffects<'LifeEvent, 'LifeAction>, sideEffect2: ConstructionSideEffects<'LifeEvent, 'LifeAction>) =
        {
            ExternalActions = sideEffect1.ExternalActions @ sideEffect2.ExternalActions
            LifeEvents      = sideEffect1.LifeEvents      @ sideEffect2.LifeEvents
            LifeActions     = sideEffect1.LifeActions     @ sideEffect2.LifeActions
        }

    static member get_Zero () = {
        ExternalActions = []
        LifeEvents      = []
        LifeActions     = []
    }

and OperationSideEffects<'LifeEvent, 'LifeAction
                          when 'LifeAction :> LifeAction
                          and  'LifeEvent :> LifeEvent> = {
    LifeEvents:      List<'LifeEvent>
    LifeActions:     List<'LifeAction>
    ExternalActions: List<ExternalOperation<'LifeAction>>
}
with
    static member (+) (sideEffect1: OperationSideEffects<'LifeEvent, 'LifeAction>, sideEffect2: OperationSideEffects<'LifeEvent, 'LifeAction>) =
        {
            ExternalActions = sideEffect1.ExternalActions @ sideEffect2.ExternalActions
            LifeEvents      = sideEffect1.LifeEvents      @ sideEffect2.LifeEvents
            LifeActions     = sideEffect1.LifeActions     @ sideEffect2.LifeActions
        }

    static member (+) (sideEffect1: OperationSideEffects<'LifeEvent, 'LifeAction>, sideEffect2: TransitionSideEffects<'Constructor, 'LifeEvent, 'LifeAction>)
        : TransitionSideEffects<'Constructor, 'LifeEvent, 'LifeAction>=
        {
            Constructors    = []
            ExternalActions = sideEffect1.ExternalActions @ sideEffect2.ExternalActions
            LifeEvents      = sideEffect1.LifeEvents      @ sideEffect2.LifeEvents
            LifeActions     = sideEffect1.LifeActions     @ sideEffect2.LifeActions
        }

    static member (+) (sideEffect1: OperationSideEffects<'LifeEvent, 'LifeAction>, sideEffect2: ConstructionSideEffects<'LifeEvent, 'LifeAction>)
        : ConstructionSideEffects<'LifeEvent, 'LifeAction>=
        {
            ExternalActions = sideEffect1.ExternalActions @ sideEffect2.ExternalActions
            LifeEvents      = sideEffect1.LifeEvents      @ sideEffect2.LifeEvents
            LifeActions     = sideEffect1.LifeActions     @ sideEffect2.LifeActions
        }

    static member get_Zero () = {
        ExternalActions = []
        LifeEvents      = []
        LifeActions     = []
    }

and InfallibleOperationSideEffects<'LifeEvent, 'LifeAction
                          when 'LifeAction :> LifeAction
                          and  'LifeEvent :> LifeEvent> = {
    LifeEvents:      List<'LifeEvent>
    LifeActions:     List<'LifeAction>
    ExternalActions: List<ExternalOperation<'LifeAction>>
}
with
    static member (+) (sideEffect1: InfallibleOperationSideEffects<'LifeEvent, 'LifeAction>, sideEffect2: InfallibleOperationSideEffects<'LifeEvent, 'LifeAction>) =
        {
            ExternalActions = sideEffect1.ExternalActions @ sideEffect2.ExternalActions
            LifeEvents      = sideEffect1.LifeEvents      @ sideEffect2.LifeEvents
            LifeActions     = sideEffect1.LifeActions     @ sideEffect2.LifeActions
        }
    
    static member (+) (sideEffect1: InfallibleOperationSideEffects<'LifeEvent, 'LifeAction>, sideEffect2: OperationSideEffects<'LifeEvent, 'LifeAction>)
        : OperationSideEffects<'LifeEvent, 'LifeAction>=
        {
            ExternalActions = sideEffect1.ExternalActions @ sideEffect2.ExternalActions
            LifeEvents      = sideEffect1.LifeEvents      @ sideEffect2.LifeEvents
            LifeActions     = sideEffect1.LifeActions     @ sideEffect2.LifeActions
        }
    
    static member (+) (sideEffect1: InfallibleOperationSideEffects<'LifeEvent, 'LifeAction>, sideEffect2: TransitionSideEffects<'Constructor, 'LifeEvent, 'LifeAction>)
        : TransitionSideEffects<'Constructor, 'LifeEvent, 'LifeAction>=
        {
            Constructors    = []
            ExternalActions = sideEffect1.ExternalActions @ sideEffect2.ExternalActions
            LifeEvents      = sideEffect1.LifeEvents      @ sideEffect2.LifeEvents
            LifeActions     = sideEffect1.LifeActions     @ sideEffect2.LifeActions
        }

    static member (+) (sideEffect1: InfallibleOperationSideEffects<'LifeEvent, 'LifeAction>, sideEffect2: ConstructionSideEffects<'LifeEvent, 'LifeAction>)
        : ConstructionSideEffects<'LifeEvent, 'LifeAction>=
        {
            ExternalActions = sideEffect1.ExternalActions @ sideEffect2.ExternalActions
            LifeEvents      = sideEffect1.LifeEvents      @ sideEffect2.LifeEvents
            LifeActions     = sideEffect1.LifeActions     @ sideEffect2.LifeActions
        }

    static member get_Zero () = {
        ExternalActions = []
        LifeEvents      = []
        LifeActions     = []
    }

and [<RequireQualifiedAccess>] LifeCycleOp<'LifeAction, 'Constructor, 'SubjectId
                                            when 'LifeAction :> LifeAction
                                            and  'SubjectId  :> SubjectId> =
| Act               of 'SubjectId * 'LifeAction * ActOptions
| Construct         of 'Constructor
| MaybeConstruct    of 'Constructor
| ActMaybeConstruct of 'SubjectId * 'LifeAction * 'Constructor * ActOptions

and [<RequireQualifiedAccess>] LifeCycleTxnOp<'LifeAction, 'Constructor, 'SubjectId
                                               when 'LifeAction :> LifeAction
                                               and  'SubjectId  :> SubjectId> =
| PrepareAct        of 'SubjectId * 'LifeAction * SubjectTransactionId
// transactional construction needs a pre-generated Id so transaction coordinator
// can locate it for commit or rollback. It means burden of Id generation is on client, unfortunately.
| PrepareInitialize of 'SubjectId * 'Constructor * SubjectTransactionId
| Commit            of 'SubjectId * SubjectTransactionId
| Rollback          of 'SubjectId * SubjectTransactionId
| CheckPhantom      of 'SubjectId * SubjectTransactionId

and ActOptions = {
    /// If false, then action will run *at least once*, if true then *exactly once* until success or permanent failure.
    /// Note that exactly-once guarantee is the subject to size of dedup cache, if subject has more distinct callers
    /// than the size of the cache then action might still run at-least-once.
    Deduplicate: bool

    /// If true, then trace context details such as UserId, SessionId and parent telemetry item id will be erased
    /// for this action, i.e. action will appear as a top-level operation in telemetry, which can be useful to break
    /// very long call chains.
    ResetTraceContext: bool
}

and Subscription<'LifeAction> =
| ForSubject      of SubjectSubscription * ActionToRaise: 'LifeAction
| ForSubjectMap   of SubjectSubscription * MapLifeEventToActionToRaise: (LifeEvent -> Option<'LifeAction>)

and SubjectSubscription = {
    TargetLifeCycleKey: LifeCycleKey
    TargetSubjectId: SubjectId
    SubscribedLifeEvent: LifeEvent
}

and FullyTypedExternalLifeCycleOperationFunction<'Res> =
    abstract member Invoke: ExternalLifeCycleOperation<_, _, _> -> 'Res

and IExternalLifeCycleOperation =
    abstract member Invoke: FullyTypedExternalLifeCycleOperationFunction<'Res> -> 'Res

and FullyTypedExternalLifeCycleTxnOperationFunction<'Res> =
    abstract member Invoke: ExternalLifeCycleTxnOperation<_, _, _> -> 'Res

and IExternalLifeCycleTxnOperation =
    abstract member Invoke: FullyTypedExternalLifeCycleTxnOperationFunction<'Res> -> 'Res

and FullyTypedIngestTimeSeriesDataPointsOperationFunction<'Res> =
    abstract member Invoke: IngestTimeSeriesDataPointsOperation<_, _, _> -> 'Res

and IIngestTimeSeriesDataPointsOperation =
    abstract member Invoke: FullyTypedIngestTimeSeriesDataPointsOperationFunction<'Res> -> 'Res


// ... and one or more side-effects that are operations other subject's life-cycles
and ExternalLifeCycleOperation<'LifeAction, 'Constructor, 'SubjectId
                when 'LifeAction           :> LifeAction
                and  'Constructor          :> Constructor
                and  'SubjectId            :> SubjectId
                and  'SubjectId            : comparison> = private {
    LifeCycleKey_: LifeCycleKey
    Op_:           LifeCycleOp<'LifeAction, 'Constructor, 'SubjectId>
}
with
    member this.LifeCycleKey = this.LifeCycleKey_
    member this.Op = this.Op_
    interface IExternalLifeCycleOperation with
        member this.Invoke fn = fn.Invoke this

and ExternalLifeCycleTxnOperation<'LifeAction, 'Constructor, 'SubjectId
                when 'LifeAction           :> LifeAction
                and  'Constructor          :> Constructor
                and  'SubjectId            :> SubjectId
                and  'SubjectId            : comparison> = private {
    LifeCycleKey_: LifeCycleKey
    Op_:           LifeCycleTxnOp<'LifeAction, 'Constructor, 'SubjectId>
}
with
    member this.LifeCycleKey = this.LifeCycleKey_
    member this.Op = this.Op_
    interface IExternalLifeCycleTxnOperation with
        member this.Invoke fn = fn.Invoke this

and IngestTimeSeriesDataPointsOperation<'TimeSeriesDataPoint, 'TimeSeriesId, [<Measure>] 'UnitOfMeasure
    when 'TimeSeriesDataPoint :> TimeSeriesDataPoint<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure>
    and  'TimeSeriesId        :> TimeSeriesId<'TimeSeriesId>> = private {
    TimeSeriesKey_: TimeSeriesKey
    Points_: list<'TimeSeriesDataPoint>
}
with
    member this.TimeSeriesKey = this.TimeSeriesKey_
    member this.Points = this.Points_
    interface IIngestTimeSeriesDataPointsOperation with
        member this.Invoke fn = fn.Invoke this

and [<RequireQualifiedAccess>] ExternalOperation<'LifeAction when 'LifeAction :> LifeAction> =
| ExternalLifeCycleOperation          of IExternalLifeCycleOperation
| ExternalSubjectTransactionOperation of IExternalLifeCycleTxnOperation * BatchNo: uint16 * OpNo: uint16
| ExternalTimeSeriesOperation         of IIngestTimeSeriesDataPointsOperation
| ExternalConnectorOperation          of ConnectorName: string * BuildRequest: obj (* ResponseChannel<'Response> -> Request  *) * BuildResponse: (obj (* Request *) -> 'LifeAction) * ResponseType: Type
| ExternalConnectorMultiResponseOperation of ConnectorName: string * BuildRequest: obj (* MultiResponseChannel<'Response> -> Request  *) * BuildResponse: (obj (* Request *) -> 'LifeAction) * ResponseType: Type

and [<RequireQualifiedAccess>] SideEffectSuccessDecision<'LifeAction when 'LifeAction :> LifeAction> =
| RogerThat
| Continue of 'LifeAction

and [<RequireQualifiedAccess>] SideEffectFailureDecision<'LifeAction when 'LifeAction :> LifeAction> =
| Escalate   of SideEffectFailureSeverity
| Dismiss
| Compensate of 'LifeAction
// Consider adding Retry to treat failure as transient error (with max count, otherwise can easily lead to infinite retries)

and SideEffectResponseDecision<'LifeAction when 'LifeAction :> LifeAction> =
    internal SideEffectResponseDecision_ of Choice<SideEffectSuccessDecision<'LifeAction>, SideEffectFailureDecision<'LifeAction>>
with
    member this.Value = match this with | SideEffectResponseDecision_ value -> value

and [<RequireQualifiedAccess>] ShouldSendTelemetryFor<'LifeAction, 'Constructor when 'LifeAction  :> LifeAction and 'Constructor :> Constructor> =
| LifeAction  of 'LifeAction
| Constructor of 'Constructor
| LifeEvent   of PublisherEvent: LifeEvent // TODO: this make sense only for hosted life cycles, remove from referenced LC builders somehow

and [<RequireQualifiedAccess>] ShouldRecordHistoryFor<'LifeAction, 'Constructor when 'LifeAction  :> LifeAction and 'Constructor :> Constructor> =
| LifeAction  of 'LifeAction
| Constructor of 'Constructor
| LifeEvent   of PublisherEvent: LifeEvent // TODO: this make sense only for hosted life cycles, remove from referenced LC builders somehow

type NextOnSideEffectSuccess = private NextOnSideEffectSuccess_ of unit
with
    member this.RogerThat<'LifeAction when 'LifeAction :> LifeAction> () : SideEffectResponseDecision<'LifeAction> =
        SideEffectSuccessDecision.RogerThat |> Choice1Of2 |> SideEffectResponseDecision_
    member this.Continue<'LifeAction when 'LifeAction :> LifeAction> (action: 'LifeAction) : SideEffectResponseDecision<'LifeAction> =
        SideEffectSuccessDecision.Continue action |> Choice1Of2 |> SideEffectResponseDecision_

type NextOnSideEffectFailure = private NextOnSideEffectFailure_ of unit
with
    member this.Escalate<'LifeAction when 'LifeAction :> LifeAction> (severity: SideEffectFailureSeverity) : SideEffectResponseDecision<'LifeAction> =
        SideEffectFailureDecision.Escalate severity |> Choice2Of2 |> SideEffectResponseDecision_
    member this.Dismiss<'LifeAction when 'LifeAction :> LifeAction> () : SideEffectResponseDecision<'LifeAction> =
        SideEffectFailureDecision.Dismiss |> Choice2Of2 |> SideEffectResponseDecision_
    member this.Compensate<'LifeAction when 'LifeAction :> LifeAction> (action: 'LifeAction) : SideEffectResponseDecision<'LifeAction> =
        SideEffectFailureDecision.Compensate action |> Choice2Of2 |> SideEffectResponseDecision_

type OnLifeCycleResponse<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId
                when 'Subject              :> Subject<'SubjectId>
                and  'LifeAction           :> LifeAction
                and  'OpError              :> OpError
                and  'Constructor          :> Constructor
                and  'LifeEvent            :> LifeEvent
                and  'LifeEvent            :  comparison
                and  'SubjectIndex         :> SubjectIndex<'OpError>
                and  'SubjectId            :> SubjectId
                and  'SubjectId            : comparison> = private OnLifeCycleResponse of SideEffectResponse * LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>

let internal createLifeCycleOp
    (lifeCycleDef: LifeCycleDef<_, 'LifeAction, _, 'Constructor, _, _, 'SubjectId>)
    (op: LifeCycleOp<'LifeAction, 'Constructor, 'SubjectId>)
    : IExternalLifeCycleOperation =
    ({ LifeCycleKey_ = lifeCycleDef.Key; Op_ = op } : ExternalLifeCycleOperation<_, _, _>)
    :> IExternalLifeCycleOperation

let internal createIngestTimeSeriesDataPointsOp
    (timeSeriesDef: TimeSeriesDef<'TimeSeriesDataPoint, _, _, _, _>)
    (points: list<'TimeSeriesDataPoint>)
    : IIngestTimeSeriesDataPointsOperation =
    { TimeSeriesKey_ = timeSeriesDef.Key; Points_ = points }
    :> IIngestTimeSeriesDataPointsOperation

let internal createLifeCycleTxnOp
    (lifeCycleDef: LifeCycleDef<_, 'LifeAction, _, 'Constructor, _, _, 'SubjectId>)
    (op: LifeCycleTxnOp<'LifeAction, 'Constructor, 'SubjectId>)
    : IExternalLifeCycleTxnOperation =
    ({ LifeCycleKey_ = lifeCycleDef.Key; Op_ = op } : ExternalLifeCycleTxnOperation<_, _, _>)
    :> IExternalLifeCycleTxnOperation

let defaultActOptions : ActOptions = { Deduplicate = true; ResetTraceContext = false }

type LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId
                when 'Subject              :> Subject<'SubjectId>
                and  'LifeAction           :> LifeAction
                and  'OpError              :> OpError
                and  'Constructor          :> Constructor
                and  'LifeEvent            :> LifeEvent
                and  'LifeEvent            :  comparison
                and  'SubjectIndex         :> SubjectIndex<'OpError>
                and  'SubjectId            :> SubjectId>
    with
        /// Enqueues the life cycle action into the caller subject side-effect queue.
        /// Default behavior is to deduplicate it and not include subject body in response.
        member this.Act (id: 'SubjectId) (action: 'LifeAction) : IExternalLifeCycleOperation =
            this.ActWith defaultActOptions id action

        /// Enqueues the life cycle action into the caller subject side-effect queue with
        /// ability to override default behavior.
        member this.ActWith (options: ActOptions) (id: 'SubjectId) (action: 'LifeAction) : IExternalLifeCycleOperation =
            createLifeCycleOp this (LifeCycleOp.Act(id, action, options))

        /// Enqueues the life cycle constructor into the caller subject side-effect queue.
        /// Constructor will be attempted *exactly once*.
        /// However, if subject with the same Id was constructed concurrently it will result
        /// in ConstructAlreadyInitialized failure.
        /// If possible concurrent construction is not an issue then MaybeConstruct can be used instead.
        member this.Construct (constructor: 'Constructor) : IExternalLifeCycleOperation =
            createLifeCycleOp this (LifeCycleOp.Construct constructor)

        /// Enqueues the life cycle constructor into the caller subject side-effect queue.
        /// Noop if target subject already constructed - regardless of whether it was
        /// odd second attempt of this construction or if constructed concurrently.
        /// If you care that other subjects can concurrently construct subject with the same id
        /// then use Construct instead and handle ConstructAlreadyInitialized failure.
        member this.MaybeConstruct (constructor: 'Constructor) : IExternalLifeCycleOperation =
            createLifeCycleOp this (LifeCycleOp.MaybeConstruct constructor)

        /// Similar to MaybeConstruct followed by Act but enqueued as one operation to reduce network costs.
        /// Supplied ctor *must* generate same subject id as supplied *id* argument.
        member this.ActMaybeConstruct (id: 'SubjectId) (action: 'LifeAction) (ctor: 'Constructor) : IExternalLifeCycleOperation =
            this.ActMaybeConstructWith defaultActOptions id action ctor

        /// Same as ActMaybeConstruct but allows to override Act options.
        member this.ActMaybeConstructWith (options: ActOptions) (id: 'SubjectId) (action: 'LifeAction) (ctor: 'Constructor) : IExternalLifeCycleOperation =
            createLifeCycleOp this (LifeCycleOp.ActMaybeConstruct(id, action, ctor, options))

        // TODO: consider NoDedup versions for Subscribe. Although one subject doesn't listen for too many publishers

        /// Creates subscription to a life event on a life cycle.
        /// Caller provides some predefined event handling action.
        /// In case of the event, action will run *exactly once*.
        member this.SubscribeToSubject<'SubscriberAction when 'SubscriberAction :> LifeAction>
                    (id: 'SubjectId) (subscribedLifeEvent: 'LifeEvent) (actionToRaise: 'SubscriberAction)
                    : Subscription<'SubscriberAction> =
            ForSubject({
                TargetLifeCycleKey  = this.Key
                TargetSubjectId     = id
                SubscribedLifeEvent = subscribedLifeEvent
            }, actionToRaise)

        /// Creates subscription to a life event on a life cycle.
        /// Caller provides mapping from a fired event to event handling action.
        member this.SubscribeToSubjectAndMapLifeEvent<'SubscriberAction when 'SubscriberAction :> LifeAction>
                    (id: 'SubjectId) (subscribedLifeEvent: 'LifeEvent) (mapLifeEventToActionToRaise: 'LifeEvent -> Option<'SubscriberAction>)
                    : Subscription<'SubscriberAction> =
            let mapper (lifeEvent: LifeEvent) =
                match lifeEvent with
                | :? 'LifeEvent as tlifeEvent ->
                    mapLifeEventToActionToRaise tlifeEvent
                | _ ->
                    None

            ForSubjectMap({
                TargetLifeCycleKey  = this.Key
                TargetSubjectId     = id
                SubscribedLifeEvent = subscribedLifeEvent
            }, mapper)

        /// Helps to match SideEffectResponse returned by this life cycle on a caller's response handler
        /// in a typesafe manner. Match return value using active patterns:
        /// Success:
        /// |ConstructOk|
        /// |ActOk|
        /// |SubscribeOk|
        /// Failures:
        /// |ConstructError|,
        /// |ConstructAlreadyInitialized|,
        /// |ActNotInitialized|,
        /// |ActError|,
        /// |ActNotAllowed|,
        /// |SubscribeNotInitialized|
        member this.OnResponse (response: SideEffectResponse) = OnLifeCycleResponse (response, this)

type LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env
                when 'Subject              :> Subject<'SubjectId>
                and  'LifeAction           :> LifeAction
                and  'OpError              :> OpError
                and  'Constructor          :> Constructor
                and  'LifeEvent            :> LifeEvent
                and  'LifeEvent            :  comparison
                and  'SubjectIndex         :> SubjectIndex<'OpError>
                and  'SubjectId            :> SubjectId
                and  'AccessPredicateInput :> AccessPredicateInput
                and  'Role                 :  comparison
                and  'Env                  :> Env>
    with
        /// Enqueues the life cycle action into the caller subject side-effect queue.
        /// Default behavior is to deduplicate it and not include subject body in response.
        member this.Act (id: 'SubjectId) (action: 'LifeAction) : IExternalLifeCycleOperation =
            this.Definition.Act id action

        /// Enqueues the life cycle action into the caller subject side-effect queue with
        /// ability to override default behavior.
        member this.ActWith (options: ActOptions) (id: 'SubjectId) (action: 'LifeAction) : IExternalLifeCycleOperation =
            this.Definition.ActWith options id action

        /// Enqueues the life cycle constructor into the caller subject side-effect queue.
        /// Constructor will be attempted *exactly once*.
        /// However if subject with the same Id was constructed concurrently it will result
        /// in ConstructAlreadyInitialized failure.
        /// If possible concurrent construction is not an issue then MaybeConstruct can
        /// be used instead (it's cheaper).
        member this.Construct (constructor: 'Constructor) : IExternalLifeCycleOperation =
            this.Definition.Construct constructor

        /// Enqueues the life cycle constructor into the caller subject side-effect queue.
        /// Noop if target subject already constructed - regardless of whether it was
        /// odd second attempt of this construction or if constructed concurrently.
        /// If you care that other subjects can concurrently construct subject with the same id
        /// then use Construct instead and handle ConstructAlreadyInitialized failure.
        member this.MaybeConstruct (constructor: 'Constructor) : IExternalLifeCycleOperation =
            this.Definition.MaybeConstruct constructor

        /// Similar to MaybeConstruct followed by Act but enqueued as one operation to reduce network costs.
        /// Supplied ctor *must* generate same subject id as supplied *id* argument.
        member this.ActMaybeConstruct (id: 'SubjectId) (action: 'LifeAction) (ctor: 'Constructor) : IExternalLifeCycleOperation =
            this.Definition.ActMaybeConstruct id action ctor

        /// Same as ActMaybeConstruct but allows to override Act options.
        member this.ActMaybeConstructWith (options: ActOptions) (id: 'SubjectId) (action: 'LifeAction) (ctor: 'Constructor) : IExternalLifeCycleOperation =
            this.Definition.ActMaybeConstructWith options id action ctor

        /// Creates subscription to a life event on a life cycle.
        /// Caller provides some predefined event handling action.
        /// In case of the event, action will run *exactly once*.
        member this.SubscribeToSubject<'SubscriberAction when 'SubscriberAction :> LifeAction>
                    (id: 'SubjectId) (subscribedLifeEvent: 'LifeEvent) (actionToRaise: 'SubscriberAction)
                    : Subscription<'SubscriberAction> =
            this.Definition.SubscribeToSubject id subscribedLifeEvent actionToRaise

        /// Creates subscription to a life event on a life cycle.
        /// Caller provides mapping from a fired event to event handling action.
        member this.SubscribeToSubjectAndMapLifeEvent<'SubscriberAction when 'SubscriberAction :> LifeAction>
                    (id: 'SubjectId) (subscribedLifeEvent: 'LifeEvent) (mapLifeEventToActionToRaise: 'LifeEvent -> Option<'SubscriberAction>)
                    : Subscription<'SubscriberAction> =
            this.Definition.SubscribeToSubjectAndMapLifeEvent id subscribedLifeEvent mapLifeEventToActionToRaise

        /// Helps to match SideEffectResponse returned by this life cycle on a caller's response handler
        /// in a typesafe manner. Match return value using active patterns:
        /// Success:
        /// |ConstructOk|
        /// |ActOk|
        /// |SubscribeOk|
        /// Failures:
        /// |ConstructError|,
        /// |ConstructAlreadyInitialized|,
        /// |ActNotInitialized|,
        /// |ActError|,
        /// |ActNotAllowed|,
        /// |SubscribeNotInitialized|
        member this.OnResponse (response: SideEffectResponse) =
            this.Definition.OnResponse response

let noLifeEvents    = []
let noBlobActions   = []

let private nextOnSuccess = NextOnSideEffectSuccess_ ()

// this used to be an actual parameter in active pattern but was ignored almost always, keep it to not break existing code.
type RemovedActivePatternParameter = private RemovedActivePatternParameter of unit
let private removedActivePatternParameter = RemovedActivePatternParameter ()

let (|ConstructOk|_|) (OnLifeCycleResponse (response: SideEffectResponse, lifeCycleDef: LifeCycleDef<'Subject, _, _, 'Constructor, _, _, _>)) =
    match response with
    | SideEffectResponse.Success (SideEffectSuccess.ConstructOk (subjectRef, ctor)) when subjectRef.LifeCycleKey = lifeCycleDef.Key ->
        Some (subjectRef.SubjectId :?> 'SubjectId, ctor :?> 'Constructor, removedActivePatternParameter, nextOnSuccess)
    | _ ->
        None

let (|ActOk|_|) (OnLifeCycleResponse (response: SideEffectResponse, lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, _, _, _, _, 'SubjectId>)) =
    match response with
    | SideEffectResponse.Success (SideEffectSuccess.ActOk (subjectRef, action)) when subjectRef.LifeCycleKey = lifeCycleDef.Key ->
        Some (subjectRef.SubjectId :?> 'SubjectId, action :?> 'LifeAction, removedActivePatternParameter, nextOnSuccess)
    | _ ->
        None

let (|SubscribeOk|_|) (OnLifeCycleResponse (response: SideEffectResponse, lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, _, _, _, _, 'SubjectId>)) =
    match response with
    | SideEffectResponse.Success (SideEffectSuccess.SubscribeOk (subjectRef, subscriptions)) when subjectRef.LifeCycleKey = lifeCycleDef.Key ->
        Some (subjectRef.SubjectId :?> 'SubjectId, subscriptions, nextOnSuccess)
    | _ ->
        None

let private nextOnFailure = NextOnSideEffectFailure_ ()

let (|ConstructAlreadyInitialized|_|) (OnLifeCycleResponse (response: SideEffectResponse, lifeCycleDef: LifeCycleDef<_, _, _, 'Constructor, _, _, 'SubjectId>)) =
    match response with
    | SideEffectResponse.Failure (SideEffectFailure.ConstructAlreadyInitialized (subjectRef, ctor)) when subjectRef.LifeCycleKey = lifeCycleDef.Key ->
        Some (subjectRef.SubjectId :?> 'SubjectId, ctor :?> 'Constructor, nextOnFailure)
    | _ ->
        None

let (|ConstructError|_|) (OnLifeCycleResponse (response: SideEffectResponse, lifeCycleDef: LifeCycleDef<_, _, 'OpError, 'Constructor, _, _, 'SubjectId>)) =
    match response with
    | SideEffectResponse.Failure (SideEffectFailure.ConstructError (subjectRef, ctor, err)) when subjectRef.LifeCycleKey = lifeCycleDef.Key ->
        Some (subjectRef.SubjectId :?> 'SubjectId, ctor :?> 'Constructor, err :?> 'OpError, nextOnFailure)
    | _ ->
        None

let (|ActNotInitialized|_|) (OnLifeCycleResponse (response: SideEffectResponse, lifeCycleDef: LifeCycleDef<_, 'LifeAction, _, _, _, _, 'SubjectId>)) =
    match response with
    | SideEffectResponse.Failure (SideEffectFailure.ActNotInitialized (subjectRef, action)) when subjectRef.LifeCycleKey = lifeCycleDef.Key ->
        Some (subjectRef.SubjectId :?> 'SubjectId, action :?> 'LifeAction, nextOnFailure)
    | _ ->
        None

let (|ActError|_|) (OnLifeCycleResponse (response: SideEffectResponse, lifeCycleDef: LifeCycleDef<_, 'LifeAction, 'OpError, _, _, _, 'SubjectId>)) =
    match response with
    | SideEffectResponse.Failure (SideEffectFailure.ActError (subjectRef, action, err)) when subjectRef.LifeCycleKey = lifeCycleDef.Key ->
        Some (subjectRef.SubjectId :?> 'SubjectId, action :?> 'LifeAction, err :?> 'OpError, nextOnFailure)
    | _ ->
        None

let (|ActNotAllowed|_|) (OnLifeCycleResponse (response: SideEffectResponse, lifeCycleDef: LifeCycleDef<_, 'LifeAction, _, _, _, _, 'SubjectId>)) =
    match response with
    | SideEffectResponse.Failure (SideEffectFailure.ActNotAllowed (subjectRef, action)) when subjectRef.LifeCycleKey = lifeCycleDef.Key ->
        Some (subjectRef.SubjectId :?> 'SubjectId, action :?> 'LifeAction, nextOnFailure)
    | _ ->
        None

let (|SubscribeNotInitialized|_|) (OnLifeCycleResponse (response: SideEffectResponse, lifeCycleDef: LifeCycleDef<_, _, _, _, 'LifeEvent, _, 'SubjectId>)) =
    match response with
    | SideEffectResponse.Failure (SideEffectFailure.SubscribeNotInitialized (subjectRef, subscriptions)) when subjectRef.LifeCycleKey = lifeCycleDef.Key ->
        Some (subjectRef.SubjectId :?> 'SubjectId, subscriptions |> Map.map (fun _ e -> e :?> 'LifeEvent), nextOnFailure)
    | _ ->
        None

// FOOT-NOTES:
// ^1 - Errors are constained to allow Return(Subject) and Return(Error) to co-exist
//      Subjects are not constrained so Bind(Task<?>) is possible


// CODECS

#if !FABLE_COMPILER

open CodecLib

type BlobAction
with
    static member get_Codec () =
        function
        | Create _ ->
            codec {
                let! payload = reqWith (Codecs.tuple3 (BlobId.get_Codec ()) (Codecs.option (MimeType.get_Codec ())) codecFor<_, FileData>) "Create" (function Create (x1, x2, x3) -> Some (x1, x2, x3) | _ -> None)
                return Create payload
            }
        | Append _ ->
            codec {
                let! payload = reqWith (Codecs.tuple3 (BlobId.get_Codec ()) (BlobId.get_Codec ()) Codecs.base64Bytes) "Append" (function Append (x1, x2, x3) -> Some (x1, x2, x3) | _ -> None)
                return Append payload
            }
        | Delete _ ->
            codec {
                let! payload = reqWith (BlobId.get_Codec ()) "Delete" (function Delete x -> Some x | _ -> None)
                return Delete payload
            }
        |> mergeUnionCases
        |> ofObjCodec

#endif // !FABLE_COMPILER