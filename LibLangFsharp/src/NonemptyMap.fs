[<AutoOpen>]
module NonemptyMap

type NonemptyMap<'K, 'V when 'K: comparison> =
    private | NonemptyMap of map: Map<'K, 'V>

    member this.ToMap : Map<'K, 'V> =
        match this with
        | NonemptyMap map -> map

    member this.AddOrUpdate (key: 'K, value: 'V) : NonemptyMap<'K, 'V> =
        this.ToMap.AddOrUpdate(key, value) |> NonemptyMap

    /// Similar to AddOrUpdate but returns Error if key already exists with a different value.
    /// Useful for idempotent action retries.
    member this.AddOrIgnoreSameValue (key: 'K, value: 'V) : Result<NonemptyMap<'K, 'V>, 'V> =
        this.ToMap.AddOrIgnoreSameValue(key, value)
        |> Result.map NonemptyMap

    member this.Remove (key: 'K) : Option<NonemptyMap<'K, 'V>> =
        let updated = this.ToMap.Remove key
        match updated.IsEmpty with
        | true  -> None
        | false -> Some (NonemptyMap updated)

    member this.TryFind (key: 'K) : Option<'V> =
        this.ToMap.TryFind key

    member this.FindOrElse (defaultValue: 'V) (key: 'K) : 'V =
        this.ToMap.FindOrElse defaultValue key

    member this.Keys : NonemptySet<'K> =
        (NonemptySet.ofSet this.ToMap.KeySet)
        |> Option.get // Ok because we know our internal map is nonempty

    member this.Values : seq<'V> =
        this.ToMap.Values

    member this.Count : PositiveInteger =
        this.ToMap.Count |> PositiveInteger.ofIntUnsafe

    member this.ContainsKey (key: 'K) : bool =
        this.ToMap.ContainsKey key

let toMap (nonemptyMap: NonemptyMap<'K, 'V>) : Map<'K, 'V> =
    match nonemptyMap with
    | NonemptyMap map -> map

let filterKeys (predicate: 'K -> bool) (source: NonemptyMap<'K, 'V>) : Map<'K, 'V> =
    toMap source |> Map.filter (fun key _ -> predicate key)

let filter (predicate: 'K -> 'V -> bool) (source: NonemptyMap<'K, 'V>) : Map<'K, 'V> =
    toMap source |> Map.filter (fun key value -> predicate key value)

let ofMap (map: Map<'K, 'V>) : Option<NonemptyMap<'K, 'V>> =
    match map.IsEmpty with
    | true  -> None
    | false -> Some (NonemptyMap map)

let ofSeq (source: seq<'K * 'V>) : Option<NonemptyMap<'K, 'V>> =
    source
    |> Map.ofSeq
    |> ofMap

let ofSeqUnsafe (source: seq<'K * 'V>) : NonemptyMap<'K, 'V> =
    match source |> Seq.isEmpty with
    | true ->
        failwith "NonemptyMap.ofSeqUnsafe : Cannot create NonemptyMap from an empty Seq"
    | false ->
        source
        |> Map.ofSeq
        |> ofMap
        |> Option.get

let ofNonemptyList (source: NonemptyList<'K * 'V>) : NonemptyMap<'K, 'V> =
    source.ToList
    |> Map.ofList
    |> ofMap
    |> Option.get // safe because started with NonemptyList

let ofOneItem (key: 'K, value: 'V) : NonemptyMap<'K, 'V> =
    Map.empty.Add(key, value)
    |> NonemptyMap

let ofNonEmptySet (set: NonemptySet<'K>) (valueFactory: 'K -> 'V) : NonemptyMap<'K, 'V> =
    set.ToSet
    |> Set.toSeq
    |> Seq.map (fun k -> k, (valueFactory k))
    |> Map.ofSeq
    |> NonemptyMap

let addNonEmptySet (set: NonemptySet<'K>) (valueFactory: 'K -> 'V) (source: NonemptyMap<'K, 'V>) : NonemptyMap<'K, 'V> =
    toMap source
    |> Map.toSeq
    |> Seq.append(
        set.ToSet
        |> Set.toSeq
        |> Seq.map (fun k -> k, (valueFactory k))
    )
    |> Map.ofSeq
    |> NonemptyMap

let toSeq (source: NonemptyMap<'K, 'V>) : seq<'K * 'V> =
    source
    |> toMap
    |> Map.toSeq

let toNonEmptyList (source: NonemptyMap<'K, 'V>) : NonemptyList<'K * 'V> =
    source
    |> toMap
    |> Map.toList
    |> NonemptyList.ofList
    |> Option.get

let count (source: NonemptyMap<'K, 'V>) : PositiveInteger =
    source.ToMap.Count |> PositiveInteger.ofIntUnsafe

let map (mapper: 'K -> 'V -> 'U) (source: NonemptyMap<'K, 'V>) : NonemptyMap<'K, 'U> =
    source.ToMap
    |> Map.map mapper
    |> NonemptyMap

let mapKeys (mapper: 'K1 -> 'K2) (source: NonemptyMap<'K1, 'V>) : NonemptyMap<'K2, 'V> =
    source.ToMap
    |> Map.mapKeys mapper
    |> NonemptyMap

let mapValues (mapper: 'V1 -> 'V2) (source: NonemptyMap<'K, 'V1>) : NonemptyMap<'K, 'V2> =
    source.ToMap
    |> Map.mapValues mapper
    |> NonemptyMap

let fold (folder: 'State -> 'K -> 'V -> 'State) (seed: 'State) (source: NonemptyMap<'K, 'V>) : 'State =
    source.ToMap
    |> Map.fold folder seed

let tryFindKey (predicate: 'K -> 'V -> bool) (source: NonemptyMap<'K, 'V>) : Option<'K> =
    Map.tryFindKey predicate (toMap source)

let tryFind (key: 'K) (source: NonemptyMap<'K, 'V>) : Option<'V> =
    source.TryFind key

let find (key: 'K) (source: NonemptyMap<'K, 'V>) : 'V =
    Map.find key (toMap source)

let updateValue (key: 'K) (updater: 'V -> 'V) (source: NonemptyMap<'K, 'V>) : NonemptyMap<'K, 'V> =
    match source.TryFind key with
    | None       -> source
    | Some value -> source.AddOrUpdate(key, updater value)

let mergeMany (maps: NonemptyList<NonemptyMap<'K, 'V>>) : NonemptyMap<'K, 'V> =
    maps.ToList
    |> Seq.map toMap
    |> Map.mergeMany
    |> ofMap
    |> Option.get // safe since started with at least one NonemptyMap

let mergeInto (target: NonemptyMap<'K, 'V1>) (merger: 'K -> Option<'V1> -> 'V2 -> 'V1) (source: NonemptyMap<'K, 'V2>) : NonemptyMap<'K, 'V1> =
    source.ToMap
    |> Map.fold
        (fun (acc: Map<'K, 'V1>) key (value: 'V2) ->
            let maybeExisting = acc.TryFind key
            acc.Add(key, (merger key maybeExisting value))
        )
        target.ToMap
    |> NonemptyMap

let multiPartition (keyer: 'K -> 'V -> 'PartitionKey) (source: Map<'K, 'V>) : Map<'PartitionKey, NonemptyMap<'K, 'V>> =
    source
    |> Map.fold
        (fun (acc: Map<'PartitionKey, NonemptyMap<'K, 'V>>) key value ->
            let partitionKey = keyer key value
            let updatedPartition =
                match acc.TryFind partitionKey with
                | Some partition -> partition.AddOrUpdate(key, value)
                | None           -> ofOneItem (key, value)

            acc.AddOrUpdate(partitionKey, updatedPartition)
        )
        Map.empty

let multiPartitionNonempty (keyer: 'K -> 'V -> 'PartitionKey) (source: NonemptyMap<'K, 'V>) : NonemptyMap<'PartitionKey, NonemptyMap<'K, 'V>> =
    source
    |> toMap
    |> multiPartition keyer
    |> NonemptyMap

let addOrUpdateOrCreate (maybeSource: Option<NonemptyMap<'K, 'V>>) (key: 'K, value: 'V) : NonemptyMap<'K, 'V> =
    match maybeSource with
    | None        -> ofOneItem(key, value)
    | Some source -> source.AddOrUpdate(key, value)

let updateValueOrCreate (key: 'K) (updater: Option<'V> -> 'V) (maybeSource: Option<NonemptyMap<'K, 'V>>) : NonemptyMap<'K, 'V> =
    match maybeSource with
    | None        -> ofOneItem(key, updater None)
    | Some source -> source |> updateValue key (fun value -> updater (Some value))

[<RequireQualifiedAccess>]
type NonemptyMapUpdateOrRemoveResult<'K, 'V> =
| NoAction
| Update  of 'V
| Replace of 'K * 'V
| UpdateAndAdd of ('V) * ('K * 'V)

let updateOrReplace
        (key: 'K)
        (handleUpdateOrReplace: 'V -> NonemptyMapUpdateOrRemoveResult<'K, 'V>)
        (conflictResolver: 'K -> 'V -> 'V -> 'V)
        (source: NonemptyMap<'K, 'V>) =
    let map = source.ToMap
    match map.TryFind key with
    | None -> source
    | Some v ->
        match handleUpdateOrReplace v with
        | NonemptyMapUpdateOrRemoveResult.NoAction ->
            source
        | NonemptyMapUpdateOrRemoveResult.Update newValue ->
            NonemptyMap (map.AddOrUpdate(key, newValue))
        | NonemptyMapUpdateOrRemoveResult.Replace (newKey, newValue) ->
            let removedMap = map.Remove(newKey)
            match removedMap.TryFind newKey with
            | None   -> removedMap.Add(newKey, newValue)
            | Some existing ->
                let resolved = conflictResolver newKey existing newValue
                removedMap.Add(newKey, resolved)
            |> NonemptyMap
        | NonemptyMapUpdateOrRemoveResult.UpdateAndAdd (updatedValue, (newKey, newValue)) ->
            let addedMap = map.Add(key, updatedValue)
            match addedMap.TryFind newKey with
            | None   -> addedMap.Add(newKey, newValue)
            | Some existing ->
                let resolved = conflictResolver newKey existing newValue
                addedMap.Add(newKey, resolved)
            |> NonemptyMap

// I want these in NonemptySet, but because of circular references dropping it here for now,
// don't want to lose momentum dealing with module declaration nonsense
let removeAndUpdateInNonemptyMap (map: NonemptyMap<'K, NonemptySet<'V>>) (key: 'K) (nonemptySet: NonemptySet<'V>) (valueToRemove: 'V) : Option<NonemptyMap<'K, NonemptySet<'V>>> =
    match nonemptySet.Remove valueToRemove with
    | None                    -> map.Remove key
    | Some updatedNonemtpySet -> map.AddOrUpdate (key, updatedNonemtpySet) |> Some

let removeMultipleAndUpdateInNonemptyMap (map: NonemptyMap<'K, NonemptySet<'V>>) (key: 'K) (nonemptySet: NonemptySet<'V>) (valuesToRemove: NonemptySet<'V>) : Option<NonemptyMap<'K, NonemptySet<'V>>> =
    match nonemptySet.Remove valuesToRemove with
    | None                    -> map.Remove key
    | Some updatedNonemtpySet -> map.AddOrUpdate (key, updatedNonemtpySet) |> Some


#if !FABLE_COMPILER

open CodecLib

let codec keyCodec valueCodec : Codec<_,NonemptyMap<'k, 'v>> =
    Codec.create (Seq.ofList >> ofSeq >> Result.ofOption (Uncategorized "Map is empty")) (toSeq >> Seq.toList)
    |> Codec.compose (Codecs.list (Codecs.tuple2 keyCodec valueCodec))

type NonemptyMap<'K, 'V when 'K: comparison> with
    static member inline get_Codec () : Codec<_,NonemptyMap<'k, 'v>> =
        Codec.create (Seq.ofList >> ofSeq >> Result.ofOption (Uncategorized "Map is empty")) (toSeq >> Seq.toList)
        |> Codec.compose (Codecs.list defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'k * 'v>)

#endif
