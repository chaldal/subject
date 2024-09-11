module LibLifeCycleHost.Web.HttpHandler

open System
open System.Reflection
open System.Threading.Tasks
open System.Runtime.CompilerServices
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Builder
open Giraffe
open LibLifeCycle
open LibLifeCycleHost
open LibLifeCycleHost.Web.Config
open LibLifeCycleHost.Web.LegacyGenericHttpHandler
open Orleans
open LibLifeCycleCore

// Browsers are going towards auto-deleting client-set cookies after 7-days
// This bit of code will copy all cookies starting with "P_" into
// equivalent server-sent cookies starting with "PS_"
// By default, expiry is set to 2 years; however if Cookie is of the format
// "....~EXP~<UNIX_TIMESTAMP>", such that UNIX_TIMESTAMP is a future value, then the
// server-sent cookies will have that expiry set
let private syncCookies (httpContext: HttpContext) : unit =
    let allCookies =  httpContext.Request.Cookies

    allCookies
    |> Seq.where (fun cookie -> cookie.Key.StartsWith "P_")
    |> Seq.map (fun cookie -> cookie, (sprintf "PS%s" (cookie.Key.TrimStart 'P')))
    |> Seq.where (fun (cookie, psCookieName) ->
        (not (allCookies.ContainsKey psCookieName)) || allCookies.[psCookieName] <> cookie.Value)
    |> Seq.iter (fun (cookie, psCookieName) ->
        // Attempt to parse out expiry from the cookie
        let cookieValueParts = cookie.Value.Split("~EXP~", 2)
        let expiry =
            if cookieValueParts.Length = 2 then
                match Int64.TryParse cookieValueParts.[1] with
                | (true, value) ->
                    try
                        DateTimeOffset.FromUnixTimeSeconds value
                        |> ValueSome
                    with
                    | _ -> ValueNone
                | _ ->
                    ValueNone
            else
                ValueNone
            |> ValueOption.defaultWith (fun _ -> DateTimeOffset.UtcNow.AddYears(2))

        let httpConfig = httpContext.RequestServices.GetRequiredService<HttpCookieConfiguration>()

        // Only set the shadow cookie if original cookie's expiry is in the future
        if expiry > DateTimeOffset.UtcNow then
            let cookieOptions = CookieOptions()
            cookieOptions.Expires <- expiry
            cookieOptions.SameSite <- SameSiteMode.Lax

            // make sure PS_ cookie has the same domain as the respective P_ cookie that it shadows
            // it's important that HttpCookieConfiguration matches corresponding app settings
            httpConfig.GetAppCookieDomainForHostName httpContext.Request.Host.Value
            |> fun cookieDomain ->
                if not (String.IsNullOrWhiteSpace cookieDomain) then
                    cookieOptions.Domain <- cookieDomain

            httpContext.Response.Cookies.Append(psCookieName, cookie.Value, cookieOptions)
    )

let private getLegacySubjectHttpHandler
        (hostEcosystemGrainFactory: IGrainFactory)
        (grainPartition: GrainPartition)
        (clock: Service<Clock>)
        (ecosystem: Ecosystem)
        (cryptographer: ApiSessionCryptographer)
        (lifeCycleAdapter: IHostedLifeCycleAdapter) : HttpHandler =
        lifeCycleAdapter.LifeCycle.Invoke
            { new FullyTypedLifeCycleFunction<_> with
                member _.Invoke (lifeCycle: LifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _, _>) =
                    getLegacyGenericSubjectHttpHandler
                        hostEcosystemGrainFactory grainPartition clock cryptographer ecosystem lifeCycle.Definition
                        (lifeCycleAdapter :?> HostedLifeCycleAdapter<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>) }

