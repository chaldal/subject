[<AutoOpen>]
module LibLifeCycle.LifeCycles.Transaction.Types

open System
open LibLifeCycleTypes

[<RequireQualifiedAccess>]
type TransactionRollbackReason =
| Failure of NonemptyString
| Timeout
| Request
| OrphanedBeforePrepare

[<RequireQualifiedAccess>]
type TransactionOutcome =
| RolledBack of TransactionRollbackReason
| Committed

type TxnBatchOpNo = (* BatchNo *) uint16 * (* OpNo *) uint16

type RunningTransactionOp<'Op when 'Op : comparison> = {
    Op: 'Op
    No: TxnBatchOpNo
    Finished: bool
}
with
    interface IKeyed<'Op> with
        member this.Key = this.Op

type IdleTransactionOp<'Op when 'Op : comparison> = {
    Op: 'Op
    No: TxnBatchOpNo
}
with
    interface IKeyed<'Op> with
        member this.Key = this.Op

[<RequireQualifiedAccess>]
type SubjectTransactionState<'Op when 'Op : comparison> =
| Running    of NonemptyKeyedSet<'Op, RunningTransactionOp<'Op>> * PendingRollbackReason: Option<TransactionRollbackReason>
| Prepared   of NonemptyKeyedSet<'Op, IdleTransactionOp<'Op>>
| Finalizing of TransactionOutcome * NonemptyKeyedSet<'Op, RunningTransactionOp<'Op>>
| Finalized  of TransactionOutcome * KeyedSet<'Op, IdleTransactionOp<'Op>> * FinalizedOn: DateTimeOffset * CheckedPhantoms: bool

type SubjectTransaction<'Op when 'Op : comparison> = {
    TransactionId: SubjectTransactionId
    StartedOn:     DateTimeOffset
    Timeout:       TimeSpan
    NextNo:        TxnBatchOpNo
    State:         SubjectTransactionState<'Op>
} with
    interface Subject<SubjectTransactionId> with
        member this.SubjectId = this.TransactionId
        member this.SubjectCreatedOn = this.StartedOn

[<RequireQualifiedAccess>]
type SubjectTransactionAction<'Op when 'Op : comparison> =
| OnOperationPrepared       of OpNo: uint16
| OnOperationFailed         of OpNo: uint16 * Error: NonemptyString
| Continue                  of NonemptySet<'Op>
| Commit
| Rollback                  of TimedOut: bool
// same callback for finalization for both success and error is OK
// implementation guarantees that request to commit or rollback will not leave subject in prepared state,
// as long as we heard _anything_ back it means it's not locked in transaction anymore
| OnOperationFinalized      of OpNo: uint16
| CheckForPhantoms
with
    interface LifeAction

[<RequireQualifiedAccess>]
type SubjectTransactionOpError<'Op when 'Op : comparison> =
| InvalidTransition of SubjectTransactionState<'Op> * SubjectTransactionAction<'Op>
with
    interface OpError

[<RequireQualifiedAccess>]
type SubjectTransactionConstructor<'Op when 'Op : comparison> =
| New of SubjectTransactionId * Operations: NonemptySet<'Op> * Timeout: Option<TimeSpan>
| NewOrphanedBeforePrepare of SubjectTransactionId
with
    interface Constructor

[<RequireQualifiedAccess>]
type SubjectTransactionLifeEvent =
// same event for success and failure so external clients can await for it
| OnPreparedOrFailed of Prepared: bool
| OnFinalized        of TransactionOutcome
with
    interface LifeEvent

type SubjectTransactionNumericIndex<'Op when 'Op : comparison> = private NoIndex of unit
with
    interface SubjectNumericIndex<SubjectTransactionOpError<'Op>> with
        member this.Primitive = shouldNotReachHereBecause "This is a no-index type, and calls should be preempted by the framework"

type SubjectTransactionStringIndex<'Op when 'Op : comparison> = private NoIndex of unit
with
    interface SubjectStringIndex<SubjectTransactionOpError<'Op>> with
        member this.Primitive = shouldNotReachHereBecause "This is a no-index type, and calls should be preempted by the framework"

type SubjectTransactionSearchIndex = private NoIndex of unit
with
    interface SubjectSearchIndex with
        member this.Primitive = shouldNotReachHereBecause "This is a no-index type, and calls should be preempted by the framework"

type SubjectTransactionGeographyIndex = NoGeographyIndex

