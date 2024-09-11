module LibLifeCycleCore.OrleansEx.Serialization

// couldn't make LibLifeCycleCore.Orleans namespace because it makes "open Orleans" ambiguous in a few places

open Orleans
open Orleans.ApplicationParts
open Orleans.Serialization
open System
open Orleans.Utilities
open FSharp.Reflection
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Options
open Orleans.Configuration
open System.Reflection

// We want to take full control of all serialization since we're dealing with F# types, and we can write better serializers than
// what Orleans can generate. Furthermore, our serializers can version forward.

let registerSerializers
        (lifeCycleDefs: List<LifeCycleDef>)
        (viewDefs: List<IViewDef>)
        (services: IServiceCollection) =

    // TODO: why it's not invoked from CodecLib module initialization code on open CodecLib ?
    CodecLib.configureCodecLib()
    // eagerly create serializers to fail fast if misconfigured
    let lifeCycleProviderTypes =
        lifeCycleDefs
        |> List.map Serializer.getOrleansSerializerTypeLifeCycleAdapter
    let viewProviderTypes =
        viewDefs
        |> List.choose Serializer.getOrleansSerializerTypeViewAdapter
    let providerTypes = lifeCycleProviderTypes @ viewProviderTypes

    services.AddSingleton<IConfigureOptions<SerializationProviderOptions>>(
        ConfigureOptions<SerializationProviderOptions>(fun options -> options.SerializationProviders.AddRange providerTypes)
        :> IConfigureOptions<SerializationProviderOptions>
    )
    |> ignore

// FIXME - ORLEANS ISSUE
// Unfortunately I need access to this Orleans internal class to register a known type with Orleans
// the right way. In theory, this should have been as simple as ".AddKnownType typeof<Foo>", but
// Orleans hasn't exposed that API. They instead expect us to use the KnownType attribute to indicate
// these types, but KnownType attributes are only processed from assemblies that contain the Grain
// classes, which would require that I link a concrete subject types project to the  generic
// LibLifeCycleHost project. I'll raise an issue with Orleans.
let private getOrleansTypeString typ =
    typeof<SerializationManager>.Assembly.GetType("Orleans.Serialization.TypeUtilities")
        .GetMethod("OrleansTypeKeyString", BindingFlags.Static ||| BindingFlags.Public)
        .Invoke(null, [| typ |])
        :?> string

let rec private getTypeAndAllRecursivelyReferencedGenericTypes (typ: Type) =
    let rec getTypeAndAllRecursivelyReferencedGenericTypes' (visitedSoFar: Map<string, Type>) (typ: Type) =
        if visitedSoFar.ContainsKey typ.AssemblyQualifiedName then
            visitedSoFar
        else
            let visitedSoFar = visitedSoFar.Add(typ.AssemblyQualifiedName, typ)
            if typ.IsGenericType then
                let genericTypeDefinition = typ.GetGenericTypeDefinition()
                let visitedSoFar =
                    if not (visitedSoFar.ContainsKey genericTypeDefinition.AssemblyQualifiedName) then
                        visitedSoFar.Add(genericTypeDefinition.AssemblyQualifiedName, genericTypeDefinition)
                    else
                        visitedSoFar

                typ.GetGenericArguments()
                |> Seq.fold (fun soFar gTyp ->
                    getTypeAndAllRecursivelyReferencedGenericTypes' soFar gTyp
                ) visitedSoFar
            else
                visitedSoFar

    getTypeAndAllRecursivelyReferencedGenericTypes' Map.empty typ
    |> Map.values

// Union type case instances are (generally) an instance of a derived type (unless it's a single-case union, or a param-less union)
let getCaseInstanceTypesForUnionTypes (typesToInspect: List<Type>) : List<(* UnionType *) Type * (* CaseType *) Type> = // TODO: extract internal TypeUtils module or smth
    let unionTypes = typesToInspect |> List.filter FSharpType.IsUnion

    let closedGenericUnionTypesWithDefinitions, nonGenericUnionTypes =
        unionTypes
        |> List.partition (fun t -> t.IsGenericType)
        |> fun (genericTypes, nonGenericTypes) ->
            genericTypes |> List.map (fun closedGenericUnionType -> closedGenericUnionType, closedGenericUnionType.GetGenericTypeDefinition()),
            System.Collections.Generic.HashSet<_>(nonGenericTypes)

    let unionTypes = System.Collections.Generic.HashSet<_>(unionTypes)
    unionTypes
    |> Seq.map (fun typ -> typ.Assembly)
    |> Seq.distinct
    |> Seq.collect (fun assembly ->
        assembly.GetTypes()
        |> Seq.where (fun candidateType -> not (candidateType.IsInterface || unionTypes.Contains candidateType)))
    |> Seq.collect (fun candidateType -> seq {
        if not candidateType.IsGenericType && nonGenericUnionTypes.Contains candidateType.BaseType then
            candidateType.BaseType, candidateType
        elif candidateType.IsGenericType && candidateType.BaseType <> null && candidateType.BaseType.IsGenericType then
            let candidateTypeBaseTypeDef = candidateType.BaseType.GetGenericTypeDefinition()
            yield!
                closedGenericUnionTypesWithDefinitions
                // This was a bit of a rabbit-hole to figure out
                // Note that candidateType is an open generic type, not the generic type definition
                // So we have to get the BaseType and compare the generic type definitions to figure
                // out the equality, and then instantiate a closed generic type based on the parameters of the
                // union type
                |> List.filter (fun (_, typDef) -> candidateTypeBaseTypeDef = typDef)
                |> List.collect (fun (closedGenericUnionType, _) ->
                    [
                        closedGenericUnionType, candidateType.MakeGenericType(closedGenericUnionType.GetGenericArguments())
                        closedGenericUnionType, candidateType.GetGenericTypeDefinition()
                    ])
        else
            ()
    })
    |> Seq.toList

