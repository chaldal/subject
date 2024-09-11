module internal LibLifeCycleHost.Web.LegacyJsonEncoding

// TODO: delete this once all clients are migrated to work against the V1 API

open System.Reflection
open FSharpPlus
open LibLifeCycle
open LibLifeCycleHost
open LibLifeCycleTypes.LegacyRealTime
open LibLifeCycleHost.Web.WebApiJsonEncoding

type AnchorTypeForModule = private AnchorTypeForModule of unit

let private getSubjectProjectionEncoder<'Subject, 'Projection>
    (projection: UntypedSubjectProjectionDef<'Subject>)
    : Encoder<'Subject> =
    let typedProjection = fun subj -> projection.Projection subj :?> 'Projection
    let projectionEncoder = generateAutoEncoder<'Projection>
    typedProjection >> projectionEncoder

let private getAccessControlledSubjectProjectionListEncoder<'Subject, 'Projection, 'SubjectId>
    (projection: UntypedSubjectProjectionDef<'Subject>)
    : Encoder<List<AccessControlled<'Subject, 'SubjectId>>> =
    let typedProjection = fun subj -> projection.Projection subj :?> 'Projection
    let projectionEncoder = generateAutoEncoder<List<AccessControlled<'Projection, 'SubjectId>>>
    List.map
        (function
         | Granted t -> typedProjection t |> Granted
         | Denied id -> Denied id)
     >> projectionEncoder

let private getAccessControlledSubjectProjectionListWithTotalCountEncoder<'Subject, 'Projection, 'SubjectId>
    (projection: UntypedSubjectProjectionDef<'Subject>)
    : Encoder<ListWithTotalCount<AccessControlled<'Subject, 'SubjectId>>> =
    let typedProjection = fun subj -> projection.Projection subj :?> 'Projection
    let projectionEncoder = generateAutoEncoder<ListWithTotalCount<AccessControlled<'Projection, 'SubjectId>>>
    fun listWithTotalCount ->
        {
            Data =
                List.map
                    (function
                     | Granted t -> typedProjection t |> Granted
                     | Denied id -> Denied id) listWithTotalCount.Data
            TotalCount = listWithTotalCount.TotalCount
        }
        |> projectionEncoder

let private getAccessControlledSubjectProjectionChangeEncoder<'Subject, 'SubjectId, 'Projection
                                                                when 'SubjectId :> SubjectId
                                                                and  'Subject :> Subject<'SubjectId>>
    (projection: UntypedSubjectProjectionDef<'Subject>)
    : Encoder<AccessControlledSubjectChange<'Subject, 'SubjectId>> =
    let typedProjection = fun subj -> projection.Projection subj :?> 'Projection
    let trnEncoder = generateAutoEncoder<AccessControlledSubjectProjectionChange<'Projection, 'SubjectId>>
    (function
     | AccessControlledSubjectChange.Granted change ->
         match change with
         | SubjectChange.Updated versionedSubject ->
             {
                 Projected = typedProjection versionedSubject.Subject
                 UpdatedOn = versionedSubject.UpdatedOn
             }
             |> SubjectProjectionChange.Updated
             |> AccessControlledSubjectProjectionChange.Granted
         | SubjectChange.NotInitialized ->
            SubjectProjectionChange.NotInitialized
            |> AccessControlledSubjectProjectionChange.Granted
     | AccessControlledSubjectChange.Denied subjId -> AccessControlledSubjectProjectionChange.Denied subjId)
    >> trnEncoder

let buildSubjectProjectionEncoders
    (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>) =
    lifeCycleDef.ProjectionDefs.Map
    |> Map.map (fun _ projection ->
        typeof<AnchorTypeForModule>.DeclaringType
            .GetMethod(nameof getSubjectProjectionEncoder, BindingFlags.NonPublic ||| BindingFlags.Static)
            .MakeGenericMethod([| typeof<'Subject>; projection.ProjectionType |])
            .Invoke(null, [| projection |])
            :?> Encoder<'Subject>)

let buildAccessControlledSubjectProjectionListEncoders
    (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>) =
    lifeCycleDef.ProjectionDefs.Map
    |> Map.map (fun _ projection ->
        typeof<AnchorTypeForModule>.DeclaringType
            .GetMethod(nameof getAccessControlledSubjectProjectionListEncoder, BindingFlags.NonPublic ||| BindingFlags.Static)
            .MakeGenericMethod([| typeof<'Subject>; projection.ProjectionType; typeof<'SubjectId> |])
            .Invoke(null, [| projection |])
            :?> Encoder<List<AccessControlled<'Subject, 'SubjectId>>>)

let buildAccessControlledSubjectProjectionListWithTotalCountEncoders
    (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>) =
    lifeCycleDef.ProjectionDefs.Map
    |> Map.map (fun _ projection ->
        typeof<AnchorTypeForModule>.DeclaringType
            .GetMethod(nameof getAccessControlledSubjectProjectionListWithTotalCountEncoder, BindingFlags.NonPublic ||| BindingFlags.Static)
            .MakeGenericMethod([| typeof<'Subject>; projection.ProjectionType; typeof<'SubjectId> |])
            .Invoke(null, [| projection |])
            :?> Encoder<ListWithTotalCount<AccessControlled<'Subject, 'SubjectId>>>)

let buildAccessControlledSubjectProjectionChangeEncoders
    (lifeCycleAdapter: HostedLifeCycleAdapter<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>) =
    lifeCycleAdapter.LifeCycle.Invoke
        { new FullyTypedLifeCycleFunction<_, _, _, _, _, _, _> with
            member _.Invoke lifeCycle =
                lifeCycle.Definition.ProjectionDefs.Map
                |> Map.map (fun _ projection ->
                    typeof<AnchorTypeForModule>.DeclaringType
                        .GetMethod(nameof getAccessControlledSubjectProjectionChangeEncoder, BindingFlags.NonPublic ||| BindingFlags.Static)
                        .MakeGenericMethod([| typeof<'Subject>; typeof<'SubjectId>; projection.ProjectionType |])
                        .Invoke(null, [| projection |])
                        :?> Encoder<AccessControlledSubjectChange<'Subject, 'SubjectId>>) }