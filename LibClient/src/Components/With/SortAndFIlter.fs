[<AutoOpen>]
module LibClient.Components.With_SortAndFilter

open Fable.React
open LibClient
open LibClient.Components

// NOTE TODO SortKey should return IComparable, not string, so that we
// can compare numbers as well. But currently in Fable, if you cast string
// to an IComparable, List.sortBy breaks, failing to call .CompareTo on
// the string. Until we figure this out, the right way to deal with this
// is to prepad numbers with zeros so that we can have a lexicographic ordering,
// like this: `sprintf "%010i" item.NumberOfEmployees`

let private processItems<'Item, 'SortField> (sortKey: 'SortField -> 'Item -> string) (filterCandidate: 'Item -> string) (allItems: seq<'Item>) (maybeFilter: Option<NonemptyString>) (sort: 'SortField * SortDirection) : seq<'Item> =
    let (sortField, sortDirection) = sort

    maybeFilter
    |> Option.map (fun filter ->
        let filterValue = filter.Value.ToLower()
        allItems
        |> Seq.filter (fun item ->
            (filterCandidate item).ToLower().Contains filterValue
        )
    )
    |> Option.getOrElse allItems
    |> Seq.sortBy (sortKey sortField)
    |> match sortDirection with SortDirection.Ascending -> id | SortDirection.Descending -> Seq.rev

type LC.With with
    [<Component>]
    static member SortAndFilter<'Item, 'SortField when 'SortField: equality> (
        allItems: seq<'Item>,
        initialSort: 'SortField * SortDirection,
        sortKey: 'SortField -> 'Item -> string,
        filterCandidate: 'Item -> string,
        content: (* sortedFilteredItems *) seq<'Item> * (* currSort *) ('SortField * SortDirection) * (* maybeCurrFilter *) Option<NonemptyString> * (* setSort *) ('SortField * SortDirection -> unit) * (* setFilter *) (Option<NonemptyString> -> unit) -> ReactElement,
        ?initialFilter: NonemptyString) : ReactElement =
        let itemsState = Hooks.useStateLazy (fun () -> processItems sortKey filterCandidate allItems None initialSort)
        let filterState = Hooks.useState initialFilter
        let sortState = Hooks.useState initialSort

        Hooks.useEffect(
            dependencies = [| allItems |],
            effect =
                fun () ->
                    itemsState.update (processItems sortKey filterCandidate allItems filterState.current sortState.current)
        )

        let setSort (value: 'SortField * SortDirection) : unit =
            match sortState.current = value with
            | true ->
                Noop
            | false ->
                sortState.update value
                itemsState.update (processItems sortKey filterCandidate allItems filterState.current value)

        let setFilter (value: Option<NonemptyString>) : unit =
            match filterState.current = value with
            | true ->
                Noop
            | false ->
                filterState.update value
                itemsState.update (processItems sortKey filterCandidate allItems value sortState.current)

        content(itemsState.current, sortState.current, filterState.current, setSort, setFilter)