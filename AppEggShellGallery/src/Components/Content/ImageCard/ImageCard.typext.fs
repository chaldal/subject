module AppEggShellGallery.Components.Content.ImageCard

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type ImageCard(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, ImageCard>("AppEggShellGallery.Components.Content.ImageCard", _initialProps, Actions, hasStyles = true)

and Actions(_this: ImageCard) =
    class end

let Make = makeConstructor<ImageCard, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
