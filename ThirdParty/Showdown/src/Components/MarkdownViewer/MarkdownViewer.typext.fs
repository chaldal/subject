module ThirdParty.Showdown.Components.MarkdownViewer

// NOTE there are some project-specific tweaks here. Ideally they would sit in the
// project that uses this component, instead of the third party library, but I simply
// wanted to pull out the component as a sample third party wrapper, and didn't want
// to do a full cleanup refactoring.

open LibClient
open LibClient.ServiceInstances
open Fable.Core.JsInterop
open LibClient.Services.HttpService.Types

let private showdown: obj = importDefault "showdown"
let defaultShowdownConverter: obj = createNew showdown?Converter ()

let makeCustomShowdownConverter (config: obj) : obj =
    createNew showdown?Converter config

type Source =
| Url  of string
| Code of string

type Props = (* GenerateMakeFunction *) {
    Source:              Source
    GlobalLinkHandler:   string option                       // defaultWithAutoWrap None
    ImageUrlTransformer: (Source -> string -> string) option // defaultWithAutoWrap None
    ShowdownConverter:   obj                                 // default ThirdParty.Showdown.Components.MarkdownViewer.defaultShowdownConverter
}

type Estate = {
    SourceCode: AsyncData<string>
}

let makeHtml (converter: obj) (maybeGlobalLinkHandler: Option<string>) (maybeImageUrlTransformer: Option<string -> string>) (source: string) : ReactElement =
    let rawHtml: string = converter?makeHtml source

    let linksProcessedHtml =
        match maybeGlobalLinkHandler with
        | None -> rawHtml
        | Some globalLinkHandler ->
            // NOTE this regex needs to have .* as the children of the <a>, because we want
            // to support things like <a><code>SomeToken</code></a>. It seems that by default
            // the regex is acting in an ungreedy way, so we're lucky here.
            let regex = System.Text.RegularExpressions.Regex """<a href="([^"]*)">(.*)</a>"""
            regex.Replace(rawHtml, "<a class='markdown-global-link' onclick=\"" + globalLinkHandler + "(event, '$1')\">$2</a>")

    let imagesProcessedHtml =
        match maybeImageUrlTransformer with
        | None -> linksProcessedHtml
        | Some imageUrlTransformer ->
            // NOTE this regex needs to have .* as the children of the <a>, because we want
            // to support things like <a><code>SomeToken</code></a>. It seems that by default
            // the regex is acting in an ungreedy way, so we're lucky here.
            let regex = System.Text.RegularExpressions.Regex """(<img .*src=")([^"]*)(".*/>)"""
            regex.Replace(linksProcessedHtml, System.Text.RegularExpressions.MatchEvaluator(fun theMatch ->
                let beforeUrl = theMatch.Groups.Item(1).Value
                let url       = theMatch.Groups.Item(2).Value
                let afterUrl  = theMatch.Groups.Item(3).Value
                beforeUrl + (imageUrlTransformer url) + afterUrl
            ))

    let props = createObj [
        "style" ==> createObj [
            // HACK we need this because ReactXP seems to add a "white-space: pre-wrap" to all
            // text elements, and there's no way to override it using the styles system.
            "whiteSpace"       ==> "normal"
            "cursor"           ==> "text"
            "WebkitUserSelect" ==> "text"
            "MozUserSelect"    ==> "text"
            "msUserSelect"     ==> "text"
            "userSelect"       ==> "text"
            "wordWrap"         ==> "break-word"
            "maxWidth"         ==> "800px"
            "lineHeight"       ==> "1.5"
        ]
        "dangerouslySetInnerHTML" ==> createObj [
            "__html" ==> imagesProcessedHtml
        ]
    ]

    Fable.React.ReactBindings.React.createElement("div", props, [])

type MarkdownViewer(initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, MarkdownViewer>("ThirdParty.Showdown.Components.MarkdownViewer", initialProps, Actions, hasStyles = true)

    override this.GetInitialEstate(initialProps: Props) =
        let source =
            match initialProps.Source with
            | Url  _      -> WillStartFetchingSoonHack
            | Code source -> Available source

        { SourceCode = source }

    member private this.SetEstateFromProps(props: Props) =
        this.SetEstate (fun _ _ -> this.GetInitialEstate props)

    override this.ComponentWillReceiveProps(nextProps: Props) =
        this.SetEstateFromProps  nextProps
        this.MaybeFetchFromProps nextProps

    override this.ComponentDidMount() =
        this.MaybeFetchFromProps this.props

    member private this.MaybeFetchFromProps (props: Props) : unit =
        match props.Source with
        | Url url -> Some url
        | Code _  -> None
        |> Option.sideEffect (fun url ->
            async {
                let requestOptions = createObj [
                    "acceptType"         ==> "text/plain"
                    "customResponseType" ==> "text"
                ]
                let! response = services().Http.RequestReactXPRaw url HttpAction.Get (Some requestOptions)
                this.SetEstate (fun estate _ -> { estate with SourceCode = Available (response.body :?> string) })
            } |> startSafely
        )

and Actions(_this: MarkdownViewer) =
    class end

let Make = makeConstructor<MarkdownViewer, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate
