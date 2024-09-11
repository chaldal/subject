module AppRenderDslCompiler.Render.Compiler

open LibDsl.Compilers
open LibDsl.CodeGeneration.FsharpCode

open AppRenderDslCompiler.Render.Types
open AppRenderDslCompiler.Render.Parsing
open AppRenderDslCompiler.Render

type InputParams = {
    ComponentName:           string
    LibAlias:                string
    AdditionalOpens:         seq<string>
    ComponentLibraryAliases: Map<string, string>
    ComponentAliases:        Map<string, string>
    WithStyles:              bool
}

let generateOpens (modules: seq<string>): string =
    modules
    |> Seq.map (sprintf "open %s")
    |> String.concat "\n"

let generateModuleAliases (shortcutToFull: Map<string, string>): string =
    shortcutToFull
    |> Map.toSeq
    |> Seq.map (fun (alias, fullName) -> (sprintf "module %s = %s") alias fullName)
    |> String.concat "\n"

let rec getUsedComponents (node: ReactTemplateNode): Set<string> =
    match node with
    // XXX this is kind of crappy setup — RtCase doesn't actually exist for the purposes
    // of code generation, since RtMatch stores the cases internally, but not as RtCase instances.
    // Either way providing the match for completeness, will clean up later.
    | RtMatch (_, cases, _) ->
        cases
        |> Seq.collect (fun (_, children, _) -> children)
        |> getUsedComponentsPlural

    | DomNode (_, maybeChildren, _, _) ->
        match maybeChildren with
        | None -> Set.empty
        | Some children -> getUsedComponentsPlural children

    | Component (_libraryAlias, nameSpace, name, _, children, _) ->
        Set.add (nameSpace + "." + name) (getUsedComponentsPlural children)

    | RtRoot { Children = children }
    | RtBlock (children, _)
    | RtCase (_, children, _)
    | RtProp (_, children)
    | RtOuterLet (_, children)
    | RtLet (_, children) ->
        getUsedComponentsPlural children

    | Text _
    | Cdata _
    | Comment _
    | RtSharp _ ->
        Set.empty

and getUsedComponentsPlural (nodes: seq<ReactTemplateNode>): Set<string> =
    nodes |> Seq.collect getUsedComponents |> Set.ofSeq


type RenderDslCompiler(inputParams: InputParams) =
    inherit DslCompiler<Unit, ReactTemplateRootNode, ParsingError, FsharpCode * ReactTemplateRootNode, OneFile, CodeGenerationError>()

    member this.Compile (source: string): Result<string, RenderCompilerError> = resultful {
        let! parseResult          = this.Parse source                     |> Result.mapError ParsingError
        let! codeGenerationResult = this.GenerateCode parseResult         |> Result.mapError CodeGenerationError
        let! (OneFile contents)   = this.CodeToFiles codeGenerationResult |> Result.mapError CodeGenerationError

        return contents
    }

    override __.Parse (source: string) : Result<ReactTemplateRootNode, ParsingError> =
        parse inputParams.LibAlias inputParams.ComponentLibraryAliases inputParams.ComponentAliases source

    override __.GenerateCode (rootNode: ReactTemplateRootNode) : Result<FsharpCode * ReactTemplateRootNode, CodeGenerationError> = resultful {
        let! rawBodyCode = CodeGeneration.generateCode inputParams.LibAlias inputParams.WithStyles (RtRoot rootNode)
        let bodyCodeWithTypeConversion = Codes [
            Line "let __parentFQN = None"
            rawBodyCode
        ]

        let bodyCodeAdjusted =
            match inputParams.WithStyles with
            | false -> bodyCodeWithTypeConversion
            | true -> Codes [
                (Line "let __class = (ReactXP.Helpers.extractProp \"ClassName\" props) |> Option.defaultValue \"\"")
                (Line "let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props")
                bodyCodeWithTypeConversion
            ]

        return (bodyCodeAdjusted, rootNode)
    }

    override __.CodeToFiles (codeAndRootNode: FsharpCode * ReactTemplateRootNode) : Result<OneFile, CodeGenerationError> = resultful {
        let (code, rootNode) = codeAndRootNode

        let componentName = inputParams.ComponentName

        let bodyCodeString = FsharpCode.codeToString 1 code

        let! nameSpace =
            inputParams.ComponentLibraryAliases.TryFind "default"
            |> Option.getAsResult MissingDefaultLibraryMapping

        let moduleDeclaration = sprintf "module %s.%sRender" nameSpace componentName
        let openFable = generateModuleAliases (Map.ofList [
            ("FRS", "Fable.React.Standard")
            ("FRH", "Fable.React.Helpers")
            // this one is used to prefix props of DOM elements
            ("FRP", "Fable.React.Props")
        ])

        let componentLibraries = inputParams.ComponentLibraryAliases |> Map.values |> List.ofSeq
        let openComponentLibraries = generateOpens componentLibraries

        let openAdditionalFromCommandLineArgs = generateOpens inputParams.AdditionalOpens

        let openFromRootRtOpen = generateOpens rootNode.Opens
        let moduleAliasesFromRootRtOpen = generateModuleAliases rootNode.ModuleAliases

        // We didn't originally include this in the interest of not polluting the open scope and
        // to thus avoid potential clobber problems, but for ease of use of types declared in the
        // component's .fs file, we do want this.
        let openThisComponent = generateOpens [sprintf "%s.%s" nameSpace componentName]

        let propsTypeParametersString =
            rootNode.MaybeTypeParameters
            |> Option.map (fun value -> "<" + value + ">")
            |> Option.getOrElse ""
        let actionsTypeParametersString = propsTypeParametersString
        let estateTypeParametersString = propsTypeParametersString

        let typesNameSpace = sprintf "%s.%s" nameSpace componentName

        let renderDeclaration =
            match inputParams.WithStyles with
            | false -> sprintf "let render(children: array<ReactElement>, props: %s.Props%s, estate: %s.Estate%s, pstate: %s.Pstate, actions: %s.Actions%s) : Fable.React.ReactElement = element {" typesNameSpace propsTypeParametersString typesNameSpace estateTypeParametersString typesNameSpace typesNameSpace actionsTypeParametersString
            | true  -> sprintf "let render(children: array<ReactElement>, props: %s.Props%s, estate: %s.Estate%s, pstate: %s.Pstate, actions: %s.Actions%s, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =" typesNameSpace propsTypeParametersString typesNameSpace estateTypeParametersString typesNameSpace typesNameSpace actionsTypeParametersString

        let renderHead = sprintf "%s\n    // sadly #nowarn has file scope, so we have to emulate it manually\n    (children, props, estate, pstate, actions) |> ignore" renderDeclaration

        let source = sprintf "%s\n\n%s\n\n\n%s\n\n%s\n\n%s\n%s\n%s\n\n%s\n%s\n" moduleDeclaration openFable openComponentLibraries openAdditionalFromCommandLineArgs openThisComponent openFromRootRtOpen moduleAliasesFromRootRtOpen renderHead bodyCodeString

        return OneFile source
    }


