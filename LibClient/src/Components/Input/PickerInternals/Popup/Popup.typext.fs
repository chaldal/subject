module LibClient.Components.Input.PickerInternals.Popup

open LibClient
open LibClient.Components.Input.PickerModel

type Props<'Item when 'Item : comparison> = (* GenerateMakeFunction *) {
    Model:    PickerModel<'Item>
    ItemView: PickerItemView<'Item>
    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate<'Item when 'Item : comparison> = {
    ModelState: PickerState<'Item>
}

type Popup<'Item when 'Item : comparison>(_initialProps) =
    inherit EstatefulComponent<Props<'Item>, Estate<'Item>, Actions<'Item>, Popup<'Item>>("LibClient.Components.Input.PickerInternals.Popup", _initialProps, Actions<'Item>, hasStyles = true)

    override _.GetInitialEstate(initialProps: Props<'Item>) : Estate<'Item> = {
        ModelState = initialProps.Model.GetState ()
    }

    override this.ComponentDidMount () : unit =
        (this.props.Model.SubscribeOnStateUpdate (fun update ->
            this.SetEstate (fun estate _ -> { estate with ModelState = update.Next })
        )).Off |> this.RunOnUnmount

and Actions<'Item when 'Item : comparison>(this: Popup<'Item>) =
    member _.Select (index: int) (item: 'Item) (_e: Browser.Types.Event) : unit =
        this.props.Model.HandleInputEvent (Select (index, item))

let Make = makeConstructor<Popup<'Item>, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate
