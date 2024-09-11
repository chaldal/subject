module RenderDSLServer.XML

open LSP.Types

let leadingTagNameRegex = System.Text.RegularExpressions.Regex(@"^<([a-zA-Z\.\-]+)(( |>|/>).*$|$)")
let getTagNameAtPositionOnLine (index: int) (line: string) : Option<string> =
    match line.Substring(0, index + 1).LastIndexOf '<' with
    | -1            -> None
    | tagStartIndex ->
        let sublineStartingAtTag = line.Substring(tagStartIndex)
        let regexMatch = leadingTagNameRegex.Match sublineStartingAtTag
        match regexMatch.Success with
        | true  -> Some (regexMatch.Groups.Item(1).Value)
        | false -> None

// super-low performance version for now, does string analysis each time
let getTagNameAtPosition (positionParams: TextDocumentPositionParams) (source: string) : Option<string> =
    source.Split("\n")
    |> Array.tryItem positionParams.position.line
    |> Option.flatMap (getTagNameAtPositionOnLine positionParams.position.character)
