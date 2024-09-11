namespace LibLifeCycle

open System.Reflection
open LibLifeCycle

type LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env
                when 'Subject              :> Subject<'SubjectId>
                and  'LifeAction           :> LifeAction
                and  'OpError              :> OpError
                and  'Constructor          :> Constructor
                and  'LifeEvent            :> LifeEvent
                and  'LifeEvent            :  comparison
                and  'SubjectIndex         :> SubjectIndex<'OpError>
                and  'SubjectId            :> SubjectId
                and  'SubjectId            : comparison
                and  'AccessPredicateInput :> AccessPredicateInput
                and  'Role                 :  comparison
                and  'Env                  :> Env> =
    internal {
        Def:                 LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>
        IdGeneration:        Option<IdGeneration<'Constructor, 'OpError, 'SubjectId, 'Env>>
        Construction:        Option<Construction<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId, 'Env>>
        Transition:          Option<Transition<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId, 'Env>>
        AutoIgnoreNoOpTransitions : bool
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
    member internal this.AssumeTypes<'NewAccessPredicateInput, 'NewRole, 'NewEnv
            when 'NewAccessPredicateInput :> AccessPredicateInput
            and  'NewRole                 :  comparison
            and  'NewEnv                  :> Env>()
            : LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'NewAccessPredicateInput, 'Session, 'NewRole, 'NewEnv> =
        {
            // All these fields have generic types that can be "filled in" as the builder is used, so we make a best effort to
            // retain existing values if their types match. Otherwise, those values are dropped.
            MaybeApiAccess =
                match this.MaybeApiAccess |> box with
                | :? (Option<LifeCycleApiAccess<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'NewAccessPredicateInput, 'Session, 'NewRole>>) as existing -> existing
                | _ -> None
            SessionHandling =
                match this.SessionHandling |> box with
                | :? (Option<EcosystemSessionHandling<'Session, 'NewRole>>) as existing -> existing
                | _ -> None
            IdGeneration =
                match this.IdGeneration |> box with
                | :? (Option<IdGeneration<'Constructor, 'OpError, 'SubjectId, 'NewEnv>>) as existing -> existing
                | _ -> None
            Construction =
                match this.Construction |> box with
                | :? (Option<Construction<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId, 'NewEnv>>) as existing -> existing
                | _ -> None
            Transition =
                match this.Transition |> box with
                | :? (Option<Transition<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId, 'NewEnv>>) as existing -> existing
                | _ -> None

            Def = this.Def
            Subscriptions = this.Subscriptions
            Timers = this.Timers
            Indices = this.Indices
            SingletonCtor = this.SingletonCtor
            AutoIgnoreNoOpTransitions = this.AutoIgnoreNoOpTransitions
            Storage = this.Storage
            MetaData = this.MetaData
            ResponseHandler = this.ResponseHandler
            ShouldSendTelemetry = this.ShouldSendTelemetry
            ShouldRecordHistory = this.ShouldRecordHistory
            LifeEventSatisfies = this.LifeEventSatisfies
        }

    member internal this.ToLifeCycle(): LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env> =
        match this.IdGeneration, this.Construction, this.Transition with
        | Some idGeneration, Some construction, Some transition ->
            {
                // Wrap these functions in some additional logic.
                IdGeneration =
                    let maxIdLength =
                        match this.Storage.Type with
                        | StorageType.Custom _
                        | StorageType.Persistent _ -> 80
                        | StorageType.Volatile _ -> 500
                    (fun (env: 'Env) ctor ->
                        Task.startOnThreadPool(fun () ->
                            backgroundTask {
                                let (IdGenerationResult idGenTask) = idGeneration env ctor
                                let! res = idGenTask
                                match res with
                                | Ok id ->
                                    let idStr = id.IdString
                                    if idStr.Length > maxIdLength then
                                        failwithf "Id cannot be longer than %d chars: %s" maxIdLength idStr
                                    elif System.String.IsNullOrWhiteSpace idStr then
                                        failwith "Id cannot be empty or whitespace"
                                | Error _ -> ()
                                return res
                            })
                        |> IdGenerationResult)
                Construction = (fun (env: 'Env) id ctor ->
                    Task.startOnThreadPool (
                        fun () ->
                            let (ConstructionResult ctorTask) = construction env id ctor
                            ctorTask)
                    |> ConstructionResult)
                AutoIgnoreNoOpTransitions = this.AutoIgnoreNoOpTransitions
                Transition = (fun (env: 'Env) subj action ->
                    Task.startOnThreadPool(
                        fun () ->
                            let (TransitionResult transitionTask) = transition env subj action
                            transitionTask)
                    |> TransitionResult)

                Definition = this.Def
                Subscriptions = this.Subscriptions
                Timers = this.Timers
                Indices = this.Indices
                SingletonCtor = this.SingletonCtor
                Storage = this.Storage
                MetaData = this.MetaData
                MaybeApiAccess = this.MaybeApiAccess
                ResponseHandler = this.ResponseHandler
                ShouldSendTelemetry = this.ShouldSendTelemetry
                ShouldRecordHistory = this.ShouldRecordHistory
                LifeEventSatisfies = this.LifeEventSatisfies
                SessionHandling = this.SessionHandling
            }
        | None, _, _ ->
            failwith $"Must provide ID generation logic via LifeCycleBuilder.withIdGeneration when building life cycle {this.Def.Key.LocalLifeCycleName}"
        | _, None, _ ->
            failwith $"Must provide construction logic via LifeCycleBuilder.withConstruction when building life cycle {this.Def.Key.LocalLifeCycleName}"
        | _, _, None ->
            failwith $"Must provide transition logic via LifeCycleBuilder.withTransition when building life cycle {this.Def.Key.LocalLifeCycleName}"

[<RequireQualifiedAccess>]
module LifeCycleBuilder =
    let newLifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'Session, 'Role
                    when 'Subject              :> Subject<'SubjectId>
                    and  'LifeAction           :> LifeAction
                    and  'OpError              :> OpError
                    and  'Constructor          :> Constructor
                    and  'LifeEvent            :> LifeEvent
                    and  'LifeEvent            :  comparison
                    and  'SubjectIndex         :> SubjectIndex<'OpError>
                    and  'SubjectId            :> SubjectId
                    and  'SubjectId            :  comparison
                    and  'Role                 :  comparison>
            (def: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            : LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, AccessPredicateInput, 'Session, 'Role, Env> =

        {
            // These are required when building.
            Def          = def
            IdGeneration        = None
            Construction        = None
            Transition          = None
            AutoIgnoreNoOpTransitions = true

            // The below have reasonable defaults.
            Subscriptions       = fun _ -> Map.empty
            Timers              = fun _ -> []
            SingletonCtor       = None
            Indices             = noIndices
            Storage             =
                {
                    Type = StorageType.Persistent (PromotedIndicesConfig.Empty, PersistentHistoryRetention.FullHistory)
                    MaxDedupCacheSize = 10us
                }
            MetaData            = { IndexKeys_ = Set.empty }
            MaybeApiAccess      = None
            ResponseHandler     = (fun _ -> Seq.empty)
            ShouldSendTelemetry = None
            ShouldRecordHistory = None
            LifeEventSatisfies  = None
            SessionHandling     = None
        }

    let withTransition
            transition
            (builder: LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'OldEnv>)
            : LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env> =
        { builder.AssumeTypes<'AccessPredicateInput, 'Role, 'Env>() with
            Transition = Some transition
        }

    let withIdGeneration
            idGeneration
            (builder: LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'OldEnv>)
            : LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env> =
        { builder.AssumeTypes<'AccessPredicateInput, 'Role, 'Env>() with
            IdGeneration = Some idGeneration
        }

    let withConstruction
            construction
            (builder: LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'OldEnv>)
            : LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env> =
        { builder.AssumeTypes<'AccessPredicateInput, 'Role, 'Env>() with
            Construction = Some construction
        }

    let withSubscriptions
            subscriptions
            (builder: LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env>)
            : LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env> =
        { builder with
            Subscriptions = subscriptions
        }

    let withTimers
            timers
            (builder: LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env>)
            : LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env> =
        { builder with
            Timers = timers
        }

    let withSingleton
            singletonConstructor
            (builder: LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env>)
            : LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env> =
        { builder with
            SingletonCtor = Some singletonConstructor
        }

    let withoutApiAccess
            (builder: LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env>)
            : LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env> =
        { builder with
            MaybeApiAccess = None
        }

    // TODO: more flexible Api access builders to fix combinatorial explosion of parameters (access rules & rate limits)

    let withApiAccessRestrictedByRulesAndRateLimits
            (accessRules: List<AccessRule<AccessPredicateInput, 'Role, 'LifeAction, 'Constructor>>)
            (rateLimits: LifeCycleRateLimitPredicate<'LifeAction, 'Constructor>)
            (builder: LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'OldAccessPredicateInput, 'Session, 'Role, 'Env>)
            : LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, AccessPredicateInput, 'Session, 'Role, 'Env> =
        { builder.AssumeTypes<AccessPredicateInput, 'Role, 'Env>() with
            MaybeApiAccess =
                Some {
                    AccessRules = accessRules
                    AccessPredicate = (fun _ _ _ _ -> true)
                    RateLimitsPredicate = rateLimits
                    AnonymousCanReadTotalCount = false
                }
        }

    let withApiAccessRestrictedByRules
            (accessRules: List<AccessRule<AccessPredicateInput, 'Role, 'LifeAction, 'Constructor>>)
            (builder: LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'OldAccessPredicateInput, 'Session, 'Role, 'Env>)
            : LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, AccessPredicateInput, 'Session, 'Role, 'Env> =
        withApiAccessRestrictedByRulesAndRateLimits accessRules (fun _ -> None) builder

    let withApiAccessRestrictedByRulesPredicateAndRateLimits
            (accessRules: List<AccessRule<'AccessPredicateInput, 'Role, 'LifeAction, 'Constructor>>)
            (accessPredicate: 'AccessPredicateInput -> AccessEvent<'Subject, 'LifeAction, 'Constructor, 'SubjectId> -> ExternalCallOrigin -> Option<'Session> -> bool)
            (rateLimits: LifeCycleRateLimitPredicate<'LifeAction, 'Constructor>)
            (builder: LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'OldAccessPredicateInput, 'Session, 'Role, 'Env>)
            : LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env> =
        { builder.AssumeTypes<'AccessPredicateInput, 'Role, 'Env>() with
            MaybeApiAccess =
                Some {
                    AccessRules = accessRules
                    AccessPredicate = accessPredicate
                    RateLimitsPredicate = rateLimits
                    AnonymousCanReadTotalCount = false
            }
        }

    let withApiAccessRestrictedByRulesAndPredicate
            (accessRules: List<AccessRule<'AccessPredicateInput, 'Role, 'LifeAction, 'Constructor>>)
            (accessPredicate: 'AccessPredicateInput -> AccessEvent<'Subject, 'LifeAction, 'Constructor, 'SubjectId> -> ExternalCallOrigin -> Option<'Session> -> bool)
            (builder: LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'OldAccessPredicateInput, 'Session, 'Role, 'Env>)
            : LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env> =
        withApiAccessRestrictedByRulesPredicateAndRateLimits accessRules accessPredicate (fun _ -> None) builder

    let withApiAccessRestrictedToRootOnlyAndRateLimits
            (rateLimits: LifeCycleRateLimitPredicate<'LifeAction, 'Constructor>)
            (builder: LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env>)
            : LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, AccessPredicateInput, 'Session, 'Role, 'Env> =
        builder
        |> withApiAccessRestrictedByRulesAndRateLimits [] rateLimits

    let withApiAccessRestrictedToRootOnly
            (builder: LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env>)
            : LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, AccessPredicateInput, 'Session, 'Role, 'Env> =
        builder
        |> withApiAccessRestrictedByRules []

    let withIndices
            (indices: 'Subject -> seq<'SubjectIndex>)
            (builder: LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env>)
            : LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env> =
        // Need to expand this out instead of using the record { ... with } syntax as the index type is different
        // (and hence the record type itself will change)
        { builder with
            Indices             = indices
            MetaData            = { IndexKeys_ = 'SubjectIndex.IndexKeys } }

    let withLifeEventSatisfies
            (lifeEventSatisfies: LifeEventSatisfiesInput<'LifeEvent> -> bool)
            (builder: LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env>)
            : LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env> =
        { builder with
            LifeEventSatisfies = Some lifeEventSatisfies
        }

    let withStorageEx
            (storageTypeOverride: Option<StorageType>)
            (maxDedupCacheSizeOverride: Option<uint16>)
            (builder: LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env>)
            : LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env> =
        let storage, maxDedupCacheSize =
            match storageTypeOverride with
            | None ->
                builder.Storage.Type,
                maxDedupCacheSizeOverride
                |> Option.defaultValue builder.Storage.MaxDedupCacheSize
            | Some storageType ->
                storageType,
                maxDedupCacheSizeOverride
                |> Option.defaultWith (fun () ->
                    match storageType with
                    | StorageType.Persistent _ ->
                        // conservative dedup cache size. Persistent subject should not have to many congesting callers, also to avoid persisting too much
                        10us
                    | StorageType.Volatile ->
                        // large dedup cache, because high-traffic subjects that serve many concurrent callers are usually volatile
                        // consider making it configurable at LifeCycle level
                        1000us
                    | StorageType.Custom _ ->
                        // assume similar to Persistent
                        10us)

        match storage with
        | StorageType.Persistent (promotedIndicesConfig, _) ->
            match promotedIndicesConfig.SubjectNumericAndStringIndexTypes with
            | Some (subjectNumericIndexType, subjectStringIndexType)
                when
                    subjectStringIndexType <> 'SubjectIndex.SubjectStringIndexType ||
                    subjectNumericIndexType <> 'SubjectIndex.SubjectNumericIndexType ->
                failwith "Promoted indices config doesn't match subject index"
            | _ ->
                ()
        | StorageType.Volatile
        | StorageType.Custom _ ->
            ()

        { builder with
            Storage = { Type = storage; MaxDedupCacheSize = maxDedupCacheSize }
        }

    let withStorage
            storageType
            (builder: LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env>)
            : LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env> =
        builder
        |> withStorageEx (Some storageType) None

    let withResponseHandler
            responseHandler
            (builder: LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env>)
            : LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env> =
        { builder with
            ResponseHandler = responseHandler
        }

    let withTelemetryRules
            shouldSendTelemetry
            (builder: LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env>)
            : LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env> =
        { builder with
            ShouldSendTelemetry = Some shouldSendTelemetry
        }

    let withHistoryRules
            shouldRecordHistory
            (builder: LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env>)
            : LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env> =
        { builder with
            ShouldRecordHistory = Some shouldRecordHistory
        }

    [<Experimental("This LifeCycleBuilder function is experimental can be removed without notice")>]
    let withAutoIgnoreNoOpTransitionsDisabled
            (builder: LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env>) =
        { builder with AutoIgnoreNoOpTransitions = false }

    let build
            (builder: LifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env>)
            : LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env> =

        let lifeCycle = builder.ToLifeCycle()

        // 'LifeEvent has several issues:
        // 1. it serves two purposes: actual published event, and also "predicate" passed by client to subscribe or observe event
        //      In perfect world client should pass a predicate ('LifeEvent -> bool) but it's not serializable.
        //      Possible workaround: one more generic parameter, e.g. 'LifeEventPredicateInput (similar to 'AccessPredicateInput)
        // 2. It's hard to come up with a type safe way to force developer implement "satisfies" logic, let's enforce it early here
        match builder.LifeEventSatisfies with
        | Some _ -> ()
        | None ->
            match
                FSharp.Reflection.FSharpType.GetUnionCases (typeof<'LifeEvent>, BindingFlags.NonPublic ||| BindingFlags.Public)
                |> List.ofArray
                with
            | [singleUnionCase] when singleUnionCase.Name = "NoLifeEvent" ->
                ()
            | _ ->
                failwithf $"Life Cycle %s{lifeCycle.Name} has non-default life event type, add %s{nameof withLifeEventSatisfies}"

        lifeCycle