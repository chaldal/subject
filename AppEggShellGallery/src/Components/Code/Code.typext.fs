module AppEggShellGallery.Components.Code

open LibClient
open Fable.Core.JsInterop
open Fable.Core
open ThirdParty.SyntaxHighlighter.Components

// aliases for ~ access
let Render = SyntaxHighlighter.Language.Render
let Fsharp = SyntaxHighlighter.Language.Fsharp


type Props = (* GenerateMakeFunction *) {
    Language:               SyntaxHighlighter.Language
    Heading:                string option              // defaultWithAutoWrap None
    StripLeadingWhitespace: bool                       // default true


    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate = {
    ProcessedSource: Result<string, string>
}

// for the life of me I couldn't find access to the JS `typeof` operator
let private isJsRuntimeString (value: obj) : bool =
    value?toLocaleLowerCase <> JS.undefined

// naive implementation, assumes that the first line's leading
// whitespace is what should be stripped from every line
let private stripLeadingWhitespace (source: string) : string =
    let lines =
        source.Split "\n"
        |> List.ofArray
        |> List.skipWhile (fun line -> line.TrimStart().Length = 0)
        |> List.rev
        |> List.skipWhile (fun line -> line.TrimStart().Length = 0)
        |> List.rev
        // FIXME yes, I'm really not inspired right now and can't think
        // of an easier way to trim from the end

    match lines with
    | [] -> source
    | head :: _ ->
        let headLeadingWhitespace = head.Substring (0, head.Length - head.TrimStart().Length)
        match headLeadingWhitespace with
        | "" -> source
        | _ ->
            // Don't think we can make this tail recursive...
            let rec trimStartLines (remainingLines: List<string>) : Result<List<string>, unit> =
                match remainingLines with
                | [] -> Ok []
                | head :: tail ->
                    match head = "" || head.StartsWith headLeadingWhitespace with
                    | true  ->
                        trimStartLines tail
                        |> Result.map (fun trimmedTail ->
                            let trimmedHead = if head = "" then "" else head.Substring headLeadingWhitespace.Length
                            trimmedHead :: trimmedTail
                        )
                    | false ->
                        // we hit a line that doesn't share the leading whitespace
                        // of the first line, so abort the whole attempt to strip
                        // leading whitespace
                        Error ()

            trimStartLines lines
            |> Result.toOption
            |> Option.getOrElse lines
            |> String.concat "\n"

type Code(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, Code>("AppEggShellGallery.Components.Code", _initialProps, Actions, hasStyles = true)

    let rec tryExtractStringChildren (props: obj) (maxDepth: int) : Result<string, string> =
        let stringChildren = (props?children :> obj :?> obj[]).[0]?props?children
        if isJsRuntimeString stringChildren then
            Ok stringChildren
        else if maxDepth > 0 && stringChildren?props <> JS.undefined then
            tryExtractStringChildren (stringChildren?props) (maxDepth - 1)
        else
            Error "children are of an unknown type"

    override _.GetInitialEstate(initialProps: Props) : Estate =
        let processedSourceMaybeStripped =
            tryExtractStringChildren initialProps (* maxDepth *) 3
            |> Result.map (fun source ->
                match initialProps.StripLeadingWhitespace with
                | true  -> stripLeadingWhitespace source
                | false -> source
            )

        {
            ProcessedSource = processedSourceMaybeStripped
        }

and Actions(_this: Code) =
    class end

let Make = makeConstructor<Code, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate
