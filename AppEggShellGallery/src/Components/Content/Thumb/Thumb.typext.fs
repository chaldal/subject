module AppEggShellGallery.Components.Content.Thumb

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Fruit = {
    Name:     string
    ImageUrl: string
}

let banana = {
    Name     = "banana"
    ImageUrl = "/images/yuumei1.jpg"
}

type Thumb(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Thumb>("AppEggShellGallery.Components.Content.Thumb", _initialProps, Actions, hasStyles = true)

and Actions(_this: Thumb) =
    class end

let Make = makeConstructor<Thumb, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
