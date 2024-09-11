module LibLifeCycleHost.Web.WebApiJsonEncoding

open System
open System.IO
open System.Threading
open LibLifeCycleTypes.File
open LibLangFsharp.BloomFilter
open System.Text.Json
open System.Text.Json.Serialization

// The objective is this module is to provide a fast Thoth.Json-compatible JSON serializer
// In my (un-) scientific benchmarks, Thoth.JSON.NET is atleast 20x slower than System.Text.Json
// Eventually we want to use Codecs for JSON ser/de here, but Codecs are still about 10x slower than
// System.Text.Json straight-up.
// Utf8Json is the fastest nuget package available for JSON (it's 4x faster than System.Text.Json), however
// it requires quite a bit of effort to make the output Thoth.Json compatible. For System.Text.Json, we have
// a ready-made FSharp.SystemTextJson package that is configurable with the output that we need.

// Thoth.Json formats some large numbers as strings to preserve them.
type DecimalJsonConverter() =
    inherit JsonConverter<decimal>()

    override this.Read(reader, _typeToConvert, _options) =
        reader.GetString()
        |> Decimal.Parse

    override this.Write(writer, value, _options) =
        value.ToString()
        |> writer.WriteStringValue

type ByteArrayJsonConverter() =
    inherit JsonConverter<byte[]>()

    override this.Read(reader, _typeToConvert, _options) =
        reader.GetString()
        |> Convert.FromBase64String

    override this.Write(writer, value, _options) =
        Convert.ToBase64String value
        |> writer.WriteStringValue

type Int64JsonConverter() =
    inherit JsonConverter<int64>()

    override this.Read(reader, _typeToConvert, _options) =
        reader.GetString()
        |> Int64.Parse

    override this.Write(writer, value, _options) =
        value.ToString()
        |> writer.WriteStringValue


type UInt64JsonConverter() =
    inherit JsonConverter<uint64>()

    override this.Read(reader, _typeToConvert, _options) =
        reader.GetString()
        |> UInt64.Parse

    override this.Write(writer, value, _options) =
        value.ToString()
        |> writer.WriteStringValue

// This needs to be cleaned up; Ideally a custom JsonConverter
// so the framework needs to expose some hooks. Ideally we should just move to
// JSON Codecs.
type StringBloomFilterJsonConverter() =
    inherit JsonConverter<StringBloomFilter>()

    override this.Read(reader, _typeToConvert, options) =
        let bytes = JsonSerializer.Deserialize<byte[]>(&reader, options)
        StringBloomFilter.DeserializeEmpty bytes

    override this.Write(writer, value, options) =
        // We don't support encoding the contents of the bitarray, as it's simply too large
        let bytes = BloomFilter.serializeEmpty value
        JsonSerializer.Serialize(writer, bytes, options)

let defaultFsharpJsonConverter =
    JsonFSharpConverter(
        unionEncoding = (
            JsonUnionEncoding.InternalTag |||
            JsonUnionEncoding.UnwrapOption |||
            JsonUnionEncoding.UnwrapFieldlessTags
        )
    )

// There's an issue with FSharp.SystemTextJson where it automatically unwraps Map key types of single-case DUs containing
// a single string, even when configured not to.
// So this is a copy-paste of the converter from that code base that excludes this unnecessary unwrapping
type FixedUpJsonMapConverter() =
    inherit JsonConverterFactory()

    static member internal CanConvert(typeToConvert: Type) =
        TypeCache.isMap typeToConvert

    static member internal CreateConverter(typeToConvert: Type) =
        let genArgs = typeToConvert.GetGenericArguments()
        let ty =
            if genArgs.[0] = typeof<string> then
                typedefof<JsonStringMapConverter<_>>
                    .MakeGenericType([|genArgs.[1]|])
            else
                typedefof<JsonMapConverter<_,_>>
                    .MakeGenericType(genArgs)
        ty.GetConstructor([||])
            .Invoke([||])
        :?> JsonConverter

    override _.CanConvert(typeToConvert) =
        FixedUpJsonMapConverter.CanConvert(typeToConvert)

    override _.CreateConverter(typeToConvert, _options) =
        FixedUpJsonMapConverter.CreateConverter(typeToConvert)


type DateOnlyJsonConverter() =
    inherit JsonConverter<DateOnly>()
    let formatSpecifier = "yyyy-M-d"

    override this.Read(reader, _typeToConvert, _options) =
        (reader.GetString(), formatSpecifier)
        |> DateOnly.ParseExact

    override this.Write(writer, value, _options) =
        value.ToString(formatSpecifier)
        |> writer.WriteStringValue

