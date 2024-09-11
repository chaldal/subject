[<AutoOpen; CodecLib.CodecAutoGenerate>]
module SuiteJobs.Types.Dispatcher

open System

type DispatcherSettings = {
    Queues: NonemptySet<NonemptyString>
    MaxProcessingJobs: UnsignedInteger // can be set to zero, to disable the dispatcher
    HotPollIfJobsCountAtOrBelow: UnsignedInteger // On job processed, enqueue next Poll if remaining jobs count is at or below (useful for faster jobs)
    HotPollIfTimeSinceLastPollAtLeast: TimeSpan // enqueue next Poll if after specified delay if there are empty spots (useful for slow running jobs)
    MaxContinuousPollRetries: uint32 // how many times to retry Poll after concurrency error before ceding to timer
}

// We need two modes of polling: timer and side effect,
// because immediately due timers still have much higher latency than enqueued action.
[<RequireQualifiedAccess>]
type PollTrigger =
| Hot of SideEffectRequestedOn: DateTimeOffset
| Cold of TimerScheduledOn: DateTimeOffset
| HotButSlow of TimerIfNoJobsProcessedUntil: DateTimeOffset

type DispatcherPollInfo = {
    // streak is a number of consecutive fruitful polls, whether by timer or by side effect
    StreakCount: uint32
    ScheduledTrigger: Option<PollTrigger>
    LastPolledOn: DateTimeOffset
    LastJobAddedOn: DateTimeOffset
}

type Dispatcher = {
    Name: NonemptyString
    CreatedOn: DateTimeOffset
    Settings: DispatcherSettings
    ProcessingJobs: Set<JobId>
    PollInfo: DispatcherPollInfo
    HardDelete: bool
}
with
    interface Subject<DispatcherId> with
        member this.SubjectCreatedOn =
            this.CreatedOn

        member this.SubjectId =
            DispatcherId this.Name

    member this.Id = (this :> Subject<_>).SubjectId

[<RequireQualifiedAccess>]
type DispatcherAction =
| Poll of Retry: uint32 * PollTrigger
| OnProcessingCompleted of JobId
| OnJobCannotBeStarted of JobId
| ChangeSettings of DispatcherSettings
| HardDelete
with interface LifeAction

[<RequireQualifiedAccess>]
type DispatcherOpError =
| JobAlreadyReserved of JobId
| InvalidSettings of Why: string
with interface OpError

[<RequireQualifiedAccess>]
type DispatcherConstructor =
| New of Name: NonemptyString * DispatcherSettings
with interface Constructor

type DispatcherLifeEvent = private NoLifeEvent of unit with interface LifeEvent

type DispatcherNumericIndex = NoNumericIndex<DispatcherOpError>

[<RequireQualifiedAccess>]
type DispatcherStringIndex =
| Queue of NonemptyString
| Job of JobId
with
    interface SubjectStringIndex<DispatcherOpError> with
        member this.Primitive =
            match this with
            | Queue name -> IndexedString name.Value
            | Job (JobId jobIdStr as jobId) -> UniqueIndexedString (jobIdStr, DispatcherOpError.JobAlreadyReserved jobId)

type DispatcherSearchIndex = NoSearchIndex
type DispatcherGeographyIndex = NoGeographyIndex

type DispatcherIndex() = inherit SubjectIndex<DispatcherIndex, DispatcherNumericIndex, DispatcherStringIndex, DispatcherSearchIndex, DispatcherGeographyIndex, DispatcherOpError>()



////////////////////////////////
// Generated code starts here //
////////////////////////////////

#if !FABLE_COMPILER

open CodecLib

#nowarn "69" // disable Interface implementations should normally be given on the initial declaration of a type


type DispatcherSettings with
    static member private get_ObjCodec_V1 () =
        codec {
            let! _version = reqWith Codecs.int "__v1" (fun _ -> Some 1)
            and! queues = reqWith (NonemptySet.codec codecFor<_, NonemptyString>) "Queues" (fun x -> Some x.Queues)
            and! maxProcessingJobs = reqWith codecFor<_, UnsignedInteger> "MaxProcessingJobs" (fun x -> Some x.MaxProcessingJobs)
            and! hotPollIfJobsCountAtOrBelow = reqWith codecFor<_, UnsignedInteger> "PollThreshold" (fun x -> Some x.HotPollIfJobsCountAtOrBelow)
            and! maybeHotPollIfTimeSinceLastPollAtLeast = optWith Codecs.timeSpan "PollTimeThreshold" (fun x -> Some x.HotPollIfTimeSinceLastPollAtLeast)
            and! maxContinuousPollRetries = reqWith Codecs.uint32 "MaxContinuousRetries" (fun x -> Some x.MaxContinuousPollRetries)
            return {
                Queues = queues
                MaxProcessingJobs = maxProcessingJobs
                HotPollIfJobsCountAtOrBelow =  hotPollIfJobsCountAtOrBelow
                HotPollIfTimeSinceLastPollAtLeast = maybeHotPollIfTimeSinceLastPollAtLeast |> Option.defaultValue (TimeSpan.FromSeconds 10.)
                MaxContinuousPollRetries = maxContinuousPollRetries
             }
        }
    static member get_Codec () = ofObjCodec (DispatcherSettings.get_ObjCodec_V1 ())


type PollTrigger with
    static member private get_ObjCodec_AllCases () =
        function
        | Hot _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Hot _ -> Some 0 | _ -> None)
                and! payload = reqWith Codecs.dateTimeOffset "Hot" (function Hot x -> Some x | _ -> None)
                return Hot payload
            }
        | Cold _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Cold _ -> Some 0 | _ -> None)
                and! payload = reqWith Codecs.dateTimeOffset "Cold" (function Cold x -> Some x | _ -> None)
                return Cold payload
            }
        | HotButSlow _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function HotButSlow _ -> Some 0 | _ -> None)
                and! payload = reqWith Codecs.dateTimeOffset "HotButSlow" (function HotButSlow x -> Some x | _ -> None)
                return HotButSlow payload
            }
        |> mergeUnionCases
    static member get_Codec () = ofObjCodec (PollTrigger.get_ObjCodec_AllCases ())


type DispatcherPollInfo with
    static member private get_ObjCodec_V1 () =
        codec {
            let! _version = reqWith Codecs.int "__v1" (fun _ -> Some 0)
            and! streakCount = reqWith Codecs.uint32 "StreakCount" (fun x -> Some x.StreakCount)
            and! scheduledTrigger = reqWith (Codecs.option codecFor<_, PollTrigger>) "ScheduledTrigger" (fun x -> Some x.ScheduledTrigger)
            and! lastPolledOn = reqWith Codecs.dateTimeOffset "LastPolledOn" (fun x -> Some x.LastPolledOn)
            and! lastJobAddedOn = reqWith Codecs.dateTimeOffset "LastJobAddedOn" (fun x -> Some x.LastJobAddedOn)
            return {
                StreakCount = streakCount
                ScheduledTrigger = scheduledTrigger
                LastPolledOn = lastPolledOn
                LastJobAddedOn = lastJobAddedOn
             }
        }
    static member get_Codec () = ofObjCodec (DispatcherPollInfo.get_ObjCodec_V1 ())


type Dispatcher with
    static member TypeLabel () = "Dispatcher"

    static member private get_ObjCodec_V1 () =
        codec {
            let! _version = reqWith Codecs.int "__v1" (fun _ -> Some 1)
            and! name = reqWith codecFor<_, NonemptyString> "Name" (fun x -> Some x.Name)
            and! createdOn = reqWith Codecs.dateTimeOffset "CreatedOn" (fun x -> Some x.CreatedOn)
            and! settings = reqWith codecFor<_, DispatcherSettings> "Settings" (fun x -> Some x.Settings)
            and! processingJobs = reqWith (Codecs.set codecFor<_, JobId>) "ProcessingJobs" (fun x -> Some x.ProcessingJobs)
            and! pollInfo = reqWith codecFor<_, DispatcherPollInfo> "PollInfo" (fun x -> Some x.PollInfo)
            and! maybeHardDelete = optWith Codecs.boolean "HardDelete" (fun x -> Some x.HardDelete)
            return {
                Name = name
                CreatedOn = createdOn
                Settings = settings
                ProcessingJobs = processingJobs
                PollInfo = pollInfo
                HardDelete = maybeHardDelete |> Option.defaultValue false
             }
        }

    static member private get_ObjCodec () = Dispatcher.get_ObjCodec_V1 ()
    static member get_Codec () = ofObjCodec <| Dispatcher.get_ObjCodec ()
    static member Init (typeLabel: string, _typeParams: _) = initializeInterfaceImplementation<Subject<DispatcherId>, Dispatcher> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| Dispatcher.get_ObjCodec ())



