[<AutoOpen>]
module LibLifeCycleHost.GrainFunctions

open System
open System.Threading.Tasks
open LibLifeCycle
open LibLifeCycleCore
open LibLifeCycleHost.GrainStorageModel
open LibLifeCycleHost.SideEffectsHandler

exception IdStabilityViolatedException of BadKey: string
exception TransitionIgnoreWithSideEffectsException of SideEffectsDump: string
exception TransientSideEffectsNotAllowedInTransaction

type private CombinedSubscribeCandidate =
| ActCandidate               of LifeAction * Deduplicate: bool * TraceContext
| InitializeCandidate        of Constructor * OkIfAlreadyInitialized: bool * TraceContext
| ActMaybeConstructCandidate of LifeAction * Constructor * Deduplicate: bool * TraceContext
| SubscribeCandidate         of Map<SubscriptionName, LifeEvent> * TraceContext

let getGrainFunctions<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId
                when 'Subject              :> Subject<'SubjectId>
                and  'LifeAction           :> LifeAction
                and  'OpError              :> OpError
                and  'Constructor          :> Constructor
                and  'LifeEvent            :> LifeEvent
                and  'LifeEvent            :  comparison
                and  'SubjectId            :> SubjectId
                and  'SubjectId            :  comparison>
    (adapters: HostedOrReferencedLifeCycleAdapterRegistry)
    (grainScopedServiceProvider: IServiceProvider)
    (lifeCycleAdapter: HostedLifeCycleAdapter<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>)
    =

    let mapTriggerSubscriptionSideEffects
        (othersSubscribing: OthersSubscribing<'LifeEvent>)
        (traceContext: TraceContext)
        (raisedLifeEvents: list<'LifeEvent>)
        : GrainPersistedSideEffect<'LifeAction, 'OpError> seq =
        raisedLifeEvents
        |> Seq.collect (
            fun raisedLifeEvent ->
                // TODO: change OthersSubscribing to list? there's no point to make it a map anymore because it's scanned anyway
                othersSubscribing
                |> Map.toSeq
                |> Seq.choose (
                    fun (subscribedLifeEvent, subscriptions) ->
                        if lifeCycleAdapter.LifeCycle.LifeEventSatisfies.Value { Raised = raisedLifeEvent; Subscribed = subscribedLifeEvent } then
                            Some(subscriptions, raisedLifeEvent)
                        else
                            None
                    )
            )
        |> Seq.collect (
            fun (subscriptions, lifeEvent) ->
                subscriptions
                |> Map.toSeq
                |> Seq.collect (
                    fun (subscriptionName, subscribers) ->
                        subscribers
                        |> Seq.map (fun subscriber -> subscriber, subscriptionName, (lifeEvent :> LifeEvent))
                   )
           )
        |> Seq.map (
            fun (subscriber, subscriptionName, lifeEvent) ->
                RpcTriggerSubscriptionOnGrain {
                    SubjectPKeyReference = subscriber
                    SubscriptionTriggerType = SubscriptionTriggerType.Named subscriptionName
                    LifeEvent = lifeEvent
                    TraceContext = traceContext
                    Deduplicate = true // provide overload if we need cheaper at-least-once version of event callback
                }
            )

    let mapToGrainSideEffectsAndRaisedLifeEvents
        (traceContext: TraceContext)
        (currentSubject: 'Subject)
        (sideEffects: TransitionSideEffects<'Constructor, 'LifeEvent, 'LifeAction>)
        (othersSubscribing: OthersSubscribing<'LifeEvent>)
        : Task<List<GrainSideEffect<'LifeAction, 'OpError>> * List<'LifeEvent>> =
        backgroundTask {
            let! result = processWorkflowSideEffectsOnGrains adapters grainScopedServiceProvider lifeCycleAdapter.LifeCycle currentSubject.SubjectId sideEffects traceContext
            let triggerSubscriptionSideEffects = mapTriggerSubscriptionSideEffects othersSubscribing traceContext result.RaisedLifeEvents
            return
                (
                    [
                        yield! result.GrainSideEffects
                        yield!
                            triggerSubscriptionSideEffects
                            |> Seq.map GrainSideEffect.Persisted
                    ],
                    result.RaisedLifeEvents
                )
        }

    let calculateDueTimerActionOrNextDueTick (now: DateTimeOffset) (subject: 'Subject) (subjectLastUpdatedOn: DateTimeOffset) :
        Choice<TimerAction<'LifeAction>, Option<DateTimeOffset>> =

        let timers: list<Timer<'LifeAction>> = lifeCycleAdapter.LifeCycle.Timers subject
        if timers.Length > 0 then
            // choose the most overdue timer action
            let (timerAction, on) =
                timers
                |> List.map (
                    fun timer ->
                        match timer.Schedule with
                        | Schedule.Now                          -> timer.TimerAction, subjectLastUpdatedOn
                        | Schedule.On on                        -> timer.TimerAction, on
                        | Schedule.AfterLastTransition timeSpan -> timer.TimerAction, (subjectLastUpdatedOn + timeSpan))
                // make it non-deterministic in case of multiple most overdue actions
                |> List.groupBy snd
                |> List.minBy fst
                |> snd
                // now is evenly distributed, no need for an extra random number
                |> (fun candidates -> candidates |> List.item (now.Millisecond % candidates.Length))

            // Why do we choose greater of two and not just "now"? It can't be less than lastUpdatedOn, right? Wrong!
            // System clock is unreliable and sometimes can travel back in time, this can no-op away correctly enqueued trigger.
            // However lastUpdatedOn is by definition in the past, so we can use it safely.
            let nowOrLastUpdatedOnWhateverIsGreater = max now subjectLastUpdatedOn

            if on <= nowOrLastUpdatedOnWhateverIsGreater then
                Choice1Of2 timerAction
            else
                Choice2Of2 (Some on)
        else
            Choice2Of2 None

    let grainPersistedSideEffectRpc = GrainPersistedSideEffect.Rpc >> GrainSideEffect.Persisted

    let tryCombineWithSubscribeSideEffects (sideEffects: List<GrainSideEffect<'LifeAction, 'OpError>>) : List<GrainSideEffect<'LifeAction, 'OpError>> =
        // TODO: can we do better? what if ambiguous e.g. Act, Construct, and Subscribe, or multiple Act etc. ? How do we discourage that?

        let (leftOvers, otherSideEffects) =
            sideEffects
            |> Seq.fold (fun (candidates,others) sideEffect ->
                match sideEffect with
                | GrainSideEffect.Transient _ ->
                    (candidates, (sideEffect::others))
                | GrainSideEffect.Persisted persistedSideEffect ->
                    match persistedSideEffect with
                    | GrainPersistedSideEffect.Rpc grainRpc ->
                        match grainRpc.RpcOperation with
                        | GrainRpcOperation.RunActionOnGrain (action, deduplicate) ->
                            match Map.tryFind grainRpc.SubjectReference candidates with
                            | None ->
                                (candidates.Add(grainRpc.SubjectReference, (ActCandidate (action, deduplicate, grainRpc.TraceContext))), others)
                            | Some (ActCandidate _)
                            | Some (InitializeCandidate _)
                            | Some (ActMaybeConstructCandidate _) ->
                                (candidates, (sideEffect::others))
                            | Some (SubscribeCandidate (subscriptions, _)) ->
                                let combinedSideEffect =
                                    {
                                        SubjectReference = grainRpc.SubjectReference
                                        RpcOperation = GrainRpcOperation.RunActionOnGrainAndSubscribe (action, deduplicate, subscriptions)
                                        TraceContext = grainRpc.TraceContext
                                    }
                                    |> grainPersistedSideEffectRpc
                                (candidates.Remove(grainRpc.SubjectReference), (combinedSideEffect::others))

                        | GrainRpcOperation.InitializeGrain (ctor, okIfAlreadyInitialized) ->
                            match Map.tryFind grainRpc.SubjectReference candidates with
                            | None ->
                                (candidates.Add(grainRpc.SubjectReference, (InitializeCandidate (ctor, okIfAlreadyInitialized, grainRpc.TraceContext))), others)
                            | Some (ActCandidate _)
                            | Some (InitializeCandidate _)
                            | Some (ActMaybeConstructCandidate _) ->
                                (candidates, (sideEffect::others))
                            | Some (SubscribeCandidate (subscriptions, _)) ->
                                let combinedSideEffect =
                                    {
                                        SubjectReference = grainRpc.SubjectReference
                                        RpcOperation = GrainRpcOperation.InitializeGrainAndSubscribe (ctor, okIfAlreadyInitialized, subscriptions)
                                        TraceContext = grainRpc.TraceContext
                                    }
                                    |> grainPersistedSideEffectRpc

                                (candidates.Remove(grainRpc.SubjectReference), (combinedSideEffect::others))

                        | GrainRpcOperation.RunActionMaybeConstructOnGrain (action, ctor, deduplicate) ->
                            match Map.tryFind grainRpc.SubjectReference candidates with
                            | None ->
                                (candidates.Add (grainRpc.SubjectReference, ActMaybeConstructCandidate (action, ctor, deduplicate, grainRpc.TraceContext)), others)
                            | Some (ActCandidate _)
                            | Some (InitializeCandidate _)
                            | Some (ActMaybeConstructCandidate _) ->
                                (candidates, (sideEffect::others))
                            | Some (SubscribeCandidate (subscriptions, _)) ->
                                let combinedSideEffect =
                                    {
                                        SubjectReference = grainRpc.SubjectReference
                                        RpcOperation = GrainRpcOperation.RunActionMaybeConstructAndSubscribeOnGrain (action, ctor, deduplicate, subscriptions)
                                        TraceContext = grainRpc.TraceContext
                                    }
                                    |> grainPersistedSideEffectRpc
                                (candidates.Remove(grainRpc.SubjectReference), (combinedSideEffect::others))

                        | GrainRpcOperation.SubscribeToGrain subscriptions ->
                            match Map.tryFind grainRpc.SubjectReference candidates with
                            | None ->
                                (candidates.Add(grainRpc.SubjectReference, SubscribeCandidate (subscriptions, grainRpc.TraceContext)), others)

                            | Some (ActCandidate (action, deduplicate, traceContext)) ->
                                let combinedSideEffect =
                                    {
                                        SubjectReference = grainRpc.SubjectReference
                                        RpcOperation = GrainRpcOperation.RunActionOnGrainAndSubscribe(action, deduplicate, subscriptions)
                                        TraceContext = traceContext
                                    }
                                    |> grainPersistedSideEffectRpc
                                (candidates.Remove(grainRpc.SubjectReference), (combinedSideEffect::others))

                            | Some (InitializeCandidate (ctor, okIfAlreadyInitialized, traceContext)) ->
                                let combinedSideEffect =
                                    {
                                        SubjectReference = grainRpc.SubjectReference
                                        RpcOperation = GrainRpcOperation.InitializeGrainAndSubscribe(ctor, okIfAlreadyInitialized, subscriptions)
                                        TraceContext = traceContext
                                    }
                                    |> grainPersistedSideEffectRpc

                                (candidates.Remove(grainRpc.SubjectReference), (combinedSideEffect::others))

                            | Some (ActMaybeConstructCandidate (action, ctor, deduplicate, traceContext)) ->
                                let combinedSideEffect =
                                    {
                                        SubjectReference = grainRpc.SubjectReference
                                        RpcOperation = GrainRpcOperation.RunActionMaybeConstructAndSubscribeOnGrain(action, ctor, deduplicate, subscriptions)
                                        TraceContext = traceContext
                                    }
                                    |> grainPersistedSideEffectRpc

                                (candidates.Remove(grainRpc.SubjectReference), (combinedSideEffect::others))

                            | Some (SubscribeCandidate _) ->
                                (candidates, (sideEffect::others))

                        | _ ->
                            (candidates, (sideEffect::others))
                    | _ ->
                        (candidates, (sideEffect::others))
            ) (Map.empty, [])

        leftOvers
        |> Map.toSeq
        |> Seq.fold (fun otherSideEffects (subjectReference, combineCandidate) ->
            match combineCandidate with
            | ActCandidate (action, deduplicate, traceContext) ->
                let sideEffect =
                    {
                        SubjectReference = subjectReference
                        RpcOperation = GrainRpcOperation.RunActionOnGrain (action, deduplicate)
                        TraceContext = traceContext
                    }
                    |> grainPersistedSideEffectRpc
                sideEffect::otherSideEffects
            | InitializeCandidate (ctor, okIfAlreadyInitialize, traceContext) ->
                let sideEffect =
                    {
                        SubjectReference = subjectReference
                        RpcOperation = GrainRpcOperation.InitializeGrain (ctor, okIfAlreadyInitialize)
                        TraceContext = traceContext
                    }
                    |> grainPersistedSideEffectRpc
                sideEffect::otherSideEffects
            | ActMaybeConstructCandidate (action, ctor, deduplicate, traceContext) ->
                let sideEffect =
                    {
                        SubjectReference = subjectReference
                        RpcOperation = GrainRpcOperation.RunActionMaybeConstructOnGrain (action, ctor, deduplicate)
                        TraceContext = traceContext
                    }
                    |> grainPersistedSideEffectRpc
                sideEffect::otherSideEffects
            | SubscribeCandidate (subscriptions, traceContext) ->
                let sideEffect =
                    {
                        SubjectReference = subjectReference
                        RpcOperation = GrainRpcOperation.SubscribeToGrain subscriptions
                        TraceContext = traceContext
                    }
                    |> grainPersistedSideEffectRpc
                sideEffect::otherSideEffects
            ) otherSideEffects

    let createOrUpdateSubject
            (now: DateTimeOffset)
            (traceContext: TraceContext)
            (createdOrUpdatedSubject: 'Subject)
            (blobActions: List<BlobAction>)
            (createOrUpdateData: Choice<'SubjectId, SubjectState<'Subject, 'SubjectId> * GrainSideEffectSequenceNumber>)
            (othersSubscribing: OthersSubscribing<'LifeEvent>)
            (sideEffects: TransitionSideEffects<'Constructor, 'LifeEvent, 'LifeAction>)
            : Task<SubjectState<'Subject, 'SubjectId> * Option<ReminderUpdate> * List<BlobAction> * Option<SideEffectGroup<'LifeAction, 'OpError>> * List<IndexAction<'OpError>> * List<'LifeEvent>> =
        backgroundTask {
            let (subjectSubscriptions, _) = getSubscriptions lifeCycleAdapter.LifeCycle createdOrUpdatedSubject

            let expectedSubjectId, currentTickState, currentSubscriptions, maybeCurrentSubject, sideEffectSeqNumber =
                match createOrUpdateData with
                | Choice1Of2 generatedSubjectId -> generatedSubjectId, TickState.NoTick, Map.empty, None, 0UL
                | Choice2Of2 (currentState, sideEffectSeqNumber) -> currentState.Subject.SubjectId, currentState.TickState, currentState.OurSubscriptions, Some currentState.Subject, sideEffectSeqNumber

            if expectedSubjectId <> createdOrUpdatedSubject.SubjectId then
                return raise (IdStabilityViolatedException createdOrUpdatedSubject.SubjectId.IdString)
            else
                let maybeTickStateAndReminderUpdate =
                    calculateUpdatedTickStateAndReminderUpdate lifeCycleAdapter.LifeCycle traceContext currentTickState now createdOrUpdatedSubject (* hasActiveSubscriptions *) subjectSubscriptions.IsNonempty

                let ourSubscriptions =
                    subjectSubscriptions
                    |> Map.map (
                        fun _ subjectSubscription ->
                            { SubjectId = subjectSubscription.TargetSubjectId
                              LifeCycleKey = subjectSubscription.TargetLifeCycleKey }
                            : SubjectReference
                        )

                let updatedState =
                    { Subject = createdOrUpdatedSubject
                      LastUpdatedOn = now
                      TickState = maybeTickStateAndReminderUpdate |> Option.map fst |> Option.defaultValue currentTickState
                      OurSubscriptions = ourSubscriptions }

                let! grainSideEffects, raisedLifeEvents = mapToGrainSideEffectsAndRaisedLifeEvents traceContext createdOrUpdatedSubject sideEffects othersSubscribing

                let remoteSubscriptionGrainSideEffects =
                    updateRemoteSubscriptions currentSubscriptions subjectSubscriptions traceContext
                    |> Seq.map grainPersistedSideEffectRpc
                    |> Seq.toList

                let sideEffectGroup =
                    [
                        yield! grainSideEffects
                        yield! remoteSubscriptionGrainSideEffects
                    ]
                    |> tryCombineWithSubscribeSideEffects
                    |> makeNewSideEffectsGroup sideEffectSeqNumber

                let indexActions = getIndexActions lifeCycleAdapter.LifeCycle createdOrUpdatedSubject maybeCurrentSubject
                return (updatedState, maybeTickStateAndReminderUpdate |> Option.map snd, blobActions, sideEffectGroup, indexActions, raisedLifeEvents)
        }

    let rethrowIfTransientInternalCallOrPassIn (callOrigin: CallOrigin) (ex: Exception) : Exception =
        let maybeUnwrappedTransientException : Option<Exception> =
            match ex with
            | :? OutOfMemoryException as ex -> Some (ex :> Exception)
            | :? TransientSubjectException as ex -> Some (ex :> Exception)
            | _ -> None
        match callOrigin, maybeUnwrappedTransientException with
        | CallOrigin.Internal _, Some ex ->
            // re-raise transient exceptions for internal calls so they will be retried
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw()
            shouldNotReachHereBecause "line above throws"
        | CallOrigin.External _, Some ex -> ex
        | _, None -> ex

    let processAction
            (now: DateTimeOffset)
            (traceContext: TraceContext)
            (sideEffectSeqNumber: GrainSideEffectSequenceNumber)
            (currentState: SubjectState<'Subject, 'SubjectId>)
            (othersSubscribing: OthersSubscribing<'LifeEvent>)
            (action: 'LifeAction)
            (callOrigin: CallOrigin)
            (serviceProvider: IServiceProvider)
            : Task<
                    Result<
                        Choice<
                            // TransitionOk - full update
                            SubjectState<'Subject, 'SubjectId> * Option<ReminderUpdate> * List<BlobAction> * Option<SideEffectGroup<'LifeAction, 'OpError>> * List<IndexAction<'OpError>> * List<'LifeEvent>,
                            // TransitionIgnored - only new tick state and runtime update e.g. to reschedule TickState.Fired after successful Noop timer action
                            Option<TickState * ReminderUpdate>>,
                        TransitionBuilderError<'OpError>>> =
        backgroundTask {
            try
                let (TransitionResult transitionTask) = lifeCycleAdapter.LifeCycle.Act callOrigin serviceProvider currentState.Subject action
                match! transitionTask with
                | Ok (TransitionOk (transitionedSubject, blobActions, sideEffects)) ->
                    let autoTransitionIgnore =
                        lifeCycleAdapter.LifeCycle.AutoIgnoreNoOpTransitions &&
                        blobActions.IsEmpty && sideEffects.IsEmpty &&
                        Object.Equals (transitionedSubject, currentState.Subject)
                    if autoTransitionIgnore then
                        return
                            calculateUpdatedTickStateAndReminderUpdate lifeCycleAdapter.LifeCycle traceContext currentState.TickState currentState.LastUpdatedOn currentState.Subject (* hasActiveSubscriptions *) currentState.OurSubscriptions.IsNonempty
                            |> Choice2Of2 |> Ok
                    else
                        let! res = createOrUpdateSubject now traceContext transitionedSubject blobActions (Choice2Of2 (currentState, sideEffectSeqNumber)) othersSubscribing sideEffects
                        return res |> Choice1Of2 |> Ok

                | Ok (TransitionIgnored (ignoredBlobActions, ignoredSideEffects)) ->
                    if (ignoredBlobActions.IsEmpty && ignoredSideEffects.IsEmpty) then
                        return
                            calculateUpdatedTickStateAndReminderUpdate lifeCycleAdapter.LifeCycle traceContext currentState.TickState currentState.LastUpdatedOn currentState.Subject (* hasActiveSubscriptions *) currentState.OurSubscriptions.IsNonempty
                            |> Choice2Of2 |> Ok
                    else
                        return raise (TransitionIgnoreWithSideEffectsException <| sprintf "Blob actions: %A; Side effects: %A" ignoredBlobActions ignoredSideEffects)
                | Error err ->
                    return Error err
            with
            | ex ->
                let ex = rethrowIfTransientInternalCallOrPassIn callOrigin ex
                return ex |> LifeCycleException |> Error
        }

    let processConstruction
        (now: DateTimeOffset)
        (traceContext: TraceContext)
        (callOrigin: CallOrigin)
        (serviceProvider: IServiceProvider)
        (subjectId: 'SubjectId)
        (ctor: 'Constructor)
        (maybeConstructSubscriptions: Option<ConstructSubscriptions>) :
        Task<Result<
                SubjectState<'Subject,'SubjectId> * Option<ReminderUpdate> * OthersSubscribing<'LifeEvent> * List<BlobAction> * Option<SideEffectGroup<'LifeAction, 'OpError>> * List<IndexAction<'OpError>> * List<'LifeEvent>,
                ConstructionBuilderError<'OpError>>> =
        backgroundTask {
            try
                let (ConstructionResult ctorTask) = lifeCycleAdapter.LifeCycle.Construct callOrigin serviceProvider subjectId ctor
                match! ctorTask with
                | Ok (initialValue, blobActions, constructionSideEffects) ->
                    let transitionSideEffects : TransitionSideEffects<'Constructor, 'LifeEvent, 'LifeAction> = {
                        Constructors    = []
                        ExternalActions = constructionSideEffects.ExternalActions
                        LifeEvents      = constructionSideEffects.LifeEvents
                        LifeActions     = constructionSideEffects.LifeActions
                    }

                    let creatorSubscribing =
                        maybeConstructSubscriptions
                        |> Option.map (fun ctorSubs ->
                            ctorSubs.Subscriptions
                            |> Map.toSeq
                            |> Seq.map (
                                fun (subscriptionName, subscribeToEvent) ->
                                    match subscribeToEvent with
                                    | :? 'LifeEvent as le -> (le, Map.ofOneItem (subscriptionName, Set.singleton ctorSubs.Subscriber))
                                    | _ -> failwithf "Invalid LifeEvent type %s provided to LifeCycle %s" (subscribeToEvent.GetType().FullName) lifeCycleAdapter.LifeCycle.Name
                                )
                            |> Map.ofSeq)
                        |> Option.defaultValue Map.empty

                    let! initialValue, maybeReminderUpdate, blobActions, constructionSideEffects, indexActions, raisedLifeEvents =
                        createOrUpdateSubject now traceContext initialValue blobActions (Choice1Of2 subjectId) creatorSubscribing transitionSideEffects

                    return Ok (initialValue, maybeReminderUpdate, creatorSubscribing, blobActions, constructionSideEffects, indexActions, raisedLifeEvents)
                | Error err ->
                    return Error err
            with
            | ex ->
                let ex = rethrowIfTransientInternalCallOrPassIn callOrigin ex
                return ex |> LifeCycleCtorException |> Error
        }

    let testingOnlyInitializeDirectlyToValue
        (now: DateTimeOffset)
        (traceContext: TraceContext)
        (subjectId: 'SubjectId)
        (initialValue: 'Subject):
        Task<SubjectState<'Subject,'SubjectId> * Option<ReminderUpdate> * List<BlobAction> * Option<SideEffectGroup<'LifeAction, 'OpError>> * List<IndexAction<'OpError>> * List<'LifeEvent>> =
        let emptySideEffects : TransitionSideEffects<'Constructor, 'LifeEvent, 'LifeAction> = {
            Constructors          = []
            ExternalActions       = []
            LifeEvents            = []
            LifeActions           = []
        }
        createOrUpdateSubject now traceContext initialValue [] (Choice1Of2 subjectId) Map.empty emptySideEffects

    let addToOthersSubscribing
            (othersSubscribing: OthersSubscribing<'LifeEvent>)
            (subscriptionNamesToEvents: Map<SubscriptionName, LifeEvent>)
            (subjectPkeyRef: SubjectPKeyReference)
            : Option<OthersSubscribing<'LifeEvent> * Map<SubscriptionName, 'LifeEvent>> =

        let (updatedOthersSubscribing, newSubscriptions) =
            subscriptionNamesToEvents
            |> Map.toSeq
            |> Seq.map (
                fun (subscriptionName, subscribeToEvent) ->
                    match subscribeToEvent with
                    | :? 'LifeEvent as le -> (subscriptionName, le)
                    | _ -> failwithf "Invalid LifeEvent type %s provided to LifeCycle %s" (subscribeToEvent.GetType().FullName) lifeCycleAdapter.LifeCycle.Name
                )
            |> Seq.fold (
                fun ((othersSubscribing: OthersSubscribing<'LifeEvent>), (newSubscriptions: Map<SubscriptionName, 'LifeEvent>)) (subscriptionName, subscribeToEvent) ->
                    match othersSubscribing.TryFind subscribeToEvent with
                    | Some nameToPkeyReferences ->
                        match nameToPkeyReferences.TryFind subscriptionName with
                        | Some pkeyReferences ->
                            if pkeyReferences.Contains subjectPkeyRef then
                                (othersSubscribing, newSubscriptions)
                            else
                                let updatedOthersSubscribing =
                                    othersSubscribing.Add(subscribeToEvent, nameToPkeyReferences.Add(subscriptionName, (pkeyReferences.Add subjectPkeyRef)))
                                let updatedNewSubscriptions =
                                    newSubscriptions.Add(subscriptionName, subscribeToEvent)
                                (updatedOthersSubscribing, updatedNewSubscriptions)
                        | None ->
                            let updatedOthersSubscribing =
                                othersSubscribing.Add(subscribeToEvent, nameToPkeyReferences.Add(subscriptionName, (Set.singleton subjectPkeyRef)))
                            let updatedNewSubscriptions =
                                newSubscriptions.Add(subscriptionName, subscribeToEvent)
                            (updatedOthersSubscribing, updatedNewSubscriptions)
                    | None ->
                        let updatedOthersSubscribing =
                            othersSubscribing.Add(subscribeToEvent, Map.empty.Add(subscriptionName, (Set.singleton subjectPkeyRef)))
                        let updatedNewSubscriptions =
                            newSubscriptions.Add(subscriptionName, subscribeToEvent)
                        (updatedOthersSubscribing, updatedNewSubscriptions)
               ) (othersSubscribing, Map.empty)

        if newSubscriptions.Count > 0 then
            Some(updatedOthersSubscribing, newSubscriptions)
        else
            None

    let removeFromOthersSubscribing
            (othersSubscribing: OthersSubscribing<'LifeEvent>)
            (subscriptions: Set<SubscriptionName>)
            (subscriberPKey: SubjectPKeyReference)
            : Option<OthersSubscribing<'LifeEvent> * Set<SubscriptionName>> =

            // Note: need this aux structure to avoid O(N^2) scans of OthersSubscribing
            // Alternative is to plumb LifeEvent all the way from OurSubscriptions (breaking change in storage)
            let subToLifeEvent =
                othersSubscribing
                |> Map.toSeq
                |> Seq.collect (
                    fun (le, subs) ->
                        subs
                        |> Map.toSeq
                        |> Seq.choose (fun (sn, refs) ->
                            if subscriptions.Contains sn && refs.Contains subscriberPKey then
                                Some (sn, le)
                            else
                                None))
                |> List.ofSeq

            let updatedOthersSubscribing =
                subToLifeEvent
                |> Seq.fold
                    (fun (othersSubscribing: OthersSubscribing<'LifeEvent>) (subscriptionName, le) ->
                        let subToRefs = othersSubscribing |> Map.find le
                        let refs = subToRefs |> Map.find subscriptionName
                        let otherRefs = refs.Remove subscriberPKey
                        let subToRefs =
                            if otherRefs.IsEmpty then
                                subToRefs.Remove subscriptionName
                            else
                                subToRefs.AddOrUpdate (subscriptionName, otherRefs)
                        if subToRefs.IsEmpty then
                            othersSubscribing.Remove le
                        else
                            othersSubscribing.AddOrUpdate (le, subToRefs))
                    othersSubscribing

            let removedSubscriptions = subToLifeEvent |> Seq.map fst |> Set.ofSeq
            if removedSubscriptions.IsEmpty then
                None
            else
                Some (updatedOthersSubscribing, removedSubscriptions)

    let getActionForTriggeredSubscription
            (subjectState: SubjectState<'Subject, 'SubjectId>)
            (subscriptionTriggerType: SubscriptionTriggerType)
            (triggeredLifeEvent: LifeEvent)
            : Option<Result<'LifeAction, {| ResolveTriggeredActionException: Exception |}>> =
        try
            match subscriptionTriggerType with
            | SubscriptionTriggerType.Named subscriptionName ->
                lifeCycleAdapter.LifeCycle.Subscriptions subjectState.Subject
                |> Map.tryFind subscriptionName
                |> Option.bind (
                    fun subscription ->
                        match subscription with
                        | ForSubject (_, action) ->
                            action |> Ok |> Some
                        | ForSubjectMap (_, mapLifeEventToActionToRaise) ->
                            mapLifeEventToActionToRaise triggeredLifeEvent |> Option.map Ok
                )
        with
        | ex ->
            let ex = rethrowIfTransientInternalCallOrPassIn CallOrigin.Internal ex
            {| ResolveTriggeredActionException = ex |} |> Error |> Some

    (processAction,
     calculateDueTimerActionOrNextDueTick,
     addToOthersSubscribing,
     removeFromOthersSubscribing,
     getActionForTriggeredSubscription,
     processConstruction,
     mapTriggerSubscriptionSideEffects,
     rethrowIfTransientInternalCallOrPassIn,
     testingOnlyInitializeDirectlyToValue)
