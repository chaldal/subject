[<AutoOpen>]
module LibLifeCycle.TimeSeries

open System

// TODO: do you need this at all?
type FullyTypedTimeSeriesFunction<'Res> =
    abstract member Invoke: TimeSeries<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex, 'AccessPredicateInput, 'Session, 'Role> -> 'Res

and FullyTypedTimeSeriesFunction<'Res, 'TimeSeriesDataPoint, 'TimeSeriesId, [<Measure>] 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex
                                  when 'TimeSeriesDataPoint :> TimeSeriesDataPoint<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure>
                                  and  'TimeSeriesId        :> TimeSeriesId<'TimeSeriesId>
                                  and  'OpError             :> OpError
                                  and  'TimeSeriesIndex     :> TimeSeriesIndex<'TimeSeriesIndex>> =
    abstract member Invoke: TimeSeries<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex, 'AccessPredicateInput, 'Session, 'Role> -> 'Res

and ITimeSeries =
    abstract member Name: string
    abstract member Def: ITimeSeriesDef
    abstract member EnableApiAccess: bool
    abstract member Invoke: FullyTypedTimeSeriesFunction<'Res> -> 'Res

and ITimeSeries<'TimeSeriesDataPoint, 'TimeSeriesId, [<Measure>] 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex
    when 'TimeSeriesDataPoint :> TimeSeriesDataPoint<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure>
    and  'TimeSeriesId        :> TimeSeriesId<'TimeSeriesId>
    and  'TimeSeriesIndex :> TimeSeriesIndex<'TimeSeriesIndex>
    and  'OpError :> OpError> =
    inherit ITimeSeries
    abstract member Invoke: FullyTypedTimeSeriesFunction<'Res, 'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex> -> 'Res
    abstract member Transform: TimeSeriesTransform<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError>
    abstract member Indices: ('TimeSeriesDataPoint -> seq<'TimeSeriesIndex>)

and TimeSeriesTransform<'TimeSeriesDataPoint, 'TimeSeriesId, [<Measure>] 'UnitOfMeasure, 'OpError
        when 'TimeSeriesDataPoint :> TimeSeriesDataPoint<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure>
        and  'TimeSeriesId        :> TimeSeriesId<'TimeSeriesId>
        and  'OpError :> OpError> =
    TimeSeriesTransformContext -> 'TimeSeriesDataPoint -> Result<'TimeSeriesDataPoint, 'OpError>

and TimeSeriesTransformContext = {
    ServerNow: DateTimeOffset
    CallOrigin: CallOrigin
}

and TimeSeriesAccessPredicate<'TimeSeriesDataPoint, 'TimeSeriesId, [<Measure>] 'UnitOfMeasure, 'AccessPredicateInput, 'Session
        when 'TimeSeriesDataPoint :> TimeSeriesDataPoint<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure>
        and  'TimeSeriesId        :> TimeSeriesId<'TimeSeriesId>
        and  'AccessPredicateInput :> AccessPredicateInput> =
    'AccessPredicateInput -> TimeSeriesAccessEvent<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure> -> ExternalCallOrigin -> Option<'Session> -> bool

and TimeSeriesApiAccess<'TimeSeriesDataPoint, 'TimeSeriesId, [<Measure>] 'UnitOfMeasure, 'AccessPredicateInput, 'Session, 'Role
                when 'TimeSeriesDataPoint  :> TimeSeriesDataPoint<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure>
                and  'TimeSeriesId         :> TimeSeriesId<'TimeSeriesId>
                and  'AccessPredicateInput :> AccessPredicateInput
                and  'Role                 :  comparison> = {
    AccessRules:                List<TimeSeriesAccessRule<'AccessPredicateInput, 'Role>>
    AccessPredicate:            TimeSeriesAccessPredicate<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'AccessPredicateInput, 'Session>
}

// A Subject's Life-Cycle defines some operations that governs its behavior, along with some metadata
and TimeSeries<'TimeSeriesDataPoint, 'TimeSeriesId, [<Measure>] 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex, 'AccessPredicateInput, 'Session, 'Role
                when 'TimeSeriesDataPoint  :> TimeSeriesDataPoint<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure>
                and  'TimeSeriesId         :> TimeSeriesId<'TimeSeriesId>
                and  'OpError              :> OpError
                and  'TimeSeriesIndex      :> TimeSeriesIndex<'TimeSeriesIndex>
                and  'AccessPredicateInput :> AccessPredicateInput
                and  'Role                 :  comparison> = internal {
    Transform:           TimeSeriesTransform<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError>
    Definition:          TimeSeriesDef<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex>
    Indices:             'TimeSeriesDataPoint -> seq<'TimeSeriesIndex>
    MaybeApiAccess:      Option<TimeSeriesApiAccess<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'AccessPredicateInput, 'Session, 'Role>>
    // TODO: don't send telemetry ? ShouldSendTelemetry: ?
    SessionHandling:     Option<EcosystemSessionHandling<'Session, 'Role>>
}
with
    member this.Name = this.Definition.Key.LocalTimeSeriesName
    member this.SessionHandler = this.SessionHandling |> Option.map(fun h -> h.Handler)

    interface ITimeSeries<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex> with
        member this.Invoke (fn: FullyTypedTimeSeriesFunction<_>) = fn.Invoke this
        member this.Invoke (fn: FullyTypedTimeSeriesFunction<_, _, _, _, _, _>) = fn.Invoke this
        member this.Name = this.Name
        member this.Def = this.Definition
        member this.Transform = this.Transform
        member this.Indices = this.Indices
        member this.EnableApiAccess =
            this.MaybeApiAccess
            |> Option.map (fun _ -> true)
            |> Option.defaultValue false


