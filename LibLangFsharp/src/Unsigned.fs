[<AutoOpen>]
module Unsigned

[<Struct>]
type UnsignedInteger =
    private | UnsignedInteger of int

    member this.Value : int =
        match this with
        | UnsignedInteger value -> value

    static member (+) (left: UnsignedInteger, right: UnsignedInteger) =
        UnsignedInteger (left.Value + right.Value)

    static member (*) (left: UnsignedInteger, right: UnsignedInteger) =
        UnsignedInteger (left.Value * right.Value)

    static member ofInt (num: int) : Option<UnsignedInteger> =
        if num >= 0 then
            UnsignedInteger num |> Some
        else
            None

    static member ofIntUnsafe (num: int) : UnsignedInteger =
        if num >= 0 then
            UnsignedInteger num
        else
            failwith "Unsigned integer cannot be less than 0"

    static member ofUint (num: uint32) : UnsignedInteger =
        UnsignedInteger (int num)

    static member ofUint64 (num: uint64) : UnsignedInteger =
        UnsignedInteger (int num)

    static member toUint (value: UnsignedInteger) : uint32 =
        uint32 value.Value

    static member ofPositiveNumber (num: PositiveInteger) : UnsignedInteger =
        UnsignedInteger (num.Value)

    static member One : UnsignedInteger =
        UnsignedInteger 1

    static member Zero : UnsignedInteger =
        UnsignedInteger 0

    static member MaxValue : UnsignedInteger =
        UnsignedInteger System.Int32.MaxValue

    override i.ToString () : string = string i.Value

type PositiveInteger with
    static member ofUnsignedInteger (num: UnsignedInteger) : Option<PositiveInteger> =
        match num.Value with
        | 0     -> None
        | value -> PositiveInteger.ofIntUnsafe value |> Some

[<Struct>]
type UnsignedFloat =
    private | UnsignedFloat of float

    member this.SignPlus()  : float = match this with UnsignedFloat m ->  m
    member this.SignMinus() : float = match this with UnsignedFloat m -> -m

    static member (+) (left: UnsignedFloat, right: UnsignedFloat) : UnsignedFloat =
        UnsignedFloat (left.SignPlus() + right.SignPlus())

    static member (*) (left: UnsignedFloat, right: UnsignedFloat) : UnsignedFloat =
        UnsignedFloat (left.SignPlus() * right.SignPlus())

    member this.Value : float =
        match this with
        | UnsignedFloat value -> value

    static member ofFloat (num: float) : Option<UnsignedFloat> =
        if num >= LanguagePrimitives.FloatWithMeasure 0.0 then
            UnsignedFloat num |> Some
        else
            None

    static member ofFloatUnsafe (num: float) : UnsignedFloat =
        match UnsignedFloat.ofFloat num with
        | Some v -> v
        | None   -> failwithf "Unsigned value expected, given: %f" num

[<Struct>]
type UnsignedDecimal =
    private | UnsignedDecimal of decimal

    member this.SignPlus()  : decimal = match this with UnsignedDecimal m ->  m
    member this.SignMinus() : decimal = match this with UnsignedDecimal m -> -m

    static member Zero = UnsignedDecimal 0m

    static member Max : UnsignedDecimal = UnsignedDecimal.ofLiteral System.Decimal.MaxValue

    static member (+) (left: UnsignedDecimal, right: UnsignedDecimal) : UnsignedDecimal =
        UnsignedDecimal (left.SignPlus() + right.SignPlus())

    static member (-) (left: UnsignedDecimal, right: UnsignedDecimal) : Option<UnsignedDecimal> =
        (left.SignPlus() - right.SignPlus())
        |> UnsignedDecimal.ofDecimal

    static member (*) (left: UnsignedDecimal, right: UnsignedDecimal) : UnsignedDecimal =
        UnsignedDecimal (left.SignPlus() * right.SignPlus())

    static member (*) (left: UnsignedDecimal, right: uint32) : UnsignedDecimal =
        UnsignedDecimal (left.SignPlus() * (decimal right))

    static member (*) (left: UnsignedDecimal, right: PositiveInteger) : UnsignedDecimal =
        UnsignedDecimal (left.SignPlus() * (decimal (right.SignPlus())))

    static member (*) (left: PositiveInteger, right: UnsignedDecimal) : UnsignedDecimal =
        UnsignedDecimal (right.SignPlus() * (decimal (left.SignPlus())))

    static member (*) (left: UnsignedDecimal, right: PositiveDecimal) : UnsignedDecimal =
        UnsignedDecimal (left.SignPlus() * (decimal (right.SignPlus())))

    static member (*) (left: UnsignedDecimal, right: UnsignedInteger) : UnsignedDecimal =
        UnsignedDecimal (left.SignPlus() * (decimal right.Value))

    static member (*) (left: UnsignedInteger, right: UnsignedDecimal) : UnsignedDecimal =
        UnsignedDecimal ((decimal left.Value) * right.SignPlus())

    static member (/) (left: UnsignedDecimal, right: UnsignedDecimal) : UnsignedDecimal =
        UnsignedDecimal (left.SignPlus() / right.SignPlus())

    static member fromPositive (p1: PositiveDecimal) : UnsignedDecimal =
        UnsignedDecimal (p1.SignPlus())

    static member fromUnsignedInteger (p1: UnsignedInteger) : UnsignedDecimal =
        UnsignedDecimal (p1.Value |> decimal)

    member this.Value : decimal =
        match this with
        | UnsignedDecimal value -> value

    static member ofDecimal (num: decimal) : Option<UnsignedDecimal> =
        if num >= LanguagePrimitives.DecimalWithMeasure 0m then
            UnsignedDecimal num |> Some
        else
            None

    static member ofDecimalUnsafe (num: decimal) : UnsignedDecimal =
        match UnsignedDecimal.ofDecimal num with
        | Some v -> v
        | None   -> failwithf "Unsigned value expected, given: %f" num

    static member ofLiteral (value: decimal) : UnsignedDecimal =
        UnsignedDecimal.ofDecimalUnsafe value

    override i.ToString () : string = string i.Value


module Seq = Microsoft.FSharp.Collections.Seq
module Seq =
    let sumPositiveInteger (source: seq<PositiveInteger>) : UnsignedInteger =
        source
        |> Seq.fold
                (fun sum total -> sum + total.Value)
                0
        |> UnsignedInteger

    let sumPositiveDecimal (source: seq<PositiveDecimal>) : UnsignedDecimal =
        source
        |> Seq.fold
                (fun sum total -> sum + total.Value)
                0m
        |> UnsignedDecimal.ofDecimalUnsafe

#if !FABLE_COMPILER

open CodecLib
open FSharpPlus

type UnsignedInteger
with
    static member TryParse (s: string) = tryParse s >>= UnsignedInteger.ofInt
    static member get_Codec () =
        Codec.create (UnsignedInteger.TryParse >> Result.ofOption (Uncategorized "Failed parsing")) string<UnsignedInteger>
        |> Codec.compose Codecs.string

type UnsignedDecimal
with
    static member TryParse (s: string) = tryParse s >>= UnsignedDecimal.ofDecimal
    static member get_Codec () =
        Codec.create (UnsignedDecimal.TryParse >> Result.ofOption (Uncategorized "Failed parsing")) string<UnsignedDecimal>
        |> Codec.compose Codecs.string

#endif