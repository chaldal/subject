namespace LibUiSubject.Services.SubjectService

open System
open Fable.SignalR
open LibClient.EventBus
open LibClient
open LibClient.Services.Subscription
open LibClient.Services.HttpService.ThothEncodedHttpService
open LibUiSubject
open LibUiSubject.Services.RealTimeService
open LibLifeCycleTypes.Api.V1
open LibUiSubject.Services.SubjectService
open Fable.Core.Reflection

// need this because even though it's auto-opened, Fable.SignalR.Subject clobbers our Subject
open LibLifeCycleTypes_SubjectTypes

[<AutoOpen>]
module private Helpers =
    let unfortunateSafetyHack<'T> (rawSubjectsAD: AsyncData<seq<'T>>) : AsyncData<seq<'T>> =
        // NOTE XXX this is VERY DISTURBING, but it seems like we're running into
        // an issue where a seq is somehow backed by a consumable data structure.
        // This has happened once before, and we've addressed it in FableHacks.fs,
        // but it's now happening again. The way this behaviour manifests in cookups
        // is that on the dish screen, after about a minute, if you toggle the shopping
        // cart back-and-forth, eventually the whole page drops into NotFound, because
        // the collection was consumed.
        rawSubjectsAD |> AsyncData.map (List.ofSeq >> Seq.ofList)

type private StreamCreationResult = {
    ObserverDisposable: IDisposable
}

type private SubscriptionImplementationCreationResult = {
    AlreadyRemoved: bool
}

type private SubscribeManyHelperInput<'Id, 'Projection when 'Id : comparison> =
| WithoutCount of Get: (unit -> Async<AsyncData<Subjects<'Id, 'Projection>>>)               * Subscriber: (AsyncData<Subjects<'Id, 'Projection>> -> unit)
| WithCount    of Get: (unit -> Async<AsyncData<SubjectsWithTotalCount<'Id, 'Projection>>>) * Subscriber: (AsyncData<SubjectsWithTotalCount<'Id, 'Projection>> -> unit)

[<RequireQualifiedAccess>]
type private QueuedMessage =
| Next of ServerStreamApi
| Error of Option<exn>

type private OnResumeCallback = unit -> unit

type Query<'Id, 'Index, 'OpError when 'OpError :> OpError and 'Index :> SubjectIndex<'OpError>> =
| All     of ResultSetOptions: ResultSetOptions<'Index>
| One     of 'Id
| Indexed of Query: IndexQuery<'Index>


