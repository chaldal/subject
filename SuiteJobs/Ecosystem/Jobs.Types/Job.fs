[<AutoOpen; CodecLib.CodecAutoGenerate>]
module SuiteJobs.Types.Job

open System

[<RequireQualifiedAccess>]
type JobFailedReason =
| Exception of Message: string * Type: string * Details: string
| ConnectorTimeout

[<RequireQualifiedAccess>]
type JobParent =
| Job of JobId * AwaitingForJobStatus
| Batch of BatchId * AwaitingForBatchStatus

type FinishedJobRun = {
    FinishedOn: DateTimeOffset
    StartedBy: DispatcherId
    /// For how long job stayed Enqueued
    Latency: TimeSpan
    /// Total duration as measured in Job Runner connector
    TotalDuration: TimeSpan
    /// Pure duration which excludes any overhead specific to Job Runner implementation e.g. network overhead if Job runner is hosted remotely.
    /// None if unknown e.g. remote request timed out, or not applicable e.g. job runner has no overhead
    PureDuration: Option<TimeSpan>
}

type UnfinishedJobRun = {
    StartedBy: DispatcherId
    StartedOn: DateTimeOffset
    Latency: TimeSpan
    LastHeartbeatOn: Option<DateTimeOffset>
}

[<RequireQualifiedAccess>]
type JobState =
| Scheduled of EnqueueOn: DateTimeOffset
| Awaiting of JobParent
| Enqueued of OrderInQueue: int64 * EnqueuedOn: DateTimeOffset
| Processing of Ticket: Guid * UnfinishedJobRun
| Succeeded of FinishedJobRun
| Failed of JobFailedReason * FinishedJobRun
| Deleted of DeletedOn: DateTimeOffset


[<RequireQualifiedAccess>]
type JobAutoRetryDelayPolicy =
| LinearIncrease of InitialDelaySeconds: uint32
/// Imitates Hangfire AutomaticRetry attribute logic, as silly as it is
| Hangfire

type JobAutoRetriesPolicy = {
    MaxAutoRetries: byte
    DeleteIfExceeded: bool
    DelayPolicy: JobAutoRetryDelayPolicy
}

type JobFailurePolicy = {
    MaybeAutoRetries: Option<JobAutoRetriesPolicy> // None if no automatic retries allowed
}
with
    member this.IsAtMostOnce =
        this.MaybeAutoRetries
        |> Option.map (fun r -> r.MaxAutoRetries = 0uy)
        |> Option.defaultValue true

type JobRetry = {
    Attempt: byte
    LastFailureMessage: string
}

[<RequireQualifiedAccess>]
type JobScope =
| Batch of BatchId
| Recurring of RecurringJobId
| Other

type JobBody = {
    Scope: JobScope
    Parent: Option<JobParent>

    Payload: JobPayload
    QueueName: NonemptyString
    QueueSortOrder: uint16
    FailurePolicy: JobFailurePolicy
    // captured on client side, to know how long it took it to relay the job to the backend
    SentOn: DateTimeOffset
    PlaceholderFilledOn: Option<DateTimeOffset>
    // to correlate with client telemetry that posted this job
    CorrelationId: Option<string>

    Retry: Option<JobRetry>
    State: JobState
}

type JobBodyPlaceholder = {
    PlacedBy: PlacedBy
    DeleteRequestedOn: Option<DateTimeOffset>
}

[<RequireQualifiedAccess>]
type JobBodyVariant =
| Proper of JobBody
// if awaiting job was constructed prior to its parent, it will construct a placeholder to subscribe to
| Placeholder of JobBodyPlaceholder

type Job = {
    Id: JobId
    CreatedOn: DateTimeOffset
    Body: JobBodyVariant
}
with
    interface Subject<JobId> with
        member this.SubjectCreatedOn =
            this.CreatedOn

        member this.SubjectId =
            this.Id

    member this.Status : JobStatus =
        match this.Body with
        | JobBodyVariant.Placeholder placeholder ->
            match placeholder.DeleteRequestedOn with
            | None -> JobStatus.Unfinished
            | Some _ ->
                // cancelled placeholder still considered Unfinished, it allows to introduce "UndoCancel" action if need be
                JobStatus.Unfinished
        | JobBodyVariant.Proper body ->
            match body.State with
            | JobState.Succeeded _ ->
                JobStatus.Finished (* Succeeded *) true
            | JobState.Deleted (_: DateTimeOffset) ->
                JobStatus.Finished (* Succeeded *) false
            | JobState.Scheduled _ | JobState.Enqueued _ | JobState.Processing _ | JobState.Awaiting _ | JobState.Failed _ ->
                JobStatus.Unfinished

type JobConstructorCommonData = {
    Payload: JobPayload
    DisplayName: NonemptyString
    QueueName: NonemptyString
    QueueSortOrder: uint16
    FailurePolicy: JobFailurePolicy
    SentOn: DateTimeOffset
    CorrelationId: Option<string>
}

type JobProcessingResult = {
    TotalDuration: TimeSpan
    PureDuration: Option<TimeSpan>
    Result: Result<unit, (* retryForFree *) bool * JobFailedReason>
}

