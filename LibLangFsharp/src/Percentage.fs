[<AutoOpen>]
module Percentage

[<Struct>]  // Using struct here virtually eliminates any added performance cost of wrapping the internal value types
type PositivePercentage =
    private | PositivePercentage of decimal

    member this.PercentOf (unsignedNum: UnsignedDecimal) =
        match this with PositivePercentage m ->  m * unsignedNum.Value / 100m
        |> UnsignedDecimal.ofDecimalUnsafe // Both are unsigned

    member this.SignPlus()  : decimal = match this with PositivePercentage m ->  m
    member this.SignMinus() : decimal = match this with PositivePercentage m -> -m

    static member (+) (left: PositivePercentage, right: PositivePercentage) =
        PositivePercentage (left.SignPlus() + right.SignPlus())

    static member (*) (left: PositivePercentage, right: PositivePercentage) =
        PositivePercentage (left.SignPlus() * right.SignPlus())

    static member (*) (left: PositivePercentage, right: uint16) =
        PositivePercentage (left.SignPlus() * (decimal right))

    static member (*) (left: UnsignedDecimal, right: PositivePercentage) : UnsignedDecimal =
        UnsignedDecimal.ofDecimalUnsafe (left.SignPlus() * right.SignPlus())

    static member (*) (left: PositivePercentage, right: UnsignedDecimal) : UnsignedDecimal =
        UnsignedDecimal.ofDecimalUnsafe (left.SignPlus() * right.SignPlus())

    member this.Value : decimal =
        match this with
        | PositivePercentage value -> value

    static member ofDecimal (num: decimal) : Option<PositivePercentage> =
        // TODO @Tejas Update Percentage modeling. Percentage can be greather than 100
        if num >= LanguagePrimitives.DecimalWithMeasure 0m && num <= LanguagePrimitives.DecimalWithMeasure 100m then
            PositivePercentage num |> Some
        else
            None

    static member ofDecimalUnsafe (num: decimal) : PositivePercentage =
        match PositivePercentage.ofDecimal num with
        | Some v -> v
        | None   -> failwithf "Positive value expected, given: %f" num

    override i.ToString () : string = string i.Value


#if !FABLE_COMPILER

open FSharpPlus
open CodecLib

type PositivePercentage with
    static member TryParse (s: string) = tryParse s >>= PositivePercentage.ofDecimal
    static member get_Codec () =
        Codec.create (PositivePercentage.TryParse >> Result.ofOption (Uncategorized "Failed parsing")) string<PositivePercentage>
        |> Codec.compose Codecs.string

#endif