module LibClient.Components.IconWithBadge

open LibClient

type Badge = LibClient.Output.Badge
let Text  = Badge.Text
let Count = Badge.Count

type Props = (* GenerateMakeFunction *) {
    Icon:  LibClient.Icons.IconConstructor
    Badge: Badge
}

type IconWithBadge(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, IconWithBadge>("LibClient.Components.IconWithBadge", _initialProps, NoActions, hasStyles = true)

and Actions = unit

let Make = makeConstructor<IconWithBadge, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate