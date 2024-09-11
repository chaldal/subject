[<AutoOpen>]
module NonemptyOrderedSet

type NonemptyOrderedSet<'V when 'V: comparison> =
    private | NonemptyOrderedSet of OrderedSet<'V>

    member this.ToOrderedSet : OrderedSet<'V> =
        match this with
        | NonemptyOrderedSet orderedSet -> orderedSet

    member this.ToNonemptySet : NonemptySet<'V> =
        this.ToSet |> NonemptySet.ofSet |> Option.get // okay because started with nonempty

    member this.ToSeq : seq<'V> =
        this.ToOrderedSet.ToSeq

    member this.ToSet : Set<'V> =
        this.ToOrderedSet.ToSet

    member this.Count : PositiveInteger =
        this.ToOrderedSet.Count |> PositiveInteger.ofIntUnsafe

    member this.Add (item: 'V) : NonemptyOrderedSet<'V> =
        this.ToOrderedSet.Add(item) |> NonemptyOrderedSet

    member this.Remove (item: 'V) : Option<NonemptyOrderedSet<'V>> =
        let updated = this.ToOrderedSet.Remove item
        match updated.IsEmpty with
        | true  -> None
        | false -> Some (NonemptyOrderedSet updated)

    member this.Contains (item: 'V) : bool =
        this.ToOrderedSet.Contains item

let ofOneItem (item: 'V): NonemptyOrderedSet<'V> =
    OrderedSet.ofOneItem item |> NonemptyOrderedSet

let tryOfOrderedSet (orderedSet: OrderedSet<'V>): Option<NonemptyOrderedSet<'V>> =
    match orderedSet.IsEmpty with
    | true  -> None
    | false -> orderedSet |> NonemptyOrderedSet |> Some

let ofOrderedSetUnsafe (orderedSet: OrderedSet<'V>): NonemptyOrderedSet<'V> =
    orderedSet |> NonemptyOrderedSet

let tryOfList (list: List<'V>): Option<NonemptyOrderedSet<'V>> =
    match list.IsEmpty with
    | true  -> None
    | false -> list |> OrderedSet.ofList |> NonemptyOrderedSet |> Some

let tryOfSeq (source: seq<'V>): Option<NonemptyOrderedSet<'V>> =
    match source |> Seq.isEmpty with
    | true  -> None
    | false -> source |> OrderedSet.ofSeq |> NonemptyOrderedSet |> Some

let toUnorderedSet (nonemptyOrderedSet: NonemptyOrderedSet<'V>) : Set<'V> =
    match nonemptyOrderedSet with
    | NonemptyOrderedSet orderedSet -> orderedSet.ToSet

let count (source: NonemptyOrderedSet<'V>) : PositiveInteger =
    source.ToOrderedSet.Count |> PositiveInteger.ofIntUnsafe

let toSeq (nonemptyOrderedSet: NonemptyOrderedSet<'V>) : seq<'V> =
    nonemptyOrderedSet.ToOrderedSet.ToSeq

let toNonemptyList (source: NonemptyOrderedSet<'V>) : NonemptyList<'V> =
    source.ToOrderedSet.ToList |> NonemptyList.ofList |> Option.get (* Okay because we started with nonempty *)

let toNonemptySet (source: NonemptyOrderedSet<'V>) : NonemptySet<'V> =
    source.ToOrderedSet |> OrderedSet.toUnorderedSet |> NonemptySet.ofSet |> Option.get (* Okay because we started with nonempty *)

let last (nonemptyOrderedSet: NonemptyOrderedSet<'V>) : 'V =
    nonemptyOrderedSet.ToSeq
    |> Seq.last


#if !FABLE_COMPILER

open CodecLib

let codec valueCodec : Codec<_, NonemptyOrderedSet<'v>> =
    Codec.create (tryOfList >> Result.ofOption (Uncategorized "Set is empty"))  (toSeq >> Seq.toList)
    |> Codec.compose (Codecs.list valueCodec)

type NonemptyOrderedSet<'V when 'V: comparison> with
    static member inline get_Codec () : Codec<_, NonemptyOrderedSet<'v>> =
        Codec.create (tryOfList >> Result.ofOption (Uncategorized "Set is empty"))  (toSeq >> Seq.toList)
        |> Codec.compose (Codecs.list defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'v>)

#endif