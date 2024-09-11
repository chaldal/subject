[<AutoOpen>]
module LibLifeCycle.Views.Healthcheck.Types

open System
open LibLifeCycleTypes

[<RequireQualifiedAccess>]
type EcosystemHealthcheckProductionIssue =
| SideEffectsWarnings           of Count: uint32 * AffectSubjects: string
| SideEffectsNotTooManyFailures of Count: byte   * AffectSubjects: string
| TimersOverdueNotSeverely      of Count: uint16 * OldestNextTickOn: DateTimeOffset * AffectSubjects: string

[<RequireQualifiedAccess>]
type EcosystemHealthcheckCriticalAlarm =
| SubjectTransactionStalledPrepared of Count: uint32 * AffectSubjects: string
| SideEffectsStalled                of Count: uint32 * AffectSubjects: string
| SideEffectsManyFailures           of Count: uint32 * AffectSubjects: string
| TimersOverdueSeverely             of Count: uint32 * OldestNextTickOn: DateTimeOffset * AffectSubjects: string

[<RequireQualifiedAccess>]
type EcosystemHealthcheckResult =
| AllClear
| ProductionIssues of list<EcosystemHealthcheckProductionIssue>
| CriticalAlarms   of list<EcosystemHealthcheckCriticalAlarm> * list<EcosystemHealthcheckProductionIssue>

[<RequireQualifiedAccess>]
type HealthcheckViewOpError =
| TransientError of Message: string
with interface OpError

let
#if FABLE_COMPILER
    inline
#endif
    addEcosystemHealthcheckViewDef
    (ecosystemDef: EcosystemDef)
    : ViewDef<int, EcosystemHealthcheckResult, HealthcheckViewOpError> * EcosystemDef =
    addViewDef ecosystemDef "EcosystemHealthcheck"