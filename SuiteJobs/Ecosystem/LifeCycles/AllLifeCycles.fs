[<AutoOpen>]
module SuiteJobs.LifeCycles.AllLifeCycles

open LibLifeCycle

open SuiteJobs.LifeCycles.Connectors
open SuiteJobs.Types
open SuiteJobs.LifeCycles

type JobsEcosystem = {
    JobRunnerConnector: JobRunnerConnector
    JobLifeCycle: LifeCycle<Job, JobAction, JobOpError, JobConstructor, JobLifeEvent, JobIndex, JobId, AccessPredicateInput, NoSession, NoRole, JobEnvironment>
    Ecosystem: Ecosystem
}

let createJobsEcosystem (runJob: RunJobDelegate) : JobsEcosystem=
    let jobRunnerConnector = createJobRunnerConnector runJob
    let jobLifeCycle = createJobLifeCycle jobRunnerConnector

    let ecosystem =
        EcosystemBuilder.newEcosystem jobsDef.EcosystemDef
        |> EcosystemBuilder.withNoSessionHandler
        |> EcosystemBuilder.addConnector jobRunnerConnector
        |> EcosystemBuilder.addLifeCycle jobLifeCycle
        |> EcosystemBuilder.addLifeCycle batchLifeCycle
        |> EcosystemBuilder.addLifeCycle recurringJobLifeCycle
        |> EcosystemBuilder.addLifeCycle recurringJobsManifestLifeCycle
        |> EcosystemBuilder.addLifeCycle dispatcherLifeCycle
        |> EcosystemBuilder.build

    { Ecosystem = ecosystem
      JobLifeCycle = jobLifeCycle
      JobRunnerConnector = jobRunnerConnector }