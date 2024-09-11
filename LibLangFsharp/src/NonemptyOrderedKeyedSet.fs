[<AutoOpen>]
module NonemptyOrderedKeyedSet

(*
    Ordered but not optimized for modifying at all
    TODO improve on this if we find ourselves using this type for large cardinality sets.
    System.Collections.Immutable apparently don't work with Fable
*)
type NonemptyOrderedKeyedSet<'K, 'V when 'K: comparison and 'V :> IKeyed<'K>> =
    private {
        Map: NonemptyMap<'K, 'V>
        OrderedKeys: array<'K>
    }
    with
        member this.ToMap : NonemptyMap<'K, 'V> =
            this.Map

        member this.GetByKey (key: 'K) : Option<'V> =
            this.Map.TryFind key

        member this.ContainsKey (key: 'K) : bool =
            this.Map.ContainsKey key

        member this.RemoveByKey (key: 'K) : Option<NonemptyOrderedKeyedSet<'K, 'V>> =
            this.Map.Remove key
            |> Option.map (fun map -> { Map = map; OrderedKeys = this.OrderedKeys |> Array.filter ((<>) key) })

        member this.Remove (value: 'V) : Option<NonemptyOrderedKeyedSet<'K, 'V>> =
            this.RemoveByKey value.Key

        member this.UpdateExisting (value: 'V) : NonemptyOrderedKeyedSet<'K, 'V> =
            let key = value.Key
            if this.Map.ContainsKey key then
                { Map = this.Map.AddOrUpdate (key, value); OrderedKeys = this.OrderedKeys }
            else
                failwithf "can't update, key not found in nonempty ordered keyed set"

        member this.PrependOrUpdate (value: 'V) : NonemptyOrderedKeyedSet<'K, 'V> =
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

        member this.PrependOrUpdate (values: seq<'V>) : NonemptyOrderedKeyedSet<'K, 'V> =
            values
            |> Seq.rev
            |> Seq.fold
                (fun acc value -> acc.PrependOrUpdate value)
                this

        member this.AppendOrUpdate (value: 'V) : NonemptyOrderedKeyedSet<'K, 'V> =
            let key = value.Key
            if this.Map.ContainsKey key then
                {
                    Map = this.Map.AddOrUpdate (key, value)
                    OrderedKeys = this.OrderedKeys
                }
            else
                {
                    Map = this.Map.AddOrUpdate (key, value)
                    OrderedKeys = Array.append this.OrderedKeys [| key |]
                }

        member this.AppendOrUpdate (values: seq<'V>) : NonemptyOrderedKeyedSet<'K, 'V> =
            values
            |> Seq.fold
                (fun acc value -> acc.AppendOrUpdate value)
                this

        member this.Count : PositiveInteger =
            this.Map.Count

        member this.Head : 'V =
            this.ValueAt 0

        member this.Keys : NonemptyOrderedSet<'K> =
            this.OrderedKeys |> NonemptyOrderedSet.tryOfSeq |> Option.get

        member this.ValueAt (index: int) =
            Map.find this.OrderedKeys.[index] this.Map.ToMap

        member this.KeyAt (index: int) =
            this.OrderedKeys.[index]

        member this.ToSeq : seq<'V> =
            let map = this.Map.ToMap
            this.OrderedKeys |> Seq.map (fun key -> Map.find key map)

        member this.ToList : NonemptyList<'V> =
            let map = this.Map.ToMap
            this.OrderedKeys |> Seq.map (fun key -> Map.find key map) |> NonemptyList.ofSeq |> Option.get

        member this.Swap (i: int) (j: int) : NonemptyOrderedKeyedSet<'K, 'V> =
            if (i < 0 || i >= this.OrderedKeys.Length || j < 0 || j > this.OrderedKeys.Length) then
                raise <| System.IndexOutOfRangeException()
            let keysCopy = Array.copy this.OrderedKeys
            keysCopy.[i] <- this.OrderedKeys.[j]
            keysCopy.[j] <- this.OrderedKeys.[i]
            { this with OrderedKeys = keysCopy }

let ofSeq<'K, 'V, 'Error when 'K: comparison and 'V :> IKeyed<'K>> (source: seq<'V>) : Option<NonemptyOrderedKeyedSet<'K, 'V>> =
    source
    |> Seq.map (fun v -> (v.Key, v))
    |> NonemptyMap.ofSeq
    |> Option.map (
        fun map ->
            let orderedKeys = source |> Seq.map (fun v -> v.Key) |> Array.ofSeq
            if orderedKeys.Length > map.Count.Value then
                failwith "ordered keyed set must not have values with duplicate keys"
            { Map = map; OrderedKeys = orderedKeys } )

let ofList<'K, 'V, 'Error when 'K: comparison and 'V :> IKeyed<'K>> (list: List<'V>) : Option<NonemptyOrderedKeyedSet<'K, 'V>> =
    list
    |> List.toSeq
    |> ofSeq

let ofNonemptyList<'K, 'V when 'K: comparison and 'V :> IKeyed<'K>> (list: NonemptyList<'V>) : NonemptyOrderedKeyedSet<'K, 'V> =
    list.ToList
    |> List.toSeq
    |> ofSeq
    |> Option.get

let ofOneItem<'K, 'V, 'Error when 'K: comparison and 'V :> IKeyed<'K>> (item: 'V) : NonemptyOrderedKeyedSet<'K, 'V> =
    item
    |> Seq.ofOneItem
    |> ofSeq
    |> Option.get // safe because have one item

let toSeq (source: NonemptyOrderedKeyedSet<'K, 'V>) : seq<'V> =
    source.ToSeq

let toOrderedKeyedSet (source: NonemptyOrderedKeyedSet<'K, 'V>) : OrderedKeyedSet<'K, 'V> =
    source |> toSeq |> OrderedKeyedSet.ofSeq

let toNonemptySet<'K, 'V when 'K: comparison and 'V : comparison and 'V :> IKeyed<'K>> (source: NonemptyOrderedKeyedSet<'K, 'V>) : NonemptySet<'V> =
    source.ToMap.Values |> NonemptySet.ofSeq |> Option.get (* Okay because we started with nonempty *)

let ofOrderedKeyedSet (source: OrderedKeyedSet<'K, 'V>) : Option<NonemptyOrderedKeyedSet<'K, 'V>> =
    source |> OrderedKeyedSet.toSeq |> ofSeq

let map (mapper: 'T -> 'U) (source: NonemptyOrderedKeyedSet<'K, 'T>) : NonemptyOrderedKeyedSet<'K, 'U> =
    source
    |> toSeq
    |> Seq.map mapper
    |> ofSeq
    |> Option.get

let fold<'State, 'K, 'V when 'K: comparison and 'V :> IKeyed<'K>> (folder: 'State -> 'V -> 'State) (state: 'State) (source: NonemptyOrderedKeyedSet<'K, 'V>) : 'State =
    source |> toSeq |> Seq.fold folder state


#if !FABLE_COMPILER

open CodecLib

let codec valueCodec : Codec<_, NonemptyOrderedKeyedSet<'k, 'v>> =
    Codec.create (ofList >> Result.ofOption (Uncategorized "Set is empty")) (toSeq >> Seq.toList)
    |> Codec.compose (Codecs.list valueCodec)

type NonemptyOrderedKeyedSet<'K, 'V when 'K: comparison and 'V :> IKeyed<'K>> with
    static member inline get_Codec () : Codec<_, NonemptyOrderedKeyedSet<'k, 'v>> =
        Codec.create (ofList >> Result.ofOption (Uncategorized "Set is empty")) (toSeq >> Seq.toList)
        |> Codec.compose (Codecs.list defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'v>)

#endif