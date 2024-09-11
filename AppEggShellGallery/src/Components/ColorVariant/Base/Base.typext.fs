module AppEggShellGallery.Components.ColorVariant.Base

open LibClient

type Props = (* GenerateMakeFunction *) {
    Color:  Color
    Name :  string
    IsMain: bool

    key: string option // defaultWithAutoWrap JsUndefined
}

type ColorVariant(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, ColorVariant>("AppEggShellGallery.Components.ColorVariant.Base", _initialProps, Actions, hasStyles = true)

and Actions(_this: ColorVariant) =
    class end

let Make = makeConstructor<ColorVariant, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