[<RequireQualifiedAccess>]
type ProperJobConstructor =
| Enqueued of JobConstructorCommonData * JobScope
| Scheduled of JobConstructorCommonData * ScheduledOn: DateTimeOffset
| Awaiting of JobConstructorCommonData * JobScope * JobParent
// in marginal case job can be created as Deleted e.g. when it's creator Batch was cancelled while it was a placeholder and then was filled
| Deleted of JobConstructorCommonData * JobScope

[<RequireQualifiedAccess>]
type JobSubscriber =
| Job of JobId
| Batch of BatchId

[<RequireQualifiedAccess>]
type JobAction =
| Schedule of On: DateTimeOffset
| Enqueue
| Start of DequeuedBy: DispatcherId
| OnHeartbeat of Ticket: Guid
| OnProcessingComplete of Ticket: Guid * JobProcessingResult
| OnParentJobUpdate of JobStatus
| OnParentBatchUpdate of BatchStatus
| Delete
| FillPlaceholder of ProperJobConstructor
| OnNewSubscriber of JobSubscriber // to repeat JobStatus event on Subscribe, should be called if subscriber does not create this Job
| DeleteAwaitingJobsBackwards // useful to delete stuck job chains with AnyFinishedState continuations (starting from last job in the chain)
with interface LifeAction

[<RequireQualifiedAccess>]
type JobOpError =
| UnableToConstructBody of Reason: string
| CannotDeleteSuccessfulJob
with interface OpError

[<RequireQualifiedAccess>]
type JobConstructor =
| NewPlaceholder of JobId * PlacedBy // in case if child job created before parent job, it will create a placeholder
| NewProper of JobId * ProperJobConstructor
with interface Constructor

[<RequireQualifiedAccess>]
type JobLifeEvent =
| OnJobStatusChanged of JobStatus * ExclusivelyForSubscriber: Option<JobSubscriber>
| OnProcessingCompleted
with interface LifeEvent

[<CodecLib.SkipCodecAutoGenerate>]
[<RequireQualifiedAccess>]
type IndexJobState =
| Scheduled
| AwaitingJob
| AwaitingBatch
| Enqueued
| Processing
| Succeeded
| Failed
| Deleted
| Placeholder

[<RequireQualifiedAccess>]
type JobNumericIndex =
| State of IndexJobState
| Retry
| SentOn of DateTimeOffset
| PlaceholderFilledOn of DateTimeOffset
| CreatedOn of DateTimeOffset
| ScheduledOn of DateTimeOffset
| EnqueuedOn of DateTimeOffset
| ProcessingFrom of DateTimeOffset
| HeartbeatOn of DateTimeOffset
| FailedOn of DateTimeOffset
| SucceededOn of DateTimeOffset
| SucceededRetryAttempt of byte
| TotalDuration of TimeSpan
| PureDuration of TimeSpan
| Latency of TimeSpan
| DeletedOn of DateTimeOffset
| Placeholder of CreatedOn: DateTimeOffset
with
    interface SubjectNumericIndex<JobOpError> with
        member this.Primitive =
            match this with
            | State state ->
                match state with
                | IndexJobState.Scheduled -> 1L
                | IndexJobState.AwaitingJob -> 2L
                | IndexJobState.AwaitingBatch -> 3L
                | IndexJobState.Enqueued -> 4L
                | IndexJobState.Processing -> 5L
                | IndexJobState.Succeeded -> 6L
                | IndexJobState.Failed -> 7L
                | IndexJobState.Deleted -> 8L
                | IndexJobState.Placeholder -> 9L
                |> IndexedNumber
            | Retry -> IndexedNumber 1L
            | Placeholder dt
            | SentOn dt
            | PlaceholderFilledOn dt
            | CreatedOn dt
            | EnqueuedOn dt
            | ScheduledOn dt
            | ProcessingFrom dt
            | HeartbeatOn dt
            | FailedOn dt
            | SucceededOn dt
            | DeletedOn dt ->
                IndexedNumber dt.UtcTicks
            | PureDuration t
            | TotalDuration t
            | Latency t ->
                IndexedNumber t.Ticks
            | SucceededRetryAttempt b ->
                IndexedNumber (int64 b)

[<RequireQualifiedAccess>]
type JobStringIndex =
| EnqueuedTo of QueueName: NonemptyString
| DequeueSort of string
| StartedBy of DispatcherId
/// batch that has created and owns this job, do not confuse with ParentBatch index
| Batch of BatchId
| ParentJob of JobId
/// Batch that this job is or was awaiting, do not confuse with Batch index
| ParentBatch of BatchId
| Recurring of RecurringJobId
| Type of string
| Method of string
| Args of string
| Queue of NonemptyString // written for all states, unlike EnqueuedTo
| CorrelationId of string
with
    interface SubjectStringIndex<JobOpError> with
        member this.Primitive =
            match this with
            | EnqueuedTo s
            | Queue s -> IndexedString s.Value
            | DequeueSort s
            | Type s
            | Method s
            | Args s
            | Batch (BatchId.BatchId s)
            | ParentJob (JobId s)
            | ParentBatch (BatchId.BatchId s)
            | Recurring (RecurringJobId s)
            | CorrelationId s ->
                IndexedString s
            | StartedBy (DispatcherId s) -> IndexedString s.Value

type JobSearchIndex = NoSearchIndex
type JobGeographyIndex = NoGeographyIndex

type JobIndex() = inherit SubjectIndex<JobIndex, JobNumericIndex, JobStringIndex, JobSearchIndex, JobGeographyIndex, JobOpError>()