type SubjectTransactionIndex<'Op when 'Op : comparison> () = inherit SubjectIndex<SubjectTransactionIndex<'Op>, SubjectTransactionNumericIndex<'Op>, SubjectTransactionStringIndex<'Op>, SubjectTransactionSearchIndex, SubjectTransactionGeographyIndex, SubjectTransactionOpError<'Op>>()

[<RequireQualifiedAccess>]
type SubjectTransactionStep =
| Prepare
| Commit
| Rollback
| CheckPhantom

type SubjectTransactionLifeCycleDef<'Op when 'Op : comparison> =
    LifeCycleDef<SubjectTransaction<'Op>, SubjectTransactionAction<'Op>, SubjectTransactionOpError<'Op>, SubjectTransactionConstructor<'Op>, SubjectTransactionLifeEvent, SubjectTransactionIndex<'Op>, SubjectTransactionId>


// CODECs

#if !FABLE_COMPILER

open CodecLib

type TransactionRollbackReason
with
    static member get_Codec () =
        function
        | Failure _ ->
            codec {
                let! payload = reqWith (NonemptyString.get_Codec ()) "Failure" (function Failure x -> Some x | _ -> None)
                return Failure payload
            }
        | Timeout ->
            codec {
                let! _ = reqWith Codecs.unit "Timeout" (function Timeout -> Some () | _ -> None)
                return Timeout
            }
        | Request ->
            codec {
                let! _ = reqWith Codecs.unit "Request" (function Request -> Some () | _ -> None)
                return Request
            }
        | OrphanedBeforePrepare ->
            codec {
                let! _ = reqWith Codecs.unit "OrphanedBeforePrepare" (function OrphanedBeforePrepare -> Some () | _ -> None)
                return OrphanedBeforePrepare
            }
        |> mergeUnionCases
        |> ofObjCodec

type TransactionOutcome
with
    static member get_Codec () =
        function
        | RolledBack _ ->
            codec {
                let! payload = reqWith (TransactionRollbackReason.get_Codec ()) "RolledBack" (function (RolledBack x) -> Some x | _ -> None)
                return RolledBack payload
            }
        | Committed ->
            codec {
                let! _ = reqWith Codecs.unit "Committed" (function Committed -> Some () | _ -> None)
                return Committed
            }
        |> mergeUnionCases
        |> ofObjCodec

type RunningTransactionOp<'Op when 'Op : comparison>
with
    static member inline get_ObjCodec_V1 ()  : Codec<_, RunningTransactionOp<_>> = codec {
        let! op = reqWith codecFor<_, 'op> "Op" (fun x -> Some x.Op)
        and! no = reqWith (Codecs.tuple2 Codecs.uint16 Codecs.uint16) "No" (fun x -> Some x.No)
        and! finished = reqWith Codecs.boolean "Finished" (fun x -> Some x.Finished)
        return { Op = op; No = no; Finished = finished }
    }

    static member inline get_Codec () = ofObjCodec (RunningTransactionOp<_>.get_ObjCodec_V1 ())

type IdleTransactionOp<'Op when 'Op : comparison>
with
    static member inline get_ObjCodec_V1 () : Codec<_, IdleTransactionOp<_>> = codec {
        let! op = reqWith codecFor<_, 'op> "Op" (fun x -> Some x.Op)
        and! no = reqWith (Codecs.tuple2 Codecs.uint16 Codecs.uint16) "No" (fun x -> Some x.No)
        return { Op = op; No = no }
    }

    static member inline get_Codec () = ofObjCodec (IdleTransactionOp<_>.get_ObjCodec_V1 ())

