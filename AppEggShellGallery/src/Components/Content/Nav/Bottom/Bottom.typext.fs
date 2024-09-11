module AppEggShellGallery.Components.Content.Nav.Bottom

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Top(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Top>("AppEggShellGallery.Components.Content.Nav.Bottom", _initialProps, Actions, hasStyles = true)

and Actions(_this: Top) =
    class end

let Make = makeConstructor<Top, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
