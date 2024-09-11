module AppEggShellGallery.Components.Content.FloatingActionButton

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type FloatingActionButton(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, FloatingActionButton>("AppEggShellGallery.Components.Content.FloatingActionButton", _initialProps, Actions, hasStyles = true)

and Actions(_this: FloatingActionButton) =
    class end

let Make = makeConstructor<FloatingActionButton, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
