module LibAutoUi.Components.InputForm

open LibAutoUi
open System
open LibClient
open LibAutoUi.Types
open LibAutoUi.TypeExtensions
open LibClient

// TODO get rid of this eventually, we're not using this Plugin infra
let (* want private but can't have because used by inline *) inputRendererPlugin: InputRendererPlugin =
    fun (defaultInputRenderer) (path) (name) (forType) ->
        defaultInputRenderer path name forType |> Some

type Props<'T> = (* GenerateMakeFunction *) {
    FormWrapper:  FormWrapper<'T>
    Settings:     Settings // default List.empty
    OnChange:     InputValidationResult<'T> -> unit
}

and FormWrapper<'T> = {
    Type:                 Type
    Form:                 LibAutoUi.Types.InputForm
    PathToType:           Map<Path, Type>
    GetResult:            Accumulator -> InputValidationResult<'T>
    ReprocessAccumulator: Settings -> Accumulator -> Async<Accumulator>
} with
    static member inline Make<'T> (name: string) : FormWrapper<'T> =
        let theType = typeof<'T>
        {
            Type                 = theType
            Form                 = LibAutoUi.FormConstruction.buildInputForm inputRendererPlugin name theType
            PathToType           = TypeExtensions.computePathToType theType
            GetResult            = LibAutoUi.ValueConstruction.getResult<'T>
            ReprocessAccumulator = LibAutoUi.ValueConstruction.reprocessAccumulator<'T>
        }

type Estate<'T> = {
    Accumulator:   Accumulator
    ReadyToRender: bool
}

let defaultPrimitiveInputComponents: InputFormElement.PrimitiveInputComponents = Map.ofList [
    (InputType.StringInput,   LibAutoUi.Components.InputFieldString.Make)
    (InputType.DateTimeInput, LibAutoUi.Components.InputFieldDateTime.Make)
]

type InputValue with
    member this.GetValue : obj =
        match this with
        | UnitValue       -> () :> obj
        | StringValue   v -> v  :> obj
        | NumericValue  v -> v  :> obj
        | BooleanValue  v -> v  :> obj
        | GuidValue     v -> v  :> obj
        | DateTimeValue v -> v  :> obj
        | FileValue     v -> v  :> obj

type InputForm<'T>(_initialProps) =
    inherit EstatefulComponent<Props<'T>, Estate<'T>, Actions<'T>, InputForm<'T>>("LibAutoUi.Components.InputForm", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(initialProps: Props<'T>) = {
        Accumulator   = Accumulator.Empty initialProps.FormWrapper.Type initialProps.FormWrapper.PathToType
        ReadyToRender = false
    }

    override this.ComponentDidMount() : unit =
        this.ReprocessAccumulator this.state.Accumulator

    // TODO this is obviously race condition prone, we need to implement serialization-with-dropping-intermediate-requests
    member this.ReprocessAccumulator (accumulator: Accumulator) : unit =
        async {
            let! updatedAccumulator = this.props.FormWrapper.ReprocessAccumulator this.props.Settings accumulator
            this.SetEstate (fun estate _ ->
                { estate with
                    Accumulator   = updatedAccumulator
                    ReadyToRender = true
                }
            )

            updatedAccumulator
            |> this.props.FormWrapper.GetResult
            |> this.props.OnChange
        }
        |> startSafely


and Actions<'T>(this: InputForm<'T>) =
    member _.OnChange (path: LibAutoUi.Types.Path) (value: LibAutoUi.Types.InputValue) : unit =
        this.SetEstate (fun estate _ ->
            let updatedAccumulator = estate.Accumulator.UpdateUserInput path value
            this.ReprocessAccumulator updatedAccumulator
            { estate with Accumulator = updatedAccumulator }
        )

    member _.OnChangeFromRange (path: LibAutoUi.Types.Path) (value: obj) (theType: Type) : unit =
        this.SetEstate (fun estate _ ->
            let updatedAccumulator = estate.Accumulator.UpdateUserInputFromRange path value theType
            this.ReprocessAccumulator updatedAccumulator
            { estate with Accumulator = updatedAccumulator }
        )

let Make = makeConstructor<InputForm<'T>, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate

