[<AutoOpen>]
module NonemptyListModule

type NonemptyList<'T> =
    private | NonemptyList of List<'T>

    member this.ToList : List<'T> =
        match this with
        | NonemptyList list -> list

    member this.Length : int =
        this.ToList.Length

    member this.Head : 'T =
        this.ToList.Head

    member this.Last : 'T =
        this.ToList.[this.Length - 1]

    member this.Tail : List<'T> =
        match this.ToList with
        | _ :: tail -> tail
        | [] -> failwith "unexpected"

    member this.Item (index: int) : Option<'T> =
        this.ToList |> List.tryItem index

    member this.Cons (item: 'T) : NonemptyList<'T> =
        NonemptyList (item :: this.ToList)

    member this.Append (items: NonemptyList<'T>) : NonemptyList<'T> =
        NonemptyList (List.append this.ToList items.ToList)

    member this.Append (items: list<'T>) : NonemptyList<'T> =
        NonemptyList (List.append this.ToList items)

module NonemptyList =

    open CodecLib

    let toList (source: NonemptyList<'T>) : List<'T> =
        source.ToList

    let toSeq (source: NonemptyList<'T>) : seq<'T> =
        source.ToList |> List.toSeq

    let ofList (source: List<'T>) : Option<NonemptyList<'T>> =
        match source with
        | [] -> None
        | _  -> Some (NonemptyList source)

    let ofListUnsafe (source: List<'T>) : NonemptyList<'T> =
        source |> ofList |> Option.get

    let ofSeq (source: seq<'T>) : Option<NonemptyList<'T>> =
        source |> List.ofSeq |> ofList

    let ofOneItem (item: 'T) : NonemptyList<'T> =
        NonemptyList [item]

    let toHeadAndTail (source: NonemptyList<'T>) : 'T * list<'T> =
        let lst = source.ToList
        (lst.Head, lst.Tail)

    let head (source: NonemptyList<'T>) : 'T =
        source.Head

    let appendSeq (maybeMore: seq<'T>) (atLeastOne: NonemptyList<'T>) : NonemptyList<'T> =
        [
            yield! atLeastOne.ToList
            yield! maybeMore
        ]
        |> NonemptyList

    let without (item: 'T) (source: NonemptyList<'T>) : Option<NonemptyList<'T>> =
        match source.ToList |> List.without item with
        | []     -> None
        | result -> NonemptyList result |> Some

    let map (mapper: 'T -> 'U) (source: NonemptyList<'T>) : NonemptyList<'U> =
        source.ToList
        |> List.map mapper
        |> NonemptyList

    let mapi (mapper: int -> 'T -> 'U) (source: NonemptyList<'T>) : NonemptyList<'U> =
        source.ToList
        |> List.mapi mapper
        |> NonemptyList

    let distinct (source: NonemptyList<'T>) : NonemptyList<'T> =
        source.ToList
        |> List.distinct
        |> NonemptyList

    let contains (item: 'T) (source: NonemptyList<'T>) : bool =
        source.ToList
        |> List.contains item

    let distinctBy (projection: 'T -> 'Key) (source: NonemptyList<'T>) : NonemptyList<'T> =
        source.ToList
        |> List.distinctBy projection
        |> NonemptyList

    let sortBy (projection: 'T -> 'Key)  (source: NonemptyList<'T>) : NonemptyList<'T> =
        source.ToList
        |> List.sortBy projection
        |> NonemptyList

    let addOrAppend (map: Map<'K, NonemptyList<'V>>) (key: 'K) (value: 'V) : Map<'K, NonemptyList<'V>> =
        match map.TryFind key with
        | Some values -> map.AddOrUpdate(key, values.Cons value)
        | None        -> map.Add(key, ofOneItem value)

    let addOrAppendMultiple (key: 'K) (values: NonemptyList<'V>) (map: Map<'K, NonemptyList<'V>>) : Map<'K, NonemptyList<'V>> =
        match map.TryFind key with
        | Some existing -> map.AddOrUpdate(key, existing.Append values)
        | None          -> map.Add(key, values)

    let groupBy (projection: 'T -> 'K) (source: NonemptyList<'T>) : NonemptyList<'K * NonemptyList<'T>> =
        source
        |> toSeq
        |> Seq.groupBy projection
        // safe as we're dealing with non-empties
        |> Seq.map (fun (k, v) -> (k, ((ofSeq >> Option.get) v)))
        |> ofSeq
        |> Option.get // safe as we're dealing with non-empties

    let replaceByIndex (index: int) (replacement: 'U) (source: NonemptyList<'U>) : NonemptyList<'U> =
        source.ToList
        |> List.replaceByIndex index replacement
        |> NonemptyList

    let minBy (projection: 'T -> 'Key) (source: NonemptyList<'T>) : 'T =
        source.ToList
        |> List.minBy projection


#if !FABLE_COMPILER
    let codec codec : Codec<_, NonemptyList<'t>> =
        Codec.create (ofList >> Result.ofOption (Uncategorized "List is empty")) toList
        |> Codec.compose (Codecs.list codec)
#endif

[<AutoOpen>]
module NonEmptyListBuilder =
    /// Creates a NonemptyList
    type NelBuilder () =
        [<CompilerMessage("A NonEmptyList doesn't support the Zero operation.", 10708, IsError = true)>]
        member _.Zero () = failwith "Unreachable code"
        member _.Combine (a: 'T, b: list<'T>) = a::b
        member _.Yield x = x
        member _.Delay expr = expr ()
        member _.Run (x: list<_>) = Option.get (NonemptyList.ofList x)
    let nel = NelBuilder ()

[<AutoOpen>]
module NonEmptyListBuilderExtensions =
    type NelBuilder with
        member _.Combine (a: 'T, b: 'T) = a::[b]
        member _.Run x = NonemptyList.ofOneItem x

type Microsoft.FSharp.Core.Result<'T,'Error> with
    static member liftNonemptyList<'T, 'Error> (nonemptyListOfResults: NonemptyList<Result<'T, 'Error>>) : Result<NonemptyList<'T>, NonemptyList<'Error>> =
        nonemptyListOfResults.ToList
        |> Result.liftList
        |> Result.map      (NonemptyList.ofList >> Option.get) // okay because we started with NonemptyList
        |> Result.mapError (NonemptyList.ofList >> Option.get) // okay because we started with NonemptyList


#if !FABLE_COMPILER

open CodecLib

type NonemptyList<'T> with
    static member inline get_Codec () : Codec<_, NonemptyList<'t>> =
        Codec.create (NonemptyList.ofList >> Result.ofOption (Uncategorized "List is empty")) NonemptyList.toList
        |> Codec.compose (Codecs.list defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 't>)

#endif
