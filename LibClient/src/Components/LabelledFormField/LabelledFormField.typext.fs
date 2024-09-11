module LibClient.Components.LabelledFormField

open LibClient

type Props = (* GenerateMakeFunction *) {
    Label:    string

}

type LabelledFormField(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, LabelledFormField>("LibClient.Components.LabelledFormField", _initialProps, NoActions, hasStyles = true)

and Actions = unit

let Make = makeConstructor<LabelledFormField, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate