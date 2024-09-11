[<AutoOpen>]
module OrderedKeyedSet

(*
    Ordered but not optimized for modifying at all
    TODO improve on this if we find ourselves using this type for large cardinality sets.
    System.Collections.Immutable apparently don't work with Fable
*)
type OrderedKeyedSet<'K, 'V when 'K: comparison and 'V :> IKeyed<'K>> =
    private {
        Map: Map<'K, 'V>
        OrderedKeys: array<'K>
    }
    with
        static member empty
            with get() = { Map = Map.empty<'K, 'V>; OrderedKeys = Array.empty }

        member this.ToMap : Map<'K, 'V> =
            this.Map

        member this.GetByKey (key: 'K) : Option<'V> =
            this.Map.TryFind key

        member this.ContainsKey (key: 'K) : bool =
            this.Map.ContainsKey key

        member this.RemoveByKey (key: 'K) : OrderedKeyedSet<'K, 'V> =
            let map = this.Map.Remove key
            { Map = map; OrderedKeys = this.OrderedKeys |> Array.filter ((<>) key) }

        member this.Remove (value: 'V) : OrderedKeyedSet<'K, 'V> =
            this.RemoveByKey value.Key

        member this.Count : int =
            this.Map.Count

        member this.IsEmpty : bool =
            this.Count = 0

        member this.IsNonempty : bool =
            not this.IsEmpty

        member this.Keys : OrderedSet<'K> =
            this.OrderedKeys |> OrderedSet.ofSeq

        member this.Values : seq<'V> =
            let map = this.Map
            this.OrderedKeys |> Seq.map (fun key -> Map.find key map)

        member this.ValueAt (index: int) =
            Map.find this.OrderedKeys.[index] this.Map

        member this.ToSeq : seq<'V> =
            let map = this.Map
            this.OrderedKeys |> Seq.map (fun key -> Map.find key map)

        member this.ToList : List<'V> =
            this.Values |> List.ofSeq

        member this.TryHead : Option<'V> =
            if this.Count = 0 then None
            else Some (this.ValueAt 0)

        member this.PrependOrUpdate (value: 'V) : OrderedKeyedSet<'K, 'V> =
            let key = value.Key
            if this.Map.ContainsKey key then
                {
                    Map = this.Map.AddOrUpdate (key, value)
                    OrderedKeys = this.OrderedKeys
                }
            else
                {
                    Map = this.Map.AddOrUpdate (key, value)
                    OrderedKeys = Array.append [| key |] this.OrderedKeys
                }

        member this.PrependOrUpdate (values: seq<'V>) : OrderedKeyedSet<'K, 'V> =
            values
            |> Seq.rev
            |> Seq.fold
                (fun acc value -> acc.PrependOrUpdate value)
                this

        member this.AppendOrUpdate (value: 'V) : OrderedKeyedSet<'K, 'V> =
            let key = value.Key
            if this.Map.ContainsKey key then
                {
                    Map = this.Map.AddOrUpdate (key, value)
                    OrderedKeys = this.OrderedKeys
                }
            else
                {
                    Map = this.Map.AddOrUpdate (key, value)
                    OrderedKeys = [| key |] |> Array.append this.OrderedKeys
                }

        member this.AppendOrUpdate (values: seq<'V>) : OrderedKeyedSet<'K, 'V> =
            values
            |> Seq.fold
                (fun acc value -> acc.AppendOrUpdate value)
                this

let ofSeq<'K, 'V, 'Error when 'K: comparison and 'V :> IKeyed<'K>> (source: seq<'V>) : OrderedKeyedSet<'K, 'V> =
    source
    |> Seq.map (fun v -> (v.Key, v))
    |> Map.ofSeq
    |> fun map ->
        let orderedKeys = source |> Seq.map (fun v -> v.Key) |> Array.ofSeq
        if orderedKeys.Length > map.Count then
            failwith "ordered keyed set must not have values with duplicate keys"
        { Map = map; OrderedKeys = orderedKeys }

let ofList<'K, 'V, 'Error when 'K: comparison and 'V :> IKeyed<'K>> (list: List<'V>) : OrderedKeyedSet<'K, 'V> =
    list
    |> List.toSeq
    |> ofSeq

let ofOrderedSet<'K, 'V, 'Error when 'K: comparison and 'V :> IKeyed<'K> and 'V : comparison> (orderedSet: OrderedSet<'V>) : OrderedKeyedSet<'K, 'V> =
    orderedSet.ToSeq
    |> ofSeq

let ofOneItem<'K, 'V, 'Error when 'K: comparison and 'V :> IKeyed<'K>> (item: 'V) : OrderedKeyedSet<'K, 'V> =
    ofList [item]

let toSeq (source: OrderedKeyedSet<'K, 'V>) : seq<'V> =
    source.ToSeq

let toKeyedSet (source: OrderedKeyedSet<'K, 'V>) : KeyedSet<'K, 'V> =
    source.Map.Values |> KeyedSet.ofSeq

let toOrderedSet (source: OrderedKeyedSet<'K, 'V>) : OrderedSet<'V> =
    source.ToSeq |> OrderedSet.ofSeq

let toSet (source: OrderedKeyedSet<'K, 'V>) : Set<'V> =
    source.ToMap.Values |> Set.ofSeq

let filter (predicate: 'K -> 'V -> bool) (source: OrderedKeyedSet<'K, 'V>) : OrderedKeyedSet<'K, 'V> =
    let map = source.Map |> Map.filter predicate
    let orderedKeys = source.OrderedKeys |> Array.filter map.Keys.Contains
    { Map = map; OrderedKeys = orderedKeys }


#if !FABLE_COMPILER

open CodecLib

let codec valueCodec : Codec<_, OrderedKeyedSet<'k, 'v>> =
    Codec.create (Ok << ofList) (toSeq >> Seq.toList)
    |> Codec.compose (Codecs.list valueCodec)

type OrderedKeyedSet<'K, 'V when 'K: comparison and 'V :> IKeyed<'K>> with
    static member inline get_Codec () : Codec<_, OrderedKeyedSet<'k, 'v>> =
        Codec.create (Ok << ofList) (toSeq >> Seq.toList)
        |> Codec.compose (Codecs.list defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'v>)

#endif