////////////////////////////////
// Generated code starts here //
////////////////////////////////

#if !FABLE_COMPILER

open CodecLib

#nowarn "69" // disable Interface implementations should normally be given on the initial declaration of a type


type JobFailedReason with
    static member private get_ObjCodec_AllCases () =
        function
        | Exception _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Exception _ -> Some 0 | _ -> None)
                and! payload = reqWith (Codecs.tuple3 Codecs.string Codecs.string Codecs.string) "Exception" (function Exception (x1, x2, x3) -> Some (x1, x2, x3) | _ -> None)
                return Exception payload
            }
        | ConnectorTimeout ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function ConnectorTimeout -> Some 0 | _ -> None)
                and! _ = reqWith Codecs.unit "ConnectorTimeout" (function ConnectorTimeout -> Some () | _ -> None)
                return ConnectorTimeout
            }
        |> mergeUnionCases
    static member get_Codec () = ofObjCodec (JobFailedReason.get_ObjCodec_AllCases ())


type JobParent with
    static member private get_ObjCodec_AllCases () =
        function
        | Job _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Job _ -> Some 0 | _ -> None)
                and! payload = reqWith (Codecs.tuple2 codecFor<_, JobId> codecFor<_, AwaitingForJobStatus>) "Job" (function Job (x1, x2) -> Some (x1, x2) | _ -> None)
                return Job payload
            }
        | Batch _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Batch _ -> Some 0 | _ -> None)
                and! payload = reqWith (Codecs.tuple2 codecFor<_, BatchId> codecFor<_, AwaitingForBatchStatus>) "Batch" (function Batch (x1, x2) -> Some (x1, x2) | _ -> None)
                return Batch payload
            }
        |> mergeUnionCases
    static member get_Codec () = ofObjCodec (JobParent.get_ObjCodec_AllCases ())


type FinishedJobRun with
    static member private get_ObjCodec_V1 () =
        codec {
            let! _version = reqWith Codecs.int "__v1" (fun _ -> Some 1)
            and! total = reqWith Codecs.timeSpan "Total" (fun x -> Some x.TotalDuration)
            and! pure' = optWith Codecs.timeSpan "Pure" (fun x -> x.PureDuration)
            and! maybeLatency = optWith Codecs.timeSpan "Latency" (fun x -> Some x.Latency)
            and! maybeFinishedOn = optWith Codecs.dateTimeOffset "FinishedOn" (fun x -> Some x.FinishedOn)
            and! maybeStartedBy = optWith codecFor<_, DispatcherId> "StartedBy" (fun x -> Some x.StartedBy)
            return {
                FinishedOn = maybeFinishedOn |> Option.defaultWith (fun () -> DateTimeOffset(2025, 2, 11, 0, 0, 0, TimeSpan.Zero))
                StartedBy = maybeStartedBy |> Option.defaultWith (fun () -> DispatcherId (NonemptyString.ofLiteral "Dispatcher-0"))
                TotalDuration = total
                PureDuration = pure'
                Latency = maybeLatency |> Option.defaultValue TimeSpan.Zero
             }
        }
    static member get_Codec () = ofObjCodec (FinishedJobRun.get_ObjCodec_V1 ())


type UnfinishedJobRun with
    static member private get_ObjCodec_V1 () =
        codec {
            let! _version = reqWith Codecs.int "__v1" (fun _ -> Some 0)
            and! latency = reqWith Codecs.timeSpan "Latency" (fun x -> Some x.Latency)
            and! startedBy = reqWith codecFor<_, DispatcherId> "StartedBy" (fun x -> Some x.StartedBy)
            and! startedOn = reqWith Codecs.dateTimeOffset "StartedOn" (fun x -> Some x.StartedOn)
            and! lastHeartbeatOn = optWith Codecs.dateTimeOffset "LastHeartbeatOn" (fun x -> x.LastHeartbeatOn)
            return {
                StartedBy = startedBy
                StartedOn = startedOn
                Latency = latency
                LastHeartbeatOn = lastHeartbeatOn
             }
        }
    static member get_Codec () = ofObjCodec (UnfinishedJobRun.get_ObjCodec_V1 ())


