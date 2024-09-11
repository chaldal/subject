module AppEggShellGallery.Components.Content.Icons

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Icons(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Icons>("AppEggShellGallery.Components.Content.Icons", _initialProps, Actions, hasStyles = true)

and Actions(_this: Icons) =
    class end

let Make = makeConstructor<Icons, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
