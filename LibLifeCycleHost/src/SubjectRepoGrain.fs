namespace LibLifeCycleHost
// Grains can't be in Modules, due to a bug in the way Orleans builds up Grain identity

open LibLifeCycleCore
open LibLifeCycle.DefaultServices
open Orleans
open System
open System.Threading.Tasks
open Orleans.Concurrency
open Microsoft.Extensions.Logging

[<StatelessWorker>]
[<Reentrant>]
type SubjectRepoGrain<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'SubjectIndex, 'OpError
                   when 'Subject      :> Subject<'SubjectId>
                   and  'LifeAction   :> LifeAction
                   and  'Constructor  :> Constructor
                   and  'SubjectIndex :> SubjectIndex<'OpError>
                   and  'SubjectId    :> SubjectId
                   and  'SubjectId    :  comparison
                   and  'OpError      :> OpError>
        (
            repo: ISubjectRepo<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'SubjectIndex, 'OpError>,
            logger: Microsoft.Extensions.Logging.ILogger<SubjectRepoGrain<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'SubjectIndex, 'OpError>>
        ) =

    inherit Grain()

    let wrapExceptions (methodName: string) (createTask: unit -> Task<'T>) : Task<'T> =
        task {
            try
                let! res = createTask ()
                return res
            with
            | :? TransientSubjectException as ex ->
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw()
                return shouldNotReachHereBecause "line above throws"
            | :? PermanentSubjectException as ex ->
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw()
                return shouldNotReachHereBecause "line above throws"
            | ex ->
                logger.LogError(ex, $"Exception in SubjectRepoGrain.${methodName}")
                return TransientSubjectException ($"Subject repo %s{methodName}", ex.ToString()) |> raise
        }

    interface ISubjectRepoGrain<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'SubjectIndex, 'OpError> with
        member _.Versioned_GetByIdsStr (ids: Set<string>) : Task<List<VersionedSubject<'Subject, 'SubjectId>>> =
            fun () -> repo.GetByIdsStr ids
            |> wrapExceptions (nameof(repo.GetByIdStr))

        member _.Versioned_GetByIds (ids: Set<'SubjectId>) : Task<List<VersionedSubject<'Subject, 'SubjectId>>> =
            fun () -> repo.GetByIds ids
            |> wrapExceptions (nameof(repo.GetByIds))

        member _.Any (predicate: PreparedIndexPredicate<'SubjectIndex>) : Task<bool> =
            fun () -> repo.Any predicate
            |> wrapExceptions (nameof(repo.Any))

        member _.Versioned_FilterFetchSubjects (query: IndexQuery<'SubjectIndex>) : Task<List<VersionedSubject<'Subject, 'SubjectId>>> =
            fun () -> repo.FilterFetchSubjects query
            |> wrapExceptions (nameof(repo.FilterFetchSubjects))

        member _.FilterFetchIds (query: IndexQuery<'SubjectIndex>) : Task<List<'SubjectId>> =
            fun () -> repo.FilterFetchIds query
            |> wrapExceptions (nameof(repo.FilterFetchIds))

        member _.FilterCountSubjects (predicate: PreparedIndexPredicate<'SubjectIndex>) : Task<uint64> =
            fun () -> repo.FilterCountSubjects predicate
            |> wrapExceptions (nameof(repo.FilterCountSubjects))

        member _.Versioned_FilterFetchSubjectsWithTotalCount (query: IndexQuery<'SubjectIndex>) : Task<List<VersionedSubject<'Subject, 'SubjectId>> * uint64> =
            fun () -> repo.FilterFetchSubjectsWithTotalCount query
            |> wrapExceptions (nameof(repo.FilterFetchSubjectsWithTotalCount))

        member _.Versioned_FetchAllSubjects (resultSetOptions: ResultSetOptions<'SubjectIndex>) : Task<List<VersionedSubject<'Subject, 'SubjectId>>> =
            fun () -> repo.FetchAllSubjects resultSetOptions
            |> wrapExceptions (nameof(repo.FetchAllSubjects))

        member _.FetchAllSubjectsWithTotalCount (resultSetOptions: ResultSetOptions<'SubjectIndex>) : Task<List<VersionedSubject<'Subject, 'SubjectId>> * uint64> =
            fun () -> repo.FetchAllSubjectsWithTotalCount resultSetOptions
            |> wrapExceptions (nameof(repo.FetchAllSubjectsWithTotalCount))

        member _.CountAllSubjects () : Task<uint64> =
            fun () -> repo.CountAllSubjects ()
            |> wrapExceptions (nameof(repo.CountAllSubjects))

        member _.GetVersionSnapshotByIdStr (idStr: string) (ofVersion: GetSnapshotOfVersion) : Task<Option<TemporalSnapshot<'Subject, 'LifeAction, 'Constructor, 'SubjectId>>> =
            fun () -> repo.GetVersionSnapshotByIdStr idStr ofVersion
            |> wrapExceptions (nameof(repo.GetVersionSnapshotByIdStr))

        member _.GetVersionSnapshotById (id: 'SubjectId) (ofVersion: GetSnapshotOfVersion) : Task<Option<TemporalSnapshot<'Subject, 'LifeAction, 'Constructor, 'SubjectId>>> =
            fun () -> repo.GetVersionSnapshotById id ofVersion
            |> wrapExceptions (nameof(repo.GetVersionSnapshotById))

        member _.FetchWithHistoryById (id: 'SubjectId) (fromLastUpdatedOn: Option<DateTimeOffset>) (toLastUpdatedOn: Option<DateTimeOffset>) (page: ResultPage) : Task<List<TemporalSnapshot<'Subject, 'LifeAction, 'Constructor, 'SubjectId>>> =
            fun () -> repo.FetchWithHistoryById id fromLastUpdatedOn toLastUpdatedOn page
            |> wrapExceptions (nameof(repo.FetchWithHistoryById))

        member _.FetchWithHistoryByIdStr (idStr: string) (fromLastUpdatedOn: Option<DateTimeOffset>) (toLastUpdatedOn: Option<DateTimeOffset>) (page: ResultPage) : Task<List<TemporalSnapshot<'Subject, 'LifeAction, 'Constructor, 'SubjectId>>> =
            fun () -> repo.FetchWithHistoryByIdStr idStr fromLastUpdatedOn toLastUpdatedOn page
            |> wrapExceptions (nameof(repo.FetchWithHistoryByIdStr))

        member _.FetchAuditTrail (idStr: string) (page: ResultPage) : Task<List<SubjectAuditData<'LifeAction, 'Constructor>>> =
            fun () -> repo.FetchAuditTrail idStr page
            |> wrapExceptions (nameof(repo.FetchAuditTrail))

        member _.GetByIdsStr (ids: Set<string>) : Task<List<'Subject>> =
            fun () -> repo.GetByIdsStr ids |> Task.map (List.map VersionedSubject.subject)
            |> wrapExceptions (nameof(repo.GetByIdStr))

        member _.GetByIds (ids: Set<'SubjectId>) : Task<List<'Subject>> =
            fun () -> repo.GetByIds ids |> Task.map (List.map VersionedSubject.subject)
            |> wrapExceptions (nameof(repo.GetByIds))

        member _.FilterFetchSubjects (query: IndexQuery<'SubjectIndex>) : Task<List<'Subject>> =
            fun () -> repo.FilterFetchSubjects query |> Task.map (List.map VersionedSubject.subject)
            |> wrapExceptions (nameof(repo.FilterFetchSubjects))

        member _.FilterFetchSubjectsWithTotalCount (query: IndexQuery<'SubjectIndex>) : Task<List<'Subject> * uint64> =
            fun () -> repo.FilterFetchSubjectsWithTotalCount query |> Task.map (fun (subjects, count) -> subjects |> List.map VersionedSubject.subject, count)
            |> wrapExceptions (nameof(repo.FilterFetchSubjectsWithTotalCount))

        member _.FetchAllSubjects (resultSetOptions: ResultSetOptions<'SubjectIndex>) : Task<List<'Subject>> =
            fun () -> repo.FetchAllSubjects resultSetOptions |> Task.map (List.map VersionedSubject.subject)
            |> wrapExceptions (nameof(repo.FetchAllSubjects))

    interface ITrackedGrain with
        member _.GetTelemetryData (_methodInfo: System.Reflection.MethodInfo) (_: obj[]) : Option<TrackedGrainTelemetryData> = None