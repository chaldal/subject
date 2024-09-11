[<AutoOpen>]
module LibLifeCycleTypes.Api.V1.Shared

open LibLifeCycleTypes

type VersionedData<'Data> =
    {
        Data: 'Data
        Version: ComparableVersion
    }

[<RequireQualifiedAccess>]
module VersionedData =
    let data (versionedData: VersionedData<'Data>): 'Data =
        versionedData.Data

    let map (fn: 'Data -> 'U) (versionedData: VersionedData<'Data>): VersionedData<'U> =
        {
            Data = fn versionedData.Data
            Version = versionedData.Version
        }