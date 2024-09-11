module LibLifeCycleHost.Storage.SqlServer.SqlServerSetup

open System
open System.Reflection

open System.IO
open System.Data
open Microsoft.Data.SqlClient
open System.Security.Cryptography
open LibLifeCycleHost
open LibLifeCycle

let private sqlStorageSqlServerNamespace = "LibLifeCycleHost.Storage.SqlServer"

let private readFromManifestResourceStream (name: string) =
    use stream = Assembly.GetExecutingAssembly().GetManifestResourceStream name
    use streamReader = new StreamReader(stream, System.Text.Encoding.UTF8)
    streamReader.ReadToEnd()

let private getSqlFile (fileName: string) =
    readFromManifestResourceStream (sprintf "%s.%s" sqlStorageSqlServerNamespace fileName)

let private getAllAutoUpgradeSqlResourceNames () =
    let prefix = sprintf "%s.Upgrades_Auto." sqlStorageSqlServerNamespace
    Assembly.GetExecutingAssembly().GetManifestResourceNames()
    |> Seq.filter (fun name -> name.StartsWith prefix)
    |> Seq.sort
    |> Seq.map (fun name -> {| FullName = name; ShortName = name.Substring prefix.Length |})
    |> List.ofSeq

let createDataProtectionStore (config: SqlServerConfiguration) (hostEcosystemName: string) =
    // TODO: is one per host enough or should be one per ecosystem somehow?
    let sql = (getSqlFile "CreateDataProtectionStore.sql").Replace("___SCHEMA_NAME___", hostEcosystemName)
    use connection = new SqlConnection(config.ConnectionString)
    connection.Open()
    use command = new SqlCommand(sql, connection)
    command.CommandText <- sql
    command.ExecuteNonQuery() |> ignore

let createSchema (config: SqlServerConnectionStrings) =
    config.ByEcosystemName.ToMap
    |> Map.iter (fun ecosystemName connString ->
        let sql =
            (getSqlFile "CreateSchema.sql")
                .Replace("___SCHEMA_NAME___", ecosystemName)
        use connection = new SqlConnection(connString)
        connection.Open()
        use command = new SqlCommand(sql, connection)
        command.CommandText <- sql
        command.ExecuteNonQuery() |> ignore)

let resetMembershipTable
        (membershipConnectionString: string)
        (targetEcosystem: string) =
    use connection = new SqlConnection(membershipConnectionString)
    connection.Open()
    use command = new SqlCommand($"DELETE FROM [OrleansMembershipTable] WHERE DeploymentId = @ecosystemName", connection)
    command.Parameters.Add("@ecosystemName", SqlDbType.NVarChar).Value <- targetEcosystem
    command.ExecuteNonQuery() |> ignore

let runOrleansSetup (membershipConnectionString: string) =
    let sql = getSqlFile "SetupOrleans.sql"
    use connection = new SqlConnection(membershipConnectionString)
    connection.Open()
    use command = new SqlCommand(sql, connection)
    command.ExecuteNonQuery() |> ignore

let private createSchemaUpgradeTable (connection: SqlConnection) (ecosystemName: string) =
    let sql = (getSqlFile "CreateSchemaUpgradeTable.sql") .Replace("___SCHEMA_NAME___", ecosystemName)
    use command = new SqlCommand(sql, connection)
    command.ExecuteNonQuery() |> ignore

let private createScriptHashTable (connection: SqlConnection) (ecosystemName: string) =
    let sql = (getSqlFile "CreateScriptHashTable.sql").Replace("___SCHEMA_NAME___", ecosystemName)
    use command = new SqlCommand(sql, connection)
    command.ExecuteNonQuery() |> ignore

let private createApplyScriptIfNewOrChangedProc (connection: SqlConnection) (ecosystemName: string) =
    let sql = (getSqlFile "ApplyScriptIfNewOrChanged.sql").Replace("___SCHEMA_NAME___", ecosystemName)
    use command = new SqlCommand(sql, connection)
    command.ExecuteNonQuery() |> ignore

type private SetupContext = {
    Connection: SqlConnection
    SchemaName: string
    LifeCycleAdapters: list<IHostedLifeCycleAdapter>
    AlreadyAppliedScriptHashes: Map<string, byte[]>
    HashAlgo1: HashAlgorithm
    HashAlgo2: HashAlgorithm
}

