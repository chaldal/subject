[<AutoOpen>]
module LibLifeCycle.DefaultServices

open System
open System.Runtime.Serialization
open System.Threading.Tasks
open System.Text
open LibLifeCycle.Caching
open LibLifeCycle.LifeCycles.Meta.Types

// similar to TemporalSnapshot but without 'LifeAction / 'Constructor details, to keep All<> definition shorter
type SubjectHistorySnapshot<'Subject, 'SubjectId when 'Subject :> Subject<'SubjectId> and  'SubjectId :> SubjectId and 'SubjectId : comparison> = {
    AsOf:      DateTimeOffset
    Subject:   'Subject
}

// TODO: fix possible ambiguity of All<>, it implicitly assumes that combination of type arguments uniquely identifies target life cycle which is not guaranteed.
// Especially so with addition of biosphere / referenced ecosystems
type All<'Subject, 'SubjectId, 'SubjectIndex, 'OpError
          when 'Subject      :> Subject<'SubjectId>
          and  'SubjectId    :> SubjectId
          and  'OpError      :> OpError
          and  'SubjectId    :  comparison
          and  'SubjectIndex :> SubjectIndex<'OpError>> =
| DoesExistById                     of 'SubjectId                            * ResponseChannel<bool>
| GetById                           of 'SubjectId                            * ResponseChannel<Option<'Subject>>
| GetByIds                          of Set<'SubjectId>                       * ResponseChannel<List<'Subject>>
| Any                               of PreparedIndexPredicate<'SubjectIndex> * ResponseChannel<bool>
| FilterFetchIds                    of IndexQuery<'SubjectIndex>             * ResponseChannel<List<'SubjectId>>
| FilterFetchSubjects               of IndexQuery<'SubjectIndex>             * ResponseChannel<List<'Subject>>
| FilterFetchSubjectsWithTotalCount of IndexQuery<'SubjectIndex>             * ResponseChannel<List<'Subject> * uint64>
| FilterCountSubjects               of PreparedIndexPredicate<'SubjectIndex> * ResponseChannel<uint64>
| FilterFetchSubject                of PreparedIndexPredicate<'SubjectIndex> * ResponseChannel<Option<'Subject>>
| FetchAll                          of ResultSetOptions<'SubjectIndex>       * ResponseChannel<List<'Subject>>
| CountAll                          of                                         ResponseChannel<uint64>
| FetchWithHistory                  of 'SubjectId * From: Option<DateTimeOffset> * To: Option<DateTimeOffset> * ResultPage * ResponseChannel<List<SubjectHistorySnapshot<'Subject, 'SubjectId>>>
| GetByIdCached                     of 'SubjectId * CachedSubjectExpirationPeriod: TimeSpan * ResponseChannel<Option<'Subject>>

with interface Request

type SideEffectPermanentFailure = {
    SubjectIdStr:    string
    SideEffectId:    Guid
    SeqNum:          uint64
    FailureSeverity: byte
    FailureReason:   string
    SideEffectData:  string
    CreatedOn:       DateTimeOffset
}

