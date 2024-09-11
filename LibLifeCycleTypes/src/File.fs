[<CodecLib.CodecAutoGenerate>]
module LibLifeCycleTypes.File

open System
open System.Text

[<Measure>] type B
[<Measure>] type KB
[<Measure>] type MB

let bToKB (b: int<B>) : int<KB> = b / 1024<B/KB>
let kBToB (kb: int<KB>) : int<B> = kb * 1024<B/KB>
let kBToMB (kb: int<KB>) : int<MB> = kb / 1024<KB/MB>
let mBToKB (mb: int<MB>) : int<KB> = mb * 1024<KB/MB>

let asB (value: int) : int<B> = value * 1<B>

[<RequireQualifiedAccess>]
type FileData =
| Bytes  of byte[]
| Base64 of string * Size: int<B>
#if !FABLE_COMPILER // Disabled on frontend
| InternalOnlyTransferBlob of TransferBlobHandlerName: string * TransferBlobId: Guid * Size: int<B>
#endif
with
    member this.ToBase64 : string =
        match this with
        | Bytes bytes       -> Convert.ToBase64String bytes
        | Base64 (value, _) -> value
        #if !FABLE_COMPILER
        | InternalOnlyTransferBlob _ ->
            failwith "ToBase64 unsupported on InternalOnlyTransferBlob"
        #endif

    member this.ToBytes : byte [] =
        match this with
        | FileData.Bytes bytes        -> bytes
        | FileData.Base64 (encoded, _) -> Convert.FromBase64String encoded
        #if !FABLE_COMPILER
        | InternalOnlyTransferBlob _ ->
            failwith "ToBytes unsupported on InternalOnlyTransferBlob"
        #endif

type File = {
    MimeType: MimeType
    Data:     FileData
} with
    static member redact (file: File) : File =
        { MimeType = file.MimeType; Data = FileData.Bytes [||] }

    member this.ToDataUri : string =
        "data:" + this.MimeType.Value + ";base64," + this.Data.ToBase64

    member this.Bytes : byte[] =
        this.Data.ToBytes

type NamedFile = {
    File: File
    Name: NonemptyString
}

type HttpFile = {
    File:                  File
    MaybeTextEncodingName: Option<string>
    MaybeDownloadFileName: Option<string>
}
#if !FABLE_COMPILER
with
    member this.MaybeTextEncoding = this.MaybeTextEncodingName |> Option.map Encoding.GetEncoding
#endif

[<RequireQualifiedAccess>]
module HttpFile =
    let ofFile (file: File) = { File = file; MaybeDownloadFileName = None; MaybeTextEncodingName = None }

#if !FABLE_COMPILER
    let withEncoding (encoding: Encoding) (httpFile: HttpFile) = { httpFile with MaybeTextEncodingName = Some encoding.WebName }
#endif

    let withDownloadFileName (name: string) (httpFile: HttpFile) = { httpFile with MaybeDownloadFileName = Some name }

////////////////////////////////
//    Hand-written condecs    //
////////////////////////////////
#if !FABLE_COMPILER

open CodecLib

type FileData with
    static member get_ObjCodec_V1 () =
        function
        | Bytes _ ->
            codec {
                let! _version = reqWith Codecs.int "__v2" (function Bytes _ -> Some 0 | _ -> None)
                and! payload = reqWith Codecs.base64Bytes "Bytes" (function (Bytes x) -> Some x | _ -> None)
                return Bytes payload
            }
            |> withDecoders [
                decoder {
                    let! _version = reqDecodeWithCodec Codecs.int "__v1"
                     // (Codecs.array Codecs.byte) is VERY slow on large arrays, evolved to base64Bytes
                    and! payload = reqDecodeWithCodec (Codecs.array Codecs.byte) "Bytes"
                    return Bytes payload
                }
            ]
        | Base64 _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Base64 _ -> Some 0 | _ -> None)
                and! (value, size) = reqWith (Codecs.tuple2 Codecs.string Codecs.uint32) "Base64" (function (Base64 (value, size)) -> Some (value, uint32 (size / 1<B>)) | _ -> None)
                return Base64 (value, (int size) * 1<B>)
            }
        | InternalOnlyTransferBlob _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function InternalOnlyTransferBlob _ -> Some 0 | _ -> None)
                and! (handlerName, transferBlobId, size) = reqWith (Codecs.tuple3 Codecs.string Codecs.guid Codecs.uint32) "InternalOnlyTransferBlob" (function (InternalOnlyTransferBlob (handlerName, transferBlobId, size)) -> Some (handlerName, transferBlobId, uint32 (size / 1<B>)) | _ -> None)
                return InternalOnlyTransferBlob (handlerName, transferBlobId, (int size) * 1<B>)
            }
        |> mergeUnionCases

    static member get_Codec () = ofObjCodec (FileData.get_ObjCodec_V1 ())


type File with
    static member get_ObjCodec_V1 () =
        codec {
            let! _version = reqWith Codecs.int "__v1" (fun _ -> Some 0)
            and! mimeType = reqWith (MimeType.get_Codec ()) "MimeType" (fun x -> Some x.MimeType)
            and! data = reqWith (FileData.get_Codec ()) "Data" (fun x -> Some x.Data)
            return { MimeType = mimeType; Data = data }
        }
    static member get_Codec () = ofObjCodec (File.get_ObjCodec_V1 ())

type HttpFile with
    static member get_ObjCodec_V1 () =
        codec {
            let! _version = reqWith Codecs.int "__v1" (fun _ -> Some 0)
            and! file = reqWith codecFor<_, File> "File" (fun x -> Some x.File)
            and! maybeTextEncodingName = reqWith (Codecs.option Codecs.string) "Encoding" (fun x -> Some x.MaybeTextEncodingName)
            and! maybeDownloadFileName = reqWith (Codecs.option Codecs.string) "Encoding" (fun x -> Some x.MaybeDownloadFileName)

            return { File = file; MaybeTextEncodingName = maybeTextEncodingName; MaybeDownloadFileName = maybeDownloadFileName }
        }
    static member get_Codec () = ofObjCodec (HttpFile.get_ObjCodec_V1 ())

type NamedFile with
    static member get_ObjCodec_V1 () =
        codec {
            let! _version = reqWith Codecs.int "__v1" (fun _ -> Some 0)
            and! file = reqWith codecFor<_, File> "File" (fun x -> Some x.File)
            and! name = reqWith codecFor<_, NonemptyString> "Name" (fun x -> Some x.Name)

            return { File = file; Name = name }
        }
    static member get_Codec () = ofObjCodec (NamedFile.get_ObjCodec_V1 ())

#endif