let private getV1SubjectHttpHandler
        (hostEcosystemGrainFactory: IGrainFactory)
        (biosphereGrainProvider: IBiosphereGrainProvider)
        (grainPartition: GrainPartition)
        (clock: Service<Clock>)
        (ecosystem: Ecosystem)
        (cryptographer: ApiSessionCryptographer)
        (lifeCycleAdapter: IHostedOrReferencedLifeCycleAdapter)
        : HttpHandler =
    lifeCycleAdapter.ReferencedLifeCycle.Invoke
            { new FullyTypedReferencedLifeCycleFunction<_> with
                member _.Invoke (referencedLifeCycle: ReferencedLifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, _, _, _>) =
                    // this seems dumb as we already have an adapter, but it is not generically typed. We'd have to introduce Invoke
                    // pattern at adapter level to avoid this.
                    let typedAdapter: HostedOrReferencedLifeCycleAdapter<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId> =
                        {
                            ReferencedLifeCycle = referencedLifeCycle
                        }

                    LibLifeCycleHost.Web.Api.V1.GenericHttpHandler.getV1GenericSubjectHttpHandler
                        hostEcosystemGrainFactory
                        biosphereGrainProvider
                        grainPartition
                        clock
                        cryptographer
                        ecosystem
                        referencedLifeCycle.Def
                        typedAdapter
              }

