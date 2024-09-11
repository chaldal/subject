namespace LibLifeCycleHost.Storage.SqlServer

open System
#nowarn "0686"

open System.Threading.Tasks
open Microsoft.Data.SqlClient
open FSharp.Control
open LibLifeCycle
open LibLifeCycleHost
open System.Data

module private Decode =
    let inline ofCompressedJsonTextWithCodec (codec: CodecLib.Codec<_, 'TimeSeriesDataToRead>) (bytes: byte[]) =
        match DataEncode.ofCompressedJsonTextWithCodec codec bytes with
        | Ok x -> x
        | Error err ->
            InvalidOperationException (sprintf "Unable to decode data point, error: %A" err)
            |> raise

type SqlServerTimeSeriesRepo<'TimeSeriesDataPoint, 'TimeSeriesId, [<Measure>] 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex
    when 'TimeSeriesDataPoint :> TimeSeriesDataPoint<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure>
    and  'TimeSeriesId :> TimeSeriesId<'TimeSeriesId>
    and  'OpError :> OpError
    and  'TimeSeriesIndex :> TimeSeriesIndex<'TimeSeriesIndex>>
    (
        timeSeries: ITimeSeries<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex>,
        connStrings: SqlServerConnectionStrings
    ) =

    let ecosystemName = timeSeries.Def.TimeSeriesKey.EcosystemName
    let sqlConnectionString = connStrings.ForEcosystem ecosystemName

    let timeIntervalBits (start: TimeIntervalEndpoint) (end': TimeIntervalEndpoint) =
        let startOp, startTimeIndex =
            match start with
            | IncludeEndpoint pt -> ">=", pt
            | ExcludeEndpoint pt -> ">",  pt
        let endOp, endTimeIndex =
            match end' with
            | IncludeEndpoint pt -> "<=", pt
            | ExcludeEndpoint pt -> "<",  pt
        {| StartOp = startOp; StartTimeIndex = startTimeIndex; EndOp = endOp; EndTimeIndex = endTimeIndex |}

    let commandByTemplate
        (connection: SqlConnection)
        (interval: TimeSeriesInterval<'TimeSeriesId, 'TimeSeriesIndex>)
        (selectWhat: string)
        (maybeOrderBy: Option<string>)
        (customizeCommand: SqlCommand -> unit) =

        let timeBits = timeIntervalBits interval.Start interval.End
        let sql =
            $"SELECT {selectWhat} FROM [%s{ecosystemName}].[TimeSeries_%s{timeSeries.Name}]
              WHERE Id = @id AND TimeIndex {timeBits.StartOp} @startTimeIndex AND TimeIndex {timeBits.EndOp} @endTimeIndex AND IndexKey = @indexKey AND IndexVal = @indexVal
              {maybeOrderBy |> Option.defaultValue String.Empty}"

        let indexKey, indexVal =
            interval.Index
            |> Option.map (fun i -> (UnionCase.ofCase i).CaseName, i.PrimitiveValue)
            |> Option.defaultValue ("", "")

        let command = new SqlCommand(sql, connection)
        command.Parameters.Add("@id", SqlDbType.NVarChar).Value <- interval.Id.IdString
        command.Parameters.Add("@startTimeIndex", SqlDbType.DateTimeOffset).Value <- timeBits.StartTimeIndex
        command.Parameters.Add("@endTimeIndex", SqlDbType.DateTimeOffset).Value <- timeBits.EndTimeIndex
        command.Parameters.Add("@indexKey", SqlDbType.VarChar).Value <- indexKey
        command.Parameters.Add("@indexVal", SqlDbType.VarChar).Value <- indexVal
        customizeCommand command
        command

    let groupCommandByTemplate
        (connection: SqlConnection)
        (groupInterval: TimeSeriesGroupInterval<'TimeSeriesId, 'TimeSeriesIndex>)
        (selectWhat: string)
        (customizeCommand: SqlCommand -> unit) =

        let timeBits = timeIntervalBits groupInterval.Start groupInterval.End
        let sql =
            $"SELECT [IndexVal], {selectWhat} FROM [%s{ecosystemName}].[TimeSeries_%s{timeSeries.Name}]
              WHERE Id = @id AND TimeIndex {timeBits.StartOp} @startTimeIndex AND TimeIndex {timeBits.EndOp} @endTimeIndex AND IndexKey = @indexKey
              GROUP BY [IndexVal]"

        let indexKey = groupInterval.GroupBy.CaseName

        let command = new SqlCommand(sql, connection)
        command.Parameters.Add("@id", SqlDbType.NVarChar).Value <- groupInterval.Id.IdString
        command.Parameters.Add("@startTimeIndex", SqlDbType.DateTimeOffset).Value <- timeBits.StartTimeIndex
        command.Parameters.Add("@endTimeIndex", SqlDbType.DateTimeOffset).Value <- timeBits.EndTimeIndex
        command.Parameters.Add("@indexKey", SqlDbType.VarChar).Value <- indexKey
        customizeCommand command
        command

    interface ITimeSeriesRepo<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'TimeSeriesIndex> with
        member _.Values (interval: TimeSeriesInterval<'TimeSeriesId, 'TimeSeriesIndex>) : Task<list<DateTimeOffset * float<'UnitOfMeasure>>> =
            fun () -> backgroundTask {
                use connection = new SqlConnection(sqlConnectionString)
                do! connection.OpenAsync()
                use command    = commandByTemplate connection interval "TimeIndex, [Value]" (Some "ORDER BY TimeIndex") ignore
                use! cursor = command.ExecuteReaderAsync()
                return!
                    AsyncSeq.unfoldAsync (
                    fun _ ->
                        async {
                            match! cursor.ReadAsync() |> Async.AwaitTask with
                            | true ->
                                let timeIndex = cursor.GetDateTimeOffset 0
                                let value = cursor.GetDouble 1
                                return Some ((timeIndex, FSharp.Core.LanguagePrimitives.FloatWithMeasure value), Nothing)
                            | false ->
                                return None
                        }
                    ) Nothing
                    |> AsyncSeq.toListAsync
                    |> Async.StartAsTask
            }
            |> SqlServerTransientErrorDetection.wrapTransientExceptions

        member _.DataPoints (interval: TimeSeriesInterval<'TimeSeriesId, 'TimeSeriesIndex>) : Task<list<'TimeSeriesDataPoint>> =
            fun () -> backgroundTask {
                use connection = new SqlConnection(sqlConnectionString)
                do! connection.OpenAsync()
                use command    = commandByTemplate connection interval "[DataPoint]" (Some "ORDER BY TimeIndex") ignore
                use! cursor = command.ExecuteReaderAsync()
                return!
                    AsyncSeq.unfoldAsync (
                    fun _ ->
                        async {
                            match! cursor.ReadAsync() |> Async.AwaitTask with
                            | true ->
                                // TODO: can we do better and cache implicitly everywhere where get_Codec() is defined?
                                let codec = Fleece.Internals.CodecCache<Fleece.Internals.OpCodec, Fleece.SystemTextJson.Encoding, _>.Run(
                                    fun () -> 'TimeSeriesDataPoint.Codec())
                                let dataPoint =
                                    (cursor.Item 0 :?> byte[])
                                    |> Decode.ofCompressedJsonTextWithCodec codec
                                return Some (dataPoint, Nothing)
                            | false ->
                                return None
                        }
                    ) Nothing
                    |> AsyncSeq.toListAsync
                    |> Async.StartAsTask
            }
            |> SqlServerTransientErrorDetection.wrapTransientExceptions

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
            fun () -> backgroundTask {
                use connection = new SqlConnection(sqlConnectionString)
                do! connection.OpenAsync()

                let percentileCommand (p: PositivePercentage) =
                    commandByTemplate connection interval "DISTINCT PERCENTILE_CONT(@percentile) WITHIN GROUP (ORDER BY [Value]) OVER ()" None
                        (fun command ->
                            command.Parameters.Add("@percentile", SqlDbType.Float).Value <- float (p.Value / 100m))

                use command =
                    match aggregate with
                    | FirstValue -> commandByTemplate connection interval "[Value]" (Some "ORDER BY [TimeIndex] ASC, [Id] ASC") ignore
                    | LastValue -> commandByTemplate connection interval "[Value]" (Some "ORDER BY [TimeIndex] DESC, [Id] DESC") ignore
                    | MinimumValue -> commandByTemplate connection interval "MIN([Value])" None ignore
                    | MaximumValue -> commandByTemplate connection interval "MAX([Value])" None ignore
                    | AverageValue -> commandByTemplate connection interval "AVG([Value])" None ignore
                    | MedianValue -> percentileCommand (PositivePercentage.ofDecimalUnsafe 50m)
                    | PercentileValue p -> percentileCommand p
                    | SumValue -> commandByTemplate connection interval "SUM([Value])" None ignore

                use! cursor = command.ExecuteReaderAsync()
                match! cursor.ReadAsync() with
                | true ->
                    if cursor.IsDBNull 0 then
                        return None
                    else
                        let value = cursor.GetDouble 0
                        return Some (FSharp.Core.LanguagePrimitives.FloatWithMeasure value)
                | false ->
                    return None
            }
            |> SqlServerTransientErrorDetection.wrapTransientExceptions


        member _.AggregateValuesBy (groupInterval: TimeSeriesGroupInterval<'TimeSeriesId, 'TimeSeriesIndex>) (aggregate: TimeBucketValueAggregate) : Task<list<'TimeSeriesIndex * float<'UnitOfMeasure>>> =
            fun () -> backgroundTask {
                use connection = new SqlConnection(sqlConnectionString)
                do! connection.OpenAsync()

                let percentileCommand (p: PositivePercentage) =
                    let timeBits = timeIntervalBits groupInterval.Start groupInterval.End
                    let sql =
                        $"SELECT DISTINCT [IndexVal], PERCENTILE_CONT(@percentile) WITHIN GROUP (ORDER BY [Value]) OVER (PARTITION BY [IndexVal])
                          FROM [%s{ecosystemName}].[TimeSeries_%s{timeSeries.Name}]
                          WHERE Id = @id AND TimeIndex {timeBits.StartOp} @startTimeIndex AND TimeIndex {timeBits.EndOp} @endTimeIndex AND IndexKey = @indexKey"

                    let indexKey = groupInterval.GroupBy.CaseName

                    let command = new SqlCommand(sql, connection)
                    command.Parameters.Add("@id", SqlDbType.NVarChar).Value <- groupInterval.Id.IdString
                    command.Parameters.Add("@startTimeIndex", SqlDbType.DateTimeOffset).Value <- timeBits.StartTimeIndex
                    command.Parameters.Add("@endTimeIndex", SqlDbType.DateTimeOffset).Value <- timeBits.EndTimeIndex
                    command.Parameters.Add("@indexKey", SqlDbType.VarChar).Value <- indexKey
                    command.Parameters.Add("@percentile", SqlDbType.Float).Value <- float (p.Value / 100m)
                    command

                use command =
                    match aggregate with
                    | FirstValue ->
                        // TODO: implement FirstValue
                        //groupCommandByTemplate connection groupInterval "[Value] ORDER BY ASC" ignore
                        failwith "AggregateValuesBy FirstValue not implemented yet"
                    | LastValue ->
                        // TODO implement LastValue
                        //groupCommandByTemplate connection groupInterval "[Value] ORDER BY DESC" ignore
                        failwith "AggregateValuesBy LastValue not implemented yet"
                    | MinimumValue -> groupCommandByTemplate connection groupInterval "MIN([Value])" ignore
                    | MaximumValue -> groupCommandByTemplate connection groupInterval "MAX([Value])" ignore
                    | AverageValue -> groupCommandByTemplate connection groupInterval "AVG([Value])" ignore
                    | MedianValue -> percentileCommand (PositivePercentage.ofDecimalUnsafe 50m)
                    | PercentileValue p -> percentileCommand p
                    | SumValue -> groupCommandByTemplate connection groupInterval "SUM([Value])" ignore

                use! cursor = command.ExecuteReaderAsync()
                return!
                    AsyncSeq.unfoldAsync (
                    fun _ ->
                        async {
                            match! cursor.ReadAsync() |> Async.AwaitTask with
                            | true ->
                                let indexVal = cursor.GetString 0
                                let value : float<'UnitOfMeasure> = cursor.GetDouble 1 |> FSharp.Core.LanguagePrimitives.FloatWithMeasure

                                return
                                    'TimeSeriesIndex.TryParse groupInterval.GroupBy.CaseName indexVal
                                    |> Option.map (fun index -> (index, value), Nothing)
                            | false ->
                                return None
                        }
                    ) Nothing
                    |> AsyncSeq.toListAsync
                    |> Async.StartAsTask
            }
            |> SqlServerTransientErrorDetection.wrapTransientExceptions


        member _.AggregateDataPoint (interval: TimeSeriesInterval<'TimeSeriesId, 'TimeSeriesIndex>) (aggregate: TimeBucketPointAggregate) : Task<Option<'TimeSeriesDataPoint>> =
            let orderBy =
                match aggregate with
                | FirstPoint -> "ASC"
                | LastPoint -> "DESC"
                |> fun directionSql -> $"ORDER BY TimeIndex %s{directionSql}, [Id] %s{directionSql}"

            fun () -> backgroundTask {
                use connection = new SqlConnection(sqlConnectionString)
                do! connection.OpenAsync()
                use command    = commandByTemplate connection interval "TOP 1 DataPoint" (Some orderBy) ignore

                use! cursor = command.ExecuteReaderAsync()
                match! cursor.ReadAsync() with
                | true ->
                    // TODO: can we do better and cache implicitly everywhere where get_Codec() is defined?
                    let codec = Fleece.Internals.CodecCache<Fleece.Internals.OpCodec, Fleece.SystemTextJson.Encoding, _>.Run(
                        fun () -> 'TimeSeriesDataPoint.Codec())
                    let dataPoint =
                        (cursor.Item 0 :?> byte[])
                        |> Decode.ofCompressedJsonTextWithCodec codec
                    return Some dataPoint
                | false ->
                    return None
            }
            |> SqlServerTransientErrorDetection.wrapTransientExceptions

        member _.AggregateDataPointsBy (_groupByInterval: TimeSeriesGroupInterval<'TimeSeriesId, 'TimeSeriesIndex>) (_aggregate: TimeBucketPointAggregate) : Task<list<'TimeSeriesIndex * 'TimeSeriesDataPoint>> =
            // TODO: just implement it
            failwith "AggregateDataPointsBy not implemented yet"

        member _.CountDataPoints (interval: TimeSeriesInterval<'TimeSeriesId, 'TimeSeriesIndex>) : Task<uint64> =
            fun () -> backgroundTask {
                use connection = new SqlConnection(sqlConnectionString)
                do! connection.OpenAsync()
                use command    = commandByTemplate connection interval "COUNT_BIG(*)" None ignore

                let! totalCountObj = command.ExecuteScalarAsync()
                return Convert.ToUInt64 totalCountObj
            }
            |> SqlServerTransientErrorDetection.wrapTransientExceptions

        member _.CountDataPointsBy (groupInterval: TimeSeriesGroupInterval<'TimeSeriesId, 'TimeSeriesIndex>) : Task<list<'TimeSeriesIndex * uint64>> =
            fun () -> backgroundTask {
                use connection = new SqlConnection(sqlConnectionString)
                do! connection.OpenAsync()
                use command    = groupCommandByTemplate connection groupInterval "COUNT_BIG(*)" ignore
                let primitiveKey = groupInterval.GroupBy.CaseName

                use! cursor = command.ExecuteReaderAsync()
                return!
                    AsyncSeq.unfoldAsync (
                    fun _ ->
                        async {
                            match! cursor.ReadAsync() |> Async.AwaitTask with
                            | true ->
                                let indexVal = cursor.GetString 0
                                let count = Convert.ToUInt64 (cursor.Item 1)

                                return
                                    'TimeSeriesIndex.TryParse primitiveKey indexVal
                                    |> Option.map (fun index -> (index, count), Nothing)
                            | false ->
                                return None
                        }
                    ) Nothing
                    |> AsyncSeq.toListAsync
                    |> Async.StartAsTask
            }
            |> SqlServerTransientErrorDetection.wrapTransientExceptions
