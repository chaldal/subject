[<AutoOpen>]
module NonemptyKeyedSet

type NonemptyKeyedSet<'K, 'V when 'K: comparison and 'V :> IKeyed<'K>> =
    private | MapOfKeyToNonemptyKeyedSet of NonemptyMap<'K, 'V>

    with
        member private this.Map : NonemptyMap<'K, 'V> =
            match this with
            | MapOfKeyToNonemptyKeyedSet map -> map

        member this.GetByKey (key: 'K) : Option<'V> =
            this.Map.TryFind key

        member this.AddOrUpdate (value: 'V) : NonemptyKeyedSet<'K, 'V> =
            this.Map.AddOrUpdate(value.Key, value)
            |> MapOfKeyToNonemptyKeyedSet

        member this.ContainsKey (key: 'K) : bool =
            this.Map.ContainsKey key

        member this.RemoveByKey (key: 'K) : Option<NonemptyKeyedSet<'K, 'V>> =
            this.Map.Remove key
            |> Option.map MapOfKeyToNonemptyKeyedSet

        member this.Remove (value: 'V) : Option<NonemptyKeyedSet<'K, 'V>> =
            this.Map.Remove value.Key |> Option.map MapOfKeyToNonemptyKeyedSet

        member this.RemoveManyByKey (keysToRemove: NonemptySet<'K>) : Option<NonemptyKeyedSet<'K, 'V>> =
            keysToRemove
            |> NonemptySet.fold
                (fun maybeMap keyToRemove ->
                    maybeMap
                    |> Option.bind(fun (map:NonemptyMap<'K, 'V>) -> map.Remove(keyToRemove)))
                (Some this.Map)
            |> Option.map MapOfKeyToNonemptyKeyedSet

        member this.Count : PositiveInteger =
            this.Map.Count

        member this.Keys : NonemptySet<'K> =
            this.Map.Keys

        member this.Values : seq<'V> =
            this.Map.Values

let private getMap (nonemptyKeyedSet: NonemptyKeyedSet<'K, 'V>) : Map<'K, 'V> =
    match nonemptyKeyedSet with
    | MapOfKeyToNonemptyKeyedSet map -> map.ToMap

let tryFindKey<'K, 'V when 'K: comparison and 'V :> IKeyed<'K>> (predicate: 'V -> bool) (source: NonemptyKeyedSet<'K, 'V>) : Option<'K> =
    source
    |> getMap
    |> Map.tryFindKey (fun _ value -> predicate value)

let tryFind<'K, 'V when 'K: comparison and 'V :> IKeyed<'K>> (predicate: 'V -> bool) (source: NonemptyKeyedSet<'K, 'V>) : Option<'V> =
    match tryFindKey predicate source with
    | Some key -> source.GetByKey key
    | None     -> None

let ofSeq<'K, 'V when 'K: comparison and 'V :> IKeyed<'K>> (source: seq<'V>) : Option<NonemptyKeyedSet<'K, 'V>> =
    source
    |> Seq.map (fun v -> (v.Key, v))
    |> NonemptyMap.ofSeq
    |> Option.map MapOfKeyToNonemptyKeyedSet

let ofList<'K, 'V when 'K: comparison and 'V :> IKeyed<'K>> (list: List<'V>) : Option<NonemptyKeyedSet<'K, 'V>> =
    list
    |> List.toSeq
    |> ofSeq

let ofNonemptyList<'K, 'V when 'K: comparison and 'V :> IKeyed<'K>> (list: NonemptyList<'V>) : NonemptyKeyedSet<'K, 'V> =
    list.ToList
    |> List.toSeq
    |> ofSeq
    |> Option.get

let ofKeyedSet (keyedSet: KeyedSet<'K, 'V>) : Option<NonemptyKeyedSet<'K, 'V>> =
    keyedSet.Map
    |> NonemptyMap.ofMap
    |> Option.map MapOfKeyToNonemptyKeyedSet

let ofOneItem<'K, 'V when 'K: comparison and 'V :> IKeyed<'K>> (item: 'V) : NonemptyKeyedSet<'K, 'V> =
    (item.Key, item)
    |> NonemptyMap.ofOneItem
    |> MapOfKeyToNonemptyKeyedSet

let exists<'K, 'V when 'K: comparison and 'V :> IKeyed<'K>> (predicate: 'V -> bool) (source: NonemptyKeyedSet<'K, 'V>) : bool =
    source
    |> getMap
    |> Map.exists (fun _ value -> predicate value)

let fold<'State, 'K, 'V when 'K: comparison and 'V :> IKeyed<'K>> (folder: 'State -> 'V -> 'State) (state: 'State) (source: NonemptyKeyedSet<'K, 'V>) : 'State =
    Map.fold (fun state _ value -> folder state value) state (source |> getMap)

let toSeq (source: NonemptyKeyedSet<'K, 'V>) : seq<'V> =
    source
    |> getMap
    |> Map.values

let map (mapper: 'T -> 'U) (source: NonemptyKeyedSet<'K, 'T>) : NonemptyKeyedSet<'K, 'U> =
    source
    |> toSeq
    |> Seq.map mapper
    |> ofSeq
    |> Option.get

let head<'K, 'V when 'K: comparison and 'V :> IKeyed<'K>> (source: NonemptyKeyedSet<'K, 'V>) : 'V =
    source
    |> toSeq
    |> Seq.head // Guaranteed to succeed

