module internal LibLifeCycleHost.Web.Http

open System
open System.IO
open System.IO.Compression
open System.Net
open System.Text
open System.Threading.Tasks
open LibLifeCycleHost.Web.Config
open Microsoft.AspNetCore.Http
open Orleans
open Giraffe
open Microsoft.Extensions.DependencyInjection
open Microsoft.Net.Http.Headers
open LibLangFSharp
open LibLifeCycleHost.Web.WebApiJsonEncoding
open LibLifeCycleHost.Web
open LibLifeCycleTypes.File
open LibLifeCycleHost.AccessControl
open LibLifeCycle
open LibLifeCycleCore
open LibLifeCycleHost
open LibLifeCycle.LifeCycles.RequestRateCounter

let MaxDeflatedBodySize = 50 * 1024 * 1024 // 50MB

let respondJson (encoder: Encoder<'T>) (t: 'T) (httpContext: HttpContext) =
    backgroundTask {
        let isDevHost =
            (httpContext.RequestServices.GetService typeof<HttpHandlerSettings> :?> HttpHandlerSettings)
            |> Option.ofObj
            |> Option.map(fun s -> s.IsDevHost)
            |> Option.defaultValue false

        if not isDevHost then // disable in dev, breaks native dev
            httpContext.SetHttpHeader ("Cache-control", "no-store")
            httpContext.SetHttpHeader ("Strict-Transport-Security", "max-age=63072000; includeSubDomains; preload")

        httpContext.Response.ContentType <- "application/json"
        do! encoder t
            |> Encode.toStream httpContext.Response.Body
        return Some httpContext
    }

let respondFile (httpContext: HttpContext) (file: File) (maybeEncoding: Option<Encoding>) (maybeFileName: Option<string>) =
    let mediaType = MediaTypeHeaderValue(file.MimeType.Value |> Microsoft.Extensions.Primitives.StringSegment.op_Implicit)
    maybeEncoding |> Option.iter (fun encoding -> mediaType.Encoding <- encoding)

    maybeFileName
    |> Option.bind (fun name -> if String.IsNullOrWhiteSpace name then None else Some name)
    |> Option.iter (fun fileName ->
        httpContext.SetHttpHeader
            (HeaderNames.ContentDisposition,
            (sprintf "attachment; filename=\"%s\""
                 (fileName.Replace("\"", "_")))))

    httpContext.Response.ContentType <- mediaType.ToString()
    httpContext.WriteBytesAsync file.Bytes

let tryGetProjectionName (httpContext: HttpContext) =
    httpContext.TryGetQueryStringValue "projection"
    |> Option.bind (fun s -> if String.IsNullOrWhiteSpace s then None else Some s)

let tryGetHeaderValue (httpContext: HttpContext) (key: string) : Option<string> =
    httpContext.Request.Headers[key]
    |> fun sv ->
        let s : string = Microsoft.Extensions.Primitives.StringValues.op_Implicit sv
        s
    |> fun s -> if String.IsNullOrWhiteSpace s then None else Some s

