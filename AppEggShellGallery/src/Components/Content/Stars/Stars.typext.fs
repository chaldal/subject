module AppEggShellGallery.Components.Content.Stars

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Stars(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Stars>("AppEggShellGallery.Components.Content.Stars", _initialProps, Actions, hasStyles = true)

and Actions(_this: Stars) =
    class end

let Make = makeConstructor<Stars, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