type ISubjectRepo<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'SubjectIndex, 'OpError
                   when 'Subject      :> Subject<'SubjectId>
                   and  'LifeAction   :> LifeAction
                   and  'Constructor  :> Constructor
                   and  'SubjectId    :> SubjectId
                   and  'OpError       :> OpError
                   and  'SubjectId     : comparison
                   and  'SubjectIndex  :> SubjectIndex<'OpError>> =
    abstract member GetById:                           id:  'SubjectId -> Task<Option<VersionedSubject<'Subject, 'SubjectId>>>
    abstract member GetByIdStr:                        idStr: string   -> Task<Option<VersionedSubject<'Subject, 'SubjectId>>>
    abstract member DoesExistById:                     id: 'SubjectId -> Task<bool>
    abstract member GetByIdsStr:                       ids: Set<string> -> Task<List<VersionedSubject<'Subject, 'SubjectId>>>
    abstract member GetByIds:                          ids: Set<'SubjectId> -> Task<List<VersionedSubject<'Subject, 'SubjectId>>>
    abstract member Any:                               predicate: PreparedIndexPredicate<'SubjectIndex> -> Task<bool>
    abstract member FilterFetchIds:                    query: IndexQuery<'SubjectIndex> -> Task<List<'SubjectId>>
    abstract member FilterFetchSubjects:               query: IndexQuery<'SubjectIndex> -> Task<List<VersionedSubject<'Subject, 'SubjectId>>>
    abstract member FilterFetchSubjectsWithTotalCount: query: IndexQuery<'SubjectIndex> -> Task<List<VersionedSubject<'Subject, 'SubjectId>> * uint64>
    abstract member FilterCountSubjects:               predicate: PreparedIndexPredicate<'SubjectIndex> -> Task<uint64>
    abstract member CountAllSubjects:                  unit -> Task<uint64>
    abstract member FetchAllSubjects:                  resultSetOptions: ResultSetOptions<'SubjectIndex> -> Task<List<VersionedSubject<'Subject, 'SubjectId>>>
    abstract member FetchAllSubjectsWithTotalCount:    resultSetOptions: ResultSetOptions<'SubjectIndex> -> Task<List<VersionedSubject<'Subject, 'SubjectId>> * uint64>
    abstract member GetVersionSnapshotById:            id: 'SubjectId -> ofVersion: GetSnapshotOfVersion -> Task<Option<TemporalSnapshot<'Subject, 'LifeAction, 'Constructor, 'SubjectId>>>
    abstract member GetVersionSnapshotByIdStr:         idStr: string -> ofVersion: GetSnapshotOfVersion -> Task<Option<TemporalSnapshot<'Subject, 'LifeAction, 'Constructor, 'SubjectId>>>
    abstract member FetchWithHistoryById:              id: 'SubjectId -> fromLastUpdatedOn: Option<DateTimeOffset> -> toLastUpdatedOn: Option<DateTimeOffset> -> page: ResultPage -> Task<List<TemporalSnapshot<'Subject, 'LifeAction, 'Constructor, 'SubjectId>>>
    abstract member FetchWithHistoryByIdStr:           idStr: string -> fromLastUpdatedOn: Option<DateTimeOffset> -> toLastUpdatedOn: Option<DateTimeOffset> -> page: ResultPage -> Task<List<TemporalSnapshot<'Subject, 'LifeAction, 'Constructor, 'SubjectId>>>
    abstract member FetchAuditTrail:                   idStr: string -> page: ResultPage -> Task<List<SubjectAuditData<'LifeAction, 'Constructor>>>
    abstract member GetSideEffectPermanentFailures:    scope: UpdatePermanentFailuresScope -> Task<List<SideEffectPermanentFailure>>

type AllBlobs =
| GetBlobDataById of EcosystemName: string * BlobId * ResponseChannel<Option<BlobData>>
with interface Request

type BlobDataStream = {
    TotalBytes: Int64
    Stream:     System.IO.Stream
    MimeType:   Option<MimeType>
}

type IBlobRepo =
    abstract member GetBlobData: ecosystemName: string -> subjectRef: LocalSubjectPKeyReference -> blobId: Guid -> Task<Option<BlobData>>
    abstract member GetBlobDataStream: ecosystemName: string -> subjectRef: LocalSubjectPKeyReference -> blobId: Guid -> readBlobDataStream: (Option<BlobDataStream> -> Task) -> Task

type AllTimeSeries<'TimeSeriesDataPoint, 'TimeSeriesId, [<Measure>] 'UnitOfMeasure, 'TimeSeriesIndex
                    when 'TimeSeriesDataPoint :> TimeSeriesDataPoint<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure>
                    and  'TimeSeriesId :> TimeSeriesId<'TimeSeriesId>
                    and  'TimeSeriesIndex :> TimeSeriesIndex<'TimeSeriesIndex>> =
