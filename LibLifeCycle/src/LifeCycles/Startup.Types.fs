[<AutoOpen>]
module LibLifeCycle.LifeCycles.Startup.Types

open System

type StartupId = StartupId of NonemptyString
with
    interface SubjectId with
        member this.IdString =
            let (StartupId id) = this
            id.Value

type Startup = {
    Id: StartupId
    CreatedOn: DateTimeOffset
    LastStartupOn: DateTimeOffset
} with
    interface Subject<StartupId> with
        member this.SubjectId = this.Id
        member this.SubjectCreatedOn = this.CreatedOn

[<RequireQualifiedAccess>]
type StartupConstructor =
| New
| NewForSilo of SiloId: NonemptyString
with
    interface Constructor

[<RequireQualifiedAccess>]
type StartupAction =
| PerformStartup
with
    interface LifeAction

type StartupLifeEvent = private NoLifeEvent of unit
with
    interface LifeEvent

type StartupOpError = private NoOpError of unit
with
    interface OpError

type StartupNumericIndex = NoNumericIndex<StartupOpError>

type StartupStringIndex = NoStringIndex<StartupOpError>

type StartupSearchIndex = NoSearchIndex

type StartupGeographyIndex = NoGeographyIndex

type StartupIndex() = inherit SubjectIndex<StartupIndex, StartupNumericIndex, StartupStringIndex, StartupSearchIndex, StartupGeographyIndex, StartupOpError>()

// CODECS

#if !FABLE_COMPILER
open CodecLib

type StartupId with
    static member TypeLabel () = "StartupId"

    static member get_ObjCodec () =
        function
        | StartupId _ ->
            codec {
                let! payload = reqWith codecFor<_, NonemptyString> "StartupId" (fun (StartupId x) -> Some x)
                return StartupId payload
            }
        |> mergeUnionCases

    static member get_Codec () = ofObjCodec <| StartupId.get_ObjCodec ()
    static member Init (typeLabel: string , _typeParams: 't) = initializeInterfaceImplementation<SubjectId, StartupId> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| StartupId.get_ObjCodec ())

type Startup with
    static member TypeLabel () = "Startup"
    static member get_ObjCodec () = codec {
        let! id = reqWith (StartupId.get_Codec ()) "Id" (fun x -> Some x.Id)
        and! createdOn = reqWith Codecs.dateTimeOffset "CreatedOn" (fun x -> Some x.CreatedOn)
        and! maybeLastStartupOn = optWith Codecs.dateTimeOffset "LastStartupOn" (fun x -> Some x.LastStartupOn)
        let lastStartupOn = maybeLastStartupOn |> Option.defaultValue createdOn
        return { Id = id; CreatedOn = createdOn; LastStartupOn = lastStartupOn }
    }

    static member get_Codec () = ofObjCodec <| Startup.get_ObjCodec ()
    static member Init (typeLabel: string , _typeParams: 't) = initializeInterfaceImplementation<Subject<StartupId>, Startup> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| Startup.get_ObjCodec ())

type StartupConstructor with
    static member TypeLabel () = "StartupConstructor"

    static member get_ObjCodec () =
        function
        | New ->
            codec {
                let! _ = reqWith Codecs.unit "New" (function New -> Some () | _ -> None)
                return New
            }
        | NewForSilo _ ->
            codec {
                let! payload = reqWith codecFor<_, NonemptyString> "NewForSilo" (function NewForSilo x -> Some x | _ -> None)
                return NewForSilo payload
            }
            |> withDecoders [
                decoder {
                    let! (id, _) = reqDecodeWithCodec (Codecs.tuple2 codecFor<_, NonemptyString> Codecs.timeSpan) "NewForSilo"
                    return NewForSilo id
                }
            ]
        |> mergeUnionCases

    static member get_Codec () = ofObjCodec <| StartupConstructor.get_ObjCodec ()
    static member Init  (typeLabel: string , _typeParams: 't) = initializeInterfaceImplementation<Constructor, StartupConstructor> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| StartupConstructor.get_ObjCodec ())

type StartupOpError
with
    static member get_ObjCodec () : Codec<MultiObj<'RawEncoding>, StartupOpError> =
        function
        | NoOpError _ ->
            codec {
                let! payload = reqWith Codecs.unit "NoOpError" (function NoOpError () -> Some ())
                return NoOpError payload
            }
        |> mergeUnionCases

    static member get_Codec () = ofObjCodec (StartupOpError.get_ObjCodec ())
    static member Init (typeLabel: string, _typeParams: _) =
        initializeInterfaceImplementation<OpError, StartupOpError> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| StartupOpError.get_ObjCodec ())
    static member TypeLabel () = "StartupOpError" // it won't be ever encoded via interface but need to be configured anyway

type StartupLifeEvent
with
    static member get_ObjCodec () : Codec<MultiObj<'RawEncoding>, StartupLifeEvent> =
        function
        | NoLifeEvent _ ->
            codec {
                let! payload = reqWith Codecs.unit "NoLifeEvent" (function NoLifeEvent () -> Some ())
                return NoLifeEvent payload
            }
        |> mergeUnionCases

    static member get_Codec () = ofObjCodec (StartupLifeEvent.get_ObjCodec ())
    static member Init (typeLabel: string, _typeParams: _) =
        initializeInterfaceImplementation<LifeEvent, StartupLifeEvent> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| StartupLifeEvent.get_ObjCodec ())
    static member TypeLabel () = "StartupLifeEvent"

type StartupAction with
    static member TypeLabel () = "StartupAction"

    static member get_ObjCodec () =
        function
        | PerformStartup ->
            codec {
                let! _ = reqWith Codecs.unit "PerformStartup" (function PerformStartup -> Some ())
                return PerformStartup
            }
        |> mergeUnionCases

    static member get_Codec () = ofObjCodec <| StartupAction.get_ObjCodec ()
    static member Init  (typeLabel: string , _typeParams: 't) = initializeInterfaceImplementation<LifeAction, StartupAction> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| StartupAction.get_ObjCodec ())

#endif
