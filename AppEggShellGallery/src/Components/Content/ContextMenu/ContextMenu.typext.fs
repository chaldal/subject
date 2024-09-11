module AppEggShellGallery.Components.Content.ContextMenu

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type PolitenessLevel =
| Polite
| Impolite
| Mean

let cart = {|
    ItemCount = 3
    IsEmpty   = false
|}

let politenessLevel = Polite

type ContextMenu(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, ContextMenu>("AppEggShellGallery.Components.Content.ContextMenu", _initialProps, Actions, hasStyles = true)

and Actions(_this: ContextMenu) =
    class end

let Make = makeConstructor<ContextMenu, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