type ActionOrConstructionError<'OpError> =
| OpError of 'OpError
| SubjectAlreadyInitialized
| SubjectLockedInTransaction
| InsufficientPermissions
| SucceededButInsufficientPermissionsToReadResult
| NetworkFailure
| TooManyRequests
with
    static member TryToErrorMessage (error: ActionOrConstructionError<'OpError>) : Result<string, 'OpError> =
        match error with
        | OpError opError                                 -> Error opError
        | SubjectAlreadyInitialized                       -> Ok "Subject with the given id already exists"
        | SubjectLockedInTransaction                      -> Ok "Subject is locked in transaction, please wait 5 minutes"
        | InsufficientPermissions                         -> Ok "Insufficient permissions"
        | SucceededButInsufficientPermissionsToReadResult -> Ok "Action succeeded, but you do not have sufficient permissions to view the result"
        | NetworkFailure                                  -> Ok "A network failure has occurred"
        | TooManyRequests                                 -> Ok "Too many requests"

    member this.ToDisplayString (opErrorMapper: 'OpError -> string) : string =
        match this |> ActionOrConstructionError.TryToErrorMessage with
        | Ok message    -> message
        | Error opError -> opErrorMapper opError


type ISubjectService<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError
                      when 'Subject      :> Subject<'Id>
                      and  'Id           :> SubjectId
                      and  'Id           :  comparison
                      and  'Constructor  :> Constructor
                      and  'Action       :> LifeAction
                      and  'Event        :> LifeEvent
                      and  'OpError      :> OpError
                      and  'Index        :> SubjectIndex<'OpError>> =
    abstract member EventQueue: EventBus.Queue<SubjectServiceEvent<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>>

    abstract member LifeCycleKey:                   LifeCycleKey
    abstract member SubscribeOne:                   id: 'Id -> useCache: UseCache -> subscriber: (AsyncData<'Projection> -> unit) -> SubscribeResult
    abstract member SubscribeMany:                  ids: Set<'Id> -> useCache: UseCache -> subscriber: (AsyncData<Subjects<'Id, 'Projection>> -> unit) -> SubscribeResult
    abstract member SubscribeAll:                   resultSetOptions: ResultSetOptions<'Index> -> useCache: UseCache -> subscriber: (AsyncData<Subjects<'Id, 'Projection>> -> unit) -> SubscribeResult
    abstract member SubscribeAllWithTotalCount:     resultSetOptions: ResultSetOptions<'Index> -> useCache: UseCache -> subscriber: (AsyncData<SubjectsWithTotalCount<'Id, 'Projection>> -> unit) -> SubscribeResult
    abstract member SubscribeIndexed:               query: IndexQuery<'Index> -> useCache: UseCache -> subscriber: (AsyncData<Subjects<'Id, 'Projection>> -> unit) -> SubscribeResult
    abstract member SubscribeIndexedWithTotalCount: query: IndexQuery<'Index> -> useCache: UseCache -> subscriber: (AsyncData<SubjectsWithTotalCount<'Id, 'Projection>> -> unit) -> SubscribeResult
    abstract member SubscribeQuery:                 query: Query<'Id, 'Index, 'OpError> -> useCache: UseCache -> subscriber: (AsyncData<Subjects<'Id, 'Projection>> -> unit) -> SubscribeResult
    abstract member SubscribeQueryWithTotalCount:   query: Query<'Id, 'Index, 'OpError> -> useCache: UseCache -> subscriber: (AsyncData<SubjectsWithTotalCount<'Id, 'Projection>> -> unit) -> SubscribeResult
    abstract member PauseSubscriptions:             unit -> IDisposable
    abstract member Act:                            id: 'Id -> 'Action -> Async<Result<unit, ActionOrConstructionError<'OpError>>>
    abstract member ActWait:                        id: 'Id -> 'Action -> 'Event -> Async<Result<ApiActOrConstructAndWaitOnLifeEventResult<'Projection, 'Event>, ActionOrConstructionError<'OpError>>>
    abstract member ActWaitWithTimeout:             id: 'Id -> action: 'Action -> event: 'Event -> timeout: TimeSpan -> Async<Result<ApiActOrConstructAndWaitOnLifeEventResult<'Projection, 'Event>, ActionOrConstructionError<'OpError>>>
    abstract member Construct:                      ctor: 'Constructor -> Async<Result<'Projection, ActionOrConstructionError<'OpError>>>
    abstract member ConstructAndWait:               ctor: 'Constructor -> 'Event -> Async<Result<ApiActOrConstructAndWaitOnLifeEventResult<'Projection, 'Event>, ActionOrConstructionError<'OpError>>>
    abstract member ActMaybeConstruct:              id: 'Id -> 'Action -> 'Constructor -> Async<Result<unit, ActionOrConstructionError<'OpError>>>
    abstract member ActMaybeConstructAndWait:       id: 'Id -> 'Action -> 'Constructor -> event: 'Event -> Async<Result<ApiActOrConstructAndWaitOnLifeEventResult<'Projection, 'Event>, ActionOrConstructionError<'OpError>>>
    abstract member GetOne:                         useCache: UseCache -> 'Id -> Async<AsyncData<'Projection>>
    abstract member GetMaybeConstruct:              useCache: UseCache -> 'Id -> 'Constructor -> Async<AsyncData<'Projection>>
    abstract member GetAll:                         useCache: UseCache -> resultSetOptions: ResultSetOptions<'Index> -> Async<AsyncData<Subjects<'Id, 'Projection>>>
    abstract member GetAllWithTotalCount:           useCache: UseCache -> resultSetOptions: ResultSetOptions<'Index> -> Async<AsyncData<SubjectsWithTotalCount<'Id, 'Projection>>>
    abstract member GetAllCount:                    useCache: UseCache -> Async<AsyncData<uint64>>
    abstract member GetMany:                        useCache: UseCache -> maybeEmptyIds: Set<'Id> -> Async<AsyncData<Subjects<'Id, 'Projection>>>
    abstract member GetIndexed:                     useCache: UseCache -> query: IndexQuery<'Index> -> Async<AsyncData<Subjects<'Id, 'Projection>>>
    abstract member GetIndexedWithTotalCount:       useCache: UseCache -> query: IndexQuery<'Index> -> Async<AsyncData<SubjectsWithTotalCount<'Id, 'Projection>>>
    abstract member GetIndexedCount:                useCache: UseCache -> predicate: PreparedIndexPredicate<'Index> -> Async<AsyncData<uint64>>
    abstract member GetAuditSnapshot:               idString: string -> version: uint64 -> Async<AsyncData<TemporalSnapshot<'Subject, 'Action, 'Constructor, 'Id>>>


and SubjectService<'Subject, 'Projection, 'Id, 'Index, 'NumericIndex, 'StringIndex, 'SearchIndex, 'GeographyIndex, 'Constructor, 'Action, 'Event, 'OpError
                      when 'Subject      :> Subject<'Id>
                      and  'Projection   :> SubjectProjection<'Id>
                      and  'Projection   :  equality
                      and  'Id           :> SubjectId
                      and  'Id           :  comparison
                      and  'Constructor  :> Constructor
                      and  'Action       :> LifeAction
                      and  'Event        :> LifeEvent
                      and  'OpError      :> OpError
                      and  'Index        :> SubjectIndex<'Index, 'NumericIndex, 'StringIndex, 'SearchIndex, 'GeographyIndex, 'OpError>
                      and  'NumericIndex :> SubjectNumericIndex<'OpError>
                      and  'StringIndex  :> SubjectStringIndex<'OpError>
                      and  'SearchIndex  :> SubjectSearchIndex
                      and  'GeographyIndex :> SubjectGeographyIndex>
    (
        lifeCycleKey:            LifeCycleKey,
        maybeProjectionName:     Option<string>,
        reasonablyFreshTTLs:     ReasonablyFreshTTLs,
        realTimeService:         RealTimeService,
        thothEncodedHttpService: ThothEncodedHttpService,
        eventBus:                EventBus,
        apiEndpoints:            SubjectEndpoints<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>
    ) =
    let ecosystemName = lifeCycleKey.EcosystemName
    let lifeCycleName = lifeCycleKey.LocalLifeCycleName

    let log =
        Log
            .WithCategory("SubjectService")
            .WithProperties(
                [
                    ("EcosystemName", box ecosystemName)
                    ("LifeCycleName", box lifeCycleName)
                    ("MaybeProjectionName", box maybeProjectionName)
                ]
                |> Map.ofList
            )

    let eventQueue: EventBus.Queue<SubjectServiceEvent<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>> = EventBus.Queue $"subjectServiceEventQueue-{lifeCycleName}"

    let inMemoryCache = InMemoryCache<'Subject, 'Projection, 'Id, 'Index, 'NumericIndex, 'StringIndex, 'SearchIndex, 'GeographyIndex, 'OpError>(reasonablyFreshTTLs)
    let throttling = Throttling<'Subject, 'Projection, 'Id, 'Index, 'NumericIndex, 'StringIndex, 'SearchIndex, 'GeographyIndex, 'Constructor, 'Action, 'Event, 'OpError>(thothEncodedHttpService, apiEndpoints, eventBus, eventQueue)

    let mutable subscriptionImplementations: Map<'Id, AdHocSubscriptionImplementation<AsyncData<VersionedData<'Projection>>>> = Map.empty

    // NOTE we do a manual resubmission of all multi-subject requests when a new subject is
    // constructed. This will result in some unnecessary queries, but overall is probably
    // a fairly reasonable tradeoff, since otherwise we have no way at all of knowing
    // that a query should be resubmitted because a new subject was added.
    // Note that we only rerun on straight-up constructs, not OrConstruct varieties.
    let mutable manySubscriptionReexecute: Map<Guid, unit -> unit> = Map.empty

    let mutable pauseCount = 0
    let mutable onResumeCallbacks: Map<Guid, OnResumeCallback> = Map.empty

    let notifyResumed () =
        onResumeCallbacks
        |> Map.values
        |> Seq.iter (fun callback -> callback ())

    member private _.UpdateSubscriptions (id: 'Id) (versionedDataAD: AsyncData<VersionedData<'Projection>>) : unit =
        subscriptionImplementations.TryFind id
        |> Option.sideEffect (fun subscriptionImplementation ->
            if subscriptionImplementation.LatestValue <> Some versionedDataAD then
                subscriptionImplementation.Update versionedDataAD

            match versionedDataAD with
            | Uninitialized
            | WillStartFetchingSoonHack
            | Fetching _
            | Available _
            // Access and availability can change throughout the lifetime of a subscription due to session state changes.
            | Unavailable
            | AccessDenied ->
                Noop
            | Failed _ ->
                subscriptionImplementations <- subscriptionImplementations.Remove id
        )

    member private this.MarkAsFetching (id: 'Id) : Option<VersionedData<'Projection>> =
        let versionedDataAD =
            match inMemoryCache.GetCachedVersionDataForIdIgnoreTTL id with
            | Some (_, cachedValue) -> AsyncData.makeFetching cachedValue
            | None                  -> Fetching None

        // This "feature" doesn't seem to make sense. Our stateful buttons that we use
        // to execute actions have spinners anyway, so marking the subject as Fetching
        // for the duraction of Act doesn't seem to make any sense. But we don't fully
        // know if there are app features in some apps that rely on this,
        // so we're currently a bit scared to turn this on

        inMemoryCache.CacheOne (id, versionedDataAD)
        |> snd
        |> this.UpdateSubscriptions id

        // TODO: not convinced this is correct any more
        match versionedDataAD with
        | Fetching maybePreviousValue -> maybePreviousValue
        | _                           -> None

    member this.LifeCycleKey = lifeCycleKey

    interface ISubjectService<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError> with
        member this.LifeCycleKey = this.LifeCycleKey

        member this.EventQueue = eventQueue

        /////////////////////////////////////////
        //            SUBSCRIBING              //
        /////////////////////////////////////////

        member this.SubscribeOne (id: 'Id) (useCache: UseCache) (subscriber: AsyncData<'Projection> -> unit) : SubscribeResult =
            this.SubscribeOne id useCache (fun value -> value |> AsyncData.map VersionedData.data |> subscriber)

        member this.SubscribeMany (ids: Set<'Id>) (useCache: UseCache) (subscriber: AsyncData<Subjects<'Id, 'Projection>> -> unit) : SubscribeResult =
            this.SubscribeMany ids useCache (fun value -> value |> AsyncData.map (Subjects.map VersionedData.data) |> subscriber)

        member this.SubscribeAll (resultSetOptions: ResultSetOptions<'Index>) (useCache: UseCache) (subscriber: AsyncData<Subjects<'Id, 'Projection>> -> unit) : SubscribeResult =
            this.SubscribeAll resultSetOptions useCache (fun value -> value |> AsyncData.map (Subjects.map VersionedData.data) |> subscriber)

        member this.SubscribeAllWithTotalCount (resultSetOptions: ResultSetOptions<'Index>) (useCache: UseCache) (subscriber: AsyncData<SubjectsWithTotalCount<'Id, 'Projection>> -> unit) : SubscribeResult =
            this.SubscribeAllWithTotalCount resultSetOptions useCache (fun value -> value |> AsyncData.map (SubjectsWithTotalCount.map VersionedData.data) |> subscriber)

        member this.SubscribeIndexed (query: IndexQuery<'Index>) (useCache: UseCache) (subscriber: AsyncData<Subjects<'Id, 'Projection>> -> unit) : SubscribeResult =
            this.SubscribeIndexed query useCache (fun value -> value |> AsyncData.map (Subjects.map VersionedData.data) |> subscriber)

        member this.SubscribeIndexedWithTotalCount (query: IndexQuery<'Index>) (useCache: UseCache) (subscriber: AsyncData<SubjectsWithTotalCount<'Id, 'Projection>> -> unit) : SubscribeResult =
            this.SubscribeIndexedWithTotalCount query useCache (fun value -> value |> AsyncData.map (SubjectsWithTotalCount.map VersionedData.data) |> subscriber)

        member this.SubscribeQuery (query: Query<'Id, 'Index, 'OpError>)  (useCache: UseCache) (subscriber: AsyncData<Subjects<'Id, 'Projection>> -> unit) : SubscribeResult =
            this.SubscribeQuery query useCache (fun value -> value |> AsyncData.map (Subjects.map VersionedData.data) |> subscriber)

        member this.SubscribeQueryWithTotalCount (query: Query<'Id, 'Index, 'OpError>)  (useCache: UseCache) (subscriber: AsyncData<SubjectsWithTotalCount<'Id, 'Projection>> -> unit) : SubscribeResult =
            this.SubscribeQueryWithTotalCount query useCache (fun value -> value |> AsyncData.map (SubjectsWithTotalCount.map VersionedData.data) |> subscriber)

        member this.PauseSubscriptions () : IDisposable =
            this.PauseSubscriptions ()

        member this.Act (id: 'Id) (action: 'Action) : Async<Result<unit, ActionOrConstructionError<'OpError>>> =
            this.Act id action

        member this.ActWait (id: 'Id) (action: 'Action) (event: 'Event) : Async<Result<ApiActOrConstructAndWaitOnLifeEventResult<'Projection, 'Event>, ActionOrConstructionError<'OpError>>> =
            this.ActWait id action event

        member this.ActWaitWithTimeout (id: 'Id) (action: 'Action) (event: 'Event) (timeout: TimeSpan) : Async<Result<ApiActOrConstructAndWaitOnLifeEventResult<'Projection, 'Event>, ActionOrConstructionError<'OpError>>> =
            this.ActWaitWithTimeout id action event timeout

        member this.ActMaybeConstruct (id: 'Id) (action: 'Action) (ctor: 'Constructor) : Async<Result<unit, ActionOrConstructionError<'OpError>>> =
            this.ActMaybeConstruct id action ctor

        member this.ActMaybeConstructAndWait (id: 'Id) (action: 'Action) (ctor: 'Constructor) (event: 'Event) : Async<Result<ApiActOrConstructAndWaitOnLifeEventResult<'Projection, 'Event>, ActionOrConstructionError<'OpError>>> =
            this.ActMaybeConstructAndWait id action ctor event

        member this.Construct (ctor: 'Constructor) : Async<Result<'Projection, ActionOrConstructionError<'OpError>>> =
            this.Construct ctor

        member this.ConstructAndWait (ctor: 'Constructor) (event: 'Event) : Async<Result<ApiActOrConstructAndWaitOnLifeEventResult<'Projection, 'Event>, ActionOrConstructionError<'OpError>>> =
            this.ConstructAndWait ctor event

        member this.GetOne (useCache: UseCache) (id: 'Id) : Async<AsyncData<'Projection>> =
            this.GetOne useCache id
            |> Async.Map (AsyncData.map VersionedData.data)

        member this.GetMaybeConstruct (useCache: UseCache) (id: 'Id) (ctor: 'Constructor) : Async<AsyncData<'Projection>> =
            this.GetMaybeConstruct useCache id ctor
            |> Async.Map (AsyncData.map VersionedData.data)

        member this.GetAll (useCache: UseCache) (resultSetOptions: ResultSetOptions<'Index>) : Async<AsyncData<Subjects<'Id, 'Projection>>> =
            this.GetAll useCache resultSetOptions
            |> Async.Map (AsyncData.map (Subjects.map VersionedData.data))

        member this.GetAllWithTotalCount (useCache: UseCache) (resultSetOptions: ResultSetOptions<'Index>) : Async<AsyncData<SubjectsWithTotalCount<'Id, 'Projection>>> =
            this.GetAllWithTotalCount useCache resultSetOptions
            |> Async.Map (AsyncData.map (SubjectsWithTotalCount.map VersionedData.data))

        member this.GetAllCount (useCache: UseCache) : Async<AsyncData<uint64>> =
            this.GetAllCount useCache

        member this.GetMany (useCache: UseCache) (ids: Set<'Id>) : Async<AsyncData<Subjects<'Id, 'Projection>>> =
            this.GetMany useCache ids
            |> Async.Map (AsyncData.map (Subjects.map VersionedData.data))

        member this.GetIndexed (useCache: UseCache) (query: IndexQuery<'Index>) : Async<AsyncData<Subjects<'Id, 'Projection>>> =
            this.GetIndexed useCache query
            |> Async.Map (AsyncData.map (Subjects.map VersionedData.data))

        member this.GetIndexedWithTotalCount (useCache: UseCache) (query: IndexQuery<'Index>) : Async<AsyncData<SubjectsWithTotalCount<'Id, 'Projection>>> =
            this.GetIndexedWithTotalCount useCache query
            |> Async.Map (AsyncData.map (SubjectsWithTotalCount.map VersionedData.data))

        member this.GetIndexedCount (useCache: UseCache) (predicate: PreparedIndexPredicate<'Index>) : Async<AsyncData<uint64>> =
            this.GetIndexedCount useCache predicate

        member this.GetAuditSnapshot (idString: string) (version: uint64) : Async<AsyncData<TemporalSnapshot<'Subject, 'Action, 'Constructor, 'Id>>> =
            this.GetAuditSnapshot idString version


    /////////////////////////////////////////
    //            SUBSCRIBING              //
    /////////////////////////////////////////

    member this.SubscribeOne (id: 'Id) (useCache: UseCache) (subscriber: AsyncData<VersionedData<'Projection>> -> unit) : SubscribeResult =
        this.SubscribeOneInternal id useCache subscriber

    // TODO: when resources permit, the below subscribe-many scenarios could/should be moved to the server, negating the need for a local cache and opening
    //       the door for a more flexible and performant implementation. One of the main challenges will be around the necessary introduction of generic
    //       type parameters into the stream API.

    member this.SubscribeMany (ids: Set<'Id>) (useCache: UseCache) (subscriber: AsyncData<Subjects<'Id, VersionedData<'Projection>>> -> unit) : SubscribeResult =
        this.SubscribeManyWithoutCountHelper (fun () -> this.GetMany useCache ids) useCache subscriber

    member this.SubscribeAll (resultSetOptions: ResultSetOptions<'Index>) (useCache: UseCache) (subscriber: AsyncData<Subjects<'Id, VersionedData<'Projection>>> -> unit) : SubscribeResult =
        this.SubscribeManyWithoutCountHelper (fun () -> this.GetAll useCache resultSetOptions) useCache subscriber

    member this.SubscribeAllWithTotalCount (resultSetOptions: ResultSetOptions<'Index>) (useCache: UseCache) (subscriber: AsyncData<SubjectsWithTotalCount<'Id, VersionedData<'Projection>>> -> unit) : SubscribeResult =
        this.SubscribeManyWithTotalCountHelper (fun () -> this.GetAllWithTotalCount useCache resultSetOptions) useCache subscriber

    member this.SubscribeIndexed (query: IndexQuery<'Index>) (useCache: UseCache) (subscriber: AsyncData<Subjects<'Id, VersionedData<'Projection>>> -> unit) : SubscribeResult =
        this.SubscribeManyWithoutCountHelper (fun () -> this.GetIndexed useCache query) useCache subscriber

    member this.SubscribeIndexedWithTotalCount (query: IndexQuery<'Index>) (useCache: UseCache) (subscriber: AsyncData<SubjectsWithTotalCount<'Id, VersionedData<'Projection>>> -> unit) : SubscribeResult =
        this.SubscribeManyWithTotalCountHelper (fun () -> this.GetIndexedWithTotalCount useCache query) useCache subscriber

    member this.SubscribeQuery (query: Query<'Id, 'Index, 'OpError>)  (useCache: UseCache) (subscriber: AsyncData<Subjects<'Id, VersionedData<'Projection>>> -> unit) : SubscribeResult =
        match query with
        | All resultSetOptions -> this.SubscribeAll resultSetOptions useCache subscriber
        | One id               -> this.SubscribeOne id useCache (fun subjectAD -> (id, subjectAD) |> Seq.ofOneItem |> Available |> subscriber)
        | Indexed indexedQuery -> this.SubscribeIndexed indexedQuery useCache subscriber

    member this.SubscribeQueryWithTotalCount (query: Query<'Id, 'Index, 'OpError>)  (useCache: UseCache) (subscriber: AsyncData<SubjectsWithTotalCount<'Id, VersionedData<'Projection>>> -> unit) : SubscribeResult =
        match query with
        | All resultSetOptions -> this.SubscribeAllWithTotalCount resultSetOptions useCache subscriber
        | One id               -> this.SubscribeOne id useCache (fun subjectAD -> (id, subjectAD) |> SubjectsWithTotalCount.ofOneItem |> Available |> subscriber)
        | Indexed indexedQuery -> this.SubscribeIndexedWithTotalCount indexedQuery useCache subscriber

    // Pause all subscriptions to this subject type. This is only intended for very specific use cases (to avoid race
    // conditions between XHR and real-time) updates, and should only be used for very short periods.
    member this.PauseSubscriptions () : IDisposable =
        pauseCount <- pauseCount + 1
        log.Debug("Paused subscriptions")

        let mutable isDisposed = false

        { new IDisposable with
            member _.Dispose() =
                if isDisposed then
                    Noop
                else
                    isDisposed <- true
                    pauseCount <- pauseCount - 1

                    if pauseCount = 0 then
                        notifyResumed ()
                        log.Debug("Resumed subscriptions")
        }

    member _.IsPaused : bool =
        pauseCount > 0

    member private _.OnResumed (callback: OnResumeCallback): IDisposable =
        let callbackId = Guid.NewGuid()
        onResumeCallbacks <-
            onResumeCallbacks
            |> Map.add callbackId callback

        { new IDisposable with
            member _.Dispose() =
                onResumeCallbacks <-
                    onResumeCallbacks
                    |> Map.remove callbackId
        }

    member (* want protected *) this.SubscribeOneInternal (id: 'Id) (useCache: UseCache) (subscriber: AsyncData<VersionedData<'Projection>> -> unit) : SubscribeResult =
        let idStr = id.IdString

        let mutable maybeStreamCreationResult:                     Option<StreamCreationResult>                     = None
        let mutable maybeSubscriptionImplementationCreationResult: Option<SubscriptionImplementationCreationResult> = None

        let mutable messageQueue: List<QueuedMessage> = List.empty
        let mutable maybeOnResumed: Option<IDisposable> = None

        let processObserverMessage (msg: ServerStreamApi) : unit =
            match msg with
            | ServerStreamApi.SubjectChanged accessControlledSubjectChangeStr ->
                let versionedDataAD =
                    match maybeProjectionName with
                    | None ->
                        let accessControlledSubjectChangeResult = apiEndpoints.DecodeAccessControlledSubjectChange accessControlledSubjectChangeStr
                        match accessControlledSubjectChangeResult with
                        | Ok accessControlledSubjectChange ->
                            match accessControlledSubjectChange with
                            | Granted subjectChange ->
                                match subjectChange with
                                | ApiSubjectChange.Updated versionedData ->
                                    // We are relying on the sanity of the declarer of the
                                    // service instance, i.e. if maybeProjectionName is None,
                                    // then 'Subject and 'Projection are the same type, so
                                    // this dynamic cast is okay.
                                    versionedData :> obj :?> VersionedData<'Projection>
                                    |> Available
                                | ApiSubjectChange.NotInitialized -> Unavailable
                            | Denied _ -> AccessDenied
                        | Error msg -> Failed (UnknownFailure ($"%s{lifeCycleName} '%s{idStr}': failed to decode subject update: %s{msg}"))

                    | Some projectionName ->
                        let accessControlledSubjectChangeResult = apiEndpoints.DecodeAccessControlledSubjectProjectionChange accessControlledSubjectChangeStr
                        match accessControlledSubjectChangeResult with
                        | Ok accessControlledSubjectChange ->
                            match accessControlledSubjectChange with
                            | Granted subjectChange ->
                                match subjectChange with
                                | ApiSubjectChange.Updated versionedData ->
                                    Available versionedData
                                | ApiSubjectChange.NotInitialized -> Unavailable
                            | Denied _ -> AccessDenied
                        | Error msg -> Failed (UnknownFailure ($"%s{lifeCycleName} '%s{idStr}': failed to decode subject projection '%s{projectionName}' update: %s{msg}"))

                inMemoryCache.CacheOne (id, versionedDataAD)
                |> snd
                |> this.UpdateSubscriptions id

        let onResume (pausedObserver: StreamSubscriber<ServerStreamApi>) () =
            messageQueue
            |> List.rev
            |> List.iter (fun queuedMsg ->
                match queuedMsg with
                | QueuedMessage.Next value -> pausedObserver.next value
                | QueuedMessage.Error ex -> pausedObserver.error ex)

            messageQueue <- List.empty

            maybeOnResumed
            |> Option.iter (fun onResumed -> onResumed.Dispose())

            maybeOnResumed <- None

        let queueMessage (pausedObserver: StreamSubscriber<ServerStreamApi>) (msg: QueuedMessage) =
            log.Debug("Queueing message: {Msg}", msg)
            messageQueue <- msg :: messageQueue

            match maybeOnResumed with
            | Some _ ->
                Noop
            | None ->
                maybeOnResumed <- Some (this.OnResumed (onResume pausedObserver))

        // We delay creating the stream asynchronously to allow us to deal with race conditions.
        let (subscriptionImplementation, maybeCreateStreamFor) =
            subscriptionImplementations.TryFind id
            |> Option.map (fun si -> (si, None))
            |> Option.getOrElseLazy (fun () ->
                let mutable maybeCurrCleanupId: Option<Guid> = None

                let subscriptionImplementation =
                    AdHocSubscriptionImplementation<AsyncData<VersionedData<'Projection>>>(
                        None,
                        Some (fun subscribers ->
                            log.Debug("Zero subscribers remaining for subject with ID {SubjectId} - will wait for 20ish seconds and if nobody else subscribes, will clean up", id.IdString)
                            let cleanupId = Guid.NewGuid ()
                            maybeCurrCleanupId <- Some cleanupId

                            // randomizing the cleanup time, so as to not flood the event loop
                            // with unsubscriptions during scrolling
                            LibClient.JsInterop.runLater (TimeSpan.FromSeconds (20. + System.Random().NextDouble() * 10.0)) (fun () ->
                                if maybeCurrCleanupId = Some cleanupId then
                                    if subscribers.HasSubscribers then
                                        log.Debug("Zero subscribers remained for subject with ID {SubjectId}, but after 20ish seconds we have some new subscribers, so carrying on", id.IdString)
                                    else
                                        log.Debug("Zero subscribers remained for subject with ID {SubjectId}, and after 20ish seconds we still have zero subscribers, cleaning up observer and removing subscription implementation", id.IdString)

                                        match maybeStreamCreationResult with
                                        | Some streamCreationResult -> streamCreationResult.ObserverDisposable.Dispose()
                                        | None -> ()

                                        subscriptionImplementations <- subscriptionImplementations.Remove id
                                        maybeSubscriptionImplementationCreationResult <- Some {
                                            AlreadyRemoved = true
                                        }
                                else
                                    log.Debug("Zero subscribers remained for subject with ID {SubjectId}, however, after 20ish seconds we've had some subscribers but once again dropped to zero, so a new instance of the cleanup function is in charge", id.IdString)
                            )
                        )
                    )
                subscriptionImplementations <- subscriptionImplementations.Add(id, subscriptionImplementation)
                maybeSubscriptionImplementationCreationResult <- Some {
                    AlreadyRemoved = false
                }

                (subscriptionImplementation, Some idStr)
            )

        let subscribeResult = subscriptionImplementation.Subscribe subscriber

        let hasJustCreatedNewStream =
            match maybeCreateStreamFor with
            | Some idStr ->
                log.Debug("Creating stream for subject with ID {SubjectId}", idStr)

                async {
                    let observer =
                        {
                            next = fun (msg: ServerStreamApi) ->
                                if subscriptionImplementation.HasSubscribers then
                                    processObserverMessage msg
                                else
                                    subscriptionImplementation.SetCallbackToRunOnTransitionFromZeroToOneSubscriber (fun () -> processObserverMessage msg)

                            complete = fun () ->
                                if subscriptionImplementation.HasSubscribers then
                                    log.Debug("Observer completed for subject with ID {SubjectId}", idStr)

                            error = fun maybeExn ->
                                let subscriptionStatus =
                                    match subscriptionImplementation.HasSubscribers with
                                    | true  -> "subscribed"
                                    | false -> "unsubscribed"

                                match maybeExn with
                                | Some exn -> log.Error($"Error occurred in %s{subscriptionStatus} observer for subject with ID {{SubjectId}}: %A{exn}", idStr)
                                | None     -> log.Error($"Undefined error occurred in %s{subscriptionStatus} observer for subject with ID {{SubjectId}}", idStr)
                        }

                    let pauseableObserver =
                        {
                            next = fun (msg: ServerStreamApi) ->
                                if this.IsPaused then
                                    msg
                                    |> QueuedMessage.Next
                                    |> queueMessage observer
                                else
                                    observer.next msg

                            error = fun maybeEx ->
                                if this.IsPaused then
                                    maybeEx
                                    |> QueuedMessage.Error
                                    |> queueMessage observer
                                else
                                    observer.error maybeEx

                            complete = observer.complete
                        }

                    // Holds the active stream observer, which will be replaced if we reconnect to the server.
                    let streamObserver = new SerialDisposable()

                    let createAndReplaceNewStreamObserver () =
                        async {
                            let maybeCurrentVersionedData =
                                inMemoryCache.GetCachedVersionDataForIdIgnoreTTL id
                                |> Option.map snd
                                |> Option.bind AsyncData.toOption
                            let maybeCurrentVersion =
                                maybeCurrentVersionedData
                                |> Option.map (fun versionedData -> versionedData.Version)

                            let streamFromMessage =
                                ClientStreamApi.ObserveSubjectV2 (ecosystemName, lifeCycleName, idStr, maybeProjectionName, maybeCurrentVersion)
                            let! observerDisposable = realTimeService.StreamFrom streamFromMessage pauseableObserver
                            streamObserver.ReplaceInnerDisposable(observerDisposable)

                            maybeCurrentVersionedData
                            |> Option.iter (fun versionedData -> this.UpdateSubscriptions id (versionedData |> AsyncData.Available))
                            log.Debug("(Re)-observed subject with ID {SubjectId}, projection {MaybeProjectionName}, current version {MaybeCurrentVersion}", idStr, maybeProjectionName, maybeCurrentVersion)
                        }
                        |> startSafely

                    // Re-establish the stream observer every 1 minute to force a sync between client and server. This will ensure the client's cached
                    // data is updated if the server has something newer (due to package loss or whatever).
                    let periodicReObserveDisposable =
                        LibClient.JsInterop.runEvery (TimeSpan.FromMinutes(1)) createAndReplaceNewStreamObserver

                    // Immediately create the initial stream observer.
                    createAndReplaceNewStreamObserver ()

                    let onReconnectedDisposable = realTimeService.OnReconnected createAndReplaceNewStreamObserver
                    let onForceReconnectDisposable =
                        realTimeService.OnForceReconnect
                            (fun _ ->
                                log.Debug("Connection is being forced to reconnect, so unsubscribing")
                                // This will remove our observer until a new one is re-established upon re-connection, which is important to avoid errors
                                // being forwarded to the observer (and therefore written to the console).
                                streamObserver.ReplaceInnerDisposable(Disposables.EmptyDisposable.Instance)
                            )
                    let disposables: IDisposable = new CompositeDisposable([streamObserver; periodicReObserveDisposable; onReconnectedDisposable; onForceReconnectDisposable])

                    // Ensure we deal with this race condition scenario: a subscription is instigated, but the caller already unsubscribed the subscription by
                    // the time the stream is established. In such a case, we need to dispose of the stream immediately here rather than waiting until Off is
                    // called on the subscription.
                    match maybeSubscriptionImplementationCreationResult with
                    | Some subscriptionImplementationCreationResult when subscriptionImplementationCreationResult.AlreadyRemoved ->
                        log.Debug("Subscriber already unsubscribed for subject with ID {SubjectId} - disposing of stream observer", idStr)
                        disposables.Dispose()
                    | _ ->
                        maybeStreamCreationResult <- Some {
                            ObserverDisposable = disposables
                        }

                }
                |> startSafely

                true

            | None -> false

        // It's important we pull this value out after we subscribe,
        // since the act of subscribing may trigger processing of the last
        // socket message that we've been storing while we had no subscribers,
        // which in turn will update the cache.
        let maybeCachedValue =
            inMemoryCache.GetCachedVersionDataForId useCache id
            |> Option.map snd

        match (maybeCachedValue, hasJustCreatedNewStream) with
        | (Some initialValue, _    ) -> subscriber initialValue
        | (None,              false) -> subscriptionImplementation.LatestValue |> Option.sideEffect subscriber
        | (None,              true ) -> Noop

        subscribeResult

    member private this.SubscribeManyWithoutCountHelper
        (get: unit -> Async<AsyncData<Subjects<'Id, VersionedData<'Projection>>>>)
        (useCache: UseCache)
        (subscriber: AsyncData<Subjects<'Id, VersionedData<'Projection>>> -> unit)
        : SubscribeResult =

        let subscriptionImplementation = AdHocSubscriptionImplementation<AsyncData<Subjects<'Id, VersionedData<'Projection>>>>(Some WillStartFetchingSoonHack, None)
        let subscriptionResult = subscriptionImplementation.Subscribe subscriber

        let mutable individualSubjectSubscriptionResults: List<SubscribeResult> = []

        let unsubscribeIndividualSubjectSubscriptions () =
            individualSubjectSubscriptionResults |> List.iter (fun subscriptionResult -> subscriptionResult.Off())
            individualSubjectSubscriptionResults <- []

        let execute () =
            unsubscribeIndividualSubjectSubscriptions ()

            async {
                let! rawSubjectsAD = get ()
                let subjectsAD = unfortunateSafetyHack rawSubjectsAD

                subjectsAD
                |> AsyncData.toOption
                |> Option.sideEffect (fun subjects ->
                    this.SubscribeManyHelper
                        useCache
                        subjects
                        subscriptionImplementation.MaybeUpdate
                        (fun value -> individualSubjectSubscriptionResults <- value)
                )

                subscriptionImplementation.Update subjectsAD

            } |> startSafely

        let key = Guid.NewGuid()
        manySubscriptionReexecute <- manySubscriptionReexecute.Add (key, execute)

        execute ()

        let unsubscribe () : unit =
            subscriptionResult.Off()
            unsubscribeIndividualSubjectSubscriptions ()
            manySubscriptionReexecute <- manySubscriptionReexecute.Remove key

        { Off = unsubscribe }

    member private this.SubscribeManyWithTotalCountHelper
        (get: unit -> Async<AsyncData<SubjectsWithTotalCount<'Id, VersionedData<'Projection>>>>)
        (useCache: UseCache)
        (subscriber: AsyncData<SubjectsWithTotalCount<'Id, VersionedData<'Projection>>> -> unit)
        : SubscribeResult =

        let subscriptionImplementation = AdHocSubscriptionImplementation<AsyncData<SubjectsWithTotalCount<'Id, VersionedData<'Projection>>>>(Some WillStartFetchingSoonHack, None)
        let subscriptionResult = subscriptionImplementation.Subscribe subscriber

        let mutable individualSubjectSubscriptionResults: List<SubscribeResult> = []

        let unsubscribeIndividualSubjectSubscriptions () =
            individualSubjectSubscriptionResults |> List.iter (fun subscriptionResult -> subscriptionResult.Off())
            individualSubjectSubscriptionResults <- []

        let execute () =
            unsubscribeIndividualSubjectSubscriptions ()

            async {
                let! subjectsWithTotalCountAD = get ()

                subjectsWithTotalCountAD
                |> AsyncData.toOption
                |> Option.sideEffect (fun subjectsWithTotalCount ->
                    let maybeUpdateSubscription (updater: Option<AsyncData<Subjects<'Id, VersionedData<'Projection>>>> -> Option<AsyncData<Subjects<'Id, VersionedData<'Projection>>>>) : unit =
                        subscriptionImplementation.MaybeUpdate (fun maybeCurrentValueAD ->
                            maybeCurrentValueAD
                            |> Option.map (AsyncData.map (fun currentValue -> currentValue.Subjects))
                            |> updater
                            |> Option.map (AsyncData.map (fun updatedValue ->
                                {
                                    Subjects   = updatedValue
                                    TotalCount = subjectsWithTotalCount.TotalCount
                                }
                            ))
                        )

                    this.SubscribeManyHelper
                        useCache
                        subjectsWithTotalCount.Subjects
                        maybeUpdateSubscription
                        (fun value -> individualSubjectSubscriptionResults <- value)
                )

                subscriptionImplementation.Update subjectsWithTotalCountAD

            } |> startSafely

        let key = Guid.NewGuid()
        manySubscriptionReexecute <- manySubscriptionReexecute.Add (key, execute)

        execute ()

        let unsubscribe () : unit =
            subscriptionResult.Off()
            unsubscribeIndividualSubjectSubscriptions ()
            manySubscriptionReexecute <- manySubscriptionReexecute.Remove key

        { Off = unsubscribe }


    member private this.SubscribeManyHelper
        (useCache: UseCache)
        (subjects: Subjects<'Id, VersionedData<'Projection>>)
        (maybeUpdateSubscription: (Option<AsyncData<Subjects<'Id, VersionedData<'Projection>>>> -> Option<AsyncData<Subjects<'Id, VersionedData<'Projection>>>>) -> unit)
        (setIndividualSubjectSubscriptionResults: List<SubscribeResult> -> unit)
        : unit =

        let fetchedIds = subjects |> Seq.map fst

        fetchedIds
        |> Seq.map (fun id ->
            this.SubscribeOneInternal
                id
                useCache
                (fun updatedVersionedDataAD ->
                    maybeUpdateSubscription (fun maybeLatestVersionedDataAD ->
                        match maybeLatestVersionedDataAD with
                        | Some (Available latestVersionedData) ->
                            let (wasUpdated, maybeUpdatedVersionedData) =
                                latestVersionedData
                                |> Seq.fold
                                    (fun (wasUpdated, maybeUpdatedSubjects) (currId, currSubjectAD) ->
                                        if currId = id && currSubjectAD <> updatedVersionedDataAD then
                                            (true, (currId, updatedVersionedDataAD) :: maybeUpdatedSubjects)
                                        else
                                            (wasUpdated, (currId, currSubjectAD) :: maybeUpdatedSubjects)
                                    )
                                    (false, [])

                            match wasUpdated with
                            | false -> None
                            | true  -> maybeUpdatedVersionedData |> Seq.ofList |> Seq.rev |> Available |> Some

                        | _ -> None
                    )
                )
        )
        |> List.ofSeq
        |> setIndividualSubjectSubscriptionResults


    member this.RerunQueries () : unit =
        inMemoryCache.InvalidateAllCache ()
        inMemoryCache.InvalidateIndexCache ()
        manySubscriptionReexecute
        |> Map.values
        |> Seq.iter (fun execute -> execute ())

    /////////////////////////////////////////
    //             GETTING                 //
    /////////////////////////////////////////

    member this.GetOne (useCache: UseCache) (id: 'Id) : Async<AsyncData<VersionedData<'Projection>>> = async {
        match inMemoryCache.GetCachedVersionDataForId useCache id with
        | Some (_, versionedDataAD) -> return versionedDataAD
        | None                      -> return! this.FetchOneAndCache id
    }

    member this.GetMaybeConstruct (useCache: UseCache) (id: 'Id) (ctor: 'Constructor) : Async<AsyncData<VersionedData<'Projection>>> = async {
        match inMemoryCache.GetCachedVersionDataForId useCache id with
        | Some (_, versionedDataAD) -> return versionedDataAD
        | None                      -> return! this.FetchOneMaybeConstructAndCache id ctor
    }

    member this.GetMany (useCache: UseCache) (maybeEmptyIds: Set<'Id>) : Async<AsyncData<Subjects<'Id, VersionedData<'Projection>>>> = async {
        match NonemptySet.ofSet maybeEmptyIds with
        | Some ids ->
            let maybeCachedValue =
                match useCache with
                | No -> None
                | _  -> inMemoryCache.GetCachedVersionDataForIds useCache ids.ToSet |> Option.map Available

            match maybeCachedValue with
            | Some value -> return value
            | None       -> return! this.FetchManyAndCache ids |> Async.Map this.ConvertAsyncDataAccessControlledToAsyncData

        | None ->
            return Available Seq.empty
    }

    member this.GetAll (useCache: UseCache) (resultSetOptions: ResultSetOptions<'Index>) : Async<AsyncData<Subjects<'Id, VersionedData<'Projection>>>> = async {
        let maybeCachedValue: Option<AsyncData<Subjects<'Id, VersionedData<'Projection>>>> =
            match useCache with
            | No -> None
            | _  ->
                inMemoryCache.GetCachedIdsForResultSetOptions useCache resultSetOptions
                |> Option.flatMap (AsyncData.tryMapIfAvailable (inMemoryCache.GetCachedVersionDataForIds useCache))

        match maybeCachedValue with
        | Some value ->
            log.Debug("Already cached: {value}", value)
            return value
        | None       ->
            let! result = this.FetchAllAndCache resultSetOptions |> Async.Map this.ConvertAsyncDataAccessControlledToAsyncData
            log.Debug("Wasn't cached: {result}", result)
            return result
    }

    member this.GetAllWithTotalCount (useCache: UseCache) (resultSetOptions: ResultSetOptions<'Index>) : Async<AsyncData<SubjectsWithTotalCount<'Id, VersionedData<'Projection>>>> = async {
        let maybeCachedValue: Option<AsyncData<SubjectsWithTotalCount<'Id, VersionedData<'Projection>>>> =
            match useCache with
            | No -> None
            | _  ->
                inMemoryCache.GetCachedIdsWithTotalCountForResultSetOptions useCache resultSetOptions
                |> Option.flatMap (AsyncData.tryMapIfAvailable (fun idsWithCount ->
                    inMemoryCache.GetCachedVersionDataForIds useCache idsWithCount.Data
                    |> Option.map (fun subjects ->
                        {
                            Subjects   = subjects
                            TotalCount = idsWithCount.TotalCount
                        }
                    )
                ))

        match maybeCachedValue with
        | Some value -> return value
        | None       -> return! this.FetchAllWithTotalCountAndCache resultSetOptions |> Async.Map this.ConvertAccessControlledToAsyncDataWithCount
    }

    member this.GetAllCount (useCache: UseCache) : Async<AsyncData<uint64>> = async {
        let maybeCachedValue: Option<AsyncData<uint64>> =
            match useCache with
            | No -> None
            | _  -> inMemoryCache.GetCachedAllCount useCache

        match maybeCachedValue with
        | Some value -> return value
        | None       -> return! this.FetchAllCountAndCache ()
    }

    member this.GetIndexed (useCache: UseCache) (query: IndexQuery<'Index>) : Async<AsyncData<Subjects<'Id, VersionedData<'Projection>>>> = async {
        let maybeCachedValue: Option<AsyncData<Subjects<'Id, VersionedData<'Projection>>>> =
            match useCache with
            | No -> None
            | _  ->
                inMemoryCache.GetCachedIdsForIndexQuery useCache query
                |> Option.flatMap (AsyncData.tryMapIfAvailable (inMemoryCache.GetCachedVersionDataForIds useCache))

        match maybeCachedValue with
        | Some value -> return value
        | None       -> return! this.FetchIndexedAndCache query |> Async.Map this.ConvertAsyncDataAccessControlledToAsyncData
    }

    member this.GetIndexedWithTotalCount (useCache: UseCache) (query: IndexQuery<'Index>) : Async<AsyncData<SubjectsWithTotalCount<'Id, VersionedData<'Projection>>>> = async {
        let maybeCachedValue: Option<AsyncData<SubjectsWithTotalCount<'Id, VersionedData<'Projection>>>> =
            match useCache with
            | No -> None
            | _  ->
                inMemoryCache.GetCachedIdsWithTotalCountForIndexQuery useCache query
                |> Option.flatMap (AsyncData.tryMapIfAvailable (fun idsWithCount ->
                    inMemoryCache.GetCachedVersionDataForIds useCache idsWithCount.Data
                    |> Option.map (fun subjects ->
                        {
                            Subjects   = subjects
                            TotalCount = idsWithCount.TotalCount
                        }
                    )
                ))

        match maybeCachedValue with
        | Some value -> return value
        | None       -> return! this.FetchIndexedWithTotalCountAndCache query |> Async.Map this.ConvertAccessControlledToAsyncDataWithCount
    }

    member this.GetIndexedCount (useCache: UseCache) (predicate: PreparedIndexPredicate<'Index>) : Async<AsyncData<uint64>> = async {
        let maybeCachedValue: Option<AsyncData<uint64>> =
            match useCache with
            | No -> None
            | _  -> inMemoryCache.GetCachedCountForIndexPredicate useCache predicate

        match maybeCachedValue with
        | Some value -> return value
        | None       -> return! this.FetchIndexedCountAndCache predicate
    }

    member private _.ConvertAccessControlledToAsyncData (accessControlled: AccessControlled<VersionedData<'Projection>, 'Id>) : 'Id * AsyncData<VersionedData<'Projection>> =
        match accessControlled with
        | AccessControlled.Granted versionedData -> (versionedData.Data.SubjectId, Available versionedData)
        | AccessControlled.Denied id             -> (id, AccessDenied)

    member private this.ConvertAsyncDataAccessControlledToAsyncData (versionedDataAD: AsyncData<seq<AccessControlled<VersionedData<'Projection>, 'Id>>>) : AsyncData<Subjects<'Id, VersionedData<'Projection>>> =
        versionedDataAD
        |> AsyncData.map (Seq.map this.ConvertAccessControlledToAsyncData)

    member private this.ConvertAccessControlledToAsyncDataWithCount (subjectsWithCountAD: AsyncData<ListWithTotalCount<AccessControlled<VersionedData<'Projection>, 'Id>>>) : AsyncData<SubjectsWithTotalCount<'Id, VersionedData<'Projection>>> =
        subjectsWithCountAD
        |> AsyncData.map (fun subjectsWithCount ->
            let subjects =
                subjectsWithCount.Data
                |> Seq.map this.ConvertAccessControlledToAsyncData
            {
                Subjects = subjects
                TotalCount = subjectsWithCount.TotalCount
            }
        )

    member this.GetAuditSnapshot (idString: string) (version: uint64) : Async<AsyncData<TemporalSnapshot<'Subject, 'Action, 'Constructor, 'Id>>> = async {
        match! thothEncodedHttpService.Request apiEndpoints.AuditSnapshot (idString, version) () with
        | Ok subject                  -> return Available subject
        | Error (Non200Code (404, _)) -> return Unavailable
        | Error (Non200Code (403, _)) -> return AccessDenied
        | Error (Non200Code (0,   _)) -> return Failed AsyncDataFailure.NetworkFailure
        | Error (Non200Code (responseCode, response)) -> return Failed (AsyncDataFailure.RequestFailure (RequestFailure.ofStatusCode (responseCode, jsonStringify(response))))
        | Error error                 -> return Failed (UnknownFailure (error.ToString()))
    }

    /////////////////////////////////////////
    //        FETCH-AND-CACHING            //
    /////////////////////////////////////////

    member private this.FetchOneAndCache (id: 'Id) : Async<AsyncData<VersionedData<'Projection>>> = async {
        let! result = throttling.ThrottledFetchOne id
        let _, cached = inMemoryCache.CacheOne (id, result)
        return cached
    }

    member private this.FetchOneMaybeConstructAndCache (id: 'Id) (ctor: 'Constructor) : Async<AsyncData<VersionedData<'Projection>>> = async {
        let! result = throttling.ThrottledFetchOneMaybeConstruct id ctor
        let _, cached = inMemoryCache.CacheOne (id, result)
        return cached
    }

    member private this.FetchManyAndCache (ids: NonemptySet<'Id>) : Async<AsyncData<seq<AccessControlled<VersionedData<'Projection>, 'Id>>>> = async {
        let! result = throttling.ThrottledFetchMany ids
        inMemoryCache.CacheMany result
        return result
    }

    member private this.FetchAllAndCache (resultSetOptions: ResultSetOptions<'Index>) : Async<AsyncData<seq<AccessControlled<VersionedData<'Projection>, 'Id>>>> = async {
        let! result = throttling.ThrottledFetchAll resultSetOptions
        inMemoryCache.CacheAll resultSetOptions result
        return result
    }

    member private this.FetchAllWithTotalCountAndCache (resultSetOptions: ResultSetOptions<'Index>) : Async<AsyncData<ListWithTotalCount<AccessControlled<VersionedData<'Projection>, 'Id>>>> = async {
        let! result = throttling.ThrottledFetchAllWithTotalCount resultSetOptions
        inMemoryCache.CacheAllWithTotalCount resultSetOptions result
        return result
    }

    member private this.FetchAllCountAndCache () : Async<AsyncData<uint64>> = async {
        let! result = throttling.ThrottledFetchAllCount ()
        inMemoryCache.CacheAllCount result
        return result
    }

    member private this.FetchIndexedAndCache (query: IndexQuery<'Index>) : Async<AsyncData<seq<AccessControlled<VersionedData<'Projection>, 'Id>>>> = async {
        let! result = throttling.ThrottledFetchIndexed query
        inMemoryCache.CacheIndexed query result
        return result
    }

    member private this.FetchIndexedWithTotalCountAndCache (query: IndexQuery<'Index>) : Async<AsyncData<ListWithTotalCount<AccessControlled<VersionedData<'Projection>, 'Id>>>> = async {
        let! result = throttling.ThrottledFetchIndexedWithTotalCount query
        inMemoryCache.CacheIndexedWithTotalCount query result
        return result
    }

    member private this.FetchIndexedCountAndCache (predicate: PreparedIndexPredicate<'Index>) : Async<AsyncData<uint64>> = async {
        let! result = throttling.ThrottledFetchIndexedCount predicate
        inMemoryCache.CacheIndexedCount predicate result
        return result
    }

    /////////////////////////////////////////
    //              ACTIONS                //
    /////////////////////////////////////////

    member private this.ProcessActError (id: 'Id) (maybePreviousValue: Option<VersionedData<'Projection>>) (error: ActionOrConstructionError<'OpError>) : unit =
        match maybePreviousValue with
        | Some previousValue ->
            inMemoryCache.CacheOneAvailable (id, previousValue)
            |> snd
            |> this.UpdateSubscriptions id
        | None ->
            let failedValue =
                match error with
                // NOTE we could take an 'OpError -> string function as part of the
                // SubjectService definition. That said, in general, 'OpErrors are delivered
                // to the user of the SubjectService explicitly in response to their calls, and this
                // case of passing it to them implicitly via an AsyncData is kind of an exception.
                | OpError opError                                 -> Failed (UnknownFailure (opError.ToString()))
                | SubjectAlreadyInitialized                       -> Failed (UserReadableFailure "Subject with the given id already exists")
                | SubjectLockedInTransaction                      -> Failed (UserReadableFailure "Subject is locked in transaction, please wait 5 minutes")
                | InsufficientPermissions                         -> AccessDenied
                | SucceededButInsufficientPermissionsToReadResult -> AccessDenied
                | NetworkFailure                                  -> Failed AsyncDataFailure.NetworkFailure
                | TooManyRequests                                 -> Failed (UserReadableFailure "Too many requests")

            inMemoryCache.CacheOne (id, failedValue)
            |> snd
            |> this.UpdateSubscriptions id

    member private this.ProcessActSuccess (id: 'Id) (action: 'Action) (updatedProjection: VersionedData<'Projection>) : unit =
        inMemoryCache.CacheOneAvailable (id, updatedProjection)
        |> snd
        |> this.UpdateSubscriptions id

        eventBus.Broadcast eventQueue (ActionPerformed action)

    member private this.ProcessConstructSuccess (id: 'Id) (updatedProjection: VersionedData<'Projection>) : unit =
        inMemoryCache.CacheOneAvailable (id, updatedProjection)
        |> snd
        |> this.UpdateSubscriptions id

    member this.Act (id: 'Id) (action: 'Action) : Async<Result<unit, ActionOrConstructionError<'OpError>>> = async {
        let maybePreviousValue = this.MarkAsFetching id

        let! response = thothEncodedHttpService.Request apiEndpoints.Act id action
        let result = this.ProcessActionResponse id action response identity

        inMemoryCache.InvalidateIndexCache ()

        match result with
        | Ok versionedData -> this.ProcessActSuccess id action versionedData
        | Error opError    -> this.ProcessActError id maybePreviousValue opError

        return result |> Result.map ignore
    }

    member this.ActWait (id: 'Id) (action: 'Action) (event: 'Event) : Async<Result<ApiActOrConstructAndWaitOnLifeEventResult<'Projection, 'Event>, ActionOrConstructionError<'OpError>>> =
        this.ActWaitWithTimeout id action event (TimeSpan.FromSeconds 20.0)

    member this.ActWaitWithTimeout (id: 'Id) (action: 'Action) (event: 'Event) (timeout: TimeSpan) : Async<Result<ApiActOrConstructAndWaitOnLifeEventResult<'Projection, 'Event>, ActionOrConstructionError<'OpError>>> = async {
        let maybePreviousValue = this.MarkAsFetching id

        let payload = {
            Action    = action
            LifeEvent = event
        }

        let! response = thothEncodedHttpService.Request apiEndpoints.ActAndWait (id, timeout) payload
        let result = this.ProcessActionResponse id action response identity

        inMemoryCache.InvalidateIndexCache ()

        match result with
        | Ok actWaitResult -> this.ProcessActSuccess id action actWaitResult.VersionedData
        | Error opError    -> this.ProcessActError id maybePreviousValue opError

        return result
    }

    member this.ActMaybeConstruct (id: 'Id) (action: 'Action) (ctor: 'Constructor) : Async<Result<unit, ActionOrConstructionError<'OpError>>> = async {
        let maybePreviousValue = this.MarkAsFetching id

        let payload = {
            Action      = action
            Constructor = ctor
        }

        let! response = thothEncodedHttpService.Request apiEndpoints.ActMaybeConstruct id payload
        let result = this.ProcessActionResponse id action response identity

        inMemoryCache.InvalidateIndexCache ()
        inMemoryCache.InvalidateAllCache ()

        match result with
        | Ok versionedData -> this.ProcessActSuccess id action versionedData
        | Error opError    -> this.ProcessActError id maybePreviousValue opError

        return result |> Result.map ignore
    }

    member this.ActMaybeConstructAndWait (id: 'Id) (action: 'Action) (ctor: 'Constructor) (event: 'Event) : Async<Result<ApiActOrConstructAndWaitOnLifeEventResult<'Projection, 'Event>, ActionOrConstructionError<'OpError>>> = async {
        let maybePreviousValue = this.MarkAsFetching id

        let payload = {
            Action      = action
            Constructor = ctor
            LifeEvent   = event
        }

        let! response = thothEncodedHttpService.Request apiEndpoints.ActMaybeConstructAndWait (id, TimeSpan.FromSeconds 20.0) payload
        let result = this.ProcessActionResponse id action response identity

        inMemoryCache.InvalidateIndexCache ()
        inMemoryCache.InvalidateAllCache ()

        match result with
        | Ok actWaitResult -> this.ProcessActSuccess id action actWaitResult.VersionedData
        | Error opError    -> this.ProcessActError id maybePreviousValue opError

        return result
    }

    member (* want protected *) this.ProcessActionResponse<'T, 'U> (id: 'Id) (action: 'Action) (response: Result<'T, RequestError>) (transform: 'T -> 'U) : Result<'U, ActionOrConstructionError<'OpError>> =
        let actionName = getCaseName action

        let processedResult =
            this.ProcessActionLikeResponse
                response
                transform
                (fun decodingError webResponse ->
                    Log.Error (sprintf "Failed to decode error body %O for action %O on id %O" webResponse.body actionName id, decodingError)
                    failwith (sprintf "Failed to decode error body %O, error was %s" webResponse.body decodingError)
                )
                (fun e ->
                    #if EGGSHELL_PLATFORM_IS_WEB
                    Log.Error (sprintf "Unexpected error in response to action %O on id %O" actionName id, e)
                    #else
                    Log.Error (sprintf "Unexpected error %s in response to action %O on id %O" (Json.ToString e) actionName id)
                    #endif
                    failwith (sprintf "Unexpected error in response %O" e)
                )

        processedResult

    member private this.ProcessActionLikeResponse<'T, 'U>
        (response: Result<'T, RequestError>)
        (transform: 'T -> 'U)
        (onDecodingError: string -> LibClient.Services.HttpService.ReactXPHttp.WebResponse -> Result<'U, ActionOrConstructionError<'OpError>>)
        (onOtherRequestError: RequestError -> Result<'U, ActionOrConstructionError<'OpError>>)
        : Result<'U, ActionOrConstructionError<'OpError>> =

        match response with
        | Ok result ->
            Ok (transform result)

        | Error e ->
            match e with
            // TransitionError, ConstructionError, TransitionNotAllowed
            | Non200Code (422, webResponse) ->
                match apiEndpoints.DecodeError (webResponse.body :?> string) with
                | Ok decodedError     -> Error (OpError decodedError)
                | Error decodingError -> onDecodingError decodingError webResponse
            | Non200Code (205, _) -> Error SucceededButInsufficientPermissionsToReadResult
            | Non200Code (403, _) -> Error InsufficientPermissions
            | Non200Code (0,   _) -> Error NetworkFailure
            | Non200Code (409, _) -> Error SubjectAlreadyInitialized
            | Non200Code (423, _) -> Error SubjectLockedInTransaction
            | Non200Code (429, _) -> Error TooManyRequests
            | otherRequestError   -> onOtherRequestError otherRequestError


    /////////////////////////////////////////
    //           CONSTRUCTION              //
    /////////////////////////////////////////

    member this.Construct (ctor: 'Constructor) : Async<Result<'Projection, ActionOrConstructionError<'OpError>>> = async {
        let! response = thothEncodedHttpService.Request apiEndpoints.Construct () ctor
        let projectionResult =
            response
            |> Result.map VersionedData.data

        inMemoryCache.InvalidateIndexCache ()
        inMemoryCache.InvalidateAllCache ()

        // this is a HACK until query realitime subscriptions are implemented
        this.RerunQueries ()

        return this.ProcessConstructionResponse projectionResult identity
    }

    member this.ConstructAndWait (ctor: 'Constructor) (event: 'Event) : Async<Result<ApiActOrConstructAndWaitOnLifeEventResult<'Projection, 'Event>, ActionOrConstructionError<'OpError>>> = async {
        let payload = {
            Constructor = ctor
            LifeEvent   = event
        }

        let! response = thothEncodedHttpService.Request apiEndpoints.ConstructAndWait () payload
        let result = this.ProcessConstructionResponse response identity

        inMemoryCache.InvalidateIndexCache ()
        inMemoryCache.InvalidateAllCache ()

        // this is a HACK until query realitime subscriptions are implemented
        this.RerunQueries ()

        match result with
        | Ok actWaitResult -> this.ProcessConstructSuccess actWaitResult.VersionedData.Data.SubjectId actWaitResult.VersionedData
        | Error _opError   -> Noop

        return result
    }

    member private this.ProcessConstructionResponse<'T, 'U> (response: Result<'T, RequestError>) (transform: 'T -> 'U) : Result<'U, ActionOrConstructionError<'OpError>> =
        this.ProcessActionLikeResponse
            response
            transform
            (fun decodingError webResponse ->
                Log.Error (sprintf "Failed to decode error body %O for construction" webResponse.body, decodingError)
                failwith (sprintf "Failed to decode error body %O, error was %s" webResponse.body decodingError)
            )
            (fun e ->
                Log.Error ("Unexpected error in response to construction", response)
                failwith (sprintf "Unexpected error in response %O" e)
            )

type SubjectService private () =
    static member inline Create<'Subject, 'Id, 'Index, 'NumericIndex, 'StringIndex, 'SearchIndex, 'GeographyIndex, 'Constructor, 'Action, 'Event, 'OpError
                when 'Subject :> Subject<'Id>
                and 'Subject: equality
                and 'Id :> SubjectId
                and 'Id: comparison
                and 'Constructor :> Constructor
                and 'Action :> LifeAction
                and 'Event :> LifeEvent
                and 'Event: comparison
                and 'OpError :> OpError
                and 'Index :> SubjectIndex<'Index, 'NumericIndex, 'StringIndex, 'SearchIndex, 'GeographyIndex, 'OpError>
                and 'NumericIndex :> SubjectNumericIndex<'OpError>
                and 'StringIndex :> SubjectStringIndex<'OpError>
                and 'SearchIndex :> SubjectSearchIndex
                and 'GeographyIndex :> SubjectGeographyIndex>
            (lifeCycleDef: LifeCycleDef<'Subject, 'Action, 'OpError, 'Constructor, 'Event, 'Index, 'Id>)
            (reasonablyFreshTTLs: ReasonablyFreshTTLs)
            (realTimeService: RealTimeService)
            (thothEncodedHttpService: ThothEncodedHttpService)
            (eventBus: EventBus)
            (backendUrl: string)
            : SubjectService<'Subject, 'Subject, 'Id, 'Index, 'NumericIndex, 'StringIndex, 'SearchIndex, 'GeographyIndex, 'Constructor, 'Action, 'Event, 'OpError> =
        let endpoints =
            SubjectEndpoints.Make<'Subject, 'Subject, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>(
                lifeCycleDef.Key.LocalLifeCycleName,
                None,
                $"{backendUrl}/api/v1/ecosystem/{lifeCycleDef.Key.EcosystemName}/subject"
            )

        SubjectService<'Subject, 'Subject, 'Id, 'Index, 'NumericIndex, 'StringIndex, 'SearchIndex, 'GeographyIndex, 'Constructor, 'Action, 'Event, 'OpError>(
            lifeCycleDef.Key,
            None,
            reasonablyFreshTTLs,
            realTimeService,
            thothEncodedHttpService,
            eventBus,
            endpoints
        )

    static member inline CreateProjected<'Subject, 'Projection, 'Id, 'Index, 'NumericIndex, 'StringIndex, 'SearchIndex, 'GeographyIndex, 'Constructor, 'Action, 'Event, 'OpError
                when 'Subject :> Subject<'Id>
                and 'Projection :> SubjectProjection<'Id>
                and 'Projection: equality
                and 'Id :> SubjectId
                and 'Id: comparison
                and 'Constructor :> Constructor
                and 'Action :> LifeAction
                and 'Event :> LifeEvent
                and 'Event: comparison
                and 'OpError :> OpError
                and 'Index :> SubjectIndex<'Index, 'NumericIndex, 'StringIndex, 'SearchIndex, 'GeographyIndex, 'OpError>
                and 'NumericIndex :> SubjectNumericIndex<'OpError>
                and 'StringIndex :> SubjectStringIndex<'OpError>
                and 'SearchIndex :> SubjectSearchIndex
                and 'GeographyIndex :> SubjectGeographyIndex>
            (lifeCycleDef: LifeCycleDef<'Subject, 'Action, 'OpError, 'Constructor, 'Event, 'Index, 'Id>)
            (projectionDef: SubjectProjectionDef<'Projection, 'Subject, 'Action, 'OpError, 'Constructor, 'Event, 'Index, 'Id>)
            (reasonablyFreshTTLs: ReasonablyFreshTTLs)
            (realTimeService: RealTimeService)
            (thothEncodedHttpService: ThothEncodedHttpService)
            (eventBus: EventBus)
            (backendUrl: string)
            : SubjectService<'Subject, 'Projection, 'Id, 'Index, 'NumericIndex, 'StringIndex, 'SearchIndex, 'GeographyIndex, 'Constructor, 'Action, 'Event, 'OpError> =
        let endpoints =
            SubjectEndpoints.Make<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>(
                lifeCycleDef.Key.LocalLifeCycleName,
                Some projectionDef.ProjectionName,
                $"{backendUrl}/api/v1/ecosystem/{lifeCycleDef.Key.EcosystemName}/subject"
            )

        SubjectService<'Subject, 'Projection, 'Id, 'Index, 'NumericIndex, 'StringIndex, 'SearchIndex, 'GeographyIndex, 'Constructor, 'Action, 'Event, 'OpError>(
            lifeCycleDef.Key,
            Some projectionDef.ProjectionName,
            reasonablyFreshTTLs,
            realTimeService,
            thothEncodedHttpService,
            eventBus,
            endpoints
        )

    static member UpcastToInterface (subjectService: ISubjectService<#Subject<'Id>, 'Projection, 'Id, #SubjectIndex<'OpError>, #Constructor, #LifeAction, #LifeEvent, 'OpError>) =
        subjectService :> ISubjectService<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>

// NOTE this is a temporary tool for solving the problem that ActAndWait responses
// have a weird modelling quirk — the return a Result, but in the Ok side of the result,
// there is a possibility for errors given the inner type. Some of our use cases assume
// that if we're given an Ok, it can be safely ignored, and so errors get missed. The code
// below remaps this situation in a way that allows us to rely on Result.Ok really meaning Ok.
type OpErrorsWithPossibleTimeout =
    // Whilst we could provide an implementation that returns Result<'T, string>, we intentionally do not expose that to avoid
    // accidentally coupling ourselves to it. It is unclear whether - in a world of unidirectional data flow and real-time streams -
    // it makes sense for the backend to include anything in the response body.
    static member HandleResultIgnoringSuccessResult
            (result: Async<Result<ApiActOrConstructAndWaitOnLifeEventResult<'T, 'LifeEvent>, ActionOrConstructionError<'OpError>>>)
            (mapLifeEvent: 'LifeEvent -> Result<unit, string>)
            (mapError: ActionOrConstructionError<'OpError> -> string): Async<Result<unit, string>> =
        async {
            match! result with
            | Ok (ApiActOrConstructAndWaitOnLifeEventResult.LifeEventTriggered (_finalValueAfterEvent, triggeredEvent)) ->
                match mapLifeEvent triggeredEvent with
                | Ok _ ->
                    return Ok ()
                | Error message ->
                    return Error message
            | Error error ->
                return
                    error
                    |> mapError
                    |> Error
            | Ok (ApiActOrConstructAndWaitOnLifeEventResult.WaitOnLifeEventTimedOut _) ->
                return Error "Operation timed out"
        }
