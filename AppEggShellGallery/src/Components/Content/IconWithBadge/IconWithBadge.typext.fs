module AppEggShellGallery.Components.Content.IconWithBadge

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type IconWithBadge(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, IconWithBadge>("AppEggShellGallery.Components.Content.IconWithBadge", _initialProps, Actions, hasStyles = true)

and Actions(_this: IconWithBadge) =
    class end

let Make = makeConstructor<IconWithBadge, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
