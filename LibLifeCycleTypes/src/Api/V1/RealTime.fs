[<AutoOpen>]
module LibLifeCycleTypes.Api.V1.RealTime

open LibLifeCycleTypes

[<RequireQualifiedAccess>]
type ApiSubjectChange<'Subject> =
| Updated of VersionedData<'Subject>
| NotInitialized

type AccessControlledSubjectChange<'Subject, 'Id> =
     AccessControlled<ApiSubjectChange<'Subject>, 'Id>

type ClientApi = unit

type ServerApi = unit

[<RequireQualifiedAccess>]
type ClientStreamApi =
// Can be invoked any number of times for the same stream. Subsequent invocations will return the same underlying stream but can be used to synchronize
// the version held by the client (MaybeClientVersion) with that held by the web node.
| ObserveSubject of LifeCycleName: string * SubjectIdStr: string * MaybeProjectionName: Option<string> * MaybeClientVersion: Option<ComparableVersion>
// Added to facilitate biosphere support, basically allowing ecosystem name to be provided.
| ObserveSubjectV2 of EcosystemName: string * LifeCycleName: string * SubjectIdStr: string * MaybeProjectionName: Option<string> * MaybeClientVersion: Option<ComparableVersion>

[<RequireQualifiedAccess>]
type ServerStreamApi =
| SubjectChanged of AccessControlledSubjectChange: string
