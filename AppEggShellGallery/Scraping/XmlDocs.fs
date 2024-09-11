module Scraping.XmlDocs

open System.Xml
open LibDsl.Parsing.XmlParsing

[<RequireQualifiedAccess>]
type private ParsingError =
| XmlSyntaxError of string
| RootNodeMalformed
| UnexpectedStructure of string

type Example = {
    MaybeDescription: Option<NonemptyString>
    Codes:            NonemptyList<NonemptyString>
}

type Parameter = {
    Name:             NonemptyString
    MaybeDescription: Option<NonemptyString>
    MaybeType:        Option<NonemptyString>
    MaybeDefault:     Option<NonemptyString>
}

type Member = {
    Name:       NonemptyString
    Summary:    Option<NonemptyString>
    Parameters: List<Parameter>
    Examples:   List<Example>
    SetupCode:  Option<NonemptyString>
}

[<RequireQualifiedAccess>]
type DocsNode =
| Root    of List<Member>
| Members of List<Member>
| Member  of Member
| Summary of string
| Example of MaybeDescription: Option<NonemptyString> * Codes: List<NonemptyString>
| Remarks of MaybeDescription: Option<NonemptyString> * Codes: List<NonemptyString>
| Code    of string * IsSetup: bool
| Text    of string
| Param   of Parameter
| Assembly

let private isEmptyLine (line: string) : bool =
    line.Trim().Length = 0

let private countLeadingWhitespace (line: string) : int =
    let mutable i = 0
    while (i < line.Length && line[i] = ' ') do
        i <- i + 1
    i

let private stripLeadingWhitespace (source: string) : string =
    let lines = source.Split "\n"
    let leadingWhitespaceLength = lines |> Seq.filterNot isEmptyLine |> Seq.map countLeadingWhitespace |> Seq.min
    lines |> Seq.map (fun line -> if isEmptyLine line then "" else line.Substring leadingWhitespaceLength) |> String.concat "\n"

let private prefixAllNonemptyLines (prefix: string) (source: string) : string =
    source.Split "\n" |> Seq.map (fun line -> if isEmptyLine line then "" else prefix + line) |> String.concat "\n"

let private extractAttribute (xmlNode: XmlNode) (name: string) : Option<NonemptyString> =
    xmlNode.AttributesSeq
    |> Seq.findMap
        (fun (currAttribute: XmlAttribute) ->
            match currAttribute.Name with
            | attrName when attrName = name -> NonemptyString.ofString currAttribute.Value
            | _                             -> None
        )

let rec private parseNodes (xmlNodeList: XmlNodeList): Result<List<DocsNode>, ParsingError> =
    // TODO figure out how to avoid going through seq
    xmlNodeList
    |> Seq.cast<XmlNode>
    |> Seq.map parseNode
    |> List.ofSeq
    |> Result.liftFirst

