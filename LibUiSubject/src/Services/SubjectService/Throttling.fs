namespace LibUiSubject.Services.SubjectService

open System
open LibLangFsharp
open LibClient
open LibClient.EventBus
open LibClient.Services.HttpService.Types
open LibClient.Services.HttpService.ThothEncodedHttpService
open LibLifeCycleTypes.Api.V1
open LibUiSubject

[<AutoOpen>]
module private ThrottlingHelpers =
    type ResultSetOptions<'SubjectIndex> with
        member inline this.EraseIndexArgumentToAvoidStackOverflow : ResultSetOptions<unit> =
            match this with
            // attempt to reflect a concrete 'SubjectIndex in fable will stack overflow
            // See: https://github.com/fable-compiler/Fable/issues/3607 remove the hack when root cause fixed
            | ResultSetOptions_ x -> ResultSetOptions_ x

    type IndexQuery<'SubjectIndex> with
        member inline this.EraseIndexArgumentToAvoidStackOverflow : IndexQuery<unit> =
            match this with
            // attempt to reflect a concrete 'SubjectIndex in fable will stack overflow
            // See: https://github.com/fable-compiler/Fable/issues/3607 remove the hack when root cause fixed
            | IndexQuery (x1, x2) -> IndexQuery (x1, x2)

    type PreparedIndexPredicate<'SubjectIndex> with
        member inline this.EraseIndexArgumentToAvoidStackOverflow : PreparedIndexPredicate<unit> =
            match this with
            // attempt to reflect a concrete 'SubjectIndex in fable will stack overflow
            // See: https://github.com/fable-compiler/Fable/issues/3607 remove the hack when root cause fixed
            | PreparedIndexPredicate x -> PreparedIndexPredicate x

