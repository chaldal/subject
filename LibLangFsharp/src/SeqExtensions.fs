[<AutoOpen>]
module SeqExtensions

[<RequireQualifiedAccess>]
type ZipOuterResult<'T, 'U> =
| BothPresent of 'T * 'U
| FirstPresent of 'T
| SecondPresent of 'U

module Seq = Microsoft.FSharp.Collections.Seq
module Seq =
    // renaming Seq.choose for readability
    let filterMap<'T, 'U> (chooser: 'T -> Option<'U>) (source: seq<'T>) : seq<'U> =
        Seq.choose chooser source

    let filterNot (predicate: 'T -> bool) (source: seq<'T>) : seq<'T> =
        Seq.filter (predicate >> not) source

    let partition (predicate: 'T -> bool) (source: seq<'T>) : seq<'T> * seq<'T> =
        let grouped = Seq.groupBy predicate source |> Map.ofSeq
        (grouped.TryFind true |> Option.getOrElse Seq.empty, grouped.TryFind false |> Option.getOrElse Seq.empty)

    // renaming Seq.tryPick for readability
    let findMap<'T, 'U> (chooser: 'T -> Option<'U>) (source: seq<'T>) : Option<'U> =
        Seq.tryPick chooser source

    let flatConcatOptionSeqs (source: seq<Option<seq<'T>>>) : seq<'T> =
        source
        |> Seq.map (fun maybeSeq -> maybeSeq |> Option.getOrElse Seq.empty)
        |> Seq.concat

    let ofOneItem (item: 'T) : seq<'T> =
        Seq.singleton item

    let isNonempty (source: seq<'T>) : bool =
        source |> Seq.isEmpty |> not

    let tryMinBy (projection: 'T -> 'U) (source: seq<'T>) : Option<'T> =
        match Seq.isEmpty source with
        | true  -> None
        | false -> Seq.minBy projection source |> Some

    let prependToList (existingList: list<'T>) (source: seq<'T>) : list<'T> =
        source
        |> Seq.fold (fun lst i -> i::lst) existingList

    let tryMaxBy (projection: 'T -> 'U) (source: seq<'T>) : Option<'T> =
        match Seq.isEmpty source with
        | true  -> None
        | false -> Seq.maxBy projection source |> Some

    let distinctByWithLimit (projection: 'T -> 'U) (maxCount: int) (source: seq<'T>) : seq<'T> =
        // The simplest way to do this is to have a mutable collection
        let dic = System.Collections.Generic.Dictionary<'U, bool>(maxCount)

        source
        |> Seq.where (
            fun i ->
                let key = projection i
                if dic.ContainsKey key then
                    false
                else
                    dic.Add(key, false)
                    true
            )
        |> Seq.take maxCount

    let notExist (predicate: 'T -> bool) (sequence: seq<'T>) : bool =
        sequence
        |> Seq.exists predicate
        |> not

    // NOTE WARNING this function seems unnecessarily complicated
    // and poorly performing — it should be possible to simply
    // generate a single random index between 0 and seq.length-1.
    // I don't feel comfortable reimplmeneting without understanding
    // why this function was written in this way in the first place.
    let random (source: seq<'T>) : Option<'T> =
        let rnd     = System.Random()
        let randoms = Seq.initInfinite (fun _ -> rnd.Next())

        source
        |> Seq.zip randoms
        |> Seq.fold
            (fun maybeMin (rnd, i) ->
                match maybeMin with
                | ValueNone -> ValueSome (struct(rnd, i))
                | ValueSome (struct(minRnd, minRndI)) ->
                    if rnd < minRnd then
                        ValueSome(struct(rnd, i))
                    else
                        ValueSome(struct(minRnd, minRndI))
            )
            ValueNone
        |> function
           | ValueNone -> None
           | ValueSome (struct(_, x)) -> Some x

    let takeRandom (count: int) (source: seq<'T>) : seq<'T> =
        let random = System.Random ()
        source
        |> Seq.sortBy (fun _ -> random.Next())
        |> Seq.take count

    let takeMax (count: int) (source: seq<'T>) : seq<'T> =
        if Seq.length source <= count then
            source
        else
            source |> Seq.take count

    let truncateRandom (count: int) (source: seq<'T>) : seq<'T> =
        let random = System.Random ()
        source
        |> Seq.sortBy (fun _ -> random.Next())
        |> Seq.truncate count


    let unzip (source: seq<'T * 'U>) : seq<'T> * seq<'U> =
        source
        |> List.ofSeq
        |> List.unzip
        |> fun (t, u) -> t |> Seq.ofList, u |> Seq.ofList

    // NOTE if we use this for anything intensive, it should be reimplemented lazily
    let repeat (times: int) (source: seq<'T>) : seq<'T> =
        [1 .. times]
        |> Seq.map (fun _ -> source)
        |> Seq.concat

    let zipOuter (first: seq<'T>) (second: seq<'U>): seq<ZipOuterResult<'T, 'U>> =
        let firstEnumerator = first.GetEnumerator()
        let secondEnumerator = second.GetEnumerator()
        let mutable finishedFirst = not (firstEnumerator.MoveNext())
        let mutable finishedSecond = not (secondEnumerator.MoveNext())

        seq {
            while not (finishedFirst && finishedSecond) do
                match finishedFirst, finishedSecond with
                | false, false -> ZipOuterResult.BothPresent (firstEnumerator.Current, secondEnumerator.Current)
                | false, true -> ZipOuterResult.FirstPresent firstEnumerator.Current
                | true, false -> ZipOuterResult.SecondPresent secondEnumerator.Current
                | true, true -> failwith "Unexpected"

                finishedFirst <- not (firstEnumerator.MoveNext())
                finishedSecond <- not (secondEnumerator.MoveNext())
        }