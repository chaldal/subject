module AppEggShellGallery.Components.ColorVariants

open LibClient

type Props = (* GenerateMakeFunction *) {
    Value: Variants

    key: string option // defaultWithAutoWrap JsUndefined
}

type ColorVariants(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, ColorVariants>("AppEggShellGallery.Components.ColorVariants", _initialProps, Actions, hasStyles = true)

and Actions(_this: ColorVariants) =
    class end

let Make = makeConstructor<ColorVariants, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