type JobState with
    static member private get_ObjCodec_AllCases () =
        function
        | Scheduled _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Scheduled _ -> Some 0 | _ -> None)
                and! payload = reqWith Codecs.dateTimeOffset "Scheduled" (function Scheduled x -> Some x | _ -> None)
                return Scheduled payload
            }
        | Awaiting _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Awaiting _ -> Some 0 | _ -> None)
                and! payload = reqWith codecFor<_, JobParent> "Awaiting" (function Awaiting x -> Some x | _ -> None)
                return Awaiting payload
            }
        | Enqueued _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Enqueued _ -> Some 0 | _ -> None)
                and! payload = reqWith (Codecs.tuple2 Codecs.int64 Codecs.dateTimeOffset) "Enqueued" (function Enqueued (x1, x2) -> Some (x1, x2) | _ -> None)
                return Enqueued payload
            }
        | Processing _ ->
            codec {
                let! _version = reqWith Codecs.int "__v3" (function Processing _ -> Some 0 | _ -> None)
                and! payload = reqWith (Codecs.tuple2 Codecs.guid codecFor<_, UnfinishedJobRun>) "Processing" (function Processing (x1, x2) -> Some (x1, x2) | _ -> None)
                return Processing payload
            }
            |> withDecoders [
                decoder {
                    let! _version = reqDecodeWithCodec Codecs.int "__v2"
                    and! x1, x2, x3, x4 = reqDecodeWithCodec (Codecs.tuple4 Codecs.guid codecFor<_, DispatcherId> Codecs.dateTimeOffset (Codecs.option Codecs.dateTimeOffset)) "Processing"
                    return Processing (x1, { StartedBy = x2; StartedOn = x3; Latency = TimeSpan.Zero; LastHeartbeatOn = x4 })
                }

                decoder {
                    let! _version = reqDecodeWithCodec Codecs.int "__v1"
                    and! x1, x2, x3 = reqDecodeWithCodec (Codecs.tuple3 Codecs.guid codecFor<_, DispatcherId> Codecs.dateTimeOffset) "Processing"
                    return Processing (x1, { StartedBy = x2; StartedOn = x3; Latency = TimeSpan.Zero; LastHeartbeatOn = None })
                }
            ]
        | Succeeded _ ->
            codec {
                let! _version = reqWith Codecs.int "__v3" (function Succeeded _ -> Some 0 | _ -> None)
                and! payload = reqWith codecFor<_, FinishedJobRun> "Succeeded" (function Succeeded x -> Some x | _ -> None)
                return Succeeded payload
            }
            |> withDecoders [
                decoder {
                    let! _version = reqDecodeWithCodec Codecs.int "__v2"
                    and! finishedJobRun, succeededOn = reqDecodeWithCodec (Codecs.tuple2 codecFor<_, FinishedJobRun> Codecs.dateTimeOffset) "Succeeded"
                    return Succeeded { finishedJobRun with FinishedOn = succeededOn }
                }
                decoder {
                    let! _version = reqDecodeWithCodec Codecs.int "__v1"
                    and! duration, succeededOn = reqDecodeWithCodec (Codecs.tuple2 Codecs.timeSpan Codecs.dateTimeOffset) "Succeeded"
                    return Succeeded { FinishedOn = succeededOn; StartedBy = DispatcherId (NonemptyString.ofLiteral "Dispatcher-0"); TotalDuration = duration; PureDuration = Some duration; Latency = TimeSpan.Zero }
                }
            ]
        | Failed _ ->
            codec {
                let! _version = reqWith Codecs.int "__v3" (function Failed _ -> Some 0 | _ -> None)
                and! payload = reqWith (Codecs.tuple2 codecFor<_, JobFailedReason> codecFor<_, FinishedJobRun>) "Failed" (function Failed (x1, x2) -> Some (x1, x2) | _ -> None)
                return Failed payload
            }
            |> withDecoders [
                decoder {
                    let! _version = reqDecodeWithCodec Codecs.int "__v2"
                    and! failedReason, finishedJobRun, failedOn = reqDecodeWithCodec (Codecs.tuple3 codecFor<_, JobFailedReason> codecFor<_, FinishedJobRun> Codecs.dateTimeOffset) "Failed"
                    return Failed (failedReason, { finishedJobRun with FinishedOn = failedOn })
                }
                decoder {
                    let! _version = reqDecodeWithCodec Codecs.int "__v1"
                    and! reason, duration, failedOn = reqDecodeWithCodec (Codecs.tuple3 codecFor<_, JobFailedReason> Codecs.timeSpan Codecs.dateTimeOffset) "Failed"
                    return Failed (reason, { FinishedOn = failedOn; StartedBy = DispatcherId (NonemptyString.ofLiteral "Dispatcher-0"); TotalDuration = duration; PureDuration = Some duration; Latency = TimeSpan.Zero })
                }
            ]
        | Deleted _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Deleted _ -> Some 0 | _ -> None)
                and! payload = reqWith Codecs.dateTimeOffset "Deleted" (function Deleted x -> Some x | _ -> None)
                return Deleted payload
            }
        |> mergeUnionCases
    static member get_Codec () = ofObjCodec (JobState.get_ObjCodec_AllCases ())


type JobAutoRetryDelayPolicy with
    static member private get_ObjCodec_AllCases () =
        function
        | LinearIncrease _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function LinearIncrease _ -> Some 0 | _ -> None)
                and! payload = reqWith Codecs.uint32 "Linear" (function LinearIncrease x -> Some x | _ -> None)
                return LinearIncrease payload
            }
        | Hangfire ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Hangfire -> Some 0 | _ -> None)
                and! _ = reqWith Codecs.unit "H" (function Hangfire -> Some () | _ -> None)
                return Hangfire
            }
        |> mergeUnionCases
    static member get_Codec () = ofObjCodec (JobAutoRetryDelayPolicy.get_ObjCodec_AllCases ())

type JobAutoRetriesPolicy with
    static member private get_ObjCodec_V1 () =
        codec {
            let! _version = reqWith Codecs.int "__v1" (fun _ -> Some 1)
            and! maxAutoRetries = reqWith Codecs.byte "MaxAutoRetries" (fun x -> Some x.MaxAutoRetries)
            and! deleteIfExceeded = reqWith Codecs.boolean "DeleteIfExceeded" (fun x -> Some x.DeleteIfExceeded)
            and! maybeDelayPolicy = optWith codecFor<_, JobAutoRetryDelayPolicy> "DP" (fun x -> Some x.DelayPolicy)
            return {
                MaxAutoRetries = maxAutoRetries
                DeleteIfExceeded = deleteIfExceeded
                DelayPolicy = maybeDelayPolicy |> Option.defaultValue JobAutoRetryDelayPolicy.Hangfire
             }
        }
    static member get_Codec () = ofObjCodec (JobAutoRetriesPolicy.get_ObjCodec_V1 ())


