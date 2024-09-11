module T__EC__T.``T__LC__T Tests``

open FsUnit.Xunit
open SuiteT__EC__T.Types
open SuiteT__EC__T.LifeCycles

[<Simulation>]
let ``Create a profile``() =
    simulation {
        let! name  = genT__LC__TName
        let! value = genT__LC__TValue

        let! T__LCI__T =
            T__LC__TConstructor.New(name, value)
            |> Ecosystem.construct T__LCI__TLifeCycle
            |> Ecosystem.thenAssertOk

        T__LCI__T.Name
        |> should equal name

        return T__LCI__T
    }
