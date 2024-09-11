module AppEggShellGallery.Components.Sidebar

open LibClient
open LibClient.Responsive
open AppEggShellGallery.Navigation

type Props = (* GenerateMakeFunction *) {
    MaybeRoute: Option<Route>

    key: string option // defaultWithAutoWrap JsUndefined
}

type Sidebar(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Sidebar>("AppEggShellGallery.Components.Sidebar", _initialProps, Actions, hasStyles = true)

and Actions(_this: Sidebar) =
    class end

let Make = makeConstructor<Sidebar, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
