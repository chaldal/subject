[<AutoOpen>]
module CodecLib.StjCodecs.Extensions

open CodecLib

exception CodecCheckException of string
with
    override this.Message = this.Data0

let unindentedOptions =
    let mutable o = System.Text.Json.JsonWriterOptions()
    o.Indented <- false
    o.SkipValidation <- true // to speed it up
    o

/// Get the json representation of the value, using its default codec.
/// Checks that encoded value can be decoded without a loss.
let inline toJsonTextChecked (x: 'T) : string =
    let jsonValue = (toJson x).ToString(unindentedOptions)
    match ofJsonText jsonValue with
    | Ok (x' : 'T) ->
        if not (System.Object.Equals (x, x')) then
            sprintf "Decoded value is not the same as original. Original: %A. Decoded: %A. Json value: %s" x x' jsonValue
            |> CodecCheckException
            |> raise
    | Error err ->
        sprintf "Unable to decode the encoded value. Original: %A. Error: %A. Json value: %s" x err jsonValue
        |> CodecCheckException
        |> raise
    jsonValue

let inline toJsonTextUnchecked (x: 'T) : string =
    (toJson x).ToString(unindentedOptions)

/// Get the json representation of the value, using explicit codec.
/// Checks that encoded value can be decoded without a loss.
let inline toJsonTextCheckedWithCodec (codec: Codec<StjEncoding, 'T>) (x: 'T) : string =
    let jsonValue = Codec.encode codec x
    match Codec.decode codec jsonValue with
    | Ok (x' : 'T) ->
        if not (System.Object.Equals (x, x')) then
            sprintf "Decoded value is not the same as original. Original: %A. Decoded: %A" x x'
            |> CodecCheckException
            |> raise
    | Error err ->
        sprintf "Unable to decode the encoded value. Original: %A. Error: %A. Json value: %A" x err jsonValue
        |> CodecCheckException
        |> raise
    jsonValue.ToString(unindentedOptions)