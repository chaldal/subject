namespace LibLifeCycleCore

open Orleans
open System.Runtime.Serialization
open System.Threading.Tasks
open System

[<Obsolete("Use PermanentSubjectException or TransientSubjectException instead. Delete it when biosphere is fully upgraded", (* error *) false)>]
[<Serializable>]
type SubjectException =
    inherit Exception
    // can't wrap typed Exception because if inner exception has type unknown to client
    // then client will not deserialize it and exception details will be lost
    new (exceptionSource: string, methodName: string, innerExceptionDetails: string) =
        // TODO: review innerExceptionDetails if this exception will ever be served to a public client, it's not secure.
        { inherit Exception ($"`%s{exceptionSource}` threw exception in method `%s{methodName}`:\n%s{innerExceptionDetails}") }
    new (info: SerializationInfo, context: StreamingContext) = { inherit Exception (info, context) }


[<RequireQualifiedAccess>]
type GrainGetError =
| AccessDenied

[<RequireQualifiedAccess>]
type GrainConstructionError<'OpError when 'OpError :> OpError> =
| SubjectAlreadyInitialized of PrimaryKey: string
| ConstructionError of 'OpError
| AccessDenied

[<RequireQualifiedAccess>]
type GrainIdGenerationError<'OpError when 'OpError :> OpError> =
| IdGenerationError of 'OpError

[<RequireQualifiedAccess>]
type GrainMaybeConstructionError<'OpError when 'OpError :> OpError> =
| ConstructionError of 'OpError
| InitializingInTransaction
| AccessDenied

[<RequireQualifiedAccess>]
type GrainTransitionError<'OpError when 'OpError :> OpError> =
| SubjectNotInitialized of PrimaryKey: string
| TransitionError of 'OpError
| TransitionNotAllowed
| LockedInTransaction
| AccessDenied

[<RequireQualifiedAccess>]
type GrainTriggerTimerError<'OpError, 'LifeAction when 'OpError :> OpError and 'LifeAction :> LifeAction> =
| SubjectNotInitialized of PrimaryKey: string
| TransitionError of 'OpError * 'LifeAction
| TransitionNotAllowed of 'LifeAction
| LockedInTransaction
| Exn of ExceptionDetails: string * Option<'LifeAction>

[<RequireQualifiedAccess>]
type GrainOperationError<'OpError when 'OpError :> OpError> =
| ConstructionError of 'OpError
| TransitionError   of 'OpError
| TransitionNotAllowed
| LockedInTransaction
| AccessDenied

[<RequireQualifiedAccess>]
type GrainExecutionError<'OpError when 'OpError :> OpError> =
| ExecutionError of 'OpError
| AccessDenied

[<RequireQualifiedAccess>]
type SessionHandle =
| NoSession
// SessionId as well as authenticated user data must match between client and server for better security,
// sessionId alone is not secure because it doesn't change after logout (in Subject stack it's machine-specific)
| Session of string * AccessUserId

type ClientGrainCallContext =
    {
        SessionHandle: SessionHandle
        CallOrigin: CallOrigin
    }

type ILifeEventAwaiter<'Subject, 'LifeEvent, 'SubjectId
                        when 'Subject   :> Subject<'SubjectId>
                        and  'LifeEvent :> LifeEvent
                        and  'SubjectId :> SubjectId
                        and  'SubjectId : comparison> =
    inherit IGrainObserver
    abstract member EventTriggered: versionedSubject: VersionedSubject<'Subject, 'SubjectId> -> lifeEvent: 'LifeEvent -> unit

type AwaiterWithTimeout<'Subject, 'LifeEvent, 'SubjectId when 'Subject :> Subject<'SubjectId> and 'LifeEvent :> LifeEvent and 'SubjectId :> SubjectId and 'SubjectId : comparison> = {
    Awaiter: ILifeEventAwaiter<'Subject, 'LifeEvent, 'SubjectId>
    ExpiryTime: DateTimeOffset
}

type IViewClientGrain<'Input, 'Output, 'OpError
    when 'Input :> ViewInput<'Input>
    and  'Output :> ViewOutput<'Output>
    and 'OpError :> ViewOpError<'OpError>
    and 'OpError :> OpError> =
    inherit IGrainWithGuidCompoundKey

    [<Orleans.Concurrency.ReadOnly>]
    abstract member Read: clientGrainCallContext: ClientGrainCallContext -> input: 'Input -> Task<Result<'Output, GrainExecutionError<'OpError>>>

