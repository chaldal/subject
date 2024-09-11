module AppRenderDslCompiler.Render.Parsing

open System.Xml

open AppRenderDslCompiler.Render.Types
open LibDsl.Parsing.XmlParsing

type ReactTemplateRootNode
with
    static member Make (children: List<ReactTemplateNode>) (rawOpensString: string) (maybeRtTypeParameters: Option<string>) : ReactTemplateRootNode =
        let (opens, moduleAliases) = ReactTemplateRootNode.ParseRawOpensString rawOpensString

        {
            Children            = children
            MaybeTypeParameters = maybeRtTypeParameters
            Opens               = opens
            ModuleAliases       = moduleAliases
        }

    static member ParseRawOpensString (rawOpensString: string) : (List<string> * Map<string, string>) =
        let rawRtOpens =
            rawOpensString.Split(";")
            |> Seq.ofArray
            |> Seq.filterMap (fun curr ->
                match curr.Trim() with
                | ""       -> None
                | nonempty -> Some nonempty
            )
            |> List.ofSeq

        let (rawOpens, rawModuleAliases) =
            rawRtOpens
            |> List.map (fun c -> c.Split(":=", 2) |> List.ofArray |> List.map (fun c -> c.Trim()))
            |> List.partition (fun parts -> parts.Length < 2)

        let opens = rawOpens |> List.map List.head // safe because of restriction on Split above

        let moduleAliases =
            rawModuleAliases
            |> List.map (fun c -> (c.Head, c.Tail.Head)) // safe because of restriction on Split above
            |> Map.ofList

        (opens, moduleAliases)

let tryGetAttributeValue (node: XmlNode) (name: string) : Option<string> =
    node.AttributesSeq
    |> Seq.tryFind (fun attribute -> attribute.Name = name)
    |> Option.map  (fun attribute -> attribute.Value)

let private bindingsRegex = System.Text.RegularExpressions.Regex(@";\s*([a-zA-Z_\(\), :]+|\{[a-zA-Z_\(\),;= ]+\})\s*:=\s*")
let private parseLetBindings (source: string) : Result<LetBindings, ParsingError> =
    let chunks = bindingsRegex.Split("; " + source)
    match chunks.Length with
    | 1 -> sprintf "rt-let binding seems malformed: %s" source |> UnexpectedStructure |> Error
    | _ ->
        let pairs =
            bindingsRegex.Split("; " + source)
            |> Seq.skip 1 // skipping first empty block introduced by added ;
            |> Seq.chunkBySize 2
            |> Seq.map (fun pair -> (Identifier (pair.[0].Trim() |> NonemptyString.ofStringUnsafe), Expression.Make pair.[1] |> Option.get))
            |> Seq.toList

        Ok pairs

let private classConditionalsSanitizeRegex = System.Text.RegularExpressions.Regex(@"^(.*?)(\s*;\s*)?$", System.Text.RegularExpressions.RegexOptions.Singleline)
let private classConditionalsRegex = System.Text.RegularExpressions.Regex(@";\s*(([a-zA-Z\-]+)|(`[^`]+`))\s*:=\s*")
let private parseClassConditionals (rawSource: string) : Result<ClassConditionals, ParsingError> =
    // given how we parse the bindings, it's a non-trivial task to strip out trailing semicolons
    let theMatch = classConditionalsSanitizeRegex.Match rawSource
    match theMatch.Success with
    | true ->
        let source = theMatch.Groups.Item(1).Value
        let pairs =
            classConditionalsRegex.Split("; " + source)
            |> Seq.skip 1 // skipping first empty block introduced by added ;
            |> Seq.chunkBySize 3
            |> Seq.map (fun pair -> (ClassNameExpression (pair.[0].Trim() |> NonemptyString.ofStringUnsafe), BooleanExpression (pair.[2].Trim())))
            |> Seq.toList

        match pairs with
        | [] -> Error (UnexpectedStructure (sprintf "rt-class value doesn't have the := binding syntax. Did you mean to use `class`? Value: %s" rawSource))
        | _  -> Ok pairs

    | false -> Error (UnexpectedStructure (sprintf "failed to sanitize rt-class value: %s" rawSource))

