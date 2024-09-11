namespace LibLifeCycle

open System.Collections.Generic
open LibLifeCycle
open LibLifeCycle.LifeCycles.Meta
open LibLifeCycle.LifeCycles.Startup
open LibLifeCycle.LifeCycles.RequestRateCounter

[<AutoOpen>]
module private Helpers =
    open LibLifeCycle.LifeCycleAccessBuilder
    let withLifeCycleApiAccessOpenedToRoot
            (rootRoles: Set<'Role>)
            (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env>) =
        match NonemptySet.ofSet rootRoles with
        | None ->
            lifeCycle
        | Some rootRoles ->
            let apiAccess =
                match lifeCycle.MaybeApiAccess with
                | Some apiAccess ->
                    { apiAccess with
                        AccessRules = (grantAll |> toRoles rootRoles) :: apiAccess.AccessRules
                    }
                | None ->
                    let emptyAccessRules = List.empty<AccessRule<'AccessPredicateInput, 'Role, 'LifeAction, 'Constructor>>

                    {
                        AccessRules = (grantAll |> toRoles rootRoles) :: emptyAccessRules
                        AccessPredicate = (fun _ _ _ _ -> true)
                        RateLimitsPredicate = fun _ -> None
                        AnonymousCanReadTotalCount = false
                    }

            { lifeCycle with
                MaybeApiAccess = Some apiAccess
            }

    let withMaybeLifeCycleApiAccessOpenedToRoot
            (rootRoles: Set<'Role>)
            (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env>) =
        match lifeCycle.MaybeApiAccess, NonemptySet.ofSet rootRoles with
        | None, _
        | _, None ->
            lifeCycle
        | Some apiAccess, Some rootRoles ->
            { lifeCycle with
                MaybeApiAccess =
                    Some
                        { apiAccess with
                            AccessRules = (grantAll |> toRoles rootRoles) :: apiAccess.AccessRules
                        }
            }

    open LibLifeCycle.ViewAccessBuilder
    let withMaybeViewApiAccessOpenedToRoot
            (rootRoles: Set<'Role>)
            (view: View<'Input, 'Output, 'OpError, 'AccessPredicateInput, 'Session, 'Role, 'Env>) =
        match view.MaybeApiAccess,  NonemptySet.ofSet rootRoles with
        | None, _
        | _, None ->
            view
        | Some apiAccess, Some rootRoles ->
            { view with
                MaybeApiAccess =
                    Some
                        { apiAccess with
                            AccessRules = (grant |> toRoles rootRoles) :: apiAccess.AccessRules
                        }
            }

    open LibLifeCycle.TimeSeriesAccessBuilder
    let withMaybeTimeSeriesApiAccessOpenedToRoot
            (rootRoles: Set<'Role>)
            (timeSeries: TimeSeries<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex, 'AccessPredicateInput, 'Session, 'Role>) =
        match NonemptySet.ofSet rootRoles with
        | None ->
            timeSeries
        | Some rootRoles ->
            let apiAccess =
                match timeSeries.MaybeApiAccess with
                | Some apiAccess ->
                    { apiAccess with
                        AccessRules = (grantAll |> toRoles rootRoles) :: apiAccess.AccessRules
                    }
                | None ->
                    let emptyAccessRules = List.empty<TimeSeriesAccessRule<'AccessPredicateInput, 'Role>>
                    {
                        AccessRules = (grantAll |> toRoles rootRoles) :: emptyAccessRules
                        AccessPredicate = (fun _ _ _ _ -> true)
                    }

            { timeSeries with
                MaybeApiAccess = Some apiAccess }

    let configureCodecsOnStartup (lifeCycleDefs: list<LifeCycleDef>) =
        // this is ugly workaround to warmup interface codecs for the ecosystem before silo starts
        // slow codec cache warmup when silo is up affects SubjectHost stability
        // TODO  how to plug in codec cache for any codecs created directly via Codec() / get_Codec()?

        printfn "Compiling codecs... %A" System.DateTimeOffset.Now

        CodecLib.configureCodecLib()

        lifeCycleDefs
        |> List.groupBy (fun lc ->
            lc.Invoke
                { new FullyTypedLifeCycleDefFunction<_> with
                    member _.Invoke (lifeCycleDef: LifeCycleDef<_, _, _, _, _, _, 'SubjectId1>) = typeof<'SubjectId1> })
        |> List.map (snd >> List.head)
        |> List.map (fun lc ->
            System.Action (fun () ->
                lc.Invoke
                    { new FullyTypedLifeCycleDefFunction<_> with
                        member _.Invoke (lifeCycleDef: LifeCycleDef<_, _, _, _, _, _, 'SubjectId2>) =
                            CodecLib.defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<CodecLib.StjCodecs.StjEncoding, Subject<'SubjectId2>> |> ignore
                            1 // can't be unit, don't ask me why, ask F# compiler
                    }
                    |> ignore))
        |> List.append
            [
                System.Action(fun () -> CodecLib.defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<CodecLib.StjCodecs.StjEncoding, LifeAction>  |> ignore)
                System.Action(fun () -> CodecLib.defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<CodecLib.StjCodecs.StjEncoding, Constructor> |> ignore)
                System.Action(fun () -> CodecLib.defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<CodecLib.StjCodecs.StjEncoding, OpError>     |> ignore)
                System.Action(fun () -> CodecLib.defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<CodecLib.StjCodecs.StjEncoding, SubjectId>   |> ignore)
                System.Action(fun () -> CodecLib.defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<CodecLib.StjCodecs.StjEncoding, LifeEvent>   |> ignore)
            ]
        |> Seq.toArray
        |> // run in parallel to speed up JIT compilation
           System.Threading.Tasks.Parallel.Invoke

        printfn "Compiled codecs %A" System.DateTimeOffset.Now

type internal SessionHandlingWithRootRoles<'Session, 'Role
        when 'Role : comparison> =
    {
        Handling: EcosystemSessionHandling<'Session, 'Role>
        RootRoles: Set<'Role>
    }

type internal SessionInfo<'Session, 'Role
        when 'Role : comparison> =
    {
        MaybeSessionHandlingWithRootRoles: Option<SessionHandlingWithRootRoles<'Session, 'Role>>
        MaybeSessionLifeCycle: Option<Option<EcosystemSessionHandling<'Session, 'Role>> -> ILifeCycle>
        MetaLifeCycle: Option<EcosystemSessionHandling<'Session, 'Role>> -> MetaLifeCycle<'Session, 'Role>
    }

type EcosystemBuilder<'Session, 'SessionLifeAction, 'SessionLifeEvent, 'Role
        when 'SessionLifeAction :> LifeAction
        and 'SessionLifeEvent :> LifeEvent
        and  'Role : comparison> =
    internal {
        Def:                    EcosystemDef
        SessionInfo:            Option<SessionInfo<'Session, 'Role>>
        // All life cycles and views (including referenced ones) need to know the specifics of session handling,
        // but we can't know that is finalized until the ecosystem is built via EcosystemBuilder.build. Since we
        // don't have strong types for life cycles and views at that point, we can't assign the session handling
        // ourselves. Instead, we register both life cycles and views as callbacks that produce an updated value.
        LifeCycles:             Map<LifeCycleKey, Option<EcosystemSessionHandling<'Session, 'Role>> -> ILifeCycle>
        Views:                  Map<string, Option<EcosystemSessionHandling<'Session, 'Role>> -> IView>
        TimeSeries:             Map<TimeSeriesKey, Option<EcosystemSessionHandling<'Session, 'Role>> -> ITimeSeries>
        Connectors:             Map<string, Connector>
        SingletonConstructions: list<IExternalLifeCycleOperation>
        ReferencedEcosystems:   Map<string, Option<EcosystemSessionHandling<'Session, 'Role>> -> ReferencedEcosystem>
        EnforceRateLimits:      bool
    }
with
    member internal this.AssumeTypes<'NewSession, 'NewSessionLifeAction, 'NewSessionLifeEvent, 'NewRole
            when 'NewSessionLifeAction :> LifeAction
            and 'NewSessionLifeEvent :> LifeEvent
            and  'NewRole : comparison>()
            : EcosystemBuilder<'NewSession, 'NewSessionLifeAction, 'NewSessionLifeEvent, 'NewRole> =
        {
            // All these fields have generic types that can be "filled in" as the builder is used, so we make a best effort to
            // retain existing values if their types match. Otherwise, those values are dropped.
            SessionInfo =
                match this.SessionInfo |> box with
                | :? (Option<SessionInfo<'NewSession, 'NewRole>>) as existing -> existing
                | _ -> None
            LifeCycles =
                match this.LifeCycles |> box with
                | :? (Map<LifeCycleKey, Option<EcosystemSessionHandling<'NewSession, 'NewRole>> -> ILifeCycle>) as existing -> existing
                | _ -> Map.empty
            Views =
                match this.Views |> box with
                | :? (Map<string, Option<EcosystemSessionHandling<'NewSession, 'NewRole>> -> IView>) as existing -> existing
                | _ -> Map.empty
            TimeSeries =
                match this.TimeSeries |> box with
                | :? (Map<TimeSeriesKey, Option<EcosystemSessionHandling<'NewSession, 'NewRole>> -> ITimeSeries>) as existing -> existing
                | _ -> Map.empty
            ReferencedEcosystems =
                match this.ReferencedEcosystems |> box with
                | :? (Map<string, Option<EcosystemSessionHandling<'NewSession, 'NewRole>> -> ReferencedEcosystem>) as existing -> existing
                | _ -> Map.empty

            Def = this.Def
            Connectors = this.Connectors
            SingletonConstructions = this.SingletonConstructions
            EnforceRateLimits = this.EnforceRateLimits
        }

    member internal this.ToEcosystem(): Ecosystem =
        match this.SessionInfo with
        | Some sessionInfo ->
            // Now that we're building, we can lock in session handling.
            let sessionHandling =
                sessionInfo.MaybeSessionHandlingWithRootRoles
                |> Option.map (fun sessionHandlingWithRootRoles -> sessionHandlingWithRootRoles.Handling)

            let allLifeCycles =
                seq {
                    let metaLifeCycle = sessionInfo.MetaLifeCycle sessionHandling

                    yield
                        (
                            metaLifeCycle.Definition.Key,
                            { metaLifeCycle with SessionHandling = sessionHandling } :> ILifeCycle
                        )

                    let startupLifeCycle = startupLifeCycle metaLifeCycle this.Def this.SingletonConstructions

                    yield
                        (
                            startupLifeCycle.Definition.Key,
                            { startupLifeCycle with SessionHandling = sessionHandling } :> ILifeCycle
                        )

                    let requestRateCounterLifeCycle = requestRateCounterLifeCycle this.Def
                    yield
                        (
                            requestRateCounterLifeCycle.Definition.Key,
                            { requestRateCounterLifeCycle with SessionHandling = sessionHandling } :> ILifeCycle
                        )

                    let maybeSessionLifeCycle =
                        sessionInfo.MaybeSessionLifeCycle
                        |> Option.map (fun sessionLifeCycle -> sessionLifeCycle sessionHandling)

                    match maybeSessionLifeCycle with
                    | Some sessionLifeCycle ->
                        yield
                            (
                                sessionLifeCycle.Def.LifeCycleKey,
                                sessionLifeCycle
                            )
                    | None ->
                        Noop

                    yield!
                        this.LifeCycles
                        |> Map.map (fun _ lcc -> lcc sessionHandling)
                        |> Map.toSeq
                }
                |> Map.ofSeq

            let allViews =
                this.Views
                |> Map.map (fun _ v -> v sessionHandling)

            let allTimeSeries =
                this.TimeSeries
                |> Map.map (fun _ timeSeries -> timeSeries sessionHandling)

            let allReferencedEcosystems =
                this.ReferencedEcosystems
                |> Map.map (fun _ v -> v sessionHandling)

            // assert that all declared life cycles and views are present in the implementation
            // we reserve ability to implement but not expose some life cycles or views
            let notImplementedLifeCycleDefinitions =
                let requiredLifeCycleDefinitions = this.Def.LifeCycleDefs |> HashSet
                let implementedLifeCycleDefinitions =
                    allLifeCycles.Values
                    |> Seq.map (fun x -> x.Def)
                    |> HashSet
                requiredLifeCycleDefinitions.ExceptWith implementedLifeCycleDefinitions
                requiredLifeCycleDefinitions |> List.ofSeq
            if notImplementedLifeCycleDefinitions.IsNonempty then
                failwithf "Ecosystem lacks some life cycles required by EcosystemDef: %A" notImplementedLifeCycleDefinitions

            let notImplementedViewDefinitions =
                let requiredViewDefinitions = this.Def.ViewDefs |> Seq.map box |> HashSet
                let implementedViewDefinitions =
                    allViews.Values
                    |> Seq.map (fun x -> box x.Def)
                    |> HashSet
                requiredViewDefinitions.ExceptWith implementedViewDefinitions
                requiredViewDefinitions |> List.ofSeq
            if notImplementedViewDefinitions.IsNonempty then
                failwithf "Ecosystem lacks some views required by EcosystemDef: %A" notImplementedViewDefinitions

            let notImplementedTimeSeriesDefinitions =
                let requiredTimeSeriesDefinitions = this.Def.TimeSeriesDefs |> HashSet
                let implementedTimeSeriesDefinitions =
                    allTimeSeries.Values
                    |> Seq.map (fun x -> x.Def)
                    |> HashSet
                requiredTimeSeriesDefinitions.ExceptWith implementedTimeSeriesDefinitions
                requiredTimeSeriesDefinitions |> List.ofSeq
            if notImplementedTimeSeriesDefinitions.IsNonempty then
                failwithf "Ecosystem lacks some time series required by EcosystemDef: %A" notImplementedTimeSeriesDefinitions

            allReferencedEcosystems.Values
            |> Seq.collect (fun x -> x.LifeCycles |> Seq.map(fun lc -> lc.Def))
            |> Seq.append (allLifeCycles.Values |> Seq.map (fun lc -> lc.Def))
            |> Seq.toList
            |> configureCodecsOnStartup

            let result: Ecosystem = {
                Name                 = this.Def.Name
                LifeCycles           = allLifeCycles
                Views                = allViews
                TimeSeries           = allTimeSeries
                Connectors           = this.Connectors
                ReferencedEcosystems = allReferencedEcosystems
                EnforceRateLimits    = this.EnforceRateLimits
                Def                  = this.Def
            }
            result
        | None ->
            failwith $"Must call either LifeCycleBuilder.withNoSessionHandler or LifeCycleBuilder.withSession when building ecosystem {this.Def.Name}"

type SessionRevalidator<'Session, 'SessionLifeAction, 'SessionLifeEvent
                                when 'SessionLifeAction :> LifeAction
                                and 'SessionLifeEvent :> LifeEvent> = {
    GetRevalidateOn:             'Session -> Option<System.DateTimeOffset>
    RevalidateAction:            'SessionLifeAction
    RevalidateCompleteEvent:     'SessionLifeEvent
    GetRevalidateCompleteResult: 'SessionLifeEvent -> RevalidateCompleteResult
}

[<RequireQualifiedAccess>]
module EcosystemBuilder =
    let newEcosystem
            (def: EcosystemDef)
            : EcosystemBuilder<NoSession, NoSessionAction, NoSessionLifeEvent, NoRole> =
        {
            Def = def
            SessionInfo = None
            LifeCycles = Map.empty
            Views = Map.empty
            TimeSeries = Map.empty
            Connectors = Map.empty
            SingletonConstructions = List.empty
            ReferencedEcosystems = Map.empty
            EnforceRateLimits = false
        }

    let withNoSessionHandler
            (builder: EcosystemBuilder<NoSession, NoSessionAction, NoSessionLifeEvent, NoRole>)
            : EcosystemBuilder<NoSession, NoSessionAction, NoSessionLifeEvent, NoRole> =
        let metaLifeCycle = metaLifeCycle<NoSession, NoRole> builder.Def.MetaLifeCycleDef

        { builder with
            SessionInfo =
                Some
                    {
                        MaybeSessionHandlingWithRootRoles = None
                        MaybeSessionLifeCycle = None
                        MetaLifeCycle = (fun sessionHandling -> { metaLifeCycle with SessionHandling = sessionHandling })
                    }
        }

    let withSessionHandler
            (sessionLifeCycle: LifeCycle<'Session, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'SessionEnv>)
            (idStr: 'Session -> string)
            (roles: ExternalCallOrigin -> Option<'Session> -> Set<'Role>)
            (userId: 'Session -> AccessUserId)
            (maybeRevalidator: Option<SessionRevalidator<'Session, 'LifeAction, 'LifeEvent>>)
            (rootRoles: Set<'Role>)
            (builder: EcosystemBuilder<'OldSession, 'OldSessionAction, 'OldSessionLifeEvent, 'OldRole>)
            : EcosystemBuilder<'Session, 'LifeAction, 'LifeEvent, 'Role> =
        let sessionLifeCycle =
            sessionLifeCycle
            |> withLifeCycleApiAccessOpenedToRoot rootRoles
        let metaLifeCycle =
            metaLifeCycle<'Session, 'Role> builder.Def.MetaLifeCycleDef
            |> withLifeCycleApiAccessOpenedToRoot rootRoles

        { builder.AssumeTypes<'Session, 'LifeAction, 'LifeEvent, 'Role>() with
            SessionInfo =
                Some
                    {
                        MaybeSessionHandlingWithRootRoles =
                            Some
                                {
                                    Handling =
                                        {
                                            Handler =
                                                {
                                                    LifeCycle        = { sessionLifeCycle with SessionHandling = None }
                                                    GetIdStr         = idStr
                                                    GetUserId        = userId
                                                    MaybeRevalidator =
                                                        maybeRevalidator
                                                        |> Option.map (fun typedRevalidator ->
                                                            { RevalidateAction = typedRevalidator.RevalidateAction
                                                              GetRevalidateOn = typedRevalidator.GetRevalidateOn
                                                              RevalidateCompleteEvent = typedRevalidator.RevalidateCompleteEvent
                                                              GetRevalidateCompleteResult = fun (lifeEvent: LifeEvent) -> typedRevalidator.GetRevalidateCompleteResult (lifeEvent :?> 'LifeEvent) })
                                                }
                                            GetRoles = roles
                                        }
                                    RootRoles = rootRoles
                                }
                        MaybeSessionLifeCycle = Some (fun sessionHandling -> { sessionLifeCycle with SessionHandling = sessionHandling })
                        MetaLifeCycle = (fun sessionHandling -> { metaLifeCycle with SessionHandling = sessionHandling })
                    }
        }

    let addLifeCycle
            (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env>)
            (builder: EcosystemBuilder<'Session, 'SessionLifeAction, 'SessionLifeEvent, 'Role>)
            : EcosystemBuilder<'Session, 'SessionLifeAction, 'SessionLifeEvent, 'Role> =
        match builder.LifeCycles.TryFind lifeCycle.Definition.Key with
        | Some otherLifeCycle ->
            failwithf "LifeCycle with Name %s has already been registered for LifeCycle type %s, conflicts with Subject type %s"
                lifeCycle.Name (otherLifeCycle.GetType().FullName) typeof<'Subject>.FullName
        | None ->
            if builder.Def.LifeCycleDefs |> Seq.exists (fun def -> box def = box lifeCycle.Definition) |> not then
                failwithf "LifeCycle with Name %s has no backing definition in EcosystemDef" lifeCycle.Name
            else
                let lifeCycle =
                    // If there's a session handler and the LC is accessible via API, we ensure root roles have access
                    builder.SessionInfo
                    |> Option.bind (fun sessionInfo -> sessionInfo.MaybeSessionHandlingWithRootRoles)
                    |> Option.map (fun sessionHandlingWithRootRoles -> lifeCycle |> withMaybeLifeCycleApiAccessOpenedToRoot sessionHandlingWithRootRoles.RootRoles)
                    |> Option.defaultValue lifeCycle

                { builder with
                    LifeCycles =
                        builder.LifeCycles.Add(
                            lifeCycle.Definition.Key,
                            (fun sessionHandling -> { lifeCycle with SessionHandling = sessionHandling })
                        )
                    SingletonConstructions =
                        // if lifecycle is a singleton, append an operation to MaybeConstruct it
                        match lifeCycle.SingletonCtor with
                        | None -> builder.SingletonConstructions
                        | Some singletonCtor ->
                            let op = createLifeCycleOp lifeCycle.Definition (LifeCycleOp.MaybeConstruct singletonCtor)
                            op :: builder.SingletonConstructions
                }

    let addView
            (view: View<'Input, 'Output, 'OpError, 'AccessPredicateInput, 'Session, 'Role, 'Env>)
            (builder: EcosystemBuilder<'Session, 'SessionLifeAction, 'SessionLifeEvent, 'Role>)
            : EcosystemBuilder<'Session, 'SessionLifeAction, 'SessionLifeEvent, 'Role> =
        match builder.Views.TryFind view.Name with
        | Some otherView ->
            failwithf "View with Name '%s' has already been registered for View type %s, conflicts with Output type %s"
                view.Name (otherView.GetType().FullName) typeof<'Output>.FullName
        | None ->
            if builder.Def.ViewDefs |> Seq.exists (fun def -> box def = box view.Definition) |> not then
                failwithf "View with Name %s has no backing definition in EcosystemDef" view.Name
            else
                let view =
                    builder.SessionInfo
                    |> Option.bind (fun sessionInfo -> sessionInfo.MaybeSessionHandlingWithRootRoles)
                    |> Option.map (fun sessionHandlingWithRootRoles -> view |> withMaybeViewApiAccessOpenedToRoot sessionHandlingWithRootRoles.RootRoles)
                    |> Option.defaultValue view

                { builder with
                    Views =
                        builder.Views.Add(
                            view.Name,
                            (fun sessionHandling -> { view with SessionHandling = sessionHandling })
                        )
                }

    let addViews
            (views: seq<View<'Input, 'Output, 'OpError, 'AccessPredicateInput, 'Session, 'Role, 'Env>>)
            (builder: EcosystemBuilder<'Session, 'SessionLifeAction, 'SessionLifeEvent, 'Role>)
            : EcosystemBuilder<'Session, 'SessionLifeAction, 'SessionLifeEvent, 'Role> =
        views
        |> Seq.fold (fun ecosystem view -> addView view ecosystem) builder

    let addTimeSeries
            (timeSeries: TimeSeries<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex, 'AccessPredicateInput, 'Session, 'Role>)
            (builder: EcosystemBuilder<'Session, 'SessionLifeAction, 'SessionLifeEvent, 'Role>)
            : EcosystemBuilder<'Session, 'SessionLifeAction, 'SessionLifeEvent, 'Role> =
        match builder.TimeSeries.TryFind timeSeries.Definition.Key with
        | Some otherTimeSeries ->
            failwithf "TimeSeries with Name %s has already been registered for TimeSeries type %s, conflicts with Data Point type %s"
                timeSeries.Name (otherTimeSeries.GetType().FullName) typeof<'TimeSeriesDataPoint>.FullName
        | None ->
            if builder.Def.TimeSeriesDefs |> Seq.exists (fun def -> box def = box timeSeries.Definition) |> not then
                failwithf "TimeSeries with Name %s has no backing definition in EcosystemDef" timeSeries.Name
            else
                let timeSeries =
                    // If there's a session handler and the LC is accessible via API, we ensure root roles have access
                    builder.SessionInfo
                    |> Option.bind (fun sessionInfo -> sessionInfo.MaybeSessionHandlingWithRootRoles)
                    |> Option.map (fun sessionHandlingWithRootRoles -> timeSeries |> withMaybeTimeSeriesApiAccessOpenedToRoot sessionHandlingWithRootRoles.RootRoles)
                    |> Option.defaultValue timeSeries

                { builder with
                    TimeSeries =
                        builder.TimeSeries.Add(
                            timeSeries.Definition.Key,
                            (fun sessionHandling -> { timeSeries with SessionHandling = sessionHandling })
                        )
                }

    let addConnector
            (connector: Connector<'Request, 'Env>)
            (builder: EcosystemBuilder<'Session, 'SessionLifeAction, 'SessionLifeEvent, 'Role>)
            : EcosystemBuilder<'Session, 'SessionLifeAction, 'SessionLifeEvent, 'Role> =
        match builder.Connectors.TryFind connector.Name with
        | Some _ ->
            failwithf "Connector with Name %s has already been registered" connector.Name
        | None ->
            { builder with Connectors = builder.Connectors.Add(connector.Name, connector) }

    let addReferencedEcosystem
        (referencedEcosystem: ReferencedEcosystem)
        (builder: EcosystemBuilder<'Session, 'SessionLifeAction, 'SessionLifeEvent, 'Role>)
        : EcosystemBuilder<'Session, 'SessionLifeAction, 'SessionLifeEvent, 'Role> =
        match builder.ReferencedEcosystems.TryFind referencedEcosystem.Def.Name with
        | Some _ ->
            failwithf "Referenced ecosystem with Name %s has already been registered" referencedEcosystem.Def.Name
        | None ->
            { builder with
                ReferencedEcosystems =
                    builder.ReferencedEcosystems.Add(
                        referencedEcosystem.Def.Name,
                        (fun sessionHandling ->
                            { referencedEcosystem with
                                LifeCycles = referencedEcosystem.LifeCycles |> List.map (fun referencedLifeCycle -> referencedLifeCycle.AssumeSessionHandling sessionHandling)
                            })
                    )
            }

    let enforceRateLimits
        (builder: EcosystemBuilder<'Session, 'SessionLifeAction, 'SessionLifeEvent, 'Role>)
        : EcosystemBuilder<'Session, 'SessionLifeAction, 'SessionLifeEvent, 'Role> =
            { builder with
                EnforceRateLimits = true
            }

    let build (builder: EcosystemBuilder<'Session, 'SessionLifeAction, 'SessionLifeEvent, 'Role>) : Ecosystem =
        builder.ToEcosystem()