let readJsonBody (httpContext: HttpContext) (decoder: Decoder<'T>) =
    backgroundTask {
        let maybeEncoding = tryGetHeaderValue httpContext "X-Content-Encoding"

        use resBodyStream =
            match maybeEncoding with
            | None ->
                httpContext.Request.Body
                |> Ok
                |> DisposableResult
            | Some "gzip" ->
                new GZipStream(httpContext.Request.Body, CompressionMode.Decompress, true)
                :> Stream
                |> Ok
                |> DisposableResult
            | Some "deflate" ->
                new DeflateStream(httpContext.Request.Body, CompressionMode.Decompress, true)
                :> Stream
                |> Ok
                |> DisposableResult
            | Some "br" ->
                new BrotliStream(httpContext.Request.Body, CompressionMode.Decompress, true)
                :> Stream
                |> Ok
                |> DisposableResult
            | Some otherEncoding ->
                Error (sprintf "Unsupported Request Encoding %s" otherEncoding)
                |> DisposableResult

        try
            match resBodyStream.Result with
            | Error err ->
                return Error err
            | Ok stream ->
                // Prevent Zip Bombs (see MaxLengthReadStream.fs for more info)
                use maxLengthStream = new MaxLengthReadStream(stream, MaxDeflatedBodySize)
                return! Decode.fromStream decoder maxLengthStream
        with
        | MaxLengthReadStreamException maxBytes ->
            return Error (sprintf "Max (deflated) Upload Size of %d bytes exceeded" maxBytes)
        | ex ->
            return Error (sprintf "Exception %s while reading request body: %s" (ex.GetType().FullName) ex.Message)
    }

let readJsonBodyAndValidateWithCodec
    (httpContext: HttpContext)
    (decoder: Decoder<'T>)
    (toCodecFriendly: 'T -> 'CodecFriendly)
    (codec: CodecLib.Codec<CodecLib.StjCodecs.StjEncoding, 'CodecFriendly>) =
    backgroundTask {
        match! readJsonBody httpContext decoder with
        | Ok value ->
            // To validate incoming JSON requests we piggyback on codecs that do good job at traversing the value statically
            // Unfortunately have to embed validation assertions right into the get_Codec() functions / decoders
            // (for example see types with private constructors like Email, PhoneNumber, NonemptyString etc.)
            // TODO: perhaps validation can be modelled as a separate Fleece Encoding somehow rather than be part of a codec body??
            // TODO: how to generically validate view inputs that have no codecs?

            // Can we validate after applying rate limits? but rate limit threshold can be resolved only by a valid request

            CodecLib.StjCodecs.Extensions.toJsonTextCheckedWithCodec codec (toCodecFriendly value) |> ignore
            return (Ok value)
        | Error err ->
            return Error err
    }


let inline readJsonBodyAndValidate (httpContext: HttpContext) (decoder: Decoder<'T>) (toCodecFriendly: 'T -> 'CodecFriendly) =
    readJsonBodyAndValidateWithCodec httpContext decoder toCodecFriendly CodecLib.defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'CodecFriendly>

let makeHostEcosystemGrainConnector (hostEcosystemGrainFactory: IGrainFactory) (grainPartition: GrainPartition) (ecosystem: Ecosystem) (httpContext: HttpContext) =
    let serviceProvider = httpContext.RequestServices
    let cryptographer = serviceProvider.GetRequiredService<ApiSessionCryptographer>()
    let sessionHandle = Session.Http.getSessionHandle cryptographer ecosystem httpContext
    let callOrigin = Session.createCallOriginFromHttpContext httpContext
    GrainConnector(hostEcosystemGrainFactory, grainPartition, sessionHandle, callOrigin)

let authorizeSubjectAndContinueWith
        (maybeApiAccess: Option<LifeCycleApiAccess<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'AccessPredicateInput, 'Session, 'Role>>)
        (maybeSessionHandling: Option<EcosystemSessionHandling<'Session, 'Role>>)
        (sessionSource: SessionSource)
        (httpContext: HttpContext)
        (accessEvent: AccessEvent<_, _, _, _>)
        (statusCodeOnAuthFail: HttpStatusCode)
        (authorizedContinuation: unit -> Task<Option<HttpContext>>)
        (maybeAuditor: Option<Auditor>): Task<Option<HttpContext>> =
    task {
        let serviceProvider = httpContext.RequestServices
        let grainPartition = serviceProvider.GetRequiredService<GrainPartition>()
        let hostEcosystemGrainFactory = serviceProvider.GetRequiredService<IGrainFactory>()
        let callOrigin = Session.createCallOriginFromHttpContext httpContext
        return! Subjects.authorizeSubjectAndContinueWith
            hostEcosystemGrainFactory
            grainPartition
            maybeApiAccess
            maybeSessionHandling
            sessionSource
            callOrigin
            accessEvent
            (fun () ->
                task {
                    httpContext.SetStatusCode (int statusCodeOnAuthFail)
                    return! earlyReturn httpContext
                }
            )
            authorizedContinuation
            maybeAuditor
    }

let authorizeTimeSeriesAndContinueWith
        (maybeApiAccess: Option<TimeSeriesApiAccess<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'AccessPredicateInput, 'Session, 'Role>>)
        (maybeSessionHandling: Option<EcosystemSessionHandling<'Session, 'Role>>)
        (sessionSource: SessionSource)
        (httpContext: HttpContext)
        (accessEvent: TimeSeriesAccessEvent<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure>)
        (statusCodeOnAuthFail: HttpStatusCode)
        (authorizedContinuation: unit -> Task<Option<HttpContext>>): Task<Option<HttpContext>> =
    task {
        let serviceProvider = httpContext.RequestServices
        let grainPartition = serviceProvider.GetRequiredService<GrainPartition>()
        let hostEcosystemGrainFactory = serviceProvider.GetRequiredService<IGrainFactory>()
        let callOrigin = Session.createCallOriginFromHttpContext httpContext
        return! TimeSeries.authorizeTimeSeriesAndContinueWith
            hostEcosystemGrainFactory
            grainPartition
            maybeApiAccess
            maybeSessionHandling
            sessionSource
            callOrigin
            accessEvent
            (fun () ->
                task {
                    httpContext.SetStatusCode (int statusCodeOnAuthFail)
                    return! earlyReturn httpContext
                }
            )
            authorizedContinuation
    }

let private hardCodedDefaultActRateLimits : list<RateLimit> =
    [
        { Key = RateLimitKey.Scoped (Set.ofList [RateLimitScope.Value "1m"; RateLimitScope.UserSessionOrIp])
          Limit = PositiveInteger.ofIntUnsafe 10
          Duration = TimeSpan.FromMinutes 1. }

        { Key = RateLimitKey.Scoped (Set.ofList [RateLimitScope.Value "1h"; RateLimitScope.UserSessionOrIp])
          Limit = PositiveInteger.ofIntUnsafe 100
          Duration = TimeSpan.FromHours 1. }
    ]



let private hardCodedDefaultConstructRateLimits : list<RateLimit> =
    [
        { Key = RateLimitKey.Scoped (Set.ofOneItem RateLimitScope.UserSessionOrIp)
          Limit = PositiveInteger.ofIntUnsafe 5
          Duration = TimeSpan.FromMinutes 1. }

        { Key = RateLimitKey.Scoped (Set.ofOneItem RateLimitScope.UserSessionOrIp)
          Limit = PositiveInteger.ofIntUnsafe 50
          Duration = TimeSpan.FromHours 1. }
    ]

// let private hardCodedDefaultReadRateLimits : list<RateLimit> =
//     Read _ -> [
//         Scoped.To(UserSessionOrIp); Limit = 1000; Duration = TimeSpan.FromMinute 1.
//     ]

let private rateLimitData
    (prefix: string)
    (rateLimitScopes: Set<RateLimitScope>)
    (duration: TimeSpan)
    (limit: PositiveInteger)
    (delta: PositiveInteger)
    (httpContext: HttpContext)
    : {| CounterId: RequestRateCounterId; Duration: TimeSpan; Limit: PositiveInteger; Delta: PositiveInteger |} =
    rateLimitScopes
    |> Seq.map (fun rateLimitScope ->
        match rateLimitScope with
        | RateLimitScope.Value value ->
            value
        | RateLimitScope.UserIp ->
            ApplicationInsights.getFirstUntrustedIp httpContext.Request
            |> Option.map string
            |> Option.defaultValue "no_IP"
        | RateLimitScope.UserSessionOrIp ->
            let hasSessionId, sessionIdObj = httpContext.Items.TryGetValue "AppInsights_SessionId"
            match hasSessionId, (sessionIdObj :?> string) with
            | false, _
            | true, ("<anon>" | "") ->
                ApplicationInsights.getFirstUntrustedIp httpContext.Request
                |> Option.map string
                |> Option.defaultValue "no_Session_or_IP"
            | true, sessionId ->
                $"_sess_{sessionId}")
    |> Seq.sort // for stable ordering
    |> fun chunks -> String.Join ('/', Seq.append (Seq.singleton prefix) chunks)
    |> RequestRateCounterId
    |> fun counterId -> {| CounterId = counterId; Duration = duration; Limit = limit; Delta = delta |}


let private withinRateLimitsImpl
    (hostEcosystemGrainFactory: IGrainFactory)
    (grainPartition: GrainPartition)
    (prioritizedGroups: list<list<{| CounterId: RequestRateCounterId; Duration: TimeSpan; Limit: PositiveInteger; Delta: PositiveInteger |}>>)
    (httpContext: HttpContext)
    (enforceRateLimits: bool)
    : Task<bool> =
    task {
        let mutable stillWithinLimits = true
        let (GrainPartition partitionGuid) = grainPartition
        for rateCounterIds in prioritizedGroups do
            if stillWithinLimits then
                let withinLimitsTask =
                    rateCounterIds
                    |> List.map (fun x ->
                        task {
                            try
                                let rateCounterSubjectKey = (x.CounterId :> SubjectId).IdString
                                let grain = hostEcosystemGrainFactory.GetGrain<ISubjectGrain<RequestRateCounter, RequestRateCounterAction, RequestRateCounterOpError, RequestRateCounterConstructor, RequestRateCounterLifeEvent, RequestRateCounterId>>(partitionGuid, rateCounterSubjectKey)
                                match!
                                    grain.ActMaybeConstructInterGrain
                                        None
                                        (* includeResponse *) false
                                        (RequestRateCounterAction.Increment x.Delta)
                                        (RequestRateCounterConstructor.New (x.CounterId, x.Duration, x.Limit))
                                        None
                                        with
                                | Ok _ ->
                                    return None
                                | Error (SubjectFailure.Err (GrainOperationError.TransitionError RequestRateCounterOpError.LimitExceeded)) ->
                                    return (Some rateCounterSubjectKey)
                                | Error _ ->
                                    // should not really reach here
                                    return (Some rateCounterSubjectKey)
                            with
                            | _ ->
                                // If the counter lookup fails, or times out, assume rate limit has not been reached and allow the request to proceed.
                                return None
                        })
                    |> Task.WhenAll

                // there's no per call timeouts in Orleans yet, use them if they are available one day: https://github.com/dotnet/orleans/issues/8591
                let! finishedTask = Task.WhenAny [Task.Delay (TimeSpan.FromSeconds 2.); withinLimitsTask]
                if finishedTask = withinLimitsTask then
                    let limitExceededForKeys = Array.choose id withinLimitsTask.Result
                    if limitExceededForKeys.Length > 0 then
                        httpContext.Items.Add ("RateLimitExceededForKeys", String.Join ("\n", limitExceededForKeys))
                        stillWithinLimits <- false

        return
            if enforceRateLimits
            then stillWithinLimits
            else true
    }
let withinRateLimits
    (hostEcosystemGrainFactory: IGrainFactory)
    (grainPartition: GrainPartition)
    (lifeCycleAdapter: IHostedOrReferencedLifeCycleAdapter<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>)
    (httpContext: HttpContext)
    (rateLimitEvent: RateLimitEvent<'LifeAction, 'Constructor>)
    (enforceRateLimits: bool)
    : Task<bool> =
        let prioritizedGroups =
            lifeCycleAdapter.ReferencedLifeCycle.RateLimitsPredicate rateLimitEvent
            |> Option.defaultWith (fun () ->
                match rateLimitEvent with
                | RateLimitEvent.Act _ -> hardCodedDefaultActRateLimits
                | RateLimitEvent.Construct _ -> hardCodedDefaultConstructRateLimits)
            // group & sort by duration, and parallelize within one group
            // e.g. if 1 minute limit exhausted then don't increment 1h limit
            |> List.groupBy (fun rateLimit -> rateLimit.Duration)
            |> List.sortBy fst
            |> List.map (fun (_, rateLimits) ->
                rateLimits
                |> List.map (fun rateLimit ->
                    match rateLimit.Key with
                    | RateLimitKey.Scoped scopes ->
                        let caseName =
                            match rateLimitEvent with
                            | RateLimitEvent.Act action -> unionCaseName action
                            | RateLimitEvent.Construct ctor -> unionCaseName ctor

                        $"{lifeCycleAdapter.ReferencedLifeCycle.Name}/{caseName}",
                        scopes
                    | RateLimitKey.Global scopes ->
                        "",
                        scopes
                    |> fun (prefix, rateLimitScopes) ->
                        rateLimitData prefix rateLimitScopes rateLimit.Duration rateLimit.Limit PositiveInteger.One httpContext))

        withinRateLimitsImpl hostEcosystemGrainFactory grainPartition prioritizedGroups httpContext enforceRateLimits

let ingestWithinRateLimits
    (hostEcosystemGrainFactory: IGrainFactory)
    (grainPartition: GrainPartition)
    (timeSeries: ITimeSeries<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex>)
    (httpContext: HttpContext)
    (points: list<'TimeSeriesDataPoint>)
    (enforceRateLimits: bool)
    : Task<bool> =
    let prefix = $"TimeSeries/{timeSeries.Name}"
    let delta = if points.IsEmpty then PositiveInteger.One else (PositiveInteger.ofIntUnsafe points.Length)
    let prioritizedGroups =
        [
            [rateLimitData prefix (Set.ofOneItem RateLimitScope.UserSessionOrIp) (TimeSpan.FromMinutes 1.) (PositiveInteger.ofIntUnsafe 200)    delta httpContext ]
            [rateLimitData prefix (Set.ofOneItem RateLimitScope.UserSessionOrIp) (TimeSpan.FromHours 1.)   (PositiveInteger.ofIntUnsafe 10_000) delta httpContext ]
        ]
    withinRateLimitsImpl hostEcosystemGrainFactory grainPartition prioritizedGroups httpContext enforceRateLimits

let markRequestToBeSkippedInTelemetry (httpContext: HttpContext) : unit =
    httpContext.Items.Add ("SkipTelemetryOnSuccess", "true")

let applyActTelemetryRules
    (lifeCycleAdapter: IHostedOrReferencedLifeCycleAdapter<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>)
    (httpContext: HttpContext)
    (action: 'LifeAction) =
    if not (lifeCycleAdapter.ShouldSendTelemetry (ShouldSendTelemetryFor.LifeAction action)) then
        markRequestToBeSkippedInTelemetry httpContext

let applyConstructTelemetryRules
    (lifeCycleAdapter: IHostedOrReferencedLifeCycleAdapter<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>)
    (httpContext: HttpContext)
    (ctor: 'Constructor) =
    if not (lifeCycleAdapter.ShouldSendTelemetry (ShouldSendTelemetryFor.Constructor ctor)) then
        markRequestToBeSkippedInTelemetry httpContext
