module LibLifeCycleTypes.LegacyHttp

// TODO: delete this once all clients are migrated to work against the V1 API

type ApiActOrConstructAndWaitOnLifeEventResult<'Subject, 'LifeEvent when 'LifeEvent :> LifeEvent> =
| LifeEventTriggered      of FinalValueAfterEvent: 'Subject * TriggeredEvent: 'LifeEvent
| WaitOnLifeEventTimedOut of InitialValueAfterActionOrConstruction: 'Subject
with
    member this.Subject: 'Subject =
        match this with
        | LifeEventTriggered (subject, _)
        | WaitOnLifeEventTimedOut subject -> subject
