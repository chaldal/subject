[<AutoOpen>]
module LibLifeCycleHost.TimeSeriesAdapter

open System
open LibLifeCycle
open System.Threading.Tasks
open FSharpPlus
open LibLifeCycleHost.Web
open Microsoft.Extensions.DependencyInjection

type ITimeSeriesAdapter =
    abstract member TimeSeries: ITimeSeries
    abstract member Ingest: serviceProvider: IServiceProvider -> clock: Service<Clock> -> callOrigin: CallOrigin -> ``list<'TimeSeriesDataPoint>``: obj -> Task<Result<unit, OpError>>

type TimeSeriesAdapter<'TimeSeriesDataPoint, 'TimeSeriesId, [<Measure>] 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex
    when 'TimeSeriesDataPoint :> TimeSeriesDataPoint<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure>
    and  'TimeSeriesId :> TimeSeriesId<'TimeSeriesId>
    and  'TimeSeriesIndex :> TimeSeriesIndex<'TimeSeriesIndex>
    and  'OpError :> OpError> = {
    TimeSeries: ITimeSeries<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex>
}
with
    member this.Ingest (serviceProvider: IServiceProvider) (clock: Service<Clock>) (callOrigin: CallOrigin) (points: list<'TimeSeriesDataPoint>) : Task<Result<unit, 'OpError>> =
        backgroundTask {
            let! now = clock.Query Now

            // TODO : probably must transform upon receiving e.g. before Ingest side effect persisted ? think subject transactions
            match
                points
                |> List.map (this.TimeSeries.Transform { ServerNow = now; CallOrigin = callOrigin })
                |> sequence with
            | Ok transformedPoints ->
                // TODO: what's the point to pass it via Orleans context? it's not even a grain call?
                let userId, _ = OrleansRequestContext.getTelemetryUserIdAndSessionId ()

                let storageProvider = serviceProvider.GetRequiredService<ITimeSeriesStorageHandler<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'TimeSeriesIndex>>()
                match
                    transformedPoints |> List.map (fun pt ->
                        { Point = pt
                          Indices = this.TimeSeries.Indices pt |> List.ofSeq })
                    |> NonemptyList.ofList
                    with
                | Some pointsToSave ->
                    do! storageProvider.Save { Points = pointsToSave; CreatedBy = userId }
                | None ->
                    ()
                return Ok ()
            | Error err ->
                return err |> Error
        }

    interface ITimeSeriesAdapter with
        member this.TimeSeries = this.TimeSeries
        member this.Ingest (serviceProvider: IServiceProvider) (clock: Service<Clock>) (callOrigin: CallOrigin) (``list<'TimeSeriesDataPoint>``: obj) : Task<Result<unit, OpError>> =
            backgroundTask {
                let points = ``list<'TimeSeriesDataPoint>`` :?> list<'TimeSeriesDataPoint>
                match! this.Ingest serviceProvider clock callOrigin points with
                | Ok () ->
                    return Ok ()
                | Error err ->
                    return (err :> OpError) |> Error
            }

type TimeSeriesAdapterCollection = TimeSeriesAdapterCollection of Map<TimeSeriesKey, ITimeSeriesAdapter>
    with
        interface System.Collections.Generic.IEnumerable<ITimeSeriesAdapter> with
            member this.GetEnumerator(): Collections.Generic.IEnumerator<ITimeSeriesAdapter> =
                let (TimeSeriesAdapterCollection dictionary) = this
                dictionary.Values.GetEnumerator()

            member this.GetEnumerator(): Collections.IEnumerator =
                let (TimeSeriesAdapterCollection dictionary) = this
                dictionary.Values.GetEnumerator() :> Collections.IEnumerator

        member this.GetTimeSeriesAdapterByKey key : Option<ITimeSeriesAdapter> =
            match this with
            | TimeSeriesAdapterCollection dictionary ->
                match dictionary.TryGetValue key with
                | true, adapter -> Some adapter
                | false, _ -> None