type RenderDslConverter(inputParams: InputParams) =
    inherit DslCompiler<Unit, ReactTemplateRootNode, ParsingError, FsharpCode * ReactTemplateRootNode, OneFile, CodeGenerationError>()

    member this.Compile (source: string): Result<string, RenderCompilerError> = resultful {
        let! parseResult          = this.Parse source                     |> Result.mapError ParsingError
        let! codeGenerationResult = this.GenerateCode parseResult         |> Result.mapError CodeGenerationError
        let! (OneFile contents)   = this.CodeToFiles codeGenerationResult |> Result.mapError CodeGenerationError

        return contents
    }

    override __.Parse (source: string) : Result<ReactTemplateRootNode, ParsingError> =
        parse inputParams.LibAlias inputParams.ComponentLibraryAliases inputParams.ComponentAliases source

    override __.GenerateCode (rootNode: ReactTemplateRootNode) : Result<FsharpCode * ReactTemplateRootNode, CodeGenerationError> = resultful {
        let! bodyCode = CodeGenerationConvert.generateCode inputParams.LibAlias inputParams.WithStyles (RtRoot rootNode)
        return (bodyCode, rootNode)
    }

    override __.CodeToFiles (codeAndRootNode: FsharpCode * ReactTemplateRootNode) : Result<OneFile, CodeGenerationError> = resultful {
        let (code, rootNode) = codeAndRootNode

        let componentName = inputParams.ComponentName

        let bodyCodeString = FsharpCode.codeToString 1 code

        let! nameSpace =
            inputParams.ComponentLibraryAliases.TryFind "default"
            |> Option.getAsResult MissingDefaultLibraryMapping

        let moduleDeclaration = sprintf "module %s.%sRender" nameSpace componentName
        let openFable = generateModuleAliases (Map.ofList [
            ("FRS", "Fable.React.Standard")
            ("FRH", "Fable.React.Helpers")
            // this one is used to prefix props of DOM elements
            ("FRP", "Fable.React.Props")
        ])

        let componentLibraries = inputParams.ComponentLibraryAliases |> Map.values |> List.ofSeq
        let openComponentLibraries = generateOpens componentLibraries

        let openAdditionalFromCommandLineArgs = generateOpens inputParams.AdditionalOpens

        let openFromRootRtOpen = generateOpens rootNode.Opens
        let moduleAliasesFromRootRtOpen = generateModuleAliases rootNode.ModuleAliases

        // We didn't originally include this in the interest of not polluting the open scope and
        // to thus avoid potential clobber problems, but for ease of use of types declared in the
        // component's .fs file, we do want this.
        let openThisComponent = generateOpens [sprintf "%s.%s" nameSpace componentName]

        let propsTypeParametersString =
            rootNode.MaybeTypeParameters
            |> Option.map (fun value -> "<" + value + ">")
            |> Option.getOrElse ""
        let actionsTypeParametersString = propsTypeParametersString
        let estateTypeParametersString = propsTypeParametersString

        let typesNameSpace = sprintf "%s.%s" nameSpace componentName

        let renderDeclaration =
            match inputParams.WithStyles with
            | false -> sprintf "let render(props: %s.Props%s, estate: %s.Estate%s, pstate: %s.Pstate, actions: %s.Actions%s) : Fable.React.ReactElement =" typesNameSpace propsTypeParametersString typesNameSpace estateTypeParametersString typesNameSpace typesNameSpace actionsTypeParametersString
            | true  -> sprintf "let render(props: %s.Props%s, estate: %s.Estate%s, pstate: %s.Pstate, actions: %s.Actions%s, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =" typesNameSpace propsTypeParametersString typesNameSpace estateTypeParametersString typesNameSpace typesNameSpace actionsTypeParametersString

        let source = sprintf "%s\n\n%s\n\n%s\n\n%s\n\n%s\n%s\n%s\n\n%s\n%s" moduleDeclaration openFable openComponentLibraries openAdditionalFromCommandLineArgs openThisComponent openFromRootRtOpen moduleAliasesFromRootRtOpen renderDeclaration bodyCodeString

        return OneFile source
    }
