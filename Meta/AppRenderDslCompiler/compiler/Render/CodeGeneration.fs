module AppRenderDslCompiler.Render.CodeGeneration

open LibDsl.CodeGeneration.FsharpCode

open AppRenderDslCompiler.Code
open AppRenderDslCompiler.Render.Types

let private stringExpressionRegex = System.Text.RegularExpressions.Regex("\{(.*?)\}")
let private directQuoteRegex = System.Text.RegularExpressions.Regex("\{=(.*?)\}")
let private backticksRegex = System.Text.RegularExpressions.Regex("^`.*`$")
let private dynamicClassNameRegex = backticksRegex

let private escapeSpecialCharacters (s: string): string =
    s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n")

// Naive implementation for now, can't deal with braces inside expressions
// As soon as F# gets proper string interporlation, this should be ditched
let private generateForStringExpressionValue (rawExpressionSource: NonemptyString): NonemptyString =
    let expressionSource = rawExpressionSource.Value.Trim()

    let parts = stringExpressionRegex.Split(expressionSource) |> Array.toList

    match parts with
    | [expression]         -> sprintf "\"%s\"" (escapeSpecialCharacters expression)
    | [""; expression; ""] -> sprintf "(System.String.Format(\"{0}\", %s))" expression
    | _ ->
        // TODO filter out empty strings
        let formatPattern =
            parts
            |> List.mapi (fun i _ -> sprintf "{%d}" i)
            |> String.concat ""
        let formatArgs =
            parts
            |> List.mapi
                (fun index part ->
                    // because of how the regex works out,
                    // size % 2 == 1 here, 2nth match is string, 2n+1th is expression
                    match index % 2 with
                    | 0 -> "\"" + (escapeSpecialCharacters part) + "\""
                    | _ -> "(" + part + ")"
                )
        // Have to use type-unsafe String.Format because F# sprintf forces you to know
        // the types of interpolated values in advance, which is not feasible. Not a problem,
        // since this is auto-generated code anyway.
        sprintf "(System.String.Format(\"%s\", %s))" formatPattern (String.concat ", " formatArgs)

    |> NonemptyString.ofStringUnsafe

let private generateForCdataNodeValue (rawValue: string) : List<FsharpCode> =
    [
        Line ("@\"" + (rawValue.Replace("\"", "\"\"")) + "\"")
        Line ("|> makeTextNode2 __parentFQN")
    ]

let private generateForTextNodeValue (rawValue: string) : List<FsharpCode> =
    let value = rawValue.Trim()

    let parts = directQuoteRegex.Split(value) |> Array.toList

    match parts with
    | [lonelyString] ->
        match NonemptyString.ofString lonelyString with
        | Some nonemptyLonelyString -> [ Line (sprintf "makeTextNode2 __parentFQN %O" (generateForStringExpressionValue nonemptyLonelyString)) ]
        | None -> []
    | [""; lonelyExpression; ""] -> [ Line lonelyExpression ]
    | _ ->
        parts
        |> List.mapi
            (fun index part ->
                // because of how the regex works out,
                // size % 2 == 1 here, 2nth match is string, 2n+1th is expression
                match index % 2 with
                | 0 ->
                    part
                    |> NonemptyString.ofString
                    |> Option.flatMap (fun nonemptyString ->
                        match generateForStringExpressionValue nonemptyString with
                        | NonemptyString "\"\"" -> None
                        | value                 -> Line (sprintf "makeTextNode2 __parentFQN %O" value) |> Some
                    )
                | _ ->
                    match part with
                    | "" -> None
                    | _  -> Line part |> Some
            )
        |> List.filterMap id

let (* public for testing *) generateForStringExpression (StringExpression expressionSource): NonemptyString =
    generateForStringExpressionValue expressionSource