| TimeSeriesValues                of TimeSeriesInterval<'TimeSeriesId, 'TimeSeriesIndex> * ResponseChannel<list<DateTimeOffset * float<'UnitOfMeasure>>>
| TimeSeriesDataPoints            of TimeSeriesInterval<'TimeSeriesId, 'TimeSeriesIndex> * ResponseChannel<list<'TimeSeriesDataPoint>>
| TimeSeriesBucketedValues        of TimeSeriesInterval<'TimeSeriesId, 'TimeSeriesIndex> * TimeBucket * TimeBucketValueAggregate * ResponseChannel<list<DateTimeOffset * float<'UnitOfMeasure>>>
| TimeSeriesBucketedValuesBy      of TimeSeriesGroupInterval<'TimeSeriesId, 'TimeSeriesIndex> * TimeBucket * TimeBucketValueAggregate * ResponseChannel<list<'TimeSeriesIndex * list<DateTimeOffset * float<'UnitOfMeasure>>>>
| TimeSeriesBucketedDataPoints    of TimeSeriesInterval<'TimeSeriesId, 'TimeSeriesIndex> * TimeBucket * TimeBucketPointAggregate * ResponseChannel<list<'TimeSeriesDataPoint>>
| TimeSeriesBucketedDataPointsBy  of TimeSeriesGroupInterval<'TimeSeriesId, 'TimeSeriesIndex> * TimeBucket * TimeBucketPointAggregate * ResponseChannel<list<'TimeSeriesIndex * list<'TimeSeriesDataPoint>>>
| TimeSeriesAggregateValue        of TimeSeriesInterval<'TimeSeriesId, 'TimeSeriesIndex> * TimeBucketValueAggregate * ResponseChannel<Option<float<'UnitOfMeasure>>>
| TimeSeriesAggregateValuesBy     of TimeSeriesGroupInterval<'TimeSeriesId, 'TimeSeriesIndex> * TimeBucketValueAggregate * ResponseChannel<list<'TimeSeriesIndex * float<'UnitOfMeasure>>>
| TimeSeriesAggregateDataPoint    of TimeSeriesInterval<'TimeSeriesId, 'TimeSeriesIndex> * TimeBucketPointAggregate * ResponseChannel<Option<'TimeSeriesDataPoint>>
| TimeSeriesAggregateDataPointsBy of TimeSeriesGroupInterval<'TimeSeriesId, 'TimeSeriesIndex> * TimeBucketPointAggregate * ResponseChannel<list<'TimeSeriesIndex * 'TimeSeriesDataPoint>>
| TimeSeriesCountDataPoints          of TimeSeriesInterval<'TimeSeriesId, 'TimeSeriesIndex> * ResponseChannel<uint64>
| TimeSeriesCountDataPointsBy        of TimeSeriesGroupInterval<'TimeSeriesId, 'TimeSeriesIndex> * ResponseChannel<list<'TimeSeriesIndex * uint64>>
with interface Request


type ITimeSeriesRepo<'TimeSeriesDataPoint, 'TimeSeriesId, [<Measure>] 'UnitOfMeasure, 'TimeSeriesIndex
    when 'TimeSeriesDataPoint :> TimeSeriesDataPoint<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure>
    and  'TimeSeriesId :> TimeSeriesId<'TimeSeriesId>
    and  'TimeSeriesIndex :> TimeSeriesIndex<'TimeSeriesIndex>> =
    abstract member Values: interval: TimeSeriesInterval<'TimeSeriesId, 'TimeSeriesIndex> -> Task<list<DateTimeOffset * float<'UnitOfMeasure>>>
    abstract member DataPoints: interval: TimeSeriesInterval<'TimeSeriesId, 'TimeSeriesIndex> -> Task<list<'TimeSeriesDataPoint>>
    abstract member BucketedValues: interval: TimeSeriesInterval<'TimeSeriesId, 'TimeSeriesIndex> -> bucket: TimeBucket -> aggregate: TimeBucketValueAggregate -> Task<list<DateTimeOffset * float<'UnitOfMeasure>>>
    abstract member BucketedValuesBy: groupInterval: TimeSeriesGroupInterval<'TimeSeriesId, 'TimeSeriesIndex> -> bucket: TimeBucket -> aggregate: TimeBucketValueAggregate -> Task<list<'TimeSeriesIndex * list<DateTimeOffset * float<'UnitOfMeasure>>>>
    abstract member BucketedDataPoints: interval: TimeSeriesInterval<'TimeSeriesId, 'TimeSeriesIndex> -> bucket: TimeBucket -> aggregate: TimeBucketPointAggregate -> Task<list<'TimeSeriesDataPoint>>
    abstract member BucketedDataPointsBy: groupInterval: TimeSeriesGroupInterval<'TimeSeriesId, 'TimeSeriesIndex> -> bucket: TimeBucket -> aggregate: TimeBucketPointAggregate -> Task<list<'TimeSeriesIndex * list<'TimeSeriesDataPoint>>>
    abstract member AggregateValue: interval: TimeSeriesInterval<'TimeSeriesId, 'TimeSeriesIndex> -> aggregate: TimeBucketValueAggregate -> Task<Option<float<'UnitOfMeasure>>>
    abstract member AggregateValuesBy: groupInterval: TimeSeriesGroupInterval<'TimeSeriesId, 'TimeSeriesIndex> -> aggregate: TimeBucketValueAggregate -> Task<list<'TimeSeriesIndex * float<'UnitOfMeasure>>>
    abstract member AggregateDataPoint: interval: TimeSeriesInterval<'TimeSeriesId, 'TimeSeriesIndex> -> aggregate: TimeBucketPointAggregate -> Task<Option<'TimeSeriesDataPoint>>
    abstract member AggregateDataPointsBy: groupInterval: TimeSeriesGroupInterval<'TimeSeriesId, 'TimeSeriesIndex> -> aggregate: TimeBucketPointAggregate -> Task<list<'TimeSeriesIndex * 'TimeSeriesDataPoint>>
    abstract member CountDataPoints: interval: TimeSeriesInterval<'TimeSeriesId, 'TimeSeriesIndex> -> Task<uint64>
    abstract member CountDataPointsBy: groupInterval: TimeSeriesGroupInterval<'TimeSeriesId, 'TimeSeriesIndex> -> Task<list<'TimeSeriesIndex * uint64>>


