module LibLifeCycleHost.SubjectReminderTable

open System
open System.Collections.Concurrent
open System.Reflection
open System.Threading.Tasks
open Microsoft.Extensions.Options
open Orleans
open Orleans.Configuration
open Orleans.Serialization
open Orleans.Runtime
open Microsoft.Extensions.Logging
open Microsoft.Data.SqlClient
open LibLifeCycle
open LibLifeCycleCore
open System.Data
open FSharp.Control
open LibLifeCycleHost.SubjectReminder
open LibLifeCycleHost.Storage.SqlServer

// This is an optimized implementation of a reminder table that simply piggybacks upon the subject tables themselves
// Consequently no data is actually written to from here, and this is only used to read existing timers when the system
// loads up
type SubjectReminderTable
    (
        config: SqlServerConnectionStrings,
        ecosystem: Ecosystem,
        grainReferenceConverter: IGrainReferenceConverter,
        lifeCycleAdapterCollection: HostedLifeCycleAdapterCollection,
        logger: ILogger<SubjectReminderTable>,
        clock: Service<Clock>,
        ringOptions: IOptions<ConsistentRingOptions>,
        isDevHost: bool
    ) =

    let connectionString = config.ForEcosystem ecosystem.Name

    // We need access to some internals to map grain reference to the underlying lifecycle

    // TODO: FIXME-ORLEANS
    // The type is in assembly Orleans.Core, incidentally ReminderEntry is in the same assembly. If that changes
    // in a future version of Orleans, the way we get the assembly needs to be updated.
    let orleansTypeUtilsType, orleansGrainInterfaceUtilsType, orleansGrainClassDataType, orleansGrainIdType =
        let assembly = typeof<ReminderEntry>.Assembly
        assembly.GetType("Orleans.Runtime.TypeUtils"),
        assembly.GetType("Orleans.CodeGeneration.GrainInterfaceUtils"),
        assembly.GetType("Orleans.Runtime.GrainClassData"),
        typeof<Orleans.Core.IGrainIdentity>.Assembly (* Orleans.Core.Abstractions.dll *) .GetType("Orleans.Runtime.GrainId")

    let getGenericArgumentsStringFromInterfaceType (interfaceType: Type) =
        orleansTypeUtilsType
            .GetMethod("GenericTypeArgsString", Reflection.BindingFlags.Static ||| Reflection.BindingFlags.Public)
            .Invoke(null, [| interfaceType.UnderlyingSystemType.FullName |])
            :?> string

    let genericArgsStrToLifeCycleAdapter =
        lifeCycleAdapterCollection
        |> Seq.map (fun adapter ->
            let genericArgsStr =
                adapter.SubjectGrainType
                |> getGenericArgumentsStringFromInterfaceType

            (genericArgsStr, adapter)
        )
        |> dict

    let lifeCycleNameToGenericArgsStr =
        genericArgsStrToLifeCycleAdapter
        |> Seq.map (fun kv -> (kv.Value.LifeCycleName, kv.Key))
        |> dict

    // TODO: FIXME-ORLEANS
    // We could've persisted type code in the db upon grain creation, but this is vulnerable to small changes
    // in ecosystem types such as renames or even assembly version change. Also it's the same for all subjects of a type.
    // Unfortunately functions that calculate GrainTypeCode is not a part of public Orleans Api,
    // and reflection-based approach is a bit involved.
    let getGrainTypeCodeFromSubjectGrainType (interfaceType: Type) =

        // Step one: get generic type code based on the implementation type and the interface
        let typeCodeObj =
            // generic impl type def
            let subjectGrainTypeDef: Type = typedefof<SubjectGrain<_, _, _, _, _, _>>
            let typeFullName =
                orleansTypeUtilsType
                    .GetMethod("GetFullName", BindingFlags.Static ||| BindingFlags.Public)
                    .Invoke(null, [| subjectGrainTypeDef |])
                    :?> string
            let grainClassTypeCode =
                orleansGrainInterfaceUtilsType
                    .GetMethod("GetGrainClassTypeCode", BindingFlags.Static ||| BindingFlags.Public)
                    .Invoke(null, [| subjectGrainTypeDef |])
                    :?> int

            let grainClassDataObj =
                orleansGrainClassDataType
                    .GetConstructors(BindingFlags.NonPublic ||| BindingFlags.Instance).[0]
                    .Invoke([|
                        grainClassTypeCode |> box;
                        typeFullName |> box;
                        // "LibLifeCycleHost.SubjectGrain`8" |> box;
                        subjectGrainTypeDef.ContainsGenericParameters |> box |])
            orleansGrainClassDataType
                .GetMethod("GetTypeCode", BindingFlags.NonPublic ||| BindingFlags.Instance)
                .Invoke(grainClassDataObj, [| interfaceType |])

        // Step two: get the final type code data from type code and grain category (key with extension),
        //     it doesn't depend on specific GrainId but it's still the easiest way to obtain it
        let struct(_, _, grainTypeCodeData) =
            let (GrainPartition defaultGrainPartitionGuid) = defaultGrainPartition
            let grainIdObj =
                orleansGrainIdType.GetMethods(BindingFlags.Static ||| BindingFlags.NonPublic)
                    |> Seq.filter (fun m -> m.Name = "GetGrainId" && m.GetParameters().Length = 3 && m.GetParameters().[1].ParameterType = typeof<Guid>)
                    |> Seq.exactlyOne
                    |> fun m ->
                        m.Invoke(null,
                                 [| typeCodeObj
                                    defaultGrainPartitionGuid |> box
                                    "Any SubjectId. TypeCode_DoesNot_DependOnIt" |> box |])
            let grainIdKeyInfo = orleansGrainIdType.GetMethod("ToKeyInfo", BindingFlags.NonPublic ||| BindingFlags.Instance).Invoke(grainIdObj, [||]) :?> _
            let keyInfo = Orleans.Serialization.GrainReferenceKeyInfo(grainIdKeyInfo, getGenericArgumentsStringFromInterfaceType interfaceType)
            keyInfo.KeyAsGuidWithExt()
        grainTypeCodeData

    let lifeCycleNameToGrainTypeCode : Map<string, uint64> =
        lifeCycleAdapterCollection
        |> Seq.map (fun adapter -> adapter.LifeCycleName, getGrainTypeCodeFromSubjectGrainType adapter.SubjectGrainType)
        |> Map.ofSeq

    let readAllRowsSql =
        let persistentAdapters =
            lifeCycleAdapterCollection
            |> Seq.where(fun adapter -> match adapter.Storage with | StorageType.Persistent _ -> true | _ -> false)
        let hasPersistentAdapters =
            persistentAdapters
            |> Seq.isNonempty
        match hasPersistentAdapters with
        | true ->
            let unionedSql =
                persistentAdapters
                |> Seq.map (fun adapter ->
                    if adapter.LifeCycleName <> MetaLifeCycleName then
                        sprintf "SELECT Id, '%s' AS LifeCycleName, NextTickOn, GrainIdHash FROM [%s].[%s] (NOLOCK) WHERE NextTickOn IS NOT NULL AND NextTickFired = 0 AND NextTickOn <= @maxNextTickOn"
                            adapter.LifeCycleName ecosystem.Name adapter.LifeCycleName
                    else
                        // read all rows for Meta, for keep alive tick
                        sprintf "SELECT Id, '%s' AS LifeCycleName, NextTickOn, GrainIdHash FROM [%s].[%s] (NOLOCK)"
                            adapter.LifeCycleName ecosystem.Name adapter.LifeCycleName)
                |> String.concat "\nUNION ALL\n"
            Some unionedSql
        | false -> None

    let initialBucketQueryTimestamp = ConcurrentDictionary<(* begin' *) uint32 * (* end' *) uint32, DateTimeOffset>()

    // TODO: eliminate ticks of subjects in Prepared (in transaction) state somehow. Not cheap to just look up _Prepared table.

    // TODO: don't read all rows, read TOP N ordered by NextTickOn (but first make sure SQL indices used well!),
    // this will self-regulate scenarios when system finds too many overdue reminders (e.g. after downtime) and then chokes on them
    // optimal top N = subjectReminderTableLookAheadLimit.TotalMilliseconds / (optimal-ms-between-overdue-reminders * ringOptions.NumVirtualBucketsConsistentRing)
    let readAllRowsSqlWithinRange =
        readAllRowsSql
        |> Option.map (sprintf "SELECT Id, LifeCycleName, NextTickOn FROM (%s) a WHERE GrainIdHash > @begin AND GrainIdHash <= @end")

    let readAllRowsSqlOutsideRange =
        readAllRowsSql
        |> Option.map (sprintf "SELECT Id, LifeCycleName, NextTickOn FROM (%s) a WHERE GrainIdHash > @begin OR GrainIdHash <= @end")

    let ensureGrainReferenceIsSubject (grainRef: GrainReference) =
        let keyInfo = grainRef.ToKeyInfo()
        if keyInfo.HasGenericArgument && genericArgsStrToLifeCycleAdapter.ContainsKey keyInfo.GenericArgument then
            Noop
        else
            failwithf "Grain %A isn't supported by this implementation of Reminder Table, only subject grains are supported"
                grainRef

    // arbitrary date in the past i.e. due immediately, ticks every minute, random second start to avoid spikes of Meta reminders
    let rnd = Random()
    let metaKeepAliveReminderStartAt () = DateTimeOffset(2010, 1, 1, 0, 0, rnd.Next(0, 59), TimeSpan.Zero)

    interface IReminderTable with
        member this.Init() : Task =
            Task.CompletedTask

        member this.ReadRow(grainRef: GrainReference, reminderName: string): Task<ReminderEntry> =
            // TODO -- if we use multiple perisistent providers, we need to move this into the provider's interface
            if reminderName = SubjectReminderName then
                ensureGrainReferenceIsSubject grainRef

                let adapter = genericArgsStrToLifeCycleAdapter.[grainRef.ToKeyInfo().GenericArgument]
                let sql = sprintf "SELECT (CASE WHEN NextTickFired = 0 THEN NextTickOn ELSE NULL END) FROM [%s].[%s] WHERE Id = @id" ecosystem.Name adapter.LifeCycleName
                let (_grainPartition, pKey) = grainRef.GetPrimaryKey()

                backgroundTask {
                    use connection = new SqlConnection(connectionString)
                    do! connection.OpenAsync()
                    use command = connection.CreateCommand()
                    command.CommandText <- sql
                    command.Parameters.Add("@id", SqlDbType.NVarChar).Value <- pKey
                    match! command.ExecuteScalarAsync() with
                    | null
                    | :? DBNull ->
                        return null
                    | nextTickOn ->
                        return newReminderEntry SubjectReminderName grainRef (nextTickOn :?> DateTimeOffset)
                }
            elif reminderName = MetaKeepAliveReminderName then
                let adapter = genericArgsStrToLifeCycleAdapter.[grainRef.ToKeyInfo().GenericArgument]
                if adapter.LifeCycleName <> MetaLifeCycleName then
                    null
                else
                    newReminderEntry MetaKeepAliveReminderName grainRef (metaKeepAliveReminderStartAt ())
                |> Task.FromResult
            else
                failwithf "Reminder with name %s is not supported by this implementation of Reminder Table" reminderName

        member this.ReadRows(grainRef: GrainReference): Task<ReminderTableData> =
            backgroundTask {
                let! reminder1 = (this :> IReminderTable).ReadRow(grainRef, SubjectReminderName)
                let! reminder2 = (this :> IReminderTable).ReadRow(grainRef, MetaKeepAliveReminderName)
                return ReminderTableData([reminder1; reminder2] |> Seq.filter (fun r -> r <> null))
            }

        member this.ReadRows(begin': uint32, end': uint32): Task<ReminderTableData> =
            // NOTE that begin can actually be higher than end, in which case, you're expected to return
            // all rows where hash > begin or hash < end
            // i.e. this is a "ring range", so think of it as a set of circular segments within the int32 space,
            // as opposed to linear segments

            let maybeSql = if begin' < end' then readAllRowsSqlWithinRange else readAllRowsSqlOutsideRange

            match maybeSql with
            | Some sql ->
                backgroundTask {
                    use connection = new SqlConnection(connectionString)
                    do! connection.OpenAsync()
                    use command = connection.CreateCommand()

                    let! nowBeforeQuery = clock.Query Now
                    let maxNextTickOn = nowBeforeQuery.Add subjectReminderTableLookAheadLimit

                    command.CommandText <- sql
                    command.Parameters.Add("@begin", SqlDbType.Int).Value <- SqlServerGrainStorageHandler.uint32ToInt32MaintainOrder begin'
                    command.Parameters.Add("@end",   SqlDbType.Int).Value <- SqlServerGrainStorageHandler.uint32ToInt32MaintainOrder end'
                    command.Parameters.Add("@maxNextTickOn", SqlDbType.DateTimeOffset).Value <- maxNextTickOn

                    use! reader = command.ExecuteReaderAsync()
                    let! listOfRawReminderEntryArgsSortedByNextTickOn =
                        AsyncSeq.unfoldAsync (
                            fun _ ->
                                async {
                                    match! reader.ReadAsync() |> Async.AwaitTask with
                                    | true ->
                                        let pKey = reader.GetString(0)
                                        let lifeCycleName = reader.GetString(1)
                                        let grainTypeCode = lifeCycleNameToGrainTypeCode.[lifeCycleName]
                                        let genericArgs = lifeCycleNameToGenericArgsStr.[lifeCycleName]
                                        let (GrainPartition defaultGrainPartitionGuid) = defaultGrainPartition
                                        let grainReference =
                                            GrainReferenceKeyInfo.KeyFromGuidWithExt(defaultGrainPartitionGuid, pKey, grainTypeCode)
                                            |> fun key -> GrainReferenceKeyInfo(key, genericArgs)
                                            |> grainReferenceConverter.GetGrainFromKeyInfo
                                        let maybeSubjectReminder =
                                            if (not <| reader.IsDBNull 2) then
                                                let nextTickOn = reader.GetDateTimeOffset(2)
                                                 // double check maxNextTickOn because Meta LC SQL query is not controlled for it
                                                if nextTickOn <= maxNextTickOn then
                                                    Some (SubjectReminderName, grainReference, nextTickOn)
                                                else
                                                    None
                                            else
                                                None
                                        let maybeMetaKeepAliveReminder =
                                            if lifeCycleName = MetaLifeCycleName then
                                                Some (MetaKeepAliveReminderName, grainReference, metaKeepAliveReminderStartAt())
                                            else
                                                None
                                        return Some([maybeSubjectReminder; maybeMetaKeepAliveReminder] |> List.choose id, Nothing)
                                    | false ->
                                        return None
                                }) Nothing
                        |> AsyncSeq.toListAsync
                        |> Async.Map (
                            List.collect id
                            // consider ORDER BY on SQL side if this is slow
                            >> List.sortBy (fun (_, _, nextTickOn) -> nextTickOn))
                        |> Async.StartAsTask

                    let listLength = listOfRawReminderEntryArgsSortedByNextTickOn.Length
                    if listLength > 1000 then // warn if many ticks
                        logger.LogWarning("Loaded {0} ticks for bucket {1}/{2}", listLength, begin', end')
                    else
                        logger.Info("Loaded {0} ticks for bucket {1}/{2}", listOfRawReminderEntryArgsSortedByNextTickOn.Length, begin', end')

                    // Overdue reminders need to be overridden to fire "asap": if unchanged they will get delayed, here's how:
                    // for example a timer that was due 7 seconds ago will skip that tick and will fire in -7 sec + 1 minute period = 53 seconds from now.
                    // To prevent too many "asap" reminders firing the moment reminder service refreshes (including silo start up) we spread out all overdue ticks.
                    // It mostly matters for dev where system can be offline for many days and accumulate many overdue reminders that would choke the system if fired together,
                    // prod however rarely falls much behind

                    let initialBucketQuery =
                        // consider queries to the bucket within first few minutes as "initial" to decrease chance of a late tick while cluster is stabilizing
                        // attempts to do it only for the first query resulted in some late ticks during deployment
                        initialBucketQueryTimestamp.TryGetValue ((begin', end'))
                        |> function | true, timestamp -> nowBeforeQuery < timestamp.AddMinutes 16. | false, _ -> true

                    let! nowAfterQuery = clock.Query Now |> Async.AwaitTask
                    let earliestNewDueReminderStartAt = nowAfterQuery.Add subjectReminderTableNewDueReminderMinDelay

                    let listOfAmendedReminderEntryArgs =
                        let spreadMilliseconds =
                            // what's the good spreadMilliseconds interval?
                            if isDevHost then
                                // for dev host, ~15 milliseconds between subsequent overdue reminders _for all buckets_ on single silo
                                // is a good middle ground to process them fast enough and not choke (here we deal only with one bucket).
                                15 * ringOptions.Value.NumVirtualBucketsConsistentRing // there's 30 buckets by default i.e. 15 x 30 = 450
                            else
                                // on prod, pack them more tightly to avoid unnecessary overdue reminders
                                40
                        listOfRawReminderEntryArgsSortedByNextTickOn |>
                        List.fold (fun (acc, newDueRemindersCount) args ->
                            match args with
                            | reminderName, _, _ when reminderName = MetaKeepAliveReminderName ->
                                args :: acc, newDueRemindersCount
                            | reminderName, grainRef, nextTickOn ->
                                let startAt, newDueRemindersCount =
                                    if (subjectReminderTableIsNewDueReminderBestGuess initialBucketQuery nowAfterQuery nextTickOn) then
                                        earliestNewDueReminderStartAt + ((spreadMilliseconds * int newDueRemindersCount) |> TimeSpan.FromMilliseconds),
                                        (newDueRemindersCount + 1u)
                                    else
                                        nextTickOn + subjectReminderImplicitDelayToReduceEarlyTicks, newDueRemindersCount
                                (reminderName, grainRef, startAt) :: acc, newDueRemindersCount)
                            ([], 0u)
                        |> fst
                        |> List.rev

                    initialBucketQueryTimestamp.TryAdd((begin', end'), nowAfterQuery) |> ignore

                    return
                        listOfAmendedReminderEntryArgs
                        |> Seq.map (fun (reminderName, grainRef, startAt) -> newReminderEntry reminderName grainRef startAt)
                        |> ReminderTableData
                }
            | None -> Task.FromResult(new ReminderTableData())


        member this.RemoveRow(grainRef: Runtime.GrainReference, _reminderName: string, _eTag: string): Task<bool> =
            ensureGrainReferenceIsSubject grainRef

            // NO-OP. We will never do any writes from the Reminder system; all reminder peristence
            // is handled by SubjectGrain when it configures the nextTick as part of subject upgrade
            Task.FromResult true

        member this.TestOnlyClearTable(): Task =
            Task.CompletedTask

        member this.UpsertRow(entry: ReminderEntry): Task<string> =
            ensureGrainReferenceIsSubject entry.GrainRef

            // NO-OP. We will never do any writes from the Reminder system; all reminder peristence
            // is handled by SubjectGrain when it configures the nextTick as part of subject upgrade
            Guid.NewGuid()
            |> sprintf "%O"
            |> Task.FromResult