module LibClient.Components.Legacy.Input.Picker

open LibClient
open LibClient.ContextMenus.Types
open ReactXP.Styles

type Props<'T> = (* GenerateMakeFunction *) {
    Items:    List<PickerItem<'T>>
    Value:    Option<SelectedItem<'T>>
    OnChange: OnChange<'T>
    Label:    string option // defaultWithAutoWrap None
    Validity: InputValidity
    styles:   array<ViewStyles> option // defaultWithAutoWrap None
    key: string option // defaultWithAutoWrap JsUndefined
}

and PickerItem<'T> = {
    Label: string
    Item:  'T
}

and SelectedItem<'T> =
| ByIndex of int
| ByItem  of 'T

and OnChange<'T> =
| CanUnselect    of ((Option<int * 'T>) -> unit)
| CannotUnselect of ((       int * 'T ) -> unit)

type Estate<'T> = {
    ContextMenuItems:  List<ContextMenuItem>
    SelectedItemLabel: string
    IsFocused:         bool
}

let getSelectedItemLabel (items: List<PickerItem<'T>>) (maybeRawSelectedItem: Option<SelectedItem<'T>>) : string =
    maybeRawSelectedItem
    |> Option.flatMap (function
        | ByIndex rawIndex -> items |> List.tryItem rawIndex
        | ByItem  rawItem  -> items |> List.tryFind (fun item -> item.Item = rawItem)
    )
    |> Option.map (fun validSelectedItem -> validSelectedItem.Label)
    |> Option.getOrElse "--"

type Picker<'T when 'T: equality>(_initialProps) =
    inherit EstatefulComponent<Props<'T>, Estate<'T>, Actions<'T>, Picker<'T>>("LibClient.Components.Legacy.Input.Picker", _initialProps, Actions, hasStyles = true)

    member this.SetSelected (maybeIndex: Option<int>) : unit =
        match this.props.OnChange with
        | CanUnselect    onChange -> maybeIndex |> Option.map        (fun index -> (index, this.props.Items.[index].Item)) |> onChange
        | CannotUnselect onChange -> maybeIndex |> Option.sideEffect (fun index -> (index, this.props.Items.[index].Item) |> onChange)

    member this.GenerateContextMenuItems (items: List<PickerItem<'T>>) (onChange: OnChange<'T>) (maybeSelectedItem: Option<SelectedItem<'T>>) : List<ContextMenuItem> =
        let baseItems =
            items
            |> List.mapi (fun index item ->
                let isSelected =
                    match maybeSelectedItem with
                    | Some (ByIndex selectedIndex) -> index      = selectedIndex
                    | Some (ByItem  selectedItem ) -> item.Item  = selectedItem
                    | None                         -> false
                ContextMenuItem.Button (item.Label, (fun _ -> this.SetSelected (Some index)), isSelected)
            )
        let unselectItem = ContextMenuItem.Button ("--", (fun _ -> this.SetSelected None), isSelected = maybeSelectedItem.IsNone)

        match (onChange, maybeSelectedItem) with
        | (CanUnselect    _, _     ) -> unselectItem :: baseItems
        | (CannotUnselect _, None  ) -> unselectItem :: baseItems
        | (CannotUnselect _, Some _) ->                 baseItems

    member this.GetDerivedEstate (props: Props<'T>) : Estate<'T> = {
        ContextMenuItems  = this.GenerateContextMenuItems props.Items props.OnChange props.Value
        SelectedItemLabel = getSelectedItemLabel props.Items props.Value
        IsFocused         = false
    }

    override this.GetInitialEstate(initialProps: Props<'T>) = this.GetDerivedEstate initialProps

    override this.ComponentWillReceiveProps(nextProps: Props<'T>) =
        this.SetEstate(fun estate _ -> {(this.GetDerivedEstate nextProps) with IsFocused = estate.IsFocused})

and Actions<'T when 'T: equality>(_this: Picker<'T>) =
    member _.OnPressIn _ : unit =
        _this.SetEstate (fun estate _ -> { estate with IsFocused = true })

    member _.OnPressOut _ : unit =
        _this.SetEstate (fun estate _ -> { estate with IsFocused = false })

let Make = makeConstructor<Picker<'T>, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate