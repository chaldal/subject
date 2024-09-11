[<AutoOpen>]
module LibLifeCycleHost.LifeCycleFunctions

open System
open LibLifeCycle
open LibLifeCycleHost.GrainStorageModel

type SubjectSubscriptions                  = Map<SubscriptionName, SubjectSubscription>
type ConstructorSubscriptions<'LifeAction> = Map<LifeCycleKey, SubscriptionName * (LifeEvent -> Option<'LifeAction>)>

exception DeleteSelfTimerWithActiveSubscriptionsException

// Some logic that is shared between LifeCycleAdapter and SubjectGrain, e.g. update Timers and Subscriptions

let makeNewSideEffectsGroup sideEffectSeqNumber effects =
    // caution: new Guid impurity
    effects
    |> Seq.map (fun se -> Guid.NewGuid (), se)
    |> NonemptyMap.ofSeq
    |> Option.map (fun effects -> { SideEffects = effects; SequenceNumber = sideEffectSeqNumber; RehydratedFromStorage = false })

let calculateUpdatedTickStateAndReminderUpdate
    (lifeCycle: ILifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>)
    (traceContext: TraceContext)
    (currentTickState: TickState)
    (subjectLastUpdatedOn: DateTimeOffset)
    (subject: 'Subject)
    (hasActiveSubscriptions: bool)
    : Option<TickState * ReminderUpdate> =

    let updatedTimers: List<Timer<'LifeAction>> = lifeCycle.Timers subject

    if updatedTimers.Length > 0 then
        let nextTickOn, isDeleteSelf, maybeNextTraceContext =
            updatedTimers
            |> Seq.map (
                fun timer ->
                    match timer.Schedule with
                    // preserve trace context if timer scheduled asap or after a short delay
                    | Schedule.Now            -> subjectLastUpdatedOn, Some traceContext
                    | Schedule.AfterLastTransition timeSpan when timeSpan < TimeSpan.FromSeconds 1. ->
                                                 subjectLastUpdatedOn + timeSpan, Some traceContext
                    // don't keep trace context for longer-term and fixed-time timers
                    | Schedule.AfterLastTransition timeSpan -> subjectLastUpdatedOn + timeSpan, None
                    | Schedule.On on          -> on, None
                    |> fun (on, maybeNextTraceContext) ->
                        let isDeleteSelf = match timer.TimerAction with | TimerAction.DeleteSelf -> true | TimerAction.RunAction _ -> false
                        (on, if isDeleteSelf then 0 else 1), // sort order
                        (on, isDeleteSelf, maybeNextTraceContext))
            |> Seq.sortBy fst
            |> Seq.head
            |> snd

        if hasActiveSubscriptions && isDeleteSelf then
            // when there are active subscriptions and DeleteSelf timer, raise exception only if the timer is the most urgent one
            raise DeleteSelfTimerWithActiveSubscriptionsException

        match currentTickState with
        | TickState.Scheduled (currentNextTickOn, _traceContext) ->
            if nextTickOn <> currentNextTickOn then
                Some (TickState.Scheduled (nextTickOn, maybeNextTraceContext), { AssumedCurrent = AssumedCurrentReminder.Set currentNextTickOn; On = Some nextTickOn })
            else
                // no new tick or reminder update
                None

        | TickState.Fired currentNextTickOn ->
            Some (TickState.Scheduled (nextTickOn, maybeNextTraceContext), { AssumedCurrent = AssumedCurrentReminder.Set currentNextTickOn; On = Some nextTickOn })
        | TickState.NoTick ->
            Some (TickState.Scheduled (nextTickOn, maybeNextTraceContext), { AssumedCurrent = AssumedCurrentReminder.NotSet; On = Some nextTickOn })
    else
        match currentTickState with
        | TickState.Scheduled (currentNextTickOn, _)
        | TickState.Fired currentNextTickOn ->
            Some (TickState.NoTick, { AssumedCurrent = AssumedCurrentReminder.Set currentNextTickOn; On = None })

        | TickState.NoTick ->
            None

let getSubscriptions
    (lifeCycle: ILifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>)
    (subject: 'Subject)
    : SubjectSubscriptions * ConstructorSubscriptions<'LifeAction> =
    lifeCycle.Subscriptions subject
    |> Map.fold (
        fun ((subjectSubscriptions: Map<SubscriptionName, SubjectSubscription>),
                (constructorSubscriptions: Map<LifeCycleKey, SubscriptionName * (LifeEvent -> Option<'LifeAction>)>))
                name sub ->
            if name.Length > 200 then
                failwithf "SubscriptionName cannot be longer than 200 chars: %s" name
            match sub with
            | ForSubject (subjectSubscription, _) ->
                subjectSubscriptions.Add(name, subjectSubscription), constructorSubscriptions
            | ForSubjectMap (subjectSubscription, _) ->
                subjectSubscriptions.Add(name, subjectSubscription), constructorSubscriptions)
        (Map.empty, Map.empty)

let updateRemoteSubscriptions
    (existingSubscriptions: Map<SubscriptionName, SubjectReference>)
    (newSubjectSubscriptions: Map<SubscriptionName, SubjectSubscription>)
    (traceContext: TraceContext)
    : List<GrainRpc> =
    [
        // unsubscribe
        Set.difference existingSubscriptions.KeySet newSubjectSubscriptions.KeySet
        |> Seq.groupBy (
            fun (name) -> existingSubscriptions |> Map.find name
            )
        |> Seq.map (
            fun (ref, names) -> {
                RpcOperation = GrainRpcOperation.UnsubscribeFromGrain (Set.ofSeq names)
                SubjectReference = ref
                TraceContext = traceContext
            }
        )
        |> Seq.toList;
        // subscribe
        newSubjectSubscriptions
        |> Map.toSeq
        |> Seq.filter (fun (subName, subjectSubscription) ->
            match existingSubscriptions.TryFind subName with
            | None -> true
            | Some existingSubjectRef ->
                let subjectSubscriptionRef: SubjectReference = {
                    LifeCycleKey = subjectSubscription.TargetLifeCycleKey
                    SubjectId    = subjectSubscription.TargetSubjectId
                }
                existingSubjectRef <> subjectSubscriptionRef
        )
        |> List.ofSeq
        |> List.groupBy (
            fun (_, subjectSubscription) ->
                { LifeCycleKey = subjectSubscription.TargetLifeCycleKey
                  SubjectId = subjectSubscription.TargetSubjectId }
                : SubjectReference
            )
        |> Seq.map (
            fun (targetSubjectReference, grouping) ->
                grouping
                |> Seq.map (
                    fun (subscriptionName, subjectSubscription) ->
                        subscriptionName, subjectSubscription.SubscribedLifeEvent)
                |> Map.ofSeq
                |> fun subscriptionNamesToEvents ->
                    {
                        RpcOperation     = GrainRpcOperation.SubscribeToGrain subscriptionNamesToEvents
                        SubjectReference = targetSubjectReference
                        TraceContext = traceContext
                    }
            )
        |> Seq.toList
    ]
    |> List.concat

let updateTimersAndSubscriptions
    (lifeCycle: ILifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>)
    (now: DateTimeOffset)
    (traceContext: TraceContext)
    (currentSubject: 'Subject)
    (currentTickState: TickState)
    (currentOurSubscriptions: Map<SubscriptionName, SubjectReference>)
    // this flag is used to force update any grain that has a tick set, it will awake the grain and hence repair outdated GrainIdHash.
    // Why not compare stored and proper GrainIdHash? This requires waking up the grain anyway i.e. catch 22
    (repairGrainIdHash: bool)
    : Option<TickState * Map<SubscriptionName, SubjectReference> * Option<ReminderUpdate> * list<GrainSideEffect<'LifeAction, 'OpError>>> =

    // constructor subscriptions ignored because they work only when this subject constructs other subject
    let subscriptions, _ctorSubs = getSubscriptions lifeCycle currentSubject
    let ourSubscriptions =
        subscriptions
        |> Map.map (
            fun _ subjectSubscription ->
                { SubjectId = subjectSubscription.TargetSubjectId
                  LifeCycleKey = subjectSubscription.TargetLifeCycleKey }
                : SubjectReference
        )

    let remoteSubscriptionGrainSideEffects =
        updateRemoteSubscriptions currentOurSubscriptions subscriptions traceContext
        |> Seq.map (Rpc >> GrainSideEffect.Persisted)
        |> Seq.toList

    let maybeTickStateAndReminderUpdate =
        calculateUpdatedTickStateAndReminderUpdate lifeCycle traceContext currentTickState now currentSubject (* hasActiveSubscriptions *) subscriptions.IsNonempty

    let tickState = maybeTickStateAndReminderUpdate |> Option.map fst |> Option.defaultValue currentTickState
    let sideEffects = remoteSubscriptionGrainSideEffects

    let forceSetTickStateToRepairGrainIdHash =
        match repairGrainIdHash, tickState with
        | false, _
        | true, TickState.NoTick ->
            false // grainId is potentially broken, but it matters only when tick is scheduled, so can skip tickState
        | true, TickState.Scheduled _
        | true, TickState.Fired _ ->
            true

    if sideEffects.IsNonempty ||
       (tickState <> currentTickState || forceSetTickStateToRepairGrainIdHash) ||
       ourSubscriptions <> currentOurSubscriptions then
        Some (tickState, ourSubscriptions, maybeTickStateAndReminderUpdate |> Option.map snd, sideEffects)
    else
        None

type internal IndexValue =
| NumIndexValue of int64
| StrIndexValue of string
| SearchIndexValue of string
| GeographyIndexValue of Wkt: string

let internal getIndexEntriesForSubject
    (lifeCycle: ILifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>)
    (subj: 'Subject)
    : Map<string, Set<IndexValue> * Option<'OpError>> =

        let getNumericIndexKeyValue (key: string, primitive: IndexedPrimitiveNumber<'OpError>) =
            let valueInt, uniqueViolationError = match primitive with | IndexedNumber i -> (i, None) | UniqueIndexedNumber (i, e) -> (i, Some e)
            let value = NumIndexValue valueInt
            (key, value, uniqueViolationError)

        let getStringIndexKeyValue (key: string, primitive: IndexedPrimitiveString<'OpError>) =
            let valueStr, uniqueViolationError = match primitive with | IndexedString i -> (i, None) | UniqueIndexedString (i, e) -> (i, Some e)
            let value = StrIndexValue valueStr
            (key, value, uniqueViolationError)

        let getSearchIndexKeyValue (key: string, primitive: IndexedPrimitiveSearchableText) =
            let (IndexedPrimitiveSearchableText searchableText) = primitive
            let value = SearchIndexValue searchableText
            (key, value, None)

        let getGeographyIndexKeyValue (key: string, primitive: IndexedPrimitiveGeography) =
            let (IndexedPrimitiveGeography geometry) = primitive
            let value = GeographyIndexValue (geometry.ToWkt())
            (key, value, None)

        lifeCycle.Invoke
            { new FullyTypedLifeCycleFunction<_, _, _, _, _, _, _> with
                member _.Invoke lifeCycle =
                    lifeCycle.Indices subj
                    |> Seq.collect (fun index -> [
                            yield! (index.MaybeKeyAndPrimitiveNumber         |> Option.map getNumericIndexKeyValue   |> Option.toList)
                            yield! (index.MaybeKeyAndPrimitiveString         |> Option.map getStringIndexKeyValue    |> Option.toList)
                            yield! (index.MaybeKeyAndPrimitiveSearchableText |> Option.map getSearchIndexKeyValue    |> Option.toList)
                            yield! (index.MaybeKeyAndPrimitiveGeography      |> Option.map getGeographyIndexKeyValue |> Option.toList)
                        ])
                        |> Seq.groupBy (fun (key, _, _) -> key)
                        |> Seq.map (
                            fun (groupKey, grouping) ->
                                groupKey,
                                (
                                    Seq.map (fun (_, value, _) -> value) grouping |> Set.ofSeq,
                                    // just assume that unique index is one in the list and get first violation error
                                    grouping |> Seq.head |> fun (_, _, uniqueViolationError) -> uniqueViolationError
                                ))
                    |> Map.ofSeq }

let internal getPromotedIndexEntries
    (lifeCycle: ILifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>)
    (baseIndexEntries: Map<string, Set<IndexValue> * Option<'OpError>>)
    : Set<(PromotedKey * PromotedValue) * (BaseKey * Choice<int64, BaseValue>)> =

        match lifeCycle.Storage.Type with
        | StorageType.Custom _
        | StorageType.Volatile ->
            Set.empty
        | StorageType.Persistent (promotedIndicesConfig, _) ->

            let filterBaseIndices (baseIndices: Map<string, Set<IndexValue> * Option<'OpError>>) : Map<BaseKey, Set<Choice<int64, BaseValue>>> =
                baseIndices
                |> Map.mapKeys BaseKey
                |> Map.map (fun _ (values, _maybeOpError) -> values)
                |> Map.filterMapValues (
                    fun _key valueSet ->
                        let filteredValues =
                            valueSet
                            |> Set.filterMap (
                                fun (value: IndexValue) ->
                                    match value with
                                    | NumIndexValue valueInt -> Some <| Choice1Of2 valueInt
                                    | StrIndexValue valueStr -> Some <| Choice2Of2 (BaseValue valueStr)
                                    | SearchIndexValue _
                                    | GeographyIndexValue _ -> None)

                        if filteredValues.IsNonempty then Some filteredValues else None)

            let getPromotedIndices (baseIndices: Map<BaseKey, Set<Choice<int64, BaseValue>>>) : Map<PromotedKey, Set<PromotedValue>> =
                let baseIndices = baseIndices |> Map.map (fun _ values -> values |> Set.map (function | Choice1Of2 valueInt -> BaseValue (string valueInt) | Choice2Of2 (BaseValue valueStr) -> BaseValue valueStr))
                promotedIndicesConfig.Mappings
                |> Map.filterMapValues (
                    fun _promotedIndexKey promotedBaseKeysAndSeparators ->
                        let promotedBaseValuesAndSeparators =
                            promotedBaseKeysAndSeparators.ToList
                            |> List.map (function | Choice1Of2 baseKey -> Map.tryFind baseKey baseIndices |> Option.map Choice1Of2 | Choice2Of2 sep -> Some (Choice2Of2 sep))
                            |> Option.flattenList

                        if promotedBaseValuesAndSeparators.Length = promotedBaseKeysAndSeparators.Length then
                            // have at least 1 value for each base index key that makes up promoted index, so create promoted index values

                            let concatenatePermutations (values1: Set<string>) (values2: Set<string>) : Set<string> =
                                List.allPairs
                                    (values1 |> Set.toList)
                                    (values2 |> Set.toList)
                                |> List.map (fun (v1, v2) -> v1 + v2)
                                |> Set.ofList

                            let permutationsInput = promotedBaseValuesAndSeparators |> List.map (function | Choice1Of2 baseValueSet -> baseValueSet |> Set.map (function BaseValue v -> v) | Choice2Of2 (BaseSeparator sep) -> Set.ofOneItem sep)

                            List.fold concatenatePermutations permutationsInput.Head permutationsInput.Tail
                            |> Set.map PromotedValue
                            |> Some
                        else
                            // missing at least 1 base index value, so do not create promoted index value
                            None)

            let filteredBaseIndices = filterBaseIndices baseIndexEntries
            let promotedIndices = getPromotedIndices filteredBaseIndices

            let flattenIndicesMap (indices: Map<'K, Set<'V>>) = indices |> Map.toList |> List.collect (fun (key, values) -> Set.toList values |> List.map (fun value -> key, value))

            List.allPairs // write value for each index into every promoted index table
                (promotedIndices |> flattenIndicesMap)
                (filteredBaseIndices |> flattenIndicesMap)
            |> Set.ofList

let getIndexActions
        (lifeCycle: ILifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>)
        (updatedSubject: 'Subject)
        (maybeCurrentSubject: Option<'Subject>)
        : list<IndexAction<'OpError>> =

        let getPromotedIndexActions oldIndices newIndices =
            let toInsertPromotedIndexActions =
                Set.toList
                >> List.map (
                    fun ((PromotedKey promotedKey, PromotedValue promotedValue), (BaseKey baseKey, baseValue)) ->
                        match baseValue with
                        | Choice1Of2 valueInt ->
                            IndexAction.PromotedInsertNumeric (promotedKey, promotedValue, baseKey, valueInt)
                        | Choice2Of2 (BaseValue valueStr) ->
                            IndexAction.PromotedInsertString (promotedKey, promotedValue, baseKey, valueStr))

            let toDeletePromotedIndexActions =
                Set.toList
                >> List.map (
                    fun ((PromotedKey promotedKey, PromotedValue promotedValue), (BaseKey baseKey, baseValue)) ->
                        match baseValue with
                        | Choice1Of2 valueInt ->
                            IndexAction.PromotedDeleteNumeric (promotedKey, promotedValue, baseKey, valueInt)
                        | Choice2Of2 (BaseValue valueStr) ->
                            IndexAction.PromotedDeleteString (promotedKey, promotedValue, baseKey, valueStr))

            let oldPromotedIndices = getPromotedIndexEntries lifeCycle oldIndices
            let newPromotedIndices = getPromotedIndexEntries lifeCycle newIndices
            let inserts = Set.difference newPromotedIndices oldPromotedIndices |> toInsertPromotedIndexActions
            let deletes = Set.difference oldPromotedIndices newPromotedIndices |> toDeletePromotedIndexActions

            inserts @ deletes


        let newIndices = getIndexEntriesForSubject lifeCycle updatedSubject

        let oldIndices =
            match maybeCurrentSubject with
            | Some subj -> getIndexEntriesForSubject lifeCycle subj
            | None -> Map.empty

        let indexActionsList = System.Collections.Generic.List<IndexAction<'OpError>>()

        let addIndexAction isDelete key (uniqueViolationError: Option<'OpError>) (indexValue: IndexValue) =
            let indexAction =
                match isDelete, indexValue with
                | false, StrIndexValue str -> IndexAction.InsertString (key, str, uniqueViolationError)
                | true,  StrIndexValue str -> IndexAction.DeleteString (key, str, uniqueViolationError.IsSome)
                | false, NumIndexValue num -> IndexAction.InsertNumeric (key, num, uniqueViolationError)
                | true,  NumIndexValue num -> IndexAction.DeleteNumeric (key, num, uniqueViolationError.IsSome)
                | false, SearchIndexValue str -> IndexAction.InsertSearch (key, str)
                | true,  SearchIndexValue str -> IndexAction.DeleteSearch (key, str)
                | false, GeographyIndexValue str -> IndexAction.InsertGeography (key, str)
                | true,  GeographyIndexValue str -> IndexAction.DeleteGeography (key, str)
            indexActionsList.Add(indexAction)

        newIndices
        |> Map.fold (
            fun oldIndices key (indexValues, uniqueViolationError) ->
                match Map.tryFind key oldIndices with
                | None ->
                    indexValues
                    |> Seq.iter (addIndexAction false key uniqueViolationError)

                    oldIndices

                | Some (oldValues, oldUniqueViolationError) ->
                    indexValues
                    |> Seq.fold (
                        fun (oldValues: Set<_>) indexValue ->
                            if oldValues.Contains indexValue then
                                oldValues.Remove indexValue
                            else
                                addIndexAction false key oldUniqueViolationError indexValue
                                oldValues
                        ) oldValues
                    |> Seq.iter (addIndexAction true key oldUniqueViolationError)

                    oldIndices.Remove key

            ) oldIndices
        // Remove the left over old entries
        |> Map.iter (
            fun key (indexValues, oldUniqueViolationError) ->
                indexValues
                |> Seq.iter (addIndexAction true key oldUniqueViolationError)
            )

        match indexActionsList |> List.ofSeq with
        | [] -> [] // optimization: skip promoted calculation if we have no changed base indices
        | actions -> actions |> List.append (getPromotedIndexActions oldIndices newIndices)