[<AutoOpen>]
module LibLifeCycleTest.ClockSimulation

open System.Threading.Tasks
open System
open Orleans
open LibLifeCycle
open LibLifeCycleCore
open LibLifeCycleHost

type ClockSimulator= private LogType of unit

type private PartitionReminders = {
    LockObj: obj
    mutable Reminders: Map<LifeCycleKey * SubjectId, DateTimeOffset>
}

type ReminderHook() =

    let remindersByPartitionGuid = System.Collections.Concurrent.ConcurrentDictionary<Guid, PartitionReminders>()

    member _.OnClockUpdated (logger: IFsLogger) (grainPartitionGuid: Guid) (newTime: DateTimeOffset) (lifeCycleAdapterCollection: HostedLifeCycleAdapterCollection) (grainFactory: IGrainFactory) : Task =
        match remindersByPartitionGuid.TryGetValue grainPartitionGuid with
        | false, _ ->
            Task.CompletedTask
        | true, reminders ->
            reminders.Reminders
            |> Seq.map (|KeyValue|)
            |> Seq.where(fun (_,remindOn) -> remindOn <= newTime)
            |> Seq.map (
                fun ((lifeCycleKey, pKey), remindOn) ->
                    lifeCycleAdapterCollection.GetLifeCycleAdapterByKey lifeCycleKey
                    |> Option.get
                    |> fun adapter ->
                        logger.Info "Triggering Reminder Scheduled for %a on LifeCycle %a/%a"
                            (logger.P "remind on") remindOn
                            (logger.P "lifecycle") lifeCycleKey.LocalLifeCycleName
                            (logger.P "pKey") pKey
                        adapter.TestingOnlyTriggerReminder grainFactory (GrainPartition grainPartitionGuid) pKey
                )
            |> Task.WhenAll

    interface IReminderHook with

        member _.ClearReminder (logger: IFsLogger) (lifeCycleKey: LifeCycleKey) (GrainPartition grainPartitionGuid) (subjectId: SubjectId): unit =
            logger.Info "Clearing Reminder On %a/%a"
                (logger.P "lifecycle") lifeCycleKey.LocalLifeCycleName
                (logger.P "pKey") subjectId.IdString

            match remindersByPartitionGuid.TryGetValue grainPartitionGuid with
            | false, _ ->
                ()
            | true, reminders ->
                lock
                    reminders.LockObj
                    (fun _ ->
                        reminders.Reminders <- reminders.Reminders.Remove (lifeCycleKey, subjectId))

        member _.RegisterReminder (logger: IFsLogger) (lifeCycleKey: LifeCycleKey) (GrainPartition grainPartitionGuid) (subjectId: SubjectId) (on: DateTimeOffset): unit =
            logger.Info "Setting NextTick Reminder On %a/%a to %a"
                (logger.P "lifecycle") lifeCycleKey.LocalLifeCycleName
                (logger.P "pKey") subjectId.IdString
                (logger.P "remind on") on

            let reminders = remindersByPartitionGuid.GetOrAdd (grainPartitionGuid, fun _ -> { LockObj = obj (); Reminders = Map.empty })
            lock
                reminders.LockObj
                (fun _ ->
                    reminders.Reminders <- reminders.Reminders.AddOrUpdate((lifeCycleKey, subjectId), on))

        member _.FirstReminderOn (GrainPartition grainPartitionGuid) =
            match remindersByPartitionGuid.TryGetValue grainPartitionGuid with
            | false, _ ->
                None
            | true, reminders ->
                reminders.Reminders
                |> Seq.map (|KeyValue|)
                |> Seq.map snd
                |> Seq.tryMinBy id

let reminderHook = ReminderHook()

let private simulatedClockByPartitionId = System.Collections.Generic.Dictionary<Guid, DateTimeOffset>()
let private lockObj = obj()

let private lazyInitSimulatedClock grainPartitionGuid =
    lock lockObj (
        fun _ ->
        match simulatedClockByPartitionId.TryGetValue grainPartitionGuid with
        | true, _ ->
            ()
        | false, _ ->
            simulatedClockByPartitionId.Add (grainPartitionGuid, DateTimeOffset.UtcNow))

let setNewTimeAndTriggerReminders
        (logger: IFsLogger)
        (GrainPartition grainPartitionGuid)
        (newTime: DateTimeOffset)
        (lifeCycleAdapterCollection: HostedLifeCycleAdapterCollection)
        (grainFactory: IGrainFactory) =
    lock lockObj (fun _ ->
        lazyInitSimulatedClock grainPartitionGuid
        let now = simulatedClockByPartitionId.[grainPartitionGuid]
        let effectiveNewTime =
            if now > newTime then
                // NO-OP, can't move the clock backwards
                logger.Info "Clock was already ahead of the new time"
                now
            else
                let delta = newTime - now
                logger.Info "Moving clock forward by %a seconds to time %a"
                    (logger.P "seconds") delta.TotalSeconds
                    (logger.P "simulated now") newTime
                simulatedClockByPartitionId.[grainPartitionGuid] <- newTime
                newTime
        // invoke reminder hook whether time was already ahead or not
        reminderHook.OnClockUpdated logger grainPartitionGuid effectiveNewTime lifeCycleAdapterCollection grainFactory
    )

let private simulatedNowImpl (increment: bool) (GrainPartition grainPartitionGuid) =
        lock lockObj (
            fun _ ->
                lazyInitSimulatedClock grainPartitionGuid
                let now = simulatedClockByPartitionId.[grainPartitionGuid]
                if increment then
                    // increment time by an epsilon (1 microsecond) every time it's queried
                    // so it imitates monotonic passage of time but more deterministic
                    let newNow = now + TimeSpan.FromTicks 100L
                    simulatedClockByPartitionId.[grainPartitionGuid] <- newNow
                    newNow
                else
                    now)

let simulatedNow = simulatedNowImpl (* increment *) true

let simulatedNowNoIncrement = simulatedNowImpl (* increment *) false

let testClockHandler (grainPartition: GrainPartition) (action: Clock) : Task<ResponseVerificationToken> =
    match action with
    | Now replyChannel ->
        replyChannel.Respond (simulatedNow grainPartition) |> Task.FromResult

let overrideInitialSimulatedTime (initialTime: DateTimeOffset) (GrainPartition grainPartitionGuid) =
    lock lockObj (
        fun _ ->
            match simulatedClockByPartitionId.TryGetValue grainPartitionGuid with
            | true, _ ->
                failwith "Simulated time already defined, can't override"
            | false, _ ->
                simulatedClockByPartitionId.Add (grainPartitionGuid, initialTime))

let isSimulatedTimeSet (GrainPartition grainPartitionGuid) =
    lock lockObj (fun _ -> simulatedClockByPartitionId.ContainsKey grainPartitionGuid)
