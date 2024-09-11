[<AutoOpen; CodecLib.CodecAutoGenerate>]
module SuiteJobs.Types.Batch

open System

[<RequireQualifiedAccess>]
type JobProgress =
| Unfinished of JobId
| Finished of JobId * Succeeded: bool * FinishedOn: DateTimeOffset
with
    interface IKeyed<JobId> with
        member this.Key =
            match this with
            | Unfinished jobId
            | Finished (jobId, _, _) -> jobId

type SequentialJobsParams = {
    AwaitingForJobStatus: AwaitingForJobStatus
    NumberOfThreads: PositiveInteger
}

[<RequireQualifiedAccess>]
type BatchJobsToConstruct =
/// Convenience option to create jobs which run sequentially (each next job is Awaiting previous)
| Sequential of List<JobId * JobConstructorCommonData> * SequentialJobsParams
/// Convenience option to create jobs which can run in parallel
| Parallel of List<JobId * JobConstructorCommonData>
/// Flexible option which shifts responsibility of job creation to the client side. e.g. to create arbitrary orchestration pattern or to avoid bloated ctor payload for a large batch.
/// Jobs can be constructed either before or after the batch (then batch will attempt to create placeholder jobs)
| Placeholders of List<JobId>

let private redactBatchJobsToConstructIfLarge (jobsToConstruct: BatchJobsToConstruct) : Option<BatchJobsToConstruct> =
    let capSizeOfListOfJobs (listOfJobs: List<JobId * JobConstructorCommonData>) : Option<_> =
        match listOfJobs with
        | [] | [_] | [_; _] -> None
        | x1 :: ((jobId2, jobData2) :: _ as rest) ->
            let remainingCount = rest.Length - 1
            let remainingJobOrJobs = if remainingCount = 1 then "job" else "jobs"
            x1 :: (jobId2, { jobData2 with DisplayName = $"(... and %d{remainingCount} more %s{remainingJobOrJobs}) %s{jobData2.DisplayName.Value}" |> NonemptyString.ofLiteral } ) :: []
            |> Some
    match jobsToConstruct with // redact to ensure sensible json size in audit log
    | BatchJobsToConstruct.Sequential (listOfJobs, sequentialJobParams) ->
        capSizeOfListOfJobs listOfJobs |> Option.map (fun cappedListOfJobs -> BatchJobsToConstruct.Sequential (cappedListOfJobs, sequentialJobParams))
    | BatchJobsToConstruct.Parallel listOfJobs ->
        capSizeOfListOfJobs listOfJobs |> Option.map BatchJobsToConstruct.Parallel
    | BatchJobsToConstruct.Placeholders _ -> None

[<RequireQualifiedAccess>]
type BatchParent =
| Batch of BatchId * AwaitingForBatchStatus
 // TODO: support await Job when needed, it's not hard

[<RequireQualifiedAccess>]
// This status drives no business logic, only IndexBatchState which is used for Dashboard, we can really do without it otherwise.
// More important things like BatchStatus are derived base on JobsProgress
type BatchActivationStatus =
| Awaiting of BatchParent
| Activated of On: DateTimeOffset * WasAwaiting: Option<BatchParent>

[<CodecLib.SkipCodecAutoGenerate>]
[<RequireQualifiedAccess>]
type IndexBatchState = // imitates Hangfire states for dashboard
| AwaitingBatch
| AwaitingJob
| Started
| Finished of Succeeded: bool
| Cancelled

type BatchBody = {
    Description: Option<NonemptyString>
    JobsProgress: KeyedSet<JobId, JobProgress>
    // CancelRequestedOn does not indicate or guarantee that all jobs will be deleted, it's only to avoid multiple cancellation requests
    CancelRequestedOn: Option<DateTimeOffset>
    ActivationStatus: BatchActivationStatus
    // captured on client side, to know how long it took it to relay the message to the backend
    SentOn: DateTimeOffset
    PlaceholderFilledOn: Option<DateTimeOffset>
}
with
    member this.Parent =
        match this.ActivationStatus with
        | BatchActivationStatus.Awaiting parent -> Some parent
        | BatchActivationStatus.Activated (_, maybeParent) -> maybeParent

