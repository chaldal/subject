[<AutoOpen>]
module
#if FABLE_COMPILER
    // See comment in LibLifeCycleTypes/AssemblyInfo.fs
    LibLifeCycleTypes_EcosystemTypes
#else
    LibLifeCycleTypes.EcosystemTypes
#endif


open System
open LibLifeCycleTypes
open LibLifeCycle.LifeCycles.Meta

type FullyTypedLifeCycleDefFunction<'Res> =
    abstract member Invoke: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId> -> 'Res

and LifeCycleDef =
    abstract member LifeCycleKey: LifeCycleKey
    abstract Invoke: FullyTypedLifeCycleDefFunction<'Res> -> 'Res

and UntypedSubjectProjectionDef<'Subject> = {
    ProjectionType: Type
    Projection:     'Subject -> obj
    Name:           string
} with interface IKeyed<string> with member this.Key = this.Name

/// Represents a combination of public life-cycle-specific types of a subject which together define
/// subject interface from client perspective.
and LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId
    when 'Subject              :> Subject<'SubjectId>
    and  'LifeAction           :> LifeAction
    and  'OpError              :> OpError
    and  'Constructor          :> Constructor
    and  'LifeEvent            :> LifeEvent
    and  'LifeEvent            :  comparison
    and  'SubjectIndex         :> SubjectIndex<'OpError>
    and  'SubjectId            :> SubjectId
    and 'SubjectId : comparison> =
    // can't be internal because instantiated inside inline functions (for codecs)
    {
        Key: LifeCycleKey
        ProjectionDefs: KeyedSet<string, UntypedSubjectProjectionDef<'Subject>>
    }
    with
        interface LifeCycleDef with
            member this.LifeCycleKey = this.Key
            member this.Invoke fn = fn.Invoke this

type SubjectProjectionDef<'Projection, 'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId
    when 'Subject              :> Subject<'SubjectId>
    and  'LifeAction           :> LifeAction
    and  'OpError              :> OpError
    and  'Constructor          :> Constructor
    and  'LifeEvent            :> LifeEvent
    and  'LifeEvent            :  comparison
    and  'SubjectIndex         :> SubjectIndex<'OpError>
    and  'SubjectId            :> SubjectId
    and 'SubjectId : comparison> =
