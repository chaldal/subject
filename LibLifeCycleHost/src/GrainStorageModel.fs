module LibLifeCycleHost.GrainStorageModel

open System
open LibLifeCycle.LifeCycle
open LibLifeCycle.LifeCycles.Meta
open LibLifeCycleCore

[<RequireQualifiedAccess>]
type SubjectStateContainer<'Subject, 'Constructor, 'SubjectId, 'LifeEvent, 'LifeAction, 'OpError
                             when 'Subject      :> Subject<'SubjectId>
                             and  'Constructor  :> Constructor
                             and  'LifeEvent    :> LifeEvent
                             and  'LifeEvent    : comparison
                             and  'LifeAction   :> LifeAction
                             and  'SubjectId    :> SubjectId
                             and 'SubjectId : comparison
                             and 'OpError       :> OpError> =

    | PreparedInitialize of Prepared: PreparedSubjectInsertData<'Subject, 'Constructor, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError> * ETag: string * TransactionId: SubjectTransactionId
    | PreparedAction     of Current: SubjectCurrentStateContainer<'Subject, 'SubjectId, 'LifeEvent, 'LifeAction> * Prepared: PreparedSubjectUpdateData<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError> * TransactionId: SubjectTransactionId
    | Committed          of Current: SubjectCurrentStateContainer<'Subject, 'SubjectId, 'LifeEvent, 'LifeAction>

with
    member this.SubjectId =
        match this with
        | PreparedInitialize (insertData, _, _) ->
            insertData.PreparedDataToInsert.UpdatedSubjectState.Subject.SubjectId
        | PreparedAction (current, _, _)
        | Committed current ->
            current.CurrentSubjectState.Subject.SubjectId

    member this.CurrentVersionedSubject: VersionedSubject<'Subject, 'SubjectId> =
        match this with
        | SubjectStateContainer.PreparedInitialize _ ->
            failwith "VersionedSubject cannot be determined for PreparedInitialize state"
        | SubjectStateContainer.PreparedAction (current, _, _)
        | SubjectStateContainer.Committed current ->
            current.VersionedSubject

and SideEffectDedupCache = Map<SubjectPKeyReference, GrainSideEffectId * DateTimeOffset>

and SubjectCurrentStateContainer<'Subject, 'SubjectId, 'LifeEvent, 'LifeAction
                             when 'Subject      :> Subject<'SubjectId>
                             and  'LifeEvent    :> LifeEvent
                             and  'LifeEvent    : comparison
                             and  'LifeAction  :> LifeAction
                             and  'SubjectId    :> SubjectId
                             and 'SubjectId : comparison> = {
    CurrentSubjectState:      SubjectState<'Subject, 'SubjectId>
    CurrentOthersSubscribing: OthersSubscribing<'LifeEvent>
    ETag:                     string
    Version:                  uint64
    NextSideEffectSeqNum:     GrainSideEffectSequenceNumber
    SideEffectDedupCache:     SideEffectDedupCache
    SkipHistoryOnNextOp:      bool
}
with
    member this.VersionedSubject: VersionedSubject<'Subject, 'SubjectId> =
        {
            Subject = this.CurrentSubjectState.Subject
            AsOf = this.CurrentSubjectState.LastUpdatedOn
            Version = this.Version
        }

and SideEffectGroup<'LifeAction, 'OpError when 'LifeAction :> LifeAction and 'OpError :> OpError> = {
    SequenceNumber: GrainSideEffectSequenceNumber
    SideEffects: NonemptyMap<GrainSideEffectId, GrainSideEffect<'LifeAction, 'OpError>>
    RehydratedFromStorage: bool
}
with
    interface IKeyed<GrainSideEffectSequenceNumber> with
        member this.Key = this.SequenceNumber

and GrainSideEffectSequenceNumber = uint64

and UpdatePermanentFailuresResult<'LifeAction when 'LifeAction :> LifeAction> = {
    Last: LastUpdatePermanentFailuresResult
    SideEffectIdsToRetry: Map<string, NonemptySet<GrainSideEffectId>> // failed side effects by subjectIdStr
}


