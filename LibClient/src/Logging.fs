namespace LibClient

open System
open System.Text
open LibClient.MessageTemplates

type LogProperty = string * obj
type LogProperties = Map<string, obj>

[<RequireQualifiedAccess>]
type LogLevel =
| Debug
| Info
| Warn
| Error
with
    static member ParseOption (value: string): Option<LogLevel> =
        match value.ToLowerInvariant() with
        | "debug" ->
            Some LogLevel.Debug
        | "info"
        | "information" ->
            Some LogLevel.Info
        | "warn"
        | "warning" ->
            Some LogLevel.Warn
        | "error" ->
            Some LogLevel.Error
        | _ ->
            None

type ILogSink =
    abstract member Log: level: LogLevel * maybeCategory: Option<string> * properties: LogProperties * event: Event -> unit

type ILog =
    abstract member WithCategory: category: string -> ILog
    abstract member WithMinimumLevel: minimumLevel: LogLevel -> ILog
    abstract member WithProperty: property: LogProperty -> ILog
    abstract member WithProperties: properties: LogProperties -> ILog

    abstract member Log: level: LogLevel * messageTemplate: string * [<ParamArray>] Args: obj[] -> unit
    abstract member Debug: messageTemplate: string * [<ParamArray>] Args: obj[] -> unit
    abstract member Info: messageTemplate: string * [<ParamArray>] Args: obj[] -> unit
    abstract member Warn: messageTemplate: string * [<ParamArray>] Args: obj[] -> unit
    abstract member Error: messageTemplate: string * [<ParamArray>] Args: obj[] -> unit

[<AutoOpen>]
module LoggingHelpers =
    [<RequireQualifiedAccess>]
    type TypedPropertyValue =
    | NullOrUndefined
    | Boolean of bool
    | String of string
    | Integer of int64
    | Real of float
    | SerializedObject of string
    with
        member this.ToDisplayString(): string =
            match this with
            | TypedPropertyValue.Boolean b -> string b
            | TypedPropertyValue.String str -> str
            | TypedPropertyValue.Integer num -> string num
            | TypedPropertyValue.Real num -> string num
            | TypedPropertyValue.SerializedObject str -> str
            | TypedPropertyValue.NullOrUndefined -> "null"

    let getTypedPropertyValue (value: obj): TypedPropertyValue =
        if Fable.Core.JsInterop.isNullOrUndefined value then
            TypedPropertyValue.NullOrUndefined
        else
            match value with
            | :? bool as b ->
                b
                |> TypedPropertyValue.Boolean
            | :? string
            | :? DateTime
            | :? DateTimeOffset ->
                value
                |> Fable.Core.JS.JSON.stringify
                |> TypedPropertyValue.String
            | :? int as num ->
                num
                |> int64
                |> TypedPropertyValue.Integer
            | :? int64 as num ->
                num
                |> TypedPropertyValue.Integer
            | :? single as num ->
                num
                |> float
                |> TypedPropertyValue.Real
            | :? double as num ->
                num
                |> float
                |> TypedPropertyValue.Real
            | _ ->
                value
                |> Fable.Core.JS.JSON.stringify
                |> TypedPropertyValue.SerializedObject

type ConsoleLogSink(?includeFormatting: bool, ?includePropertyGroup: bool) =
    let includeFormatting = defaultArg includeFormatting true
    let includePropertyGroup = defaultArg includePropertyGroup false

    [<Literal>]
    let resetStyle =""

    [<Literal>]
    let categoryStyle ="color: #999999"

    [<Literal>]
    let booleanNamedHoleStyle ="color: #3499d8; background: #f5f5f5"

    [<Literal>]
    let stringNamedHoleStyle ="color: #af3434; background: #f5f5f5"

    [<Literal>]
    let numberNamedHoleStyle ="color: #0a820a; background: #f5f5f5"

    [<Literal>]
    let nullOrUndefinedNamedHoleStyle ="color: #777777; background: #f5f5f5"

    [<Literal>]
    let objectNamedHoleStyle ="color: #bcaf1c; background: #f5f5f5"

    interface ILogSink with
        member _.Log(level: LogLevel, maybeCategory: Option<string>, properties: LogProperties, event: Event) : Unit =
            let formattedMessage, consoleArgs =
                if event.Arguments.Length = 0 then
                    match maybeCategory with
                    | None ->
                        (event.MessageTemplate.Value, Array.empty)
                    | Some category ->
                        if includeFormatting then
                            ($"%%c[{category}]%%c {event.MessageTemplate.Value}", [| box categoryStyle; box resetStyle |])
                        else
                            ($"[{category}] {event.MessageTemplate.Value}", Array.empty)
                else
                    let output = StringBuilder()
                    let styles = ResizeArray<obj>()

                    match maybeCategory with
                    | None -> ()
                    | Some category ->
                        if includeFormatting then
                            output.Append($"%%c[{category}]%%c ")
                            |> ignore

                            styles.Add(categoryStyle)
                            styles.Add(resetStyle)
                        else
                            output.Append($"[{category}] ")
                            |> ignore

                    event.Append(
                        output,
                        fun property ->
                            let typedPropertyValue = getTypedPropertyValue property.Value

                            if includeFormatting then
                                match typedPropertyValue with
                                | TypedPropertyValue.Boolean _ -> booleanNamedHoleStyle
                                | TypedPropertyValue.String _ -> stringNamedHoleStyle
                                | TypedPropertyValue.Integer _
                                | TypedPropertyValue.Real _ -> numberNamedHoleStyle
                                | TypedPropertyValue.SerializedObject _ -> objectNamedHoleStyle
                                | TypedPropertyValue.NullOrUndefined -> nullOrUndefinedNamedHoleStyle
                                |> styles.Add
                                styles.Add(resetStyle)

                                $"%%c%s{typedPropertyValue.ToDisplayString()}%%c"
                            else
                                typedPropertyValue.ToDisplayString()
                    )

                    (output.ToString(), styles.ToArray())

            match level with
            | LogLevel.Debug -> Browser.Dom.console.debug(formattedMessage, consoleArgs)
            | LogLevel.Info  -> Browser.Dom.console.info(formattedMessage, consoleArgs)
            | LogLevel.Warn  -> Browser.Dom.console.warn(formattedMessage, consoleArgs)
            | LogLevel.Error -> Browser.Dom.console.error(formattedMessage, consoleArgs)

            if includePropertyGroup && (properties.Count > 0 || event.Arguments.Length > 0) then
                Browser.Dom.console.groupCollapsed("")

                Seq.concat
                    [
                        properties |> Map.toSeq
                        event.GetProperties() |> Seq.map (fun property -> (property.NamedHole.Name, property.Value))
                    ]
                |> Seq.iter (fun (name, value) ->
                    let typedPropertyValue = getTypedPropertyValue value
                    Browser.Dom.console.log($"{name} = {typedPropertyValue.ToDisplayString()}")
                )

                Browser.Dom.console.groupEnd()

