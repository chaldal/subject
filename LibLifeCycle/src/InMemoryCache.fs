module LibLifeCycle.Caching

open Microsoft.Extensions.Caching.Memory
open System
open System.Threading.Tasks

// Read the following before making any changes:
// https://docs.microsoft.com/en-us/aspnet/core/performance/caching/memory?view=aspnetcore-3.1#use-setsize-size-and-sizelimit-to-limit-cache-size
type InMemoryCache =
    abstract member TryFind : 'K -> ('K -> Task<Option<'V>>) -> TimeSpan -> Task<Option<'V>> when 'K : comparison

type internal DotnetInMemoryCache() =
    let memoryCache =
        let options = MemoryCacheOptions()
        options.SizeLimit               <- 1000L
        options.ExpirationScanFrequency <- TimeSpan.FromMinutes 1.0
        new MemoryCache(options)
with
    interface InMemoryCache with
        member this.TryFind
                (key:                        'K when 'K : comparison)
                (fetch:                      'K -> Task<Option<'V>>)
                (cachedItemExpirationPeriod: TimeSpan)
                : Task<Option<'V>> =
            match memoryCache.TryGetValue key with
            | true, (:? TaskCompletionSource<Option<'V>>  as t)->
                t.Task
            | _ ->
                let taskCompletionSource = TaskCompletionSource<Option<'V>>()

                backgroundTask {
                   try
                       let! value = fetch key
                       taskCompletionSource.SetResult value
                   with
                   | ex -> taskCompletionSource.SetException ex
                }
                |> Task.fireAndForget

                let options = MemoryCacheEntryOptions()
                options.Size                            <- 1L
                options.AbsoluteExpirationRelativeToNow <- cachedItemExpirationPeriod

                memoryCache.Set(key, taskCompletionSource, options) |> ignore
                taskCompletionSource.Task


type internal NonCachingInMemoryCache() =
    interface InMemoryCache with
        member this.TryFind
                (key:                       'K when 'K : comparison)
                (fetch:                     'K -> Task<Option<'V>>)
                (_:                         TimeSpan)
                : Task<Option<'V>> =
            fetch key