[<AutoOpen>]
module LibClient.RenderHelpers

open LibClient

// Has to be inline because otherwise in the JS runtime Thoth can't get
// a hold of the runtime reflection information necessary for decoding
let inline jsonStringify<'T> (data: 'T) : string =
    Json.ToString data

let inline jsonStringifyWithIndent<'T> (indent: int) (data: 'T) : string =
    Thoth.Json.Encode.Auto.toString(indent, data)

let TopLevelBlockClass = ReactXP.LegacyStyles.Runtime.TopLevelBlockClass

let pluralize (count: uint32) (singularString: string) (pluralString: string) : string =
    let pluralizedString =
        match count with
        | 1ul -> singularString
        | _   -> pluralString

    sprintf "%d %s" count pluralizedString

[<RequireQualifiedAccess>]
type CollectionItemPosition = {
    IsFirst: bool
    IsLast:  bool
    Index:   int
} with
    static member OfIndex (index: int) (length: int) : CollectionItemPosition = {
        IsFirst = index = 0
        IsLast  = index = length - 1
        Index   = index
    }

    static member Only : CollectionItemPosition = {
        IsFirst = true
        IsLast  = true
        Index   = 0
    }

module Seq = Microsoft.FSharp.Collections.Seq
module Seq =
    let mapWithPosition (mapper: CollectionItemPosition -> 'T -> 'U) (source: seq<'T>) : seq<'U> =
        let length = Seq.length source
        source |> Seq.mapi (fun i t ->
            mapper (CollectionItemPosition.OfIndex i length) t
        )
