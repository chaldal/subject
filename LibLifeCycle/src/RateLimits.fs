[<AutoOpen>]
module LibLifeCycle.RateLimits

open System

[<RequireQualifiedAccess>]
type RateLimitEvent<'LifeAction, 'Constructor
        when 'LifeAction :> LifeAction
        and  'Constructor :> Constructor> =
| Construct of 'Constructor
| Act       of 'LifeAction

[<RequireQualifiedAccess>]
type RateLimitScope =
| UserIp
| UserSessionOrIp
| Value of string

[<RequireQualifiedAccess>]
type RateLimitKey =
| Scoped of Set<RateLimitScope>
| Global of Set<RateLimitScope>

type RateLimit = {
    Key: RateLimitKey
    Limit: PositiveInteger
    Duration: TimeSpan
}

type LifeCycleRateLimitPredicate<'LifeAction, 'Constructor
                when 'LifeAction           :> LifeAction
                and  'Constructor          :> Constructor> =
    RateLimitEvent<'LifeAction, 'Constructor> -> Option<list<RateLimit>>