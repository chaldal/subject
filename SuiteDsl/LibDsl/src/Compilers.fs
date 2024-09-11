module LibDsl.Compilers

open System.Xml

open LibDsl.Parsing.XmlParsing

[<AbstractClass>]
type DslCompiler<'InputParams, 'ParsedCode, 'ParseError, 'OutputCode, 'CodeGenerationResult, 'CodeGenerationError> (_inputParams: 'InputParams) =
    abstract member Parse : (* source *) string -> Result<'ParsedCode, 'ParseError>
    abstract member GenerateCode : (* parsedCode *) 'ParsedCode -> Result<'OutputCode, 'CodeGenerationError>
    abstract member CodeToFiles : 'OutputCode -> Result<'CodeGenerationResult, 'CodeGenerationError>

and OneFile   = OneFile of Contents: string
and ManyFiles = ManyFiles of Map<Filename, string>

and Filename = Filename of string

[<AbstractClass>]
type XmlDslCompiler<'InputParams, 'ParsedCode, 'ParseError, 'OutputCode, 'CodeGenerationResult, 'CodeGenerationError> (inputParams: 'InputParams) =
    inherit DslCompiler<'InputParams, 'ParsedCode, XmlDslParseError<'ParseError>, 'OutputCode, 'CodeGenerationResult, 'CodeGenerationError> (inputParams)

    override this.Parse (source: string) : Result<'ParsedCode, XmlDslParseError<'ParseError>> = resultful {
        let! xmlRootNode =
            toXmlTree source
            |> Result.mapError XmlSyntaxError
        return!
            this.ParseXml xmlRootNode
            |> Result.mapError ParseError
    }

    abstract member ParseXml : (* sourceRootNode *) XmlNode -> Result<'ParsedCode, 'ParseError>

and XmlDslParseError<'ParseError> =
| XmlSyntaxError of Message: string
| ParseError     of 'ParseError

