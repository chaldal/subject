module AppEggShellGallery.Components.Content.ColorVariants

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type ColorVariants(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, ColorVariants>("AppEggShellGallery.Components.Content.ColorVariants", _initialProps, Actions, hasStyles = true)

and Actions(_this: ColorVariants) =
    class end

let Make = makeConstructor<ColorVariants, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
