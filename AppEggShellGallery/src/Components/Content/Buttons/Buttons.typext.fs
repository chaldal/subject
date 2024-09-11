module AppEggShellGallery.Components.Content.Buttons

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Buttons(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Buttons>("AppEggShellGallery.Components.Content.Buttons", _initialProps, Actions, hasStyles = true)

and Actions(_this: Buttons) =
    class end

let Make = makeConstructor<Buttons, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
