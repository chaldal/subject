module LibLifeCycleHost.ApplicationInsights

open System
open System.Net
open System.Threading
open System.Threading.Tasks
open LibLifeCycleCore
open LibLifeCycleHost.TelemetryModel
open LibLifeCycleHost.Web
open Microsoft.ApplicationInsights
open Microsoft.ApplicationInsights.Channel
open Microsoft.ApplicationInsights.DataContracts
open Microsoft.ApplicationInsights.DependencyCollector
open Microsoft.ApplicationInsights.Extensibility
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Primitives

/// Tracking SQL leads to a bloat in AppInsights, and costs us a lot of $$$
/// This processor logs only SQL that takes a long time to execute
type private SlowSqlDependencies(next: ITelemetryProcessor) =
    let shouldLogThreshold = TimeSpan.FromMilliseconds 1000.;
    let shouldSkip (item: ITelemetry) =
        match item with
        | :? DependencyTelemetry as dependency when dependency.Type = "SQL" ->
            // Capture SQL Dependencies that take longer than threshold
            // ALSO NOTE: SQL Command Text capture requires full ASP.NET Core
            // (i.e. we need to run on ASP.NET Core directly; or we need to migrate to using Microsoft.Data.SqlClient)
            // (.. or we need to install a profiler agent on all servers, which is too much work and messy ..)
            // so for now, we'll do the logging in Entity Framework as a simple trace message
            dependency.Duration <= shouldLogThreshold
        | _ ->
            false

    interface ITelemetryProcessor with
        member this.Process item =
            if not (shouldSkip item) then
                next.Process item

type private RequestTelemetryProcessor(next: ITelemetryProcessor) =
    static let shouldSendViewTelemetry (req : RequestTelemetry) =

        // TODO: test. add proper telemetry rules for views too
        [
            "/healthcheck"
            "/hello"
            "/api/v1/view/RequestQueuePermanentFailures"
            "/api/v1/view/EcosystemHealthcheck"
            "/api/v1/realTime"
        ]
        |> List.exists req.Url.AbsolutePath.StartsWith
        |> not

    static let shouldSendLifeCycleTelemetry (req: RequestTelemetry) =
        // send telemetry for all not OK responses
        req.Success.HasValue = false ||
        req.Success.Value = false ||
        req.Properties.ContainsKey "RateLimitExceededForKeys" ||
        // and for OK responses unless requested to skip
        not (req.Properties.ContainsKey "SkipTelemetryOnSuccess")

    static let shouldSendTelemetry (req: RequestTelemetry) =
        shouldSendViewTelemetry req && shouldSendLifeCycleTelemetry req

    interface ITelemetryProcessor with
        member __.Process (item: ITelemetry) : unit =
            match item with
            | :? RequestTelemetry as reqTelemetry ->
                shouldSendTelemetry reqTelemetry
            | _ ->
                true
            |> fun sendTelemetry ->
                if sendTelemetry then
                    next.Process item
                else
                    ()