let (* public for testing *) generateForExpression (SingleLineExpression expressionSource): NonemptyString =
    match expressionSource.Value with
    | sourceWithBackticks when backticksRegex.Match(sourceWithBackticks).Success ->
        let trimmedSource =
            NonemptyString.ofStringUnsafe (sourceWithBackticks.[1 .. sourceWithBackticks.Length - 2])
        generateForStringExpression (StringExpression trimmedSource)
    | source ->
        "(" + source + ")"
        |> NonemptyString.ofStringUnsafe

let private generateForDomNodeAttribute (attribute: (AttributeName * SingleLineExpression)): NonemptyString =
    let (AttributeName name, expression) = attribute
    if name.Value = "key" then
        sprintf "unbox(\"key\", %s)" (generateForExpression expression).Value
    else
        sprintf "(FRP.%O (%O))" name (generateForExpression expression)
    |> NonemptyString.ofStringUnsafe

let private speciallyTreatedAttributeNames = Set.ofList [NonemptyString.ofStringUnsafe "class"]

let private generateForDomNodeAttributes (attributes: Attributes): seq<NonemptyString> =
    attributes
    |> Seq.filter (fun (AttributeName name, _) -> speciallyTreatedAttributeNames.DoesNotContain name)
    |> Seq.map generateForDomNodeAttribute

let private generateClassNameValue (attributes: Attributes) (maybeDynamicClass: Option<Class>): Option<NonemptyString> =
    let maybeStaticClass =
        attributes
        |> Seq.tryFind (fun (AttributeName name, _) -> name.Value = "class")
        |> Option.map snd

    match (maybeStaticClass, maybeDynamicClass) with
    | (None, None) -> None
    // NOTE class is treated specially, even though it's typed as an Expression, we treat it as a StringExpression.
    // This is because the standard use case for the class attribute is a string, yet we don't have a clean way
    // of modelling this exception in the type system.
    | (Some (SingleLineExpression staticClass), None) -> Some (generateForStringExpression (StringExpression staticClass))

    | (_, Some { ClassConditionals = classConditionals }) ->
        let maybeStaticClassString =
            maybeStaticClass
            |> Option.map (fun (SingleLineExpression staticClass) -> StringExpression staticClass)
            |> Option.map generateForStringExpression

        let maybeDynamicClassString =
            match classConditionals with
            | [] -> None
            | _ ->
                let formatPattern = [0 .. classConditionals.Length - 1] |> List.map (fun i -> sprintf " {%d}" i) |> String.concat ""
                let formatArgs =
                    classConditionals
                    |> List.map
                        (fun (ClassNameExpression nameExpression, BooleanExpression valueExpression) ->
                            let nameExpressionString =
                                match nameExpression with
                                | expression when dynamicClassNameRegex.Match(expression.Value).Success ->
                                    generateForExpression (SingleLineExpression expression)
                                | _ ->
                                    sprintf "\"%O\"" nameExpression
                                    |> NonemptyString.ofStringUnsafe

                            sprintf "(if (%s) then %O else \"\")" valueExpression nameExpressionString
                        )
                    |> String.concat ", "

                let dynamicClassString =
                    sprintf "System.String.Format(\"%s\", %s)" formatPattern formatArgs
                    |> NonemptyString.ofStringUnsafe

                Some dynamicClassString

        match (maybeStaticClassString, maybeDynamicClassString) with
        | (None, None)                    -> None
        | (Some staticClassString, None)  -> Some staticClassString
        | (None, Some dynamicClassString) -> Some dynamicClassString
        | (Some staticClassString, Some dynamicClassString) ->
            sprintf "%O + %O" staticClassString dynamicClassString
            |> NonemptyString.ofStringUnsafe
            |> Some

let private generateClassNameAttribute (attributes: Attributes) (maybeDynamicClass: Class option): Option<NonemptyString> = optional {
    let! value = generateClassNameValue attributes maybeDynamicClass
    return sprintf "(FRP.ClassName (%O))" value |> NonemptyString.ofStringUnsafe
}

let private expandBackreferenceOperator (fullyQualifiedComponentName: string) (rawSource: string) : string =
    rawSource.Replace("~", fullyQualifiedComponentName + ".")

let private polymorphicConstructorName (fullyQualifiedComponentName: string) (propName: NonemptyString) : string =
    fullyQualifiedComponentName + ".Prop" + propName.Value + "Factory.Make"

let private expandPolymorphicPropConstructorOperator (fullyQualifiedComponentName: string) (propName: NonemptyString) (rawSource: string) : string =
    if rawSource.StartsWith "^" then
        (polymorphicConstructorName fullyQualifiedComponentName propName) + rawSource.Substring(1)
    else rawSource

let private maybeExpandBackreferenceOperator (maybeFullyQualifiedComponentName: Option<string>) (rawSource: string) : string =
    maybeFullyQualifiedComponentName
    |> Option.map (fun fqcn -> expandBackreferenceOperator fqcn rawSource)
    |> Option.getOrElse rawSource

let private generateForRawComponentProp (fullyQualifiedComponentName: string) (prop: AttributeName * SingleLineExpression): FsharpCode =
    let (AttributeName name, SingleLineExpression rawSource) = prop
    let sourceWithBackreferenceAndPolymorphicConstructorExpanded =
        rawSource.Value
        |> expandBackreferenceOperator fullyQualifiedComponentName
        |> expandPolymorphicPropConstructorOperator fullyQualifiedComponentName name
        |> NonemptyString.ofStringUnsafe

    (generateForExpression (SingleLineExpression sourceWithBackreferenceAndPolymorphicConstructorExpanded)).Value
    |> Line

let private generateForComponentProp (prop: (AttributeName * FsharpCode)): FsharpCode =
    let (AttributeName name, valueCode) = prop
    match valueCode with
    | Line source ->
        Line (sprintf "%s = %s" (makeFunctionParameterName name.Value) source)
    | _ ->
        Codes [
            Line (sprintf "%s =" (makeFunctionParameterName name.Value))
            IndentedBlock [valueCode]
        ]

let private generateForComponentProps (props: List<AttributeName * FsharpCode>): FsharpCode =
    props
    |> Seq.filter (fun (AttributeName name, _) -> speciallyTreatedAttributeNames.DoesNotContain name)
    |> Seq.map generateForComponentProp
    |> List.ofSeq
    |> ParameterList

let private sanitizedPropName (rawPropName: NonemptyString) : NonemptyString =
    match rawPropName.Value with
    | s when s.StartsWith "^" -> s.Substring(1)
    | s                       -> s
    |> NonemptyString.ofStringUnsafe

// TODO get rid of maybeFullyQualifiedComponentName, it's a leftover from a big refactoring
let private generateForBindings (_maybeFullyQualifiedComponentName: Option<string>) (bindings: Map<LetBinding, FsharpCode>) : List<NonemptyString * FsharpCode> =
    bindings
    |> Map.toList
    |> List.map (fun (letBinding, innerCode) ->
        let bindingName =
            match letBinding with
            | LetBinding.Regular (SingleLineExpression name) -> name

        let code =
            let innerCodeAsFragment =
                Codes [
                    IndentedBlock [innerCode]
                ]

            match letBinding with
            | LetBinding.Regular _ -> innerCodeAsFragment
        (bindingName, code)
    )

type private ChildrenFormat =
| AsElement
| AsElements
| AsElementsWithExtraParentheses
with
    member this.CEStart : string =
        match this with
        | AsElement                      -> "(castAsElementAckingKeysWarning [|"
        | AsElements                     -> "[|"
        | AsElementsWithExtraParentheses -> "([|"

    member this.CEEnd : string =
        match this with
        | AsElement                      -> "|])"
        | AsElements                     -> "|]"
        | AsElementsWithExtraParentheses -> "|])"

