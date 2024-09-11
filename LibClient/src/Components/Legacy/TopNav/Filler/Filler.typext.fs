module LibClient.Components.Legacy.TopNav.Filler

open LibClient

type Props = (* GenerateMakeFunction *) {
    Count: int // default 1
}

type Estate = NoEstate
type Pstate = NoPstate

type Filler(initialProps) =
    inherit PureStatelessComponent<Props, Actions, Filler>("LibClient.Components.Legacy.TopNav.Filler", initialProps, NoActions, hasStyles = true)

and Actions = unit

let Make = makeConstructor<Filler,_,_>