type BatchBodyPlaceholder = {
    PlacedBy: PlacedBy
    // unlike in proper BatchBody, placeholder CancelRequestedOn guarantees that batch will detele all jobs upon FillPlaceholder
    CancelRequestedOn: Option<DateTimeOffset>
}

[<RequireQualifiedAccess>]
type BatchBodyVariant =
| Proper of BatchBody
// if awaiting batch was constructed prior to its parent, it will construct a placeholder to subscribe to
| Placeholder of BatchBodyPlaceholder

type Batch = {
    Id: BatchId
    CreatedOn: DateTimeOffset
    Body: BatchBodyVariant
}
with
    interface Subject<BatchId> with
        member this.SubjectCreatedOn =
            this.CreatedOn
        member this.SubjectId =
            this.Id

    member this.Status =
        match this.Body with
        | BatchBodyVariant.Placeholder placeholder ->
            match placeholder.CancelRequestedOn with
            | None -> BatchStatus.Unfinished
            | Some _ ->
                // cancelled placeholder still considered Unfinished, because upon FillPlaceholder it can still succeed! e.g. if batch is empty
                // also it allows to introduce "UndoCancel" action if need be
                BatchStatus.Unfinished
        | BatchBodyVariant.Proper body ->
            body.JobsProgress.Values
            |> Seq.fold (fun status progress ->
                match status, progress with
                | BatchStatus.Unfinished, _
                | BatchStatus.Finished _, JobProgress.Unfinished _ ->
                     BatchStatus.Unfinished
                | BatchStatus.Finished isSoFarSuccess, JobProgress.Finished (_, alsoSuccess, _) ->
                    BatchStatus.Finished (isSoFarSuccess && alsoSuccess))
                    // initial value is important, it's a default status of empty batch (considered Succeeded)
                    (BatchStatus.Finished (* success *) true)

    member this.IndexBatchState : Option<IndexBatchState> =
        match this.Body with
        | BatchBodyVariant.Proper body ->
            match this.Status, body.ActivationStatus with
            | BatchStatus.Finished (* success *) true, _ ->
                IndexBatchState.Finished (* success *) true
            | BatchStatus.Finished (* success *) false, _ ->
                match body.CancelRequestedOn with
                | None ->
                    IndexBatchState.Finished (* success *) false
                | Some _ ->
                    IndexBatchState.Cancelled
            | BatchStatus.Unfinished, BatchActivationStatus.Awaiting (BatchParent.Batch _) ->
                IndexBatchState.AwaitingBatch
            | BatchStatus.Unfinished, BatchActivationStatus.Activated _ ->
                IndexBatchState.Started
            |> Some
        | BatchBodyVariant.Placeholder _ ->
            None // we really don't know the index state until placeholder filled

type ProperBatchConstructor = {
    Description: Option<NonemptyString>
    JobsToConstruct: BatchJobsToConstruct
    Parent: Option<BatchParent>
    SentOn: DateTimeOffset
}

[<RequireQualifiedAccess>]
type BatchSubscriber =
| Job of JobId
| Batch of BatchId

[<RequireQualifiedAccess>]
type BatchAction =
| OnJobStatusChanged of JobId * JobStatus
| OnParentBatchUpdate of BatchStatus
| Cancel
| FillPlaceholder of ProperBatchConstructor
| OnNewSubscriber of BatchSubscriber // to repeat BatchStatus event on Subscribe, should be called if subscriber does not create this Batch
with
    interface LifeAction
    interface IRedactable with
        member this.Redact() =
            match this with
            | FillPlaceholder ({ JobsToConstruct = jobs } as ctor) ->
                redactBatchJobsToConstructIfLarge jobs
                |> Option.map (fun redactedJobs -> FillPlaceholder { ctor with JobsToConstruct = redactedJobs })
                |> Option.defaultValue this
            | _ -> this
            |> box

