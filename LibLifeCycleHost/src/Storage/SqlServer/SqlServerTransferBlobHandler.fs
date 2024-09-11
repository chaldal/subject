module LibLifeCycleHost.Storage.SqlServer.SqlServerTransferBlobHandler

open System
open System.Data
open FSharpPlus
open LibLifeCycle
open LibLifeCycleHost
open Microsoft.Data.SqlClient
open FSharp.Control
open System.Threading.Tasks
open Microsoft.Extensions.Logging
open System.Data.SqlTypes

type SqlServerTransferBlobHandler (config: SqlServerConnectionStrings, ecosystemName: string, clock: Service<Clock>, logger: Microsoft.Extensions.Logging.ILogger<SqlServerTransferBlobHandler>) =
    let sqlConnectionString = config.ForEcosystem ecosystemName

    member _.SqlConnectionString = sqlConnectionString

    interface ITransferBlobHandler with
        member this.Name = "SqlServerTransferBlobHandler"

        member this.GetTransferBlobs transferBlobIds =
            if transferBlobIds.Count = 0 then
                Map.empty |> Task.FromResult
            else
                backgroundTask {
                    use connection = new SqlConnection(sqlConnectionString)
                    do! connection.OpenAsync()

                    let transferBlobIdsList = transferBlobIds |> Set.toList

                    let parameterNames = [1 .. transferBlobIdsList.Length] |> List.map (sprintf "@id%d")

                    let sql = sprintf "SELECT Id, Bytes FROM [%s].[TransferBlob] WHERE Id IN (%s)" ecosystemName (parameterNames |> String.concat ",")
                    use command = new SqlCommand(sql, connection)

                    List.zip transferBlobIdsList parameterNames
                    |> List.iter (fun (blobId, parameterName) -> command.Parameters.Add(parameterName, SqlDbType.UniqueIdentifier).Value <- blobId)

                    use! cursor = command.ExecuteReaderAsync()
                    let! blobs =
                        AsyncSeq.unfoldAsync (
                            fun _ ->
                                async {
                                    match! cursor.ReadAsync() |> Async.AwaitTask with
                                    | true ->
                                        let id = (cursor.Item 0 :?> Guid)
                                        let bytes = (cursor.Item 1 :?> byte[])
                                        return ((id, bytes), Nothing) |> Some
                                    | false ->
                                        return None
                                }
                        ) Nothing
                        |> AsyncSeq.toListAsync
                        |> Async.StartAsTask

                    return blobs |> Map.ofSeq
                }

        member this.StoreBlobsForTransfer transferBlobsByIds =
            backgroundTask {
                let table = new DataTable()
                let! now = clock.Query Now
                table.Columns.Add("Id", typeof<Guid>) |> ignore
                table.Columns.Add("Bytes", typeof<SqlBinary>) |> ignore
                table.Columns.Add("CreatedOn", typeof<DateTimeOffset>) |> ignore

                transferBlobsByIds
                |> Map.toSeq
                |> Seq.iter (fun (id, bytes) ->
                    let row = table.NewRow()
                    row.["Id"] <- id
                    row.["Bytes"] <- SqlBinary(bytes)
                    row.["CreatedOn"] <- now
                    table.Rows.Add row)

                use connection = new SqlConnection(sqlConnectionString)
                use bulkCopy = new SqlBulkCopy(connection)
                bulkCopy.DestinationTableName <- sprintf "[%s].TransferBlob" ecosystemName
                bulkCopy.ColumnMappings.Add("Id", "Id") |> ignore
                bulkCopy.ColumnMappings.Add("Bytes", "Bytes")  |> ignore
                bulkCopy.ColumnMappings.Add("CreatedOn", "CreatedOn")  |> ignore
                do! connection.OpenAsync()
                do! bulkCopy.WriteToServerAsync(table)
            }

        member this.DeleteTransferBlobs transferBlobIds =
            if transferBlobIds.Count = 0 then
                Nothing |> Task.FromResult
            else
                backgroundTask {
                    try
                        use connection = new SqlConnection(sqlConnectionString)
                        do! connection.OpenAsync()

                        let transferBlobIdsList = transferBlobIds |> Set.toList

                        let parameterNames = [1 .. transferBlobIdsList.Length] |> List.map (sprintf "@id%d")

                        let sql = sprintf "DELETE FROM [%s].[TransferBlob] WHERE Id IN (%s)" ecosystemName (parameterNames |> String.concat ",")
                        use command = new SqlCommand(sql, connection)

                        List.zip transferBlobIdsList parameterNames
                        |> List.iter (fun (blobId, parameterName) -> command.Parameters.Add(parameterName, SqlDbType.UniqueIdentifier).Value <- blobId)

                        let! _ = command.ExecuteNonQueryAsync()
                        return Nothing
                    with
                    | ex ->
                        // This will get cleaned up by maintenance
                        logger.LogWarning(ex, "Exception while deleting transfer blob with IDs {ids}", transferBlobIds)
                        return Nothing
                }