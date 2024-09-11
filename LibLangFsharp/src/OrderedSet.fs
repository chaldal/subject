
[<AutoOpen>]
module OrderedSetModule

open System

(*
    Space complexity is 2n * (sizeOf<'V> + sizeOf<int>)
    TODO improve on this if we find ourselves using this type for large cardinality sets
*)
#if !FABLE_COMPILER
    [<CustomEquality>]
    [<CustomComparison>]
#endif
type OrderedSet<'V when 'V : comparison> =
    private {
        ValueToIndex: Map<'V, uint32>
        IndexToValue: Map<uint32, 'V>
        LastIndex:    uint32
    }

#if !FABLE_COMPILER
    override this.GetHashCode() = this.IndexToValue.GetHashCode()
    override this.Equals(other) =
        match other with
        | :? OrderedSet<'V> as typedOther ->
            this.IndexToValue.Count = typedOther.IndexToValue.Count &&
            (this.IndexToValue.Values |> Seq.zip (typedOther.IndexToValue.Values) |> Seq.forall (fun (x, y) -> x = y))
        | _ -> false
    interface IComparable with
        member this.CompareTo(other) =
            match other with
            | :? OrderedSet<'V> as typedOther -> (this.ToList :> IComparable<_>).CompareTo typedOther.ToList
            | _ -> invalidArg "obj" "not a OrderedSet"
#endif

    member this.ToSeq : seq<'V> =
        this.IndexToValue.Values

    member this.ToList : List<'V> =
        this.ToSeq |> Seq.toList

    member this.ToSet : Set<'V> =
        this.ValueToIndex.KeySet

    member this.Count : int =
        this.ValueToIndex.Count

    member this.ContainsOneOf (items: List<'V>) : bool =
        let rec loop (currentItems: List<'V>) : bool =
            match currentItems with
            | [] -> false
            | head :: tail ->
                if this.Contains head then
                    true
                else
                    this.ContainsOneOf tail

        loop items

    member this.Contains (item: 'V) : bool =
        this.ValueToIndex.ContainsKey item

    member this.NotContains (item: 'V) : bool =
        this.Contains item
        |> not

    member this.IsEmpty : bool =
        this.ValueToIndex.IsEmpty

    member this.IsNonempty : bool =
        not this.IsEmpty

    member this.Add (item: 'V) : OrderedSet<'V> =
        match this.ValueToIndex.TryFind item with
        | Some _ -> this
        | None   ->
            if this.LastIndex = UInt32.MaxValue then
                raise (OverflowException "Index of OrderedSet is out of the maximum limit of 'uint32'")
            else
                let nextIndex = this.LastIndex + 1u
                {
                    ValueToIndex = this.ValueToIndex.Add (item, nextIndex)
                    IndexToValue = this.IndexToValue.Add (nextIndex, item)
                    LastIndex    = nextIndex
                }

    member this.Add (items: seq<'V>) : OrderedSet<'V> =
        Seq.fold
            (fun (acc: OrderedSet<'V>) (curr:'V) -> acc.Add curr)
            this
            items

    member this.Remove (item: 'V) : OrderedSet<'V> =
        match this.ValueToIndex.TryFind item with
        | None       -> this
        | Some index ->
            { this with
                IndexToValue = this.IndexToValue.Remove index
                ValueToIndex = this.ValueToIndex.Remove item
            }

    member this.Toggle (item: 'V) : OrderedSet<'V> =
        if (this.Contains item) then
            this.Remove item
        else
            this.Add item

#if !FABLE_COMPILER

open CodecLib

#endif

module OrderedSet =
    let empty<'V when 'V : comparison> : OrderedSet<'V> =
        {
            ValueToIndex = Map.empty<'V, uint32>
            IndexToValue = Map.empty<uint32, 'V>
            LastIndex    = 0u
        }

    let ofOneItem (item: 'V): OrderedSet<'V> =
        empty<'V>.Add item

    let ofList (list: List<'V>): OrderedSet<'V> =
        empty<'V>.Add list

    let ofSeq (source: seq<'V>): OrderedSet<'V> =
        empty<'V>.Add source

    let toUnorderedSet (orderedSet: OrderedSet<'V>) : Set<'V> =
        orderedSet.ToSeq
        |> Set.ofSeq

    let count (source: OrderedSet<'V>) =
        source.Count

    let toSeq (orderedSet: OrderedSet<'V>) : seq<'V> =
        orderedSet.ToSeq

    let toArray (orderedSet: OrderedSet<'V>) : array<'V> =
        orderedSet.ToSeq
        |> Seq.toArray

    let toList (orderedSet: OrderedSet<'V>) : List<'V> =
        orderedSet.ToList

    let tryLast (orderedSet: OrderedSet<'V>) : Option<'V> =
        orderedSet.ToSeq
        |> Seq.tryLast

    let filter (predicate: 'V -> bool) (orderedSet: OrderedSet<'V>) : OrderedSet<'V> =
        orderedSet.ToSeq
        |> Seq.filter predicate
        |> ofSeq

    let map (mapping: 'V -> 'T) (source: OrderedSet<'V>) : OrderedSet<'T> =
        source.ToSeq
        |> Seq.map mapping
        |> ofSeq

    let mapi (mapping: int -> 'V -> 'T) (source: OrderedSet<'V>) : OrderedSet<'T> =
        source.ToSeq
        |> Seq.mapi mapping
        |> ofSeq

    let difference (left: OrderedSet<'V>) (right: OrderedSet<'V>) : OrderedSet<'V> =
        left
        |> filter right.NotContains

    let containsOneOf (items: List<'V>) (source: OrderedSet<'V>) : bool =
        source.ContainsOneOf items


#if !FABLE_COMPILER

    let codec valueCodec : Codec<_, OrderedSet<'v>> =
        Codec.create (Ok << ofList) (toSeq >> Seq.toList)
        |> Codec.compose (Codecs.list valueCodec)

type OrderedSet<'V when 'V : comparison> with
    static member inline get_Codec () : Codec<_, OrderedSet<'v>> =
        Codec.create (Ok << OrderedSet.ofList) (OrderedSet.toSeq >> Seq.toList)
        |> Codec.compose (Codecs.list defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'v>)

#endif