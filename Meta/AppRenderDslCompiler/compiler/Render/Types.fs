module AppRenderDslCompiler.Render.Types

type Identifier = Identifier of NonemptyString

type AttributeName = AttributeName of NonemptyString

type StringExpression = StringExpression of NonemptyString

[<RequireQualifiedAccess>]
type Expression =
| SingleLine of SingleLineExpression
| Multiline  of List<string>
with
    static member Make (rawString: string) : Option<Expression> =
        NonemptyString.ofString rawString
        |> Option.map (fun nonemptyString ->
            if nonemptyString.Value.Contains "\n" then
                nonemptyString.Value.Split "\n" |> Array.toList |> Multiline
            else
                nonemptyString |> SingleLineExpression |> SingleLine
        )

and SingleLineExpression = SingleLineExpression of NonemptyString

type BooleanExpression = BooleanExpression of string

type ClassNameExpression = ClassNameExpression of NonemptyString // either string or expression that evaluates to string surrounded by []

type LetBindings = List<Identifier * Expression>

type ClassConditionals = List<ClassNameExpression * BooleanExpression>

type Attributes = List<AttributeName * SingleLineExpression>

type If = {
    Condition: BooleanExpression
}

type Let = {
    Bindings: LetBindings
}

type Map =
| Map         of Collection: SingleLineExpression * CurrExpression: SingleLineExpression * WithIndex: bool
| MapOfOption of Option: SingleLineExpression * SomeName: NonemptyString

type Class = {
    ClassConditionals: ClassConditionals
}

type PropBinding = {
    Name:            NonemptyString
    MaybeParameters: Option<NonemptyString>
    MaybeTransforms: Option<NonemptyString>
}

type [<RequireQualifiedAccess>] LetBinding =
| Regular of Name: SingleLineExpression

type MetaAttributes = {
    MaybeIf:           Option<If>
    MaybeLet:          Option<Let>
    MaybeMap:          Option<Map>
    MaybeClass:        Option<Class>
    MaybePropChildren: Option<PropBinding>
    MaybeStyle:        Option<SingleLineExpression>
    MaybeAniStyle:     Option<SingleLineExpression>
    MaybeNewStyles:    Option<SingleLineExpression>
    RtFsharp:          bool
}
with
    static member None = {
        MaybeIf           = None
        MaybeLet          = None
        MaybeMap          = None
        MaybeClass        = None
        MaybePropChildren = None
        MaybeStyle        = None
        MaybeAniStyle     = None
        MaybeNewStyles    = None
        RtFsharp          = false
    }

    member this.SomeNames : List<string> =
        [
            this.MaybeIf           |> Option.map (fun _ -> "rt-if")
            this.MaybeLet          |> Option.map (fun _ -> "rt-let")
            this.MaybeMap          |> Option.map (fun _ -> "rt-map")
            this.MaybeClass        |> Option.map (fun _ -> "rt-class")
            this.MaybePropChildren |> Option.map (fun _ -> "rt-prop-children")
            this.MaybeStyle        |> Option.map (fun _ -> "rt-style")
            this.MaybeAniStyle     |> Option.map (fun _ -> "rt-anistyle")
            this.MaybeNewStyles    |> Option.map (fun _ -> "rt-new-styles")
        ]
        |> List.filterMap id

type ReactTemplateRootNode = {
    Children:            List<ReactTemplateNode>
    MaybeTypeParameters: Option<string>
    Opens:               List<string>
    ModuleAliases:       Map<string, string>
}

and ReactTemplateNode =
| RtRoot     of ReactTemplateRootNode
| RtSharp    of Value: NonemptyString
| RtBlock    of Children: List<ReactTemplateNode> * MetaAttributes
| RtMatch    of What: SingleLineExpression * Cases: List<SingleLineExpression * List<ReactTemplateNode> * MetaAttributes> * MetaAttributes
| RtCase     of Is: SingleLineExpression * Children: List<ReactTemplateNode> * MetaAttributes
| RtProp     of PropBinding * Children: List<ReactTemplateNode>
| RtOuterLet of Name: SingleLineExpression * Children: List<ReactTemplateNode>
| RtLet      of Name: SingleLineExpression * Children: List<ReactTemplateNode>
| Text       of Value: string
| Cdata      of Value: string
| Comment    of Value: string
| DomNode    of Name: string * MaybeChildren: Option<List<ReactTemplateNode>> * Attributes * MetaAttributes
| Component  of LibAlias: string * NameSpace: string * Name: string * Props: Attributes * Children: List<ReactTemplateNode> * MetaAttributes

// TODO we would have loved to have line numbers in errors (and for source maps too)
// but the XmlDocument library does not give us line information, and a library that
// seems to, XDocument, doesn't seem to provide a palatable interface for traversing
// the node tree.

type ParsingError =
| XmlSyntaxError       of Message: string
| UnexpectedStructure  of Message: string
| UnexpectedAttributes of Tag: string * Allowed: List<string> * Actual: (Attributes * MetaAttributes)
| UnknownRtAttribute   of Name: string
| MissingDefaultLibraryMapping

type CodeGenerationError =
| CodeGenerationError of Message: string
| MissingDefaultLibraryMapping

type RenderCompilerError =
| ParsingError        of ParsingError
| CodeGenerationError of CodeGenerationError
with
    override this.ToString () : string =
        match this with
        | ParsingError parsingError ->
            match parsingError with
            | XmlSyntaxError message                    -> sprintf "XML Syntax Error: %s" message
            | UnexpectedStructure message               -> sprintf "Unexpected Structure: %s" message
            | UnknownRtAttribute name                   -> sprintf "Unknown rt- Attribute: %s" name
            | ParsingError.MissingDefaultLibraryMapping -> sprintf "Missing Library Mapping for the default library"
            | UnexpectedAttributes  (tag, allowedAttributes, (actualAttributes, actualMetaAttributes)) ->
                let allowedAttributesString = String.concat ", " allowedAttributes
                let actualAttributeNames = actualAttributes |> List.map (fun (AttributeName name, _) -> name)
                let actualAttributesString = String.concat ", " ((actualAttributeNames |> List.map NonemptyString.value) @ actualMetaAttributes.SomeNames)
                sprintf "Unexpected Attribute: tag `%s` expects [%s], but got [%s]" tag allowedAttributesString actualAttributesString

        | CodeGenerationError codeGenerationError ->
            match codeGenerationError with
            | CodeGenerationError.CodeGenerationError message  -> sprintf "Code generation error: %s" message
            | MissingDefaultLibraryMapping                     -> "Default mapping is missing from componentLibraryAliases"
