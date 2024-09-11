module internal LibLifeCycleHost.Web.Api.V1.JsonEncoding

open System.Reflection
open LibLifeCycleTypes
open LibLifeCycle
open LibLifeCycleHost
open LibLifeCycleHost.Web.WebApiJsonEncoding
open LibLifeCycleTypes.Api.V1.Shared
open LibLifeCycleTypes.Api.V1.RealTime
open LibLifeCycleTypes.Api.V1.Http

type AnchorTypeForModule = private AnchorTypeForModule of unit // To get typeof<Module>

let private getVersionedSubjectProjectionEncoder<'Subject, 'Projection>
        (projection: UntypedSubjectProjectionDef<'Subject>)
        : Encoder<VersionedData<'Subject>> =
    let typedProjection =
        fun versionedData ->
            {
                Data = projection.Projection versionedData.Data :?> 'Projection
                Version = versionedData.Version
            }
    let versionedProjectionEncoder = generateAutoEncoder<VersionedData<'Projection>>
    typedProjection >> versionedProjectionEncoder

let private getAccessControlledSubjectChangeEncoder<'Subject, 'SubjectId, 'Projection
            when 'Subject   :> Subject<'SubjectId>
            and  'SubjectId :> SubjectId>
        (projection: UntypedSubjectProjectionDef<'Subject>)
        : Encoder<AccessControlledSubjectChange<'Subject, 'SubjectId>> =
    let typedProjection =
        fun versionedData ->
            {
                Data = projection.Projection versionedData.Data :?> 'Projection
                Version = versionedData.Version
            }
    let accessControlledSubjectChangeEncoder = generateAutoEncoder<AccessControlledSubjectChange<'Projection, 'SubjectId>>
    fun accessControlledSubjectChange ->
        accessControlledSubjectChange
        |> AccessControlled.map (fun subjectChange ->
            match subjectChange with
            | ApiSubjectChange.Updated versionedData ->
                versionedData
                |> typedProjection
                |> ApiSubjectChange.Updated
            | ApiSubjectChange.NotInitialized -> ApiSubjectChange.NotInitialized
        )
        |> accessControlledSubjectChangeEncoder

let private getAccessControlledVersionedSubjectProjectionListEncoder<'Subject, 'Projection, 'SubjectId>
        (projection: UntypedSubjectProjectionDef<'Subject>)
        : Encoder<List<AccessControlled<VersionedData<'Subject>, 'SubjectId>>> =
    let typedProjection =
        fun versionedData ->
            {
                Data = projection.Projection versionedData.Data :?> 'Projection
                Version = versionedData.Version
            }
    let projectionEncoder = generateAutoEncoder<List<AccessControlled<VersionedData<'Projection>, 'SubjectId>>>
    List.map
        (function
         | Granted t -> typedProjection t |> Granted
         | Denied id -> Denied id)
     >> projectionEncoder

let private getAccessControlledVersionedSubjectProjectionListWithTotalCountEncoder<'Subject, 'Projection, 'SubjectId>
        (projection: UntypedSubjectProjectionDef<'Subject>)
        : Encoder<ListWithTotalCount<AccessControlled<VersionedData<'Subject>, 'SubjectId>>> =
    let typedProjection =
        fun versionedData ->
            {
                Data = projection.Projection versionedData.Data :?> 'Projection
                Version = versionedData.Version
            }
    let projectionEncoder = generateAutoEncoder<ListWithTotalCount<AccessControlled<VersionedData<'Projection>, 'SubjectId>>>
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

let private getActOrConstructAndWaitOnLifeEventResultProjectionEncoder<'Subject, 'Projection, 'LifeEvent when 'LifeEvent :> LifeEvent>
        (projection: UntypedSubjectProjectionDef<'Subject>)
        : Encoder<ApiActOrConstructAndWaitOnLifeEventResult<'Subject, 'LifeEvent>> =
    let typedProjection =
        fun versionedData ->
            {
                Data = projection.Projection versionedData.Data :?> 'Projection
                Version = versionedData.Version
            }
    let projectionEncoder = generateAutoEncoder<ApiActOrConstructAndWaitOnLifeEventResult<'Projection, 'LifeEvent>>
    (function
    | WaitOnLifeEventTimedOut versionedSubject -> (typedProjection versionedSubject) |> WaitOnLifeEventTimedOut
    | LifeEventTriggered(versionedSubject, lifeEvent) -> ((typedProjection versionedSubject), lifeEvent) |> LifeEventTriggered)
    >> projectionEncoder

let buildVersionedSubjectProjectionEncoders<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId
                    when 'Subject              :> Subject<'SubjectId>
                    and  'LifeAction           :> LifeAction
                    and  'OpError              :> OpError
                    and  'Constructor          :> Constructor
                    and  'LifeEvent            :> LifeEvent
                    and  'LifeEvent            : comparison
                    and  'SubjectIndex         :> SubjectIndex<'OpError>
                    and  'SubjectId            :> SubjectId
                    and  'SubjectId            : comparison>
        (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
        : Map<string, Encoder<VersionedData<'Subject>>> =
    lifeCycleDef.ProjectionDefs.Map
    |> Map.map (fun _ projection ->
        typeof<AnchorTypeForModule>.DeclaringType
            .GetMethod(nameof getVersionedSubjectProjectionEncoder, BindingFlags.NonPublic ||| BindingFlags.Static)
            .MakeGenericMethod([| typeof<'Subject>; projection.ProjectionType |])
            .Invoke(null, [| projection |])
            :?> Encoder<VersionedData<'Subject>>)

let buildAccessControlledSubjectChangeEncoders<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId
                    when 'Subject              :> Subject<'SubjectId>
                    and  'LifeAction           :> LifeAction
                    and  'OpError              :> OpError
                    and  'Constructor          :> Constructor
                    and  'LifeEvent            :> LifeEvent
                    and  'LifeEvent            : comparison
                    and  'SubjectId            :> SubjectId
                    and  'SubjectId            : comparison>
        (lifeCycleAdapter: IHostedOrReferencedLifeCycleAdapter<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>)
        : Map<string, Encoder<AccessControlledSubjectChange<'Subject, 'SubjectId>>> =
    lifeCycleAdapter.ReferencedLifeCycle.Invoke
        { new FullyTypedReferencedLifeCycleFunction<_, _, _, _, _, _, _> with
            member _.Invoke referencedLifeCycle =
                referencedLifeCycle.Def.ProjectionDefs.Map
                |> Map.map (fun _ projection ->
                    typeof<AnchorTypeForModule>.DeclaringType
                        .GetMethod(nameof getAccessControlledSubjectChangeEncoder, BindingFlags.NonPublic ||| BindingFlags.Static)
                        .MakeGenericMethod([| typeof<'Subject>; typeof<'SubjectId>; projection.ProjectionType |])
                        .Invoke(null, [| projection |])
                        :?> Encoder<AccessControlledSubjectChange<'Subject, 'SubjectId>>) }

let buildAccessControlledVersionedSubjectProjectionListEncoders
        (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>) =
    lifeCycleDef.ProjectionDefs.Map
    |> Map.map (fun _ projection ->
        typeof<AnchorTypeForModule>.DeclaringType
            .GetMethod(nameof getAccessControlledVersionedSubjectProjectionListEncoder, BindingFlags.NonPublic ||| BindingFlags.Static)
            .MakeGenericMethod([| typeof<'Subject>; projection.ProjectionType; typeof<'SubjectId> |])
            .Invoke(null, [| projection |])
            :?> Encoder<List<AccessControlled<VersionedData<'Subject>, 'SubjectId>>>)

let buildAccessControlledVersionedSubjectProjectionListWithTotalCountEncoders
        (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>) =
    lifeCycleDef.ProjectionDefs.Map
    |> Map.map (fun _ projection ->
        typeof<AnchorTypeForModule>.DeclaringType
            .GetMethod(nameof getAccessControlledVersionedSubjectProjectionListWithTotalCountEncoder, BindingFlags.NonPublic ||| BindingFlags.Static)
            .MakeGenericMethod([| typeof<'Subject>; projection.ProjectionType; typeof<'SubjectId> |])
            .Invoke(null, [| projection |])
            :?> Encoder<ListWithTotalCount<AccessControlled<VersionedData<'Subject>, 'SubjectId>>>)

let buildActOrConstructAndWaitOnLifeEventResultProjectionEncoders
        (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>) =
    lifeCycleDef.ProjectionDefs.Map
    |> Map.map (fun _ projection ->
        typeof<AnchorTypeForModule>.DeclaringType
            .GetMethod(nameof getActOrConstructAndWaitOnLifeEventResultProjectionEncoder, BindingFlags.NonPublic ||| BindingFlags.Static)
            .MakeGenericMethod([| typeof<'Subject>; projection.ProjectionType; typeof<'LifeEvent> |])
            .Invoke(null, [| projection |])
            :?> Encoder<ApiActOrConstructAndWaitOnLifeEventResult<'Subject, 'LifeEvent>>)