type TimeOnlyJsonConverter() =
    inherit JsonConverter<TimeOnly>()
    let formatSpecifier = "H:m:s"

    override this.Read(reader, _typeToConvert, _options) =
        (reader.GetString(), formatSpecifier)
        |> TimeOnly.ParseExact

    override this.Write(writer, value, _options) =
        value.ToString(formatSpecifier)
        |> writer.WriteStringValue

let jsonSerializerOptions = JsonSerializerOptions()
jsonSerializerOptions.Converters.Add (DecimalJsonConverter())
jsonSerializerOptions.Converters.Add (Int64JsonConverter())
jsonSerializerOptions.Converters.Add (UInt64JsonConverter())
jsonSerializerOptions.Converters.Add (ByteArrayJsonConverter())
jsonSerializerOptions.Converters.Add (StringBloomFilterJsonConverter())
jsonSerializerOptions.Converters.Add (FixedUpJsonMapConverter()) // Note this needs to come before the default converters below
jsonSerializerOptions.Converters.Add (DateOnlyJsonConverter())
jsonSerializerOptions.Converters.Add (TimeOnlyJsonConverter())
jsonSerializerOptions.Converters.Add defaultFsharpJsonConverter

// Override FileData to capture uploaded bytes and replace with a reference
module FileDataJson =

    type FileDataCaptureContext = {
        HandlerName: string
        CapturedDatas: System.Collections.Generic.Dictionary<Guid, byte[]>
    }

    let capturedFileDatas = AsyncLocal<Option<FileDataCaptureContext>>()

    type FileDataJsonConverter() =
        inherit JsonConverter<FileData>()

        let defaultFileDataConverter =
            defaultFsharpJsonConverter.CreateConverter(typeof<FileData>, jsonSerializerOptions)
            :?> JsonConverter<FileData>

        override this.Read(reader, typeToConvert, options) =
            let fileData = defaultFileDataConverter.Read(&reader, typeToConvert, options)
            match fileData with
            | FileData.InternalOnlyTransferBlob _ ->
                failwith "FileData.InternalOnlyTransferBlob is unsupported for JSON Desrialization"
            | _ ->
                match capturedFileDatas.Value with
                | Some captureCtx ->
                    let bytes = fileData.ToBytes
                    let transferBlobId = Guid.NewGuid()
                    captureCtx.CapturedDatas.Add(transferBlobId, bytes)
                    FileData.InternalOnlyTransferBlob(captureCtx.HandlerName, transferBlobId, bytes.Length * 1<B>)
                | None ->
                    fileData

        override this.Write(writer, value, options) =
            match value with
            | FileData.InternalOnlyTransferBlob _ ->
                failwith "FileData.InternalOnlyTransferBlob is unsupported for JSON Serialization"
            | _ ->
                defaultFileDataConverter.Write(writer, value, options)

jsonSerializerOptions.Converters.Add (FileDataJson.FileDataJsonConverter())

type Encodable = private Encodable of Type * obj // Erase generic type
type Encoder<'T> =  'T -> Encodable
type Decoder<'T> = private Decoder of WillAlwaysBeNone: Option<'T> // This is just a marker type

let generateAutoEncoder<'T> : Encoder<'T> =
    fun t ->
        Encodable(typeof<'T>, box t)

let generateAutoDecoder<'T> : Decoder<'T> =
    Decoder None

module Encode =
    let toStream (stream: Stream) (Encodable(typ, value)) =
        JsonSerializer.SerializeAsync(stream, value, typ, jsonSerializerOptions)

    let toString (Encodable(typ, value)) =
        JsonSerializer.Serialize(value, typ, jsonSerializerOptions)

module Decode =
    let fromStream (_: Decoder<'T>) (stream: Stream) : System.Threading.Tasks.Task<Result<'T, string>> =
        task { // Should be backgroundTask, but I can't seem to ! a ValueTask here
            try
                let! res = JsonSerializer.DeserializeAsync<'T>(stream, jsonSerializerOptions)
                return Ok res
            with
            | ex ->
                return Error (ex.Message)
        }

    let fromString (_: Decoder<'T>) (json: string) : Result<'T, string> =
        try
            let res = JsonSerializer.Deserialize<'T>(json, jsonSerializerOptions)
            Ok res
        with
        | ex ->
            Error (ex.Message)