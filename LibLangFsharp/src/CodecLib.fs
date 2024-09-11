/// CodecLib is a very thin wrapper over Fleece, main purposes of it are not obivous:
/// * first of all, it helps to avoid importing all Fleece symbols via `open Fleece` which in some scenarios leads to extreme compile time degradation e.g.
/// `open Fleece` and `open FSharpPlus` in one module can nearly kill F# compiler: https://github.com/fsprojects/Fleece/issues/146
/// * second, it shadows codec operators and offers composition alternatives such as codec CE which is easier to read for an average developer
/// * finally it somewhat abstracts away Fleece dependency, however it's impossible to do in full because it's essentially one big inline engine
module CodecLib

open System

/// Signals code generator to generate the required Codec methods for types in the annotated module.
type CodecAutoGenerate () = inherit System.Attribute ()

/// Signals code generator to skip generation of Codec methods for annotated type or its inheritor.
[<AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)>]
type SkipCodecAutoGenerate () = inherit System.Attribute ()

#if FABLE_COMPILER

type IInterfaceCodec<'Base> = interface end

#endif

#if !FABLE_COMPILER // START OF THE GLOBAL CODEC DISABLE FOR FABLE

open FSharpPlus
open Fleece
/// Marker interface for all interfaces whose derived classes will support codecs
type IInterfaceCodec<'Base> = ICodecInterface<'Base>

open System

// must be invoked before very first codec usage
let configureCodecLib() =
    // Enable Codec cache
    Fleece.Config.codecCacheEnabled <- true
    // lossless DateTime formats
    Fleece.SystemTextJson.Encoding.DateTimeOffsetFormat <- "yyyy-MM-ddTHH:mm:ss.fffffffK"
    Fleece.SystemTextJson.Encoding.DateTimeFormat <- "yyyy-MM-ddTHH:mm:ss.fffffffZ"

do
    configureCodecLib()

let improveDecodeError (err: DecodeError) : DecodeError =
    let rec decodeErrorToString (err: DecodeError) : string =
        // after switching to AdHocEncoding the IEncoding.ToString() became useless
        // TODO: get rid of AdHocEncoding and use IEncdoing.ToString() - it should work for StjEncoding. Probably only when we get rid of interface codecs (see below)
        //  Removing AdHocEncoding is extremely annoying atm as it requires breaking changes in initializeInterfaceImplementation for each interface-derived type
        match err with
        | DecodeError.EncodingCaseMismatch (t: Type, _: IEncoding, expected: string, actual: string) ->
             $"%s{expected} expected but got %s{actual} while decoding value of type %s{t.FullName}"
        | DecodeError.NullString (t: Type) ->
             $"Expected type %s{t.FullName}, got null"
        | DecodeError.IndexOutOfRange (e, a: IEncoding array) ->
             $"Expected array with %d{e} items, was %d{a.Length} items"
        | DecodeError.InvalidValue (t: Type, _: IEncoding, s: string) ->
             let extra = if String.IsNullOrEmpty s then "" else " " + s
             $"Value is invalid for type %s{t.FullName}%s{extra}"
        | DecodeError.PropertyNotFound (p: string, props: PropertyList<IEncoding>) ->
             let availableProps = props.Properties |> Seq.map (fun (name: string, _) -> name) |> String.concat "; "
             $"Property: '%s{p}' not found. Available properties: '%s{availableProps}'"
        | DecodeError.ParseError (t: Type, ex: Exception, v: string) ->
             $"Error decoding %s{v} from %s{t.FullName}: %s{ex.ToString()}"
        | DecodeError.Uncategorized (str: string) ->
             str
        | DecodeError.Multiple (lst: list<DecodeError>) ->
            List.map decodeErrorToString lst
            |> String.concat "\r\n"
    // We don't really care about error case, only text, but keep the DecodeError type for compatibility with nuget packages
    // Uncategorized .ToString() is equals to its payload so it is ideal
    decodeErrorToString err |> DecodeError.Uncategorized

[<AutoOpen>]
module StjCodecs =
    type StjEncoding = Fleece.SystemTextJson.Encoding
    let inline toJson x = Fleece.SystemTextJson.Operators.toJson x
    let inline ofJson x = Fleece.SystemTextJson.Operators.ofJson x |> Result.mapError improveDecodeError
    let inline toJsonText x = Fleece.SystemTextJson.Operators.toJsonText x
    let inline ofJsonText x = Fleece.SystemTextJson.Operators.ofJsonText x |> Result.mapError improveDecodeError

