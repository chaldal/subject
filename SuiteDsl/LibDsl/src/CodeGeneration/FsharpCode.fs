namespace LibDsl.CodeGeneration.FsharpCode

type FsharpCode =
| Nothing // sugar for building lists, lest we are forced to use concat and nested lists
| Line of string
| CommentLine of string
| Codes of List<FsharpCode>
| ParameterList of List<FsharpCode>
| IndentedBlock of children: List<FsharpCode>
| ParenthesizedBlock of children: List<FsharpCode>
| RecordBlock of fields: List<string * string> // being lazy, not doing (string * Code) until need arises

module FsharpCode =
    let private makeIndent (indentCount: int): string = String.replicate indentCount "    "

    let rec private codeToStringHelper (indent: int) (code: FsharpCode) : string =
        let indentString = makeIndent indent
        match code with
        | Nothing -> failwith "We do not expect Nothing to be used in anything other than lists, from which it should be filtered out"
        | Line line ->
            sprintf "%s%s" indentString line
        | CommentLine line ->
            sprintf "%s%s" indentString line
        | Codes codes ->
            childrenCodeToString indent codes
        | ParameterList children ->
            children
            |> List.without Nothing
            |> List.map (codeToStringHelper indent)
            |> List.mapi (fun index child -> if index < children.Length - 1 then child + "," else child)
            |> String.concat "\n"
        | IndentedBlock children ->
            childrenCodeToString (indent + 1) children
        | ParenthesizedBlock children ->
            let childrenString = childrenCodeToString (indent + 1) children
            sprintf "%s(\n%s\n%s)" indentString childrenString indentString
        | RecordBlock children ->
            let childrenLines =
                children
                |> List.map (fun (name, value) -> Line (sprintf "%s = %s" name value))
            let childrenString = childrenCodeToString (indent + 1) childrenLines
            sprintf "%s{\n%s\n%s}" indentString childrenString indentString

    and private childrenCodeToString (indent: int) (children: List<FsharpCode>) : string =
        children
        |> List.without Nothing
        |> List.map (codeToStringHelper indent)
        |> String.concat "\n"

    let codeToString (initialIndent: int) (code: FsharpCode) : string =
        codeToStringHelper initialIndent code

    let rec mapLines (mapper: string -> string) (code: FsharpCode) : FsharpCode =
        match code with
        | Line source              -> Line (mapper source)
        | Codes codes              -> Codes (codes |> List.map (mapLines mapper))
        | IndentedBlock codes      -> IndentedBlock (codes |> List.map (mapLines mapper))
        | ParenthesizedBlock codes -> ParenthesizedBlock (codes |> List.map (mapLines mapper))
        | RecordBlock fields       -> RecordBlock (fields |> List.map (fun (name, source) -> (name, mapper source)))
        | _ -> code
