module AppPerformancePlayground.Services.Session.SessionService

open System
open LibClient.EventBus
open LibUiSubject.Services.RealTimeService
open LibClient.Services.HttpService.ThothEncodedHttpService
open LibClient.Services.LocalStorageService
open LibClient
open LibUiSubject.Services.SubjectService

type ConstructionThrottledSession = LibUiAuth.Services.SessionService.ConstructionThrottledSession<SampleSession, SessionId>

type SessionService(backendUrl: string, maybeCookieDomain: Option<NonemptyString>, realTimeService: RealTimeService, thothEncodedHttpService: ThothEncodedHttpService, localStorageService: LocalStorageService, eventBus: EventBus) =
    inherit LibUiAuth.Services.SessionService.SessionService<SampleSession, SessionId, SampleSessionIndex, SampleSessionNumericIndex, SampleSessionStringIndex, SampleSessionSearchIndex, SampleSessionConstructor, SampleSessionAction, SampleSessionLifeEvent, SampleSessionOpError>(
        SubjectEndpoints.Make<SampleSession, SampleSession, SessionId, SampleSessionIndex, SampleSessionNumericIndex, SampleSessionStringIndex, SampleSessionSearchIndex, SampleSessionConstructor, SampleSessionAction, SampleSessionLifeEvent, SampleSessionOpError>("Session", None, backendUrl + "/subject"),
        maybeCookieDomain,
        realTimeService,
        thothEncodedHttpService,
        localStorageService,
        eventBus,
        SessionId.SessionId,
        {|
            Encoder = Json.ToString
            Decoder = Json.FromString
        |},
        SampleSessionConstructor.NewFromId,
        (fun currentSessionId -> {
            Id        = currentSessionId
            CreatedOn = DateTimeOffset.Now
            State     = SessionState.Unauthenticated
        }),
        (fun session ->
            match session.State with
            | SessionState.Authenticated authenticated ->
                TelemetryUser.Identified (authenticated.Who.UserIdStringForTelemetry, Map.empty)
            | _ -> TelemetryUser.Anonymous
        )
    )

    // Delete this hardcoded session, and the service will start hitting
    // the subject backend, and you'll get standard Subject authentication
    override this.GetOne (_useCache: LibUiSubject.Types.UseCache) (id: SampleSessionId) : Async<AsyncData<ConstructionThrottledSession>> =
        let who = {
            Name                     = NonemptyString.ofLiteral "Kudo Chika"
            PrefersBananas           = false
            UserIdStringForTelemetry = "Kudo Chika"
        }

        let authenticated = {
            RevalidatedOn        = DateTimeOffset.Now
            RevalidationCookies  = []
            Name                 = who.Name.Value
            Who                  = who
            State                = AuthenticatedState.Validated
        }

        {
            Id        = id
            CreatedOn = DateTimeOffset.Now
            State     = SessionState.Authenticated authenticated
        }
        |> ConstructionThrottledSession.Initialized
        |> AsyncData.Available
        |> Async.Of
