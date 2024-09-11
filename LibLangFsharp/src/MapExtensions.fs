[<AutoOpen>]
module MapExtensions

type Microsoft.FSharp.Collections.Map<'K, 'V when 'K : comparison> with
    // renaming for better descriptiveness
    member this.AddOrUpdate (key: 'K, value: 'V) : Microsoft.FSharp.Collections.Map<'K, 'V> =
        this.Add(key, value)

    /// Similar to AddOrUpdate but returns Error if key already exists with a different value.
    /// Useful for idempotent action retries.
    member this.AddOrIgnoreSameValue (key: 'K, value: 'V) : Result<Microsoft.FSharp.Collections.Map<'K, 'V>, 'V> =
        match this.TryFind key with
        | None ->
            Ok <| this.Add (key, value)
        | Some sameValue when System.Object.Equals(value, sameValue) ->
            Ok this
        | Some differentValue ->
            Error differentValue

    member this.NotContainsKey (key: 'K) : bool =
        this.ContainsKey key |> not

    member this.FindOrElse (defaultValue: 'V) (key: 'K) : 'V =
        match this.TryFind key with
        | Some value -> value
        | None       -> defaultValue

    member this.KeySet : Set<'K> =
        this.Keys |> Set.ofSeq

    member this.IsNonempty : bool =
        not this.IsEmpty

    member this.RemoveMultiple (keys: Set<'K>) : Map<'K, 'V> =
        keys
        |> Set.fold
            (fun acc key ->
                acc.Remove key
            )
            this

    member this.UpdateOrCreateValue (key: 'K) (updater: Option<'V> -> 'V) : Map<'K, 'V> =
        this.Add (key, updater (this.TryFind key))

module Map =
    let keys (m: Map<'K, 'V>) : Set<'K> =
        m |> Map.toSeq |> Seq.map fst |> Set.ofSeq

    let values (m: Map<'K, 'V>) : seq<'V> =
        m |> Map.toSeq |> Seq.map snd

    let filterKeys (predicate: 'K -> bool) (source: Map<'K, 'V>) : Map<'K, 'V> =
        source |> Map.filter (fun key _ -> predicate key)

    let mapKeys (mapper: 'K1 -> 'K2) (source: Map<'K1, 'V>) : Map<'K2, 'V> =
        source
        |> Map.toSeq
        |> Seq.map (fun (k1, v) -> (mapper k1, v))
        |> Map.ofSeq

    let mapValues (mapper: 'V1 -> 'V2) (source: Map<'K, 'V1>) : Map<'K, 'V2> =
        source |> Map.map (fun _ v1 -> mapper v1)

    let filterNotKeys (predicate: 'K -> bool) (source: Map<'K, 'V>) : Map<'K, 'V> =
        source |> filterKeys (predicate >> not)

    let filterValues<'K, 'V when 'K: comparison> (predicate: 'V -> bool) (source: Map<'K, 'V>) : Map<'K, 'V> =
        source |> Map.filter (fun _ value -> predicate value)

    let filterMapValues (chooser: 'K -> 'V -> Option<'U>) (source: Map<'K, 'V>) : Map<'K, 'U> =
        source
        |> Map.toSeq
        |> Seq.choose
            (fun (key, value) ->
                (chooser key value)
                |> (Option.map (fun mappedValue -> (key, mappedValue)))
            )
        |> Map.ofSeq

    let filterMapKeys (chooser: 'K1 -> 'V-> Option<'K2>) (source: Map<'K1, 'V>) : Map<'K2, 'V> =
        source
        |> Map.toSeq
        |> Seq.choose
            (fun (key, value) ->
                (chooser key value)
                |> (Option.map (fun mappedKey -> (mappedKey, value)))
            )
        |> Map.ofSeq

    let filterMap (chooser: 'K1 * 'V1 -> Option<'K2 * 'V2>) (source: Map<'K1, 'V1>) : Map<'K2, 'V2> =
        source
        |> Map.toSeq
        |> Seq.choose chooser
        |> Map.ofSeq

    let merge (left: Map<'K, 'V>) (right: Map<'K, 'V>) : Map<'K, 'V> =
        right
        |> Map.fold
            (fun acc key value -> acc.Add(key, value))
            left

    let mergeMany (maps: seq<Map<'K, 'V>>) : Map<'K, 'V> =
        maps
        |> Seq.fold
            (fun acc currMap -> merge acc currMap)
            Map.empty

    let mergeInto (target: Map<'K, 'V1>) (merger: 'K -> Option<'V1> -> 'V2 -> 'V1) (source: Map<'K, 'V2>) : Map<'K, 'V1> =
        source
        |> Map.fold
            (fun acc key (value: 'V2) ->
                let maybeExisting = acc.TryFind key
                acc.Add(key, (merger key maybeExisting value)))
            target

    let updateValue (key: 'K) (updater: 'V -> 'V) (source: Map<'K, 'V>) : Map<'K, 'V> =
        match source.TryFind key with
        | None       -> source
        | Some value -> source.Add(key, updater value)

    let updateOrCreateValue (key: 'K) (updater: Option<'V> -> 'V) (source: Map<'K, 'V>) : Map<'K, 'V> =
        match source.TryFind key with
        | None       -> source.Add(key, updater None)
        | Some value -> source.Add(key, updater (Some value))

    let updateOrRemoveValue (key: 'K) (updater: 'V -> Option<'V>) (source: Map<'K, 'V>) : Map<'K, 'V> =
        match source.TryFind key with
        | None       -> source
        | Some value ->
            match updater value with
            | Some updatedValue -> source.Add(key, updatedValue)
            | None              -> source.Remove key

    let tryUpdateValue (key: 'K) (updater: 'V -> Result<'V, 'Error>) (source: Map<'K, 'V>) : Result<Map<'K, 'V>, 'Error> =
        match source.TryFind key with
        | None -> Ok source
        | Some value ->
            match updater value with
            | Ok updatedValue -> Ok (source.Add(key, updatedValue))
            | Error e         -> Error e

    let multiPartition (keyer: 'K -> 'V -> 'PartitionKey) (source: Map<'K, 'V>) : Map<'PartitionKey, Map<'K, 'V>> =
        source
        |> Map.fold
            (fun acc key value ->
                let partitionKey = keyer key value
                let partition = acc.FindOrElse Map.empty partitionKey
                acc.AddOrUpdate(partitionKey, partition.Add(key, value))
            )
            Map.empty

    let ofOneItem (key: 'K, value: 'V) : Map<'K, 'V> =
        Map.empty.Add(key, value)

    let isNonempty (map: Map<'K, 'V>) : bool =
        not map.IsEmpty

    let flattenNestedMaps (tToUtoV: Map<'T, Map<'U, 'V>>) : seq<'T * 'U * 'V> =
        tToUtoV
        |> Map.toSeq
        |> Seq.collect (
            fun (t, uToV) ->
                uToV
                |> Map.toSeq
                |> Seq.map (fun (u, v) -> (t, u, v))
        )

    let addOption (opt: Option<'K * 'V>) (map: Map<'K, 'V>) : Map<'K, 'V> =
        opt |> Option.fold (fun acc (k, v) -> acc.Add(k, v))  map

    let inline sumBy (projection: 'K -> 'V -> ^U) (source: Map<'K, 'V>) : ^U =
        (LanguagePrimitives.GenericZero< ^U>, source)
        ||> Map.fold (fun acc k v -> acc + (projection k v))
