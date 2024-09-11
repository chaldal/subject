module LibClient.Components.Legacy.Sidebar.Filler

open LibClient

type Props = (* GenerateMakeFunction *) {
    HackWeDoNotSupportProplessComponents: bool // default true
}

type Filler(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Filler>("LibClient.Components.Legacy.Sidebar.Filler", _initialProps, Actions, hasStyles = true)

and Actions(_this: Filler) =
    class end

let Make = makeConstructor<Filler, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
