module LibClient.Components.Sidebar.Base

open LibClient
open ReactXP.Styles

type Props = (* GenerateMakeFunction *) {
    FixedTop:         ReactElement // default noElement
    FixedBottom:      ReactElement // default noElement
    ScrollableMiddle: ReactElement // default noElement
    styles:           array<ViewStyles> option // defaultWithAutoWrap None

    key: string option // defaultWithAutoWrap JsUndefined
}

type Base(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Base>("LibClient.Components.Sidebar.Base", _initialProps, Actions, hasStyles = true)

and Actions(_this: Base) =
    class end

let Make = makeConstructor<Base, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
