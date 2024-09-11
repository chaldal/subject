[<AutoOpen>]
module ThirdParty.FormatfulText.Components.RenderFormatfulText

// All these modules are required for this component to work in both web and native
open Fable.React
open Fable.Core.JsInterop
open ReactXP.Styles
open FormatfulText.TextService

#if EGGSHELL_PLATFORM_IS_WEB
open LibClient.Components
#endif

type FormatfulText with
    [<Component>]
    static member Render (
        text: FormatfulTextSource,
        ?styles: array<TextStyles>
    ): ReactElement =
        let htmlText =
            match text with
            | FormatfulTextSource.Html          source        -> source
            | FormatfulTextSource.MaybeMarkdown (_, rendered) -> rendered

        #if EGGSHELL_PLATFORM_IS_WEB
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
            ]
            "className"               ==> "style-hack-if-it-contains-a-pre-tag"
            "dangerouslySetInnerHTML" ==> createObj [
                "__html" ==> htmlText
            ]
        ]

        LC.Text (?styles = styles, children = [|
            Fable.React.ReactBindings.React.createElement("div", props, [])
        |])

        #else

        let renderHtmlRaw: obj = import "default" "react-native-render-html"

        let source =
            createObj [
                "html" ==> $"<div>{htmlText}</div>"
            ]

        let containerStyles : obj =
            styles
            |> Option.map
                (fun styles ->
                    styles
                    |> Array.fold
                        (fun accStyles currStyles ->
                            let currStyleEntries = Fable.Core.JS.Constructors.Object?entries(currStyles :> obj) :> obj :?> (string * obj)[]

                            [
                                yield! accStyles
                                yield! currStyleEntries
                            ]
                        )
                        [ "fontFamily" ==> "Montserrat" ]
                )
            |> Option.defaultValue []
            |> createObj

        let tagsStyles =
            createObj [
                "div" ==> containerStyles
            ]

        let props =
            createObj [
                "source"      ==> source
                "tagsStyles"  ==> tagsStyles
                "systemFonts" ==> [|"Montserrat"|]
            ]

        Fable.React.ReactBindings.React.createElement(
            renderHtmlRaw,
            props,
            [||]
        )

        #endif