let toKeyedSet (source: NonemptyKeyedSet<'K, 'V>) : KeyedSet<'K, 'V> =
    source |> toSeq |> KeyedSet.ofSeq

let toNonemptySet (source: NonemptyKeyedSet<'K, 'V>) : NonemptySet<'V> =
    source
    |> getMap
    |> Map.values
    |> NonemptySet.ofSeq
    |> Option.get // safe because started with nonempty

let keys (source: NonemptyKeyedSet<'K, 'V>) : NonemptySet<'K> =
    source
    |> getMap
    |> Map.keys
    |> NonemptySet.ofSet
    |> Option.get // safe because started with nonempty

let toNonemptyList (source: NonemptyKeyedSet<'K, 'V>) : NonemptyList<'V> =
    source |> toSeq |> NonemptyList.ofSeq |> Option.get // Safe because source is non-empty

let mergeWith (target: NonemptyKeyedSet<'K, 'V>) (source: NonemptyKeyedSet<'K, 'V>) : NonemptyKeyedSet<'K, 'V> =
    let (MapOfKeyToNonemptyKeyedSet mapSource) = source
    let (MapOfKeyToNonemptyKeyedSet mapTarget) = target
    NonemptyMap.mergeInto mapTarget (fun _ maybeFromTarget fromSource -> Option.defaultValue fromSource maybeFromTarget) mapSource
    |> MapOfKeyToNonemptyKeyedSet

let isSubsetByKeys (source: NonemptyKeyedSet<'K, 'V>) (target: NonemptyKeyedSet<'K, 'V>) : bool =
    NonemptySet.isSubset source.Keys target.Keys

let take (count: PositiveInteger) (source: NonemptyKeyedSet<'K, 'V>) : NonemptyKeyedSet<'K, 'V> =
    source.Values
    |> Seq.truncate count.Value
    |> ofSeq
    |> Option.get // Safe since count is a positive integer so we should have at least 1 value

let count (source: NonemptyKeyedSet<'K, 'V>) : PositiveInteger =
    source.Count

let addMultipleToMap
    (key:   'K)
    (values: NonemptyKeyedSet<'K2, 'V>)
    (map:    Map<'K, NonemptyKeyedSet<'K2, 'V>>)
    : Map<'K, NonemptyKeyedSet<'K2, 'V>> =
    match map.TryFind key with
    | Some existing -> map.AddOrUpdate (key, mergeWith existing values)
    | None          -> map.Add (key, values)

let mergeMaps
    (existingMap: Map<'K, NonemptyKeyedSet<'K2, 'V>>)
    (newMap:      Map<'K, NonemptyKeyedSet<'K2, 'V>>)
    : Map<'K, NonemptyKeyedSet<'K2, 'V>> =
    newMap
    |> Map.fold
        ( fun acc key value ->
             acc
             |> addMultipleToMap key value
        )
        existingMap

[<RequireQualifiedAccess>]
type NonemptyKeyedSetUpdateOrRemoveResult<'K, 'V> =
| NoAction
| Update  of 'V
| Replace of 'V
| UpdateAndAdd of UpdateValue: 'V * AddValue: 'V

let updateOrReplace
        (key: 'K)
        (handleUpdateOrReplace: 'V -> NonemptyKeyedSetUpdateOrRemoveResult<'K, 'V>)
        (conflictResolver: 'V -> 'V -> 'V)
        (source: NonemptyKeyedSet<'K, 'V>) =
    let (MapOfKeyToNonemptyKeyedSet nonEmptyMap) = source

    let handleUpdateOrReplaceForMap =
        fun v ->
            match handleUpdateOrReplace v with
            | NonemptyKeyedSetUpdateOrRemoveResult.NoAction  -> NonemptyMapUpdateOrRemoveResult.NoAction
            | NonemptyKeyedSetUpdateOrRemoveResult.Update v  -> NonemptyMapUpdateOrRemoveResult.Update v
            | NonemptyKeyedSetUpdateOrRemoveResult.Replace v -> NonemptyMapUpdateOrRemoveResult.Replace(v.Key, v)
            | NonemptyKeyedSetUpdateOrRemoveResult.UpdateAndAdd (updateValue, addValue) ->
                NonemptyMapUpdateOrRemoveResult.UpdateAndAdd(updateValue, (addValue.Key, addValue))

    let conflictResolverForMap _ v1 v2 = conflictResolver v1 v2

    NonemptyMap.updateOrReplace key handleUpdateOrReplaceForMap conflictResolverForMap nonEmptyMap
    |> MapOfKeyToNonemptyKeyedSet


#if !FABLE_COMPILER

open CodecLib

let codec valueCodec : Codec<_, NonemptyKeyedSet<'k, 'v>> =
    Codec.create (ofList >> Result.ofOption (Uncategorized "Set is empty")) (toSeq >> Seq.toList)
    |> Codec.compose (Codecs.list valueCodec)

type NonemptyKeyedSet<'K, 'V when 'K: comparison and 'V :> IKeyed<'K>> with
    static member inline get_Codec () : Codec<_, NonemptyKeyedSet<'k, 'v>> =
        Codec.create (ofList >> Result.ofOption (Uncategorized "Set is empty")) (toSeq >> Seq.toList)
        |> Codec.compose (Codecs.list defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'v>)

#endif