let private applyScriptIfNewOrChanged (context: SetupContext) (scriptName: string) (sql: string) =

    let hash1 = context.HashAlgo1.ComputeHash(System.Text.Encoding.UTF8.GetBytes sql)
    let hash2 = context.HashAlgo2.ComputeHash(System.Text.Encoding.UTF8.GetBytes sql)
    let hash = Array.concat [ hash1; hash2 ]

    let isNewOrNotApplied =
        match context.AlreadyAppliedScriptHashes.TryFind scriptName with
        | None -> true
        | Some alreadyAppliedHash ->
            alreadyAppliedHash.Length <> hash.Length || (Seq.zip alreadyAppliedHash hash |> Seq.exists (fun (b1, b2) -> b1 <> b2))
    if isNewOrNotApplied then
        use command = new SqlCommand(sprintf "[%s].[__ApplyScriptIfNewOrChanged]" context.SchemaName, context.Connection)
        command.CommandType <- CommandType.StoredProcedure
        command.Parameters.Add("@scriptName", SqlDbType.VarChar).Value <- scriptName
        command.Parameters.Add("@sql", SqlDbType.NVarChar).Value <- sql
        command.Parameters.Add("@hash", SqlDbType.VarBinary).Value <- hash
        command.ExecuteNonQuery() |> ignore

let private createTypes (context: SetupContext) =
    let sql = (getSqlFile "CreateTypes.sql").Replace("___SCHEMA_NAME___", context.SchemaName)
    applyScriptIfNewOrChanged context "CreateTypes.sql" sql

let private createTransferBlobTable (context: SetupContext) =
    let sql = (getSqlFile "CreateTransferBlobTable.sql").Replace("___SCHEMA_NAME___", context.SchemaName)
    applyScriptIfNewOrChanged context "CreateTransferBlobTable.sql" sql

let private createTablesForLifeCycle (context: SetupContext) (hasSearchIndex: bool) (hasGeographyIndex: bool) (lifeCycleName: string) =
    let sql =
        (getSqlFile "CreateTables.sql")
            .Replace("___SCHEMA_NAME___", context.SchemaName)
            .Replace("___LIFECYCLE_NAME___", lifeCycleName)
    let scriptName = sprintf "%s_CreateTables.sql" lifeCycleName
    applyScriptIfNewOrChanged context scriptName sql

    if hasSearchIndex then
        let sql =
            (getSqlFile "CreateSearchIndexTable.sql")
                .Replace("___SCHEMA_NAME___", context.SchemaName)
                .Replace("___LIFECYCLE_NAME___", lifeCycleName)
        let scriptName = sprintf "%s_CreateSearchIndexTable.sql" lifeCycleName
        applyScriptIfNewOrChanged context scriptName sql

    if hasGeographyIndex then
        let sql =
            (getSqlFile "CreateGeographyIndexTable.sql")
                .Replace("___SCHEMA_NAME___", context.SchemaName)
                .Replace("___LIFECYCLE_NAME___", lifeCycleName)
        let scriptName = sprintf "%s_CreateGeographyIndexTable.sql" lifeCycleName
        applyScriptIfNewOrChanged context scriptName sql

let private createTablesAndProcsForTimeSeries (context: SetupContext) (timeSeriesName: string) =
    [
        "CreateTimeSeriesTables.sql"
        "SaveTimeSeries.sql"
    ]
    |> List.iter (fun scriptName ->
        let sql =
            (getSqlFile scriptName)
                .Replace("___SCHEMA_NAME___", context.SchemaName)
                .Replace("___TIMESERIES_NAME___", timeSeriesName)
        let scriptName = $"%s{timeSeriesName}_%s{scriptName}"
        applyScriptIfNewOrChanged context scriptName sql)

let private createPromotedIndicesTablesForLifeCycle (context: SetupContext) (lifeCycleName: string) (promotedIndicesConfig: PromotedIndicesConfig) =
    let sql = getSqlFile "CreatePromotedIndexTable.sql"
    promotedIndicesConfig.Mappings
    |> Map.iter (fun (PromotedKey indexName) _ ->
        let sql =
            sql
                .Replace("___SCHEMA_NAME___", context.SchemaName)
                .Replace("___LIFECYCLE_NAME___", lifeCycleName)
                .Replace("___INDEX_NAME___", indexName)
        let scriptName = sprintf "%s_%s_CreatePromotedIndexTable.sql" lifeCycleName indexName
        applyScriptIfNewOrChanged context scriptName sql
    )