let private getLegacyViewHttpHandler
        (ecosystem: Ecosystem)
        (cryptographer: ApiSessionCryptographer)
        (viewAdapter: IViewAdapter) : HttpHandler =
    viewAdapter.View.Invoke
        { new FullyTypedViewFunction<_> with
            member _.Invoke (view: View<'Input, 'Output, 'OpError, _, _, _, _>) =
                getLegacyGenericViewHttpHandler
                    ecosystem
                    cryptographer
                    (viewAdapter :?> ViewAdapter<'Input, 'Output, 'OpError>)
        }

let private getV1ViewHttpHandler
        (hostEcosystemGrainFactory: IGrainFactory)
        (grainPartition: GrainPartition)
        (ecosystem: Ecosystem)
        (cryptographer: ApiSessionCryptographer)
        (viewAdapter: IViewAdapter) : HttpHandler =
    viewAdapter.View.Invoke
        { new FullyTypedViewFunction<_> with
            member _.Invoke (view: View<'Input, 'Output, 'OpError, _, _, _, _>) =
                LibLifeCycleHost.Web.Api.V1.GenericHttpHandler.getV1GenericViewHttpHandler
                    hostEcosystemGrainFactory
                    grainPartition
                    ecosystem
                    cryptographer
                    (viewAdapter :?> ViewAdapter<'Input, 'Output, 'OpError>)
        }

#nowarn "1240" // ignores This type test or downcast will ignore the unit-of-measure 'UnitOfMeasure in getV1TimeSeriesHttpHandler

let private getV1TimeSeriesHttpHandler
        (hostEcosystemGrainFactory: IGrainFactory)
        (grainPartition: GrainPartition)
        (clock: Service<Clock>)
        (ecosystem: Ecosystem)
        (cryptographer: ApiSessionCryptographer)
        (timeSeriesAdapter: ITimeSeriesAdapter) : HttpHandler =
    timeSeriesAdapter.TimeSeries.Invoke
        { new FullyTypedTimeSeriesFunction<_> with
            member _.Invoke (timeSeries: TimeSeries<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex, _, _, _>) =
                LibLifeCycleHost.Web.Api.V1.GenericHttpHandler.getV1GenericTimeSeriesHttpHandler
                    hostEcosystemGrainFactory
                    grainPartition
                    clock
                    ecosystem
                    cryptographer
                    (timeSeriesAdapter :?> TimeSeriesAdapter<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex>)
        }

open Microsoft.Extensions.Logging

type HttpExceptionHandlingMiddleware
        (
            next: RequestDelegate,
            loggerFactory: ILoggerFactory
        ) =

    member _.Invoke (httpContext : HttpContext) : Task =
        task {
            try
                return! next.Invoke httpContext
            with
            | ex ->
                let logger = loggerFactory.CreateLogger(nameof(HttpExceptionHandlingMiddleware))
                logger.LogError (ex, "Exception in HTTP request")
                httpContext.Response.Clear()
                httpContext.SetStatusCode(int System.Net.HttpStatusCode.InternalServerError)
                return ()
        }

type HttpEndpointsMiddleware
        (
            next: RequestDelegate,
            clock: Service<Clock>,
            cryptographer: ApiSessionCryptographer,
            ecosystem: Ecosystem,
            lifeCycleAdapterCollection: HostedLifeCycleAdapterCollection,
            viewAdapterCollection: ViewAdapterCollection,
            timeSeriesAdapterCollection: TimeSeriesAdapterCollection,
            // TODO: expose TimeSeries via api
            _timeSeriesAdapterCollection: TimeSeriesAdapterCollection,
            hostEcosystemGrainFactory: IGrainFactory,
            biosphereGrainProvider: IBiosphereGrainProvider
        ) =

    let noneTask = Task.FromResult None

    // The grain partition only differs in test scenarios, so we hard-code it here given that HTTP APIs are not used during testing.
    let grainPartition = defaultGrainPartition

    let referencedEcosystemRouter (referencedEcosystem: ReferencedEcosystem) =
        subRoute
            "/ecosystem"
            (
                subRoute
                    $"/{referencedEcosystem.Def.Name}"
                    (choose
                        [
                            subRoute
                                "/subject"
                                (
                                    referencedEcosystem.LifeCycles
                                    |> Seq.choose (fun referencedLifeCycle ->
                                        referencedLifeCycle.Invoke
                                            { new FullyTypedReferencedLifeCycleFunction<_> with
                                                member _.Invoke referencedLifeCycle =
                                                    match referencedLifeCycle.MaybeApiAccess with
                                                    | Some _ ->
                                                        { ReferencedLifeCycle = referencedLifeCycle }
                                                        |> (fun referencedLifeCycleAdapter -> getV1SubjectHttpHandler hostEcosystemGrainFactory biosphereGrainProvider grainPartition clock ecosystem cryptographer referencedLifeCycleAdapter)
                                                        |> Some
                                                    | None -> None
                                            }
                                    )
                                    |> Seq.toList
                                    |> choose
                                )

                            // TODO: add referenced view support
                        ]
                    )
            )

    let lazyRouter =
        lazy(
            task {
                let referencedEcosystemRouters =
                    ecosystem.ReferencedEcosystems
                    |> Map.values
                    |> Seq.map referencedEcosystemRouter
                    |> Seq.toList

                return
                    choose [
                        subRoute
                            "/api"
                            (choose
                                [
                                    subRoute
                                        "/v1"
                                        (choose
                                            [
                                                subRoute
                                                    "/ecosystem"
                                                    (
                                                        subRoute
                                                            $"/{ecosystem.Def.Name}"
                                                            (choose
                                                                [
                                                                    subRoute
                                                                        "/subject"
                                                                        (
                                                                            lifeCycleAdapterCollection
                                                                                |> Seq.filter (fun lifeCycleAdapter ->
                                                                                    lifeCycleAdapter.LifeCycle.Invoke
                                                                                        { new FullyTypedLifeCycleFunction<_> with
                                                                                            member _.Invoke lifeCycle =
                                                                                                lifeCycle.MaybeApiAccess
                                                                                                |> Option.map (fun _ -> true)
                                                                                                |> Option.defaultValue false
                                                                                        }
                                                                                )
                                                                            |> Seq.map (getV1SubjectHttpHandler hostEcosystemGrainFactory biosphereGrainProvider grainPartition clock ecosystem cryptographer)
                                                                            |> Seq.toList
                                                                            |> choose
                                                                        )

                                                                    subRoute
                                                                        "/view"
                                                                        (
                                                                            viewAdapterCollection
                                                                            |> Seq.filter (fun viewAdapter -> viewAdapter.EnableApiAccess)
                                                                            |> Seq.map (getV1ViewHttpHandler hostEcosystemGrainFactory grainPartition ecosystem cryptographer)
                                                                            |> Seq.toList
                                                                            |> choose
                                                                        )

                                                                    subRoute
                                                                        "/timeSeries"
                                                                        (
                                                                            timeSeriesAdapterCollection
                                                                            |> Seq.filter (fun timeSeriesAdapter -> timeSeriesAdapter.TimeSeries.EnableApiAccess)
                                                                            |> Seq.map (getV1TimeSeriesHttpHandler hostEcosystemGrainFactory grainPartition clock ecosystem cryptographer)
                                                                            |> Seq.toList
                                                                            |> choose
                                                                        )
                                                                ]
                                                            )
                                                    )

                                                // Technically not required anymore because we expose these under ecosystem name above.
                                                // Can probably remove these in the future.
                                                subRoute
                                                    "/subject"
                                                    (
                                                        lifeCycleAdapterCollection
                                                            |> Seq.filter (fun lifeCycleAdapter ->
                                                                lifeCycleAdapter.LifeCycle.Invoke
                                                                    { new FullyTypedLifeCycleFunction<_> with
                                                                        member _.Invoke lifeCycle =
                                                                            lifeCycle.MaybeApiAccess
                                                                            |> Option.map (fun _ -> true)
                                                                            |> Option.defaultValue false
                                                                    }
                                                            )
                                                        |> Seq.map (getV1SubjectHttpHandler hostEcosystemGrainFactory biosphereGrainProvider grainPartition clock ecosystem cryptographer)
                                                        |> Seq.toList
                                                        |> choose
                                                    )

                                                subRoute
                                                    "/view"
                                                    (
                                                        viewAdapterCollection
                                                        |> Seq.filter (fun viewAdapter -> viewAdapter.EnableApiAccess)
                                                        |> Seq.map (getV1ViewHttpHandler hostEcosystemGrainFactory grainPartition ecosystem cryptographer)
                                                        |> Seq.toList
                                                        |> choose
                                                    )

                                                subRoute
                                                    "/timeSeries"
                                                    (
                                                        timeSeriesAdapterCollection
                                                        |> Seq.filter (fun timeSeriesAdapter -> timeSeriesAdapter.TimeSeries.EnableApiAccess)
                                                        |> Seq.map (getV1TimeSeriesHttpHandler hostEcosystemGrainFactory grainPartition clock ecosystem cryptographer)
                                                        |> Seq.toList
                                                        |> choose
                                                    )

                                                yield! referencedEcosystemRouters
                                            ]
                                        )
                                ]
                            )

                        // Routes below this point are legacy
                        subRoute
                            "/subject"
                            (
                                lifeCycleAdapterCollection
                                |> Seq.map (getLegacySubjectHttpHandler hostEcosystemGrainFactory grainPartition clock ecosystem cryptographer)
                                |> Seq.toList
                                |> choose
                            )

                        subRoute
                            "/view"
                            (
                                viewAdapterCollection
                                |> Seq.map (getLegacyViewHttpHandler ecosystem cryptographer)
                                |> Seq.toList
                                |> choose
                            )
                        ]
            }
        )

    member _.Invoke(httpContext: HttpContext) : Task =
        task {
            syncCookies httpContext

            let! router = lazyRouter.Value

            match! router (fun _ -> noneTask) httpContext with
            | Some _ ->
                return! Task.CompletedTask
            | None ->
                return! next.Invoke(httpContext)
        }
        |> Task.Ignore

type [<Extension>] HttpMiddlewareExtensions() =
    [<Extension>]
    static member UseHttpEndpoints(builder: IApplicationBuilder, suppressExceptionDetails: bool) =
        if suppressExceptionDetails then
            builder.UseMiddleware<HttpExceptionHandlingMiddleware>()
        else
            builder
        |> fun builder ->
            builder.UseMiddleware<HttpEndpointsMiddleware>()
