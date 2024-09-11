module CodecGenerator

open System.Reflection
open System.IO

open MBrace.FsPickler
open CodecLib


[<EntryPoint>]
let main argv =
    let outPath = argv[0]

    let assembly = Assembly.Load("__TYPES_ASSEMBLY_NAME__")

    let timer = System.Diagnostics.Stopwatch()
    timer.Start()

    // Create binary serializer instance")
    let binarySerializer = FsPickler.CreateBinarySerializer()

    // call init methods and get types of interface codecs
    let codecInterfaces =
        assembly.GetTypes()
        |> Array.collect (fun t ->
            let typeLabelMethod = t.GetMethod("TypeLabel", BindingFlags.Public ||| BindingFlags.Static)
            let initMethod = t.GetMethod("Init", BindingFlags.Public ||| BindingFlags.Static)
            if typeLabelMethod <> null && initMethod <> null && not t.IsGenericType then
                printfn $"Initializing {t.FullName}"
                let typeLabel = typeLabelMethod.Invoke(null, [||])
                let param =
                    Array.create (initMethod.GetGenericArguments().Length) typeof<Encoding>
                let _init: unit =
                            initMethod
                                .MakeGenericMethod(param)
                                .Invoke(null, [| typeLabel; () |])
                            :?> unit

                let codecInterfaces =
                    t.GetInterfaces()
                    |> Array.choose (fun i ->
                        if i.IsGenericType && i.GetGenericTypeDefinition() = typedefof<IInterfaceCodec<_>> then
                            Some i
                        else
                            None
                    )
                codecInterfaces
            else
                [||]
        )
        |> Array.distinctBy (fun t -> t.FullName)

    timer.Stop ()
    printfn $"Init methods called in %.3f{timer.Elapsed.TotalSeconds} seconds"
    timer.Restart ()

    let getInterfaceFullTypeName (interfaceType: System.Type) : string =
        let getGenericArgs (t: System.Type) =
            t.GetGenericArguments() |> Array.map (fun arg -> arg.FullName.Replace("+",".")) |> String.concat ","

        if interfaceType.IsGenericType then
            $"interface_{interfaceType.Namespace}.{interfaceType.Name}[[{getGenericArgs interfaceType}]]"
        else
            $"interface_{interfaceType.Namespace}.{interfaceType.Name}"

    let interfaceJsonNodes : array<string * JsonNode> =
        codecInterfaces
        |> Array.map (fun t ->
            let interfaceType = t.GetGenericArguments()[0]
            let codecCollectionTypeDef = typedefof<CodecCollection<_, _>>
            let codecCollection = codecCollectionTypeDef.MakeGenericType([| typeof<AdHocEncoding>; interfaceType |])

            let getSubtypesProperty = codecCollection.GetProperty("GetSubtypes")
            let subtypesDict = getSubtypesProperty.GetValue(null)

            let decoderMethod = typeof<CodecLib.HelperMethods>.GetMethod("getJsonNodeFromCodecCollectionSubtypes")
            let genericDecoderMethod = decoderMethod.MakeGenericMethod([| typeof<Encoding>; interfaceType |])
            let jsonNode : JsonNode = genericDecoderMethod.Invoke(null, [|subtypesDict|]) :?> JsonNode
            getInterfaceFullTypeName interfaceType, jsonNode
        )

    timer.Stop ()
    printfn $"JsonNodes for interfaces generated in %.3f{timer.Elapsed.TotalSeconds} seconds"
    timer.Restart ()

    let srtpJsonNodes: array<string * JsonNode> =
        assembly.GetTypes()
        |> Array.filter (fun t ->
            not t.IsGenericType &&
            t.GetMethod("get_Codec", BindingFlags.Public ||| BindingFlags.Static) <> null)
        |> Array.map (fun t ->
            let fullName = t.FullName.Replace("+", ".")

            // Get the Codec property
            let codecMethod = t.GetMethod("get_Codec", BindingFlags.Public ||| BindingFlags.Static)
            let genericCodecMethod = codecMethod.MakeGenericMethod(typeof<Encoding>)
            let codec = genericCodecMethod.Invoke(null, [||])

            // Create specific version of getDecodersAsJsonNode for this type
            let decoderMethod = typeof<CodecLib.HelperMethods>.GetMethod("getDecodersAsJsonNode")
            let codecForType = genericCodecMethod.ReturnType.GetGenericArguments()[3] // some type has Codec of anonymous type
            let genericDecoderMethod = decoderMethod.MakeGenericMethod([| typeof<Encoding>; codecForType |])
            let jsonNode : JsonNode = genericDecoderMethod.Invoke(null, [|codec|]) :?> JsonNode

            (fullName, jsonNode))

    timer.Stop ()
    printfn $"JsonNodes for other types generated in %0.3f{timer.Elapsed.TotalSeconds} seconds"
    timer.Restart ()

    let data =
        Array.append interfaceJsonNodes srtpJsonNodes
        |> Map.ofArray

    printfn "Started serializing Json Schema"

    // Serialize directly to file using binary format
    use stream = File.Create(outPath)
    binarySerializer.Serialize(stream, data)
    timer.Stop()
    printfn $"Serializing Json Schema took %f{timer.Elapsed.TotalSeconds} seconds"

    // Verify round-trip serialization
    use verifyStream = File.OpenRead(outPath)
    let roundTripped = binarySerializer.Deserialize<Map<string, JsonNode>>(verifyStream)
    if data <> roundTripped then
        failwith "Round trip serialization failed"

    0

