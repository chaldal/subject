[<AutoOpen; CodecLib.CodecAutoGenerate>]
module SuiteJobs.Types.Common

// JobId and BatchId format is loose, it should be smth unique within the system and supplied by client.
// To document decision, why we don't just generate incremental Id on host side?
// The reason is to keep things simple. Sequential ids are tricky because:
//  - Client should be able to plan job requests even if SuiteJobs is offline, i.e. client must own the sequence of Ids
//  - however SuiteJobs must generate unique job ids too - for RecurringJob and Batch subjects (they just do tiny guid strings)
//    so it'd need to tap into the client's sequence, but it quickly becomes a rabbit hole.
type JobId = JobId of string
with
    interface SubjectId with
        member this.IdString =
            let (JobId key) = this
            key

type BatchId = BatchId of string
with
    interface SubjectId with
        member this.IdString =
            let (BatchId key) = this
            key

type RecurringJobId = RecurringJobId of string
with
    interface SubjectId with
        member this.IdString =
            let (RecurringJobId key) = this
            key

type DispatcherId = DispatcherId of Name: NonemptyString
with
    interface SubjectId with
        member this.IdString =
            let (DispatcherId name) = this
            name.Value

/// Information required to rehydrate and run the job
type JobPayload = {
    Type: string
    Method: string
    ParameterTypes: string
    Arguments: string
    CustomContext: string
}

[<RequireQualifiedAccess>]
type JobStatus =
| Unfinished
| Finished of Succeeded: bool

[<RequireQualifiedAccess>]
type AwaitingForJobStatus =
| OnlySucceeded
| AnyFinishedState // Succeeded, Deleted

[<RequireQualifiedAccess>]
type BatchStatus = // similar to job status but it's incidental
| Unfinished
| Finished of Succeeded: bool

[<RequireQualifiedAccess>]
type AwaitingForBatchStatus =
| OnlySucceeded
| AnyFinishedState

[<RequireQualifiedAccess>]
type PlacedBy = // who created a Placeholder job or batch
| Job of JobId
| Batch of BatchId
| Client



////////////////////////////////
// Generated code starts here //
////////////////////////////////

#if !FABLE_COMPILER

open CodecLib

#nowarn "69" // disable Interface implementations should normally be given on the initial declaration of a type


type JobId with
    static member TypeLabel () = "Jobs_JobId"

    static member private get_ObjCodec_AllCases () =
        function
        | JobId _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function JobId _ -> Some 0)
                and! payload = reqWith Codecs.string "JobId" (function JobId x -> Some x)
                return JobId payload
            }
        |> mergeUnionCases

    static member private get_ObjCodec () = JobId.get_ObjCodec_AllCases ()
    static member get_Codec () = ofObjCodec <| JobId.get_ObjCodec ()
    static member Init (typeLabel: string, _typeParams: _) = initializeInterfaceImplementation<SubjectId, JobId> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| JobId.get_ObjCodec ())



type BatchId with
    static member TypeLabel () = "Jobs_BatchId"

    static member private get_ObjCodec_AllCases () =
        function
        | BatchId _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function BatchId _ -> Some 0)
                and! payload = reqWith Codecs.string "BatchId" (function BatchId x -> Some x)
                return BatchId payload
            }
        |> mergeUnionCases

    static member private get_ObjCodec () = BatchId.get_ObjCodec_AllCases ()
    static member get_Codec () = ofObjCodec <| BatchId.get_ObjCodec ()
    static member Init (typeLabel: string, _typeParams: _) = initializeInterfaceImplementation<SubjectId, BatchId> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| BatchId.get_ObjCodec ())



type RecurringJobId with
    static member TypeLabel () = "Jobs_RecurringJobId"

    static member private get_ObjCodec_AllCases () =
        function
        | RecurringJobId _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function RecurringJobId _ -> Some 0)
                and! payload = reqWith Codecs.string "RecurringJobId" (function RecurringJobId x -> Some x)
                return RecurringJobId payload
            }
        |> mergeUnionCases

    static member private get_ObjCodec () = RecurringJobId.get_ObjCodec_AllCases ()
    static member get_Codec () = ofObjCodec <| RecurringJobId.get_ObjCodec ()
    static member Init (typeLabel: string, _typeParams: _) = initializeInterfaceImplementation<SubjectId, RecurringJobId> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| RecurringJobId.get_ObjCodec ())



type DispatcherId with
    static member TypeLabel () = "Jobs_DispatcherId"

    static member private get_ObjCodec_AllCases () =
        function
        | DispatcherId _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function DispatcherId _ -> Some 0)
                and! payload = reqWith codecFor<_, NonemptyString> "DispatcherId" (function DispatcherId x -> Some x)
                return DispatcherId payload
            }
        |> mergeUnionCases

    static member private get_ObjCodec () = DispatcherId.get_ObjCodec_AllCases ()
    static member get_Codec () = ofObjCodec <| DispatcherId.get_ObjCodec ()
    static member Init (typeLabel: string, _typeParams: _) = initializeInterfaceImplementation<SubjectId, DispatcherId> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| DispatcherId.get_ObjCodec ())



