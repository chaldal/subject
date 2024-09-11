module LibDsl.Parsing.XmlParsing

open System.Xml

let private domNodeNameCommentRegex = System.Text.RegularExpressions.Regex("^.*#comment$")

let isCommentNodeName (nodeName: string) : bool =
    domNodeNameCommentRegex.IsMatch nodeName

let toXmlDocAndTree (source: string) : Result<XmlDocument * XmlNode, string> =
    try
        let doc = XmlDocument()
        doc.LoadXml source

        let rootNode =
            doc.ChildNodes
            |> Seq.cast<XmlNode>
            |> Seq.find (fun node -> node.Name |> isCommentNodeName |> not && node.Name <> "xml")

        Ok (doc, rootNode)
    with
    | error -> Error error.Message

let toXmlTree (source: string) : Result<XmlNode, string> =
    toXmlDocAndTree source |> Result.map snd

type XmlNode with
    member this.AttributesSeq : seq<XmlAttribute> =
        match isNull this.Attributes with
        | true  -> Seq.empty
        | false -> this.Attributes |> Seq.cast<XmlAttribute>
