module AppEggShellGallery.Components.Content.Carousel

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Carousel(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Carousel>("AppEggShellGallery.Components.Content.Carousel", _initialProps, Actions, hasStyles = true)

and Actions(_this: Carousel) =
    class end

let Make = makeConstructor<Carousel, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
