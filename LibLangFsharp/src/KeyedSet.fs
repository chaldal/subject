[<AutoOpen>]
module KeyedSet

type IKeyed<'K when 'K: comparison> =
    abstract member Key : 'K

type KeyedSet<'K, 'V when 'K: comparison and 'V :> IKeyed<'K>> =
    private | MapOfKeyToKeyedSet of Map<'K, 'V>

    with
        static member empty
            with get() = MapOfKeyToKeyedSet(Map.empty<'K, 'V>)

        member this.Map : Map<'K, 'V> =
            match this with
            | MapOfKeyToKeyedSet map -> map

        member this.GetByKey (key: 'K) : Option<'V> =
            this.Map.TryFind key

        member this.AddOrUpdate (value: 'V) : KeyedSet<'K, 'V> =
            this.Map.Add(value.Key, value)
            |> MapOfKeyToKeyedSet

        member this.Update (key: 'K) (update: 'V -> 'V) : KeyedSet<'K, 'V> =
            this.Map
            |> Map.updateValue key update
            |> MapOfKeyToKeyedSet

        member this.ContainsKey (key: 'K) : bool =
            this.Map.ContainsKey key

        member this.Contains (value: 'V) : bool =
            this.Map.ContainsKey value.Key

        member this.RemoveByKey (key: 'K) : KeyedSet<'K, 'V> =
            this.Map.Remove key |> MapOfKeyToKeyedSet

        member this.Remove (value: 'V) : KeyedSet<'K, 'V> =
            this.Map.Remove value.Key |> MapOfKeyToKeyedSet

        member this.IsEmpty : bool =
            this.Map.IsEmpty

        member this.IsNonempty : bool =
            not this.IsEmpty

        member this.Count : int =
            this.Map.Count

        member this.Keys : Set<'K> =
            this.Map.KeySet

        member this.Values : seq<'V> =
            this.Map.Values

        member this.SetMembership (value: 'V) (shouldBeMember: bool) : KeyedSet<'K, 'V> =
            match (shouldBeMember, this.Contains value) with
            | (true, false) -> this.AddOrUpdate value
            | (false, true) -> this.Remove value
            | _ -> this


let private getMap (keyedSet: KeyedSet<'K, 'V>) : Map<'K, 'V> =
    match keyedSet with
    | MapOfKeyToKeyedSet map -> map


let tryFindKey<'K, 'V when 'K: comparison and 'V :> IKeyed<'K>> (predicate: 'V -> bool) (source: KeyedSet<'K, 'V>) : Option<'K> =
    source
    |> getMap
    |> Map.tryFindKey (fun _ value -> predicate value)

let tryFind<'K, 'V when 'K: comparison and 'V :> IKeyed<'K>> (predicate: 'V -> bool) (source: KeyedSet<'K, 'V>) : Option<'V> =
    match tryFindKey predicate source with
    | Some key -> source.GetByKey key
    | None     -> None

let tryUpdateValue<'K, 'V, 'Error when 'K: comparison and 'V :> IKeyed<'K>> (key: 'K) (updater: 'V -> Result<'V, 'Error>) (source: KeyedSet<'K, 'V>) : Result<KeyedSet<'K, 'V>, 'Error> =
    source
    |> getMap
    |> Map.tryUpdateValue key updater
    |> Result.map MapOfKeyToKeyedSet

let ofSeq<'K, 'V when 'K: comparison and 'V :> IKeyed<'K>> (source: seq<'V>) : KeyedSet<'K, 'V> =
    source
    |> Seq.map (fun v -> (v.Key, v))
    |> Map.ofSeq
    |> MapOfKeyToKeyedSet

let ofList<'K, 'V when 'K: comparison and 'V :> IKeyed<'K>> (list: List<'V>) : KeyedSet<'K, 'V> =
    list
    |> List.toSeq
    |> ofSeq

let ofOneItem<'K, 'V when 'K: comparison and 'V :> IKeyed<'K>> (item: 'V) : KeyedSet<'K, 'V> =
    (item.Key, item)
    |> Map.ofOneItem
    |> MapOfKeyToKeyedSet

let exists<'K, 'V when 'K: comparison and 'V :> IKeyed<'K>> (predicate: 'V -> bool) (source: KeyedSet<'K, 'V>) : bool =
    source
    |> getMap
    |> Map.exists (fun _ value -> predicate value)

let fold<'State, 'K, 'V when 'K: comparison and 'V :> IKeyed<'K>> (folder: 'State -> 'V -> 'State) (state: 'State) (source: KeyedSet<'K, 'V>) : 'State =
    Map.fold (fun state _ value -> folder state value) state (source |> getMap)

let toSeq (source: KeyedSet<'K, 'V>) : seq<'V> =
    source
    |> getMap
    |> Map.values

let toSet (source: KeyedSet<'K, 'V>) : Set<'V> =
    source
    |> getMap
    |> Map.values
    |> Set.ofSeq

let map (mapper: 'T -> 'U) (source: KeyedSet<'K, 'T>) : KeyedSet<'K, 'U> =
    source
    |> toSeq
    |> Seq.map mapper
    |> ofSeq

let filter (predicate: 'K -> 'V -> bool) (source: KeyedSet<'K, 'V>) : KeyedSet<'K, 'V> =
    source.Map |> Map.filter predicate |> MapOfKeyToKeyedSet

let merge (left: KeyedSet<'K, 'V>) (right: KeyedSet<'K, 'V>) : KeyedSet<'K, 'V> =
    Map.merge left.Map right.Map |> MapOfKeyToKeyedSet

let removeByKey (key: 'K) (source: KeyedSet<'K, 'V>) : KeyedSet<'K, 'V> =
    source.RemoveByKey key

let addOrUpdate (value: 'V) (source: KeyedSet<'K, 'V>) : KeyedSet<'K, 'V> =
    source.AddOrUpdate value

let addOrUpdateWith (key: 'K) (updater: Option<'V> -> 'V) (source: KeyedSet<'K, 'V>) : KeyedSet<'K, 'V> =
    MapOfKeyToKeyedSet (source.Map.UpdateOrCreateValue key updater)

#if !FABLE_COMPILER

open CodecLib

let codec valueCodec : Codec<'RawEncoding, KeyedSet<'k, 'v>> =
    Codec.create (Ok << ofList) (toSeq >> Seq.toList)
    |> Codec.compose (Codecs.list valueCodec)

type KeyedSet<'K, 'V when 'K: comparison and 'V :> IKeyed<'K>> with
    static member inline get_Codec () : Codec<'RawEncoding, KeyedSet<'k, 'v>> =
        Codec.create (Ok << ofList) (toSeq >> Seq.toList)
        |> Codec.compose (Codecs.list defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'v>)

#endif