type JobPayload with
    static member private get_ObjCodec_V1 () =
        codec {
            let! _version = reqWith Codecs.int "__v1" (fun _ -> Some 0)
            and! ``type`` = reqWith Codecs.string "Type" (fun x -> Some x.Type)
            and! method = reqWith Codecs.string "Method" (fun x -> Some x.Method)
            and! parameterTypes = reqWith Codecs.string "ParameterTypes" (fun x -> Some x.ParameterTypes)
            and! arguments = reqWith Codecs.string "Arguments" (fun x -> Some x.Arguments)
            and! customContext = reqWith Codecs.string "CustomContext" (fun x -> Some x.CustomContext)
            return {
                Type = ``type``
                Method = method
                ParameterTypes = parameterTypes
                Arguments = arguments
                CustomContext = customContext
             }
        }
    static member get_Codec () = ofObjCodec (JobPayload.get_ObjCodec_V1 ())


type JobStatus with
    static member private get_ObjCodec_AllCases () =
        function
        | Unfinished ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Unfinished -> Some 0 | _ -> None)
                and! _ = reqWith Codecs.unit "Unfinished" (function Unfinished -> Some () | _ -> None)
                return Unfinished
            }
        | Finished _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Finished _ -> Some 0 | _ -> None)
                and! payload = reqWith Codecs.boolean "Finished" (function Finished x -> Some x | _ -> None)
                return Finished payload
            }
        |> mergeUnionCases
    static member get_Codec () = ofObjCodec (JobStatus.get_ObjCodec_AllCases ())


type AwaitingForJobStatus with
    static member private get_ObjCodec_AllCases () =
        function
        | OnlySucceeded ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function OnlySucceeded -> Some 0 | _ -> None)
                and! _ = reqWith Codecs.unit "OnlySucceeded" (function OnlySucceeded -> Some () | _ -> None)
                return OnlySucceeded
            }
        | AnyFinishedState ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function AnyFinishedState -> Some 0 | _ -> None)
                and! _ = reqWith Codecs.unit "AnyFinishedState" (function AnyFinishedState -> Some () | _ -> None)
                return AnyFinishedState
            }
        |> mergeUnionCases
    static member get_Codec () = ofObjCodec (AwaitingForJobStatus.get_ObjCodec_AllCases ())


type BatchStatus with
    static member private get_ObjCodec_AllCases () =
        function
        | Unfinished ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Unfinished -> Some 0 | _ -> None)
                and! _ = reqWith Codecs.unit "Unfinished" (function Unfinished -> Some () | _ -> None)
                return Unfinished
            }
        | Finished _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Finished _ -> Some 0 | _ -> None)
                and! payload = reqWith Codecs.boolean "Finished" (function Finished x -> Some x | _ -> None)
                return Finished payload
            }
        |> mergeUnionCases
    static member get_Codec () = ofObjCodec (BatchStatus.get_ObjCodec_AllCases ())


type AwaitingForBatchStatus with
    static member private get_ObjCodec_AllCases () =
        function
        | OnlySucceeded ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function OnlySucceeded -> Some 0 | _ -> None)
                and! _ = reqWith Codecs.unit "OnlySucceeded" (function OnlySucceeded -> Some () | _ -> None)
                return OnlySucceeded
            }
        | AnyFinishedState ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function AnyFinishedState -> Some 0 | _ -> None)
                and! _ = reqWith Codecs.unit "AnyFinishedState" (function AnyFinishedState -> Some () | _ -> None)
                return AnyFinishedState
            }
        |> mergeUnionCases
    static member get_Codec () = ofObjCodec (AwaitingForBatchStatus.get_ObjCodec_AllCases ())


type PlacedBy with
    static member private get_ObjCodec_AllCases () =
        function
        | Job _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Job _ -> Some 0 | _ -> None)
                and! payload = reqWith codecFor<_, JobId> "Job" (function Job x -> Some x | _ -> None)
                return Job payload
            }
        | Batch _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Batch _ -> Some 0 | _ -> None)
                and! payload = reqWith codecFor<_, BatchId> "Batch" (function Batch x -> Some x | _ -> None)
                return Batch payload
            }
        | Client ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Client -> Some 0 | _ -> None)
                and! _ = reqWith Codecs.unit "Client" (function Client -> Some () | _ -> None)
                return Client
            }
        |> mergeUnionCases
    static member get_Codec () = ofObjCodec (PlacedBy.get_ObjCodec_AllCases ())
#endif // !FABLE_COMPILER