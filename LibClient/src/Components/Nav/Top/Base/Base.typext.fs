module LibClient.Components.Nav.Top.Base

open LibClient
open LibClient.Responsive
open ReactXP.Styles

type Props = (* GenerateMakeFunction *) {
    Handheld: unit -> ReactElement
    Desktop:  unit -> ReactElement
    styles: array<ViewStyles> option // defaultWithAutoWrap None
    key: string option // defaultWithAutoWrap JsUndefined
}

type Top(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Top>("LibClient.Components.Nav.Top.Base", _initialProps, Actions, hasStyles = true)

and Actions(_this: Top) =
    class end

let Make = makeConstructor<Top, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
