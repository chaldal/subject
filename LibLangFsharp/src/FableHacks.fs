[<AutoOpen>]
module FableHacks

// because https://github.com/fable-compiler/Fable/issues/2242
#if FABLE_COMPILER
module Map = Microsoft.FSharp.Collections.Map
module Map =
    let private originalToSeq = Map.toSeq
    let toSeqSafe (source: Map<'K, 'V>) : seq<'K * 'V> =
        source |> originalToSeq |> Seq.toList |> List.toSeq
    let toSeq = toSeqSafe
#endif