[<RequireQualifiedAccess>]
type BatchOpError =
| JobNotInBatch of JobId
with interface OpError

[<RequireQualifiedAccess>]
type BatchConstructor =
| NewPlaceholder of BatchId * PlacedBy
| NewProper of BatchId * ProperBatchConstructor
with
    interface Constructor
    interface IRedactable with
        member this.Redact() =
            match this with
            | NewPlaceholder _ -> this
            | NewProper (batchId, ({ JobsToConstruct = jobs } as ctor)) ->
                redactBatchJobsToConstructIfLarge jobs
                |> Option.map (fun redactedJobs -> NewProper (batchId, { ctor with JobsToConstruct = redactedJobs }))
                |> Option.defaultValue this
            |> box

[<RequireQualifiedAccess>]
type BatchLifeEvent =
| OnBatchStatusChanged of BatchStatus * ExclusivelyForSubscriber: Option<BatchSubscriber>
with interface LifeEvent

[<RequireQualifiedAccess>]
type BatchNumericIndex =
| State of IndexBatchState
| CreatedOn of DateTimeOffset
| StartedOn of DateTimeOffset
| FinishedOn of DateTimeOffset
| CancelledOn of DateTimeOffset
| Placeholder of CreatedOn: DateTimeOffset
| SentOn of DateTimeOffset
| PlaceholderFilledOn of DateTimeOffset
with
    interface SubjectNumericIndex<BatchOpError> with
        member this.Primitive =
            match this with
            | State state ->
                match state with
                | IndexBatchState.AwaitingBatch -> 1L
                | IndexBatchState.AwaitingJob -> 2L
                | IndexBatchState.Started -> 3L
                | IndexBatchState.Finished true -> 4L
                | IndexBatchState.Finished false -> 5L
                | IndexBatchState.Cancelled -> 6L
                |> IndexedNumber
            | CreatedOn dt
            | StartedOn dt
            | FinishedOn dt
            | CancelledOn dt
            | Placeholder dt
            | SentOn dt
            | PlaceholderFilledOn dt ->
                IndexedNumber dt.UtcTicks

type BatchStringIndex =
| ParentBatch of BatchId
| ParentJob of JobId
with
    interface SubjectStringIndex<BatchOpError> with
        member this.Primitive =
            match this with
            | ParentBatch (BatchId s) -> IndexedString s
            | ParentJob (JobId s) -> IndexedString s

type BatchSearchIndex = NoSearchIndex
type BatchGeographyIndex = NoGeographyIndex

type BatchIndex() = inherit SubjectIndex<BatchIndex, BatchNumericIndex, BatchStringIndex, BatchSearchIndex, BatchGeographyIndex, BatchOpError>()



////////////////////////////////
// Generated code starts here //
////////////////////////////////

#if !FABLE_COMPILER

open CodecLib

#nowarn "69" // disable Interface implementations should normally be given on the initial declaration of a type


type JobProgress with
    static member private get_ObjCodec_AllCases () =
        function
        | Unfinished _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Unfinished _ -> Some 0 | _ -> None)
                and! payload = reqWith codecFor<_, JobId> "Unfinished" (function Unfinished x -> Some x | _ -> None)
                return Unfinished payload
            }
        | Finished _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Finished _ -> Some 0 | _ -> None)
                and! payload = reqWith (Codecs.tuple3 codecFor<_, JobId> Codecs.boolean Codecs.dateTimeOffset) "Finished" (function Finished (x1, x2, x3) -> Some (x1, x2, x3) | _ -> None)
                return Finished payload
            }
        |> mergeUnionCases
    static member get_Codec () = ofObjCodec (JobProgress.get_ObjCodec_AllCases ())


