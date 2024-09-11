[<AutoOpen>]
module LibLifeCycleCore.ValueSummaryCodecs

#if !FABLE_COMPILER // START OF THE GLOBAL CODEC DISABLE FOR FABLE

// Provides a succinct summary of a value, optimized for high-perf production use-cases, such as logging

open System
open CodecLib
open System.Text

let private totalLimit = 500
let dec x = Codec.decode x
let enc x = Codec.encode x

type StringBuilder
    with
        member this.Write (c: char) : bool =
            if (this.Length >= totalLimit) then
                false
            elif this.Length < totalLimit - 2 then
                this.Append c |> ignore
                true
            else
                this.Append ".." |> ignore
                false

        member this.Write (s: string) : bool =
            if (this.Length >= totalLimit) then
                false
            elif this.Length + s.Length >= totalLimit - 2 then
                this.Append (s.Substring(0, totalLimit - this.Length - 2)) |> ignore
                this.Append ".." |> ignore
                false
            else
                this.Append s |> ignore
                true

        member this.WriteIndent (depth: int) : bool =
            if depth = 0 && this.Length = 0 then
                true
            else
                this.Write '\n' && this.Write (new string (' ', depth))


open FSharpPlus.Data


[<RequireQualifiedAccess; Struct>]
type ValueSummaryEncoding =
| Atom of Atom: string
| Obj  of ObjMaxDepthBelow: int * Obj: array<string * ValueSummaryEncoding>
| Arr  of ArrMaxDepthBelow: int * Arr: array<ValueSummaryEncoding>