let private wrapChildrenBlock (format: ChildrenFormat) (maybeFullyQualifiedComponentName: Option<string>) (innerLetBindings: Map<LetBinding, FsharpCode>) (childrenCode: List<FsharpCode>) : FsharpCode =
    match childrenCode.Length with
    | 0 -> Line "[||]"
    | _ ->
        let bindingsCode = generateForBindings maybeFullyQualifiedComponentName innerLetBindings
        Codes [
            Line format.CEStart
            IndentedBlock (
                (bindingsCode |> List.map (fun (name, code) ->
                    [
                        Line ("let " + name.Value + " = (")
                        IndentedBlock [code]
                        Line ")"
                    ]
                ) |> List.concat)
                @ childrenCode
            )
            Line format.CEEnd
        ]

type ControlFlowWrapper = {
        before:           string
        after:            string
        additionalIndent: int
    } with
        static member Empty = { before = ""; after = ""; additionalIndent = 0 }

let private wrapInControlFlow (maybeFullyQualifiedComponentName: Option<string>) (metaAttributes: MetaAttributes) (code: FsharpCode): FsharpCode =
    // NOTE if > map > let
    // i.e. if is evaluated above all, so it guards the whole iterator,
    // let is on the other hand in the context of an individual iteration,
    // which is convenient for pulling out values

    let maybeExpandBackreferenceOperator =
        maybeFullyQualifiedComponentName
        |> Option.map expandBackreferenceOperator
        |> Option.getOrElse id

    let afterLet =
        match metaAttributes.MaybeLet with
        | Some theLet ->
            let bindings =
                theLet.Bindings
                |> List.map
                    (fun (Identifier identifier, expression) ->
                        match expression with
                        | Expression.SingleLine (SingleLineExpression source) ->
                            Line (sprintf "let %O = %s" identifier (maybeExpandBackreferenceOperator source.Value))
                        | Expression.Multiline sourceLines ->
                            // TODO by right we should be stripping the most-common-leading-whitespace
                            // from the lines before putting them in the IndentedBlock, but because of
                            // how we do parsing, the whitespace of the first line is stripped anyway,
                            // and the code that I need this multiline stuff to work for now happens to
                            // work correctly, so I'm not bothering.
                            Codes [
                                Line (sprintf "let %O = (" identifier)
                                IndentedBlock (sourceLines |> List.map (maybeExpandBackreferenceOperator >> Line))
                                Line ")"
                            ]
                    )

            ParenthesizedBlock (List.append bindings [code])

        | _ -> code

    let afterMap =
        match metaAttributes.MaybeMap with
        | Some (Map(SingleLineExpression collection, SingleLineExpression currExpression, withIndex)) ->
            let mapFnName = if withIndex then "mapi" else "map"

            ParenthesizedBlock [
                Line (sprintf "(%O)" collection)
                Line (sprintf "|> Seq.%s" mapFnName)
                IndentedBlock [
                    Line (sprintf "(fun %O ->" currExpression)
                    IndentedBlock [afterLet]
                    Line ")"
                ]
                Line "|> Array.ofSeq |> castAsElement"
            ]
        | Some (MapOfOption(SingleLineExpression option, someName)) ->
            ParenthesizedBlock [
                Line (sprintf "(%O)" option)
                Line "|> Option.map"
                IndentedBlock [
                    Line (sprintf "(fun %O ->" someName)
                    IndentedBlock [afterLet]
                    Line ")"
                ]
                Line "|> Option.getOrElse noElement"
            ]

        | _ -> afterLet

    let afterIf =
        match metaAttributes.MaybeIf with
        | Some theIf ->
            let (BooleanExpression condition) = theIf.Condition
            ParenthesizedBlock [
                Line (sprintf "if (%s) then" condition)
                IndentedBlock [afterMap]
                Line "else noElement"
            ]
        | _ -> afterMap

    afterIf

let private combineAttributeStringsWithClassName (delimeter: string) (genericAttributeStrings: seq<NonemptyString>) (maybeClassName: Option<NonemptyString>) : string =
    let attributeStrings =
        match maybeClassName with
        | None -> genericAttributeStrings
        // XXX is there really no better way to append a single element?
        | Some classString -> Seq.append genericAttributeStrings [classString]

    attributeStrings
    |> Seq.map (fun v -> v.Value)
    |> String.concat delimeter