let private mapRegex = System.Text.RegularExpressions.Regex(@"^(.*) := (.*)$")
let private parseMapParts (source: string) : Result<{| Curr: NonemptyString; Collection: NonemptyString |}, ParsingError> =
    let theMatch = mapRegex.Match source
    match theMatch.Success with
    | true ->
        {|
          Collection = theMatch.Groups.Item(2).Value |> NonemptyString.ofStringUnsafe
          Curr       = theMatch.Groups.Item(1).Value |> NonemptyString.ofStringUnsafe
        |}
        |> Ok
    | false -> Error (UnexpectedStructure (sprintf "rt-map[i|o]? expression does not match expected syntax: %s" source))

let private propBindingsRegex = System.Text.RegularExpressions.Regex(@"^([\^]?[a-zA-Z]+)\s*(\(([^|]*)\))?( \|> (.*))?$")
let private parsePropBinding (source: NonemptyString) : Result<PropBinding, ParsingError> =
    let theMatch = propBindingsRegex.Match source.Value
    match theMatch.Success with
    | true ->
        let propName        = theMatch.Groups.Item(1).Value |> NonemptyString.ofStringUnsafe
        let maybeParameters = theMatch.Groups.Item(3).Value |> NonemptyString.ofString
        let maybeTransforms = theMatch.Groups.Item(5).Value |> NonemptyString.ofString
        Ok {
            Name            = propName
            MaybeParameters = maybeParameters
            MaybeTransforms = maybeTransforms
        }
    | false -> Error (UnexpectedStructure (sprintf "rt-prop expression does not match expected syntax: \"%O\"" source))

let private withBindingsRegex = System.Text.RegularExpressions.Regex(@"^([\^]?)(.*?)( \|> (.*))?$")
let private parseWithBinding (source: NonemptyString) : Result<PropBinding, ParsingError> =
    let theMatch = withBindingsRegex.Match source.Value
    match theMatch.Success with
    | true ->
        let propName        = theMatch.Groups.Item(1).Value + "With" |> NonemptyString.ofLiteral
        let maybeParameters = theMatch.Groups.Item(2).Value |> NonemptyString.ofString
        let maybeTransforms = theMatch.Groups.Item(4).Value |> NonemptyString.ofString
        Ok {
            Name            = propName
            MaybeParameters = maybeParameters
            MaybeTransforms = maybeTransforms
        }
    | false -> Error (UnexpectedStructure (sprintf "rt-with expression does not match expected syntax: \"%O\"" source))

