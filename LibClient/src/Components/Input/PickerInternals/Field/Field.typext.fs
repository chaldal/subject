module LibClient.Components.Input.PickerInternals.Field

open LibClient
open LibClient.Components.Input.PickerModel
open ReactXP.Styles

type Props<'Item when 'Item : comparison> = (* GenerateMakeFunction *) {
    Model:       PickerModel<'Item>
    Label:       string option // defaultWithAutoWrap None
    Placeholder: string option // defaultWithAutoWrap None
    Value:       SelectableValue<'Item>
    Validity:    InputValidity
    ItemView:    PickerItemView<'Item>
    styles: array<ViewStyles> option // defaultWithAutoWrap None
    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate<'Item when 'Item : comparison> = {
    ModelState: PickerState<'Item>
    // the updates coming from ModelState are not fast enough to keep up
    // with user's typing speed, so we are forced to keep local state,
    // to make sure we can echo the input text as soon as possible
    MaybeQuery: Option<NonemptyString>
    IsFocused:  bool
}

type Field<'Item when 'Item : comparison>(_initialProps) =
    inherit EstatefulComponent<Props<'Item>, Estate<'Item>, Actions<'Item>, Field<'Item>>("LibClient.Components.Input.PickerInternals.Field", _initialProps, Actions<'Item>, hasStyles = true)

    let mutable maybeSubscriptionOff: Option<unit -> unit> = None

    let mutable maybeTextInput: Option<ReactXP.Components.TextInput.ITextInputRef> = None
    member _.MaybeTextInput
        with get () = maybeTextInput
        and  set (value: Option<ReactXP.Components.TextInput.ITextInputRef>) : unit =
             maybeTextInput <- value

    override _.GetInitialEstate(initialProps: Props<'Item>) : Estate<'Item> =
        let modelState = initialProps.Model.GetState()
        {
            ModelState = modelState
            MaybeQuery = modelState.MaybeQuery
            IsFocused  = false
        }

    member private _.MaybeUnsubscribeOnModel () : unit =
        maybeSubscriptionOff |> Option.sideEffect (fun off ->
            off()
            maybeSubscriptionOff <- None
        )

    member private this.ResubscribeOnModel (model: PickerModel<'Item>) : unit =
        this.MaybeUnsubscribeOnModel ()
        maybeSubscriptionOff <-
            (
                model.SubscribeOnStateUpdate (fun update ->
                    this.SetEstate (fun estate _ -> { estate with ModelState = update.Next; MaybeQuery = update.Next.MaybeQuery })
                )
            ).Off
            |> Some

    override this.ComponentDidMount () : unit =
        this.RunOnUnmount this.MaybeUnsubscribeOnModel
        this.ResubscribeOnModel this.props.Model

    override this.ComponentWillReceiveProps (nextProps: Props<'Item>) : unit =
        if this.props.Model <> nextProps.Model then
            this.ResubscribeOnModel this.props.Model

and Actions<'Item when 'Item : comparison>(this: Field<'Item>) =
    member _.RefTextInput (nullableInstance: LibClient.JsInterop.JsNullable<ReactXP.Components.TextInput.ITextInputRef>) =
        this.MaybeTextInput <- nullableInstance.ToOption

    member _.ShowItemSelector (_: ReactEvent.Action) : unit =
        this.props.Model.HandleInputEvent ShowList

    member _.OnKeyPress (e: Browser.Types.KeyboardEvent) : unit =
        match e.key with
        | KeyboardEvent.Key.Backspace -> Backspace
        | KeyboardEvent.Key.ArrowUp   -> ArrowUp
        | KeyboardEvent.Key.ArrowDown -> ArrowDown
        | KeyboardEvent.Key.Enter     -> Enter
        | KeyboardEvent.Key.Tab       -> Tab
        | _                           -> ResetDeleteState
        |> this.props.Model.HandleInputEvent

        match e.key with
        | KeyboardEvent.Key.ArrowUp
        | KeyboardEvent.Key.ArrowDown ->
            // don't let up and down arrows navigate the cursor in the field
            e.preventDefault()
        | KeyboardEvent.Key.Enter ->
           this.MaybeTextInput |> Option.sideEffect (fun textInput -> textInput.blur())
        | _ -> Noop


    member _.RequestFocus (e: ReactEvent.Action) =
        this.Actions.ShowItemSelector e
        this.MaybeTextInput |> Option.sideEffect (fun textInput ->
            this.SetEstate (fun estate _ -> { estate with IsFocused = true })
            textInput.requestFocus()
        )

    member _.ShouldShowSelectedValue () : bool =
        this.props.Model.GetState().IsListVisible = false && this.state.IsFocused = false

    member _.OnFocus (_: Browser.Types.FocusEvent) =
        this.Actions.ShowItemSelector ReactEvent.Action.NonUserOriginatingAction
        this.SetEstate (fun estate _ -> { estate with IsFocused = true })

    member actions.OnBlur (_: Browser.Types.FocusEvent) =
        this.SetEstate (fun estate _ -> { estate with IsFocused = false })

    member actions.OnChangeQuery (maybeQuery: Option<NonemptyString>) : unit =
        this.SetEstate (fun estate _ -> { estate with MaybeQuery = maybeQuery })
        this.props.Model.HandleInputEvent (QueryChange maybeQuery)

    member _.Unselect (item: 'Item) (_: ReactEvent.Action) : unit =
        this.props.Model.HandleInputEvent (Unselect item)

    member _.Clear (_: ReactEvent.Action) : unit =
        this.props.Model.HandleInputEvent UnselectAllIfAllowed

    member _.OnLayout (e: ReactXP.Types.ViewOnLayoutEvent) : unit =
        // this is called far too frequently, need to throttle manually here
        if (this.props.Model.GetState()).MaybeFieldWidth <> Some e.width then
            this.props.Model.HandleInputEvent (FieldWidthChange e.width)

let Make = makeConstructor<Field<'Item>, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate


// Don't want to clobber && for the whole file
// TODO this code is repeated in the Text.typext.fs file as well. Need to move it to a common place.
open ReactXP.LegacyStyles

let extractPlaceholderColor (mergedStyles: List<RuntimeStyles>) : Color =
    let maybeColor: Option<Color> =
        let placeholderStyles = Runtime.findApplicableStyles mergedStyles "SPECIAL-placeholder"
        placeholderStyles
        |> List.filterMap (function
            | RuntimeStyles.StaticRules lazyRulesObject ->
                lazyRulesObject.GetRawStyleRules
                |> Seq.findMap
                    (fun rawRule ->
                        let (key, value) = rawRule :> obj :?> (string * obj)
                        match key with
                        | "color"    -> Some (Color.InternalString (value :?> string))
                        | _          -> None
                    )
            | _ -> None
        )
        |> List.tryLast

    maybeColor |> Option.getOrElse Color.DevRed
