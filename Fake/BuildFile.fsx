#if !FAKE_BUILD_FSPROJ
#load "./PackageReferences.fsx"
#load "./Operators.fsx"
#endif

namespace Egg.Shell.Fake

[<RequireQualifiedAccess>]
module BuildFile =

    open Fake.Core
    open Fake.IO
    open FSharp.Reflection
    open Fake.Core.TargetOperators

    type CurrentDirPathNormalizer = string -> string

    [<NoComparison; NoEquality>]
    type BuildContext = {
        TargetParameter: TargetParameter
        NormalizePath:   CurrentDirPathNormalizer
    }

    type TargetValue<'T> =
    | ValuePresent of 'T
    | Usage of string

    [<NoComparison; NoEquality>]
    type TargetValueFactory<'T> = {
        GetAll:        unit -> Map<string, TargetValue<'T>>
        GetTargetName: 'T -> string
    }

    type TargetBody<'T> = BuildContext -> CurrentDirPathNormalizer -> 'T -> unit

    // To enhance readability in usage sites
    let onlyProcessDependencies = ()

    [<Literal>]
    let Current = "."

    type BuildFile = private {
        Directory: string
        File:      string
    }
    with
        member this.Path =
            Path.combine this.Directory this.File
            |> System.IO.Path.GetFullPath

    type MaterializationContext = private {
        EntryBuildFile:         BuildFile
        MaterializedBuildFiles: Set<BuildFile>
    }

    [<NoComparison; NoEquality>]
    type BuildFileAndValues<'T> = private {
        BuildFile: BuildFile
        Values:    TargetValueFactory<'T>
    }

    type TypeAssemblyQualifiedNameToKnownValues = Map<string, list<obj>>

    [<NoComparison; NoEquality>]
    type TargetFactory<'T> = private {
        BuildFile:              BuildFile
        DefaultTarget:          Option<'T>
        Values:                 TargetValueFactory<'T>
        Body:                   TargetBody<'T>
        Dependencies:           list<DependencyInitializer<'T>>
        KnownTypeNamesToValues: TypeAssemblyQualifiedNameToKnownValues
        IsInitialized:          bool
    }


    and DependencyInitializer<'T> = MaterializationContext -> TargetFactory<'T> -> MaterializationContext

    let forPath (dir: string) (file: string) =
        { Directory = dir; File = file }

    let private targetValuesForUnionType<'T> =
        let tagReader = FSharpValue.PreComputeUnionTagReader typeof<'T>

        let optionsDocForUnionCase (uc: UnionCaseInfo) =
            let fields = uc.GetFields()

            if fields.Length = 0 then
                ""
            else
                fields
                |> Seq.fold (
                    fun doc fieldInfo ->
                        sprintf "%s--%s=<%s>\n" doc (fieldInfo.Name.ToLowerInvariant()) fieldInfo.Name
                    ) ""

        let optionsDocForUnionCases ucs =
            ucs
            |> Seq.fold (
                fun doc uc ->
                    sprintf "%s%s" doc (optionsDocForUnionCase uc)
                ) ""

        let doc =
            FSharpType.GetUnionCases(typeof<'T>)
            |> optionsDocForUnionCases
            |> (fun optionsDoc -> sprintf "Usage: <FAKE_COMMAND> [options]\n\nOptions:\n%s" optionsDoc)

        // lazy because Fake context might be not set yet when this is evaluated
        let tagToNameAndValue =
            FSharpType.GetUnionCases(typeof<'T>)
            |> Seq.map (fun uc ->
                let fields = uc.GetFields()
                if fields.Length = 0 then
                    (uc.Tag, (uc.Name, (FSharpValue.MakeUnion(uc, Array.empty) :?> 'T |> ValuePresent)))
                else
                    match Target.getArguments() with
                    | None ->
                        (uc.Tag, (uc.Name, TargetValue.Usage doc))

                    | Some commandLineArgs ->
                        let parser = Docopt(doc)
                        let parsedArgs = parser.Parse(commandLineArgs |> Array.filter (fun arg -> arg <> "--list"))

                        let args =
                            fields
                            |> Seq.choose (fun propertyInfo ->
                                match parsedArgs.TryFind (sprintf "--%s" (propertyInfo.Name.ToLowerInvariant())) with
                                | Some value ->
                                    match value with
                                    | Argument arg ->
                                        Some arg
                                    | _ -> None
                                    |> Option.map box
                                | None ->
                                    None
                            )
                            |> Seq.toArray

                        if args.Length = fields.Length then
                            (uc.Tag, (uc.Name, (FSharpValue.MakeUnion(uc, args) :?> 'T |> ValuePresent)))
                        else
                            (uc.Tag, (uc.Name, TargetValue.Usage doc))
            )
            |> Map.ofSeq

        {
            GetAll =
                fun _ ->
                    tagToNameAndValue
                    |> Map.toSeq
                    |> Seq.map snd
                    |> Map.ofSeq

            GetTargetName =
                fun target ->
                    let tag = tagReader target
                    tagToNameAndValue.[tag] |> fst
        }

    let private withTargetValues (values: TargetValueFactory<'T>) (buildFile: BuildFile) =
        { BuildFile = buildFile; Values = values }

    let withTargetsFromUnionType<'T> (buildFile: BuildFile) =
        withTargetValues targetValuesForUnionType<'T> buildFile

    let withDefinition (body: TargetBody<'T>) (buildFileAndValues: BuildFileAndValues<'T>) = {
        BuildFile =              buildFileAndValues.BuildFile
        Values =                 buildFileAndValues.Values
        DefaultTarget =          None
        Body =                   body
        Dependencies =           []
        KnownTypeNamesToValues = Map.empty
        IsInitialized =          false
    }

    let withDefaultTarget (target: 'T) (targetFactory: TargetFactory<'T>) : TargetFactory<'T> =
        { targetFactory with DefaultTarget = Some target }

    let private combineTargetNames (prefix: string) (targetName: string) =
        if prefix = "" then
            targetName
        elif targetName = "" then
            prefix
        else
            sprintf "%s/%s" (String.trimEndChars [| '/' |] prefix) targetName

    let currentDirPathNormalizer (buildFile: BuildFile) (relativePath: string) =
        Path.combine buildFile.Directory relativePath
        |> System.IO.Path.GetFullPath

    let changeDir (buildContext: BuildContext) (newRelativePath: string) : BuildContext =
        let newDirPath = buildContext.NormalizePath newRelativePath
        let newDirPathNormalizer =
            fun relativePath ->
                Path.combine newDirPath relativePath
                |> System.IO.Path.GetFullPath
        { buildContext with NormalizePath = newDirPathNormalizer }

    let rec listStartsWith (stem: list<'T>) (lst: list<'T>) : bool =
        match (stem, lst) with
        | ([], _) ->
            true
        | (_, []) ->
            false
        | ((i::iTail), (j::jTail)) ->
            if i = j then
                listStartsWith iTail jTail
            else
                false

    let private getRelativePath (anchor: string) (path: string) : string =
        let anchorParts =
            System.IO.Path.GetFullPath anchor
            |> String.splitStr Path.directorySeparator
            |> fun parts ->
                if Path.isFile anchor then
                    List.truncate (parts.Length - 1) parts
                else
                    parts

        let (pathParts, fileName) =
            System.IO.Path.GetFullPath path
            |> String.splitStr Path.directorySeparator
            |> fun parts ->
                if Path.isFile path then
                    ((List.truncate (parts.Length - 1) parts), (Some (List.last parts)))
                else
                    (parts, None)

        if anchorParts = pathParts then
            sprintf "%s" (Option.defaultValue "" fileName)
        elif listStartsWith anchorParts pathParts then
            Seq.skip anchorParts.Length pathParts
            |> String.concat Path.directorySeparator
            |> fun s -> sprintf "%s%s%s" s Path.directorySeparator (Option.defaultValue "" fileName)
        else
            let rec removeStemAndGetLeftOvers anchorParts pathParts =
                match (anchorParts, pathParts) with
                | ([], lst) ->
                    (0, lst)
                | (lst, []) ->
                    (lst.Length, [])
                | ((i::iTail), (j::jTail)) ->
                    if i = j then
                        removeStemAndGetLeftOvers iTail jTail
                    else
                        ((anchorParts.Length), pathParts)

            let (numUpWards, forwardList) =
                removeStemAndGetLeftOvers anchorParts pathParts

            forwardList
            |> Seq.append (Seq.init numUpWards (fun _ -> ".."))
            |> String.concat Path.directorySeparator
            |> fun s -> sprintf "%s%s%s" s Path.directorySeparator (Option.defaultValue "" fileName)

    let getRelativePrefix (anchor: string) (path: string) : string =
        getRelativePath anchor path
        |> Seq.fold ( // Normalize, PascalCase etc ..
            fun (acc, shouldCapitalize, shouldSkipSymbol) ch ->
                if System.Char.IsLetter ch then
                    let ch = if shouldCapitalize then (string ch).ToUpperInvariant() else (string ch)
                    ((acc + ch), false, false)
                elif System.Char.IsDigit ch then
                    ((acc + (ch.ToString().ToUpper())), shouldCapitalize, false)
                elif ch = '\\' || ch = '/' then
                    (acc + "/", true, false)
                elif ch = '.' then
                    (acc + ".", true, false)
                elif shouldSkipSymbol then
                    (acc, shouldCapitalize, true)
                else
                    (acc + "_", true, true)

        ) ("", true, false)
        |> fun (result, _, _) -> result

    let private getPrefixedTargetName (entryBuildFile: BuildFile) (targetBuildFile: BuildFile) (targetName: string) : string =
        if entryBuildFile = targetBuildFile then
            targetName
        else
            let prefix = getRelativePrefix entryBuildFile.Directory targetBuildFile.Directory
            combineTargetNames prefix targetName

    let private getTargetName (entryBuildFile: BuildFile) (targetValue: 'T) (targetFactory: TargetFactory<'T>) : string =
        let targetName = targetFactory.Values.GetTargetName targetValue
        let targetBuildFile = targetFactory.BuildFile
        getPrefixedTargetName entryBuildFile targetBuildFile targetName

    let private materialize (dependentTargetValues: Set<'T>) (materializationContext: MaterializationContext) (targetFactory: TargetFactory<'T>) : MaterializationContext =
        let dependentTargetNamesToValues =
            dependentTargetValues
            |> Seq.map (fun targetValue -> ((targetFactory.Values.GetTargetName targetValue), targetValue))
            |> Map.ofSeq

        targetFactory.Values.GetAll()
        |> Map.iter (fun targetName targetValue ->
            let prefixedTargetName = getPrefixedTargetName materializationContext.EntryBuildFile targetFactory.BuildFile targetName
            Target.create prefixedTargetName (
                fun targetParameter ->
                    let normalizePath =
                        currentDirPathNormalizer targetFactory.BuildFile

                    let buildCtx = {
                        TargetParameter = targetParameter
                        NormalizePath = normalizePath
                    }

                    match targetValue with
                    | ValuePresent value ->
                        targetFactory.Body buildCtx normalizePath value

                    | Usage cliUsage ->
                        match dependentTargetNamesToValues.TryFind targetName with
                        | Some parameteriedTargetValue ->
                            targetFactory.Body buildCtx normalizePath parameteriedTargetValue
                        | None ->
                            printfn "%s" cliUsage
                    )
        )

        targetFactory.Dependencies
        |> Seq.fold (
            fun materializationContext dependencyInitializer  ->
                dependencyInitializer materializationContext targetFactory
        ) materializationContext

    let private addKnownValue (value: 'T) (knownValues: TypeAssemblyQualifiedNameToKnownValues) : TypeAssemblyQualifiedNameToKnownValues =
        let asmTypeName = typeof<'T>.AssemblyQualifiedName
        match knownValues.TryFind asmTypeName with
        | Some values ->
            knownValues.Add(asmTypeName, (box value)::values)
        | None ->
            knownValues.Add(asmTypeName, [box value])

    let private withTargetFactoryDependency (shouldMaterializeTarget: bool) (otherTargetFactory: TargetFactory<'U>) (dependency: Dependency<'U, 'T>) targetFactory : TargetFactory<'T> =
        let dependencyMaterializer =
            fun materializationContext targetFactory ->
                let leftOp =
                    getTargetName materializationContext.EntryBuildFile dependency.Op otherTargetFactory

                let rightOp =
                    getTargetName materializationContext.EntryBuildFile dependency.DependentOp targetFactory

                let isOtherTargetFactoryAlreadyMaterialized =
                    Set.contains otherTargetFactory.BuildFile materializationContext.MaterializedBuildFiles

                let updatedMaterializationContext =
                    if shouldMaterializeTarget && not isOtherTargetFactoryAlreadyMaterialized then
                        let updatedMaterializationContext = {
                            materializationContext with
                                MaterializedBuildFiles = materializationContext.MaterializedBuildFiles.Add otherTargetFactory.BuildFile }

                        let knownValues =
                            match targetFactory.KnownTypeNamesToValues.TryFind (typeof<'U>.AssemblyQualifiedName) with
                            | Some values ->
                                values |> Seq.cast<'U> |> Set.ofSeq
                            | None ->
                                Set.empty

                        materialize knownValues updatedMaterializationContext otherTargetFactory
                    else
                        materializationContext

                let dependencyOperator = if dependency.IsHardDependency then (==>) else (?=>)
                dependencyOperator leftOp rightOp
                    |> ignore

                updatedMaterializationContext

        { targetFactory
            with
                Dependencies           = dependencyMaterializer::targetFactory.Dependencies
                KnownTypeNamesToValues =
                    targetFactory.KnownTypeNamesToValues
                    |> addKnownValue dependency.Op
                    |> addKnownValue dependency.DependentOp
        }

    let withInternalDependency (dependency: Dependency<'T, 'T>) (targetFactory: TargetFactory<'T>) : TargetFactory<'T> =
        withTargetFactoryDependency false targetFactory dependency targetFactory

    let withExternalDependency (otherTargetFactory: TargetFactory<'U>) (dependency: Dependency<'U, 'T>) (targetFactory: TargetFactory<'T>) : TargetFactory<'T> =
        withTargetFactoryDependency true otherTargetFactory dependency targetFactory

    let withInternalDependencyBuilder (builder: ('T -> Option<'T>)) (targetFactory: TargetFactory<'T>) : TargetFactory<'T> =
        targetFactory.Values.GetAll()
        |> Seq.choose (fun (KeyValue (_, targetValue)) ->
            match targetValue with
            | ValuePresent value ->
                match builder value with
                | Some dependentValue when dependentValue <> value ->
                    Some (dependentValue >=> value)
                | _ ->
                    None
            | _ -> None
        )
        |> Seq.fold (
            fun targetFactory dependency ->
                withInternalDependency dependency targetFactory
        ) targetFactory

    let withExternalDependencyBuilder (builder: ('T -> Option<'U * TargetFactory<'U>>)) (targetFactory: TargetFactory<'T>) : TargetFactory<'T> =
        targetFactory.Values.GetAll()
        |> Map.toSeq
        |> Seq.map snd
        |> Seq.choose (fun targetValue ->
            match targetValue with
            | ValuePresent value ->
                match builder value with
                | Some (dependentValue, otherTargetFactory) ->
                    (otherTargetFactory, (dependentValue >=> value))
                    |> Some
                | _ ->
                    None
            | _ -> None
        )
        |> Seq.fold (
            fun targetFactory (otherTargetFactory, dependency) ->
                withExternalDependency otherTargetFactory dependency targetFactory
        ) targetFactory

    let initialize (targetFactory: TargetFactory<'T>) : TargetFactory<'T> =
        if targetFactory.IsInitialized then
            failwithf "TargetFactory %s cannot be initialized more than once" targetFactory.BuildFile.Path

        let isEntryPointScript =
            Context.getExecutionContext()
            |> Context.getFakeExecutionContext
            |> Option.map (
                fun fakeExecCtx ->
                    let scriptFileNormalized = System.IO.Path.GetFullPath fakeExecCtx.ScriptFile |> Path.normalizeFileName
                    let buildFileNormalized = System.IO.Path.GetFullPath targetFactory.BuildFile.Path |> Path.normalizeFileName
                    scriptFileNormalized = buildFileNormalized)
            |> Option.defaultValue false

        if isEntryPointScript then
            let materializationContext = {
                EntryBuildFile = targetFactory.BuildFile
                MaterializedBuildFiles = Set.empty
            }

            materialize Set.empty materializationContext targetFactory
            |> ignore

            match targetFactory.DefaultTarget with
            | Some targetValue ->
                targetFactory.Values.GetTargetName targetValue
                |> Target.runOrDefaultWithArguments

            | None ->
                Target.runOrList()

        { targetFactory with IsInitialized = true }