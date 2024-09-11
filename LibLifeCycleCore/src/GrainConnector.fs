namespace LibLifeCycleCore

open System
open System.Threading
open System.Threading.Tasks
open LibLifeCycleTypes
open Microsoft.FSharp.Core
open Orleans

type GrainConnector (grainFactory: IGrainFactory, grainPartition: GrainPartition, sessionHandle: SessionHandle, callOrigin: CallOrigin) =
    let (GrainPartition partitionGuid) = grainPartition

    let clientGrainCallContext =
        {
            SessionHandle = sessionHandle
            CallOrigin = callOrigin
        }

    member _.SessionHandle = sessionHandle

    member this.GetRepoGrainForLifeCycle
        (_lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>) =
        let idStr = "Repo" // idStr is a constant, combination of generic param must point to correct grain unambiguously
        grainFactory.GetGrain<ISubjectRepoGrain<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'SubjectIndex, 'OpError>>(partitionGuid, idStr)

    member this.GetGrainByIdString
            (_lifeCycle: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (idStr: string)
            : ISubjectClientGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId> =
        grainFactory.GetGrain<ISubjectClientGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>>(partitionGuid, idStr)

    member this.GetGrain
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (id: 'SubjectId)
            : ISubjectClientGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId> =
        this.GetGrainByIdString lifeCycleDef (getIdString id)

    member this.DoesSubjectExist
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (id: 'SubjectId)
            : Task<bool> =
        let grain = this.GetGrain lifeCycleDef id
        grain.IsConstructed()

    member this.GetSubjectFromGrain
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (idStr: string)
            : Task<Result<Option<VersionedSubject<'Subject, 'SubjectId>>, GrainGetError>> =
        let grain = this.GetGrainByIdString lifeCycleDef idStr
        grain.Get clientGrainCallContext

    member this.GetPreparedSubjectFromGrain
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (idStr: string)
            (transactionId: SubjectTransactionId)
            : Task<Result<Option<'Subject>, GrainGetError>> =
        let grain = this.GetGrainByIdString lifeCycleDef idStr
        grain.Prepared clientGrainCallContext transactionId

    member this.GetSubjectVersionSnapshot
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (idStr: string)
            (ofVersion: GetSnapshotOfVersion)
            : Task<Option<TemporalSnapshot<'Subject, 'LifeAction, 'Constructor, 'SubjectId>>> =
        let grain = this.GetRepoGrainForLifeCycle lifeCycleDef
        grain.GetVersionSnapshotByIdStr idStr ofVersion

    member this.GetSubjectVersionSnapshotById
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (id: 'SubjectId)
            (ofVersion: GetSnapshotOfVersion)
            : Task<Option<TemporalSnapshot<'Subject, 'LifeAction, 'Constructor, 'SubjectId>>> =
        let grain = this.GetRepoGrainForLifeCycle lifeCycleDef
        grain.GetVersionSnapshotById id ofVersion

    member this.GetSubjectFromGrainById
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (id: 'SubjectId)
            : Task<Result<Option<VersionedSubject<'Subject, 'SubjectId>>, GrainGetError>> =
        this.GetSubjectFromGrain lifeCycleDef (id :> SubjectId).IdString


    member this.RunRead
            (viewDef: ViewDef<'Input, 'Output, 'OpError>)
            (input: 'Input) =
        let grain = grainFactory.GetGrain<IViewClientGrain<'Input, 'Output, 'OpError>>(partitionGuid, viewDef.Name)
        grain.Read clientGrainCallContext input

    member private this.RunAndWait
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (idStr: string)
            (timeout: TimeSpan)
            (runAndAwaitImpl:
                ISubjectClientGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId> ->
                ILifeEventAwaiter<'Subject, 'LifeEvent, 'SubjectId> ->
                Task<Result<VersionedSubject<'Subject, 'SubjectId>, 'Error>>)
            : Task<Result<ActOrConstructAndWaitOnLifeEventResult<'Subject, 'SubjectId, 'LifeEvent>, 'Error>> =

        let grain = this.GetGrainByIdString lifeCycleDef idStr

        // See the comment below - need it to keep awaiter alive. Simple `ignore awaiter` or similar
        // gets optimized away in Release so I had to write this nonsense if-condition which will never
        // be true but optimizer can't know it.
        let nonOptimizableIgnore (x: obj) =
            if x <> null && DateTime.Now.Year = -1 then
                failwith "This code is unreachable"

        backgroundTask {
            let taskCompletionSource = TaskCompletionSource<ActOrConstructAndWaitOnLifeEventResult<'Subject, 'SubjectId, 'LifeEvent>>()

            // NOTE: this is held as a weak reference, and the reference needs to be held in order to receive the event
            // However I strongly suspect that the perfect storm of being inside a CE + Weak Reference + F# compiler optimizations
            // is causing this reference (and the underlying object) to get released if we go over a bind (such as a let!)
            // Now this is just a guess, but so far my observations have corroborated this theory.
            // Workaround, see the addition of `ignore awaiter` below.
            let awaiter =
                { new ILifeEventAwaiter<'Subject, 'LifeEvent, 'SubjectId> with
                    member this.EventTriggered (versionedSubject: VersionedSubject<'Subject, 'SubjectId>) (lifeEvent: 'LifeEvent) =
                        LifeEventTriggered(versionedSubject, lifeEvent) |> taskCompletionSource.TrySetResult |> ignore
                }

            use cancellationTokenSource = new CancellationTokenSource(timeout)

            use _disposable = cancellationTokenSource.Token.Register(fun _ -> taskCompletionSource.TrySetCanceled() |> ignore)
            let! objRef = grainFactory.CreateObjectReference<ILifeEventAwaiter<'Subject, 'LifeEvent, 'SubjectId>> awaiter
            match! runAndAwaitImpl grain objRef with
            | Ok versionedSubj ->
                return! backgroundTask {
                    try
                        let! res = taskCompletionSource.Task
                        nonOptimizableIgnore awaiter // See note above
                        return res |> Ok
                    with
                    | :? TaskCanceledException ->
                        nonOptimizableIgnore awaiter // See note above
                        return versionedSubj |> WaitOnLifeEventTimedOut |> Ok
                }
            | Error err ->
                nonOptimizableIgnore awaiter // See note above
                return err |> Error
        }

    member this.GenerateId
            (_lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (ctor: 'Constructor)
            : Task<Result<'SubjectId, GrainIdGenerationError<'OpError>>> =

        let idStr = "IdGen" // idStr is a constant, combination of generic param must point to correct grain unambiguously
        let grain = grainFactory.GetGrain<ISubjectIdGenerationGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>>(partitionGuid, idStr)
        grain.GenerateIdV2 CallOrigin.Internal ctor

    member this.Construct
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (id: 'SubjectId)
            (ctor: 'Constructor)
            : Task<Result<VersionedSubject<'Subject, 'SubjectId>, GrainConstructionError<'OpError>>> =
        let grain = this.GetGrain lifeCycleDef id
        grain.Construct clientGrainCallContext id ctor

    member this.ConstructNoContent
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (id: 'SubjectId)
            (ctor: 'Constructor)
            : Task<Result<unit, GrainConstructionError<'OpError>>> =
        let grain = this.GetGrain lifeCycleDef id
        grain.ConstructNoContent clientGrainCallContext id ctor

    member this.ConstructAndWait
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (id: 'SubjectId)
            (ctor: 'Constructor)
            (lifeEvent: 'LifeEvent)
            (timeout: TimeSpan)
            : Task<Result<ActOrConstructAndWaitOnLifeEventResult<'Subject, 'SubjectId, 'LifeEvent>, GrainConstructionError<'OpError>>> =
            this.RunAndWait
                lifeCycleDef
                (id :> SubjectId).IdString
                timeout
                (fun grain objRef ->
                    grain.ConstructAndWait clientGrainCallContext id ctor lifeEvent objRef timeout)

    member this.GetMaybeConstruct
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (id: 'SubjectId)
            (constructor: 'Constructor)
            : Task<Result<VersionedSubject<'Subject, 'SubjectId>, GrainMaybeConstructionError<'OpError>>> =
        let grain = this.GetGrain lifeCycleDef id
        grain.GetMaybeConstruct clientGrainCallContext id constructor

    member this.MaybeConstructNoContent
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (id: 'SubjectId)
            (constructor: 'Constructor)
            : Task<Result<unit, GrainMaybeConstructionError<'OpError>>> =
        let grain = this.GetGrain lifeCycleDef id
        grain.MaybeConstructNoContent clientGrainCallContext id constructor

    member this.GetById
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (id: 'SubjectId)
            : Task<Result<Option<VersionedSubject<'Subject, 'SubjectId>>, GrainGetError>> =
        let grain = this.GetGrain lifeCycleDef id
        grain.Get clientGrainCallContext

    member this.GetByIdStr
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (idStr: string)
            : Task<Result<Option<VersionedSubject<'Subject, 'SubjectId>>, GrainGetError>> =
        let grain = this.GetGrainByIdString lifeCycleDef idStr
        grain.Get clientGrainCallContext

    member this.Act
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (idStr: string)
            (action: 'LifeAction)
            : Task<Result<VersionedSubject<'Subject, 'SubjectId>, GrainTransitionError<'OpError>>> =
        let grain = this.GetGrainByIdString lifeCycleDef idStr
        grain.Act clientGrainCallContext action

    member this.ActNoContent
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (idStr: string)
            (action: 'LifeAction)
            : Task<Result<unit, GrainTransitionError<'OpError>>> =
        let grain = this.GetGrainByIdString lifeCycleDef idStr
        grain.ActNoContent clientGrainCallContext action

    member this.ActMaybeConstruct
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (idStr: string)
            (action  :    'LifeAction)
            (constructor: 'Constructor)
            : Task<Result<VersionedSubject<'Subject, 'SubjectId>, GrainOperationError<'OpError>>> =
        let grain = this.GetGrainByIdString lifeCycleDef idStr
        grain.ActMaybeConstruct clientGrainCallContext action constructor

    member this.ActMaybeConstructNoContent
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (idStr: string)
            (action  :    'LifeAction)
            (constructor: 'Constructor)
            : Task<Result<unit, GrainOperationError<'OpError>>> =
        let grain = this.GetGrainByIdString lifeCycleDef idStr
        grain.ActMaybeConstructNoContent clientGrainCallContext action constructor

    member this.ActAndWait
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (idStr: string)
            (action: 'LifeAction)
            (lifeEvent: 'LifeEvent)
            (timeout: TimeSpan)
            : Task<Result<ActOrConstructAndWaitOnLifeEventResult<'Subject, 'SubjectId, 'LifeEvent>, GrainTransitionError<'OpError>>> =

            // TODO: fix, functions accepts idStr but also generates Id within grain.RunActionAndAwait - potentially different

            this.RunAndWait
                lifeCycleDef
                idStr
                timeout
                (fun grain objRef ->
                    grain.ActAndWait clientGrainCallContext action lifeEvent objRef timeout)

    member this.ActMaybeConstructAndWait
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (idStr: string)
            (action: 'LifeAction)
            (constructor: 'Constructor)
            (lifeEvent: 'LifeEvent)
            (timeout: TimeSpan)
            : Task<Result<ActOrConstructAndWaitOnLifeEventResult<'Subject, 'SubjectId, 'LifeEvent>, GrainOperationError<'OpError>>> =

            this.RunAndWait
                lifeCycleDef
                idStr
                timeout
                (fun grain objRef ->
                    grain.ActMaybeConstructAndWait clientGrainCallContext action constructor lifeEvent objRef timeout)

    member this.GetByIds
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (ids: Set<'SubjectId>)
            : Task<List<VersionedSubject<'Subject, 'SubjectId>>> =
        let grain = this.GetRepoGrainForLifeCycle lifeCycleDef
        grain.Versioned_GetByIds ids

    member this.GetByIdsStr
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (idsStr: Set<string>)
            : Task<List<VersionedSubject<'Subject, 'SubjectId>>> =
        let grain = this.GetRepoGrainForLifeCycle lifeCycleDef
        grain.Versioned_GetByIdsStr idsStr

    member this.Any
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (predicate: PreparedIndexPredicate<'SubjectIndex>)
            : Task<bool> =
        let grain = this.GetRepoGrainForLifeCycle lifeCycleDef
        grain.Any predicate

    member this.FilterFetchSubjects
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (query: IndexQuery<'SubjectIndex>)
            : Task<List<VersionedSubject<'Subject, 'SubjectId>>> =
        let grain = this.GetRepoGrainForLifeCycle lifeCycleDef
        grain.Versioned_FilterFetchSubjects query

    member this.FilterFetchIds
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (query: IndexQuery<'SubjectIndex>)
            : Task<List<'SubjectId>> =
        let grain = this.GetRepoGrainForLifeCycle lifeCycleDef
        grain.FilterFetchIds query

    member this.FilterCountSubjects
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (predicate: PreparedIndexPredicate<'SubjectIndex>)
            : Task<uint64> =
        let grain = this.GetRepoGrainForLifeCycle lifeCycleDef
        grain.FilterCountSubjects predicate

    member this.FilterFetchSubjectsWithTotalCount
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (query: IndexQuery<'SubjectIndex>):
            Task<List<VersionedSubject<'Subject, 'SubjectId>> * uint64> =
        let grain = this.GetRepoGrainForLifeCycle lifeCycleDef
        grain.Versioned_FilterFetchSubjectsWithTotalCount query

    member this.FetchAllSubjectsWithTotalCount
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (resultSetOptions: ResultSetOptions<'SubjectIndex>)
            : Task<List<VersionedSubject<'Subject, 'SubjectId>> * uint64> =
        let grain = this.GetRepoGrainForLifeCycle lifeCycleDef
        grain.FetchAllSubjectsWithTotalCount resultSetOptions

    member this.FetchWithHistoryByIdStr
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (idStr: string)
            (fromLastUpdatedOn: Option<DateTimeOffset>)
            (toLastUpdatedOn: Option<DateTimeOffset>)
            (page: ResultPage)
            : Task<List<TemporalSnapshot<'Subject, 'LifeAction, 'Constructor, 'SubjectId>>> =
        let grain = this.GetRepoGrainForLifeCycle lifeCycleDef
        grain.FetchWithHistoryByIdStr idStr fromLastUpdatedOn toLastUpdatedOn page

    member this.FetchAuditTrail
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (idStr: string)
            (page: ResultPage)
            : Task<List<SubjectAuditData<'LifeAction, 'Constructor>>> =
        let grain = this.GetRepoGrainForLifeCycle lifeCycleDef
        grain.FetchAuditTrail idStr page

    member this.FilterFetchAllSubjects
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (resultSetOptions: ResultSetOptions<'SubjectIndex>)
            : Task<List<VersionedSubject<'Subject, 'SubjectId>>> =
        let grain = this.GetRepoGrainForLifeCycle lifeCycleDef
        grain.Versioned_FetchAllSubjects resultSetOptions

    member this.CountAllSubjects
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            : Task<uint64> =
        let grain = this.GetRepoGrainForLifeCycle lifeCycleDef
        grain.CountAllSubjects ()

    member this.GetReflectionGrainForLifeCycle
        (_lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>) =
        let idStr = "Reflection" // idStr is a constant, combination of generic param must point to correct grain unambiguously
        grainFactory.GetGrain<ISubjectReflectionGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>>(partitionGuid, idStr)

    member this.IsActionAllowedForSubject
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (subject: 'Subject)
            (action: 'LifeAction)
        : Task<bool>=
        let grain = this.GetReflectionGrainForLifeCycle lifeCycleDef
        grain.IsActionAllowed subject action

    member this.AllowedActionsForSubject
            (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
            (subject: 'Subject)
        : Task<list<string>>=
        let grain = this.GetReflectionGrainForLifeCycle lifeCycleDef
        grain.GetAllowedActionCaseNames subject

    member this.GetBlobRepoGrain () =
        let idStr = "BlobRepo"
        grainFactory.GetGrain<IBlobRepoGrain>(partitionGuid, idStr)

    member this.BlobData (subjectRef: LocalSubjectPKeyReference) (blobId: Guid)
        : Task<Option<BlobData>> =
        let grain = this.GetBlobRepoGrain()
        grain.GetBlobData subjectRef blobId