[<AutoOpen>]
module LibLifeCycleTest.SideEffectTracking

open System
open System.Collections.Concurrent
open System.Threading.Tasks
open LibLifeCycleCore
open LibLifeCycleHost

type internal UnprocessedSideEffectsState =
| Processed of Version: int
| Unprocessed of Version: int * AllSideEffectsProcessedPromise: TaskCompletionSource<int> * NonemptyMap<GrainSideEffectId, GrainSideEffect<LifeAction, OpError>>

type internal TrackedSideEffectsData = private {
    LockObj: obj
    mutable UnprocessedSideEffects: UnprocessedSideEffectsState
}

type internal TestSideEffectTrackerHook (grainPartition: GrainPartition) as this =

    let (GrainPartition grainPartitionGuid) = grainPartition

    static member val DataByPartition = ConcurrentDictionary<Guid, TrackedSideEffectsData>() with get

    member private _.Data () = TestSideEffectTrackerHook.DataByPartition.GetOrAdd (grainPartitionGuid, fun _ -> { LockObj = obj (); UnprocessedSideEffects = Processed 0})

    interface ISideEffectTrackerHook with
        member _.OnNewSideEffects (newSideEffects: seq<GrainSideEffectId * GrainSideEffect<LifeAction, OpError>>) =
            let data = this.Data ()
            lock data.LockObj
                (fun _ ->
                    match NonemptyMap.ofSeq newSideEffects with
                    | None ->
                        ()
                    | Some newEffects ->
                        match data.UnprocessedSideEffects with
                        | Processed version ->
                            data.UnprocessedSideEffects <- Unprocessed (version + 1, TaskCompletionSource<int> (), newEffects)
                        | Unprocessed (version, promise, existingEffects) ->
                            let effects = NonemptyMap.addNonEmptySet newEffects.Keys (fun k -> newEffects.TryFind k |> Option.get) existingEffects
                            data.UnprocessedSideEffects <- Unprocessed (version, promise, effects))

        member _.OnSideEffectProcessed (processedSideEffectId: GrainSideEffectId) =
            let data = this.Data ()
            lock data.LockObj
                (fun _ ->
                    match data.UnprocessedSideEffects with
                    | Processed _ ->
                        ()
                    | Unprocessed (version, promise, existing) ->
                        match existing.Remove processedSideEffectId with
                        | None ->
                            // first reset, only then fill the promise! Otherwise deadlocks are possible
                            data.UnprocessedSideEffects <- Processed (version + 1)
                            promise.SetResult (version + 1)
                        | Some remaining ->
                            data.UnprocessedSideEffects <- Unprocessed (version, promise, remaining))

        member _.WaitForAllSideEffectsProcessed (waitFor: TimeSpan) : Task<Result<int, list<GrainSideEffect<LifeAction, OpError>>>> =
            let data = this.Data()
            match data.UnprocessedSideEffects with
            | Processed version ->
                Task.FromResult (Ok version)
            | Unprocessed (_, promise, unprocessed) ->
                backgroundTask {
                    let! _ = Task.WhenAny (promise.Task, Task.Delay waitFor)
                    if promise.Task.IsCompleted then
                        return Ok promise.Task.Result
                    else
                        let latestUnprocessed =
                            match data.UnprocessedSideEffects with
                            | Unprocessed (_, _, latestUnprocessed) -> latestUnprocessed
                            | Processed _ -> unprocessed

                        return Error (List.ofSeq latestUnprocessed.Values)
                }