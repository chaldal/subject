[<AutoOpen; CodecLib.CodecAutoGenerate>]
module SuiteJobs.Types.RecurringJob

open System

type RecurringJobExecution = {
    FiredOn: DateTimeOffset
    Id: JobId
}

[<RequireQualifiedAccess>]
type RecurringJobState =
| Scheduled of DueOn: DateTimeOffset * LastExecution: Option<RecurringJobExecution>
| Fired of NextDueOn: DateTimeOffset * RecurringJobExecution

type RecurringJob = {
    Id: RecurringJobId
    Name: NonemptyString
    // TODO : make separate type for JobInstanceData, at least remove SentOn field, anything else?
    JobInstanceData: JobConstructorCommonData
    CreatedOn: DateTimeOffset
    CronExpression: string
    TimeZoneId: string
    State: RecurringJobState
    HardDelete: bool
}
with
    interface Subject<RecurringJobId> with
        member this.SubjectCreatedOn =
            this.CreatedOn
        member this.SubjectId = this.Id

[<RequireQualifiedAccess>]
type RecurringJobAction =
| TriggerJob
| OnFiredJobTimeout
| OnJobStatusChanged of JobStatus
| Update of CronExpression: string * TimeZoneId: string * JobConstructorCommonData
| HardDelete
with interface LifeAction

[<RequireQualifiedAccess>]
type RecurringJobOpError =
| ScheduleError of Message: string
| NameAlreadyReserved of Name: NonemptyString
with interface OpError

[<RequireQualifiedAccess>]
type RecurringJobConstructor =
| New of Name: NonemptyString * CronExpression: string * TimeZoneId: string * JobConstructorCommonData
with interface Constructor

type RecurringJobLifeEvent = private NoLifeEvent of unit with interface LifeEvent

type RecurringJobNumericIndex = NoNumericIndex<RecurringJobOpError>

[<RequireQualifiedAccess>]
type RecurringJobStringIndex =
| Name of NonemptyString
with
    interface SubjectStringIndex<RecurringJobOpError> with
        member this.Primitive =
            match this with
            | Name name -> UniqueIndexedString (name.Value, RecurringJobOpError.NameAlreadyReserved name)

type RecurringJobSearchIndex = NoSearchIndex
type RecurringJobGeographyIndex = NoGeographyIndex

type RecurringJobIndex() = inherit SubjectIndex<RecurringJobIndex, RecurringJobNumericIndex, RecurringJobStringIndex, RecurringJobSearchIndex, RecurringJobGeographyIndex, RecurringJobOpError>()



////////////////////////////////
// Generated code starts here //
////////////////////////////////

#if !FABLE_COMPILER

open CodecLib

#nowarn "69" // disable Interface implementations should normally be given on the initial declaration of a type


type RecurringJobExecution with
    static member private get_ObjCodec_V1 () =
        codec {
            let! _version = reqWith Codecs.int "__v1" (fun _ -> Some 0)
            and! firedOn = reqWith Codecs.dateTimeOffset "FiredOn" (fun x -> Some x.FiredOn)
            and! id = reqWith codecFor<_, JobId> "Id" (fun x -> Some x.Id)
            return {
                FiredOn = firedOn
                Id = id
             }
        }
    static member get_Codec () = ofObjCodec (RecurringJobExecution.get_ObjCodec_V1 ())


type RecurringJobState with
    static member private get_ObjCodec_AllCases () =
        function
        | Scheduled _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Scheduled _ -> Some 0 | _ -> None)
                and! payload = reqWith (Codecs.tuple2 Codecs.dateTimeOffset (Codecs.option codecFor<_, RecurringJobExecution>)) "Scheduled" (function Scheduled (x1, x2) -> Some (x1, x2) | _ -> None)
                return Scheduled payload
            }
        | Fired _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Fired _ -> Some 0 | _ -> None)
                and! payload = reqWith (Codecs.tuple2 Codecs.dateTimeOffset codecFor<_, RecurringJobExecution>) "Fired" (function Fired (x1, x2) -> Some (x1, x2) | _ -> None)
                return Fired payload
            }
        |> mergeUnionCases
    static member get_Codec () = ofObjCodec (RecurringJobState.get_ObjCodec_AllCases ())


type RecurringJob with
    static member TypeLabel () = "RecurringJob"

    static member private get_ObjCodec_V1 () =
        codec {
            let! _version = reqWith Codecs.int "__v1" (fun _ -> Some 1)
            and! maybeId = optWith codecFor<_, RecurringJobId> "Id" (fun x -> Some x.Id)
            and! name = reqWith codecFor<_, NonemptyString> "Name" (fun x -> Some x.Name)
            and! jobInstanceData = reqWith codecFor<_, JobConstructorCommonData> "JobInstanceData" (fun x -> Some x.JobInstanceData)
            and! createdOn = reqWith Codecs.dateTimeOffset "CreatedOn" (fun x -> Some x.CreatedOn)
            and! cronExpression = reqWith Codecs.string "CronExpression" (fun x -> Some x.CronExpression)
            and! timeZoneId = reqWith Codecs.string "TimeZoneId" (fun x -> Some x.TimeZoneId)
            and! state = reqWith codecFor<_, RecurringJobState> "State" (fun x -> Some x.State)
            and! hardDelete = reqWith Codecs.boolean "HardDelete" (fun x -> Some x.HardDelete)
            return {
                Id = maybeId |> Option.defaultWith (fun () -> RecurringJobId name.Value)
                Name = name
                JobInstanceData = jobInstanceData
                CreatedOn = createdOn
                CronExpression = cronExpression
                TimeZoneId = timeZoneId
                State = state
                HardDelete = hardDelete
             }
        }

    static member private get_ObjCodec () = RecurringJob.get_ObjCodec_V1 ()
    static member get_Codec () = ofObjCodec <| RecurringJob.get_ObjCodec ()
    static member Init (typeLabel: string, _typeParams: _) = initializeInterfaceImplementation<Subject<RecurringJobId>, RecurringJob> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| RecurringJob.get_ObjCodec ())