type JobFailurePolicy with
    static member private get_ObjCodec_V1 () =
        codec {
            let! _version = reqWith Codecs.int "__v1" (fun _ -> Some 1)
            and! maybeAutoRetries = reqWith (Codecs.option codecFor<_, JobAutoRetriesPolicy>) "MaybeAutoRetries" (fun x -> Some x.MaybeAutoRetries)
            return {
                MaybeAutoRetries = maybeAutoRetries
             }
        }
    static member get_Codec () = ofObjCodec (JobFailurePolicy.get_ObjCodec_V1 ())


type JobRetry with
    static member private get_ObjCodec_V1 () =
        codec {
            let! _version = reqWith Codecs.int "__v1" (fun _ -> Some 0)
            and! attempt = reqWith Codecs.byte "Attempt" (fun x -> Some x.Attempt)
            and! lastFailureMessage = reqWith Codecs.string "LastFailureMessage" (fun x -> Some x.LastFailureMessage)
            return {
                Attempt = attempt
                LastFailureMessage = lastFailureMessage
             }
        }
    static member get_Codec () = ofObjCodec (JobRetry.get_ObjCodec_V1 ())


type JobScope with
    static member private get_ObjCodec_AllCases () =
        function
        | Batch _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Batch _ -> Some 0 | _ -> None)
                and! payload = reqWith codecFor<_, BatchId> "Batch" (function Batch x -> Some x | _ -> None)
                return Batch payload
            }
        | Recurring _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Recurring _ -> Some 0 | _ -> None)
                and! payload = reqWith codecFor<_, RecurringJobId> "Recurring" (function Recurring x -> Some x | _ -> None)
                return Recurring payload
            }
        | Other ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Other -> Some 0 | _ -> None)
                and! _ = reqWith Codecs.unit "Other" (function Other -> Some () | _ -> None)
                return Other
            }
        |> mergeUnionCases
    static member get_Codec () = ofObjCodec (JobScope.get_ObjCodec_AllCases ())


type JobBody with
    static member private get_ObjCodec_V1 () =
        codec {
            let! _version = reqWith Codecs.int "__v1" (fun _ -> Some 2)
            and! scope = reqWith codecFor<_, JobScope> "Creator" (fun x -> Some x.Scope)
            and! parent = reqWith (Codecs.option codecFor<_, JobParent>) "Parent" (fun x -> Some x.Parent)
            and! payload = reqWith codecFor<_, JobPayload> "Payload" (fun x -> Some x.Payload)
            and! queueName = reqWith codecFor<_, NonemptyString> "QueueName" (fun x -> Some x.QueueName)
            and! queueSortOrder = reqWith Codecs.uint16 "QueueSortOrder" (fun x -> Some x.QueueSortOrder)
            and! failurePolicy = reqWith codecFor<_, JobFailurePolicy> "FailurePolicy" (fun x -> Some x.FailurePolicy)
            and! retry = reqWith (Codecs.option codecFor<_, JobRetry>) "Retry" (fun x -> Some x.Retry)
            and! state = reqWith codecFor<_, JobState> "State" (fun x -> Some x.State)
            and! maybeSentOn = optWith Codecs.dateTimeOffset "SentOn" (fun x -> Some x.SentOn)
            and! correlationId = optWith Codecs.string "CorrelationId" (fun x -> x.CorrelationId)
            and! placeholderFilledOn = optWith Codecs.dateTimeOffset "PlaceholderFilledOn" (fun x -> x.PlaceholderFilledOn)
            return {
                Scope = scope
                Parent = parent
                Payload = payload
                QueueName = queueName
                QueueSortOrder = queueSortOrder
                FailurePolicy = failurePolicy
                SentOn = maybeSentOn |> Option.defaultValue DateTimeOffset.UnixEpoch
                PlaceholderFilledOn = placeholderFilledOn
                CorrelationId = correlationId
                Retry = retry
                State = state
             }
        }
    static member get_Codec () = ofObjCodec (JobBody.get_ObjCodec_V1 ())


type JobBodyPlaceholder with
    static member private get_ObjCodec_V1 () =
        codec {
            let! _version = reqWith Codecs.int "__v1" (fun _ -> Some 0)
            and! placedBy = reqWith codecFor<_, PlacedBy> "PlacedBy" (fun x -> Some x.PlacedBy)
            and! deleteRequestedOn = reqWith (Codecs.option Codecs.dateTimeOffset) "DeleteRequestedOn" (fun x -> Some x.DeleteRequestedOn)
            return {
                PlacedBy = placedBy
                DeleteRequestedOn = deleteRequestedOn
             }
        }
    static member get_Codec () = ofObjCodec (JobBodyPlaceholder.get_ObjCodec_V1 ())


