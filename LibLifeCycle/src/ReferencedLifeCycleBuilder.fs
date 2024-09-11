namespace LibLifeCycle

type ReferencedLifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role
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
                and  'Role                 :  comparison> =
    internal {
        Def:             LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>
        MaybeApiAccess:  Option<LifeCycleApiAccess<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role>>
        ShouldSendTelemetry: Option<ShouldSendTelemetryFor<'LifeAction, 'Constructor> -> bool>
        ShouldRecordHistory: Option<ShouldRecordHistoryFor<'LifeAction, 'Constructor> -> bool>
        SessionHandling: Option<EcosystemSessionHandling<'Session, 'Role>>
    }
with
    member internal this.AssumeTypes<'NewAccessPredicateInput, 'NewRole
            when 'NewAccessPredicateInput :> AccessPredicateInput
            and  'NewRole                 :  comparison>()
            : ReferencedLifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'NewAccessPredicateInput, 'Session, 'NewRole> =
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

            ShouldSendTelemetry = this.ShouldSendTelemetry
            ShouldRecordHistory = this.ShouldRecordHistory
            Def = this.Def
        }

    member internal this.ToReferencedLifeCycle(): ReferencedLifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role> =
        {
            Def = this.Def
            MaybeApiAccess = this.MaybeApiAccess
            ShouldSendTelemetry = this.ShouldSendTelemetry
            ShouldRecordHistory = this.ShouldRecordHistory
            SessionHandling = this.SessionHandling
        }

[<RequireQualifiedAccess>]
module ReferencedLifeCycleBuilder =
    let newReferencedLifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'Session, 'Role
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
            : ReferencedLifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, AccessPredicateInput, 'Session, 'Role> =

        {
            Def = def
            MaybeApiAccess = None
            ShouldSendTelemetry = None
            ShouldRecordHistory = None
            SessionHandling = None
        }

    let withoutApiAccess
            (builder: ReferencedLifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role>)
            : ReferencedLifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role> =
        { builder with
            MaybeApiAccess = None
        }

    // TODO: more flexible Api access builders to fix combinatorial explosion of parameters (access rules & rate limits)

    let withApiAccessRestrictedByRulesAndRateLimits
            (accessRules: List<AccessRule<AccessPredicateInput, 'Role, 'LifeAction, 'Constructor>>)
            (rateLimits: LifeCycleRateLimitPredicate<'LifeAction, 'Constructor>)
            (builder: ReferencedLifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'OldAccessPredicateInput, 'Session, 'Role>)
            : ReferencedLifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, AccessPredicateInput, 'Session, 'Role> =
        { builder.AssumeTypes<AccessPredicateInput, 'Role>() with
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
            (builder: ReferencedLifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'OldAccessPredicateInput, 'Session, 'Role>)
            : ReferencedLifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, AccessPredicateInput, 'Session, 'Role> =
        withApiAccessRestrictedByRulesAndRateLimits accessRules (fun _ -> None) builder

    let withApiAccessRestrictedByRulesPredicateAndRateLimits
            (accessRules: List<AccessRule<'AccessPredicateInput, 'Role, 'LifeAction, 'Constructor>>)
            (accessPredicate: 'AccessPredicateInput -> AccessEvent<'Subject, 'LifeAction, 'Constructor, 'SubjectId> -> ExternalCallOrigin -> Option<'Session> -> bool)
            (rateLimits: LifeCycleRateLimitPredicate<'LifeAction, 'Constructor>)
            (builder: ReferencedLifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'OldAccessPredicateInput, 'Session, 'Role>)
            : ReferencedLifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role> =
        { builder.AssumeTypes<'AccessPredicateInput, 'Role>() with
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
            (builder: ReferencedLifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'OldAccessPredicateInput, 'Session, 'Role>)
            : ReferencedLifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role> =
        withApiAccessRestrictedByRulesPredicateAndRateLimits accessRules accessPredicate (fun _ -> None) builder

    let withApiAccessRestrictedToRootOnlyAndRateLimits
            (rateLimits: LifeCycleRateLimitPredicate<'LifeAction, 'Constructor>)
            (builder: ReferencedLifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role>)
            : ReferencedLifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, AccessPredicateInput, 'Session, 'Role> =
        builder
        |> withApiAccessRestrictedByRulesAndRateLimits [] rateLimits

    let withApiAccessRestrictedToRootOnly
            (builder: ReferencedLifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role>)
            : ReferencedLifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, AccessPredicateInput, 'Session, 'Role> =
        withApiAccessRestrictedToRootOnlyAndRateLimits (fun _ -> None) builder

    let withTelemetryRules
            shouldSendTelemetry
            (builder: ReferencedLifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'Session, 'Role, 'Env>)
            : ReferencedLifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'Session, 'Role, 'Env> =
        { builder with ShouldSendTelemetry = Some shouldSendTelemetry }

    let build
            (builder: ReferencedLifeCycleBuilder<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role>)
            : ReferencedLifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role> =
        builder.ToReferencedLifeCycle()