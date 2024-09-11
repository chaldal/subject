module LibLifeCycleTypes.LegacyRealTime

// TODO: delete this once all clients are migrated to work against the V1 API

open System

type SubjectSnapshot<'Subject, 'SubjectId
        when 'Subject   :> Subject<'SubjectId>
        and  'SubjectId :> SubjectId
        and 'SubjectId : comparison> = {
    Subject:   'Subject
    UpdatedOn: DateTimeOffset
}

[<RequireQualifiedAccess>]
type SubjectChange<'Subject, 'SubjectId
        when 'Subject   :> Subject<'SubjectId>
        and  'SubjectId :> SubjectId
        and 'SubjectId : comparison> =
| Updated of SubjectSnapshot<'Subject, 'SubjectId>
| NotInitialized

type AccessControlledSubjectChange<'Subject, 'SubjectId
        when 'Subject   :> Subject<'SubjectId>
        and  'SubjectId :> SubjectId
        and 'SubjectId : comparison> =
     AccessControlled<SubjectChange<'Subject, 'SubjectId>, 'SubjectId>


type SubjectProjectionSnapshot<'Projection> = {
    Projected: 'Projection
    UpdatedOn: DateTimeOffset
}

[<RequireQualifiedAccess>]
type SubjectProjectionChange<'Projection> =
| Updated of SubjectProjectionSnapshot<'Projection>
| NotInitialized

type AccessControlledSubjectProjectionChange<'Projection, 'SubjectId when 'SubjectId :> SubjectId> =
     AccessControlled<SubjectProjectionChange<'Projection>, 'SubjectId>

type ClientApi = unit

type ServerApi = unit

[<RequireQualifiedAccess>]
type ClientStreamApi =
| ObserveSubject of LifeCycleName: string * SubjectIdStr: string * SendCurrentValue: bool
| ObserveSubjectProjection of LifeCycleName: string * ProjectionName: string * SubjectIdStr: string * SendCurrentValue: bool

[<RequireQualifiedAccess>]
type ServerStreamApi =
| SubjectChanged of AccessControlledSubjectChange: string
