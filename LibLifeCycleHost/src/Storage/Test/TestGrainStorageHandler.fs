[<AutoOpen>]
module LibLifeCycleHost.Storage.Test.Handler

open System
open LibLifeCycle
open LibLifeCycle.LifeCycles.Meta
open LibLifeCycleHost
open LibLifeCycleHost.GrainStorageModel
open System.Threading.Tasks
open System.Collections.Concurrent
open LibLifeCycleCore
open Microsoft.Extensions.Logging

#nowarn "0686"  // Allow exceptionally here to have explicit parameters for codec related functions

// hacky mutable to avoid plumbing flag through many many layers
let mutable doSerializationCheck = true

let inline private serializationCheck (x: 't) =
    if doSerializationCheck then
        x
        |> CodecLib.StjCodecs.Extensions.toJsonTextChecked<'t>
        |> ignore

// Trivial (and un-optimized) in-memory "persistent" store for tests

// FIXME, we should create 1 blob storage / repo per lifecycle type, similar to grainstorage / repo
let private blobsById = ConcurrentDictionary<Guid * LocalSubjectPKeyReference, BlobData * UInt32>()

type TestBlobStorage () =
    let blobIdToKey (blobId: BlobId) = blobId.Id, blobId.Owner
with
    member this.ApplyBlobAction (blobAction: BlobAction) : unit =
        match blobAction with
        | BlobAction.Create (blobId, mimeType, data) ->
            if not <| blobsById.TryAdd (blobIdToKey blobId, ({ MimeType = mimeType; Data = data.ToBytes }, blobId.Revision)) then
                failwith "Blob with this id already exists, can't create."

        | BlobAction.Append (blobId, updatedBlobId, bytesToAppend) ->
            match blobsById.TryRemove (blobIdToKey blobId) with
            | true, (data, revision) when blobId.Revision = revision ->
                let updatedData = Array.concat [data.Data; bytesToAppend]
                if not <| blobsById.TryAdd (blobIdToKey updatedBlobId, ({ data with Data = updatedData }, updatedBlobId.Revision)) then
                    failwith "Appended blob created concurrently, can't append."
            |_ -> failwith "Blob not found or changed concurrently, can't append."

        | BlobAction.Delete blobId ->
            match blobsById.TryRemove (blobIdToKey blobId) with
            | true, (_, revision) when blobId.Revision = revision -> ()
            | _ -> failwith "Blob not found or changed concurrently, can't delete."

    member this.GetBlobData (key: Guid * LocalSubjectPKeyReference) : Option<BlobData> =
        match blobsById.TryGetValue key with
        | true, (data, _) ->
            Some data
        | false, _ ->
            None

type TestStorageDb<'Subject, 'LifeAction, 'Constructor, 'LifeEvent, 'OpError, 'SubjectId
                            when 'Subject      :> Subject<'SubjectId>
                            and  'LifeAction   :> LifeAction
                            and  'Constructor  :> Constructor
                            and  'LifeEvent    :> LifeEvent
                            and  'LifeEvent    : comparison
                            and  'SubjectId    :> SubjectId
                            and  'SubjectId    : comparison
                            and  'OpError      :> OpError> = {
    LockObj: obj
    mutable SubjectsByIdStr: Map<string, SubjectStateContainer<'Subject, 'Constructor, 'SubjectId, 'LifeEvent, 'LifeAction, 'OpError>>
    mutable UniqueIndicesByIndexKeyThenIdStr: Map<string, Map<string, Choice<int64, string>>>
}

