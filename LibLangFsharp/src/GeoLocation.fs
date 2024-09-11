[<AutoOpen; CodecLib.CodecAutoGenerate>]
module GeoLocationModule

type GeoLocation =
| GeoLocation of Latitude: decimal * Longitude: decimal
with
    member this.Lat : decimal =
        match this with
        | GeoLocation (lat, _) -> lat

    member this.Lng : decimal =
        match this with
        | GeoLocation (_, lng) -> lng

    member this.AsTuple : decimal * decimal =
        (this.Lat, this.Lng)

// CODECs

#if !FABLE_COMPILER

open CodecLib

type GeoLocation with
    static member private get_ObjCodec_V1 () =
        function
        | GeoLocation _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function GeoLocation _ -> Some 0)
                and! payload = reqWith (Codecs.tuple2 Codecs.decimal Codecs.decimal) "GeoLocation" (function (GeoLocation (x1, x2)) -> Some (x1, x2))
                return GeoLocation payload
            }
        |> mergeUnionCases
    static member get_Codec () = ofObjCodec (GeoLocation.get_ObjCodec_V1 ())

#endif