type CodeAndLetBindings = {
    Code:             List<FsharpCode>
    OuterLetBindings: Map<LetBinding,  FsharpCode>
    InnerLetBindings: Map<LetBinding,  FsharpCode>
    PropBindings:     Map<PropBinding, FsharpCode>
}
with
    static member Empty : CodeAndLetBindings = {
        Code             = []
        InnerLetBindings = Map.empty
        OuterLetBindings = Map.empty
        PropBindings     = Map.empty
    }

let rec generateForChildren (libAlias: string) (withStyles: bool) (children: List<ReactTemplateNode>) : Result<CodeAndLetBindings, CodeGenerationError> =
    resultful {
        let! childrenCode =
            children
            |> List.map (generateCode libAlias withStyles)
            |> Result.liftFirst

        // not a fan of zip, but it's easier to do this than deal with Result in a fold
        let sourceToCode = List.zip children childrenCode

        let split =
            List.fold
                (fun (acc: CodeAndLetBindings) -> function
                    | (RtProp (propBinding, _), code) ->
                        { acc with PropBindings = acc.PropBindings.Add(propBinding, code) }
                    | (RtOuterLet (name, _), code) ->
                        { acc with OuterLetBindings = acc.OuterLetBindings.Add(LetBinding.Regular name, code) }
                    | (RtLet (name, _), code) ->
                        { acc with InnerLetBindings = acc.InnerLetBindings.Add(LetBinding.Regular name, code) }
                    | (_, code) ->
                        { acc with Code = code :: acc.Code }
                )
                CodeAndLetBindings.Empty
                sourceToCode

        return { split with Code = split.Code |> List.rev }
    }

