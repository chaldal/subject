module LibClient.Components.Tabs

open LibClient
open ReactXP.Styles

let Selected   = LC.Tab.Selected
let Unselected = LC.Tab.Unselected

type Props = (* GenerateMakeFunction *) {
    styles: array<ScrollViewStyles> option // defaultWithAutoWrap None
    key: string option // defaultWithAutoWrap JsUndefined
}

type Tabs(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Tabs>("LibClient.Components.Tabs", _initialProps, NoActions, hasStyles = true)

and Actions = unit

let Make = makeConstructor<Tabs, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
