namespace LibClient

type TelemetryProperty = string * obj
type TelemetryProperties = Map<string, obj>

[<RequireQualifiedAccess>]
type TelemetryUser =
| Anonymous
| Identified of UserId: string * UserProperties: TelemetryProperties

type ITelemetrySink =
    abstract member TrackEvent: name: string -> user: TelemetryUser -> properties: TelemetryProperties -> unit
    abstract member TrackScreenView: url: string -> user: TelemetryUser -> properties: TelemetryProperties -> unit

type ITelemetry =
    abstract member GetUser: unit -> TelemetryUser
    abstract member SetUser: user: TelemetryUser -> unit
    abstract member SetProperty: property: TelemetryProperty -> unit
    abstract member SetProperties: properties: TelemetryProperties -> unit
    abstract member ClearProperty: key: string -> unit
    abstract member TrackEvent: name: string -> additionalProperties: TelemetryProperties -> unit
    abstract member TrackScreenView: url: string -> additionalProperties: TelemetryProperties -> unit

[<AutoOpen>]
module TelemetryHelpers =
    let stringifyValueForTelemetry (value: obj): string =
        match value with
        // We don't want to include quotes around string values because it makes filtering on those values clunky.
        | :? string as str -> str
        | _ -> value |> Fable.Core.JS.JSON.stringify

type ConsoleTelemetrySink() =
    let log = Log.WithCategory("ConsoleTelemetrySink")

    interface ITelemetrySink with
        override _.TrackEvent (name: string) (user: TelemetryUser) (properties: TelemetryProperties) : unit =
            log.Info("Event: {Name}, {User}, {Properties}", name, user, properties)

        override _.TrackScreenView (url: string) (user: TelemetryUser) (properties: TelemetryProperties) : unit =
            log.Info("ScreenView: {Url}, {User}, {Properties}", url, user, properties)

type private CompositeTelemetrySink(initialTelemetrySinks: List<ITelemetrySink>) =
    let mutable telemetrySinks: List<ITelemetrySink> = initialTelemetrySinks

    let forEachSinkSafely (fn: ITelemetrySink -> unit) : unit =
        // we need to guard against the possibility that some sink has a blocking implementation,
        // so we delay execution until the next event loop cycle
        async {
            telemetrySinks
            |> Seq.iter (fun sink ->
                try
                    fn sink
                with
                | e ->
                    Browser.Dom.console.error e
                    Noop // nothing we can do if a sink fails
            )
        } |> Async.StartImmediate // not using startSafely because we don't want to log

    member _.TelemetrySinks = telemetrySinks

    member _.ClearTelemetrySinks(): unit =
        telemetrySinks <- []

    member _.AddTelemetrySink(telemetrySink: ITelemetrySink): unit =
        telemetrySinks <- telemetrySinks |> List.append [telemetrySink]

    member _.AddTelemetrySinks(telemetrySinks': seq<ITelemetrySink>): unit =
        telemetrySinks <- telemetrySinks |> List.append (telemetrySinks' |> List.ofSeq)

    interface ITelemetrySink with
        override _.TrackEvent (name: string) (user: TelemetryUser) (properties: TelemetryProperties) : unit =
            forEachSinkSafely (fun sink -> sink.TrackEvent name user properties)

        override _.TrackScreenView (url: string) (user: TelemetryUser) (properties: TelemetryProperties) : unit =
            forEachSinkSafely (fun sink -> sink.TrackScreenView url user properties)

type private TelemetryImpl(telemetrySink: ITelemetrySink) =
    let mutable user: TelemetryUser = TelemetryUser.Anonymous
    let mutable properties: TelemetryProperties = Map.empty

    let mergeAdditionalProperties (additionalProperties: TelemetryProperties): TelemetryProperties =
        Map.merge properties additionalProperties

    member _.TelemetrySink = telemetrySink

    interface ITelemetry with
        member _.GetUser () : TelemetryUser =
            user

        member _.SetUser (user': TelemetryUser) : unit =
            user <- user'

        member _.SetProperty (property: TelemetryProperty) : unit =
            let name, value = property
            properties <- properties.AddOrUpdate(name, value)

        member _.SetProperties (properties': TelemetryProperties) : unit =
            properties <- mergeAdditionalProperties properties'

        member _.ClearProperty (key: string) : unit =
            properties <- properties.Remove(key)

        member _.TrackEvent (name: string) (additionalProperties: TelemetryProperties) : unit =
            telemetrySink.TrackEvent name user (additionalProperties |> mergeAdditionalProperties)

        member _.TrackScreenView (url: string) (additionalProperties: TelemetryProperties) : unit =
            telemetrySink.TrackScreenView url user (additionalProperties |> mergeAdditionalProperties)

[<AutoOpen>]
module Telemetry =
    let mutable private compositeTelemetrySink = CompositeTelemetrySink([ ConsoleTelemetrySink() ])
    
    let private telemetry: ITelemetry = TelemetryImpl(compositeTelemetrySink)
    let Telemetry: ITelemetry = telemetry

    let registerTelemetrySinks (telemetrySinks: seq<ITelemetrySink>) : unit =
        compositeTelemetrySink.ClearTelemetrySinks()
        compositeTelemetrySink.AddTelemetrySinks(telemetrySinks)

    let addTelemetrySink (telemetrySink: ITelemetrySink) : unit =
        compositeTelemetrySink.AddTelemetrySink(telemetrySink)
