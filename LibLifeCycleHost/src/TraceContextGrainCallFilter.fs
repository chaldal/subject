module LibLifeCycleHost.TraceContextGrainCallFilter

open System.Collections.Concurrent
open LibLifeCycleCore
open LibLifeCycleHost.TelemetryModel
open Orleans
open System
open System.Threading.Tasks
open Orleans.Runtime
open LibLifeCycleCore.OrleansEx.TraceContextGrainCallFilter

let private hostAssembly = typeof<LibLifeCycleHost.Config.AnchorTypeForProject>.Assembly
let private coreAssembly = typeof<LibLifeCycleCore.Anchor.AnchorTypeForProject>.Assembly

// TODO: should be the same as LibLifeCycleCore.OrleansEx.TraceContextGrainCallFilter ? (in Core i.e. external client it sets the ParentId more aggressively)
type TraceContextOutgoingGrainCallFilter () =
    interface IOutgoingGrainCallFilter with
        member this.Invoke(context: IOutgoingGrainCallContext) =
            // Pass activity Id only if it's not yet set and it's not a sys grain
            // i.e. for initial passing, typically from HTTP Request activity.
            // Cross-silo passing is more involved and is done by TraceContextIncomingGrainCallFilter and SideEffectProcessor
            if (context.InterfaceMethod = null || context.InterfaceMethod.DeclaringType.Assembly = coreAssembly || context.InterfaceMethod.DeclaringType.Assembly = hostAssembly) &&
                RequestContext.Get ParentActivityIdKey = null  then
                let activity = System.Diagnostics.Activity.Current
                RequestContext.Set (ParentActivityIdKey, if activity = null then null else activity.Id)
                context.Invoke()
            else
                context.Invoke()

let private resultTypeDef = typedefof<Result<_, _>>
let mutable private resultTagReaders = ConcurrentDictionary<Type, {| Reader: obj -> int |} >()
let private tryGetResultTagReader (typ: Type) =
    if not typ.IsGenericType then
        None
    else
        let typeDef = typ.GetGenericTypeDefinition()
        if typeDef <> resultTypeDef then
            None
        else
            resultTagReaders.GetOrAdd(typ, fun typ -> {| Reader = Microsoft.FSharp.Reflection.FSharpValue.PreComputeUnionTagReader typ |})
            |> Some

let private argTypesToSkip =
    [
        typeof<SideEffectDedupInfo>
        typeof<Option<SideEffectDedupInfo>>
        typeof<ClientGrainCallContext>
        typeof<unit>
    ]
    |> Seq.map (fun t -> t, ())
    |> readOnlyDict

type TraceContextIncomingGrainCallFilter
        (operationTracker: OperationTracker,
         valueSummarizers: ValueSummarizers) =

    interface IIncomingGrainCallFilter with
        member this.Invoke(context: IIncomingGrainCallContext) =

            match context.Grain with
            | :? ITrackedGrain as trackedGrain ->
                // Receive reminder should not be tracked
                if (context.InterfaceMethod <> null && context.InterfaceMethod.DeclaringType = typeof<IRemindable>) then
                    context.Invoke()
                else
                    task {
                        match trackedGrain.GetTelemetryData context.InterfaceMethod context.Arguments with
                        | Some telemetryData ->
                            let opName =
                                if not (String.IsNullOrEmpty telemetryData.Name) then
                                    telemetryData.Name
                                elif context.InterfaceMethod <> null then
                                    context.InterfaceMethod.Name
                                else
                                    // investigate this, method is null for some methods like ConstructAndWait, or connector grains
                                    // See https://github.com/dotnet/orleans/issues/6578
                                    trackedGrain.GetType().Name

                            let beforeRunProperties =
                                let maxArgCount = 4
                                seq {
                                    yield! telemetryData.Scope |> Seq.filter (fun kvp ->
                                        // partition always the same, don't waste space
                                        kvp.Key <> "Partition" &&
                                        // LC / connector names are already part of opName where available
                                        kvp.Key <> "LifeCycle" && kvp.Key <> "Connector")
                                        |> Seq.map (fun kvp -> kvp.Key, kvp.Value)

                                    // TODO: why don't we just track grain telemetry explicitly in methods? It'll be simpler and more reliable albeit verbose (or is it), and we'll lose auto-coverage of new members

                                    let arguments = context.Arguments |> fun args -> if args = null then [||] else args
                                    yield!
                                        arguments
                                        |> Seq.filter (fun arg -> arg = null || arg.GetType() |> argTypesToSkip.ContainsKey |> not)
                                        |> Seq.mapi (fun i arg ->
                                            if i < maxArgCount then
                                                ($"Arg%d{i}", valueSummarizers.FormatValue arg) |> Some
                                            elif i = maxArgCount then
                                                ("ArgN", $"..+%d{arguments.Length - maxArgCount}") |> Some
                                            else
                                                None)
                                        |> Seq.choose id

                                    // incidentally, Call Id is the SideEffectId of the caller, write here so we can eliminate Side Effect telemetry aggressively
                                    yield!
                                        arguments
                                        |> Seq.choose (function
                                            | :? SideEffectDedupInfo as dedupInfo -> Some dedupInfo
                                            | :? Option<SideEffectDedupInfo> as maybeDedupInfo -> maybeDedupInfo
                                            | _ -> None)
                                        |> Seq.mapi (fun i dedupInfo ->
                                            let getKey key = if i = 0 then key else $"%s{key}_%d{i+1}"
                                            [
                                                getKey "SideEffectId", dedupInfo.Id.ToString("D")
                                                getKey "CallerLifeCycle", $"%s{dedupInfo.Caller.LifeCycleKey.EcosystemName}/%s{dedupInfo.Caller.LifeCycleKey.LocalLifeCycleName}"
                                                getKey "CallerId", dedupInfo.Caller.SubjectIdStr
                                            ])
                                        |> Seq.concat
                                }
                                |> Map.ofSeq

                            let maybeParentActivityId =
                                let parentActivityId = RequestContext.Get ParentActivityIdKey :?> string
                                if (String.IsNullOrEmpty parentActivityId |> not) then Some parentActivityId  else  None

                            do! operationTracker.TrackOperation<_>
                                 { Partition = telemetryData.Partition
                                   Type = telemetryData.Type
                                   Name = opName
                                   MaybeParentActivityId = maybeParentActivityId
                                   // create new parent Id at this tracked level of nesting so the yielded "Side Effect" dependencies appear below this "Grain" dependency
                                   MakeItNewParentActivityId = true
                                   BeforeRunProperties = beforeRunProperties }
                                 (fun () ->
                                    task {
                                        do! context.Invoke ()
                                        let afterRunProperties = Map.ofOneItem ("RetVal", valueSummarizers.FormatValue context.Result)

                                        let isSuccess =
                                            if context.Result <> null then
                                                let typ = context.Result.GetType()
                                                match tryGetResultTagReader typ with
                                                | Some r ->
                                                    let ok = (r.Reader context.Result) = 0
                                                    Some ok
                                                | None ->
                                                    None
                                            else
                                                None
                                        return { ReturnValue = (); IsSuccess = isSuccess; AfterRunProperties = afterRunProperties }
                                    })
                                 |> Task.Ignore
                        | None ->
                            // leave ParentActivityIdKey for untracked call so the yielded "Side Effect" dependencies appear below last known parent
                            do! context.Invoke ()
                    } |> Task.Ignore

            | _ ->
                // pass thru calls to untracked grains
                context.Invoke()

