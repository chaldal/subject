module AppEggShellGallery.Components.Snippets

open LibClient

type Scope =
| All
| One of string
with
    member this.Filter (snippet: Scraping.Types.Snippet) : bool =
        match this with
        | All       -> true
        | One scope -> snippet.Scope = scope

type Props = (* GenerateMakeFunction *) {
    Scope: Scope // default Scope.All

    key: string option // defaultWithAutoWrap JsUndefined
}

type Snippets(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Snippets>("AppEggShellGallery.Components.Snippets", _initialProps, Actions, hasStyles = true)

and Actions(_this: Snippets) =
    class end

let Make = makeConstructor<Snippets, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
