module AppEggShellGallery.Components.Content.Button

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Button(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Button>("AppEggShellGallery.Components.Content.Button", _initialProps, Actions, hasStyles = true)

and Actions(_this: Button) =
    class end

let Make = makeConstructor<Button, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