type DispatcherAction with
    static member TypeLabel () = "DispatcherAction"

    static member private get_ObjCodec_AllCases () =
        function
        | Poll _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Poll _ -> Some 0 | _ -> None)
                and! payload = reqWith (Codecs.tuple2 Codecs.uint32 codecFor<_, PollTrigger>) "Poll" (function Poll (x1, x2) -> Some (x1, x2) | _ -> None)
                return Poll payload
            }
        | OnProcessingCompleted _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function OnProcessingCompleted _ -> Some 0 | _ -> None)
                and! payload = reqWith codecFor<_, JobId> "OnProcessingCompleted" (function OnProcessingCompleted x -> Some x | _ -> None)
                return OnProcessingCompleted payload
            }
        | OnJobCannotBeStarted _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function OnJobCannotBeStarted _ -> Some 0 | _ -> None)
                and! payload = reqWith codecFor<_, JobId> "OnJobCannotBeStarted" (function OnJobCannotBeStarted x -> Some x | _ -> None)
                return OnJobCannotBeStarted payload
            }
        | ChangeSettings _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function ChangeSettings _ -> Some 0 | _ -> None)
                and! payload = reqWith codecFor<_, DispatcherSettings> "ChangeSettings" (function ChangeSettings x -> Some x | _ -> None)
                return ChangeSettings payload
            }
        | HardDelete ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function HardDelete -> Some 0 | _ -> None)
                and! _ = reqWith Codecs.unit "HardDelete" (function HardDelete -> Some () | _ -> None)
                return HardDelete
            }
        |> mergeUnionCases

    static member private get_ObjCodec () = DispatcherAction.get_ObjCodec_AllCases ()
    static member get_Codec () = ofObjCodec <| DispatcherAction.get_ObjCodec ()
    static member Init (typeLabel: string, _typeParams: _) = initializeInterfaceImplementation<LifeAction, DispatcherAction> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| DispatcherAction.get_ObjCodec ())



type DispatcherOpError with
    static member TypeLabel () = "DispatcherOpError"

    static member private get_ObjCodec_AllCases () =
        function
        | JobAlreadyReserved _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function JobAlreadyReserved _ -> Some 0 | _ -> None)
                and! payload = reqWith codecFor<_, JobId> "JobAlreadyReserved" (function JobAlreadyReserved x -> Some x | _ -> None)
                return JobAlreadyReserved payload
            }
        | InvalidSettings _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function InvalidSettings _ -> Some 0 | _ -> None)
                and! payload = reqWith Codecs.string "InvalidSettings" (function InvalidSettings x -> Some x | _ -> None)
                return InvalidSettings payload
            }
        |> mergeUnionCases

    static member private get_ObjCodec () = DispatcherOpError.get_ObjCodec_AllCases ()
    static member get_Codec () = ofObjCodec <| DispatcherOpError.get_ObjCodec ()
    static member Init (typeLabel: string, _typeParams: _) = initializeInterfaceImplementation<OpError, DispatcherOpError> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| DispatcherOpError.get_ObjCodec ())



type DispatcherConstructor with
    static member TypeLabel () = "DispatcherConstructor"

    static member private get_ObjCodec_AllCases () =
        function
        | New _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function New _ -> Some 0)
                and! payload = reqWith (Codecs.tuple2 codecFor<_, NonemptyString> codecFor<_, DispatcherSettings>) "New" (function New (x1, x2) -> Some (x1, x2))
                return New payload
            }
        |> mergeUnionCases

    static member private get_ObjCodec () = DispatcherConstructor.get_ObjCodec_AllCases ()
    static member get_Codec () = ofObjCodec <| DispatcherConstructor.get_ObjCodec ()
    static member Init (typeLabel: string, _typeParams: _) = initializeInterfaceImplementation<Constructor, DispatcherConstructor> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| DispatcherConstructor.get_ObjCodec ())



type DispatcherLifeEvent with
    static member TypeLabel () = "Jobs_DispatcherLifeEvent"

    static member private get_ObjCodec_AllCases () =
        function
        | NoLifeEvent _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function NoLifeEvent _ -> Some 0)
                and! payload = reqWith Codecs.unit "NoLifeEvent" (function NoLifeEvent x -> Some x)
                return NoLifeEvent payload
            }
        |> mergeUnionCases

    static member private get_ObjCodec () = DispatcherLifeEvent.get_ObjCodec_AllCases ()
    static member get_Codec () = ofObjCodec <| DispatcherLifeEvent.get_ObjCodec ()
    static member Init (typeLabel: string, _typeParams: _) = initializeInterfaceImplementation<LifeEvent, DispatcherLifeEvent> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| DispatcherLifeEvent.get_ObjCodec ())

#endif // !FABLE_COMPILER