module LibClient.Components.Input.PickerModel

open LibClient
open LibClient.EventBus

type PickerItemMetadata = {
    IsSelected:    bool
    IsHighlighted: bool
}

type PickerItemVisuals = {|
    Label: string
|}

type PickerItemView<'T> =
| Default of ('T -> PickerItemVisuals)
| Custom  of ('T -> ReactElement)
// later can also have this | CompletelyCustom of ('T -> PickerItemMetadata -> ReactElement)

type Items<'T when 'T : comparison> =
| Static of Items: OrderedSet<'T> * ToFilterString: ('T -> string)
| Async  of FetchItems: (Option<NonemptyString> -> Async<OrderedSet<'T>>)
with
    member this.Fetch (maybeQuery: Option<NonemptyString>) : Async<OrderedSet<'T>> =
        match this with
        | Static (items, toFilterString) ->
            let filteredItems =
                match maybeQuery with
                | None -> items
                | Some query ->
                    let queryLowerCase = query.Value.ToLower()
                    items
                    |> OrderedSet.filter (fun item ->
                        (toFilterString item).ToLower().Contains queryLowerCase
                    )

            filteredItems
            |> Async.Of

        | Async fetchItems -> fetchItems maybeQuery

type PickerState<'Item when 'Item : comparison> = {
    SelectableItems:           AsyncData<List<'Item>>
    Value:                     SelectableValue<'Item>
    MaybeQuery:                Option<NonemptyString>
    MaybeHighlightedItemIndex: Option<int>
    KeyboardSelectionState:    KeyboardSelectionState
    DeleteState:               DeleteState<'Item>
    IsListVisible:             bool
    MaybeFieldWidth:           Option<int>
}

and [<RequireQualifiedAccess>] DeleteState<'Item when 'Item : comparison> =
| Idle
| Selected of 'Item

and [<RequireQualifiedAccess>] KeyboardSelectionState =
| NothingSelected
| Selected of ItemIndex: int

type StateUpdate<'Item when 'Item : comparison> = {
    Prev: PickerState<'Item>
    Next: PickerState<'Item>
}

type PickerInputEvent<'Item when 'Item : comparison> =
| QueryChange of Option<NonemptyString>
| Select      of Index: int * Item: 'Item
| Toggle      of Index: int * Item: 'Item
| Unselect    of Item: 'Item
| UnselectAllIfAllowed
| ResetDeleteState
| Backspace
| ArrowUp
| ArrowDown
| Enter
| Tab
| ShowList
| ListWasHidden
| FieldWidthChange of int

type PickerModel<'Item when 'Item : comparison> (eventBus: EventBus, items: Items<'Item>, initialState: PickerState<'Item>) =
    let instanceId = System.Guid.NewGuid()
    let queue: LibClient.EventBus.Queue<StateUpdate<'Item>> = Queue $"pickerOutQueue{instanceId}"

    let mutable state: PickerState<'Item> = initialState

    member this.GetState () = state

    member this.ReloadSelectableItems () : unit =
        async {
            let maybeQuery = state.MaybeQuery
            this.UpdateState
                { state with
                    SelectableItems           = state.SelectableItems |> AsyncData.makeFetching
                    MaybeHighlightedItemIndex = None
                }
            let! selectableItems = items.Fetch maybeQuery
            if maybeQuery = state.MaybeQuery then
                this.UpdateState
                    { state with
                        SelectableItems           = selectableItems |> OrderedSet.toList |> Available
                        MaybeHighlightedItemIndex =
                            match (state.IsListVisible, selectableItems.IsEmpty) with
                            | (false, _)     -> None
                            | (true,  true)  -> None
                            | (true,  false) -> Some 0
                    }
        } |> startSafely

    member this.UpdateState (newState: PickerState<'Item>) : unit =
        let prevState = state
        state <- newState
        eventBus.Broadcast queue { Prev = prevState; Next = newState }

    member this.MaybeSelectForDeletion (maybeItem: Option<'Item>) : unit =
        let deleteState =
            match maybeItem with
            | None -> DeleteState.Idle
            | Some item -> DeleteState.Selected item
        this.UpdateState { state with DeleteState = deleteState }

    member this.SetValue (value: SelectableValue<'Item>) : unit =
        this.UpdateState { state with Value = value }

    member this.HandleInputEvent (e: PickerInputEvent<'Item>) : unit =
        LibClient.JsInterop.runOnNextTick (fun () -> this.ActuallyHandleInputEvent e)

    member private this.ActuallyHandleInputEvent (e: PickerInputEvent<'Item>) : unit =
        match e with
        | Select (_index, item) ->
            this.UpdateState { state with DeleteState = DeleteState.Idle; MaybeQuery = None }
            state.Value.Select item
            if not state.Value.CanSelectMultiple then
                this.UpdateState { state with IsListVisible = false; MaybeQuery = None }

        | Toggle (_index, item) ->
            this.UpdateState { state with DeleteState = DeleteState.Idle }
            state.Value.Toggle item
            if not state.Value.CanSelectMultiple then
                this.UpdateState { state with IsListVisible = false }

        | Unselect item ->
            this.UpdateState { state with DeleteState = DeleteState.Idle }
            state.Value.Unselect item

        | UnselectAllIfAllowed ->
            this.UpdateState { state with DeleteState = DeleteState.Idle }
            state.Value.UnselectAllIfAllowed ()

        | QueryChange maybeQuery ->
            let maybeUpdatedIsListVisible =
                match maybeQuery with
                | None   -> state.IsListVisible
                | Some _ -> true
            this.UpdateState { state with MaybeQuery = maybeQuery; IsListVisible = maybeUpdatedIsListVisible }
            LibClient.JsInterop.runOnNextTick this.ReloadSelectableItems

        | ResetDeleteState ->
            this.UpdateState { state with DeleteState = DeleteState.Idle }

        | Backspace ->
            if state.MaybeQuery = None then
                match (state.Value, state.DeleteState) with
                | (AtLeastOne (maybeSelectedValues, _), DeleteState.Idle) -> maybeSelectedValues |> Option.sideEffect (fun selectedValues -> if selectedValues.Count > 1 then this.MaybeSelectForDeletion (OrderedSet.tryLast selectedValues))
                | (Any        (maybeSelectedValues, _), DeleteState.Idle) -> maybeSelectedValues |> Option.sideEffect (fun selectedValues -> this.MaybeSelectForDeletion (OrderedSet.tryLast selectedValues))
                | (AtLeastOne _, DeleteState.Selected index)
                | (Any _,        DeleteState.Selected index) ->
                    state.Value.Unselect index
                    this.UpdateState { state with DeleteState = DeleteState.Idle }
                | _ -> Noop

        | ArrowUp ->
            this.HandleArrow (fun index _count -> if index = 0 then index else index - 1)

        | ArrowDown ->
            this.HandleArrow (fun index count -> if index = count - 1 then index else index + 1)

        | Enter ->
            match state.MaybeHighlightedItemIndex with
            | None -> Noop
            | Some highlightedIndex ->
                state.SelectableItems
                |> AsyncData.toOption
                |> Option.sideEffect (fun items ->
                    items
                    |> Seq.item highlightedIndex
                    |> state.Value.Toggle

                    if not state.Value.CanSelectMultiple then
                        let hasQueryChanged = state.MaybeQuery <> None
                        this.UpdateState
                            { state with
                                MaybeHighlightedItemIndex = None
                                MaybeQuery                = None
                                IsListVisible             = false
                            }
                        if hasQueryChanged then this.ReloadSelectableItems ()
                )

        | Tab ->
            // not sure we actually want to use Tab as a selection key
            Noop

        | ShowList ->
            if state.SelectableItems = WillStartFetchingSoonHack then this.ReloadSelectableItems ()
            this.UpdateState { state with IsListVisible = true }

        | ListWasHidden ->
            this.UpdateState { state with IsListVisible = false; MaybeHighlightedItemIndex = None }

        | FieldWidthChange value ->
            this.UpdateState { state with MaybeFieldWidth = Some value }

    member private this.HandleArrow (nextIndex: int -> int -> int) : unit =
        this.UpdateState { state with IsListVisible = true }; // WTF?! why am I forced to put a semicolon here?!

        state.SelectableItems
        |> AsyncData.toOption
        |> Option.map List.length
        |> Option.sideEffect (fun count ->
            if count > 0 then
                let nextIndex =
                    match state.MaybeHighlightedItemIndex with
                    | None                  -> 0
                    | Some highlightedIndex -> nextIndex highlightedIndex count
                this.UpdateState { state with MaybeHighlightedItemIndex = Some nextIndex }
        )


    member _.SubscribeOnStateUpdate (callback: StateUpdate<'Item> -> unit) : OnResult =
        eventBus.On queue callback
