[<AutoOpen>]
module LibLifeCycle.Views.EcosystemHealthcheckView

open LibLifeCycle
open LibLifeCycle.Views.Healthcheck
open LibLifeCycle.MetaServices
open LibLifeCycle.ViewAccessBuilder
open Microsoft.Extensions.Logging

type SubjectTransactionHealthcheckViewEnvironment = {
    Clock: Service<Clock>
    HealthcheckManager: Service<EcosystemHealthManager>
    Logger: ILogger<SubjectTransactionHealthcheckViewEnvironment>
} with interface Env

let private view
    (env: SubjectTransactionHealthcheckViewEnvironment)
    (_dummyInput: int)
    : ViewResult<EcosystemHealthcheckResult, HealthcheckViewOpError> =
    view {
        let! now = env.Clock.Query Now
        let! res = env.HealthcheckManager.Query PerformEcosystemHealthcheck now
        match res with
        | Ok res ->
            match res with
            | EcosystemHealthcheckResult.AllClear ->
                ()
            | EcosystemHealthcheckResult.ProductionIssues _
            | EcosystemHealthcheckResult.CriticalAlarms _ ->
                let details = sprintf "%A" res
                env.Logger.LogWarning("Ecosystem Healthcheck found problems: {details}", details)
            return res

        | Error errorMessage ->
            return HealthcheckViewOpError.TransientError errorMessage
    }

let private accessRules = [
    // grant to everyone so a monitoring tool can use it
    grant
]

let createEcosystemHealthcheckView<'Session, 'Role when 'Role : comparison>
    ecosystemHealthcheckViewDef
    : View<int, EcosystemHealthcheckResult, HealthcheckViewOpError, AccessPredicateInput, 'Session, 'Role, SubjectTransactionHealthcheckViewEnvironment> =
    ViewBuilder.newView<int, EcosystemHealthcheckResult, HealthcheckViewOpError, 'Session, 'Role> ecosystemHealthcheckViewDef
    |> ViewBuilder.withApiAccessRestrictedByRules accessRules
    |> ViewBuilder.withRead           view
    |> ViewBuilder.build
