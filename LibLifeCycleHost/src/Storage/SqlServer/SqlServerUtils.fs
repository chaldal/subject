[<AutoOpen>]
module LibLifeCycleHost.Storage.SqlServer.Utils

#nowarn "0686"  // Allow exceptionally here to have explicit parameters for codec related functions

open System
open CodecLib.StjCodecs

let int64ToUInt64MaintainOrder (signed: int64) = if signed < 0L then (signed + Int64.MaxValue + 1L |> uint64) else (uint64 signed) + (uint64 Int64.MaxValue) + 1UL
let uint64ToInt64MaintainOrder (unsigned: uint64) = if (unsigned <= uint64 Int64.MaxValue) then int64 (unsigned - uint64 Int64.MaxValue) - 1L else (int64 unsigned) - Int64.MaxValue - 1L

// SQL server timestamp is a Big Endian UInt64
let timestampToUInt64 (bytes: byte[]) = bytes |> Array.rev |> BitConverter.ToUInt64
let uInt64ToTimestamp (x: uint64) = x |> BitConverter.GetBytes |> Array.rev

module internal DataEncode =
    let inline toCompressedJson (x: 't) =
        x
        // FIXME - toJsonTextChecked is ~2x as expensive as toJsonText. Need to find cheaper way to check integrity of serialized data
        |> Extensions.toJsonTextChecked<'t>
        |> gzipCompressUtf8String

    let inline toCompressedJsonUnchecked (x: 't) =
        x
        |> Extensions.toJsonTextUnchecked
        |> gzipCompressUtf8String

    let inline toJsonTextWithCodec (codec: CodecLib.Codec<_, 't>) (x: 't) =
        x
        // FIXME - toJsonTextChecked is ~2x as expensive as toJsonText. Need to find cheaper way to check integrity of serialized data
        |> Extensions.toJsonTextCheckedWithCodec<'t> codec

    let inline ofJsonText (json: string) : Result<'t, _> =
        json
        |> ofJsonText

    let inline ofCompressedJsonText (bytes: byte[]) : Result<'t, _> =
        bytes
        |> gzipDecompressToUtf8String
        |> ofJsonText

    let inline ofCompressedJsonTextWithCodec (codec: CodecLib.Codec<StjEncoding, 't>) (bytes: byte[]) : Result<'t, _> =
        let json = gzipDecompressToUtf8String bytes
        let x = StjEncoding.Parse json
        CodecLib.Codec.decode codec x

    let decodeSubjectAuditOperation operationBytes =
        match
            operationBytes
            |> ofCompressedJsonText<LifeAction>
            |> Result.map (fun a -> a :?> 'LifeAction) with
        | Ok action ->
            action |> SubjectAuditOperation.Act |> Ok
        | Error actionError ->
            match
                operationBytes
                |> ofCompressedJsonText<Constructor>
                |> Result.map (fun a -> a :?> 'Constructor) with
            | Ok ctor ->
                ctor |> SubjectAuditOperation.Construct |> Ok
            | Error ctorError ->
                sprintf "Can't decode operation.\nAction decode error: %s\n\nCtor decode error: %s" (actionError.ToString()) (ctorError.ToString())
                |> Error