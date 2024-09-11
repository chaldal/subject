module T__EC__T.``T__LC__T Tests``

open FsCheck
open FsUnit.Xunit
open SuiteT__EC__T.Types
open SuiteT__EC__T.LifeCycles

[<Simulation>]
let ``Check view output``() =
    simulation {
        let! input = Gen.choose(1, 100)
        let! output =
            Ecosystem.readView T__LCI__TView input
            |> Ecosystem.thenAssertOk
            |> Ecosystem.thenAssert (fun o ->
                let expected = input % 2 = 1
                o = expected
            )

        return output
    }
