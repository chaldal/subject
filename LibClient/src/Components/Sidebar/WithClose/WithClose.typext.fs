module LibClient.Components.Sidebar.WithClose

open LibClient

type Props = (* GenerateMakeFunction *) {
    With: (ReactEvent.Action -> unit) -> ReactElement

    key: string option // defaultWithAutoWrap JsUndefined
}

type WithClose(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, WithClose>("LibClient.Components.Sidebar.WithClose", _initialProps, Actions, hasStyles = false)

and Actions(_this: WithClose) =
    class end

let Make = makeConstructor<WithClose, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
