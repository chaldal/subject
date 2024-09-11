[<AutoOpen>]
module LibLifeCycleTypes.Api.V1.Http

open LibLifeCycleTypes
open LibLifeCycleTypes.Api.V1

type ApiActOrConstructAndWaitOnLifeEventResult<'Data, 'LifeEvent when 'LifeEvent :> LifeEvent> =
| LifeEventTriggered      of FinalValueAfterEvent: VersionedData<'Data> * TriggeredEvent: 'LifeEvent
| WaitOnLifeEventTimedOut of InitialValueAfterActionOrConstruction: VersionedData<'Data>
with
    member this.VersionedData: VersionedData<'Data> =
        match this with
        | LifeEventTriggered (versionedData, _)
        | WaitOnLifeEventTimedOut versionedData -> versionedData
    
    member this.TriggeredEvent: Option<'LifeEvent> =
        match this with
        | LifeEventTriggered (_, event) -> Some event
        | WaitOnLifeEventTimedOut _     -> None

