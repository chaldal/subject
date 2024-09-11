[<AutoOpen>]
module LibLifeCycleTest.Random

open FsCheck

// TODO, clean up randoms and Gens
let mutable private stdGen = ref (Random.newSeed())
let private defaultSize = 5

let evalGen (gen: Gen<'T>) =
    lock stdGen (fun _ ->
        let current = stdGen.Value
        stdGen.Value <- Random.stdNext current |> snd
        current
    )
    |> fun stdGen ->
        Gen.eval defaultSize stdGen gen