let private createVariousProcsForLifeCycle
    (context: SetupContext)
    (lifeCycleName: string)
    (hasSearchIndex: bool)
    (hasGeographyIndex: bool)
    (promotedIndicesConfig: PromotedIndicesConfig) =

    let upsertStateSharedSql =
        let upsertAllPromotedIndicesSql =
            let upsertPromotedIndexSql = (getSqlFile "UpsertStateShared_UpsertPromotedIndex.sql")
            promotedIndicesConfig.Mappings
            |> Map.map (fun (PromotedKey promotedKey) _ ->
                upsertPromotedIndexSql.Replace("___PROMOTED_KEY___", promotedKey)
            ) |> Map.values |> String.concat ""
        let upsertSearchIndexSql = if hasSearchIndex then getSqlFile "UpsertStateShared_UpsertSearchIndex.sql" else ""
        let upsertGeographyIndexSql = if hasGeographyIndex then getSqlFile "UpsertStateShared_UpsertGeographyIndex.sql" else ""

        (getSqlFile "UpsertStateShared.sql")
            .Replace("___UPSERT_SEARCH_INDEX_SQL___", upsertSearchIndexSql)
            .Replace("___UPSERT_GEOGRAPHY_INDEX_SQL___", upsertGeographyIndexSql)
            .Replace("___UPSERT_ALL_PROMOTED_INDICES_SQL___", upsertAllPromotedIndicesSql)
            .Replace("___SCHEMA_NAME___", context.SchemaName)
            .Replace("___LIFECYCLE_NAME___", lifeCycleName)

    let prepareStateSharedSql =
        (getSqlFile "PrepareStateShared.sql")
            .Replace("___SCHEMA_NAME___", context.SchemaName)
            .Replace("___LIFECYCLE_NAME___", lifeCycleName)

    let rollbackStateSharedSql =
        (getSqlFile "RollbackStateShared.sql")
            .Replace("___SCHEMA_NAME___", context.SchemaName)
            .Replace("___LIFECYCLE_NAME___", lifeCycleName)

    let deleteSearchIndexSql =
        if hasSearchIndex then getSqlFile "ClearState_DeleteSearchIndex.sql" else ""
    let deleteGeographyIndexSql =
        if hasGeographyIndex then getSqlFile "ClearState_DeleteGeographyIndex.sql" else ""

    let deletePromotedIndicesSql =
        promotedIndicesConfig.Mappings
        |> Map.map (fun (PromotedKey promotedKey) _ ->
            $"DELETE FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Index_%s{promotedKey}] WHERE SubjectId = @id;
"
        ) |> Map.values |> String.concat ""

    [ "InitializeState.sql"
      "UpdateState.sql"
      "SetTickState.sql"
      "EnqueueAction.sql"
      "PrepareInitialize.sql"
      "RollbackPreparedInitialize.sql"
      "PrepareUpdate.sql"
      "RollbackPreparedUpdate.sql"
      "ClearState.sql"
      "SaveSubscriptions.sql"
      "SetTickStateAndSubscriptions.sql"
      "RetryPermanentFailures.sql"
      "ReEncode.sql"
      "ReEncodeHistory.sql"
      "ClearExpiredHistoryBatch.sql"
      "RepairGrainIdHash.sql"]
    |> Seq.iter (fun file ->
        let sql =
            (getSqlFile file)
                .Replace("___DELETE_SEARCH_INDEX_SQL___", deleteSearchIndexSql)
                .Replace("___DELETE_GEOGRAPHY_INDEX_SQL___", deleteGeographyIndexSql)
                .Replace("___DELETE_PROMOTED_INDICES_SQL___", deletePromotedIndicesSql)
                .Replace("___SCHEMA_NAME___", context.SchemaName).Replace("___LIFECYCLE_NAME___", lifeCycleName)
                .Replace("___UPSERT_STATE_SHARED___", upsertStateSharedSql)
                .Replace("___PREPARE_STATE_SHARED___", prepareStateSharedSql)
                .Replace("___ROLLBACK_STATE_SHARED___", rollbackStateSharedSql)

        let scriptName = sprintf "%s_%s" lifeCycleName file
        applyScriptIfNewOrChanged context scriptName sql)

