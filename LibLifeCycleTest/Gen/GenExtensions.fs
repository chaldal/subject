module FsCheck.Gen

let filterMap (predicate: 'T -> Option<'U>) (gen: Gen<'T>) : Gen<'U> =
    gen
    |> Gen.map predicate
    |> Gen.where Option.isSome
    |> Gen.map Option.get

let unfold (size: int) (gen: Gen<'T>) : seq<'T> =
    Seq.unfold (
        fun stdGen ->
            let next = Gen.eval size stdGen gen
            Some (next, (Random.stdNext stdGen |> snd))
    ) (Random.newSeed())

let bind (binder: 'T -> Gen<'U>) (gen: Gen<'T>) : Gen<'U> =
    gen >>= binder

let ofOptionWithNoneProbability (p: double) (source: Gen<'T>) : Gen<Option<'T>> = gen {
    let random = System.Random()
    if random.NextDouble () < p then
        return None
    else
        let! sourceValue = source
        return Some sourceValue
}

let asOption (underlyingGen: Gen<'T>) : Gen<Option<'T>> =
    gen {
        match! Gen.elements [true; false] with
        | true ->
            let! v = underlyingGen
            return Some v
        | false ->
            return None
    }

let tryElements (collection: seq<'T>) : Option<Gen<'T>> =
    let arr = Seq.toArray collection
    if arr.Length = 0 then
        None
    else
        Gen.elements arr
        |> Some