and TimeSeriesAccessRule<'AccessPredicateInput, 'Role
        when 'AccessPredicateInput :> AccessPredicateInput
        and  'Role                 :  comparison> = {
    Input:      AccessMatch<'AccessPredicateInput>
    EventTypes: AccessMatch<NonemptySet<TimeSeriesAccessEventType>>
    Roles:      AccessMatch<NonemptySet<'Role>>
    Decision:   AccessDecision
}

type OnTimeSeriesResponse<'TimeSeriesDataPoint, 'TimeSeriesId, [<Measure>] 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex
                 when 'TimeSeriesDataPoint :> TimeSeriesDataPoint<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure>
                 and  'TimeSeriesId        :> TimeSeriesId<'TimeSeriesId>
                 and  'OpError             :> OpError
                 and  'TimeSeriesIndex     :> TimeSeriesIndex<'TimeSeriesIndex>> = private OnTimeSeriesResponse of SideEffectResponse * TimeSeriesDef<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex>

type TimeSeriesDef<'TimeSeriesDataPoint, 'TimeSeriesId, [<Measure>] 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex
    when 'TimeSeriesDataPoint :> TimeSeriesDataPoint<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure>
    and  'OpError :> OpError
    and  'TimeSeriesIndex :> TimeSeriesIndex<'TimeSeriesIndex>>
with
    member this.Ingest (point: 'TimeSeriesDataPoint) : IIngestTimeSeriesDataPointsOperation =
        createIngestTimeSeriesDataPointsOp this [point]

    member this.Ingest (points: seq<'TimeSeriesDataPoint>) : IIngestTimeSeriesDataPointsOperation =
        createIngestTimeSeriesDataPointsOp this (List.ofSeq points)

    /// Helps to match SideEffectResponse returned by this time series on a caller's response handler
    /// in a typesafe manner. Match return value using active patterns:
    /// Failures:
    /// |IngestTimeSeriesError|
    member this.OnResponse (response: SideEffectResponse) = OnTimeSeriesResponse (response, this)

type TimeSeries<'TimeSeriesDataPoint, 'TimeSeriesId, [<Measure>] 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex, 'AccessPredicateInput, 'Session, 'Role
                 when 'TimeSeriesDataPoint :> TimeSeriesDataPoint<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure>
                 and  'OpError              :> OpError
                 and  'TimeSeriesIndex      :> TimeSeriesIndex<'TimeSeriesIndex>
                 and  'AccessPredicateInput :> AccessPredicateInput
                 and  'Role                 :  comparison>
with
    member this.Ingest (point: 'TimeSeriesDataPoint) : IIngestTimeSeriesDataPointsOperation =
        this.Definition.Ingest point

    member this.Ingest (points: seq<'TimeSeriesDataPoint>) : IIngestTimeSeriesDataPointsOperation =
        this.Definition.Ingest points

    member this.OnResponse (response: SideEffectResponse) =
        this.Definition.OnResponse response


type NextOnTimeSeriesIngestFailure = private NextOnTimeSeriesIngestFailure_ of unit
with
    member this.Escalate<'LifeAction when 'LifeAction :> LifeAction> (severity: SideEffectFailureSeverity) : SideEffectResponseDecision<'LifeAction> =
        SideEffectFailureDecision.Escalate severity |> Choice2Of2 |> SideEffectResponseDecision_
    member this.Dismiss<'LifeAction when 'LifeAction :> LifeAction> () : SideEffectResponseDecision<'LifeAction> =
        SideEffectFailureDecision.Dismiss |> Choice2Of2 |> SideEffectResponseDecision_

let private nextOnTimeSeriesIngestFailure = NextOnTimeSeriesIngestFailure_ ()

let (|IngestTimeSeriesError|_|) (OnTimeSeriesResponse (response: SideEffectResponse, timeSeriesDef: TimeSeriesDef<'TimeSeriesDataPoint, _, _, 'OpError, _>)) =
    match response with
    | SideEffectResponse.Failure (SideEffectFailure.IngestTimeSeriesError (timeSeriesKey, ``list<'TimeSeriesDataPoint>``, err)) when timeSeriesKey = timeSeriesDef.Key ->
        Some (``list<'TimeSeriesDataPoint>`` :?> list<'TimeSeriesDataPoint>, err :?> 'OpError, nextOnTimeSeriesIngestFailure)
    | _ ->
        None