let private createRebuildIndicesProcForLifeCycle
    (context: SetupContext)
    (lifeCycleName: string)
    (hasSearchIndex: bool)
    (hasGeographyIndex: bool)
    (promotedIndicesConfig: PromotedIndicesConfig) =

    let rebuildAllPromotedIndicesSql =
        let rebuildPromotedIndexSql = (getSqlFile "RebuildIndices_PromotedIndex.sql").Replace("___SCHEMA_NAME___", context.SchemaName).Replace("___LIFECYCLE_NAME___", lifeCycleName)
        promotedIndicesConfig.Mappings
        |> Map.map (fun (PromotedKey promotedKey) _ ->
            rebuildPromotedIndexSql.Replace("___PROMOTED_KEY___", promotedKey)
        ) |> Map.values |> String.concat ""

    let rebuildSearchIndicesSql =
        if hasSearchIndex then (getSqlFile "RebuildIndices_SearchIndex.sql").Replace("___SCHEMA_NAME___", context.SchemaName).Replace("___LIFECYCLE_NAME___", lifeCycleName) else ""
    let rebuildGeographyIndicesSql =
        if hasGeographyIndex then (getSqlFile "RebuildIndices_GeographyIndex.sql").Replace("___SCHEMA_NAME___", context.SchemaName).Replace("___LIFECYCLE_NAME___", lifeCycleName) else ""

    let sql =
        (getSqlFile "RebuildIndices.sql").Replace("___SCHEMA_NAME___", context.SchemaName).Replace("___LIFECYCLE_NAME___", lifeCycleName)
            .Replace("___REBUILD_ALL_PROMOTED_INDICES_SQL___", rebuildAllPromotedIndicesSql)
            .Replace("___REBUILD_SEARCH_INDICES_SQL___", rebuildSearchIndicesSql)
            .Replace("___REBUILD_GEOGRAPHY_INDICES_SQL___", rebuildGeographyIndicesSql)

    let scriptName = sprintf "%s_RebuildIndices.sql" lifeCycleName
    applyScriptIfNewOrChanged context scriptName sql

let private createFullTextCatalog (context: SetupContext) =
    let sql = (getSqlFile "CreateFullTextCatalog.sql").Replace("___SCHEMA_NAME___", context.SchemaName)
    applyScriptIfNewOrChanged context "CreateFullTextCatalog.sql" sql

let private sequenceProcs (context: SetupContext) =
    ["SequenceNextValue.sql"; "SequenceCurrentValue.sql"]
    |> List.iter (fun scriptName ->
        let sql = (getSqlFile scriptName).Replace("___SCHEMA_NAME___", context.SchemaName)
        applyScriptIfNewOrChanged context scriptName sql)

