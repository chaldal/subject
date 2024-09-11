module LibAutoUi.Components.InputFormElement

open System
open LibClient
open LibAutoUi.Types

type PrimitiveInputFieldProps = {
    OnChange:   LibAutoUi.Types.InputValue -> unit
    MaybeValue: Option<LibAutoUi.Types.InputValue>
}


type Props<'T> = (* GenerateMakeFunction *) {
    Form:                     LibAutoUi.Types.InputForm
    Accumulator:              Accumulator
    OnChange:                 LibAutoUi.Types.Path -> LibAutoUi.Types.InputValue -> unit
    OnChangeFromRange:        LibAutoUi.Types.Path -> obj -> Type -> unit
    PrimitiveInputComponents: PrimitiveInputComponents
}

and PrimitiveInputComponents = Map<InputType, PrimitiveInputFieldProps -> seq<ReactElement> -> ReactElement>

type InputFormElement<'T>(_initialProps) =
    inherit PureStatelessComponent<Props<'T>, Actions<'T>, InputFormElement<'T>>("LibAutoUi.Components.InputFormElement", _initialProps, Actions, hasStyles = true)

and Actions<'T>(this: InputFormElement<'T>) =
    class end

let Make = makeConstructor<InputFormElement<'T>, _, _>

// Unfortunately necessary boilerplate
type Estate<'T> = NoEstate1<'T>
type Pstate = NoPstate
