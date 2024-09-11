module AppEggShellGallery.Components.Content.IconButton

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type IconButton(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, IconButton>("AppEggShellGallery.Components.Content.IconButton", _initialProps, Actions, hasStyles = true)

and Actions(_this: IconButton) =
    class end

let Make = makeConstructor<IconButton, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