and SubjectWriteData<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError
                      when 'Subject     :> Subject<'SubjectId>
                      and  'SubjectId   :> SubjectId
                      and 'SubjectId : comparison
                      and  'LifeAction  :> LifeAction
                      and  'LifeEvent   :> LifeEvent
                      and  'LifeEvent   : comparison
                      and  'OpError     :> OpError> = {
    UpdatedSubjectState: SubjectState<'Subject, 'SubjectId>
    TraceContext:        TraceContext
    ReminderUpdate:   Option<ReminderUpdate>
    NextSideEffectSeq:   GrainSideEffectSequenceNumber
    SideEffectGroup:     Option<SideEffectGroup<'LifeAction, 'OpError>>
    BlobActions:         List<BlobAction>
    IndexActions:        List<IndexAction<'OpError>>
}
with
    member this.TruncatedOperationBy(maxLen: uint8) =
        let operationBy = this.TraceContext.TelemetryUserId
        let len = int maxLen
        if operationBy.Length <= len then
            operationBy
        else
            operationBy.Substring len

    member this.TryCastToPrepared<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError> (raisedLifeEvents: list<'LifeEvent>) :
        Result<PreparedSubjectWriteData<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError> * list<UniqueIndexToReserveOnPrepare<'OpError>>, PrepareWriteDataError> =
        let codecFriendlyPersistedSideEffectsResult =
            this.SideEffectGroup
            |> Option.map (fun g -> g.SideEffects.Values |> List.ofSeq)
            |> Option.defaultValue []
            |> List.fold
                (fun state sideEffect ->
                     match state with
                     | Ok codecFriendlySideEffects ->
                         match sideEffect with
                         | GrainSideEffect.Transient _ ->
                             Error PrepareWriteDataError.TransientSideEffectsNotAllowed
                         | GrainSideEffect.Persisted persistedSideEffect ->
                             match persistedSideEffect with
                             | RpcTriggerSubscriptionOnGrain _ ->
                                 // don't add TriggerSubscription side effects to persisted side effects, they will be calculated just-in-time upon commit
                                 Ok codecFriendlySideEffects
                             | _ ->
                                 match persistedSideEffect.AsCodecFriendlyDataUnsafeForTriggerSubscriptionOnGrain with // Safe because we filterd out TriggerSubscriptionOnGrain case above
                                 | Choice1Of2 x ->
                                    Ok (x :: codecFriendlySideEffects)
                                 | Choice2Of2 _ ->
                                    // Ingest time series not supported in transactions just yet,
                                    // but it can and should be later (when all persisted side effects migrate to strongly-typed encoding)
                                    Error PrepareWriteDataError.TransientSideEffectsNotAllowed
                     | Error PrepareWriteDataError.TransientSideEffectsNotAllowed ->
                         state)
                (Ok [])

        let preparedIndexActions =
            let baseActions =
                this.IndexActions
                |> List.choose (
                    function
                    | IndexAction.InsertNumeric (key, value, uniqueViolationError) -> Some <| PreparedIndexAction.InsertNumeric (key, value, uniqueViolationError.IsSome)
                    | IndexAction.InsertString (key, value, uniqueViolationError) -> Some <| PreparedIndexAction.InsertString (key, value, uniqueViolationError.IsSome)
                    | IndexAction.InsertSearch (key, value) -> Some <| PreparedIndexAction.InsertSearch (key, value)
                    | IndexAction.InsertGeography (key, wktValue) -> Some <| PreparedIndexAction.InsertGeography (key, wktValue)
                    | IndexAction.DeleteNumeric (key, value, isUnique) -> Some <| PreparedIndexAction.DeleteNumeric (key, value, isUnique)
                    | IndexAction.DeleteString (key, value, isUnique) -> Some <| PreparedIndexAction.DeleteString (key, value, isUnique)
                    | IndexAction.DeleteSearch (key, value) -> Some <| PreparedIndexAction.DeleteSearch (key, value)
                    | IndexAction.DeleteGeography (key, wktValue) -> Some <| PreparedIndexAction.DeleteGeography (key, wktValue)
                    | IndexAction.PromotedInsertNumeric _
                    | IndexAction.PromotedDeleteNumeric _
                    | IndexAction.PromotedInsertString _
                    | IndexAction.PromotedDeleteString _ ->
                        None)

            let promotedActions =
                this.IndexActions
                |> List.choose (
                    function
                    | IndexAction.InsertNumeric _
                    | IndexAction.InsertString _
                    | IndexAction.InsertSearch _
                    | IndexAction.DeleteNumeric _
                    | IndexAction.DeleteString _
                    | IndexAction.DeleteSearch _
                    | IndexAction.InsertString _
                    | IndexAction.InsertNumeric _
                    | IndexAction.InsertGeography _
                    | IndexAction.DeleteGeography _ ->
                        None
                    | IndexAction.PromotedInsertNumeric (promotedKey, promotedValue, baseKey, baseValue) -> Some (Choice1Of2 (promotedKey, promotedValue), Choice1Of2 (baseKey, baseValue))
                    | IndexAction.PromotedDeleteNumeric (promotedKey, promotedValue, baseKey, baseValue) -> Some (Choice2Of2 (promotedKey, promotedValue), Choice1Of2 (baseKey, baseValue))
                    | IndexAction.PromotedInsertString (promotedKey, promotedValue, baseKey, baseValue) -> Some (Choice1Of2 (promotedKey, promotedValue), Choice2Of2 (baseKey, baseValue))
                    | IndexAction.PromotedDeleteString (promotedKey, promotedValue, baseKey, baseValue) -> Some (Choice2Of2 (promotedKey, promotedValue), Choice2Of2 (baseKey, baseValue)))
                |> List.groupBy fst
                |> List.map (fun (insertOrDelete, baseValues) ->
                    let baseValues = baseValues |> List.map snd
                    let baseNumerics = baseValues |> List.choose (function Choice1Of2 (baseKey, baseValue) -> Some (baseKey, baseValue) | Choice2Of2 _ -> None)
                    let baseStrings = baseValues |> List.choose (function Choice1Of2 _ -> None | Choice2Of2 (baseKey, baseValue) -> Some (baseKey, baseValue))
                    match insertOrDelete with
                    | Choice1Of2 (promotedKey, promotedValue) ->
                        PreparedIndexAction.PromotedInsert (promotedKey, promotedValue, baseNumerics, baseStrings)
                    | Choice2Of2 (promotedKey, promotedValue) ->
                        PreparedIndexAction.PromotedDelete (promotedKey, promotedValue, baseNumerics, baseStrings))

            baseActions @ promotedActions

        let uniqueIndicesToReserve =
            this.IndexActions
            |> List.choose (
                function
                | IndexAction.InsertNumeric (key, value, Some uniqueViolationError) -> Some <| UniqueIndexToReserveOnPrepare.Numeric (key, value, uniqueViolationError)
                | IndexAction.InsertString (key, value, Some uniqueViolationError) -> Some <| UniqueIndexToReserveOnPrepare.String (key, value, uniqueViolationError)
                | IndexAction.InsertNumeric (_, _, None)
                | IndexAction.InsertString (_, _, None)
                | IndexAction.InsertSearch _
                | IndexAction.InsertGeography _
                | IndexAction.DeleteNumeric _
                | IndexAction.DeleteString _
                | IndexAction.DeleteSearch _
                | IndexAction.DeleteGeography _
                | IndexAction.PromotedInsertNumeric _
                | IndexAction.PromotedDeleteNumeric _
                | IndexAction.PromotedInsertString _
                | IndexAction.PromotedDeleteString _ ->
                    None)

        match codecFriendlyPersistedSideEffectsResult with
        | Error err ->
            Error err
        | Ok codecFriendlyPersistedSideEffects ->
            // all good
            Ok <|
            (
                {
                    UpdatedSubjectState  = this.UpdatedSubjectState
                    TraceContext         = this.TraceContext
                    ReminderUpdate       = this.ReminderUpdate
                    PersistedSideEffects = codecFriendlyPersistedSideEffects
                    BlobActions          = this.BlobActions
                    IndexActions         = preparedIndexActions
                    RaisedLifeEvents     = raisedLifeEvents
                },
                uniqueIndicesToReserve
            )

and PreparedSubjectWriteData<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError
                      when 'Subject     :> Subject<'SubjectId>
                      and  'SubjectId   :> SubjectId
                      and  'SubjectId : comparison
                      and  'LifeAction  :> LifeAction
                      and  'LifeEvent   :> LifeEvent
                      and  'LifeEvent   : comparison
                      and  'OpError     :> OpError> = {
    UpdatedSubjectState:  SubjectState<'Subject, 'SubjectId>
    TraceContext:         TraceContext
    ReminderUpdate:    Option<ReminderUpdate>
    // transactions don't support transient side effects, as we can't guarantee that they will commit.
    // Also they can't be serialized
    PersistedSideEffects: List<CodecFriendlyGrainPersistedSideEffect<'LifeAction, 'OpError>>
    BlobActions:          List<BlobAction>
    IndexActions:         List<PreparedIndexAction>
    // capture raised life events to resolve trigger subscription side effects upon commit
    // Caution: it goes against idea of precalculating everything on prepare, is this OK?
    RaisedLifeEvents:     List<'LifeEvent>

    // TODO: consider explicitly assigning an initial version number and passing through to SQL.
}
with
    member this.Upcast<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError when 'OpError :> OpError>
        (sideEffectSeqNum: GrainSideEffectSequenceNumber)
        (overrideTraceContextParentId: string)
        (mapRaisedLifeEventsToTriggerSubscriptionSideEffects: TraceContext -> list<'LifeEvent> -> seq<GrainPersistedSideEffect<'LifeAction, 'OpError>>)
        : SubjectWriteData<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError> =

        let indexActions =
            this.IndexActions
            |> List.collect (
                function
                | PreparedIndexAction.InsertNumeric (key, value, isUnique) ->
                    // can just put a dummy 'OpError for unique insert because it's not going to be used upon commit, only the fact that it is unique matters
                    [IndexAction.InsertNumeric (key, value, if isUnique then Some Unchecked.defaultof<'OpError> else None)]
                | PreparedIndexAction.DeleteNumeric (key, value, isUnique) -> [IndexAction.DeleteNumeric (key, value, isUnique)]
                | PreparedIndexAction.InsertString (key, value, isUnique) ->
                    // can just put a dummy 'OpError for unique insert because it's not going to be used upon commit, only the fact that it is unique matters
                    [IndexAction.InsertString (key, value, if isUnique then Some Unchecked.defaultof<'OpError> else None)]
                | PreparedIndexAction.DeleteString (key, value, isUnique) -> [IndexAction.DeleteString (key, value, isUnique)]
                | PreparedIndexAction.InsertSearch (key, value) -> [IndexAction.InsertSearch (key, value)]
                | PreparedIndexAction.DeleteSearch (key, value) -> [IndexAction.DeleteSearch (key, value)]
                | PreparedIndexAction.InsertGeography (key, wktValue) -> [IndexAction.InsertGeography (key, wktValue)]
                | PreparedIndexAction.DeleteGeography (key, wktValue) -> [IndexAction.DeleteGeography (key, wktValue)]
                | PreparedIndexAction.PromotedInsert (promotedKey, promotedValue, numericKeyValues, stringKeyValues) ->
                    [
                        yield! numericKeyValues |> Seq.map (fun (baseKey, baseValue) -> IndexAction.PromotedInsertNumeric (promotedKey, promotedValue, baseKey, baseValue))
                        yield! stringKeyValues |> Seq.map (fun (baseKey, baseValue) -> IndexAction.PromotedInsertString (promotedKey, promotedValue, baseKey, baseValue))
                    ]
                | PreparedIndexAction.PromotedDelete (promotedKey, promotedValue, numericKeyValues, stringKeyValues) ->
                    [
                        yield! numericKeyValues |> Seq.map (fun (baseKey, baseValue) -> IndexAction.PromotedDeleteNumeric (promotedKey, promotedValue, baseKey, baseValue))
                        yield! stringKeyValues |> Seq.map (fun (baseKey, baseValue) -> IndexAction.PromotedDeleteString (promotedKey, promotedValue, baseKey, baseValue))
                    ])

        let overrideTraceContext = { this.TraceContext with ParentId = overrideTraceContextParentId }

        // Caution: new guid impurity!
        let persistedSideEffects, deducedReminderUpdateFromSideEffects =
            let choices =
                // side effects must appear under Commit, not under Prepare
                this.PersistedSideEffects
                |> Seq.map (fun se -> se.AsGrainPersistedSideEffect (Some overrideTraceContextParentId))
                |> List.ofSeq
            choices |> List.map fst,
            choices |> List.choose snd |> List.tryLast

        let sideEffectGroup =
            // calculate TriggerSubscription side effects just in time, in case if new subscribers appeared while state was prepared
            Seq.append persistedSideEffects (mapRaisedLifeEventsToTriggerSubscriptionSideEffects overrideTraceContext this.RaisedLifeEvents)
            |> Seq.map (fun se -> Guid.NewGuid (), GrainSideEffect.Persisted se)
            |> NonemptyMap.ofSeq
            |> Option.map (fun g -> { SideEffects = g; SequenceNumber = sideEffectSeqNum; RehydratedFromStorage = false })

        {
            UpdatedSubjectState = this.UpdatedSubjectState
            TraceContext        = overrideTraceContext
            ReminderUpdate      = this.ReminderUpdate |> Option.orElse deducedReminderUpdateFromSideEffects
            NextSideEffectSeq   = sideEffectSeqNum + (match sideEffectGroup with Some _ -> 1UL | None -> 0UL)
            SideEffectGroup     = sideEffectGroup
            BlobActions         = this.BlobActions
            IndexActions        = indexActions
        }

    member this.UniqueIndicesToReleaseOnRollback =
        this.IndexActions
        |> List.choose (
            function
            | PreparedIndexAction.InsertNumeric (key, value, (* isUnique *) true) ->
                Some <| UniqueIndexToReleaseOnRollback.Numeric (key, value)
            | PreparedIndexAction.InsertString (key, value, (* isUnique *) true) ->
                Some <| UniqueIndexToReleaseOnRollback.String (key, value)
            | PreparedIndexAction.InsertNumeric (_, _, (* isUnique *) false)
            | PreparedIndexAction.InsertString (_, _, (* isUnique *) false)
            | PreparedIndexAction.DeleteNumeric _
            | PreparedIndexAction.DeleteString _
            | PreparedIndexAction.InsertSearch _
            | PreparedIndexAction.DeleteSearch _
            | PreparedIndexAction.InsertGeography _
            | PreparedIndexAction.DeleteGeography _
            | PreparedIndexAction.PromotedInsert _
            | PreparedIndexAction.PromotedDelete _ ->
                None)

and [<RequireQualifiedAccess>] PrepareWriteDataError =
| TransientSideEffectsNotAllowed // TODO: make connector request side effect serializable so it can be prepared too

and [<RequireQualifiedAccess>] IndexAction<'OpError when 'OpError :> OpError> =
    | InsertNumeric   of Key: string * Value: int64 * UniqueViolationError: Option<'OpError>
    | DeleteNumeric   of Key: string * Value: int64 * IsUnique: bool
    | InsertString    of Key: string * Value: string * UniqueViolationError: Option<'OpError>
    | DeleteString    of Key: string * Value: string * IsUnique: bool
    | InsertSearch    of Key: string * Value: string
    | DeleteSearch    of Key: string * Value: string
    | InsertGeography of Key: string * WktValue: string
    | DeleteGeography of Key: string * WktValue: string

    | PromotedInsertNumeric of PromotedKey: string * PromotedValue: string * BaseKey: string * BaseValue: int64
    | PromotedDeleteNumeric of PromotedKey: string * PromotedValue: string * BaseKey: string * BaseValue: int64
    | PromotedInsertString  of PromotedKey: string * PromotedValue: string * BaseKey: string * BaseValue: string
    | PromotedDeleteString  of PromotedKey: string * PromotedValue: string * BaseKey: string * BaseValue: string

and [<RequireQualifiedAccess>] PreparedIndexAction =
    // can't insert unique indices inside transaction, this is by design, to guarantee clean commit
    | InsertNumeric   of Key: string * Value: int64 * IsUnique: bool
    | DeleteNumeric   of Key: string * Value: int64 * IsUnique: bool
    | InsertString    of Key: string * Value: string * IsUnique: bool
    | DeleteString    of Key: string * Value: string * IsUnique: bool
    | InsertSearch    of Key: string * Value: string
    | DeleteSearch    of Key: string * Value: string
    | InsertGeography of Key: string * WktValue: string
    | DeleteGeography of Key: string * WktValue: string
    // special representation of promoted indices for more compact representation when serialized
    | PromotedInsert of PromotedKey: string * PromotedValue: string * NumericKeyValues: List<string * int64> * StringKeyValues: List<string * string>
    | PromotedDelete of PromotedKey: string * PromotedValue: string * List<string * int64> * StringKeyValues: List<string * string>

and [<RequireQualifiedAccess>] UniqueIndexToReserveOnPrepare<'OpError when 'OpError :> OpError> =
    | Numeric of Key: string * Value: int64 * UniqueViolationError: 'OpError
    | String  of Key: string * Value: string * UniqueViolationError: 'OpError

and [<RequireQualifiedAccess>] UniqueIndexToReleaseOnRollback =
    | Numeric of Key: string * Value: int64
    | String  of Key: string * Value: string

and SubscriptionsToAdd<'LifeEvent when 'LifeEvent :> LifeEvent and 'LifeEvent : comparison> = {
    Subscriber: SubjectPKeyReference
    NewSubscriptions: Map<SubscriptionName, 'LifeEvent>
}

and SubjectInsertData<'Subject, 'Constructor, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError
                       when 'Subject     :> Subject<'SubjectId>
                       and  'Constructor :> Constructor
                       and  'LifeAction  :> LifeAction
                       and  'LifeEvent   :> LifeEvent
                       and  'LifeEvent   : comparison
                       and  'SubjectId   :> SubjectId
                       and 'SubjectId : comparison
                       and  'OpError     :> OpError> = {
    DataToInsert:                SubjectWriteData<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>
    CreatorSubscribing:          OthersSubscribing<'LifeEvent>
    ConstructorThatCausedInsert: 'Constructor
    GrainIdHash:                 uint32
}
with
    member this.TryCastToPrepared<'Subject, 'Constructor, 'SubjectId, 'LifeAction, 'LifeEvent>(raisedLifeEvents: list<'LifeEvent>)
            : Result<PreparedSubjectInsertData<'Subject, 'Constructor, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError> * list<UniqueIndexToReserveOnPrepare<'OpError>>, PrepareWriteDataError> =
        this.DataToInsert.TryCastToPrepared raisedLifeEvents
        |> Result.map (
            fun (preparedWriteData, uniqueIndicesToReserve) ->
                {
                    PreparedDataToInsert        = preparedWriteData
                    ConstructorThatCausedInsert = this.ConstructorThatCausedInsert
                    GrainIdHash                 = this.GrainIdHash
                },
                uniqueIndicesToReserve)
and PreparedSubjectInsertData<'Subject, 'Constructor, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError
                       when 'Subject     :> Subject<'SubjectId>
                       and  'Constructor :> Constructor
                       and  'LifeAction  :> LifeAction
                       and  'LifeEvent   :> LifeEvent
                       and  'LifeEvent   : comparison
                       and  'SubjectId   :> SubjectId
                       and 'SubjectId : comparison
                       and 'OpError      :> OpError> = {
    PreparedDataToInsert:        PreparedSubjectWriteData<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>
    ConstructorThatCausedInsert: 'Constructor
    GrainIdHash:                 uint32
}
with
    member this.Upcast<'Subject, 'Constructor, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError
                        when 'OpError :> OpError
                        and  'LifeEvent :> LifeEvent
                        and  'LifeEvent :  comparison>
                        (overrideTraceContextParentId: string)
        : SubjectInsertData<'Subject, 'Constructor, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError> =
        {
            DataToInsert                = this.PreparedDataToInsert.Upcast 0UL overrideTraceContextParentId (fun _ _ -> [])
            CreatorSubscribing          = Map.empty
            ConstructorThatCausedInsert = this.ConstructorThatCausedInsert
            GrainIdHash                 = this.GrainIdHash
        }

and SubjectUpdateData<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError
                       when 'Subject     :> Subject<'SubjectId>
                       and  'LifeAction  :> LifeAction
                       and  'LifeEvent   :> LifeEvent
                       and  'SubjectId   :> SubjectId
                       and 'SubjectId : comparison
                       and  'LifeEvent   : comparison
                       and  'OpError     :> OpError> = {
    DataToUpdate:           SubjectWriteData<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>
    ActionThatCausedUpdate: 'LifeAction
    SubscriptionsToAdd:     Option<SubscriptionsToAdd<'LifeEvent>>
    ExpectedETag:           string
    CurrentVersion:         uint64
    SkipHistory:            bool
}
with
    member this.TryCastToPrepared<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent>(raisedLifeEvent: list<'LifeEvent>)
            : Result<PreparedSubjectUpdateData<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError> * list<UniqueIndexToReserveOnPrepare<'OpError>>, PrepareWriteDataError> =
        this.DataToUpdate.TryCastToPrepared raisedLifeEvent
        |> Result.map (
            fun (preparedWriteData, uniqueIndicesToReserveOnPrepare) ->
                {
                    PreparedDataToUpdate   = preparedWriteData
                    ActionThatCausedUpdate = this.ActionThatCausedUpdate
                    SkipHistory            = this.SkipHistory
                },
                uniqueIndicesToReserveOnPrepare)

and PreparedSubjectUpdateData<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError
                       when 'Subject     :> Subject<'SubjectId>
                       and  'LifeAction  :> LifeAction
                       and  'LifeEvent   :> LifeEvent
                       and  'LifeEvent   : comparison
                       and  'SubjectId   :> SubjectId
                       and 'SubjectId : comparison
                       and  'LifeEvent   : comparison
                       and  'OpError     :> OpError> = {
    PreparedDataToUpdate:   PreparedSubjectWriteData<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>
    ActionThatCausedUpdate: 'LifeAction
    SkipHistory:            bool
}
with
    member this.Upcast<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError when 'OpError :> OpError>
        (expectedETag: string)
        (currentVersion: uint64)
        (sideEffectSeqNum: GrainSideEffectSequenceNumber)
        (overrideTraceContextParentId: string)
        (mapRaisedLifeEventsToTriggerSubscriptionSideEffects: TraceContext -> list<'LifeEvent> -> seq<GrainPersistedSideEffect<'LifeAction, 'OpError>>)
        : SubjectUpdateData<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError> =
        {
            DataToUpdate           = this.PreparedDataToUpdate.Upcast sideEffectSeqNum overrideTraceContextParentId mapRaisedLifeEventsToTriggerSubscriptionSideEffects
            ExpectedETag           = expectedETag
            CurrentVersion         = currentVersion
            ActionThatCausedUpdate = this.ActionThatCausedUpdate
            SubscriptionsToAdd     = None
            SkipHistory            = this.SkipHistory
        }

type UpsertDedupData = {
    DedupInfo: SideEffectDedupInfo
    IsInsert: bool
    EvictedDedupInfoToDelete: Option<SideEffectDedupInfo * DateTimeOffset>
}

type UpdateSubjectSuccessResult = {
    NewETag: string
    NewVersion: uint64
    SkipHistoryOnNextOp: bool
}

type ReadStateResult<'Subject, 'Constructor, 'LifeAction, 'LifeEvent, 'OpError, 'SubjectId
                      when 'Subject     :> Subject<'SubjectId>
                      and  'Constructor :> Constructor
                      and  'LifeAction  :> LifeAction
                      and  'LifeEvent   :> LifeEvent
                      and  'LifeEvent   : comparison
                      and  'SubjectId   :> SubjectId
                      and  'SubjectId   : comparison
                      and  'OpError     :> OpError> = {
    SubjectStateContainer: SubjectStateContainer<'Subject, 'Constructor,'SubjectId, 'LifeEvent, 'LifeAction, 'OpError>
    PendingSideEffects:    KeyedSet<GrainSideEffectSequenceNumber, SideEffectGroup<'LifeAction, 'OpError>>
    PersistedGrainIdHash:  Option<uint32>
}