type SequentialJobsParams with
    static member private get_ObjCodec_V1 () =
        codec {
            let! _version = reqWith Codecs.int "__v1" (fun _ -> Some 0)
            and! awaitingForJobStatus = reqWith codecFor<_, AwaitingForJobStatus> "AwaitingForJobStatus" (fun x -> Some x.AwaitingForJobStatus)
            and! numberOfThreads = reqWith codecFor<_, PositiveInteger> "NumberOfThreads" (fun x -> Some x.NumberOfThreads)
            return {
                AwaitingForJobStatus = awaitingForJobStatus
                NumberOfThreads = numberOfThreads
             }
        }
    static member get_Codec () = ofObjCodec (SequentialJobsParams.get_ObjCodec_V1 ())


type BatchJobsToConstruct with
    static member private get_ObjCodec_AllCases () =
        function
        | Sequential _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Sequential _ -> Some 0 | _ -> None)
                and! payload = reqWith (Codecs.tuple2 (Codecs.list (Codecs.tuple2 codecFor<_, JobId> codecFor<_, JobConstructorCommonData>)) codecFor<_, SequentialJobsParams>) "Sequential" (function Sequential (x1, x2) -> Some (x1, x2) | _ -> None)
                return Sequential payload
            }
        | Parallel _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Parallel _ -> Some 0 | _ -> None)
                and! payload = reqWith (Codecs.list (Codecs.tuple2 codecFor<_, JobId> codecFor<_, JobConstructorCommonData>)) "Parallel" (function Parallel x -> Some x | _ -> None)
                return Parallel payload
            }
        | Placeholders _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Placeholders _ -> Some 0 | _ -> None)
                and! payload = reqWith (Codecs.list codecFor<_, JobId>) "Placeholders" (function Placeholders x -> Some x | _ -> None)
                return Placeholders payload
            }
        |> mergeUnionCases
    static member get_Codec () = ofObjCodec (BatchJobsToConstruct.get_ObjCodec_AllCases ())


type BatchParent with
    static member private get_ObjCodec_AllCases () =
        function
        | Batch _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Batch _ -> Some 0)
                and! payload = reqWith (Codecs.tuple2 codecFor<_, BatchId> codecFor<_, AwaitingForBatchStatus>) "Batch" (function Batch (x1, x2) -> Some (x1, x2))
                return Batch payload
            }
        |> mergeUnionCases
    static member get_Codec () = ofObjCodec (BatchParent.get_ObjCodec_AllCases ())


type BatchActivationStatus with
    static member private get_ObjCodec_AllCases () =
        function
        | Awaiting _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Awaiting _ -> Some 0 | _ -> None)
                and! payload = reqWith codecFor<_, BatchParent> "Awaiting" (function Awaiting x -> Some x | _ -> None)
                return Awaiting payload
            }
        | Activated _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Activated _ -> Some 0 | _ -> None)
                and! payload = reqWith (Codecs.tuple2 Codecs.dateTimeOffset (Codecs.option codecFor<_, BatchParent>)) "Activated" (function Activated (x1, x2) -> Some (x1, x2) | _ -> None)
                return Activated payload
            }
        |> mergeUnionCases
    static member get_Codec () = ofObjCodec (BatchActivationStatus.get_ObjCodec_AllCases ())


