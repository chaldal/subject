module AppEggShellGallery.Components.Content.Card

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Card(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Card>("AppEggShellGallery.Components.Content.Card", _initialProps, Actions, hasStyles = true)

and Actions(_this: Card) =
    class end

let Make = makeConstructor<Card, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
