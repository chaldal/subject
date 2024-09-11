module LibClient.Components.With.GlobalDataFlowControl.Context

open System
open LibLangFsharp
open LibClient
open LibClient.Components

type DataFlowPolicy = LC.With.DataFlowControlTypes.DataFlowPolicy

type DataFlowPolicyValue =
| BlockWhileDeferred of Deferred<unit>
with
    static member forward (value: DataFlowPolicyValue) : DataFlowPolicy =
        match value with
        | BlockWhileDeferred _ -> DataFlowPolicy.Block

type Control = {
    Update: (Map<string, DataFlowPolicyValue> -> Map<string, DataFlowPolicyValue>) -> unit
    Data:   Map<string, DataFlowPolicyValue>
}
with
    member this.Get (key: string) : DataFlowPolicy =
        this.Data.TryFind key |> Option.map DataFlowPolicyValue.forward |> Option.getOrElse DataFlowPolicy.PropagateImmediately

    member this.BlockForAtMost (key: string) (timeout: TimeSpan) : {| Unblock: unit -> unit |} =
        let deferred = Deferred()

        this.Update (fun keyToForward -> keyToForward.AddOrUpdate (key, BlockWhileDeferred deferred))

        async {
            do! Async.Sleep timeout
            deferred.ResolveIfPending ()
        } |> startSafely

        async {
            do! deferred.Value
            this.Update (fun keyToForward ->
                match keyToForward.TryFind key with
                | Some (BlockWhileDeferred currentDeferred) ->
                    if currentDeferred = deferred then
                        keyToForward.Remove key
                    else
                        keyToForward
                | _ ->
                    // the value was since changed, do nothing
                    keyToForward
            )
        } |> startSafely

        {| Unblock = deferred.ResolveIfPending |}

let globalDataFlowControlContext = Fable.React.ReactBindings.React.createContext { Update = ignore; Data = Map.empty }
let globalDataFlowControlContextProvider: Control -> ReactElements -> ReactElement = Fable.React.Helpers.contextProvider globalDataFlowControlContext
