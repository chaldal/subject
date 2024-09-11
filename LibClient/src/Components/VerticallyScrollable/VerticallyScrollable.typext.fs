module LibClient.Components.VerticallyScrollable

open LibClient
open ReactXP.Styles

type Props = (* GenerateMakeFunction *) {
    FixedTop:         ReactElement option // defaultWithAutoWrap None
    FixedBottom:      ReactElement option // defaultWithAutoWrap None
    ScrollableMiddle: ReactElement option // defaultWithAutoWrap None
    styles:           array<ViewStyles> option // defaultWithAutoWrap None
    key: string option // defaultWithAutoWrap JsUndefined
}

type VerticallyScrollable(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, VerticallyScrollable>("LibClient.Components.VerticallyScrollable", _initialProps, Actions, hasStyles = true)

and Actions(_this: VerticallyScrollable) =
    class end

let Make = makeConstructor<VerticallyScrollable, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