type BatchBody with
    static member private get_ObjCodec_V1 () =
        codec {
            let! _version = reqWith Codecs.int "__v1" (fun _ -> Some 2)
            and! description = reqWith (Codecs.option codecFor<_, NonemptyString>) "Description" (fun x -> Some x.Description)
            and! jobsProgress = reqWith (KeyedSet.codec codecFor<_, JobProgress>) "JobsProgress" (fun x -> Some x.JobsProgress)
            and! cancelRequestedOn = reqWith (Codecs.option Codecs.dateTimeOffset) "CancelRequestedOn" (fun x -> Some x.CancelRequestedOn)
            and! activationStatus = reqWith codecFor<_, BatchActivationStatus> "ActivationStatus" (fun x -> Some x.ActivationStatus)
            and! maybeSentOn = optWith Codecs.dateTimeOffset "SentOn" (fun x -> Some x.SentOn)
            and! placeholderFilledOn = optWith Codecs.dateTimeOffset "PlaceholderFilledOn" (fun x -> x.PlaceholderFilledOn)
            return {
                Description = description
                JobsProgress = jobsProgress
                CancelRequestedOn = cancelRequestedOn
                ActivationStatus = activationStatus
                SentOn = maybeSentOn |> Option.defaultValue DateTimeOffset.UnixEpoch
                PlaceholderFilledOn = placeholderFilledOn
             }
        }
    static member get_Codec () = ofObjCodec (BatchBody.get_ObjCodec_V1 ())


type BatchBodyPlaceholder with
    static member private get_ObjCodec_V1 () =
        codec {
            let! _version = reqWith Codecs.int "__v1" (fun _ -> Some 0)
            and! placedBy = reqWith codecFor<_, PlacedBy> "PlacedBy" (fun x -> Some x.PlacedBy)
            and! cancelRequestedOn = reqWith (Codecs.option Codecs.dateTimeOffset) "CancelRequestedOn" (fun x -> Some x.CancelRequestedOn)
            return {
                PlacedBy = placedBy
                CancelRequestedOn = cancelRequestedOn
             }
        }
    static member get_Codec () = ofObjCodec (BatchBodyPlaceholder.get_ObjCodec_V1 ())


type BatchBodyVariant with
    static member private get_ObjCodec_AllCases () =
        function
        | Proper _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Proper _ -> Some 0 | _ -> None)
                and! payload = reqWith codecFor<_, BatchBody> "Proper" (function Proper x -> Some x | _ -> None)
                return Proper payload
            }
        | Placeholder _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Placeholder _ -> Some 0 | _ -> None)
                and! payload = reqWith codecFor<_, BatchBodyPlaceholder> "Placeholder" (function Placeholder x -> Some x | _ -> None)
                return Placeholder payload
            }
        |> mergeUnionCases
    static member get_Codec () = ofObjCodec (BatchBodyVariant.get_ObjCodec_AllCases ())


type Batch with
    static member TypeLabel () = "Batch"

    static member private get_ObjCodec_V1 () =
        codec {
            let! _version = reqWith Codecs.int "__v1" (fun _ -> Some 0)
            and! id = reqWith codecFor<_, BatchId> "Id" (fun x -> Some x.Id)
            and! createdOn = reqWith Codecs.dateTimeOffset "CreatedOn" (fun x -> Some x.CreatedOn)
            and! body = reqWith codecFor<_, BatchBodyVariant> "Body" (fun x -> Some x.Body)
            return {
                Id = id
                CreatedOn = createdOn
                Body = body
             }
        }

    static member private get_ObjCodec () = Batch.get_ObjCodec_V1 ()
    static member get_Codec () = ofObjCodec <| Batch.get_ObjCodec ()
    static member Init (typeLabel: string, _typeParams: _) = initializeInterfaceImplementation<Subject<BatchId>, Batch> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| Batch.get_ObjCodec ())


type private OBSOLETE_BatchConstructorCommonData = {
    Description: Option<NonemptyString>
    JobsToConstruct: BatchJobsToConstruct
    SentOn: DateTimeOffset
} with
    static member private get_ObjCodec_V1 () =
        codec {
            let! _version = reqWith Codecs.int "__v1" (fun _ -> Some 1)
            and! description = reqWith (Codecs.option codecFor<_, NonemptyString>) "Description" (fun x -> Some x.Description)
            and! jobsToConstruct = reqWith codecFor<_, BatchJobsToConstruct> "JobsToConstruct" (fun x -> Some x.JobsToConstruct)
            and! maybeSentOn = optWith Codecs.dateTimeOffset "SentOn" (fun x -> Some x.SentOn)
            return {
                Description = description
                JobsToConstruct = jobsToConstruct
                SentOn = maybeSentOn |> Option.defaultValue DateTimeOffset.UnixEpoch
             }
        }
    static member get_Codec () = ofObjCodec (OBSOLETE_BatchConstructorCommonData.get_ObjCodec_V1 ())


