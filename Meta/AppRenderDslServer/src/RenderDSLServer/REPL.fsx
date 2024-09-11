#r "./bin/Debug/netcoreapp2.0/osx.10.11-x64/RenderDSLServer.dll"

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

let result = RenderDSLServer.XML.getTagNameAtPositionOnLine 0  "<div>"
printf "%O" result
