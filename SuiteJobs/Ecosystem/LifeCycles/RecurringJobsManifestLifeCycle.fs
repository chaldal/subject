[<AutoOpen>]
module SuiteJobs.LifeCycles.RecurringJobsManifestLifeCycle

open System
open LibLifeCycle
open SuiteJobs.Types
open LibLifeCycle.LifeCycleAccessBuilder
open type AccessTo<RecurringJobsManifestAction, RecurringJobsManifestConstructor>

type RecurringJobsManifestEnvironment = {
    Clock: Service<Clock>
    AllRecurringJobs: Service<All<RecurringJob, RecurringJobId, RecurringJobIndex, RecurringJobOpError>>
} with interface Env

let private transition (env: RecurringJobsManifestEnvironment) (recurringJobsManifest: RecurringJobsManifest) (action: RecurringJobsManifestAction) : TransitionResult<RecurringJobsManifest, RecurringJobsManifestAction, RecurringJobsManifestOpError, RecurringJobsManifestLifeEvent, RecurringJobsManifestConstructor> =
    transition {
        match action with
        | RecurringJobsManifestAction.Apply manifestedRecurringJobs ->
            let! existingRecurringJobs =
                ResultSetOptions<_>.OrderByFastestWithPage { Size = UInt16.MaxValue; Offset = 0UL }
                |> env.AllRecurringJobs.Query FetchAll
                |> Task.map (fun jobs -> jobs |> Seq.map (fun j -> j.Name, j) |> Map.ofSeq)

            let allKeys = Set.union (existingRecurringJobs |> Map.keys) manifestedRecurringJobs.Keys

            let sideEffects =
                allKeys
                |> Seq.choose (fun name ->
                    match Map.tryFind name existingRecurringJobs, manifestedRecurringJobs.GetByKey name with
                    | None, Some manifested ->
                        // In concurrent manifest apply job might be already constructed. This works only if multiple client nodes apply the same manifest
                        recurringJobLifeCycle.MaybeConstruct (RecurringJobConstructor.New (name, manifested.CronExpression, manifested.TimeZoneId, manifested.JobInstanceData))
                        |> Some
                    | Some existing, None ->
                        recurringJobLifeCycle.Act existing.Id RecurringJobAction.HardDelete
                        |> Some
                    | Some existing, Some manifested ->
                        if existing.CronExpression = manifested.CronExpression &&
                           existing.TimeZoneId = manifested.TimeZoneId &&
                           // eliminate SentOn comparison, otherwise it will always mismatch
                           existing.JobInstanceData = { manifested.JobInstanceData with SentOn = existing.JobInstanceData.SentOn } then
                            None
                        else
                            recurringJobLifeCycle.Act existing.Id (RecurringJobAction.Update (manifested.CronExpression, manifested.TimeZoneId, manifested.JobInstanceData))
                            |> Some
                    | None, None ->
                        shouldNotReachHereBecause "either existing or manifested recurring job must be present")
                |> List.ofSeq

            match sideEffects with
            | [] ->
                return Transition.Ignore // don't write no-op apply in history
            | _ ->
                yield! sideEffects
                return recurringJobsManifest
    }

let private construction (env: RecurringJobsManifestEnvironment) (_id: RecurringJobsManifestId) (ctor: RecurringJobsManifestConstructor) : ConstructionResult<RecurringJobsManifest, RecurringJobsManifestAction, RecurringJobsManifestOpError, RecurringJobsManifestLifeEvent> =
    construction {
        match ctor with
        | SingletonRecurringJobsManifestConstructor ->
            let! now = env.Clock.Query Now
            return {
                CreatedOn = now
            }
    }

let private idGeneration (_env: RecurringJobsManifestEnvironment) (ctor: RecurringJobsManifestConstructor) : IdGenerationResult<RecurringJobsManifestId, RecurringJobsManifestOpError> =
    idgen {
        match ctor with
        | SingletonRecurringJobsManifestConstructor ->
            return SingletoRecurringJobsManifestId
    }

let private shouldSendTelemetry =
    function
    | ShouldSendTelemetryFor.Constructor ctor ->
        match ctor with
        | SingletonRecurringJobsManifestConstructor -> true
    | ShouldSendTelemetryFor.LifeAction action ->
        match action with
        | RecurringJobsManifestAction.Apply _ -> true
    | ShouldSendTelemetryFor.LifeEvent _ -> false

let private responseHandler (response: SideEffectResponse) =
    seq {
        match jobsDef.LifeCycles.recurringJob.OnResponse response with
        | ActNotInitialized (_, RecurringJobAction.HardDelete, next) ->
            // In concurrent manifest apply job might be already deleted. This works only if multiple client nodes apply the same manifest
            next.Dismiss()
        | _ ->
            ()
    }

let recurringJobsManifestLifeCycle =
    newJobsLifeCycle                 jobsDef.LifeCycles.recurringJobsManifest
    |> LifeCycleBuilder.withoutApiAccess
    |> LifeCycleBuilder.withTransition   transition
    |> LifeCycleBuilder.withIdGeneration idGeneration
    |> LifeCycleBuilder.withConstruction construction
    |> LifeCycleBuilder.withSingleton    SingletonRecurringJobsManifestConstructor
    |> LifeCycleBuilder.withResponseHandler responseHandler
    |> LifeCycleBuilder.withStorage
           (StorageType.Persistent
                (PromotedIndicesConfig.Empty,
                System.TimeSpan.FromDays 365.
                |> PersistentHistoryExpiration.AfterSubjectChange |> Some
                |> PersistentHistoryRetention.Unfiltered))
    |> LifeCycleBuilder.withTelemetryRules shouldSendTelemetry
    |> LifeCycleBuilder.build
