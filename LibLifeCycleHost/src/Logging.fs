[<AutoOpen>]
module LibLifeCycleHost.Logging

open System
open LibLifeCycleCore
open System.Collections.Generic
open Printf
open FSharpPlus
open Microsoft.Extensions.Logging
open System.Threading
open System.Text

[<Literal>]
let PartitionScopeKey         = "Partition"

[<Literal>]
let IdStrScopeKey             = "Id"

[<Literal>]
let LifeCycleNameScopeKey     = "LifeCycle"

[<Literal>]
let ConnectorNameScopeKey     = "Connector"

[<Literal>]
let RepoNameScopeKey          = "Repo"

[<Literal>]
let ObserverSubjectIdScopeKey = "ObserverSubjectId"

[<Literal>]
let HubConnectionIdScopeKey   = "HubConnectionId"

let private defaultGrainPartitionIdStr =
    let (GrainPartition grainPartitionGuid) = defaultGrainPartition
    grainPartitionGuid.ToString()

// Ideally we just want to override the _.ToString in a IReadOnlyDictionary
type LoggerScopeDictionary = private LoggerScopeDictionary of IReadOnlyDictionary<string, string>
with
    // This allows proper formatting by console logger
    override this.ToString() : string =
        // Needs to be very very fast, as this will be used for all traces
        // So we're going to do this the non-functional way
        let sb = StringBuilder()
        let (LoggerScopeDictionary scope) = this
        scope
        |> Seq.filter (fun kvp ->
            // Skip this useless entry, which will mostly likely be true in almost all cases
            kvp.Key <> PartitionScopeKey || kvp.Value <> defaultGrainPartitionIdStr
        )
        |> Seq.filter (fun kvp ->
            // The logger<'T> has been shunted to the subject type itself, life cycle name isn't really necessary
            kvp.Key <> LifeCycleNameScopeKey)
        |> Seq.iteri (fun i kvp ->
            if i > 0 then
                sb.Append ", " |> ignore
            sb.Append(kvp.Key).Append(" = ").Append(kvp.Value) |> ignore)

        sb.ToString()

    interface IEnumerable<KeyValuePair<string, obj>> with
        member this.GetEnumerator(): Collections.Generic.IEnumerator<KeyValuePair<string, obj>> =
            let (LoggerScopeDictionary scope) = this
            let e = scope.GetEnumerator()

            { new Collections.Generic.IEnumerator<KeyValuePair<string, obj>> with
                member _.Current = KeyValuePair<string, obj>(e.Current.Key, box e.Current.Value)
                member _.Current = (e :> Collections.IEnumerator).Current
                member _.MoveNext() = e.MoveNext()
                member _.Reset() = e.Reset()
                member _.Dispose() = e.Dispose()
            }

    interface IEnumerable<KeyValuePair<string, string>> with
        member this.GetEnumerator(): Collections.Generic.IEnumerator<KeyValuePair<string, string>> =
            let (LoggerScopeDictionary scope) = this
            scope.GetEnumerator()

    interface Collections.IEnumerable with
        member this.GetEnumerator(): Collections.IEnumerator =
            (this :> IEnumerable<KeyValuePair<string, string>>).GetEnumerator()

    interface IReadOnlyDictionary<string, string> with
        member this.ContainsKey(key) = let (LoggerScopeDictionary scope) = this in scope.ContainsKey key
        member this.Count = let (LoggerScopeDictionary scope) = this in scope.Count
        member this.Item with get (key) = let (LoggerScopeDictionary scope) = this in scope.[key]
        member this.Keys = let (LoggerScopeDictionary scope) = this in scope.Keys
        member this.TryGetValue(key, value) = let (LoggerScopeDictionary scope) = this in scope.TryGetValue(key, &value)
        member this.Values = let (LoggerScopeDictionary scope) = this in scope.Values