[<AutoOpen>]
module NecessarilyPublicHelpers =
    let defaultIdToString<'Id when 'Id :> SubjectId> (id: 'Id) : string =
        id.IdString

    let inline endpointNotImplemented<'UrlParams, 'Request, 'Response>() = makeEndpoint<'UrlParams, 'Request, 'Request, 'Response> Post (fun _ -> failwith "endpoint not implemented") id

// NOTE this was originally called SubjectServiceTypeBindings, because
// that's what its purpose is, to facilitate inline type reflection binding
// necessitated by Fable. But in practice, this record ended up only having
// endpoints in it, so for better readability, until something other than
// endpoitns needs to go in here, I'm renaming it to SubjectEndpoints
type SubjectEndpoints<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError
                        when 'Subject      :> Subject<'Id>
                        and  'Id           :> SubjectId
                        and  'Id           :  comparison
                        and  'Constructor  :> Constructor
                        and  'Action       :> LifeAction
                        and  'Event        :> LifeEvent
                        and  'OpError      :> OpError
                        and  'Index        :> SubjectIndex<'OpError>> = {
    Get: ApiEndpoint<unit, 'Id, VersionedData<'Projection>>
    GetMaybeConstruct: ApiEndpoint<unit, GetMaybeConstruct<'Id, 'Constructor>, VersionedData<'Projection>>
    GetMany: ApiEndpoint<unit, NonemptySet<'Id>, List<AccessControlled<VersionedData<'Projection>, 'Id>>>
    Act: ApiEndpoint<'Id, 'Action, VersionedData<'Projection>>
    ActAndWait: ApiEndpoint<'Id * TimeSpan,  ActAndWaitOnLifeEvent<'Action, 'Event>, ApiActOrConstructAndWaitOnLifeEventResult<'Projection, 'Event>>
    ActMaybeConstruct: ApiEndpoint<'Id, ActMaybeConstruct<'Action, 'Constructor>, VersionedData<'Projection>>
    ActMaybeConstructAndWait: ApiEndpoint<'Id * TimeSpan,  ActMaybeConstructAndWaitOnLifeEvent<'Action, 'Constructor, 'Event>, ApiActOrConstructAndWaitOnLifeEventResult<'Projection, 'Event>>
    GetAll: ApiEndpoint<unit, ResultSetOptions<'Index>, List<AccessControlled<VersionedData<'Projection>, 'Id>>>
    GetAllWithTotalCount: ApiEndpoint<unit, ResultSetOptions<'Index>, ListWithTotalCount<AccessControlled<VersionedData<'Projection>, 'Id>>>
    TotalCount: ApiEndpoint<unit, unit, uint64>
    GetIndexed: ApiEndpoint<unit, IndexQuery<'Index>, List<AccessControlled<VersionedData<'Projection>, 'Id>>>
    GetIndexedWithTotalCount: ApiEndpoint<unit, IndexQuery<'Index>, ListWithTotalCount<AccessControlled<VersionedData<'Projection>, 'Id>>>
    GetIndexedCount: ApiEndpoint<unit, PreparedIndexPredicate<'Index>, uint64>
    Construct: ApiEndpoint<unit, 'Constructor, VersionedData<'Projection>>
    ConstructAndWait: ApiEndpoint<unit, ConstructAndWaitOnLifeEvent<'Constructor, 'Event>, ApiActOrConstructAndWaitOnLifeEventResult<'Projection, 'Event>>
    AuditSnapshot: ApiEndpoint<string * uint64, unit, TemporalSnapshot<'Subject, 'Action, 'Constructor, 'Id>>
    DecodeError: string -> Result<'OpError, string>
    DecodeAccessControlledSubjectChange: string -> Result<AccessControlledSubjectChange<'Subject, 'Id>, string>
    DecodeAccessControlledSubjectProjectionChange: string -> Result<AccessControlledSubjectChange<'Projection, 'Id>, string>
} with
    static member inline Make<'Subject1, 'Projection1, 'Id1, 'Index1, 'Constructor1, 'Action1, 'Event1, 'OpError1
                        when 'Subject1      :> Subject<'Id1>
                        and  'Id1           :> SubjectId
                        and  'Id1           :  comparison
                        and  'Constructor1  :> Constructor
                        and  'Action1       :> LifeAction
                        and  'Event1        :> LifeEvent
                        and  'OpError1      :> OpError
                        and  'Index1        :> SubjectIndex<'OpError1>> (
        lifeCycleName: string,
        maybeProjectionName: Option<string>,
        subjectUrlBase: string,
        ?maybeIToString: ('Id1 -> string)
    ) : SubjectEndpoints<'Subject1, 'Projection1, 'Id1, 'Index1, 'Constructor1, 'Action1, 'Event1, 'OpError1> =
        let idToString = (defaultArg maybeIToString defaultIdToString) >> Uri.EscapeDataString
        let (projectionParamWithQuestionMark, projectionParamWithAmpersand) =
            maybeProjectionName
            |> Option.map (fun name -> ($"?projection={name}", $"&projection={name}"))
            |> Option.getOrElse ("", "")
        {
            Get = makeEndpoint<unit, 'Id1, _, VersionedData<'Projection1>> Post (fun () -> $"{subjectUrlBase}/{lifeCycleName}/get{projectionParamWithQuestionMark}") id
            GetMaybeConstruct = makeEndpoint<unit, GetMaybeConstruct<'Id1, 'Constructor1>, _, VersionedData<'Projection1>> Post (fun () -> $"{subjectUrlBase}/{lifeCycleName}/getMaybeConstruct{projectionParamWithQuestionMark}") id
            GetMany = makeEndpoint<unit, NonemptySet<'Id1>, _, List<AccessControlled<VersionedData<'Projection1>, 'Id1>>> Post (fun () -> $"{subjectUrlBase}/{lifeCycleName}/getMany{projectionParamWithQuestionMark}") id
            Act = makeEndpoint<'Id1, 'Action1, _, VersionedData<'Projection1>> Post (fun id -> $"{subjectUrlBase}/{lifeCycleName}/act/{idToString id}{projectionParamWithQuestionMark}") id
            ActAndWait = makeEndpoint<'Id1 * TimeSpan, ActAndWaitOnLifeEvent<'Action1, 'Event1>, _, ApiActOrConstructAndWaitOnLifeEventResult<'Projection1, 'Event1>> Post (fun (id, waitFor) -> $"{subjectUrlBase}/{lifeCycleName}/actAndWait/{idToString id}?waitForMs={int waitFor.TotalMilliseconds}{projectionParamWithAmpersand}") id
            ActMaybeConstruct = makeEndpoint<'Id1, ActMaybeConstruct<'Action1, 'Constructor1>, _, VersionedData<'Projection1>> Post (fun id -> $"{subjectUrlBase}/{lifeCycleName}/actMaybeConstruct/{idToString id}{projectionParamWithQuestionMark}") id
            ActMaybeConstructAndWait = makeEndpoint<'Id1 * TimeSpan, ActMaybeConstructAndWaitOnLifeEvent<'Action1, 'Constructor1, 'Event1>, _, ApiActOrConstructAndWaitOnLifeEventResult<'Projection1, 'Event1>> Post (fun (id, waitFor) -> $"{subjectUrlBase}/{lifeCycleName}/actMaybeConstructAndWait/{idToString id}?waitForMs={int waitFor.TotalMilliseconds}{projectionParamWithAmpersand}") id
            GetAll = makeEndpoint<unit, ResultSetOptions<'Index1>, ResultSetOptions<unit>, List<AccessControlled<VersionedData<'Projection1>, 'Id1>>> Post (fun () -> $"{subjectUrlBase}/{lifeCycleName}/all{projectionParamWithQuestionMark}") (fun x -> x.EraseIndexArgumentToAvoidStackOverflow)
            GetAllWithTotalCount = makeEndpoint<unit, ResultSetOptions<'Index1>, ResultSetOptions<unit>, ListWithTotalCount<AccessControlled<VersionedData<'Projection1>, 'Id1>>> Post (fun () -> $"{subjectUrlBase}/{lifeCycleName}/allWithTotalCount{projectionParamWithQuestionMark}") (fun x -> x.EraseIndexArgumentToAvoidStackOverflow)
            TotalCount = makeEndpoint<unit, unit, _, uint64> Post (fun () -> $"{subjectUrlBase}/{lifeCycleName}/totalCount") id
            GetIndexed = makeEndpoint<unit, IndexQuery<'Index1>, IndexQuery<unit>, List<AccessControlled<VersionedData<'Projection1>, 'Id1>>> Post (fun () -> $"{subjectUrlBase}/{lifeCycleName}/index{projectionParamWithQuestionMark}") (fun x -> x.EraseIndexArgumentToAvoidStackOverflow)
            GetIndexedWithTotalCount = makeEndpoint<unit, IndexQuery<'Index1>, IndexQuery<unit>, ListWithTotalCount<AccessControlled<VersionedData<'Projection1>, 'Id1>>> Post (fun () -> $"{subjectUrlBase}/{lifeCycleName}/indexWithTotalCount{projectionParamWithQuestionMark}") (fun x -> x.EraseIndexArgumentToAvoidStackOverflow)
            GetIndexedCount = makeEndpoint<unit, PreparedIndexPredicate<'Index1>, PreparedIndexPredicate<unit>, uint64> Post (fun () -> $"{subjectUrlBase}/{lifeCycleName}/index/count") (fun x -> x.EraseIndexArgumentToAvoidStackOverflow)
            Construct = makeEndpoint<unit, 'Constructor1, _, VersionedData<'Projection1>> Put  (fun () -> $"{subjectUrlBase}/{lifeCycleName}/{projectionParamWithQuestionMark}") id
            ConstructAndWait = makeEndpoint<unit, ConstructAndWaitOnLifeEvent<'Constructor1, 'Event1>, _, ApiActOrConstructAndWaitOnLifeEventResult<'Projection1, 'Event1>> Put  (fun () -> $"{subjectUrlBase}/{lifeCycleName}/constructWait{projectionParamWithQuestionMark}") id
            AuditSnapshot = makeEndpoint<string * uint64, unit, _, TemporalSnapshot<'Subject1, 'Action1, 'Constructor1, 'Id1>> Get (fun (id, version) -> $"{subjectUrlBase}/{lifeCycleName}/audit/{id}/{version}") id
            DecodeError = Json.FromString<'OpError1>
            DecodeAccessControlledSubjectChange = fun (json: string) -> Json.FromString<AccessControlledSubjectChange<'Subject1, 'Id1>> json
            DecodeAccessControlledSubjectProjectionChange = fun (json: string) -> Json.FromString<AccessControlledSubjectChange<'Projection1, 'Id1>> json
        }

type SubjectServiceEvent<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError
                      when 'Subject      :> Subject<'Id>
                      and  'Id           :> SubjectId
                      and  'Id           :  comparison
                      and  'Constructor  :> Constructor
                      and  'Action       :> LifeAction
                      and  'Event        :> LifeEvent
                      and  'OpError      :> OpError
                      and  'Index        :> SubjectIndex<'OpError>> =
| ActionPerformed of 'Action
| MultipleFetched of seq<AccessControlled<'Projection, 'Id>>

type internal Throttling<'Subject, 'Projection, 'Id, 'Index, 'NumericIndex, 'StringIndex, 'SearchIndex, 'GeographyIndex, 'Constructor, 'Action, 'Event, 'OpError
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
                      and  'GeographyIndex :> SubjectGeographyIndex>(
            thothEncodedHttpService: ThothEncodedHttpService,
            apiEndpoints: SubjectEndpoints<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>,
            eventBus: EventBus,
            eventQueue: EventBus.Queue<SubjectServiceEvent<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>>
        ) =
    let mutable ongoingRequestsOne: Map<'Id, Deferred<AsyncData<VersionedData<'Projection>>>> = Map.empty
    let mutable ongoingRequestsMany: Map<NonemptySet<'Id>, Deferred<AsyncData<seq<AccessControlled<VersionedData<'Projection>, 'Id>>>>> = Map.empty
    let mutable ongoingRequestsAll: Map<ResultSetOptions<'Index>, Deferred<AsyncData<seq<AccessControlled<VersionedData<'Projection>, 'Id>>>>> = Map.empty
    let mutable ongoingRequestsAllWithTotalCount: Map<ResultSetOptions<'Index>, Deferred<AsyncData<ListWithTotalCount<AccessControlled<VersionedData<'Projection>, 'Id>>>>> = Map.empty
    let mutable ongoingRequestsAllCount: Map<unit, Deferred<AsyncData<uint64>>> = Map.empty
    let mutable ongoingRequestsIndexed: Map<IndexQuery<'Index>, Deferred<AsyncData<seq<AccessControlled<VersionedData<'Projection>, 'Id>>>>> = Map.empty
    let mutable ongoingRequestsIndexedWithTotalCount: Map<IndexQuery<'Index>, Deferred<AsyncData<ListWithTotalCount<AccessControlled<VersionedData<'Projection>, 'Id>>>>> = Map.empty
    let mutable ongoingRequestsIndexedCount: Map<PreparedIndexPredicate<'Index>, Deferred<AsyncData<uint64>>> = Map.empty

    member private _.Throttled<'T, 'K when 'K : comparison> (ongoingRequests: Map<'K, Deferred<'T>>) (updateOngoingRequests: (Map<'K, Deferred<'T>> -> Map<'K, Deferred<'T>>) -> unit) (key: 'K) (rawFetch: unit -> Async<'T>) : Async<'T> =
        match ongoingRequests.TryFind key with
        | Some ongoingRequestDeferred -> ongoingRequestDeferred.Value
        | None ->
            let newRequestDeferred = Deferred<'T>()
            updateOngoingRequests (fun ongoingRequests -> ongoingRequests.Add (key, newRequestDeferred))

            async {
                try
                    let! result = rawFetch ()
                    updateOngoingRequests (fun ongoingRequests -> ongoingRequests.Remove key)
                    newRequestDeferred.Resolve result
                with
                | exn ->
                    updateOngoingRequests (fun ongoingRequests -> ongoingRequests.Remove key)
                    newRequestDeferred.Reject exn
            } |> startSafely

            newRequestDeferred.Value

    member this.ThrottledFetchOne (id: 'Id) : Async<AsyncData<VersionedData<'Projection>>> =
        this.Throttled
            ongoingRequestsOne
            (fun updater -> ongoingRequestsOne <- updater ongoingRequestsOne)
            id
            (fun () -> this.RawFetchOne id)

    member this.ThrottledFetchOneMaybeConstruct (id: 'Id) (ctor: 'Constructor) : Async<AsyncData<VersionedData<'Projection>>> =
        this.Throttled
            ongoingRequestsOne
            (fun updater -> ongoingRequestsOne <- updater ongoingRequestsOne)
            id
            (fun () -> this.RawFetchOneMaybeConstruct id ctor)

    member this.ThrottledFetchMany (ids: NonemptySet<'Id>) : Async<AsyncData<seq<AccessControlled<VersionedData<'Projection>, 'Id>>>> =
        this.Throttled
            ongoingRequestsMany
            (fun updater -> ongoingRequestsMany <- updater ongoingRequestsMany)
            ids
            (fun () -> this.RawFetchMany ids)

    member this.ThrottledFetchAll (resultSetOptions: ResultSetOptions<'Index>) : Async<AsyncData<seq<AccessControlled<VersionedData<'Projection>, 'Id>>>> =
        this.Throttled
            ongoingRequestsAll
            (fun updater -> ongoingRequestsAll <- updater ongoingRequestsAll)
            resultSetOptions
            (fun () -> this.RawFetchAll resultSetOptions)

    member this.ThrottledFetchAllWithTotalCount (resultSetOptions: ResultSetOptions<'Index>) : Async<AsyncData<ListWithTotalCount<AccessControlled<VersionedData<'Projection>, 'Id>>>> =
        this.Throttled
            ongoingRequestsAllWithTotalCount
            (fun updater -> ongoingRequestsAllWithTotalCount <- updater ongoingRequestsAllWithTotalCount)
            resultSetOptions
            (fun () -> this.RawFetchAllWithTotalCount resultSetOptions)

    member this.ThrottledFetchAllCount () : Async<AsyncData<uint64>> =
        this.Throttled
            ongoingRequestsAllCount
            (fun updater -> ongoingRequestsAllCount <- updater ongoingRequestsAllCount)
            ()
            (fun () -> this.RawFetchAllCount ())

    member this.ThrottledFetchIndexed (query: IndexQuery<'Index>) : Async<AsyncData<seq<AccessControlled<VersionedData<'Projection>, 'Id>>>> =
        this.Throttled
            ongoingRequestsIndexed
            (fun updater -> ongoingRequestsIndexed <- updater ongoingRequestsIndexed)
            query
            (fun () -> this.RawFetchIndexed query)

    member this.ThrottledFetchIndexedWithTotalCount (query: IndexQuery<'Index>) : Async<AsyncData<ListWithTotalCount<AccessControlled<VersionedData<'Projection>, 'Id>>>> =
        this.Throttled
            ongoingRequestsIndexedWithTotalCount
            (fun updater -> ongoingRequestsIndexedWithTotalCount <- updater ongoingRequestsIndexedWithTotalCount)
            query
            (fun () -> this.RawFetchIndexedWithTotalCount query)

    member this.ThrottledFetchIndexedCount (predicate: PreparedIndexPredicate<'Index>) : Async<AsyncData<uint64>> =
        this.Throttled
            ongoingRequestsIndexedCount
            (fun updater -> ongoingRequestsIndexedCount <- updater ongoingRequestsIndexedCount)
            predicate
            (fun () -> this.RawFetchIndexedCount predicate)

    member private _.RawFetchOne (id: 'Id) : Async<AsyncData<VersionedData<'Projection>>> = async {
        match! thothEncodedHttpService.Request apiEndpoints.Get () id with
        | Ok versionedData                            -> return (Available versionedData)
        | Error (Non200Code (404, _))                 -> return Unavailable
        | Error (Non200Code (403, _))                 -> return AccessDenied
        | Error (Non200Code (0,   _))                 -> return Failed AsyncDataFailure.NetworkFailure
        | Error (Non200Code (responseCode, response)) -> return Failed (AsyncDataFailure.RequestFailure (RequestFailure.ofStatusCode (responseCode, jsonStringify(response))))
        | Error error                                 -> return Failed (UnknownFailure (error.ToString()))
    }

    member private _.RawFetchOneMaybeConstruct (id: 'Id) (ctor: 'Constructor) : Async<AsyncData<VersionedData<'Projection>>> = async {
        let payload = {
            Id          = id
            Constructor = ctor
        }

        match! thothEncodedHttpService.Request apiEndpoints.GetMaybeConstruct () payload with
        | Ok versionedData                            -> return (Available versionedData)
        | Error (Non200Code (404, _))                 -> return Unavailable
        | Error (Non200Code (403, _))                 -> return AccessDenied
        | Error (Non200Code (0,   _))                 -> return Failed AsyncDataFailure.NetworkFailure
        | Error (Non200Code (responseCode, response)) -> return Failed (AsyncDataFailure.RequestFailure (RequestFailure.ofStatusCode (responseCode, jsonStringify(response))))
        | Error error                                 -> return Failed (UnknownFailure (error.ToString()))
    }

    member private this.RawFetchMany (ids: NonemptySet<'Id>) : Async<AsyncData<seq<AccessControlled<VersionedData<'Projection>, 'Id>>>> = async {
        let! response = thothEncodedHttpService.Request apiEndpoints.GetMany () ids
        return this.ProcessGetManyResponse response
    }

    member private this.RawFetchAll (resultSetOptions: ResultSetOptions<'Index>) : Async<AsyncData<seq<AccessControlled<VersionedData<'Projection>, 'Id>>>> = async {
        let! response = thothEncodedHttpService.Request apiEndpoints.GetAll () resultSetOptions
        return this.ProcessGetManyResponse response
    }

    member private this.RawFetchAllWithTotalCount (resultSetOptions: ResultSetOptions<'Index>) : Async<AsyncData<ListWithTotalCount<AccessControlled<VersionedData<'Projection>, 'Id>>>> = async {
        let! response = thothEncodedHttpService.Request apiEndpoints.GetAllWithTotalCount () resultSetOptions
        return this.ProcessGetManyWithTotalCountResponse response
    }

    member private this.RawFetchAllCount (): Async<AsyncData<uint64>> = async {
        let! response = thothEncodedHttpService.Request apiEndpoints.TotalCount () ()
        return this.ProcessGetCountResponse response
    }

    member private this.RawFetchIndexed (query: IndexQuery<'Index>) : Async<AsyncData<seq<AccessControlled<VersionedData<'Projection>, 'Id>>>> = async {
        let! response = thothEncodedHttpService.Request apiEndpoints.GetIndexed () query
        return this.ProcessGetManyResponse response
    }

    member private this.RawFetchIndexedWithTotalCount (query: IndexQuery<'Index>) : Async<AsyncData<ListWithTotalCount<AccessControlled<VersionedData<'Projection>, 'Id>>>> = async {
        let! response = thothEncodedHttpService.Request apiEndpoints.GetIndexedWithTotalCount () query
        return this.ProcessGetManyWithTotalCountResponse response
    }

    member private this.RawFetchIndexedCount (predicate: PreparedIndexPredicate<'Index>) : Async<AsyncData<uint64>> = async {
        let! response = thothEncodedHttpService.Request apiEndpoints.GetIndexedCount () predicate
        return this.ProcessGetCountResponse response
    }

    member private _.ProcessGetManyResponse (response: Result<List<AccessControlled<VersionedData<'Projection>, 'Id>>, RequestError>) : AsyncData<seq<AccessControlled<VersionedData<'Projection>, 'Id>>> =
        match response with
        | Ok accessControlledVersionedData ->
            let accessControlledProjections =
                accessControlledVersionedData
                |> List.map (AccessControlled.map VersionedData.data)
            eventBus.Broadcast eventQueue (MultipleFetched accessControlledProjections)
            Available accessControlledVersionedData
        | Error (Non200Code (404, _)) -> Unavailable
        | Error (Non200Code (403, _)) -> AccessDenied
        | Error (Non200Code (0,   _)) -> Failed AsyncDataFailure.NetworkFailure
        | Error (Non200Code (responseCode, response)) -> Failed (AsyncDataFailure.RequestFailure (RequestFailure.ofStatusCode (responseCode, jsonStringify(response))))
        | Error error                 -> Failed (UnknownFailure (error.ToString()))

    member private _.ProcessGetManyWithTotalCountResponse (response: Result<ListWithTotalCount<AccessControlled<VersionedData<'Projection>, 'Id>>, RequestError>) : AsyncData<ListWithTotalCount<AccessControlled<VersionedData<'Projection>, 'Id>>> =
        match response with
        | Ok accessControlledVersionedDataWithCount ->
            let accessControlledProjectionsWithCount =
                accessControlledVersionedDataWithCount
                |> ListWithTotalCount.map (AccessControlled.map VersionedData.data)
            eventBus.Broadcast eventQueue (MultipleFetched accessControlledProjectionsWithCount.Data)
            Available accessControlledVersionedDataWithCount
        | Error (Non200Code (404, _)) -> Unavailable
        | Error (Non200Code (403, _)) -> AccessDenied
        | Error (Non200Code (0,   _)) -> Failed AsyncDataFailure.NetworkFailure
        | Error (Non200Code (responseCode, response)) -> Failed (AsyncDataFailure.RequestFailure (RequestFailure.ofStatusCode (responseCode, jsonStringify(response))))
        | Error error                 -> Failed (UnknownFailure (error.ToString()))

    member private _.ProcessGetCountResponse (response: Result<uint64, RequestError>) : AsyncData<uint64> =
        match response with
        | Ok count -> Available count
        | Error (Non200Code (404, _)) -> Unavailable
        | Error (Non200Code (403, _)) -> AccessDenied
        | Error (Non200Code (0,   _)) -> Failed AsyncDataFailure.NetworkFailure
        | Error (Non200Code (responseCode, response)) -> Failed (AsyncDataFailure.RequestFailure (RequestFailure.ofStatusCode (responseCode, jsonStringify(response))))
        | Error error                 -> Failed (UnknownFailure (error.ToString()))
