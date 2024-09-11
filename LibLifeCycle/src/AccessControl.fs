[<AutoOpen>]
module LibLifeCycle.AccessControl

open System

type AccessDecision =
| Grant
| Deny

type AccessMatch<'T> =
| MatchAny
| Match of 'T

type SubjectProjection =
| OriginalProjection
| Projection of ProjectionName: string

[<RequireQualifiedAccess>]
type AccessEventType<'LifeAction, 'Constructor
        when 'LifeAction :> LifeAction
        and  'Constructor :> Constructor> =
| ConstructCase of UnionCase<'Constructor>
| ActCase       of UnionCase<'LifeAction>
| Read          of SubjectProjection
| ReadBlob
| ReadHistory

[<RequireQualifiedAccess>]
type AccessEvent<'Subject, 'LifeAction, 'Constructor, 'SubjectId
                                when 'Subject     :> Subject<'SubjectId>
                                and  'SubjectId   :> SubjectId
                                and 'SubjectId : comparison
                                and  'LifeAction  :> LifeAction
                                and  'Constructor :> Constructor> =
| Read        of 'Subject * SubjectProjection
| ReadBlob    of 'Subject * BlobGuid: Guid
| ReadHistory of 'Subject
| Act         of 'Subject * 'LifeAction
| Construct   of 'Constructor
with
    member this.Type : AccessEventType<'LifeAction, 'Constructor> =
        match this with
        | AccessEvent.Read (_, projection) -> AccessEventType.Read projection
        | AccessEvent.ReadBlob _ -> AccessEventType.ReadBlob
        | AccessEvent.ReadHistory _ -> AccessEventType.ReadHistory
        | AccessEvent.Act (_, action) -> action |> UnionCase.ofCase |> AccessEventType.ActCase
        | AccessEvent.Construct ctor -> ctor |> UnionCase.ofCase |> AccessEventType.ConstructCase

let (|OnExistingSubject|_|) =
    function
    | AccessEvent.Read        (subj, _)
    | AccessEvent.ReadBlob    (subj, _)
    | AccessEvent.ReadHistory subj
    | AccessEvent.Act         (subj, _) ->
        Some subj
    | AccessEvent.Construct _ ->
        None

let (|OnNewSubject|_|) =
    function
    | AccessEvent.Construct ctor -> Some ctor
    | _ -> None


[<RequireQualifiedAccess>]
type TimeSeriesAccessEventType =
| Ingest
| Read

[<RequireQualifiedAccess>]
type TimeSeriesAccessEvent<'TimeSeriesDataPoint, 'TimeSeriesId, [<Measure>] 'UnitOfMeasure
                            when 'TimeSeriesDataPoint :> TimeSeriesDataPoint<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure>
                            and  'TimeSeriesId        :> TimeSeriesId<'TimeSeriesId>> =
| Ingest of list<'TimeSeriesDataPoint>
// TODO:  Read   of list<'TimeSeriesDataPoint> ? or of  one point ? does it make sense to authorize each point individually ?

with
    member this.Type : TimeSeriesAccessEventType =
        match this with
        //| TimeSeriesAccessEvent.Read _ -> TimeSeriesAccessEventType.Read
        | TimeSeriesAccessEvent.Ingest _ -> TimeSeriesAccessEventType.Ingest

    member this.DataPoints =
        match this with
        | TimeSeriesAccessEvent.Ingest dataPoints ->
            dataPoints