type IFsLogger =
    abstract member Log:   LogLevel -> Printf.StringFormat<'T, unit> -> 'T
    abstract member Trace: Printf.StringFormat<'T, unit> -> 'T
    abstract member Debug: Printf.StringFormat<'T, unit> -> 'T
    abstract member Info:  Printf.StringFormat<'T, unit> -> 'T
    abstract member Warn:  Printf.StringFormat<'T, unit> -> 'T
    abstract member Error: Printf.StringFormat<'T, unit> -> 'T

    abstract member LogExn:   exn -> LogLevel -> Printf.StringFormat<'T, unit> -> 'T
    abstract member InfoExn:  exn -> Printf.StringFormat<'T, unit> -> 'T
    abstract member WarnExn:  exn -> Printf.StringFormat<'T, unit> -> 'T
    abstract member ErrorExn: exn -> Printf.StringFormat<'T, unit> -> 'T

    // P Short for Param -- this is a way to use "%a" format specifier, to provide named params to ILogger for structured logging
    abstract member P:   string -> unit -> obj -> string

    // scope of the logger. Is it leaky abstraction? Still helps to inject values into telemetry
    abstract member Scope: LoggerScopeDictionary

let private formatValues (valueSummarizers: ValueSummarizers) (values: ResizeArray<obj>) : obj[] =
    values
    |> Seq.map (valueSummarizers.FormatValue >> box)
    |> Seq.toArray