type private DependencyTelemetryProcessor(next: ITelemetryProcessor) =
    let worthAttentionSideEffectProperties = ["MultipleAttempts"; "HadFatalTransientError"; "Rehydrated"]
    let worthAttentionSideEffectDurationThreshold = TimeSpan.FromSeconds 30.
    let shouldSend (item: ITelemetry) =
        match item with
        | :? DependencyTelemetry as dependency ->
            match dependency.Type |> OperationType.TryParseIconString with
            | None ->
                // exclude built-in outgoing HTTP dependencies which succeeded, they are usually redundant and manifest in other telemetry items anyway,
                // such as connector calls that invoked them, or incoming Request telemetry on server side
                not (String.Equals(dependency.Type, "HTTP", StringComparison.OrdinalIgnoreCase)) || (dependency.Success.HasValue && dependency.Success.Value = false)
            | Some OperationType.GrainCallVanilla
            | Some OperationType.GrainCallTriggerSubscription
            | Some OperationType.GrainCallConnector
            | Some OperationType.GrainCallTriggerTimer
            | Some OperationType.SideEffectTransaction
            | Some OperationType.SideEffectDeleteSelf
            | Some OperationType.TestSimulation
            | Some OperationType.TestTimeForward ->
                dependency.ProactiveSamplingDecision <- SamplingDecision.SampledIn
                true

            | Some OperationType.SideEffectVanilla ->
                // Note: to maintain proper parent-child hierarchy in Telemetry, keep it in sync with SideEffectProcessor
                // Unfortunately App Insights / Activity design makes it hard to filter out already created telemetry while keeping proper parent-child relations
                // Here we eliminate all Vanilla side effect telemetry without anomalies
                let sideEffectItem = dependency
                sideEffectItem.ProactiveSamplingDecision <- SamplingDecision.SampledIn
                let succeeded = sideEffectItem.Success.HasValue && sideEffectItem.Success.Value
                if succeeded then
                    sideEffectItem.Properties.Remove "TargetSubjectId" |> ignore // will be visible in underlying grain call
                let worthKeeping =
                    (not succeeded) ||
                    (worthAttentionSideEffectProperties |> Seq.exists sideEffectItem.Properties.ContainsKey) ||
                    sideEffectItem.Duration > worthAttentionSideEffectDurationThreshold
                worthKeeping
        | _ -> true

    interface ITelemetryProcessor with
        member _.Process (item: ITelemetry) : unit =
            if shouldSend item then
                next.Process item

