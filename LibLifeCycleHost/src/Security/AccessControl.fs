module LibLifeCycleHost.AccessControl

open System.Threading.Tasks
open Orleans

open LibLifeCycle
open LibLifeCycleCore
open LibLifeCycleHost.Web
open System

type SessionInfo<'Session, 'Role
        when 'Role : comparison> = {
    Session: Option<'Session>
    UserId:  AccessUserId
    Roles:   Set<'Role>
}

[<RequireQualifiedAccess>]
type private AuditorIndentType =
| EmptySpace
| ListItem of IsOnFirstLine: bool

type Auditor() =
    let audit = System.Text.StringBuilder()
    let mutable indent = 0
    let mutable isLineStart = true

    member this.Indent() =
        indent <- indent + 1
        this

    member this.Outdent() =
        if indent = 0 then
            failwith "Attempting outdent when no indent"

        indent <- indent - 1
        this

    member private this.WriteIndent() =
        audit.Append(' ', indent * 3)
        |> ignore

        this

    member this.BeginItem() =
        this.WriteIndent()
        |> ignore

        audit.Append(" - ")
        |> ignore
        isLineStart <- false
        this.Indent()

    member this.EndItem() =
        this.Outdent()

    member private this.WriteIndentIfLineStart() =
        if isLineStart then
            this.WriteIndent()
        else
            this

    member this.Write(value: string) =
        this.WriteIndentIfLineStart() |> ignore
        audit.Append(value) |> ignore
        isLineStart <- String.IsNullOrEmpty(value)
        this

    member this.WriteLine() =
        audit.AppendLine() |> ignore
        isLineStart <- true
        this

    member this.WriteLine(value: string) =
        this.WriteIndentIfLineStart() |> ignore
        audit.AppendLine(value) |> ignore
        isLineStart <- true
        this

    override _.ToString() =
        audit.ToString()

