module LibClient.Components.Input.PickerInternals.Base

open LibLangFsharp
open LibClient
open LibClient.Responsive
open LibClient.Components.Input.PickerModel
open Fable.Core.JsInterop
open ReactXP.Styles

type Props<'Item when 'Item : comparison> = (* GenerateMakeFunction *) {
    Label:         string option // defaultWithAutoWrap None
    Placeholder:   string option // defaultWithAutoWrap None
    Items:         Items<'Item>
    ItemView:      PickerItemView<'Item>
    Value:         SelectableValue<'Item>
    Validity:      InputValidity
    ScreenSize:    ScreenSize
    ShowSearchBar: bool
    PickerId:      string option // defaultWithAutoWrap JsUndefined
    styles: array<ViewStyles> option // defaultWithAutoWrap None
    key: string option // defaultWithAutoWrap JsUndefined
}

type Base<'Item when 'Item : comparison>(initialProps) =
    inherit PureStatelessComponent<Props<'Item>, Actions<'Item>, Base<'Item>>("LibClient.Components.Input.PickerInternals.Base", initialProps, Actions<'Item>, hasStyles = false)

    let model = (
        let initialState = {
            SelectableItems           = WillStartFetchingSoonHack
            Value                     = initialProps.Value
            MaybeQuery                = None
            MaybeHighlightedItemIndex = None
            KeyboardSelectionState    = KeyboardSelectionState.NothingSelected
            DeleteState               = DeleteState.Idle
            IsListVisible             = false
            MaybeFieldWidth           = None
        }
        PickerModel (LibClient.ServiceInstances.services().EventBus, initialProps.Items, initialState)
    )

    static let popupId = "LibClient.Components.Input.Picker"
    let mutable maybePopupHide:  Option<unit -> unit> = None
    let mutable maybeDialogHide: Option<unit -> unit> = None

    member this.GetModel () = model

    override this.ComponentDidMount () : unit =
        (model.SubscribeOnStateUpdate (fun update ->
            match (update.Prev.IsListVisible, update.Next.IsListVisible) with
            | (false, true) -> this.ShowList ()
            | (true, false) -> this.HideList ()
            | _ -> Noop
            Noop
        )).Off |> this.RunOnUnmount
        
        // fix for cases where the component did unmount while the dialog is still open
        (fun _ -> this.HideList ()) |> this.RunOnUnmount

    override this.ComponentWillReceiveProps (nextProps: Props<'Item>) : unit =
        model.SetValue nextProps.Value

    member private this.ShowList () : unit =
        match this.props.ScreenSize with
        | ScreenSize.Desktop ->
            let popup =
                LibClient.Components.Input.PickerInternals.Popup.Make
                    {
                        Model    = model
                        ItemView = this.props.ItemView
                        key      = this.props.PickerId
                    }
                    [||]

            let options = createObj [
                "getAnchor"   ==> fun () -> this
                "renderPopup" ==> fun (_anchorPosition: obj, _anchorOffset: int, _popupWidth: int, _popupHeight: int) -> popup
                "onDismiss"   ==> (fun () ->
                    maybePopupHide <- None
                    model.HandleInputEvent ListWasHidden
                )
            ]
            ReactXP.Helpers.ReactXPRaw?Popup?show(options, popupId)

            maybePopupHide <- Some (fun () ->
                ReactXP.Helpers.ReactXPRaw?Popup?dismiss(popupId)
                maybePopupHide <- None
            )

        | ScreenSize.Handheld ->
            let hideDeferred = Deferred<unit>()
            maybeDialogHide <- Some (fun () ->
                hideDeferred.Resolve ()
                maybeDialogHide <- None
            )
            LibClient.Dialogs.AdHoc.go
                (LibClient.Components.Input.PickerInternals.Dialog.Open this.props.ItemView model hideDeferred this.props.Placeholder this.props.ShowSearchBar)
                ReactEvent.Action.NonUserOriginatingAction // hack, but should be okay for now


    member private _.HideList () : unit =
        maybePopupHide  |> Option.sideEffect (fun hide -> hide ())
        maybeDialogHide |> Option.sideEffect (fun hide -> hide ())

and Actions<'Item when 'Item : comparison>(this: Base<'Item>) =
    member _.GetModel () : PickerModel<'Item> =
        this.GetModel ()

let Make = makeConstructor<Base<'Item>, _, _>

// Unfortunately necessary boilerplate
type Estate<'Item when 'Item : comparison> = NoEstate1<'Item>
type Pstate = NoPstate
