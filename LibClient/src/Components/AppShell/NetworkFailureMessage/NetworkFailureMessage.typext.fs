module LibClient.Components.AppShell.NetworkFailureMessage

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type NetworkFailureMessage(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, NetworkFailureMessage>("LibClient.Components.AppShell.NetworkFailureMessage", _initialProps, Actions, hasStyles = true)

and Actions(_this: NetworkFailureMessage) =
    class end

let Make = makeConstructor<NetworkFailureMessage, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
