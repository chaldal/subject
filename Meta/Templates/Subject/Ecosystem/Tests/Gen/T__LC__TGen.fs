[<AutoOpen>]
module T__LC__TGen

open FsCheck
open LibLifeCycleTest
open SuiteT__EC__T.Types

let genT__LC__TName : Gen<NonemptyString> =
  genUniqueLipsumWordCapitalized
  |> Gen.map NonemptyString.ofStringUnsafe

let genT__LC__TValue : Gen<int> =
  gen {
    let! value = Gen.choose(5, 15)
    return value * 2
  }
