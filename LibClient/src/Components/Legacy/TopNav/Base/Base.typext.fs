module LibClient.Components.Legacy.TopNav.Base

open LibClient

type Props = (* GenerateMakeFunction *) {
    Left:     ReactElement // default noElement
    Center:   Center

    Right:    ReactElement // default noElement
}

and Center =
| Children
| Heading of string

type Estate = NoEstate
type Pstate = NoPstate

type Base(initialProps) =
    inherit PureStatelessComponent<Props, Actions, Base>("LibClient.Components.Legacy.TopNav.Base", initialProps, NoActions, hasStyles = true)

and Actions = unit

let Make = makeConstructor<Base,_,_>