and generateCode (libAlias: string) (withStyles: bool) (node: ReactTemplateNode) : Result<FsharpCode, CodeGenerationError> = resultful {
    match node with
    | Text value ->
        return Codes (generateForTextNodeValue value)

    | Cdata value ->
        return Codes (generateForCdataNodeValue value)

    | DomNode (name, maybeChildren, attributes, metaAttributes) ->
        let! maybeChildrenGenerateResults =
            match (Parsing.domNodeNamesChildless.Contains name, maybeChildren) with
            | (_,     None)          -> Ok None
            | (true,  Some _)        -> sprintf "DOM node %s expected to have no children, but some were given" name |> CodeGenerationError.CodeGenerationError |> Error
            | (false, Some children) -> (generateForChildren libAlias withStyles children) |> Result.map Some

        return wrapInControlFlow
            None
            metaAttributes
            (Codes [
                Line (sprintf "FRS.%s" name)
                IndentedBlock (
                    List.append
                        [Line (sprintf "[%s]" (combineAttributeStringsWithClassName "; " (generateForDomNodeAttributes attributes) (generateClassNameAttribute attributes metaAttributes.MaybeClass)))]
                        (
                            maybeChildrenGenerateResults
                            |> Option.map (fun childrenGenerateResults ->
                                [wrapChildrenBlock AsElementsWithExtraParentheses None childrenGenerateResults.InnerLetBindings childrenGenerateResults.Code]
                            )
                            |> Option.getOrElse []
                        )
                )
            ])

    | Component (componentLibAlias, nameSpace, name, props, children, metaAttributes) ->
        let fullyQualifiedComponentName = nameSpace + "." + name

        let! childrenGenerateResults = generateForChildren libAlias withStyles children

        let (childrenCode, innerLetBindings, outerLetBindings, propBindings) =
            match metaAttributes.MaybePropChildren with
            | None ->
                (
                    childrenGenerateResults.Code,
                    childrenGenerateResults.InnerLetBindings,
                    childrenGenerateResults.OuterLetBindings,
                    childrenGenerateResults.PropBindings
                )
            | Some propBinding ->
                (
                    [],
                    Map.empty,
                    childrenGenerateResults.OuterLetBindings,
                    childrenGenerateResults.PropBindings.Add(propBinding, (wrapChildrenBlock AsElement (Some fullyQualifiedComponentName) childrenGenerateResults.InnerLetBindings childrenGenerateResults.Code))
                )

        return wrapInControlFlow
            (Some fullyQualifiedComponentName)
            metaAttributes
            (
                // XXX ugh what's a dynamically sized O(1) append data strcture in this language?!
                // I need it for readability, and don't care for "idiomatic functional" crap here.
                let mutable localBindings: List<NonemptyString * FsharpCode> = []

                let maybeClassNameValue = generateClassNameValue props metaAttributes.MaybeClass

                let propsIncludingPropBidings: List<AttributeName * FsharpCode> =
                    let processedProps =
                        props
                        |> List.map (fun (name, expression) -> (name, generateForRawComponentProp fullyQualifiedComponentName (name, expression)))

                    let propBindingProps =
                        propBindings
                        |> Map.toList
                        |> List.map (fun (propBinding, rawCode) ->
                            let innerCodeAsFragment =
                                Codes [
                                    IndentedBlock [rawCode]
                                ]

                            let maybeParameterProcessedCode =
                                match propBinding.MaybeParameters with
                                | None -> innerCodeAsFragment
                                | Some parameters ->
                                    Codes [
                                        Line (sprintf "(fun (%O) ->" (maybeExpandBackreferenceOperator (Some fullyQualifiedComponentName) parameters.Value))
                                        IndentedBlock [innerCodeAsFragment]
                                        Line ")"
                                    ]

                            let maybeTransformsProcessCode =
                                match propBinding.MaybeTransforms with
                                | None -> maybeParameterProcessedCode
                                | Some transforms ->
                                    Codes [
                                        ParenthesizedBlock [
                                            Line (sprintf "%s" (expandBackreferenceOperator fullyQualifiedComponentName transforms.Value))
                                            IndentedBlock [
                                                ParenthesizedBlock [maybeParameterProcessedCode]
                                            ]
                                        ]
                                    ]

                            let maybePolymorphicConstructorProcessedCode =
                                match propBinding.Name.Value.StartsWith "^" with
                                | false -> maybeTransformsProcessCode
                                | true ->
                                    Codes [
                                        Line (polymorphicConstructorName fullyQualifiedComponentName (sanitizedPropName propBinding.Name))
                                        IndentedBlock [maybeTransformsProcessCode]
                                    ]

                            (AttributeName (sanitizedPropName propBinding.Name), maybePolymorphicConstructorProcessedCode)
                        )
                    List.append processedProps propBindingProps

                let letLocalBindings = generateForBindings (Some fullyQualifiedComponentName) outerLetBindings

                localBindings <- List.append localBindings letLocalBindings

                let maybeStyleInjectedProp =
                    match withStyles && not metaAttributes.RtFsharp with
                    | false -> None
                    | true  ->
                        let hasCurrStyles =
                            match maybeClassNameValue with
                            | None -> false
                            | Some classNameValue ->
                                localBindings <- List.append localBindings [
                                    (NonemptyString.ofLiteral "__currClass",  Line classNameValue.Value)
                                    (NonemptyString.ofLiteral "__currStyles", Line "(ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)")
                                ]
                                true

                        // NOTE we include the types here so that the error is given to the caller
                        // as close to the place of error as possible
                        let hasCurrStylesSecondRound =
                            match (metaAttributes.MaybeStyle, metaAttributes.MaybeAniStyle) with
                            | (Some (SingleLineExpression dynamicStylesExpression), None) ->
                                localBindings <- List.append localBindings [
                                    (NonemptyString.ofLiteral "__dynamicStyles: List<ReactXP.LegacyStyles.RuleFunctionReturnedStyleRules>", Line dynamicStylesExpression.Value)
                                    (NonemptyString.ofLiteral "__currStyles", Line ((if hasCurrStyles then "__currStyles @ " else "") + "(ReactXP.LegacyStyles.Designtime.processDynamicStyles __dynamicStyles)"))
                                ]
                                true
                            | (None, Some (SingleLineExpression dynamicAniStylesExpression)) ->
                                localBindings <- List.append localBindings [
                                    (NonemptyString.ofLiteral "__dynamicAniStyles: List<ReactXP.LegacyStyles.RuntimeStyles>", Line dynamicAniStylesExpression.Value)
                                    (NonemptyString.ofLiteral "__currStyles", Line ((if hasCurrStyles then "__currStyles @ " else "") + "(ReactXP.LegacyStyles.Designtime.processDynamicAniStyles __dynamicAniStyles)"))
                                ]
                                true
                            | (Some (SingleLineExpression dynamicStylesExpression), Some (SingleLineExpression dynamicAniStylesExpression)) ->
                                localBindings <- List.append localBindings [
                                    (NonemptyString.ofLiteral "__dynamicStyles: List<ReactXP.LegacyStyles.RuleFunctionReturnedStyleRules>", Line dynamicStylesExpression.Value)
                                    (NonemptyString.ofLiteral "__dynamicAniStyles: List<ReactXP.LegacyStyles.RuntimeStyles>", Line dynamicAniStylesExpression.Value)
                                    (NonemptyString.ofLiteral "__currStyles", Line ((if hasCurrStyles then "__currStyles @ " else "") + "(ReactXP.LegacyStyles.Designtime.processDynamicStyles __dynamicStyles) @ (ReactXP.LegacyStyles.Legacy.processDynamicAniStyles __dynamicAniStyles)"))
                                ]
                                true
                            | (None, None) -> hasCurrStyles

                        let maybeStyleInjectedProp =
                            if hasCurrStylesSecondRound then
                                if nameSpace = "ReactXP.Components" then
                                    (
                                        AttributeName (NonemptyString.ofLiteral "?styles"),
                                        match metaAttributes.MaybeNewStyles with
                                        | None -> Line (sprintf "(if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent \"%s\" __currStyles |> Some) else None)" fullyQualifiedComponentName)
                                        | Some (SingleLineExpression newStylesExpression) ->
                                            ParenthesizedBlock [
                                                Line "let __currProcessedStyles = if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent \"%s\" __currStyles) else [||]"
                                                Line $"match {newStylesExpression.Value} with"
                                                Line "| Some styles ->"
                                                IndentedBlock [
                                                    Line $"Array.append __currProcessedStyles styles |> Some"
                                                ]
                                                Line "| None -> Some __currProcessedStyles"
                                            ]
                                    )
                                else
                                    (
                                        AttributeName (NonemptyString.ofLiteral "?xLegacyStyles"),
                                        Line (sprintf "(if (not __currStyles.IsEmpty) then Some __currStyles else None)")
                                    )
                                |> Some
                            else None

                        maybeStyleInjectedProp

                let localBindingsCodes =
                    localBindings
                    |> List.map
                        (fun (name, value) ->
                            match value with
                            | Line content -> Line (sprintf "let %O = %s" name content)
                            | _ ->
                                Codes [
                                    Line (sprintf "let %O =" name)
                                    IndentedBlock [value]
                                ]
                        )


                let makeComponentCode =
                    let propsIncludingPropBindingsAndStylesAndChildren =
                        if metaAttributes.RtFsharp then
                            propsIncludingPropBidings
                            @ (
                                match childrenCode with
                                | [] -> []
                                | _ ->
                                    let wrappedChildrenCode = wrapChildrenBlock AsElements (Some fullyQualifiedComponentName) innerLetBindings childrenCode
                                    [
                                        (AttributeName (NonemptyString.ofLiteral "children"), wrappedChildrenCode)
                                    ]
                            )
                        else
                            propsIncludingPropBidings
                            @
                            (List.flatten [
                                match maybeStyleInjectedProp with | Some value -> [value] | None -> Noop
                                match childrenCode with
                                | [] -> Noop
                                | _ ->
                                    let wrappedChildrenCode = wrapChildrenBlock AsElements (Some fullyQualifiedComponentName) innerLetBindings childrenCode
                                    [
                                        (AttributeName (NonemptyString.ofLiteral "children"), wrappedChildrenCode)
                                    ]
                            ])

                    let parameters = generateForComponentProps propsIncludingPropBindingsAndStylesAndChildren

                    // NOTE by right we should be refactoring to not have such messes,
                    // but this code will all be going away, so quick hacks are okay.
                    let libAliasWithDefaultRemapped =
                        match componentLibAlias with
                        | "default" -> libAlias
                        | _         -> componentLibAlias

                    // todo it's messy that we have this condition here...
                    match propsIncludingPropBindingsAndStylesAndChildren |> List.exists (fun (AttributeName name, _) -> speciallyTreatedAttributeNames.DoesNotContain name) with
                    | false -> Line (sprintf "%s.Constructors.%s.%s()" nameSpace libAliasWithDefaultRemapped name)
                    | true ->
                        Codes [
                            Line (sprintf "%s.Constructors.%s.%s(" nameSpace libAliasWithDefaultRemapped name)
                            IndentedBlock [parameters]
                            Line ")"
                        ]

                let lines =
                    Line ("let __parentFQN = Some \"" + fullyQualifiedComponentName + "\"")
                    ::
                    localBindingsCodes
                    @
                    [
                        makeComponentCode
                    ]

                Codes lines
            )

    | RtBlock (children, metaAttributes) ->
        let! childrenGenerateResults = generateForChildren libAlias withStyles children

        return wrapInControlFlow
            None
            metaAttributes
            (Codes [
                (wrapChildrenBlock AsElement None childrenGenerateResults.InnerLetBindings childrenGenerateResults.Code)
            ])

    | RtRoot { Children = children } ->
        match children with
        | [ onlyChild ] ->
            return! generateCode libAlias withStyles onlyChild

        | _ ->
            let! childrenGenerateResults = generateForChildren libAlias withStyles children

            return
                Codes [
                    (wrapChildrenBlock AsElement None childrenGenerateResults.InnerLetBindings childrenGenerateResults.Code)
                ]

    | RtMatch (SingleLineExpression what, cases, metaAttributes) ->
        let! caseChildrenCode =
            List.map
                (fun (_, children, _) ->
                    generateForChildren libAlias withStyles children
                )
                cases
            |> Result.liftFirst
        let casesCode =
            cases
            |> List.mapi
                (fun index (SingleLineExpression expression, _, metaAttributes) ->
                    let innerCode =
                        let caseCode = caseChildrenCode.[index].Code
                        match caseCode |> List.filter (function CommentLine _ -> false | _ -> true) with
                        | [] -> (Line "[||]") :: caseCode
                        | _ ->
                            [
                                wrapInControlFlow
                                    None
                                    metaAttributes
                                    (Codes [
                                        (wrapChildrenBlock AsElements None caseChildrenCode.[index].InnerLetBindings caseCode)
                                    ])
                            ]

                    Codes [
                        Line (sprintf "| %O ->" expression)
                        IndentedBlock innerCode
                    ]
                )
        return wrapInControlFlow
            None
            metaAttributes
            (Codes [
                Line (sprintf "match (%O) with" what)
                Codes casesCode
                Line "|> castAsElementAckingKeysWarning"
            ])

    | RtProp (_, children)
    | RtOuterLet (_, children) ->
        let! childrenGenerateResults = generateForChildren libAlias withStyles children
        return wrapChildrenBlock AsElement None childrenGenerateResults.InnerLetBindings childrenGenerateResults.Code

    | RtLet (_, children) ->
        let! childrenGenerateResults = generateForChildren libAlias withStyles children
        return wrapChildrenBlock AsElement None childrenGenerateResults.InnerLetBindings childrenGenerateResults.Code

    | Comment(value) ->
        return CommentLine (sprintf "(* %s *)" value)

    | RtSharp (NonemptyString value) ->
        return Line ("#" + value)

    | _ -> return Line "(* other codez not yet implemented *)"
}