type TestGrainStorageHandler<'Subject, 'LifeAction, 'Constructor, 'LifeEvent, 'SubjectId, 'OpError
                                  when 'Subject      :> Subject<'SubjectId>
                                  and  'LifeAction   :> LifeAction
                                  and  'OpError      :> OpError
                                  and  'Constructor  :> Constructor
                                  and  'LifeEvent    :> LifeEvent
                                  and  'LifeEvent    : comparison
                                  and  'SubjectId    :> SubjectId
                                  and  'SubjectId    : comparison> (
                                      logger: Microsoft.Extensions.Logging.ILogger<TestGrainStorageHandler<'Subject, 'LifeAction, 'Constructor, 'LifeEvent, 'SubjectId, 'OpError>>,
                                      blobStorage: TestBlobStorage,
                                      timeSeriesAdapters: TimeSeriesAdapterCollection,
                                      grainPartition: GrainPartition) =

    let (GrainPartition partitionGuid) = grainPartition

    let db () : TestStorageDb<'Subject, 'LifeAction, 'Constructor, 'LifeEvent, 'OpError, 'SubjectId> =
        let d : ConcurrentDictionary<Guid, TestStorageDb<'Subject, 'LifeAction, 'Constructor, 'LifeEvent, 'OpError, 'SubjectId>> = TestGrainStorageHandler<'Subject, 'LifeAction, 'Constructor, 'LifeEvent, 'SubjectId, 'OpError>.Db
        d.GetOrAdd(
            partitionGuid,
            fun _ ->
                {
                    LockObj = obj()
                    SubjectsByIdStr = Map.empty
                    UniqueIndicesByIndexKeyThenIdStr = Map.empty
                })

    let sequences : ConcurrentDictionary<string, Ref<uint64>> = TestGrainStorageHandler<'Subject, 'LifeAction, 'Constructor, 'LifeEvent, 'SubjectId, 'OpError>.Sequences

    let tryApplyIndexActions (db: TestStorageDb<'Subject, 'LifeAction, 'Constructor, 'LifeEvent, 'OpError, 'SubjectId>) (pKey: string) (indexActions: list<IndexAction<'OpError>>) =
        let removeUniqueIndex key value (uniqueIndices: Map<string, Map<string, Choice<int64, string>>>) =
            match uniqueIndices.TryFind key with
            | Some byIdStr ->
                if byIdStr.TryFind pKey <> (Some value) then
                    uniqueIndices
                else
                    uniqueIndices.AddOrUpdate (key, byIdStr.Remove pKey)
            | None ->
                uniqueIndices

        let tryAddUniqueIndex key value (err: 'OpError) (uniqueIndices: Map<string, Map<string, Choice<int64, string>>>) =
            // TODO: update key if value reserved in transaction for empty key, similar to Sql Storage Handler
            key
            |> uniqueIndices.TryFind
            |> Option.bind (
                fun indicesByIdStr ->
                    indicesByIdStr
                    |> Map.toSeq
                    |> Seq.choose (
                        fun (idStr, otherValue) ->
                            if idStr <> pKey && value = otherValue then
                                Some (Error err)
                            else
                                None)
                    |> Seq.tryHead)
            |> Option.defaultWith (
                fun () ->
                    match uniqueIndices.TryFind key with
                    | None ->
                        Ok <| uniqueIndices.AddOrUpdate (key, Map.ofOneItem (pKey, value))
                    | Some byIdStr ->
                        Ok <| uniqueIndices.AddOrUpdate (key, byIdStr.AddOrUpdate (key, value)))

        indexActions
        |> List.fold (
            fun (indicesResult: Result<Map<string, Map<string, Choice<int64, string>>>, _>) indexAction ->
                match indicesResult with
                | Ok uniqueIndices ->
                    match indexAction with
                    | IndexAction.InsertNumeric (_, _, None)
                    | IndexAction.InsertString (_, _, None)
                    | IndexAction.InsertSearch _
                    | IndexAction.DeleteSearch _
                    | IndexAction.InsertGeography _
                    | IndexAction.DeleteGeography _
                    | IndexAction.DeleteNumeric (_, _, (* isUnique *) false)
                    | IndexAction.DeleteString (_, _, (* isUnique *) false)
                    | IndexAction.PromotedInsertNumeric _
                    | IndexAction.PromotedDeleteNumeric _
                    | IndexAction.PromotedInsertString _
                    | IndexAction.PromotedDeleteString _ ->
                        indicesResult
                    | IndexAction.DeleteNumeric (key, value, (* isUnique *) true) ->
                        Ok <| removeUniqueIndex key (Choice1Of2 value) uniqueIndices
                    | IndexAction.DeleteString (key, value, (* isUnique *) true) ->
                        Ok <| removeUniqueIndex key (Choice2Of2 value) uniqueIndices
                    | IndexAction.InsertNumeric (key, value, Some err) ->
                        tryAddUniqueIndex key (Choice1Of2 value) err uniqueIndices
                    | IndexAction.InsertString (key, value, Some err) ->
                        tryAddUniqueIndex key (Choice2Of2 value) err uniqueIndices
                | Error _ ->
                    indicesResult)
            (Ok db.UniqueIndicesByIndexKeyThenIdStr)

    let addToOthersSubscribing
        (currentOthersSubscribing: OthersSubscribing<'LifeEvent>)
        (subscriptionsToAdd: SubscriptionsToAdd<'LifeEvent>) =
        let subscriberRef = subscriptionsToAdd.Subscriber
        subscriptionsToAdd.NewSubscriptions
        |> Seq.fold (fun (othersSubscribing: OthersSubscribing<'LifeEvent>) kv ->
            match othersSubscribing.TryFind kv.Value with
            | None ->
                othersSubscribing.Add(kv.Value, Map.empty.Add(kv.Key, (Set.singleton subscriberRef)))
            | Some subscriptions ->
                let updatedSubscribers =
                    match subscriptions.TryFind kv.Key with
                    | None ->
                        Set.singleton subscriberRef
                    | Some subscribers -> subscribers.Add subscriberRef

                othersSubscribing.AddOrUpdate(kv.Value, subscriptions.AddOrUpdate(kv.Key, updatedSubscribers))
            ) currentOthersSubscribing

    static member val Sequences = ConcurrentDictionary<string, Ref<uint64>>()

    static member val Db = ConcurrentDictionary<Guid, TestStorageDb<'Subject, 'LifeAction, 'Constructor, 'LifeEvent, 'OpError, 'SubjectId>>() with get

    member private this.InitializeOrCommitPreparedInitializeSubject
        (pKey: string)
        (insertData: SubjectInsertData<'Subject, 'Constructor, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>)
        (payload: Choice<Option<SideEffectDedupInfo>, {| ExpectedETag: string; TransactionId: SubjectTransactionId |}>)
        : Task<Result<InitializeSubjectSuccessResult, 'OpError>> =
        insertData.DataToInsert.BlobActions |> List.iter blobStorage.ApplyBlobAction

        let maybeTxnData, dedupInfo =
            match payload with
            | Choice1Of2 dedupInfo -> None, dedupInfo
            | Choice2Of2 txnData -> Some txnData, None

        let dedupCache =
            match dedupInfo with
            | Some d -> Map.ofOneItem (d.Caller, (d.Id, insertData.DataToInsert.UpdatedSubjectState.LastUpdatedOn))
            | None -> Map.empty

        // Double check that all data can serialize correctly, it's not used in Test storage but checks that Sql storage will work too
        // check very same parameters that serialized in SqlServerGrainStorageHandler
        // "@newSideEffects"
        insertData.DataToInsert.SideEffectGroup
            |> Option.map (fun g -> g.SideEffects |> NonemptyMap.toMap |> Map.toList)
            |> Option.defaultValue []
            |> List.choose (fun (sideEffectId, sideEffect) ->
                match sideEffect with
                | GrainSideEffect.Transient _ -> None
                | GrainSideEffect.Persisted persistedSideEffect -> Some (sideEffectId, persistedSideEffect))
            |> GrainPersistedSideEffect.AsCodecFriendlyData
            |> List.iter (fun (_sideEffectId, codecFriendlyPersistedSideEffect) ->
                match codecFriendlyPersistedSideEffect with
                | Choice1Of2 e ->
                    e |> CodecFriendlyGrainPersistedSideEffect<LifeAction, OpError>.CastUnsafe |> serializationCheck
                | Choice2Of2 (timeSeriesKey, ``list<'TimeSeriesDataPoint>``, traceContext) ->
                    match timeSeriesAdapters.GetTimeSeriesAdapterByKey timeSeriesKey with
                    | Some adapter ->
                        adapter.TimeSeries.Invoke
                            { new FullyTypedTimeSeriesFunction<_> with
                                // inlining of types is so finicky that I must use different 'TimeSeriesDataPoint type param name when reading and writing side effects
                                member _.Invoke (timeSeries: TimeSeries<'TimeSeriesDataPointWrite, _, _, _, _, _, _, _>) =
                                    let points = ``list<'TimeSeriesDataPoint>`` :?> list<'TimeSeriesDataPointWrite>
                                    TypedTimeSeriesSideEffect.Ingest (points, traceContext)
                                    |> serializationCheck
                                    // compiler refuses to implement fully typed func that returns unit (don't ask me why)
                                    // have to return som dummy value then ignore it, see https://github.com/dotnet/fsharp/issues/35
                                    1 }
                            |> ignore
                    | None ->
                        sprintf "TimeSeries with key %A not found" timeSeriesKey
                        |> InvalidOperationException
                        |> raise)
        // "@subject"
        insertData.DataToInsert.UpdatedSubjectState.Subject :> Subject<'SubjectId> |> serializationCheck
        // "@nextTickContext"
        insertData.DataToInsert.UpdatedSubjectState.TickState
            |> (function | TickState.Scheduled (_, context) -> context | TickState.Fired _ -> None | TickState.NoTick -> None)
            |> serializationCheck
        // "@ourSubscriptions"
        insertData.DataToInsert.UpdatedSubjectState.OurSubscriptions |> serializationCheck
        // "@creatorSubscriptions"
        insertData.CreatorSubscribing |> Map.iter (fun lifeEvent _ -> lifeEvent :> LifeEvent |> serializationCheck)
        // "@operation"
        insertData.ConstructorThatCausedInsert :> Constructor |> serializationCheck

        let subjectCurrentStateContainer =
           {
                CurrentSubjectState      = insertData.DataToInsert.UpdatedSubjectState
                CurrentOthersSubscribing = insertData.CreatorSubscribing
                ETag                     = Guid.NewGuid().ToString()
                Version                  = 0UL
                NextSideEffectSeqNum     = insertData.DataToInsert.NextSideEffectSeq
                SideEffectDedupCache     = dedupCache
                SkipHistoryOnNextOp      = false // unused in this storage
           }

        let db = db()
        lock db.LockObj
            (fun _ ->

                match maybeTxnData with
                | None ->
                    match db.SubjectsByIdStr.TryGetValue pKey with
                    | false, _ ->
                        subjectCurrentStateContainer
                    | true, existing ->
                        match existing with
                        | SubjectStateContainer.PreparedInitialize (_, actualETag, _) ->
                            raise (Orleans.Storage.InconsistentStateException("ETag doesn't match", actualETag, "<MISSING>"))
                        | SubjectStateContainer.Committed subjectCurrentStateContainer
                        | SubjectStateContainer.PreparedAction (subjectCurrentStateContainer, _, _) ->
                            raise (Orleans.Storage.InconsistentStateException("ETag doesn't match", subjectCurrentStateContainer.ETag, "<MISSING>"))

                | Some txnData ->
                    match db.SubjectsByIdStr.TryGetValue pKey with
                    | true, existing ->
                        match existing with
                        | SubjectStateContainer.PreparedInitialize (_, actualETag, actualTransactionId) ->
                            // commit prepared initialize special case
                            if txnData.ExpectedETag = actualETag && actualTransactionId = txnData.TransactionId then
                                subjectCurrentStateContainer
                            else
                                raise (Orleans.Storage.InconsistentStateException("ETag doesn't match", actualETag, txnData.ExpectedETag))
                        | SubjectStateContainer.Committed current
                        | SubjectStateContainer.PreparedAction (current, _, _) ->
                            raise (Orleans.Storage.InconsistentStateException("ETag doesn't match", current.ETag, txnData.ExpectedETag))
                    | false, _ ->
                        raise (Orleans.Storage.InconsistentStateException("ETag doesn't match", "<MISSING>", txnData.ExpectedETag))

                |> fun updatedStateContainer ->
                    match tryApplyIndexActions db pKey insertData.DataToInsert.IndexActions with
                    | Ok uniqueIndices ->
                        db.SubjectsByIdStr <- db.SubjectsByIdStr.AddOrUpdate (pKey, SubjectStateContainer.Committed updatedStateContainer)
                        db.UniqueIndicesByIndexKeyThenIdStr <- uniqueIndices
                        { ETag = updatedStateContainer.ETag
                          Version = updatedStateContainer.Version
                          SkipHistoryOnNextOp = updatedStateContainer.SkipHistoryOnNextOp }
                        |> Ok
                    | Error err ->
                        Error err)

        |> Task.FromResult

    member _.UpdateOrCommitPreparedUpdateSubject
        (pKey: string)
        (updateData: SubjectUpdateData<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>)
        (payload: Choice<Option<UpsertDedupData>, SubjectTransactionId>)
        : Task<Result<UpdateSubjectSuccessResult, 'OpError>> =
            let (maybeSubjectTransactionId,
                 dedupData) =
                match payload with
                | Choice1Of2 dedupData ->
                    None,
                    dedupData

                | Choice2Of2 transactionId ->
                    Some transactionId,
                    None

            // Double check that all data can serialize correctly, it's not used in Test storage but checks that Sql storage will work too
            // check very same parameters that serialized in SqlServerGrainStorageHandler
            // "@newSideEffects" // TODO: how much will it slow down tests?
            updateData.DataToUpdate.SideEffectGroup
                |> Option.map (fun g -> g.SideEffects |> NonemptyMap.toMap |> Map.toList)
                |> Option.defaultValue []
                |> List.choose (fun (sideEffectId, sideEffect) ->
                    match sideEffect with
                    | GrainSideEffect.Transient _ -> None
                    | GrainSideEffect.Persisted persistedSideEffect -> Some (sideEffectId, persistedSideEffect))
                |> GrainPersistedSideEffect.AsCodecFriendlyData
                |> List.iter (fun (_sideEffectId, codecFriendlyPersistedSideEffect) ->
                    match codecFriendlyPersistedSideEffect with
                    | Choice1Of2 e ->
                        e |> CodecFriendlyGrainPersistedSideEffect<LifeAction, OpError>.CastUnsafe |> serializationCheck
                    | Choice2Of2 (timeSeriesKey, ``list<'TimeSeriesDataPoint>``, traceContext) ->
                        match timeSeriesAdapters.GetTimeSeriesAdapterByKey timeSeriesKey with
                        | Some adapter ->
                            adapter.TimeSeries.Invoke
                                { new FullyTypedTimeSeriesFunction<_> with
                                    // inlining of types is so finicky that I must use different 'TimeSeriesDataPoint type param name when reading and writing side effects
                                    member _.Invoke (timeSeries: TimeSeries<'TimeSeriesDataPointWrite, _, _, _, _, _, _, _>) =
                                        let points = ``list<'TimeSeriesDataPoint>`` :?> list<'TimeSeriesDataPointWrite>
                                        TypedTimeSeriesSideEffect.Ingest (points, traceContext)
                                        |> serializationCheck
                                        // compiler refuses to implement fully typed func that returns unit (don't ask me why)
                                        // have to return som dummy value then ignore it, see https://github.com/dotnet/fsharp/issues/35
                                        1 }
                                |> ignore
                        | None ->
                            sprintf "TimeSeries with key %A not found" timeSeriesKey
                            |> InvalidOperationException
                            |> raise)
            // "@subject"
            updateData.DataToUpdate.UpdatedSubjectState.Subject :> Subject<'SubjectId> |> serializationCheck
            // "@nextTickContext"
            updateData.DataToUpdate.UpdatedSubjectState.TickState
                |> (function | TickState.Scheduled (_, context) -> context | TickState.Fired _ -> None | TickState.NoTick -> None)
                |> serializationCheck
            // "@ourSubscriptions"
            updateData.DataToUpdate.UpdatedSubjectState.OurSubscriptions |> serializationCheck
            // "@operation"
            updateData.ActionThatCausedUpdate :> LifeAction |> serializationCheck
            // "@lifeEvent"
            updateData.SubscriptionsToAdd |> Option.iter (fun subscriptionsToAdd ->
                subscriptionsToAdd.NewSubscriptions.Values
                |> Seq.iter (fun lifeEvent -> lifeEvent :> LifeEvent |> serializationCheck))

            updateData.DataToUpdate.BlobActions |> List.iter blobStorage.ApplyBlobAction

            let db = db()
            lock db.LockObj
                (fun _ ->
                    match db.SubjectsByIdStr.TryGetValue pKey with
                    | false, _ ->
                        Orleans.Storage.InconsistentStateException("ETag doesn't match", "<MISSING>", updateData.ExpectedETag) |> raise
                    | true, existing ->
                        match existing with
                        | SubjectStateContainer.PreparedInitialize (_, actualETag, _) ->
                            Orleans.Storage.InconsistentStateException("ETag doesn't match", actualETag, updateData.ExpectedETag) |> raise
                        | SubjectStateContainer.Committed current ->
                            if current.ETag = updateData.ExpectedETag then
                                current
                            else
                                Orleans.Storage.InconsistentStateException("ETag doesn't match") |> raise
                        | SubjectStateContainer.PreparedAction (current, _, transactionId) ->
                            if current.ETag = updateData.ExpectedETag && (Some transactionId) = maybeSubjectTransactionId then
                                current
                            else
                                Orleans.Storage.InconsistentStateException("ETag doesn't match") |> raise
                    |> fun current ->
                        let successResult: UpdateSubjectSuccessResult =
                            { NewETag = Guid.NewGuid().ToString()
                              NewVersion = updateData.CurrentVersion + 1UL
                              SkipHistoryOnNextOp = false }

                        let newDedupCache =
                            match dedupData with
                            | None -> current.SideEffectDedupCache
                            | Some d ->
                                match d.EvictedDedupInfoToDelete with
                                | None -> current.SideEffectDedupCache
                                | Some (evicted, _) -> current.SideEffectDedupCache.Remove evicted.Caller
                                |> fun cache ->
                                    cache.Add (d.DedupInfo.Caller, (d.DedupInfo.Id, updateData.DataToUpdate.UpdatedSubjectState.LastUpdatedOn))

                        let updatedOthersSubscribing =
                            updateData.SubscriptionsToAdd
                            |> Option.map (addToOthersSubscribing current.CurrentOthersSubscribing)
                            |> Option.defaultValue current.CurrentOthersSubscribing

                        let updatedCurrentStateContainer =
                            { CurrentSubjectState = updateData.DataToUpdate.UpdatedSubjectState
                              CurrentOthersSubscribing = updatedOthersSubscribing
                              ETag = successResult.NewETag
                              Version = successResult.NewVersion
                              SkipHistoryOnNextOp = successResult.SkipHistoryOnNextOp
                              SideEffectDedupCache = newDedupCache
                              NextSideEffectSeqNum = updateData.DataToUpdate.NextSideEffectSeq }

                        match tryApplyIndexActions db pKey updateData.DataToUpdate.IndexActions with
                        | Ok uniqueIndices ->
                            db.SubjectsByIdStr <- db.SubjectsByIdStr.AddOrUpdate (pKey, SubjectStateContainer.Committed updatedCurrentStateContainer)
                            db.UniqueIndicesByIndexKeyThenIdStr <- uniqueIndices
                            Ok successResult
                        | Error err ->
                            Error err)
            |>
            function
                | Ok successResult ->
                    // imitate occasional concurrent write
                    // if (DateTimeOffset.Now.Millisecond % 30 = 0) then
                    //     db.AddOrUpdate(pKey,
                    //         (fun _ -> failwith "Subject deleted concurrently."),
                    //         (fun _ existing ->
                    //             match existing with
                    //             | SubjectStateContainer.Committed current ->
                    //                 SubjectStateContainer.Committed { current with ETag = sprintf "HA-HA-HA %d" DateTimeOffset.Now.Millisecond }
                    //             | _ -> existing)) |> ignore
                    Ok successResult
                | Error err ->
                    Error err
                |> Task.FromResult

    interface IGrainStorageHandler<'Subject, 'LifeAction, 'Constructor, 'LifeEvent, 'SubjectId, 'OpError> with

        member _.TryReadState(pKey: string) =
            let db = db()
            lock db.LockObj
                (fun _ ->
                    match db.SubjectsByIdStr.TryGetValue pKey with
                    | true, stateContainer ->
                        // If you see this warning one day then implement side effect persistence in test storage
                        // Normally test simulations are small and fast and there's no need for Orleans to recycle grains (in theory)
                        logger.LogWarning("Subject grain is rehydrated from Test Storage, it doesn't persist side effects, may lead to flaky test behavior")
                        {
                            SubjectStateContainer = stateContainer
                            PendingSideEffects    = KeyedSet.empty
                            PersistedGrainIdHash  = None
                        }
                        |> Some
                    | (false, _) -> None)

            |> Task.FromResult

        member _.RemindersImplementation = RemindersImplementation.TestNotPersistedManuallyTriggered

        member this.InitializeSubject pKey dedupInfo insertData =
            this.InitializeOrCommitPreparedInitializeSubject pKey insertData (Choice1Of2 dedupInfo)

        member this.CommitInitializeSubject pKey expectedETag subjectTransactionId insertData =
            backgroundTask {
                match! this.InitializeOrCommitPreparedInitializeSubject pKey insertData (Choice2Of2 {| ExpectedETag = expectedETag; TransactionId = subjectTransactionId |}) with
                | Ok res ->
                    return { ETag = res.ETag; Version = res.Version; SkipHistoryOnNextOp = res.SkipHistoryOnNextOp }
                | Error err ->
                    return failwithf "Domain error is unexpected while committing subject: %A, %A" pKey err
            }

        member this.PrepareInitializeSubject pKey preparedInsertData _uniqueIndicesToReserve subjectTransactionId =
            // "@preparedTransactionalState"
            preparedInsertData
            |> PreparedSubjectInsertData<Subject<'SubjectId>, Constructor, 'SubjectId, LifeAction, LifeEvent, OpError>.CastUnsafe |> serializationCheck

            let db = db()
            let newETag = Guid.NewGuid().ToString()
            lock db.LockObj
                (fun _ ->
                    match db.SubjectsByIdStr.TryGetValue pKey with
                    | false, _ ->
                        SubjectStateContainer.PreparedInitialize (preparedInsertData, newETag, subjectTransactionId)
                    | true, existing ->
                        match existing with
                        | SubjectStateContainer.PreparedInitialize (_, actualETag, _) ->
                            raise (Orleans.Storage.InconsistentStateException("ETag doesn't match", actualETag, "<MISSING>"))
                        | SubjectStateContainer.Committed subjectCurrentStateContainer
                        | SubjectStateContainer.PreparedAction (subjectCurrentStateContainer, _, _) ->
                            raise (Orleans.Storage.InconsistentStateException("ETag doesn't match", subjectCurrentStateContainer.ETag, "<MISSING>"))
                    |> fun subjectStateContainer ->
                        db.SubjectsByIdStr <- db.SubjectsByIdStr.AddOrUpdate (pKey, subjectStateContainer)
                        // TODO: reserve uniqueIndicesToReserve for empty pKey, similar to Sql Storage Handler
                        subjectStateContainer)

            |> fun _ ->
                newETag
                |> Ok
                |> Task.FromResult

        member this.RollbackInitializeSubject pKey _uniqueIndicesToRelease subjectTransactionId eTag =
            // rolled back prepared - delete
            let db = db()
            lock db.LockObj
                (fun _ ->
                    match db.SubjectsByIdStr.TryGetValue pKey with
                    | false, _ ->
                        raise (Orleans.Storage.InconsistentStateException("ETag doesn't match", "<MISSING>", eTag))
                    | true, existing ->
                        match existing with
                        | SubjectStateContainer.Committed current
                        | SubjectStateContainer.PreparedAction (current, _, _) ->
                            raise (Orleans.Storage.InconsistentStateException("ETag doesn't match", current.ETag, eTag))
                        | SubjectStateContainer.PreparedInitialize(_, actualETag, actualTransactionId) ->
                            if actualETag <> eTag || actualTransactionId <> subjectTransactionId then
                                raise (Orleans.Storage.InconsistentStateException("ETag doesn't match", actualETag, eTag))
                            else
                                db.SubjectsByIdStr <- db.SubjectsByIdStr.Remove pKey
                                // TODO: release uniqueIndicesToRelease for empty pKey, similar to Sql Storage Handler
                                // clean up indices
                                db.UniqueIndicesByIndexKeyThenIdStr <-
                                    db.UniqueIndicesByIndexKeyThenIdStr
                                    |> Map.map (fun _ indicesByIdStr -> indicesByIdStr.Remove pKey))
            Task.CompletedTask

        member _.ClearState(pKey: string) expectedETag _skipHistory _sideEffectId =
            let db = db()
            lock db.LockObj (
                fun _ ->
                    match db.SubjectsByIdStr.TryGetValue pKey with
                    | false, _ ->
                        raise (Orleans.Storage.InconsistentStateException("ETag doesn't match", "<MISSING>", expectedETag))
                    | true, existing ->
                        match existing with
                        | SubjectStateContainer.Committed { ETag = actualETag }
                        | SubjectStateContainer.PreparedAction ({ ETag = actualETag }, _, _)
                        | SubjectStateContainer.PreparedInitialize(_, actualETag, _) ->
                            if actualETag <> expectedETag then
                                raise (Orleans.Storage.InconsistentStateException("ETag doesn't match", actualETag, expectedETag))
                            else
                                db.SubjectsByIdStr <- db.SubjectsByIdStr.Remove pKey
                                // clean up indices
                                db.UniqueIndicesByIndexKeyThenIdStr <-
                                    db.UniqueIndicesByIndexKeyThenIdStr
                                    |> Map.map (fun _ indicesByIdStr -> indicesByIdStr.Remove pKey)
                )
            Task.CompletedTask

        member _.AssertStateConsistency (pKey: string) (expectedETagIfCreated: Option<string>) : Task =
            let db = db()
            lock db.LockObj (
                fun _ ->
                    let actualETagIfCreated =
                        match db.SubjectsByIdStr.TryGetValue pKey with
                        | false, _ ->
                            None
                        | true, existing ->
                            match existing with
                            | SubjectStateContainer.PreparedInitialize (_, eTag, _) ->
                                Some eTag
                            | SubjectStateContainer.Committed current
                            | SubjectStateContainer.PreparedAction (current, _, _) ->
                                Some current.ETag

                    if expectedETagIfCreated <> actualETagIfCreated then
                        let actualETag = match actualETagIfCreated with | Some actual -> actual | None -> "<MISSING>"
                        let expectedETag = match expectedETagIfCreated with | Some expected -> expected | None -> "<MISSING>"
                        raise (Orleans.Storage.InconsistentStateException("ETag doesn't match", actualETag, expectedETag))
                )
            Task.CompletedTask

        member _.ClearExpiredSubjectsHistory _now _batchSize =
            Task.FromResult 0us

        member _.UpdateSideEffectStatus _pKey _sideEffectId _sideEffectResult =
            Task.CompletedTask

        member this.RebuildIndices (_lifeCycleName: string) (_rebuildType: IndexRebuildType) (_maybeLastSubjectIdRebuilt: Option<string>) (_batchSize: uint16) : Task<RebuildIndicesBatchResult> =
            Task.FromResult RebuildIndicesBatchResult.CompletedBatchNoMoreBatchesPending

        member this.ReEncodeSubjects (_lifeCycleName: string) (_maybeLastSubjectIdReEncoded: Option<string>) (_batchSize: uint16) : Task<ReEncodeSubjectsBatchResult> =
            Task.FromResult ReEncodeSubjectsBatchResult.CompletedBatchNoMoreBatchesPending

        member this.ReEncodeSubjectsHistory (_lifeCycleName: string) (_maybeLastSubjectIdVersionReEncoded: Option<string * uint64>) (_batchSize: uint16) : Task<ReEncodeSubjectsHistoryBatchResult> =
            Task.FromResult ReEncodeSubjectsHistoryBatchResult.CompletedBatchNoMoreBatchesPending

        member this.UpdateSubject pKey dedupCache updateData =
            this.UpdateOrCommitPreparedUpdateSubject pKey updateData (Choice1Of2 dedupCache)

        member this.CommitUpdateSubject pKey subjectTransactionId updateData =
            backgroundTask {
                match! this.UpdateOrCommitPreparedUpdateSubject pKey updateData (Choice2Of2 subjectTransactionId) with
                | Ok res ->
                    return
                        { NewETag = res.NewETag
                          NewVersion = res.NewVersion
                          SkipHistoryOnNextOp = res.SkipHistoryOnNextOp }
                | Error err ->
                    return failwithf "Domain error is unexpected while committing subject: %A, %A" pKey err
            }

        member this.PrepareUpdateSubject pKey expectedETag preparedUpdateData _uniqueIndicesToReserve subjectTransactionId =
            // Double check that all data can serialize correctly, it's not used in Test storage but checks that Sql storage will work too
            // check very same parameters that serialized in SqlServerGrainStorageHandler
            // "@preparedTransactionalState"
            preparedUpdateData |> PreparedSubjectUpdateData<Subject<'SubjectId>, 'SubjectId, LifeAction, LifeEvent, OpError>.CastUnsafe |> serializationCheck

            let newETag = Guid.NewGuid().ToString()

            let db = db()
            lock db.LockObj
                (fun _ ->
                    match db.SubjectsByIdStr.TryGetValue pKey with
                    | false, _ ->
                        Orleans.Storage.InconsistentStateException("ETag doesn't match", "<MISSING>", expectedETag) |> raise
                    | true, existing ->
                        match existing with
                        | SubjectStateContainer.PreparedInitialize (_, actualETag, _)
                        | SubjectStateContainer.PreparedAction ({ ETag = actualETag }, _, _) ->
                            Orleans.Storage.InconsistentStateException("ETag doesn't match", actualETag, expectedETag) |> raise
                        | SubjectStateContainer.Committed current ->
                            if current.ETag = expectedETag then
                                SubjectStateContainer.PreparedAction ({ current with ETag = newETag }, preparedUpdateData, subjectTransactionId)
                            else
                                Orleans.Storage.InconsistentStateException("ETag doesn't match") |> raise
                    |> fun updatedStateContainer ->
                        // TODO: reserve uniqueIndicesToReserve for empty pKey, similar to Sql Storage Handler
                        db.SubjectsByIdStr <- db.SubjectsByIdStr.AddOrUpdate (pKey, updatedStateContainer)
                        updatedStateContainer)
            |> fun _ ->
                Task.FromResult (Ok newETag)

        member this.RollbackUpdateSubject pKey expectedETag _uniqueIndicesToRelease subjectTransactionId =
            let newETag = Guid.NewGuid().ToString()
            let db = db()
            lock db.LockObj
                (fun _ ->
                    match db.SubjectsByIdStr.TryGetValue pKey with
                    | false, _ ->
                        Orleans.Storage.InconsistentStateException("ETag doesn't match", "<MISSING>", expectedETag) |> raise
                    | true, existing ->
                        match existing with
                        | SubjectStateContainer.PreparedInitialize (_, actualETag, _)
                        | SubjectStateContainer.Committed { ETag = actualETag } ->
                            Orleans.Storage.InconsistentStateException("ETag doesn't match", actualETag, expectedETag) |> raise
                        | SubjectStateContainer.PreparedAction (current, _, preparedTransactionId) ->
                            if current.ETag <> expectedETag || preparedTransactionId <> subjectTransactionId then
                                Orleans.Storage.InconsistentStateException("ETag doesn't match", current.ETag, expectedETag) |> raise
                            SubjectStateContainer.Committed { current with ETag = newETag }
                    |> fun updatedStateContainer ->
                        db.SubjectsByIdStr <- db.SubjectsByIdStr.AddOrUpdate (pKey, updatedStateContainer)
                        // TODO: release uniqueIndicesToRelease for empty pKey, similar to Sql Storage Handler
                        updatedStateContainer)
            |> fun _ ->
                newETag
                |> Task.FromResult

        member this.EnqueueSideEffects
            pKey now dedupData expectedETag nextSideEffectSeqNum _sideEffectGroups =

            let db = db()
            let newETag = Guid.NewGuid().ToString()
            lock db.LockObj
                (fun _ ->
                    match db.SubjectsByIdStr.TryGetValue pKey with
                    | false, _ ->
                        Orleans.Storage.InconsistentStateException("ETag doesn't match", "<MISSING>", expectedETag) |> raise
                    | true, existing ->
                        match existing with
                        | SubjectStateContainer.PreparedInitialize (_, actualETag, _)
                        | SubjectStateContainer.PreparedAction ({ ETag = actualETag }, _, _) ->
                            Orleans.Storage.InconsistentStateException("ETag doesn't match", actualETag, expectedETag) |> raise
                        | SubjectStateContainer.Committed current ->
                            if current.ETag = expectedETag then
                                let newDedupCache =
                                    match dedupData with
                                    | None -> current.SideEffectDedupCache
                                    | Some d ->
                                        match d.EvictedDedupInfoToDelete with
                                        | None -> current.SideEffectDedupCache
                                        | Some (evicted, _) -> current.SideEffectDedupCache.Remove evicted.Caller
                                        |> fun cache ->
                                            cache.Add (d.DedupInfo.Caller, (d.DedupInfo.Id, now))
                                current, newDedupCache
                            else
                                Orleans.Storage.InconsistentStateException("ETag doesn't match") |> raise
                    |> fun (current, newDedupCache) ->
                            let updatedCurrentStateContainer =
                                { current with
                                    ETag = newETag
                                    SideEffectDedupCache = newDedupCache
                                    NextSideEffectSeqNum = nextSideEffectSeqNum }
                            db.SubjectsByIdStr <- db.SubjectsByIdStr.AddOrUpdate (pKey, SubjectStateContainer.Committed updatedCurrentStateContainer)
                            newETag)
            |> Task.FromResult

        member this.SetTickState
            pKey expectedETag nextSideEffectSeqNum tickState _sideEffectGroup =

            let db = db()
            let newETag = Guid.NewGuid().ToString()
            lock db.LockObj
                (fun _ ->
                    match db.SubjectsByIdStr.TryGetValue pKey with
                    | false, _ ->
                        Orleans.Storage.InconsistentStateException("ETag doesn't match", "<MISSING>", expectedETag) |> raise
                    | true, existing ->
                        match existing with
                        | SubjectStateContainer.PreparedInitialize (_, actualETag, _)
                        | SubjectStateContainer.PreparedAction ({ ETag = actualETag }, _, _) ->
                            Orleans.Storage.InconsistentStateException("ETag doesn't match", actualETag, expectedETag) |> raise
                        | SubjectStateContainer.Committed current ->
                            if current.ETag = expectedETag then
                                current
                            else
                                Orleans.Storage.InconsistentStateException("ETag doesn't match") |> raise
                    |> fun current ->
                            let updatedCurrentStateContainer =
                                { current with
                                    CurrentSubjectState = { current.CurrentSubjectState with TickState = tickState }
                                    ETag = newETag
                                    NextSideEffectSeqNum = nextSideEffectSeqNum }
                            db.SubjectsByIdStr <- db.SubjectsByIdStr.AddOrUpdate (pKey, SubjectStateContainer.Committed updatedCurrentStateContainer)
                            newETag)
            |> Task.FromResult

        member this.SetTickStateAndSubscriptions _pKey _expectedETag _nextSideEffectSeqNum _tickState _subscriptions _sideEffects =
            failwith "Test storage doesn't support rebuild of subscriptions"

        member this.ReadFailedSideEffects _pKey _failedSideEffectIds =
            failwith "Test storage doesn't support retrying of failed side effects"

        member this.RetrySideEffects _pKey _expectedETag _nextSideEffectSeqNum _retryingSideEffectGroups =
            failwith "Test storage doesn't support retrying of failed side effects"

        member _.AddSubscriptions pKey expectedETag subscriptionsToAdd =
            let db = db()
            lock db.LockObj
                (fun _ ->
                    match db.SubjectsByIdStr.TryGetValue pKey with
                    | false, _ ->
                        raise (Orleans.Storage.InconsistentStateException("ETag doesn't match", "<MISSING>", expectedETag))
                    | true, currentStateContainer ->
                        match currentStateContainer with
                        | SubjectStateContainer.PreparedInitialize _ ->
                            raise (Orleans.Storage.InconsistentStateException("ETag doesn't match", "<MISSING>", expectedETag))

                        | SubjectStateContainer.Committed currentState
                        | SubjectStateContainer.PreparedAction (currentState, _, _) ->
                            if currentState.ETag <> expectedETag then
                                raise (Orleans.Storage.InconsistentStateException("ETag doesn't match", currentState.ETag, expectedETag))
                            else
                                let newETag = Guid.NewGuid().ToString()
                                // Double check that all data can serialize correctly, it's not used in Test storage but checks that Sql storage will work too
                                // check very same parameters that serialized in SqlServerGrainStorageHandler
                                // "@lifeEvent"
                                subscriptionsToAdd.NewSubscriptions.Values |> Seq.iter (fun lifeEvent -> lifeEvent :> LifeEvent |> serializationCheck)
                                let updatedOthersSubscribing = addToOthersSubscribing currentState.CurrentOthersSubscribing subscriptionsToAdd

                                let updatedCurrentState =
                                    { currentState with
                                        ETag = newETag
                                        CurrentOthersSubscribing = updatedOthersSubscribing }

                                match currentStateContainer with
                                | SubjectStateContainer.PreparedInitialize _ ->
                                    failwith "Unexpected."
                                | SubjectStateContainer.Committed _ ->
                                    SubjectStateContainer.Committed updatedCurrentState
                                | SubjectStateContainer.PreparedAction (_, preparedState, transactionId) ->
                                    SubjectStateContainer.PreparedAction (updatedCurrentState, preparedState, transactionId)

                                |> fun updatedStateContainer ->
                                    db.SubjectsByIdStr <- db.SubjectsByIdStr.AddOrUpdate (pKey, updatedStateContainer)
                                    newETag
                    )
            |> Task.FromResult

        member this.RemoveSubscriptions pKey expectedETag subscriberPKeyRef subscriptionNames =
            let db = db()
            lock db.LockObj
                (fun _ ->
                    match db.SubjectsByIdStr.TryGetValue pKey with
                    | false, _ ->
                        raise (Orleans.Storage.InconsistentStateException("ETag doesn't match", "<MISSING>", expectedETag))
                    | true, currentStateContainer ->
                        match currentStateContainer with
                        | SubjectStateContainer.PreparedInitialize _ ->
                            raise (Orleans.Storage.InconsistentStateException("ETag doesn't match", "<MISSING>", expectedETag))

                        | SubjectStateContainer.Committed currentState
                        | SubjectStateContainer.PreparedAction (currentState, _, _) ->
                            let newETag = Guid.NewGuid().ToString()
                            if currentState.ETag <> expectedETag then
                                raise (Orleans.Storage.InconsistentStateException("ETag doesn't match", currentState.ETag, expectedETag))
                            else
                                let updatedOthersSubscribing =
                                    subscriptionNames
                                    |> Seq.fold (fun (othersSubscribing: OthersSubscribing<'LifeEvent>) subscriptionName ->
                                        othersSubscribing
                                        |> Seq.where(fun kv -> kv.Value.ContainsKey subscriptionName)
                                        |> Seq.fold (fun (othersSubscribing: OthersSubscribing<'LifeEvent>) kv ->
                                            match kv.Value.TryFind subscriptionName with
                                            | None ->
                                                othersSubscribing
                                            | Some subscribers ->
                                                othersSubscribing.AddOrUpdate(kv.Key, kv.Value.AddOrUpdate(subscriptionName, (subscribers.Remove subscriberPKeyRef)))
                                        ) othersSubscribing
                                    ) currentState.CurrentOthersSubscribing

                                let updatedCurrentState =
                                    { currentState with
                                        CurrentOthersSubscribing = updatedOthersSubscribing
                                        ETag = newETag }

                                match currentStateContainer with
                                | SubjectStateContainer.PreparedInitialize _ ->
                                    failwith "Unexpected."
                                | SubjectStateContainer.Committed _ ->
                                    SubjectStateContainer.Committed updatedCurrentState
                                | SubjectStateContainer.PreparedAction (_, preparedState, transactionId) ->
                                    SubjectStateContainer.PreparedAction (updatedCurrentState, preparedState, transactionId)

                            |> fun updatedStateContainer ->
                                db.SubjectsByIdStr <- db.SubjectsByIdStr.AddOrUpdate (pKey, updatedStateContainer)
                                newETag
                        )
            |> Task.FromResult

        member _.GetIdsOfSubjectsWithStalledSideEffects _ =
            Task.FromResult Set.empty

        member _.GetBatchOfSubjectsWithTickStateAndOurSubscriptions _ _ =
            Task.FromResult List.empty

        member _.ProcessPermanentFailures _ _ _ =
            Task.FromResult None

        member _.GetSideEffectMetrics () =
            Task.FromResult None

        member _.GetTimerMetrics () =
            Task.FromResult None

        member _.GetNextSequenceNumber (sequenceName: string) : Task<uint64> =
            let refCell = sequences.GetOrAdd(sequenceName, fun _ -> (ref 0UL))
            System.Threading.Interlocked.Increment refCell
            |> Task.FromResult

        member _.PeekCurrentSequenceNumber (sequenceName: string) : Task<uint64> =
            let refCell = sequences.GetOrAdd(sequenceName, fun _ -> (ref 0UL))
            refCell.Value
            |> Task.FromResult