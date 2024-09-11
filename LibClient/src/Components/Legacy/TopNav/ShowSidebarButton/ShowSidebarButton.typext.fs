module LibClient.Components.Legacy.TopNav.ShowSidebarButton

open LibClient

type Props = (* GenerateMakeFunction *) {
    OnlyOnHandheld: bool

    key: string option // defaultWithAutoWrap JsUndefined
}

type ShowSidebarButton(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, ShowSidebarButton>("LibClient.Components.Legacy.TopNav.ShowSidebarButton", _initialProps, Actions, hasStyles = true)

and Actions(_this: ShowSidebarButton) =
    class end

let Make = makeConstructor<ShowSidebarButton, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