let private applySchemaUpgrades (connection: SqlConnection) (schemaName: string) (lifeCycles: list<IHostedLifeCycleAdapter>) =

    use transaction = connection.BeginTransaction()

    let autoUpgradeNames = getAllAutoUpgradeSqlResourceNames ()

    let schemaInitialized =
        let sql = sprintf "SELECT SchemaInitialized FROM [%s].[__SchemaUpgrade]" schemaName
        use command = new SqlCommand(sql, transaction.Connection, transaction)
        let schemaInitializedObj = command.ExecuteScalar ()
        if schemaInitializedObj = null then
            failwith "__SchemaUpgrade table expected to have a row but has none! Can't setup ecosystem SQL schema"
        else
            (schemaInitializedObj |> Convert.ToInt32) = 1

    if schemaInitialized then
        // schema was initialized before, check if any upgrades need to be applied
        let lastAppliedUpgradeScriptName =
            let sql = sprintf "SELECT LastUpgradeScriptName FROM [%s].[__SchemaUpgrade]" schemaName
            use command = new SqlCommand(sql, transaction.Connection, transaction)
            (command.ExecuteScalar () |> Convert.ToString)

        let notAppliedUpgrades = autoUpgradeNames |> List.filter (fun x -> x.ShortName > lastAppliedUpgradeScriptName)
        if notAppliedUpgrades.IsEmpty then
            // all upgrades already applied nothing to do
            ()
        else
            let preExistingTables =
                let sql = sprintf "SELECT TABLE_NAME FROM [INFORMATION_SCHEMA].[TABLES] WHERE TABLE_SCHEMA = '%s'" schemaName
                use command = new SqlCommand(sql, transaction.Connection, transaction)
                use reader = command.ExecuteReader ()
                reader |> List.unfold (fun reader -> if reader.Read() then Some (reader.GetString 0, reader) else None) |> Set.ofList

            for upgrade in notAppliedUpgrades do
                let sql = (readFromManifestResourceStream upgrade.FullName).Replace("___SCHEMA_NAME___", schemaName)
                // upgrade script can be either per life cycle or one for schema
                if sql.Contains "___LIFECYCLE_NAME___" then
                    for lifeCycle in lifeCycles do
                        match lifeCycle.Storage with
                        // upgrade only if life cycle was already deployed before to avoid SQL errors
                        | StorageType.Persistent _ when preExistingTables.Contains lifeCycle.LifeCycleName ->
                            let sql = sql.Replace("___LIFECYCLE_NAME___", lifeCycle.LifeCycleName)
                            use command = new SqlCommand(sql, transaction.Connection, transaction)
                            command.ExecuteNonQuery() |> ignore
                        | _ ->
                            ()
                else
                    use command = new SqlCommand(sql, transaction.Connection, transaction)
                    command.ExecuteNonQuery() |> ignore

                // mark script applied
                let sql =
                    sprintf "UPDATE [%s].[__SchemaUpgrade] SET SchemaInitialized = 1, LastUpgradeScriptName = '%s'"
                        schemaName upgrade.ShortName
                use command = new SqlCommand(sql, transaction.Connection, transaction)
                command.ExecuteNonQuery() |> ignore
    else
        // schema was initialized for the first time means it'll be up to date. Just pretend that latest upgrade was applied
        // so it's not actually applied after a restart
        let lastUpgradeScriptName = autoUpgradeNames |> List.tryLast |> Option.map (fun x -> x.ShortName) |> Option.defaultValue ""
        let sql =
            sprintf "UPDATE [%s].[__SchemaUpgrade] SET SchemaInitialized = 1, LastUpgradeScriptName = '%s'"
                schemaName lastUpgradeScriptName
        use command = new SqlCommand(sql, transaction.Connection, transaction)
        command.ExecuteNonQuery() |> ignore

    transaction.Commit()

let private createDiagnosticsFuncsAndViews (context: SetupContext) =

    let sql = (getSqlFile "Diagnostics.decode.sql").Replace("___SCHEMA_NAME___", context.SchemaName)
    applyScriptIfNewOrChanged context "Diagnostics.decode.sql" sql

    let sql = (getSqlFile "Diagnostics.encode.sql").Replace("___SCHEMA_NAME___", context.SchemaName)
    applyScriptIfNewOrChanged context "Diagnostics.encode.sql" sql

    match
        context.LifeCycleAdapters
        |> Seq.choose (fun adapter -> match adapter.Storage with StorageType.Persistent _ -> Some adapter.LifeCycleName | _ -> None)
        |> List.ofSeq
        with
    | [] ->
        ()
    | lifeCycleNames ->

        let createViewFromTemplate (fileName: string) (viewName: string) =
            let sql =
                let template = getSqlFile $"Diagnostics.{fileName}"
                lifeCycleNames
                |> List.map (fun lifeCycleName -> template.Replace("___LIFECYCLE_NAME___", lifeCycleName))
                |> fun chunks -> String.Join ("\nUNION ALL\n", chunks)
                |> fun body -> $"CREATE OR ALTER VIEW [___SCHEMA_NAME___].[{viewName}] AS\n{body}".Replace("___SCHEMA_NAME___", context.SchemaName)
            let scriptName = sprintf "CreateView_%s.sql" viewName
            applyScriptIfNewOrChanged context scriptName sql

        createViewFromTemplate "FailedSideEffects.sql"  "AllFailedSideEffects"
        createViewFromTemplate "StalledSideEffects.sql" "AllStalledSideEffects"
        createViewFromTemplate "StalledPrepared.sql"    "AllStalledPrepared"
        // in TestDataSeeding co-hosting scenario implemented referenced ecosystems don't have _Meta subj required by this view
        if lifeCycleNames |> Seq.contains MetaLifeCycleName then
            createViewFromTemplate "StalledTimers.sql"      "AllStalledTimers"

let private readAlreadyAppliedScriptHashes (connection: SqlConnection) (ecosystemName: string) : Map<string, byte[]> =
    let sql = sprintf "SELECT ScriptName, [Hash] FROM [%s].[__ScriptHash] WITH (NOLOCK)" ecosystemName
    use command = new SqlCommand (sql, connection)
    use reader = command.ExecuteReader ()
    reader
    |> List.unfold (fun reader ->
        if reader.Read() then
            let scriptName = reader.GetString 0
            let hash = reader.Item 1 :?> byte[]
            Some ((scriptName, hash), reader)
        else None)
    |> Map.ofList


let doSetup
    (config: SqlServerConnectionStrings)
    (allHostedLifeCycleAdapterCollection: HostedLifeCycleAdapterCollection)
    (allTimeSeriesAdapterCollection: TimeSeriesAdapterCollection) =

    allTimeSeriesAdapterCollection |> Seq.map (fun ts -> ts.TimeSeries.Def.TimeSeriesKey.EcosystemName)
    |> Seq.append (
        allHostedLifeCycleAdapterCollection |> Seq.map (fun lc -> lc.LifeCycle.Def.LifeCycleKey.EcosystemName))
    |> Seq.distinct
    |> List.ofSeq
    |> List.iter (fun ecosystemName ->

        let lifeCycles =
            allHostedLifeCycleAdapterCollection
            |> Seq.filter (fun lc -> lc.LifeCycle.Def.LifeCycleKey.EcosystemName = ecosystemName)
            |> List.ofSeq

        let hostedTimeSeries =
            allTimeSeriesAdapterCollection
            |> Seq.filter (fun ts -> ts.TimeSeries.Def.TimeSeriesKey.EcosystemName = ecosystemName)
            |> List.ofSeq

        let connectionString = config.ForEcosystem ecosystemName
        use connection = new SqlConnection(connectionString)
        connection.Open()

        createSchemaUpgradeTable connection ecosystemName
        applySchemaUpgrades connection ecosystemName lifeCycles

        createScriptHashTable connection ecosystemName
        createApplyScriptIfNewOrChangedProc connection ecosystemName

        let context =
            { Connection = connection
              SchemaName = ecosystemName
              LifeCycleAdapters = lifeCycles
              AlreadyAppliedScriptHashes = readAlreadyAppliedScriptHashes connection ecosystemName
              HashAlgo1 = SHA256.Create()
              HashAlgo2 = MD5.Create() }

        createTypes context
        createTransferBlobTable context
        createFullTextCatalog context
        sequenceProcs context

        for lifeCycle in lifeCycles do
            let hasSearchIndex, hasGeographyIndex =
                lifeCycle.LifeCycle.Invoke
                    { new FullyTypedLifeCycleFunction<_> with
                        member _.Invoke (lifeCycle: LifeCycle<_, _, _, _, _, 'SubjectIndex, _, _, _, _, _>) =
                            let hasSearchIndex =
                                ('SubjectIndex.SubjectSearchIndexType <> typeof<NoSearchIndex> &&
                                 lifeCycle.MetaData.IndexKeys |> Seq.exists (function Search key -> key <> "NoIndex" | _ -> false))
                            let hasGeographyIndex =
                                ('SubjectIndex.SubjectGeographyIndexType <> typeof<NoGeographyIndex> &&
                                 lifeCycle.MetaData.IndexKeys |> Seq.exists (function Geography key -> key <> "NoIndex" | _ -> false))
                            hasSearchIndex, hasGeographyIndex }
            match lifeCycle.Storage with
            | StorageType.Persistent (promotedIndicesConfig, _) ->
                createTablesForLifeCycle context hasSearchIndex hasGeographyIndex lifeCycle.LifeCycleName
                createPromotedIndicesTablesForLifeCycle context lifeCycle.LifeCycleName promotedIndicesConfig
                createVariousProcsForLifeCycle context lifeCycle.LifeCycleName hasSearchIndex hasGeographyIndex promotedIndicesConfig
                createRebuildIndicesProcForLifeCycle context lifeCycle.LifeCycleName hasSearchIndex hasGeographyIndex promotedIndicesConfig
            | StorageType.Volatile
            | StorageType.Custom _ ->
                Noop

        for timeSeries in hostedTimeSeries do
            createTablesAndProcsForTimeSeries context timeSeries.TimeSeries.Name

        createDiagnosticsFuncsAndViews context)