module LibAutoUi.Components.InputFieldString

open LibClient

type Props = LibAutoUi.Components.InputFormElement.PrimitiveInputFieldProps

type InputFieldString(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, InputFieldString>("LibAutoUi.Components.InputFieldString", _initialProps, NoActions, hasStyles = true)

and Actions = unit

let Make = makeConstructor<InputFieldString, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
