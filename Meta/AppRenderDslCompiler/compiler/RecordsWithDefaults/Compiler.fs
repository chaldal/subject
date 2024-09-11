module AppRenderDslCompiler.RecordsWithDefaults.Compiler

open System.Text.RegularExpressions

open LibDsl.Compilers
open LibDsl.CodeGeneration.FsharpCode

open LibRenderDSL.Types
open LibRenderDSL.RecordsWithDefaults

open AppRenderDslCompiler.Code

type InputParams = {
    ComponentName:    string
    ComponentsAlias:  string
    NamespaceToAlias: Map<string, string>
}

type ParseResult = {
    ExtractedTaggedRecordTypes:          List<TaggedRecordType>
    FullyQualifiedComponentName:         string
    ComponentNameUnderscoreSeparated:    string
    ComponentNameDotSeparated:           string
    FullyQualifiedComponentsLibraryName: string
    LibraryAlias:                        string
    CopiedOpens:                         List<string>
    CopiedModuleAliases:                 List<string>
}

// TODO push typed errors up to LibRenderDSL
type ParsingError =
| Generic of Message: string

type CodeGenerationError =
| Generic of Message: string

type RecordsWithDefaultsCompilerError =
| ParsingError        of ParsingError
| CodeGenerationError of CodeGenerationError

let private generateRecordConstructor (componentName: string) (componentNameSuffix: string) (fullyQualifiedComponentName: string) (taggedRecordType: TaggedRecordType) : FsharpCode =
    let fieldsWithInjectedChildren =
        (WithDefault ("children", "ReactChildrenProp", "NOT USED")) :: taggedRecordType.Fields

        // In F# optional parameters need to come after all required ones
    let orderedFields =
        fieldsWithInjectedChildren
        |> List.sortBy (function
            | WithDefault ("children", _, _) -> 1
            | Regular _                      -> 0
            | WithDefault _                  -> 2
            | WithDefaultAutoWrapSome _      -> 3
        )

    let parameterList (makeName: string -> string) =
        orderedFields
        |> List.collect
            (fun field ->
                match field with
                | Regular                 (name, theType)          -> [sprintf "%s: %s"  (makeName name) theType]
                | WithDefault             (name, theType, _)       -> [sprintf "?%s: %s" (makeName name) theType]
                | WithDefaultAutoWrapSome (name, unwrappedType, _) -> [sprintf "?%s: %s" (makeName name) unwrappedType]
            )
        |> String.concat ", "

    let fieldAssignments (makeName: string -> string) =
        orderedFields
        |> List.filterMap
            (function
                | WithDefault ("children", _, _)                 -> None
                | Regular(name, _)                               -> Some (name, makeName name)
                | WithDefault(name, _, defaultValue)             -> Some (name, sprintf "defaultArg %s (%s)" (makeName name) defaultValue)
                | WithDefaultAutoWrapSome(name, _, defaultValue) -> Some (name, sprintf "%s |> Option.orElse (%s)" (makeName name) defaultValue)
            )

    Codes [
        if taggedRecordType.Name.StartsWith "Props" then
            let updatedParameterList = $"{parameterList makeFunctionParameterName}, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>"
            Codes [
                Line (sprintf "static member %s(%s) =" componentNameSuffix updatedParameterList)
                IndentedBlock [
                    Line "let __props ="
                    IndentedBlock [
                        RecordBlock (fieldAssignments makeFunctionParameterName)
                    ]

                    Line  "match xLegacyStyles with"
                    Line  "| Option.None | Option.Some [] -> ()"
                    Line  "| Option.Some styles -> __props?__style <- styles"

                    Line $"{fullyQualifiedComponentName}.Make"
                    IndentedBlock [
                        Line  "__props"
                        Line "(Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])"
                    ]
                    Line ""
                ]
            ]

        if not (taggedRecordType.Name.StartsWith "Props") then
            Codes [
                Line (sprintf "static member Make%s%s(%s) =" componentName taggedRecordType.Name (parameterList legacyMakeFunctionParameterName))

                IndentedBlock [
                    RecordBlock (fieldAssignments legacyMakeFunctionParameterName)
                ]
            ]
    ]

let private moduleDeclarationRegex = Regex("^module [^=]+$")
let private moduleAliasRegex       = Regex(@"^module\s+[a-zA-Z]+\s*=\s*.+$")


let private extractOpens (lines: List<string>): List<string> =
    lines
    |> List.filter (fun l -> l.StartsWith "open ")

let private extractModuleAliases (lines: List<string>): List<string> =
    lines
    |> List.filter moduleAliasRegex.IsMatch

let extractModuleName (lines: List<string>): Result<string, ParsingError> =
    let candidates =
        lines
        |> List.filter moduleDeclarationRegex.IsMatch

    match candidates with
    | [candidate] -> candidate.Substring(7 (* for "module " *)).Trim() |> Ok
    | [] -> ParsingError.Generic "No lines matching the file top level module declaration" |> Error
    | _  -> ParsingError.Generic "Too many lines matching the file top level module declaration" |> Error


