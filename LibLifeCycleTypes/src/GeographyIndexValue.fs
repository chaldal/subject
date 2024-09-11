[<AutoOpen>]
module
#if FABLE_COMPILER
    // See comment in LibLifeCycleTypes/AssemblyInfo.fs
    LibLifeCycleTypes_GeographyIndexValue
#else
    LibLifeCycleTypes.GeographyIndexValue
#endif

type GeographyRing = private { Points: NonemptyList<GeoLocation> }
with
    static member OfPoints(points: seq<GeoLocation>) =
        match NonemptyList.ofSeq points with
        | None ->
            Error "no points specified, must be at least three points"
        | Some points ->
            if points.Length < 3 then
                Error "less than three points specified"
            else
                Ok { Points = points }

    static member OfPointsUnsafe (points: seq<GeoLocation>) =
        match GeographyRing.OfPoints points with
        | Ok ring -> ring
        | Error err -> failwith err

type GeographyPolygon = {
    // from https://en.wikipedia.org/wiki/Well-known_text_representation_of_geometry, SQL server expects that too for geospatial functions to work correctly
    // The OGC standard definition requires a polygon to be topologically closed. It also states that if the exterior linear ring of a polygon is defined
    // in a counterclockwise direction, then it will be seen from the "top".
    // Any interior linear rings should be defined in opposite fashion compared to the exterior ring, in this case, clockwise
    ExteriorCounterclockwiseRing: GeographyRing
    InteriorClockwiseRings: list<GeographyRing>
}

type GeographyMultiPolygon = private { Polygons: NonemptyList<GeographyPolygon> }
with
    static member OfPolygons(polygons: seq<GeographyPolygon>) =
        match NonemptyList.ofSeq polygons with
        | None ->
            Error "no polygons specified, must be at least two"
        | Some polygons ->
            if polygons.Length < 2 then
                Error "less than two polygons specified"
            else
                Ok { Polygons = polygons }

    static member OfPolygonsUnsafe (polygons: seq<GeographyPolygon>) =
        match GeographyMultiPolygon.OfPolygons polygons with
        | Ok multiPolygon -> multiPolygon
        | Error err -> failwith err

type GeographyIndexValue =
| Point of GeoLocation
| MultiPoint of Point1: GeoLocation * NextPoints: NonemptyList<GeoLocation>
| Polygon of GeographyPolygon
| MultiPolygon of GeographyMultiPolygon
// | ... add more when required
with
    // NB: from the documentation the parameter order is (longitude, latitude) for WKT point
    // https://learn.microsoft.com/en-us/sql/t-sql/spatial-geography/spatial-types-geography?view=sql-server-ver16#a-showing-how-to-add-and-query-geography-data
    static member private GeoLocationWkt(GeoLocation (lat, lon)) = $"%.6f{lon} %.6f{lat}"
    static member private PolygonWkt (p: GeographyPolygon) =
        (p.ExteriorCounterclockwiseRing :: p.InteriorClockwiseRings)
        |> Seq.map (fun c ->
            System.String.Join(",", c.Points.ToList @ [c.Points.Last] |> Seq.map GeographyIndexValue.GeoLocationWkt)
            |> fun pointsCsv -> $"({pointsCsv})")
        |> fun rings -> System.String.Join(",", rings)
        |> fun ringsCsv -> $"({ringsCsv})"

    member this.ToWkt() : string =
        match this with
        | Point loc ->
            $"POINT({GeographyIndexValue.GeoLocationWkt loc})"
        | MultiPoint (point1, nextPoints) ->
            System.String.Join(",", (point1 :: nextPoints.ToList) |> Seq.map GeographyIndexValue.GeoLocationWkt)
            |> fun pointsCsv ->
                $"MULTIPOINT({pointsCsv})"
        | Polygon p ->
            $"POLYGON{GeographyIndexValue.PolygonWkt p}"
        | MultiPolygon mp ->
            System.String.Join(",", mp.Polygons.ToList |> Seq.map GeographyIndexValue.PolygonWkt)
            |> fun polygonsCsv -> $"MULTIPOLYGON({polygonsCsv})"

// CODECs

#if !FABLE_COMPILER

open CodecLib

type GeographyRing with
    static member private get_ObjCodec_V1 () =
        codec {
            let! _version = reqWith Codecs.int "__v1" (fun _ -> Some 0)
            and! points = reqWith (NonemptyList.codec codecFor<_, GeoLocation>) "Pts" (fun x -> Some x.Points)
            return { Points = points }
        }
    static member get_Codec () = ofObjCodec (GeographyRing.get_ObjCodec_V1 ())

type GeographyPolygon with
    static member private get_ObjCodec_V1 () =
        codec {
            let! _version = reqWith Codecs.int "__v1" (fun _ -> Some 0)
            and! exteriorRing = reqWith codecFor<_, GeographyRing> "Exterior" (fun x -> Some x.ExteriorCounterclockwiseRing)
            and! interiorRings = reqWith (Codecs.list codecFor<_, GeographyRing>) "Interior" (fun x -> Some x.InteriorClockwiseRings)
            return { ExteriorCounterclockwiseRing = exteriorRing; InteriorClockwiseRings =  interiorRings }
        }
    static member get_Codec () = ofObjCodec (GeographyPolygon.get_ObjCodec_V1 ())

type GeographyMultiPolygon with
    static member private get_ObjCodec_V1 () =
        codec {
            let! _version = reqWith Codecs.int "__v1" (fun _ -> Some 0)
            and! polygons = reqWith (NonemptyList.codec codecFor<_, GeographyPolygon>) "Polygons" (fun x -> Some x.Polygons)
            return { Polygons = polygons }
        }
    static member get_Codec () = ofObjCodec (GeographyMultiPolygon.get_ObjCodec_V1 ())

type GeographyIndexValue with
    static member private get_ObjCodec_V1 () =
        function
        | Point _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Point _ -> Some 0 | _ -> None)
                and! payload = reqWith codecFor<_, GeoLocation> "Point" (function Point x -> Some x | _ -> None)
                return Point payload
            }
        | MultiPoint _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function MultiPoint _ -> Some 0 | _ -> None)
                and! payload = reqWith (Codecs.tuple2 codecFor<_, GeoLocation> (NonemptyList.codec codecFor<_, GeoLocation>)) "MultiPoint" (function MultiPoint (x1, x2) -> Some (x1, x2) | _ -> None)
                return MultiPoint payload
            }
        | Polygon _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Polygon _ -> Some 0 | _ -> None)
                and! payload = reqWith codecFor<_, GeographyPolygon> "Polygon" (function Polygon x -> Some x | _ -> None)
                return Polygon payload
            }
        | MultiPolygon _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function MultiPolygon _ -> Some 0 | _ -> None)
                and! payload = reqWith codecFor<_, GeographyMultiPolygon> "MultiPolygon" (function MultiPolygon x -> Some x | _ -> None)
                return MultiPolygon payload
            }
        |> mergeUnionCases
    static member get_Codec () = ofObjCodec (GeographyIndexValue.get_ObjCodec_V1 ())

#endif