let parseAttributes (isRootNode: bool) (elementName: string) (attributes: seq<XmlAttribute>) : Result<Attributes * MetaAttributes, ParsingError> =
    // NOTE: even though the type is List, attribute names are guaranteed to be unique by
    // the XML library, lest we get a XmlSyntaxError at LoadXml time.

    // NOTE: using mutability internally here for readability, otherwise the fold operation's type become insane
    // because of error handling and lifting that would need to be performed out of the nested list
    let mutable attributesAcc = List.empty<AttributeName * SingleLineExpression>
    let mutable specialAttributes = MetaAttributes.None
    let mutable errors = List.empty<ParsingError>

    attributes
    |> Seq.iter
        (fun (currAttribute: XmlAttribute) ->
            match currAttribute.Name with
            | "" ->
                errors <- (UnexpectedStructure "empty XML attribute name") :: errors

            | "rt-if"  ->
                specialAttributes <- { specialAttributes with MaybeIf = Some { Condition = BooleanExpression currAttribute.Value } }

            | "rt-fs"  ->
                specialAttributes <- { specialAttributes with RtFsharp = currAttribute.Value = "true" }

            | "rt-if-not"  ->
                specialAttributes <- { specialAttributes with MaybeIf = Some { Condition = BooleanExpression ("not (" + currAttribute.Value + ")") } }

            | "rt-let" ->
                match parseLetBindings currAttribute.Value with
                | Error error -> errors <- error :: errors
                | Ok letBindings -> specialAttributes <- { specialAttributes with MaybeLet = Some { Bindings = letBindings } }

            | "rt-class" ->
                match parseClassConditionals currAttribute.Value with
                | Error error -> errors <- error :: errors
                | Ok classConditionals -> specialAttributes <- { specialAttributes with MaybeClass = Some { ClassConditionals = classConditionals } }

            | "rt-map" | "rt-mapi" | "rt-mapo" ->
                match parseMapParts currAttribute.Value with
                | Error error -> errors <- error :: errors
                | Ok parts ->
                    let map =
                        match currAttribute.Name with
                        | "rt-map"  -> Map (SingleLineExpression parts.Collection, SingleLineExpression parts.Curr, WithIndex = false)
                        | "rt-mapi" -> Map (SingleLineExpression parts.Collection, SingleLineExpression parts.Curr, WithIndex = true)
                        | "rt-mapo" -> MapOfOption (SingleLineExpression parts.Collection, parts.Curr)
                        | _ -> failwith "Should never happen, we already narrowed the scope of this value down above"
                    specialAttributes <- { specialAttributes with MaybeMap = Some map }

            | "rt-prop-children" ->
                match NonemptyString.ofString currAttribute.Value with
                | None ->
                    errors <- (UnexpectedStructure (sprintf "Attribute %s had an empty value" currAttribute.Name)) :: errors
                | Some attributeValue ->
                    match parsePropBinding attributeValue with
                    | Error error    -> errors <- error :: errors
                    | Ok propBinding -> specialAttributes <- { specialAttributes with MaybePropChildren = Some propBinding }

            | "rt-with" ->
                match NonemptyString.ofString currAttribute.Value with
                | None ->
                    errors <- (UnexpectedStructure (sprintf "Attribute %s had an empty value" currAttribute.Name)) :: errors
                | Some attributeValue ->
                    match parseWithBinding attributeValue with
                    | Error error    -> errors <- error :: errors
                    | Ok propBinding -> specialAttributes <- { specialAttributes with MaybePropChildren = Some propBinding }

            | "rt-open" | "rt-type-parameters" ->
                if not isRootNode then
                    errors <- (UnexpectedStructure (currAttribute.Name + " is only allowed on the root element")) :: errors

            | "rt-style" ->
                match NonemptyString.ofString currAttribute.Value with
                | None ->
                    errors <- (UnexpectedStructure (sprintf "Attribute %s had an empty value" currAttribute.Name)) :: errors
                | Some attributeValue ->
                    specialAttributes <- { specialAttributes with MaybeStyle = Some (SingleLineExpression attributeValue) }

            | "rt-anistyle" ->
                match NonemptyString.ofString currAttribute.Value with
                | None ->
                    errors <- (UnexpectedStructure (sprintf "Attribute %s had an empty value" currAttribute.Name)) :: errors
                | Some attributeValue ->
                    specialAttributes <- { specialAttributes with MaybeAniStyle = Some (SingleLineExpression attributeValue) }

            | "rt-new-styles" ->
                match NonemptyString.ofString currAttribute.Value with
                | None ->
                    errors <- (UnexpectedStructure (sprintf "Attribute %s had an empty value" currAttribute.Name)) :: errors
                | Some attributeValue ->
                    specialAttributes <- { specialAttributes with MaybeNewStyles = Some (SingleLineExpression attributeValue) }

            | attrName when attrName.StartsWith "rt-" ->
                errors <- (UnknownRtAttribute attrName) :: errors

            | attrName ->
                match NonemptyString.ofString currAttribute.Value with
                | None ->
                    errors <- (UnexpectedStructure (sprintf "Attribute %s had an empty value" attrName)) :: errors
                | Some attributeValue ->
                    attributesAcc <- (AttributeName (attrName |> NonemptyString.ofStringUnsafe), SingleLineExpression attributeValue) :: attributesAcc
        )

    if specialAttributes.RtFsharp then
        match attributesAcc |> List.findMap (fun (AttributeName name, SingleLineExpression value) -> if name.Value = "class" then Some value.Value else None) with
        | Some classValue -> errors <- (UnexpectedStructure $"Element {elementName} has rt-fs set to true, but also has a class attribute set to {classValue}") :: errors
        | None -> ()

    match errors.IsEmpty with
    | true  -> Ok (attributesAcc, specialAttributes)
    | false -> Error errors.Head