/// Subset of ISubjectGrain that is available to out-of-process clients
type ISubjectClientGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId
                    when 'Subject              :> Subject<'SubjectId>
                    and  'LifeAction           :> LifeAction
                    and  'OpError              :> OpError
                    and  'Constructor          :> Constructor
                    and  'LifeEvent            :> LifeEvent
                    and  'LifeEvent            :  comparison
                    and  'SubjectId            :> SubjectId
                    and  'SubjectId            :  comparison> =
    inherit IGrainWithGuidCompoundKey

    abstract member Construct: clientGrainCallContext: ClientGrainCallContext -> subjectId: 'SubjectId -> ctor: 'Constructor -> Task<Result<VersionedSubject<'Subject, 'SubjectId>, GrainConstructionError<'OpError>>>
    abstract member ConstructNoContent: clientGrainCallContext: ClientGrainCallContext -> subjectId: 'SubjectId -> ctor: 'Constructor -> Task<Result<unit, GrainConstructionError<'OpError>>>
    abstract member ConstructAndWait: clientGrainCallContext: ClientGrainCallContext -> subjectId: 'SubjectId -> ctor: 'Constructor -> lifeEventToAwaitOn: 'LifeEvent -> awaiter: ILifeEventAwaiter<'Subject, 'LifeEvent, 'SubjectId> -> waitTimeout: TimeSpan -> Task<Result<VersionedSubject<'Subject, 'SubjectId>, GrainConstructionError<'OpError>>>
    abstract member Act: clientGrainCallContext: ClientGrainCallContext -> action: 'LifeAction -> Task<Result<VersionedSubject<'Subject, 'SubjectId>, GrainTransitionError<'OpError>>>
    abstract member ActNoContent: clientGrainCallContext: ClientGrainCallContext -> action: 'LifeAction -> Task<Result<unit, GrainTransitionError<'OpError>>>
    abstract member ActAndWait: clientGrainCallContext: ClientGrainCallContext -> action: 'LifeAction -> lifeEventToAwaitOn: 'LifeEvent -> awaiter: ILifeEventAwaiter<'Subject, 'LifeEvent, 'SubjectId> -> waitTimeout: TimeSpan -> Task<Result<VersionedSubject<'Subject, 'SubjectId>, GrainTransitionError<'OpError>>>
    abstract member ActMaybeConstruct: clientGrainCallContext: ClientGrainCallContext -> action: 'LifeAction -> ctor: 'Constructor -> Task<Result<VersionedSubject<'Subject, 'SubjectId>, GrainOperationError<'OpError>>>
    abstract member ActMaybeConstructNoContent: clientGrainCallContext: ClientGrainCallContext -> action: 'LifeAction -> ctor: 'Constructor -> Task<Result<unit, GrainOperationError<'OpError>>>
    abstract member ActMaybeConstructAndWait: clientGrainCallContext: ClientGrainCallContext -> action: 'LifeAction -> ctor: 'Constructor -> lifeEventToAwaitOn: 'LifeEvent -> awaiter: ILifeEventAwaiter<'Subject, 'LifeEvent, 'SubjectId> -> waitTimeout: TimeSpan -> Task<Result<VersionedSubject<'Subject, 'SubjectId>, GrainOperationError<'OpError>>>
    abstract member GetMaybeConstruct: clientGrainCallContext: ClientGrainCallContext -> subjectId: 'SubjectId -> ctor: 'Constructor -> Task<Result<VersionedSubject<'Subject, 'SubjectId>, GrainMaybeConstructionError<'OpError>>>
    abstract member MaybeConstructNoContent: clientGrainCallContext: ClientGrainCallContext -> subjectId: 'SubjectId -> ctor: 'Constructor -> Task<Result<unit, GrainMaybeConstructionError<'OpError>>>

    [<Orleans.Concurrency.ReadOnly>]
    abstract member Get: clientGrainCallContext: ClientGrainCallContext -> Task<Result<Option<VersionedSubject<'Subject, 'SubjectId>>, GrainGetError>>

    [<Orleans.Concurrency.ReadOnly>]
    abstract member Prepared: clientGrainCallContext: ClientGrainCallContext -> transactionId: SubjectTransactionId -> Task<Result<Option<'Subject>, GrainGetError>>

    [<Orleans.Concurrency.ReadOnly>]
    abstract member IsConstructed: unit -> Task<bool>


/// a stateless grain for out-of-process clients that can't use LifeCycle.IdGeneration directly because it requires the environment
type ISubjectIdGenerationGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId
                    when 'Subject              :> Subject<'SubjectId>
                    and  'LifeAction           :> LifeAction
                    and  'OpError              :> OpError
                    and  'Constructor          :> Constructor
                    and  'LifeEvent            :> LifeEvent
                    and  'LifeEvent            :  comparison
                    and  'SubjectId            :> SubjectId
                    and  'SubjectId            :  comparison> =
    inherit IGrainWithGuidCompoundKey
    [<Orleans.Concurrency.ReadOnly>]
    abstract member GenerateIdV2: callOrigin: CallOrigin -> ctor: 'Constructor -> Task<Result<'SubjectId, GrainIdGenerationError<'OpError>>>

    // Obsoleted on 27th June, 2023. Can be removed after some period of time when all backends rolled out. Then we can rename its
    // replacement method after a similar delay.
    [<Obsolete>]
    [<Orleans.Concurrency.ReadOnly>]
    abstract member GenerateId: ctor: 'Constructor -> Task<Result<'SubjectId, GrainIdGenerationError<'OpError>>>

