namespace LibLifeCycleHost // Grains can't be in Modules, due to a bug in the way Orleans builds up Grain identity

open System
open FSharp.Control
open LibLifeCycleHost
open LibLifeCycleHost.TelemetryModel
open Orleans.Concurrency
open LibLifeCycle
open LibLifeCycleCore
open System.Threading.Tasks
open Orleans
open Orleans.Runtime
open Microsoft.Extensions.DependencyInjection

[<StatelessWorker(maxLocalWorkers = Int32.MaxValue)>]
[<Reentrant>]
type ConnectorGrain<'Request, 'Env when 'Request :> Request and 'Env :> Env>
        (serviceProvider: IServiceProvider, envFactory: EnvironmentFactory<'Env>, connectorAdapter: ConnectorAdapter<'Request, 'Env>, lifeCycleAdapterCollection: HostedLifeCycleAdapterCollection,
          valueSummarizers: ValueSummarizers, grainProvider: IBiosphereGrainProvider, unscopedLogger: Microsoft.Extensions.Logging.ILogger<'Request>, ctx: IGrainActivationContext) =

    inherit Grain()

    let env            = envFactory.Invoke { CallOrigin = CallOrigin.Internal; LocalSubjectRef = None }
    let connector      = connectorAdapter.Connector
    let grainPartition = ctx.GrainIdentity.GetPrimaryKey() |> fst |> GrainPartition
    let logger         = newConnectorScopedLogger valueSummarizers unscopedLogger connector.Name grainPartition
    let trackerHook    = serviceProvider.GetService<ISideEffectTrackerHook>() |> Option.ofObj

    let rec getNextInterceptor
        (middlewares: list<ConnectorInterceptor<'Request>>)
        (serviceProvider: IServiceProvider) (req: 'Request) : Task<ResponseVerificationToken> =
        match middlewares with
        | head::tail ->
            let next env req = getNextInterceptor tail env req
            head serviceProvider req next
        | [] ->
            connector.RequestProcessor env req

    let requestProcessorInterceptorPipeline = getNextInterceptor connector.Interceptors serviceProvider

    let connectorService = createService connector.Name requestProcessorInterceptorPipeline

    // made lazy because rarely used
    let connectorMultiResponseService = lazy(createMultiResponseService connector.Name requestProcessorInterceptorPipeline)

    member _.RespondToSubjectGrain (requestor: SubjectPKeyReference) (sourceAction: LifeAction) (request: 'Request) (response: 'Response) : Task =
        task {
            match lifeCycleAdapterCollection.GetLifeCycleAdapterByKey requestor.LifeCycleKey with
            | Some adapter ->
                let maybeDedupInfo = None // no dedup for connector response. Connector is fire-and-forget i.e. at most one

                // Sould we actually retry failed response delivery to SubjectGrain, to reduce risk of connector timeouts?
                // No! because it violates at-most-once convention of connector.
                // It also would further complicate enumerable responses, e.g. what if one of responses fail? Should we stop or skip & continue?
                // TODO: maybe the only exception, is to retry after a Orleans.Storage.InconsistentStateException

                match! adapter.RunActionOnGrain grainProvider grainPartition maybeDedupInfo requestor.SubjectIdStr sourceAction None with
                | Ok _ ->
                    logger.Info "REQUEST %a ==> RESPONSE %a ==> ACT %a ==> REQUESTOR %a ==> OK"
                        (logger.P "request")   request
                        (logger.P "response")  response
                        (logger.P "action")    sourceAction
                        (logger.P "requestor") requestor
                | Error err ->
                    logger.Warn "REQUEST %a ==> RESPONSE %a ==> ACT %a ==> REQUESTOR %a ==> ERROR %a"
                        (logger.P "request")   request
                        (logger.P "response")  response
                        (logger.P "action")    sourceAction
                        (logger.P "requestor") requestor
                        (logger.P "error")     err
            | None ->
                logger.Warn "REQUEST %a ==> RESPONSE %a ==> ACT %a ==> REQUESTOR %a ==> ERROR NoLifeCycle"
                    (logger.P "request")   request
                    (logger.P "response")  response
                    (logger.P "action")    sourceAction
                    (logger.P "requestor") requestor
        }

    interface IConnectorGrain<'Request, 'Env> with

        member this.SendRequest<'Response> (buildRequest: ResponseChannel<'Response> -> 'Request) (buildAction: 'Response -> LifeAction) (requestor: SubjectPKeyReference) (sideEffectId: GrainSideEffectId) : Task =
            task {
                try
                    logger.Info "Received Request from REQUESTOR %a" (logger.P "requestor") requestor


                    let request, responseTask = connectorService.QueryAndReturnRequestResponse buildRequest
                    let! response = responseTask
                    let sourceAction = buildAction response
                    do! this.RespondToSubjectGrain requestor sourceAction request response

                    trackerHook |> Option.iter (fun hook -> hook.OnSideEffectProcessed sideEffectId)
                    return ()
                with
                | ex ->
                    logger.WarnExn ex "REQUEST ==> REQUESTOR %a ==> ERROR Exception"
                            (logger.P "requestor") requestor
                    trackerHook |> Option.iter (fun hook -> hook.OnSideEffectProcessed sideEffectId)
            } |> Task.Ignore

        member this.SendRequestMultiResponse<'Response> (buildRequest: MultiResponseChannel<'Response> -> 'Request) (buildAction: 'Response -> LifeAction) (requestor: SubjectPKeyReference) (sideEffectId: GrainSideEffectId) : Task =
            task {
                try
                    logger.Info "Received Request from REQUESTOR %a" (logger.P "requestor") requestor

                    let request, enumerable = connectorMultiResponseService.Force().QueryAndReturnRequestResponse buildRequest
                    // TODO: use while! keyword in F# 8 or later
                    do!
                        AsyncSeq.ofAsyncEnum enumerable
                        |> AsyncSeq.iterAsync (fun response ->
                            async {
                                let sourceAction = buildAction response
                                do! this.RespondToSubjectGrain requestor sourceAction request response |> Async.AwaitTask
                            }
                        )
                        |> Async.StartImmediateAsTask // this should keep it on correct thread context

                    trackerHook |> Option.iter (fun hook -> hook.OnSideEffectProcessed sideEffectId)
                    return ()
                with
                | ex ->
                    logger.WarnExn ex "REQUEST ==> REQUESTOR %a ==> ERROR Exception"
                            (logger.P "requestor") requestor
                    trackerHook |> Option.iter (fun hook -> hook.OnSideEffectProcessed sideEffectId)
            } |> Task.Ignore

    interface ITrackedGrain with
        member this.GetTelemetryData (_methodInfo: System.Reflection.MethodInfo) (_: obj[]) : Option<TrackedGrainTelemetryData> =
            if connector.ShouldSendTelemetry then
                Some { Type = OperationType.GrainCallConnector; Name = $"%s{connector.Name} Request"; Scope = logger.Scope; Partition = grainPartition }
            else
                None