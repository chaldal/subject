[<AutoOpen>]
module LibLifeCycle.ReferencedLifeCycle

type FullyTypedReferencedLifeCycleFunction<'Res> =
    abstract member Invoke: ReferencedLifeCycle<_, _, _, _, _, _, _, _, _, _> -> 'Res

and FullyTypedReferencedLifeCycleFunction<'Res, 'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId
        when 'Subject              :> Subject<'SubjectId>
        and  'LifeAction           :> LifeAction
        and  'OpError              :> OpError
        and  'Constructor          :> Constructor
        and  'LifeEvent            :> LifeEvent
        and  'LifeEvent            :  comparison
        and  'SubjectId            :> SubjectId
        and  'SubjectId            : comparison> =
    abstract member Invoke: ReferencedLifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role> -> 'Res

and IReferencedLifeCycle =
    abstract member Name: string
    abstract member Def: LifeCycleDef
    abstract member Invoke: FullyTypedReferencedLifeCycleFunction<'Res> -> 'Res
    // TODO: HACK: need a way to update session handling in a referenced LC using only this interface, since the builder needs to
    // fill this in when building the referenced ecosystem.
    abstract member AssumeSessionHandling: Option<EcosystemSessionHandling<'Session, 'Role>> -> IReferencedLifeCycle
    abstract member IsSessionLifeCycle: bool

and IReferencedLifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId
                when 'Subject              :> Subject<'SubjectId>
                and  'LifeAction           :> LifeAction
                and  'OpError              :> OpError
                and  'Constructor          :> Constructor
                and  'LifeEvent            :> LifeEvent
                and  'LifeEvent            :  comparison
                and  'SubjectId            :> SubjectId
                and  'SubjectId            :  comparison> =
    inherit IReferencedLifeCycle
    abstract member Invoke: FullyTypedReferencedLifeCycleFunction<'Res, 'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId> -> 'Res
    abstract member RateLimitsPredicate: LifeCycleRateLimitPredicate<'LifeAction, 'Constructor>
    abstract member ShouldSendTelemetry: Option<ShouldSendTelemetryFor<'LifeAction, 'Constructor> -> bool>
    abstract member ShouldRecordHistory: Option<ShouldRecordHistoryFor<'LifeAction, 'Constructor> -> bool>


and ReferencedLifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role
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
                and  'Role                 :  comparison> =
    internal {
        Def:             LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>
        MaybeApiAccess:  Option<LifeCycleApiAccess<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role>>
        ShouldSendTelemetry: Option<ShouldSendTelemetryFor<'LifeAction, 'Constructor> -> bool>
        ShouldRecordHistory: Option<ShouldRecordHistoryFor<'LifeAction, 'Constructor> -> bool>
        SessionHandling: Option<EcosystemSessionHandling<'Session, 'Role>>
    }
with
    member this.Name = this.Def.Key.LocalLifeCycleName
    member this.SessionHandler = this.SessionHandling |> Option.map(fun h -> h.Handler)

    interface IReferencedLifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId> with
        member this.Invoke (fn: FullyTypedReferencedLifeCycleFunction<_, _, _, _, _, _, _>) = fn.Invoke this
        member this.Invoke (fn: FullyTypedReferencedLifeCycleFunction<_>) = fn.Invoke this
        member this.Def = this.Def
        member this.Name = this.Name
        member this.AssumeSessionHandling (sessionHandling: Option<EcosystemSessionHandling<'NewSession, 'NewRole>>) =
            {
                MaybeApiAccess =
                    match this.MaybeApiAccess |> box with
                    | :? (Option<LifeCycleApiAccess<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'AccessPredicateInput, 'NewSession, 'NewRole>>) as existing -> existing
                    | _ -> None
                ShouldSendTelemetry = this.ShouldSendTelemetry
                ShouldRecordHistory = this.ShouldRecordHistory
                SessionHandling = sessionHandling
                Def = this.Def
            }
        member this.IsSessionLifeCycle =
            this.SessionHandling
            |> Option.map (fun h -> h.Handler.LifeCycle.Def.LifeCycleKey = this.Def.Key)
            |> Option.defaultValue false

        member this.RateLimitsPredicate =
            this.MaybeApiAccess
            |> Option.map (fun apiAccess -> apiAccess.RateLimitsPredicate)
            |> Option.defaultWith (fun () -> (fun _ -> None))

        member this.ShouldSendTelemetry =
            this.ShouldSendTelemetry

        member this.ShouldRecordHistory =
            this.ShouldRecordHistory

type LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role, 'Env
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
                and  'Env                  :> Env>
with
    member this.ToReferencedLifeCycle(): ReferencedLifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role> =
        {
            Def = this.Definition
            MaybeApiAccess = this.MaybeApiAccess
            ShouldSendTelemetry = this.ShouldSendTelemetry
            ShouldRecordHistory = this.ShouldRecordHistory
            SessionHandling = this.SessionHandling
        }