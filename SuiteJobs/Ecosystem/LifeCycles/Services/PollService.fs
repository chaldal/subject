[<AutoOpen>]
module SuiteJobs.LifeCycles.Services.PollService

open System
open System.Data
open Microsoft.Data.SqlClient
open System.Threading.Tasks
open FSharp.Control
open LibLifeCycle
open Microsoft.Extensions.Configuration
open SuiteJobs.Types

type PollService(
    configuration: IConfiguration,
    allJobs: Service<All<Job, JobId, JobIndex, JobOpError>>) =
    let sectionName  = "Storage.SqlServer"
    let parameter    = "ConnectionString"
    let configSection = configuration.GetSection sectionName
    let maybeConnectionString =
        match configSection.Exists () with
        | false -> None
        | true -> configSection[parameter] |> Option.ofObj

    let pollFasterAndLessRacyViaRawSqlBecauseYouCan (connectionString: string) (spotsLeft: int) (queues: NonemptySet<NonemptyString>) : Task<List<JobId>> =
        backgroundTask {
        try
            use connection = new SqlConnection(connectionString)
            do! connection.OpenAsync()
            use command = connection.CreateCommand()

            let queueParams =
                queues.ToSet |> Seq.mapi (fun i queue -> {| Name = $"@queue%d{i}"; Value = queue.Value |}) |> Seq.toList

            let sql =
                String.Format (
                @"SELECT eqt.SubjectId, eqt.ValueStr
    FROM Jobs.Job_Index_EnqueuedTo eqt
    WHERE
        eqt.PromotedValue IN ({0})
        AND eqt.[Key] = @dequeueSortKey
        AND eqt.SubjectId NOT IN (SELECT di.ValueStr FROM Jobs.Dispatcher_Index di WHERE di.[Key] = @jobKey)
    ORDER BY eqt.ValueStr
    OFFSET 0 ROWS
    FETCH NEXT @spotsLeft ROWS ONLY",
                String.Join (", ", queueParams |> Seq.map (fun p -> p.Name)))

            command.CommandText <- sql
            command.Parameters.AddWithValue("@dequeueSortKey", (UnionCase.ofCase JobStringIndex.DequeueSort).CaseName).SqlDbType <- SqlDbType.VarChar
            command.Parameters.AddWithValue("@jobKey", (UnionCase.ofCase DispatcherStringIndex.Job).CaseName).SqlDbType <- SqlDbType.VarChar
            command.Parameters.AddWithValue("@spotsLeft", spotsLeft).SqlDbType <- SqlDbType.Int
            queueParams
            |> List.iter (fun p -> command.Parameters.AddWithValue(p.Name, p.Value).SqlDbType <- SqlDbType.VarChar)

            use! cursor = command.ExecuteReaderAsync()
            let! jobIds =
                AsyncSeq.unfoldAsync (
                    fun _ ->
                        async {
                            match! cursor.ReadAsync() |> Async.AwaitTask with
                            | true ->
                                let jobIdStr = cursor.GetString 0
                                return Some (JobId jobIdStr, Nothing)
                            | false ->
                                return None
                        }
                ) Nothing
                |> AsyncSeq.toListAsync
                |> Async.StartAsTask
            return jobIds
        with
        | ex ->
            return TransientSubjectException("Jobs PollService", ex.ToString()) |> raise
    }

    let pollSlowerAndMoreRacyViaStandardIndexQuery (spotsLeft: int) (queues: NonemptySet<NonemptyString>) : Task<List<JobId>> =
        // Normal query can't join both Job index and Dispatcher index tables to reduce risk of races,
        // so it naturally leads to more retries / worse performance
        queues.ToSet
        |> Seq.map (JobStringIndex.EnqueuedTo >> EqualToString)
        |> Seq.toList
        |> NonemptyList.ofListUnsafe
        |> IndexPredicate.orList
        |> JobIndex.PrepareQuery
               {
                   Page = { Size = uint16 spotsLeft; Offset = 0UL }
                   OrderBy = OrderBy.StringIndexEntry (UnionCase.ofCase JobStringIndex.DequeueSort, OrderDirection.Ascending)
               }
       |> allJobs.Query FilterFetchIds

    member _.Poll (spotsLeft: int) (queues: NonemptySet<NonemptyString>) : Task<List<JobId>> =
        // if there's SQL backend then take advantage of it, otherwise use slower index query (e.g. for tests)
        match maybeConnectionString with
        | Some connectionString ->
            pollFasterAndLessRacyViaRawSqlBecauseYouCan connectionString spotsLeft queues
        | None ->
            pollSlowerAndMoreRacyViaStandardIndexQuery spotsLeft queues