type InitializeSubjectSuccessResult = {
    ETag: string
    Version: uint64
    SkipHistoryOnNextOp: bool
}

// CODECs

#if !FABLE_COMPILER

open CodecLib

type PreparedIndexAction with
    static member get_Codec () =
        function
        | InsertNumeric _ ->
            codec {
                let! payload = reqWith (Codecs.tuple3 Codecs.string Codecs.int64 Codecs.boolean) "InsNum" (function InsertNumeric (x1, x2, x3) -> Some (x1, x2, x3) | _ -> None)
                return InsertNumeric payload
            }
            |> withDecoders [
                decoder {
                    let! (x1, x2) = reqDecodeWithCodec (Codecs.tuple2 Codecs.string Codecs.int64) "InsNum"
                    return InsertNumeric (x1, x2, false)
                }
            ]
        | DeleteNumeric _ ->
            codec {
                let! payload = reqWith (Codecs.tuple3 Codecs.string Codecs.int64 Codecs.boolean) "DelNum" (function DeleteNumeric (x1, x2, x3) -> Some (x1, x2, x3) | _ -> None)
                return DeleteNumeric payload
            }
        | InsertString _  ->
            codec {
                let! payload = reqWith (Codecs.tuple3 Codecs.string Codecs.string Codecs.boolean) "InsStr" (function InsertString (x1, x2, x3) -> Some (x1, x2, x3) | _ -> None)
                return InsertString payload
            }
            |> withDecoders [
                decoder {
                    let! (x1, x2) = reqDecodeWithCodec (Codecs.tuple2 Codecs.string Codecs.string) "InsStr"
                    return InsertString (x1, x2, false)
                }
            ]
        | DeleteString _ ->
            codec {
                let! payload = reqWith (Codecs.tuple3 Codecs.string Codecs.string Codecs.boolean) "DelStr" (function DeleteString (x1, x2, x3) -> Some (x1, x2, x3) | _ -> None)
                return DeleteString payload
            }
        | InsertSearch _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 Codecs.string Codecs.string) "InsSearch" (function InsertSearch (x1, x2) -> Some (x1, x2) | _ -> None)
                return InsertSearch payload
            }
        | DeleteSearch _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 Codecs.string Codecs.string) "DelSearch" (function DeleteSearch (x1, x2) -> Some (x1, x2) | _ -> None)
                return DeleteSearch payload
            }
        | InsertGeography _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 Codecs.string Codecs.string) "InsGeography" (function InsertGeography (x1, x2) -> Some (x1, x2) | _ -> None)
                return InsertGeography payload
            }
        | DeleteGeography _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 Codecs.string Codecs.string) "DelGeography" (function DeleteGeography (x1, x2) -> Some (x1, x2) | _ -> None)
                return DeleteGeography payload
            }
        | PromotedInsert _ ->
            codec {
                let! payload = reqWith (Codecs.tuple4 Codecs.string Codecs.string (Codecs.list (Codecs.tuple2 Codecs.string Codecs.int64)) (Codecs.list (Codecs.tuple2 Codecs.string Codecs.string))) "PromIns" (function PromotedInsert (x1, x2, x3, x4) -> Some (x1, x2, x3, x4) | _ -> None)
                return PromotedInsert payload
            }
        | PromotedDelete _ ->
            codec {
                let! payload = reqWith (Codecs.tuple4 Codecs.string Codecs.string (Codecs.list (Codecs.tuple2 Codecs.string Codecs.int64)) (Codecs.list (Codecs.tuple2 Codecs.string Codecs.string))) "PromDel" (function PromotedDelete (x1, x2, x3, x4) -> Some (x1, x2, x3, x4) | _ -> None)
                return PromotedDelete payload
            }
        |> mergeUnionCases
        |> ofObjCodec

