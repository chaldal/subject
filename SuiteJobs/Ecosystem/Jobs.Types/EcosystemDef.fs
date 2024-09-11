[<AutoOpen>]
module SuiteJobs.Types.EcosystemDef

let jobsDef =
    // TODO: ideally "Jobs" name should be configurable for reusability but will hardcode 'cause we can't support two such ecosystems in one biosphere anyway (at very least code type labels will be a rabbit hole)
    let ecosystemDef : EcosystemDef = newEcosystemDef "Jobs"

    // Life Cycles

    let (job: LifeCycleDef<Job, JobAction, JobOpError, JobConstructor, JobLifeEvent, JobIndex, JobId>,
             ecosystemDef) =
             addLifeCycleDef ecosystemDef "Job"

    let (batch: LifeCycleDef<Batch, BatchAction, BatchOpError, BatchConstructor, BatchLifeEvent, BatchIndex, BatchId>,
             ecosystemDef) =
             addLifeCycleDef ecosystemDef "Batch"

    let (recurringJob: LifeCycleDef<RecurringJob, RecurringJobAction, RecurringJobOpError, RecurringJobConstructor, RecurringJobLifeEvent, RecurringJobIndex, RecurringJobId>,
             ecosystemDef) =
             addLifeCycleDef ecosystemDef "RecurringJob"

    let (dispatcher: LifeCycleDef<Dispatcher, DispatcherAction, DispatcherOpError, DispatcherConstructor, DispatcherLifeEvent, DispatcherIndex, DispatcherId>,
             ecosystemDef) =
             addLifeCycleDef ecosystemDef "Dispatcher"

    let (recurringJobsManifest: LifeCycleDef<RecurringJobsManifest, RecurringJobsManifestAction, RecurringJobsManifestOpError, RecurringJobsManifestConstructor, RecurringJobsManifestLifeEvent, RecurringJobsManifestIndex, RecurringJobsManifestId>,
             ecosystemDef) =
             addLifeCycleDef ecosystemDef "RecurringJobsManifest"

    {|
          EcosystemDef = ecosystemDef
          LifeCycles =
            {|
               recurringJobsManifest = recurringJobsManifest
               dispatcher = dispatcher
               recurringJob = recurringJob
               batch = batch
               job = job
            |}
    |}