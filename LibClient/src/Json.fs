[<AutoOpen>]
module LibClient.Json

open System
open System.Globalization
open Thoth.Json

module private Char =
    let decoder : Decoder<char> =
        Decode.object (
            fun get ->
                get.Required.Raw<string> Decode.string
                |> fun s -> s.[0]
            )

    let encoder : Encoder<char> =
        sprintf "%c" >> Encode.string

module private Uint16 =
    let decoder : Decoder<uint16> =
        Decode.object (
            fun get ->
                get.Required.Raw<int> Decode.int
                |> uint16
            )

    let encoder : Encoder<uint16> =
        int >> Encode.int

module private BigInteger =
    let decoder : Decoder<bigint> =
        Decode.object (
            fun get ->
                get.Required.Raw<string> Decode.string
                |> bigint.Parse
            )

    let encoder : Encoder<bigint> =
        string >> Encode.string

module private Byte =
    let decoder : Decoder<byte> =
        Decode.object (
            fun get ->
                get.Required.Raw<int> Decode.int
                |> byte
            )

    let encoder : Encoder<byte> =
        int >> Encode.int

module private ByteArray =
    let decoder : Decoder<byte[]> =
        Decode.object (
            fun get ->
                get.Required.Raw<string> Decode.string
                |> System.Convert.FromBase64String
            )

    let encoder : Encoder<byte[]> =
        System.Convert.ToBase64String >> Encode.string

module private DateOnly =
    let formatSpecifier = "yyyy-M-d"
    let toString (dateOnly: DateOnly) =
        $"{dateOnly.Year}-{dateOnly.Month}-{dateOnly.Day}"
    let decoder : Decoder<DateOnly> =
        Decode.object (
            fun get ->
                get.Required.Raw<string> Decode.string
                |> DateOnly.Parse
            )

    let encoder : Encoder<DateOnly> =
        toString >> Encode.string

module private TimeOnly =
    let toString (timeOnly: TimeOnly) =
        $"{timeOnly.Hour}:{timeOnly.Minute}:{timeOnly.Second}"
    let decoder : Decoder<TimeOnly> =
        Decode.object (
            fun get ->
                get.Required.Raw<string> Decode.string
                |> TimeOnly.Parse
            )

    let encoder : Encoder<TimeOnly> =
        toString >> Encode.string

let (* want private *) extras =
    Extra.empty
    |> Extra.withDecimal
    |> Extra.withUInt64
    |> Extra.withInt64
    |> Extra.withCustom Char.encoder       Char.decoder
    |> Extra.withCustom Uint16.encoder     Uint16.decoder
    |> Extra.withCustom BigInteger.encoder BigInteger.decoder
    |> Extra.withCustom Byte.encoder       Byte.decoder
    |> Extra.withCustom ByteArray.encoder  ByteArray.decoder
    |> Extra.withCustom DateOnly.encoder   DateOnly.decoder
    |> Extra.withCustom TimeOnly.encoder   TimeOnly.decoder

type Json() =
    static member inline ToString<'T> (value: 'T) : string =
        Encode.Auto.toString(0, value, CaseStrategy.PascalCase, extras)

    static member inline ToStringWithIndent<'T> (indent: int) (value: 'T) : string =
        Encode.Auto.toString(indent, value, CaseStrategy.PascalCase, extras)

    static member inline FromString<'T> (source: string) : Result<'T, string> =
        try
            // There's a case when Thoth throws instead of returning an Error
            // when you're parsing ["CaseName"] for a union case that has parameters.
            // Until this is fixed in the underlying library, we catch ourselves.
            Decode.Auto.fromString<'T>(source, CaseStrategy.PascalCase, extras)
        with
        | e -> Error e.Message
