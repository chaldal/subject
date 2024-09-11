namespace LibLifeCycleHost
// Grains can't be in Modules, due to a bug in the way Orleans builds up Grain identity

open LibLifeCycle.LifeCycleReflection
open Orleans
open Orleans.Concurrency
open System.Threading.Tasks
open LibLifeCycleCore
open Microsoft.Extensions.Logging

[<StatelessWorker>]
[<Reentrant>]
type SubjectReflectionGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId
                when 'Subject              :> Subject<'SubjectId>
                and  'LifeAction           :> LifeAction
                and  'OpError              :> OpError
                and  'Constructor          :> Constructor
                and  'LifeEvent            :> LifeEvent
                and  'LifeEvent            :  comparison
                and  'SubjectId            :> SubjectId
                and  'SubjectId            :  comparison>
        (
            lifeCycleAdapter: HostedLifeCycleAdapter<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>,
            logger: Microsoft.Extensions.Logging.ILogger<SubjectReflectionGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>>
        ) =

    inherit Grain()

    let wrapExceptions (methodName: string) (f: unit -> 'T) : 'T =
        try
            f ()
        with
        | :? TransientSubjectException as ex ->
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw()
            shouldNotReachHereBecause "line above throws"
        | :? PermanentSubjectException as ex ->
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw()
            shouldNotReachHereBecause "line above throws"
        | ex ->
            logger.LogError(ex, $"Exception in SubjectReflectionGrain.${methodName}")
            TransientSubjectException ($"Life Cycle %s{lifeCycleAdapter.LifeCycle.Name} %s{methodName}", ex.ToString()) |> raise

    interface ISubjectReflectionGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId> with

        member _.IsActionAllowed (subject: 'Subject) (action: 'LifeAction) : Task<bool> =
            (fun () -> isActionAllowedForSubject lifeCycleAdapter.LifeCycle subject action)
            |> wrapExceptions "IsActionAllowed"
            |> Task.FromResult

        member _.GetAllowedActionCaseNames (subject: 'Subject) : Task<list<string>> =
            (fun () ->
                allowedActionsForSubject lifeCycleAdapter.LifeCycle subject
                |> List.map (fun unionCase -> unionCase.CaseName))
            |> wrapExceptions "GetAllowedActionCaseNames"
            |> Task.FromResult