let private componentNameRegex = System.Text.RegularExpressions.Regex("[a-zA-Z]+")

// Whether something is childless or childless is dictated by the parameters its make function in Fable.React.Standard takes
let domNodeNamesChildless = Set.ofList ["br"; "col"; "embed"; "hr"; "img"; "input"; "keygen"; "link"; "menuitem"; "meta"; "param"; "source"; "track"; "wbr";"circle";"ellipse";"line";"polygon";"polyline";"rect";"text";"tspan";"textPath"]
let private domNodeNamesChildful  = Set.ofList ["div"; "span"; "i"; "b"; "strong"; "a"; "h1"; "h2"; "h3"; "h4"; "h5"; "h6"; "ul"; "li"; "p"; "button"; "select"; "option"; "table"; "thead"; "tbody"; "tr"; "th"; "td";"svg";"g";"path";"sup"]
let private domNodeNames = Set.union domNodeNamesChildful domNodeNamesChildless

let private nameSpaceAndNameForComponent (thisLibraryAlias: string) (componentLibraryAliases: Map<string, string>) (rawComponentName: string) : Result<string * string * string, ParsingError> =
    let (maybeLibraryAlias, name) =
        match rawComponentName.Split(".", 2) with
        | [| unqualifiedName |] -> ("default", unqualifiedName)
        | [| maybeLibraryAlias; name |] -> (maybeLibraryAlias, name)
        | _ -> failwith "String.Split was asked for at most 2 items, but we got something else."

    match componentLibraryAliases.TryFind maybeLibraryAlias with
    | Some fullNamespace -> Ok (fullNamespace, name, maybeLibraryAlias)
    | None ->
        match componentLibraryAliases.TryFind "default" with
        | Some defaultNamespace -> Ok (defaultNamespace, rawComponentName, thisLibraryAlias)
        | None -> Error (ParsingError.MissingDefaultLibraryMapping)

let private withoutComments (nodes: List<ReactTemplateNode>): List<ReactTemplateNode> =
    nodes
    |> List.filter (function | Comment _ -> false | _ -> true)

let rec private parseNodes (thisLibraryAlias: string) (componentLibraryAliases: Map<string, string>) (componentAliases: Map<string, string>) (xmlNodeList: XmlNodeList): Result<List<ReactTemplateNode>, ParsingError> =
    // TODO figure out how to avoid going through seq
    xmlNodeList
    |> Seq.cast<XmlNode>
    |> Seq.map (parseNode thisLibraryAlias componentLibraryAliases componentAliases false)
    |> List.ofSeq
    |> Result.liftFirst

