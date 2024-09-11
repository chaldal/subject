[<AutoOpen>]
module LibLifeCycleHost.TimeSeriesStorageHandler

open System.Threading.Tasks

type TimeSeriesDataPointToSave<'TimeSeriesDataPoint, 'TimeSeriesId, [<Measure>] 'UnitOfMeasure, 'TimeSeriesIndex
                                when 'TimeSeriesDataPoint :> TimeSeriesDataPoint<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure>
                                and  'TimeSeriesId :> TimeSeriesId<'TimeSeriesId>
                                and  'TimeSeriesIndex :> TimeSeriesIndex<'TimeSeriesIndex>> = {
    Point: 'TimeSeriesDataPoint
    Indices: list<'TimeSeriesIndex>
}

type TimeSeriesDataToSave<'TimeSeriesDataPoint, 'TimeSeriesId, [<Measure>] 'UnitOfMeasure, 'TimeSeriesIndex
                           when 'TimeSeriesDataPoint :> TimeSeriesDataPoint<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure>
                           and  'TimeSeriesId :> TimeSeriesId<'TimeSeriesId>
                           and  'TimeSeriesIndex :> TimeSeriesIndex<'TimeSeriesIndex>> = {
    Points: NonemptyList<TimeSeriesDataPointToSave<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'TimeSeriesIndex>>
    CreatedBy: string
}

type ITimeSeriesStorageHandler<'TimeSeriesDataPoint, 'TimeSeriesId, [<Measure>] 'UnitOfMeasure, 'TimeSeriesIndex
                                when 'TimeSeriesDataPoint :> TimeSeriesDataPoint<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure>
                                and  'TimeSeriesId :> TimeSeriesId<'TimeSeriesId>
                                and  'TimeSeriesIndex :> TimeSeriesIndex<'TimeSeriesIndex>> =

    abstract member Save: data: TimeSeriesDataToSave<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'TimeSeriesIndex> -> Task