/// a stateless grain for out-of-process clients that can't use ISubjectRepo directly because it requires SQL connection
type ISubjectRepoGrain<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'SubjectIndex, 'OpError
                    when 'Subject      :> Subject<'SubjectId>
                    and  'LifeAction   :> LifeAction
                    and  'Constructor  :> Constructor
                    and  'SubjectIndex :> SubjectIndex<'OpError>
                    and  'SubjectId    :> SubjectId
                    and  'SubjectId    :  comparison
                    and  'OpError      :> OpError> =
    inherit IGrainWithGuidCompoundKey
    [<Orleans.Concurrency.ReadOnly>]
    abstract member Versioned_GetByIdsStr:                       ids: Set<string> -> Task<List<VersionedSubject<'Subject, 'SubjectId>>>

    [<Orleans.Concurrency.ReadOnly>]
    abstract member Versioned_GetByIds:                          ids: Set<'SubjectId> -> Task<List<VersionedSubject<'Subject, 'SubjectId>>>

    [<Orleans.Concurrency.ReadOnly>]
    abstract member Any:                                         predicate: PreparedIndexPredicate<'SubjectIndex> -> Task<bool>

    [<Orleans.Concurrency.ReadOnly>]
    abstract member Versioned_FilterFetchSubjects:               query: IndexQuery<'SubjectIndex> -> Task<List<VersionedSubject<'Subject, 'SubjectId>>>

    [<Orleans.Concurrency.ReadOnly>]
    abstract member FilterFetchIds:                              query: IndexQuery<'SubjectIndex> -> Task<List<'SubjectId>>

    [<Orleans.Concurrency.ReadOnly>]
    abstract member FilterCountSubjects:                         predicate: PreparedIndexPredicate<'SubjectIndex> -> Task<uint64>

    [<Orleans.Concurrency.ReadOnly>]
    abstract member Versioned_FilterFetchSubjectsWithTotalCount: query: IndexQuery<'SubjectIndex> -> Task<List<VersionedSubject<'Subject, 'SubjectId>> * uint64>

    [<Orleans.Concurrency.ReadOnly>]
    abstract member Versioned_FetchAllSubjects:                  resultSetOptions: ResultSetOptions<'SubjectIndex> -> Task<List<VersionedSubject<'Subject, 'SubjectId>>>

    [<Orleans.Concurrency.ReadOnly>]
    abstract member FetchAllSubjectsWithTotalCount:              resultSetOptions: ResultSetOptions<'SubjectIndex> -> Task<List<VersionedSubject<'Subject, 'SubjectId>> * uint64>

    [<Orleans.Concurrency.ReadOnly>]
    abstract member CountAllSubjects:                            unit -> Task<uint64>

    [<Orleans.Concurrency.ReadOnly>]
    abstract member GetVersionSnapshotByIdStr:                   idStr: string -> ofVersion: GetSnapshotOfVersion -> Task<Option<TemporalSnapshot<'Subject, 'LifeAction, 'Constructor, 'SubjectId>>>

    [<Orleans.Concurrency.ReadOnly>]
    abstract member GetVersionSnapshotById:                      id: 'SubjectId -> ofVersion: GetSnapshotOfVersion -> Task<Option<TemporalSnapshot<'Subject, 'LifeAction, 'Constructor, 'SubjectId>>>

    [<Orleans.Concurrency.ReadOnly>]
    abstract member FetchWithHistoryById:                        id: 'SubjectId -> fromLastUpdatedOn: Option<DateTimeOffset> -> toLastUpdatedOn: Option<DateTimeOffset> -> page: ResultPage -> Task<List<TemporalSnapshot<'Subject, 'LifeAction, 'Constructor, 'SubjectId>>>

    [<Orleans.Concurrency.ReadOnly>]
    abstract member FetchWithHistoryByIdStr:                     idStr: string -> fromLastUpdatedOn: Option<DateTimeOffset> -> toLastUpdatedOn: Option<DateTimeOffset> -> page: ResultPage -> Task<List<TemporalSnapshot<'Subject, 'LifeAction, 'Constructor, 'SubjectId>>>

    [<Orleans.Concurrency.ReadOnly>]
    abstract member FetchAuditTrail:                             idStr: string -> page: ResultPage -> Task<List<SubjectAuditData<'LifeAction, 'Constructor>>>

    // As of 23rd June, 2023, these are obsolete. They can be removed some time in the future when all
    // suites have seen a rollout. The "Versioned_" prefix in methods above can then be removed (also
    // requires an interim rollout).
    [<Obsolete>]
    [<Orleans.Concurrency.ReadOnly>]
    abstract member GetByIdsStr:                       ids: Set<string> -> Task<List<'Subject>>

    [<Obsolete>]
    [<Orleans.Concurrency.ReadOnly>]
    abstract member GetByIds:                          ids: Set<'SubjectId> -> Task<List<'Subject>>

    [<Obsolete>]
    [<Orleans.Concurrency.ReadOnly>]
    abstract member FilterFetchSubjects:               query: IndexQuery<'SubjectIndex> -> Task<List<'Subject>>

    [<Obsolete>]
    [<Orleans.Concurrency.ReadOnly>]
    abstract member FilterFetchSubjectsWithTotalCount: query: IndexQuery<'SubjectIndex> -> Task<List<'Subject> * uint64>

    [<Obsolete>]
    [<Orleans.Concurrency.ReadOnly>]
    abstract member FetchAllSubjects:                  resultSetOptions: ResultSetOptions<'SubjectIndex> -> Task<List<'Subject>>

/// a stateless grain for out-of-process clients that can't use IBlobRepo directly because it requires SQL connection
type IBlobRepoGrain =
    inherit IGrainWithGuidCompoundKey
    [<Orleans.Concurrency.ReadOnly>]
    abstract member GetBlobData: subjectRef: LocalSubjectPKeyReference -> blobId: Guid -> Task<Option<BlobData>>

// TODO: consider moving to ISubjectClientGrain and do it by 'SubjectId. Pros & Cons ?
/// a stateless grain that can list actions allowed for a subject
type ISubjectReflectionGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId
                    when 'Subject              :> Subject<'SubjectId>
                    and  'LifeAction           :> LifeAction
                    and  'OpError              :> OpError
                    and  'Constructor          :> Constructor
                    and  'LifeEvent            :> LifeEvent
                    and  'LifeEvent            :  comparison
                    and  'SubjectId            :> SubjectId
                    and  'SubjectId            :  comparison> =
    inherit IGrainWithGuidCompoundKey
    [<Orleans.Concurrency.ReadOnly>]
    abstract member IsActionAllowed: subject: 'Subject -> action: 'LifeAction -> Task<bool>

    [<Orleans.Concurrency.ReadOnly>]
    abstract member GetAllowedActionCaseNames: subject: 'Subject -> Task<list<string>>


// Below are the grain types that should really belong to the LibLifeCycleHost but they are still here to configure serialization in one place.
// Deliberately put under the client grain interfaces to make sure they are not accidentally used there.

[<RequireQualifiedAccess>]
type SubjectChange<'Subject, 'SubjectId
        when 'Subject   :> Subject<'SubjectId>
        and  'SubjectId :> SubjectId
        and 'SubjectId : comparison> =
| Updated of VersionedSubject<'Subject, 'SubjectId>
| NotInitialized
with
    member this.MaybeComparableVersion: Option<ComparableVersion> =
        match this with
        | SubjectChange.Updated versionedSubject ->
            (versionedSubject.AsOf.Ticks, versionedSubject.Version)
            |> Some
        | SubjectChange.NotInitialized ->
            None

// TODO: need to be renamed to ConstructOrBeforeActSubscriptions, but it may break ISubjectGrain contract, so not touching
type ConstructSubscriptions = {
    Subscriber: SubjectPKeyReference // subscriber can be from other unreferenced ecosystem, hence weakly typed
    Subscriptions: Map<SubscriptionName, LifeEvent>
}
type BeforeActSubscriptions = ConstructSubscriptions


// When grain called via side-effect, this type helps to capture exception in app code and classify it as permanent failure
// instead of introducing ugly LifeCycleException and alike in public part of api (ISubjectClientGrain)
[<RequireQualifiedAccess>]
type SubjectFailure<'Error> =
| Err of 'Error
| Exn of ExceptionDetails: string
with
    static member CastUnsafe (innerCastUnsafe: 'Error -> 'OtherError) (error: SubjectFailure<'Error>) : SubjectFailure<'OtherError> =
        match error with
        | Err err ->
            err |> innerCastUnsafe |> Err
        | Exn ex ->
            Exn ex


[<RequireQualifiedAccess>]
type GrainPrepareConstructionError<'OpError when 'OpError :> OpError> =
| SubjectAlreadyInitialized of PrimaryKey: string
| ConstructionError of 'OpError
| ConflictingPrepare of SubjectTransactionId

[<RequireQualifiedAccess>]
type GrainPrepareTransitionError<'OpError when 'OpError :> OpError> =
| TransitionError of 'OpError
| TransitionNotAllowed
| SubjectNotInitialized of PrimaryKey: string
| ConflictingPrepare of SubjectTransactionId

[<RequireQualifiedAccess>]
type GrainSubscriptionError =
| SubjectNotInitialized of PrimaryKey: string

[<RequireQualifiedAccess>]
type GrainEnqueueActionError =
| SubjectNotInitialized of PrimaryKey: string
| LockedInTransaction

[<RequireQualifiedAccess>]
type GrainTriggerSubscriptionError<'OpError when 'OpError :> OpError> =
| SubjectNotInitialized of PrimaryKey: string
// TODO: retire TransitionError and TransitionNotAllowed when biosphere upgraded - they will be caught on Subscriber side and set aside for the subscriber's response handler
| TransitionError of 'OpError
| TransitionNotAllowed
| LockedInTransaction

[<RequireQualifiedAccess>]
type GrainTriggerDynamicSubscriptionError =
| SubjectNotInitialized of PrimaryKey: string
// TODO: retire TransitionError and TransitionNotAllowed when biosphere upgraded - they will be caught on Subscriber side and set aside for the subscriber's response handler
| TransitionError of UntypedOpError: string
| TransitionNotAllowed
| LockedInTransaction
| LifeCycleNotFound of LifeCycleName: string

[<RequireQualifiedAccess>]
type GrainRefreshTimersAndSubsError =
| SubjectNotInitialized of PrimaryKey: string

// Helps to deduplicate possible second attempt after a transient error on a side-effect that actually succeeded.
// Duplicate side-effect invocations results in Noop.
// Passed to some inter-grain operations that are not already idempotent.
// The auto-dedup idea based on assumptions listed below, and **WILL BREAK** if any of them become invalid:
// * Caller executes all its side-effects sequentially, never dequeues next SE until heard 100% Ok or Error from current
// * Caller is not interested in Result.Ok payload details (which is usually just unit)
// * If target (callee) fails with a domain error it does not write dedup info into storage (or anything else for that matter)
type SideEffectDedupInfo = {
    Id: Guid // unique Id of the call, usually equal to GrainSideEffectId (defined in LibLifeCycleHost)
    Caller: SubjectPKeyReference
}

// Codecs & casts

#if !FABLE_COMPILER

open CodecLib

type GrainGetError
with
    static member get_Codec () =
        function
        | AccessDenied ->
            codec {
                let! _ = reqWith Codecs.unit "AccessDenied" (function AccessDenied -> Some ())
                return AccessDenied
            }
        |> mergeUnionCases
        |> ofObjCodec

type GrainConstructionError<'OpError when 'OpError :> OpError>
with
    static member inline get_Codec () =
        function
        | SubjectAlreadyInitialized _ ->
            codec {
                let! payload = reqWith Codecs.string "SubjectAlreadyInitialized" (function (SubjectAlreadyInitialized x) -> Some x | _ -> None)
                return SubjectAlreadyInitialized payload
            }
        | ConstructionError _ ->
            codec {
                let! payload = reqWith defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'opError> "ConstructionError" (function (ConstructionError x) -> Some x | _ -> None)
                return ConstructionError payload
            }
        | AccessDenied ->
            codec {
                let! _ = reqWith Codecs.unit "AccessDenied" (function AccessDenied -> Some () | _ -> None)
                return AccessDenied
            }
        |> mergeUnionCases
        |> ofObjCodec

    static member CastUnsafe(error: GrainConstructionError<#OpError>) : GrainConstructionError<'OpError> =
        match error with
        | SubjectAlreadyInitialized pk ->
            SubjectAlreadyInitialized pk
        | ConstructionError err ->
            ConstructionError (err |> box :?> 'OpError)
        | AccessDenied ->
            AccessDenied

type GrainIdGenerationError<'OpError when 'OpError :> OpError>
with
    static member inline get_Codec () =
        function
        | IdGenerationError _ ->
            codec {
                let! payload = reqWith defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'opError> "IdGenerationError" (function (IdGenerationError x) -> Some x)
                return IdGenerationError payload
            }
        |> mergeUnionCases
        |> ofObjCodec

    static member CastUnsafe(error: GrainIdGenerationError<#OpError>) : GrainIdGenerationError<'OpError> =
        match error with
        | IdGenerationError err ->
            IdGenerationError (err |> box :?> 'OpError)

type GrainMaybeConstructionError<'OpError when 'OpError :> OpError>
with
    static member inline get_Codec () =
        function
        | ConstructionError _ ->
            codec {
                let! payload = reqWith defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'opError> "ConstructionError" (function (ConstructionError x) -> Some x | _ -> None)
                return ConstructionError payload
            }
        | InitializingInTransaction ->
            codec {
                let! _ = reqWith Codecs.unit "InitializingInTransaction" (function InitializingInTransaction -> Some () | _ -> None)
                return InitializingInTransaction
            }
        | AccessDenied ->
            codec {
                let! _ = reqWith Codecs.unit "AccessDenied" (function AccessDenied -> Some () | _ -> None)
                return AccessDenied
            }
        |> mergeUnionCases
        |> ofObjCodec

    static member CastUnsafe(error: GrainMaybeConstructionError<#OpError>) : GrainMaybeConstructionError<'OpError> =
        match error with
        | ConstructionError err ->
            ConstructionError (err |> box :?> 'OpError)
        | InitializingInTransaction ->
            InitializingInTransaction
        | AccessDenied ->
            AccessDenied

type GrainTransitionError<'OpError when 'OpError :> OpError>
with
    static member inline get_Codec () =
        function
        | SubjectNotInitialized _ ->
            codec {
                let! payload = reqWith Codecs.string "SubjectNotInitialized" (function (SubjectNotInitialized x) -> Some x | _ -> None)
                return SubjectNotInitialized payload
            }
        | TransitionError _ ->
            codec {
                let! payload = reqWith defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'opError> "TransitionError" (function (TransitionError x) -> Some x | _ -> None)
                return TransitionError payload
            }
        | TransitionNotAllowed ->
            codec {
                let! _ = reqWith Codecs.unit "TransitionNotAllowed" (function TransitionNotAllowed -> Some () | _ -> None)
                return TransitionNotAllowed
            }
        | LockedInTransaction ->
            codec {
                let! _ = reqWith Codecs.unit "LockedInTransaction" (function LockedInTransaction -> Some () | _ -> None)
                return LockedInTransaction
            }
        | AccessDenied ->
            codec {
                let! _ = reqWith Codecs.unit "AccessDenied" (function AccessDenied -> Some () | _ -> None)
                return AccessDenied
            }
        |> mergeUnionCases
        |> ofObjCodec

    static member CastUnsafe(error: GrainTransitionError<#OpError>) : GrainTransitionError<'OpError> =
        match error with
        | SubjectNotInitialized pk ->
            SubjectNotInitialized pk
        | TransitionError err ->
            TransitionError (err |> box :?> 'OpError)
        | TransitionNotAllowed ->
            TransitionNotAllowed
        | LockedInTransaction ->
            LockedInTransaction
        | AccessDenied ->
            AccessDenied

type GrainTriggerTimerError<'OpError, 'LifeAction when 'OpError :> OpError and 'LifeAction :> LifeAction>
with
    static member inline get_Codec () =
        function
        | SubjectNotInitialized _ ->
            codec {
                let! payload = reqWith Codecs.string "SubjectNotInitialized" (function (SubjectNotInitialized x) -> Some x | _ -> None)
                return SubjectNotInitialized payload
            }
        | TransitionError _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'opError> defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'lifeAction>) "TransitionError" (function TransitionError (x1, x2) -> Some (x1, x2) | _ -> None)
                return TransitionError payload
            }
        | TransitionNotAllowed _ ->
            codec {
                let! payload = reqWith defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'lifeAction> "TransitionNotAllowed" (function TransitionNotAllowed x -> Some x | _ -> None)
                return TransitionNotAllowed payload
            }
        | LockedInTransaction ->
            codec {
                let! _ = reqWith Codecs.unit "LockedInTransaction" (function LockedInTransaction -> Some () | _ -> None)
                return LockedInTransaction
            }
        | Exn _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 Codecs.string (Codecs.option defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'lifeAction>)) "Exn" (function Exn (x1, x2) -> Some (x1, x2) | _ -> None)
                return Exn payload
            }
        |> mergeUnionCases
        |> ofObjCodec

    static member CastUnsafe(error: GrainTriggerTimerError<#OpError, #LifeAction>) : GrainTriggerTimerError<'OpError, 'LifeAction> =
        match error with
        | SubjectNotInitialized pk ->
            SubjectNotInitialized pk
        | TransitionError (err, action) ->
            TransitionError (err |> box :?> 'OpError, action |> box :?> 'LifeAction)
        | TransitionNotAllowed action ->
            TransitionNotAllowed (action |> box :?> 'LifeAction)
        | LockedInTransaction ->
            LockedInTransaction
        | Exn (details, maybeAction) ->
            Exn (details, maybeAction |> Option.map (fun action -> action |> box :?> 'LifeAction))

type GrainOperationError<'OpError when 'OpError :> OpError>
with
    static member inline get_Codec () =
        function
        | ConstructionError _ ->
            codec {
                let! payload = reqWith defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'opError> "ConstructionError" (function (ConstructionError x) -> Some x | _ -> None)
                return ConstructionError payload
            }
        | TransitionError _ ->
            codec {
                let! payload = reqWith defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'opError> "TransitionError" (function (TransitionError x) -> Some x | _ -> None)
                return TransitionError payload
            }
        | TransitionNotAllowed ->
            codec {
                let! _ = reqWith Codecs.unit "TransitionNotAllowed" (function TransitionNotAllowed -> Some () | _ -> None)
                return TransitionNotAllowed
            }
        | LockedInTransaction ->
            codec {
                let! _ = reqWith Codecs.unit "LockedInTransaction" (function LockedInTransaction -> Some () | _ -> None)
                return LockedInTransaction
            }
        | AccessDenied ->
            codec {
                let! _ = reqWith Codecs.unit "AccessDenied" (function AccessDenied -> Some () | _ -> None)
                return AccessDenied
            }
        |> mergeUnionCases
        |> ofObjCodec

    static member CastUnsafe(error: GrainOperationError<#OpError>) : GrainOperationError<'OpError> =
        match error with
        | ConstructionError err ->
            ConstructionError (err |> box :?> 'OpError)
        | TransitionError err ->
            TransitionError (err |> box :?> 'OpError)
        | TransitionNotAllowed ->
            TransitionNotAllowed
        | LockedInTransaction ->
            LockedInTransaction
        | AccessDenied ->
            AccessDenied

type GrainExecutionError<'OpError when 'OpError :> OpError>
with
    static member CodecWithExplicitOpError (opErrorCodec: Codec<_, 'OpError>) =
        function
        | ExecutionError _ ->
            codec {
                let! payload = reqWith opErrorCodec "ExecutionError" (function (ExecutionError x) -> Some x | _ -> None)
                return ExecutionError payload
            }
        | AccessDenied ->
            codec {
                let! _ = reqWith Codecs.unit "AccessDenied" (function AccessDenied -> Some () | _ -> None)
                return AccessDenied
            }
        |> mergeUnionCases
        |> ofObjCodec

    static member inline get_Codec () =
        GrainExecutionError<'opError>.CodecWithExplicitOpError defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'opError>

    static member CastUnsafe(error: GrainExecutionError<#OpError>) : GrainExecutionError<'OpError> =
        match error with
        | ExecutionError err ->
            ExecutionError (err |> box :?> 'OpError)
        | AccessDenied ->
            AccessDenied

type SessionHandle
with
    static member get_ObjCodec_AllVersions () =
        function
        | NoSession ->
            codec {
                let! _ = reqWith Codecs.unit "NoSession" (function NoSession -> Some () | _ -> None)
                return NoSession
            }
        | Session _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 Codecs.string codecFor<_, AccessUserId>) "Session" (function (Session (x1, x2)) -> Some (x1, x2) | _ -> None)
                return Session payload
            }
            |> withDecoders [
                decoder {
                    let! sessionId = reqDecodeWithCodec Codecs.string "SessionId"
                    return Session (sessionId, Anonymous)
                }
            ]
        |> mergeUnionCases

    static member get_Codec () =
        SessionHandle.get_ObjCodec_AllVersions ()
        |> ofObjCodec

type ClientGrainCallContext with
    static member private get_Decoder_V0 () =
        Codec.create (fun sessionHandle -> Ok { SessionHandle = sessionHandle; CallOrigin = CallOrigin.Internal }) (fun x -> x)
        |> Codec.compose (SessionHandle.get_ObjCodec_AllVersions ())
        |> fun codec -> codec.Decoder

    static member private get_ObjCodec_V1 () =
        codec {
            let! _version = reqWith Codecs.int "__v1" (fun _ -> Some 0)
            and! sessionHandle = reqWith codecFor<_, SessionHandle> "SessionHandle" (fun x -> Some x.SessionHandle)
            and! callOrigin = reqWith codecFor<_, CallOrigin> "CallOrigin" (fun x -> Some x.CallOrigin)
            return {
                SessionHandle = sessionHandle
                CallOrigin = callOrigin
            }
        }

    static member get_Codec () =
        ClientGrainCallContext.get_ObjCodec_V1 ()
        |> withDecoders [ ClientGrainCallContext.get_Decoder_V0 () ]
        |> ofObjCodec

// Below are the grain types that should really belong to the LibLifeCycleHost but they are still here to configure serialization in one place.
// Deliberately put under the client grain interfaces to make sure they are not accidentally used there.


type SubjectChange<'Subject, 'SubjectId
        when 'Subject   :> Subject<'SubjectId>
        and  'SubjectId :> SubjectId>
with
    static member get_Codec () : Codec<'RawEncoding, SubjectChange<Subject<'SubjectId>, 'SubjectId>> =
        function
        | Updated _ ->
            codec {
                let! payload = reqWith codecFor<_, VersionedSubject<Subject<'SubjectId>, 'SubjectId>> "Updated" (function (Updated versionedSubject) -> Some (versionedSubject) | _ -> None)
                return Updated payload
            }
        | NotInitialized ->
            codec {
                let! _ = reqWith Codecs.unit "Deleted" (function NotInitialized -> Some () | _ -> None)
                return NotInitialized
            }
        |> mergeUnionCases
        |> ofObjCodec

    static member CastUnsafe (subjectUpdate: SubjectChange<#Subject<'SubjectId>, 'SubjectId>) : SubjectChange<'Subject, 'SubjectId> =
        match subjectUpdate with
        | Updated subjectUpdate -> subjectUpdate |> VersionedSubject.CastUnsafe |> Updated
        | NotInitialized  -> NotInitialized

type ConstructSubscriptions
with
    static member private get_ObjDecoder_V1 () = decoder {
        let! subscriber = reqDecodeWithCodec (SubjectReference.get_Codec ()) "Subscriber"
        and! subscriptions = reqDecodeWithCodec (Codecs.gmap Codecs.string defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, LifeEvent>) "Subscriptions"
        return { Subscriber = subscriber.SubjectPKeyReference; Subscriptions = subscriptions } }

    static member private get_ObjCodec_V2 () = codec {
        let! subscriber = reqWith (SubjectPKeyReference.get_Codec ()) "Subscriber" (fun x -> Some x.Subscriber)

        // DANGER! do not use (Codecs.gmap Codecs.string defaultCodec<_, LifeEvent>) for the "Subscription" field!!!
        // This does not generate the same json as defaultCodec<_, Map<string, LifeEvent>>
        // This must be a bug because it happens only value type is an interface (like LifeEvent)
        // TODO: report and fix in Fleece? Then what? we already rely on this bug in a few places in framework
        // (search for "Codecs.gmap string defaultCodec)
        // Luckily it's used only inside framework and only for data in motion or for short-living data
        // such as persisted side effects.
        // If Fleece gets fixed, do doubleEncode and rollout biosphere _before_ upgrading to fixed Fleece version.

        and! subscriptions = reqWith defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, Map<string, LifeEvent>> "Subscriptions" (fun x -> Some x.Subscriptions)
        return { Subscriber = subscriber; Subscriptions = subscriptions }
    }

    static member get_Codec () =
        ConstructSubscriptions.get_ObjCodec_V2 ()
        |> withDecoders [ConstructSubscriptions.get_ObjDecoder_V1 ()]
        |> ofObjCodec

type SubjectFailure<'Error>
with
    static member inline get_Codec () =
        function
        | Err _ ->
            codec {
                let! payload = reqWith codecFor<_, 'err> "Err" (function Err x -> Some x | _ -> None)
                return Err payload
            }
        | Exn _ ->
            codec {
                let! payload = reqWith Codecs.string "Exn" (function Exn x -> Some x | _ -> None)
                return Exn payload
            }
        |> mergeUnionCases
        |> ofObjCodec

type GrainPrepareConstructionError<'OpError when 'OpError :> OpError>
with
    static member inline get_Codec () =
        function
        | SubjectAlreadyInitialized _ ->
            codec {
                let! payload = reqWith Codecs.string "SubjectAlreadyInitialized" (function (SubjectAlreadyInitialized x) -> Some x | _ -> None)
                return SubjectAlreadyInitialized payload
            }
        | ConstructionError _ ->
            codec {
                let! payload = reqWith defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'opError> "ConstructionError" (function (ConstructionError x) -> Some x | _ -> None)
                return ConstructionError payload
            }
        | ConflictingPrepare _ ->
            codec {
                let! payload = reqWith (SubjectTransactionId.get_Codec ()) "ConflictingPrepare" (function (ConflictingPrepare x) -> Some x | _ -> None)
                return ConflictingPrepare payload
            }
        |> mergeUnionCases
        |> ofObjCodec

    static member CastUnsafe(error: GrainPrepareConstructionError<#OpError>) : GrainPrepareConstructionError<'OpError> =
        match error with
        | SubjectAlreadyInitialized pk ->
            SubjectAlreadyInitialized pk
        | ConstructionError err ->
            ConstructionError (err |> box :?> 'OpError)
        | ConflictingPrepare transactionId ->
            ConflictingPrepare transactionId

type GrainPrepareTransitionError<'OpError when 'OpError :> OpError>
with
    static member inline get_Codec () =
        function
        | TransitionError _ ->
            codec {
                let! payload = reqWith defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'opError> "TransitionError" (function (TransitionError x) -> Some x | _ -> None)
                return TransitionError payload
            }
        | TransitionNotAllowed ->
            codec {
                let! _ = reqWith Codecs.unit "TransitionNotAllowed" (function TransitionNotAllowed -> Some () | _ -> None)
                return TransitionNotAllowed
            }
        | SubjectNotInitialized _ ->
            codec {
                let! payload = reqWith Codecs.string "SubjectNotInitialized" (function (SubjectNotInitialized x) -> Some x | _ -> None)
                return SubjectNotInitialized payload
            }
        | ConflictingPrepare _ ->
            codec {
                let! payload = reqWith (SubjectTransactionId.get_Codec ()) "ConflictingPrepare" (function (ConflictingPrepare x) -> Some x | _ -> None)
                return ConflictingPrepare payload
            }
        |> mergeUnionCases
        |> ofObjCodec

    static member CastUnsafe(error: GrainPrepareTransitionError<#OpError>) : GrainPrepareTransitionError<'OpError>=
        match error with
        | SubjectNotInitialized pk ->
            SubjectNotInitialized pk
        | TransitionError err ->
            TransitionError (err |> box :?> 'OpError)
        | TransitionNotAllowed ->
            TransitionNotAllowed
        | ConflictingPrepare transactionId ->
            ConflictingPrepare transactionId

type GrainSubscriptionError
with
    static member get_Codec () =
        function
        | SubjectNotInitialized _ ->
            codec {
                let! payload = reqWith Codecs.string "SubjectNotInitialized" (function SubjectNotInitialized x -> Some x)
                return SubjectNotInitialized payload
            }
        |> mergeUnionCases
        |> ofObjCodec

type GrainEnqueueActionError
with
    static member get_Codec () =
        function
        | SubjectNotInitialized _ ->
            codec {
                let! payload = reqWith Codecs.string "SubjectNotInitialized" (function (SubjectNotInitialized x) -> Some x | _ -> None)
                return SubjectNotInitialized payload
            }
        | LockedInTransaction ->
            codec {
                let! _ = reqWith Codecs.unit "LockedInTransaction" (function LockedInTransaction -> Some () | _ -> None)
                return LockedInTransaction
            }
        |> mergeUnionCases
        |> ofObjCodec

type GrainTriggerSubscriptionError<'OpError when 'OpError :> OpError>
with
    static member inline get_Codec () =
        function
        | SubjectNotInitialized _ ->
            codec {
                let! payload = reqWith Codecs.string "SubjectNotInitialized" (function (SubjectNotInitialized x) -> Some x | _ -> None)
                return SubjectNotInitialized payload
            }
        | TransitionError _ ->
            codec {
                let! payload = reqWith defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'opError> "TransitionError" (function (TransitionError x) -> Some x | _ -> None)
                return TransitionError payload
            }
        | TransitionNotAllowed ->
            codec {
                let! _ = reqWith Codecs.unit "TransitionNotAllowed" (function TransitionNotAllowed -> Some () | _ -> None)
                return TransitionNotAllowed
            }
        | LockedInTransaction ->
            codec {
                let! _ = reqWith Codecs.unit "LockedInTransaction" (function LockedInTransaction -> Some () | _ -> None)
                return LockedInTransaction
            }
        |> mergeUnionCases
        |> ofObjCodec

    static member CastUnsafe(error: GrainTriggerSubscriptionError<#OpError>) : GrainTriggerSubscriptionError<'OpError> =
        match error with
        | SubjectNotInitialized pk ->
            SubjectNotInitialized pk
        | TransitionError err ->
            TransitionError (err |> box :?> 'OpError)
        | TransitionNotAllowed ->
            TransitionNotAllowed
        | LockedInTransaction ->
            LockedInTransaction

type GrainTriggerDynamicSubscriptionError
with
    static member inline get_Codec () =
        function
        | SubjectNotInitialized _ ->
            codec {
                let! payload = reqWith Codecs.string "SubjectNotInitialized" (function (SubjectNotInitialized x) -> Some x | _ -> None)
                return SubjectNotInitialized payload
            }
        | TransitionError _ ->
            codec {
                let! payload = reqWith Codecs.string "TransitionError" (function (TransitionError x) -> Some x | _ -> None)
                return TransitionError payload
            }
        | TransitionNotAllowed ->
            codec {
                let! _ = reqWith Codecs.unit "TransitionNotAllowed" (function TransitionNotAllowed -> Some () | _ -> None)
                return TransitionNotAllowed
            }
        | LockedInTransaction ->
            codec {
                let! _ = reqWith Codecs.unit "LockedInTransaction" (function LockedInTransaction -> Some () | _ -> None)
                return LockedInTransaction
            }
        | LifeCycleNotFound _ ->
            codec {
                let! payload = reqWith Codecs.string "LifeCycleNotFound" (function (LifeCycleNotFound x) -> Some x | _ -> None)
                return LifeCycleNotFound payload
            }
        |> mergeUnionCases
        |> ofObjCodec

type GrainRefreshTimersAndSubsError
with
    static member inline get_Codec () =
        function
        | SubjectNotInitialized _ ->
            codec {
                let! payload = reqWith Codecs.string "SubjectNotInitialized" (function (SubjectNotInitialized x) -> Some x)
                return SubjectNotInitialized payload
            }
        |> mergeUnionCases
        |> ofObjCodec

type SideEffectDedupInfo
with
    static member get_Codec () = ofObjCodec <| codec {
        let! id = reqWith Codecs.guid "Id" (fun x -> Some x.Id)
        and! caller = reqWith (SubjectPKeyReference.get_Codec ()) "Caller" (fun x -> Some x.Caller)
        return { Id = id; Caller = caller }
    }

#endif