type ProperBatchConstructor with
    static member private get_ObjCodec_V2 () =
        codec {
            let! _version = reqWith Codecs.int "__v2" (fun _ -> Some 1)
            and! description = reqWith (Codecs.option codecFor<_, NonemptyString>) "Description" (fun x -> Some x.Description)
            and! jobsToConstruct = reqWith codecFor<_, BatchJobsToConstruct> "JobsToConstruct" (fun x -> Some x.JobsToConstruct)
            and! parent = reqWith (Codecs.option codecFor<_, BatchParent>) "Parent" (fun x -> Some x.Parent)
            and! maybeSentOn = optWith Codecs.dateTimeOffset "SentOn" (fun x -> Some x.SentOn)
            return {
                Description = description
                JobsToConstruct = jobsToConstruct
                Parent = parent
                SentOn = maybeSentOn |> Option.defaultValue DateTimeOffset.UnixEpoch
             }
        }

    static member private get_ObjDecoder_V1_Enqueued () =
        decoder {
            let! _version = reqDecodeWithCodec Codecs.int "__v1"
            and! data = reqDecodeWithCodec (OBSOLETE_BatchConstructorCommonData.get_Codec()) "Enqueued"
            return {
                Description = data.Description
                JobsToConstruct = data.JobsToConstruct
                Parent = None
                SentOn = data.SentOn
            }
        }

    static member private get_ObjDecoder_V1_Awaiting () =
        decoder {
            let! _version = reqDecodeWithCodec Codecs.int "__v1"
            and! data, parent = reqDecodeWithCodec (Codecs.tuple2 (OBSOLETE_BatchConstructorCommonData.get_Codec()) codecFor<_, BatchParent>) "Awaiting"
            return {
                Description = data.Description
                JobsToConstruct = data.JobsToConstruct
                Parent = Some parent
                SentOn = data.SentOn
            }
        }

    static member get_Codec () =
        ProperBatchConstructor.get_ObjCodec_V2 ()
        |> withDecoders [
            ProperBatchConstructor.get_ObjDecoder_V1_Enqueued()
            ProperBatchConstructor.get_ObjDecoder_V1_Awaiting()
        ]
        |> ofObjCodec


type BatchSubscriber with
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
        |> mergeUnionCases
    static member get_Codec () = ofObjCodec (BatchSubscriber.get_ObjCodec_AllCases ())


type BatchAction with
    static member TypeLabel () = "BatchAction"

    static member private get_ObjCodec_AllCases () =
        function
        | OnJobStatusChanged _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function OnJobStatusChanged _ -> Some 0 | _ -> None)
                and! payload = reqWith (Codecs.tuple2 codecFor<_, JobId> codecFor<_, JobStatus>) "OnJobStatusChanged" (function OnJobStatusChanged (x1, x2) -> Some (x1, x2) | _ -> None)
                return OnJobStatusChanged payload
            }
        | OnParentBatchUpdate _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function OnParentBatchUpdate _ -> Some 0 | _ -> None)
                and! payload = reqWith codecFor<_, BatchStatus> "OnParentBatchUpdate" (function OnParentBatchUpdate x -> Some x | _ -> None)
                return OnParentBatchUpdate payload
            }
        | Cancel ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Cancel -> Some 0 | _ -> None)
                and! _ = reqWith Codecs.unit "Cancel" (function Cancel -> Some () | _ -> None)
                return Cancel
            }
        | FillPlaceholder _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function FillPlaceholder _ -> Some 0 | _ -> None)
                and! payload = reqWith codecFor<_, ProperBatchConstructor> "FillPlaceholder" (function FillPlaceholder x -> Some x | _ -> None)
                return FillPlaceholder payload
            }
        | OnNewSubscriber _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function OnNewSubscriber _ -> Some 0 | _ -> None)
                and! payload = reqWith codecFor<_, BatchSubscriber> "OnNewSubscriber" (function OnNewSubscriber x -> Some x | _ -> None)
                return OnNewSubscriber payload
            }
        |> mergeUnionCases

    static member private get_ObjCodec () = BatchAction.get_ObjCodec_AllCases ()
    static member get_Codec () = ofObjCodec <| BatchAction.get_ObjCodec ()
    static member Init (typeLabel: string, _typeParams: _) = initializeInterfaceImplementation<LifeAction, BatchAction> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| BatchAction.get_ObjCodec ())



