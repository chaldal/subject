module LibClient.ApplicationInsights

open Fable.Core.JsInterop
open LibClient.MessageTemplates

let private AppInsightsRaw: obj = import "ApplicationInsights" "@microsoft/applicationinsights-web"

let private getAppInsightExtensions () =
    #if !EGGSHELL_PLATFORM_IS_WEB
    let ReactNativePlugin: obj = import "ReactNativePlugin" "@microsoft/applicationinsights-react-native"
    [| createNew ReactNativePlugin () |]
    #else
    [||]
    #endif

type AppInsightsConfig = {
    InstrumentationKey: string
    CloudRole:          string
}

type ApplicationInsightsSink (config: AppInsightsConfig) =
    let mutable globalProperties: TelemetryProperties = Map.empty

    let propertiesToPojo (properties: TelemetryProperties) : obj =
        properties
        |> Map.toSeq
        |> Seq.map (fun (key, value) -> (key, (stringifyValueForTelemetry value) :> obj))
        |> createObj

    let mergeProperties (first: TelemetryProperties) (second: TelemetryProperties) : TelemetryProperties =
        Map.merge first second

    let mergeEventProperties (event: Event) (properties: TelemetryProperties) : TelemetryProperties =
        mergeProperties properties (event.GetProperties() |> Seq.map (fun property -> property.NamedHole.Name, property.Value) |> Map.ofSeq)

    let mergeGlobalProperties (properties: TelemetryProperties) : TelemetryProperties =
        mergeProperties globalProperties properties

    let mergeUserProperties (user: TelemetryUser) (properties: TelemetryProperties): TelemetryProperties =
        match user with
        | TelemetryUser.Anonymous ->
            properties
        | TelemetryUser.Identified (_, userProperties) ->
            Map.merge properties userProperties

    let appInsights =
        createNew AppInsightsRaw (createObj [
            "config" ==> createObj [
                "instrumentationKey" ==> config.InstrumentationKey
                // default is 15000, which I'm afraid will not survive user attempted page reloads when errors happen
                "maxBatchInterval"   ==> 5000
                "extensions"         ==> getAppInsightExtensions ()
            ]
        ])

    let setUser (user: TelemetryUser) : unit =
        match user with
        | TelemetryUser.Anonymous -> appInsights?clearAuthenticatedUserContext()
        | TelemetryUser.Identified (userId, _) -> appInsights?setAuthenticatedUserContext userId

    let trackTrace (level: LogLevel) (maybeCategory: Option<string>) (properties: LogProperties) (event: Event) : Unit =
        let propertiesPojo =
            [
                (
                    "severity",
                    match level with
                    | LogLevel.Debug -> box "debug"
                    | LogLevel.Info -> box "info"
                    | LogLevel.Warn -> box "warning"
                    | LogLevel.Error -> box "error"
                )

                match maybeCategory with
                | Some category ->
                    ("category", box category)
                | None ->
                    ()
            ]
            |> Map.ofSeq
            |> mergeProperties properties
            |> mergeEventProperties event
            |> mergeGlobalProperties
            |> propertiesToPojo

        appInsights?trackTrace (createObj [
            "message"    ==> string event
            "properties" ==> propertiesPojo
        ])

    do
        appInsights?loadAppInsights()
        appInsights?addTelemetryInitializer (fun envelope ->
            envelope?tags?("ai.cloud.role") <- config.CloudRole
        )

    interface ITelemetrySink with
        override _.TrackEvent (name: string) (user: TelemetryUser) (properties: TelemetryProperties) : unit =
            setUser user

            let propertiesPojo =
                properties
                |> mergeUserProperties user
                |> mergeGlobalProperties
                |> propertiesToPojo
            appInsights?trackEvent (createObj [
                "name"       ==> name
                "properties" ==> propertiesPojo
            ])

        override _.TrackScreenView (url: string) (user: TelemetryUser) (properties: TelemetryProperties) : unit =
            setUser user

            let propertiesPojo =
                properties
                |> mergeUserProperties user
                |> mergeGlobalProperties
                |> propertiesToPojo
            appInsights?trackPageView (createObj [
                // The AppInsights backend is responding with an HTTP 400, saying that
                // the name for the page view is 1024 characters instead of the expected
                // 512. This is probably because the backend and the library used to be in
                // sync, both where updated to the shorter value, but we still have an older
                // version of the library. Since 1024 is too short for a lot of our encoded
                // URLs anyway, and on the eve of the launch we don't want to
                // upgrade the library, we just trim the length here.
                "name"       ==> url.Substring(0, min 512 url.Length)
                "properties" ==> propertiesPojo
            ])

    interface ILogSink with
        member _.Log(level: LogLevel, maybeCategory: Option<string>, properties: LogProperties, event: Event) : Unit =
            trackTrace level maybeCategory properties event
