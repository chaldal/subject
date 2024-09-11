module AppEggShellGallery.RenderHelpers

open LibClient
open Fable.Core.JsInterop

let showdownConverterWithSyntaxHighlighting =
    ThirdParty.Showdown.Components.MarkdownViewer.makeCustomShowdownConverter
        (createObj [
            "extensions" ==> [|importDefault "showdown-highlight"|]
        ])

let markdownImageUrlTransformer (source: ThirdParty.Showdown.Components.MarkdownViewer.Source) (imageUrl: string) : string =
    if imageUrl.StartsWith "./" then
        match source with
        | ThirdParty.Showdown.Components.MarkdownViewer.Source.Url documentUrl ->
            let parts = documentUrl.Split "/" |> List.ofArray
            List.append (parts |> List.take (parts.Length - 1)) [imageUrl.Substring 2]
            |> String.concat "/"
        | _ -> imageUrl
    else
        imageUrl
