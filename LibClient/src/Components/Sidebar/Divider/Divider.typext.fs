module LibClient.Components.Sidebar.Divider

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Divider(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Divider>("LibClient.Components.Sidebar.Divider", _initialProps, Actions, hasStyles = true)

and Actions(_this: Divider) =
    class end

let Make = makeConstructor<Divider, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
