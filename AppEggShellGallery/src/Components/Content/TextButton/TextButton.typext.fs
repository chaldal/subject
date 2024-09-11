module AppEggShellGallery.Components.Content.TextButton

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type TextButton(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, TextButton>("AppEggShellGallery.Components.Content.TextButton", _initialProps, Actions, hasStyles = true)

and Actions(_this: TextButton) =
    class end

let Make = makeConstructor<TextButton, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate