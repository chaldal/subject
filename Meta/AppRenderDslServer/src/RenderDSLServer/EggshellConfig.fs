module RenderDSLServer.EggshellConfig

open LSP.Json.Ser
open FSharp.Data

type Exception = System.Exception

[<RequireQualifiedAccess>]
type EggshellProjectType = App | Library | Root

type EggshellRenderConfig = {
    ``additionalModulesToOpen``:  List<string>
    ``componentLibraryAliases``:  List<List<string>> // actually List<string * string> but JSON parser doesn't deal with tuples
    ``componentLibraryPaths``:    List<List<string>> // actually List<string * string> but JSON parser doesn't deal with tuples
    ``componentAliases``:         List<List<string>> // actually List<string * string> but JSON parser doesn't deal with tuples
}
with
    static member DecodeToMap (raw: List<List<string>>): Map<string, string> =
        raw
        |> List.map (fun l -> (l.Head, l.Tail.Head))
        |> Map.ofList

type EggshellConfig = {
    name:     string
    ``type``: EggshellProjectType
    render:   EggshellRenderConfig
}

let private parseProjectType(text: string): EggshellProjectType =
    match text with
    | "app"      -> EggshellProjectType.App
    | "library"  -> EggshellProjectType.Library
    | "repoRoot" -> EggshellProjectType.Root
    | _ -> raise(Exception(sprintf "Unexpected project type %s" text))

let private jsonReadOptions =
    { defaultJsonReadOptions with
        customReaders = [parseProjectType]
    }

// TODO I don't know why the return type here is EggshellConfig and not Result<...>,
// need to wrap it correctly eventually
let parseEggshellConfig: string -> EggshellConfig =
    JsonValue.Parse >> deserializerFactory<EggshellConfig> jsonReadOptions