type BatchOpError with
    static member TypeLabel () = "BatchOpError"

    static member private get_ObjCodec_AllCases () =
        function
        | JobNotInBatch _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function JobNotInBatch _ -> Some 0)
                and! payload = reqWith codecFor<_, JobId> "JobNotInBatch" (function JobNotInBatch x -> Some x)
                return JobNotInBatch payload
            }
        |> mergeUnionCases

    static member private get_ObjCodec () = BatchOpError.get_ObjCodec_AllCases ()
    static member get_Codec () = ofObjCodec <| BatchOpError.get_ObjCodec ()
    static member Init (typeLabel: string, _typeParams: _) = initializeInterfaceImplementation<OpError, BatchOpError> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| BatchOpError.get_ObjCodec ())



type BatchConstructor with
    static member TypeLabel () = "BatchConstructor"

    static member private get_ObjCodec_AllCases () =
        function
        | NewPlaceholder _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function NewPlaceholder _ -> Some 0 | _ -> None)
                and! payload = reqWith (Codecs.tuple2 codecFor<_, BatchId> codecFor<_, PlacedBy>) "NewPlaceholder" (function NewPlaceholder (x1, x2) -> Some (x1, x2) | _ -> None)
                return NewPlaceholder payload
            }
        | NewProper _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function NewProper _ -> Some 0 | _ -> None)
                and! payload = reqWith (Codecs.tuple2 codecFor<_, BatchId> codecFor<_, ProperBatchConstructor>) "NewProper" (function NewProper (x1, x2) -> Some (x1, x2) | _ -> None)
                return NewProper payload
            }
        |> mergeUnionCases

    static member private get_ObjCodec () = BatchConstructor.get_ObjCodec_AllCases ()
    static member get_Codec () = ofObjCodec <| BatchConstructor.get_ObjCodec ()
    static member Init (typeLabel: string, _typeParams: _) = initializeInterfaceImplementation<Constructor, BatchConstructor> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| BatchConstructor.get_ObjCodec ())



type BatchLifeEvent with
    static member TypeLabel () = "Jobs_BatchLifeEvent"

    static member private get_ObjCodec_AllCases () =
        function
        | OnBatchStatusChanged _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function OnBatchStatusChanged _ -> Some 0)
                and! payload = reqWith (Codecs.tuple2 codecFor<_, BatchStatus> (Codecs.option codecFor<_, BatchSubscriber>)) "OnBatchStatusChanged" (function OnBatchStatusChanged (x1, x2) -> Some (x1, x2))
                return OnBatchStatusChanged payload
            }
        |> mergeUnionCases

    static member private get_ObjCodec () = BatchLifeEvent.get_ObjCodec_AllCases ()
    static member get_Codec () = ofObjCodec <| BatchLifeEvent.get_ObjCodec ()
    static member Init (typeLabel: string, _typeParams: _) = initializeInterfaceImplementation<LifeEvent, BatchLifeEvent> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| BatchLifeEvent.get_ObjCodec ())

#endif // !FABLE_COMPILER