type IActionContext =
    abstract member GetUserId:    unit -> string
    abstract member GetSessionId: unit -> Option<string>

type ISequence =
    abstract member GetNext: sequenceName: string -> Task<uint64>
    abstract member PeekCurrent: sequenceName: string -> Task<uint64>

type DecryptionFailure = DecryptionFailure

type ICryptographer =
    abstract member Encrypt: decrypted: byte[] -> purpose: string -> byte[]
    abstract member Decrypt: encrypted: byte[] -> purpose: string -> Result<byte[], DecryptionFailure>

type Clock =
| Now of ResponseChannel<DateTimeOffset>
with interface Request

type Sequence =
| GetNext of Name: string * ResponseChannel<uint64>
| PeekCurrent of Name: string * ResponseChannel<uint64>
with interface Request

type Random =
| RandomIntegerBetween of Min: int * Max: int * ResponseChannel<int>
with interface Request

type Unique =
| NewUuid of ResponseChannel<Guid>
with interface Request

type Cryptographer =
| Encrypt of Decrypted: string * Purpose: string * ResponseChannel<byte[]>
| Decrypt of Decrypted: byte[] * Purpose: string * ResponseChannel<Result<string, DecryptionFailure>>
| EncryptBytes of Decrypted: byte[] * Purpose: string * ResponseChannel<byte[]>
| DecryptBytes of Encrypted: byte[] * Purpose: string * ResponseChannel<Result<byte[], DecryptionFailure>>
with interface Request

type ActionContext =
| CurrentUserId     of ResponseChannel<string>
| CurrentSessionId  of ResponseChannel<Option<string>>
| CurrentSubjectRef of ResponseChannel<LocalSubjectPKeyReference>
with interface Request