[<AutoOpen>]
module private AuditHelpers =
    type Auditor with
        member this.Audit(externalCallOrigin: ExternalCallOrigin) : Auditor =
            this
                .Write("Remote address: ")
                .WriteLine(externalCallOrigin.RemoteAddress)

        member this.Audit(callOrigin: CallOrigin) : Auditor =
            match callOrigin with
            | CallOrigin.Internal ->
                this.WriteLine("Internal")
            | CallOrigin.External externalCallOrigin ->
                this
                    .WriteLine()
                    .Indent()
                    .WriteLine("External:")
                    .Indent()
                    .Audit(externalCallOrigin)
                    .Outdent()
                    .Outdent()

        member this.Audit(subjectProjection: SubjectProjection): Auditor =
            match subjectProjection with
            | SubjectProjection.OriginalProjection ->
                this.WriteLine("(unprojected)")
            | SubjectProjection.Projection projectionName ->
                this.WriteLine(projectionName)

        member this.Audit<'Subject, 'LifeAction, 'Constructor, 'SubjectId
                when 'Subject     :> Subject<'SubjectId>
                and  'SubjectId   :> SubjectId
                and 'SubjectId : comparison
                and  'LifeAction  :> LifeAction
                and  'Constructor :> Constructor>
                (
                    accessEvent: AccessEvent<'Subject, 'LifeAction, 'Constructor, 'SubjectId>
                ): Auditor =
            let accessEventType =
                match accessEvent with
                | AccessEvent.Read _ -> "Read"
                | AccessEvent.ReadBlob _ -> "Read blob"
                | AccessEvent.ReadHistory _ -> "Read history"
                | AccessEvent.Act _ -> "Act"
                | AccessEvent.Construct _ -> "Construct"

            this
                .WriteLine()
                .Indent()
                .Write("Type: ")
                .WriteLine(accessEventType)
            |> ignore

            match accessEvent with
            | AccessEvent.Read (subject, subjectProjection) ->
                this
                    .Write("Subject ID: ")
                    .WriteLine(subject.SubjectId.IdString)
                    .Write("Projection: ")
                    .Audit(subjectProjection)
            | AccessEvent.ReadBlob (subject, blobGuid) ->
                this
                    .Write("Subject ID: ")
                    .WriteLine(subject.SubjectId.IdString)
                    .Write("Blob ID: ")
                    .WriteLine(string blobGuid)
            | AccessEvent.ReadHistory subject ->
                this
                    .Write("Subject ID: ")
                    .WriteLine(subject.SubjectId.IdString)
            | AccessEvent.Act (subject, lifeAction) ->
                this
                    .Write("Subject ID: ")
                    .WriteLine(subject.SubjectId.IdString)
                    .Write("Action: ")
                    .WriteLine(string lifeAction)
            | AccessEvent.Construct constructor ->
                this
                    .Write("Constructor: ")
                    .WriteLine(string constructor)
            |> ignore

            this.Outdent()

        member this.Audit(accessControlled: AccessControlled<'Input, 'InputId>): Auditor =
            match accessControlled with
            | AccessControlled.Denied _id ->
                this.WriteLine("DENIED")
            | AccessControlled.Granted _input ->
                this.WriteLine("GRANTED")

        member this.Audit(option: Option<'T>, auditInner: 'T -> Auditor): Auditor =
            match option with
            | Some t ->
                auditInner t
            | None ->
                this
                    .WriteLine("(none)")

        member this.Audit(accessMatch: AccessMatch<'T>, auditInner: 'T -> Auditor): Auditor =
            match accessMatch with
            | AccessMatch.MatchAny ->
                this
                    .WriteLine("Match any")
            | AccessMatch.Match t ->
                this
                    .WriteLine()
                    .Indent()
                    .Write("Match: ")
                |> ignore

                auditInner t
                |> ignore

                this
                    .Outdent()

        member this.Audit(eventType: AccessEventType<'LifeAction, 'Constructor>): Auditor =
            match eventType with
            | AccessEventType.ConstructCase ctor ->
                this
                    .Write("Construct: ")
                    .WriteLine(string ctor)
            | AccessEventType.ActCase lifeAction ->
                this
                    .Write("Act: ")
                    .WriteLine(string lifeAction)
            | AccessEventType.Read subjectProjection ->
                this
                    .WriteLine("Read:")
                    .Indent()
                    .Write("Projection: ")
                    .Audit(subjectProjection)
                    .Outdent()
            | AccessEventType.ReadBlob ->
                this
                    .WriteLine("Read blob")
            | AccessEventType.ReadHistory ->
                this
                    .WriteLine("Read history")

        member this.Audit(eventTypes: NonemptySet<AccessEventType<'LifeAction, 'Constructor>>): Auditor =
            this
                .WriteLine()
            |> ignore

            eventTypes
            |> NonemptySet.toSeq
            |> Seq.iter (fun eventType ->
                this
                    .BeginItem()
                    .Audit(eventType)
                    .EndItem()
                |> ignore
            )

            this

        member this.Audit(roles: NonemptySet<'Role>): Auditor =
            this
                .WriteLine()
            |> ignore

            roles
            |> NonemptySet.toSeq
            |> Seq.iter (fun role ->
                this
                    .BeginItem()
                    .WriteLine(string role)
                    .EndItem()
                |> ignore
            )

            this

        member this.Audit(accessDecision: AccessDecision): Auditor =
            match accessDecision with
            | Grant -> this.WriteLine("Grant")
            | Deny -> this.WriteLine("Deny")

        member this.Audit(accessRule: AccessRule<'AccessPredicateInput, 'Role, 'LifeAction, 'Constructor>): Auditor =
            this
                .Write("Input: ")
                .Audit(accessRule.Input, fun i -> this.WriteLine(string i))
                .Write("Event types: ")
                .Audit(accessRule.EventTypes, fun eventType -> this.Audit(eventType))
                .Write("Roles: ")
                .Audit(accessRule.Roles, fun role -> this.Audit(role))
                .Write("Decision: ")
                .Audit(accessRule.Decision)

        member this.Audit(apiAccess: LifeCycleApiAccess<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role>): Auditor =
            this
                .WriteLine()
                .Indent()
                .Write("Anonymous can read total count: ")
                .WriteLine(string apiAccess.AnonymousCanReadTotalCount)
                .WriteLine("Access rules:")
            |> ignore

            apiAccess.AccessRules
            |> List.iter (fun accessRule ->
                this
                    .BeginItem()
                    .Audit(accessRule)
                    .EndItem()
                |> ignore
            )

            this
                .Outdent()

        member this.Audit(sessionInfo: SessionInfo<'Session, 'Role>): Auditor =
            this
                .WriteLine()
                .Indent()
                .Write("User ID: ")
                .WriteLine(string sessionInfo.UserId)
                .Write("Session: ")
                .Audit(sessionInfo.Session, fun s -> this.WriteLine(s |> string |> (fun v -> v.ReplaceLineEndings(" "))))
                .WriteLine("Roles: ")
            |> ignore

            sessionInfo.Roles
            |> Seq.iter (fun role ->
                this
                    .BeginItem()
                    .WriteLine(string role)
                    .EndItem()
                |> ignore
            )

            this

[<RequireQualifiedAccess>]
type SessionSource =
| Handle   of SessionHandle
| InMemory of Option<obj>

[<RequireQualifiedAccess>]
type InternalRevalidateError =
| SubjectNotInitialized
| TransitionError
| TransitionNotAllowed
| LockedInTransaction
| AccessDenied
| Timeout
| Exception of exn

[<RequireQualifiedAccess>]
type ExternalRevalidateError =
| RevalidationFailed

[<RequireQualifiedAccess>]
type RevalidateError =
| Internal of InternalRevalidateError
| External of ExternalRevalidateError

[<AutoOpen>]
module private Session =
    // HACK: we can't defer this responsibility to the ecosystem because the ecosystem code cannot depend on the life cycle host. But in this context we don't have all the
    // types necessary to load the session, so we need to use reflection to do so.
    let loadSessionGenericWithoutVerify
            (_sessionLifeCycleDef: LifeCycleDef<'Session, 'SessionLifeAction, 'SessionOpError, 'SessionConstructor, 'SessionLifeEvent, 'SessionIndex, 'SessionSubjectId>)
            (hostEcosystemGrainFactory: IGrainFactory)
            (grainPartition: GrainPartition)
            (sessionIdStr: string):
            Task<Option<'Session>> =
        backgroundTask {
            let (GrainPartition partitionGuid) = grainPartition
            let sessionGrain = hostEcosystemGrainFactory.GetGrain<ISubjectClientGrain<'Session, 'SessionLifeAction, 'SessionOpError, 'SessionConstructor, 'SessionLifeEvent, 'SessionSubjectId>>(partitionGuid, sessionIdStr)
            let! currentSession = sessionGrain.Get { SessionHandle = SessionHandle.NoSession; CallOrigin = CallOrigin.Internal }
            let maybeVersionedSession =
                match currentSession with
                | Ok maybeVersionedSubject -> maybeVersionedSubject
                | Error GrainGetError.AccessDenied -> failwith "Unexpected access denial"

            return maybeVersionedSession |> Option.map (fun x -> x.Subject)
        }

    let loadSession
            (sessionHandler: EcosystemSessionHandler<'Session>)
            (hostEcosystemGrainFactory: IGrainFactory)
            (grainPartition: GrainPartition)
            (sessionIdStr: string)
            (expectedUserId: AccessUserId)
            : Task<Option<'Session>> =
        sessionHandler.LifeCycle.Invoke
            { new FullyTypedLifeCycleFunction<_> with
                member _.Invoke sessionLifeCycle =
                    backgroundTask {
                        let! maybeSessionSubj = loadSessionGenericWithoutVerify sessionLifeCycle.Definition hostEcosystemGrainFactory grainPartition sessionIdStr

                        let maybeVerifiedSession =
                            maybeSessionSubj
                            |> Option.map (fun s -> (box s) :?> 'Session)
                            |> Option.bind (fun session ->
                                let actualUserId = sessionHandler.GetUserId session
                                // assert loaded session matches SessionHandle's UserId. return None so access will be denied were a session is required
                                if actualUserId <> expectedUserId then
                                    None
                                else
                                    Some session)

                        return maybeVerifiedSession
                    } }

    let getSessionInfo
            (maybeSession: Option<'Session>)
            (sessionHandling: EcosystemSessionHandling<'Session, 'Role>)
            (externalCallOrigin: ExternalCallOrigin): SessionInfo<'Session, 'Role> =
        match maybeSession with
        | Some session ->
            {
                Session = Some session
                UserId = sessionHandling.Handler.GetUserId session
                Roles = sessionHandling.GetRoles externalCallOrigin (Some session)
            }
        | None ->
            {
                Session = None
                UserId = Anonymous
                Roles = sessionHandling.GetRoles externalCallOrigin None
            }

module Sessions =
    let private revalidateSessionGeneric
            (session: 'Session)
            (sessionHandler: EcosystemSessionHandler<'Session>)
            (hostEcosystemGrainFactory: IGrainFactory)
            (sessionLifeCycleDef: LifeCycleDef<'SessionSubject, 'SessionLifeAction, 'SessionOpError, 'SessionConstructor, 'SessionLifeEvent, _, 'SessionSubjectId>)
            (grainPartition: GrainPartition)
            : Task<Result<unit, RevalidateError>> =
        backgroundTask {
            try
                match sessionHandler.MaybeRevalidator with
                | Some revalidator ->
                    // We're relying on the session's life cycle to properly handle timeouts when integrating with a connector, which is
                    // why this is high.
                    let timeout = TimeSpan.FromMinutes(2.)
                    let sessionIdStr = sessionHandler.GetIdStr session

                    let grainConnector = GrainConnector(hostEcosystemGrainFactory, grainPartition, SessionHandle.NoSession, CallOrigin.Internal)
                    let! actAndWaitResult =
                        grainConnector.ActAndWait
                            sessionLifeCycleDef
                            sessionIdStr
                            (revalidator.RevalidateAction :?> 'SessionLifeAction)
                            (revalidator.RevalidateCompleteEvent :?> 'SessionLifeEvent)
                            timeout

                    match actAndWaitResult with
                    | Ok result ->
                        match result with
                        | ActOrConstructAndWaitOnLifeEventResult.LifeEventTriggered (_, event) ->
                            let revalidateCompleteResult = revalidator.GetRevalidateCompleteResult (event :> LifeEvent)

                            match revalidateCompleteResult with
                            | RevalidateCompleteResult.Success ->
                                return
                                    ()
                                    |> Ok
                            | RevalidateCompleteResult.Failure ->
                                // Revalidation failed externally
                                return
                                    ExternalRevalidateError.RevalidationFailed
                                    |> RevalidateError.External
                                    |> Error
                        | ActOrConstructAndWaitOnLifeEventResult.WaitOnLifeEventTimedOut _ ->
                            return
                                InternalRevalidateError.Timeout
                                |> RevalidateError.Internal
                                |> Error
                    | Error transitionError ->
                        // Revalidation failed internally (i.e. something wrong in our session life cycle).
                        return
                            match transitionError with
                            | GrainTransitionError.SubjectNotInitialized _ -> InternalRevalidateError.SubjectNotInitialized
                            | GrainTransitionError.TransitionError _ -> InternalRevalidateError.TransitionError
                            | GrainTransitionError.TransitionNotAllowed -> InternalRevalidateError.TransitionNotAllowed
                            | GrainTransitionError.LockedInTransaction -> InternalRevalidateError.LockedInTransaction
                            | GrainTransitionError.AccessDenied -> InternalRevalidateError.AccessDenied
                            |> RevalidateError.Internal
                            |> Error

                | _ ->
                    // There is no session revalidator.
                    return
                        ()
                        |> Ok
            with
            | ex ->
                return
                    ex
                    |> InternalRevalidateError.Exception
                    |> RevalidateError.Internal
                    |> Error
        }

    let revalidateSession<'Session>
            (session: 'Session)
            (sessionHandler: EcosystemSessionHandler<'Session>)
            (_now: DateTimeOffset)
            (hostEcosystemGrainFactory: IGrainFactory)
            (grainPartition: GrainPartition): Task<Result<unit, RevalidateError>> =
        sessionHandler.LifeCycle.Invoke
            { new FullyTypedLifeCycleFunction<_> with
                member _.Invoke sessionLifeCycle =
                    revalidateSessionGeneric session sessionHandler hostEcosystemGrainFactory sessionLifeCycle.Definition grainPartition }

    let resolveSession
            (sessionSource: SessionSource)
            (sessionHandler: EcosystemSessionHandler<'Session>)
            (hostEcosystemGrainFactory: IGrainFactory)
            (grainPartition: GrainPartition): Task<Option<'Session>> =
        match sessionSource with
        | SessionSource.Handle handle ->
            match handle with
            | SessionHandle.NoSession ->
                None |> Task.FromResult
            | SessionHandle.Session (sessionIdStr, userId) ->
                backgroundTask {
                    let! maybeSession = loadSession sessionHandler hostEcosystemGrainFactory grainPartition sessionIdStr userId
                    return maybeSession
                }
        | SessionSource.InMemory maybeSession ->
            maybeSession |> Option.map (fun o -> o :?> 'Session) |> Task.FromResult

module Subjects =
    let private doesSubjectAccessRuleMatch
            (sessionInfo: SessionInfo<'Session, 'Role>)
            (accessEvent: AccessEvent<'Subject, 'LifeAction, 'Constructor, 'SubjectId>)
            (accessRule: AccessRule<'AccessPredicateInput, 'Role, 'LifeAction, 'Constructor>)
            (accessPredicate: 'AccessPredicateInput -> AccessEvent<'Subject, 'LifeAction, 'Constructor, 'SubjectId> -> ExternalCallOrigin -> Option<'Session> -> bool)
            (externalCallOrigin: ExternalCallOrigin)
            (maybeAuditor: Option<Auditor>): bool =
        let rolesIsMatch =
            match accessRule.Roles with
            | MatchAny ->
                true
            | Match accessRuleRoles ->
                accessRuleRoles.ToSet
                |> Set.intersect sessionInfo.Roles
                |> Set.isEmpty
                |> not
        let accessEventTypeIsMatch =
            match accessRule.EventTypes with
            | MatchAny -> true
            | Match accessEvents ->
                accessEvents.ToSet
                |> Set.contains accessEvent.Type
        let inputIsMatch =
            match accessRule.Input with
            | MatchAny -> true
            | Match input ->
                // If an access rule has an input, the access predicate must determine the match.
                accessPredicate input accessEvent externalCallOrigin sessionInfo.Session

        rolesIsMatch && accessEventTypeIsMatch && inputIsMatch

    let private getSubjectAccessDecision
            (maybeApiAccess: Option<LifeCycleApiAccess<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role>>)
            (sessionInfo: SessionInfo<'Session, 'Role>)
            (accessEvent: AccessEvent<'Subject, 'LifeAction, 'Constructor, 'SubjectId>)
            (externalCallOrigin: ExternalCallOrigin)
            (maybeAuditor: Option<Auditor>) =
        match maybeApiAccess with
        | Some apiAccess ->
            maybeAuditor
            |> Option.iter (fun auditor ->
                auditor
                    .Write("API access:")
                    .Audit(apiAccess)
                |> ignore
            )

            let accessRules = apiAccess.AccessRules
            let maybeSession = sessionInfo.Session

            // Find the first matching access rule and honor its decision.
            let firstMatchingAccessRule =
                accessRules
                |> Seq.ofList
                |> Seq.filter (fun accessRule -> doesSubjectAccessRuleMatch sessionInfo accessEvent accessRule apiAccess.AccessPredicate externalCallOrigin maybeAuditor)
                |> Seq.filter (
                    fun accessRule ->
                        match accessRule.Input with
                        | MatchAny -> true
                        | Match input -> apiAccess.AccessPredicate input accessEvent externalCallOrigin maybeSession
                    )
                |> Seq.tryHead

            match firstMatchingAccessRule with
            | None -> Deny
            | Some accessRule -> accessRule.Decision
        | None ->
            maybeAuditor
            |> Option.iter (fun auditor ->
                auditor
                    .WriteLine("API access: None")
                |> ignore
            )

            Deny

    [<RequireQualifiedAccess>]
    type private SubjectAccessKind =
    | External of AccessUserId
    | Internal

    let private performAccessControlGeneric
            (hostEcosystemGrainFactory: IGrainFactory)
            (grainPartition: GrainPartition)
            (maybeApiAccess: Option<LifeCycleApiAccess<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role>>)
            (maybeSessionHandling: Option<EcosystemSessionHandling<'Session, 'Role>>)
            (sessionSource: SessionSource)
            (callOrigin: CallOrigin)
            // workaround of weird F# compiler bug that leads to BadImageFormatException.  Search for "BadImageFormatException" to see other instances
            // this used to be higher-order function getSubjectAccessResolver returning another function, which then crashed during JIT compile at usage site.
            (getInputId: 'Input -> 'InputId)
            (subjectAccessEvents: List<'Input * AccessEvent<'Subject, 'LifeAction, 'Constructor, 'SubjectId>>)
            (maybeAuditor: Option<Auditor>)
            : Task<List<AccessControlled<'Input, 'InputId>> * (* grantTotalCount *) bool * SubjectAccessKind> =

        maybeAuditor
        |> Option.iter (fun auditor ->
            auditor
                .WriteLine("ACL check:")
                .Indent()
                .Write("Subject type: ")
                .WriteLine(typeof<'Subject>.FullName)
                .Write("Call origin: ")
                .Audit(callOrigin)
            |> ignore

            // We don't outdent yet because we add to the info in the below logic
        )

        let resolveIt (maybeSession: Option<'Session>) (sessionHandling: EcosystemSessionHandling<'Session, 'Role>) (externalCallOrigin: ExternalCallOrigin) : list<AccessControlled<'Input, 'InputId>> * (* grantTotalCount *) bool * SubjectAccessKind =
            let sessionInfo = getSessionInfo maybeSession sessionHandling externalCallOrigin

            maybeAuditor
            |> Option.iter (fun auditor ->
                auditor
                    .Write("Session info:")
                    .Audit(sessionInfo)
                |> ignore
            )

            let grantTotalCount =
                match maybeApiAccess with
                | None -> false
                | Some apiAccess ->
                    match sessionInfo.UserId with
                    | Authenticated _ -> true
                    | Anonymous -> apiAccess.AnonymousCanReadTotalCount

            let accessControlledList =
                subjectAccessEvents
                |> List.map (
                    fun (input, accessEvent) ->
                        match getSubjectAccessDecision maybeApiAccess sessionInfo accessEvent externalCallOrigin maybeAuditor with
                        | Grant -> Granted input
                        | Deny  -> Denied (getInputId input))
            (accessControlledList, grantTotalCount, SubjectAccessKind.External sessionInfo.UserId)

        backgroundTask {
            let! (results, grantTotalCount, subjectAccessKind) =
                backgroundTask {
                    match callOrigin with
                    | CallOrigin.Internal ->
                        return (subjectAccessEvents |> List.map (fst >> Granted), (* grantTotalCount *) true, SubjectAccessKind.Internal)
                    | CallOrigin.External externalCallOrigin ->
                        match sessionSource with
                        | SessionSource.Handle sessionHandle ->
                            match sessionHandle, maybeSessionHandling with
                            | SessionHandle.NoSession, None
                            | SessionHandle.Session _, None ->
                                return (subjectAccessEvents |> List.map (fst >> Granted), (* grantTotalCount *) true, SubjectAccessKind.External Anonymous)
                            | SessionHandle.NoSession, Some sessionHandling ->
                                return resolveIt None sessionHandling externalCallOrigin
                            | SessionHandle.Session (sessionId, userId), Some sessionHandling ->
                                let! maybeSession = loadSession sessionHandling.Handler hostEcosystemGrainFactory grainPartition sessionId userId
                                return resolveIt maybeSession sessionHandling externalCallOrigin
                        | SessionSource.InMemory maybeSession ->
                            match maybeSession, maybeSessionHandling with
                            | None, None
                            | Some _, None ->
                                return (subjectAccessEvents |> List.map (fst >> Granted), (* grantTotalCount *) true, SubjectAccessKind.External Anonymous)
                            | None, Some sessionHandling ->
                                return resolveIt None sessionHandling externalCallOrigin
                            | Some session, Some sessionHandling ->
                                return resolveIt (Some (session :?> 'Session)) sessionHandling externalCallOrigin
                }

            maybeAuditor
            |> Option.iter (fun auditor ->
                // Writing access events after all the other info because it's easier to then correlate them with the outcomes below
                auditor
                    .WriteLine("Access events:")
                |> ignore

                subjectAccessEvents
                |> List.iter (fun (input, accessEvent) ->
                    auditor
                        .BeginItem()
                        .Write($"Input: ")
                        .WriteLine(string input)
                        .Write("Access event: ")
                        .Audit(accessEvent)
                        .EndItem()
                    |> ignore
                )

                auditor
                    .Outdent()
                    .WriteLine()
                    .WriteLine($"ACL check outcome (one per access event):")
                    .Indent()
                    .Write("Grant total count: ")
                    .WriteLine(string grantTotalCount)
                    .WriteLine("Results:")
                |> ignore

                results
                |> List.iter (fun accessControlled ->
                    auditor
                        .BeginItem()
                        .Audit(accessControlled)
                        .EndItem()
                    |> ignore
                )

                auditor
                    .Outdent()
                |> ignore
            )

            return (results, grantTotalCount, subjectAccessKind)
        }

    let authorizeSubjectAndContinueWith
            (hostEcosystemGrainFactory: IGrainFactory)
            (grainPartition: GrainPartition)
            (maybeApiAccess: Option<LifeCycleApiAccess<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role>>)
            (maybeSessionHandling: Option<EcosystemSessionHandling<'Session, 'Role>>)
            (sessionSource: SessionSource)
            (callOrigin: CallOrigin)
            (accessEvent: AccessEvent<'Subject, 'LifeAction, 'Constructor, 'SubjectId>)
            (unauthorizedContinuation: unit -> Task<'T>)
            (authorizedContinuation: unit -> Task<'T>)
            (maybeAuditor: Option<Auditor>): Task<'T> =
        task { // must be context-sensitive task because invoked from Orleans grain
            let! accessControlledList, _, accessKind =
                performAccessControlGeneric
                    hostEcosystemGrainFactory
                    grainPartition
                    maybeApiAccess
                    maybeSessionHandling
                    sessionSource
                    callOrigin
                    (fun _ -> 1)
                    [(1, accessEvent)]
                    maybeAuditor

            match accessKind with
            | SubjectAccessKind.External userId ->
                let telemetryUserId =
                    match userId with
                    | Authenticated (userId, _) -> userId
                    | Anonymous -> ""

                let telemetrySessionId =
                    match sessionSource with
                    | SessionSource.Handle sessionHandle ->
                        match sessionHandle with
                        | SessionHandle.NoSession ->
                            None
                        | SessionHandle.Session (sessionId, _) ->
                            Some sessionId
                    | SessionSource.InMemory maybeSessionObj ->
                        maybeSessionObj
                        |> Option.bind (fun sessionObj -> maybeSessionHandling |> Option.map (fun h -> h.Handler.GetIdStr (sessionObj :?> 'Session)))

                OrleansRequestContext.setTelemetryUserIdAndSessionId (telemetryUserId, telemetrySessionId)
            | SubjectAccessKind.Internal ->
                // don't overwrite user if subject accessed internally i.e. a nested invocation
                // or off the SideEffectProcessor
                // or via native Orleans client
                ()

            match accessControlledList with
            | [accessControlled] ->
                match accessControlled with
                | Denied _ -> return! unauthorizedContinuation ()
                | Granted _ -> return! authorizedContinuation ()
            | _ ->
                return failwith "unexpected"
        }

    let getAccessControlledSubjectForRead
            (hostEcosystemGrainFactory: IGrainFactory)
            (grainPartition: GrainPartition)
            (maybeApiAccess: Option<LifeCycleApiAccess<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role>>)
            (maybeSessionHandling: Option<EcosystemSessionHandling<'Session, 'Role>>)
            (sessionSource: SessionSource)
            (callOrigin: CallOrigin)
            (subject: 'Subject)
            (projection: SubjectProjection)
            (maybeAuditor: Option<Auditor>) =
        backgroundTask {
            let accessEvent = AccessEvent.Read(subject, projection)
            let! accessControlledList, _, _ =
                performAccessControlGeneric
                    hostEcosystemGrainFactory
                    grainPartition
                    maybeApiAccess
                    maybeSessionHandling
                    sessionSource
                    callOrigin
                    (fun (s: 'Subject) -> s.SubjectId)
                    [subject, accessEvent]
                    maybeAuditor
            return
                match accessControlledList with
                | [accessControlled] ->
                    // Second parameter is required to workaround https://github.com/dotnet/fsharp/issues/12761
                    // Where the compiler fails when backgroundTask+generics are involved, unless the generic type is also returned
                    // Also see below, where we use Task.map to drop it
                    accessControlled, (maybeApiAccess, maybeSessionHandling)
                | _ ->
                    failwith "unexpected"
        }
        |> Task.map fst // See comment above

    let getAccessControlledSubjectsForRead
            (hostEcosystemGrainFactory: IGrainFactory)
            (grainPartition: GrainPartition)
            (maybeApiAccess: Option<LifeCycleApiAccess<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role>>)
            (maybeSessionHandling: Option<EcosystemSessionHandling<'Session, 'Role>>)
            (sessionSource: SessionSource)
            (callOrigin: CallOrigin)
            (subjects: List<'Subject>)
            (totalCount: uint64)
            (projection: SubjectProjection)
            (maybeAuditor: Option<Auditor>) =
        backgroundTask {
            let! accessControlled, grantTotalCount, _ =
                performAccessControlGeneric
                    hostEcosystemGrainFactory
                    grainPartition
                    maybeApiAccess
                    maybeSessionHandling
                    sessionSource
                    callOrigin
                    (fun (s: 'Subject) -> s.SubjectId)
                    (subjects |> List.map (fun subject -> subject, AccessEvent.Read(subject, projection)))
                    maybeAuditor
            let accessControlledTotalCount = if grantTotalCount then totalCount else 0UL
            return accessControlled, accessControlledTotalCount
        }

    let getAccessControlledVersionedSubjectsForRead
            (hostEcosystemGrainFactory: IGrainFactory)
            (grainPartition: GrainPartition)
            (maybeApiAccess: Option<LifeCycleApiAccess<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role>>)
            (maybeSessionHandling: Option<EcosystemSessionHandling<'Session, 'Role>>)
            (sessionSource: SessionSource)
            (callOrigin: CallOrigin)
            (versionedSubjects: List<VersionedSubject<'Subject, 'SubjectId>>)
            (totalCount: uint64)
            (projection: SubjectProjection)
            (maybeAuditor: Option<Auditor>) =
        backgroundTask {
            let! accessControlled, grantTotalCount, _ =
                performAccessControlGeneric
                    hostEcosystemGrainFactory
                    grainPartition
                    maybeApiAccess
                    maybeSessionHandling
                    sessionSource
                    callOrigin
                    (fun (versionedSubject: VersionedSubject<'Subject, 'SubjectId>) -> versionedSubject.Subject.SubjectId)
                    (versionedSubjects |> List.map (fun versionedSubject -> versionedSubject, AccessEvent.Read(versionedSubject.Subject, projection)))
                    maybeAuditor
            let accessControlledTotalCount = if grantTotalCount then totalCount else 0UL
            return accessControlled, accessControlledTotalCount
        }

module Views =
    let private doesViewAccessRuleMatch
            (roles: Set<'Role>)
            (accessRule: AccessRule<'AccessPredicateInput, 'Role>) =
        let rolesIsMatch =
            match accessRule.Roles with
            | MatchAny -> true
            | Match accessRuleRoles ->
                accessRuleRoles.ToSet
                |> Set.intersect roles
                |> Set.isEmpty
                |> not

        rolesIsMatch

    let private getViewAccessDecision
            (view: View<'Input, 'Output, 'OpError, 'AccessPredicateInput, 'Session, 'Role, 'Env>)
            (sessionInfo: SessionInfo<'Session, 'Role>)
            (callOrigin: CallOrigin)
            (input: 'Input) =
        match view.MaybeApiAccess with
        | Some apiAccess ->
            let accessRules = apiAccess.AccessRules

            // Find the first matching access rule and honor its decision. If an access rule has an input, the life cycle's predicate must also agree on the match.
            let firstMatchingAccessRule =
                accessRules
                |> Seq.ofList
                |> Seq.filter (fun accessRule -> doesViewAccessRuleMatch sessionInfo.Roles accessRule)
                |> Seq.filter (
                    fun accessRule ->
                        match accessRule.Input with
                        | MatchAny -> true
                        | Match predicateInput -> apiAccess.AccessPredicate predicateInput input callOrigin sessionInfo.Session
                    )
                |> Seq.tryHead

            match firstMatchingAccessRule with
            | None -> Deny
            | Some accessRule -> accessRule.Decision
        | None ->
            Deny

    let authorizeViewAndContinueWith
            (hostEcosystemGrainProvider: IGrainFactory)
            (grainPartition: GrainPartition)
            (view: IView<'Input, 'Output, 'OpError>)
            (sessionSource: SessionSource)
            (callOrigin: CallOrigin)
            (input: 'Input)
            (unauthorizedResult: 'T)
            (authorizedContinuation: unit -> Task<'T>): Task<'T> =
        view.Invoke
            { new FullyTypedViewFunction<_, _, _, _> with
                member _.Invoke view =
                    let makeDecision (getSession: unit -> Task<Option<'Session>>) (sessionHandling: EcosystemSessionHandling<'Session, 'Role>) (externalCallOrigin: ExternalCallOrigin) : Task<AccessDecision> =
                        backgroundTask {
                            let! maybeSession = getSession ()
                            let sessionInfo = getSessionInfo maybeSession sessionHandling externalCallOrigin
                            let result = getViewAccessDecision view sessionInfo callOrigin input
                            return result
                        }

                    backgroundTask {
                        let! accessDecision =
                            match callOrigin with
                            | CallOrigin.Internal -> Grant |> Task.FromResult
                            | CallOrigin.External externalCallOrigin ->
                                match sessionSource with
                                | SessionSource.Handle sessionHandle ->
                                    match sessionHandle with
                                    | SessionHandle.NoSession ->
                                        match view.SessionHandling with
                                        | None -> Grant |> Task.FromResult
                                        | Some sessionHandling ->
                                            makeDecision (fun () -> None |> Task.FromResult) sessionHandling externalCallOrigin
                                    | SessionHandle.Session (sessionId, userId) ->
                                        match view.SessionHandling with
                                        | None -> Grant |> Task.FromResult
                                        | Some sessionHandling ->
                                            makeDecision (fun () -> loadSession sessionHandling.Handler hostEcosystemGrainProvider grainPartition sessionId userId) sessionHandling externalCallOrigin
                                | SessionSource.InMemory maybeSession ->
                                    match maybeSession, view.SessionHandling with
                                    | None, None
                                    | Some _, None ->
                                        Grant |> Task.FromResult
                                    | None, Some sessionHandling ->
                                        makeDecision (fun () -> None |> Task.FromResult) sessionHandling externalCallOrigin
                                    | Some _, Some sessionHandling ->
                                        makeDecision (fun () -> maybeSession |> Option.map (fun o -> o :?> 'Session) |> Task.FromResult) sessionHandling externalCallOrigin


                        match accessDecision with
                        | Deny -> return unauthorizedResult
                        | Grant -> return! authorizedContinuation ()
                    }
            }

// TODO: there's a lot of copy-paste from module Subjects here.  Can we make it DRY-er?
module TimeSeries =
    let private doesTimeSeriesAccessRuleMatch
            (sessionInfo: SessionInfo<'Session, 'Role>)
            (accessEvent: TimeSeriesAccessEvent<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure>)
            (accessRule: TimeSeriesAccessRule<'AccessPredicateInput, 'Role>)
            (accessPredicate: 'AccessPredicateInput -> TimeSeriesAccessEvent<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure> -> ExternalCallOrigin -> Option<'Session> -> bool)
            (externalCallOrigin: ExternalCallOrigin) =
        let rolesIsMatch =
            match accessRule.Roles with
            | MatchAny -> true
            | Match accessRuleRoles ->
                accessRuleRoles.ToSet
                |> Set.intersect sessionInfo.Roles
                |> Set.isEmpty
                |> not
        let accessEventTypeIsMatch =
            match accessRule.EventTypes with
            | MatchAny -> true
            | Match accessEvents ->
                accessEvents.ToSet
                |> Set.contains accessEvent.Type
        let inputIsMatch =
            match accessRule.Input with
            | MatchAny -> true
            | Match input ->
                // If an access rule has an input, the access predicate must determine the match.
                accessPredicate input accessEvent externalCallOrigin sessionInfo.Session

        rolesIsMatch && accessEventTypeIsMatch && inputIsMatch

    let private getTimeSeriesAccessDecision
            (maybeApiAccess: Option<TimeSeriesApiAccess<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'AccessPredicateInput, 'Session, 'Role>>)
            (sessionInfo: SessionInfo<'Session, 'Role>)
            (accessEvent: TimeSeriesAccessEvent<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure>)
            (externalCallOrigin: ExternalCallOrigin) =
        match maybeApiAccess with
        | Some apiAccess ->
            let accessRules = apiAccess.AccessRules
            let maybeSession = sessionInfo.Session

            // Find the first matching access rule and honor its decision.
            let firstMatchingAccessRule =
                accessRules
                |> Seq.ofList
                |> Seq.filter (fun accessRule -> doesTimeSeriesAccessRuleMatch sessionInfo accessEvent accessRule apiAccess.AccessPredicate externalCallOrigin)
                |> Seq.filter (
                    fun accessRule ->
                        match accessRule.Input with
                        | MatchAny -> true
                        | Match input -> apiAccess.AccessPredicate input accessEvent externalCallOrigin maybeSession
                    )
                |> Seq.tryHead

            match firstMatchingAccessRule with
            | None -> Deny
            | Some accessRule -> accessRule.Decision
        | None ->
            Deny

    [<RequireQualifiedAccess>]
    type private TimeSeriesAccessKind =
    | External of AccessUserId
    | Internal

    let private performAccessControlGeneric
            (hostEcosystemGrainFactory: IGrainFactory)
            (grainPartition: GrainPartition)
            (maybeApiAccess: Option<TimeSeriesApiAccess<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'AccessPredicateInput, 'Session, 'Role>>)
            (maybeSessionHandling: Option<EcosystemSessionHandling<'Session, 'Role>>)
            (sessionSource: SessionSource)
            (callOrigin: CallOrigin)
            // workaround of weird F# compiler bug that leads to BadImageFormatException.  Search for "BadImageFormatException" to see other instances
            // this used to be higher-order function getTimeSeriesAccessResolver returning another function, which then crashed during JIT compile at usage site.
            (getInputId: 'Input -> string)
            (timeSeriesAccessEvents: List<'Input * TimeSeriesAccessEvent<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure>>)
            : Task<List<AccessControlled<'Input, string>> * TimeSeriesAccessKind> =

        let resolveIt (maybeSession: Option<'Session>) (sessionHandling: EcosystemSessionHandling<'Session, 'Role>) (externalCallOrigin: ExternalCallOrigin) : list<AccessControlled<'Input, string>> * TimeSeriesAccessKind =
            let sessionInfo = getSessionInfo maybeSession sessionHandling externalCallOrigin

            let accessControlledList =
                timeSeriesAccessEvents
                |> List.map (
                    fun (input, accessEvent) ->
                        match getTimeSeriesAccessDecision maybeApiAccess sessionInfo accessEvent externalCallOrigin with
                        | Grant -> Granted input
                        | Deny  -> Denied (getInputId input))
            (accessControlledList, TimeSeriesAccessKind.External sessionInfo.UserId)

        backgroundTask {
            match callOrigin with
            | CallOrigin.Internal ->
                return (timeSeriesAccessEvents |> List.map (fst >> Granted), TimeSeriesAccessKind.Internal)
            | CallOrigin.External externalCallOrigin ->
                match sessionSource with
                | SessionSource.Handle sessionHandle ->
                    match sessionHandle, maybeSessionHandling with
                    | SessionHandle.NoSession, None
                    | SessionHandle.Session _, None ->
                        return (timeSeriesAccessEvents |> List.map (fst >> Granted), TimeSeriesAccessKind.External Anonymous)
                    | SessionHandle.NoSession, Some sessionHandling ->
                        return resolveIt None sessionHandling externalCallOrigin
                    | SessionHandle.Session (sessionId, userId), Some sessionHandling ->
                        let! maybeSession = loadSession sessionHandling.Handler hostEcosystemGrainFactory grainPartition sessionId userId
                        return resolveIt maybeSession sessionHandling externalCallOrigin
                | SessionSource.InMemory maybeSession ->
                    match maybeSession, maybeSessionHandling with
                    | None, None
                    | Some _, None ->
                        return (timeSeriesAccessEvents |> List.map (fst >> Granted), TimeSeriesAccessKind.External Anonymous)
                    | None, Some sessionHandling ->
                        return resolveIt None sessionHandling externalCallOrigin
                    | Some session, Some sessionHandling ->
                        return resolveIt (Some (session :?> 'Session)) sessionHandling externalCallOrigin
        }

    let authorizeTimeSeriesAndContinueWith
            (hostEcosystemGrainFactory: IGrainFactory)
            (grainPartition: GrainPartition)
            (maybeApiAccess: Option<TimeSeriesApiAccess<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'AccessPredicateInput, 'Session, 'Role>>)
            (maybeSessionHandling: Option<EcosystemSessionHandling<'Session, 'Role>>)
            (sessionSource: SessionSource)
            (callOrigin: CallOrigin)
            (accessEvent: TimeSeriesAccessEvent<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure>)
            (unauthorizedContinuation: unit -> Task<'T>)
            (authorizedContinuation: unit -> Task<'T>): Task<'T> =
        task { // must be context-sensitive task because invoked from Orleans grain
            let! accessControlledList, accessKind =
                performAccessControlGeneric
                    hostEcosystemGrainFactory
                    grainPartition
                    maybeApiAccess
                    maybeSessionHandling
                    sessionSource
                    callOrigin
                    (fun _ -> "1")
                    [(1, accessEvent)]

            match accessKind with
            | TimeSeriesAccessKind.External userId ->
                let telemetryUserId =
                    match userId with
                    | Authenticated (userId, _) -> userId
                    | Anonymous -> ""

                let telemetrySessionId =
                    match sessionSource with
                    | SessionSource.Handle sessionHandle ->
                        match sessionHandle with
                        | SessionHandle.NoSession ->
                            None
                        | SessionHandle.Session (sessionId, _) ->
                            Some sessionId
                    | SessionSource.InMemory maybeSessionObj ->
                        maybeSessionObj
                        |> Option.bind (fun sessionObj -> maybeSessionHandling |> Option.map (fun h -> h.Handler.GetIdStr (sessionObj :?> 'Session)))

                OrleansRequestContext.setTelemetryUserIdAndSessionId (telemetryUserId, telemetrySessionId)
            | TimeSeriesAccessKind.Internal ->
                // don't overwrite user if time series accessed internally i.e. a nested invocation
                // or off the SideEffectProcessor
                // or via native Orleans client
                ()

            match accessControlledList with
            | [accessControlled] ->
                match accessControlled with
                | Denied _ -> return! unauthorizedContinuation ()
                | Granted _ -> return! authorizedContinuation ()
            | _ ->
                return failwith "unexpected"
        }
