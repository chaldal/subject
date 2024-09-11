module LibClient.Components.Legacy.Input.PositiveInteger

open LibClient
open LibClient.Components
open LibClient.JsInterop

type InputPositiveIntegerRef =
    abstract member SelectAll:    unit -> unit;
    abstract member RequestFocus: unit -> unit;

type Props = (* GenerateMakeFunction *) {
    InitialValue: Positive.PositiveInteger option // defaultWithAutoWrap None
    OnChange:     Result<Positive.PositiveInteger, InputValidationError> -> unit
    OnKeyPress:   (Browser.Types.KeyboardEvent -> unit)                             option // defaultWithAutoWrap JsUndefined
    ref:          (LibClient.JsInterop.JsNullable<InputPositiveIntegerRef> -> unit) option // defaultWithAutoWrap JsUndefined
}

type Estate = {
    CurrInput: string
}

type PositiveInteger(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, PositiveInteger>("LibClient.Components.Legacy.Input.PositiveInteger", _initialProps, Actions, hasStyles = true)

    let mutable maybeTextInput: Option<ReactXP.Components.TextInput.ITextInputRef> = None

    interface InputPositiveIntegerRef with
        member _.SelectAll () : unit =
            maybeTextInput |> Option.sideEffect (fun textInput -> textInput.selectAll())

        member _.RequestFocus () : unit =
            maybeTextInput |> Option.sideEffect (fun textInput -> textInput.requestFocus())

    member _.MaybeTextInput
        with get () = maybeTextInput
        and  set (value: Option<ReactXP.Components.TextInput.ITextInputRef>) : unit =
             maybeTextInput <- value

    override _.GetInitialEstate(initialProps: Props) = {
        CurrInput =
            initialProps.InitialValue
            |> Option.map (fun v -> v.Value.ToString())
            |> Option.getOrElse ""
    }

    override this.ComponentDidMount() : unit =
        let initialResult =
            match this.props.InitialValue with
            | None              -> Error InputValidationError.Missing
            | Some initialValue -> Ok initialValue

        this.props.OnChange initialResult

and Actions(this: PositiveInteger) =
    let bound = {|
        RefTextInput = fun (nullableInstance: LibClient.JsInterop.JsNullable<ReactXP.Components.TextInput.ITextInputRef>) ->
        this.MaybeTextInput <- nullableInstance.ToOption
    |}
    member _.Bound = bound

    member _.OnChange(stringValue: string) : unit =
        this.SetEstate (fun _ _ -> { CurrInput = stringValue })

        let result =
            match stringValue with
            | ""                  -> Error InputValidationError.Missing
            | nonemptyStringValue ->
                nonemptyStringValue
                |> System.Int32.ParseOption
                |> Option.flatMap   PositiveInteger.ofInt
                |> Option.map       Ok
                |> Option.getOrElse (Error InvalidValue)

        this.props.OnChange result

let Make = makeConstructor<PositiveInteger, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate
