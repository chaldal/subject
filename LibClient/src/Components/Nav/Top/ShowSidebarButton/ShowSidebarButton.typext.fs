module LibClient.Components.Nav.Top.ShowSidebarButton

open LibClient
open LibClient.Icons

type Badge = LibClient.Output.Badge
let Text  = Badge.Text
let Count = Badge.Count

type Props = (* GenerateMakeFunction *) {
    Badge:    Badge option           // defaultWithAutoWrap None
    MenuIcon: IconConstructor option // defaultWithAutoWrap None
    key: string option // defaultWithAutoWrap JsUndefined
}

type ShowSidebarButton(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, ShowSidebarButton>("LibClient.Components.Nav.Top.ShowSidebarButton", _initialProps, Actions, hasStyles = true)

and Actions(_this: ShowSidebarButton) =
    class end

let Make = makeConstructor<ShowSidebarButton, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
