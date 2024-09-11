[<AutoOpen>]
module LibClient.LegacyTextWrapping

open ReactXP.Components

let private isText (maybeParentFullyQualifiedName: Option<string>) : bool =
    match maybeParentFullyQualifiedName with
    | Some "ReactXP.Components.Text"
    | Some "ReactXP.Components.UiText"
    | Some "ReactXP.Components.AniText"
    | Some "ReactXP.Components.AniUiText"
    | Some "LibClient.Components.Text"
    | Some "LibClient.Components.LegacyText"
    | Some "LibClient.Components.UiText"
    | Some "LibClient.Components.LegacyUiText" -> true
    | _ -> false

let mutable wrapRawText : string -> Fable.React.ReactElement = fun (text: string) ->
    RX.Text(children = [|Fable.React.Helpers.str text|])

let makeTextNode (text: string) (_siblingIndex: int, _siblingCount: int, maybeParentFullyQualifiedName: Option<string>) : Fable.React.ReactElement =
    if isText maybeParentFullyQualifiedName then
        Fable.React.Helpers.str text
    else
        wrapRawText text

let makeTextNode2 (maybeParentFullyQualifiedName: Option<string>) (text: string) : Fable.React.ReactElement =
    if isText maybeParentFullyQualifiedName then
        Fable.React.Helpers.str text
    else
        wrapRawText text
