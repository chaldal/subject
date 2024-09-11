namespace LibUiSubject.Services.ViewService

open System
open LibClient
open LibUiSubject.Types

type private CacheEntry<'Value> = {
    Value:    'Value
    CachedOn: DateTimeOffset
}

type internal InMemoryCache<'Input, 'Output when 'Input : comparison>(reasonablyFreshTTL: TimeSpan) =
    let mutable cacheInstance: Map<'Input, CacheEntry<'Input * AsyncData<'Output>>> = Map.empty

    //
    // Read (private)
    //

    member private _.GetOneCachedIfWithinTTL<'Key, 'Value when 'Key : comparison> (cache: Map<'Key, CacheEntry<'Value>>) (ttl: TimeSpan) (key: 'Key) : Option<'Value> =
        cache.TryFind key
        |> Option.flatMap (fun entry ->
            if (DateTimeOffset.Now - entry.CachedOn) < ttl then Some entry.Value else None
        )

    member private this.GetOneCached<'Key, 'Value when 'Key : comparison> (cache: Map<'Key, CacheEntry<'Value>>) (useCache: UseCache) (key: 'Key) : Option<'Value> =
        match useCache with
        | IfNewerThan acceptableTTL -> this.GetOneCachedIfWithinTTL cache acceptableTTL               key
        | IfReasonablyFresh         -> this.GetOneCachedIfWithinTTL cache reasonablyFreshTTL key
        | IfAvailable               -> cache.TryFind key |> Option.map (fun entry -> entry.Value)
        | No                        -> None

    //
    // Read (public)
    //

    member this.GetCachedOutputForInput (useCache: UseCache) (input: 'Input) : Option<'Input * AsyncData<'Output>> =
        this.GetOneCached cacheInstance useCache input

    //
    // Write
    //

    member _.CacheOne (input: 'Input) (outputAD: AsyncData<'Output>) : unit =
        outputAD
        |> AsyncData.sideEffectIfNotNetworkFailure (fun _value ->
            cacheInstance <- cacheInstance.AddOrUpdate (input, { Value = (input, outputAD); CachedOn = DateTimeOffset.Now })
        )

    member this.CacheOneAvailable (input: 'Input) (output: 'Output) : unit =
        this.CacheOne input (Available output)