type Codec<'S1, 'S2, 't1, 't2> = Fleece.Codec<'S1, 'S2, 't1, 't2>
type Codec<'S, 't> = Fleece.Codec<'S, 't>
type Decoder<'S, 't> = Fleece.Decoder<'S, 't>

module Codec =
    let compose x y = Fleece.Codec.compose x y
    let encode codec x = Fleece.Codec.encode codec x
    let decode codec x = Fleece.Codec.decode codec x |> Result.mapError improveDecodeError
    let create decoder encoder = Fleece.Codec.create decoder encoder

let Uncategorized x = Fleece.Uncategorized x


// from F#+

/// <summary> Creates a constant function.</summary>
/// <param name="k">The constant value.</param>
/// <returns>The constant value function.</returns>
/// <category index="0">Common Combinators</category>
let inline konst (k: 'T) = fun (_: 'Ignored) -> k

module Codecs =
    let unit<'Encoding when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding)> = Fleece.Codecs.unit<'Encoding>
    let string<'Encoding when 'Encoding :> IEncoding and 'Encoding: (new : unit -> 'Encoding)> = Fleece.Codecs.string<'Encoding>
    let int<'Encoding when 'Encoding :> IEncoding and 'Encoding: (new : unit -> 'Encoding)> = Fleece.Codecs.int<'Encoding>
    let int16<'Encoding when 'Encoding :> IEncoding and 'Encoding: (new : unit -> 'Encoding)> = Fleece.Codecs.int16<'Encoding>
    let int64<'Encoding when 'Encoding :> IEncoding and 'Encoding: (new : unit -> 'Encoding)> = Fleece.Codecs.int64<'Encoding>
    let uint16<'Encoding when 'Encoding :> IEncoding and 'Encoding: (new : unit -> 'Encoding)> = Fleece.Codecs.uint16<'Encoding>
    let uint32<'Encoding when 'Encoding :> IEncoding and 'Encoding: (new : unit -> 'Encoding)> = Fleece.Codecs.uint32<'Encoding>
    let uint64<'Encoding when 'Encoding :> IEncoding and 'Encoding: (new : unit -> 'Encoding)> = Fleece.Codecs.uint64<'Encoding>
    let decimal<'Encoding when 'Encoding :> IEncoding and 'Encoding: (new : unit -> 'Encoding)> = Fleece.Codecs.decimal<'Encoding>
    let boolean<'Encoding when 'Encoding :> IEncoding and 'Encoding: (new : unit -> 'Encoding)> = Fleece.Codecs.boolean<'Encoding>
    let date<'Encoding when 'Encoding :> IEncoding and 'Encoding: (new : unit -> 'Encoding)> = Fleece.Codecs.date<'Encoding>
    let time<'Encoding when 'Encoding :> IEncoding and 'Encoding: (new : unit -> 'Encoding)> = Fleece.Codecs.time<'Encoding>
    let float<'Encoding when 'Encoding :> IEncoding and 'Encoding: (new : unit -> 'Encoding)> = Fleece.Codecs.float<'Encoding>

    let inline dateTimeOffset<'Encoding when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding)> =
        let dateTimeOffsetD (x: 'Encoding) =
            match x with
            | JString null -> Decode.Fail.nullString
            | JString s    ->
                match DateTimeOffset.TryParseExact (s, [| "yyyy-MM-ddTHH:mm:ss.fffffffK"; "yyyy-MM-ddTHH:mm:ssK" |], null, System.Globalization.DateTimeStyles.RoundtripKind) with
                | true, t -> Ok t
                | _       -> Decode.Fail.invalidValue x ""
            | a -> Decode.Fail.strExpected a
        let dateTimeOffsetE (x: DateTimeOffset) = JString (x.ToString ("yyyy-MM-ddTHH:mm:ss.fffffffK")) : 'Encoding
        Codec.create dateTimeOffsetD dateTimeOffsetE

    let timeSpan<'Encoding when 'Encoding :> IEncoding and 'Encoding: (new : unit -> 'Encoding)> = Fleece.Codecs.timeSpan<'Encoding>
    let guid<'Encoding when 'Encoding :> IEncoding and 'Encoding: (new : unit -> 'Encoding)> = Fleece.Codecs.guid<'Encoding>
    let char<'Encoding when 'Encoding :> IEncoding and 'Encoding: (new : unit -> 'Encoding)> = Fleece.Codecs.char<'Encoding>
    let byte<'Encoding when 'Encoding :> IEncoding and 'Encoding: (new : unit -> 'Encoding)> = Fleece.Codecs.byte<'Encoding>
    let base64Bytes<'Encoding when 'Encoding :> IEncoding and 'Encoding: (new : unit -> 'Encoding)> = Fleece.Codecs.base64Bytes<'Encoding>

    let list x = Fleece.Codecs.list x
    let set x = Fleece.Codecs.set x
    let tuple2 x = Fleece.Codecs.tuple2 x
    let tuple3 x = Fleece.Codecs.tuple3 x
    let tuple4 x = Fleece.Codecs.tuple4 x
    let tuple5 x = Fleece.Codecs.tuple5 x
    let tuple6 x = Fleece.Codecs.tuple6 x
    let tuple7 x = Fleece.Codecs.tuple7 x
    let array x = Fleece.Codecs.array x
    let option x = Fleece.Codecs.option x
    let result x = Fleece.Codecs.result x
    let choice x = Fleece.Codecs.choice x
    let choice3 x = Fleece.Codecs.choice3 x
    let gmap x = Fleece.Codecs.map x

    let decodeAnyToUnit<'Encoding when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding)> = (Codec.create (fun _ -> Ok ()) (fun _ -> failwith "Error")) : Codec<'Encoding, _>