type RecordsWithDefaultsCompiler(inputParams: InputParams) =
    inherit DslCompiler<Unit, ParseResult, ParsingError, FsharpCode, OneFile, CodeGenerationError>()

    member this.Compile (source: string) : Result<string, RecordsWithDefaultsCompilerError> = resultful {
        let! parseResult          = this.Parse source                     |> Result.mapError ParsingError
        let! codeGenerationResult = this.GenerateCode parseResult         |> Result.mapError CodeGenerationError
        let! (OneFile contents)   = this.CodeToFiles codeGenerationResult |> Result.mapError CodeGenerationError

        return contents
    }

    override __.Parse (source: string) : Result<ParseResult, ParsingError> = resultful {
        let! extractedTaggedRecordTypes = extractTaggedRecordTypes source |> Result.mapError ParsingError.Generic

        let lines = toLines source

        // We're obviously going to be overzealous here, since we have no way
        // of knowing which of the opens are actually necessary for the record
        // field types. The only problem that can come out of this is clobbering
        // of equally named types in different namespaces.
        let copiedOpens = extractOpens lines
        let copiedModuleAliases = extractModuleAliases lines

        let! fullyQualifiedComponentName = extractModuleName lines
        let! fullyQualifiedComponentsLibraryName =
            let parts = fullyQualifiedComponentName.Split(".")
            match parts |> Array.tryFindIndex (fun candidate -> candidate = "Components") with
            | Some componentsIndex -> String.concat "." parts.[0 .. componentsIndex] |> Ok
            | None                 -> ParsingError.Generic (sprintf "Component %s seems to be missing the 'Component' part in its fully qualified name" fullyQualifiedComponentName) |> Error

        let libraryAlias =
            match inputParams.NamespaceToAlias.TryFind fullyQualifiedComponentsLibraryName with
            | Some alias when alias <> "default" -> alias
            | _                                  -> inputParams.ComponentsAlias

        return {
            ExtractedTaggedRecordTypes          = extractedTaggedRecordTypes
            FullyQualifiedComponentName         = fullyQualifiedComponentName
            ComponentNameUnderscoreSeparated    = inputParams.ComponentName.Replace(".", "_")
            ComponentNameDotSeparated           = inputParams.ComponentName
            FullyQualifiedComponentsLibraryName = fullyQualifiedComponentsLibraryName
            LibraryAlias                        = libraryAlias
            CopiedOpens                         = copiedOpens
            CopiedModuleAliases                 = copiedModuleAliases
        }
    }

    override __.GenerateCode (parseResult: ParseResult) : Result<FsharpCode, CodeGenerationError> = resultful {
        let (typePrefix, componentNameSuffix) =
            match parseResult.ComponentNameDotSeparated.Split(".") with
            | [| onlySuffix |] -> ("", onlySuffix)
            | partsArray ->
                ("." + (partsArray |> Seq.take (partsArray.Length - 1) |> String.concat "."), partsArray[partsArray.Length - 1])

        let types: FsharpCode =
            match parseResult.ExtractedTaggedRecordTypes with
            | [] -> Line "end"
            | taggedRecordTypes ->
                taggedRecordTypes
                |> List.map (generateRecordConstructor parseResult.ComponentNameUnderscoreSeparated componentNameSuffix parseResult.FullyQualifiedComponentName)
                |> List.intersperse (Line "")
                |> Codes

        let componentOpen = sprintf "open %s" parseResult.FullyQualifiedComponentName

        let code = Codes (List.concat [
            [
                Line ("namespace " + parseResult.FullyQualifiedComponentsLibraryName)
                Line ""
            ]
            ("open LibClient" :: parseResult.CopiedOpens) |> List.distinct |> List.map FsharpCode.Line
            parseResult.CopiedModuleAliases |> List.map FsharpCode.Line
            [
                Line componentOpen
                Line "open Fable.Core.JsInterop"
                Line ""
                Line "// Don't warn about incorrect usage of PascalCased function parameter names"
                Line "#nowarn \"0049\""
                Line ""
                Line "[<AutoOpen>]"
                Line (sprintf "module %sTypeExtensions =" parseResult.ComponentNameUnderscoreSeparated)
                IndentedBlock [
                    Line (sprintf "type %s.Constructors.%s%s with" parseResult.FullyQualifiedComponentsLibraryName parseResult.LibraryAlias typePrefix)
                    IndentedBlock [
                        types
                    ]
                ]
            ]
        ])

        return code
    }

    override __.CodeToFiles (code: FsharpCode) : Result<OneFile, CodeGenerationError> = resultful {
        let fileContents = FsharpCode.codeToString 0 code
        return OneFile fileContents
    }
