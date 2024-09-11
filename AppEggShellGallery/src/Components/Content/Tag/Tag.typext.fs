module AppEggShellGallery.Components.Content.Tag

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Tag(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Tag>("AppEggShellGallery.Components.Content.Tag", _initialProps, Actions, hasStyles = true)

and Actions(_this: Tag) =
    class end

let Make = makeConstructor<Tag, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