and private parseNode (thisLibraryAlias: string) (componentLibraryAliases: Map<string, string>) (componentAliases: Map<string, string>) (isRootNode: bool) (xmlNode: XmlNode): Result<ReactTemplateNode, ParsingError> =
    let parseNodesBound = parseNodes thisLibraryAlias componentLibraryAliases componentAliases

    let possiblyAliasedNodeName =
        match componentAliases.TryFind xmlNode.Name with
        | None         -> xmlNode.Name
        | Some mapping -> mapping

    resultful {
        match possiblyAliasedNodeName with
        | prefixedDomNodeName when prefixedDomNodeName.StartsWith "dom." && domNodeNames.Contains (prefixedDomNodeName.Substring 4) ->
            let domNodeName = prefixedDomNodeName.Substring 4
            let! (attributes, metaAttributes) = parseAttributes isRootNode possiblyAliasedNodeName xmlNode.AttributesSeq
            // TODO look into more powerful computation expression primitives, possibility of nesting, etc
            let childrenWrapped: Result<Option<List<ReactTemplateNode>>, ParsingError> =
                match (xmlNode.ChildNodes.Count = 0, Set.contains domNodeName domNodeNamesChildless) with
                | (true,   true) -> Ok None
                | (false,  true) -> Error (UnexpectedStructure (sprintf "DOM node %s doesn't expect children but got %d of them" domNodeName xmlNode.ChildNodes.Count))
                | (_,     false) -> (parseNodesBound xmlNode.ChildNodes) |> Result.map Some
            let! maybeChildren = childrenWrapped
            return DomNode(domNodeName, maybeChildren, attributes, metaAttributes)

        | "rt-root" ->
            match isRootNode with
            | false -> return! Error (UnexpectedStructure "rt-root can only be the root node")
            | true ->
                let! (attributes, metaAttributes) = parseAttributes isRootNode possiblyAliasedNodeName xmlNode.AttributesSeq
                match (attributes, metaAttributes) with
                | (_, { MaybeIf = None; MaybeLet = None; MaybeMap = None; MaybeClass = None; MaybePropChildren = None; }) ->
                    let! children = parseNodesBound xmlNode.ChildNodes

                    let rootNode =
                        ReactTemplateRootNode.Make
                            children
                            (tryGetAttributeValue xmlNode "rt-open" |> Option.getOrElse "")
                            (tryGetAttributeValue xmlNode "rt-type-parameters")

                    return RtRoot rootNode
                | _ ->
                    return! UnexpectedAttributes ("rt-root", ["rt-open"; "rt-type-parameters"], (attributes, metaAttributes)) |> Error

        | "rt-sharp" ->
                let! (attributes, metaAttributes) = parseAttributes isRootNode possiblyAliasedNodeName xmlNode.AttributesSeq
                match (attributes, metaAttributes) with
                | ([(AttributeName (NonemptyString "value"), SingleLineExpression value)], { MaybeIf = None; MaybeLet = None; MaybeMap = None; MaybeClass = None; MaybePropChildren = None; }) ->
                    return RtSharp value
                | _ ->
                    return! UnexpectedAttributes ("rt-sharp", ["value"], (attributes, metaAttributes)) |> Error

        | "rt-match" ->
            let! (attributes, metaAttributes) = parseAttributes isRootNode possiblyAliasedNodeName xmlNode.AttributesSeq
            match (attributes, metaAttributes) with
            | ([(AttributeName (NonemptyString "what"), expression)], {MaybeIf = _; MaybeLet = None; MaybeMap = None; MaybeClass = None; MaybePropChildren = None}) ->
                let! children = parseNodesBound xmlNode.ChildNodes
                let cases =
                    List.foldBack
                        (fun curr acc ->
                            match curr with
                            | RtCase (is, children, metaAttributes) -> (is, children, metaAttributes) :: acc
                            | _                                     -> acc
                        )
                        children
                        List.empty
                match cases.Length = (withoutComments children).Length with
                | true  -> return RtMatch (expression, cases, metaAttributes)
                | false -> return! Error (UnexpectedStructure "rt-match is only allowed rt-case children")
            | _, _ ->
                return! UnexpectedAttributes ("rt-match", ["what"; "rt-if"], (attributes, metaAttributes)) |> Error

        | "rt-case" ->
            let! (attributes, metaAttributes) = parseAttributes isRootNode possiblyAliasedNodeName xmlNode.AttributesSeq
            match (attributes, metaAttributes) with
            // TODO we may want to support rt-let inside these
            | ([(AttributeName (NonemptyString "is"), expression)], {MaybeIf = None; MaybeLet = _; MaybeMap = None; MaybeClass = None; MaybePropChildren = None; }) ->
                let! children = parseNodesBound xmlNode.ChildNodes
                return RtCase (expression, children, metaAttributes)
            | _, _ ->
                return! UnexpectedAttributes ("rt-case", ["is"; "rt-let"], (attributes, metaAttributes)) |> Error

        | "rt-block" ->
            let! (attributes, metaAttributes) = parseAttributes isRootNode possiblyAliasedNodeName xmlNode.AttributesSeq
            match (isRootNode, attributes, metaAttributes) with
            | (false, [], { MaybeIf = None; MaybeLet = None; MaybeMap = None; MaybeClass = None; MaybePropChildren = None; }) ->
                return! Error (UnexpectedStructure "Pointless rt-block")
            | (_, [], { MaybeIf = _; MaybeLet = _; MaybeMap = _; MaybeClass = None; MaybePropChildren = None; }) ->
                let! children = parseNodesBound xmlNode.ChildNodes
                return RtBlock (children, metaAttributes)
            | _, _, _ ->
                return! UnexpectedAttributes ("rt-block", ["rt-if"; "rt-let"; "rt-map"], (attributes, metaAttributes)) |> Error

        | "rt-prop" ->
            let! (attributes, metaAttributes) = parseAttributes isRootNode possiblyAliasedNodeName xmlNode.AttributesSeq
            match (attributes, metaAttributes) with
            | ([(AttributeName (NonemptyString "name"), SingleLineExpression propBindingSource)], {MaybeIf = None; MaybeLet = None; MaybeMap = None; MaybeClass = None; MaybePropChildren = None; MaybeStyle = None; MaybeAniStyle = None}) ->
                let! propBinding = parsePropBinding propBindingSource
                let! children = parseNodesBound xmlNode.ChildNodes
                return RtProp(propBinding, children)
            | _ -> return! UnexpectedAttributes ("rt-prop", ["name"], (attributes, metaAttributes)) |> Error

        | "rt-outer-let" ->
            let! (attributes, metaAttributes) = parseAttributes isRootNode possiblyAliasedNodeName xmlNode.AttributesSeq
            match (attributes, metaAttributes) with
            | ([(AttributeName (NonemptyString "name"), name)], {MaybeIf = None; MaybeLet = None; MaybeMap = None; MaybeClass = None; MaybePropChildren = None; MaybeStyle = None; MaybeAniStyle = None}) ->
                let! children = parseNodesBound xmlNode.ChildNodes
                return RtOuterLet(name, children)
            | _ -> return! UnexpectedAttributes ("rt-outer-let", ["name"], (attributes, metaAttributes)) |> Error

        | "rt-let" ->
            let! (attributes, metaAttributes) = parseAttributes isRootNode possiblyAliasedNodeName xmlNode.AttributesSeq
            match (attributes, metaAttributes) with
            | ([(AttributeName (NonemptyString "name"), name)], {MaybeIf = None; MaybeLet = None; MaybeMap = None; MaybeClass = None; MaybePropChildren = None; MaybeStyle = None; MaybeAniStyle = None}) ->
                let! children = parseNodesBound xmlNode.ChildNodes
                return RtLet(name, children)
            | _ -> return! UnexpectedAttributes ("rt-let", ["name"], (attributes, metaAttributes)) |> Error

        | "#text" ->
            return Text xmlNode.InnerText

        | "#cdata-section" ->
            return Cdata xmlNode.InnerText

        | comment when isCommentNodeName comment ->
            return Comment xmlNode.InnerText

        | componentName when componentNameRegex.IsMatch componentName ->
            let! (props, metaAttributes) = parseAttributes isRootNode possiblyAliasedNodeName xmlNode.AttributesSeq
            let! children = parseNodesBound xmlNode.ChildNodes
            let! (nameSpace, name, libraryAlias) = nameSpaceAndNameForComponent thisLibraryAlias componentLibraryAliases componentName
            return Component(libraryAlias, nameSpace, name, props, children, metaAttributes)

        | _ -> return! Error (UnexpectedStructure (sprintf "Unexpected node name %s (possibly aliased from %s)" possiblyAliasedNodeName xmlNode.Name))
    }

let parse (thisLibraryAlias: string) (componentLibraryAliases: Map<string, string>) (componentAliases: Map<string, string>) (source: string): Result<ReactTemplateRootNode, ParsingError> =
    resultful {
        let! tree =
            source
            |> LibDsl.Parsing.XmlParsing.toXmlTree
            |> Result.mapError XmlSyntaxError

        let! rawRootNode = tree |> parseNode thisLibraryAlias componentLibraryAliases componentAliases true
        let rootNode =
            match rawRootNode with
            | RtRoot rootNode -> rootNode
            | _ ->
                let rtOpenString = tryGetAttributeValue tree "rt-open" |> Option.getOrElse ""
                ReactTemplateRootNode.Make [rawRootNode] rtOpenString None

        return rootNode
    }