and private parseNode (xmlNode: XmlNode): Result<DocsNode, ParsingError> =
    let nodeName = xmlNode.Name
    resultful {
        match nodeName with
        | "doc" ->
            let! children = parseNodes xmlNode.ChildNodes
            let members =
                children
                |> List.filterMap (function DocsNode.Members members -> Some members | _ -> None)
                |> List.flatten

            return DocsNode.Root members

        | "members" ->
            let! children = parseNodes xmlNode.ChildNodes
            let members = children |> List.filterMap (function DocsNode.Member mem -> Some mem | _ -> None)

            return DocsNode.Members members

        | "member" ->
            let! children = parseNodes xmlNode.ChildNodes
            let summary = children |> List.findMap (function DocsNode.Summary summary -> Some summary | _ -> None)
            let examples = children |> List.filterMap (function DocsNode.Example (description, codes) -> Some (description, codes) | _ -> None)
            let parameters = children |> List.filterMap (function DocsNode.Param parameter -> Some parameter | _ -> None)
            let setupCode = children |> List.findMap (function DocsNode.Remarks (_, codes) -> codes |> List.tryHead | _ -> None)

            match extractAttribute xmlNode "name" with
            | None -> return! ParsingError.UnexpectedStructure "No name or empty name on member" |> Error
            | Some nonemptyName ->
                return DocsNode.Member {
                    Name       = nonemptyName
                    Summary    = summary |> Option.flatMap NonemptyString.ofString
                    Parameters = parameters
                    SetupCode  = setupCode
                    Examples =
                        examples |> List.filterMap (fun (maybeDescription, codes) ->
                            codes
                            |> NonemptyList.ofList
                            |> Option.map (fun nonemptyCodes ->
                                {
                                    MaybeDescription = maybeDescription
                                    Codes            = nonemptyCodes
                                }
                            )
                        )
                }

        | "param" ->
            let! children = parseNodes xmlNode.ChildNodes
            let description = children |> List.findMap (function DocsNode.Text text -> Some text | _ -> None) |> Option.flatMap NonemptyString.ofString

            match extractAttribute xmlNode "name" with
            | None -> return! ParsingError.UnexpectedStructure "No name or empty name on member" |> Error
            | Some nonemptyName ->
                return DocsNode.Param {
                    Name             = nonemptyName
                    MaybeDescription = description
                    MaybeType        = extractAttribute xmlNode "type"
                    MaybeDefault     = extractAttribute xmlNode "default"
                }

        | "example" ->
            let! children = parseNodes xmlNode.ChildNodes
            let maybeDescription = children |> List.filterMap (function DocsNode.Text text -> NonemptyString.ofString text | _ -> None) |> List.tryHead
            let codes = children |> List.filterMap (function DocsNode.Code (code, false) -> NonemptyString.ofString code | _ -> None)

            return DocsNode.Example (maybeDescription, codes)

        | "remarks" ->
            let! children = parseNodes xmlNode.ChildNodes
            let maybeDescription = children |> List.filterMap (function DocsNode.Text text -> NonemptyString.ofString text | _ -> None) |> List.tryHead
            let codes = children |> List.filterMap (function DocsNode.Code (code, true) -> NonemptyString.ofString code | _ -> None)

            return DocsNode.Remarks (maybeDescription, codes)

        | "code" ->
            return DocsNode.Code (xmlNode.InnerText, extractAttribute xmlNode "setup" = Some (NonemptyString.ofLiteral "true"))

        | "summary" -> return DocsNode.Summary (xmlNode.InnerText.Trim())
        | "#text"   -> return DocsNode.Text (xmlNode.InnerText.Trim())

        | "assembly" -> return DocsNode.Assembly

        | _ -> return! ParsingError.UnexpectedStructure $"Unexpected node name {nodeName}" |> Error
    }

let private tripleQuote = "\"\"\""
let private openBrace = "{"
let private closeBrace = "}"

let private generateContentComponentName (rawName: NonemptyString) : string =
    (((rawName.Value.Split "(")[0]).Substring 2).Replace(".", "_")

let private extractComponentName (lossyNameMappings: Map<string, string>) (libraryPrefix: string) (rawName: NonemptyString) : Option<NonemptyString> =
    let trimmedRawName = ((rawName.Value.Split "(")[0]).Substring 2
    match lossyNameMappings.TryFind trimmedRawName with
    | Some mappedName -> Some (NonemptyString.ofStringUnsafe mappedName) // safe because manual literals
    | None ->
        let parts = trimmedRawName.Split $".{libraryPrefix}."
        if parts.Length = 2 then
            let suffix = parts[1]
            if suffix.EndsWith ".Static" then
                suffix.Substring (0, suffix.Length - ".Static".Length)
            else
                suffix
            |> NonemptyString.ofString
        else
            None

type Names = {
    Display:          string
    ContentComponent: string
    PrivateModule:    string
}

let private namesForMember (lossyNameMappings: Map<string, string>) (libraryPrefix: string) (rawName: NonemptyString) : Names =
    match extractComponentName lossyNameMappings libraryPrefix rawName with
    | Some componentName ->
        {
            Display          = componentName.Value
            ContentComponent = componentName.Value.Replace (".", "_")
            PrivateModule    = "Module_" + componentName.Value.Replace (".", "_")
        }
    | None ->
        {
            Display          = rawName.Value
            ContentComponent = generateContentComponentName rawName
            PrivateModule    = "Module_" + (generateContentComponentName rawName)
        }

