module LibCodecValidation.Program

open System
open System.IO
open MBrace.FsPickler

open CodecLib
open EvolutionCheckerLib

let binarySerializer = FsPickler.CreateBinarySerializer()

let loadSchema (filePath: string) : Map<string, JsonNode> =
    printfn "Loading codec schema from %s" filePath
    let timer = Diagnostics.Stopwatch.StartNew()
    use verifyStream = File.OpenRead(filePath)
    let schema = binarySerializer.Deserialize<Map<string, JsonNode>>(verifyStream)
    timer.Stop()
    printfn "Loaded codec schema in %.3f seconds" (timer.Elapsed.TotalSeconds)
    schema

let printColored (color: ConsoleColor) (message: string) =
    let originalColor = Console.ForegroundColor
    Console.ForegroundColor <- color
    printfn "%s" message
    Console.ForegroundColor <- originalColor

let printSuccess = printColored ConsoleColor.Green
let printSuccessUnChanged = printColored ConsoleColor.Gray
let printWarning = printColored ConsoleColor.Yellow
let printError = printColored ConsoleColor.Red

let evaluateCodecs (oldSchema: Map<string, JsonNode>) (newSchema: Map<string, JsonNode>) : (* exitCode *) int =
    // Get all unique types
    let allTypes =
        Set.union
            (oldSchema |> Map.keys |> Set.ofSeq)
            (newSchema |> Map.keys |> Set.ofSeq)

    // Evaluate each type
    allTypes
    |> Set.toSeq
    |> Seq.sort
    |> Seq.map (fun typeName ->
        match oldSchema.TryFind typeName, newSchema.TryFind typeName with
        | Some oldSchema, Some newSchema ->
            // Type exists in both codecs
            match checkEvolutionCorrectness 0u oldSchema newSchema with
            | Ok EvolutionOk.ExactMatch  ->
                printSuccessUnChanged $"✓ {typeName}: Type unchanged"
                0
            | Ok EvolutionOk.Evolved ->
                printSuccess $"✓ {typeName}: Evolution is correct"
                0
            | Error e ->
                printError $"✗ {typeName}: Evolution is incorrect\n Error: {e}"
                1

        | Some _, None ->
            // Type removed in new codec
            printWarning $"! {typeName}: Type removed in new codec"
            0

        | None, Some _ ->
            // New type added
            printWarning $"+ {typeName}: New type added"
            0

        | None, None ->
            // Should never happen due to set union
            printError $"? {typeName}: Internal error - type not found in either codec"
            1
    )
    |> Seq.max

[<EntryPoint>]
let main argv =
    let oldSchemaPath = argv[0]
    let newSchemaPath = argv[1]

    let oldSchema = loadSchema oldSchemaPath
    let newSchema = loadSchema newSchemaPath

    let timer = Diagnostics.Stopwatch.StartNew()
    printfn "Evaluating codec evolution..."
    let exitCode = evaluateCodecs oldSchema newSchema
    timer.Stop()
    printfn "Evaluation complete in %.3f seconds" (timer.Elapsed.TotalSeconds)

    printfn "\nSummary:"
    printfn "- Types in old codec: %d" oldSchema.Count
    printfn "- Types in new codec: %d" newSchema.Count
    printfn "- Total unique types: %d" (Set.union (oldSchema |> Map.keys |> Set.ofSeq) (newSchema |> Map.keys |> Set.ofSeq) |> Set.count)

    exitCode