[<AbstractClass>]
type AppInsightsOperationTrackerBase (telemetryClient: TelemetryClient) =
    abstract RootParentActivityId: partition: GrainPartition -> Option<string>
    abstract OnActivityStarted: partition: GrainPartition -> activityId: string -> telemetry: DependencyTelemetry -> unit

    interface OperationTracker with
        member this.TrackOperation (input: TrackedOperationInput) (run: unit -> Task<TrackedOperationResult<'T>>) : Task<'T> =

            task { // must be context-sensitive task because invoked from Orleans grain call filter
                use activity =
                    match input.MaybeParentActivityId with
                    | None ->
                        System.Diagnostics.Activity.Current <- null
                        match this.RootParentActivityId input.Partition with
                        | None -> new System.Diagnostics.Activity(input.Name)
                        | Some testRootActivityParentId ->
                            (new System.Diagnostics.Activity(input.Name)).SetParentId(testRootActivityParentId)

                    | Some parentActivityId ->
                        System.Diagnostics.Activity.Current <- null
                        (new System.Diagnostics.Activity(input.Name)).SetParentId(parentActivityId)

                let activity = activity.Start()

                if input.MakeItNewParentActivityId then activity.Id else activity.ParentId
                |> fun nextParentId ->
                    // Can we not use Orleans inside AppInsights module?
                    Orleans.Runtime.RequestContext.Set (LibLifeCycleCore.OrleansEx.TraceContextGrainCallFilter.ParentActivityIdKey, nextParentId)

                try
                    // BEWARE !!! under no circumstances use other overloads of StartOperation except one that accepts Activity.
                    // Other overloads full of nasty surprises such as quietly overwriting your explicitly set operationId (!!!)
                    // It costed me many hours of debugging. Activity however gives full control over hierarchy while still
                    // getting useful stuff like Cloud Role Name etc. out of the box.
                    use op = telemetryClient.StartOperation<DependencyTelemetry>(activity)
                    try
                        try
                            op.Telemetry.Type <- input.Type.IconString
                            input.BeforeRunProperties |> Seq.iter op.Telemetry.Properties.Add

                            this.OnActivityStarted input.Partition activity.Id op.Telemetry

                            let! result = run ()
                            result.AfterRunProperties |> Seq.iter op.Telemetry.Properties.Add
                            op.Telemetry.Success <- result.IsSuccess |> Option.toNullable
                            return result.ReturnValue
                        with
                        | ex ->
                            op.Telemetry.Success <- false
                            // reraise() can't be used inside CEs so do this to keep stack trace
                            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw()
                            return shouldNotReachHereBecause "line above throws"
                    finally
                        // the order of stops is important, first stop telemetry client then outer activity
                        telemetryClient.StopOperation op
                finally
                    activity.Stop()
            }

        member this.SendMetric name value = telemetryClient.TrackMetric (name, value)

        member this.Shutdown () =
            backgroundTask {
                // 100 sec is plenty but may still fail with transient network error
                // for more reliable telemetry flush on shutdown look into ServerTelemetryChannel
                use cts = new CancellationTokenSource(TimeSpan.FromSeconds 100.)
                let! result = telemetryClient.FlushAsync(cts.Token)
                ignore result
            }

type AppInsightsOperationTracker (telemetryClient: TelemetryClient) =
    inherit AppInsightsOperationTrackerBase(telemetryClient)
    override _.RootParentActivityId _ = None
    override _.OnActivityStarted _ _ _ = ()

type private IpChainType =
// Gets the "processed" IP chain - i.e. it lists only untrusted IPs
| ProcessedIpChain
// Gets the "raw" IP chain - i.e. it lists all IPs, including our trusted proxies
| RawIpChain

let private getIpChain
    (request: HttpRequest)
    (chainType: IpChainType)
    : seq<IPAddress> =

    let xffHeaderName, appendRemoteIp =
        match chainType with
        | ProcessedIpChain -> "X-Forwarded-For", true
        | RawIpChain -> "X-Egg-Original-IP-Chain", false

    request.Headers
        .GetCommaSeparatedValues(xffHeaderName)
    |> Seq.filter (String.IsNullOrWhiteSpace >> not)
    |> Seq.choose (fun ipStr ->
        match IPAddress.TryParse ipStr with
        | true, ip -> Some ip
        | false, _ -> None)
    |> fun left ->
        if appendRemoteIp then
            Seq.append left [request.HttpContext.Connection.RemoteIpAddress]
        else
            left
    |> Seq.map (fun ip -> if ip.IsIPv4MappedToIPv6 then ip.MapToIPv4() else ip)

let getFirstUntrustedIp (request: HttpRequest) : Option<IPAddress> =
    getIpChain request ProcessedIpChain |> Seq.tryHead

let addTelemetryEx<'OperationTracker when 'OperationTracker :> AppInsightsOperationTrackerBase and 'OperationTracker : not struct>
    (instrumentationKey: string) (roleName: string) (serviceCollection: IServiceCollection) (developerMode: bool) (isWorkerService: bool) : unit =
    serviceCollection
        .AddSingleton<ITelemetryInitializer>(
            fun serviceProvider ->
                let maybeHttpContextAccessor =
                    serviceProvider.GetService<IHttpContextAccessor>()
                    |> Option.ofObj
                let maybeRoleInstance =
                   Environment.GetEnvironmentVariable("K8S_NODE_NAME")
                   |> fun nodeName ->
                       if String.IsNullOrWhiteSpace nodeName then None else Some nodeName
                let maybeInstanceId =
                   Environment.GetEnvironmentVariable("K8S_POD_NAME")
                   |> fun podName ->
                       if String.IsNullOrWhiteSpace podName then None else Some podName

                { new ITelemetryInitializer with
                    member __.Initialize (telemetry: ITelemetry) : unit =
                        telemetry.Context.Cloud.RoleName <- roleName
                        match maybeRoleInstance with
                        | None -> ()
                        | Some roleInstance ->
                            telemetry.Context.Cloud.RoleInstance <- roleInstance
                        match maybeInstanceId with
                        | None -> ()
                        | Some instanceId ->
                            match telemetry with
                            | :? ISupportProperties as props ->
                                props.Properties["InstanceId"] <- instanceId
                            | _ -> ()

                        match telemetry with
                        | :? RequestTelemetry as requestTelemetry ->

                            let maybeHttpContext =
                                maybeHttpContextAccessor
                                |> Option.bind (fun httpContextAccessor -> httpContextAccessor.HttpContext |> Option.ofObj)

                            match maybeHttpContext with
                            | Some httpContext ->
                                let getMaybeStringValue (v: StringValues) =
                                    match v.Count with
                                    | 1 -> Some v[0]
                                    | _ -> None

                                let maybeHeaderVersion = getMaybeStringValue httpContext.Request.Headers["X-AppVersion"]
                                let maybeQueryVersion = getMaybeStringValue httpContext.Request.Query["AppVersion"]

                                match maybeHeaderVersion, maybeQueryVersion with
                                | Some headerVersion, _ -> Some headerVersion
                                | None, Some queryVersion -> Some queryVersion
                                | None, None -> None
                                |> Option.iter (fun appVersion -> telemetry.Context.Component.Version <- appVersion)

                                [
                                    "ProcessedIpChain", ProcessedIpChain
                                    "RawIpChain", RawIpChain
                                ]
                                |> Seq.iter (fun (label, ipChainType) ->
                                    if not (requestTelemetry.Properties.ContainsKey label) then
                                        let csvIps =
                                            getIpChain httpContext.Request ipChainType
                                            |> fun ips -> String.Join(", ", ips)
                                        if not (String.IsNullOrWhiteSpace csvIps) then
                                            requestTelemetry.Properties.Add (label, csvIps))

                                match httpContext.Items.TryGetValue "AppInsights_UserId" with
                                | true, id ->
                                    let userId = id :?> string
                                    telemetry.Context.User.Id <- userId
                                    telemetry.Context.User.AccountId <- userId
                                | false, _ -> ()

                                match httpContext.Items.TryGetValue "AppInsights_SessionId" with
                                | true, id -> telemetry.Context.Session.Id <- id :?> string
                                | false, _ -> ()

                                match httpContext.Items.TryGetValue "RateLimitExceededForKeys" with
                                | true, keys ->
                                    requestTelemetry.Properties.Add ("RateLimitExceeded", "true")
                                    requestTelemetry.Properties.Add ("RateLimitExceededForKeys", keys :?> string)
                                | false, _ -> ()

                                if httpContext.Items.ContainsKey "SkipTelemetryOnSuccess" then
                                    requestTelemetry.Properties.Add ("SkipTelemetryOnSuccess", "true")

                            | None ->
                                ()

                        | _ -> // non-request telemetry e.g. dependency telemetry
                            let userId, maybeSessionId =
                                OrleansRequestContext.getTelemetryUserIdAndSessionId ()

                            if telemetry.Context.User.Id |> String.IsNullOrEmpty then
                                telemetry.Context.User.Id <- userId
                            if telemetry.Context.User.AccountId |> String.IsNullOrEmpty then
                                telemetry.Context.User.AccountId <- userId

                            maybeSessionId
                            |> Option.iter (fun sessionId ->
                                // set session Id unless it's inherited from upstream telemetry e.g. HTTP request
                                if telemetry.Context.Session.Id |> String.IsNullOrEmpty then
                                    telemetry.Context.Session.Id <- sessionId)
                }
        )
        .AddSingleton<OperationTracker, 'OperationTracker>()
    |> fun serviceCollection ->
        if not isWorkerService then
            serviceCollection
                .AddApplicationInsightsTelemetry (
                    AspNetCore.Extensions.ApplicationInsightsServiceOptions(
                        // Developer mode sends telemetry without buffering, disabled by default.
                        // Don't enable unless debugging App Insights, because it impacts performance
                        DeveloperMode          = developerMode,
                        EnableAdaptiveSampling = false,
                        InstrumentationKey     = instrumentationKey
                    )
                ) |> ignore
            serviceCollection
                .ConfigureTelemetryModule<DependencyTrackingTelemetryModule>(
                    // specify type explicitly to disambiguate WorkerService and AspNetCore versions
                    fun (mdl: DependencyTrackingTelemetryModule) (_options: AspNetCore.Extensions.ApplicationInsightsServiceOptions) ->
                        // log the SQL statements in AppInsights
                        // TODO: try also mdl.EnableRequestIdHeaderInjectionInW3CMode <- true
                        mdl.EnableSqlCommandTextInstrumentation <- true)
        else
            serviceCollection
                .AddApplicationInsightsTelemetryWorkerService(
                    WorkerService.ApplicationInsightsServiceOptions (
                        // Developer mode sends telemetry without buffering, disabled by default.
                        // Don't enable unless debugging App Insights, because it impacts performance
                        DeveloperMode          = developerMode,
                        EnableAdaptiveSampling = false,
                        InstrumentationKey     = instrumentationKey
                    )
                )
                .ConfigureTelemetryModule<DependencyTrackingTelemetryModule>(
                    // specify type explicitly to disambiguate WorkerService and AspNetCore versions
                    fun (mdl: DependencyTrackingTelemetryModule) (_options: WorkerService.ApplicationInsightsServiceOptions) ->
                        // log the SQL statements in AppInsights
                        // TODO: try also mdl.EnableRequestIdHeaderInjectionInW3CMode <- true
                        mdl.EnableSqlCommandTextInstrumentation <- true)
    |> fun serviceCollection ->
        // both AspNetCore and WorkerService have same extension methods in the same namespace and same parameter types
        // F# doesn't have extern aliases like C# . I don't want to add wrapper projects to disambiguate it, so using reflection hack
        // TODO: report bug to AppInsights team to move them to different namespaces
        let assembly =
            if isWorkerService then
                typeof<WorkerService.ApplicationInsightsServiceOptions>.Assembly
            else
                typeof<AspNetCore.Extensions.ApplicationInsightsServiceOptions>.Assembly
        let extensionsType = assembly.GetType(typeof<ApplicationInsightsExtensions>.FullName)

        // .AddApplicationInsightsTelemetryProcessor<SlowSqlDependencies>()
        // .AddApplicationInsightsTelemetryProcessor<RequestTelemetryProcessor>()
        // .AddApplicationInsightsTelemetryProcessor<SideEffectTelemetryProcessor>()
        extensionsType.GetMethods()
        |> Seq.filter (fun m -> m.Name = nameof(ApplicationInsightsExtensions.AddApplicationInsightsTelemetryProcessor) && m.IsGenericMethod)
        |> Seq.head
        |> fun genericMethod ->
            genericMethod.MakeGenericMethod([| typeof<SlowSqlDependencies> |]).Invoke(null, [| serviceCollection |]) |> ignore
            genericMethod.MakeGenericMethod([| typeof<RequestTelemetryProcessor> |]).Invoke(null, [| serviceCollection |]) |> ignore
            genericMethod.MakeGenericMethod([| typeof<DependencyTelemetryProcessor> |]).Invoke(null, [| serviceCollection |]) |> ignore

let addTelemetry (instrumentationKey: string) (roleName: string) (serviceCollection: IServiceCollection) (developerMode: bool) (isWorkerService: bool) : unit =
    addTelemetryEx<AppInsightsOperationTracker> instrumentationKey roleName serviceCollection developerMode isWorkerService

open LibLifeCycle.Config

[<CLIMutable>]
type AppInsightsConfiguration = {
    InstrumentationKey: string
}
with
    interface IValidatable with
        member this.Validate(): unit =
            if String.IsNullOrWhiteSpace(this.InstrumentationKey) then
                ConfigurationValidationException("InstrumentationKey not specified", this.InstrumentationKey) |> raise