type RecurringJobAction with
    static member TypeLabel () = "RecurringJobAction"

    static member private get_ObjCodec_AllCases () =
        function
        | TriggerJob ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function TriggerJob -> Some 0 | _ -> None)
                and! _ = reqWith Codecs.unit "TriggerJob" (function TriggerJob -> Some () | _ -> None)
                return TriggerJob
            }
        | OnFiredJobTimeout ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function OnFiredJobTimeout -> Some 0 | _ -> None)
                and! _ = reqWith Codecs.unit "OnFiredJobTimeout" (function OnFiredJobTimeout -> Some () | _ -> None)
                return OnFiredJobTimeout
            }
        | OnJobStatusChanged _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function OnJobStatusChanged _ -> Some 0 | _ -> None)
                and! payload = reqWith codecFor<_, JobStatus> "OnJobStatusChanged" (function OnJobStatusChanged x -> Some x | _ -> None)
                return OnJobStatusChanged payload
            }
        | Update _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Update _ -> Some 0 | _ -> None)
                and! payload = reqWith (Codecs.tuple3 Codecs.string Codecs.string codecFor<_, JobConstructorCommonData>) "Update" (function Update (x1, x2, x3) -> Some (x1, x2, x3) | _ -> None)
                return Update payload
            }
        | HardDelete ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function HardDelete -> Some 0 | _ -> None)
                and! _ = reqWith Codecs.unit "HardDelete" (function HardDelete -> Some () | _ -> None)
                return HardDelete
            }
        |> mergeUnionCases

    static member private get_ObjCodec () = RecurringJobAction.get_ObjCodec_AllCases ()
    static member get_Codec () = ofObjCodec <| RecurringJobAction.get_ObjCodec ()
    static member Init (typeLabel: string, _typeParams: _) = initializeInterfaceImplementation<LifeAction, RecurringJobAction> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| RecurringJobAction.get_ObjCodec ())



type RecurringJobOpError with
    static member TypeLabel () = "RecurringJobOpError"

    static member private get_ObjCodec_AllCases () =
        function
        | ScheduleError _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function ScheduleError _ -> Some 0 | _ -> None)
                and! payload = reqWith Codecs.string "ScheduleError" (function ScheduleError x -> Some x | _ -> None)
                return ScheduleError payload
            }
        | NameAlreadyReserved _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function NameAlreadyReserved _ -> Some 0 | _ -> None)
                and! payload = reqWith codecFor<_, NonemptyString> "NameAlreadyReserved" (function NameAlreadyReserved x -> Some x | _ -> None)
                return NameAlreadyReserved payload
            }
        |> mergeUnionCases

    static member private get_ObjCodec () = RecurringJobOpError.get_ObjCodec_AllCases ()
    static member get_Codec () = ofObjCodec <| RecurringJobOpError.get_ObjCodec ()
    static member Init (typeLabel: string, _typeParams: _) = initializeInterfaceImplementation<OpError, RecurringJobOpError> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| RecurringJobOpError.get_ObjCodec ())



type RecurringJobConstructor with
    static member TypeLabel () = "RecurringJobConstructor"

    static member private get_ObjCodec_AllCases () =
        function
        | New _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function New _ -> Some 0)
                and! payload = reqWith (Codecs.tuple4 codecFor<_, NonemptyString> Codecs.string Codecs.string codecFor<_, JobConstructorCommonData>) "New" (function New (x1, x2, x3, x4) -> Some (x1, x2, x3, x4))
                return New payload
            }
        |> mergeUnionCases

    static member private get_ObjCodec () = RecurringJobConstructor.get_ObjCodec_AllCases ()
    static member get_Codec () = ofObjCodec <| RecurringJobConstructor.get_ObjCodec ()
    static member Init (typeLabel: string, _typeParams: _) = initializeInterfaceImplementation<Constructor, RecurringJobConstructor> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| RecurringJobConstructor.get_ObjCodec ())



type RecurringJobLifeEvent with
    static member TypeLabel () = "Jobs_RecurringJobLifeEvent"

    static member private get_ObjCodec_AllCases () =
        function
        | NoLifeEvent _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function NoLifeEvent _ -> Some 0)
                and! payload = reqWith Codecs.unit "NoLifeEvent" (function NoLifeEvent x -> Some x)
                return NoLifeEvent payload
            }
        |> mergeUnionCases

    static member private get_ObjCodec () = RecurringJobLifeEvent.get_ObjCodec_AllCases ()
    static member get_Codec () = ofObjCodec <| RecurringJobLifeEvent.get_ObjCodec ()
    static member Init (typeLabel: string, _typeParams: _) = initializeInterfaceImplementation<LifeEvent, RecurringJobLifeEvent> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| RecurringJobLifeEvent.get_ObjCodec ())

#endif // !FABLE_COMPILER