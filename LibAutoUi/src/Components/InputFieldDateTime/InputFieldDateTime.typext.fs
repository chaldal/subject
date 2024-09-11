module LibAutoUi.Components.InputFieldDateTime

open LibClient

type Props = LibAutoUi.Components.InputFormElement.PrimitiveInputFieldProps

type InputFieldDateTime(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, InputFieldDateTime>("LibAutoUi.Components.InputFieldDateTime", _initialProps, NoActions, hasStyles = true)

and Actions = unit

let Make = makeConstructor<InputFieldDateTime, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
