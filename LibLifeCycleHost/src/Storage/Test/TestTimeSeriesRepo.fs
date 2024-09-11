[<AutoOpen>]
module LibLifeCycleHost.Storage.Test.TimeSeriesRepo

open System
open System.Collections.Concurrent
open LibLifeCycleCore
#nowarn "0686"

open System.Threading.Tasks
open LibLifeCycle

type TestTimeSeriesRepo<'TimeSeriesDataPoint, 'TimeSeriesId, [<Measure>] 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex
    when 'TimeSeriesDataPoint :> TimeSeriesDataPoint<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure>
    and  'TimeSeriesId :> TimeSeriesId<'TimeSeriesId>
    and  'OpError :> OpError
    and  'TimeSeriesIndex :> TimeSeriesIndex<'TimeSeriesIndex>>
    (
        _timeSeries: ITimeSeries<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex>,
        grainPartition: GrainPartition
   ) as this =

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

    member private _.KeyMatchesInterval (interval: TimeSeriesInterval<'TimeSeriesId, 'TimeSeriesIndex>) (key: TestTimeSeriesDataPointKey) =
        let indexKey, indexVal =
            interval.Index
            |> Option.map (fun i ->
                // TODO: fix duplication
                // TODO: allow to override Case name somehow ?
                (UnionCase.ofCase i).CaseName, i.PrimitiveValue)
            |> Option.defaultValue ("", "")

        key.IndexKey = indexKey && key.IndexVal = indexVal &&
        (match interval.Start with | IncludeEndpoint start -> key.TimeIndex >= start | ExcludeEndpoint start -> key.TimeIndex > start) &&
        (match interval.End with | IncludeEndpoint end' -> key.TimeIndex <= end' | ExcludeEndpoint end' -> key.TimeIndex < end')

    member private _.KeyMatchesTimeGroupInterval (groupInterval: TimeSeriesGroupInterval<'TimeSeriesId, 'TimeSeriesIndex>) (key: TestTimeSeriesDataPointKey) =
        let indexKey = groupInterval.GroupBy.CaseName
        key.IndexKey = indexKey &&
        (match groupInterval.Start with | IncludeEndpoint start -> key.TimeIndex >= start | ExcludeEndpoint start -> key.TimeIndex > start) &&
        (match groupInterval.End with | IncludeEndpoint end' -> key.TimeIndex <= end' | ExcludeEndpoint end' -> key.TimeIndex < end')


    interface ITimeSeriesRepo<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'TimeSeriesIndex> with
        member _.Values (interval: TimeSeriesInterval<'TimeSeriesId, 'TimeSeriesIndex>) : Task<list<DateTimeOffset * float<'UnitOfMeasure>>> =
            let db = db ()
            // no need to lock because time series is append only
            match db.TimeSeriesByIdStr |> Map.tryFind interval.Id.IdString with
            | None ->
                []
            | Some points ->
                points
                |> Map.toSeq
                |> Seq.choose (fun (key, point) -> if this.KeyMatchesInterval interval key then Some (point.TimeIndex, point.Value) else None)
                |> Seq.sortBy fst
                |> List.ofSeq
            |> Task.FromResult

        member _.DataPoints (interval: TimeSeriesInterval<'TimeSeriesId, 'TimeSeriesIndex>) : Task<list<'TimeSeriesDataPoint>> =
            let db = db ()
            // no need to lock because time series is append only
            match db.TimeSeriesByIdStr |> Map.tryFind interval.Id.IdString with
            | None ->
                []
            | Some points ->
                points
                |> Map.toSeq
                |> Seq.choose (fun (key, point) -> if this.KeyMatchesInterval interval key then Some point else None)
                |> Seq.sortBy (fun pt -> pt.TimeIndex)
                |> List.ofSeq
            |> Task.FromResult

        member _.BucketedValues (_interval: TimeSeriesInterval<'TimeSeriesId, 'TimeSeriesIndex>) (_bucket: TimeBucket) (_aggregate: TimeBucketValueAggregate) : Task<list<DateTimeOffset * float<'UnitOfMeasure>>> =
            NotImplementedException "Bucket functions not implemented yet (SQL 2022 DATE_BUCKET() or a custom func required"
            |> raise

        member _.BucketedValuesBy (_groupInterval: TimeSeriesGroupInterval<'TimeSeriesId, 'TimeSeriesIndex>) (_bucket: TimeBucket) (_aggregate: TimeBucketValueAggregate) : Task<list<'TimeSeriesIndex * list<DateTimeOffset * float<'UnitOfMeasure>>>> =
            NotImplementedException "Bucket functions not implemented yet (SQL 2022 DATE_BUCKET() or a custom func required"
            |> raise

        member _.BucketedDataPoints (_interval: TimeSeriesInterval<'TimeSeriesId, 'TimeSeriesIndex>) (_bucket: TimeBucket) (_aggregate: TimeBucketPointAggregate) : Task<list<'TimeSeriesDataPoint>> =
            NotImplementedException "Bucket functions not implemented yet (SQL 2022 DATE_BUCKET() or a custom func required"
            |> raise

        member _.BucketedDataPointsBy (_groupInterval: TimeSeriesGroupInterval<'TimeSeriesId, 'TimeSeriesIndex>) (_bucket: TimeBucket) (_aggregate: TimeBucketPointAggregate) : Task<list<'TimeSeriesIndex * list<'TimeSeriesDataPoint>>> =
            NotImplementedException "Bucket functions not implemented yet (SQL 2022 DATE_BUCKET() or a custom func required"
            |> raise

        member _.AggregateValue (interval: TimeSeriesInterval<'TimeSeriesId, 'TimeSeriesIndex>) (aggregate: TimeBucketValueAggregate) : Task<Option<float<'UnitOfMeasure>>> =
            backgroundTask {
                let! valuesSortedByTime = (this :> ITimeSeriesRepo<_, _, _, _>).Values interval
                let valuesSorted = valuesSortedByTime |> Seq.map snd |> Seq.sort |> Array.ofSeq
                return
                    if valuesSorted.Length = 0 then
                        None
                    else
                        match aggregate with
                        | LastValue -> valuesSortedByTime |> Seq.last |> snd
                        | FirstValue -> valuesSortedByTime |> Seq.head |> snd
                        | MinimumValue -> Array.min valuesSorted
                        | MaximumValue -> Array.max valuesSorted
                        | AverageValue -> Array.average valuesSorted
                        | MedianValue -> (valuesSorted[valuesSorted.Length / 2] + valuesSorted[(valuesSorted.Length + 1) / 2]) / 2.
                        | PercentileValue p ->
                            // TODO: make it like PERCENTILE_CONT to imitate Sql Time Series repo
                            let index = int ((p.Value / 100.0m) * decimal (valuesSorted.Length - 1))
                            valuesSorted[index]
                        | SumValue -> Array.sum valuesSorted
                        |> Some
            }

        member _.AggregateValuesBy (groupInterval: TimeSeriesGroupInterval<'TimeSeriesId, 'TimeSeriesIndex>) (aggregate: TimeBucketValueAggregate) : Task<list<'TimeSeriesIndex * float<'UnitOfMeasure>>> =
            let db = db ()
            // no need to lock because time series is append only
            match db.TimeSeriesByIdStr |> Map.tryFind groupInterval.Id.IdString with
            | None ->
                []
            | Some points ->
                let indexKey = groupInterval.GroupBy.CaseName
                points
                |> Map.toSeq
                |> Seq.filter (fst >> this.KeyMatchesTimeGroupInterval groupInterval)
                |> Seq.groupBy (fun (key, _) -> key.IndexVal)
                |> Seq.choose (fun (primitiveVal, unsortedPoints) ->
                    let pointsSortedByTime = unsortedPoints |> Seq.sortBy (fun (key, _) -> key.TimeIndex)
                    let valuesSorted = pointsSortedByTime |> Seq.map (fun (_, p) -> p.Value) |> Array.ofSeq
                    'TimeSeriesIndex.TryParse indexKey primitiveVal
                    |> Option.map (fun idx ->
                        idx,
                        match aggregate with
                        | LastValue -> unsortedPoints |> Seq.last |> snd |> fun p -> p.Value
                        | FirstValue -> unsortedPoints |> Seq.head |> snd |> fun p -> p.Value
                        | MinimumValue -> Array.min valuesSorted
                        | MaximumValue -> Array.max valuesSorted
                        | AverageValue -> Array.average valuesSorted
                        | MedianValue -> (valuesSorted[valuesSorted.Length / 2] + valuesSorted[(valuesSorted.Length + 1) / 2]) / 2.
                        | PercentileValue p ->
                            // TODO: make it like PERCENTILE_CONT to imitate Sql Time Series repo
                            let index = int ((p.Value / 100.0m) * decimal (valuesSorted.Length - 1))
                            valuesSorted[index]
                        | SumValue -> Array.sum valuesSorted
                        ))
                |> List.ofSeq
            |> Task.FromResult

        member _.AggregateDataPoint (interval: TimeSeriesInterval<'TimeSeriesId, 'TimeSeriesIndex>) (aggregate: TimeBucketPointAggregate) : Task<Option<'TimeSeriesDataPoint>> =
            backgroundTask {
                let! points = (this :> ITimeSeriesRepo<_, _, _, _>).DataPoints interval
                return
                    match aggregate with
                    | LastPoint -> points |> List.tryLast
                    | FirstPoint -> points |> List.tryHead
            }

        member _.AggregateDataPointsBy (groupInterval: TimeSeriesGroupInterval<'TimeSeriesId, 'TimeSeriesIndex>) (aggregate: TimeBucketPointAggregate) : Task<list<'TimeSeriesIndex * 'TimeSeriesDataPoint>> =
            let db = db ()
            // no need to lock because time series is append only
            match db.TimeSeriesByIdStr |> Map.tryFind groupInterval.Id.IdString with
            | None ->
                []
            | Some points ->
                let indexKey = groupInterval.GroupBy.CaseName
                points
                |> Map.toSeq
                |> Seq.filter (fst >> this.KeyMatchesTimeGroupInterval groupInterval)
                |> Seq.groupBy (fun (key, _) -> key.IndexVal)
                |> Seq.choose (fun (primitiveVal, points) ->
                    let sortedPoints = points |> Seq.sortBy (fun (key, _) -> key.TimeIndex)
                    'TimeSeriesIndex.TryParse indexKey primitiveVal
                    |> Option.map (fun idx ->
                        idx,
                        match aggregate with
                        | LastPoint -> sortedPoints |> Seq.last
                        | FirstPoint -> sortedPoints |> Seq.head
                        |> snd))
                |> List.ofSeq
            |> Task.FromResult

        member _.CountDataPoints (interval: TimeSeriesInterval<'TimeSeriesId, 'TimeSeriesIndex>) : Task<uint64> =
            backgroundTask {
                let! points = (this :> ITimeSeriesRepo<_, _, _, _>).DataPoints interval
                return uint64 points.Length
            }

        member _.CountDataPointsBy (groupInterval: TimeSeriesGroupInterval<'TimeSeriesId, 'TimeSeriesIndex>) : Task<list<'TimeSeriesIndex * uint64>> =
            let db = db ()
            // no need to lock because time series is append only
            match db.TimeSeriesByIdStr |> Map.tryFind groupInterval.Id.IdString with
            | None ->
                []
            | Some points ->
                let indexKey = groupInterval.GroupBy.CaseName
                points
                |> Map.toSeq
                |> Seq.filter (fst >> this.KeyMatchesTimeGroupInterval groupInterval)
                |> Seq.countBy (fun (key, _) -> key.IndexVal)
                |> Seq.choose (fun (primitiveVal, count) -> 'TimeSeriesIndex.TryParse indexKey primitiveVal |> Option.map (fun idx -> idx, uint64 count))
                |> List.ofSeq
            |> Task.FromResult
