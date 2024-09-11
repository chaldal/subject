[<AutoOpen>]
module ListExtensions

type Microsoft.FSharp.Collections.List<'V> with
    member this.IsNonempty : bool =
        not this.IsEmpty

    // Need this because `Item` which is an "indexer property" cannot be used as a partial function
    member this.GetByIndex (index: int) : 'V =
        this.Item index

// For some reason, without this alias, we get the following error:
// error FS0534: A module abbreviation must be a simple name, not a path
module List = Microsoft.FSharp.Collections.List
module List =
    let filterNot (predicate: 'T -> bool) (source: list<'T>) : list<'T> =
        List.filter (predicate >> not) source

    // renaming List.choose for readability
    let filterMap<'T, 'U> (chooser: 'T -> Option<'U>) (source: list<'T>) : list<'U> =
        List.choose chooser source

    // renaming List.tryPick for readability
    let findMap<'T, 'U> (chooser: 'T -> Option<'U>) (source: list<'T>) : Option<'U> =
        List.tryPick chooser source

    let without<'U when 'U : equality> (item: 'U) (source: list<'U>) : list<'U> =
        List.filter (fun (curr: 'U) -> curr <> item) source

    let replace (item: 'U) (replacement: 'U) (source: list<'U>) : list<'U> =
        source
        |> List.choose (function
            | candidate when candidate = item -> Some replacement
            | candidate                       -> Some candidate
        )

    let replaceByIndex (index: int) (replacement: 'U) (source: list<'U>) : list<'U> =
        (source |> List.take index) @ [replacement] @ (source |> List.skip (index + 1))

    let intersperse<'T> (separator: 'T) (source: list<'T>) : list<'T> =
        List.foldBack
            (fun currItem -> function
                | [] -> [currItem]
                | accItems -> currItem :: separator :: accItems
            )
            source
            []

    let ofOneItem (item: 'T) : list<'T> = [item]

    let keyBy (keyer: 'T -> 'K) (source: list<'T>) : Map<'K, List<'T>> =
        source
        |> List.fold
            (fun acc item ->
                let key = keyer item
                let listToUpdate = acc.TryFind key |> Option.getOrElse []
                acc.Add (key, item :: listToUpdate)
            )
            Map.empty

    let removeAt (index: int) (source: list<'T>) : list<'T> =
        (source |> List.take index) @ (source |> List.skip (index + 1))

    let mapIfNonEmptyOrElse<'T, 'U> (elseValue: List<'U>) (mapper: 'T -> 'U) (source: List<'T>) : List<'U> =
        if source.IsNonempty then
            source |> List.map mapper
        else
            elseValue

    let flatten<'T> (itemss: List<List<'T>>) : List<'T> =
        itemss |> List.collect id

    let random<'T> (items: List<'T>) : 'T =
        List.item (System.Random().Next(0, List.length items)) items
    
    let shuffle<'T when 'T : equality> (items: List<'T>) : 'T list =
        let rec shuffleRec (acc: 'T list) (remaining: 'T list) : 'T list =
            match remaining with
            | [] -> acc
            | _ ->
                let pickedElement = remaining |> random
                let rest = remaining |> List.except [pickedElement]
                shuffleRec (pickedElement :: acc) rest
        shuffleRec [] items

    let takeOrLess<'T> (size: int) (items: List<'T>): 'T List =
        match items.Length with
        | x when x <= size -> items
        | _                -> items |> List.take size