let internal allSubjectsServiceHandler
        (repoImpl: ISubjectRepo<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'SubjectIndex, 'OpError>)
        (cache:    InMemoryCache)
        (action:   All<'Subject, 'SubjectId, 'SubjectIndex, 'OpError>)
        : Task<ResponseVerificationToken> =
    match action with

    | DoesExistById (id, responseChannel) ->
        backgroundTask {
            let! res = repoImpl.DoesExistById id
            return responseChannel.Respond res
        }

    | Any (predicate, responseChannel) ->
        backgroundTask {
            let! res = repoImpl.Any predicate
            return responseChannel.Respond res
        }

    | GetById (id, responseChannel) ->
        backgroundTask {
            let! res = repoImpl.GetById id
            return responseChannel.Respond (res |> Option.map VersionedSubject.subject)
        }

    | GetByIds (ids, responseChannel) ->
        backgroundTask {
            let! res = repoImpl.GetByIds ids
            return responseChannel.Respond (res |> List.map VersionedSubject.subject)
        }

    | GetByIdCached (id, cachedSubjectExpirationPeriod, responseChannel) ->
        backgroundTask {
            let! res = cache.TryFind id repoImpl.GetById cachedSubjectExpirationPeriod
            return responseChannel.Respond (res |> Option.map VersionedSubject.subject)
        }

    | FilterFetchIds (query, responseChannel) ->
        backgroundTask {
            let! res = repoImpl.FilterFetchIds query
            return responseChannel.Respond res
        }

    | FilterFetchSubjects (query, responseChannel) ->
        backgroundTask {
            let! res = repoImpl.FilterFetchSubjects query
            return responseChannel.Respond (res |> List.map VersionedSubject.subject)
        }

    | FilterFetchSubjectsWithTotalCount (query, responseChannel) ->
        backgroundTask {
            let! res = repoImpl.FilterFetchSubjectsWithTotalCount query
            return responseChannel.Respond (res |> (fun (subjects, count) -> subjects |> List.map VersionedSubject.subject, count))
        }

    | FilterCountSubjects (predicate, responseChannel) ->
        backgroundTask {
            let! res = repoImpl.FilterCountSubjects predicate
            return responseChannel.Respond res
        }

    | FilterFetchSubject (predicate, responseChannel) ->
        backgroundTask {
            let resultSetOptions = {
                UntypedResultSetOptions.Page = { Offset = 0UL; Size = 1us }
                UntypedResultSetOptions.OrderBy = UntypedOrderBy.FastestOrSingleSearchScoreIfAvailable
            }
            let! res = repoImpl.FilterFetchSubjects (predicate.PrepareQuery resultSetOptions)
            return responseChannel.Respond (res |> Seq.tryHead |> Option.map VersionedSubject.subject)
        }

    | FetchWithHistory (id, fromLastUpdatedOn, toLastUpdatedOn, page, responseChannel) ->
        backgroundTask {
            let! temporalSnapshot = repoImpl.FetchWithHistoryById id fromLastUpdatedOn toLastUpdatedOn page
            let res = temporalSnapshot |> List.map (fun s -> { AsOf = s.AsOf; Subject = s.Subject })
            return responseChannel.Respond res
        }

    | FetchAll (resultSetOptions, responseChannel) ->
        backgroundTask {
            let! res = repoImpl.FetchAllSubjects resultSetOptions
            return responseChannel.Respond (res |> List.map VersionedSubject.subject)
        }

    | CountAll responseChannel ->
        backgroundTask {
            let! res = repoImpl.CountAllSubjects ()
            return responseChannel.Respond res
        }

let allBlobsServiceHandler (repoImpl: IBlobRepo) (action: AllBlobs) : Task<ResponseVerificationToken> =
    match action with

    | GetBlobDataById (ecosystemName, id, responseChannel) ->
        backgroundTask {
            let! res = repoImpl.GetBlobData ecosystemName id.Owner id.Id
            return responseChannel.Respond res
        }

