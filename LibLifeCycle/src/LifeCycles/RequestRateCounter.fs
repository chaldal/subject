[<AutoOpen>]
module LibLifeCycle.LifeCycles.RequestRateCounter.LifeCycle

open LibLifeCycle

type RequestRateCounterEnvironment = {
    Clock: Service<Clock>
} with interface Env

let idGeneration (_env: RequestRateCounterEnvironment) ctor =
    idgen {
        return
            match ctor with
            | RequestRateCounterConstructor.New (key, _, _) -> key
    }

let construction (env: RequestRateCounterEnvironment) (id: RequestRateCounterId) (constructor: RequestRateCounterConstructor) =
    construction {
        let! now = env.Clock.Query Now

        match constructor with
        | RequestRateCounterConstructor.New (_, expiry, limit) ->
            return {
                Id = id
                CreatedOn = now
                Expiry = expiry
                Counter = 0u
                Limit = limit
            }
    }

let transition
    (_env: RequestRateCounterEnvironment) (requestRateCounter: RequestRateCounter) (action: RequestRateCounterAction)
    : TransitionResult<RequestRateCounter, RequestRateCounterAction, RequestRateCounterOpError, RequestRateCounterLifeEvent, RequestRateCounterConstructor> =
    transition {
        match action with
        | RequestRateCounterAction.Increment delta ->
            if (int requestRateCounter.Counter) + delta.Value <= requestRateCounter.Limit.Value then
                return { requestRateCounter with Counter = requestRateCounter.Counter + (uint32 delta.Value) }
            else
                return RequestRateCounterOpError.LimitExceeded
    }

let private timers (requestRateCounter: RequestRateCounter) : list<Timer<RequestRateCounterAction>> =
    [
        { TimerAction = TimerAction.DeleteSelf
          Schedule = Schedule.On (requestRateCounter.CreatedOn.Add requestRateCounter.Expiry) }
    ]

let private shouldSendTelemetry =
    function
    | ShouldSendTelemetryFor.LifeAction (RequestRateCounterAction.Increment _)
    | ShouldSendTelemetryFor.Constructor (RequestRateCounterConstructor.New _)
    | ShouldSendTelemetryFor.LifeEvent _ ->
        false

type RequestRateCounterLifeCycle<'Session, 'Role when 'Role : comparison> =
    LifeCycle<RequestRateCounter, RequestRateCounterAction, RequestRateCounterOpError, RequestRateCounterConstructor, RequestRateCounterLifeEvent, RequestRateCounterIndex, RequestRateCounterId, AccessPredicateInput, 'Session, 'Role, RequestRateCounterEnvironment>

[<Literal>]
let private requestRateCounterLifeCycleName = "_RequestRateCounter"

let internal requestRateCounterLifeCycle<'Session, 'Role when 'Role : comparison> (ecosystemDef: EcosystemDef)
    : RequestRateCounterLifeCycle<'Session, 'Role> =

    let requestRateCounterLifeCycleKey = LifeCycleKey (requestRateCounterLifeCycleName, ecosystemDef.Name)
    let requestRateCounterLifeCycleDef: LifeCycleDef<RequestRateCounter, RequestRateCounterAction, RequestRateCounterOpError, RequestRateCounterConstructor, RequestRateCounterLifeEvent, RequestRateCounterIndex, RequestRateCounterId>
        = { Key = requestRateCounterLifeCycleKey; ProjectionDefs = KeyedSet.empty }
    initLifeCycleTypesCodecs None requestRateCounterLifeCycleDef

    LifeCycleBuilder.newLifeCycle requestRateCounterLifeCycleDef
    |> LifeCycleBuilder.withoutApiAccess
    |> LifeCycleBuilder.withStorage StorageType.Volatile
    |> LifeCycleBuilder.withTransition transition
    |> LifeCycleBuilder.withIdGeneration idGeneration
    |> LifeCycleBuilder.withConstruction construction
    |> LifeCycleBuilder.withTimers timers
    |> LifeCycleBuilder.withTelemetryRules shouldSendTelemetry
    |> LifeCycleBuilder.build