type private CompositeLogSink(initialLogSinks: List<ILogSink>) =
    let mutable logSinks: List<ILogSink> = initialLogSinks

    let forEachSinkSafely (fn: ILogSink -> unit) : unit =
        // we need to guard against the possibility that some sink has a blocking implementation,
        // so we delay execution until the next event loop cycle
        async {
            logSinks
            |> Seq.iter (fun sink ->
                try
                    fn sink
                with
                | e ->
                    Browser.Dom.console.error e
                    Noop // nothing we can do if a sink fails
            )
        } |> Async.StartImmediate // not using startSafely because we don't want to log

    member _.LogSinks = logSinks

    member _.ClearLogSinks(): unit =
        logSinks <- []

    member _.AddLogSink(logSink: ILogSink): unit =
        logSinks <- logSinks |> List.append [logSink]

    member _.AddLogSinks(logSinks': seq<ILogSink>): unit =
        logSinks <- logSinks |> List.append (logSinks' |> List.ofSeq)

    interface ILogSink with
        member _.Log(level: LogLevel, maybeCategory: Option<string>, properties: LogProperties, event: Event) : unit =
            forEachSinkSafely (fun sink -> sink.Log(level, maybeCategory, properties, event))

type private LogImpl(
        initialMinimumLevel: LogLevel,
        maybeCategory: Option<string>,
        initialProperties: LogProperties,
        logSink: ILogSink ) =
    let mutable minimumLevel = initialMinimumLevel
    let mutable properties = initialProperties

    member _.MinimumLevel
        with get () = minimumLevel
        and set (value) = minimumLevel <- value

    member _.MaybeCategory = maybeCategory

    member _.Properties
        with get () = properties
        and set (value) = properties <- value

    member _.LogSink = logSink

    interface ILog with
        member _.WithCategory(category: string) : ILog =
            LogImpl(minimumLevel, Some category, properties, logSink)

        member _.WithMinimumLevel(minimumLevel: LogLevel) : ILog =
            LogImpl(minimumLevel, maybeCategory, properties, logSink)

        member _.WithProperty(property: LogProperty) : ILog =
            let name, value = property
            LogImpl(
                minimumLevel,
                maybeCategory,
                Map.add name value properties,
                logSink)

        member _.WithProperties(properties': LogProperties) : ILog =
            LogImpl(
                minimumLevel,
                maybeCategory,
                Map.merge properties properties',
                logSink)

        member this.Log(level: LogLevel, messageTemplate: string, args: obj[]) : unit =
            if level >= minimumLevel then
                let event =
                    {
                        MessageTemplate = MessageTemplate messageTemplate
                        Arguments = if args = null then Array.empty else args
                    }

                let countOfNamedHolesWithoutValues, countOfValuesWithoutNamedHoles =
                    event.GetNamedHolesAndValues()
                    |> Seq.choose (fun zipOuterResult ->
                        match zipOuterResult with
                        | ZipOuterResult.BothPresent _ -> None
                        | ZipOuterResult.FirstPresent _ -> Some (1, 0)
                        | ZipOuterResult.SecondPresent _ -> Some (0, 1)
                    )
                    |> Seq.fold
                        (fun (runningCount1, runningCount2) (delta1, delta2) ->
                            runningCount1 + delta1, runningCount2 + delta2
                        )
                        (0, 0)

                // Detect developer errors and report accordingly.
                //
                // Be sure to take extra care and test if you edit these messages, since a mismatch in named holes and values here will
                // result in runaway recursion.
                if countOfNamedHolesWithoutValues > 0 then
                    (this :> ILog).Error("Log message with template {MessageTemplate} has {OrphanedNamedHoleCount} named holes without corresponding values.", event.MessageTemplate.Value, countOfNamedHolesWithoutValues)

                if countOfValuesWithoutNamedHoles > 0 then
                    (this :> ILog).Error("Log message with template {MessageTemplate} has {OrphanedValueCount} values without corresponding named holes.", event.MessageTemplate.Value, countOfValuesWithoutNamedHoles)

                logSink.Log(level, maybeCategory, properties, event)

        member this.Debug(messageTemplate: string, args: obj[]) : unit =
            (this :> ILog).Log(LogLevel.Debug, messageTemplate, args)

        member this.Info(messageTemplate: string, args: obj[]) : unit =
            (this :> ILog).Log(LogLevel.Info, messageTemplate, args)

        member this.Warn(messageTemplate: string, args: obj[]) : unit =
            (this :> ILog).Log(LogLevel.Warn, messageTemplate, args)

        member this.Error(messageTemplate: string, args: obj[]) : unit =
            (this :> ILog).Log(LogLevel.Error, messageTemplate, args)

[<AutoOpen>]
module Logging =
    let mutable private compositeLogSink = CompositeLogSink([])

    let private log = LogImpl(LogLevel.Debug, None, Map.empty, compositeLogSink)
    let Log: ILog = log

    // TODO: rename to setRootLogLevel (or come up with alternate design)
    let setLogLevel (level: LogLevel) : unit =
        log.MinimumLevel <- level

    let setRootLogProperties (properties: LogProperties) : unit =
        log.Properties <- properties

    let registerLogSinks (logSinks: seq<ILogSink>) : unit =
        compositeLogSink.ClearLogSinks()
        compositeLogSink.AddLogSinks(logSinks)

    let addLogSink (logSink: ILogSink) : unit =
        compositeLogSink.AddLogSink(logSink)
