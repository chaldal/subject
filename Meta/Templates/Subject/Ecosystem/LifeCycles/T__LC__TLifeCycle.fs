[<AutoOpen>]
module SuiteT__EC__T.LifeCycles.T__LC__TLifeCycle

open LibLifeCycle
open SuiteT__EC__T.Types
open LibLifeCycle.LifeCycleAccessBuilder
open type AccessTo<T__LC__TAction, T__LC__TConstructor>

type T__LC__TEnvironment = {
    Clock: Service<Clock>
} with interface Env

let private transition (_env: T__LC__TEnvironment) (T__LCI__T: T__LC__T) (action: T__LC__TAction) : TransitionResult<T__LC__T, T__LC__TAction, T__LC__TOpError, T__LC__TLifeEvent, T__LC__TConstructor> =
    transition {
        match action with
        | T__LC__TAction.Add value ->
            return { T__LCI__T with Counter = T__LCI__T.Counter + value }

        | T__LC__TAction.ResetIfNonZero ->
            if T__LCI__T.Counter <> 0 then
                return { T__LCI__T with Counter = 0 }
            else
                return T__LC__TOpError.CounterIsZero
    }

let private construction (env: T__LC__TEnvironment) (_id: T__LC__TId) (ctor: T__LC__TConstructor) : ConstructionResult<T__LC__T, T__LC__TAction, T__LC__TOpError, T__LC__TLifeEvent> =
    construction {
        match ctor with
        | T__LC__TConstructor.New (name, value) ->
            let! now = env.Clock.Query Now
            return {
                Name      = name
                Counter   = value
                CreatedOn = now
            }
    }

let private idGeneration (_env: T__LC__TEnvironment) (ctor: T__LC__TConstructor) : IdGenerationResult<T__LC__TId, T__LC__TOpError> =
    idgen {
        match ctor with
        | T__LC__TConstructor.New (name, _) ->
            return T__LC__TId name
    }


let T__LCI__TLifeCycle =
    newT__EC__TLifeCycle                 T__ECI__TDef.LifeCycles.T__LCI__T
    |> LifeCycleBuilder.withoutApiAccess
    |> LifeCycleBuilder.withTransition   transition
    |> LifeCycleBuilder.withIdGeneration idGeneration
    |> LifeCycleBuilder.withConstruction construction
    |> LifeCycleBuilder.build
