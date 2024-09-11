[<AutoOpen>]
module Positive

open System

[<Struct>] // Using struct here virtually eliminates any added performance cost of wrapping the internal value types
type PositiveInteger =
    private | PositiveInteger of int

    member this.SignPlus()  : int = match this with PositiveInteger m ->  m
    member this.SignMinus() : int = match this with PositiveInteger m -> -m

    member this.Value : int =
        match this with
        | PositiveInteger value -> value

    static member MaxValue : PositiveInteger =
        PositiveInteger System.Int32.MaxValue

    static member value (m: PositiveInteger) : int = m.Value

    static member (+) (left: PositiveInteger, right: PositiveInteger) : PositiveInteger =
        PositiveInteger (left.SignPlus() + right.SignPlus())

    static member (+) (left: PositiveInteger, maybeRight: Option<PositiveInteger>) : PositiveInteger =
        match maybeRight with
        | Some right ->
            PositiveInteger (left.SignPlus() + right.SignPlus())
        | None ->
            left

    static member (+) (left: PositiveInteger, right: uint32) : PositiveInteger =
        PositiveInteger (left.SignPlus() + (int right))

    static member (-) (left: PositiveInteger, right: int) : Option<PositiveInteger> =
        let diff = left.SignPlus() - right
        if diff > 0 then
            diff |> PositiveInteger |> Some
        else
            None

    static member (-) (left: PositiveInteger, right: PositiveInteger) : Option<PositiveInteger> =
        let diff = left.SignPlus() - right.SignPlus()
        if diff > 0 then
            diff |> PositiveInteger |> Some
        else
            None

    static member (*) (left: PositiveInteger, right: PositiveInteger) : PositiveInteger =
        PositiveInteger (left.SignPlus() * right.SignPlus())

    static member One : PositiveInteger =
        PositiveInteger 1

    static member ofInt (num: int) : Option<PositiveInteger> =
        if num > 0 then
            PositiveInteger num |> Some
        else
            None

    static member ofLiteral (num: int) : PositiveInteger =
        PositiveInteger.ofIntUnsafe num

    static member ofIntUnsafe (num: int) : PositiveInteger =
        match PositiveInteger.ofInt num with
        | Some v -> v
        | None   -> failwithf "Positive value expected, given: %d" num

    override i.ToString () : string = string i.Value


[<Struct>]  // Using struct here virtually eliminates any added performance cost of wrapping the internal value types
type PositiveFloat =
    private | PositiveFloat of float

    member this.SignPlus()  : float = match this with PositiveFloat m ->  m
    member this.SignMinus() : float = match this with PositiveFloat m -> -m

    static member (+) (left: PositiveFloat, right: PositiveFloat) =
        PositiveFloat (left.SignPlus() + right.SignPlus())

    static member (*) (left: PositiveFloat, right: PositiveFloat) =
        PositiveFloat (left.SignPlus() * right.SignPlus())

    member this.Value : float =
        match this with
        | PositiveFloat value -> value

    static member ofFloat (num: float) : Option<PositiveFloat> =
        if num > LanguagePrimitives.FloatWithMeasure 0.0 then
            PositiveFloat num |> Some
        else
            None

    static member ofFloatUnsafe (num: float) : PositiveFloat =
        match PositiveFloat.ofFloat num with
        | Some v -> v
        | None   -> failwithf "Positive value expected, given: %f" num

[<Struct>]  // Using struct here virtually eliminates any added performance cost of wrapping the internal value types
type PositiveDecimal =
    private | PositiveDecimal of decimal

    member this.SignPlus()  : decimal = match this with PositiveDecimal m ->  m
    member this.SignMinus() : decimal = match this with PositiveDecimal m -> -m

    static member (+) (left: PositiveDecimal, right: PositiveDecimal) =
        PositiveDecimal (left.SignPlus() + right.SignPlus())

    static member (*) (left: PositiveDecimal, right: PositiveDecimal) =
        PositiveDecimal (left.SignPlus() * right.SignPlus())

    static member (/) (left: PositiveDecimal, right: PositiveDecimal) =
        PositiveDecimal (left.SignPlus() / right.SignPlus())

    static member (*) (left: PositiveDecimal, right: PositiveInteger) =
        PositiveDecimal (left.SignPlus() * (decimal (right.SignPlus())))

    static member (/) (left: PositiveDecimal, right: PositiveInteger) =
        PositiveDecimal (left.SignPlus() / (decimal (right.SignPlus())))

    static member (*) (left: PositiveDecimal, right: uint16) =
        PositiveDecimal (left.SignPlus() * (decimal right))

    member this.Value : decimal =
        match this with
        | PositiveDecimal value -> value

    static member ofDecimal (num: decimal) : Option<PositiveDecimal> =
        if num > LanguagePrimitives.DecimalWithMeasure 0m then
            PositiveDecimal num |> Some
        else
            None

    static member ofDecimalUnsafe (num: decimal) : PositiveDecimal =
        match PositiveDecimal.ofDecimal num with
        | Some v -> v
        | None   -> failwithf "Positive value expected, given: %f" num

    static member ofLiteral (value: decimal) : PositiveDecimal =
        PositiveDecimal.ofDecimalUnsafe value

    static member Round (decimalPlacesToRoundTo: int) (num: PositiveDecimal) : PositiveDecimal =
        match num with PositiveDecimal value -> (value, decimalPlacesToRoundTo) |> Decimal.Round |> PositiveDecimal.ofDecimal |> Option.defaultValue (PositiveDecimal 1m)

    static member optionToSignPlus (candidate: Option<PositiveDecimal>) : decimal =
        match candidate with
        | Some (PositiveDecimal value) -> value
        | None                         -> 0m

    static member optionToSignMinus (candidate: Option<PositiveDecimal>) : decimal =
        match candidate with
        | Some (PositiveDecimal value) -> -value
        | None                         -> 0m

    override i.ToString () : string = string i.Value

let (|PositiveInteger|) (candidate: PositiveInteger) =
    match candidate with
    | PositiveInteger.PositiveInteger s -> s


#if !FABLE_COMPILER

open CodecLib
open FSharpPlus

type PositiveInteger
with
    static member TryParse (s: string) = tryParse s >>= PositiveInteger.ofInt
    static member get_Codec () =
        Codec.create (PositiveInteger.TryParse >> Result.ofOption (Uncategorized "Failed parsing")) string<PositiveInteger>
        |> Codec.compose Codecs.string

type PositiveDecimal
with
    static member TryParse (s: string) = tryParse s >>= PositiveDecimal.ofDecimal
    static member get_Codec () =
        Codec.create (PositiveDecimal.TryParse >> Option.toResultWith (Uncategorized "Failed parsing")) string<PositiveDecimal>
        |> Codec.compose Codecs.string

#endif