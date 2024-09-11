module LibClient.Components.LabelledValue

open LibClient

type Props = (* GenerateMakeFunction *) {
    Label:    string

}

type LabelledValue(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, LabelledValue>("LibClient.Components.LabelledValue", _initialProps, NoActions, hasStyles = true)

and Actions = unit

let Make = makeConstructor<LabelledValue, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate