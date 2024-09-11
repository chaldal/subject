module LibRenderDSL.RecordsWithDefaults

open System.Text.RegularExpressions
open LibLang
open LibRenderDSL.Types

let private newlineRegex         = Regex("[" + System.Environment.NewLine + "]")
let private taggedTypeStartRegex = Regex(@"^(type|and) ([a-zA-Z]+(<.+>)?) = \(\* GenerateMakeFunction \*\) {$")
let private taggedTypeEndRegex   = Regex(@"^}(\s*with)?$")
let private taggedTypeFieldRegex = Regex("^    ([a-zA-Z]+)\\s*:\\s*(.+?)( // (default|defaultWithAutoWrap) (.*))?$")
let private commentOnlyLineRegex = Regex(@"^\s*//.*$")

let extractName (line: string) : Result<string, string> =
    let theMatch = taggedTypeStartRegex.Match line
    match theMatch.Success with
    | false -> Error (sprintf "Cannot extract type name, line doesn't match expected format: %s" line)
    | true  -> Ok (theMatch.Groups.Item(2).Value)

let private optionTypeRegex = Regex("(Option<(.*)>|(.*) option)")
let unwrapOption (wrappedType: string): Result<string, string> =
    let theMatch = optionTypeRegex.Match wrappedType
    match theMatch.Success with
    | false -> Error (sprintf "Cannot unwrap what is expected to be an option type: %s" wrappedType)
    | true ->
        match (theMatch.Groups.Item(2).Value.Trim(), theMatch.Groups.Item(3).Value.Trim()) with
        | (t, "") -> Ok t
        | ("", t) -> Ok t
        | _ -> failwith (sprintf "We must have screwed up the optionTypeRegex, input was: %s" wrappedType)

let parseField (line: string) : Result<Option<TaggedRecordField>, string> =
    let theMatch = taggedTypeFieldRegex.Match line
    match theMatch.Success with
    | false ->
        match commentOnlyLineRegex.IsMatch line with
        | true  -> Ok None
        | false -> Error (sprintf "Record field line malformed: %s" line)

    | true ->
        let name     = theMatch.Groups.Item(1).Value
        let theType  = theMatch.Groups.Item(2).Value.Trim()
        let autoWrap = theMatch.Groups.Item(4).Value = "defaultWithAutoWrap"
        let maybeDefaultValue = theMatch.Groups.Item(5).Value.Trim()
        match maybeDefaultValue with
        | "" -> Ok (Some (Regular(name, theType)))
        | defaultValue ->
            match autoWrap with
            | false -> Ok (Some (WithDefault(name, theType, defaultValue)))
            | true ->
                resultful {
                    let! unwrappedType = unwrapOption theType
                    return Some (WithDefaultAutoWrapSome(name, unwrappedType, defaultValue))
                }

let parseTaggedRecordType (lines: list<string>): Result<TaggedRecordType, string> =
    resultful {
        match lines.Length > 2 with
        | false -> return! Error (sprintf "Seems like an empty record: %A" lines)
        | true ->
            let! name = extractName lines.Head
            let! fields =
                lines.[1 .. lines.Length - 2]
                |> List.filterNot (fun s -> s.Trim().Length = 0)
                |> List.map parseField
                |> Result.liftFirst

            return {
                Name   = name
                Fields = fields |> Option.flattenList
            }
    }

let extractTaggedRecordType (lines: list<string>): Result<Option<TaggedRecordType> * list<string>, string> =
    resultful {
        match lines |> Seq.tryFindIndex taggedTypeStartRegex.IsMatch with
        | None -> return (None, List.empty)
        | Some startIndex ->
            let linesFromStartOfCurrentType = List.skip startIndex lines
            match linesFromStartOfCurrentType |> Seq.tryFindIndex taggedTypeEndRegex.IsMatch with
            | None -> return! Error (sprintf "Had a start of a record type (%s) but no end" lines.[startIndex])
            | Some endIndex ->
                let currentTypeLines = List.take (endIndex + 1) linesFromStartOfCurrentType
                let! currentType = parseTaggedRecordType currentTypeLines
                return (Some currentType, List.skip endIndex linesFromStartOfCurrentType)
    }

let rec extractTaggedRecordTypeHelper (lines: list<string>): Result<List<TaggedRecordType>, string> =
    resultful {
        let! result = extractTaggedRecordType lines
        match result with
        | (None, []) -> return []
        | (Some taggedRecordType, remainingLines) ->
            let! remainderResult = extractTaggedRecordTypeHelper remainingLines
            return taggedRecordType :: remainderResult
        | (None, remainingLines) -> return! Error (sprintf "If no result is returned, all lines should have been consumed, but were not: %A" remainingLines)
    }

let extractTaggedRecordTypes (source: string): Result<List<TaggedRecordType>, string> =
    newlineRegex.Split source
    |> List.ofArray
    |> extractTaggedRecordTypeHelper


let toLines (source: string): List<string> =
    newlineRegex.Split source
    |> List.ofArray