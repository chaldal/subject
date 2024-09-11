[<AutoOpen>]
module SuiteT__EC__T.LifeCycles.T__LC__TView

open LibLifeCycle
open SuiteT__EC__T.Types
open LibLifeCycle.ViewAccessBuilder

type T__LC__TEnvironment = {
    Clock: Service<Clock>
} with interface Env

let private view (_env: T__LC__TEnvironment) (input: T__LC__TInput) : ViewResult<T__LC__TOutput, T__LC__TOpError> =
    view {
        let isOdd = input % 2 = 1
        return isOdd
    }

let T__LCI__TView =
    newT__EC__TView T__ECI__TDef.Views.T__LCI__T
    |> ViewBuilder.withoutApiAccess
    |> ViewBuilder.withRead view
    |> ViewBuilder.build
