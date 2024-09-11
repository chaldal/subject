[<AutoOpen>]
module SetExtensions

module Set = Microsoft.FSharp.Collections.Set
module Set =
    // NOTE it is strange that this function is missing on Set, since it's present
    // in all other collections. I tried to figure out the reason why that may be,
    // but didn't find one.
    let tryFind<'T when 'T: comparison> (predicate: 'T -> bool) (source: Set<'T>) : Option<'T> =
        source
        |> Seq.tryFind predicate

    let findMap<'T, 'U when 'T: comparison> (predicateMap: 'T -> Option<'U>) (source: Set<'T>) : Option<'U> =
        source
        |> Seq.findMap predicateMap

    let filterMap<'T, 'U when 'T: comparison and 'U: comparison> (chooser: 'T -> Option<'U>) (source: Set<'T>) : Set<'U> =
        source
        |> Seq.filterMap chooser
        |> Set.ofSeq

    let ofOneItem (item: 'T) : Set<'T> =
        Set.ofList [item]

    let isNonEmpty (source: Set<'T>) : bool =
        not source.IsEmpty

    let notContains (value: 'T) (source: Set<'T>) : bool =
        Set.contains value source |> not

type Microsoft.FSharp.Collections.Set<'V when 'V : comparison> with
    member this.Add (values: Set<'V>) : Set<'V> =
        Set.union this values

    member this.Toggle (value: 'V) : Set<'V> =
        match this.Contains value with
        | true  -> this.Remove value
        | false -> this.Add value

    member this.SetMembership (value: 'V) (shouldBeMember: bool) : Set<'V> =
        match (shouldBeMember, this.Contains value) with
        | (true, false) -> this.Add value
        | (false, true) -> this.Remove value
        | _ -> this

    member this.ContainsAll (values: Set<'V>): bool =
        Set.isSubset values this

    member this.DoesNotContain (value: 'V): bool =
        Set.contains value this |> not

    member this.Remove (values: Set<'V>): Set<'V> =
        Set.difference this values

    member this.IsNonempty : bool =
        not this.IsEmpty