#if FABLE_COMPILER
    (* can't have internal because inline for fable *)
    {
#else
    internal {
#endif
        LifeCycleDef_:   LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>
        ProjectionName_: string
        Projection_:     'Subject -> 'Projection
    }
with
    member this.ProjectionName = this.ProjectionName_
    member this.Projection     = this.Projection_

type ITimeSeriesDef =
    abstract TimeSeriesKey: TimeSeriesKey

and TimeSeriesDef<'TimeSeriesDataPoint, 'TimeSeriesId, [<Measure>] 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex
    when 'TimeSeriesDataPoint :> TimeSeriesDataPoint<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure>
    and  'TimeSeriesId        :> TimeSeriesId<'TimeSeriesId>
    and  'OpError :> OpError
    and  'TimeSeriesIndex :> TimeSeriesIndex<'TimeSeriesIndex>> =
    #if FABLE_COMPILER
    (* can't have internal because inline for fable *)
    {
#else
    internal {
#endif
        Key_: TimeSeriesKey
    }
with
    member this.Key = this.Key_

    interface ITimeSeriesDef with
        member this.TimeSeriesKey = this.Key

type FullyTypedViewDefFunction<'Res> =
    abstract member Invoke: ViewDef<'Input, 'Output, 'OpError> -> 'Res

and IViewDef =
    abstract ViewName: string
    abstract Invoke: FullyTypedViewDefFunction<'Res> -> 'Res

// TODO: ideally 'Input 'Output and 'OpError should be constrained to ViewInput<> ViewOutput<> and ViewOpError<> but there's already lots of free-typed legacy views
/// Represents a combination of public view-specific types which together define view interface from client perspective.
and ViewDef<'Input, 'Output, 'OpError when 'OpError :> OpError> =
#if FABLE_COMPILER
    (* can't have internal because inline for fable *)
    {
#else
    internal {
#endif
        Name_: string
    }
with
    member this.Name = this.Name_

    interface IViewDef with
        member this.ViewName = this.Name
        member this.Invoke fn = fn.Invoke this

type ViewInput<'ViewInput when 'ViewInput :> ViewInput<'ViewInput>> =
#if !FABLE_COMPILER
    static abstract member Codec<'Encoding when 'Encoding :> CodecLib.IEncoding and 'Encoding : (new: unit -> 'Encoding)> : unit -> CodecLib.Codec<'Encoding, 'ViewInput>
#else
    interface end
#endif

type ViewOutput<'ViewOutput when 'ViewOutput :> ViewOutput<'ViewOutput>> =
#if !FABLE_COMPILER
    static abstract member Codec<'Encoding when 'Encoding :> CodecLib.IEncoding and 'Encoding : (new: unit -> 'Encoding)> : unit -> CodecLib.Codec<'Encoding, 'ViewOutput>
#else
    interface end
#endif

type ViewOpError<'ViewOpError when 'ViewOpError :> ViewOpError<'ViewOpError> and 'ViewOpError :> OpError> =
#if !FABLE_COMPILER
    static abstract member Codec<'Encoding when 'Encoding :> CodecLib.IEncoding and 'Encoding : (new: unit -> 'Encoding)> : unit -> CodecLib.Codec<'Encoding, 'ViewOpError>
#else
    interface end
#endif

#if !FABLE_COMPILER // disable interface codec registration in Fable

let inline initTypeWithWitness<'w, ^t
                                when ^t : (static member Init : string * 'w -> unit )> label (witness: ^w)  =
    // Register StjEncoding using type-safe static member constraint
    (^t : (static member Init : string * 'w -> unit ) (label, witness))

let inline codecTypeLabel< ^t when ^t : (static member TypeLabel : unit -> string ) > =
    (^t : (static member TypeLabel : unit -> string ) ())

let inline initLifeCycleTypesCodecs
    (maybeTypeParameterWitnessAndTypeArgsLabels:
        Option<
            Option<'w> *
            ('Subject -> Option<string>) *
            ('SubjectId -> Option<string>) *
            ('LifeAction -> Option<string>) *
            ('OpError -> Option<string>) *
            ('LifeEvent -> Option<string>) *
            ('Constructor -> Option<string>)>)
    (_lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>) =

    let typeParameter = Unchecked.defaultof<'w>
    match maybeTypeParameterWitnessAndTypeArgsLabels with
    | None ->
        initTypeWithWitness<'w, 'Subject>     codecTypeLabel<'Subject>     typeParameter
        initTypeWithWitness<'w, 'SubjectId>   codecTypeLabel<'SubjectId>   typeParameter
        initTypeWithWitness<'w, 'LifeAction>  codecTypeLabel<'LifeAction>  typeParameter
        initTypeWithWitness<'w, 'OpError>     codecTypeLabel<'OpError>     typeParameter
        initTypeWithWitness<'w, 'LifeEvent>   codecTypeLabel<'LifeEvent>   typeParameter
        initTypeWithWitness<'w, 'Constructor> codecTypeLabel<'Constructor> typeParameter

    | Some (_, subjectTypeArgsLabel, subjectIdTypeArgsLabel, lifeActionTypeArgsLabel, opErrorTypeArgsLabel, lifeEventTypeArgsLabel, constructorTypeArgsLabel) ->
        let wrapTypeArgs = function | None -> "" | Some args -> sprintf "<%s>" args
        initTypeWithWitness<'w, 'Subject>     (sprintf "%s%s" codecTypeLabel<'Subject>     (subjectTypeArgsLabel     Unchecked.defaultof<'Subject>     |> wrapTypeArgs)) typeParameter
        initTypeWithWitness<'w, 'SubjectId>   (sprintf "%s%s" codecTypeLabel<'SubjectId>   (subjectIdTypeArgsLabel   Unchecked.defaultof<'SubjectId>   |> wrapTypeArgs)) typeParameter
        initTypeWithWitness<'w, 'LifeAction>  (sprintf "%s%s" codecTypeLabel<'LifeAction>  (lifeActionTypeArgsLabel  Unchecked.defaultof<'LifeAction>  |> wrapTypeArgs)) typeParameter
        initTypeWithWitness<'w, 'OpError>     (sprintf "%s%s" codecTypeLabel<'OpError>     (opErrorTypeArgsLabel     Unchecked.defaultof<'OpError>     |> wrapTypeArgs)) typeParameter
        initTypeWithWitness<'w, 'LifeEvent>   (sprintf "%s%s" codecTypeLabel<'LifeEvent>   (lifeEventTypeArgsLabel   Unchecked.defaultof<'LifeEvent>   |> wrapTypeArgs)) typeParameter
        initTypeWithWitness<'w, 'Constructor> (sprintf "%s%s" codecTypeLabel<'Constructor> (constructorTypeArgsLabel Unchecked.defaultof<'Constructor> |> wrapTypeArgs)) typeParameter

#else

// used only for codecs that are disabled in Fable
let inline codecTypeLabel<'t> = ""

#endif

// For views without input parameters
type NoInput = NoInput
with
    interface ViewInput<NoInput> with
        // static member Codec() = QQQ

type NoOutput = NoOutput
with
    interface ViewOutput<NoOutput> with
        // static member Codec() = QQQ

type NoViewError = NoViewError
with
    interface OpError
    interface ViewOpError<NoViewError> with
        // static member Codec() = QQQ

[<Literal>]
let MetaLifeCycleName = "_Meta"

type MetaLifeCycleDef = LifeCycleDef<Meta, MetaAction, MetaOpError, MetaConstructor, MetaLifeEvent, MetaIndex, MetaId>

/// Collection of life cycle definitions that represent an ecosystem
/// Note: the "Internal" prefixes here are necessary because Fable doesn't
/// like to have record fields and interface methods with the same name.
type EcosystemDef = {
    Name:             string
    MetaLifeCycleDef: MetaLifeCycleDef
    LifeCycleDefs:    List<LifeCycleDef>
    ViewDefs:         List<IViewDef>
    TimeSeriesDefs:   List<ITimeSeriesDef>
}

let
#if FABLE_COMPILER
    inline
#endif
    newEcosystemDef (ecosystemName: string) =
    // slash is used in some places to format and parse qualified life cycle name e.g. EcosystemName/LCName
    if ecosystemName.Contains "/" then
        invalidArg "ecosystemName" "Ecosystem name cannot contain `/`, why not just choose alphabetical name?"

    // TODO: consider hiding meta away from out-of-host clients, expose necessary bits via special MetaGrain
    let metaLifeCycleDef : MetaLifeCycleDef = { Key = LifeCycleKey (MetaLifeCycleName, ecosystemName); ProjectionDefs = KeyedSet.empty }
#if !FABLE_COMPILER
    initLifeCycleTypesCodecs None metaLifeCycleDef
#endif
    {
        Name             = ecosystemName
        LifeCycleDefs    = [metaLifeCycleDef]
        ViewDefs         = []
        TimeSeriesDefs   = []
        MetaLifeCycleDef = metaLifeCycleDef
    }

let inline PRIVATE_addLifeCycleDefImpl // want private but must be public for inlining
    (maybeTypeParameterWitnessAndTypeArgsLabels:
        Option<
            Option<'w> *
            ('Subject -> Option<string>) *
            ('SubjectId -> Option<string>) *
            ('LifeAction -> Option<string>) *
            ('OpError -> Option<string>) *
            ('LifeEvent -> Option<string>) *
            ('Constructor -> Option<string>)>)
    (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>)
    (ecosystemDef: EcosystemDef)
    : LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId> * EcosystemDef =
    // somewhat counterintuitive, but two lifecycles can't have same types even if they have different names
    // that would violate integrity of certain services and grains e.g. Repo
    let conflictsWithExistingLifeCycle =
        ecosystemDef.LifeCycleDefs
        #if FABLE_COMPILER
        // Even though we have a mismatch in what "conflict" means for runtime-generics-erased Fable
        // and the dotnet runtime, practically lifecycles will be "vetted" by the backend process
        // anyway, so this mismatch is unlikely to cause real world problems
        |> Seq.exists (fun def -> def.LifeCycleKey = lifeCycleDef.Key)
        #else
        |> Seq.exists (fun def -> def.GetType() = lifeCycleDef.GetType() || def.LifeCycleKey = lifeCycleDef.Key)
        #endif

    if conflictsWithExistingLifeCycle then
        failwithf "Ecosystem can't define two life cycles with identical types or keys (while adding lifeCycleDef with Key %A)" lifeCycleDef.Key

#if !FABLE_COMPILER
    initLifeCycleTypesCodecs maybeTypeParameterWitnessAndTypeArgsLabels lifeCycleDef
#endif
    ignore maybeTypeParameterWitnessAndTypeArgsLabels // Hide unused variable warning
    let ecosystemDef = { ecosystemDef with LifeCycleDefs = lifeCycleDef :: ecosystemDef.LifeCycleDefs }
    lifeCycleDef, ecosystemDef

let inline addLifeCycleDef
    (ecosystemDef: EcosystemDef)
    (name: string)
    : LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId> * EcosystemDef =
    if name.StartsWith "_" || name.Length > 40 || name.Contains "/" then
        #if FABLE_COMPILER
        invalidArg "name" "LifeCycle names cannot start with `_` contain `/` or be longer than 40 chars"
        #else
        invalidArg (nameof name) "LifeCycle names cannot start with `_` contain `/` or be longer than 40 chars"
        #endif
    let lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId> =
        { Key = LifeCycleKey (name, ecosystemDef.Name); ProjectionDefs = KeyedSet.empty }
    PRIVATE_addLifeCycleDefImpl None lifeCycleDef ecosystemDef

let inline addGenericLifeCycleDef
    (ecosystemDef: EcosystemDef)
    (name: string)
    (typeParameter: Option<'w>)
    (subjectTypeArgsLabel : 'Subject -> Option<string>)
    (subjectIdTypeArgsLabel : 'SubjectId -> Option<string>)
    (lifeActionTypeArgsLabel : 'LifeAction -> Option<string>)
    (opErrorTypeArgsLabel : 'OpError -> Option<string>)
    (lifeEventTypeArgsLabel : 'LifeEvent -> Option<string>)
    (constructorTypeArgsLabel : 'Constructor -> Option<string>)
    : LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId> * EcosystemDef =
    if name.StartsWith "_" || name.Length > 40 || name.Contains "/" then
        #if FABLE_COMPILER
        invalidArg "name" "LifeCycle names cannot start with `_` contain `/` or be longer than 40 chars"
        #else
        invalidArg (nameof name) "LifeCycle names cannot start with `_` contain `/` or be longer than 40 chars"
        #endif
    let lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId> =
        { Key = LifeCycleKey (name, ecosystemDef.Name); ProjectionDefs = KeyedSet.empty }
    PRIVATE_addLifeCycleDefImpl
        (Some (typeParameter, subjectTypeArgsLabel, subjectIdTypeArgsLabel, lifeActionTypeArgsLabel, opErrorTypeArgsLabel, lifeEventTypeArgsLabel, constructorTypeArgsLabel))
        lifeCycleDef ecosystemDef

type LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId
                    when 'Subject              :> Subject<'SubjectId>
                    and  'LifeAction           :> LifeAction
                    and  'OpError              :> OpError
                    and  'Constructor          :> Constructor
                    and  'LifeEvent            :> LifeEvent
                    and  'LifeEvent            :  comparison
                    and  'SubjectIndex         :> SubjectIndex<'OpError>
                    and  'SubjectId            :> SubjectId>
with
    #if FABLE_COMPILER
    member inline this.CreateProjection (projection: 'Subject -> 'Projection) (name: string) (ecosystemDef: EcosystemDef) :
    #else
    member this.CreateProjection (projection: 'Subject -> 'Projection) (name: string) (ecosystemDef: EcosystemDef) :
    #endif
        (SubjectProjectionDef<'Projection, 'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId> *
         LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId> *
         EcosystemDef)=
        if name.StartsWith "_" || name.Length > 40 || name.Contains "/" then
            #if FABLE_COMPILER
            invalidArg "name" "LifeCycle names cannot start with `_` contain `/` or be longer than 40 chars"
            #else
            invalidArg (nameof name) "LifeCycle names cannot start with `_` contain `/` or be longer than 40 chars"
            #endif
        let projectionDef = { ProjectionName_ = name; LifeCycleDef_ = this; Projection_ = projection }
        let untypedProjectionDef : UntypedSubjectProjectionDef<'Subject> =
            { Projection = projection >> box; ProjectionType = typeof<'Projection>; Name = name }
        let updatedLifeCycleDef = { this with ProjectionDefs = this.ProjectionDefs.AddOrUpdate untypedProjectionDef }

        let updatedEcosystemDef =
            { ecosystemDef with
                LifeCycleDefs =
                    ecosystemDef.LifeCycleDefs
                    |> List.map (fun def -> if def.LifeCycleKey <> this.Key then def else updatedLifeCycleDef) }

        (projectionDef, updatedLifeCycleDef, updatedEcosystemDef)


let
#if FABLE_COMPILER
    inline
#endif
    addViewDef<'Input, 'Output, 'OpError when 'OpError :> OpError>
        (ecosystemDef: EcosystemDef)
        (name: string)
        : ViewDef<'Input, 'Output, 'OpError> * EcosystemDef =

    if name.StartsWith "_" then
        #if FABLE_COMPILER
        invalidArg "name" "View names cannot start with `_`"
        #else
        invalidArg (nameof name) "View names cannot start with `_`"
        #endif

    let viewDef: ViewDef<'Input, 'Output, 'OpError> = { Name_ = name }

    // somewhat counterintuitive, but two views can't have same types even if they have different names
    let conflictsWithExistingView =
        ecosystemDef.ViewDefs
        #if FABLE_COMPILER
        // Even though we have a mismatch in what "conflict" means for runtime-generics-erased Fable
        // and the dotnet runtime, practically lifecycles will be "vetted" by the backend process
        // anyway, so this mismatch is unlikely to cause real world problems
        |> Seq.exists (fun def -> def.ViewName = viewDef.Name)
        #else
        |> Seq.exists (fun def -> def.GetType() = viewDef.GetType() || def.ViewName = viewDef.Name)
        #endif

    if conflictsWithExistingView then
        failwith "Ecosystem can't define two views with identical types or names"

    viewDef, { ecosystemDef with ViewDefs = viewDef :: ecosystemDef.ViewDefs }

let
#if FABLE_COMPILER
    inline
#endif
    addTimeSeriesDef<'TimeSeriesDataPoint, 'TimeSeriesId, [<Measure>] 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex
        when 'TimeSeriesDataPoint :> TimeSeriesDataPoint<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure>
        and  'TimeSeriesId :> TimeSeriesId<'TimeSeriesId>
        and 'OpError :> OpError
        and 'TimeSeriesIndex :> TimeSeriesIndex<'TimeSeriesIndex>>
        (ecosystemDef: EcosystemDef)
        (name: string)
        : TimeSeriesDef<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex> * EcosystemDef =

    if name.StartsWith "_" then
        #if FABLE_COMPILER
        invalidArg "name" "Time Series names cannot start with `_`"
        #else
        invalidArg (nameof name) "Time Series names cannot start with `_`"
        #endif

    let timeSeriesDef: TimeSeriesDef<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex> = { Key_ = TimeSeriesKey (name, ecosystemDef.Name) }

    // somewhat counterintuitive, but two time series can't have same types even if they have different names
    let conflictsWithExistingTimeSeries =
        ecosystemDef.TimeSeriesDefs
        #if FABLE_COMPILER
        // Even though we have a mismatch in what "conflict" means for runtime-generics-erased Fable
        // and the dotnet runtime, practically lifecycles will be "vetted" by the backend process
        // anyway, so this mismatch is unlikely to cause real world problems
        |> Seq.exists (fun def -> def.TimeSeriesKey = timeSeriesDef.Key)
        #else
        |> Seq.exists (fun def -> def.GetType() = timeSeriesDef.GetType() || def.TimeSeriesKey = timeSeriesDef.Key)
        #endif

    if conflictsWithExistingTimeSeries then
        failwith "Ecosystem can't define two time series with identical types or keys"

    timeSeriesDef, { ecosystemDef with TimeSeriesDefs = timeSeriesDef :: ecosystemDef.TimeSeriesDefs }


// TODO: better home for types below / somewhere in Host. These are here so LibLifeCycleCore can see it, but they really belong to Host - and so is dependent Core code


type SubscriptionName = string

type [<RequireQualifiedAccess>] SubscriptionTriggerType =
| Named of SubscriptionName

type ExternalCallOrigin = {
    RemoteAddress: string
    Headers: Map<string, Set<string>>
}

[<RequireQualifiedAccess>]
type CallOrigin =
| Internal
| External of ExternalCallOrigin
with
    member this.MaybeGetTelemetryIpAddressStr : Option<NonemptyString> =
        match this with
        | CallOrigin.Internal ->
            None

        | CallOrigin.External externalCallOrigin ->
            externalCallOrigin.RemoteAddress
            |> NonemptyString.ofString


type [<RequireQualifiedAccess>] SideEffectFailureSeverity =
| Warning
| Error

[<RequireQualifiedAccess>]
type SideEffectFailure =
| ConstructAlreadyInitialized of SubjectReference * Constructor
| ConstructError              of SubjectReference * Constructor * OpError
| ActNotInitialized           of SubjectReference * LifeAction
| ActError                    of SubjectReference * LifeAction * OpError
| ActNotAllowed               of SubjectReference * LifeAction
| SubscribeNotInitialized     of Publisher: SubjectReference * Map<SubscriptionName, LifeEvent>
| IngestTimeSeriesError       of TimeSeriesKey * ``list<'TimeSeriesDataPoint>``: obj * OpError

[<RequireQualifiedAccess>]
type SideEffectSuccess =
| ConstructOk of SubjectReference * Constructor
// TODO: add MaybeConstruct success/error cases too if ever needed (for both MaybeConstruct and first part of ActMaybeConstruct side effects). Do not tell if constructed for the first time.
| ActOk       of SubjectReference * LifeAction
| SubscribeOk of SubjectReference * Map<SubscriptionName, LifeEvent>

[<RequireQualifiedAccess>]
type SideEffectResponse =
| Success of SideEffectSuccess
| Failure of SideEffectFailure


#if !FABLE_COMPILER

#nowarn "69" // disable Interface implementations should normally be given on the initial declaration of a type

open CodecLib

type SubscriptionTriggerType with
    static member get_Codec () =
        function
        | Named _ ->
            codec {
                let! payload = reqWith Codecs.string "Named" (function (Named x) -> Some x)
                return Named payload
            }
        |> mergeUnionCases
        |> ofObjCodec

type ExternalCallOrigin with
    static member private get_ObjCodec_V1 () =
        codec {
            let! _version = reqWith Codecs.int "__v1" (fun _ -> Some 0)
            and! remoteAddress = reqWith Codecs.string "RemoteAddress" (fun x -> Some x.RemoteAddress)
            and! headers = reqWith (Codecs.gmap Codecs.string (Codecs.set Codecs.string)) "Headers" (fun x -> Some x.Headers)
            return {
                RemoteAddress = remoteAddress
                Headers = headers
             }
        }
    static member get_Codec () = ofObjCodec (ExternalCallOrigin.get_ObjCodec_V1 ())

type CallOrigin with
    static member private get_ObjCodec_AllCases () =
        function
        | Internal ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Internal -> Some 0 | _ -> None)
                and! _ = reqWith Codecs.unit "Internal" (function Internal -> Some () | _ -> None)
                return Internal
            }
        | External _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function External _ -> Some 0 | _ -> None)
                and! payload = reqWith codecFor<_, ExternalCallOrigin> "External" (function External x -> Some x | _ -> None)
                return External payload
            }
        |> mergeUnionCases
    static member get_Codec () = ofObjCodec (CallOrigin.get_ObjCodec_AllCases ())

type NoInput with
    static member get_Codec () =
        function
        | NoInput ->
            codec {
                let! _ = reqWith Codecs.unit "NoInput" (function NoInput -> Some ())
                return NoInput
            }
        |> mergeUnionCases
        |> ofObjCodec
    interface ViewInput<NoInput> with
         static member Codec () = NoInput.get_Codec ()

type NoOutput with
    static member get_Codec () =
        function
        | NoOutput ->
            codec {
                let! _ = reqWith Codecs.unit "NoOutput" (function NoOutput -> Some ())
                return NoOutput
            }
        |> mergeUnionCases
        |> ofObjCodec
    interface ViewOutput<NoOutput> with
         static member Codec () = NoOutput.get_Codec ()

type NoViewError with
    static member get_Codec () =
        function
        | NoViewError ->
            codec {
                let! _ = reqWith Codecs.unit "NoViewError" (function NoViewError -> Some ())
                return NoViewError
            }
        |> mergeUnionCases
        |> ofObjCodec
    interface ViewOpError<NoViewError> with
         static member Codec () = NoViewError.get_Codec ()

#endif