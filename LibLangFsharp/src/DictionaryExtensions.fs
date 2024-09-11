[<AutoOpen>]
module DictionaryExtensions

open System.Collections.Generic

type Dictionary<'K, 'V> with
    member this.Get (key: 'K) : Option<'V> =
        match this.TryGetValue key with
        | (true, value) -> Some value
        | _ -> None
