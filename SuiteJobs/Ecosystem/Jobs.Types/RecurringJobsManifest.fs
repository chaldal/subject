[<AutoOpen; CodecLib.CodecAutoGenerate>]
module SuiteJobs.Types.RecurringJobsManifest

open System

type RecurringJobsManifestId = SingletoRecurringJobsManifestId
with
    interface SubjectId with
        member this.IdString =
            "$"

type ManifestedRecurringJob = {
    Name: NonemptyString
    CronExpression: string
    TimeZoneId: string
    JobInstanceData: JobConstructorCommonData
}
with
    interface IKeyed<NonemptyString> with
        member this.Key = this.Name

type RecurringJobsManifest = {
    CreatedOn: DateTimeOffset
}
with
    interface Subject<RecurringJobsManifestId> with
        member this.SubjectCreatedOn =
            this.CreatedOn

        member this.SubjectId =
            SingletoRecurringJobsManifestId

[<RequireQualifiedAccess>]
type RecurringJobsManifestAction =
| Apply of KeyedSet<NonemptyString, ManifestedRecurringJob>
with interface LifeAction

[<RequireQualifiedAccess>]
type RecurringJobsManifestOpError = private NoOpError of unit
with interface OpError

type RecurringJobsManifestConstructor =
| SingletonRecurringJobsManifestConstructor
with interface Constructor

type RecurringJobsManifestLifeEvent = private NoLifeEvent of unit with interface LifeEvent

type RecurringJobsManifestNumericIndex = NoNumericIndex<RecurringJobsManifestOpError>
type RecurringJobsManifestStringIndex = NoStringIndex<RecurringJobsManifestOpError>
type RecurringJobsManifestSearchIndex = NoSearchIndex
type RecurringJobsManifestGeographyIndex = NoGeographyIndex

type RecurringJobsManifestIndex() = inherit SubjectIndex<RecurringJobsManifestIndex, RecurringJobsManifestNumericIndex, RecurringJobsManifestStringIndex, RecurringJobsManifestSearchIndex, RecurringJobsManifestGeographyIndex, RecurringJobsManifestOpError>()



////////////////////////////////
// Generated code starts here //
////////////////////////////////

#if !FABLE_COMPILER

open CodecLib

#nowarn "69" // disable Interface implementations should normally be given on the initial declaration of a type


type RecurringJobsManifestId with
    static member TypeLabel () = "Jobs_RecurringJobsManifestId"

    static member private get_ObjCodec_AllCases () =
        function
        | SingletoRecurringJobsManifestId ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function SingletoRecurringJobsManifestId -> Some 0)
                and! _ = reqWith Codecs.unit "SingletoRecurringJobsManifestId" (function SingletoRecurringJobsManifestId -> Some ())
                return SingletoRecurringJobsManifestId
            }
        |> mergeUnionCases

    static member private get_ObjCodec () = RecurringJobsManifestId.get_ObjCodec_AllCases ()
    static member get_Codec () = ofObjCodec <| RecurringJobsManifestId.get_ObjCodec ()
    static member Init (typeLabel: string, _typeParams: _) = initializeInterfaceImplementation<SubjectId, RecurringJobsManifestId> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| RecurringJobsManifestId.get_ObjCodec ())



type ManifestedRecurringJob with
    static member private get_ObjCodec_V1 () =
        codec {
            let! _version = reqWith Codecs.int "__v1" (fun _ -> Some 0)
            and! name = reqWith codecFor<_, NonemptyString> "Name" (fun x -> Some x.Name)
            and! cronExpression = reqWith Codecs.string "CronExpression" (fun x -> Some x.CronExpression)
            and! timeZoneId = reqWith Codecs.string "TimeZoneId" (fun x -> Some x.TimeZoneId)
            and! jobInstanceData = reqWith codecFor<_, JobConstructorCommonData> "JobInstanceData" (fun x -> Some x.JobInstanceData)
            return {
                Name = name
                CronExpression = cronExpression
                TimeZoneId = timeZoneId
                JobInstanceData = jobInstanceData
             }
        }
    static member get_Codec () = ofObjCodec (ManifestedRecurringJob.get_ObjCodec_V1 ())


