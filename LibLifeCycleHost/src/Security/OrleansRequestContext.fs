namespace LibLifeCycleHost.Web

open Orleans.Runtime

module OrleansRequestContext =
    let private telemetryUserIdKey = "UserId"
    let private telemetrySessionIdKey = "SessionId"

    /// Sets current user Id and session Id for telemetry/audit. Not recommended to use for authorization.
    /// Calls originated within the system (e.g. inside a long-term timer invocation) should reset it to empty.
    let setTelemetryUserIdAndSessionId (userId: string, sessionId: Option<string>) = // public for LibLifeCycleTest. FIXME: allow test lib to see internals and change visibility
        RequestContext.Set(telemetryUserIdKey, userId)
        RequestContext.Set(telemetrySessionIdKey, sessionId |> Option.defaultValue null)

    /// Gets telemetry user Id and session Id. Not recommended to use for authorization.
    /// Empty if system accessed anonymously or call originated within the system (e.g. inside a timer invocation).
    let internal getTelemetryUserIdAndSessionId (): string * Option<string> =
        let telemetryUserId =
            match RequestContext.Get(telemetryUserIdKey) with
            | :? string as userId -> userId
            | _ -> ""
        let telemetrySessionId =
            match RequestContext.Get(telemetrySessionIdKey) with
            | :? string as sessionId ->
                if System.String.IsNullOrEmpty sessionId then None else Some sessionId
            | _ -> None
        telemetryUserId, telemetrySessionId
