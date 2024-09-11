module CodecLib

open System
open System.Collections.Generic
open FSharpPlus
open FSharpPlus.Data

type CodecAutoGenerate () = inherit System.Attribute ()

[<AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)>]
type SkipCodecAutoGenerate () = inherit System.Attribute ()

type IInterfaceCodec<'Base> =
    interface
    end

type IEncoding = interface end

module NonEmptyList =
    let getOnlyElement (x: NonEmptyList<'T>): 'T =
        if x.Tail |> List.isEmpty |> not then
            failwithf "expected only one element, got %A" x
        else
            x.Head

[<RequireQualifiedAccess>]
type TerminalJsonNode =
    | Boolean
    | String
    | DateTime
    | DateTimeOffset
    | TimeSpan
    | Decimal
    | Float
    | Float32
    | Int
    | Uint32
    | Int64
    | Uint64
    | Int16
    | Uint16
    | Byte
    | Sbyte
    | Char
    | Guid
    | Unit // Empty array for System.Text.Json

[<RequireQualifiedAccess>]
type JsonNode =
    | Terminal of TerminalJsonNode
    | Choice of NonEmptyList<JsonNode>
    | Result of Ok: JsonNode * Error: JsonNode
    | Option of JsonNode
    | Array of JsonNode
    | Record of Map<string, JsonNode>
    | Tuple of NonEmptyList<JsonNode>
    | AnyOneOf of NonEmptyList<JsonNode>
    | Any // anything only for decode
    | OptWithOption of JsonNode
with
    member this.IsRequiredKey : bool =
        match this with
        | Terminal _
        | Choice _
        | Result _
        | Option _
        | Array _
        | Record _
        | Tuple _
        | AnyOneOf _
        | Any ->
            true
        | OptWithOption _ ->
            false

module JsonNode =
    let UnwrapRecord (node: JsonNode) : Map<string, JsonNode> =
        match node with
        | JsonNode.Record x -> x
        | _ -> failwithf $"expected record found {node}"

type [<Struct>] Encoding = Encoding of JsonNode
with
    interface IEncoding

// Helper function to create tuples easily
let makeTuple nodes =
    JsonNode.Tuple(NonEmptyList.ofList nodes)

// optimize use of AnyOneOf kinds of node, use JsonNode if input list is singleton
let makeAnyOneOf (nodes: NonEmptyList<JsonNode>) : JsonNode =
    match nodes with
    | { Head = jsonNode; Tail = [] } -> jsonNode
    | { Head = _jsonNode; Tail = _ } -> JsonNode.AnyOneOf(nodes)
type PropertyList<'Encoding> = PropertyList of unit
type MultiObj<'Encoding> = PropertyList<'Encoding>
type Decoder<'Encoding, 'T> = Decoder of NonEmptyList<JsonNode>
with
    static member (<|>) (x: Decoder<'Encoding, 'T>, y: Decoder<'Encoding, 'T>) : Decoder<'Encoding, 'T> =
        let (Decoder xx) = x
        let (Decoder yy) = y
        xx ++ yy |> Decoder

[<RequireQualifiedAccess>]
module Decoder =
    let run (Decoder x) = x


// Codec<> Record will always contain single encoder JsonNode, and multiple decoder JsonNode

// Decoder<PropertyList<'Encoding>, 'T> should be simple a Map<string, JsonNode>.
// But Codec<PropertyList<'Encoding>, 'T> and Codec<'Encoding,'T> must be same container.
// So hacky solution is it to use JsonNode.Record in Encoder and and single JsonNode.Record in decoder

//  encode t2 to s2, decode s1 to t1
type Codec<'S1, 'S2, 'T1, 'T2> = {
    Encoder: JsonNode
    Decoder: Decoder<'S2, 'T1>
}
with
    static member (<|>) (x: Codec<PropertyList<'Encoding>,PropertyList<'Encoding>, 'T, 'T>, y: Codec<PropertyList<'Encoding>,PropertyList<'Encoding>, 'T, 'T>) : Codec<PropertyList<'Encoding>,PropertyList<'Encoding>, 'T, 'T> =
        {
            Encoder = [ x.Encoder; y.Encoder ] |> NonEmptyList.ofList |> makeAnyOneOf
            Decoder =
                (x.Decoder |> Decoder.run) ++ (y.Decoder |> Decoder.run) |> Decoder
        }
type Codec<'S, 'T> = Codec<'S, 'S, 'T, 'T>

type DecodeError =
    | Uncategorized of string

module Decode =
    module Fail =
        let nullString = Error (Uncategorized "null")

// Codec<PropertyList<'Encoding>, PropertyList<'Encoding>, 't1, 't2>
// CodecPropertyList has virtual Encoders : Map<string * JsonNode> in a single JsonNode.Record case
// CodecPropertyList has virtual Decoders : NonemptyList<Map<string ,JsonNode>> in decoder NonemptyList<JsonNode.Record> case

module Codecs =
    let unit<'Encoding when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding)> : Codec<'Encoding , unit> =
        { Encoder = JsonNode.Terminal TerminalJsonNode.Unit
          Decoder =
            NonEmptyList.singleton (JsonNode.Terminal TerminalJsonNode.Unit)
            |> Decoder }

    let [<GeneralizableValue>] string<'Encoding when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding)> : Codec<'Encoding, string> =
        { Encoder = JsonNode.Terminal TerminalJsonNode.String
          Decoder =
            NonEmptyList.singleton (JsonNode.Terminal TerminalJsonNode.String)
            |> Decoder }

    let [<GeneralizableValue>] int<'Encoding when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding)> : Codec<'Encoding, int> =
        { Encoder = JsonNode.Terminal TerminalJsonNode.Int
          Decoder =
            NonEmptyList.singleton (JsonNode.Terminal TerminalJsonNode.Int)
            |> Decoder }

    let int16<'Encoding when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding)> : Codec<'Encoding, int16> =
        { Encoder = JsonNode.Terminal TerminalJsonNode.Int16
          Decoder =
            NonEmptyList.singleton (JsonNode.Terminal TerminalJsonNode.Int16)
            |> Decoder }

    let int64<'Encoding when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding)> : Codec<'Encoding, int64> =
        { Encoder = JsonNode.Terminal TerminalJsonNode.Int64
          Decoder =
            NonEmptyList.singleton (JsonNode.Terminal TerminalJsonNode.Int64)
            |> Decoder }

    let uint16<'Encoding when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding)> : Codec<'Encoding, uint16> =
        { Encoder = JsonNode.Terminal TerminalJsonNode.Uint16
          Decoder =
            NonEmptyList.singleton (JsonNode.Terminal TerminalJsonNode.Uint16)
            |> Decoder }

    let uint32<'Encoding when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding)> : Codec<'Encoding, uint32> =
        { Encoder = JsonNode.Terminal TerminalJsonNode.Uint32
          Decoder =
            NonEmptyList.singleton (JsonNode.Terminal TerminalJsonNode.Uint32)
            |> Decoder }

    let uint64<'Encoding when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding)> : Codec<'Encoding, uint64> =
        { Encoder = JsonNode.Terminal TerminalJsonNode.Uint64
          Decoder =
            NonEmptyList.singleton (JsonNode.Terminal TerminalJsonNode.Uint64)
            |> Decoder }

    let decimal<'Encoding when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding)> : Codec<'Encoding, decimal> =
        { Encoder = JsonNode.Terminal TerminalJsonNode.Decimal
          Decoder =
            NonEmptyList.singleton (JsonNode.Terminal TerminalJsonNode.Decimal)
            |> Decoder }

    let boolean<'Encoding when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding)> : Codec<'Encoding, Boolean> =
        { Encoder = JsonNode.Terminal TerminalJsonNode.Boolean
          Decoder =
            NonEmptyList.singleton (JsonNode.Terminal TerminalJsonNode.Boolean)
            |> Decoder }

    let date<'Encoding when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding)> : Codec<'Encoding, DateOnly> =
        { Encoder = JsonNode.Terminal TerminalJsonNode.DateTime
          Decoder =
            NonEmptyList.singleton (JsonNode.Terminal TerminalJsonNode.DateTime)
            |> Decoder }

    let time<'Encoding when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding)> : Codec<'Encoding, TimeOnly> =
        { Encoder = JsonNode.Terminal TerminalJsonNode.TimeSpan
          Decoder =
            NonEmptyList.singleton (JsonNode.Terminal TerminalJsonNode.TimeSpan)
            |> Decoder }

    let float<'Encoding when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding)> : Codec<'Encoding, float> =
        { Encoder = JsonNode.Terminal TerminalJsonNode.Float
          Decoder =
            NonEmptyList.singleton (JsonNode.Terminal TerminalJsonNode.Float)
            |> Decoder }

    let float32<'Encoding when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding)> : Codec<'Encoding, float32> =
        { Encoder = JsonNode.Terminal TerminalJsonNode.Float32
          Decoder =
            NonEmptyList.singleton (JsonNode.Terminal TerminalJsonNode.Float32)
            |> Decoder }

    let dateTimeOffset<'Encoding when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding)> : Codec<'Encoding, DateTimeOffset> =
        { Encoder = JsonNode.Terminal TerminalJsonNode.DateTimeOffset
          Decoder =
            NonEmptyList.singleton (JsonNode.Terminal TerminalJsonNode.DateTimeOffset)
            |> Decoder }

    let timeSpan<'Encoding when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding)> : Codec<'Encoding, TimeSpan> =
        { Encoder = JsonNode.Terminal TerminalJsonNode.TimeSpan
          Decoder =
            NonEmptyList.singleton (JsonNode.Terminal TerminalJsonNode.TimeSpan)
            |> Decoder }

    let guid<'Encoding when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding)> : Codec<'Encoding, Guid> =
        { Encoder = JsonNode.Terminal TerminalJsonNode.Guid
          Decoder =
            NonEmptyList.singleton (JsonNode.Terminal TerminalJsonNode.Guid)
            |> Decoder }

    let char<'Encoding when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding)> : Codec<'Encoding, char> =
        { Encoder = JsonNode.Terminal TerminalJsonNode.Char
          Decoder =
            NonEmptyList.singleton (JsonNode.Terminal TerminalJsonNode.Char)
            |> Decoder }

    let byte<'Encoding when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding)> : Codec<'Encoding, byte> =
        { Encoder = JsonNode.Terminal TerminalJsonNode.Byte
          Decoder =
            NonEmptyList.singleton (JsonNode.Terminal TerminalJsonNode.Byte)
            |> Decoder }

    let base64Bytes<'Encoding when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding)> : Codec<'Encoding, byte []> =
        { Encoder = JsonNode.Terminal TerminalJsonNode.String
          Decoder =
            NonEmptyList.singleton (JsonNode.Terminal TerminalJsonNode.String)
            |> Decoder }

    let list (x: Codec<'Encoding, 'T>) : Codec<'Encoding, list<'T>> =
        { Encoder = JsonNode.Array x.Encoder
          Decoder =
            NonEmptyList.singleton (JsonNode.Array(makeAnyOneOf (Decoder.run x.Decoder)))
            |> Decoder }

    let set (x: Codec<'Encoding, 'T>) : Codec<'Encoding, Set<'T>> =
        { Encoder = JsonNode.Array x.Encoder
          Decoder =
            NonEmptyList.singleton (JsonNode.Array(makeAnyOneOf (Decoder.run x.Decoder)))
            |> Decoder }

    let tuple2 (a: Codec<'Encoding, 'a>) (b: Codec<'Encoding, 'b>) : Codec<'Encoding, 'a * 'b> =
        { Encoder = makeTuple [a.Encoder; b.Encoder]
          Decoder =
            NonEmptyList.singleton (
                makeTuple [
                    makeAnyOneOf (Decoder.run a.Decoder)
                    makeAnyOneOf (Decoder.run b.Decoder)
                ]
            )
            |> Decoder }

    let tuple3 (a: Codec<'Encoding, 'a>) (b: Codec<'Encoding, 'b>) (c: Codec<'Encoding, 'c>) : Codec<'Encoding, 'a * 'b * 'c> =
        { Encoder = makeTuple [a.Encoder; b.Encoder; c.Encoder]
          Decoder =
            NonEmptyList.singleton (
                makeTuple [
                    makeAnyOneOf (Decoder.run a.Decoder)
                    makeAnyOneOf (Decoder.run b.Decoder)
                    makeAnyOneOf (Decoder.run c.Decoder)
                ]
            )
            |> Decoder }

    let tuple4 (a: Codec<'Encoding, 'a>) (b: Codec<'Encoding, 'b>) (c: Codec<'Encoding, 'c>) (d: Codec<'Encoding, 'd>) : Codec<'Encoding, 'a * 'b * 'c * 'd> =
        { Encoder = makeTuple [a.Encoder; b.Encoder; c.Encoder; d.Encoder]
          Decoder =
            NonEmptyList.singleton (
                makeTuple [
                    makeAnyOneOf (Decoder.run a.Decoder)
                    makeAnyOneOf (Decoder.run b.Decoder)
                    makeAnyOneOf (Decoder.run c.Decoder)
                    makeAnyOneOf (Decoder.run d.Decoder)
                ]
            )
            |> Decoder }

    let tuple5 (a: Codec<'Encoding, 'a>) (b: Codec<'Encoding, 'b>) (c: Codec<'Encoding, 'c>) (d: Codec<'Encoding, 'd>) (e: Codec<'Encoding, 'e>) : Codec<'Encoding, 'a * 'b * 'c * 'd * 'e> =
        { Encoder = makeTuple [a.Encoder; b.Encoder; c.Encoder; d.Encoder; e.Encoder]
          Decoder =
            NonEmptyList.singleton (
                makeTuple [
                    makeAnyOneOf (Decoder.run a.Decoder)
                    makeAnyOneOf (Decoder.run b.Decoder)
                    makeAnyOneOf (Decoder.run c.Decoder)
                    makeAnyOneOf (Decoder.run d.Decoder)
                    makeAnyOneOf (Decoder.run e.Decoder)
                ]
            )
            |> Decoder }

    let tuple6 (a: Codec<'Encoding, 'a>) (b: Codec<'Encoding, 'b>) (c: Codec<'Encoding, 'c>) (d: Codec<'Encoding, 'd>) (e: Codec<'Encoding, 'e>) (f: Codec<'Encoding, 'f>) : Codec<'Encoding, 'a * 'b * 'c * 'd * 'e * 'f> =
        { Encoder = makeTuple [a.Encoder; b.Encoder; c.Encoder; d.Encoder; e.Encoder; f.Encoder]
          Decoder =
            NonEmptyList.singleton (
                makeTuple [
                    makeAnyOneOf (Decoder.run a.Decoder)
                    makeAnyOneOf (Decoder.run b.Decoder)
                    makeAnyOneOf (Decoder.run c.Decoder)
                    makeAnyOneOf (Decoder.run d.Decoder)
                    makeAnyOneOf (Decoder.run e.Decoder)
                    makeAnyOneOf (Decoder.run f.Decoder)
                ]
            )
            |> Decoder }

    let tuple7 (a: Codec<'Encoding, 'a>) (b: Codec<'Encoding, 'b>) (c: Codec<'Encoding, 'c>) (d: Codec<'Encoding, 'd>) (e: Codec<'Encoding, 'e>) (f: Codec<'Encoding, 'f>) (g: Codec<'Encoding, 'g>) : Codec<'Encoding, 'a * 'b * 'c * 'd * 'e * 'f * 'g> =
        { Encoder = makeTuple [a.Encoder; b.Encoder; c.Encoder; d.Encoder; e.Encoder; f.Encoder; g.Encoder]
          Decoder =
            NonEmptyList.singleton (
                makeTuple [
                    makeAnyOneOf (Decoder.run a.Decoder)
                    makeAnyOneOf (Decoder.run b.Decoder)
                    makeAnyOneOf (Decoder.run c.Decoder)
                    makeAnyOneOf (Decoder.run d.Decoder)
                    makeAnyOneOf (Decoder.run e.Decoder)
                    makeAnyOneOf (Decoder.run f.Decoder)
                    makeAnyOneOf (Decoder.run g.Decoder)
                ]
            )
            |> Decoder }

    let array (x: Codec<'Encoding, 'a>) : Codec<'Encoding, 'a []> =
        { Encoder = JsonNode.Array x.Encoder
          Decoder =
            NonEmptyList.singleton (JsonNode.Array(makeAnyOneOf (Decoder.run x.Decoder)))
            |> Decoder }

    let option (x: Codec<'Encoding, 'a>) : Codec<'Encoding, Option<'a>> =
        { Encoder = JsonNode.Option x.Encoder
          Decoder =
            NonEmptyList.singleton (JsonNode.Option(makeAnyOneOf (Decoder.run x.Decoder)))
            |> Decoder }

    let result (a: Codec<'Encoding ,'a>) (b: Codec<'Encoding, 'b>) : Codec<'Encoding, Result<'a, 'b>> =
        { Encoder = JsonNode.Result(a.Encoder, b.Encoder)
          Decoder =
            NonEmptyList.singleton (
                JsonNode.Result((makeAnyOneOf (Decoder.run a.Decoder)), (makeAnyOneOf (Decoder.run b.Decoder)))
            )
            |> Decoder }

    let choice (a: Codec<'Encoding, 'a>) (b: Codec<'Encoding, 'b>) : Codec<'Encoding, Choice<'a, 'b>> =
        { Encoder = JsonNode.Choice <| NonEmptyList.ofList [a.Encoder; b.Encoder]
          Decoder =
            NonEmptyList.singleton (
                JsonNode.Choice <| NonEmptyList.ofList [(makeAnyOneOf (Decoder.run a.Decoder)); (makeAnyOneOf (Decoder.run b.Decoder))]
            )
            |> Decoder }

    let choice3 (a: Codec<'Encoding, 'a>) (b: Codec<'Encoding, 'b>) (c: Codec<'Encoding, 'c>) : Codec<'Encoding, Choice<'a, 'b, 'c>> =
        { Encoder = JsonNode.Choice <| NonEmptyList.ofList [a.Encoder; b.Encoder; c.Encoder]
          Decoder =
            NonEmptyList.singleton (
                JsonNode.Choice <| NonEmptyList.ofList [(makeAnyOneOf (Decoder.run a.Decoder)); (makeAnyOneOf (Decoder.run b.Decoder)); (makeAnyOneOf (Decoder.run c.Decoder))]
            )
            |> Decoder }

    let gmap (keyCodec: Codec<'Encoding, 'k>) (valueCodec: Codec<'Encoding, 'v>) : Codec<'Encoding, Map<'k, 'v>> =
        { Encoder = JsonNode.Array(JsonNode.Tuple <| NonEmptyList.ofList [keyCodec.Encoder; valueCodec.Encoder])
          Decoder =
            NonEmptyList.singleton (
                JsonNode.Array(
                    JsonNode.Tuple <| NonEmptyList.ofList [
                        (makeAnyOneOf (Decoder.run keyCodec.Decoder))
                        (makeAnyOneOf (Decoder.run valueCodec.Decoder))
                    ]
                )
            )
            |> Decoder }

    let decodeAnyToUnit<'Encoding when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding)> : Codec<'Encoding, unit> =
        { Encoder = JsonNode.Any
          Decoder = NonEmptyList.singleton JsonNode.Any |> Decoder }

module Codec =

    // Hacked up support for Codec.create and Codec.compose
    // ('a -> 'ParseResult<'b>) -> ('c -> 'd) -> ('a -> 'ParseResult<'b>) * ('c -> 'd) = Codec<'a, 'd , 'b, 'c>
    let create (decoder: 'a -> '``ParseResult<'b>``) (encoder: 'c -> 'd) = (decoder, encoder)

    // Codec<'a,'b,'c,'d> -> Codec<'c,'d,'e,'f> -> Codec<'a,'b,'e,'f>
    // Codec<'c, 'd, 'e, 'f> = ('c -> 'ParseResult<'e>) * ('f -> 'd)
    let compose (codec1: Codec<'Encoding, 'Encoding, 'c, 'd>) ((_decoder, _encoder): (('c -> '``ParseResult<'f>``) * ('f -> 'd))) : Codec<'Encoding, 'Encoding, 'f, 'f> =
        {
            Encoder = codec1.Encoder
            Decoder = Decoder.run codec1.Decoder |> Decoder
        }

let mutable private codecCollectionGlobalVersion : uint = 0u
let private codecCollectionMonitor = obj ()

type CodecCollection<'Encoding, 'Interface> () =
    static let mutable subtypes : Dictionary<Type, unit -> Codec<PropertyList<'Encoding>,'Interface>> = new Dictionary<_, _> ()
    static member GetSubtypes = subtypes
    static member AddSubtype ty (x: unit -> Codec<PropertyList<'Encoding>,'Interface>) =
        lock codecCollectionMonitor (fun () ->
            subtypes.[ty] <- x
            codecCollectionGlobalVersion <- codecCollectionGlobalVersion + 1u)

type CodecCache<'Encoding, 'T> () =
    static let mutable cachedCodecInterface : option<uint * Codec<'Encoding, 'T>> = None
    static let mutable cachedCodec          : option<       Codec<'Encoding, 'T>> = None

    static member Run (f: unit -> Codec<'Encoding, 'T>) =
        match cachedCodec with
        | Some c ->
            c
        | None   ->
            let c = f ()
            cachedCodec <- Some c
            c

    static member RunForInterfaces (f: unit -> Codec<'Encoding, 'T>) =
        match cachedCodecInterface with
        | Some (version, c) when version = codecCollectionGlobalVersion -> c
        | _ ->
            let version = codecCollectionGlobalVersion
            let c = f ()
            cachedCodecInterface <- Some (version, c)
            c


[<AutoOpen>]
module CodecInterfaceExtensions =
    type IInterfaceCodec<'Base> with
        static member RegisterCodec<'Encoding, 'Type> (codec: unit -> Codec<PropertyList<'Encoding>, 'Type>) =
            let codec () : Codec<PropertyList<'Encoding>, 'Base> =
                let objCodec = codec ()
                { Encoder = objCodec.Encoder
                  Decoder =
                        objCodec.Decoder
                        |> Decoder.run
                        |> Decoder }
            codec |> CodecCollection<'Encoding, 'Base>.AddSubtype typeof<'Type>

type AdHocEncoding = Encoding
let initializeInterfaceImplementation<'Interface, 'Type> (codec: unit -> Codec<PropertyList<AdHocEncoding>, 'Type>) =
    IInterfaceCodec<'Interface>.RegisterCodec<AdHocEncoding, 'Type> codec

let private mergeMapUnsafe (x: Map<string, 'a>) (y: Map<string, 'a>) : Map<string, 'a> =
    (x, y)
    ||> Map.fold (fun acc k v ->
        if acc.ContainsKey k then
            failwithf "Key %s already exists while merging maps %A %A" k x y
        else
            acc
            |> Map.add k v
    )

let unsafeCheckConsistencyOfDecoderBuildingPhase (c: Decoder<PropertyList<'Encoding>, 'T>) : unit =
    match c|> Decoder.run with
    | { Head = JsonNode.Record _; Tail = [] } ->
        ()
    | _ ->
        failwithf $"Unwanted state: (Decoder is not JsonNode.Record of singleton) of codec in builder phase: {c}"

let unsafeCheckConsistencyOfCodecBuildingPhase (c: Codec<PropertyList<'Encoding>, PropertyList<'Encoding>, 't1, 't2>) : unit =
    match c.Encoder with
    | JsonNode.Record _ ->
        ()
    | _ ->
        failwithf $"Unwanted state: (Encoder JsonNode is not Record) of codec in builder phase: {c}"
    unsafeCheckConsistencyOfDecoderBuildingPhase c.Decoder

type CodecApplicativeBuilder() =

    member _.Delay x = x ()
    member _.ReturnFrom expr = expr

    member _.MergeSources
        (t1: Codec<PropertyList<'Encoding>,PropertyList<'Encoding>, 'a, 's>, t2: Codec<PropertyList<'Encoding>, PropertyList<'Encoding>, 'b, 's>)
        : Codec<PropertyList<'Encoding>, PropertyList<'Encoding>, 'a * 'b, 's> =
        unsafeCheckConsistencyOfCodecBuildingPhase t1
        unsafeCheckConsistencyOfCodecBuildingPhase t2
        // merge two maps of encoder and decoder
        {
            Encoder =
                JsonNode.UnwrapRecord t1.Encoder
                |> mergeMapUnsafe (JsonNode.UnwrapRecord t2.Encoder)
                |> JsonNode.Record
            Decoder =
                (JsonNode.UnwrapRecord (NonEmptyList.getOnlyElement (t1.Decoder |> Decoder.run)))
                |> mergeMapUnsafe (JsonNode.UnwrapRecord (NonEmptyList.getOnlyElement (t2.Decoder |> Decoder.run)))
                |> JsonNode.Record
                |> NonEmptyList.singleton
                |> Decoder
        }

    member _.MergeSources
        (t1: Codec<PropertyList<'Encoding>,PropertyList<'Encoding>, 'a, 's>, t2: Codec<PropertyList<'Encoding>, PropertyList<'Encoding>, 'b, 's>, t3: Codec<PropertyList<'Encoding>, PropertyList<'Encoding>, 'c, 's>)
        : Codec<PropertyList<'Encoding>, PropertyList<'Encoding>, 'a * 'b * 'c, 's> =
        unsafeCheckConsistencyOfCodecBuildingPhase t1
        unsafeCheckConsistencyOfCodecBuildingPhase t2
        unsafeCheckConsistencyOfCodecBuildingPhase t3
        // merge three maps of encoder and decoder
        {
            Encoder =
                JsonNode.UnwrapRecord t1.Encoder
                |> mergeMapUnsafe (JsonNode.UnwrapRecord t2.Encoder)
                |> mergeMapUnsafe (JsonNode.UnwrapRecord t3.Encoder)
                |> JsonNode.Record
            Decoder =
                (JsonNode.UnwrapRecord (NonEmptyList.getOnlyElement (t1.Decoder |> Decoder.run)))
                |> mergeMapUnsafe (JsonNode.UnwrapRecord (NonEmptyList.getOnlyElement (t2.Decoder |> Decoder.run)))
                |> mergeMapUnsafe (JsonNode.UnwrapRecord (NonEmptyList.getOnlyElement (t3.Decoder |> Decoder.run)))
                |> JsonNode.Record
                |> NonEmptyList.singleton
                |> Decoder
        }

    member _.MergeSources
        (t1: Codec<PropertyList<'Encoding>,PropertyList<'Encoding>, 'a, 's>, t2: Codec<PropertyList<'Encoding>, PropertyList<'Encoding>, 'b, 's>, t3: Codec<PropertyList<'Encoding>, PropertyList<'Encoding>, 'c, 's>, t4: Codec<PropertyList<'Encoding>, PropertyList<'Encoding>, 'd, 's>)
        : Codec<PropertyList<'Encoding>, PropertyList<'Encoding>, 'a * 'b * 'c * 'd, 's> =
        unsafeCheckConsistencyOfCodecBuildingPhase t1
        unsafeCheckConsistencyOfCodecBuildingPhase t2
        unsafeCheckConsistencyOfCodecBuildingPhase t3
        unsafeCheckConsistencyOfCodecBuildingPhase t4
        // merge four maps of encoder and decoder
        {
            Encoder =
                JsonNode.UnwrapRecord t1.Encoder
                |> mergeMapUnsafe (JsonNode.UnwrapRecord t2.Encoder)
                |> mergeMapUnsafe (JsonNode.UnwrapRecord t3.Encoder)
                |> mergeMapUnsafe (JsonNode.UnwrapRecord t4.Encoder)
                |> JsonNode.Record
            Decoder =
                (JsonNode.UnwrapRecord (NonEmptyList.getOnlyElement (t1.Decoder |> Decoder.run)))
                |> mergeMapUnsafe (JsonNode.UnwrapRecord (NonEmptyList.getOnlyElement (t2.Decoder |> Decoder.run)))
                |> mergeMapUnsafe (JsonNode.UnwrapRecord (NonEmptyList.getOnlyElement (t3.Decoder |> Decoder.run)))
                |> mergeMapUnsafe (JsonNode.UnwrapRecord (NonEmptyList.getOnlyElement (t4.Decoder |> Decoder.run)))
                |> JsonNode.Record
                |> NonEmptyList.singleton
                |> Decoder
        }

    member _.MergeSources
        (t1: Codec<PropertyList<'Encoding>,PropertyList<'Encoding>, 'a, 's>, t2: Codec<PropertyList<'Encoding>, PropertyList<'Encoding>, 'b, 's>, t3: Codec<PropertyList<'Encoding>, PropertyList<'Encoding>, 'c, 's>, t4: Codec<PropertyList<'Encoding>, PropertyList<'Encoding>, 'd, 's>, t5: Codec<PropertyList<'Encoding>, PropertyList<'Encoding>, 'e, 's>)
        : Codec<PropertyList<'Encoding>, PropertyList<'Encoding>, 'a * 'b * 'c * 'd * 'e, 's> =
        unsafeCheckConsistencyOfCodecBuildingPhase t1
        unsafeCheckConsistencyOfCodecBuildingPhase t2
        unsafeCheckConsistencyOfCodecBuildingPhase t3
        unsafeCheckConsistencyOfCodecBuildingPhase t4
        unsafeCheckConsistencyOfCodecBuildingPhase t5
        // merge five maps of encoder and decoder
        {
            Encoder =
                JsonNode.UnwrapRecord t1.Encoder
                |> mergeMapUnsafe (JsonNode.UnwrapRecord t2.Encoder)
                |> mergeMapUnsafe (JsonNode.UnwrapRecord t3.Encoder)
                |> mergeMapUnsafe (JsonNode.UnwrapRecord t4.Encoder)
                |> mergeMapUnsafe (JsonNode.UnwrapRecord t5.Encoder)
                |> JsonNode.Record
            Decoder =
                (JsonNode.UnwrapRecord (NonEmptyList.getOnlyElement (t1.Decoder |> Decoder.run)))
                |> mergeMapUnsafe (JsonNode.UnwrapRecord (NonEmptyList.getOnlyElement (t2.Decoder |> Decoder.run)))
                |> mergeMapUnsafe (JsonNode.UnwrapRecord (NonEmptyList.getOnlyElement (t3.Decoder |> Decoder.run)))
                |> mergeMapUnsafe (JsonNode.UnwrapRecord (NonEmptyList.getOnlyElement (t4.Decoder |> Decoder.run)))
                |> mergeMapUnsafe (JsonNode.UnwrapRecord (NonEmptyList.getOnlyElement (t5.Decoder |> Decoder.run)))
                |> JsonNode.Record
                |> NonEmptyList.singleton
                |> Decoder
        }

    member _.MergeSources
        (t1: Codec<PropertyList<'Encoding>,PropertyList<'Encoding>, 'a, 's>, t2: Codec<PropertyList<'Encoding>, PropertyList<'Encoding>, 'b, 's>, t3: Codec<PropertyList<'Encoding>, PropertyList<'Encoding>, 'c, 's>, t4: Codec<PropertyList<'Encoding>, PropertyList<'Encoding>, 'd, 's>, t5: Codec<PropertyList<'Encoding>, PropertyList<'Encoding>, 'e, 's>, t6: Codec<PropertyList<'Encoding>, PropertyList<'Encoding>, 'f, 's>)
        : Codec<PropertyList<'Encoding>, PropertyList<'Encoding>, 'a * 'b * 'c * 'd * 'e * 'f, 's> =
        unsafeCheckConsistencyOfCodecBuildingPhase t1
        unsafeCheckConsistencyOfCodecBuildingPhase t2
        unsafeCheckConsistencyOfCodecBuildingPhase t3
        unsafeCheckConsistencyOfCodecBuildingPhase t4
        unsafeCheckConsistencyOfCodecBuildingPhase t5
        unsafeCheckConsistencyOfCodecBuildingPhase t6
        // merge six maps of encoder and decoder
        {
            Encoder =
                JsonNode.UnwrapRecord t1.Encoder
                |> mergeMapUnsafe (JsonNode.UnwrapRecord t2.Encoder)
                |> mergeMapUnsafe (JsonNode.UnwrapRecord t3.Encoder)
                |> mergeMapUnsafe (JsonNode.UnwrapRecord t4.Encoder)
                |> mergeMapUnsafe (JsonNode.UnwrapRecord t5.Encoder)
                |> mergeMapUnsafe (JsonNode.UnwrapRecord t6.Encoder)
                |> JsonNode.Record
            Decoder =
                (JsonNode.UnwrapRecord (NonEmptyList.getOnlyElement (t1.Decoder |> Decoder.run)))
                |> mergeMapUnsafe (JsonNode.UnwrapRecord (NonEmptyList.getOnlyElement (t2.Decoder |> Decoder.run)))
                |> mergeMapUnsafe (JsonNode.UnwrapRecord (NonEmptyList.getOnlyElement (t3.Decoder |> Decoder.run)))
                |> mergeMapUnsafe (JsonNode.UnwrapRecord (NonEmptyList.getOnlyElement (t4.Decoder |> Decoder.run)))
                |> mergeMapUnsafe (JsonNode.UnwrapRecord (NonEmptyList.getOnlyElement (t5.Decoder |> Decoder.run)))
                |> mergeMapUnsafe (JsonNode.UnwrapRecord (NonEmptyList.getOnlyElement (t6.Decoder |> Decoder.run)))
                |> JsonNode.Record
                |> NonEmptyList.singleton
                |> Decoder
        }

    member _.BindReturn(x: Codec<PropertyList<'Encoding>,PropertyList<'Encoding>, 't1, 't2>, _f: 't1 -> 't2) : Codec<PropertyList<'Encoding>,PropertyList<'Encoding>, 't2, 't2> =
        // apply encoder logic , here simply re type the encoder and decoder
        {
            Encoder = x.Encoder
            Decoder = Decoder.run x.Decoder |> Decoder
        }

    member _.Run x : Codec<PropertyList<'Encoding>, 't> = x

let codec = CodecApplicativeBuilder()

let reqWith
    (c: Codec<'Encoding, _, _, 'Value>)
    (prop: string)
    (_getter: 'T -> 'Value option)
    : Codec<PropertyList<'Encoding>, PropertyList<'Encoding>, 'Value, 'T> =
    {
        Encoder = JsonNode.Record (Map.ofList [prop, c.Encoder])
        Decoder =
            [prop, makeAnyOneOf (Decoder.run c.Decoder)]
            |> Map.ofList
            |> JsonNode.Record
            |> NonEmptyList.singleton
            |> Decoder
    }


let reqWithLazy (c: unit -> Codec<'RawEncoding, _, _, 'Value>) (prop: string) (_getter: 'T -> 'Value option) : Codec<PropertyList<'Encoding>, PropertyList<'Encoding>, 'Value, 'T> =
    let codec = c ()
    {
        Encoder = JsonNode.Record (Map.ofList [prop, (codec).Encoder])
        Decoder =
            [prop, makeAnyOneOf (Decoder.run (codec).Decoder)]
            |> Map.ofList
            |> JsonNode.Record
            |> NonEmptyList.singleton
            |> Decoder
    }

let ofObjCodec (x: Codec<PropertyList<'Encoding>, 'T>) : Codec<'Encoding, 'T> =
    // finalize current codec building phase
    {
        Encoder = x.Encoder
        Decoder = x.Decoder |> Decoder.run |> Decoder
    }

let optWith (c: Codec<'Encoding, 'Value>) (prop: string) (_getter: 'T -> 'Value option) : Codec<PropertyList<'Encoding>, PropertyList<'Encoding>, Option<'Value>, 'T> =
    // prop might be unavailable while decoding
    {
        Encoder = JsonNode.Record (Map.ofList [prop, JsonNode.OptWithOption c.Encoder])
        Decoder =
            [prop, JsonNode.OptWithOption (makeAnyOneOf (Decoder.run c.Decoder))]
            |> Map.ofList
            |> JsonNode.Record
            |> NonEmptyList.singleton
            |> Decoder
    }

let reqDecoderWith (dec: Decoder<'Encoding, 'Value>) (prop: string) : Decoder<PropertyList<'Encoding>, 'Value> =
    [(prop, makeAnyOneOf (Decoder.run dec))]
    |> Map.ofList
    |> JsonNode.Record
    |> NonEmptyList.singleton
    |> Decoder

let reqDecodeWithCodec (codec: Codec<'Encoding, 'Value>) (prop: string) : Decoder<PropertyList<'Encoding>, 'Value> =
    reqDecoderWith codec.Decoder (prop: string)

let optDecoderWith (dec: Decoder<'Encoding, Option<'Value>>) (prop: string) : Decoder<PropertyList<'Encoding>, Option<'Value>> =
    let decodeNodes =
        dec |> Decoder.run
    // make all node JsonNode.Option to JsonNode.OptWithOption
    decodeNodes |> NonEmptyList.map (function
        | JsonNode.Option x -> JsonNode.OptWithOption x
        | x ->
            failwithf $"Unwanted state: (Decoder node of Option<'Value> is not JsonNode.Option) of decoder in builder phase: %A{x}"
    )
    |> fun nodes ->
        [(prop, makeAnyOneOf nodes)]
        |> Map.ofList
        |> JsonNode.Record
        |> NonEmptyList.singleton
        |> Decoder

let optDecodeWithCodec (codec: Codec<'Encoding, 'Value>) (prop: string) : Decoder<PropertyList<'Encoding>, Option<'Value>> =
    optDecoderWith (Codecs.option codec).Decoder (prop: string)

let combineDecoders (source: Decoder<PropertyList<'Encoding>, 't>) (alternative: Decoder<PropertyList<'Encoding>, 't>) : Decoder<PropertyList<'Encoding>, 't> =
    (Decoder.run source) ++ (Decoder.run alternative) |> Decoder

let ofObjDecoder (objCodec: Decoder<PropertyList<'Encoding>, 't>) : Decoder<'Encoding, 't> =
    objCodec
    |> Decoder.run
    |> Decoder

type DecoderApplicativeBuilder() =
    member _.Delay x = x ()
    member _.ReturnFrom expr = expr

    member _.MergeSources(t1: Decoder<PropertyList<'Encoding>, 't1>, t2: Decoder<PropertyList<'Encoding>, 't2>) : Decoder<PropertyList<'Encoding>, 't1 * 't2> =
        unsafeCheckConsistencyOfDecoderBuildingPhase t1
        unsafeCheckConsistencyOfDecoderBuildingPhase t2

        (t1 |> Decoder.run |> NonEmptyList.getOnlyElement |> JsonNode.UnwrapRecord)
        |> mergeMapUnsafe (t2 |> Decoder.run |> NonEmptyList.getOnlyElement |> JsonNode.UnwrapRecord)
        |> JsonNode.Record
        |> NonEmptyList.singleton
        |> Decoder

    member _.MergeSources(t1: Decoder<PropertyList<'Encoding>, 't1>, t2: Decoder<PropertyList<'Encoding>, 't2>, t3: Decoder<PropertyList<'Encoding>, 't3>) : Decoder<PropertyList<'Encoding>, 't1 * 't2 * 't3> =
        unsafeCheckConsistencyOfDecoderBuildingPhase t1
        unsafeCheckConsistencyOfDecoderBuildingPhase t2
        unsafeCheckConsistencyOfDecoderBuildingPhase t3

        (t1 |> Decoder.run |> NonEmptyList.getOnlyElement |> JsonNode.UnwrapRecord)
        |> mergeMapUnsafe (t2 |> Decoder.run |> NonEmptyList.getOnlyElement |> JsonNode.UnwrapRecord)
        |> mergeMapUnsafe (t3 |> Decoder.run |> NonEmptyList.getOnlyElement |> JsonNode.UnwrapRecord)
        |> JsonNode.Record
        |> NonEmptyList.singleton
        |> Decoder

    member  _.MergeSources(t1: Decoder<PropertyList<'Encoding>, 't1>, t2: Decoder<PropertyList<'Encoding>, 't2>, t3: Decoder<PropertyList<'Encoding>, 't3>, t4: Decoder<PropertyList<'Encoding>, 't4>) : Decoder<PropertyList<'Encoding>, 't1 * 't2 * 't3 * 't4> =
        unsafeCheckConsistencyOfDecoderBuildingPhase t1
        unsafeCheckConsistencyOfDecoderBuildingPhase t2
        unsafeCheckConsistencyOfDecoderBuildingPhase t3
        unsafeCheckConsistencyOfDecoderBuildingPhase t4

        (t1 |> Decoder.run |> NonEmptyList.getOnlyElement |> JsonNode.UnwrapRecord)
        |> mergeMapUnsafe (t2 |> Decoder.run |> NonEmptyList.getOnlyElement |> JsonNode.UnwrapRecord)
        |> mergeMapUnsafe (t3 |> Decoder.run |> NonEmptyList.getOnlyElement |> JsonNode.UnwrapRecord)
        |> mergeMapUnsafe (t4 |> Decoder.run |> NonEmptyList.getOnlyElement |> JsonNode.UnwrapRecord)
        |> JsonNode.Record
        |> NonEmptyList.singleton
        |> Decoder

    member  _.MergeSources(t1: Decoder<PropertyList<'Encoding>, 't1>, t2: Decoder<PropertyList<'Encoding>, 't2>, t3: Decoder<PropertyList<'Encoding>, 't3>, t4: Decoder<PropertyList<'Encoding>, 't4>, t5: Decoder<PropertyList<'Encoding>, 't5>) : Decoder<PropertyList<'Encoding>, 't1 * 't2 * 't3 * 't4 * 't5> =
        unsafeCheckConsistencyOfDecoderBuildingPhase t1
        unsafeCheckConsistencyOfDecoderBuildingPhase t2
        unsafeCheckConsistencyOfDecoderBuildingPhase t3
        unsafeCheckConsistencyOfDecoderBuildingPhase t4
        unsafeCheckConsistencyOfDecoderBuildingPhase t5

        (t1 |> Decoder.run |> NonEmptyList.getOnlyElement |> JsonNode.UnwrapRecord)
        |> mergeMapUnsafe (t2 |> Decoder.run |> NonEmptyList.getOnlyElement |> JsonNode.UnwrapRecord)
        |> mergeMapUnsafe (t3 |> Decoder.run |> NonEmptyList.getOnlyElement |> JsonNode.UnwrapRecord)
        |> mergeMapUnsafe (t4 |> Decoder.run |> NonEmptyList.getOnlyElement |> JsonNode.UnwrapRecord)
        |> mergeMapUnsafe (t5 |> Decoder.run |> NonEmptyList.getOnlyElement |> JsonNode.UnwrapRecord)
        |> JsonNode.Record
        |> NonEmptyList.singleton
        |> Decoder

    member  _.MergeSources(t1: Decoder<PropertyList<'Encoding>, 't1>, t2: Decoder<PropertyList<'Encoding>, 't2>, t3: Decoder<PropertyList<'Encoding>, 't3>, t4: Decoder<PropertyList<'Encoding>, 't4>, t5: Decoder<PropertyList<'Encoding>, 't5>, t6: Decoder<PropertyList<'Encoding>, 't6>) : Decoder<PropertyList<'Encoding>, 't1 * 't2 * 't3 * 't4 * 't5 * 't6> =
        unsafeCheckConsistencyOfDecoderBuildingPhase t1
        unsafeCheckConsistencyOfDecoderBuildingPhase t2
        unsafeCheckConsistencyOfDecoderBuildingPhase t3
        unsafeCheckConsistencyOfDecoderBuildingPhase t4
        unsafeCheckConsistencyOfDecoderBuildingPhase t5
        unsafeCheckConsistencyOfDecoderBuildingPhase t6

        (t1 |> Decoder.run |> NonEmptyList.getOnlyElement |> JsonNode.UnwrapRecord)
        |> mergeMapUnsafe (t2 |> Decoder.run |> NonEmptyList.getOnlyElement |> JsonNode.UnwrapRecord)
        |> mergeMapUnsafe (t3 |> Decoder.run |> NonEmptyList.getOnlyElement |> JsonNode.UnwrapRecord)
        |> mergeMapUnsafe (t4 |> Decoder.run |> NonEmptyList.getOnlyElement |> JsonNode.UnwrapRecord)
        |> mergeMapUnsafe (t5 |> Decoder.run |> NonEmptyList.getOnlyElement |> JsonNode.UnwrapRecord)
        |> mergeMapUnsafe (t6 |> Decoder.run |> NonEmptyList.getOnlyElement |> JsonNode.UnwrapRecord)
        |> JsonNode.Record
        |> NonEmptyList.singleton
        |> Decoder

    member  _.MergeSources(t1: Decoder<PropertyList<'Encoding>, 't1>, t2: Decoder<PropertyList<'Encoding>, 't2>, t3: Decoder<PropertyList<'Encoding>, 't3>, t4: Decoder<PropertyList<'Encoding>, 't4>, t5: Decoder<PropertyList<'Encoding>, 't5>, t6: Decoder<PropertyList<'Encoding>, 't6>, t7: Decoder<PropertyList<'Encoding>, 't7>) : Decoder<PropertyList<'Encoding>, 't1 * 't2 * 't3 * 't4 * 't5 * 't6 * 't7> =
        unsafeCheckConsistencyOfDecoderBuildingPhase t1
        unsafeCheckConsistencyOfDecoderBuildingPhase t2
        unsafeCheckConsistencyOfDecoderBuildingPhase t3
        unsafeCheckConsistencyOfDecoderBuildingPhase t4
        unsafeCheckConsistencyOfDecoderBuildingPhase t5
        unsafeCheckConsistencyOfDecoderBuildingPhase t6
        unsafeCheckConsistencyOfDecoderBuildingPhase t7

        (t1 |> Decoder.run |> NonEmptyList.getOnlyElement |> JsonNode.UnwrapRecord)
        |> mergeMapUnsafe (t2 |> Decoder.run |> NonEmptyList.getOnlyElement |> JsonNode.UnwrapRecord)
        |> mergeMapUnsafe (t3 |> Decoder.run |> NonEmptyList.getOnlyElement |> JsonNode.UnwrapRecord)
        |> mergeMapUnsafe (t4 |> Decoder.run |> NonEmptyList.getOnlyElement |> JsonNode.UnwrapRecord)
        |> mergeMapUnsafe (t5 |> Decoder.run |> NonEmptyList.getOnlyElement |> JsonNode.UnwrapRecord)
        |> mergeMapUnsafe (t6 |> Decoder.run |> NonEmptyList.getOnlyElement |> JsonNode.UnwrapRecord)
        |> mergeMapUnsafe (t7 |> Decoder.run |> NonEmptyList.getOnlyElement |> JsonNode.UnwrapRecord)
        |> JsonNode.Record
        |> NonEmptyList.singleton
        |> Decoder


    member _.BindReturn(x: Decoder<PropertyList<'Encoding>, 't1>, _f: 't1 -> 't2) : Decoder<PropertyList<'Encoding>, 't2> =
        x
        |> Decoder.run
        |> Decoder

    member _.Run x : Decoder<PropertyList<'Encoding>, 't> = x


let decoder = DecoderApplicativeBuilder()

type IDefault8 = interface end
type IDefault7 = interface inherit IDefault8 end
type IDefault6 = interface inherit IDefault7 end
type IDefault5 = interface inherit IDefault6 end
type IDefault4 = interface inherit IDefault5 end
type IDefault3 = interface inherit IDefault4 end
type IDefault2 = interface inherit IDefault3 end
type IDefault1 = interface inherit IDefault2 end
type IDefault0 = interface inherit IDefault1 end

type GetCodec =
    interface IDefault0

    static member GetCodec (_: bool          , _: GetCodec, _) : Codec<'Encoding, _> = Codecs.boolean
    static member GetCodec (_: string        , _: GetCodec, _) : Codec<'Encoding, _> = Codecs.string
    static member GetCodec (_: DateOnly      , _: GetCodec, _) : Codec<'Encoding, _> = Codecs.date
    static member GetCodec (_: TimeOnly      , _: GetCodec, _) : Codec<'Encoding, _> = Codecs.time
    static member GetCodec (_: DateTimeOffset, _: GetCodec, _) : Codec<'Encoding, _> = Codecs.dateTimeOffset
    static member GetCodec (_: TimeSpan      , _: GetCodec, _) : Codec<'Encoding, _> = Codecs.timeSpan
    static member GetCodec (_: decimal       , _: GetCodec, _) : Codec<'Encoding, _> = Codecs.decimal
    static member GetCodec (_: Double        , _: GetCodec, _) : Codec<'Encoding, _> = Codecs.float
    static member GetCodec (_: Single        , _: GetCodec, _) : Codec<'Encoding, _> = Codecs.float32
    static member GetCodec (_: int           , _: GetCodec, _) : Codec<'Encoding, _> = Codecs.int
    static member GetCodec (_: uint32        , _: GetCodec, _) : Codec<'Encoding, _> = Codecs.uint32
    static member GetCodec (_: int64         , _: GetCodec, _) : Codec<'Encoding, _> = Codecs.int64
    static member GetCodec (_: uint64        , _: GetCodec, _) : Codec<'Encoding, _> = Codecs.uint64
    static member GetCodec (_: int16         , _: GetCodec, _) : Codec<'Encoding, _> = Codecs.int16
    static member GetCodec (_: uint16        , _: GetCodec, _) : Codec<'Encoding, _> = Codecs.uint16
    static member GetCodec (_: byte          , _: GetCodec, _) : Codec<'Encoding, _> = Codecs.byte
    static member GetCodec (_: char          , _: GetCodec, _) : Codec<'Encoding, _> = Codecs.char
    static member GetCodec (_: Guid          , _: GetCodec, _) : Codec<'Encoding, _> = Codecs.guid
    static member GetCodec (()               , _: GetCodec, _) : Codec<'Encoding, _> = Codecs.unit
    static member inline Invoke<'Encoding, .. when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding)> (x: ^t) : Codec<'Encoding, 't>  =
        let inline call (a: ^a, b: ^b) = ((^a or ^b) : (static member GetCodec: ^b * ^a * ^a -> Codec<'Encoding, ^t>) b, a, a)
        call (Unchecked.defaultof<GetCodec>, x)

type GetCodec with
    static member inline GetCodec (_: Result<'a, 'b> when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding), _: GetCodec, _) : Codec<'Encoding, Result<'a,'b>> =
        (fun () -> Codecs.result (GetCodec.Invoke<'Encoding, 'a> Unchecked.defaultof<'a>) (GetCodec.Invoke<'Encoding, 'b> Unchecked.defaultof<'b>))
        |> CodecCache<'Encoding, Result<'a, 'b>>.Run

type GetCodec with
    static member inline GetCodec (_: Choice<'a, 'b> when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding), _: GetCodec, _) : Codec<'Encoding, Choice<'a,'b>> =
        (fun () -> Codecs.choice (GetCodec.Invoke<'Encoding, 'a> Unchecked.defaultof<'a>) (GetCodec.Invoke<'Encoding, 'b> Unchecked.defaultof<'b>))
        |> CodecCache<'Encoding, Choice<'a, 'b>>.Run

type GetCodec with
    static member inline GetCodec (_: 'a option   when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding), _: GetCodec, _) : Codec<'Encoding, option<'a>>   =
            (fun () -> Codecs.option (GetCodec.Invoke<'Encoding, _> Unchecked.defaultof<'a>))
            |> CodecCache<'Encoding, option<'a>>.Run

    static member inline GetCodec(_: 'a * 'b when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding), _: IDefault8, _) : Codec<'Encoding, 'a * 'b> =
        (fun () -> Codecs.tuple2 (GetCodec.Invoke<'Encoding, _> Unchecked.defaultof<'a>) (GetCodec.Invoke<'Encoding, _> Unchecked.defaultof<'b>))
        |> CodecCache<'Encoding, 'a * 'b>.Run

    static member inline GetCodec(_: 'a * 'b * 'c when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding), _: IDefault8, _) : Codec<'Encoding, 'a * 'b * 'c> =
        (fun () -> Codecs.tuple3 (GetCodec.Invoke<'Encoding, _> Unchecked.defaultof<'a>) (GetCodec.Invoke<'Encoding, _> Unchecked.defaultof<'b>) (GetCodec.Invoke<'Encoding, _> Unchecked.defaultof<'c>))
        |> CodecCache<'Encoding, 'a * 'b * 'c>.Run

    static member inline GetCodec(_: 'a * 'b * 'c * 'd when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding), _: IDefault8, _) : Codec<'Encoding, 'a * 'b * 'c * 'd> =
        (fun () -> Codecs.tuple4 (GetCodec.Invoke<'Encoding, _> Unchecked.defaultof<'a>) (GetCodec.Invoke<'Encoding, _> Unchecked.defaultof<'b>) (GetCodec.Invoke<'Encoding, _> Unchecked.defaultof<'c>) (GetCodec.Invoke<'Encoding, _> Unchecked.defaultof<'d>))
        |> CodecCache<'Encoding, 'a * 'b * 'c * 'd>.Run

    static member inline GetCodec(_: 'a * 'b * 'c * 'd * 'e when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding), _: IDefault8, _) : Codec<'Encoding, 'a * 'b * 'c * 'd * 'e> =
        (fun () -> Codecs.tuple5 (GetCodec.Invoke<'Encoding, _> Unchecked.defaultof<'a>) (GetCodec.Invoke<'Encoding, _> Unchecked.defaultof<'b>) (GetCodec.Invoke<'Encoding, _> Unchecked.defaultof<'c>) (GetCodec.Invoke<'Encoding, _> Unchecked.defaultof<'d>) (GetCodec.Invoke<'Encoding, _> Unchecked.defaultof<'e>))
        |> CodecCache<'Encoding, 'a * 'b * 'c * 'd * 'e>.Run

    static member inline GetCodec(_: 'a * 'b * 'c * 'd * 'e * 'f when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding), _: IDefault8, _) : Codec<'Encoding, 'a * 'b * 'c * 'd * 'e * 'f> =
        (fun () -> Codecs.tuple6 (GetCodec.Invoke<'Encoding, _> Unchecked.defaultof<'a>) (GetCodec.Invoke<'Encoding, _> Unchecked.defaultof<'b>) (GetCodec.Invoke<'Encoding, _> Unchecked.defaultof<'c>) (GetCodec.Invoke<'Encoding, _> Unchecked.defaultof<'d>) (GetCodec.Invoke<'Encoding, _> Unchecked.defaultof<'e>) (GetCodec.Invoke<'Encoding, _> Unchecked.defaultof<'f>))
        |> CodecCache<'Encoding, 'a * 'b * 'c * 'd * 'e * 'f>.Run

    static member inline GetCodec(_: 'a * 'b * 'c * 'd * 'e * 'f * 'g when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding), _: IDefault8, _) : Codec<'Encoding, 'a * 'b * 'c * 'd * 'e * 'f * 'g> =
        (fun () -> Codecs.tuple7 (GetCodec.Invoke<'Encoding, _> Unchecked.defaultof<'a>) (GetCodec.Invoke<'Encoding, _> Unchecked.defaultof<'b>) (GetCodec.Invoke<'Encoding, _> Unchecked.defaultof<'c>) (GetCodec.Invoke<'Encoding, _> Unchecked.defaultof<'d>) (GetCodec.Invoke<'Encoding, _> Unchecked.defaultof<'e>) (GetCodec.Invoke<'Encoding, _> Unchecked.defaultof<'f>) (GetCodec.Invoke<'Encoding, _> Unchecked.defaultof<'g>))
        |> CodecCache<'Encoding, 'a * 'b * 'c * 'd * 'e * 'f * 'g>.Run

type GetCodec with
    static member inline GetCodec (_: list<'a> when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding), _, _) : Codec<'Encoding, list<'a>> =
        (fun () -> Codecs.list (GetCodec.Invoke<'Encoding, _> Unchecked.defaultof<'a>))
        |> CodecCache<'Encoding, list<'a>>.Run


type GetCodec with
    static member inline GetCodec (_: 'Base when 'Base :> IInterfaceCodec<'Base>, _, _: IDefault2) : Codec<'Encoding, 'Base> when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding) =
        match CodecCollection<'Encoding, 'Base>.GetSubtypes |> NonEmptySeq.tryOfSeq with
        | None ->
            failwithf $"No codec registered for %s{typeof<'Base>.Name}"
        | Some cs ->
            fun () ->
                cs
                |> map (fun (KeyValue(_, x)) -> x ())
                |> choice
                |> ofObjCodec
            |> CodecCache<'Encoding, _>.RunForInterfaces
type GetCodec with
    static member inline GetCodec (_: 'T, _: IDefault3, _) : Codec<'Encoding, 'T> =
        fun () ->
            (^T : (static member Codec: Codec<_, ^T>) ())
        |> CodecCache<'Encoding, 'T>.Run

    static member inline GetCodec (_: 'T, _: IDefault4, _) =
        fun () ->
            let mutable r = Unchecked.defaultof<Codec< 'Encoding, 'T>>
            do (^T : (static member Codec : byref<Codec< 'Encoding, 'T>> -> unit) &r)
            r
        |> CodecCache<'Encoding, 'T>.Run

let inline codecFor<'Encoding, 'T when 'T : (static member Codec: Codec<'Encoding, 'T>) > =
    fun () ->
        (^T : (static member Codec: Codec< 'Encoding, ^T>) ())
    |> CodecCache<'Encoding, 'T>.Run

let inline defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<'Encoding, ^t when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding) and (GetCodec or ^t) : (static member GetCodec: ^t * GetCodec * GetCodec -> Codec<'Encoding, ^t>)> : Codec<'Encoding, ^t> =
    GetCodec.Invoke<'Encoding, 't> Unchecked.defaultof<'t>


module CodecsWithMeasure =
    let float(): Codec<_, float<'UnitOfMeasure>> =
        Codec.create
            (Ok << (fun (x: float) -> FSharp.Core.LanguagePrimitives.FloatWithMeasure x))
            (fun x -> float x)
        |> Codec.compose Codecs.float


open System.Reflection
open FSharp.Reflection


let inline mergeUnionCases (fn: 'Union -> '``Codec/Decoder<PropertyList<'Encoding>, 'Union>``) : '``Codec/Decoder<PropertyList<'Encoding>, 'Union>`` =
    FSharpType.GetUnionCases (typeof<'Union>, BindingFlags.NonPublic ||| BindingFlags.Public)
    |> NonEmptySeq.ofArray
    |> NonEmptySeq.map (fun c -> FSharpValue.MakeUnion (c, Array.zeroCreate (Array.length (c.GetFields ())), BindingFlags.NonPublic ||| BindingFlags.Public) :?> 'Union)
    |> NonEmptySeq.map fn
    |> choice

[<System.Obsolete("Use mergeUnionCases instead.")>]
let inline unionCodec x = mergeUnionCases x

let withDecoders (decoders: list<Decoder<PropertyList<'Encoding>, 't1>>) (codec: Codec<PropertyList<'Encoding>, PropertyList<'Encoding>, 't1, 't2>) : Codec<PropertyList<'Encoding>, PropertyList<'Encoding>, 't1, 't2> =
    // Ideally Codec<PropertyList<>> should have single record, But here mergeUnionCases might be called earlier before withDecoders.
    // In some ecosystem, Codec<PropertyList<>> might contain multiple record.
    // TODO: Sanity check, Encoder can erither single record or mulltiple record with AnyOneOf node. same for decoder
    // Encoder in codec can contain merged records
    // Decoder in codec can contain merged records
    {
        codec with
            Decoder =
                decoders
                |> List.collect (fun decoderPropList ->
                    decoderPropList
                    |> Decoder.run
                    |> NonEmptyList.toList
                )
                |> List.append (NonEmptyList.toList (codec.Decoder |> Decoder.run))
                |> NonEmptyList.ofList
                |> Decoder
    }

let rec attachPropertyToJsonNode (key: string) (valueNode: JsonNode) (jsonNode: JsonNode) =
    match jsonNode with
    | JsonNode.Terminal _
    | JsonNode.Choice _
    | JsonNode.Result _
    | JsonNode.Option _
    | JsonNode.Array _
    | JsonNode.Tuple _
    | JsonNode.Any _
    | JsonNode.OptWithOption _ ->
        failwithf $"Cannot attach property in current JsonNode {jsonNode}"
    | JsonNode.AnyOneOf nodes ->
        nodes
        |> NonEmptyList.map (attachPropertyToJsonNode key valueNode)
        |> makeAnyOneOf

    | JsonNode.Record record ->
        record.Add (key, valueNode)
        |> JsonNode.Record

let doubleEncode
    (latestEncoder: Codec<_, _>)
    (compatibleEncoder: Codec<_, _>) =
    latestEncoder <|> compatibleEncoder

let attachCodecTypeLabel (typeLabel: string) (toCodec: Codec<PropertyList<'Encoding>, 't>) : Codec<PropertyList<'Encoding>, 't> =
    {
        Encoder =
            attachPropertyToJsonNode typeLabel (JsonNode.Terminal TerminalJsonNode.Unit) toCodec.Encoder
        Decoder =
            toCodec.Decoder
            |> Decoder.run
            |> NonEmptyList.map (attachPropertyToJsonNode typeLabel (JsonNode.Terminal TerminalJsonNode.Unit))
            |> Decoder
    }

let attachDecoderTypeLabel (typeLabel: string) (toDecoder: Decoder<PropertyList<'Encoding>, 't>) : Decoder<PropertyList<'Encoding>, 't> =
    toDecoder
    |> Decoder.run
    |> NonEmptyList.map (attachPropertyToJsonNode typeLabel (JsonNode.Terminal TerminalJsonNode.Unit))
    |> Decoder


// Hack: To discover helper types using reflection
type HelperMethods = HelperMethods
    with
        static member getDecodersAsJsonNode (codec: Codec<'Encoding, 't>) : JsonNode =
            codec.Decoder
            |> Decoder.run
            |> makeAnyOneOf

        static member getJsonNodeFromCodecCollectionSubtypes (subtypes : Dictionary<Type, unit -> Codec<PropertyList<'Encoding>,'Interface>>) : JsonNode =
            (subtypes
            |> NonEmptySeq.ofSeq
            |> map (fun (KeyValue(_, x)) -> x ())
            |> choice
            |> ofObjCodec).Decoder
            |> Decoder.run
            |> makeAnyOneOf
