[<AutoOpen>]
module SuiteT__EC__T.LifeCycles.AllLifeCycles

open LibLifeCycle
open LibLifeCycle.Ecosystem

open SuiteT__EC__T.Types
open SuiteT__EC__T.LifeCycles

let T__ECI__TEcosystem : Ecosystem =
    EcosystemBuilder.newEcosystem T__ECI__TDef.EcosystemDef
    |> EcosystemBuilder.withNoSessionHandler
    |> EcosystemBuilder.build