let private generateContentFiles (lossyNameMappings: Map<string, string>) (libraryPrefix: string) (members: List<Member>) : unit =
    let path = $"../src/Components/ScrapedXmlDocsBasedContents/{libraryPrefix}.fs"

    let componentPrivateModules =
        members |> List.map (fun mem ->
            let names = namesForMember lossyNameMappings libraryPrefix mem.Name

            let parameters = (
                match mem.Parameters with
                | [] -> ""
                | parameters ->
                    let fields =
                        parameters
                        |> List.map (fun parameter ->
                            let typeString = parameter.MaybeType |> Option.map NonemptyString.value |> Option.getOrElse "unknown"
                            $"""
                        {openBrace}
                            Name = "{parameter.Name.Value}"
                            Type = "{typeString}"
                            Default = {match parameter.MaybeDefault with | None -> "None" | Some value -> "Some \"" + value.Value + "\""}
                            Description = {match parameter.MaybeDescription with | None -> "None" | Some value -> "Some \"" + value.Value + "\""}
                        {closeBrace}"""
                        )
                        |> String.concat ""

                    $"""
            props = AppEggShellGallery.Components.ComponentContent.PropsConfig.Manual (
                Ui.ComponentProps (data = {openBrace}
                    Fields = (Choice2Of2 [
{fields}
                    ])
                    MaybeScrapeErrors = None
                {closeBrace})
            ),"""
            )

            let maybeSetupCode =
                match mem.SetupCode with
                | None -> ""
                | Some setupCode -> $"""
                            Ui.Code (language = AppEggShellGallery.Components.Code.Fsharp, heading = "Setup Code", children = [| LC.Text {tripleQuote}
{setupCode.Value}{tripleQuote}
                            |])"""

            let samples =
                match mem.Examples with
                | [] -> "LC.Text \"No examples\""
                | examples ->
                    examples |> List.map (fun example ->
                        let maybeHeadingParameter = example.MaybeDescription |> Option.map (fun description -> $"heading = {tripleQuote}{description.Value}{tripleQuote},")

                        $"""
                Ui.ComponentSample (
                    {maybeHeadingParameter |> Option.getOrElse ""}
                    visuals = (element {openBrace}
{example.Codes.Head.Value}
                    {closeBrace}),
                    code =
                        ComponentSample.Children (element {openBrace}
                            Ui.Code (language = AppEggShellGallery.Components.Code.Fsharp, children = [| LC.Text {tripleQuote}
{example.Codes.Head.Value}{tripleQuote}
                            |])
{maybeSetupCode}
                        {closeBrace})
                )
"""
                    )
                    |> String.concat ""

            $"""
module private {names.PrivateModule} =
{mem.SetupCode |> Option.map NonemptyString.value |> Option.map (stripLeadingWhitespace >> prefixAllNonemptyLines "    ") |> Option.getOrElse ""}
    [<Component>]
    let content () : ReactElement =
        Ui.ComponentContent (
            displayName = "{names.Display}",
            {parameters}
            notes       = LC.Text {tripleQuote}{mem.Summary.Value}{tripleQuote},
            samples     = element {openBrace}
                {samples}
            {closeBrace}
        )
"""
        )
        |> String.concat "\n"


    let components =
        members |> List.map (fun mem ->
            let names = namesForMember lossyNameMappings libraryPrefix mem.Name

            $"""
    static member {names.ContentComponent} () : ReactElement =
        {names.PrivateModule}.content ()"""
        )
        |> String.concat "\n"


    let source = $"""
// ************************************************************ //
//                                                              //
//  THIS FILE IS AUTO-GENERATED FROM XML DOCUMENTATION COMMENTS //
//                                                              //
// ************************************************************ //

[<AutoOpen>]
module AppEggShellGallery.Components.ScrapedXmlDocsBasedContents_{libraryPrefix}

open Fable.React
open LibClient
open LibClient.Components
open ReactXP.Components
open ReactXP.Styles

{componentPrivateModules}

type Ui.XmlDocsContent.{libraryPrefix} with
    {components}
    """

    System.IO.File.WriteAllText (path, source)

let scrapeDocsAndGenerateContentFilesForLibrary (lossyNameMappings: Map<string, string>) (libraryPrefix: string, path: string) =
    let source = System.IO.File.ReadAllText path

    let membersRes =
        resultful {
            let! tree =
                source
                |> LibDsl.Parsing.XmlParsing.toXmlTree
                |> Result.mapError ParsingError.XmlSyntaxError

            let! rawRootNode = tree |> parseNode
            let! members =
                match rawRootNode with
                | DocsNode.Root members -> Ok members
                | _ -> Error ParsingError.RootNodeMalformed

            return members
        }

    match membersRes with
    | Ok members -> generateContentFiles lossyNameMappings libraryPrefix members
    | Error error ->
        printf "ERROR: %O" error
        printf "FULL SOURCE:\n%s" source


let libraries = [
    ("LC", "../../LibClient/src/bin/Debug/net7.0/LibClient.xml")
]

let lossyNameMappings = Map.ofList <| [
    ("LibClient.Components.With_Executor.With.Executor.Static", "With.Executor")
]

let scrapeDocsAndGenerateContentFiles () =
    libraries |> List.iter (scrapeDocsAndGenerateContentFilesForLibrary lossyNameMappings)
