namespace LibLifeCycleHost
// Grains can't be in Modules, due to a bug in the way Orleans builds up Grain identity

open System
open LibLifeCycle
open LibLifeCycleHost
open Orleans
open Orleans.Concurrency
open Orleans.Runtime
open System.Threading.Tasks
open LibLifeCycleCore

[<StatelessWorker>]
[<Reentrant>]
type SubjectIdGenerationGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId
                when 'Subject              :> Subject<'SubjectId>
                and  'LifeAction           :> LifeAction
                and  'OpError              :> OpError
                and  'Constructor          :> Constructor
                and  'LifeEvent            :> LifeEvent
                and  'LifeEvent            :  comparison
                and  'SubjectId            :> SubjectId
                and  'SubjectId            :  comparison>
        (lifeCycleAdapter: HostedLifeCycleAdapter<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>,
         ctx: IGrainActivationContext, valueSummarizers: ValueSummarizers, serviceProvider: IServiceProvider,
         unscopedLogger: Microsoft.Extensions.Logging.ILogger<SubjectIdGenerationGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>>) =

    inherit Grain()

    let (grainPartition, grainPKey) =
        let (grainPartition, pKey) = ctx.GrainIdentity.GetPrimaryKey()
        ((grainPartition |> GrainPartition), pKey)

    let logger = newGrainScopedLogger valueSummarizers unscopedLogger lifeCycleAdapter.LifeCycle.Name grainPartition grainPKey

    let wrapExceptions (methodName: string) (createTask: unit -> Task<'T>) : Task<'T> =
        task {
            try
                let! res = createTask ()
                return res
            with
            | :? TransientSubjectException as ex ->
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw()
                return shouldNotReachHereBecause "line above throws"
            | :? PermanentSubjectException as ex ->
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw()
                return shouldNotReachHereBecause "line above throws"
            | ex ->
                logger.ErrorExn ex "Exception in SubjectIdGenerationGrain.%a" (logger.P "methodName") methodName
                return TransientSubjectException ($"Life Cycle %s{lifeCycleAdapter.LifeCycle.Name} %s{methodName}", ex.ToString()) |> raise
        }

    interface ISubjectIdGenerationGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId> with

        member _.GenerateIdV2 (callOrigin: CallOrigin) (ctor: 'Constructor) : Task<Result<'SubjectId, GrainIdGenerationError<'OpError>>> =
            fun () -> task {
                let (IdGenerationResult idGenTask) = lifeCycleAdapter.LifeCycle.GenerateId callOrigin serviceProvider ctor
                match! idGenTask with
                | Ok id ->
                    return Ok id

                | Error err ->
                    logger.Warn "JUST ==> IDGEN %a %a ==> ERROR %a" (logger.P "callOrigin") callOrigin (logger.P "ctor") ctor (logger.P "error") err
                    return err |> GrainIdGenerationError.IdGenerationError |> Error
            }
            |> wrapExceptions "GenerateIdV2"

        member this.GenerateId (ctor: 'Constructor) : Task<Result<'SubjectId, GrainIdGenerationError<'OpError>>> =
            (this :> ISubjectIdGenerationGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>).GenerateIdV2 CallOrigin.Internal ctor

    interface ITrackedGrain with
        member this.GetTelemetryData (_methodInfo: System.Reflection.MethodInfo) (_: obj[]) : Option<TrackedGrainTelemetryData> = None