type JobBodyVariant with
    static member private get_ObjCodec_AllCases () =
        function
        | Proper _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Proper _ -> Some 0 | _ -> None)
                and! payload = reqWith codecFor<_, JobBody> "Proper" (function Proper x -> Some x | _ -> None)
                return Proper payload
            }
        | Placeholder _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Placeholder _ -> Some 0 | _ -> None)
                and! payload = reqWith codecFor<_, JobBodyPlaceholder> "Placeholder" (function Placeholder x -> Some x | _ -> None)
                return Placeholder payload
            }
        |> mergeUnionCases
    static member get_Codec () = ofObjCodec (JobBodyVariant.get_ObjCodec_AllCases ())


type Job with
    static member TypeLabel () = "Job"

    static member private get_ObjCodec_V1 () =
        codec {
            let! _version = reqWith Codecs.int "__v1" (fun _ -> Some 0)
            and! id = reqWith codecFor<_, JobId> "Id" (fun x -> Some x.Id)
            and! createdOn = reqWith Codecs.dateTimeOffset "CreatedOn" (fun x -> Some x.CreatedOn)
            and! body = reqWith codecFor<_, JobBodyVariant> "Body" (fun x -> Some x.Body)
            return {
                Id = id
                CreatedOn = createdOn
                Body = body
             }
        }

    static member private get_ObjCodec () = Job.get_ObjCodec_V1 ()
    static member get_Codec () = ofObjCodec <| Job.get_ObjCodec ()
    static member Init (typeLabel: string, _typeParams: _) = initializeInterfaceImplementation<Subject<JobId>, Job> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| Job.get_ObjCodec ())



type JobConstructorCommonData with
    static member private get_ObjCodec_V1 () =
        codec {
            let! _version = reqWith Codecs.int "__v1" (fun _ -> Some 1)
            and! payload = reqWith codecFor<_, JobPayload> "Payload" (fun x -> Some x.Payload)
            and! displayName = reqWith codecFor<_, NonemptyString> "DisplayName" (fun x -> Some x.DisplayName)
            and! queueName = reqWith codecFor<_, NonemptyString> "QueueName" (fun x -> Some x.QueueName)
            and! queueSortOrder = reqWith Codecs.uint16 "QueueSortOrder" (fun x -> Some x.QueueSortOrder)
            and! failurePolicy = reqWith codecFor<_, JobFailurePolicy> "FailurePolicy" (fun x -> Some x.FailurePolicy)
            and! maybeSentOn = optWith Codecs.dateTimeOffset "SentOn" (fun x -> Some x.SentOn)
            and! correlationId = optWith Codecs.string "CorrelationId" (fun x -> x.CorrelationId)
            return {
                Payload = payload
                DisplayName = displayName
                QueueName = queueName
                QueueSortOrder = queueSortOrder
                FailurePolicy = failurePolicy
                SentOn = maybeSentOn |> Option.defaultValue DateTimeOffset.UnixEpoch
                CorrelationId = correlationId
             }
        }
    static member get_Codec () = ofObjCodec (JobConstructorCommonData.get_ObjCodec_V1 ())


type JobProcessingResult with
    static member private get_ObjCodec_V4 () =
        codec {
            let! _version = reqWith Codecs.int "__v4" (fun _ -> Some 0)
            and! totalDuration = reqWith Codecs.timeSpan "TotalDuration" (fun x -> Some x.TotalDuration)
            and! pureDuration = optWith Codecs.timeSpan "PureDuration" (fun x -> x.PureDuration)
            and! result = reqWith (Codecs.result Codecs.unit (Codecs.tuple2 Codecs.boolean codecFor<_, JobFailedReason>)) "Result" (fun x -> Some x.Result)
            return { TotalDuration = totalDuration; PureDuration = pureDuration; Result = result }
        }

    static member private get_ObjDecoder_V3 () =
        decoder {
            let! _version = reqDecodeWithCodec Codecs.int "__v3"
            and! jobRun = reqDecodeWithCodec codecFor<_, FinishedJobRun> "Duration"
            and! result = reqDecodeWithCodec (Codecs.result Codecs.unit (Codecs.tuple2 Codecs.boolean codecFor<_, JobFailedReason>)) "Result"
            return { TotalDuration = jobRun.TotalDuration; PureDuration = jobRun.PureDuration; Result = result }
        }

    static member private get_ObjDecoder_V2 () =
        decoder {
            let reqWith = reqDecodeWithCodec
            let! _version = reqWith Codecs.int "__v2"
            and! duration = reqWith Codecs.timeSpan "Duration"
            and! result = reqWith (Codecs.result Codecs.unit (Codecs.tuple2 Codecs.boolean codecFor<_, JobFailedReason>)) "Result"
            return { TotalDuration = duration; PureDuration = Some duration; Result = result }
        }

    static member private get_ObjDecoder_V1 () =
        decoder {
            let! _version = reqDecodeWithCodec Codecs.int "__v1"
            and! duration = reqDecodeWithCodec Codecs.timeSpan "Duration"
            and! result = reqDecodeWithCodec (Codecs.result Codecs.unit codecFor<_, JobFailedReason>) "Result"
            return { TotalDuration = duration; PureDuration = Some duration; Result = result |> Result.mapError (fun err -> (* retryForFree *) false, err) }
        }

    static member get_Codec () =
        JobProcessingResult.get_ObjCodec_V4 ()
        |> withDecoders [
            JobProcessingResult.get_ObjDecoder_V3 ()
            JobProcessingResult.get_ObjDecoder_V2 ()
            JobProcessingResult.get_ObjDecoder_V1 ()
        ]
        |> ofObjCodec

