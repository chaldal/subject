module RenderDSLServer.Hovers

open LibLangFsharp

open LSP.Types
type Session       = Session.Session
type DocumentStore = LSP.DocumentStore
let dprintfn       = LSP.Log.dprintfn
type TaggedRecordField = LibRenderDSL.RecordsWithDefaults.TaggedRecordField
open StringFormatting

let private rtCaseContent = String.concat "\n" [
    "# Pattern Matching"
    ""
    "Translates directly to the F# `match expression with` syntax, with the `rt-match`'s `what` attribute being the `expression`, and the `rt-case`'s `is` attribute being dropped as is into the `| caseExpression ->` line."
    ""
    "Example:"
    ""
    "```renderdsl"
    "<rt-match what='maybeUser'>"
    "    <rt-case is='None'>no user</rt-case>"
    "    <rt-case is='Some Anonymous'>anonymous user</rt-case>"
    "    <rt-case is='Some (LoggedIn username)'>Logged in user {username}</rt-case>"
    "</rt-match>"
    "```"
]

let private rtPropContent = String.concat "\n" [
    "# Passing Fragment Through Props"
    ""
    "An `rt-prop` element lets you pass a React fragment as a prop to the parent element."
    ""
    "All `rt-prop` elements are excluded from the elements passed as the `children` prop."
    ""
    "The `name` attribute is either a string, in which case the receiving component's prop is expected to be of type `ReactElement`, or it is of the form `PropName(paramList)` in which case the prop `PropName` on the receiving component must have type `(paramTypeList) -> ReactElement`"
    ""
    "If you want to send all children as a named prop (and thus avoid having an extra `rt-prop` tag wrapping anything, you can specify the `rt-prop-children` attribute on the receiving element, with the same syntax as `rt-prop`'s `name` attribute."
    ""
    "Example (contrived):"
    ""
    "```renderdsl"
    "<LC.AsyncData Data='pstate.AsyncSomething' rt-prop-children='WhenAvailable(something: Something)'>"
    "    <LC.Pre>{jsonStringify 4 something}</LC.Pre>"
    "    <rt-prop name='WhenFetching(maybeOldSomething: Option<Something>)'>"
    "        loading..."
    "    </rt-prop>"
    "    <rt-prop name='WhenElse'>"
    "        not available or error or something"
    "    </rt-prop>"
    "</LC.AsyncData>"
    "```"
]

let private isRecordTypeProps (theType: LibRenderDSL.RecordsWithDefaults.TaggedRecordType) : bool =
    // HACK for now, AsyncData is a typed type... should have a regex or something
    theType.name = "Props" || theType.name = "Props<'T>"

let private tryGetComponentPropsHover (documentStore: DocumentStore) (session: Session) (tag: string) : Async<Result<List<MarkedString>, string>> = asyncResult {
    let! componentName = session.GetComponentName tag
    let! filename = session.GetFilenameForComponent componentName

    let! maybeContents = Files.getTextFromStoreOrFilesystemIfNotOpen documentStore filename

    match maybeContents with
    | None ->
        dprintfn "We got a supposedly existing filename, but couldn't read its content %s" filename
        return! Error (sprintf "We resolved %s tag to a filename %s, but failed to fetch its content" tag filename)

    | Some contents ->
        let! taggedRecordTypes = LibRenderDSL.RecordsWithDefaults.extractTaggedRecordTypes contents

        match taggedRecordTypes |> List.tryFind isRecordTypeProps with
        | None -> return! Error (sprintf "No Props type found for %s" tag)
        | Some propsRecordType ->
            let propParts: List<string * string * string> =
                propsRecordType.fields
                |> List.map (function
                    | TaggedRecordField.Regular(name, theType)                             -> (name, theType, "")
                    | TaggedRecordField.WithDefault(name, theType, theDefault)             -> (name, theType, "(dflt " + theDefault + ")")
                    | TaggedRecordField.WithDefaultAutoWrapSome(name, theType, theDefault) -> (name, theType, "(opt, dflt " + theDefault + ")")
                )

            let (nameMaxLength, theTypeMaxLength, defaultsInfoMaxLength) = findMaxes propParts |> Option.getOrElse (0, 0, 0)
            let formattedProps =
                propParts
                |> List.map
                    (fun (name, theType, defaultsInfo) ->
                        (pad name nameMaxLength) + "  " + (pad theType theTypeMaxLength) + "  " + (pad defaultsInfo defaultsInfoMaxLength)
                    )

            return [PlainString (String.concat "\n" [
                sprintf "### %s" tag
                ""
                "```"
                String.concat "\n" formattedProps
                "```"
            ])]
    }

let getComponentPropsHover (documentStore: DocumentStore) (session: Session) (tag: string) : Async<List<MarkedString>> = async {
    match! tryGetComponentPropsHover documentStore session tag with
    | Ok message -> return message
    | Error e ->
        return [PlainString (String.concat "\n" [
            "Error"
            ""
            e
        ])]
}

let getHoverContent (documentStore: DocumentStore) (session: Session) (tag: string) : Async<List<MarkedString>> =
    match tag with
    | "rt-match"
    | "rt-case" -> async { return [PlainString rtCaseContent] }
    | "rt-prop" -> async { return [PlainString rtPropContent] }
    | otherTag -> getComponentPropsHover documentStore session otherTag
