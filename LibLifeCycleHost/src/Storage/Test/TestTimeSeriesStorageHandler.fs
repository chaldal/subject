[<AutoOpen>]
module LibLifeCycleHost.Storage.Test.TimeSeriesHandler

open System
open System.Collections.Concurrent
open System.Threading.Tasks
open LibLifeCycleCore
open LibLifeCycleHost

type TestTimeSeriesDataPointKey = {
    TimeIndex: DateTimeOffset
    IndexKey: string
    IndexVal: string
    Hash: int
}

let private murmur32 = Murmur.MurmurHash.Create32()

#nowarn "0686"  // Allow exceptionally here to have explicit parameters for codec related functions

let inline private toJsonTextWithCodec (codec: CodecLib.Codec<_, 't>) (x: 't) =
    x
    // FIXME - toJsonTextChecked is ~2x as expensive as toJsonText. Need to find cheaper way to check integrity of serialized data
    |> CodecLib.StjCodecs.Extensions.toJsonTextCheckedWithCodec<'t> codec

type TestTimeSeriesDb<'TimeSeriesDataPoint, 'TimeSeriesId, [<Measure>] 'UnitOfMeasure, 'TimeSeriesIndex
                       when 'TimeSeriesDataPoint :> TimeSeriesDataPoint<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure>
                       and  'TimeSeriesId :> TimeSeriesId<'TimeSeriesId>
                       and  'TimeSeriesIndex :> TimeSeriesIndex<'TimeSeriesIndex>> = {
    LockObj: obj
    mutable TimeSeriesByIdStr: Map<string, Map<TestTimeSeriesDataPointKey, 'TimeSeriesDataPoint>>
}

type TestTimeSeriesStorageHandler<'TimeSeriesDataPoint, 'TimeSeriesId, [<Measure>] 'UnitOfMeasure, 'TimeSeriesIndex
                                        when 'TimeSeriesDataPoint :> TimeSeriesDataPoint<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure>
                                        and  'TimeSeriesId :> TimeSeriesId<'TimeSeriesId>
                                        and  'TimeSeriesIndex :> TimeSeriesIndex<'TimeSeriesIndex>> (grainPartition: GrainPartition) =

    let (GrainPartition partitionGuid) = grainPartition

    let db () : TestTimeSeriesDb<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'TimeSeriesIndex> =
        let d : ConcurrentDictionary<Guid, TestTimeSeriesDb<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'TimeSeriesIndex>> = TestTimeSeriesStorageHandler<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'TimeSeriesIndex>.Db
        d.GetOrAdd(
            partitionGuid,
            fun _ ->
                {
                    LockObj = obj()
                    TimeSeriesByIdStr = Map.empty
                })

    static member val Db = ConcurrentDictionary<Guid, TestTimeSeriesDb<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'TimeSeriesIndex>>() with get

    interface ITimeSeriesStorageHandler<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'TimeSeriesIndex> with
        member this.Save data =
            let db = db()

            // TODO: can we do better and cache implicitly everywhere where get_Codec() is defined?
            let codec = Fleece.Internals.CodecCache<Fleece.Internals.OpCodec, Fleece.SystemTextJson.Encoding, _>.Run(
                fun () -> 'TimeSeriesDataPoint.Codec())

            lock db.LockObj
                (fun _ ->
                    let pointsByIdStr =
                        data.Points.ToList
                        |> List.groupBy (fun p -> p.Point.Id.IdString)
                    pointsByIdStr
                    |> List.fold (
                        fun (tsByIdStr: Map<string, _>) (group: string * list<TimeSeriesDataPointToSave<_, _, _, _>>) ->
                            let (idStr, points) = group
                            let dataPointsMap = tsByIdStr |> Map.tryFind idStr |> Option.defaultValue Map.empty

                            // TODO: reuse index str, hash, validation logic between Sql and Test
                            let dataPointsMap =
                                points
                                |> List.fold (
                                    fun (dataPointsMap: Map<TestTimeSeriesDataPointKey, 'TimeSeriesDataPoint>) (p: TimeSeriesDataPointToSave<_, _, _, _>) ->

                                        let dataPointJson = toJsonTextWithCodec codec p.Point
                                        let dataPoint = gzipCompressUtf8String dataPointJson
                                        let hash = BitConverter.ToInt32(murmur32.ComputeHash dataPoint, 0)

                                        let newValues =
                                            p.Indices
                                            // index rows
                                            |> Seq.map (fun i -> (UnionCase.ofCase i).CaseName, i.PrimitiveValue)
                                            // "parent" row
                                            |> Seq.append [("", "")]
                                            |> Seq.map (fun (indexKey, indexVal) ->
                                                { IndexKey = indexKey; IndexVal = indexVal; TimeIndex = p.Point.TimeIndex; Hash = hash }, p.Point)
                                            |> Map.ofSeq

                                        Map.merge dataPointsMap newValues)
                                    dataPointsMap

                            tsByIdStr.AddOrUpdate (idStr, dataPointsMap))
                        db.TimeSeriesByIdStr
                        |> fun timeSeriesByIdStr ->
                            db.TimeSeriesByIdStr <- timeSeriesByIdStr)
            Task.CompletedTask