module AppEggShellGallery.Components.Content.Sidebar

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Sidebar(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Sidebar>("AppEggShellGallery.Components.Content.Sidebar", _initialProps, Actions, hasStyles = true)

and Actions(_this: Sidebar) =
    class end

let Make = makeConstructor<Sidebar, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
