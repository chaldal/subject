module AppEggShellGallery.Components.TopNav

open LibClient
open AppEggShellGallery.Navigation

type Props = (* GenerateMakeFunction *) {
    MaybeRoute: Option<Route>

    key: string option // defaultWithAutoWrap JsUndefined
}

type TopNav(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, TopNav>("AppEggShellGallery.Components.TopNav", _initialProps, Actions, hasStyles = true)

and Actions(_this: TopNav) =
    class end

let Make = makeConstructor<TopNav, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate