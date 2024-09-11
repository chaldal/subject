[<AutoOpen>]
module LibLifeCycleHost.Storage.Test.Repo

open System.Text.RegularExpressions
open LibLifeCycle
open LibLifeCycle.LifeCycles.Meta
open LibLifeCycleCore
open LibLifeCycleHost
open System.Threading.Tasks
open System.Collections.Concurrent
open LibLifeCycleHost.GrainStorageModel
open System
open LibLifeCycleTypes
open NetTopologySuite.Geometries
open NetTopologySuite.IO

let private wktReader = WKTReader()

type GeographyIndexValue with
    member this.ToGeometry() : Geometry =
        wktReader.Read(this.ToWkt())

// Trivial (and un-optimized) in-memory "persistent" store for tests

type TestSubjectRepo<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId
                          when 'Subject              :> Subject<'SubjectId>
                          and  'LifeAction           :> LifeAction
                          and  'OpError              :> OpError
                          and  'Constructor          :> Constructor
                          and  'LifeEvent            :> LifeEvent
                          and  'LifeEvent            : comparison
                          and  'SubjectIndex         :> SubjectIndex<'OpError>
                          and  'SubjectId            :> SubjectId
                          and  'SubjectId            : comparison>(indices: 'Subject -> seq<'SubjectIndex>, grainPartition: GrainPartition) =

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

    let getSearchScore (text1: string) (text2: string) =
        // A very rudimentary full-text search algorithm, not reflective of actual search algorithm used in a real search engines
        let words1 = Regex.Split(text1.ToLowerInvariant(), @"\W|_") |> Set.ofArray
        let words2 = Regex.Split(text2.ToLowerInvariant(), @"\W|_") |> Set.ofArray
        Set.intersect words1 words2
        |> Set.count

    let doesMatch (predicate: UntypedPredicate) (subject: 'Subject) : bool =
        let indicesMap =
            indices subject
            |> List.ofSeq
            |> List.map (fun idx ->
                match
                    idx.MaybeKeyAndPrimitiveNumber,
                    idx.MaybeKeyAndPrimitiveString,
                    idx.MaybeKeyAndPrimitiveSearchableText,
                    idx.MaybeKeyAndPrimitiveGeography
                with
                | Some (key, value), None, None, None ->
                    key, Choice1Of4 value
                | None, Some (key, value), None, None ->
                    key, Choice2Of4 value
                | None, None, Some (key, value), None ->
                    key, Choice3Of4 value
                | None, None, None, Some (key, value) ->
                    key, Choice4Of4 value
                | _ ->
                    failwith "should not reach" )
            |> List.fold
                (fun state (key, value) -> state |> Map.change key (function | None -> Some [value] | Some curValues -> Some (value :: curValues)))
                Map.empty

        let numOp op (queryValue: int64)     (values: list<Choice<IndexedPrimitiveNumber<'OpError>, _, _, _>>) : bool =
            values |> List.filter (function | Choice1Of4 idxNum        -> op idxNum.Value        queryValue | Choice2Of4 _ | Choice3Of4 _ | Choice4Of4 _ -> false) |> List.isEmpty |> not
        let strOp op (queryValue: string)    (values: list<Choice<_, IndexedPrimitiveString<'OpError>, _, _>>) : bool =
            values |> List.filter (function | Choice2Of4 idxStr        -> op idxStr.Value        queryValue | Choice1Of4 _ | Choice3Of4 _ | Choice4Of4 _ -> false) |> List.isEmpty |> not
        let searchOp op (queryValue: string) (values: list<Choice<_, _, IndexedPrimitiveSearchableText, _>>) : bool =
            values |> List.filter (function | Choice3Of4 idxSearchable -> op idxSearchable.Value queryValue | Choice1Of4 _ | Choice2Of4 _ | Choice4Of4 _ -> false) |> List.isEmpty |> not
        let geographyOp op (queryValue: Geometry) (values: list<Choice<_, _, _, IndexedPrimitiveGeography>>) : bool =
            values |> List.filter (function | Choice4Of4 idxGeography -> op (idxGeography.Value.ToGeometry()) queryValue | Choice1Of4 _ | Choice2Of4 _ | Choice3Of4 _ -> false) |> List.isEmpty |> not

        let hasIndexValueMatchingOp op (key, queryValue) =
            Map.tryFind key indicesMap |> Option.mapOrElse false (op queryValue)

        let rec doesMatchRec (predicate: UntypedPredicate) : bool =
            match predicate with
            | UntypedPredicate.EqualToNumeric (key, value) ->
                (key, value) |> hasIndexValueMatchingOp (numOp (=))
            | UntypedPredicate.GreaterThanNumeric (key, value) ->
                (key, value) |> hasIndexValueMatchingOp (numOp (>))
            | UntypedPredicate.GreaterThanOrEqualToNumeric (key, value) ->
                (key, value) |> hasIndexValueMatchingOp (numOp (>=))
            | UntypedPredicate.LessThanNumeric (key, value) ->
                (key, value) |> hasIndexValueMatchingOp (numOp (<))
            | UntypedPredicate.LessThanOrEqualToNumeric (key, value) ->
                (key, value) |> hasIndexValueMatchingOp (numOp (<=))
            | UntypedPredicate.EqualToString (key, value) ->
                (key, value) |> hasIndexValueMatchingOp (strOp (=))
            | UntypedPredicate.GreaterThanString (key, value) ->
                (key, value) |> hasIndexValueMatchingOp (strOp (>))
            | UntypedPredicate.GreaterThanOrEqualToString (key, value) ->
                (key, value) |> hasIndexValueMatchingOp (strOp (>=))
            | UntypedPredicate.LessThanString (key, value) ->
                (key, value) |> hasIndexValueMatchingOp (strOp (<))
            | UntypedPredicate.LessThanOrEqualToString (key, value) ->
                (key, value) |> hasIndexValueMatchingOp (strOp (<=))
            | UntypedPredicate.StartsWith (key, value) ->
                (key, value) |> hasIndexValueMatchingOp (strOp (fun v (start: string) -> v.StartsWith start))
            | UntypedPredicate.Matches (key, value) ->
                (key, value) |> hasIndexValueMatchingOp (searchOp (fun x y -> getSearchScore x y > 0))
            | UntypedPredicate.MatchesExact (key, value) ->
                (key, value) |> hasIndexValueMatchingOp (searchOp (fun x y -> getSearchScore x y > 0))
            | UntypedPredicate.MatchesPrefix (key, value) ->
                (key, value) |> hasIndexValueMatchingOp (searchOp (fun x -> x.StartsWith))
            | UntypedPredicate.IntersectsGeography (key, value) ->
                (key, value.ToGeometry()) |> hasIndexValueMatchingOp (geographyOp (fun x -> x.Intersects))

            | UntypedPredicate.And (left, right) ->
                (doesMatchRec left) && (doesMatchRec right)

            | UntypedPredicate.Or (left, right) ->
                (doesMatchRec left) || (doesMatchRec right)

            | UntypedPredicate.Diff (left, right) ->
                (doesMatchRec left) && not (doesMatchRec right)

        doesMatchRec predicate

    let sortByDirn = function | OrderDirection.Ascending -> Seq.sortBy | OrderDirection.Descending -> Seq.sortByDescending

    let sort (orderBy: UntypedOrderBy) (versionedSubjects: seq<VersionedSubject<'Subject, 'SubjectId>>) : seq<VersionedSubject<'Subject, 'SubjectId>> =
        match orderBy with
        | UntypedOrderBy.FastestOrSingleSearchScoreIfAvailable ->
            versionedSubjects

        | UntypedOrderBy.Random ->
            let rnd = Random()
            versionedSubjects
            |> Seq.map (fun vs -> vs, (rnd.Next()))
            |> Seq.sortBy snd
            |> Seq.map fst

        | UntypedOrderBy.SubjectId dirn ->
            versionedSubjects
            |> Seq.map (fun vs -> vs, ((getId >> getIdString) vs.Subject))
            |> sortByDirn dirn snd
            |> Seq.map fst

        | UntypedOrderBy.NumericIndexEntry (key, dirn) ->
            versionedSubjects
            |> Seq.map (fun vs ->
                let indexValue =
                    indices vs.Subject
                    |> Seq.choose (fun index ->
                        match index.MaybeKeyAndPrimitiveNumber with
                        | Some (iKey, iValue) when iKey = key -> Some iValue.Value
                        | _ -> None)
                    |> Seq.tryHead
                (vs, indexValue))
            |> sortByDirn dirn snd
            |> Seq.map fst

        | UntypedOrderBy.StringIndexEntry (key, dirn) ->
            versionedSubjects
            |> Seq.map (fun vs ->
                let indexValue =
                    indices vs.Subject
                    |> Seq.choose (fun index ->
                        match index.MaybeKeyAndPrimitiveString with
                        | Some (iKey, iValue) when iKey = key -> Some iValue.Value
                        | _ -> None)
                    |> Seq.tryHead
                (vs, indexValue))
            |> sortByDirn dirn snd
            |> Seq.map fst

    let sortAndPaginateAndReturnTotalCount (options: UntypedResultSetOptions) (subjects: seq<VersionedSubject<'Subject, 'SubjectId>>) : seq<VersionedSubject<'Subject, 'SubjectId>> * int =
        let subjectList = Seq.toList subjects

        let data =
            subjectList
            |> sort options.OrderBy
            // Seq.skip is unsafe, and might crash when offset > # of items
            |> Seq.mapi (fun i s -> (s, (uint64 i)))
            |> Seq.choose (fun (s, i) -> if i >= options.Page.Offset then Some s else None)
            |> Seq.truncate (int options.Page.Size)

        (data, subjectList.Length)

    let maybeCurrentState =
        function
        | SubjectStateContainer.Committed currentState
        | SubjectStateContainer.PreparedAction (currentState, _, _) ->
            Some currentState
        | SubjectStateContainer.PreparedInitialize _ ->
            None

    let subjectCurrentStateContainerToVersionedSubject (subjectStateContainer: SubjectCurrentStateContainer<'Subject, 'SubjectId, 'LifeEvent, 'LifeAction>): VersionedSubject<'Subject, 'SubjectId> =
        {
            Subject = subjectStateContainer.CurrentSubjectState.Subject
            AsOf = subjectStateContainer.CurrentSubjectState.LastUpdatedOn
            Version = subjectStateContainer.Version
        }

    let filterAndSortAndReturnTotalCount (query: IndexQuery<'SubjectIndex>) : seq<VersionedSubject<'Subject, 'SubjectId>> * int =
        let db = db()

        lock
            db.LockObj
            (fun _ ->
                db.SubjectsByIdStr
                |> List.ofSeq
                |> List.map (|KeyValue|)
                |> List.map snd
                |> List.choose maybeCurrentState
                |> List.map subjectCurrentStateContainerToVersionedSubject
                |> List.where (fun versionedSubject -> doesMatch query.Predicate versionedSubject.Subject)
                |> sortAndPaginateAndReturnTotalCount query.ResultSetOptions)

    // This is likely going to get solved by moving this over to Service Fabric Volatile Reliable Collections
    // Until then, process starts will dump this memory
    interface ISubjectRepo<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'SubjectIndex, 'OpError> with

        member _.DoesExistById (id: 'SubjectId): Task<bool> =
            let db = db()
            lock
                db.LockObj
                (fun _ -> db.SubjectsByIdStr.ContainsKey (getIdString id))
            |> Task.FromResult

        member _.GetByIdStr (idStr: string): Task<Option<VersionedSubject<'Subject, 'SubjectId>>> =
            let db = db()
            lock
                db.LockObj
                (fun _ ->
                    match db.SubjectsByIdStr.TryGetValue idStr with
                    | (true, v) ->  maybeCurrentState v |> Option.map subjectCurrentStateContainerToVersionedSubject
                    | (false, _) -> None)
            |> Task.FromResult

        member this.GetById (id: 'SubjectId): Task<Option<VersionedSubject<'Subject, 'SubjectId>>> =
            getIdString id
            |> (this :> ISubjectRepo<_, _, _, _, _, _>).GetByIdStr

        member this.GetByIdsStr (ids: Set<string>): Task<list<VersionedSubject<'Subject, 'SubjectId>>> =
            backgroundTask {
                let! subjects =
                    ids
                    |> Set.toSeq
                    |> Seq.map (this :> ISubjectRepo<_, _, _, _, _, _>).GetByIdStr
                    |> Task.WhenAll

                return
                    subjects
                    |> Seq.collect Option.toList
                    |> Seq.toList
            }

        member this.GetByIds (ids: Set<'SubjectId>): Task<list<VersionedSubject<'Subject, 'SubjectId>>> =
            ids
            |> Set.map getIdString
            |> (this :> ISubjectRepo<_, _, _, _, _, _>).GetByIdsStr

        member this.Any(predicate: PreparedIndexPredicate<'SubjectIndex>): Task<bool> =
            let db = db()
            lock
                db.LockObj
                (fun _ ->
                    db.SubjectsByIdStr
                    |> Seq.map (|KeyValue|)
                    |> Seq.map snd
                    |> Seq.choose maybeCurrentState
                    |> Seq.map (fun state -> state.CurrentSubjectState.Subject)
                    |> Seq.where (doesMatch predicate.Predicate)
                    |> Seq.isNonempty)
            |> Task.FromResult

        member this.FilterFetchIds (query: IndexQuery<'SubjectIndex>) : Task<list<'SubjectId>> =
            filterAndSortAndReturnTotalCount query
            |> fst
            |> Seq.map (fun s -> s.Subject.SubjectId)
            |> Seq.toList
            |> Task.FromResult

        member this.FilterFetchSubjects (query: IndexQuery<'SubjectIndex>) : Task<list<VersionedSubject<'Subject, 'SubjectId>>> =
            filterAndSortAndReturnTotalCount query
            |> fst
            |> Seq.toList
            |> Task.FromResult

        member this.FilterFetchSubjectsWithTotalCount(query) : Task<list<VersionedSubject<'Subject, 'SubjectId>> * uint64> =
            let (subjects, totalCount) = filterAndSortAndReturnTotalCount query
            ((Seq.toList subjects), uint64 totalCount)
            |> Task.FromResult

        member this.FilterCountSubjects (predicate: PreparedIndexPredicate<'SubjectIndex>) : Task<uint64> =
            let db = db()
            lock
                db.LockObj
                (fun _ ->
                    db.SubjectsByIdStr
                    |> Seq.map (|KeyValue|)
                    |> Seq.map snd
                    |> Seq.choose maybeCurrentState
                    |> Seq.map (fun state -> state.CurrentSubjectState.Subject)
                    |> Seq.where (doesMatch predicate.Predicate)
                    |> Seq.length
                    |> uint64)
            |> Task.FromResult

        member this.FetchAllSubjects (resultSetOptions: ResultSetOptions<'SubjectIndex>): System.Threading.Tasks.Task<list<VersionedSubject<'Subject, 'SubjectId>>> =
            let db = db()
            lock
                db.LockObj
                (fun _ ->
                    db.SubjectsByIdStr
                    |> Seq.choose (fun kv -> maybeCurrentState kv.Value)
                    |> Seq.map subjectCurrentStateContainerToVersionedSubject
                    |> sortAndPaginateAndReturnTotalCount resultSetOptions.Options
                    |> fst
                    |> Seq.toList)
            |> Task.FromResult

        member this.CountAllSubjects () : System.Threading.Tasks.Task<uint64> =
            let db = db ()
            lock
                db.LockObj
                (fun _ ->
                    db.SubjectsByIdStr
                    |> Seq.choose (fun kv -> maybeCurrentState kv.Value)
                    |> Seq.length
                    |> uint64)
                |> Task.FromResult

        member this.FetchAllSubjectsWithTotalCount (resultSetOptions: ResultSetOptions<'SubjectIndex>): System.Threading.Tasks.Task<list<VersionedSubject<'Subject, 'SubjectId>> * uint64> =
            let db = db()
            lock
                db.LockObj
                (fun _ ->
                    db.SubjectsByIdStr
                    |> Seq.choose (fun kv -> maybeCurrentState kv.Value)
                    |> Seq.map subjectCurrentStateContainerToVersionedSubject
                    |> sortAndPaginateAndReturnTotalCount resultSetOptions.Options
                    |> fun (data, totalCount) -> ((Seq.toList data), uint64 totalCount))
            |> Task.FromResult


        member this.GetVersionSnapshotByIdStr (_idStr: string) (_ofVersion: GetSnapshotOfVersion) : Task<Option<TemporalSnapshot<'Subject, 'LifeAction, 'Constructor, 'SubjectId>>> =
            NotImplementedException() |> raise

        member this.GetVersionSnapshotById (_id: 'SubjectId) (_ofVersion: GetSnapshotOfVersion) : Task<Option<TemporalSnapshot<'Subject, 'LifeAction, 'Constructor, 'SubjectId>>> =
            NotImplementedException() |> raise

        member this.FetchWithHistoryById (_id: 'SubjectId) (_fromLastUpdatedOn: Option<DateTimeOffset>) (_toLastUpdatedOn: Option<DateTimeOffset>) (_page: ResultPage): Task<list<TemporalSnapshot<'Subject, 'LifeAction, 'Constructor, 'SubjectId>>> =
            Task.FromResult []

        member _.FetchWithHistoryByIdStr (_idStr: string) (_fromLastUpdatedOn: Option<DateTimeOffset>) (_toLastUpdatedOn: Option<DateTimeOffset>) (_page: ResultPage): Task<list<TemporalSnapshot<'Subject, 'LifeAction, 'Constructor, 'SubjectId>>> =
            Task.FromResult []

        member _.GetSideEffectPermanentFailures (_scope: UpdatePermanentFailuresScope): Task<list<SideEffectPermanentFailure>> =
            Task.FromResult []

        member this.FetchAuditTrail _idStr _page =
            Task.FromResult []

type TestSubjectBlobRepo (blobStorage: TestBlobStorage) =
    interface IBlobRepo with
        member _.GetBlobData (_ecosystemName: string) (subjectRef: LocalSubjectPKeyReference) (blobId: System.Guid) : Task<Option<BlobData>> =
            blobStorage.GetBlobData (blobId, subjectRef)
            |> Task.FromResult

        member _.GetBlobDataStream (_ecosystemName: string) (subjectRef: LocalSubjectPKeyReference) (blobId: Guid) (readBlobDataStream: Option<BlobDataStream> -> Task) : Task =
            match blobStorage.GetBlobData (blobId, subjectRef) with
            | None ->
                readBlobDataStream None
            | Some blobData ->
                use memoryStream = new System.IO.MemoryStream(blobData.Data, writable = false)
                readBlobDataStream (Some { TotalBytes = blobData.Data.Length; Stream = memoryStream; MimeType = blobData.MimeType })
