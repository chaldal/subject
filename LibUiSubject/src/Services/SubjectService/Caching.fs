namespace LibUiSubject.Services.SubjectService

open System
open LibClient
open LibUiSubject
open LibLifeCycleTypes.Api.V1

type ReasonablyFreshTTLs = {
    Subject: TimeSpan
    Query:   TimeSpan
} with
    static member NeverFreshEnough : ReasonablyFreshTTLs = {
        Subject = TimeSpan.FromSeconds 0.
        Query   = TimeSpan.FromSeconds 0.
    }

type private CacheEntry<'Value> = {
    Value:    'Value
    CachedOn: DateTimeOffset
}

type internal InMemoryCache<'Subject, 'Projection, 'Id, 'Index, 'NumericIndex, 'StringIndex, 'SearchIndex, 'GeographyIndex, 'OpError
                      when 'Subject      :> Subject<'Id>
                      and  'Projection   :> SubjectProjection<'Id>
                      and  'Projection   :  equality
                      and  'Id           :> SubjectId
                      and  'Id           :  comparison
                      and  'OpError      :> OpError
                      and  'Index        :> SubjectIndex<'Index, 'NumericIndex, 'StringIndex, 'SearchIndex, 'GeographyIndex, 'OpError>
                      and  'NumericIndex :> SubjectNumericIndex<'OpError>
                      and  'StringIndex  :> SubjectStringIndex<'OpError>
                      and  'SearchIndex  :> SubjectSearchIndex
                      and  'GeographyIndex :> SubjectGeographyIndex>(reasonablyFreshTTLs: ReasonablyFreshTTLs) =
    let mutable cacheInstance: Map<'Id, CacheEntry<'Id * AsyncData<VersionedData<'Projection>>>> = Map.empty
    let mutable cacheIndexed: Map<IndexQuery<'Index>, CacheEntry<AsyncData<seq<'Id>>>> = Map.empty
    let mutable cacheIndexedWithTotalCount: Map<IndexQuery<'Index>, CacheEntry<AsyncData<ListWithTotalCount<'Id>>>> = Map.empty
    let mutable cacheIndexedCount: Map<PreparedIndexPredicate<'Index>, CacheEntry<AsyncData<uint64>>> = Map.empty
    let mutable cacheAll: Map<ResultSetOptions<'Index>, CacheEntry<AsyncData<seq<'Id>>>> = Map.empty
    let mutable cacheAllWithTotalCount: Map<ResultSetOptions<'Index>, CacheEntry<AsyncData<ListWithTotalCount<'Id>>>> = Map.empty
    // Using a map with single unit key rather than an Option because caching helpers assume maps are being used.
    let mutable cacheAllCount: Map<unit, CacheEntry<AsyncData<uint64>>> = Map.empty

    // TODO: duplicated in SubjectService, so might be good to extract
    member private _.ConvertAccessControlledToAsyncData (accessControlled: AccessControlled<VersionedData<'Projection>, 'Id>) : 'Id * AsyncData<VersionedData<'Projection>> =
        match accessControlled with
        | AccessControlled.Granted versionedData -> (versionedData.Data.SubjectId, Available versionedData)
        | AccessControlled.Denied id             -> (id, AccessDenied)


    //
    // Invalidation
    //

    member this.InvalidateIndexCache () =
        cacheIndexed               <- Map.empty
        cacheIndexedWithTotalCount <- Map.empty

    member this.InvalidateAllCache () =
        cacheAll               <- Map.empty
        cacheAllWithTotalCount <- Map.empty
        cacheAllCount          <- Map.empty

    //
    // Read (private)
    //

    member private _.GetOneCachedIgnoreTTL<'Key, 'Value when 'Key : comparison> (cache: Map<'Key, CacheEntry<'Value>>) (key: 'Key) : Option<'Value> =
        cache.TryFind key
        |> Option.map (fun entry -> entry.Value)

    member private _.GetOneCachedIfWithinTTL<'Key, 'Value when 'Key : comparison> (cache: Map<'Key, CacheEntry<'Value>>) (ttl: TimeSpan) (key: 'Key) : Option<'Value> =
        cache.TryFind key
        |> Option.flatMap (fun entry ->
            if (DateTimeOffset.Now - entry.CachedOn) < ttl then Some entry.Value else None
        )

    member private this.GetOneCached<'Key, 'Value when 'Key : comparison> (cache: Map<'Key, CacheEntry<'Value>>) (useCache: UseCache) (key: 'Key) : Option<'Value> =
        match useCache with
        | IfNewerThan acceptableTTL -> this.GetOneCachedIfWithinTTL cache acceptableTTL               key
        | IfReasonablyFresh         -> this.GetOneCachedIfWithinTTL cache reasonablyFreshTTLs.Subject key
        | IfAvailable               -> cache.TryFind key |> Option.map (fun entry -> entry.Value)
        | No                        -> None

    member private this.GetManyCachedIgnoreTTL (ids: Set<'Id>) : Option<Subjects<'Id, VersionedData<'Projection>>> =
        let cachedSubjects =
            ids
            |> Seq.filterMap (this.GetOneCachedIgnoreTTL cacheInstance)
        match Seq.length cachedSubjects = ids.Count with
        | false -> None // just re-fetch everything instead of trying to merge
        | true  -> Some cachedSubjects

    //
    // Read (public)
    //

    member this.GetCachedVersionDataForId (useCache: UseCache) (id: 'Id) : Option<'Id * AsyncData<VersionedData<'Projection>>> =
        this.GetOneCached cacheInstance useCache id

    member this.GetCachedVersionDataForIds (useCache: UseCache) (ids: seq<'Id>) : Option<Subjects<'Id, VersionedData<'Projection>>> =
        let cachedSubjects =
            ids
            |> Seq.filterMap (this.GetCachedVersionDataForId useCache)
        match Seq.length cachedSubjects = Seq.length ids with
        | false -> None // just re-fetch everything instead of trying to merge
        | true  -> Some cachedSubjects

    member this.GetCachedVersionDataForIdIgnoreTTL (id: 'Id) : Option<'Id * AsyncData<VersionedData<'Projection>>> =
        this.GetOneCachedIgnoreTTL cacheInstance id

    member this.GetCachedIdsForResultSetOptions (useCache: UseCache) (resultSetOptions: ResultSetOptions<'Index>) : Option<AsyncData<seq<'Id>>> =
        this.GetOneCached cacheAll useCache resultSetOptions

    member this.GetCachedIdsWithTotalCountForResultSetOptions (useCache: UseCache) (resultSetOptions: ResultSetOptions<'Index>) : Option<AsyncData<ListWithTotalCount<'Id>>> =
        this.GetOneCached cacheAllWithTotalCount useCache resultSetOptions

    member this.GetCachedAllCount (useCache: UseCache) : Option<AsyncData<uint64>> =
        this.GetOneCached cacheAllCount useCache ()

    member this.GetCachedIdsForIndexQuery (useCache: UseCache) (indexQuery: IndexQuery<'Index>) : Option<AsyncData<seq<'Id>>> =
        this.GetOneCached cacheIndexed useCache indexQuery

    member this.GetCachedIdsWithTotalCountForIndexQuery (useCache: UseCache) (indexQuery: IndexQuery<'Index>) : Option<AsyncData<ListWithTotalCount<'Id>>> =
        this.GetOneCached cacheIndexedWithTotalCount useCache indexQuery

    member this.GetCachedCountForIndexPredicate (useCache: UseCache) (indexPredicate: PreparedIndexPredicate<'Index>) : Option<AsyncData<uint64>> =
        this.GetOneCached cacheIndexedCount useCache indexPredicate

    //
    // Write
    //

    member _.CacheOne (id: 'Id, versionedDataAD: AsyncData<VersionedData<'Projection>>) : bool * AsyncData<VersionedData<'Projection>> =
        match cacheInstance.TryFind id with
        | None ->
            cacheInstance <- cacheInstance.Add (id, { Value = (id, versionedDataAD); CachedOn = DateTimeOffset.Now })
            true, versionedDataAD
        | Some cacheEntry ->
            let maybeExistingVersion =
                cacheEntry.Value
                |> snd
                |> AsyncData.toOption
                |> Option.map (fun versionedData ->
                    versionedData.Version
                )
            let maybeNewVersion =
                versionedDataAD
                |> AsyncData.toOption
                |> Option.map (fun versionedData ->
                    versionedData.Version
                )
            let shouldCache =
                match maybeExistingVersion, maybeNewVersion with
                | None, Some _
                | None, None ->
                    true
                | Some x, Some y ->
                    // Only cache if the version received is newer than the current version.
                    y > x
                | Some _, None ->
                    // TODO: think this through
                    // If changing from data where we have a version to where we don't (e.g. the AD is an error), we can only
                    // assume the cache should be updated.
                    true

            if shouldCache then
                cacheInstance <- cacheInstance.AddOrUpdate (id, { Value = (id, versionedDataAD); CachedOn = DateTimeOffset.Now })
                true, versionedDataAD
            else
                false, snd cacheEntry.Value

    // NOTE by right we should be taking the id from subject.SubjectId, but we have one hacky use
    // case where the IDs are mismatched (with the Session subject), and for this reason we
    // take id as a parameter here
    member this.CacheOneAvailable (id: 'Id, versionedData: VersionedData<'Projection>) : bool * AsyncData<VersionedData<'Projection>> =
        this.CacheOne (id, Available versionedData)

    member private this.CacheOneAccessControlled (accessControlled: AccessControlled<VersionedData<'Projection>, 'Id>) : unit =
        accessControlled
        |> this.ConvertAccessControlledToAsyncData
        |> this.CacheOne
        |> ignore

    member this.CacheMany (accessControlledAD: AsyncData<seq<AccessControlled<VersionedData<'Projection>, 'Id>>>) : unit =
        match accessControlledAD with
        | Available subjects -> subjects |> Seq.iter (fun accessControlled -> this.CacheOneAccessControlled accessControlled)
        | _                  -> Noop

    member private this.AccessControlledId (value: AccessControlled<VersionedData<'Projection>, 'Id>) : 'Id =
        match value with
        | Granted versionedData -> versionedData.Data.SubjectId
        | Denied  id      -> id

    member this.CacheIndexed (query: IndexQuery<'Index>) (subjectsAD: AsyncData<seq<AccessControlled<VersionedData<'Projection>, 'Id>>>) : unit =
        this.CacheMany subjectsAD

        subjectsAD
        |> AsyncData.map (Seq.map this.AccessControlledId)
        |> AsyncData.sideEffectIfNotNetworkFailure (fun value ->
            cacheIndexed <- cacheIndexed.AddOrUpdate (query, { Value = value; CachedOn = DateTimeOffset.Now })
        )

    member this.CacheIndexedWithTotalCount (query: IndexQuery<'Index>) (subjectsWithCountAD: AsyncData<ListWithTotalCount<AccessControlled<VersionedData<'Projection>, 'Id>>>) : unit =
        subjectsWithCountAD
        |> AsyncData.map (fun subjectsWithCount -> subjectsWithCount.Data |> Seq.ofList)
        |> this.CacheMany

        subjectsWithCountAD
        |> AsyncData.map (ListWithTotalCount.map this.AccessControlledId)
        |> AsyncData.sideEffectIfNotNetworkFailure (fun value ->
            cacheIndexedWithTotalCount <- cacheIndexedWithTotalCount.AddOrUpdate (query, { Value = value; CachedOn = DateTimeOffset.Now })
        )

    member this.CacheAll (resultSetOptions: ResultSetOptions<'Index>) (subjectsAD: AsyncData<seq<AccessControlled<VersionedData<'Projection>, 'Id>>>) : unit =
        this.CacheMany subjectsAD
        subjectsAD
        |> AsyncData.map (Seq.map this.AccessControlledId)
        |> AsyncData.sideEffectIfNotNetworkFailure (fun value ->
            cacheAll <- cacheAll.AddOrUpdate (resultSetOptions, { Value = value; CachedOn = DateTimeOffset.Now })
        )

    member this.CacheAllWithTotalCount (resultSetOptions: ResultSetOptions<'Index>) (subjectsWithCountAD: AsyncData<ListWithTotalCount<AccessControlled<VersionedData<'Projection>, 'Id>>>) : unit =
        subjectsWithCountAD
        |> AsyncData.map (fun subjectsWithCount -> subjectsWithCount.Data |> Seq.ofList)
        |> this.CacheMany

        subjectsWithCountAD
        |> AsyncData.map (ListWithTotalCount.map this.AccessControlledId)
        |> AsyncData.sideEffectIfNotNetworkFailure (fun value ->
            cacheAllWithTotalCount <- cacheAllWithTotalCount.AddOrUpdate (resultSetOptions, { Value = value; CachedOn = DateTimeOffset.Now })
        )

    member _.CacheAllCount (count: AsyncData<uint64>) : unit =
        count
        |> AsyncData.sideEffectIfNotNetworkFailure (fun value ->
            cacheAllCount <- cacheAllCount.AddOrUpdate ((), { Value = value; CachedOn = DateTimeOffset.Now })
        )

    member _.CacheIndexedCount (predicate: PreparedIndexPredicate<'Index>) (count: AsyncData<uint64>) : unit =
        count
        |> AsyncData.sideEffectIfNotNetworkFailure (fun value ->
            cacheIndexedCount <- cacheIndexedCount.AddOrUpdate (predicate, { Value = value; CachedOn = DateTimeOffset.Now })
        )
