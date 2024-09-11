[<AutoOpen>]
module MimeTypeModule

type MimeType = private MimeType of string
with
    member this.Value : string =
        match this with
        | MimeType value -> value

    member this.IsImage : bool =
        match this with
        | MimeType value -> value.StartsWith ("image/")


[<RequireQualifiedAccess>]
module MimeType =
    let ofString (candidate: string) : Option<MimeType> =
        match candidate with
        | null  -> None

        (* MIME type is blank for some application specific file types (like .ovpn etc).
        For now, it seems safe to assume that they are plain text files. *)
        | ""    -> Some (MimeType "text/plain")
        | value ->
            if value.Length > 127 then
                failwithf "MimeType cannot be longer than 127 chars: %s" value
            Some (MimeType value)


module MimeTypes =
    let ``text/csv`` =
        MimeType.ofString "text/csv" |> Option.get


// Codecs & casts

#if !FABLE_COMPILER

open CodecLib
open FSharpPlus

type MimeType with
    static member get_Codec () : Codec<_, MimeType> =
        Codec.create (MimeType.ofString >> Option.fold (konst Ok) Decode.Fail.nullString) (fun (x: MimeType) -> x.Value)
        |> Codec.compose Codecs.string

#endif