module Decode =
    module Fail =
        let nullString<'t> = Fleece.Decode.Fail.nullString<'t>

// because for some reason I don't understand, this is not flimsy and dangerous, but actually okay
let UnreachableCodeAfterEvolution<'T> : 'T = invalidOp "Old version encoder shouldn't have been invoked"

open FSharpPlus.Data

// Candidates to be included in Fleece

let reqDecoderWith (dec: Decoder<'Encoding,'Value>) (prop: string) : Decoder<_,_> =
    let getFromListWith decoder (m: PropertyList<_>) key =
        match m.[key] with
        | []        -> Decode.Fail.propertyNotFound key m
        | value:: _ -> ReaderT.run decoder value
    ReaderT (fun (o: PropertyList<'Encoding>) -> getFromListWith dec o prop)

let reqDecodeWithCodec (codec: Codec<'Encoding,'Value>) (prop: string) : Decoder<_,_> = reqDecoderWith codec.Decoder (prop: string)

let reqDecoderWithLazy (dec: unit -> Decoder<'Encoding,'Value>) (prop: string) : Decoder<_,_> =
    let getFromListWith decoder (m: PropertyList<_>) key =
        match m.[key] with
        | []        -> Decode.Fail.propertyNotFound key m
        | value:: _ -> ReaderT.run decoder value
    ReaderT (fun (o: PropertyList<'Encoding>) -> getFromListWith (dec ()) o prop)

let optDecoderWith (dec: Decoder<'Encoding, Option<'Value>>) (prop: string) : Decoder<_,_> =
    let getFromListWith decoder (m: PropertyList<_>) key =
        match m.[key] with
        | []        -> Decode.Success None
        | value:: _ -> ReaderT.run decoder value
    ReaderT (fun (o: PropertyList<'Encoding>) -> getFromListWith dec o prop)

let optDecodeWithCodec (codec: Codec<'Encoding, 'Value>) (prop: string) : Decoder<_,_> = optDecoderWith (Codecs.option codec).Decoder (prop: string)

module Decoders =
    let propList     (dec: Decoder<'Encoding, 'a>) = (Codecs.propList (Codec.create (ReaderT.run dec) (Unchecked.defaultof<_>))).Decoder


let combineDecoders (source: Decoder<PropertyList<'S>, 't>) (alternative: Decoder<PropertyList<'S>, 't>) : Decoder<PropertyList<'S>, 't> = //fun r ->
    source ++ alternative
    // match source r, lazy (alternative r) with
    // | Ok x, _ -> Ok x
    // | Error x, Lazy (Error y) -> Error (x ++ y)
    // | _, Lazy d -> d

let ofObjDecoder (objCodec: Decoder<PropertyList<'Encoding>, 't>) : Decoder<_, 't> =
    let (>.>) codec2 codec1 =
        let dec1 = ReaderT.run codec1
        let dec2 = ReaderT.run codec2
        ReaderT (dec1 >> (=<<) dec2)
    objCodec >.> Decoders.propList Codecs.id.Decoder

[<AutoOpen>]
module ComputationExpressions =
    type DecoderApplicativeBuilder () =

        let privReturn f = ReaderT (fun _ -> Ok f) : Decoder<PropertyList<'S>, _>
        let privlift2 (f: 'x ->'y ->'r) (x: Decoder<PropertyList<'S>,'x>) (y:  Decoder<PropertyList<'S>, 'y>) : Decoder<PropertyList<'S>,'r> = ReaderT (fun s -> lift2 f (ReaderT.run x s) (ReaderT.run y s))
        let privlift3 (f: 'x -> 'y -> 'z -> 'r) (x: Decoder<PropertyList<'S>,'x>) (y: Decoder<PropertyList<'S>,'y>) (z: Decoder<PropertyList<'S>,'z>) : Decoder<PropertyList<'S>,'r> = ReaderT (fun s -> lift3 f (ReaderT.run x s) (ReaderT.run y s) (ReaderT.run z s))

        member _.Delay x = x ()
        member _.ReturnFrom expr = expr
        member _.Return x = privReturn x
        member _.Yield  x : Decoder<PropertyList<'r>, 't> = x
        member _.MergeSources  (t1, t2)     = privlift2 tuple2 t1 t2
        member _.MergeSources3 (t1, t2, t3) = privlift3 tuple3 t1 t2 t3
        member _.BindReturn (x: Decoder<PropertyList<'r>,_>, f) = ReaderT.map f x
        member _.Run x : Decoder<PropertyList<_>,'t> = x
        member _.Combine (source: Decoder<PropertyList<'S>, 't>, alternative: Decoder<PropertyList<'S>, 't>) : Decoder<PropertyList<'S>, 't> = combineDecoders source alternative


        // Clients using F# lower than 5 will need this method
        [<CompilerMessage("A Codec doesn't support the Zero operation.", 10708, IsError = true)>]
        member _.Zero () = invalidOp "Fleece internal error: this code should be unreachable."

    /// Decoder Applicative Computation Expression.
    let decoder = DecoderApplicativeBuilder ()


// Our aliased Fleece functions / types

/// An alias for a MultimMap with string keys
type MultiObj<'t> = Fleece.PropertyList<'t>
type PropertyList<'t> = Fleece.PropertyList<'t>

type IRawEncoding = IEncoding

type IEncoding = Fleece.IEncoding


let codec = Fleece.ComputationExpressions.codec

let reqWith (c: Codec<'RawEncoding,_,_,'Value>) (prop: string) (getter: 'T -> 'Value option) = jreqWith c prop getter

let reqWithLazy (c: unit -> Codec<'RawEncoding,_,_,'Value>) (prop: string) (getter: 'T -> 'Value option) = jreqWithLazy c prop getter

let ofObjCodec x = Fleece.Operators.ofObjCodec x

let inline toEncoding<'Encoding, .. when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding)> (x: 't) : 'Encoding = Fleece.Operators.toEncoding x

let optWith c (prop: string) (getter: 'T -> 'Value option) = joptWith (Codecs.option c) prop getter

/// Retrieves default codec for 'T where there's a static member Codec defined. Compiles fast!
/// Doesn't work for types where there's no such static member e.g. primitives, tuples or interfaces.
/// If you struggle to compile a generated codec for a generic type then probably some of the type arguments
/// at the invocation site doesn't have static member Codec e.g. if it's a primitive type or an interface type.
/// Example of code that doesn't compile: codecFor<_, MyContainerType<DateTimeOffset>>
/// Best thing you can do: instead of DateTimeOffset use custom type with static Codec member e.g. codecFor<_, MyContainerType<SomeDateTimeWrapper>>
/// Last resort if you absolutely have to use a primitive / other type without static member Codec:
///   first drill into the codec of innermost generic type - in this case MyContainerType<'t>,
///   then find the immediate codec of 't value it'll probably be codecFor<_, 't>,
///   then replace it with defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 't>.
///   You made it but compilation speed might never be the same.
let inline codecFor<'Encoding, 'T when 'T : (static member Codec: Codec< 'Encoding, 'T>) > =
    // Cache improves speed of codec creation.
    // Encode/decode speed will not change because indirection applies only when lambdas are constructed but not when invoked
    Fleece.Internals.CodecCache<Fleece.Internals.OpCodec, 'Encoding, 'T>
        .Run(fun () -> (^T : (static member Codec: Codec< 'Encoding, ^T>) ()))

/// !! Advanced function !! If in doubt - use codecFor.
/// Retrieves default codec for any generic ^t which has default codec, including primitives, tuples interfaces etc.
/// Must be avoided when possible, because this power comes at huge cost when ^t is not constrained: it makes codec resolution extremely expensive compile-time operation,
/// every invocation can literally cost SECONDS of compile time (as of Fleece 0.10.0 and net7.0 and beyond)
/// Usages must be strictly limited only to cases where codecFor<> doesn't work, specifically:
/// 1. ^t is truly generic unconstrained type, which can be almost anything (a primitive, a tuple, a custom type etc.).
///   Think codecs of basic generic types like OrderedSet<_>, KeyedSet<_, _> and alike.
/// 2. ^t is constrained to IInterfaceCodec<_> and potentially itself can be the interface rather than its concrete implementation.
///   In this case it actually compiles fast.
let inline defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<'Encoding, ^t when 'Encoding :> IEncoding and 'Encoding : (new : unit -> 'Encoding) and (Fleece.Internals.GetCodec or ^t) : (static member GetCodec: ^t * Fleece.Internals.GetCodec * Fleece.Internals.GetCodec * Fleece.Internals.OpCodec -> Codec<'Encoding, ^t>)> =
    Fleece.Internals.CodecCache<Fleece.Internals.OpCodec, 'Encoding, 't>
        .Run(fun () -> Fleece.Operators.defaultCodec<'Encoding,'t>)

module CodecsWithMeasure =
    // unfortunately no easy way to erase/add unit of measure for generic 'T type, so has to be per specific primitive type
    // watch this: https://github.com/fsharp/fslang-suggestions/issues/892
    let float(): Codec<_, float<'UnitOfMeasure>> =
        Codec.create
            (Ok << (fun (x: float) -> FSharp.Core.LanguagePrimitives.FloatWithMeasure x))
            (fun x -> float x)
        |> Codec.compose Codecs.float


/// This is the entry point to register codecs for interface implementations.
let initializeInterfaceImplementation<'Interface, 'Type> (codec: unit -> Codec<MultiObj<AdHocEncoding>, 'Type>) =
    Fleece.ICodecInterface<'Interface>.RegisterCodec<AdHocEncoding, 'Type> codec

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

let codecOfDecoders decoders = { Decoder = Seq.reduceBack combineDecoders decoders; Encoder = Const << (fun _ -> PropertyList [||]) }

let withDecoders decoders codec = codec <|> { Decoder = Seq.reduceBack combineDecoders decoders; Encoder = Const << (fun _ -> PropertyList [||]) }

/// for advanced use cases when data needs to be encoded in two formats, don't use without a good reason
/// e.g. when client can't be upgraded quickly enough to understand latest format
let doubleEncode
    (latestEncoder: Codec<_, _>)
    (compatibleEncoder: Codec<_, _>) =
    latestEncoder <|> compatibleEncoder

let inline attachCodecTypeLabel (typeLabel: string) (toCodec: Codec<_, _, _, 't>) : Codec<_, _, _, 't> =
    reqWith Codecs.unit typeLabel (fun _ -> Some ()) *> toCodec

let inline attachDecoderTypeLabel (typeLabel: string) (toDecoder: Decoder<_, 't>) : Decoder<_, 't> =
    reqDecodeWithCodec Codecs.unit typeLabel *> toDecoder

/// shadows FSharpPlus <|> applicative operator to discourage double encoding, use withDecoders instead
let (<|>) (_dummy1: unit) (_dummy2: unit) : unit =
    failwith "this should shadow Fleece <|> operator to discourage double encoding"

/// shadows <-> Fleece operator as it's very confusing for devs not used to this style of programming.
/// What you should use instead is Codec.create
let ( <-> ) (_dummy1: unit) (_dummy2: unit) : unit =
    failwith "this should shadow Fleece <-> operator to encourage usage of functions instead of operators"

/// shadows <!> operator as it's a footgun which slows the compilation big times when both in Fleece and FSharpPlus are open.
/// It's also very confusing for devs not used to this style of programming.
/// What you should use instead is plain codec CE (or decoder CE in evolutions)
let (<!>) (_dummy1: unit) (_dummy2: unit) : unit =
    failwith "this should shadow Fleece and FSharp <!> operator overloads to prevent slow compilation"

/// shadows <* operator as it's a footgun which slows the compilation big times when both in Fleece and FSharpPlus are open.
/// It's also very confusing for devs not used to this style of programming.
/// What you should use instead is plain codec CE (or decoder CE in evolutions)
let ( <* ) (_dummy1: unit) (_dummy2: unit) : unit =
    failwith "this should shadow Fleece and FSharp <* operator overloads to prevent slow compilation"

/// shadows *> operator as it's a footgun which slows the compilation big times when both in Fleece and FSharpPlus are open.
/// It's also very confusing for devs not used to this style of programming.
/// What you should use is probably attachCodecTypeLabel
let ( *> ) (_dummy1: unit) (_dummy2: unit) : unit =
    failwith "this should shadow Fleece and FSharp <* operator overloads to prevent slow compilation"

#endif // END OF THE GLOBAL CODEC DISABLE FOR FABLE
