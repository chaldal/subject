namespace LibLifeCycleHost

open System.Threading.Tasks
open LibLifeCycleCore
open Orleans
open Orleans.Concurrency
open Orleans.Runtime

[<StatelessWorker>]
[<Reentrant>]
type DynamicSubscriptionDispatcherGrain
    (
        lifeCycleAdapters: HostedLifeCycleAdapterCollection,
        hostEcosystemGrainFactory: IGrainFactory,
        ctx: IGrainActivationContext
    ) =

    inherit Grain()

    let grainProvider =
        { new IBiosphereGrainProvider with
            member _.GetGrainFactory (_ecosystemName: string) = Task.FromResult hostEcosystemGrainFactory
            member _.IsHostedLifeCycle (_lcKey: LifeCycleKey) = true
            member _.Close () = Task.CompletedTask }

    let grainPartition =
        let (grainPartition, _pKey) = ctx.GrainIdentity.GetPrimaryKey()
        (grainPartition |> GrainPartition)

    interface IDynamicSubscriptionDispatcherGrain with
        member this.TriggerSubscription
            (maybeDedupInfo: Option<SideEffectDedupInfo>)
            (target: LocalSubjectPKeyReference)
            (subscriptionTriggerType: SubscriptionTriggerType)
            (triggeredLifeEvent: LifeEvent)
            : Task<Result<unit, SubjectFailure<GrainTriggerDynamicSubscriptionError>>> =

            backgroundTask {
                match lifeCycleAdapters.GetLifeCycleAdapterByLocalName target.LifeCycleName with
                | Some targetLifeCycleAdapter ->
                    match! targetLifeCycleAdapter.TriggerSubscriptionOnGrain
                        grainProvider
                        grainPartition
                        maybeDedupInfo
                        target.SubjectIdStr
                        subscriptionTriggerType
                        triggeredLifeEvent with
                    | Ok () ->
                        return Ok ()

                    | Error (SubjectFailure.Err err) ->
                        return
                            match err with
                            | GrainTriggerSubscriptionError.SubjectNotInitialized primaryKey ->
                                GrainTriggerDynamicSubscriptionError.SubjectNotInitialized primaryKey
                            | GrainTriggerSubscriptionError.TransitionError opError ->
                                GrainTriggerDynamicSubscriptionError.TransitionError (sprintf "%A" opError)
                            | GrainTriggerSubscriptionError.TransitionNotAllowed ->
                                GrainTriggerDynamicSubscriptionError.TransitionNotAllowed
                            | GrainTriggerSubscriptionError.LockedInTransaction ->
                                GrainTriggerDynamicSubscriptionError.LockedInTransaction
                            |> SubjectFailure.Err
                            |> Error

                    | Error (SubjectFailure.Exn message) ->
                        return Error (SubjectFailure.Exn message)

                | None ->
                    return
                        target.LifeCycleName
                        |> GrainTriggerDynamicSubscriptionError.LifeCycleNotFound
                        |> SubjectFailure.Err
                        |> Error
            }

    interface ITrackedGrain with
        member _.GetTelemetryData (_methodInfo: System.Reflection.MethodInfo) (_: obj[]) : Option<TrackedGrainTelemetryData> = None
