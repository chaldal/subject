module LibClient.Components.Input.PickerInternals.Dialog

open LibLang
open LibClient
open LibClient.Dialogs
open LibClient.Components.Input.PickerModel
open LibLangFsharp

type Parameters<'Item when 'Item : comparison> = {
    Placeholder:  string option // defaultWithAutoWrap None
    Model:        PickerModel<'Item>
    ItemView:     PickerItemView<'Item>
    HideDeferred: Deferred<unit>
    ShowSearchBar: bool
}

type Props<'Item when 'Item : comparison> = DialogProps<Parameters<'Item>, unit>

type Estate<'Item when 'Item : comparison> = {
    ModelState: PickerState<'Item>
}

type Pstate = EmptyRecordType

type Dialog<'Item when 'Item : comparison>(initialProps) =
    inherit DialogComponent<Parameters<'Item>, unit, Estate<'Item>, Pstate, Actions<'Item>, Dialog<'Item>>("LibClient.Components.Input.PickerInternals.Dialog", initialProps.PstoreKey, initialProps, Actions, hasStyles = true)

    override _.GetDefaultPstate(_initialProps: Props<'Item>) = EmptyRecord

    override _.GetInitialEstate(initialProps: Props<'Item>) : Estate<'Item> = {
        ModelState = initialProps.Parameters.Model.GetState ()
    }

    override this.ComponentDidMount () : unit =
        (this.props.Parameters.Model.SubscribeOnStateUpdate (fun update ->
            this.SetEstate (fun estate _ _ -> { estate with ModelState = update.Next })
        )).Off |> this.RunOnUnmount

        async {
            do! this.props.Parameters.HideDeferred.Value
            this.TryCancel DialogCloseMethod.HistoryBack ReactEvent.Action.NonUserOriginatingAction // hack, but should be okay for now
        } |> startSafely

    override _.CanCancel() : Async<bool> = async {
        return true
    }


and Actions<'Item when 'Item : comparison>(this: Dialog<'Item>) =
    member _.Toggle (index: int) (item: 'Item) (_e: ReactEvent.Action) : unit =
        this.props.Parameters.Model.HandleInputEvent (Toggle (index, item))

    member _.TryCancel (_e: ReactEvent.Action) : unit =
        this.props.Parameters.Model.HandleInputEvent ListWasHidden

    member _.OnQueryChange (value: Option<NonemptyString>) : unit =
        this.props.Parameters.Model.HandleInputEvent (QueryChange value)

let Make = makeConstructor<Dialog<'Item>, _, _>

let Open
    (itemView:     PickerItemView<'Item>)
    (model:        PickerModel<'Item>)
    (hideDeferred: Deferred<unit>)
    (placeholder:  string option)
    (showSearchBar:   bool)
    (close:    DialogCloseMethod -> ReactEvent.Action -> unit)
    : ReactElement =

    doOpen<Parameters<'Item>, unit>
        "PickerInternals.Dialog"
        {
            Placeholder  = placeholder
            Model        = model
            ItemView     = itemView
            HideDeferred = hideDeferred
            ShowSearchBar = showSearchBar
        }
        Make
        {
            OnResult      = ignore
            MaybeOnCancel = None
        }
        close
