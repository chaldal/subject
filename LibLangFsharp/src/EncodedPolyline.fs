[<AutoOpen; CodecLib.CodecAutoGenerate>]
module EncodedPolylineModule

open System.Text

// Variation of Google's Encoded Polyline Algorithm with 6 digits of precision
// https://developers.google.com/maps/documentation/utilities/polylinealgorithm

[<RequireQualifiedAccess>]
type EncodedPolyline = | Precision6 of Shape: string

module EncodedPolyline =

    // Similar to Valhalla's Implementation (MIT license)
    // https://github.com/valhalla/valhalla/blob/master/valhalla/midgard/encoded.h
    let ofLocations (locations: array<GeoLocation>) : EncodedPolyline =

        // checking if values in locations is within range, otherwise fail early
        locations
        |> Array.iter (fun location ->
            if location.Lat < - 90.0m && 90.0m  < location.Lat then failwithf $"Latitude value {location.Lat} is out of range [-90, 90]"
            if location.Lng < -180.0m && 180.0m < location.Lng then failwithf $"Longitude value {location.Lng} is out of range [-180, 180]"
        )

        let sb = StringBuilder()
        let serialize (number: int32) : unit =
            let mutable number: uint32 = if number < 0 then ~~~((number <<< 1) |> uint32) else (number <<< 1) |> uint32

            while number >= 0x20ul do
                let nextValue: uint32 = (0x20ul ||| (number &&& 0x1ful)) + 63u
                sb.Append (char nextValue) |> ignore
                number <- number >>> 5

            sb.Append (char (number + 63u)) |> ignore

        locations
        |> Array.fold (fun (lastLat, lastLng) (location: GeoLocation) ->
            let lat: int32 = location.Lat * 1e6M |> round |> int32
            let lng: int32 = location.Lng * 1e6M |> round |> int32
            serialize (lat - lastLat)
            serialize (lng - lastLng)
            (lat, lng)
        ) (0, 0)
        |> ignore
        sb.ToString () |> EncodedPolyline.Precision6


    // Supports decoding of shape from valhalla api
    // https://valhalla.github.io/valhalla/decoding/
    let toLocations ((EncodedPolyline.Precision6 shape): EncodedPolyline) : array<GeoLocation> =

        let transformToSigned (number: uint32) : int32 =
            if number &&& 1u = 1u then ~~~((number >>> 1) |> int) else ((number >>> 1) |> int)

        shape.ToCharArray ()
        |> Array.map (fun c -> (int c) - 63)
        |> Array.map uint32
        |> Array.fold (fun (shift: int, result: uint32, reversedResults: list<int>) (byte: uint32) ->
            if byte >= 0x20ul then
                shift + 5, result ||| ((byte &&& 0x1ful) <<< shift), reversedResults
            else
                0, 0ul, ((result ||| ((byte &&& 0x1ful) <<< shift)) |> transformToSigned) :: reversedResults
        ) (0, 0ul, [])
        |> fun (_, _, reversedResults) ->
            reversedResults
        |> List.toArray
        |> Array.rev
        |> fun numbers ->
            if numbers.Length % 2 = 1 then
                failwithf "INVALID Polyline format: Odd number of numbers"
            else
                numbers
                |> Array.chunkBySize 2// safe because array length is even
                |> Array.mapFold (fun (lastLat: int, lastLng: int) latLng ->
                    let lat = (lastLat + latLng[0])
                    let lng = (lastLng + latLng[1])
                    GeoLocation((lat |> decimal) / 1e6M , (lng |> decimal) / 1e6M), (lat, lng)
                ) (0, 0)
                |> fst



#if !FABLE_COMPILER

open CodecLib
type EncodedPolyline with
    static member private get_ObjCodec_AllCases () =
        function
        | Precision6 _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Precision6 _ -> Some 0)
                and! payload = reqWith Codecs.string "Precision6" (function Precision6 x -> Some x)
                return Precision6 payload
            }
        |> mergeUnionCases
    static member get_Codec () = ofObjCodec (EncodedPolyline.get_ObjCodec_AllCases ())

#endif // FABLE_COMPILER