let internal allTimeSeriesServiceHandler
        (repoImpl: ITimeSeriesRepo<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'TimeSeriesIndex>)
        (action:   AllTimeSeries<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'TimeSeriesIndex>)
        : Task<ResponseVerificationToken> =
    match action with

    | TimeSeriesValues (interval, responseChannel) ->
        backgroundTask {
            let! res = repoImpl.Values interval
            return responseChannel.Respond res
        }

    | TimeSeriesDataPoints (interval, responseChannel) ->
        backgroundTask {
            let! res = repoImpl.DataPoints interval
            return responseChannel.Respond res
        }

    | TimeSeriesBucketedValues (interval, bucket, aggregate, responseChannel) ->
        backgroundTask {
            let! res = repoImpl.BucketedValues interval bucket aggregate
            return responseChannel.Respond res
        }

    | TimeSeriesBucketedValuesBy (groupInterval, bucket, aggregate, responseChannel) ->
        backgroundTask {
            let! res = repoImpl.BucketedValuesBy groupInterval bucket aggregate
            return responseChannel.Respond res
        }

    | TimeSeriesBucketedDataPoints (interval, bucket, aggregate, responseChannel) ->
        backgroundTask {
            let! res = repoImpl.BucketedDataPoints interval bucket aggregate
            return responseChannel.Respond res
        }

    | TimeSeriesBucketedDataPointsBy (groupInterval, bucket, aggregate, responseChannel) ->
        backgroundTask {
            let! res = repoImpl.BucketedDataPointsBy groupInterval bucket aggregate
            return responseChannel.Respond res
        }

    | TimeSeriesAggregateValue (interval, aggregate, responseChannel) ->
        backgroundTask {
            let! res = repoImpl.AggregateValue interval aggregate
            return responseChannel.Respond res
        }

    | TimeSeriesAggregateValuesBy (groupInterval, aggregate, responseChannel) ->
        backgroundTask {
            let! res = repoImpl.AggregateValuesBy groupInterval aggregate
            return responseChannel.Respond res
        }

    | TimeSeriesAggregateDataPoint (interval, aggregate, responseChannel) ->
        backgroundTask {
            let! res = repoImpl.AggregateDataPoint interval aggregate
            return responseChannel.Respond res
        }

    | TimeSeriesAggregateDataPointsBy (groupInterval, aggregate, responseChannel) ->
        backgroundTask {
            let! res = repoImpl.AggregateDataPointsBy groupInterval aggregate
            return responseChannel.Respond res
        }

    | TimeSeriesCountDataPoints (interval, responseChannel) ->
        backgroundTask {
            let! res = repoImpl.CountDataPoints interval
            return responseChannel.Respond res
        }

    | TimeSeriesCountDataPointsBy (groupInterval, responseChannel) ->
        backgroundTask {
            let! res = repoImpl.CountDataPointsBy groupInterval
            return responseChannel.Respond res
        }

let clockHandler (action: Clock) : Task<ResponseVerificationToken> =
    match action with
    | Now responseChannel ->
        responseChannel.Respond DateTimeOffset.UtcNow |> Task.FromResult

let sequenceHandler (sequenceImpl: ISequence) (action: Sequence) : Task<ResponseVerificationToken> =
    match action with
    | GetNext(sequenceName, responseChannel) ->
        backgroundTask {
            let! next = sequenceImpl.GetNext sequenceName
            return responseChannel.Respond(next)
        }
    | PeekCurrent(sequenceName, responseChannel) ->
        backgroundTask {
            let! next = sequenceImpl.PeekCurrent sequenceName
            return responseChannel.Respond(next)
        }

let randomHandler (action: Random) : Task<ResponseVerificationToken> =
    match action with
    | RandomIntegerBetween (min, max, responseChannel) ->
        System.Random().Next(min, max)
        |> responseChannel.Respond
        |> Task.FromResult

let uniqueHandler (action: Unique) : Task<ResponseVerificationToken> =
    match action with
    | NewUuid responseChannel ->
        Guid.NewGuid()
        |> responseChannel.Respond
        |> Task.FromResult

let actionContextHandler (actionContextImpl: IActionContext) (maybePKey: Option<LocalSubjectPKeyReference>) (action: ActionContext) : Task<ResponseVerificationToken> =
    match action with
    | CurrentUserId responseChannel ->
        actionContextImpl.GetUserId()
        |> responseChannel.Respond
        |> Task.FromResult
    | CurrentSessionId responseChannel ->
        actionContextImpl.GetSessionId()
        |> responseChannel.Respond
        |> Task.FromResult
    | CurrentSubjectRef responseChannel ->
        match maybePKey with
        | Some pKey ->
            pKey
            |> responseChannel.Respond
            |> Task.FromResult
        | None ->
            failwith "CurrentSubjectRef can't be invoked outside a subject context"

let cryptographerHandler (cryptographer: ICryptographer) (action: Cryptographer) : Task<ResponseVerificationToken> =
    match action with
    | Encrypt(decrypted, purpose, responseChannel) ->
        Encoding.UTF8.GetBytes decrypted
        |> fun bytes -> cryptographer.Encrypt bytes purpose
        |> responseChannel.Respond
        |> Task.FromResult

    | Decrypt(encrypted, purpose, responseChannel) ->
        cryptographer.Decrypt encrypted purpose
        |> Result.map Encoding.UTF8.GetString
        |> responseChannel.Respond
        |> Task.FromResult

    | EncryptBytes(decrypted, purpose, responseChannel) ->
        cryptographer.Encrypt decrypted purpose
        |> responseChannel.Respond
        |> Task.FromResult

    | DecryptBytes(encrypted, purpose, responseChannel) ->
        cryptographer.Decrypt encrypted purpose
        |> responseChannel.Respond
        |> Task.FromResult