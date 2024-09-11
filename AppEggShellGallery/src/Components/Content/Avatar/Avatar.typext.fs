module AppEggShellGallery.Components.Content.Avatar

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Avatar(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Avatar>("AppEggShellGallery.Components.Content.Avatar", _initialProps, Actions, hasStyles = true)

and Actions(_this: Avatar) =
    class end

let Make = makeConstructor<Avatar, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
