module internal LibLifeCycleCore.OrleansEx.Serializer

// couldn't make LibLifeCycleCore.Orleans namespace because it makes "open Orleans" ambiguous in a few places

open System
open System.Text
open System.Threading.Tasks
open Orleans.Serialization
open LibLifeCycleCore
open FSharpPlus
open CodecLib.StjCodecs

#nowarn "0686"  // Allow exceptionally here to have explicit parameters for codec related functions

let inline private summarizerForType (toCodecFriendly: 'T -> 'CodecFriendly) : (Type * (obj -> string)) =
    typeof<'T>, (fun (t: obj) -> t :?> 'T |> toCodecFriendly |> ValueSummaryCodecs.toSummaryValue<'CodecFriendly> |> ValueSummaryCodecs.getSummaryString 6)


type private UntypedSerializer = {
    To:        obj -> byte[]
    Fro:       byte[] -> obj
    Type:      Type
    TypeId:    byte

    // TODO: can we move it away? it's incidental that we need logging summarizers and custom Orleans serializers for the same types
    //  (in fact we may need more type for summarizers e.g. error types (not just errors inside Result<_, _>)
    ToSummary: obj -> string
}
with
    static member inline ForIsomorphicType
        (typeId: byte)
        (toCodecFriendly: 'T -> 'CodecFriendly)
        (fromCodecFriendly: 'CodecFriendly -> 'T)
        // since F# doesn't support higher ranks we need to supply two copies of the same 'CodecFriendly type in order to be generalized with different generics
        (codecFriendlyId: 'CodecFriendly -> 'CodecFriendly2) : UntypedSerializer =
        // 'CodecFriendly can be encoded or decoded in a type-safe way
        // 'T is the type isomorphic to 'CodecFriendly (in unsafe way) that grain actually wants to expose
        let toJsonText (x: 'CodecFriendly) =
            x
#if DEBUG // TODO:  instead of if-def, pass config from upstream to check json in Dev Host and Tests but it is very tedious
            |> toJsonTextChecked<'CodecFriendly>
#else
            |> toJson<'CodecFriendly> |> (fun v -> v.ToString(unindentedOptions))
#endif
        {
            To        = fun (t: obj) -> t :?> 'T |> toCodecFriendly |> toJsonText |> gzipCompressUtf8String
            Fro       = fun (bytes: byte[]) ->
                let jsonText = gzipDecompressToUtf8String bytes
                match ofJsonText<'CodecFriendly> jsonText with
                | Ok x ->
                    fromCodecFriendly x |> box
                | Error err ->
                    InvalidOperationException (sprintf "Orleans deserializer is unable to decode. Type Id: %d. Error: %A. Json value: %s" typeId err jsonText)
                    |> raise
            ToSummary = summarizerForType (toCodecFriendly >> codecFriendlyId) |> snd
            Type      = typeof<'T>
            TypeId    = typeId
        }

    static member inline ForConcreteTypeWithExplicitCodec
        (typeId: byte)
        (codec: Fleece.Codec<StjEncoding, 'T>) : UntypedSerializer =
        let toJsonText (x: 'T) =
            x
#if DEBUG // TODO:  instead of if-def, pass config from upstream to check json in Dev Host and Tests but it is very tedious
            |> toJsonTextCheckedWithCodec<'T> codec
#else
            |> CodecLib.Codec.encode codec |> (fun v -> v.ToString(unindentedOptions))
#endif
        {
            To        = fun (t: obj) -> t :?> 'T |> toJsonText |> gzipCompressUtf8String
            Fro       = fun (bytes: byte[]) ->
                let jsonText = gzipDecompressToUtf8String bytes
                let enc = StjEncoding.Parse jsonText
                match CodecLib.Codec.decode codec enc with
                | Ok x ->
                    x |> box
                | Error err ->
                    InvalidOperationException (sprintf "Orleans deserializer is unable to decode. Type Id: %d. Error: %A. Json value: %s" typeId err jsonText)
                    |> raise
            ToSummary = fun _ ->
                NotImplementedException (sprintf "Orleans deserializer ForConcreteTypeWithExplicitCodec does not implement ToSummary. Type Id: %d" typeId)
                |> raise
            Type      = typeof<'T>
            TypeId    = typeId
        }

let private getUntypedSubjectSerializers<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId
                                     when 'Subject              :> Subject<'SubjectId>
                                     and  'LifeAction           :> LifeAction
                                     and  'OpError              :> OpError
                                     and  'Constructor          :> Constructor
                                     and  'LifeEvent            :> LifeEvent
                                     and  'LifeEvent            : comparison
                                     and  'SubjectIndex         :> SubjectIndex<'OpError>
                                     and  'SubjectId            :> SubjectId
                                     and  'SubjectId            : comparison> () =

    // List out all ISubjectGrain, ISubjectIdGenerationGrain, ISubjectRepoGrain, IBlobRepoGrain, ISubjectReflectionGrain parameters and response types
    // NOTES:
    // * no need to cover IConnectorGrain because it's always executed locally
    // * the type ID mapping can *never* change. Do not re-use. If you ever remove a serializer
    //   simply comment out that line, so others don't accidentally re-use

    [
        UntypedSerializer.ForIsomorphicType   1uy
            (fun x -> x :> Subject<'SubjectId>)
            (fun x -> x :?> 'Subject)
            id<_>

        UntypedSerializer.ForIsomorphicType   2uy
            (fun x -> x :> SubjectId)
            (fun x -> x :?> 'SubjectId)
            id<_>

        UntypedSerializer.ForIsomorphicType   3uy
            (fun x -> x :> LifeAction)
            (fun x -> x :?> 'LifeAction)
            id<_>

//        UntypedSerializer.ForIsomorphicType   4uy

        UntypedSerializer.ForIsomorphicType   5uy
            (fun x -> x :> Constructor)
            (fun x -> x :?> 'Constructor)
            id<_>

        UntypedSerializer.ForIsomorphicType   6uy
            (fun x -> x :> LifeEvent)
            (fun x -> x :?> 'LifeEvent)
            id<_>

//        UntypedSerializer.ForIsomorphicType   7uy

        UntypedSerializer.ForIsomorphicType   8uy
            (Result.mapBoth (fun x -> x :> Subject<'SubjectId>) GrainConstructionError<OpError>.CastUnsafe)
            (Result.mapBoth (fun x -> x :?> 'Subject)           GrainConstructionError<'OpError>.CastUnsafe)
            id<_>

//        UntypedSerializer.ForIsomorphicType  9uy

//        UntypedSerializer.ForIsomorphicType  10uy

//        UntypedSerializer.ForIsomorphicType  11uy

//        UntypedSerializer.ForIsomorphicType  12uy

        UntypedSerializer.ForIsomorphicType  13uy
            id<Map<SubscriptionName, LifeEvent>>
            id<Map<SubscriptionName, LifeEvent>>
            id<_>

//        UntypedSerializer.ForIsomorphicType  14uy

        UntypedSerializer.ForIsomorphicType  15uy
            id<Result<unit, SubjectFailure<GrainSubscriptionError>>>
            id<Result<unit, SubjectFailure<GrainSubscriptionError>>>
            id<_>

        UntypedSerializer.ForIsomorphicType  16uy
            id<SubscriptionTriggerType>
            id<SubscriptionTriggerType>
            id<_>

        UntypedSerializer.ForIsomorphicType  17uy
            (Result.mapBoth id<unit> (SubjectFailure.CastUnsafe GrainTriggerSubscriptionError<OpError>.CastUnsafe))
            (Result.mapBoth id<unit> (SubjectFailure.CastUnsafe GrainTriggerSubscriptionError<'OpError>.CastUnsafe))
            id<_>

        UntypedSerializer.ForIsomorphicType  18uy
            id<Option<ConstructSubscriptions>>
            id<Option<ConstructSubscriptions>>
            id<_>

        UntypedSerializer.ForIsomorphicType  19uy
            (Result.mapBoth (fun x -> x :> SubjectId)           GrainIdGenerationError<OpError>.CastUnsafe)
            (Result.mapBoth (fun x -> x :?> 'SubjectId)         GrainIdGenerationError<'OpError>.CastUnsafe)
            id<_>

//        UntypedSerializer.ForIsomorphicType  20uy

        UntypedSerializer.ForIsomorphicType  21uy
            (Result.mapBoth id<unit> (SubjectFailure.CastUnsafe GrainPrepareConstructionError<OpError>.CastUnsafe))
            (Result.mapBoth id<unit> (SubjectFailure.CastUnsafe GrainPrepareConstructionError<'OpError>.CastUnsafe))
            id<_>

        UntypedSerializer.ForIsomorphicType  22uy
            (Result.mapBoth id<unit> (SubjectFailure.CastUnsafe GrainPrepareTransitionError<OpError>.CastUnsafe))
            (Result.mapBoth id<unit> (SubjectFailure.CastUnsafe GrainPrepareTransitionError<'OpError>.CastUnsafe))
            id<_>

//        UntypedSerializer.ForIsomorphicType  23uy

//        UntypedSerializer.ForIsomorphicType  24uy

        UntypedSerializer.ForIsomorphicType  25uy
            (Set.map (fun v -> v :> SubjectId))
            (Set.map (fun v -> v :?> 'SubjectId))
            id<_>

//        UntypedSerializer.ForIsomorphicType  26uy

        UntypedSerializer.ForIsomorphicType  27uy
            id<IndexQuery<'SubjectIndex>>
            id<IndexQuery<'SubjectIndex>>
            id<_>

        UntypedSerializer.ForIsomorphicType  28uy
            id<Set<string>>
            id<Set<string>>
            id<_>

        UntypedSerializer.ForIsomorphicType  29uy
            id<ResultSetOptions<'SubjectIndex>>
            id<ResultSetOptions<'SubjectIndex>>
            id<_>

        UntypedSerializer.ForIsomorphicType  30uy
            (List.map (fun v -> v :> Subject<'SubjectId>))
            (List.map (fun v -> v :?> 'Subject))
            id<_>

        UntypedSerializer.ForIsomorphicType  31uy
            (List.map TemporalSnapshot<Subject<'SubjectId>, LifeAction, Constructor, 'SubjectId>.CastUnsafe)
            (List.map TemporalSnapshot<'Subject, 'LifeAction, 'Constructor, 'SubjectId>.CastUnsafe)
            id<_>

        UntypedSerializer.ForIsomorphicType  32uy
            (Option.map TemporalSnapshot<Subject<'SubjectId>, LifeAction, Constructor, 'SubjectId>.CastUnsafe)
            (Option.map TemporalSnapshot<'Subject, 'LifeAction, 'Constructor, 'SubjectId>.CastUnsafe)
            id<_>

        UntypedSerializer.ForIsomorphicType  33uy
            id<PreparedIndexPredicate<'SubjectIndex>>
            id<PreparedIndexPredicate<'SubjectIndex>>
            id<_>

//        UntypedSerializer.ForIsomorphicType  34uy

//        UntypedSerializer.ForIsomorphicType  35uy

        UntypedSerializer.ForIsomorphicType  36uy
            (Result.mapBoth (Option.map (fun x -> x :> Subject<'SubjectId>)) id<GrainGetError>)
            (Result.mapBoth (Option.map (fun x -> x :?> 'Subject))           id<GrainGetError>)
            id<_>

        UntypedSerializer.ForIsomorphicType  37uy
            (Result.mapBoth (Option.map (fun x -> x :> Subject<'SubjectId>)) (SubjectFailure.CastUnsafe GrainConstructionError<OpError>.CastUnsafe))
            (Result.mapBoth (Option.map (fun x -> x :?> 'Subject))           (SubjectFailure.CastUnsafe GrainConstructionError<'OpError>.CastUnsafe))
            id<_>

        UntypedSerializer.ForIsomorphicType  38uy
            (Result.mapBoth (Option.map (fun x -> x :> Subject<'SubjectId>)) (SubjectFailure.CastUnsafe GrainTransitionError<OpError>.CastUnsafe))
            (Result.mapBoth (Option.map (fun x -> x :?> 'Subject))           (SubjectFailure.CastUnsafe GrainTransitionError<'OpError>.CastUnsafe))
            id<_>

        UntypedSerializer.ForIsomorphicType  39uy
            (Result.mapBoth (Option.map (fun x -> x :> Subject<'SubjectId>)) (SubjectFailure.CastUnsafe GrainOperationError<OpError>.CastUnsafe))
            (Result.mapBoth (Option.map (fun x -> x :?> 'Subject))           (SubjectFailure.CastUnsafe GrainOperationError<'OpError>.CastUnsafe))
            id<_>

        // UntypedSerializer.ForIsomorphicType  40uy

        UntypedSerializer.ForIsomorphicType  41uy
            id<ResultPage>
            id<ResultPage>
            id<_>

        UntypedSerializer.ForIsomorphicType  42uy
            id<LifeEvent>
            id<LifeEvent>
            id<_>

        UntypedSerializer.ForIsomorphicType  43uy
            id<SubjectTransactionId>
            id<SubjectTransactionId>
            id<_>

        UntypedSerializer.ForIsomorphicType  44uy
            id<SideEffectDedupInfo>
            id<SideEffectDedupInfo>
            id<_>

        UntypedSerializer.ForIsomorphicType  45uy
            id<Option<SideEffectDedupInfo>>
            id<Option<SideEffectDedupInfo>>
            id<_>

        UntypedSerializer.ForIsomorphicType  46uy
            id<Result<unit, GrainRefreshTimersAndSubsError>>
            id<Result<unit, GrainRefreshTimersAndSubsError>>
            id<_>

        UntypedSerializer.ForIsomorphicType  47uy
            id<SubjectPKeyReference>
            id<SubjectPKeyReference>
            id<_>

        //UntypedSerializer.ForIsomorphicType  48uy

        //UntypedSerializer.ForIsomorphicType  49uy

        //UntypedSerializer.ForIsomorphicType  50uy

        UntypedSerializer.ForIsomorphicType  51uy
            id<NonemptySet<Guid>> // GrainSideEffectId
            id<NonemptySet<Guid>>
            id<_>

        //UntypedSerializer.ForIsomorphicType  52uy

        UntypedSerializer.ForIsomorphicType  53uy
            id<Option<DateTimeOffset>>
            id<Option<DateTimeOffset>>
            id<_>

        UntypedSerializer.ForIsomorphicType  54uy
            id<Result<unit, SubjectFailure<GrainEnqueueActionError>>>
            id<Result<unit, SubjectFailure<GrainEnqueueActionError>>>
            id<_>

        UntypedSerializer.ForIsomorphicType  55uy
            (fun (s, x: uint64) -> (s |> List.map (fun v -> v :> Subject<'SubjectId>)), x)
            (fun (s, x: uint64) -> (s |> List.map (fun v -> v :?> 'Subject)), x)
            id<_>

        UntypedSerializer.ForIsomorphicType  56uy
            id<GetSnapshotOfVersion>
            id<GetSnapshotOfVersion>
            id<_>

        // UntypedSerializer.ForIsomorphicType  57uy

        UntypedSerializer.ForIsomorphicType  58uy
            id<LocalSubjectPKeyReference>
            id<LocalSubjectPKeyReference>
            id<_>

        UntypedSerializer.ForIsomorphicType  59uy
            id<Result<unit, SubjectFailure<GrainTriggerDynamicSubscriptionError>>>
            id<Result<unit, SubjectFailure<GrainTriggerDynamicSubscriptionError>>>
            id<_>

        UntypedSerializer.ForIsomorphicType  60uy
            id<ClientGrainCallContext>
            id<ClientGrainCallContext>
            id<_>

        UntypedSerializer.ForIsomorphicType  61uy
            VersionedSubject<Subject<'SubjectId>, 'SubjectId>.CastUnsafe
            VersionedSubject<'Subject, 'SubjectId>.CastUnsafe
            id<_>

        UntypedSerializer.ForIsomorphicType   62uy
            (Result.mapBoth VersionedSubject<Subject<'SubjectId>, 'SubjectId>.CastUnsafe GrainConstructionError<OpError>.CastUnsafe)
            (Result.mapBoth VersionedSubject<'Subject, 'SubjectId>.CastUnsafe            GrainConstructionError<'OpError>.CastUnsafe)
            id<_>

        UntypedSerializer.ForIsomorphicType   63uy
            (Result.mapBoth VersionedSubject<Subject<'SubjectId>, 'SubjectId>.CastUnsafe GrainTransitionError<OpError>.CastUnsafe)
            (Result.mapBoth VersionedSubject<'Subject, 'SubjectId>.CastUnsafe            GrainTransitionError<'OpError>.CastUnsafe)
            id<_>

        UntypedSerializer.ForIsomorphicType   64uy
            (Result.mapBoth VersionedSubject<Subject<'SubjectId>, 'SubjectId>.CastUnsafe GrainOperationError<OpError>.CastUnsafe)
            (Result.mapBoth VersionedSubject<'Subject, 'SubjectId>.CastUnsafe            GrainOperationError<'OpError>.CastUnsafe)
            id<_>

        UntypedSerializer.ForIsomorphicType   65uy
            (Result.mapBoth VersionedSubject<Subject<'SubjectId>, 'SubjectId>.CastUnsafe GrainMaybeConstructionError<OpError>.CastUnsafe)
            (Result.mapBoth VersionedSubject<'Subject, 'SubjectId>.CastUnsafe            GrainMaybeConstructionError<'OpError>.CastUnsafe)
            id<_>

        UntypedSerializer.ForIsomorphicType   66uy
            (Result.mapBoth (Option.map VersionedSubject<Subject<'SubjectId>, 'SubjectId>.CastUnsafe) id<GrainGetError>)
            (Result.mapBoth (Option.map VersionedSubject<'Subject, 'SubjectId>.CastUnsafe)            id<GrainGetError>)
            id<_>

        UntypedSerializer.ForIsomorphicType   67uy
            LibLifeCycleCore.SubjectChange<Subject<'SubjectId>, 'SubjectId>.CastUnsafe
            LibLifeCycleCore.SubjectChange<'Subject, 'SubjectId>.CastUnsafe
            id<_>

        UntypedSerializer.ForIsomorphicType  68uy
            id<Option<Tuple<int64, uint64>>>
            id<Option<Tuple<int64, uint64>>>
            id<_>

        UntypedSerializer.ForIsomorphicType  69uy
            id<list<string>>
            id<list<string>>
            id<_>

        UntypedSerializer.ForIsomorphicType  70uy
            (List.map VersionedSubject<Subject<'SubjectId>, 'SubjectId>.CastUnsafe)
            (List.map VersionedSubject<'Subject, 'SubjectId>.CastUnsafe)
            id<_>

        UntypedSerializer.ForIsomorphicType  71uy
            (Tuple.Map (List.map VersionedSubject<Subject<'SubjectId>, 'SubjectId>.CastUnsafe) id<uint64>)
            (Tuple.Map (List.map VersionedSubject<'Subject, 'SubjectId>.CastUnsafe) id<uint64>)
            id<_>

        UntypedSerializer.ForIsomorphicType 72uy
            id<CallOrigin>
            id<CallOrigin>
            id<_>

        UntypedSerializer.ForIsomorphicType 73uy
            id<Option<BlobData>>
            id<Option<BlobData>>
            id<_>

        UntypedSerializer.ForIsomorphicType  74uy
            (List.map (fun (id: 'SubjectId) -> id :> SubjectId))
            (List.map (fun (id: SubjectId) -> id :?> 'SubjectId))
            id<_>

        UntypedSerializer.ForIsomorphicType  75uy
            (List.map SubjectAuditData<LifeAction, Constructor>.CastUnsafe)
            (List.map SubjectAuditData<'LifeAction, 'Constructor>.CastUnsafe)
            id<_>

        // UntypedSerializer.ForIsomorphicType  76uy

        UntypedSerializer.ForIsomorphicType  77uy
            (Result.mapBoth id<unit> (SubjectFailure.CastUnsafe GrainTransitionError<OpError>.CastUnsafe))
            (Result.mapBoth id<unit> (SubjectFailure.CastUnsafe GrainTransitionError<'OpError>.CastUnsafe))
            id<_>

        UntypedSerializer.ForIsomorphicType  78uy
            (NonemptyList.map (fun (id: 'LifeAction) -> id :> LifeAction))
            (NonemptyList.map (fun (id: LifeAction) -> id :?> 'LifeAction))
            id<_>

        UntypedSerializer.ForIsomorphicType  79uy
            // Result<Option<'LifeAction>, GrainTriggerTimerError<'OpError, 'LifeAction>>
            (Result.mapBoth (fun (id: Option<'LifeAction>) -> id |> Option.map (fun action -> action :> LifeAction)) GrainTriggerTimerError<OpError, LifeAction>.CastUnsafe)
            (Result.mapBoth (fun (id: Option<LifeAction>) -> id |> Option.map (fun action -> action :?> 'LifeAction)) GrainTriggerTimerError<'OpError, 'LifeAction>.CastUnsafe)
            id<_>

        UntypedSerializer.ForIsomorphicType  80uy
            (Result.mapBoth id<unit> GrainConstructionError<OpError>.CastUnsafe)
            (Result.mapBoth id<unit> GrainConstructionError<'OpError>.CastUnsafe)
            id<_>

        UntypedSerializer.ForIsomorphicType  81uy
            (Result.mapBoth id<unit> GrainTransitionError<OpError>.CastUnsafe)
            (Result.mapBoth id<unit> GrainTransitionError<'OpError>.CastUnsafe)
            id<_>

        UntypedSerializer.ForIsomorphicType  82uy
            (Result.mapBoth id<unit> GrainOperationError<OpError>.CastUnsafe)
            (Result.mapBoth id<unit> GrainOperationError<'OpError>.CastUnsafe)
            id<_>

        UntypedSerializer.ForIsomorphicType  83uy
            (Result.mapBoth id<unit> GrainMaybeConstructionError<OpError>.CastUnsafe)
            (Result.mapBoth id<unit> GrainMaybeConstructionError<'OpError>.CastUnsafe)
            id<_>
    ]
    |> fun untypedSerializers ->
        // assert no duplicate Ids before return
        match untypedSerializers |> Seq.groupBy (fun s -> s.TypeId) |> Seq.filter (fun (_, g) -> (Seq.length g) > 1) |> Seq.map fst |> Seq.tryHead with
        | None ->
            untypedSerializers
        | Some duplicateTypeId ->
            failwithf "Duplicate typeId in subject serializers: %d" duplicateTypeId

type private OrleansSubjectGrainsContractSingletonSerializer<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId
                                                     when 'Subject              :> Subject<'SubjectId>
                                                     and  'LifeAction           :> LifeAction
                                                     and  'OpError              :> OpError
                                                     and  'Constructor          :> Constructor
                                                     and  'LifeEvent            :> LifeEvent
                                                     and  'LifeEvent            : comparison
                                                     and  'SubjectIndex         :> SubjectIndex<'OpError>
                                                     and  'SubjectId            :> SubjectId
                                                     and  'SubjectId            : comparison> () =

    let untypedSerializers =

        // assert all grains params and return value types are covered by serializers

        // FIXME - awful hack to reflect ISubjectGrain & other types defined in LibLifeCycleHost (will be skipped on client side)
        let maybeLifeCycleHostAssembly =
            AppDomain.CurrentDomain.GetAssemblies()
            |> Seq.tryFind (fun a -> a.GetName().Name = "LibLifeCycleHost")

        let maybeISubjectGrainTypeDef =
            maybeLifeCycleHostAssembly
            |> Option.map (fun a -> a.GetType("LibLifeCycleHost.ISubjectGrain`6")) // 6 is number of params

        let maybeISubjectGrainObserverTypeDef =
            maybeLifeCycleHostAssembly
            |> Option.map (fun a -> a.GetType("LibLifeCycleHost.ISubjectGrainObserver`2")) // 2 is number of params

        let maybeIDynamicSubscriptionDispatcherGrainType =
            maybeLifeCycleHostAssembly
            |> Option.map (fun a -> a.GetType("LibLifeCycleHost.IDynamicSubscriptionDispatcherGrain"))

        getUntypedSubjectSerializers<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>()
        |> fun serializers ->

            let supportedTypes =
                serializers
                |> Seq.map (fun x -> x.Type, x.TypeId)
                |> readOnlyDict

            let declaredGrainParamAndRetValTypes =
                [
                    yield typeof<ISubjectClientGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>>
                    yield typeof<ISubjectRepoGrain<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'SubjectIndex, 'OpError>>
                    yield typeof<IBlobRepoGrain>
                    yield typeof<ISubjectIdGenerationGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>>
                    yield typeof<ISubjectReflectionGrain<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>>
                    yield typeof<ILifeEventAwaiter<'Subject, 'LifeEvent, 'SubjectId>>

                    match maybeISubjectGrainTypeDef with
                    | None -> ()
                    | Some iSubjectGrainTypeDef ->
                        yield iSubjectGrainTypeDef.MakeGenericType(
                            [| typeof<'Subject>; typeof<'LifeAction>; typeof<'OpError>; typeof<'Constructor>; typeof<'LifeEvent>; typeof<'SubjectId> |])

                    match maybeISubjectGrainObserverTypeDef with
                    | None -> ()
                    | Some iSubjectGrainObserverTypeDef ->
                        yield iSubjectGrainObserverTypeDef.MakeGenericType(
                            [| typeof<'Subject>; typeof<'SubjectId> |])

                    match maybeIDynamicSubscriptionDispatcherGrainType with
                    | None -> ()
                    | Some iDynamicSubscriptionDispatcherGrainType ->
                        yield iDynamicSubscriptionDispatcherGrainType
                ]
                |> Seq.collect (fun typ -> typ.GetMethods ())
                |> Seq.collect (
                    fun method ->
                        seq {
                            yield! method.GetParameters() |> Seq.map (fun p -> p.ParameterType)
                            let retType = method.ReturnType
                            yield
                                if retType.IsGenericType && retType.GetGenericTypeDefinition() = typedefof<Task<_>> then
                                    retType.GetGenericArguments().[0]
                                else
                                    retType
                        })
                |> Seq.map (fun t -> t, ())
                |> readOnlyDict

            // also assert that no redundant serializers (only inside host, because client will have a few)
            match maybeISubjectGrainTypeDef with
            | None -> ()
            | Some _ ->
                supportedTypes
                |> Seq.filter (fun kvp -> kvp.Key |> declaredGrainParamAndRetValTypes.ContainsKey |> not)
                |> List.ofSeq
                |>
                    function
                    | [] -> ()
                    | redundantTypesWithTypeIds ->
                        let typesMessage =
                            redundantTypesWithTypeIds
                            |> List.fold
                                (fun (sb: StringBuilder) kvp ->
                                    sb
                                        .Append(" - ")
                                        .Append("type ID ")
                                        .Append(kvp.Value)
                                        .Append(": ")
                                        .Append(kvp.Key.FullName)
                                        .AppendLine()
                                )
                                (StringBuilder())
                            |> (fun sb -> sb.ToString())

                        failwithf "Remove redundant subject serializers for following types:\n%s" typesMessage

            declaredGrainParamAndRetValTypes
            |> fun dict -> dict.Keys
            |> Seq.filter (not << supportedTypes.ContainsKey)
            |> Seq.filter (
                // excuse some of declared types that Orleans knows about
                fun typ ->
                    [
                        yield typeof<string>
                        yield typeof<bool>
                        yield typeof<byte>
                        yield typeof<uint64>
                        yield typeof<TimeSpan>
                        yield typeof<Guid>
                        yield typeof<unit>
                        yield typeof<Void>
                        yield typeof<Task>

                        // these implement Orleans.IGrainObserver i.e. OK
                        yield typeof<ILifeEventAwaiter<'Subject, 'LifeEvent, 'SubjectId>>

                        match maybeISubjectGrainObserverTypeDef with
                        | None -> ()
                        | Some iSubjectGrainObserverTypeDef ->
                            yield iSubjectGrainObserverTypeDef.MakeGenericType(
                                [| typeof<'Subject>; typeof<'SubjectId> |])
                    ]
                    |> List.contains typ
                    |> not
                )
            |> List.ofSeq
            |>
                function
                | [] -> serializers
                | notSupportedTypes ->
                    let typesMessage =
                        notSupportedTypes
                        |> List.fold
                            (fun (sb: StringBuilder) t ->
                                sb
                                    .Append(" - ")
                                    .Append(t.FullName)
                                    .AppendLine()
                            )
                            (StringBuilder())
                        |> (fun sb -> sb.ToString())

                    failwithf "Need to define serializers for following types on subject grain interfaces (if Orleans can serialize this type without the help of a custom serializer, add an exception to the above filter):\n%s" typesMessage

    let untypedSerializersByTypeId = untypedSerializers |> Seq.map (fun untypedSer -> (untypedSer.TypeId, untypedSer)) |> readOnlyDict
    let untypedSerializersByType   = untypedSerializers |> Seq.map (fun untypedSer -> (untypedSer.Type,   untypedSer)) |> readOnlyDict

    static let singletonInstance: Lazy<OrleansSubjectGrainsContractSingletonSerializer<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>> =
        lazy(OrleansSubjectGrainsContractSingletonSerializer<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>())

    static member Singleton = singletonInstance.Force()

    member _.DeepCopy(source: obj, _context: ICopyContext): obj =
        // All types deserialized by Form are expected to be immutable
        source

    member _.Deserialize(_expectedType: Type, context: IDeserializationContext): obj =
        let _version = context.DeserializeInner(typeof<byte>) :?> byte // Unused for now
        let serializedTypeId = context.DeserializeInner(typeof<byte>) :?> byte
        let serializedBytes = context.DeserializeInner(typeof<byte[]>) :?> byte[]

        match untypedSerializersByTypeId.TryGetValue serializedTypeId with
        | (true, serializer) ->
            serializer.Fro serializedBytes
        | (false, _) ->
            failwithf "Invalid type header for deserialization %A" serializedTypeId

    member _.IsSupportedType(itemType: Type): bool =
        untypedSerializersByType.ContainsKey itemType ||
        untypedSerializersByType.ContainsKey (itemType.BaseType) // for union case instance types that might be derived


    member _.Serialize(item: obj, context: ISerializationContext, _expectedType: Type): unit =
        if isNull item then
            failwith "OrleansSerializer.Serialize got null; that's not expected"

        let itemType = item.GetType()

        let maybeSerializer =
            IReadOnlyDictionary.tryGetValue itemType untypedSerializersByType
            // F# Union instances may be derived type
            |> Option.orElseWith (fun _ -> IReadOnlyDictionary.tryGetValue itemType.BaseType untypedSerializersByType)

        match maybeSerializer with
        | Some serializer ->
            context.SerializeInner<byte> 0uy // Version. Unused for now.
            context.SerializeInner<byte> serializer.TypeId

            item
            |> serializer.To
            |> context.SerializeInner<byte[]>

        | None ->
            failwithf "Invalid type for serialization %s, item: %O" itemType.FullName item

type private OrleansSubjectGrainsContractSerializer<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId
                                                     when 'Subject              :> Subject<'SubjectId>
                                                     and  'LifeAction           :> LifeAction
                                                     and  'OpError              :> OpError
                                                     and  'Constructor          :> Constructor
                                                     and  'LifeEvent            :> LifeEvent
                                                     and  'LifeEvent            : comparison
                                                     and  'SubjectIndex         :> SubjectIndex<'OpError>
                                                     and  'SubjectId            :> SubjectId
                                                     and  'SubjectId            : comparison> () =

    // reuse because it's expensive to create
    let impl = OrleansSubjectGrainsContractSingletonSerializer<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>.Singleton

    interface IExternalSerializer with
        member _.DeepCopy(source: obj, context: ICopyContext): obj =
            impl.DeepCopy(source, context)

        member _.Deserialize(expectedType: Type, context: IDeserializationContext): obj =
            impl.Deserialize (expectedType, context)

        member _.IsSupportedType(itemType: Type): bool =
            impl.IsSupportedType itemType

        member _.Serialize(item: obj, context: ISerializationContext, expectedType: Type): unit =
            impl.Serialize(item, context, expectedType)

let getOrleansSerializerTypeLifeCycleAdapter (lifeCycle: LifeCycleDef) : Type =
    lifeCycle.Invoke
        { new FullyTypedLifeCycleDefFunction<_> with
            member _.Invoke (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>) =
               // create dummy instance to validate types early and fail fast if invalid (tried static values too but it didn't work)
               let serializer = OrleansSubjectGrainsContractSerializer<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>()
               serializer.GetType() }

let getSummaryEncoders<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId
                             when 'Subject              :> Subject<'SubjectId>
                             and  'LifeAction           :> LifeAction
                             and  'OpError              :> OpError
                             and  'Constructor          :> Constructor
                             and  'LifeEvent            :> LifeEvent
                             and  'LifeEvent            : comparison
                             and  'SubjectIndex         :> SubjectIndex<'OpError>
                             and  'SubjectId            :> SubjectId
                             and  'SubjectId            : comparison> () : seq<Type * (obj -> string)> =

    getUntypedSubjectSerializers<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>()
    |> Seq.map (fun x -> (x.Type, x.ToSummary))
    // append summarizers for types not present in grain contract but still useful for logging
    |> Seq.append (
        seq {
            yield summarizerForType (fun (e: 'OpError) -> e :> OpError)
            yield summarizerForType (fun (e: GrainTriggerSubscriptionError<'OpError>) -> GrainTriggerSubscriptionError.CastUnsafe e)
            yield summarizerForType id<GrainTriggerDynamicSubscriptionError>
            yield summarizerForType id<GrainEnqueueActionError>
            yield summarizerForType (fun (e: GrainIdGenerationError<'OpError>) -> GrainIdGenerationError.CastUnsafe e)
            yield summarizerForType (fun (e: GrainMaybeConstructionError<'OpError>) -> GrainMaybeConstructionError.CastUnsafe e)
            yield summarizerForType (fun (e: GrainPrepareConstructionError<'OpError>) -> GrainPrepareConstructionError.CastUnsafe e)
            yield summarizerForType (fun (e: GrainPrepareTransitionError<'OpError>) -> GrainPrepareTransitionError.CastUnsafe e)
            yield summarizerForType id<GrainGetError>
            yield summarizerForType id<LocalSubjectPKeyReference>
            yield summarizerForType (fun (actions: NonemptyList<'LifeAction>) -> actions.ToList |> List.map (fun action -> action :> LifeAction))
        })

let private getUntypedViewSerializers<'Input, 'Output, 'OpError
                                        when 'Input :> ViewInput<'Input>
                                        and  'Output :> ViewOutput<'Output>
                                        and  'OpError :> ViewOpError<'OpError>
                                        and  'OpError :> OpError> () =

    // List out all IViewGrain parameters and response types
    // NOTES:
    // * the type ID mapping can *never* change. Do not re-use. If you ever remove a serializer
    //   simply comment out that line, so others don't accidentally re-use

    [
        UntypedSerializer.ForIsomorphicType 1uy
            id<ClientGrainCallContext>
            id<ClientGrainCallContext>
            id<_>

        UntypedSerializer.ForConcreteTypeWithExplicitCodec 2uy
            ('Input.Codec())

        UntypedSerializer.ForConcreteTypeWithExplicitCodec 3uy
            (CodecLib.Codecs.result ('Output.Codec()) (GrainExecutionError<'OpError>.CodecWithExplicitOpError ('OpError.Codec())))
    ]
    |> fun untypedSerializers ->
        // assert no duplicate Ids before return
        match untypedSerializers |> Seq.groupBy (fun s -> s.TypeId) |> Seq.filter (fun (_, g) -> (Seq.length g) > 1) |> Seq.map fst |> Seq.tryHead with
        | None ->
            untypedSerializers
        | Some duplicateTypeId ->
            failwithf "Duplicate typeId in view serializers: %d" duplicateTypeId

type private OrleansViewGrainsContractSerializer<'Input, 'Output, 'OpError
                                                  when 'Input :> ViewInput<'Input>
                                                  and  'Output :> ViewOutput<'Output>
                                                  and  'OpError :> ViewOpError<'OpError>
                                                  and  'OpError :> OpError> () =

    let untypedSerializers =

        // assert all grains params and return value types are covered by serializers

        // FIXME - awful hack to reflect IViewGrain & other types defined in LibLifeCycleHost (will be skipped on client side)
        let maybeIViewGrainTypeDef =
            AppDomain.CurrentDomain.GetAssemblies()
            |> Seq.tryFind (fun a -> a.GetName().Name = "LibLifeCycleHost")
            |> Option.map (fun a -> a.GetType("LibLifeCycleHost.IViewGrain`3")) // 3 is number of params

        getUntypedViewSerializers<'Input, 'Output, 'OpError>()
        |> fun serializers ->

            let supportedTypes =
                serializers
                |> Seq.map (fun x -> x.Type, x.TypeId)
                |> readOnlyDict

            let declaredGrainParamAndRetValTypes =
                [
                    yield typeof<IViewClientGrain<'Input, 'Output, 'OpError>>
                    match maybeIViewGrainTypeDef with
                    | None -> ()
                    | Some iViewGrainTypeDef ->
                        yield iViewGrainTypeDef.MakeGenericType(
                            [| typeof<'Input>; typeof<'Output>; typeof<'OpError> |])
                ]
                |> Seq.collect (fun typ -> typ.GetMethods ())
                |> Seq.collect (
                    fun method ->
                        seq {
                            yield! method.GetParameters() |> Seq.map (fun p -> p.ParameterType)
                            let retType = method.ReturnType
                            yield
                                if retType.IsGenericType && retType.GetGenericTypeDefinition() = typedefof<Task<_>> then
                                    retType.GetGenericArguments().[0]
                                else
                                    retType
                        })
                |> Seq.map (fun t -> t, ())
                |> readOnlyDict

            // also assert that no redundant serializers (only inside host, because client will have a few)
            match maybeIViewGrainTypeDef with
            | None -> ()
            | Some _ ->
                supportedTypes
                |> Seq.filter (fun kvp -> kvp.Key |> declaredGrainParamAndRetValTypes.ContainsKey |> not)
                |> List.ofSeq
                |>
                    function
                    | [] -> ()
                    | redundantTypesWithTypeIds ->
                        let typesMessage =
                            redundantTypesWithTypeIds
                            |> List.fold
                                (fun (sb: StringBuilder) kvp ->
                                    sb
                                        .Append(" - ")
                                        .Append("type ID ")
                                        .Append(kvp.Value)
                                        .Append(": ")
                                        .Append(kvp.Key.FullName)
                                        .AppendLine()
                                )
                                (StringBuilder())
                            |> (fun sb -> sb.ToString())

                        failwithf "Remove redundant view serializers for following types:\n%s" typesMessage

            declaredGrainParamAndRetValTypes
            |> fun dict -> dict.Keys
            |> Seq.filter (not << supportedTypes.ContainsKey)
            |> Seq.filter (
                // excuse some of declared types that Orleans knows about
                fun _typ ->
                    true)
            |> List.ofSeq
            |>
                function
                | [] -> serializers
                | notSupportedTypes ->
                    let typesMessage =
                        notSupportedTypes
                        |> List.fold
                            (fun (sb: StringBuilder) t ->
                                sb
                                    .Append(" - ")
                                    .Append(t.FullName)
                                    .AppendLine()
                            )
                            (StringBuilder())
                        |> (fun sb -> sb.ToString())

                    failwithf "Need to define serializers for following types on view grain interfaces (if Orleans can serialize this type without the help of a custom serializer, add an exception to the above filter):\n%s" typesMessage

    let untypedSerializersByTypeId = untypedSerializers |> Seq.map (fun untypedSer -> (untypedSer.TypeId, untypedSer)) |> readOnlyDict
    let untypedSerializersByType   = untypedSerializers |> Seq.map (fun untypedSer -> (untypedSer.Type,   untypedSer)) |> readOnlyDict

    interface IExternalSerializer with

        member _.DeepCopy(source: obj, _context: ICopyContext): obj =
            // All types deserialized by Form are expected to be immutable
            source

        member _.Deserialize(_expectedType: Type, context: IDeserializationContext): obj =
            let _version = context.DeserializeInner(typeof<byte>) :?> byte // Unused for now
            let serializedTypeId = context.DeserializeInner(typeof<byte>) :?> byte
            let serializedBytes = context.DeserializeInner(typeof<byte[]>) :?> byte[]

            match untypedSerializersByTypeId.TryGetValue serializedTypeId with
            | (true, serializer) ->
                serializer.Fro serializedBytes
            | (false, _) ->
                failwithf "Invalid type header for deserialization %A" serializedTypeId

        member _.IsSupportedType(itemType: Type): bool =
            untypedSerializersByType.ContainsKey itemType ||
            untypedSerializersByType.ContainsKey (itemType.BaseType) // for union case instance types that might be derived


        member _.Serialize(item: obj, context: ISerializationContext, _expectedType: Type): unit =
            if isNull item then
                failwith "OrleansSerializer.Serialize got null; that's not expected"

            let itemType = item.GetType()

            let maybeSerializer =
                IReadOnlyDictionary.tryGetValue itemType untypedSerializersByType
                // F# Union instances may be derived type
                |> Option.orElseWith (fun _ -> IReadOnlyDictionary.tryGetValue itemType.BaseType untypedSerializersByType)

            match maybeSerializer with
            | Some serializer ->
                context.SerializeInner<byte> 0uy // Version. Unused for now.
                context.SerializeInner<byte> serializer.TypeId

                item
                |> serializer.To
                |> context.SerializeInner<byte[]>

            | None ->
                failwithf "Invalid type for serialization %s, item: %O" itemType.FullName item

type private AnchorTypeForModule = private AnchorTypeForModule of unit

let private getOrleansSerializerTypeViewAdapterTyped<'Input, 'Output, 'OpError
      when 'Input :> ViewInput<'Input>
      and  'Output :> ViewOutput<'Output>
      and  'OpError :> ViewOpError<'OpError>
      and  'OpError :> OpError>
   (_view: ViewDef<'Input, 'Output, 'OpError>) =
    // create dummy instance to validate types early and fail fast if invalid (tried static values too but it didn't work)
   let serializer = OrleansViewGrainsContractSerializer<'Input, 'Output, 'OpError>()
   serializer.GetType()

let getOrleansSerializerTypeViewAdapter (view: IViewDef) : Option<Type> =
    view.Invoke
        { new FullyTypedViewDefFunction<_> with
            member _.Invoke (viewDef: ViewDef<'Input, 'Output, 'OpError>) =
                let viewDefType = viewDef.GetType()
                let inputType = typeof<'Input>
                let outputType = typeof<'Output>
                let errorType = typeof<'OpError>
                if // 'Input implements ViewInput<'Input>
                    inputType.GetInterfaces() |> Seq.exists (fun it -> it.IsGenericType && it.GetGenericTypeDefinition() = typedefof<ViewInput<NoInput>> && it.GenericTypeArguments[0] = inputType) &&
                    // 'Output implements ViewOutput<'Output>
                    outputType.GetInterfaces() |> Seq.exists (fun it -> it.IsGenericType && it.GetGenericTypeDefinition() = typedefof<ViewOutput<NoOutput>> && it.GenericTypeArguments[0] = outputType) &&
                    // 'OpError implements ViewOpError<'OpError>
                    errorType.GetInterfaces() |> Seq.exists (fun it -> it.IsGenericType && it.GetGenericTypeDefinition() = typedefof<ViewOpError<NoViewError>> && it.GenericTypeArguments[0] = errorType)
                    then
                    typedefof<AnchorTypeForModule>.DeclaringType
                        .GetMethod(nameof(getOrleansSerializerTypeViewAdapterTyped), System.Reflection.BindingFlags.Static ||| System.Reflection.BindingFlags.NonPublic)
                        .MakeGenericMethod(viewDefType.GenericTypeArguments)
                        .Invoke(null, [| view |])
                        :?> Type
                    |> Some
                else
                    None }