type private ScopedLogger = {
    Logger:           ILogger
    Values:           AsyncLocal<ResizeArray<obj>>
    Scope:            LoggerScopeDictionary
    ValueSummarizers: ValueSummarizers
} with
    member private this.Log'<'T> (logLevel: LogLevel) (fmt: Printf.StringFormat<'T, unit>) : 'T =
        if this.Logger.IsEnabled logLevel then
            let currentValues = ResizeArray<obj>()
            this.Values.Value <- currentValues
            kprintf (fun str ->
                // Cast is here only to break compilation if the implementation is removed. Log sinks like Seq and AppInsights do a runtime check
                // for IEnumerable<KeyValuePair<string, obj>> and extract each scope entry individually.
                use _scope = this.Logger.BeginScope (this.Scope :> IEnumerable<KeyValuePair<string, obj>>)
                let formattedValues = formatValues this.ValueSummarizers this.Values.Value
                this.Logger.Log(logLevel, str, formattedValues)
            ) fmt
        else
            kprintf ignore fmt

    member private this.LogExn'<'T> (excn: exn) (logLevel: LogLevel) (fmt: Printf.StringFormat<'T, unit>) : 'T =
        if this.Logger.IsEnabled logLevel then
            let currentValues = ResizeArray<obj>()
            this.Values.Value <- currentValues
            kprintf (fun str ->
                // Cast is here only to break compilation if the implementation is removed. Log sinks like Seq and AppInsights do a runtime check
                // for IEnumerable<KeyValuePair<string, obj>> and extract each scope entry individually.
                use _scope = this.Logger.BeginScope (this.Scope :> IEnumerable<KeyValuePair<string, obj>>)
                let formattedValues = formatValues this.ValueSummarizers this.Values.Value
                this.Logger.Log(logLevel, excn, str, formattedValues)
            ) fmt
        else
            kprintf ignore fmt

    interface IFsLogger with
        member this.Log (logLevel: LogLevel) (fmt: Printf.StringFormat<'T, unit>) : 'T =
            this.Log' logLevel fmt

        member this.Trace (fmt: Printf.StringFormat<'T, unit>) : 'T =
            this.Log' LogLevel.Trace fmt

        member this.Debug (fmt: Printf.StringFormat<'T, unit>) : 'T =
            this.Log' LogLevel.Debug fmt

        member this.Info (fmt: Printf.StringFormat<'T, unit>) : 'T =
            this.Log' LogLevel.Information fmt

        member this.Warn (fmt: Printf.StringFormat<'T, unit>) : 'T =
            this.Log' LogLevel.Warning fmt

        member this.Error (fmt: Printf.StringFormat<'T, unit>) : 'T =
            this.Log' LogLevel.Error fmt

        member this.LogExn (excn: exn) (logLevel: LogLevel) (fmt: Printf.StringFormat<'T, unit>) : 'T =
            this.LogExn' excn logLevel fmt

        member this.InfoExn (excn: exn) (fmt: Printf.StringFormat<'T, unit>) : 'T =
            this.LogExn' excn LogLevel.Information fmt

        member this.WarnExn (excn: exn) (fmt: Printf.StringFormat<'T, unit>) : 'T =
            this.LogExn' excn LogLevel.Warning fmt

        member this.ErrorExn (excn: exn) (fmt: Printf.StringFormat<'T, unit>) : 'T =
            this.LogExn' excn LogLevel.Error fmt

        member this.P (name: string) () (arg: obj) : string =
            let arr = this.Values.Value
            if not (isNull arr) then
                arr.Add arg
            "`{" + name  + "}`"

        member this.Scope = this.Scope

let newGrainScopedLogger (summarizers: ValueSummarizers) (logger: ILogger) (lifeCycleName: string) (GrainPartition grainPartition) (idStr: string) : IFsLogger =
    {
        Logger = logger
        Values  = AsyncLocal<ResizeArray<obj>>()
        Scope = seq {
            (PartitionScopeKey,     (grainPartition.ToString()))
            (LifeCycleNameScopeKey, lifeCycleName)
            (IdStrScopeKey,         idStr)
        } |> readOnlyDict |> LoggerScopeDictionary
        ValueSummarizers = summarizers
    } :> IFsLogger

let newConnectorScopedLogger (summarizers: ValueSummarizers) (logger: ILogger) (connectorName: string) (GrainPartition grainPartition) : IFsLogger =
    {
        Logger = logger
        Values  = AsyncLocal<ResizeArray<obj>>()
        Scope = seq {
            (PartitionScopeKey,     (grainPartition.ToString()))
            (ConnectorNameScopeKey, connectorName)
        } |> readOnlyDict |> LoggerScopeDictionary
        ValueSummarizers = summarizers
    } :> IFsLogger

let newRepoScopedLogger (summarizers: ValueSummarizers) (logger: ILogger) (subjectName: string) (GrainPartition grainPartition) : IFsLogger =
    {
        Logger = logger
        Values  = AsyncLocal<ResizeArray<obj>>()
        Scope = seq {
            (PartitionScopeKey, (grainPartition.ToString()))
            (RepoNameScopeKey,  subjectName)
        } |> readOnlyDict |> LoggerScopeDictionary
        ValueSummarizers = summarizers
    } :> IFsLogger

let newPartitionScopedLogger (summarizers: ValueSummarizers) (logger: ILogger) (GrainPartition grainPartition) : IFsLogger =
    {
        Logger = logger
        Values  = AsyncLocal<ResizeArray<obj>>()
        Scope = seq {
            (PartitionScopeKey, (grainPartition.ToString()))
        } |> readOnlyDict |> LoggerScopeDictionary
        ValueSummarizers = summarizers
    } :> IFsLogger

let newRealTimeScopedLogger (summarizers: ValueSummarizers) (logger: ILogger) (lifeCycleName: string) (hubConnectionId: string) : IFsLogger =
    {
        Logger = logger
        Values  = AsyncLocal<ResizeArray<obj>>()
        Scope = seq {
            (LifeCycleNameScopeKey, lifeCycleName)
            (HubConnectionIdScopeKey, hubConnectionId)
        } |> readOnlyDict |> LoggerScopeDictionary
        ValueSummarizers = summarizers
    } :> IFsLogger

let newPartitionScopedLoggerUntyped  (summarizers: ValueSummarizers) (loggerType: Type) (loggerOfTheCorrectType: ILogger) (grainPartition: GrainPartition) : IFsLogger =
    typeof<IFsLogger>.DeclaringType.GetMethod(nameof newPartitionScopedLogger)
        .MakeGenericMethod(loggerType)
        .Invoke(null, [|summarizers; loggerOfTheCorrectType; grainPartition|])
        :?> IFsLogger
