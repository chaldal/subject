open System.IO
open System.Xml
open System

#r "System.Xml.Linq.dll"

open System.Xml.Linq

// Find all new views that need to be added to AllLifeCycles.fs

// FIXME 1: Eventually use FSharp.Compiler.Service to emit tokens at the right position instead of emitting text
// FIXME 2: postActions in dotnet new is limited; we're unable to get any symbols from template processing, instead
//          having to rely on the template processing pushing the symbol into the file system (under the .tmp dir)
//          and reading from there. Apparently they're working on refactoring this.
//          Until then, be aware that if this script doesn't run, and the .tmp file not cleared, we may have funny issues

let sourceDir = __SOURCE_DIRECTORY__

let appendViewToAllLifeCycles (viewName: string) =
    let viewNameWithFirstLetterLowerCase =
        viewName.ToCharArray()
        |> Array.mapi (fun i ch -> if i = 0 then (Char.ToLowerInvariant ch) else ch)
        |> String

    let filePath = sprintf "%s/../Ecosystem/LifeCycles/AllLifeCycles.fs" sourceDir
    let code = File.ReadAllText filePath
    let addViewCode = sprintf "    |> addView %sView%s    |> buildEcosystem" viewNameWithFirstLetterLowerCase System.Environment.NewLine
    let newCode = code.Replace ("    |> buildEcosystem", addViewCode)
    File.WriteAllText(filePath, newCode)

let appendViewDefToEcosystemDef (viewName: string) =
    let viewNameWithFirstLetterLowerCase =
        viewName.ToCharArray()
        |> Array.mapi (fun i ch -> if i = 0 then (Char.ToLowerInvariant ch) else ch)
        |> String

    let filePath = sprintf "%s/../Ecosystem/T__EC__T.Types/EcosystemDef.fs" sourceDir
    let code = File.ReadAllText filePath

    let lineToAdd =
        "let (T__LCI__T: ViewDef<T__LC__TInput, T__LC__TOutput, T__LC__TOpError>,
             ecosystemDef) =
             addViewDef ecosystemDef \"T__LC__T\"
        \n    "

    let lineToAdd = lineToAdd.Replace("T__LCI__T", viewNameWithFirstLetterLowerCase)
    let lineToAdd = lineToAdd.Replace("T__LC__T", viewName)

    let indexOfFirstBracket = code.IndexOf("{|")
    let code = code.Insert(indexOfFirstBracket, lineToAdd)

    let indexOfLastBracket = code.LastIndexOf("{|")
    let lineToAdd =
        "\n               " + viewNameWithFirstLetterLowerCase + " = " + viewNameWithFirstLetterLowerCase
    let code = code.Insert(indexOfLastBracket + 2, lineToAdd)

    File.WriteAllText(filePath, code)




let labelAttribute = XName.op_Implicit "Label"
let includeAttribute = XName.op_Implicit "Include"

let getCompileNodeForFile (fileToCompile: string) =
    let xel = XName.op_Implicit "Compile" |> XElement
    xel.SetAttributeValue(includeAttribute, fileToCompile)
    xel

let saveXmlDoc (xdoc: XDocument) (filePath: string) =
    let settings = XmlWriterSettings(OmitXmlDeclaration = true, Indent = true)
    use xw = XmlWriter.Create(filePath, settings)
    xdoc.Save xw

let addCompileIncludeFileToFsProj (fsProjFile: string) (fileToCompile: string) (addToLast: bool) =
    let xmlDoc = XDocument.Load fsProjFile
    let maybeItemGroupNode =
        xmlDoc.Root.Nodes()
        |> Seq.where (fun xNode -> xNode.NodeType = XmlNodeType.Element)
        |> Seq.cast<XElement>
        |> Seq.where (fun xElement -> xElement.Name.LocalName = "ItemGroup")
        |> Seq.sortByDescending (fun xElement ->
            let attr = xElement.Attribute(labelAttribute)
            attr <> null && attr.Value = "Subject"
        )
        |> Seq.tryHead

    match maybeItemGroupNode with
    | Some itemGroupNode ->
        let compileNode = getCompileNodeForFile fileToCompile
        if addToLast then
            itemGroupNode.Add compileNode
        else
            itemGroupNode.AddFirst compileNode
        saveXmlDoc xmlDoc fsProjFile
    | None ->
        failwithf "No ItemGroups found within %s when trying to add %s" fsProjFile fileToCompile

let includeViewInTypesProject viewName =
    let fsProjFile = sprintf "%s/../Ecosystem/T__EC__T.Types/T__EC__T.Types.fsproj" sourceDir
    addCompileIncludeFileToFsProj fsProjFile (sprintf "%s.fs" viewName) true

let includeViewInLifeCyclesProject viewName =
    let fsProjFile = sprintf "%s/../Ecosystem/LifeCycles/LifeCycles.fsproj" sourceDir
    addCompileIncludeFileToFsProj fsProjFile (sprintf "%sView.fs" viewName)  false

let includeViewInTestsProject viewName =
    let fsProjFile = sprintf "%s/../Ecosystem/Tests/Tests.fsproj" sourceDir
    addCompileIncludeFileToFsProj fsProjFile (sprintf "%sTests.fs" viewName)  true

if Directory.Exists (sprintf "%s/.tmp/" sourceDir) then
    Directory.GetFiles((sprintf "%s/.tmp/" sourceDir), "*.new")
    |> Seq.iter (fun fileName ->
        let viewName = Path.GetFileNameWithoutExtension fileName
        appendViewToAllLifeCycles viewName
        appendViewDefToEcosystemDef viewName
        includeViewInTypesProject viewName
        includeViewInLifeCyclesProject viewName
        includeViewInTestsProject viewName
        File.Delete fileName
    )