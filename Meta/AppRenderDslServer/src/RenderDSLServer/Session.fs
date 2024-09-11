module RenderDSLServer.Session

open LibLangFsharp
open EggshellConfig

let dprintfn = LSP.Log.dprintfn

type Path          = System.IO.Path
type DocumentStore = LSP.DocumentStore

type PascalCasedToken    = string
type LibraryAbbreviation = string
type Namespace           = string

type ComponentName =
| LibraryComponentName of LibraryNamespace: Namespace * Name: PascalCasedToken
| LocalComponentName   of Namespace: Namespace * Name: PascalCasedToken
with
    member this.Name =
        match this with
        | LibraryComponentName (_, name) -> name
        | LocalComponentName (_, name)   -> name

    member this.Namespace =
        match this with
        | LibraryComponentName (theNamespace, _) -> theNamespace
        | LocalComponentName (theNamespace, _)   -> theNamespace

    member this.ToString =
        match this with
        | LibraryComponentName (libraryNamespace, name) -> libraryNamespace + "." + name
        | LocalComponentName (theNamespace, name)       -> theNamespace + "." + name

type ComponentProps = string

let private defaultLibraryPrefix = "default"

type Session = {
    RootPath:                 string
    ComponentLibraryAliases:  Map<LibraryAbbreviation, Namespace>
    ComponentLibraryPaths:    Map<Namespace, string>
    ComponentAliases:         Map<string, string>
    CachedComponentProps:     Map<ComponentName, ComponentProps>
} with
    member this.GetComponentName (rawComponentTag: string) : Result<ComponentName, string> = resultful {
        let aliasResolvedComponentTag = this.ComponentAliases.TryFind rawComponentTag |> Option.getOrElse rawComponentTag

        let! (maybeLibraryPrefixPart, namePart) =
            match aliasResolvedComponentTag.Split "." with
            | [|_|]           -> Ok (None, aliasResolvedComponentTag)
            | [|left; right|] -> Ok (Some left, right)
            | _               -> Error (sprintf "aliasResolvedComponentTag seems malformed: %s. Raw was: %s" aliasResolvedComponentTag rawComponentTag)

        match (maybeLibraryPrefixPart, this.ComponentLibraryAliases.TryFind (maybeLibraryPrefixPart |> Option.getOrElse defaultLibraryPrefix)) with
        | (None,   Some libraryNamespace) -> return LocalComponentName (libraryNamespace, namePart)
        | (Some _, Some libraryNamespace) -> return LibraryComponentName (libraryNamespace, namePart)
        | _ -> return! Error (sprintf "Mapping for %O library not found when trying to GetComponentName for %s" maybeLibraryPrefixPart rawComponentTag)
    }

    member private this.FindBasePath (componentName: ComponentName) : Option<string> =
        match componentName with
        | LibraryComponentName (libraryNamespace, _) -> this.ComponentLibraryPaths.TryFind libraryNamespace
        | LocalComponentName _                       -> Some "src/Components"

    member this.GetFilenameForComponent (componentName: ComponentName) : Result<string, string> =
        match this.FindBasePath componentName with
        | None -> Error (sprintf "When trying to GetFilenameForComponent %s, failed to find relative path for %s lib" componentName.ToString componentName.Namespace)
        | Some basePath ->
            let candidate = Path.GetFullPath(Path.Combine(this.RootPath, basePath, componentName.Name, componentName.Name + ".typext.fs"))
            match System.IO.File.Exists candidate with
            | true  -> Ok candidate
            | false -> Error (sprintf "When trying to GetFilenameForComponent %s, got %s but it does not exist in the file system" componentName.ToString candidate)

let emptySession: Session = {
    RootPath                = "" // HACK this is horrible
    ComponentLibraryAliases = Map.empty
    ComponentLibraryPaths   = Map.empty
    ComponentAliases        = Map.empty
    CachedComponentProps    = Map.empty
}

let initializeSession (rootPath: string) (documentStore: DocumentStore) : Async<Session> = async {
    let configPath = Path.Combine(rootPath, "eggshell.json")

    let! maybeConfigContent = Files.getTextFromStoreOrFilesystemIfNotOpen documentStore configPath

    match maybeConfigContent with
    | None ->
        dprintfn "No eggshell config found at %s" configPath
        return emptySession
    | Some configContent ->
        dprintfn "Found config found at %s" configPath
        let config = parseEggshellConfig configContent
        return {
            RootPath                = rootPath
            ComponentLibraryAliases = EggshellRenderConfig.DecodeToMap config.render.componentLibraryAliases
            ComponentLibraryPaths   = EggshellRenderConfig.DecodeToMap config.render.componentLibraryPaths
            ComponentAliases        = EggshellRenderConfig.DecodeToMap config.render.componentAliases
            CachedComponentProps    = Map.empty
        }
    }
