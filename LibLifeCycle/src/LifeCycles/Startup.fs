[<AutoOpen>]
module LibLifeCycle.LifeCycles.Startup.LifeCycle

open LibLifeCycle
open LibLifeCycle.LifeCycles.Meta

type StartupEnvironment = {
    Clock: Service<Clock>
} with interface Env

let idGeneration (_env: StartupEnvironment) ctor =
    idgen {
        return
            match ctor with
            | StartupConstructor.New -> NonemptyString.ofStringUnsafe "$"
            |StartupConstructor.NewForSilo siloId -> siloId
            |> StartupId
    }

let construction (env: StartupEnvironment) (id: StartupId) (_constructor: StartupConstructor) =
    construction {
        let! now = env.Clock.Query Now
        return {
            Id = id
            CreatedOn = now
            LastStartupOn = now
        }
    }

let transition
    (meta: MetaLifeCycle<'Session, 'Role>)
    (lifeCycleNames: Set<string>)
    (singletonMaybeConstructs: seq<IExternalLifeCycleOperation>)
    (env: StartupEnvironment) (startup: Startup) (action: StartupAction)
    : TransitionResult<Startup, StartupAction, StartupOpError, StartupLifeEvent, StartupConstructor> =
    transition {
        match action with
        | StartupAction.PerformStartup ->
            let! now = env.Clock.Query Now

            yield!
                lifeCycleNames
                |> Seq.map(fun lifeCycleName ->
                    let metaId = MetaId lifeCycleName
                    meta.MaybeConstruct (MetaConstructor.New metaId)
                )

            yield! singletonMaybeConstructs

            return { startup with LastStartupOn = now }
    }

let private timers (startup: Startup) : list<Timer<StartupAction>> =
    [
        { TimerAction = TimerAction.DeleteSelf
          Schedule = Schedule.On (startup.LastStartupOn.AddDays 7.) }
    ]

type StartupLifeCycle<'Session, 'Role when 'Role : comparison> =
    LifeCycle<Startup, StartupAction, StartupOpError, StartupConstructor, StartupLifeEvent, StartupIndex, StartupId, AccessPredicateInput, 'Session, 'Role, StartupEnvironment>

[<Literal>]
let private startupLifeCycleName = "_Startup"

let internal startupLifeCycle<'Session, 'Role when 'Role : comparison>
    (meta: MetaLifeCycle<'Session, 'Role>)
    (ecosystemDef: EcosystemDef)
    (singletonMaybeConstructs: seq<IExternalLifeCycleOperation>)
    : StartupLifeCycle<'Session, 'Role> =

    let startupLifeCycleKey = LifeCycleKey (startupLifeCycleName, ecosystemDef.Name)
    let startupLifeCycleDef: LifeCycleDef<Startup, StartupAction, StartupOpError, StartupConstructor, StartupLifeEvent, StartupIndex, StartupId>
        = { Key = startupLifeCycleKey; ProjectionDefs = KeyedSet.empty }
    initLifeCycleTypesCodecs None startupLifeCycleDef

    let allLifeCycleNames =
        startupLifeCycleKey :: (ecosystemDef.LifeCycleDefs |> List.map (fun def -> def.LifeCycleKey))
        |> List.map (
            function
            | LifeCycleKey (name, _) ->
                name
            | OBSOLETE_LocalLifeCycleKey _ ->
                failwith "unexpected obsolete local LC key")
        |> Set.ofList

    LifeCycleBuilder.newLifeCycle startupLifeCycleDef
    |> LifeCycleBuilder.withApiAccessRestrictedToRootOnly
    |> LifeCycleBuilder.withTransition (transition meta allLifeCycleNames singletonMaybeConstructs)
    |> LifeCycleBuilder.withIdGeneration idGeneration
    |> LifeCycleBuilder.withConstruction construction
    |> LifeCycleBuilder.withTimers timers
    |> LifeCycleBuilder.withStorage
           (StorageType.Persistent
                (PromotedIndicesConfig.Empty,
                System.TimeSpan.FromDays 365.
                // ideally want AfterSubjectDeletion but can't be bothered to retrofit historical tombstones in every ecosystem in Prod
                // so let it flush for a year or so, then can switch
                |> PersistentHistoryExpiration.AfterSubjectChange |> Some
                |> PersistentHistoryRetention.Unfiltered))
    |> LifeCycleBuilder.build
