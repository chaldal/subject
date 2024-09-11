module LibLifeCycleHost.Storage.SqlServer.SqlServerTimeSeriesStorageHandler

#nowarn "0686"  // Allow exceptionally here to have explicit parameters for codec related functions

open System
open System.Data
open Microsoft.Data.SqlClient
open FSharpPlus
open LibLifeCycle
open System.Threading.Tasks
open LibLifeCycleHost
open Microsoft.Extensions.Logging

let private murmur32 = Murmur.MurmurHash.Create32()

[<Literal>]
let private maxCompressableDataPointJsonSizeBytes = 10240 // 10kB , data points must be small

type SqlServerTimeSeriesStorageHandler<'TimeSeriesDataPoint, 'TimeSeriesId, [<Measure>] 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex
                                        when 'TimeSeriesDataPoint :> TimeSeriesDataPoint<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure>
                                        and  'TimeSeriesId :> TimeSeriesId<'TimeSeriesId>
                                        and  'TimeSeriesIndex :> TimeSeriesIndex<'TimeSeriesIndex>
                                        and  'OpError :> OpError>
    (
        timeSeries: ITimeSeries<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex>,
        config: SqlServerConnectionStrings,
        logger: Microsoft.Extensions.Logging.ILogger<SqlServerTimeSeriesStorageHandler<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex>>
    ) =

    let ecosystemName = timeSeries.Def.TimeSeriesKey.EcosystemName
    let sqlConnectionString = config.ForEcosystem ecosystemName

    let save (data: TimeSeriesDataToSave<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'TimeSeriesIndex>) : Task =

        let pointsTable = new DataTable()
        pointsTable.Columns.Add("Id",        typeof<string>).MaxLength <- 80
        pointsTable.Columns.Add("IndexKey",  typeof<string>).MaxLength <- 60
        pointsTable.Columns.Add("IndexVal",  typeof<string>).MaxLength <- 300
        pointsTable.Columns.Add("TimeIndex", typeof<DateTimeOffset>) |> ignore
        pointsTable.Columns.Add("Hash",      typeof<int32>)          |> ignore
        pointsTable.Columns.Add("Value",     typeof<float>)          |> ignore
        pointsTable.Columns.Add("DataPoint", typeof<byte[]>)         |> ignore

        // TODO: can we do better and cache implicitly everywhere where get_Codec() is defined?
        let codec = Fleece.Internals.CodecCache<Fleece.Internals.OpCodec, Fleece.SystemTextJson.Encoding, _>.Run(
            fun () -> 'TimeSeriesDataPoint.Codec())

        data.Points.ToList
        |> Seq.iter (fun p ->
            let point = p.Point
            let dataPointJson = DataEncode.toJsonTextWithCodec codec point
            if dataPointJson.Length > maxCompressableDataPointJsonSizeBytes then
                $"Encoded data point is too large. Uncompressed size is over %d{maxCompressableDataPointJsonSizeBytes} bytes: %d{dataPointJson.Length}. TimeSeries: %s{timeSeries.Name}"
                |> fun details -> PermanentSubjectException ("timeSeries", details)
                |> raise
            elif dataPointJson.Length > maxCompressableDataPointJsonSizeBytes / 2 then
                logger.LogWarning $"Encoded data point is large. Uncompressed size is over %d{maxCompressableDataPointJsonSizeBytes} bytes: %d{dataPointJson.Length}. TimeSeries: %s{timeSeries.Name}"

            let dataPoint = gzipCompressUtf8String dataPointJson
            let hash = BitConverter.ToInt32(murmur32.ComputeHash dataPoint, 0)
            let id = point.Id.IdString
            let value = point.Value
            let timeIndex = point.TimeIndex

            p.Indices
            // index rows
            |> Seq.map (fun i -> (UnionCase.ofCase i).CaseName, i.PrimitiveValue)
            // "parent" row
            |> Seq.append [("", "")]
            |> Seq.iter (fun (indexKey, indexVal) ->
                try
                    pointsTable.Rows.Add(id, indexKey, indexVal, timeIndex, hash, value, dataPoint) |> ignore
                with
                | :? ArgumentException as ex ->
                    // if index size violated then no need to retry side effect endlessly (unlike subject)
                    PermanentSubjectException ("timeSeries", ex.ToString()) |> raise))

        fun () -> backgroundTask {
            use connection = new SqlConnection(sqlConnectionString)
            do! connection.OpenAsync()
            use command = new SqlCommand((sprintf "[%s].TimeSeries_%s_Save" ecosystemName timeSeries.Name), connection)

            command.CommandType <- CommandType.StoredProcedure
            command.Parameters.Add("@createdBy", SqlDbType.NVarChar).Value <- data.CreatedBy
            command.Parameters.Add("@points", SqlDbType.Structured)
            |> fun param ->
                param.Value    <- pointsTable
                param.TypeName <- sprintf "[%s].TimeSeriesPointsList" ecosystemName

            do! command.ExecuteNonQueryAsync() |> Task.Ignore
        }
        |> SqlServerTransientErrorDetection.wrapTransientExceptions
        |> Task.Ignore

    interface ITimeSeriesStorageHandler<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'TimeSeriesIndex> with
        member this.Save data =
            save data