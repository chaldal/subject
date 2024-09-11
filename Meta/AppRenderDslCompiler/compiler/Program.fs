open System

open AppRenderDslCompiler.Render.Main
open AppRenderDslCompiler.RecordsWithDefaults.Main

// The following line gets inspected by a script to verify that local binary
// has the same version as the source.
let (* SPECIAL *) private version = 67

let private usageMessage = """
Usage:
    render-dsl-compiler Render <ComponentName> [<AdditionalOpensSemicolonSeparated> <componentLibraryAliasesEqualsAndSemicolonSeparated> <ComponentAliasesEqualsAndSemicolonSeparated>]
    render-dsl-compiler RenderConvert <ComponentName> [<AdditionalOpensSemicolonSeparated> <componentLibraryAliasesEqualsAndSemicolonSeparated> <ComponentAliasesEqualsAndSemicolonSeparated>]
    render-dsl-compiler RecordsWithDefaults <ComponentName> <libAlias> <componentLibraryAliasesEqualsAndSemicolonSeparated>
"""

let private parseStringPairs (serailized: string) (typeForErrorReporting: string) : Result<Map<string, string>, (string * int)> =
    resultful {
        let! pairs =
            serailized.Split(";")
            |> Array.map
                (fun s ->
                    match s.Split("=") with
                    | [|alias; moduleName|] -> Ok (alias, moduleName)
                    | _ -> Error (sprintf "Something wrong with he %s string: %s" typeForErrorReporting serailized, -1)
                )
            |> List.ofArray
            |> Result.liftFirst

        return pairs |> Map.ofList
    }

let mainReactTemplate (argv: List<string>): Result<string, (string * int)> =
    resultful {
        let! (componentName, libAlias, additionalOpens, componentLibraryAliases, componentAliases) =
            match argv with
            | [componentName] -> Ok (componentName, "LibAlias", Seq.empty, DefaultComponentLibraryAliases, Map.empty)
            | [componentName; libAlias; additionalOpensSerialized; componentLibraryAliasesSerialized; componentAliasesSerialized] ->
                resultful {
                    let additionalOpens =
                        match additionalOpensSerialized.Substring(1, additionalOpensSerialized.Length - 2) with
                        | "" -> Seq.empty
                        | nonempty -> nonempty.Split(";") |> Seq.ofArray
                    let! componentLibraryAliases = parseStringPairs componentLibraryAliasesSerialized "component library mappings"
                    let! componentAliases        = parseStringPairs componentAliasesSerialized        "component aliases"
                    return (componentName, libAlias, additionalOpens, componentLibraryAliases, componentAliases)
                }
            | _ -> Error (usageMessage, -1)

        let source = Console.In.ReadToEnd()

        let withStyles = true

        let compilationResult = generateRenderFunctionFile componentName libAlias additionalOpens componentLibraryAliases componentAliases withStyles source

        return!
            compilationResult
            |> Result.mapError (fun e -> (e.ToString(), -1))
    }

let mainReactTemplateConvert (argv: List<string>): Result<string, (string * int)> =
    resultful {
        let! (componentName, libAlias, additionalOpens, componentLibraryAliases, componentAliases) =
            match argv with
            | [componentName] -> Ok (componentName, "LibAlias", Seq.empty, DefaultComponentLibraryAliases, Map.empty)
            | [componentName; libAlias; additionalOpensSerialized; componentLibraryAliasesSerialized; componentAliasesSerialized] ->
                resultful {
                    let additionalOpens =
                        match additionalOpensSerialized.Substring(1, additionalOpensSerialized.Length - 2) with
                        | "" -> Seq.empty
                        | nonempty -> nonempty.Split(";") |> Seq.ofArray
                    let! componentLibraryAliases = parseStringPairs componentLibraryAliasesSerialized "component library mappings"
                    let! componentAliases        = parseStringPairs componentAliasesSerialized        "component aliases"
                    return (componentName, libAlias, additionalOpens, componentLibraryAliases, componentAliases)
                }
            | _ -> Error (usageMessage, -1)

        let source = Console.In.ReadToEnd()

        let withStyles = true

        let compilationResult = generateRenderFunction componentName libAlias additionalOpens componentLibraryAliases componentAliases withStyles source

        return!
            compilationResult
            |> Result.mapError (fun e -> (e.ToString(), -1))
    }

let mainRecordsWithDefaults (argv: List<string>): Result<string, (string * int)> =
    resultful {
        let! (componentName, componentsAlias, componentLibraryAliasesSerialized) =
            match argv with
            | [componentName; componentsAlias; componentLibraryAliasesSerialized] -> Ok (componentName, componentsAlias, componentLibraryAliasesSerialized)
            | _ -> Error (usageMessage, -1)

        let! componentLibraryAliases = parseStringPairs componentLibraryAliasesSerialized "component library mappings"
        let namespaceToAlias =
            componentLibraryAliases
            |> Map.toSeq
            |> Seq.map (fun (a, b) -> (b, a))
            |> Map.ofSeq

        let source = Console.In.ReadToEnd()
        return! (generateRecordConstructors componentName componentsAlias namespaceToAlias source) |> Result.mapError (fun e -> (e.ToString(), -1))
    }

[<EntryPoint>]
let main argv =
    let result =
        match List.ofArray argv with
        | "Render"              :: tail -> mainReactTemplate tail
        | "RenderConvert"       :: tail -> mainReactTemplateConvert tail
        | "RecordsWithDefaults" :: tail -> mainRecordsWithDefaults tail
        | ["--version"]
        | ["-v"] ->
            Ok (sprintf "%d\n" version)
        | _ -> Error (usageMessage, -1)

    match result with
    | Ok output ->
        Console.Out.Write(output)
        0
    | Error (message, errorCode) ->
        Console.Error.WriteLine(message)
        errorCode