type RecurringJobsManifest with
    static member TypeLabel () = "RecurringJobsManifest"

    static member private get_ObjCodec_V1 () =
        codec {
            let! _version = reqWith Codecs.int "__v1" (fun _ -> Some 0)
            and! createdOn = reqWith Codecs.dateTimeOffset "CreatedOn" (fun x -> Some x.CreatedOn)
            return {
                CreatedOn = createdOn
             }
        }

    static member private get_ObjCodec () = RecurringJobsManifest.get_ObjCodec_V1 ()
    static member get_Codec () = ofObjCodec <| RecurringJobsManifest.get_ObjCodec ()
    static member Init (typeLabel: string, _typeParams: _) = initializeInterfaceImplementation<Subject<RecurringJobsManifestId>, RecurringJobsManifest> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| RecurringJobsManifest.get_ObjCodec ())



type RecurringJobsManifestAction with
    static member TypeLabel () = "RecurringJobsManifestAction"

    static member private get_ObjCodec_AllCases () =
        function
        | Apply _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Apply _ -> Some 0)
                and! payload = reqWith (KeyedSet.codec codecFor<_, ManifestedRecurringJob>) "Apply" (function Apply x -> Some x)
                return Apply payload
            }
        |> mergeUnionCases

    static member private get_ObjCodec () = RecurringJobsManifestAction.get_ObjCodec_AllCases ()
    static member get_Codec () = ofObjCodec <| RecurringJobsManifestAction.get_ObjCodec ()
    static member Init (typeLabel: string, _typeParams: _) = initializeInterfaceImplementation<LifeAction, RecurringJobsManifestAction> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| RecurringJobsManifestAction.get_ObjCodec ())



type RecurringJobsManifestOpError with
    static member TypeLabel () = "RecurringJobsManifestOpError"

    static member private get_ObjCodec_AllCases () =
        function
        | NoOpError _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function NoOpError _ -> Some 0)
                and! payload = reqWith Codecs.unit "NoOpError" (function NoOpError x -> Some x)
                return NoOpError payload
            }
        |> mergeUnionCases

    static member private get_ObjCodec () = RecurringJobsManifestOpError.get_ObjCodec_AllCases ()
    static member get_Codec () = ofObjCodec <| RecurringJobsManifestOpError.get_ObjCodec ()
    static member Init (typeLabel: string, _typeParams: _) = initializeInterfaceImplementation<OpError, RecurringJobsManifestOpError> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| RecurringJobsManifestOpError.get_ObjCodec ())



type RecurringJobsManifestConstructor with
    static member TypeLabel () = "RecurringJobsManifestConstructor"

    static member private get_ObjCodec_AllCases () =
        function
        | SingletonRecurringJobsManifestConstructor ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function SingletonRecurringJobsManifestConstructor -> Some 0)
                and! _ = reqWith Codecs.unit "SingletonRecurringJobsManifestConstructor" (function SingletonRecurringJobsManifestConstructor -> Some ())
                return SingletonRecurringJobsManifestConstructor
            }
        |> mergeUnionCases

    static member private get_ObjCodec () = RecurringJobsManifestConstructor.get_ObjCodec_AllCases ()
    static member get_Codec () = ofObjCodec <| RecurringJobsManifestConstructor.get_ObjCodec ()
    static member Init (typeLabel: string, _typeParams: _) = initializeInterfaceImplementation<Constructor, RecurringJobsManifestConstructor> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| RecurringJobsManifestConstructor.get_ObjCodec ())



type RecurringJobsManifestLifeEvent with
    static member TypeLabel () = "Jobs_RecurringJobsManifestLifeEvent"

    static member private get_ObjCodec_AllCases () =
        function
        | NoLifeEvent _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function NoLifeEvent _ -> Some 0)
                and! payload = reqWith Codecs.unit "NoLifeEvent" (function NoLifeEvent x -> Some x)
                return NoLifeEvent payload
            }
        |> mergeUnionCases

    static member private get_ObjCodec () = RecurringJobsManifestLifeEvent.get_ObjCodec_AllCases ()
    static member get_Codec () = ofObjCodec <| RecurringJobsManifestLifeEvent.get_ObjCodec ()
    static member Init (typeLabel: string, _typeParams: _) = initializeInterfaceImplementation<LifeEvent, RecurringJobsManifestLifeEvent> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| RecurringJobsManifestLifeEvent.get_ObjCodec ())

#endif // !FABLE_COMPILER