with

    static member MaxDepthBelow =
        function
        | ValueSummaryEncoding.Atom _ ->
            0
        | ValueSummaryEncoding.Obj (maxDepthBelow, _)
        | ValueSummaryEncoding.Arr (maxDepthBelow, _) ->
            maxDepthBelow

    static member SAndXMore (count: int) = ValueSummaryEncoding.Atom (sprintf "..+%d" count)

    static member inline SArray (x: ValueSummaryEncoding[]) =
        let depthBelow =
            if x.Length = 0 then 0
            else x |> Seq.map ValueSummaryEncoding.MaxDepthBelow |> Seq.max
        ValueSummaryEncoding.Arr (depthBelow + 1, x)

    static member inline SObject (x: seq<string * ValueSummaryEncoding>) =
        let arr =
            x
            // TODO: consider truncate long property names - use ReadOnlySpan<char> (values too ?)
            //|> Seq.map (fun ((k, v) as kvp) -> if k.Length <= 15 then kvp else ((k.Substring (0, 14)) + "+", v))
            |> Array.ofSeq
        let depthBelow =
            if arr.Length = 0 then 0
            else arr |> Seq.map (snd >> ValueSummaryEncoding.MaxDepthBelow) |> Seq.max

        ValueSummaryEncoding.Obj (depthBelow + 1, arr)

    static member SNull                = ValueSummaryEncoding.Atom "<NULL>"
    static member SString (x: string)  = if isNull x then ValueSummaryEncoding.SNull else (ValueSummaryEncoding.Atom x)

    /// Creates a new summary object for serialization
    static member sobj x = ValueSummaryEncoding.SObject (x |> Seq.filter (fun (k,_) -> not (isNull k)))


    static member toIRawCodec (c : Codec<ValueSummaryEncoding, 't>)  : Codec<IRawEncoding, 't> = Codec.create Unchecked.defaultof<_> (fun x -> ((c.Encoder >> Const.run) x) :> IRawEncoding)
    static member ofIRawCodec (c : Codec<IRawEncoding, 't>)  : Codec<ValueSummaryEncoding, 't> = Codec.create Unchecked.defaultof<_> (fun x -> ((c.Encoder >> Const.run) x) :?> ValueSummaryEncoding)

    static member Instance = ValueSummaryEncoding.SNull

    /// Unwraps the ValueSummaryEncoding inside an IRawEncoding
    static member Unwrap (x: IRawEncoding) = x :?> ValueSummaryEncoding

    //////////////
    // Encoders //
    //////////////

    static member redactForAuditEncode (encoder: 'a -> ValueSummaryEncoding) (value: 'a) =
        if not <| typeof<'a>.IsValueType then // perf optimization, only non-value types can implement the interface
            match (box value) with
            | :? IRedactable as redactable -> redactable.Redact() |> unbox
            | _ -> value
        else
            value

        |> encoder

    static member resultE (encoder1: _ -> ValueSummaryEncoding) (encoder2: _ -> ValueSummaryEncoding) = function
        | Ok    a -> ValueSummaryEncoding.sobj [ "Ok"   , ValueSummaryEncoding.redactForAuditEncode encoder1 a ]
        | Error a -> ValueSummaryEncoding.sobj [ "Error", ValueSummaryEncoding.redactForAuditEncode encoder2 a ]

    static member choiceE (encoder1: _ -> ValueSummaryEncoding) (encoder2: _ -> ValueSummaryEncoding) = function
        | Choice1Of2 a -> ValueSummaryEncoding.sobj [ "Choice1Of2", ValueSummaryEncoding.redactForAuditEncode encoder1 a ]
        | Choice2Of2 a -> ValueSummaryEncoding.sobj [ "Choice2Of2", ValueSummaryEncoding.redactForAuditEncode encoder2 a ]

    static member choice3E (encoder1: _ -> ValueSummaryEncoding) (encoder2: _ -> ValueSummaryEncoding) (encoder3: _ -> ValueSummaryEncoding) = function
        | Choice1Of3 a -> ValueSummaryEncoding.sobj [ "Choice1Of3", ValueSummaryEncoding.redactForAuditEncode encoder1 a ]
        | Choice2Of3 a -> ValueSummaryEncoding.sobj [ "Choice2Of3", ValueSummaryEncoding.redactForAuditEncode encoder2 a ]
        | Choice3Of3 a -> ValueSummaryEncoding.sobj [ "Choice3Of3", ValueSummaryEncoding.redactForAuditEncode encoder3 a ]

    static member optionE (encoder: _ -> ValueSummaryEncoding) = function
        | None   -> ValueSummaryEncoding.SNull
        | Some a -> ValueSummaryEncoding.redactForAuditEncode encoder a

    static member arrayE    (encoder: _ -> ValueSummaryEncoding) (x: 'a [])        =
            let limit = 3
            ValueSummaryEncoding.SArray (
                if x.Length <= limit then
                    (x |> Array.map (ValueSummaryEncoding.redactForAuditEncode encoder))
                else
                    (x |> Seq.take limit |> Seq.map (ValueSummaryEncoding.redactForAuditEncode encoder) |> Seq.append [ValueSummaryEncoding.SAndXMore (x.Length - limit)] |> Array.ofSeq))

    static member multiMapE (encoder: 'a -> ValueSummaryEncoding) (x: PropertyList<'a>) =
        let limit = 3
        // exclude nulls & system fields such as version or type marker.
        let filteredProps = x |> PropertyList.ToList |> List.filter (fun (k, _) -> not (isNull k || k.StartsWith "__v" || k.StartsWith "__type_"))
        let len = filteredProps.Length
        if len <= limit then
            filteredProps
        else
            filteredProps |> List.take limit
        |> Seq.map (fun (k, v) -> k, encoder v)
        |> Seq.append (if len > limit then ["*", ValueSummaryEncoding.Atom (sprintf "+%d" (len-limit))] else List.empty)
        |> ValueSummaryEncoding.SObject

            static member tuple1E (encoder1: 'a -> ValueSummaryEncoding) (a: Tuple<_>) = ValueSummaryEncoding.SArray ([|ValueSummaryEncoding.redactForAuditEncode encoder1 a.Item1|])
            static member tuple2E (encoder1: 'a -> ValueSummaryEncoding) (encoder2: 'b -> ValueSummaryEncoding) (a, b) = ValueSummaryEncoding.SArray ([|ValueSummaryEncoding.redactForAuditEncode encoder1 a; ValueSummaryEncoding.redactForAuditEncode encoder2 b|])
            static member tuple3E (encoder1: 'a -> ValueSummaryEncoding) (encoder2: 'b -> ValueSummaryEncoding) (encoder3: 'c -> ValueSummaryEncoding) (a, b, c) = ValueSummaryEncoding.SArray ([|ValueSummaryEncoding.redactForAuditEncode encoder1 a; ValueSummaryEncoding.redactForAuditEncode encoder2 b; ValueSummaryEncoding.redactForAuditEncode encoder3 c|])
            static member tuple4E (encoder1: 'a -> ValueSummaryEncoding) (encoder2: 'b -> ValueSummaryEncoding) (encoder3: 'c -> ValueSummaryEncoding) (_encoder4: 'd -> ValueSummaryEncoding) (a, b, c, _d) = ValueSummaryEncoding.SArray ([|ValueSummaryEncoding.SAndXMore 1; ValueSummaryEncoding.redactForAuditEncode encoder1 a; ValueSummaryEncoding.redactForAuditEncode encoder2 b; ValueSummaryEncoding.redactForAuditEncode encoder3 c |])
            static member tuple5E (encoder1: 'a -> ValueSummaryEncoding) (encoder2: 'b -> ValueSummaryEncoding) (encoder3: 'c -> ValueSummaryEncoding) (_encoder4: 'd -> ValueSummaryEncoding) (_encoder5: 'e -> ValueSummaryEncoding) (a, b, c, _d, _e) = ValueSummaryEncoding.SArray ([| ValueSummaryEncoding.SAndXMore 2; ValueSummaryEncoding.redactForAuditEncode encoder1 a; ValueSummaryEncoding.redactForAuditEncode encoder2 b; ValueSummaryEncoding.redactForAuditEncode encoder3 c |])
            static member tuple6E (encoder1: 'a -> ValueSummaryEncoding) (encoder2: 'b -> ValueSummaryEncoding) (encoder3: 'c -> ValueSummaryEncoding) (_encoder4: 'd -> ValueSummaryEncoding) (_encoder5: 'e -> ValueSummaryEncoding) (_encoder6: 'f -> ValueSummaryEncoding) (a, b, c, _d, _e, _f) = ValueSummaryEncoding.SArray ([| ValueSummaryEncoding.SAndXMore 3; ValueSummaryEncoding.redactForAuditEncode encoder1 a; ValueSummaryEncoding.redactForAuditEncode encoder2 b; ValueSummaryEncoding.redactForAuditEncode encoder3 c |])
            static member tuple7E (encoder1: 'a -> ValueSummaryEncoding) (encoder2: 'b -> ValueSummaryEncoding) (encoder3: 'c -> ValueSummaryEncoding) (_encoder4: 'd -> ValueSummaryEncoding) (_encoder5: 'e -> ValueSummaryEncoding) (_encoder6: 'f -> ValueSummaryEncoding) (_encoder7: 'g -> ValueSummaryEncoding) (a, b, c, _d, _e, _f, _g) = ValueSummaryEncoding.SArray ([| ValueSummaryEncoding.SAndXMore 4; ValueSummaryEncoding.redactForAuditEncode encoder1 a; ValueSummaryEncoding.redactForAuditEncode encoder2 b; ValueSummaryEncoding.redactForAuditEncode encoder3 c |])

            static member enumE (x: 't when 't : enum<_>) = ValueSummaryEncoding.SString (string x)
            static member unitE () = ValueSummaryEncoding.Arr (0, Array.empty)

    static member booleanE        (x: bool          ) = ValueSummaryEncoding.Atom (if x then "T" else "F")
    static member stringE         (x: string        ) = ValueSummaryEncoding.SString
                                                            // truncate long strings, avoid newlines
                                                            ((if x = null then "null" elif x.Length < 50 then x else x.Substring(0, 50)).Replace("\r\n", " ").Replace('\n', ' '))
    static member dateTimeE       (x: DateTime      ) = ValueSummaryEncoding.SString (x.ToString ("yyyyMMdd|HH:mm:ssZ"))
    static member dateTimeOffsetE (x: DateTimeOffset) = ValueSummaryEncoding.SString (x.ToString ("yyyyMMdd|HH:mm:ssK"))
    static member timeSpanE       (x: TimeSpan      ) = ValueSummaryEncoding.SString ((x.Subtract (TimeSpan.FromMilliseconds (float x.Milliseconds))).ToString("c"))
    static member decimalE        (x: decimal       ) = ValueSummaryEncoding.SString (string x)
    static member floatE          (x: Double        ) = ValueSummaryEncoding.SString (string x)
    static member float32E        (x: Single        ) = ValueSummaryEncoding.SString (string x)
    static member intE            (x: int           ) = ValueSummaryEncoding.SString (string x)
    static member uint32E         (x: uint32        ) = ValueSummaryEncoding.SString (string x)
    static member int64E          (x: int64         ) = ValueSummaryEncoding.SString (string x)
    static member uint64E         (x: uint64        ) = ValueSummaryEncoding.SString (string x)
    static member int16E          (x: int16         ) = ValueSummaryEncoding.SString (string x)
    static member uint16E         (x: uint16        ) = ValueSummaryEncoding.SString (string x)
    static member byteE           (x: byte          ) = ValueSummaryEncoding.SString (string x)
    static member sbyteE          (x: sbyte         ) = ValueSummaryEncoding.SString (string x)
    static member charE           (x: char          ) = ValueSummaryEncoding.SString (string x)
    static member bigintE         (x: bigint        ) = ValueSummaryEncoding.SString (string x)
    static member guidE           (x: Guid          ) = ValueSummaryEncoding.SString (x.ToString("D"))


    ////////////
    // Codecs //
    ////////////

    static member result  (codec1: Codec<_,_>) (codec2: Codec<_,_>) = Codec.create Unchecked.defaultof<_> (ValueSummaryEncoding.resultE (enc codec1) (enc codec2))

    static member choice  (codec1: Codec<_,_>) (codec2: Codec<_,_>) = Codec.create Unchecked.defaultof<_> (ValueSummaryEncoding.choiceE (enc codec1) (enc codec2))
    static member choice3 (codec1: Codec<_,_>) (codec2: Codec<_,_>) (codec3: Codec<_,_>) = Codec.create Unchecked.defaultof<_> (ValueSummaryEncoding.choice3E (enc codec1) (enc codec2) (enc codec3))
    static member option (codec: Codec<_,_>) = Codec.create Unchecked.defaultof<_> (ValueSummaryEncoding.optionE (enc codec))

    static member array    (codec: Codec<_,_>) = Codec.create Unchecked.defaultof<_> (ValueSummaryEncoding.arrayE    (enc codec))
    static member multiMap (codec: Codec<_,_>) = Codec.create Unchecked.defaultof<_> (ValueSummaryEncoding.multiMapE (enc codec))

    static member unit () = Codec.create Unchecked.defaultof<_> ValueSummaryEncoding.unitE
    static member tuple1 (codec1: Codec<_,_>)                                                                                                                               = Codec.create Unchecked.defaultof<_> (ValueSummaryEncoding.tuple1E (enc codec1))
    static member tuple2 (codec1: Codec<_,_>) (codec2: Codec<_,_>)                                                                                                          = Codec.create Unchecked.defaultof<_> (ValueSummaryEncoding.tuple2E (enc codec1) (enc codec2))
    static member tuple3 (codec1: Codec<_,_>) (codec2: Codec<_,_>) (codec3: Codec<_,_>)                                                                                     = Codec.create Unchecked.defaultof<_> (ValueSummaryEncoding.tuple3E (enc codec1) (enc codec2) (enc codec3))
    static member tuple4 (codec1: Codec<_,_>) (codec2: Codec<_,_>) (codec3: Codec<_,_>) (codec4: Codec<_,_>)                                                                = Codec.create Unchecked.defaultof<_> (ValueSummaryEncoding.tuple4E (enc codec1) (enc codec2) (enc codec3) (enc codec4))
    static member tuple5 (codec1: Codec<_,_>) (codec2: Codec<_,_>) (codec3: Codec<_,_>) (codec4: Codec<_,_>) (codec5: Codec<_,_>)                                           = Codec.create Unchecked.defaultof<_> (ValueSummaryEncoding.tuple5E (enc codec1) (enc codec2) (enc codec3) (enc codec4) (enc codec5))
    static member tuple6 (codec1: Codec<_,_>) (codec2: Codec<_,_>) (codec3: Codec<_,_>) (codec4: Codec<_,_>) (codec5: Codec<_,_>) (codec6: Codec<_,_>)                      = Codec.create Unchecked.defaultof<_> (ValueSummaryEncoding.tuple6E (enc codec1) (enc codec2) (enc codec3) (enc codec4) (enc codec5) (enc codec6))
    static member tuple7 (codec1: Codec<_,_>) (codec2: Codec<_,_>) (codec3: Codec<_,_>) (codec4: Codec<_,_>) (codec5: Codec<_,_>) (codec6: Codec<_,_>) (codec7: Codec<_,_>) = Codec.create Unchecked.defaultof<_> (ValueSummaryEncoding.tuple7E (enc codec1) (enc codec2) (enc codec3) (enc codec4) (enc codec5) (enc codec6) (enc codec7))

    static member boolean  : Codec<ValueSummaryEncoding, bool>      =  Codec.create Unchecked.defaultof<_> ValueSummaryEncoding.booleanE
    static member string         = Codec.create Unchecked.defaultof<_> ValueSummaryEncoding.stringE
    static member dateTime       = Codec.create Unchecked.defaultof<_> ValueSummaryEncoding.dateTimeE
    static member dateTimeOffset = Codec.create Unchecked.defaultof<_> ValueSummaryEncoding.dateTimeOffsetE
    static member timeSpan       = Codec.create Unchecked.defaultof<_> ValueSummaryEncoding.timeSpanE
    static member decimal        = Codec.create Unchecked.defaultof<_> ValueSummaryEncoding.decimalE
    static member float          = Codec.create Unchecked.defaultof<_> ValueSummaryEncoding.floatE
    static member float32        = Codec.create Unchecked.defaultof<_> ValueSummaryEncoding.float32E
    static member int            = Codec.create Unchecked.defaultof<_> ValueSummaryEncoding.intE
    static member uint32         = Codec.create Unchecked.defaultof<_> ValueSummaryEncoding.uint32E
    static member int64          = Codec.create Unchecked.defaultof<_> ValueSummaryEncoding.int64E
    static member uint64         = Codec.create Unchecked.defaultof<_> ValueSummaryEncoding.uint64E
    static member int16          = Codec.create Unchecked.defaultof<_> ValueSummaryEncoding.int16E
    static member uint16         = Codec.create Unchecked.defaultof<_> ValueSummaryEncoding.uint16E
    static member byte           = Codec.create Unchecked.defaultof<_> ValueSummaryEncoding.byteE
    static member sbyte          = Codec.create Unchecked.defaultof<_> ValueSummaryEncoding.sbyteE
    static member char           = Codec.create Unchecked.defaultof<_> ValueSummaryEncoding.charE
    static member bigint         = Codec.create Unchecked.defaultof<_> ValueSummaryEncoding.bigintE
    static member guid           = Codec.create Unchecked.defaultof<_> ValueSummaryEncoding.guidE

    interface IRawEncoding with
        member _.boolean        = ValueSummaryEncoding.toIRawCodec ValueSummaryEncoding.boolean
        member _.string         = ValueSummaryEncoding.toIRawCodec ValueSummaryEncoding.string
        member _.dateTime _     = ValueSummaryEncoding.toIRawCodec ValueSummaryEncoding.dateTime
        member _.dateTimeOffset = ValueSummaryEncoding.toIRawCodec ValueSummaryEncoding.dateTimeOffset
        member _.timeSpan       = ValueSummaryEncoding.toIRawCodec ValueSummaryEncoding.timeSpan
        member _.decimal        = ValueSummaryEncoding.toIRawCodec ValueSummaryEncoding.decimal
        member _.float          = ValueSummaryEncoding.toIRawCodec ValueSummaryEncoding.float
        member _.float32        = ValueSummaryEncoding.toIRawCodec ValueSummaryEncoding.float32
        member _.int            = ValueSummaryEncoding.toIRawCodec ValueSummaryEncoding.int
        member _.uint32         = ValueSummaryEncoding.toIRawCodec ValueSummaryEncoding.uint32
        member _.int64          = ValueSummaryEncoding.toIRawCodec ValueSummaryEncoding.int64
        member _.uint64         = ValueSummaryEncoding.toIRawCodec ValueSummaryEncoding.uint64
        member _.int16          = ValueSummaryEncoding.toIRawCodec ValueSummaryEncoding.int16
        member _.uint16         = ValueSummaryEncoding.toIRawCodec ValueSummaryEncoding.uint16
        member _.byte           = ValueSummaryEncoding.toIRawCodec ValueSummaryEncoding.byte
        member _.sbyte          = ValueSummaryEncoding.toIRawCodec ValueSummaryEncoding.sbyte
        member _.char           = ValueSummaryEncoding.toIRawCodec ValueSummaryEncoding.char
        member _.bigint         = ValueSummaryEncoding.toIRawCodec ValueSummaryEncoding.bigint
        member _.guid           = ValueSummaryEncoding.toIRawCodec ValueSummaryEncoding.guid

        member _.result c1 c2     = ValueSummaryEncoding.toIRawCodec (ValueSummaryEncoding.result   (ValueSummaryEncoding.ofIRawCodec c1) (ValueSummaryEncoding.ofIRawCodec c2))
        member _.choice c1 c2     = ValueSummaryEncoding.toIRawCodec (ValueSummaryEncoding.choice   (ValueSummaryEncoding.ofIRawCodec c1) (ValueSummaryEncoding.ofIRawCodec c2))
        member _.choice3 c1 c2 c3 = ValueSummaryEncoding.toIRawCodec (ValueSummaryEncoding.choice3  (ValueSummaryEncoding.ofIRawCodec c1) (ValueSummaryEncoding.ofIRawCodec c2) (ValueSummaryEncoding.ofIRawCodec c3))
        member _.option c         = ValueSummaryEncoding.toIRawCodec (ValueSummaryEncoding.option   (ValueSummaryEncoding.ofIRawCodec c))
        member _.array c          = ValueSummaryEncoding.toIRawCodec (ValueSummaryEncoding.array    (ValueSummaryEncoding.ofIRawCodec c))
        member _.propertyList c   = ValueSummaryEncoding.toIRawCodec (ValueSummaryEncoding.multiMap (ValueSummaryEncoding.ofIRawCodec c))

        member _.enum<'t, 'u when 't : enum<'u> and 't : (new : unit -> 't) and 't : struct and 't :> ValueType> () : Codec<IRawEncoding, 't> = ValueSummaryEncoding.toIRawCodec (Codec.create Unchecked.defaultof<_> ValueSummaryEncoding.enumE)

        member _.getCase = "str"


///////////////////////
// Main entry points //
///////////////////////

/// Get the summary representation of the value, using its default codec.
let inline toSummaryValue (x: 'T) : ValueSummaryEncoding = toEncoding<ValueSummaryEncoding, 'T> x |> ValueSummaryEncoding.Unwrap

let getSummaryString (maxDepth: int) (summaryValue: ValueSummaryEncoding) =
    // pretty-print that tries to balance readability & redundant whitespace
    let sw = StringBuilder ()
    let rec f depth s =
        match (depth, s) with

        // empty array
        | _, ValueSummaryEncoding.Arr (_, a) when a.Length = 0 ->
            sw.Write "[]"

        // array where all the items are too deep
        | _, ValueSummaryEncoding.Arr _ when maxDepth - depth < 2 ->
            sw.Write "[..]"

        // empty object
        | _, ValueSummaryEncoding.Obj (_, o) when o.Length = 0 ->
            sw.Write "{}"

        // short atoms written regardless of depth
        | _, ValueSummaryEncoding.Atom s when s.Length < 10 ->
            sw.Write s

        | _ when depth = maxDepth ->
            // Don't go deeper than certain depth.
            // Can we further improve performance by never creating a deep summary in the first place?
            // Best we can do is trim arrays, tuples and multiMap to some arbitrary small N (which we already do)
            // but we can't control the absolute depth of traversal with current design: codec function composition creates
            // encoder "thunks" inside-out and this is the biggest cost (excessive Value Summary is cheap).
            sw.Write ".."

        | _, ValueSummaryEncoding.Atom s ->
            sw.Write s

        // one-item obj that doesn't have much depth left - just inline
        | _, ValueSummaryEncoding.Obj (depthBelow, o) when o.Length = 1 && (depthBelow < 3 || maxDepth - depth < 2) ->
            let key, value = o.[0]
            (sw.Write key) && (sw.Write ": ") && (f depth value)

        | _, ValueSummaryEncoding.Obj (_, o) ->
            if sw.Write '{' then
                let mutable break' = false
                let mutable i = 0
                while (i < o.Length && not break') do
                    let key, value = o.[i]
                    if (sw.WriteIndent (depth+1)) && (sw.Write key) && (sw.Write ": ") && (f (depth+1) value) then
                        i <- i + 1
                    else
                        break' <- true
                sw.Write '}'
            else
                false

        // one-item arr that doesn't have much depth left - no new line
        | _, ValueSummaryEncoding.Arr (depthBelow, a) when a.Length = 1 && depthBelow < 3 ->
            let value = a.[0]
            (sw.Write '[') && (f depth value) && sw.Write ']'

        | _, ValueSummaryEncoding.Arr (_, a) ->
            if sw.Write '[' then
                let mutable break' = false
                let mutable i = 0
                while (i < a.Length && not break') do
                    let x = a.[i]
                    if (sw.WriteIndent (depth+1)) && (f (depth + 1) x) then
                        i <- i + 1
                    else
                        break' <- true
                sw.Write ']'
            else
                false

    f 0 summaryValue |> ignore
    sw.ToString()

#endif // END OF THE GLOBAL CODEC DISABLE FOR FABLE