type ProperJobConstructor with
    static member private get_ObjCodec_AllCases () =
        function
        | Enqueued _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Enqueued _ -> Some 0 | _ -> None)
                and! payload = reqWith (Codecs.tuple2 codecFor<_, JobConstructorCommonData> codecFor<_, JobScope>) "Enqueued" (function Enqueued (x1, x2) -> Some (x1, x2) | _ -> None)
                return Enqueued payload
            }
        | Scheduled _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Scheduled _ -> Some 0 | _ -> None)
                and! payload = reqWith (Codecs.tuple2 codecFor<_, JobConstructorCommonData> Codecs.dateTimeOffset) "Scheduled" (function Scheduled (x1, x2) -> Some (x1, x2) | _ -> None)
                return Scheduled payload
            }
        | Awaiting _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Awaiting _ -> Some 0 | _ -> None)
                and! payload = reqWith (Codecs.tuple3 codecFor<_, JobConstructorCommonData> codecFor<_, JobScope> codecFor<_, JobParent>) "Awaiting" (function Awaiting (x1, x2, x3) -> Some (x1, x2, x3) | _ -> None)
                return Awaiting payload
            }
        | Deleted _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Deleted _ -> Some 0 | _ -> None)
                and! payload = reqWith (Codecs.tuple2 codecFor<_, JobConstructorCommonData> codecFor<_, JobScope>) "Deleted" (function Deleted (x1, x2) -> Some (x1, x2) | _ -> None)
                return Deleted payload
            }
        |> mergeUnionCases
    static member get_Codec () = ofObjCodec (ProperJobConstructor.get_ObjCodec_AllCases ())


type JobSubscriber with
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
    static member get_Codec () = ofObjCodec (JobSubscriber.get_ObjCodec_AllCases ())


type JobAction with
    static member TypeLabel () = "JobAction"

    static member private get_ObjCodec_AllCases () =
        function
        | Schedule _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Schedule _ -> Some 0 | _ -> None)
                and! payload = reqWith Codecs.dateTimeOffset "Schedule" (function Schedule x -> Some x | _ -> None)
                return Schedule payload
            }
        | Enqueue ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Enqueue -> Some 0 | _ -> None)
                and! _ = reqWith Codecs.unit "Enqueue" (function Enqueue -> Some () | _ -> None)
                return Enqueue
            }
        | Start _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Start _ -> Some 0 | _ -> None)
                and! payload = reqWith codecFor<_, DispatcherId> "Start" (function Start x -> Some x | _ -> None)
                return Start payload
            }
        | OnProcessingComplete _ ->
            codec {
                let! _version = reqWith Codecs.int "__v2" (function OnProcessingComplete _ -> Some 0 | _ -> None)
                and! payload = reqWith (Codecs.tuple2 Codecs.guid codecFor<_, JobProcessingResult>) "OnProcessingComplete" (function OnProcessingComplete (x1, x2) -> Some (x1, x2) | _ -> None)
                return OnProcessingComplete payload
            }
            |> withDecoders [
                decoder {
                    let! _version = reqDecodeWithCodec Codecs.int "__v1"
                    and! ticket, res = reqDecodeWithCodec (Codecs.tuple2 Codecs.guid (Codecs.result Codecs.unit codecFor<_, JobFailedReason>)) "OnProcessingComplete"
                    return OnProcessingComplete (ticket, { TotalDuration = TimeSpan.Zero; PureDuration = Some TimeSpan.Zero; Result = res |> Result.mapError (fun err -> (* retryForFree *) false, err)  })
                }
            ]
        | OnHeartbeat _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function OnHeartbeat _ -> Some 0 | _ -> None)
                and! payload = reqWith Codecs.guid "OnHeartbeat" (function OnHeartbeat x -> Some x | _ -> None)
                return OnHeartbeat payload
            }
        | OnParentJobUpdate _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function OnParentJobUpdate _ -> Some 0 | _ -> None)
                and! payload = reqWith codecFor<_, JobStatus> "OnParentJobUpdate" (function OnParentJobUpdate x -> Some x | _ -> None)
                return OnParentJobUpdate payload
            }
        | OnParentBatchUpdate _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function OnParentBatchUpdate _ -> Some 0 | _ -> None)
                and! payload = reqWith codecFor<_, BatchStatus> "OnParentBatchUpdate" (function OnParentBatchUpdate x -> Some x | _ -> None)
                return OnParentBatchUpdate payload
            }
        | Delete ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Delete -> Some 0 | _ -> None)
                and! _ = reqWith Codecs.unit "Delete" (function Delete -> Some () | _ -> None)
                return Delete
            }
        | FillPlaceholder _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function FillPlaceholder _ -> Some 0 | _ -> None)
                and! payload = reqWith codecFor<_, ProperJobConstructor> "FillPlaceholder" (function FillPlaceholder x -> Some x | _ -> None)
                return FillPlaceholder payload
            }
        | OnNewSubscriber _ ->
            codec {
                let! _version = reqWith Codecs.int "__v2" (function OnNewSubscriber _ -> Some 0 | _ -> None)
                and! payload = reqWith codecFor<_, JobSubscriber> "OnNewSubscriber" (function OnNewSubscriber x -> Some x | _ -> None)
                return OnNewSubscriber payload
            }
            |> withDecoders [
                decoder {
                    let! _version = reqDecodeWithCodec Codecs.int "__v1"
                    and! payload = reqDecodeWithCodec codecFor<_, JobId> "OnNewSubscriber"
                    return OnNewSubscriber (JobSubscriber.Job payload)
                }
            ]
        | DeleteAwaitingJobsBackwards ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function DeleteAwaitingJobsBackwards -> Some 0 | _ -> None)
                and! _ = reqWith Codecs.unit "DeleteAwaitingJobsBackwards" (function DeleteAwaitingJobsBackwards -> Some () | _ -> None)
                return DeleteAwaitingJobsBackwards
            }

        |> mergeUnionCases

    static member private get_ObjCodec () = JobAction.get_ObjCodec_AllCases ()
    static member get_Codec () = ofObjCodec <| JobAction.get_ObjCodec ()
    static member Init (typeLabel: string, _typeParams: _) = initializeInterfaceImplementation<LifeAction, JobAction> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| JobAction.get_ObjCodec ())



