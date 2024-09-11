[<AutoOpen>]
module PlaceholderTypes

// These types will typically be defined by your backend,
// but as we're scaffolding an app without scaffolding a
// backend at the same time, we provide some placeholders.

type SampleWho = {
    Name:                     NonemptyString
    PrefersBananas:           bool
    UserIdStringForTelemetry: string
}

[<RequireQualifiedAccess>]
type SessionOpsAction =
| DoSomething
with
    static member TypeLabel () = "SessionOpsAction"

type SampleSession = Session<SampleWho>
type SampleSessionId = SessionId
type SampleSessionConstructor = SessionConstructor
type SampleSessionLifeEvent = SessionLifeEvent
type SampleSessionOpError = SessionOpError
type SampleSessionAction = SessionAction<SessionOpsAction>
type SampleSessionNumericIndex = SessionNumericIndex
type SampleSessionStringIndex = SessionStringIndex
type SampleSessionSearchIndex = SessionSearchIndex
type SampleSessionIndex = SessionIndex