let configureApplicationParts (lifeCycleDefs: List<LifeCycleDef>) (viewDefs: List<IViewDef>) (parts: IApplicationPartManager) (buildAssembly: Assembly) : unit =
    // We need to register types that orleans should expect to serialize/deserialize
    // This is mostly only required for deserialization, so that the deserializer knows which
    // assembly to get the type from. Note that this is a very advanced use of Orleans, and I
    // had to go through the code to figure these undocumented apis out. As described in the comment
    // above, Orleans expects us to use the KnownType or KnownAssembly assembly-level attribute
    // to indicate these types, but that doesn't work for us, due to separation of the grain class
    // and the actual model classes.
    let lifeCyclesTypes =
        lifeCycleDefs
        |> Seq.collect (fun def ->
            def.Invoke
                { new FullyTypedLifeCycleDefFunction<_> with
                    member _.Invoke (def: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>) =
                        seq {
                            // TODO: do we repeat ourselves ? is this list of types already in Serializer.fs ?
                            typeof<'Subject>
                            typeof<'LifeAction>
                            typeof<'OpError>
                            typeof<'Constructor>
                            typeof<'LifeEvent>
                            typeof<'SubjectIndex>
                            typeof<'SubjectId>
                            typeof<NonemptySet<Guid>> // think NonemptySet<GrainSideEffectId>
                            // for repo grain
                            typedefof<IndexQuery<'SubjectIndex>>
                            typedefof<PreparedIndexPredicate<'SubjectIndex>>
                            typedefof<ResultSetOptions<'SubjectIndex>>
                            typedefof<TemporalSnapshot<'Subject, 'LifeAction, 'Constructor, 'SubjectId>>
                            typeof<SubscriptionTriggerType>
                            // for trigger dynamic subscription
                            typeof<LocalSubjectPKeyReference> } })

    let viewsTypes =
        viewDefs
        |> Seq.collect (fun def ->
            def.Invoke
                { new FullyTypedViewDefFunction<_> with
                    member _.Invoke (def: ViewDef<'Input, 'Output, 'ViewOpError>) =
                        let inputType = typeof<'Input>
                        let outputType = typeof<'Output>
                        let errorType = typeof<'ViewOpError>
                        if // 'Input implements ViewInput<'Input>
                            inputType.GetInterfaces() |> Seq.exists (fun it -> it.IsGenericType && it.GetGenericTypeDefinition() = typedefof<ViewInput<NoInput>> && it.GenericTypeArguments[0] = inputType) &&
                            // 'Output implements ViewOutput<'Output>
                            outputType.GetInterfaces() |> Seq.exists (fun it -> it.IsGenericType && it.GetGenericTypeDefinition() = typedefof<ViewOutput<NoOutput>> && it.GenericTypeArguments[0] = outputType) &&
                            // 'OpError implements ViewOpError<'OpError>
                            errorType.GetInterfaces() |> Seq.exists (fun it -> it.IsGenericType && it.GetGenericTypeDefinition() = typedefof<ViewOpError<NoViewError>> && it.GenericTypeArguments[0] = errorType)
                            then
                            seq {
                                inputType
                                outputType
                                errorType
                            }
                        else
                            Seq.empty})

    let allLifeCycleAndViewArgTypes =
        Seq.append lifeCyclesTypes viewsTypes
        |> Seq.toList

    allLifeCycleAndViewArgTypes
    |> Seq.collect getTypeAndAllRecursivelyReferencedGenericTypes
    |> Seq.append (
            // Union type case instances are (generally) an instance of a derived type.
            // We need to make orleans aware of these, in addition to the base type itself
            allLifeCycleAndViewArgTypes
            |> getCaseInstanceTypesForUnionTypes
            |> List.map snd
        )
    |> Seq.append (
        seq {
            typeof<SubjectId>
            typeof<Subject>
            typedefof<Subject<_>>
            typeof<LifeAction>
            typeof<LifeEvent>
            typeof<OpError>
            typeof<Constructor>
            typedefof<ViewInput<NoInput>>
            typedefof<ViewOutput<NoOutput>>
            typedefof<ViewOpError<NoViewError>>
        })
    |> Seq.distinct
    |> fun allTypes ->
        // Register a Serialization "extension point"
        parts.AddFeatureProvider(
            { new IApplicationFeatureProvider<SerializerFeature> with
                member this.PopulateFeature(_parts: seq<IApplicationPart>, feature: SerializerFeature) =
                    allTypes
                    |> Seq.iter (fun typ ->
                        let fullyQualifiedTypeName =
                            RuntimeTypeNameFormatter.Format(typ)
                            // FIXME - ORLEANS ISSUE
                            // strip out the assembly name, as Assembly.GetType doesn't like it,
                            // and this is what Orleans appears to use. Must report bug.
                            |> fun fullNameWithAsm ->
                                let parts = fullNameWithAsm.Split(',')
                                if parts.Length > 1 then
                                    parts.[0]
                                else
                                    fullNameWithAsm

                        let orleansTypeKey = getOrleansTypeString typ
                        let knownTypeMetadata = SerializerKnownTypeMetadata(fullyQualifiedTypeName, orleansTypeKey)
                        feature.KnownTypes.Add(knownTypeMetadata)
                    )
            })
            .AddApplicationPart(buildAssembly)
    |> ignore