type SubjectTransactionState<'Op when 'Op : comparison>
with
    static member inline get_Codec () : Codec<_, SubjectTransactionState<'op>> =
        function
        | Running _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 (NonemptyKeyedSet.codec codecFor<_, RunningTransactionOp<'op>>) (Codecs.option (TransactionRollbackReason.get_Codec ()))) "Running" (function (Running (x1, x2)) -> Some (x1, x2) | _ -> None)
                return Running payload
            }
        | Prepared _ ->
            codec {
                let! payload = reqWith (NonemptyKeyedSet.codec codecFor<_, IdleTransactionOp<'op>>) "Prepared" (function (Prepared x) -> Some x | _ -> None)
                return Prepared payload
            }
        | Finalizing _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 (TransactionOutcome.get_Codec ()) (NonemptyKeyedSet.codec codecFor<_, RunningTransactionOp<'op>>)) "Finalizing" (function (Finalizing (x1, x2)) -> Some (x1, x2) | _ -> None)
                return Finalizing payload
            }
        | Finalized _  ->
                codec {
                    let! payload = reqWith (Codecs.tuple4 (TransactionOutcome.get_Codec ()) (KeyedSet.codec codecFor<_, IdleTransactionOp<'op>>) Codecs.dateTimeOffset Codecs.boolean) "Finalized" (function (Finalized (x1, x2, x3, x4)) -> Some (x1, x2, x3, x4) | _ -> None)
                    return Finalized payload
                }
                // TODO: can remove this decoder after a week when it's deployed (all transactions will be deleted by then)
                |> withDecoders [
                    decoder {
                        let! (x1, x2, x3) = reqDecodeWithCodec (Codecs.tuple3 (TransactionOutcome.get_Codec ()) (KeyedSet.codec codecFor<_, IdleTransactionOp<'op>>) Codecs.dateTimeOffset) "Finalized"
                        return Finalized (x1, x2, x3, true)
                    }
                ]
        |> mergeUnionCases
        |> ofObjCodec

type SubjectTransaction<'Op when 'Op : comparison>
with
    static member TypeLabel () = "SubjectTransaction"

    static member inline get_ObjCodec_V1 () = codec {
        let! transactionId = reqWith (SubjectTransactionId.get_Codec ()) "TransactionId" (fun x -> Some x.TransactionId)
        and! startedOn = reqWith Codecs.dateTimeOffset "StartedOn" (fun x -> Some x.StartedOn)
        and! timeout = reqWith Codecs.timeSpan "Timeout" (fun x -> Some x.Timeout)
        and! nextNo = reqWith (Codecs.tuple2 Codecs.uint16 Codecs.uint16) "NextNo" (fun x -> Some x.NextNo)
        and! state = reqWith (SubjectTransactionState<_>.get_Codec ()) "State" (fun x -> Some x.State)
        return { TransactionId = transactionId; StartedOn = startedOn; Timeout = timeout; NextNo = nextNo; State = state }
    }

    static member inline get_ObjCodec () = SubjectTransaction<_>.get_ObjCodec_V1 ()

    static member inline get_Codec () = ofObjCodec (SubjectTransaction<_>.get_ObjCodec ())

    static member inline Init (typeLabel: string, _typeParams: 't) =
        initializeInterfaceImplementation<Subject<SubjectTransactionId>, SubjectTransaction<'t>> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| SubjectTransaction<'t>.get_ObjCodec())

type SubjectTransactionAction<'Op when 'Op : comparison>
with
    static member TypeLabel () = "SubjectTransactionAction"
    static member inline get_ObjCodec () =
        function
        | OnOperationPrepared _ ->
            codec {
                let! payload = reqWith Codecs.uint16 "OnOperationPrepared" (function (OnOperationPrepared x) -> Some x | _ -> None)
                return OnOperationPrepared payload
            }
        | OnOperationFailed _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 Codecs.uint16 (NonemptyString.get_Codec())) "OnOperationFailed" (function (OnOperationFailed (x1, x2)) -> Some (x1, x2) | _ -> None)
                return OnOperationFailed payload
            }
        | Continue _ ->
            codec {
                let! payload = reqWith (NonemptySet.codec codecFor<_, 'op>) "Continue" (function (Continue x) -> Some x | _ -> None)
                return Continue payload
            }
        | Commit ->
            codec {
                let! _ = reqWith Codecs.unit "Commit" (function Commit -> Some () | _ -> None)
                return Commit
            }
        | Rollback _ ->
            codec {
                let! payload = reqWith Codecs.boolean "Rollback" (function (Rollback x) -> Some x | _ -> None)
                return Rollback payload
            }
        | OnOperationFinalized _ ->
            codec {
                let! payload = reqWith Codecs.uint16 "OnOperationFinalized" (function (OnOperationFinalized x) -> Some x | _ -> None)
                return OnOperationFinalized payload
            }
        | CheckForPhantoms ->
            codec {
                let! _ = reqWith Codecs.unit "CheckForPhantoms" (function CheckForPhantoms -> Some () | _ -> None)
                return CheckForPhantoms
            }
        |> mergeUnionCases

    static member inline get_Codec () = ofObjCodec (SubjectTransactionAction<_>.get_ObjCodec ())

    static member inline Init (typeLabel: string, _typeParams: 't) =
        initializeInterfaceImplementation<LifeAction, SubjectTransactionAction<'t>> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| SubjectTransactionAction<'t>.get_ObjCodec())

type SubjectTransactionOpError<'Op when 'Op : comparison>
with
    static member TypeLabel () = "SubjectTransactionOpError"

    static member inline get_ObjCodec () =
        function
        | InvalidTransition _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 codecFor<_, SubjectTransactionState<'op>> codecFor<_, SubjectTransactionAction<'op>>) "InvalidTransition" (function (InvalidTransition (x1, x2)) -> Some (x1, x2))
                return InvalidTransition payload
            }
        |> mergeUnionCases

    static member inline get_Codec () = ofObjCodec (SubjectTransactionOpError<_>.get_ObjCodec ())

    static member inline Init (typeLabel: string, _typeParams: 't) =
        initializeInterfaceImplementation<OpError, SubjectTransactionOpError<'t>> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| SubjectTransactionOpError<'t>.get_ObjCodec())

type SubjectTransactionConstructor<'Op when 'Op : comparison>
with
    static member TypeLabel () = "SubjectTransactionConstructor"

    static member inline get_ObjCodec_V1 () =
        function
        | New _ ->
            codec {
                let! payload = reqWith (Codecs.tuple3 codecFor<_, SubjectTransactionId> (NonemptySet.codec codecFor<_, 'op>) (Codecs.option Codecs.timeSpan)) "New" (function (New (x1, x2, x3)) -> Some (x1, x2, x3) | _ -> None)
                return New payload
            }
        | NewOrphanedBeforePrepare _ ->
            codec {
                let! payload = reqWith codecFor<_, SubjectTransactionId> "NewOrphanedBeforePrepare" (function NewOrphanedBeforePrepare x -> Some x | _ -> None)
                return NewOrphanedBeforePrepare payload
            }
        |> mergeUnionCases

    static member inline get_ObjCodec () = SubjectTransactionConstructor<_>.get_ObjCodec_V1 ()

    static member inline get_Codec () = ofObjCodec (SubjectTransactionConstructor<_>.get_ObjCodec ())

    static member inline Init (typeLabel: string, _typeParams: 't) =
        initializeInterfaceImplementation<Constructor, SubjectTransactionConstructor<'t>> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| (SubjectTransactionConstructor<'t>.get_ObjCodec ()))

type SubjectTransactionLifeEvent
with
    static member TypeLabel () = "SubjectTransactionLifeEvent"

    static member get_ObjCodec () =
        function
        | OnFinalized _ ->
            codec {
                let! payload = reqWith (TransactionOutcome.get_Codec ()) "OnFinalized" (function (OnFinalized x) -> Some x | _ -> None)
                return OnFinalized payload
            }
        | OnPreparedOrFailed _ ->
            codec {
                let! payload = reqWith Codecs.boolean "OnPreparedOrFailed" (function (OnPreparedOrFailed x) -> Some x | _ -> None)
                return OnPreparedOrFailed payload
            }
        |> mergeUnionCases

    static member get_Codec () = ofObjCodec <| SubjectTransactionLifeEvent.get_ObjCodec ()

    static member inline Init (typeLabel: string, _typeParams: 't) =
        initializeInterfaceImplementation<LifeEvent, SubjectTransactionLifeEvent> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| SubjectTransactionLifeEvent.get_ObjCodec ())

#endif

let inline addSubjectTransactionLifeCycleDef
    (ecosystemDef: EcosystemDef)
    (subjectTypeArgsLabel : SubjectTransaction<'Op> -> Option<string>)
    (lifeActionTypeArgsLabel : SubjectTransactionAction<'Op> -> Option<string>)
    (opErrorTypeArgsLabel : SubjectTransactionOpError<'Op> -> Option<string>)
    (constructorTypeArgsLabel : SubjectTransactionConstructor<'Op> -> Option<string>)
    : SubjectTransactionLifeCycleDef<'Op> * EcosystemDef =

    let lifeCycleDef: SubjectTransactionLifeCycleDef<'Op> =
        { Key = LifeCycleKey ("_SubjectTransaction", ecosystemDef.Name)
          ProjectionDefs = KeyedSet.empty }

    PRIVATE_addLifeCycleDefImpl
        (Some (
            (None: Option<'Op>),
            subjectTypeArgsLabel,
            (fun (_: SubjectTransactionId) -> None),
            lifeActionTypeArgsLabel,
            opErrorTypeArgsLabel,
            (fun (_: SubjectTransactionLifeEvent) -> None),
            constructorTypeArgsLabel))
        lifeCycleDef
        ecosystemDef