type JobOpError with
    static member TypeLabel () = "JobOpError"

    static member private get_ObjCodec_AllCases () =
        function
        | UnableToConstructBody _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function UnableToConstructBody _ -> Some 0 | _ -> None)
                and! payload = reqWith Codecs.string "UnableToConstructBody" (function UnableToConstructBody x -> Some x | _ -> None)
                return UnableToConstructBody payload
            }
        | CannotDeleteSuccessfulJob ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function CannotDeleteSuccessfulJob -> Some 0 | _ -> None)
                and! _ = reqWith Codecs.unit "CannotDeletedSuccessfulJob" (function CannotDeleteSuccessfulJob -> Some () | _ -> None)
                return CannotDeleteSuccessfulJob
            }
        |> mergeUnionCases

    static member private get_ObjCodec () = JobOpError.get_ObjCodec_AllCases ()
    static member get_Codec () = ofObjCodec <| JobOpError.get_ObjCodec ()
    static member Init (typeLabel: string, _typeParams: _) = initializeInterfaceImplementation<OpError, JobOpError> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| JobOpError.get_ObjCodec ())



type JobConstructor with
    static member TypeLabel () = "JobConstructor"

    static member private get_ObjCodec_AllCases () =
        function
        | NewPlaceholder _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function NewPlaceholder _ -> Some 0 | _ -> None)
                and! payload = reqWith (Codecs.tuple2 codecFor<_, JobId> codecFor<_, PlacedBy>) "NewPlaceholder" (function NewPlaceholder (x1, x2) -> Some (x1, x2) | _ -> None)
                return NewPlaceholder payload
            }
        | NewProper _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function NewProper _ -> Some 0 | _ -> None)
                and! payload = reqWith (Codecs.tuple2 codecFor<_, JobId> codecFor<_, ProperJobConstructor>) "NewProper" (function NewProper (x1, x2) -> Some (x1, x2) | _ -> None)
                return NewProper payload
            }
        |> mergeUnionCases

    static member private get_ObjCodec () = JobConstructor.get_ObjCodec_AllCases ()
    static member get_Codec () = ofObjCodec <| JobConstructor.get_ObjCodec ()
    static member Init (typeLabel: string, _typeParams: _) = initializeInterfaceImplementation<Constructor, JobConstructor> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| JobConstructor.get_ObjCodec ())



type JobLifeEvent with
    static member TypeLabel () = "Jobs_JobLifeEvent"

    static member private get_ObjCodec_AllCases () =
        function
        | OnJobStatusChanged _ ->
            codec {
                let! _version = reqWith Codecs.int "__v2" (function OnJobStatusChanged _ -> Some 0 | _ -> None)
                and! payload = reqWith (Codecs.tuple2 codecFor<_, JobStatus> (Codecs.option codecFor<_, JobSubscriber>)) "OnJobStatusChanged" (function OnJobStatusChanged (x1, x2) -> Some (x1, x2) | _ -> None)
                return OnJobStatusChanged payload
            }
            |> withDecoders [
                decoder {
                    let! _version = reqDecodeWithCodec Codecs.int "__v1"
                    and! status, maybeJobId = reqDecodeWithCodec (Codecs.tuple2 codecFor<_, JobStatus> (Codecs.option codecFor<_, JobId>)) "OnJobStatusChanged"
                    return OnJobStatusChanged (status, maybeJobId |> Option.map JobSubscriber.Job)
                }
            ]
        | OnProcessingCompleted ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function OnProcessingCompleted -> Some 0 | _ -> None)
                and! _ = reqWith Codecs.unit "OnProcessingCompleted" (function OnProcessingCompleted -> Some () | _ -> None)
                return OnProcessingCompleted
            }
        |> mergeUnionCases

    static member private get_ObjCodec () = JobLifeEvent.get_ObjCodec_AllCases ()
    static member get_Codec () = ofObjCodec <| JobLifeEvent.get_ObjCodec ()
    static member Init (typeLabel: string, _typeParams: _) = initializeInterfaceImplementation<LifeEvent, JobLifeEvent> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| JobLifeEvent.get_ObjCodec ())

#endif // !FABLE_COMPILER