type PreparedSubjectWriteData<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError
                      when 'Subject     :> Subject<'SubjectId>
                      and  'SubjectId   :> SubjectId
                      and  'LifeAction  :> LifeAction
                      and  'LifeEvent   :> LifeEvent
                      and  'LifeEvent   :  comparison
                      and  'OpError     :> OpError> with
    // must be inline, or project won't compile
    static member inline get_Codec () : Codec<_, PreparedSubjectWriteData<'subject, 'subjectId, 'lifeAction, 'lifeEvent, 'opError>> =
        ofObjCodec <|
        codec {
            let! updatedSubjectState  = reqWith (SubjectState<'subject, 'subjectId>.get_Codec ()) "UpdatedSubjectState" (fun x -> Some x.UpdatedSubjectState)
            and! maybeOperationBy     = optWith Codecs.string "OperationBy" (fun _ -> None)
            and! maybeTraceContext    = optWith codecFor<_, TraceContext> "TraceContext" (fun x -> Some x.TraceContext)
            and! persistedSideEffects = reqWith (Codecs.list (CodecFriendlyGrainPersistedSideEffect<'lifeAction, 'opError>.get_Codec ())) "PersistedSideEffects" (fun x -> Some x.PersistedSideEffects)
            and! blobActions          = reqWith (Codecs.list (BlobAction.get_Codec ())) "BlobActions" (fun x -> Some x.BlobActions)
            and! indexActions         = reqWith (Codecs.list (PreparedIndexAction.get_Codec ())) "IndexActions" (fun x -> Some x.IndexActions)
            and! maybeRaisedEvents    = optWith (Codecs.list defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'lifeEvent>) "RaisedLifeEvents" (fun x -> Some x.RaisedLifeEvents)
            and! reminderUpdate       = optWith codecFor<_, ReminderUpdate> "ReminderUpdate" (fun x -> x.ReminderUpdate)
            let traceContext =
                maybeTraceContext
                |> Option.orElseWith (fun () -> maybeOperationBy |> Option.map (fun operationBy -> { TelemetryUserId = operationBy; TelemetrySessionId = None; ParentId = ""; }))
                |> Option.defaultValue emptyTraceContext
            return
                { UpdatedSubjectState = updatedSubjectState
                  TraceContext = traceContext
                  ReminderUpdate = reminderUpdate
                  PersistedSideEffects = persistedSideEffects
                  BlobActions = blobActions
                  IndexActions = indexActions
                  RaisedLifeEvents = maybeRaisedEvents |> Option.defaultValue [] }
        }

    static member CastUnsafe (data: PreparedSubjectWriteData<#Subject<'SubjectId>, 'SubjectId, #LifeAction, #LifeEvent, #OpError>) : PreparedSubjectWriteData<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError> =
        {
            UpdatedSubjectState  = SubjectState<'Subject, 'SubjectId>.CastUnsafe data.UpdatedSubjectState
            TraceContext         = data.TraceContext
            ReminderUpdate       = data.ReminderUpdate
            PersistedSideEffects = data.PersistedSideEffects |> List.map CodecFriendlyGrainPersistedSideEffect<'LifeAction, 'OpError>.CastUnsafe
            BlobActions          = data.BlobActions
            IndexActions         = data.IndexActions
            RaisedLifeEvents     = data.RaisedLifeEvents |> List.map (fun x -> x |> box :?> 'LifeEvent)
        }

type PreparedSubjectInsertData<'Subject, 'Constructor, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError
                       when 'Subject     :> Subject<'SubjectId>
                       and  'Constructor :> Constructor
                       and  'LifeAction  :> LifeAction
                       and  'LifeEvent   :> LifeEvent
                       and  'LifeEvent   :  comparison
                       and  'SubjectId   :> SubjectId
                       and  'OpError     :> OpError> with
     // must be inline, or project won't compile
     static member inline get_Codec () : Codec<_, PreparedSubjectInsertData<'subject, 'constructor, 'subjectId, 'lifeAction, 'lifeEvent, 'opError>> =
        ofObjCodec <|
        codec {
            let! preparedDataToInsert        = reqWith (PreparedSubjectWriteData<'subject, 'subjectId, 'lifeAction, 'lifeEvent, 'opError>.get_Codec ()) "PreparedDataToInsert" (fun x -> Some x.PreparedDataToInsert)
            and! constructorThatCausedInsert = reqWith defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'constructor> "ConstructorThatCausedInsert" (fun x -> Some x.ConstructorThatCausedInsert)
            and! grainIdHash                 = reqWith Codecs.uint32 "GrainIdHash" (fun x -> Some x.GrainIdHash)
            return { PreparedDataToInsert = preparedDataToInsert; ConstructorThatCausedInsert = constructorThatCausedInsert; GrainIdHash = grainIdHash }
        }

     static member CastUnsafe (data: PreparedSubjectInsertData<#Subject<'SubjectId>, #Constructor, 'SubjectId, #LifeAction, #LifeEvent, #OpError>) : PreparedSubjectInsertData<'Subject, 'Constructor, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError> =
        {
            PreparedDataToInsert        = PreparedSubjectWriteData<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>.CastUnsafe data.PreparedDataToInsert
            ConstructorThatCausedInsert = data.ConstructorThatCausedInsert |> box :?> 'Constructor
            GrainIdHash                 = data.GrainIdHash
        }

type PreparedSubjectUpdateData<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError
                       when 'Subject     :> Subject<'SubjectId>
                       and  'LifeAction  :> LifeAction
                       and  'LifeEvent   :> LifeEvent
                       and  'SubjectId   :> SubjectId
                       and  'LifeEvent   : comparison
                       and  'OpError     :> OpError> with
     // must be inline, or project won't compile
     static member inline get_Codec () : Codec<_, PreparedSubjectUpdateData<'subject, 'subjectId, 'lifeAction, 'lifeEvent, 'opError>> =
        ofObjCodec <|
        codec {
            let! preparedDataToUpdate   = reqWith (PreparedSubjectWriteData<'subject, 'subjectId, 'lifeAction, 'lifeEvent, 'opError>.get_Codec ()) "PreparedDataToUpdate" (fun x -> Some x.PreparedDataToUpdate)
            and! actionThatCausedUpdate = reqWith defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'lifeAction> "ActionThatCausedUpdate" (fun x -> Some x.ActionThatCausedUpdate)
            and! maybeSkipHistory       = optWith Codecs.boolean "SkipHistory" (fun x -> Some x.SkipHistory)
            return { PreparedDataToUpdate = preparedDataToUpdate; ActionThatCausedUpdate = actionThatCausedUpdate; SkipHistory = maybeSkipHistory |> Option.defaultValue false }
        }

     static member CastUnsafe (data: PreparedSubjectUpdateData<#Subject<'SubjectId>, 'SubjectId, #LifeAction, #LifeEvent, #OpError>) : PreparedSubjectUpdateData<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError> =
        {
            PreparedDataToUpdate   = PreparedSubjectWriteData<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>.CastUnsafe data.PreparedDataToUpdate
            ActionThatCausedUpdate = data.ActionThatCausedUpdate |> box :?> 'LifeAction
            SkipHistory            = data.SkipHistory
        }

#endif