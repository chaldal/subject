[<AutoOpen>]
module NonemptySetModule

type NonemptySet<'V when 'V: comparison> =
    private | NonemptySet of set: Set<'V>

    member this.ToSet : Set<'V> =
        match this with
        | NonemptySet set -> set

    member this.Count : PositiveInteger =
        this.ToSet.Count |> PositiveInteger.ofIntUnsafe

    member this.Add (value: 'V) : NonemptySet<'V> =
        this.ToSet.Add value |> NonemptySet

    member this.Add (values: Set<'V>) : NonemptySet<'V> =
        this.ToSet.Add values |> NonemptySet

    member this.Remove (value: 'V) : Option<NonemptySet<'V>> =
        let updated = this.ToSet.Remove value
        match updated.IsEmpty with
        | true  -> None
        | false -> Some (NonemptySet updated)

    member this.Remove (values: NonemptySet<'V>) : Option<NonemptySet<'V>> =
        let updated = this.ToSet.Remove values.ToSet
        match updated.IsEmpty with
        | true  -> None
        | false -> Some (NonemptySet updated)

    member this.Contains (value: 'V) : bool =
        this.ToSet.Contains value

    member this.ContainsAll (values: NonemptySet<'V>) : bool =
        this.ToSet.ContainsAll values.ToSet

module NonemptySet =
    let ofOneItem (value: 'V): NonemptySet<'V> =
        NonemptySet (Set.ofList [value])

    let ofSet (set: Set<'V>): Option<NonemptySet<'V>> =
        match set.IsEmpty with
        | true  -> None
        | false -> set |> NonemptySet |> Some

    let ofSetUnsafe (set: Set<'V>): NonemptySet<'V> =
        set |> ofSet |> Option.get

    let ofList (list: List<'V>): Option<NonemptySet<'V>> =
        match list.IsEmpty with
        | true  -> None
        | false -> list |> Set.ofList |> NonemptySet |> Some

    let ofListUnsafe (list: List<'V>): NonemptySet<'V> =
        list |> ofList |> Option.get

    let ofSeq (source: seq<'V>): Option<NonemptySet<'V>> =
        // Seqs can be consumable, and there are Fable bugs (I think) that lead to
        // certain Seqs being unexpectedly consumable, so calling Seq.isEmpty will
        // return false, but then consume the seq, so it'll be empty when you call
        // Set.ofSeq on it. So we convert up-front, and only then test for emptiness.
        Set.ofSeq source |> ofSet

    let ofSeqUnsafe (source: seq<'V>): NonemptySet<'V> =
        source |> ofSeq |> Option.get

    let toSet (nonemptySet: NonemptySet<'V>) : Set<'V> =
        match nonemptySet with
        | NonemptySet set -> set

    let length (nonemptySet: NonemptySet<'V>) =
        nonemptySet.ToSet.Count |> PositiveInteger.ofIntUnsafe

    let toSeq (nonemptySet: NonemptySet<'V>) : seq<'V> =
        nonemptySet |> toSet |> Set.toSeq

    let toNonemptyList (source: NonemptySet<'V>) : NonemptyList<'V> =
        source |> toSet |> Set.toList |> NonemptyList.ofList |> Option.get (* Okay because we started with nonempty *)

    let removeAndUpdateInMap (map: Map<'K, NonemptySet<'V>>) (key: 'K) (valueToRemove: 'V) : Map<'K, NonemptySet<'V>> =
        match map.TryFind key with
        | None -> map
        | Some currentValues ->
            match currentValues.Remove valueToRemove with
            | None                    -> map.Remove key
            | Some updatedNonemptySet -> map.AddOrUpdate (key, updatedNonemptySet)

    let removeMultipleAndUpdateInMap (map: Map<'K, NonemptySet<'V>>) (key: 'K) (valuesToRemove: NonemptySet<'V>) : Map<'K, NonemptySet<'V>> =
        match map.TryFind key with
        | None -> map
        | Some currentValues ->
            match currentValues.Remove valuesToRemove with
            | None                    -> map.Remove key
            | Some updatedNonemptySet -> map.AddOrUpdate (key, updatedNonemptySet)

    let addAll (values: seq<'V>) (a: NonemptySet<'V>) : NonemptySet<'V> =
        a.ToSet
        |> Set.toSeq
        |> Seq.append values
        |> Set.ofSeq
        |> NonemptySet

    let union (a: NonemptySet<'V>) (b: NonemptySet<'V>) : NonemptySet<'V> =
        Set.union (toSet a) (toSet b) |> NonemptySet

    let unionMany (nonemptySets: List<NonemptySet<'V>>) : NonemptySet<'V> =
        nonemptySets
        |> Seq.map toSet
        |> Set.unionMany
        |> NonemptySet

    let isSubset (a: NonemptySet<'V>) (b: NonemptySet<'V>) : bool =
        Set.isSubset (toSet a) (toSet b)

    let difference (a: NonemptySet<'V>) (b: NonemptySet<'V>) : Set<'V> =
        Set.difference (toSet a) (toSet b)

    let intersect (a: NonemptySet<'V>) (b: NonemptySet<'V>) : Set<'V> =
        Set.intersect (toSet a) (toSet b)

    let unionSet (a: Set<'V>) (b: NonemptySet<'V>) : NonemptySet<'V> =
        Set.union (toSet b) a |> NonemptySet

    let replace (a: NonemptySet<'V>) (valueToRemove: 'V ) (valueToAdd: 'V) : NonemptySet<'V> =
        (toSet a)
        |> Set.remove valueToRemove
        |> Set.add valueToAdd
        |> ofSet
        |> Option.get

    // Option of NonemptySet will naturally come up, so we make convenience functions for dealing with it
    let unionMaybe (a: NonemptySet<'V>) (maybeB: Option<NonemptySet<'V>>) : NonemptySet<'V> =
        match maybeB with
        | None   -> a
        | Some b -> union a b

    let map (mapper: 'V -> 'U) (source: NonemptySet<'V>) : NonemptySet<'U> =
        source
        |> toSet
        |> Set.map mapper
        |> NonemptySet

    let fold (folder: 'State -> 'V -> 'State) (initial: 'State) (source: NonemptySet<'V>) : 'State =
        source
        |> toSet
        |> Set.fold folder initial

    let addToMap (key: 'K) (value: 'V) (map: Map<'K, NonemptySet<'V>>) : Map<'K, NonemptySet<'V>> =
        match map.TryFind key with
        | Some values -> map.AddOrUpdate (key, values.Add value)
        | None        -> map.Add (key, ofOneItem value)

    let addMultipleToMap (key: 'K) (values: NonemptySet<'V>) (map: Map<'K, NonemptySet<'V>>) : Map<'K, NonemptySet<'V>> =
        match map.TryFind key with
        | Some existing -> map.AddOrUpdate (key, union existing values)
        | None          -> map.Add (key, values)

    let removeMultipleFromMap (key: 'K) (values: NonemptySet<'V>) (map: Map<'K, NonemptySet<'V>>) : Map<'K, NonemptySet<'V>> =
        let existing =
            map.TryFind key
            |> Option.map toSet
            |> Option.getOrElse Set.empty

        let maybeRemaining =
            existing.Remove values.ToSet
            |> ofSet

        match maybeRemaining with
        | Some remaining -> map.AddOrUpdate (key, remaining)
        | None           -> map.Remove key

    let mergeMaps (existingMap: Map<'K, NonemptySet<'V>>) (newMap: Map<'K, NonemptySet<'V>>) : Map<'K, NonemptySet<'V>> =
        newMap
        |> Map.fold
            ( fun acc key value ->
                 acc
                 |> addMultipleToMap key value
            )
            existingMap

    let filter (predicate: 'V -> bool) (source: NonemptySet<'V>) : Option<NonemptySet<'V>> =
        source.ToSet |> Set.filter predicate |> ofSet

    let ofNonemptyList (nonemptyList: NonemptyList<'V>) : NonemptySet<'V> =
        nonemptyList.ToList
        |> ofListUnsafe

#if !FABLE_COMPILER

    open CodecLib
    let codec codec : Codec<_, NonemptySet<'t>> =
        Codec.create (ofSet >> Result.ofOption (Uncategorized "Set is empty")) toSet
        |> Codec.compose (Codecs.set codec)

open CodecLib

type NonemptySet<'V2 when 'V2: comparison> with
    static member inline get_Codec () : Codec<_, NonemptySet<'t>> =
        Codec.create (NonemptySet.ofSet >> Result.ofOption (Uncategorized "Set is empty")) NonemptySet.toSet
        |> Codec.compose (Codecs.set defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 't>)

#endif

[<AutoOpen>]
module NonEmptySetBuilder =
    /// Creates a NonemptyList
    type NonemptySetBuilder () =
        [<CompilerMessage("A NonEmptySet doesn't support the Zero operation.", 10708, IsError = true)>]
        member _.Zero () = failwith "Unreachable code"
        member _.Combine (a: 'T, b: Set<'T>) = Set.add a b
        member _.Yield x = x
        member _.Delay expr = expr ()
        member _.Run (x: Set<_>) = Option.get (NonemptySet.ofSet x)
    let nset = NonemptySetBuilder ()

[<AutoOpen>]
module NonEmptySetBuilderExtensions =
    type NonemptySetBuilder with
        member _.Combine (a: 'T, b: 'T) = Set.empty |> Set.add a |> Set.add b
        member _.Run x = NonemptySet.ofOneItem x
