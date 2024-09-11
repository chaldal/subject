module LibClient.Seq

// Provides integration with Seq (https://datalust.co/seq), nothing to do with F# sequences.

open System
open System.Text
open Fable.Core.JsInterop
open LibClient
open LibClient.JsInterop
open LibClient.Services.HttpService.HttpService
open LibClient.Services.HttpService.Types
open LibClient.MessageTemplates

type SeqConfig = {
    Endpoint: string
    MaybeApiKey: Option<string>
    MaxBufferCapacity: int
    MaxFlushDelay: TimeSpan
    MaxFlushBufferCapacity: int
}

type private LogEvent = {
    Timestamp: DateTimeOffset
    MaybeCategory: Option<string>
    Level: LogLevel
    Properties: LogProperties
    Event: Event
}

[<RequireQualifiedAccess>]
module private LogEvent =
    let toJson (logEvent: LogEvent): string =
        // https://docs.datalust.co/docs/posting-raw-events
        seq {
            ("Origin", box "Frontend")
            ("@t", box logEvent.Timestamp)
            ("@mt", box logEvent.Event.MessageTemplate.Value)
            (
                "@l",
                match logEvent.Level with
                | LogLevel.Debug -> "Debug"
                | LogLevel.Info -> "Info"
                | LogLevel.Warn -> "Warn"
                | LogLevel.Error -> "Error"
                |> box
            )

            match logEvent.MaybeCategory with
            | Some category ->
                // "SourceContext" is the name Seq uses to store the category (see https://github.com/datalust/seq-extensions-logging/blob/90a7471e0c48d1065e60338a1c8f646a85e845c8/src/Seq.Extensions.Logging/Serilog/Core/Constants.cs)
                ("SourceContext", box category)
            | None ->
                ()

            yield!
                Seq.concat
                    [
                        logEvent.Event.GetProperties() |> Seq.map (fun property -> (property.NamedHole.Name, property.Value))
                        logEvent.Properties |> Map.toSeq
                    ]
                |> Seq.map (fun (name, value) ->
                    (
                        name,
                        // Fable elides properties that have None as their value. We want to explicitly include them as null so that Seq doesn't highlight
                        // them as missing.
                        if value = None then null else value
                    )
                )
        }
        |> createObj
        |> Fable.Core.JS.JSON.stringify

    let writeAsJson (sb: StringBuilder) (logEvent: LogEvent): unit =
        logEvent
        |> toJson
        |> sb.AppendLine
        |> ignore

type SeqLogSink (config: SeqConfig, getHttpService: unit -> HttpService) =
    let buffer = ResizeArray<LogEvent>(config.MaxBufferCapacity)
    let flushingBuffer = ResizeArray<LogEvent>(config.MaxFlushBufferCapacity)
    let timerDisposable = new SerialDisposable()
    let mutable lastSendSucceeded = true

    let trySendToEndpoint (logEvents: seq<LogEvent>) (count: int): Async<bool> =
        async {
            if count = 0 then
                return true
            else
                let payload = StringBuilder()
                logEvents
                |> Seq.take count
                |> Seq.iter (LogEvent.writeAsJson payload)

                try
                    let httpService = getHttpService ()
                    let! _ =
                        httpService.RequestRaw<string, string>
                            $"{config.Endpoint}/api/events/raw"
                            HttpAction.Post
                            (payload.ToString())
                            (Some "application/vnd.serilog.clef")
                            None

                    lastSendSucceeded <- true
                    return true
                with
                | e ->
                    // Don't output for every failure, only for failures that follow a successful send (otherwise the console becomes too busy).
                    if lastSendSucceeded then
                        Browser.Dom.console.warn $"Failed to send log events to seq: %A{e}"
                        lastSendSucceeded <- false

                    return false
        }

    let flushBuffer (): Async<unit> =
        async {
            flushingBuffer.AddRange(buffer)
            buffer.Clear()

            let countToSend = flushingBuffer.Count
            let! result = trySendToEndpoint flushingBuffer countToSend

            if result then
                // Successfully sent, so remove the sent log events.
                flushingBuffer.RemoveRange(0, countToSend)
            else if flushingBuffer.Count > config.MaxFlushBufferCapacity then
                // We've exceeded the maximum number of unflushed items we can cache, so we need to drop some.
                let countToDrop = flushingBuffer.Count - config.MaxFlushBufferCapacity
                Browser.Dom.console.warn $"Dropping %i{countToDrop} log events because flush buffer has exceeded configured maximum of %i{config.MaxFlushBufferCapacity}"
                flushingBuffer.RemoveRange(0, countToDrop)
        }

    let recordLogEvent logEvent =
        let flushInBackground () =
            flushBuffer ()
            |> startSafely

        buffer.Add(logEvent)

        if buffer.Count = config.MaxBufferCapacity then
            flushInBackground ()
        else
            timerDisposable.ReplaceInnerDisposable(runLaterDisposable config.MaxFlushDelay flushInBackground)

    interface ILogSink with
        override _.Log (level: LogLevel, maybeCategory: Option<string>, properties: LogProperties, event: Event) : Unit =
            recordLogEvent
                {
                    Timestamp = DateTimeOffset.UtcNow
                    MaybeCategory = maybeCategory
                    Level = level
                    Properties = properties
                    Event = event
                }

    interface IDisposable with
        member _.Dispose() =
            (timerDisposable :> IDisposable).Dispose()