[<AutoOpen>]
module LibLifeCycle.LifeCycles.RequestRateCounter.Types

open System

[<Struct>]
type RequestRateCounterId = RequestRateCounterId of string
with
    interface SubjectId with
        member this.IdString =
            let (RequestRateCounterId id) = this
            id

type RequestRateCounter = {
    Id: RequestRateCounterId
    CreatedOn: DateTimeOffset
    Expiry: TimeSpan
    Limit: PositiveInteger
    Counter: uint32
} with
    interface Subject<RequestRateCounterId> with
        member this.SubjectId = this.Id
        member this.SubjectCreatedOn = this.CreatedOn

[<RequireQualifiedAccess>]
type RequestRateCounterConstructor =
| New of RequestRateCounterId * Expiry: TimeSpan * Limit: PositiveInteger
with
    interface Constructor

[<RequireQualifiedAccess>]
type RequestRateCounterAction =
| Increment of Delta: PositiveInteger
with
    interface LifeAction

type RequestRateCounterLifeEvent = private NoLifeEvent of unit
with
    interface LifeEvent

type RequestRateCounterOpError =
| LimitExceeded
with
    interface OpError

type RequestRateCounterNumericIndex = NoNumericIndex<RequestRateCounterOpError>

type RequestRateCounterStringIndex = NoStringIndex<RequestRateCounterOpError>

type RequestRateCounterSearchIndex = NoSearchIndex

type RequestRateCounterGeographyIndex = NoGeographyIndex

type RequestRateCounterIndex() = inherit SubjectIndex<RequestRateCounterIndex, RequestRateCounterNumericIndex, RequestRateCounterStringIndex, RequestRateCounterSearchIndex, RequestRateCounterGeographyIndex, RequestRateCounterOpError>()

// CODECS

#if !FABLE_COMPILER

open CodecLib


type RequestRateCounterId with
    static member TypeLabel () = "Shared_RequestRateCounterId"

    static member private get_ObjCodec_AllCases () =
        function
        | RequestRateCounterId _ ->
            codec {
                let! payload = reqWith Codecs.string "RCId" (function RequestRateCounterId x -> Some x)
                return RequestRateCounterId payload
            }
        |> mergeUnionCases

    static member private get_ObjCodec () = RequestRateCounterId.get_ObjCodec_AllCases ()
    static member get_Codec () = ofObjCodec <| RequestRateCounterId.get_ObjCodec ()
    static member Init (typeLabel: string, _typeParams: _) = initializeInterfaceImplementation<SubjectId, RequestRateCounterId> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| RequestRateCounterId.get_ObjCodec ())



type RequestRateCounter with
    static member TypeLabel () = "RequestRateCounter"

    static member private get_ObjCodec_V1 () =
        codec {
            let! _version = reqWith Codecs.int "__v1" (fun _ -> Some 0)
            and! id = reqWith codecFor<_, RequestRateCounterId> "Id" (fun x -> Some x.Id)
            and! createdOn = reqWith Codecs.dateTimeOffset "CreatedOn" (fun x -> Some x.CreatedOn)
            and! expiry = reqWith Codecs.timeSpan "Expiry" (fun x -> Some x.Expiry)
            and! limit = reqWith codecFor<_, PositiveInteger> "Limit" (fun x -> Some x.Limit)
            and! counter = reqWith Codecs.uint32 "Counter" (fun x -> Some x.Counter)
            return {
                Id = id
                CreatedOn = createdOn
                Expiry = expiry
                Limit = limit
                Counter = counter
             }
        }

    static member private get_ObjCodec () = RequestRateCounter.get_ObjCodec_V1 ()
    static member get_Codec () = ofObjCodec <| RequestRateCounter.get_ObjCodec ()
    static member Init (typeLabel: string, _typeParams: _) = initializeInterfaceImplementation<Subject<RequestRateCounterId>, RequestRateCounter> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| RequestRateCounter.get_ObjCodec ())



type RequestRateCounterConstructor with
    static member TypeLabel () = "RequestRateCounterConstructor"

    static member private get_ObjCodec_AllCases () =
        function
        | New _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function New _ -> Some 0)
                and! payload = reqWith (Codecs.tuple3 codecFor<_, RequestRateCounterId> Codecs.timeSpan codecFor<_, PositiveInteger>) "N" (function New (x1, x2, x3) -> Some (x1, x2, x3))
                return New payload
            }
        |> mergeUnionCases

    static member private get_ObjCodec () = RequestRateCounterConstructor.get_ObjCodec_AllCases ()
    static member get_Codec () = ofObjCodec <| RequestRateCounterConstructor.get_ObjCodec ()
    static member Init (typeLabel: string, _typeParams: _) = initializeInterfaceImplementation<Constructor, RequestRateCounterConstructor> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| RequestRateCounterConstructor.get_ObjCodec ())



type RequestRateCounterAction with
    static member TypeLabel () = "RequestRateCounterAction"

    static member private get_ObjCodec_AllCases () =
        function
        | Increment _ ->
            codec {
                let! payload = reqWith codecFor<_, PositiveInteger> "I" (function Increment x -> Some x)
                return Increment payload
            }
        |> mergeUnionCases

    static member private get_ObjCodec () = RequestRateCounterAction.get_ObjCodec_AllCases ()
    static member get_Codec () = ofObjCodec <| RequestRateCounterAction.get_ObjCodec ()
    static member Init (typeLabel: string, _typeParams: _) = initializeInterfaceImplementation<LifeAction, RequestRateCounterAction> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| RequestRateCounterAction.get_ObjCodec ())



type RequestRateCounterLifeEvent with
    static member TypeLabel () = "Shared_RequestRateCounterLifeEvent"

    static member private get_ObjCodec_AllCases () =
        function
        | NoLifeEvent _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function NoLifeEvent _ -> Some 0)
                and! payload = reqWith Codecs.unit "NoLifeEvent" (function NoLifeEvent x -> Some x)
                return NoLifeEvent payload
            }
        |> mergeUnionCases

    static member private get_ObjCodec () = RequestRateCounterLifeEvent.get_ObjCodec_AllCases ()
    static member get_Codec () = ofObjCodec <| RequestRateCounterLifeEvent.get_ObjCodec ()
    static member Init (typeLabel: string, _typeParams: _) = initializeInterfaceImplementation<LifeEvent, RequestRateCounterLifeEvent> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| RequestRateCounterLifeEvent.get_ObjCodec ())



type RequestRateCounterOpError with
    static member TypeLabel () = "RequestRateCounterOpError"

    static member private get_ObjCodec_AllCases () =
        function
        | LimitExceeded ->
            codec {
                let! _ = reqWith Codecs.unit "LE" (function LimitExceeded -> Some ())
                return LimitExceeded
            }
        |> mergeUnionCases

    static member private get_ObjCodec () = RequestRateCounterOpError.get_ObjCodec_AllCases ()
    static member get_Codec () = ofObjCodec <| RequestRateCounterOpError.get_ObjCodec ()
    static member Init (typeLabel: string, _typeParams: _) = initializeInterfaceImplementation<OpError, RequestRateCounterOpError> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| RequestRateCounterOpError.get_ObjCodec ())

#endif // !FABLE_COMPILER