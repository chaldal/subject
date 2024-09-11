module LibClient.Components.Nav.Bottom.Base

open LibClient
open LibClient.Responsive
open ReactXP.Styles

type Props = (* GenerateMakeFunction *) {
    Handheld: unit -> ReactElement
    Desktop:  unit -> ReactElement
    styles: array<ViewStyles> option // defaultWithAutoWrap None

    key: string option // defaultWithAutoWrap JsUndefined
}

type Bottom(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Bottom>("LibClient.Components.Nav.Bottom.Base", _initialProps, Actions, hasStyles = true)

and Actions(_this: Bottom) =
    class end

let Make = makeConstructor<Bottom, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
