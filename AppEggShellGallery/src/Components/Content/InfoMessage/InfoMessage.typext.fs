module AppEggShellGallery.Components.Content.InfoMessage

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type InfoMessage(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, InfoMessage>("AppEggShellGallery.Components.Content.InfoMessage", _initialProps, Actions, hasStyles = true)

and Actions(_this: InfoMessage) =
    class end

let Make = makeConstructor<InfoMessage, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
