module LibLifeCycleHost.Storage.SqlServer.SqlServerGrainStorageHandler

#nowarn "0686"  // Allow exceptionally here to have explicit parameters for codec related functions

open System
open System.Data
open FSharpPlus
open LibLifeCycle
open LibLifeCycle.MetaServices
open Microsoft.Data.SqlClient
open FSharp.Control
open System.Threading.Tasks
open LibLifeCycleCore
open LibLifeCycleHost.GrainStorageModel
open Microsoft.Extensions.Logging
open LibLifeCycleTypes.File
open LibLifeCycle.LifeCycles.Meta
open LibLifeCycleHost
open LibLifeCycleHost.Storage.SqlServer.SqlServerTransferBlobHandler
open Microsoft.SqlServer.Server

type ETag = string

type private IndexKindCode =
| NumericNonUnique = 1
| NumericUnique = 2
| StringNonUnique = 3
| StringUnique = 4
| StringSearch = 5
| Geography = 6

let private byteArrayToHexString (bytes: byte[]) =
    BitConverter.ToString(bytes).Replace("-","")
    |> sprintf "0x%s"

let private hexStringToByteArray (hexString: string) =
    hexString.Replace("0x", "").ToCharArray()
    |> Seq.chunkBySize 2
    |> Seq.map (fun chunk -> String.Join("", chunk))
    |> Seq.map (fun hexChar -> Convert.ToByte(hexChar, 16))
    |> Seq.toArray

// A way to encode uints to ints in SQL Server (which doesnt support unsigned values) such
// that relative ordering is maintained. This is necessary if we're querying ranges of the value
let uint32ToInt32MaintainOrder (value: uint32) : int =
    (int64 value) - (int64 Int32.MaxValue) - 1L
    |> int

let int32ToUInt32MaintainOrder (value: int32) : uint32 =
    (int64 value) + (int64 Int32.MaxValue) + 1L
    |> uint32

let inline private toCompressedJson (x: 't) = DataEncode.toCompressedJson x

[<Literal>]
let private maxCompressableJsonSizeBytes = 5242880//1048576 //  1024 * 1024

// Returns the compressed json, and maybe logs a warning if the size is over 50% of the max compressable json size
let inline private toJsonTextControlledForSize (warningLogger: string -> unit) (lifeCycleName: string) (pKey: string) (field: string) (x: 't) : string =
    let json =
        x
        // FIXME - toJsonTextChecked is ~2x as expensive as toJsonText. Need to find cheaper way to check integrity of serialized data
        |> CodecLib.StjCodecs.Extensions.toJsonTextChecked<'t>

    if json.Length > maxCompressableJsonSizeBytes / 2 then
        if json.Length > maxCompressableJsonSizeBytes then
            sprintf "Encoded data is too large. Uncompressed size is over %d bytes: %d. Subject: %s; Id: %s; Field: %s"
                maxCompressableJsonSizeBytes json.Length lifeCycleName pKey field
            |> fun details -> PermanentSubjectException ("toJsonTextControlledForSize", details)
            |> raise
        else
            (sprintf "Encoded data is large. Uncompressed size is over %d: %d. Subject: %s; Id: %s; Field: %s" (maxCompressableJsonSizeBytes / 2) json.Length lifeCycleName pKey field)
            |> warningLogger
    json

let inline private toCompressedJsonControlledForSize (warningLogger: string -> unit) (lifeCycleName: string) (pKey: string) (field: string) (x: 't) : byte[] =
    let json = toJsonTextControlledForSize warningLogger lifeCycleName pKey field x
    gzipCompressUtf8String json

let inline private tryOfCompressedJson (bytes: byte[]) =
    DataEncode.ofCompressedJsonText bytes
    |> Result.toOption

let inline private ofJsonTextWithContextInfoInError (lifeCycleName: string) (pKey: string) (field: string) (json: string) =
    match DataEncode.ofJsonText json with
    | Ok x -> x
    | Error err ->
        InvalidOperationException (sprintf "Unable to decode field %s: %s, %s ; Error: %A" field lifeCycleName pKey err)
        |> raise

let inline private ofCompressedJsonTextWithContextInfoInError (lifeCycleName: string) (pKey: string) (field: string) (bytes: byte[]) =
    match DataEncode.ofCompressedJsonText bytes with
    | Ok x -> x
    | Error err ->
        InvalidOperationException (sprintf "Unable to decode field %s: %s, %s ; Error: %A" field lifeCycleName pKey err)
        |> raise

type SqlServerGrainStorageHandler<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId
                 when 'Subject              :> Subject<'SubjectId>
                 and  'LifeAction           :> LifeAction
                 and  'OpError              :> OpError
                 and  'Constructor          :> Constructor
                 and  'LifeEvent            :> LifeEvent
                 and  'LifeEvent            : comparison
                 and  'SubjectId            :> SubjectId
                 and  'SubjectId            : comparison>
    (
        allTransferBlobHandlers: list<ITransferBlobHandler>,
        lifeCycleAdapter: HostedLifeCycleAdapter<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>,
        timeSeriesAdapters: TimeSeriesAdapterCollection,
        config: SqlServerConnectionStrings,
        logger: Microsoft.Extensions.Logging.ILogger<SqlServerGrainStorageHandler<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>>,
        remindersTriggeredManually: bool
    ) =

    let lifeCycleName = lifeCycleAdapter.LifeCycle.Def.LifeCycleKey.LocalLifeCycleName
    let ecosystemName = lifeCycleAdapter.LifeCycle.Def.LifeCycleKey.EcosystemName
    let sqlConnectionString = config.ForEcosystem ecosystemName

    let historyRetention =
        match lifeCycleAdapter.LifeCycle.Storage.Type with
        | StorageType.Persistent (_, historyRetention) ->
            historyRetention
        | StorageType.Volatile
        | StorageType.Custom _ ->
            failwith "unexpected storage type for Sql storage handler"

    let reminderImplementation =
        if remindersTriggeredManually then
            RemindersImplementation.TestNotPersistedManuallyTriggered
        else
            RemindersImplementation.Persisted

    let transferBlobHandlesByName =
        allTransferBlobHandlers
        |> Seq.map (fun i -> i.Name, i)
        |> Map.ofSeq

    let parseLifeCycleKey (lifeCycleName: string) =
        if lifeCycleName.Contains "/" then
            let ecosystemThenLifeCycleName = lifeCycleName.Split "/"
            LifeCycleKey (ecosystemThenLifeCycleName[1], ecosystemThenLifeCycleName[0])
        else
            LifeCycleKey (lifeCycleName, ecosystemName)

    let serializeLifeCycleKey =
        function
        | LifeCycleKey (lifeCycleName, ecosystemName')
            when ecosystemName' = ecosystemName ->
            lifeCycleName
        | LifeCycleKey (lifeCycleName, ecosystemName') ->
            sprintf "%s/%s" ecosystemName' lifeCycleName
        | OBSOLETE_LocalLifeCycleKey _ ->
            failwith "unexpected obsolete local LC key in sql storage layer"

    let readTickState (reader: SqlDataReader) (tickOnIndex: int) (tickFiredIndex: int) (tickContextIndex: int) : TickState =
        let hasTick = not (reader.IsDBNull tickOnIndex)
        let tickFired = not (reader.IsDBNull tickFiredIndex) && reader.GetBoolean tickFiredIndex
        let tickHasContext = not (reader.IsDBNull 4)
        match hasTick, tickFired, tickHasContext with
        | true,  false, true ->
            TickState.Scheduled (
                reader.GetDateTimeOffset tickOnIndex,
                (reader.Item tickContextIndex :?> byte[]) |> tryOfCompressedJson<TraceContext>)
        | true,  false, false ->
            TickState.Scheduled (reader.GetDateTimeOffset tickOnIndex, None)
        | true,  true,  false ->
            TickState.Fired (reader.GetDateTimeOffset tickOnIndex)
        | false, false, false ->
            TickState.NoTick
        | false, true, _
        | _, true, true ->
            shouldNotReachHereBecause "Fired tick can't be null or have context"
        | false, false, true  ->
            shouldNotReachHereBecause "Null tick can't have context"

    let readSubject (pKey: string) (reader: SqlDataReader) : Task<Option<{| ETag: string; Version: uint64; NextSideEffectSeqNum: uint64; PersistedGrainIdHash: uint32; SubjectState: SubjectState<'Subject,'SubjectId>; SkipHistoryOnNextOp: bool |}>>=
        backgroundTask {
            match! reader.ReadAsync() with
            | true ->
                let subjectBytes = reader.Item 0 :?> byte[]
                if isNull subjectBytes
                then
                    return None
                else
                    let tickState = readTickState reader 2 3 4
                    let ourSubscriptions : Map<SubscriptionName, SubjectReference> =
                        (reader.Item 5 :?> byte[])
                        |> ofCompressedJsonTextWithContextInfoInError lifeCycleName pKey "OurSubscriptions"

                    // amend obsolete LC key format, if it leaks further it'll break LC key equality
                    let ourSubscriptions =
                        ourSubscriptions
                        |> Map.mapValues (fun subjectRef ->
                            match subjectRef.LifeCycleKey with
                            | LifeCycleKey _ -> subjectRef
                            | OBSOLETE_LocalLifeCycleKey name ->
                                { subjectRef with LifeCycleKey = LifeCycleKey (name, ecosystemName) })

                    let subjectState = {
                        Subject          = subjectBytes |> ofCompressedJsonTextWithContextInfoInError lifeCycleName pKey "Subject" |> fun (x: Subject<'SubjectId>) -> x :?> 'Subject
                        LastUpdatedOn    = reader.GetDateTimeOffset(1)
                        TickState        = tickState
                        OurSubscriptions = ourSubscriptions
                    }

                    // this is a bit of a mess, take careful note of types in DB/F#:
                    let concurrencyToken = Array.zeroCreate<byte> 8 // concurrencyToken stored BigEndian uint64 encoded into binary(8)
                    reader.GetBytes(6, (int64 0), concurrencyToken, 0, 8) |> ignore
                    let nextSideEffectSeqNum = reader.GetInt64 7 |> int64ToUInt64MaintainOrder // nextSideEffectSeqNum stored as uint64 but in an int64 column
                    let version = reader.GetInt64 8 |> uint64 // version stored as +ve int64 so simple conversion OK
                    let persistedGrainIdHash = reader.GetInt32 9 |> int32ToUInt32MaintainOrder

                    let skipHistoryOnNextOp =
                        match historyRetention with
                        | PersistentHistoryRetention.Unfiltered _ -> false
                        | PersistentHistoryRetention.NoHistory _ -> true
                        | PersistentHistoryRetention.FilteredByTelemetryRules _ ->
                            match lifeCycleAdapter.LifeCycle.ShouldSendTelemetry with
                            | None -> false
                            | Some shouldSendTelemetry ->
                                match  (reader.Item 10 :?> byte[]) |> DataEncode.decodeSubjectAuditOperation with
                                | Ok op ->
                                    match op with
                                    | SubjectAuditOperation.Construct ctor ->
                                        shouldSendTelemetry (ShouldSendTelemetryFor.Constructor ctor)
                                    | SubjectAuditOperation.Act action ->
                                        shouldSendTelemetry (ShouldSendTelemetryFor.LifeAction action)
                                    |> not
                                | Error _ -> false
                        | PersistentHistoryRetention.FilteredByHistoryRules _ ->
                            match lifeCycleAdapter.LifeCycle.ShouldRecordHistory with
                            | None -> false
                            | Some shouldRecordHistory ->
                                match  (reader.Item 10 :?> byte[]) |> DataEncode.decodeSubjectAuditOperation with
                                | Ok op ->
                                    match op with
                                    | SubjectAuditOperation.Construct ctor ->
                                        shouldRecordHistory (ShouldRecordHistoryFor.Constructor ctor)
                                    | SubjectAuditOperation.Act action ->
                                        shouldRecordHistory (ShouldRecordHistoryFor.LifeAction action)
                                    |> not
                                | Error _ -> false

                    return
                      Some {| SubjectState = subjectState
                              NextSideEffectSeqNum = nextSideEffectSeqNum
                              ETag = (byteArrayToHexString concurrencyToken)
                              Version = version
                              PersistedGrainIdHash = persistedGrainIdHash
                              SkipHistoryOnNextOp = skipHistoryOnNextOp |}
             | false ->
                 return None
        }

    let readPreparedState (reader: SqlDataReader) =
        backgroundTask {
            match! reader.ReadAsync() with
            | true ->
                let preparedStateBytes = reader.Item 0 :?> byte[]
                if isNull preparedStateBytes
                then
                    return None
                else
                    let preparedInitializeConcurrencyToken = Array.zeroCreate<byte> 8
                    reader.GetBytes(2, (int64 0), preparedInitializeConcurrencyToken, 0, 8) |> ignore
                    return
                        Some {| PreparedStateBytes = preparedStateBytes
                                SubjectTransactionId = reader.GetGuid(1) |> SubjectTransactionId
                                ETag = byteArrayToHexString preparedInitializeConcurrencyToken |}
            | false ->
                return None
        }

    let readSubscriptions (pKey: string) (reader: SqlDataReader) : Task<OthersSubscribing<'LifeEvent>> =
        AsyncSeq.unfoldAsync (
            fun _ ->
                async {
                    match! reader.ReadAsync() |> Async.AwaitTask with
                    | true ->
                        let subjectPKeyRef: SubjectPKeyReference = {
                            LifeCycleKey = reader.GetString(0) |> parseLifeCycleKey
                            SubjectIdStr  = reader.GetString(1)
                        }

                        let subscriptionName = reader.GetString(2)

                        let lifeEvent =
                            (reader.Item 3 :?> byte[])
                            |> ofCompressedJsonTextWithContextInfoInError lifeCycleName pKey "LifeEvent"
                            |> fun (x: LifeEvent) ->  x :?> 'LifeEvent

                        return Some((lifeEvent, subscriptionName, subjectPKeyRef), Nothing)
                    | false ->
                        return None
                }
        ) Nothing
        |> AsyncSeq.toListAsync
        |> Async.map (
            fun result ->
                result
                |> Seq.groupBy (fun (lifeEvent, _, _) -> lifeEvent)
                |> Seq.map (
                    fun (lifeEvent, subscNamesAndRefs) ->
                        let subscNamesToRefs =
                            subscNamesAndRefs
                            |> Seq.groupBy (fun (_, subscriptionName, _) -> subscriptionName)
                            |> Seq.map (
                                fun (subscriptionName, refs) ->
                                    let refsSet =
                                        refs
                                        |> Seq.map (fun (_, _, ref) -> ref)
                                        |> Set.ofSeq

                                    subscriptionName, refsSet
                                )
                            |> Map.ofSeq
                        lifeEvent, subscNamesToRefs)
                |> Map.ofSeq)
        |> Async.StartAsTask

    let readSideEffects (pKey: string) (reader: SqlDataReader) : Task<KeyedSet<GrainSideEffectSequenceNumber, SideEffectGroup<'LifeAction, 'OpError>>> =

        // TODO: remove this? probably all legacy side effects already processed long time ago?
        let amendLifeCycleKey sideEffect =
            // amend obsolete LC key format in restored side effects, if it leaks further it'll break LC key equality
            match sideEffect with
            | CodecFriendlyGrainPersistedSideEffect.Rpc rpc ->
                match rpc.SubjectReference.LifeCycleKey with
                | LifeCycleKey _ ->
                    sideEffect
                | OBSOLETE_LocalLifeCycleKey name ->
                    CodecFriendlyGrainPersistedSideEffect.Rpc { rpc with SubjectReference = { rpc.SubjectReference with LifeCycleKey = LifeCycleKey (name, ecosystemName) } }
            | CodecFriendlyGrainPersistedSideEffect.RpcTransactionStep step ->
                match step.SubjectReference.LifeCycleKey with
                | LifeCycleKey _ ->
                    sideEffect
                | OBSOLETE_LocalLifeCycleKey name ->
                    CodecFriendlyGrainPersistedSideEffect.RpcTransactionStep { step with SubjectReference = { step.SubjectReference with LifeCycleKey = LifeCycleKey (name, ecosystemName) } }
            | CodecFriendlyGrainPersistedSideEffect.RpcTriggerSubscriptionOnGrain rpc ->
                match rpc.SubjectPKeyReference.LifeCycleKey with
                | LifeCycleKey _ ->
                    sideEffect
                | OBSOLETE_LocalLifeCycleKey name ->
                    CodecFriendlyGrainPersistedSideEffect.RpcTriggerSubscriptionOnGrain { rpc with SubjectPKeyReference = { rpc.SubjectPKeyReference with LifeCycleKey = LifeCycleKey (name, ecosystemName) } }
            | CodecFriendlyGrainPersistedSideEffect.RunActionOnSelf _
            | CodecFriendlyGrainPersistedSideEffect.TriggerTimerActionOnSelf _
            | CodecFriendlyGrainPersistedSideEffect.HandleSubscriptionResponseOnSelf _
            | CodecFriendlyGrainPersistedSideEffect.TryDeleteSelf _
            | CodecFriendlyGrainPersistedSideEffect.UpdateTimer _
            | CodecFriendlyGrainPersistedSideEffect.ClearTimer _ ->
                sideEffect

        AsyncSeq.unfoldAsync (
            fun _ ->
                async {
                    match! reader.ReadAsync() |> Async.AwaitTask with
                    | true ->
                        let sideEffectId = reader.GetGuid(0)
                        let sideEffectSeqNum = reader.GetInt64 2 |> int64ToUInt64MaintainOrder
                        let sideEffectBytes = reader.Item 1 :?> byte[]
                        let maybeSideEffectTarget =
                            if reader.IsDBNull 3 then
                                None
                            else
                                let sideEffectTargetJson = reader.GetString(3)
                                let sideEffectTarget: SideEffectTarget = ofJsonTextWithContextInfoInError lifeCycleName (sprintf "%s:%A" pKey sideEffectId) "SideEffectTarget" sideEffectTargetJson
                                Some sideEffectTarget

                        let sideEffect =
                            match maybeSideEffectTarget with
                            | None ->
                                let x: CodecFriendlyGrainPersistedSideEffect<LifeAction, OpError> =
                                    ofCompressedJsonTextWithContextInfoInError lifeCycleName (sprintf "%s:%A" pKey sideEffectId) "SideEffect" sideEffectBytes
                                (CodecFriendlyGrainPersistedSideEffect<'LifeAction, 'OpError>.CastUnsafe x)
                                |> amendLifeCycleKey
                                |> fun e -> e.AsGrainPersistedSideEffect None |> fst

                            | Some (SideEffectTarget.TimeSeries timeSeriesKey) ->
                                match timeSeriesAdapters.GetTimeSeriesAdapterByKey timeSeriesKey with
                                | Some adapter ->
                                    adapter.TimeSeries.Invoke
                                        { new FullyTypedTimeSeriesFunction<_> with
                                            // inlining of types is so finicky that I must use different 'TimeSeriesDataPoint type param name when reading and writing side effects
                                            member _.Invoke (timeSeries: TimeSeries<'TimeSeriesDataPointRead, _, _, _, _, _, _, _>) =
                                                let typedTimeSeriesSideEffect: TypedTimeSeriesSideEffect<'TimeSeriesDataPointRead, _, _> =
                                                    ofCompressedJsonTextWithContextInfoInError lifeCycleName (sprintf "%s:%A" pKey sideEffectId) "SideEffect" sideEffectBytes
                                                match typedTimeSeriesSideEffect with
                                                | TypedTimeSeriesSideEffect.Ingest (points, traceContext) ->
                                                    GrainPersistedSideEffect.IngestTimeSeries (timeSeriesKey, box points, traceContext) }
                                | None ->
                                    sprintf "TimeSeries with key %A not found" timeSeriesKey
                                    |> InvalidOperationException
                                    |> raise

                            |> GrainSideEffect.Persisted
                        return Some((sideEffectSeqNum, (sideEffectId, sideEffect)), Nothing)
                    | false ->
                        return None
                }
        ) Nothing
        |> AsyncSeq.toListAsync
        |> Async.map (fun sideEffects ->
            sideEffects
            |> Seq.groupBy fst
            |> Seq.map (fun (seqNum, group) -> {
                    SequenceNumber = seqNum
                    SideEffects = group |> Seq.map snd |> NonemptyMap.ofSeq |> Option.get
                    RehydratedFromStorage = true
                })
            |> KeyedSet.ofSeq
        )
        |> Async.StartAsTask

    let readSideEffectDedupCache (reader: SqlDataReader) : Task<Map<SubjectPKeyReference, GrainSideEffectId * DateTimeOffset>> =
        AsyncSeq.unfoldAsync (
            fun _ ->
                async {
                    match! reader.ReadAsync() |> Async.AwaitTask with
                    | true ->
                        // SELECT CallerLifeCycleName, CallerId, CallId, CalledOn FROM [%s].[%s_CallDedup] WHERE SubjectId = @id
                        let callerLifeCycleName = reader.GetString(0)
                        let callerId = reader.GetString(1)
                        let callId = reader.GetGuid(2)
                        let calledOn = reader.GetDateTimeOffset 3
                        let callerRef : SubjectPKeyReference = { LifeCycleKey = parseLifeCycleKey callerLifeCycleName; SubjectIdStr = callerId }

                        return Some((callerRef, (callId, calledOn)), Nothing)
                    | false ->
                        return None
                }
        ) Nothing
        |> AsyncSeq.toListAsync
        |> Async.map Map.ofSeq
        |> Async.StartAsTask

    let tryGetState (pKey: string) : Task<Option<ReadStateResult<'Subject, 'Constructor, 'LifeAction, 'LifeEvent, 'OpError, 'SubjectId>>> =
        fun () -> backgroundTask {
            let subjectSql =
                sprintf "SELECT Subject, SubjectLastUpdatedOn, NextTickOn, NextTickFired, NextTickContext, OurSubscriptions, ConcurrencyToken, NextSideEffectSeqNumber, Version, GrainIdHash, Operation FROM [%s].[%s] WHERE Id = @id"
                    ecosystemName lifeCycleName

            let preparedStateSql =
                sprintf "SELECT PreparedTransactionalState, SubjectTransactionId, PreparedInitializeConcurrencyToken FROM [%s].[%s_Prepared] WHERE Id = @id"
                    ecosystemName lifeCycleName

            let subscriptionsSql =
                sprintf "SELECT LifeCycleName, SubscriberId, SubscriptionName, LifeEvent FROM [%s].[%s_Subscription] WHERE SubjectId = @id"
                    ecosystemName lifeCycleName

            let sideEffectSql =
                // failed side effects not loaded, they should be manually retried or deleted by ops via Meta subject
                sprintf "SELECT SideEffectId, SideEffect, SideEffectSeqNumber, SideEffectTarget FROM [%s].[%s_SideEffect] WHERE SubjectId = @id AND FailureSeverity IS NULL"
                    ecosystemName lifeCycleName

            let callDedupSql =
                sprintf "SELECT CallerLifeCycleName, CallerId, CallId, CalledOn FROM [%s].[%s_CallDedup] WHERE SubjectId = @id"
                    ecosystemName lifeCycleName

            let sql = sprintf "%s;%s;%s;%s;%s" subjectSql preparedStateSql subscriptionsSql sideEffectSql callDedupSql

            use connection = new SqlConnection(sqlConnectionString)
            do! connection.OpenAsync()
            use command = new SqlCommand(sql, connection)
            command.Parameters.Add("@id", SqlDbType.NVarChar).Value <- pKey
            use! reader = command.ExecuteReaderAsync()

            let! subjectResult = readSubject pKey reader

            let! preparedStateResult =
                backgroundTask {
                    match! reader.NextResultAsync() with
                    | true ->
                        return! readPreparedState reader
                    | false ->
                        return None
                }

            match (subjectResult, preparedStateResult) with
            | (None, Some preparedState) ->
                let preparedInsertData =
                    preparedState.PreparedStateBytes
                    |> ofCompressedJsonTextWithContextInfoInError lifeCycleName pKey "PreparedSubjectInsertData"
                    |> fun (x: PreparedSubjectInsertData<Subject<'SubjectId>, Constructor, 'SubjectId, LifeAction, LifeEvent, OpError>) -> PreparedSubjectInsertData<'Subject, 'Constructor,'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>.CastUnsafe x

                return {
                    SubjectStateContainer = SubjectStateContainer.PreparedInitialize (preparedInsertData,  preparedState.ETag, preparedState.SubjectTransactionId)
                    PendingSideEffects    = KeyedSet.empty
                    PersistedGrainIdHash  = None
                } |> Some

            | (Some subjectResult, maybePreparedState) ->
                let! subscriptions =
                    backgroundTask {
                        match! reader.NextResultAsync() with
                        | true ->
                            return! readSubscriptions pKey reader
                        | false ->
                            return Map.empty
                    }

                let! sideEffects =
                    backgroundTask {
                        match! reader.NextResultAsync() with
                        | true ->
                            return! readSideEffects pKey reader
                        | false ->
                            return KeyedSet.empty
                    }

                let! sideEffectDedupCache =
                    backgroundTask {
                        match! reader.NextResultAsync() with
                        | true ->
                            return! readSideEffectDedupCache reader
                        | false ->
                            return Map.empty
                    }

                let currentStateContainer = {
                    CurrentSubjectState      = subjectResult.SubjectState
                    CurrentOthersSubscribing = subscriptions
                    ETag                     = subjectResult.ETag
                    Version                  = subjectResult.Version
                    NextSideEffectSeqNum     = subjectResult.NextSideEffectSeqNum
                    SideEffectDedupCache     = sideEffectDedupCache
                    SkipHistoryOnNextOp      = subjectResult.SkipHistoryOnNextOp
                }

                let stateContainer =
                    match maybePreparedState with
                    | None ->
                        SubjectStateContainer.Committed currentStateContainer
                    | Some preparedState ->
                        let preparedUpdateData =
                            preparedState.PreparedStateBytes
                            |> ofCompressedJsonTextWithContextInfoInError lifeCycleName pKey "PreparedSubjectUpdateData"
                            |> fun (x: PreparedSubjectUpdateData<Subject<'SubjectId>, 'SubjectId, LifeAction, LifeEvent, OpError>) -> PreparedSubjectUpdateData<'Subject,'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>.CastUnsafe x

                        SubjectStateContainer.PreparedAction (currentStateContainer, preparedUpdateData, preparedState.SubjectTransactionId)

                return {
                    SubjectStateContainer = stateContainer
                    PendingSideEffects    = sideEffects
                    PersistedGrainIdHash  = Some subjectResult.PersistedGrainIdHash
                } |> Some

            | (None, None) ->
                return None
        }
        |> SqlServerTransientErrorDetection.wrapTransientExceptions

    let getSideEffectIdsListDataTable (sideEffectIds: Set<Guid>) =
        let dataTable = new DataTable()
        dataTable.Columns.Add("Id", typeof<Guid>) |> ignore
        sideEffectIds
        |> Seq.iter (fun sideEffectId -> dataTable.Rows.Add(sideEffectId) |> ignore)
        dataTable

    let readFailedSideEffects
        (subjectId: string)
        (sideEffectIds: NonemptySet<GrainSideEffectId>)
        : Task<KeyedSet<GrainSideEffectSequenceNumber, SideEffectGroup<'LifeAction, 'OpError>>> =
        fun () -> backgroundTask {
            let sql =
                sprintf "
                    SELECT se.SideEffectId, se.SideEffect, se.SideEffectSeqNumber, se.SideEffectTarget
                    FROM [%s].[%s_SideEffect] AS se
                    JOIN @failedSideEffectsToRetry input ON input.[Id] = se.SideEffectId
                    WHERE SubjectId = @id AND FailureSeverity IS NOT NULL"
                    ecosystemName lifeCycleAdapter.LifeCycle.Name

            use connection = new SqlConnection(sqlConnectionString)
            do! connection.OpenAsync()
            use command = new SqlCommand(sql, connection)

            command.Parameters.Add("@id", SqlDbType.NVarChar).Value <- subjectId
            let param = command.Parameters.Add("@failedSideEffectsToRetry", SqlDbType.Structured)
            param.TypeName <- sprintf "[%s].GuidList" ecosystemName
            param.Value <- getSideEffectIdsListDataTable sideEffectIds.ToSet

            use! reader = command.ExecuteReaderAsync()
            return! readSideEffects subjectId reader
        }
        |> SqlServerTransientErrorDetection.wrapTransientExceptions

    let getMaybeIncrementSideEffectsDataTable
            pKey
            (newSideEffects: Option<NonemptyKeyedSet<GrainSideEffectSequenceNumber, SideEffectGroup<'LifeAction, 'OpError>>>)
            : DataTable =
        let dataTable = new DataTable()
        dataTable.Columns.Add("Id", typeof<Guid>) |> ignore
        dataTable.Columns.Add("SideEffectTarget", typeof<string>) |> ignore
        dataTable.Columns.Add("SideEffect", typeof<byte[]>) |> ignore
        dataTable.Columns.Add("SideEffectSeqNumber", typeof<int64>) |> ignore

        match newSideEffects with
        | Some newSideEffects ->
            newSideEffects
            |> NonemptyKeyedSet.toSeq
            |> Seq.sortBy (fun g -> g.SequenceNumber)
            |> Seq.iter (fun g ->
                let sideEffectIdAndPersistedSideEffects =
                    g.SideEffects
                    |> NonemptyMap.toMap
                    |> Map.toList
                    |> List.choose (fun (sideEffectId, sideEffect) ->
                        match sideEffect with
                        | GrainSideEffect.Persisted persistedSideEffect -> Some (sideEffectId, persistedSideEffect)
                        // Transient side effects aren't written to the store
                        | GrainSideEffect.Transient _ -> None)
                sideEffectIdAndPersistedSideEffects
                |> GrainPersistedSideEffect.AsCodecFriendlyData
                |> List.iter (fun (sideEffectId, codecFriendlyPersistedSideEffect) ->
                    let sideEffectBytes, sideEffectTargetObj =
                        match codecFriendlyPersistedSideEffect with
                        | Choice1Of2 e ->
                            e
                            |> CodecFriendlyGrainPersistedSideEffect<LifeAction, OpError>.CastUnsafe
                            // TODO Enable the JSON size constrain for the side effects when we can better handle the current blob situation
                            // |> toCompressedJsonControlledForSize logger.LogWarning lifeCycleName pKey "sideEffect"
                            |> DataEncode.toCompressedJsonUnchecked, // Disabled roundtrip serialization check for side effects only.
                            box DBNull.Value
                        | Choice2Of2 (timeSeriesKey, ``list<'TimeSeriesDataPoint>``, traceContext) ->
                            match timeSeriesAdapters.GetTimeSeriesAdapterByKey timeSeriesKey with
                            | Some adapter ->
                                adapter.TimeSeries.Invoke
                                    { new FullyTypedTimeSeriesFunction<_> with
                                        // inlining of types is so finicky that I must use different 'TimeSeriesDataPoint type param name when reading and writing side effects
                                        member _.Invoke (timeSeries: TimeSeries<'TimeSeriesDataPointWrite, _, _, _, _, _, _, _>) =
                                            let points = ``list<'TimeSeriesDataPoint>`` :?> list<'TimeSeriesDataPointWrite>
                                            TypedTimeSeriesSideEffect.Ingest (points, traceContext)
                                            |> toCompressedJsonControlledForSize logger.LogWarning lifeCycleName pKey "sideEffect",
                                            SideEffectTarget.TimeSeries timeSeriesKey
                                            |> toJsonTextControlledForSize logger.LogWarning lifeCycleName pKey "sideEffectTarget"
                                            |> box }
                            | None ->
                                PermanentSubjectException ("sideEffectsTable", sprintf "TimeSeries with key %A not found" timeSeriesKey)
                                |> raise

                    dataTable.Rows.Add(sideEffectId, sideEffectTargetObj, sideEffectBytes, uint64ToInt64MaintainOrder g.SequenceNumber)
                    |> ignore
                    ))

            dataTable
        | None ->
            dataTable

    let getEmptyIndexingListDataTable () =
        let dataTable = new DataTable()
        dataTable.Columns.Add("Key",      typeof<string>).MaxLength <- 80
        dataTable.Columns.Add("ValueStr", typeof<string>).MaxLength <- -1
        dataTable.Columns.Add("ValueInt", typeof<int64>)   |> ignore
        dataTable.Columns.Add("Kind",     typeof<int>) |> ignore
        dataTable.Columns.Add("IsDelete", typeof<bool>) |> ignore

        dataTable

    let getEmptyPromotedIndexingListDataTable () =
        let dataTable = new DataTable()
        dataTable.Columns.Add("PromotedKey",   typeof<string>).MaxLength <- 240
        dataTable.Columns.Add("PromotedValue", typeof<string>).MaxLength <- -1
        dataTable.Columns.Add("Key",           typeof<string>).MaxLength <- 80
        dataTable.Columns.Add("ValueStr",      typeof<string>).MaxLength <- -1
        dataTable.Columns.Add("ValueInt",      typeof<int64>) |> ignore
        dataTable.Columns.Add("IsDelete",      typeof<bool>) |> ignore
        dataTable

    let getEmptyReEncodeSubjectsListDataTable () =
        let dataTable = new DataTable()
        dataTable.Columns.Add("Id",                typeof<string>).MaxLength <- 80
        dataTable.Columns.Add("Version",           typeof<int64>)  |> ignore
        dataTable.Columns.Add("Subject",           typeof<byte[]>) |> ignore
        dataTable.Columns.Add("Operation",         typeof<byte[]>) |> ignore
        dataTable.Columns.Add("OurSubscriptions",  typeof<byte[]>) |> ignore
        dataTable.Columns.Add("ConcurrencyToken",  typeof<byte[]>) |> ignore
        dataTable

    let getIndexViolationError (key: string) (indexActions: list<IndexAction<'OpError>>) =
        indexActions
        |> Seq.choose (
            function
            | IndexAction.InsertNumeric (k, _, Some err)
            | IndexAction.InsertString  (k, _, Some err) when k = key ->
                Some err
            | _ -> None)
        |> Seq.head

    let getUniqueIndexReserveViolationError (key: string) (uniqueIndicesToReserve: list<UniqueIndexToReserveOnPrepare<'OpError>>) : 'OpError =
        uniqueIndicesToReserve
        |> Seq.choose (
            function
            | UniqueIndexToReserveOnPrepare.Numeric (k, _, err)
            | UniqueIndexToReserveOnPrepare.String  (k, _, err) when k = key ->
                Some err
            | _ -> None)
        |> Seq.head

    let getCallDedupListDataTable (dedupData: Option<UpsertDedupData>) =
        let dataTable = new DataTable()
        dataTable.Columns.Add("CallerLifeCycleName", typeof<string>).MaxLength <- 80
        dataTable.Columns.Add("CallerId",            typeof<string>).MaxLength <- 80
        dataTable.Columns.Add("CallId",              typeof<Guid>)   |> ignore
        dataTable.Columns.Add("IsInsert",            typeof<bool>)   |> ignore
        dataTable.Columns.Add("IsDelete",            typeof<bool>)   |> ignore
        [
            match dedupData with
            | None -> ()
            | Some data ->
                match data.EvictedDedupInfoToDelete with
                | Some (evicted, _) ->
                    evicted.Caller, (* callId *) None, (* IsInsert *) false, (* isDelete *) true
                | None -> ()
                data.DedupInfo.Caller, Some data.DedupInfo.Id, data.IsInsert, (* IsDelete *) false
        ]
        |> List.iter (
            fun (key, maybeCallId, isInsert, isDelete) ->
                let callId = maybeCallId |> Option.toNullable
                let qualifiedLifeCycleName = serializeLifeCycleKey key.LifeCycleKey
                dataTable.Rows.Add (qualifiedLifeCycleName, key.SubjectIdStr, callId, isInsert, isDelete) |> ignore)

        dataTable

    let getEmptyBlobActionListDataTable () =
        let dataTable = new DataTable()
        dataTable.Columns.Add("Id",             typeof<Guid>)   |> ignore
        dataTable.Columns.Add("Revision",       typeof<int>)    |> ignore
        dataTable.Columns.Add("NewRevision",    typeof<int>)    |> ignore
        dataTable.Columns.Add("MimeType",       typeof<string>).MaxLength <- 127
        dataTable.Columns.Add("Data",           typeof<byte[]>) |> ignore
        dataTable.Columns.Add("IsDelete",       typeof<bool>)   |> ignore
        dataTable.Columns.Add("IsAppend",       typeof<bool>)   |> ignore
        dataTable.Columns.Add("TransferBlobId", typeof<Guid>)   |> ignore

        dataTable

    let resolveTransferBlobs (blobActions: List<BlobAction>) : Task<List<BlobAction> * list<ITransferBlobHandler * Set<Guid>>> =
        backgroundTask {
            let transferBlobIdsByHandler =
                blobActions
                |> Seq.choose (
                    function
                    | BlobAction.Create (_, _, FileData.InternalOnlyTransferBlob(handlerName, transferBlobId, _)) ->
                        match transferBlobHandlesByName.TryFind handlerName with
                        | Some handler ->
                            Some (handler, transferBlobId)
                        | None ->
                            failwithf "TransferBlobHandler with name %s is not found" handlerName
                    | _ -> None)
                |> Seq.groupBy fst
                |> Seq.map (fun (handler, items) ->
                    (handler, items |> Seq.map snd |> Set.ofSeq)
                )
                |> Seq.where (fun (transferBlobHandler, _) ->
                    // Filter out transfer blobs that use SQL provider with the same connection string; these can be moved directly within SQL
                    // If the connection string is the same, we're guaranteed to be able to move it directly within SQL Server itself
                    match transferBlobHandler with
                    | :? SqlServerTransferBlobHandler as sqlTransferHandler ->
                        sqlTransferHandler.SqlConnectionString <> sqlConnectionString
                    | _ -> true)
                |> Seq.toList

            if transferBlobIdsByHandler.Length > 0 then
                let allTransferBlobIds =
                    transferBlobIdsByHandler
                    |> Seq.map snd
                    |> Seq.map (Set.toSeq)
                    |> Seq.collect id
                    |> Set.ofSeq

                let! transferBlobsByIdsManyMaps =
                    transferBlobIdsByHandler
                    |> Seq.map (fun (transferBlobHandler, transferBlobIds) -> transferBlobHandler.GetTransferBlobs transferBlobIds)
                    |> Task.WhenAll

                let transferBlobsByIds =
                    transferBlobsByIdsManyMaps
                    |> Seq.map (Map.toSeq)
                    |> Seq.collect id
                    |> Map.ofSeq

                let missingTransferBlobIds =
                    transferBlobsByIds
                    |> MapExtensions.Map.keys // F#+ hides Map.keys
                    |> Set.difference allTransferBlobIds

                if missingTransferBlobIds.Count > 0 then
                    failwithf "TransferBlobs for the following IDs were not found: %A" missingTransferBlobIds

                // At this point, the only InternalOnlyTransferBlob which wont be resolved are those within SQL Server

                let updatedActions =
                    blobActions
                    |> List.map (fun fileData ->
                        match fileData with
                        | BlobAction.Create (blobId, maybeMimeType, FileData.InternalOnlyTransferBlob(_, transferBlobId, _)) ->
                            match transferBlobsByIds.TryFind transferBlobId with
                            | Some temporaryBlob ->
                                BlobAction.Create (blobId, maybeMimeType, FileData.Bytes(temporaryBlob))
                            | None ->
                                fileData
                        | _ -> fileData)

                return (updatedActions, transferBlobIdsByHandler)
            else
                return (blobActions, [])
        }

    let getBlobActionListDataTable (blobActions: List<BlobAction>) : Task<DataTable * list<ITransferBlobHandler * Set<Guid>>> =
        backgroundTask {
            let! (blobActions, transferBlobIdsByHandlers) = resolveTransferBlobs blobActions
            let dataTable = getEmptyBlobActionListDataTable ()
            blobActions
            |> List.map (
                function
                | BlobAction.Create (blobId, mimeType, fileData) ->
                    let maybeBytes =
                        match fileData with
                        | FileData.Bytes _ | FileData.Base64 _ -> fileData.ToBytes |> Some
                        | FileData.InternalOnlyTransferBlob _ -> None

                    let maybeTransferBlobId =
                        match fileData with
                        | FileData.Bytes _ | FileData.Base64 _ -> None
                        | FileData.InternalOnlyTransferBlob (_, transferBlobId, _) -> Some transferBlobId

                    (blobId.Id, blobId.Revision, (* newRevision *) None, mimeType, maybeBytes, (* IsDelete *) false, (* IsAppend *) false, (* TransferBlobId *) maybeTransferBlobId)

                | BlobAction.Append (blobId, updatedBlobId, bytesToAppend) ->
                    (blobId.Id, blobId.Revision, Some updatedBlobId.Revision, (* mimeType *) None, Some bytesToAppend, (* IsDelete *) false, (* IsAppend *) true, (* TransferBlobId *)  None)

                | BlobAction.Delete blobId ->
                    (blobId.Id, blobId.Revision, (* newRevision *) None, (* mimeType *) None, (* bytes *) None, (* IsDelete *) true, (* IsAppend *) false, (* TransferBlobId *)  None))

            |> List.iter (
                    fun (blobId, revision, maybeNewRevision, maybeMimeType, maybeData, isDelete, isAppend, maybeTransferBlobId) ->
                        let newRevision = maybeNewRevision |> Option.map (int >> box) |> Option.defaultValue null
                        let data = maybeData |> Option.defaultValue null
                        let mimeType = maybeMimeType |> Option.map (fun v -> v.Value) |> Option.defaultValue null
                        let transferBlobId = maybeTransferBlobId |> Option.toNullable
                        dataTable.Rows.Add (blobId, int revision, newRevision, mimeType, data, isDelete, isAppend, transferBlobId) |> ignore
                )

            return (dataTable, transferBlobIdsByHandlers)
        }

    let getIndexingListDataTable (indexActions: list<IndexAction<'OpError>>) : DataTable =

        let dataTable = getEmptyIndexingListDataTable()
        indexActions
        |> Seq.choose (
            function
            | IndexAction.InsertNumeric (key, value, None)                 -> Some (key, None, Some value, IndexKindCode.NumericNonUnique, (* isDelete *) false)
            | IndexAction.DeleteNumeric (key, value, (* isUnique *) false) -> Some (key, None, Some value, IndexKindCode.NumericNonUnique, (* isDelete *) true)
            | IndexAction.InsertNumeric (key, value, Some _uniqueErr)      -> Some (key, None, Some value, IndexKindCode.NumericUnique, (* isDelete *) false)
            | IndexAction.DeleteNumeric (key, value, (* isUnique *) true)  -> Some (key, None, Some value, IndexKindCode.NumericUnique, (* isDelete *) true)
            | IndexAction.InsertString  (key, value, None)                 -> Some (key, Some value, None, IndexKindCode.StringNonUnique, (* isDelete *) false)
            | IndexAction.DeleteString  (key, value, (* isUnique *) false) -> Some (key, Some value, None, IndexKindCode.StringNonUnique, (* isDelete *) true)
            | IndexAction.InsertString  (key, value, Some _uniqueErr)      -> Some (key, Some value, None, IndexKindCode.StringUnique (* 4 - string unique *), (* isDelete *) false)
            | IndexAction.DeleteString  (key, value, (* isUnique *) true)  -> Some (key, Some value, None, IndexKindCode.StringUnique, (* isDelete *) true)
            | IndexAction.InsertSearch  (key, value)                       -> Some (key, Some value, None, IndexKindCode.StringSearch (* 5 string search *), (* isDelete *) false)
            | IndexAction.DeleteSearch  (key, value)                       -> Some (key, Some value, None, IndexKindCode.StringSearch, (* isDelete *) true)
            | IndexAction.InsertGeography  (key, value)                    -> Some (key, Some value, None, IndexKindCode.Geography (* 6 - geography *), (* isDelete *) false)
            | IndexAction.DeleteGeography  (key, value)                    -> Some (key, Some value, None, IndexKindCode.Geography, (* isDelete *) true)
            | IndexAction.PromotedInsertNumeric _
            | IndexAction.PromotedDeleteNumeric _
            | IndexAction.PromotedInsertString  _
            | IndexAction.PromotedDeleteString  _ -> None)
        |> Seq.iter (
            fun (key, optStr, optNum, kindCode, isDelete) ->
                let valueStr = optStr |> Option.map box |> Option.defaultValue null
                let valueInt = optNum |> Option.map box |> Option.defaultValue null
                dataTable.Rows.Add(key, valueStr, valueInt, kindCode, isDelete) |> ignore)
        dataTable

    let getReserveUniqueIndicesDataTable (uniqueIndicesToReserve: list<UniqueIndexToReserveOnPrepare<'OpError>>) : DataTable =
        let dataTable = getEmptyIndexingListDataTable()
        uniqueIndicesToReserve
        |> Seq.map (
            function
            | UniqueIndexToReserveOnPrepare.Numeric (key, value, _uniqueErr) -> (key, None, Some value, IndexKindCode.NumericUnique)
            | UniqueIndexToReserveOnPrepare.String  (key, value, _uniqueErr) -> (key, Some value, None, IndexKindCode.StringUnique))
        |> Seq.iter (
            fun (key, optStr, optNum, kindCode) ->
                let valueStr = optStr |> Option.map box |> Option.defaultValue null
                let valueInt = optNum |> Option.map box |> Option.defaultValue null
                dataTable.Rows.Add(key, valueStr, valueInt, kindCode, (* isDelete *) false) |> ignore)
        dataTable

    let getReleaseUniqueIndicesDataTable (uniqueIndicesToRelease: list<UniqueIndexToReleaseOnRollback>) : DataTable =
        let dataTable = getEmptyIndexingListDataTable()
        uniqueIndicesToRelease
        |> Seq.map (
            function
            | UniqueIndexToReleaseOnRollback.Numeric (key, value) -> (key, None, Some value, IndexKindCode.NumericUnique)
            | UniqueIndexToReleaseOnRollback.String  (key, value) -> (key, Some value, None, IndexKindCode.StringUnique))
        |> Seq.iter (
            fun (key, optStr, optNum, kindCode) ->
                let valueStr = optStr |> Option.map box |> Option.defaultValue null
                let valueInt = optNum |> Option.map box |> Option.defaultValue null
                dataTable.Rows.Add(key, valueStr, valueInt, kindCode, (* isDelete *) true) |> ignore)
        dataTable

    let getPromotedIndexingListDataTable (indexActions: list<IndexAction<'OpError>>) : DataTable =
        let dataTable = getEmptyPromotedIndexingListDataTable()
        indexActions
        |> Seq.choose (
            function
            | IndexAction.InsertNumeric _
            | IndexAction.DeleteNumeric _
            | IndexAction.InsertString _
            | IndexAction.DeleteString _
            | IndexAction.InsertSearch _
            | IndexAction.DeleteSearch _
            | IndexAction.InsertGeography _
            | IndexAction.DeleteGeography _ -> None
            | IndexAction.PromotedInsertNumeric (promotedKey, promotedValue, baseKey, baseValue) -> Some (promotedKey, promotedValue, baseKey, None, Some baseValue, false)
            | IndexAction.PromotedDeleteNumeric (promotedKey, promotedValue, baseKey, baseValue) -> Some (promotedKey, promotedValue, baseKey, None, Some baseValue, true)
            | IndexAction.PromotedInsertString  (promotedKey, promotedValue, baseKey, baseValue) -> Some (promotedKey, promotedValue, baseKey, Some baseValue, None, false)
            | IndexAction.PromotedDeleteString  (promotedKey, promotedValue, baseKey, baseValue) -> Some (promotedKey, promotedValue, baseKey, Some baseValue, None, true))
        |> Seq.iter (
            fun (promotedKey, promotedValue, key, optStr, optNum, isDelete) ->
                let valueStr = optStr |> Option.map box |> Option.defaultValue null
                let valueInt = optNum |> Option.map box |> Option.defaultValue null
                dataTable.Rows.Add(promotedKey, promotedValue, key, valueStr, valueInt, isDelete) |> ignore)
        dataTable

    let getEmptySubscriptionNameAndLifeEventListDataTable () =
        let dataTable = new DataTable()
        dataTable.Columns.Add("SubscriptionName", typeof<string>).MaxLength <- 200
        dataTable.Columns.Add("LifeEvent",        typeof<byte[]>).MaxLength <- -1
        dataTable

    let getSubscriptionNameAndLifeEventListDataTable pKey (subscriptions: Map<SubscriptionName, 'LifeEvent>) =
        let dataTable = getEmptySubscriptionNameAndLifeEventListDataTable()
        subscriptions
        |> Map.iter (
            fun subscriptionName lifeEvent ->
                let lifeEventBytes =
                    lifeEvent :> LifeEvent
                    |> toCompressedJsonControlledForSize logger.LogWarning lifeCycleName pKey "lifeEvent"
                dataTable.Rows.Add(subscriptionName, lifeEventBytes) |> ignore
        )
        dataTable

    let tickStateToNextTickOnAndNextTickFields =
        function
        | TickState.Scheduled (nextTickOn, maybeTraceContext) ->
            Some nextTickOn, false, maybeTraceContext
        | TickState.Fired originalNextTickOn ->
            Some originalNextTickOn, true, None
        | TickState.NoTick ->
            None, false, None

    let prepareInitializeSubject
        (pKey: string)
        (preparedInsertData: PreparedSubjectInsertData<'Subject, 'Constructor, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>)
        (uniqueIndicesToReserve: list<UniqueIndexToReserveOnPrepare<'OpError>>)
        (subjectTransactionId: SubjectTransactionId)
        : Task<Result<(* ETag *) string, 'OpError>> =
        fun () -> backgroundTask {
            use connection = new SqlConnection(sqlConnectionString)
            do! connection.OpenAsync()

            use command = new SqlCommand((sprintf "[%s].%s_PrepareInitialize_V2" ecosystemName lifeCycleName), connection)

            command.CommandType <- CommandType.StoredProcedure
            command.Parameters.Add("@id", SqlDbType.NVarChar).Value <- pKey

            let concurrencyToken = SqlParameter("@concurrencyToken", SqlDbType.Binary, 8)
            concurrencyToken.Direction <- ParameterDirection.InputOutput
            command.Parameters.Add concurrencyToken |> ignore
            concurrencyToken.Value <- DBNull.Value |> box

            command.Parameters.Add("@preparedTransactionalState", SqlDbType.VarBinary).Value <-
                (preparedInsertData
                 |> PreparedSubjectInsertData<Subject<'SubjectId>, Constructor, 'SubjectId, LifeAction, LifeEvent, OpError>.CastUnsafe
                 |> toCompressedJsonControlledForSize logger.LogWarning lifeCycleName pKey "preparedTransactionalState"
                 |> box)

            let (SubjectTransactionId transactionId) = subjectTransactionId
            command.Parameters.Add("@subjectTransactionId", SqlDbType.UniqueIdentifier).Value <- transactionId |> box

            let param = command.Parameters.Add("@uniqueIndicesToReserve", SqlDbType.Structured)
            param.TypeName <- sprintf "[%s].IndexingList_V3" ecosystemName
            param.Value <- getReserveUniqueIndicesDataTable uniqueIndicesToReserve

            let offSyncConcurrencyToken = SqlParameter("@offSyncConcurrencyToken", SqlDbType.Binary, 8)
            offSyncConcurrencyToken.Direction <- ParameterDirection.Output
            command.Parameters.Add offSyncConcurrencyToken |> ignore

            let violatedUniqueIndexKey = SqlParameter("@violatedUniqueIndexKey", SqlDbType.NVarChar, 80)
            violatedUniqueIndexKey.Direction <- ParameterDirection.Output
            command.Parameters.Add violatedUniqueIndexKey |> ignore

            do! command.ExecuteNonQueryAsync() |> Task.Ignore

            if violatedUniqueIndexKey.Value <> null && violatedUniqueIndexKey.Value <> (box DBNull.Value) then
                let violatedIndexKey = violatedUniqueIndexKey.Value :?> string
                let error = getUniqueIndexReserveViolationError violatedIndexKey uniqueIndicesToReserve
                return Error error

            elif offSyncConcurrencyToken.Value <> null && offSyncConcurrencyToken.Value <> (box DBNull.Value) then
                let updatedETag = offSyncConcurrencyToken.Value :?> byte[] |> byteArrayToHexString
                return raise (Orleans.Storage.InconsistentStateException("ETag doesn't match", updatedETag, "<MISSING>"))

            elif concurrencyToken.Value <> null && concurrencyToken.Value <> (box DBNull.Value) then
                let eTag = concurrencyToken.Value :?> byte[] |> byteArrayToHexString
                return Ok eTag
            else
                return failwith "prepareInitializeSubject neither prepared state nor detected concurrently prepared state, this is bug"
        }
        |> SqlServerTransientErrorDetection.wrapTransientExceptions

    let rollbackInitializeSubject (pKey: string) (subjectTransactionId: SubjectTransactionId) (uniqueIndicesToRelease: list<UniqueIndexToReleaseOnRollback>) (eTag: string) : Task =
        fun () -> backgroundTask {
            let (maybeETag,
                 maybeSubjectTransactionId
                 ) =
                    Some eTag,
                    Some subjectTransactionId

            use connection = new SqlConnection(sqlConnectionString)
            do! connection.OpenAsync()

            use command = new SqlCommand((sprintf "[%s].%s_RollbackPreparedInitialize_V2" ecosystemName lifeCycleName), connection)

            command.CommandType <- CommandType.StoredProcedure
            command.Parameters.Add("@id", SqlDbType.NVarChar).Value <- pKey

            let concurrencyToken = SqlParameter("@concurrencyToken", SqlDbType.Binary, 8)
            concurrencyToken.Direction <- ParameterDirection.InputOutput
            command.Parameters.Add concurrencyToken |> ignore
            concurrencyToken.Value <-
                maybeETag |> Option.map (hexStringToByteArray >> box)
                |> Option.defaultValue (DBNull.Value |> box)

            command.Parameters.Add("@subjectTransactionId", SqlDbType.UniqueIdentifier).Value <-
                (maybeSubjectTransactionId |> Option.map (fun (SubjectTransactionId transactionId) -> transactionId |> box)
                 |> Option.defaultValue (DBNull.Value |> box))

            let param = command.Parameters.Add("@uniqueIndicesToRelease", SqlDbType.Structured)
            param.TypeName <- sprintf "[%s].IndexingList_V3" ecosystemName
            param.Value <- getReleaseUniqueIndicesDataTable uniqueIndicesToRelease

            let offSyncConcurrencyToken = SqlParameter("@offSyncConcurrencyToken", SqlDbType.Binary, 8)
            offSyncConcurrencyToken.Direction <- ParameterDirection.Output
            command.Parameters.Add offSyncConcurrencyToken |> ignore

            do! command.ExecuteNonQueryAsync() |> Task.Ignore

            if offSyncConcurrencyToken.Value <> null && offSyncConcurrencyToken.Value <> (box DBNull.Value) then
                let updatedETag = offSyncConcurrencyToken.Value :?> byte[] |> byteArrayToHexString
                raise (Orleans.Storage.InconsistentStateException("ETag doesn't match", updatedETag, maybeETag |> Option.defaultValue "<MISSING>"))
            return ()
        }
        |> SqlServerTransientErrorDetection.wrapTransientExceptions
        |> Task.Ignore

    let initializeOrCommitPreparedInitializeSubject
            (pKey: string)
            (insertData: SubjectInsertData<'Subject, 'Constructor, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>)
            (payload: Choice<Option<SideEffectDedupInfo>, {| ExpectedETag: string; TransactionId: SubjectTransactionId |}>)
            : Task<Result<InitializeSubjectSuccessResult, 'OpError>> =
        fun () -> backgroundTask {
            let maybeTxnData, dedupData =
                match payload with
                | Choice1Of2 dedupInfo ->
                    None, dedupInfo |> Option.map (fun info -> { DedupInfo = info; IsInsert = true; EvictedDedupInfoToDelete = None })
                | Choice2Of2 txnData ->
                    Some txnData, None

            let maybeCreatorSubscriptionsData =
                insertData.CreatorSubscribing
                |> Map.toSeq
                |> Seq.collect (fun (lifeEvent, subs) ->
                    subs |> Map.toSeq |> Seq.map (fun (name, refs) -> (name, lifeEvent), refs |> Set.toSeq |> Seq.exactlyOne))
                |> List.ofSeq
                |> fun x ->
                    x
                    |> List.map snd
                    |> List.distinct
                    |> List.tryExactlyOne
                    |> Option.map (fun creatorRef ->
                        {| CreatorRef = creatorRef; CreatorSubscriptions = x |> List.map fst |> Map.ofList |})

            use connection = new SqlConnection(sqlConnectionString)
            do! connection.OpenAsync()

            use command = new SqlCommand((sprintf "[%s].%s_InitializeState_V3" ecosystemName lifeCycleName), connection)

            command.CommandType <- CommandType.StoredProcedure
            command.Parameters.Add("@id", SqlDbType.NVarChar).Value <- pKey

            let concurrencyToken = SqlParameter("@concurrencyToken", SqlDbType.Binary, 8)
            concurrencyToken.Direction <- ParameterDirection.InputOutput
            command.Parameters.Add concurrencyToken |> ignore
            concurrencyToken.Value <-
                maybeTxnData |> Option.map (fun d -> d.ExpectedETag |> hexStringToByteArray |> box)
                |> Option.defaultValue (DBNull.Value |> box)

            let version = SqlParameter("@version", SqlDbType.BigInt)
            version.Direction <- ParameterDirection.Output
            command.Parameters.Add version |> ignore

            command.Parameters.Add("@subjectTransactionId", SqlDbType.UniqueIdentifier).Value <-
                (maybeTxnData |> Option.map (fun d -> d.TransactionId) |> Option.map (fun (SubjectTransactionId transactionId) -> transactionId |> box)
                 |> Option.defaultValue (DBNull.Value |> box))

            let writeData = insertData.DataToInsert
            command.Parameters.Add("@subject", SqlDbType.VarBinary).Value <-
                writeData.UpdatedSubjectState.Subject :> Subject<'SubjectId>
                |> toCompressedJsonControlledForSize logger.LogWarning lifeCycleName pKey "subject"
                |> box

            command.Parameters.Add("@subjectLastUpdatedOn", SqlDbType.DateTimeOffset).Value <-
                writeData.UpdatedSubjectState.LastUpdatedOn |> box

            let tickData =
                tickStateToNextTickOnAndNextTickFields writeData.UpdatedSubjectState.TickState

            command.Parameters.Add("@nextTickOn", SqlDbType.DateTimeOffset).Value <-
                tickData
                |> fun (on, _, _) -> on
                |> Option.map box
                |> Option.defaultValue (DBNull.Value |> box)

            command.Parameters.Add("@nextTickFired", SqlDbType.Bit).Value <-
                tickData
                |> fun (_, fired, _) -> box fired

            command.Parameters.Add("@nextTickContext", SqlDbType.VarBinary).Value <-
                tickData
                |> fun (_, _, maybeContext) -> maybeContext
                |> Option.map (fun context -> context |> toCompressedJson |> box)
                |> Option.map box
                |> Option.defaultValue (DBNull.Value |> box)

            command.Parameters.Add("@grainIdHash", SqlDbType.Int).Value <-
                insertData.GrainIdHash |> uint32ToInt32MaintainOrder |> box

            command.Parameters.Add("@ourSubscriptions", SqlDbType.VarBinary).Value <-
                writeData.UpdatedSubjectState.OurSubscriptions
                |> toCompressedJsonControlledForSize logger.LogWarning lifeCycleName pKey "ourSubscriptions"
                |> box

            command.Parameters.Add("@creatorId", SqlDbType.NVarChar).Value <-
                (maybeCreatorSubscriptionsData
                 |> Option.map (
                     fun creatorSubscriptionsData ->
                         creatorSubscriptionsData.CreatorRef.SubjectIdStr |> box)
                 |> Option.defaultValue (DBNull.Value |> box))

            command.Parameters.Add("@creatorLifeCycleName", SqlDbType.NVarChar).Value <-
                (maybeCreatorSubscriptionsData
                 |> Option.map (
                     fun creatorSubscriptionsData ->
                         creatorSubscriptionsData.CreatorRef.LifeCycleKey |> serializeLifeCycleKey |> box)
                 |> Option.defaultValue (DBNull.Value |> box))

            command.Parameters.Add("@creatorSubscriptions", SqlDbType.Structured)
            |> fun param ->
                param.TypeName <- sprintf "[%s].SubscriptionNameAndLifeEventList" ecosystemName
                param.Value <-
                    match maybeCreatorSubscriptionsData with
                    | Some creatorSubscriptionsData ->
                        getSubscriptionNameAndLifeEventListDataTable pKey creatorSubscriptionsData.CreatorSubscriptions
                    | None ->
                        getEmptySubscriptionNameAndLifeEventListDataTable()

            command.Parameters.Add("@operation", SqlDbType.VarBinary).Value <-
                let ctor = insertData.ConstructorThatCausedInsert
                let ctor =
                    match (box ctor) with
                    | :? IRedactable as ctor -> ctor.Redact() |> unbox
                    | _ -> ctor
                (ctor :> Constructor)
                |> toCompressedJsonControlledForSize logger.LogWarning lifeCycleName pKey "operation"
                |> box

            command.Parameters.Add("@lastOperationBy", SqlDbType.NVarChar).Value <-
                writeData.TruncatedOperationBy 80uy |> box

            let newSideEffects =
                writeData.SideEffectGroup |> Option.map NonemptyKeyedSet.ofOneItem

            let sideEffectDataTable =
                getMaybeIncrementSideEffectsDataTable pKey newSideEffects

            command.Parameters.Add("@newSideEffects", SqlDbType.Structured)
            |> fun param ->
                param.TypeName <- sprintf "[%s].SideEffectList_V2" ecosystemName
                param.Value <- sideEffectDataTable

            command.Parameters.Add("@nextSideEffectSeqNumber", SqlDbType.BigInt).Value <-
                writeData.NextSideEffectSeq |>uint64ToInt64MaintainOrder |> box

            let param = command.Parameters.Add("@indices", SqlDbType.Structured)
            param.TypeName <- sprintf "[%s].IndexingList_V3" ecosystemName
            param.Value <- getIndexingListDataTable writeData.IndexActions

            let param = command.Parameters.Add("@promotedIndices", SqlDbType.Structured)
            param.TypeName <- sprintf "[%s].PromotedIndexingList" ecosystemName
            param.Value <- getPromotedIndexingListDataTable writeData.IndexActions

            let param = command.Parameters.Add("@callDedupActions", SqlDbType.Structured)
            param.TypeName <- sprintf "[%s].CallDedupList_V2" ecosystemName
            param.Value <- getCallDedupListDataTable dedupData

            // TODO: make blob saving a separate customizable concern (still must be transactional where possible)
            let param = command.Parameters.Add("@blobActions", SqlDbType.Structured)
            param.TypeName <- sprintf "[%s].BlobActionList_V2" ecosystemName
            let! blobActionTable, transferBlobIdsByHandlers = getBlobActionListDataTable writeData.BlobActions
            param.Value <- blobActionTable

            let offSyncConcurrencyToken = SqlParameter("@offSyncConcurrencyToken", SqlDbType.Binary, 8)
            offSyncConcurrencyToken.Direction <- ParameterDirection.Output
            command.Parameters.Add offSyncConcurrencyToken |> ignore

            let violatedUniqueIndexKey = SqlParameter("@violatedUniqueIndexKey", SqlDbType.NVarChar, 80)
            violatedUniqueIndexKey.Direction <- ParameterDirection.Output
            command.Parameters.Add violatedUniqueIndexKey |> ignore

            do! command.ExecuteNonQueryAsync() |> Task.Ignore

            if violatedUniqueIndexKey.Value <> null && violatedUniqueIndexKey.Value <> (box DBNull.Value) then
                let violatedIndexKey = violatedUniqueIndexKey.Value :?> string
                let error = getIndexViolationError violatedIndexKey writeData.IndexActions
                return Error error

            else if offSyncConcurrencyToken.Value <> null && offSyncConcurrencyToken.Value <> (box DBNull.Value) then
                let updatedETag = offSyncConcurrencyToken.Value :?> byte[] |> byteArrayToHexString
                return raise (Orleans.Storage.InconsistentStateException("ETag doesn't match", updatedETag, maybeTxnData |> Option.map (fun d -> d.ExpectedETag) |> Option.defaultValue "<MISSING>"))

            else if concurrencyToken.Value <> null && concurrencyToken.Value <> (box DBNull.Value) then
                let res =
                    { ETag = concurrencyToken.Value :?> byte[] |> byteArrayToHexString
                      Version = version.Value :?> int64 |> uint64
                      SkipHistoryOnNextOp =
                          match historyRetention with
                          | PersistentHistoryRetention.Unfiltered _ -> false
                          | PersistentHistoryRetention.NoHistory _ -> true
                          | PersistentHistoryRetention.FilteredByTelemetryRules _ ->
                              match lifeCycleAdapter.LifeCycle.ShouldSendTelemetry with
                              | None -> false
                              | Some shouldSendTelemetry ->
                                  shouldSendTelemetry (ShouldSendTelemetryFor.Constructor insertData.ConstructorThatCausedInsert)
                                  |> not
                          | PersistentHistoryRetention.FilteredByHistoryRules _ ->
                              match lifeCycleAdapter.LifeCycle.ShouldRecordHistory with
                              | None -> false
                              | Some shouldRecordHistory ->
                                  shouldRecordHistory (ShouldRecordHistoryFor.Constructor insertData.ConstructorThatCausedInsert)
                                  |> not }

                // Clear transfer blobs
                do! transferBlobIdsByHandlers
                    |> Seq.map (fun (handler, ids) -> handler.DeleteTransferBlobs ids)
                    |> Task.WhenAll
                    |> Task.Ignore

                return Ok res

            else
                match maybeTxnData with
                | Some txnData ->
                    let msg = sprintf "State with ID %s not found during commitInitializeSubject" pKey
                    return raise (Orleans.Storage.InconsistentStateException(msg, "<MISSING>", txnData.ExpectedETag))
                | None ->
                    return failwith "initializeSubject neither prepared state nor detected concurrently prepared state, this is bug"
        }
        |> SqlServerTransientErrorDetection.wrapTransientExceptions

    let initializeSubject
        (pKey: string)
        (dedupInfo: Option<SideEffectDedupInfo>)
        (data: SubjectInsertData<'Subject, 'Constructor, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>) =
        initializeOrCommitPreparedInitializeSubject pKey data (Choice1Of2 dedupInfo)

    let commitInitializeSubject
        (pKey: string)
        (eTag: string)
        (subjectTransactionId: SubjectTransactionId)
        (insertData: SubjectInsertData<'Subject, 'Constructor, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>)
        : Task<InitializeSubjectSuccessResult> =
        backgroundTask {
            match! initializeOrCommitPreparedInitializeSubject pKey insertData (Choice2Of2 {| ExpectedETag = eTag; TransactionId = subjectTransactionId |}) with
            | Ok res ->
                return { ETag = res.ETag; Version = res.Version; SkipHistoryOnNextOp = res.SkipHistoryOnNextOp }
            | Error err ->
                return failwithf "domain error is not expected when committing prepared initialize: %A, %A" pKey err
        }

    let updateOrCommitPreparedUpdateSubject
        (pKey: string)
        (updateData: SubjectUpdateData<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>)
        (payload: Choice<Option<UpsertDedupData>, SubjectTransactionId>)
        : Task<Result<UpdateSubjectSuccessResult, 'OpError>> =
        fun () -> backgroundTask {
            let (maybeSubjectTransactionId,
                 dedupData) =
                match payload with
                | Choice1Of2 dedupData ->
                    None,
                    dedupData

                | Choice2Of2 transactionId ->
                    Some transactionId,
                    None

            use connection = new SqlConnection(sqlConnectionString)
            do! connection.OpenAsync()
            use command = new SqlCommand((sprintf "[%s].%s_UpdateState_V4" ecosystemName lifeCycleName), connection)

            command.CommandType <- CommandType.StoredProcedure
            command.Parameters.Add("@id", SqlDbType.NVarChar).Value <- pKey

            let concurrencyToken = SqlParameter("@concurrencyToken", SqlDbType.Binary, 8)
            concurrencyToken.Direction <- ParameterDirection.InputOutput
            command.Parameters.Add concurrencyToken |> ignore
            concurrencyToken.Value <- hexStringToByteArray updateData.ExpectedETag

            let version = SqlParameter("@version", SqlDbType.BigInt)
            version.Direction <- ParameterDirection.Output
            command.Parameters.Add version |> ignore

            command.Parameters.Add("@subjectTransactionId", SqlDbType.UniqueIdentifier).Value <-
                (maybeSubjectTransactionId |> Option.map (fun (SubjectTransactionId transactionId) -> transactionId |> box)
                 |> Option.defaultValue (DBNull.Value |> box))

            command.Parameters.Add("@subject", SqlDbType.VarBinary).Value <-
                updateData.DataToUpdate.UpdatedSubjectState.Subject :> Subject<'SubjectId>
                |> toCompressedJsonControlledForSize logger.LogWarning lifeCycleName pKey "subject"
                |> box

            command.Parameters.Add("@subjectLastUpdatedOn", SqlDbType.DateTimeOffset).Value <-
                updateData.DataToUpdate.UpdatedSubjectState.LastUpdatedOn |> box

            let tickData = tickStateToNextTickOnAndNextTickFields updateData.DataToUpdate.UpdatedSubjectState.TickState

            command.Parameters.Add("@nextTickOn", SqlDbType.DateTimeOffset).Value <-
                tickData
                |> fun (on, _, _) -> on
                |> Option.map box
                |> Option.defaultValue (DBNull.Value |> box)

            command.Parameters.Add("@nextTickFired", SqlDbType.Bit).Value <-
                tickData
                |> fun (_, fired, _) -> box fired

            command.Parameters.Add("@nextTickContext", SqlDbType.VarBinary).Value <-
                tickData
                |> fun (_, _, maybeContext) -> maybeContext
                |> Option.map (fun context -> context |> toCompressedJson |> box)
                |> Option.map box
                |> Option.defaultValue (DBNull.Value |> box)

            command.Parameters.Add("@ourSubscriptions", SqlDbType.VarBinary).Value <-
                updateData.DataToUpdate.UpdatedSubjectState.OurSubscriptions
                |> toCompressedJsonControlledForSize logger.LogWarning lifeCycleName pKey "ourSubscriptions"
                |> box

            command.Parameters.Add("@skipHistory", SqlDbType.Bit).Value <- updateData.SkipHistory

            command.Parameters.Add("@operation", SqlDbType.VarBinary).Value <-
                 let action = updateData.ActionThatCausedUpdate
                 let action =
                     match (box action) with
                     | :? IRedactable as a -> a.Redact() |> unbox
                     | _ -> action
                 action :> LifeAction
                 |> toCompressedJsonControlledForSize logger.LogWarning lifeCycleName pKey "operation"
                 |> box

            command.Parameters.Add("@lastOperationBy", SqlDbType.NVarChar).Value <-
                updateData.DataToUpdate.TruncatedOperationBy 80uy |> box

            let param = command.Parameters.Add("@indices", SqlDbType.Structured)
            param.TypeName <- sprintf "[%s].IndexingList_V3" ecosystemName
            param.Value <- getIndexingListDataTable updateData.DataToUpdate.IndexActions

            let param = command.Parameters.Add("@promotedIndices", SqlDbType.Structured)
            param.TypeName <- sprintf "[%s].PromotedIndexingList" ecosystemName
            param.Value <- getPromotedIndexingListDataTable updateData.DataToUpdate.IndexActions

            let param = command.Parameters.Add("@callDedupActions", SqlDbType.Structured)
            param.TypeName <- sprintf "[%s].CallDedupList_V2" ecosystemName
            param.Value <- getCallDedupListDataTable dedupData

            // TODO: make blob saving a separate customizable concern (still must be transactional where possible)
            let param = command.Parameters.Add("@blobActions", SqlDbType.Structured)
            param.TypeName <- sprintf "[%s].BlobActionList_V2" ecosystemName
            let! blobActionTable, transferBlobIdsByHandlers = getBlobActionListDataTable updateData.DataToUpdate.BlobActions
            param.Value <- blobActionTable

            let subscriberLifeCycleNameValue, subscriberIdValue, newSubscriptions =
                match updateData.SubscriptionsToAdd with
                | None ->
                    box DBNull.Value, box DBNull.Value, Map.empty
                | Some s ->
                    serializeLifeCycleKey s.Subscriber.LifeCycleKey |> box,
                    s.Subscriber.SubjectIdStr |> box,
                    s.NewSubscriptions
            command.Parameters.Add("@subscriberLifeCycleName", SqlDbType.NVarChar).Value <- subscriberLifeCycleNameValue
            command.Parameters.Add("@subscriberId", SqlDbType.NVarChar).Value <- subscriberIdValue
            command.Parameters.Add("@subscriptionsToAdd", SqlDbType.Structured)
            |> fun param ->
                param.Value    <- getSubscriptionNameAndLifeEventListDataTable pKey newSubscriptions
                param.TypeName <- sprintf "[%s].SubscriptionNameAndLifeEventList" ecosystemName

            let newSideEffects =
                updateData.DataToUpdate.SideEffectGroup |> Option.map NonemptyKeyedSet.ofOneItem

            let sideEffectDataTable =
                getMaybeIncrementSideEffectsDataTable pKey newSideEffects

            command.Parameters.Add("@newSideEffects", SqlDbType.Structured)
            |> fun param ->
                param.TypeName <- sprintf "[%s].SideEffectList_V2" ecosystemName
                param.Value <- sideEffectDataTable

            command.Parameters.Add("@nextSideEffectSeqNumber", SqlDbType.BigInt).Value <-
                updateData.DataToUpdate.NextSideEffectSeq |> uint64ToInt64MaintainOrder |> box

            let offSyncConcurrencyToken = SqlParameter("@offSyncConcurrencyToken", SqlDbType.Binary, 8)
            offSyncConcurrencyToken.Direction <- ParameterDirection.Output
            command.Parameters.Add offSyncConcurrencyToken |> ignore

            let violatedUniqueIndexKey = SqlParameter("@violatedUniqueIndexKey", SqlDbType.NVarChar, 80)
            violatedUniqueIndexKey.Direction <- ParameterDirection.Output
            command.Parameters.Add violatedUniqueIndexKey |> ignore

            do! command.ExecuteNonQueryAsync() |> Task.Ignore

            if violatedUniqueIndexKey.Value <> null && violatedUniqueIndexKey.Value <> (box DBNull.Value) then
                let violatedIndexKey = violatedUniqueIndexKey.Value :?> string
                let error = getIndexViolationError violatedIndexKey updateData.DataToUpdate.IndexActions
                return Error error

            else if concurrencyToken.Value <> null && concurrencyToken.Value <> (box DBNull.Value) then
                // Clear transfer blobs
                do! transferBlobIdsByHandlers
                    |> Seq.map (fun (handler, ids) -> handler.DeleteTransferBlobs ids)
                    |> Task.WhenAll
                    |> Task.Ignore

                return
                    { NewETag = concurrencyToken.Value :?> byte[] |> byteArrayToHexString
                      NewVersion = version.Value :?> int64 |> uint64
                      SkipHistoryOnNextOp =
                          match historyRetention with
                          | PersistentHistoryRetention.Unfiltered _ -> false
                          | PersistentHistoryRetention.NoHistory _ -> true
                          | PersistentHistoryRetention.FilteredByTelemetryRules _ ->
                              (lifeCycleAdapter :> IHostedLifeCycleAdapter<_, _, _, _, _, _>).ShouldSendTelemetry
                                (ShouldSendTelemetryFor.LifeAction updateData.ActionThatCausedUpdate)
                              |> not
                          | PersistentHistoryRetention.FilteredByHistoryRules _ ->
                              (lifeCycleAdapter :> IHostedLifeCycleAdapter<_, _, _, _, _, _>).ShouldRecordHistory
                                (ShouldRecordHistoryFor.LifeAction updateData.ActionThatCausedUpdate)
                              |> not }
                    |> Ok

            else if offSyncConcurrencyToken.Value <> null && offSyncConcurrencyToken.Value <> (box DBNull.Value) then
                let updatedETag = offSyncConcurrencyToken.Value :?> byte[] |> byteArrayToHexString
                return raise (Orleans.Storage.InconsistentStateException("ETag doesn't match", updatedETag, updateData.ExpectedETag))

            else
                let msg = sprintf "State with ID %s not found during updateOrCommitPreparedUpdateSubject" pKey
                return raise (Orleans.Storage.InconsistentStateException(msg, "<MISSING>", updateData.ExpectedETag))
        }
        |> SqlServerTransientErrorDetection.wrapTransientExceptions

    let updateSubject
            (pKey: string)
            (dedupData: Option<UpsertDedupData>)
            (updateData: SubjectUpdateData<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>)
            : Task<Result<UpdateSubjectSuccessResult, 'OpError>> =
        updateOrCommitPreparedUpdateSubject pKey updateData (Choice1Of2 dedupData)

    let commitUpdateSubject
            (pKey: string)
            (subjectTransactionId: SubjectTransactionId)
            (updateData: SubjectUpdateData<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>)
            : Task<UpdateSubjectSuccessResult> =
        backgroundTask {
            match! updateOrCommitPreparedUpdateSubject pKey updateData (Choice2Of2 subjectTransactionId) with
            | Ok res ->
                return
                    { NewETag = res.NewETag
                      NewVersion = res.NewVersion
                      SkipHistoryOnNextOp = res.SkipHistoryOnNextOp }
            | Error err ->
                return failwithf "domain error is not expected when committing prepared update: %A, %A" pKey err
        }

    let prepareUpdateSubject
            (pKey: string)
            (expectedETag: string)
            (preparedUpdateData: PreparedSubjectUpdateData<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>)
            (uniqueIndicesToReserve: list<UniqueIndexToReserveOnPrepare<'OpError>>)
            (subjectTransactionId: SubjectTransactionId)
            : Task<Result<(* ETag *) string, 'OpError>> =
        fun () -> backgroundTask {
            use connection = new SqlConnection(sqlConnectionString)
            do! connection.OpenAsync()
            use command = new SqlCommand((sprintf "[%s].%s_PrepareUpdate_V2" ecosystemName lifeCycleName), connection)

            command.CommandType <- CommandType.StoredProcedure
            command.Parameters.Add("@id", SqlDbType.NVarChar).Value <- pKey

            let concurrencyToken = SqlParameter("@concurrencyToken", SqlDbType.Binary, 8)
            concurrencyToken.Direction <- ParameterDirection.InputOutput
            command.Parameters.Add concurrencyToken |> ignore
            concurrencyToken.Value <- hexStringToByteArray expectedETag

            command.Parameters.Add("@preparedTransactionalState", SqlDbType.VarBinary).Value <-
                preparedUpdateData
                |> PreparedSubjectUpdateData<Subject<'SubjectId>, 'SubjectId, LifeAction, LifeEvent, OpError>.CastUnsafe
                |> toCompressedJsonControlledForSize logger.LogWarning lifeCycleName pKey "preparedTransactionalState"
                |> box

            command.Parameters.Add("@subjectTransactionId", SqlDbType.UniqueIdentifier).Value <-
                let (SubjectTransactionId transactionId) = subjectTransactionId
                transactionId |> box

            let param = command.Parameters.Add("@uniqueIndicesToReserve", SqlDbType.Structured)
            param.TypeName <- sprintf "[%s].IndexingList_V3" ecosystemName
            param.Value <- getReserveUniqueIndicesDataTable uniqueIndicesToReserve

            let offSyncConcurrencyToken = SqlParameter("@offSyncConcurrencyToken", SqlDbType.Binary, 8)
            offSyncConcurrencyToken.Direction <- ParameterDirection.Output
            command.Parameters.Add offSyncConcurrencyToken |> ignore

            let violatedUniqueIndexKey = SqlParameter("@violatedUniqueIndexKey", SqlDbType.NVarChar, 80)
            violatedUniqueIndexKey.Direction <- ParameterDirection.Output
            command.Parameters.Add violatedUniqueIndexKey |> ignore

            do! command.ExecuteNonQueryAsync() |> Task.Ignore

            if violatedUniqueIndexKey.Value <> null && violatedUniqueIndexKey.Value <> (box DBNull.Value) then
                let violatedIndexKey = violatedUniqueIndexKey.Value :?> string
                let error = getUniqueIndexReserveViolationError violatedIndexKey uniqueIndicesToReserve
                return Error error
            elif concurrencyToken.Value <> null && concurrencyToken.Value <> (box DBNull.Value) then
                let eTag = concurrencyToken.Value :?> byte[] |> byteArrayToHexString
                return Ok eTag
            else if offSyncConcurrencyToken.Value <> null && offSyncConcurrencyToken.Value <> (box DBNull.Value) then
                let updatedETag = offSyncConcurrencyToken.Value :?> byte[] |> byteArrayToHexString
                return raise (Orleans.Storage.InconsistentStateException("ETag doesn't match", updatedETag, expectedETag))
            else
                let msg = sprintf "State with ID %s not found during prepareUpdateSubject" pKey
                return raise (Orleans.Storage.InconsistentStateException(msg, "<MISSING>", expectedETag))
        }
        |> SqlServerTransientErrorDetection.wrapTransientExceptions

    let rollbackUpdateSubject
            (pKey: string)
            (expectedETag: string)
            (subjectTransactionId: SubjectTransactionId)
            (uniqueIndicesToRelease: list<UniqueIndexToReleaseOnRollback>)
            : Task<(* ETag *) string> =
        fun () -> backgroundTask {
            use connection = new SqlConnection(sqlConnectionString)
            do! connection.OpenAsync()
            use command = new SqlCommand((sprintf "[%s].%s_RollbackPreparedUpdate_V2" ecosystemName lifeCycleName), connection)

            command.CommandType <- CommandType.StoredProcedure
            command.Parameters.Add("@id", SqlDbType.NVarChar).Value <- pKey

            let concurrencyToken = SqlParameter("@concurrencyToken", SqlDbType.Binary, 8)
            concurrencyToken.Direction <- ParameterDirection.InputOutput
            command.Parameters.Add concurrencyToken |> ignore
            concurrencyToken.Value <- hexStringToByteArray expectedETag

            command.Parameters.Add("@subjectTransactionId", SqlDbType.UniqueIdentifier).Value <-
                let (SubjectTransactionId transactionId) = subjectTransactionId
                box transactionId

            let param = command.Parameters.Add("@uniqueIndicesToRelease", SqlDbType.Structured)
            param.TypeName <- sprintf "[%s].IndexingList_V3" ecosystemName
            param.Value <- getReleaseUniqueIndicesDataTable uniqueIndicesToRelease

            let offSyncConcurrencyToken = SqlParameter("@offSyncConcurrencyToken", SqlDbType.Binary, 8)
            offSyncConcurrencyToken.Direction <- ParameterDirection.Output
            command.Parameters.Add offSyncConcurrencyToken |> ignore

            do! command.ExecuteNonQueryAsync() |> Task.Ignore

            if concurrencyToken.Value <> null && concurrencyToken.Value <> (box DBNull.Value) then
                let newETag = concurrencyToken.Value :?> byte[] |> byteArrayToHexString
                return newETag

            else if offSyncConcurrencyToken.Value <> null && offSyncConcurrencyToken.Value <> (box DBNull.Value) then
                let newETag = offSyncConcurrencyToken.Value :?> byte[] |> byteArrayToHexString
                return raise (Orleans.Storage.InconsistentStateException("ETag doesn't match", newETag, expectedETag))

            else
                let msg = sprintf "State with ID %s not found during rollbackUpdateSubject" pKey
                return raise (Orleans.Storage.InconsistentStateException(msg, "<MISSING>", expectedETag))
        }
        |> SqlServerTransientErrorDetection.wrapTransientExceptions

    let enqueueSideEffects
            (pKey: string)
            (now: DateTimeOffset)
            (dedupData: Option<UpsertDedupData>)
            (expectedETag: string)
            (nextSideEffectSeqNum: GrainSideEffectSequenceNumber)
            (sideEffectGroups: NonemptyKeyedSet<GrainSideEffectSequenceNumber, SideEffectGroup<'LifeAction, 'OpError>>)
            : Task<(* newETag *) string> =
        fun () -> backgroundTask {
            use connection = new SqlConnection(sqlConnectionString)
            do! connection.OpenAsync()
            use command = new SqlCommand((sprintf "[%s].%s_EnqueueAction_V2" ecosystemName lifeCycleName), connection)

            command.CommandType <- CommandType.StoredProcedure
            command.Parameters.Add("@id", SqlDbType.NVarChar).Value <- pKey
            command.Parameters.Add("@now", SqlDbType.DateTimeOffset).Value <- now |> box

            let concurrencyToken = SqlParameter("@concurrencyToken", SqlDbType.Binary, 8)
            concurrencyToken.Direction <- ParameterDirection.InputOutput
            command.Parameters.Add concurrencyToken |> ignore
            concurrencyToken.Value <- hexStringToByteArray expectedETag

            let param = command.Parameters.Add("@callDedupActions", SqlDbType.Structured)
            param.TypeName <- sprintf "[%s].CallDedupList_V2" ecosystemName
            param.Value <- getCallDedupListDataTable dedupData

            let sideEffectDataTable =
                getMaybeIncrementSideEffectsDataTable pKey (Some sideEffectGroups)

            command.Parameters.Add("@newSideEffects", SqlDbType.Structured)
            |> fun param ->
                param.TypeName <- sprintf "[%s].SideEffectList_V2" ecosystemName
                param.Value <- sideEffectDataTable

            command.Parameters.Add("@nextSideEffectSeqNumber", SqlDbType.BigInt).Value <-
                nextSideEffectSeqNum |> uint64ToInt64MaintainOrder |> box

            let offSyncConcurrencyToken = SqlParameter("@offSyncConcurrencyToken", SqlDbType.Binary, 8)
            offSyncConcurrencyToken.Direction <- ParameterDirection.Output
            command.Parameters.Add offSyncConcurrencyToken |> ignore

            do! command.ExecuteNonQueryAsync() |> Task.Ignore

            if concurrencyToken.Value <> null && concurrencyToken.Value <> (box DBNull.Value) then
                let newETag = concurrencyToken.Value :?> byte[] |> byteArrayToHexString
                return newETag

            else if offSyncConcurrencyToken.Value <> null && offSyncConcurrencyToken.Value <> (box DBNull.Value) then
                let updatedETag = offSyncConcurrencyToken.Value :?> byte[] |> byteArrayToHexString
                return raise (Orleans.Storage.InconsistentStateException("ETag doesn't match", updatedETag, expectedETag))

            else
                let msg = sprintf "State with ID %s not found during enqueueSideEffects" pKey
                return raise (Orleans.Storage.InconsistentStateException(msg, "<MISSING>", expectedETag))
        }
        |> SqlServerTransientErrorDetection.wrapTransientExceptions

    let setTickState
            (pKey: string)
            (expectedETag: string)
            (nextSideEffectSeqNum: GrainSideEffectSequenceNumber)
            (tickState: TickState)
            (sideEffectGroup: Option<SideEffectGroup<'LifeAction, 'OpError>>)
            : Task<(* newETag *) string> =
        fun () -> backgroundTask {
            use connection = new SqlConnection(sqlConnectionString)
            do! connection.OpenAsync()
            use command = new SqlCommand((sprintf "[%s].%s_SetTickState_V2" ecosystemName lifeCycleName), connection)

            command.CommandType <- CommandType.StoredProcedure
            command.Parameters.Add("@id", SqlDbType.NVarChar).Value <- pKey

            let concurrencyToken = SqlParameter("@concurrencyToken", SqlDbType.Binary, 8)
            concurrencyToken.Direction <- ParameterDirection.InputOutput
            command.Parameters.Add concurrencyToken |> ignore
            concurrencyToken.Value <- hexStringToByteArray expectedETag

            let tickData = tickStateToNextTickOnAndNextTickFields tickState

            command.Parameters.Add("@nextTickOn", SqlDbType.DateTimeOffset).Value <-
                tickData
                |> fun (on, _, _) -> on
                |> Option.map box
                |> Option.defaultValue (DBNull.Value |> box)

            command.Parameters.Add("@nextTickFired", SqlDbType.Bit).Value <-
                tickData
                |> fun (_, fired, _) -> box fired

            command.Parameters.Add("@nextTickContext", SqlDbType.VarBinary).Value <-
                tickData
                |> fun (_, _, maybeContext) -> maybeContext
                |> Option.map (fun context -> context |> toCompressedJson |> box)
                |> Option.map box
                |> Option.defaultValue (DBNull.Value |> box)

            let sideEffectDataTable =
                getMaybeIncrementSideEffectsDataTable pKey (sideEffectGroup |> Option.map NonemptyKeyedSet.ofOneItem)

            command.Parameters.Add("@newSideEffects", SqlDbType.Structured)
            |> fun param ->
                param.TypeName <- sprintf "[%s].SideEffectList_V2" ecosystemName
                param.Value <- sideEffectDataTable

            command.Parameters.Add("@nextSideEffectSeqNumber", SqlDbType.BigInt).Value <-
                nextSideEffectSeqNum |> uint64ToInt64MaintainOrder |> box

            let offSyncConcurrencyToken = SqlParameter("@offSyncConcurrencyToken", SqlDbType.Binary, 8)
            offSyncConcurrencyToken.Direction <- ParameterDirection.Output
            command.Parameters.Add offSyncConcurrencyToken |> ignore

            do! command.ExecuteNonQueryAsync() |> Task.Ignore

            if concurrencyToken.Value <> null && concurrencyToken.Value <> (box DBNull.Value) then
                let newETag = concurrencyToken.Value :?> byte[] |> byteArrayToHexString
                return newETag

            else if offSyncConcurrencyToken.Value <> null && offSyncConcurrencyToken.Value <> (box DBNull.Value) then
                let updatedETag = offSyncConcurrencyToken.Value :?> byte[] |> byteArrayToHexString
                return raise (Orleans.Storage.InconsistentStateException("ETag doesn't match", updatedETag, expectedETag))

            else
                let msg = sprintf "State with ID %s not found during setTickState" pKey
                return raise (Orleans.Storage.InconsistentStateException(msg, "<MISSING>", expectedETag))
        }
        |> SqlServerTransientErrorDetection.wrapTransientExceptions

    let setTickStateAndSubscriptions
            (pKey: string)
            (expectedETag: string)
            (nextSideEffectSeqNum: GrainSideEffectSequenceNumber)
            (tickState: TickState)
            (ourSubscriptions: Map<SubscriptionName, SubjectReference>)
            (sideEffectGroup: Option<SideEffectGroup<'LifeAction, 'OpError>>)
            : Task<(* newETag *) string> =
        fun () -> backgroundTask {
            use connection = new SqlConnection(sqlConnectionString)
            do! connection.OpenAsync()
            use command = new SqlCommand((sprintf "[%s].%s_SetTickStateAndSubscriptions_V2" ecosystemName lifeCycleName), connection)

            command.CommandType <- CommandType.StoredProcedure
            command.Parameters.Add("@id", SqlDbType.NVarChar).Value <- pKey

            let concurrencyToken = SqlParameter("@concurrencyToken", SqlDbType.Binary, 8)
            concurrencyToken.Direction <- ParameterDirection.InputOutput
            command.Parameters.Add concurrencyToken |> ignore
            concurrencyToken.Value <- hexStringToByteArray expectedETag

            let tickData = tickStateToNextTickOnAndNextTickFields tickState

            command.Parameters.Add("@nextTickOn", SqlDbType.DateTimeOffset).Value <-
                tickData
                |> fun (on, _, _) -> on
                |> Option.map box
                |> Option.defaultValue (DBNull.Value |> box)

            command.Parameters.Add("@nextTickFired", SqlDbType.Bit).Value <-
                tickData
                |> fun (_, fired, _) -> box fired

            command.Parameters.Add("@nextTickContext", SqlDbType.VarBinary).Value <-
                tickData
                |> fun (_, _, maybeContext) -> maybeContext
                |> Option.map (fun context -> context |> toCompressedJson |> box)
                |> Option.map box
                |> Option.defaultValue (DBNull.Value |> box)

            command.Parameters.Add("@ourSubscriptions", SqlDbType.VarBinary).Value <-
                (ourSubscriptions
                 |> toCompressedJsonControlledForSize logger.LogWarning lifeCycleName pKey "ourSubscriptions"
                 |> box)

            let sideEffectDataTable =
                getMaybeIncrementSideEffectsDataTable pKey (sideEffectGroup |> Option.map NonemptyKeyedSet.ofOneItem)

            command.Parameters.Add("@newSideEffects", SqlDbType.Structured)
            |> fun param ->
                param.TypeName <- sprintf "[%s].SideEffectList_V2" ecosystemName
                param.Value <- sideEffectDataTable

            command.Parameters.Add("@nextSideEffectSeqNumber", SqlDbType.BigInt).Value <-
                nextSideEffectSeqNum |> uint64ToInt64MaintainOrder |> box

            let offSyncConcurrencyToken = SqlParameter("@offSyncConcurrencyToken", SqlDbType.Binary, 8)
            offSyncConcurrencyToken.Direction <- ParameterDirection.Output
            command.Parameters.Add offSyncConcurrencyToken |> ignore

            do! command.ExecuteNonQueryAsync() |> Task.Ignore

            if concurrencyToken.Value <> null && concurrencyToken.Value <> (box DBNull.Value) then
                let newETag = concurrencyToken.Value :?> byte[] |> byteArrayToHexString
                return newETag

            else if offSyncConcurrencyToken.Value <> null && offSyncConcurrencyToken.Value <> (box DBNull.Value) then
                let updatedETag = offSyncConcurrencyToken.Value :?> byte[] |> byteArrayToHexString
                return raise (Orleans.Storage.InconsistentStateException("ETag doesn't match", updatedETag, expectedETag))

            else
                let msg = sprintf "State with ID %s not found during setTickStateAndSubscriptions" pKey
                return raise (Orleans.Storage.InconsistentStateException(msg, "<MISSING>", expectedETag))
        }
        |> SqlServerTransientErrorDetection.wrapTransientExceptions

    let retrySideEffects
        (pKey: string)
        (expectedETag: string)
        (nextSideEffectSeqNum: GrainSideEffectSequenceNumber)
        (retryingSideEffectGroups: NonemptyKeyedSet<GrainSideEffectSequenceNumber, SideEffectGroup<'LifeAction, 'OpError>>)
        : Task<(* newETag *) string> =
        fun () -> backgroundTask {
            let failedSideEffectIdsToDelete =
                retryingSideEffectGroups.Values |> Seq.map (fun g -> g.SideEffects.Keys.ToSet) |> Set.unionMany

            use connection = new SqlConnection(sqlConnectionString)
            do! connection.OpenAsync()
            use command = new SqlCommand((sprintf "[%s].%s_RetryPermanentFailures_V2" ecosystemName lifeCycleName), connection)

            command.CommandType <- CommandType.StoredProcedure
            command.Parameters.Add("@id", SqlDbType.NVarChar).Value <- pKey

            let concurrencyToken = SqlParameter("@concurrencyToken", SqlDbType.Binary, 8)
            concurrencyToken.Direction <- ParameterDirection.InputOutput
            command.Parameters.Add concurrencyToken |> ignore
            concurrencyToken.Value <- hexStringToByteArray expectedETag

            let param = command.Parameters.Add("@failedSideEffectsToDelete", SqlDbType.Structured)
            param.TypeName <- sprintf "[%s].GuidList" ecosystemName
            param.Value <- getSideEffectIdsListDataTable failedSideEffectIdsToDelete

            let sideEffectDataTable =
                getMaybeIncrementSideEffectsDataTable pKey (Some retryingSideEffectGroups)

            command.Parameters.Add("@newSideEffects", SqlDbType.Structured)
            |> fun param ->
                param.TypeName <- sprintf "[%s].SideEffectList_V2" ecosystemName
                param.Value <- sideEffectDataTable

            command.Parameters.Add("@nextSideEffectSeqNumber", SqlDbType.BigInt).Value <-
                nextSideEffectSeqNum |> uint64ToInt64MaintainOrder |> box

            let offSyncConcurrencyToken = SqlParameter("@offSyncConcurrencyToken", SqlDbType.Binary, 8)
            offSyncConcurrencyToken.Direction <- ParameterDirection.Output
            command.Parameters.Add offSyncConcurrencyToken |> ignore

            do! command.ExecuteNonQueryAsync() |> Task.Ignore

            if concurrencyToken.Value <> null && concurrencyToken.Value <> (box DBNull.Value) then
                let newETag = concurrencyToken.Value :?> byte[] |> byteArrayToHexString
                return newETag
            else if offSyncConcurrencyToken.Value <> null && offSyncConcurrencyToken.Value <> (box DBNull.Value) then
                let updatedETag = offSyncConcurrencyToken.Value :?> byte[] |> byteArrayToHexString
                return raise (Orleans.Storage.InconsistentStateException("ETag doesn't match", updatedETag, expectedETag))

            else
                let msg = sprintf "State with ID %s not found during retrySideEffects" pKey
                return raise (Orleans.Storage.InconsistentStateException(msg, "<MISSING>", expectedETag))
        }
        |> SqlServerTransientErrorDetection.wrapTransientExceptions

    let clearSubject
            (pKey: string)
            (expectedETag: ETag)
            (skipHistory: bool)
            (sideEffectId: Option<GrainSideEffectId>)
            : Task =
        fun () -> backgroundTask {
            use connection = new SqlConnection(sqlConnectionString)
            do! connection.OpenAsync()
            use command = new SqlCommand((sprintf "[%s].[%s_ClearState]" ecosystemName lifeCycleName), connection)
            command.CommandType <- CommandType.StoredProcedure
            command.Parameters.Add("@id", SqlDbType.NVarChar).Value <- pKey
            command.Parameters.Add("@concurrencyToken", SqlDbType.Binary, 8).Value <- hexStringToByteArray expectedETag
            command.Parameters.Add("@skipHistory", SqlDbType.Bit).Value <- skipHistory

            let isDeleted = SqlParameter("@isDeleted", SqlDbType.Bit)
            isDeleted.Direction <- ParameterDirection.Output
            command.Parameters.Add isDeleted |> ignore

            let offSyncConcurrencyToken = SqlParameter("@offSyncConcurrencyToken", SqlDbType.Binary, 8)
            offSyncConcurrencyToken.Direction <- ParameterDirection.Output
            command.Parameters.Add offSyncConcurrencyToken |> ignore

            command.Parameters.Add("@sideEffectId", SqlDbType.UniqueIdentifier).Value <-
                (sideEffectId |> Option.map box |> Option.defaultValue (DBNull.Value |> box))

            do! command.ExecuteNonQueryAsync() |> Task.Ignore

            if isDeleted.Value <> (box DBNull.Value) && (isDeleted.Value :?> bool) then
                return ()
            else if offSyncConcurrencyToken.Value <> null && offSyncConcurrencyToken.Value <> (box DBNull.Value) then
                let updatedETag = offSyncConcurrencyToken.Value :?> byte[] |> byteArrayToHexString
                return raise (Orleans.Storage.InconsistentStateException("ETag doesn't match", expectedETag, updatedETag))
            else
                let msg = sprintf "State with ID %s not found during clearSubject" pKey
                return raise (Orleans.Storage.InconsistentStateException(msg, "<MISSING>", expectedETag))
        }
        |> SqlServerTransientErrorDetection.wrapTransientExceptions
        |> Task.Ignore

    let saveSubscriptions
        (pKey: string)
        (expectedETag: string)
        (subscriber: SubjectPKeyReference)
        (subscriptionsToAdd: Map<SubscriptionName, 'LifeEvent>)
        (subscriptionsToRemove: Set<SubscriptionName>)
        : Task<(* newETag *) string> =
        fun () -> backgroundTask {
            use connection = new SqlConnection(sqlConnectionString)
            do! connection.OpenAsync()
            use command = new SqlCommand((sprintf "[%s].%s_SaveSubscriptions" ecosystemName lifeCycleName), connection)

            command.CommandType <- CommandType.StoredProcedure
            command.Parameters.Add("@id", SqlDbType.NVarChar).Value <- pKey

            command.Parameters.Add("@lifeCycleName", SqlDbType.NVarChar).Value <- serializeLifeCycleKey subscriber.LifeCycleKey
            command.Parameters.Add("@subscriberId", SqlDbType.NVarChar).Value <- subscriber.SubjectIdStr
            command.Parameters.Add("@subscriptionsToAdd", SqlDbType.Structured)
            |> fun param ->
                param.Value    <- getSubscriptionNameAndLifeEventListDataTable pKey subscriptionsToAdd
                param.TypeName <- sprintf "[%s].SubscriptionNameAndLifeEventList" ecosystemName

            command.Parameters.Add("@subscriptionsToRemove", SqlDbType.Structured)
            |> fun param ->
                let subscriptionNameTable = new DataTable()
                subscriptionNameTable.Columns.Add("SubscriptionName", typeof<string>).MaxLength <- 200
                subscriptionsToRemove
                |> Set.iter (subscriptionNameTable.Rows.Add >> ignore)
                param.Value    <- subscriptionNameTable
                param.TypeName <- sprintf "[%s].SubscriptionNameList" ecosystemName

            let concurrencyToken = SqlParameter("@concurrencyToken", SqlDbType.Binary, 8)
            concurrencyToken.Direction <- ParameterDirection.InputOutput
            command.Parameters.Add concurrencyToken |> ignore
            concurrencyToken.Value <- hexStringToByteArray expectedETag

            let offSyncConcurrencyToken = SqlParameter("@offSyncConcurrencyToken", SqlDbType.Binary, 8)
            offSyncConcurrencyToken.Direction <- ParameterDirection.Output
            command.Parameters.Add offSyncConcurrencyToken |> ignore

            do! command.ExecuteNonQueryAsync() |> Task.Ignore

            if concurrencyToken.Value <> null && concurrencyToken.Value <> (box DBNull.Value) then
                let newETag = concurrencyToken.Value :?> byte[] |> byteArrayToHexString
                return newETag

            else if offSyncConcurrencyToken.Value <> null && offSyncConcurrencyToken.Value <> (box DBNull.Value) then
                let updatedETag = offSyncConcurrencyToken.Value :?> byte[] |> byteArrayToHexString
                return raise (Orleans.Storage.InconsistentStateException("ETag doesn't match", updatedETag, expectedETag))

            else
                let msg = sprintf "State with ID %s not found during saveSubscriptions" pKey
                return raise (Orleans.Storage.InconsistentStateException(msg, "<MISSING>", expectedETag))
        }
        |> SqlServerTransientErrorDetection.wrapTransientExceptions

    let assertStateConsistency (pKey: string) (expectedETagIfCreated: Option<string>) : Task =
        let readConcurrencyTokenFromMainTable (connection: SqlConnection) =
            backgroundTask {
                let concurrencyTokenSql =
                    sprintf "SELECT ConcurrencyToken FROM [%s].[%s] (NOLOCK) WHERE Id = @id"
                        ecosystemName lifeCycleName

                use command = new SqlCommand(concurrencyTokenSql, connection)
                command.Parameters.Add("@id", SqlDbType.NVarChar).Value <- pKey
                use! reader = command.ExecuteReaderAsync()

                match! reader.ReadAsync() with
                | true ->
                    let concurrencyToken = Array.zeroCreate<byte> 8 // concurrencyToken stored BigEndian uint64 encoded into binary(8)
                    reader.GetBytes(0, (int64 0), concurrencyToken, 0, 8) |> ignore
                    let actualETag = byteArrayToHexString concurrencyToken
                    return Some actualETag

                | false ->
                    return None
            }

        let readConcurrencyTokenFromPreparedTable (connection: SqlConnection) =
            backgroundTask {
                let preparedInitializeConcurrencyTokenSql =
                    sprintf "SELECT PreparedInitializeConcurrencyToken FROM [%s].[%s_Prepared] (NOLOCK) WHERE Id = @id"
                        ecosystemName lifeCycleName

                use command = new SqlCommand(preparedInitializeConcurrencyTokenSql, connection)
                command.Parameters.Add("@id", SqlDbType.NVarChar).Value <- pKey
                use! reader = command.ExecuteReaderAsync()

                match! reader.ReadAsync() with
                | true ->
                    let preparedInitializeConcurrencyToken = Array.zeroCreate<byte> 8 // concurrencyToken stored BigEndian uint64 encoded into binary(8)
                    reader.GetBytes(0, (int64 0), preparedInitializeConcurrencyToken, 0, 8) |> ignore
                    let actualETag = byteArrayToHexString preparedInitializeConcurrencyToken
                    return Some actualETag
                | false ->
                    return None
            }

        fun () -> backgroundTask {
            let! actualETagIfCreated =
                backgroundTask {
                    use connection = new SqlConnection(sqlConnectionString)
                    do! connection.OpenAsync()
                    // Note: do not inline Sql command functions to avoid multiple open Sql readers (requires connection with MARS enabled)
                    match! readConcurrencyTokenFromMainTable connection with
                    | Some actualETag ->
                        return Some actualETag
                    | None ->
                        // be lazy, look up prepared concurrencyToken only if main state doesn't exist
                        return! readConcurrencyTokenFromPreparedTable connection
                }

            if expectedETagIfCreated <> actualETagIfCreated then
                let actualETag = match actualETagIfCreated with | Some actual -> actual | None -> "<MISSING>"
                let expectedETag = match expectedETagIfCreated with | Some expected -> expected | None -> "<MISSING>"
                return raise (Orleans.Storage.InconsistentStateException("ETag doesn't match", actualETag, expectedETag))
        }
        |> SqlServerTransientErrorDetection.wrapTransientExceptions
        |> Task.Ignore

    let clearExpiredSubjectsHistory
            (now: DateTimeOffset)
            (batchSize: uint16)
            : Task<uint16> =

        let maybeExpireAfterAndMode =
            match historyRetention with
            | PersistentHistoryRetention.Unfiltered expiration
            | PersistentHistoryRetention.FilteredByTelemetryRules expiration
            | PersistentHistoryRetention.FilteredByHistoryRules expiration -> expiration
            | PersistentHistoryRetention.NoHistory _ -> None
            |> Option.map (function
                | PersistentHistoryExpiration.AfterSubjectDeletion keepHistoryFor -> (now - keepHistoryFor, 1)
                | PersistentHistoryExpiration.AfterSubjectChange keepHistoryFor -> (now - keepHistoryFor, 2))

        match maybeExpireAfterAndMode with
        | None ->
            Task.FromResult 0us
        | Some (expireAfter, mode) ->
            fun () -> backgroundTask {
                use connection = new SqlConnection(sqlConnectionString)
                do! connection.OpenAsync()
                use command = new SqlCommand((sprintf "[%s].%s_ClearExpiredHistoryBatch" ecosystemName lifeCycleName), connection)

                command.CommandType <- CommandType.StoredProcedure
                command.Parameters.Add("@mode", SqlDbType.Int).Value <- mode
                command.Parameters.Add("@expireAfter", SqlDbType.DateTimeOffset).Value <- expireAfter |> box
                command.Parameters.Add("@batchSize", SqlDbType.Int).Value <- batchSize

                let! retVal = command.ExecuteScalarAsync()
                let rowsDeleted = retVal |> System.Convert.ToUInt16
                return rowsDeleted
            }
            |> SqlServerTransientErrorDetection.wrapTransientExceptions

    let updateSideEffectStatus (pKey: string) (sideEffectId: GrainSideEffectId) (sideEffectResult: GrainSideEffectResult) : Task =
        fun () -> backgroundTask {
            use connection = new SqlConnection(sqlConnectionString)
            do! connection.OpenAsync()
            use command = connection.CreateCommand()

            let (isSuccess, maybeReasonAndSeverity) =
                match sideEffectResult with
                | GrainSideEffectResult.Success ->
                    (true, None)
                | GrainSideEffectResult.PermanentFailure (err, severity) ->
                    let mappedSeverity =
                        match severity with
                        | SideEffectFailureSeverity.Warning -> 1
                        | SideEffectFailureSeverity.Error -> 2
                    (false, Some (err, mappedSeverity))

            if isSuccess
            then
                command.CommandText <-
                    (sprintf "DELETE FROM [%s].[%s_SideEffect] WHERE SubjectId = @subjectId AND SideEffectId = @sideEffectId"
                        ecosystemName lifeCycleName)
                command.Parameters.Add("@subjectId", SqlDbType.NVarChar).Value <- pKey
                command.Parameters.Add("@sideEffectId", SqlDbType.UniqueIdentifier).Value <- sideEffectId
            else
                command.CommandText <-
                    (sprintf "UPDATE [%s].[%s_SideEffect] SET FailureReason = @reason, FailureSeverity = @severity WHERE SubjectId = @subjectId AND SideEffectId = @sideEffectId"
                        ecosystemName lifeCycleName)
                command.Parameters.Add("@reason", SqlDbType.NVarChar).Value <-
                    (maybeReasonAndSeverity |> Option.map (fst >> box) |> Option.defaultValue (DBNull.Value |> box))
                command.Parameters.Add("@severity", SqlDbType.TinyInt).Value <-
                    (maybeReasonAndSeverity |> Option.map (snd >> box) |> Option.defaultValue (DBNull.Value |> box))
                command.Parameters.Add("@subjectId", SqlDbType.NVarChar).Value <- pKey
                command.Parameters.Add("@sideEffectId", SqlDbType.UniqueIdentifier).Value <- sideEffectId

            let! numRowsAffected = command.ExecuteNonQueryAsync()
            if numRowsAffected = 1 then
                logger.LogTrace("SideEffect for subject:/{type}/{pKey} with ID {sideEffectId} updated to {sideEffectResult}",
                    typeof<'Subject>.Name, pKey, sideEffectId, sideEffectResult)
            elif numRowsAffected = 0 then
                logger.LogTrace("SideEffect for subject:/{type}/{pKey} with ID {sideEffectId} not found and can't be updated to {sideEffectResult}",
                    typeof<'Subject>.Name, pKey, sideEffectId, sideEffectResult)
            else
                logger.LogError("updateSideEffectStatus affected {numRowsAffected} rows upon update, expecting 1 or 0 (e.g. a retry), for subject:/{type}/{pKey} with ID {sideEffectId} updated to {sideEffectResult}",
                    numRowsAffected, typeof<'Subject>.Name, pKey, sideEffectId, sideEffectResult)

        }
        |> SqlServerTransientErrorDetection.wrapTransientExceptions
        |> Task.Ignore

    let rebuildIndices (lifeCycleName: string) (rebuildType: IndexRebuildType) (maybeLastSubjectIdRebuilt: Option<string>) (batchSize: uint16) : Task<RebuildIndicesBatchResult> =
        let emptyRebuildIndexList () =
            let dataTable = new DataTable()
            dataTable.Columns.Add("PromotedKey",       typeof<string>).MaxLength <- 240
            dataTable.Columns.Add("PromotedValue",     typeof<string>).MaxLength <- 100
            dataTable.Columns.Add("Id",                typeof<string>).MaxLength <- 80
            dataTable.Columns.Add("ConcurrencyToken",  typeof<byte[]>) |> ignore
            dataTable.Columns.Add("Key",               typeof<string>).MaxLength <- 80
            dataTable.Columns.Add("ValueStr",          typeof<string>).MaxLength <- -1
            dataTable.Columns.Add("ValueInt",          typeof<int64>) |> ignore
            dataTable.Columns.Add("Kind",              typeof<int>)   |> ignore
            dataTable.Columns.Add("IsDelete",          typeof<bool>)  |> ignore
            dataTable

        fun () -> backgroundTask {
            let allIndexKeys = lifeCycleAdapter.LifeCycle.MetaData.IndexKeys

            let (indexKeysToUpdate, indexKeysToRemove, removeAllNonMatchingKeys) =
                match rebuildType with
                | IndexRebuildType.All ->
                    (allIndexKeys, Set.empty, true)

                | IndexRebuildType.Selected selectedIndexKeysNonEmpty ->
                    let selectedIndexKeys = selectedIndexKeysNonEmpty.ToSet
                    let indexKeysToUpdate =
                        Set.intersect allIndexKeys selectedIndexKeys

                    let indexKeysToRemove =
                        Set.difference selectedIndexKeys indexKeysToUpdate

                    (indexKeysToUpdate, indexKeysToRemove, false)

            if indexKeysToUpdate.IsEmpty && indexKeysToRemove.IsEmpty then
                return RebuildIndicesBatchResult.CompletedBatchNoMoreBatchesPending

            else
                use connection = new SqlConnection(sqlConnectionString)
                do! connection.OpenAsync()

                let! subjectsWithConcurrencyToken =
                  // nest command task to dispose command & reader early
                  backgroundTask {
                    use command = connection.CreateCommand()

                    let (whereCondition, param) =
                        match maybeLastSubjectIdRebuilt with
                        | None -> ("", None)
                        | Some lastRebuiltKey -> ("WHERE Id > @id", (Some (SqlParameter("@id", lastRebuiltKey))))

                    command.CommandText <-
                        sprintf
                            "SELECT TOP %d Subject, ConcurrencyToken, [Id] FROM [%s].[%s] %s ORDER BY Id ASC"
                                batchSize ecosystemName lifeCycleName whereCondition

                    param
                    |> Option.iter (command.Parameters.Add >> ignore)

                    use! cursor = command.ExecuteReaderAsync()

                    return! AsyncSeq.unfoldAsync (
                        fun _ ->
                            async {
                                match! cursor.ReadAsync() |> Async.AwaitTask with
                                | true ->
                                    let pKey = cursor.GetString 2
                                    let subject =
                                        (cursor.Item 0 :?> byte[])
                                        |> ofCompressedJsonTextWithContextInfoInError lifeCycleName pKey "Subject"
                                        |> fun (x: Subject<'SubjectId>) -> x :?> 'Subject
                                    let concurrencyToken = Array.zeroCreate<byte> 8 // concurrencyToken stored BigEndian uint64 encoded into binary(8)
                                    cursor.GetBytes(1, (int64 0), concurrencyToken, 0, 8) |> ignore
                                    return Some ((subject, concurrencyToken), Nothing)
                                | false ->
                                    return None
                            }
                    ) Nothing
                    |> AsyncSeq.toListAsync
                    |> Async.StartAsTask
                  }

                if List.isEmpty subjectsWithConcurrencyToken then
                    return RebuildIndicesBatchResult.CompletedBatchNoMoreBatchesPending
                else
                    let indexKeyNamesToUpdate = indexKeysToUpdate |> Set.map (fun i -> i.KeyName)

                    let rebuildDataTable = emptyRebuildIndexList ()

                    let subjectIdWithConcurrencyTokenAndIndexEntries =
                        subjectsWithConcurrencyToken
                        |> Seq.map (fun (subject, concurrencyToken) ->
                            ((getId subject).IdString, concurrencyToken, getIndexEntriesForSubject lifeCycleAdapter.LifeCycle subject))

                    subjectIdWithConcurrencyTokenAndIndexEntries
                    |> Seq.collect (fun (idStr, concurrencyToken, indexValuesWithViolationErrs) ->
                        indexValuesWithViolationErrs
                        |> Map.toSeq
                        |> Seq.collect (fun (key, (values, uniqueViolationError)) -> values |> Seq.map(fun value -> (key, value, uniqueViolationError.IsSome)))
                        |> Seq.where (fun (key, _, _) -> indexKeyNamesToUpdate.Contains key)
                        |> Seq.map (fun kv -> (idStr, concurrencyToken, kv))
                    )
                    |> Seq.iter (fun (idStr, concurrencyToken, (key, indexValue, isUnique)) ->
                        let valueInt, valueStr, kind =
                            match indexValue, isUnique with
                            | NumIndexValue intVal, false   -> box intVal, null, IndexKindCode.NumericNonUnique
                            | NumIndexValue intVal, true    -> box intVal, null, IndexKindCode.NumericUnique
                            | StrIndexValue strVal, false   -> null, box strVal, IndexKindCode.StringNonUnique
                            | StrIndexValue strVal, true    -> null, box strVal, IndexKindCode.StringUnique
                            | SearchIndexValue strVal, _    -> null, box strVal, IndexKindCode.StringSearch
                            | GeographyIndexValue strVal, _ -> null, box strVal, IndexKindCode.Geography
                        rebuildDataTable.Rows.Add("" (* promotedKey *), "" (* promotedValue *), idStr, concurrencyToken, key, valueStr, valueInt, box kind, (* isDelete *) false) |> ignore
                    )

                    // index keys to remove without new values
                    subjectsWithConcurrencyToken
                    |> Seq.iter (fun (subject, concurrencyToken) ->
                        let idStr = subject |> getId |> getIdString
                        indexKeysToRemove
                        |> Set.iter (fun indexKey ->
                            let keyName, kindCode =
                                match indexKey with
                                | IndexKey.Numeric   keyName -> keyName, IndexKindCode.NumericNonUnique
                                | IndexKey.String    keyName -> keyName, IndexKindCode.StringNonUnique
                                | IndexKey.Search    keyName -> keyName, IndexKindCode.StringSearch
                                | IndexKey.Geography keyName -> keyName, IndexKindCode.Geography
                            rebuildDataTable.Rows.Add("" (* promotedKey *), "" (* promotedValue *), idStr, concurrencyToken, keyName, (* valueStr *) null, (* valueInt *) null, kindCode, (* isDelete *) true) |> ignore
                        )
                    )

                    // rebuild promoted index tables
                    match lifeCycleAdapter.LifeCycle.Storage.Type with
                    | StorageType.Volatile
                    | StorageType.Custom _ ->
                        failwith "unexpected storage type for Sql storage handler"
                    | StorageType.Persistent (promotedIndicesConfig, _) ->
                        let promotedKeysToUpdate =
                            let promotedKeysByBaseKey =
                                promotedIndicesConfig.Mappings
                                |> Map.map (fun _ -> NonemptyList.toList >> List.choose (function | Choice1Of2 baseKey -> Some baseKey | Choice2Of2 _sep -> None))
                                |> Map.fold (
                                    fun state promotedKey baseKeys ->
                                        List.fold (
                                            fun state baseKey ->
                                                state |> Map.add baseKey (state |> Map.tryFind baseKey |> Option.defaultValue Set.empty |> Set.add promotedKey)
                                            ) state baseKeys
                                    ) Map.empty

                            indexKeyNamesToUpdate |> Set.map (fun baseKeyName -> Map.tryFind (BaseKey baseKeyName) promotedKeysByBaseKey |> Option.defaultValue Set.empty) |> Set.unionMany

                        subjectIdWithConcurrencyTokenAndIndexEntries
                        |> Seq.map (fun (id, concurrencyToken, indexEntries) ->
                            (id, concurrencyToken, indexEntries |> getPromotedIndexEntries lifeCycleAdapter.LifeCycle))
                        |> Seq.collect (fun (idStr, concurrencyToken, promotedIndexEntries) ->
                            promotedIndexEntries
                            |> Set.toList
                            |> Seq.where (fun ((promotedKey, _promotedValue), (BaseKey baseKey, _baseValue)) -> indexKeyNamesToUpdate.Contains baseKey || promotedKeysToUpdate.Contains promotedKey)
                            |> Seq.map (fun kv -> (idStr, concurrencyToken, kv)))
                        |> Seq.iter (fun (idStr, concurrencyToken, ((PromotedKey promotedKey, PromotedValue promotedValue), (BaseKey baseKey, baseValue))) ->
                            let valueInt, valueStr, kindCode =
                                match baseValue with
                                | Choice1Of2 intVal -> box intVal, null, 1
                                | Choice2Of2 (BaseValue strVal) -> null, box strVal, 2
                            rebuildDataTable.Rows.Add(promotedKey, promotedValue, idStr, concurrencyToken, baseKey, valueStr, valueInt, kindCode, (* isDelete *) false) |> ignore)

                    // TODO: what if index violation on rebuild ?
                    //     especially false-positive e.g. if indices are "eventually unique" after rebuild but can conflict during rebuild

                    use updateCommand = new SqlCommand((sprintf "[%s].[%s_RebuildIndices_V2]" ecosystemName lifeCycleName), connection)
                    updateCommand.Parameters.Add("@removeAllNonMatchingKeys", SqlDbType.Bit).Value <- removeAllNonMatchingKeys
                    updateCommand.CommandType <- CommandType.StoredProcedure
                    updateCommand.Parameters.Add("@indices", SqlDbType.Structured)
                    |> fun param ->
                        param.Value <- rebuildDataTable
                        param.TypeName <- sprintf "[%s].RebuildIndexList_V4" ecosystemName
                    let subjectUpdatedConcurrently = SqlParameter("@subjectUpdatedConcurrently", SqlDbType.Bit)
                    subjectUpdatedConcurrently.Direction <- ParameterDirection.Output
                    updateCommand.Parameters.Add subjectUpdatedConcurrently |> ignore

                    do! updateCommand.ExecuteNonQueryAsync() |> Task.Ignore
                    if Object.Equals (subjectUpdatedConcurrently.Value, true) then
                        return RebuildIndicesBatchResult.SubjectUpdatedConcurrentlyTryAgain
                    elif subjectsWithConcurrencyToken.Length < int batchSize then
                        return RebuildIndicesBatchResult.CompletedBatchNoMoreBatchesPending
                    else
                        let maxId = subjectsWithConcurrencyToken |> List.last |> fst |> getId |> getIdString
                        return RebuildIndicesBatchResult.CompletedBatch(maxId)
        }
        |> SqlServerTransientErrorDetection.wrapTransientExceptions

    let byteArraysEqual (arr1: byte[]) (arr2: byte[]) =
        arr1.Length = arr2.Length && (Array.forall2 (=) arr1 arr2)

    let parallelChoose (f: 'a -> Option<'b>) (l: list<'a>) : list<'b> =
        l
        |> System.Linq.ParallelEnumerable.AsParallel
        |> fun pQuery -> System.Linq.ParallelEnumerable.Select (pQuery, f)
        |> Seq.choose id
        |> List.ofSeq

    let reEncodeSubjects (lifeCycleName: string) (maybeLastSubjectIdReEncoded: Option<string>) (batchSize: uint16) : Task<ReEncodeSubjectsBatchResult> =
        fun () -> backgroundTask {
            use connection = new SqlConnection(sqlConnectionString)
            do! connection.OpenAsync()

            let! rawDataToReEncode =
              // nest command task to dispose command & reader early
              backgroundTask {
                use command = connection.CreateCommand()

                let whereCondition, param =
                    match maybeLastSubjectIdReEncoded with
                    | None -> ("", None)
                    | Some lastRebuiltId -> ("WHERE Id > @id", (Some (SqlParameter("@id", lastRebuiltId))))

                command.CommandText <-
                    sprintf
                        "SELECT TOP %d Id, Subject, [Operation], OurSubscriptions, ConcurrencyToken FROM [%s].[%s] %s ORDER BY Id ASC"
                            batchSize ecosystemName lifeCycleName whereCondition

                param
                |> Option.iter (command.Parameters.Add >> ignore)

                use! cursor = command.ExecuteReaderAsync()

                return! AsyncSeq.unfoldAsync (
                    fun _ ->
                        async {
                            match! cursor.ReadAsync() |> Async.AwaitTask with
                            | true ->
                                let subjectIdStr = cursor.GetString 0
                                let subjectBytes = (cursor.Item 1 :?> byte[])
                                let operationBytes = (cursor.Item 2 :?> byte[])
                                let ourSubscriptionsBytes = (cursor.Item 3 :?> byte[])
                                let concurrencyToken = Array.zeroCreate<byte> 8
                                cursor.GetBytes(4, (int64 0), concurrencyToken, 0, 8) |> ignore

                                return Some ((subjectIdStr, subjectBytes, operationBytes, ourSubscriptionsBytes, concurrencyToken), Nothing)
                            | false ->
                                return None
                        }
                ) Nothing
                |> AsyncSeq.toListAsync
                |> Async.StartAsTask
              }

            let filteredDataToReEncode =
                rawDataToReEncode
                |> parallelChoose (fun (subjectIdStr, originalSubjectBytes, originalOperationBytes, originalOurSubscriptionsBytes, concurrencyToken) ->
                    // TODO: add options to stop or ignore if unable to decode subject
                    let maybeSubject =
                        originalSubjectBytes
                        |> tryOfCompressedJson<Subject<'SubjectId>>
                        |> Option.map (fun s -> s :?> 'Subject)

                    // operations are audit-only so sometimes codecs not evolved, don't crash on error
                    let maybeOperation: Option<SubjectAuditOperation<'LifeAction, 'Constructor>> =
                        originalOperationBytes
                        |> DataEncode.decodeSubjectAuditOperation
                        |> Result.toOption

                    // ourSubscriptions *in history* is not important, can ignore if codecs not evolved
                    let maybeOurSubscriptions: Option<Map<SubscriptionName, SubjectReference>> =
                        originalOurSubscriptionsBytes
                        |> tryOfCompressedJson

                    let maybeUpdatedSubjectBytes =
                        maybeSubject
                        |> Option.map (fun subject -> toCompressedJson (subject :> Subject<'SubjectId>))
                        |> Option.bind (fun updatedSubjectBytes ->
                            if byteArraysEqual originalSubjectBytes updatedSubjectBytes then None else Some updatedSubjectBytes)

                    let maybeUpdatedOperationBytes =
                        maybeOperation
                        |> Option.map (function
                            | SubjectAuditOperation.Act action ->
                                toCompressedJson (action :> LifeAction)
                            | SubjectAuditOperation.Construct ctor ->
                                toCompressedJson (ctor :> Constructor))
                        |> Option.bind (fun updatedOperationBytes ->
                            if byteArraysEqual originalOperationBytes updatedOperationBytes then None else Some updatedOperationBytes)

                    let maybeUpdatedOurSubscriptionsBytes =
                        maybeOurSubscriptions
                        |> Option.map toCompressedJson
                        |> Option.bind (fun updatedOurSubscriptionsBytes ->
                            if byteArraysEqual originalOurSubscriptionsBytes updatedOurSubscriptionsBytes then None else Some updatedOurSubscriptionsBytes)

                    // TODO: also NextTickContext ? maybe later if it becomes an issue

                    match maybeUpdatedSubjectBytes, maybeUpdatedOperationBytes, maybeUpdatedOurSubscriptionsBytes with
                    | None, None, None ->
                        None // nothing to update
                    | _ ->
                        Some (subjectIdStr, maybeUpdatedSubjectBytes, maybeUpdatedOperationBytes, maybeUpdatedOurSubscriptionsBytes, concurrencyToken)
                    )

            let! subscriptionsDataToReEncode =
                if rawDataToReEncode.IsEmpty then
                    Task.FromResult []
                else
                    backgroundTask {
                        use command = connection.CreateCommand()
                        command.CommandText <-
                            sprintf
                                "SELECT [SubjectId], [SubscriptionName], [LifeCycleName], [SubscriberId], [LifeEvent] FROM [%s].[%s_Subscription] s JOIN @ids i ON s.SubjectId = i.Id"
                                    ecosystemName lifeCycleAdapter.LifeCycle.Name
                        let idsStrRecords =
                            let dataTable = new DataTable()
                            dataTable.Columns.Add("Id", typeof<string>).MaxLength <- 80
                            rawDataToReEncode
                            |> Seq.map (fun (subjectIdStr, _, _, _, _) -> subjectIdStr)
                            |> Set.ofSeq
                            |> Seq.iter (fun subjectIdStr ->
                                dataTable.Rows.Add(subjectIdStr) |> ignore)
                            dataTable
                        command.Parameters.Add("@ids", SqlDbType.Structured)
                        |> fun param ->
                            param.TypeName <- sprintf "[%s].IdList" ecosystemName
                            param.Value <- idsStrRecords

                        use! cursor = command.ExecuteReaderAsync()
                        return!
                            AsyncSeq.unfoldAsync (
                                fun _ ->
                                    async {
                                        match! cursor.ReadAsync() |> Async.AwaitTask with
                                        | true ->
                                            // [SubjectId], [SubscriptionName], [LifeCycleName], [SubscriberId], [LifeEvent]
                                            let subjectId = cursor.GetString 0
                                            let subscriptionName = cursor.GetString 1
                                            let lifeCycleName = cursor.GetString 2
                                            let subscriberId = cursor.GetString 3
                                            let lifeEventBytes = cursor.Item 4 :?> byte[]
                                            return Some ((subjectId, subscriptionName, lifeCycleName, subscriberId, lifeEventBytes), Nothing)
                                        | false ->
                                            return None
                                    }
                            ) Nothing
                            |> AsyncSeq.toListAsync
                            |> Async.StartAsTask
                    }

            let filteredSubscriptionsDataToReEncode =
                subscriptionsDataToReEncode
                |> parallelChoose (fun (subjectIdStr, subscriptionName, lifeCycleName, subscriberId, originalLifeEventBytes) ->
                    // TODO: add options to stop or ignore if unable to decode life event
                    let maybeLifeEvent =
                        originalLifeEventBytes
                        |> tryOfCompressedJson<LifeEvent>

                    maybeLifeEvent
                    |> Option.map toCompressedJson
                    |> Option.bind (fun updatedLifeEventBytes ->
                        if byteArraysEqual originalLifeEventBytes updatedLifeEventBytes then
                            None
                        else
                            Some (subjectIdStr, subscriptionName, lifeCycleName, subscriberId, updatedLifeEventBytes)))

            let subjectUpdatedConcurrently = SqlParameter("@subjectUpdatedConcurrently", SqlDbType.Bit)
            if filteredDataToReEncode.IsNonempty || filteredSubscriptionsDataToReEncode.IsNonempty then
                let reEncodeDataTable = getEmptyReEncodeSubjectsListDataTable ()

                filteredDataToReEncode
                |> List.iter (fun (subjectIdStr, maybeUpdatedSubjectBytes, maybeUpdatedOperationBytes, maybeUpdatedOurSubscriptionsBytes, concurrencyToken) ->
                    reEncodeDataTable.Rows.Add(subjectIdStr, (* version *) int64 0L, Option.toObj maybeUpdatedSubjectBytes, Option.toObj maybeUpdatedOperationBytes, Option.toObj maybeUpdatedOurSubscriptionsBytes, concurrencyToken) |> ignore)

                let reEncodeSubscriptionsDataTable =
                    let dataTable = new DataTable()
                    dataTable.Columns.Add("SubjectId",        typeof<string>).MaxLength <- 80
                    dataTable.Columns.Add("SubscriptionName", typeof<string>).MaxLength <- 200
                    dataTable.Columns.Add("LifeCycleName",    typeof<string>).MaxLength <- 80
                    dataTable.Columns.Add("SubscriberId",     typeof<string>).MaxLength <- 80
                    dataTable.Columns.Add("LifeEvent",        typeof<byte[]>).MaxLength <- -1
                    dataTable

                filteredSubscriptionsDataToReEncode
                |> List.iter (fun (subjectIdStr, subscriptionName, lifeCycleName, subscriberId, updatedLifeEventBytes) ->
                    reEncodeSubscriptionsDataTable.Rows.Add (subjectIdStr, subscriptionName, lifeCycleName, subscriberId, updatedLifeEventBytes) |> ignore)

                use updateCommand = new SqlCommand((sprintf "[%s].[%s_ReEncode]" ecosystemName lifeCycleName), connection)
                updateCommand.CommandType <- CommandType.StoredProcedure
                updateCommand.Parameters.Add("@data", SqlDbType.Structured)
                |> fun param ->
                    param.Value <- reEncodeDataTable
                    param.TypeName <- sprintf "[%s].ReEncodeSubjectsList" ecosystemName
                updateCommand.Parameters.Add("@subscriptions", SqlDbType.Structured)
                |> fun param ->
                    param.Value <- reEncodeSubscriptionsDataTable
                    param.TypeName <- sprintf "[%s].ReEncodeSubscriptionsList" ecosystemName
                subjectUpdatedConcurrently.Direction <- ParameterDirection.Output
                updateCommand.Parameters.Add subjectUpdatedConcurrently |> ignore

                do! updateCommand.ExecuteNonQueryAsync() |> Task.Ignore

            if Object.Equals (subjectUpdatedConcurrently.Value, true) then
                return ReEncodeSubjectsBatchResult.SubjectUpdatedConcurrentlyTryAgain
            elif rawDataToReEncode.Length < int batchSize then
                return ReEncodeSubjectsBatchResult.CompletedBatchNoMoreBatchesPending
            else
                let maxSubjectIdStr = rawDataToReEncode |> List.last |> fun (subjectIdStr, _, _, _, _) -> subjectIdStr
                return ReEncodeSubjectsBatchResult.CompletedBatch maxSubjectIdStr
        }
        |> SqlServerTransientErrorDetection.wrapTransientExceptions

    let reEncodeSubjectsHistory (lifeCycleName: string) (maybeLastSubjectIdVersionReEncoded: Option<string * uint64>) (batchSize: uint16) : Task<ReEncodeSubjectsHistoryBatchResult> =
        fun () -> backgroundTask {
            use connection = new SqlConnection(sqlConnectionString)
            do! connection.OpenAsync()
            use command = connection.CreateCommand()

            let whereCondition, param =
                match maybeLastSubjectIdVersionReEncoded with
                | None -> ("", [])
                | Some (lastReEncodedId, lastReEncodedVersion) ->
                    let versionParam = SqlParameter("@version", SqlDbType.BigInt)
                    versionParam.Value <- int64 lastReEncodedVersion
                    // Very sensitive piece of SQL, benign changes can lead to table scans. If you change then must profile.
                    ("WHERE Id >= @id AND (Id <> @id OR [Version] > @version)",
                     [ SqlParameter("@id", lastReEncodedId); versionParam ])

            command.CommandText <-
                sprintf
                    "SELECT TOP %d Id, [Version], Subject, [Operation], OurSubscriptions FROM [%s].[%s_History] %s ORDER BY Id ASC, [Version] ASC"
                        batchSize ecosystemName lifeCycleName whereCondition

            param
            |> List.iter (command.Parameters.Add >> ignore)

            use! cursor = command.ExecuteReaderAsync()
            let! rawDataToReEncode =
                AsyncSeq.unfoldAsync (
                    fun _ ->
                        async {
                            match! cursor.ReadAsync() |> Async.AwaitTask with
                            | true ->
                                let subjectIdStr = cursor.GetString 0
                                let version = cursor.GetInt64 1
                                let subjectBytes = (cursor.Item 2 :?> byte[])
                                let operationBytes = (cursor.Item 3 :?> byte[])
                                let ourSubscriptionsBytes = (cursor.Item 4 :?> byte[])

                                return Some ((subjectIdStr, version, subjectBytes, operationBytes, ourSubscriptionsBytes), Nothing)
                            | false ->
                                return None
                        }
                ) Nothing
                |> AsyncSeq.toListAsync
                |> Async.StartAsTask

            let filteredDataToReEncode =
                rawDataToReEncode
                |> parallelChoose (fun (subjectIdStr, version, originalSubjectBytes, originalOperationBytes, originalOurSubscriptionsBytes) ->
                    // TODO: add options to stop or ignore if unable to decode subject
                    let maybeSubject =
                        originalSubjectBytes
                        |> tryOfCompressedJson<Subject<'SubjectId>>
                        |> Option.map (fun subject -> subject :?> 'Subject)

                    // operations are audit-only so sometimes codecs not evolved, don't crash on error
                    let maybeOperation: Option<SubjectAuditOperation<'LifeAction, 'Constructor>> =
                        originalOperationBytes
                        |> DataEncode.decodeSubjectAuditOperation
                        |> Result.toOption

                    // ourSubscriptions *in history* is not important, can ignore if codecs not evolved
                    let maybeOurSubscriptions: Option<Map<SubscriptionName, SubjectReference>> =
                        originalOurSubscriptionsBytes
                        |> tryOfCompressedJson

                    let maybeUpdatedSubjectBytes =
                        maybeSubject
                        |> Option.map (fun subject -> toCompressedJson (subject :> Subject<'SubjectId>))
                        |> Option.bind (fun updatedSubjectBytes ->
                            if byteArraysEqual originalSubjectBytes updatedSubjectBytes then None else Some updatedSubjectBytes)

                    let maybeUpdatedOperationBytes =
                        maybeOperation
                        |> Option.map (function
                            | SubjectAuditOperation.Act action ->
                                toCompressedJson (action :> LifeAction)
                            | SubjectAuditOperation.Construct ctor ->
                                toCompressedJson (ctor :> Constructor))
                        |> Option.bind (fun updatedOperationBytes ->
                            if byteArraysEqual originalOperationBytes updatedOperationBytes then None else Some updatedOperationBytes)

                    let maybeUpdatedOurSubscriptionsBytes =
                        maybeOurSubscriptions
                        |> Option.map toCompressedJson
                        |> Option.bind (fun updatedOurSubscriptionsBytes ->
                            if byteArraysEqual originalOurSubscriptionsBytes updatedOurSubscriptionsBytes then None else Some updatedOurSubscriptionsBytes)

                    // TODO: also NextTickContext ? maybe later if it becomes an issue

                    match maybeUpdatedSubjectBytes, maybeUpdatedOperationBytes, maybeUpdatedOurSubscriptionsBytes with
                    | None, None, None ->
                        None // nothing to update
                    | _ ->
                        Some (subjectIdStr, version, maybeUpdatedSubjectBytes, maybeUpdatedOperationBytes, maybeUpdatedOurSubscriptionsBytes)
                    )

            if filteredDataToReEncode.IsNonempty then
                let reEncodeDataTable = getEmptyReEncodeSubjectsListDataTable ()

                filteredDataToReEncode
                |> List.iter (fun (subjectIdStr, version: int64, maybeUpdatedSubjectBytes, maybeUpdatedOperationBytes, maybeUpdatedOurSubscriptionsBytes) ->
                    reEncodeDataTable.Rows.Add(subjectIdStr, version, Option.toObj maybeUpdatedSubjectBytes, Option.toObj maybeUpdatedOperationBytes, Option.toObj maybeUpdatedOurSubscriptionsBytes, (* concurrencyToken *) null) |> ignore)

                use updateCommand = new SqlCommand((sprintf "[%s].[%s_ReEncodeHistory]" ecosystemName lifeCycleName), connection)
                updateCommand.CommandType <- CommandType.StoredProcedure
                updateCommand.Parameters.Add("@data", SqlDbType.Structured)
                |> fun param ->
                    param.Value <- reEncodeDataTable
                    param.TypeName <- sprintf "[%s].ReEncodeSubjectsList" ecosystemName

                do! updateCommand.ExecuteNonQueryAsync() |> Task.Ignore

            if rawDataToReEncode.Length < int batchSize then
                return ReEncodeSubjectsHistoryBatchResult.CompletedBatchNoMoreBatchesPending
            else
                let maxVersionId = rawDataToReEncode |> List.last |> fun (subjectIdStr, version, _, _, _) -> (subjectIdStr, uint64 version)
                return ReEncodeSubjectsHistoryBatchResult.CompletedBatch maxVersionId
        }
        |> SqlServerTransientErrorDetection.wrapTransientExceptions

    let getIdsOfSubjectsWithStalledSideEffects (batchSize: uint16) =
        fun () -> backgroundTask {
            use connection = new SqlConnection(sqlConnectionString)
            do! connection.OpenAsync()
            use command = connection.CreateCommand()

            // stalled side effects excluding failed,
            // plus subjects with stalled timers e.g. due to invalidated GrainIdHash
            command.CommandText <- sprintf
                "SELECT TOP %u SubjectId
                 FROM
                 (
                     SELECT SubjectId
                     FROM [%s].[%s_SideEffect] (NOLOCK)
                     WHERE FailureSeverity IS NULL
                     GROUP BY SubjectId
                     HAVING MIN(CreatedOn) < DATEADD(minute, -1, SYSDATETIMEOFFSET())
                     UNION ALL
                     SELECT [Id] AS SubjectId FROM [%s].[%s] (NOLOCK)
                     WHERE NextTickOn IS NOT NULL AND NextTickFired = 0 AND NextTickOn < DATEADD(minute, -2, SYSDATETIMEOFFSET())
                 ) stalled"
                batchSize ecosystemName lifeCycleName ecosystemName lifeCycleName
            use! reader = command.ExecuteReaderAsync()
            let! subjectIds =
                AsyncSeq.unfoldAsync (
                    fun _ ->
                        async {
                            match! reader.ReadAsync() |> Async.AwaitTask with
                            | true ->
                                return
                                    (reader.Item 0 :?> string)
                                    |> fun subj -> subj, Nothing
                                    |> Some
                            | false ->
                                return None
                        }
                ) Nothing
                |> AsyncSeq.toListAsync
                |> Async.StartAsTask

            return Set.ofList subjectIds
        }
        |> SqlServerTransientErrorDetection.wrapTransientExceptions

    let getBatchOfSubjectsWithTickStateAndOurSubscriptions (cursor: Option<string>) (batchSize: uint16) =
        fun () -> backgroundTask {
            use connection = new SqlConnection(sqlConnectionString)
            do! connection.OpenAsync()
            use command = connection.CreateCommand()

            match cursor with
            | None ->
                command.CommandText <-
                    sprintf
                        "SELECT TOP %u Subject, SubjectLastUpdatedOn, NextTickOn, NextTickFired, NextTickContext, OurSubscriptions, [Id] FROM [%s].[%s]
                        ORDER BY Id"
                        batchSize ecosystemName lifeCycleName

            | Some cursor ->
                command.CommandText <-
                    sprintf
                        "SELECT TOP %u Subject, SubjectLastUpdatedOn, NextTickOn, NextTickFired, NextTickContext, OurSubscriptions, [Id] FROM [%s].[%s]
                        WHERE Id > @cursor
                        ORDER BY Id"
                        batchSize ecosystemName lifeCycleName
                command.Parameters.Add("@cursor", SqlDbType.NVarChar).Value <- cursor

            use! reader = command.ExecuteReaderAsync()
            return!
                AsyncSeq.unfoldAsync (
                    fun _ ->
                        async {
                            match! reader.ReadAsync() |> Async.AwaitTask with
                            | true ->
                                let pKey = reader.GetString 6
                                let subject =
                                        (reader.Item 0 :?> byte[])
                                        |> ofCompressedJsonTextWithContextInfoInError lifeCycleName pKey "Subject"
                                        |> fun (x: Subject<'SubjectId>) -> x :?> 'Subject
                                let subjectLastUpdatedOn = reader.GetDateTimeOffset 1
                                let tickState = readTickState reader 2 3 4
                                let ourSubscriptions =
                                    (reader.Item 5 :?> byte[])
                                    |> ofCompressedJsonTextWithContextInfoInError lifeCycleName pKey "OurSubscriptions"
                                return Some ((subject, subjectLastUpdatedOn, tickState, ourSubscriptions), Nothing)
                            | false ->
                                return None
                        }
                ) Nothing
                |> AsyncSeq.toListAsync
                |> Async.StartAsTask
        }
        |> SqlServerTransientErrorDetection.wrapTransientExceptions

    let getSideEffectMetrics () : Task<Option<GetSideEffectMetricsResult>> =
        fun () -> backgroundTask {
            use connection = new SqlConnection(sqlConnectionString)
            do! connection.OpenAsync()
            use command = connection.CreateCommand()

            command.CommandText <-
                // oldest age includes only Side effects that did not fail
                sprintf
                    "SELECT DATEDIFF_BIG(MILLISECOND, MIN(CASE ISNULL(FailureSeverity, 0) WHEN 0 THEN CreatedOn END), GETUTCDATE()) AS OldestAge,
                         COUNT(*) AS QueueLength,
                         COUNT(CASE FailureSeverity WHEN 1 THEN 1 END) AS WarningCount,
                         COUNT(CASE FailureSeverity WHEN 2 THEN 1 END) AS ErrorCount
                     FROM [%s].[%s_SideEffect] (NOLOCK)
                     WHERE FailureAckedUntil IS NULL OR [FailureAckedUntil] < GETUTCDATE()" ecosystemName lifeCycleName
            use! reader = command.ExecuteReaderAsync()

             // no need to check result, always should be one row
            do! reader.ReadAsync() |> Task.Ignore
            let oldestAgeOfNonFailed =
                match reader.IsDBNull 0 with
                | true -> TimeSpan.Zero
                | false -> reader.GetInt64 0 |> float |> TimeSpan.FromMilliseconds |> max TimeSpan.Zero

            return Some {
                OldestAgeOfNonFailed = oldestAgeOfNonFailed
                QueueLength = reader.GetInt32 1
                FailureWarningCount = reader.GetInt32 2
                FailureErrorCount = reader.GetInt32 3
            }
        }
        |> SqlServerTransientErrorDetection.wrapTransientExceptions

    let getTimerMetrics () : Task<Option<GetTimerMetricsResult>> =
        fun () -> backgroundTask {
            use connection = new SqlConnection(sqlConnectionString)
            do! connection.OpenAsync()
            use command = connection.CreateCommand()

            command.CommandText <- sprintf "
                SELECT DATEDIFF(second, MIN(NextTickOn), GETUTCDATE()),
                COUNT(CASE WHEN NextTickOn < GETUTCDATE() THEN 1 END)
                FROM [%s].[%s] (NOLOCK) WHERE NextTickOn IS NOT NULL" ecosystemName lifeCycleName
            use! reader = command.ExecuteReaderAsync()

            match! reader.ReadAsync() with
            | true ->
                let oldestAge =
                    match reader.IsDBNull 0 with
                    | true -> 0.0
                    | false ->
                        reader.GetInt32 0 |> fun age -> if age < 0 then 0.0 else float age
                    |> TimeSpan.FromSeconds
                let expiredCount = reader.GetInt32 1

                return Some {
                    OldestAge = oldestAge
                    ExpiredCount = expiredCount
                }
            | false ->
                return None
        }
        |> SqlServerTransientErrorDetection.wrapTransientExceptions

    let processPermanentFailures
        (scope: UpdatePermanentFailuresScope)
        (filters: Set<UpdatePermanentFailuresFilter>)
        (operation: UpdatePermanentFailuresOperation)
        : Task<Option<UpdatePermanentFailuresResult<'LifeAction>>> =
            backgroundTask {

            let tableName = sprintf "[%s].[%s_SideEffect]" ecosystemName lifeCycleAdapter.LifeCycle.Name
            let maybeOnlyCompletedGroupsSql = // TODO: consider removing this
                match operation with
                | UpdatePermanentFailuresOperation.Retry ->
                    // Just in case, don't allow to retry failed side effect if part of its group is still in progress:
                    // if whole group retried then framework may reorder side effects within the group depending on side effect type
                    "HAVING MIN(ISNULL(FailureSeverity, 0)) > 0"
                | UpdatePermanentFailuresOperation.Ack _
                | UpdatePermanentFailuresOperation.Delete ->
                    // it's ok to ack or delete side effect if its group is still in progress
                    ""

            let filterSql, filterParams =
                filters
                |> Seq.mapi (fun i filter ->
                    match filter with
                    | UpdatePermanentFailuresFilter.OnlyWarnings ->
                        "AND FailureSeverity = 1", None
                    | UpdatePermanentFailuresFilter.ReasonContains searchStr ->
                        let paramName = sprintf "@failureReason%d" i
                        sprintf "AND FailureReason LIKE %s" paramName,
                        Some (paramName, sprintf "%%%s%%" searchStr |> box))
                |> List.ofSeq
                |> fun s ->
                    s |> Seq.map fst |> (fun parts -> String.Join ("\n", parts)),
                    s |> Seq.map snd |> Seq.choose id

            let sqlPart1 = sprintf "
                WITH failureGroups AS
                (
                    SELECT SubjectId, SideEffectSeqNumber FROM %s
                    WHERE
                        FailureSeverity IS NOT NULL
                        AND ISNULL(@sideEffectId, SideEffectId) = SideEffectId
                        AND ISNULL(@subjectId, SubjectId) = SubjectId
                        AND ISNULL(@seqNumber, SideEffectSeqNumber) = SideEffectSeqNumber
                        %s
                    GROUP BY SubjectId, SideEffectSeqNumber
                    %s
                    ORDER BY SubjectId, SideEffectSeqNumber
                    OFFSET 0 ROWS
                    FETCH NEXT @batchSize ROWS ONLY)" tableName filterSql maybeOnlyCompletedGroupsSql

            let sqlPart2, retryOutputSideEffects =
                match operation with
                | UpdatePermanentFailuresOperation.Ack _ ->
                    "UPDATE se SET FailureAckedUntil = DATEADD(SECOND, @ackForSeconds, GETUTCDATE())
                     OUTPUT DELETED.SubjectId, DELETED.SideEffectId, DELETED.SideEffectSeqNumber",
                    false
                | UpdatePermanentFailuresOperation.Retry ->
                    // in case for Retry just select, upstream code will invoke the grain to submit retried side effects
                    "SELECT se.SubjectId, se.SideEffectId, se.SideEffectSeqNumber",
                    true
                | UpdatePermanentFailuresOperation.Delete ->
                    "DELETE se
                     OUTPUT DELETED.SubjectId, DELETED.SideEffectId, DELETED.SideEffectSeqNumber",
                    false

            let sqlPart3 =
                sprintf "FROM %s se
                JOIN failureGroups ON failureGroups.SubjectId = se.SubjectId AND failureGroups.SideEffectSeqNumber = se.SideEffectSeqNumber
                WHERE
                    se.FailureSeverity IS NOT NULL
                    AND ISNULL(@sideEffectId, se.SideEffectId) = se.SideEffectId
                    AND ISNULL(@seqNumber, se.SideEffectSeqNumber) = se.SideEffectSeqNumber
                    %s" tableName filterSql

            let sql = sprintf "%s\n%s\n%s" sqlPart1 sqlPart2 sqlPart3

            let batchSize, subjectIdArg, sideEffectIdArg, seqNumArg =
                match scope with
                | UpdatePermanentFailuresScope.Single (subjectId, sideEffectId) ->
                    1uy, box subjectId, box sideEffectId, box DBNull.Value
                | UpdatePermanentFailuresScope.SeqNum (subjectId, seqNum) ->
                    1uy, box subjectId, box DBNull.Value, seqNum |> uint64ToInt64MaintainOrder |> box
                | UpdatePermanentFailuresScope.Subject subjectId ->
                    50uy, box subjectId, box DBNull.Value, box DBNull.Value
                | UpdatePermanentFailuresScope.NextSeqBatch batchSize ->
                    batchSize, box DBNull.Value, box DBNull.Value, box DBNull.Value

            let ackForSecondsArg =
                match operation with
                | UpdatePermanentFailuresOperation.Ack timeSpan ->
                    timeSpan.TotalSeconds |> int |> box
                | UpdatePermanentFailuresOperation.Retry
                | UpdatePermanentFailuresOperation.Delete ->
                    box DBNull.Value

            use connection = new SqlConnection(sqlConnectionString)
            do! connection.OpenAsync()
            use command = connection.CreateCommand()
            command.CommandText <- sql
            command.Parameters.Add("@batchSize", SqlDbType.TinyInt).Value <- batchSize
            command.Parameters.Add("@subjectId", SqlDbType.NVarChar).Value <- subjectIdArg
            command.Parameters.Add("@sideEffectId", SqlDbType.UniqueIdentifier).Value <- sideEffectIdArg
            command.Parameters.Add("@seqNumber", SqlDbType.BigInt).Value <- seqNumArg
            command.Parameters.Add("@ackForSeconds", SqlDbType.Int).Value <- ackForSecondsArg
            filterParams |> Seq.iter (command.Parameters.AddWithValue >> ignore)

            use! reader = command.ExecuteReaderAsync()

            let! sortedSideEffectsData =
                AsyncSeq.unfoldAsync (
                    fun _ ->
                        async {
                            match! reader.ReadAsync() |> Async.AwaitTask with
                            | true ->
                                let subjectId = reader.GetString 0
                                let sideEffectId = reader.GetGuid 1
                                let sideEffectSeqNum = reader.GetInt64 2 |> int64ToUInt64MaintainOrder
                                return Some ((subjectId, sideEffectSeqNum, sideEffectId), Nothing)
                            | false ->
                                return None
                        }
                ) Nothing
                |> AsyncSeq.toListAsync
                |> Async.map List.sort
                |> Async.StartAsTask

            if sortedSideEffectsData.IsEmpty then
                return None
            else
                let lastSubjectId, lastSideEffectSeqNum, lastSideEffectId = List.last sortedSideEffectsData
                let last: LastUpdatePermanentFailuresResult = {
                    LastSubjectIdStr = lastSubjectId
                    LastSideEffectId = lastSideEffectId
                    SideEffectSeqNum = lastSideEffectSeqNum
                    LastOperation    = operation
                }

                let sideEffectIdsToRetry =
                    if retryOutputSideEffects then
                        sortedSideEffectsData
                        |> List.groupBy (fun (subjectId, _, _) -> subjectId)
                        |> List.map (fun (subjectId, group) ->
                            subjectId,
                                group
                                |> Seq.map (fun (_, _, sideEffectId) -> sideEffectId)
                                |> NonemptySet.ofSeq
                                |> Option.get)
                        |> Map.ofList
                    else
                        Map.empty

                return Some { Last = last; SideEffectIdsToRetry = sideEffectIdsToRetry }
        }

    let getNextSequenceNumber (sequenceName: string) : Task<uint64> =
        fun () -> backgroundTask {
            use connection = new SqlConnection(sqlConnectionString)
            do! connection.OpenAsync()

            use command = new SqlCommand((sprintf "[%s]._SequenceNextValue" ecosystemName), connection)
            command.CommandType <- CommandType.StoredProcedure
            command.Parameters.Add("@sequenceName", SqlDbType.NVarChar).Value <- sequenceName

            let! nextObj = command.ExecuteScalarAsync()
            let nextSigned = nextObj :?> int64
            let nextUnsigned = int64ToUInt64MaintainOrder nextSigned
            return nextUnsigned
        }
        |> SqlServerTransientErrorDetection.wrapTransientExceptions

    let peekCurrentSequenceNumber (sequenceName: string) : Task<uint64> =
        fun () -> backgroundTask {
            use connection = new SqlConnection(sqlConnectionString)
            do! connection.OpenAsync()

            use command = new SqlCommand((sprintf "[%s]._SequenceCurrentValue" ecosystemName), connection)
            command.CommandType <- CommandType.StoredProcedure
            command.Parameters.Add("@sequenceName", SqlDbType.NVarChar).Value <- sequenceName

            let! currentObj = command.ExecuteScalarAsync()
            if (currentObj = null || currentObj = (box DBNull.Value)) then
                return 0UL
            else
                let currentSigned = currentObj :?> int64
                let currentUnsigned = int64ToUInt64MaintainOrder currentSigned
                return currentUnsigned
        }
        |> SqlServerTransientErrorDetection.wrapTransientExceptions

    member this.RepairGrainIdHash (pKey: string) (expectedETag: string) (newGrainIdHash: uint32) : Task<string> =
        fun () -> backgroundTask {
            use connection = new SqlConnection(sqlConnectionString)
            do! connection.OpenAsync()

            use command = new SqlCommand((sprintf "[%s].%s_RepairGrainIdHash" ecosystemName lifeCycleName), connection)
            command.CommandType <- CommandType.StoredProcedure

            command.Parameters.Add("@id", SqlDbType.NVarChar, 80).Value <- pKey
            command.Parameters.Add("@grainIdHash", SqlDbType.Int).Value <- newGrainIdHash |> uint32ToInt32MaintainOrder

            let concurrencyToken = SqlParameter("@concurrencyToken", SqlDbType.Binary, 8)
            concurrencyToken.Direction <- ParameterDirection.InputOutput
            command.Parameters.Add concurrencyToken |> ignore
            concurrencyToken.Value <- hexStringToByteArray expectedETag

            let offSyncConcurrencyToken = SqlParameter("@offSyncConcurrencyToken", SqlDbType.Binary, 8)
            offSyncConcurrencyToken.Direction <- ParameterDirection.Output
            command.Parameters.Add offSyncConcurrencyToken |> ignore

            do! command.ExecuteNonQueryAsync() |> Task.Ignore

            if concurrencyToken.Value <> null && concurrencyToken.Value <> (box DBNull.Value) then
                let updatedETag = concurrencyToken.Value :?> byte[] |> byteArrayToHexString
                return updatedETag
            else if offSyncConcurrencyToken.Value <> null && offSyncConcurrencyToken.Value <> (box DBNull.Value) then
                let updatedETag = offSyncConcurrencyToken.Value :?> byte[] |> byteArrayToHexString
                return raise (Orleans.Storage.InconsistentStateException("ETag doesn't match", updatedETag, expectedETag))
            else
                let msg = sprintf "State with ID %s not found during RepairGrainIdHash" pKey
                return raise (Orleans.Storage.InconsistentStateException(msg, "<MISSING>", expectedETag))
        }
        |> SqlServerTransientErrorDetection.wrapTransientExceptions

    interface IGrainStorageHandler<'Subject, 'LifeAction, 'Constructor, 'LifeEvent, 'SubjectId, 'OpError> with
        member this.RebuildIndices (lifeCycleName: string) (rebuildType: IndexRebuildType) (maybeLastSubjectIdRebuilt: Option<string>) (batchSize: uint16) : Task<RebuildIndicesBatchResult> =
            rebuildIndices lifeCycleName rebuildType maybeLastSubjectIdRebuilt batchSize

        member this.ReEncodeSubjects (lifeCycleName: string) (maybeLastSubjectIdReEncoded: Option<string>) (batchSize: uint16) : Task<ReEncodeSubjectsBatchResult> =
            reEncodeSubjects lifeCycleName maybeLastSubjectIdReEncoded batchSize

        member this.ReEncodeSubjectsHistory (lifeCycleName: string) (maybeLastSubjectIdVersionReEncoded: Option<string * uint64>) (batchSize: uint16) : Task<ReEncodeSubjectsHistoryBatchResult> =
            reEncodeSubjectsHistory lifeCycleName maybeLastSubjectIdVersionReEncoded batchSize

        member _.RemindersImplementation = reminderImplementation

        member _.TryReadState (pKey: string) : Task<Option<ReadStateResult<'Subject, 'Constructor, 'LifeAction, 'LifeEvent, 'OpError, 'SubjectId>>> =
            tryGetState pKey

        member _.ClearState (pKey: string) expectedETag skipHistory sideEffectId : Task =
            clearSubject pKey expectedETag skipHistory sideEffectId

        member _.AssertStateConsistency (pKey: string) (expectedETagIfCreated: Option<string>) : Task =
            assertStateConsistency pKey expectedETagIfCreated

        member _.InitializeSubject (pKey: string) (dedupInfo: Option<SideEffectDedupInfo>) (insertData: SubjectInsertData<'Subject, 'Constructor, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>) : Task<Result<InitializeSubjectSuccessResult, 'OpError>> =
            initializeSubject pKey dedupInfo insertData

        member _.PrepareInitializeSubject (pKey: string) (preparedSubjectInsertData: PreparedSubjectInsertData<'Subject, 'Constructor, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>) (uniqueIndicesToReserve: list<UniqueIndexToReserveOnPrepare<'OpError>>) (subjectTransactionId: SubjectTransactionId) : Task<Result<(* ETag *) string, 'OpError>> =
            prepareInitializeSubject pKey preparedSubjectInsertData uniqueIndicesToReserve subjectTransactionId

        member _.CommitInitializeSubject (pKey: string) (eTag: string) (subjectTransactionId: SubjectTransactionId) (insertData: SubjectInsertData<'Subject, 'Constructor, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>) : Task<InitializeSubjectSuccessResult> =
            commitInitializeSubject pKey eTag subjectTransactionId insertData

        member _.RollbackInitializeSubject (pKey: string) (uniqueIndicesToRelease: list<UniqueIndexToReleaseOnRollback>) (subjectTransactionId: SubjectTransactionId) (eTag: string) : Task =
            rollbackInitializeSubject pKey subjectTransactionId uniqueIndicesToRelease eTag

        member _.UpdateSubject (pKey: string) (dedupData: Option<UpsertDedupData>) (updateData: SubjectUpdateData<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>) : Task<Result<UpdateSubjectSuccessResult, 'OpError>> =
            updateSubject pKey dedupData updateData

        member _.PrepareUpdateSubject (pKey: string) (expectedETag: string) (preparedUpdateData: PreparedSubjectUpdateData<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>) (uniqueIndicesToReserve: list<UniqueIndexToReserveOnPrepare<'OpError>>) (subjectTransactionId: SubjectTransactionId) : Task<Result<(* ETag *) string, 'OpError>> =
            prepareUpdateSubject pKey expectedETag preparedUpdateData uniqueIndicesToReserve subjectTransactionId

        member _.RollbackUpdateSubject (pKey: string) (expectedETag: string) (uniqueIndicesToRelease: list<UniqueIndexToReleaseOnRollback>) (subjectTransactionId: SubjectTransactionId) : Task<(* ETag *) string> =
            rollbackUpdateSubject pKey expectedETag subjectTransactionId uniqueIndicesToRelease

        member _.CommitUpdateSubject (pKey: string) (subjectTransactionId: SubjectTransactionId) (updateData: SubjectUpdateData<'Subject, 'SubjectId, 'LifeAction, 'LifeEvent, 'OpError>) : Task<UpdateSubjectSuccessResult> =
            commitUpdateSubject pKey subjectTransactionId updateData

        member _.EnqueueSideEffects (pKey: string) (now: DateTimeOffset) (dedupData: Option<UpsertDedupData>) (expectedETag: string) (nextSideEffectSeqNum: GrainSideEffectSequenceNumber) (sideEffectGroups: NonemptyKeyedSet<GrainSideEffectSequenceNumber, SideEffectGroup<'LifeAction, 'OpError>>) : Task<(* newETag *) string> =
            enqueueSideEffects pKey now dedupData expectedETag nextSideEffectSeqNum sideEffectGroups

        member _.SetTickState (pKey: string) (expectedETag: string) (nextSideEffectSeqNum: GrainSideEffectSequenceNumber) (tickState: TickState) (sideEffectGroup: Option<SideEffectGroup<'LifeAction, 'OpError>>) : Task<(* newETag *) string> =
            setTickState pKey expectedETag nextSideEffectSeqNum tickState sideEffectGroup

        member _.SetTickStateAndSubscriptions (pKey: string) (expectedETag: string) (nextSideEffectSeqNum: GrainSideEffectSequenceNumber) (tickState: TickState) (subscriptions: Map<SubscriptionName, SubjectReference>) (sideEffectGroup: Option<SideEffectGroup<'LifeAction, 'OpError>>) : Task<(* newETag *) string> =
            setTickStateAndSubscriptions (pKey: string) expectedETag nextSideEffectSeqNum tickState subscriptions sideEffectGroup

        member _.ReadFailedSideEffects (pKey: string) (sideEffectIds: NonemptySet<GrainSideEffectId>) : Task<KeyedSet<GrainSideEffectSequenceNumber, SideEffectGroup<'LifeAction, 'OpError>>> =
            readFailedSideEffects pKey sideEffectIds

        member _.RetrySideEffects (pKey: string) (expectedETag: string) (nextSideEffectSeqNum: GrainSideEffectSequenceNumber) (retryingSideEffectGroups: NonemptyKeyedSet<GrainSideEffectSequenceNumber, SideEffectGroup<'LifeAction, 'OpError>>) : Task<(* newETag *) string> =
            retrySideEffects pKey expectedETag nextSideEffectSeqNum retryingSideEffectGroups

        member _.AddSubscriptions (pkey: string) (expectedETag: string) (subscriptionsToAdd: SubscriptionsToAdd<'LifeEvent>) : Task<string> =
            saveSubscriptions pkey expectedETag subscriptionsToAdd.Subscriber subscriptionsToAdd.NewSubscriptions Set.empty

        member _.RemoveSubscriptions (pkey: string) (expectedETag: string) (subscriberPKeyRef: SubjectPKeyReference) (subscriptionNames: Set<SubscriptionName>) : Task<string> =
            saveSubscriptions pkey expectedETag subscriberPKeyRef Map.empty subscriptionNames

        member _.ClearExpiredSubjectsHistory now batchSize =
            clearExpiredSubjectsHistory now batchSize

        member _.UpdateSideEffectStatus pKey sideEffectId sideEffectResult =
            updateSideEffectStatus pKey sideEffectId sideEffectResult

        member _.GetIdsOfSubjectsWithStalledSideEffects batchSize =
            getIdsOfSubjectsWithStalledSideEffects batchSize

        member _.GetBatchOfSubjectsWithTickStateAndOurSubscriptions cursor batchSize =
            getBatchOfSubjectsWithTickStateAndOurSubscriptions cursor batchSize

        member _.ProcessPermanentFailures scope filters operation =
            processPermanentFailures scope filters operation

        member _.GetSideEffectMetrics () =
            getSideEffectMetrics()

        member _.GetTimerMetrics () =
            getTimerMetrics()

        member this.GetNextSequenceNumber sequenceName =
            getNextSequenceNumber sequenceName

        member this.PeekCurrentSequenceNumber sequenceName =
            peekCurrentSequenceNumber sequenceName