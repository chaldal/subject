namespace LibLifeCycleHost.Storage.Volatile

open System
open LibLifeCycle.LifeCycles.Meta
open Orleans
open LibLifeCycle
open LibLifeCycleCore
open LibLifeCycleHost
open System.Threading.Tasks

type VolatileSubjectRepo<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId
                          when 'Subject              :> Subject<'SubjectId>
                          and  'LifeAction           :> LifeAction
                          and  'OpError              :> OpError
                          and  'Constructor          :> Constructor
                          and  'LifeEvent            :> LifeEvent
                          and  'LifeEvent            : comparison
                          and  'SubjectIndex         :> SubjectIndex<'OpError>
                          and  'SubjectId            :> SubjectId
                          and  'SubjectId            : comparison>(hostEcosystemGrainFactory: IGrainFactory, grainPartition: GrainPartition) =

    let (GrainPartition grainPartitionGuid) = grainPartition

    // TODO; need to work on in-memory indices
    // This is likely going to get solved by moving this over to Service Fabric Volatile Reliable Collections
    // Until then, process starts will dump this memory
    interface ISubjectRepo<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'SubjectIndex, 'OpError> with

        member _.DoesExistById (id: 'SubjectId): Task<bool> =
            let grain = hostEcosystemGrainFactory.GetGrain<ISubjectGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>>(grainPartitionGuid, (getIdString id))
            fun () -> grain.IsConstructed()
            |> OrleansTransientErrorDetection.wrapTransientExceptions

        member this.GetById (id: 'SubjectId): Task<Option<VersionedSubject<'Subject, 'SubjectId>>> =
            getIdString id
            |> (this :> ISubjectRepo<_, _, _, _, _, _>).GetByIdStr

        member _.GetByIdStr (idStr: string): Task<Option<VersionedSubject<'Subject, 'SubjectId>>> =
            fun () -> backgroundTask {
                let grain = hostEcosystemGrainFactory.GetGrain<ISubjectGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>>(grainPartitionGuid, idStr)
                let! current = grain.Get { SessionHandle = SessionHandle.NoSession; CallOrigin = CallOrigin.Internal }
                let maybeVersionedSubject =
                    match current with
                    | Ok maybeSubject -> maybeSubject
                    | Error GrainGetError.AccessDenied -> failwith "Unexpected access denial"
                return maybeVersionedSubject
            }
            |> OrleansTransientErrorDetection.wrapTransientExceptions

        member this.GetByIds (ids: Set<'SubjectId>): Task<List<VersionedSubject<'Subject, 'SubjectId>>> =
            ids
            |> Set.map getIdString
            |> (this :> ISubjectRepo<_, _, _, _, _, _>).GetByIdsStr

        member this.GetByIdsStr (ids: Set<string>): Task<List<VersionedSubject<'Subject, 'SubjectId>>> =
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

        member this.Any(_predicate: PreparedIndexPredicate<'SubjectIndex>): Task<bool> =
            // QQQ // This requires moving to an actual volatile storage, such as Service Fabric Volatile Reliable Collections
            NotImplementedException "Volatile storage can't query subjects" |> raise

        member this.FilterFetchIds (_query: IndexQuery<'SubjectIndex>) : Task<List<'SubjectId>> =
            // QQQ // This requires moving to an actual volatile storage, such as Service Fabric Volatile Reliable Collections
            NotImplementedException "Volatile storage can't query subjects" |> raise

        member this.FilterFetchSubjects (_query: IndexQuery<'SubjectIndex>) : Task<List<VersionedSubject<'Subject, 'SubjectId>>> =
            // QQQ // This requires moving to an actual volatile storage, such as Service Fabric Volatile Reliable Collections
            NotImplementedException "Volatile storage can't query subjects" |> raise


        member this.FilterFetchSubjectsWithTotalCount(_query: IndexQuery<'SubjectIndex>) : Task<List<VersionedSubject<'Subject, 'SubjectId>> * uint64> =
            // QQQ // This requires moving to an actual volatile storage, such as Service Fabric Volatile Reliable Collections
            NotImplementedException "Volatile storage can't query subjects" |> raise

        member this.FilterCountSubjects (_predicate: PreparedIndexPredicate<'SubjectIndex>) : Task<uint64> =
            // QQQ // This requires moving to an actual volatile storage, such as Service Fabric Volatile Reliable Collections
            NotImplementedException "Volatile storage can't query subjects" |> raise

        member this.FetchAllSubjects (_resultSetOptions: ResultSetOptions<'SubjectIndex>): System.Threading.Tasks.Task<List<VersionedSubject<'Subject, 'SubjectId>>> =
            // QQQ // This requires moving to an actual volatile storage, such as Service Fabric Volatile Reliable Collections
            NotImplementedException "Volatile storage can't query subjects" |> raise

        member this.CountAllSubjects (): System.Threading.Tasks.Task<uint64> =
            // QQQ // This requires moving to an actual volatile storage, such as Service Fabric Volatile Reliable Collections
            NotImplementedException "Volatile storage can't query subjects" |> raise

        member this.FetchAllSubjectsWithTotalCount (_resultSetOptions: ResultSetOptions<'SubjectIndex>): System.Threading.Tasks.Task<List<VersionedSubject<'Subject, 'SubjectId>> * uint64> =
            // QQQ // This requires moving to an actual volatile storage, such as Service Fabric Volatile Reliable Collections
            NotImplementedException "Volatile storage re doesn't support history" |> raise


        member this.GetVersionSnapshotByIdStr (_idStr: string) (_ofVersion: GetSnapshotOfVersion) : Task<Option<TemporalSnapshot<'Subject, 'LifeAction, 'Constructor, 'SubjectId>>> =
            NotImplementedException "Volatile storage doesn't support audit" |> raise

        member this.GetVersionSnapshotById (_id: 'SubjectId) (_ofVersion: GetSnapshotOfVersion) : Task<Option<TemporalSnapshot<'Subject, 'LifeAction, 'Constructor, 'SubjectId>>> =
            NotImplementedException "Volatile storage doesn't support audit" |> raise

        member this.FetchWithHistoryById (_id: 'SubjectId) (_fromLastUpdatedOn: Option<DateTimeOffset>) (_toLastUpdatedOn: Option<DateTimeOffset>) (_page: ResultPage): Task<List<TemporalSnapshot<'Subject, 'LifeAction, 'Constructor, 'SubjectId>>> =
            NotImplementedException "Volatile storage doesn't support history" |> raise

        member _.FetchWithHistoryByIdStr (_idStr: string) (_fromLastUpdatedOn: Option<DateTimeOffset>) (_toLastUpdatedOn: Option<DateTimeOffset>) (_page: ResultPage): Task<List<TemporalSnapshot<'Subject, 'LifeAction, 'Constructor, 'SubjectId>>> =
            NotImplementedException "Volatile storage doesn't support history" |> raise

        member _.GetSideEffectPermanentFailures (_scope: UpdatePermanentFailuresScope): Task<List<SideEffectPermanentFailure>> =
            Task.FromResult []

        member this.FetchAuditTrail _idStr _page =
            NotImplementedException "Volatile storage doesn't support history" |> raise

type VolatileSubjectBlobRepo () =
    interface IBlobRepo with
        member _.GetBlobData (_ecosystemName: string) (_subjectRef: LocalSubjectPKeyReference) (_blobId: Guid) : Task<Option<BlobData>> =
            // QQQ // This requires moving to an actual volatile storage, such as Service Fabric Volatile Reliable Collections
            NotImplementedException "Blob volatile storage not available" |> raise

        member _.GetBlobDataStream (_ecosystemName: string) (_subjectRef: LocalSubjectPKeyReference) (_blobId: Guid) (_readBlobDataStream: Option<BlobDataStream> -> Task) : Task =
            NotImplementedException "Blob volatile storage not available" |> raise
