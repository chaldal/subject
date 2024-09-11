module LibUiAdmin.Components.Grid

open Fable.React
open LibClient

type PaginatedGridData<'T> = {
    PageNumber:          PositiveInteger
    PageSize:            PositiveInteger
    MaybePageCount:      Option<UnsignedInteger>
    Items:               AsyncData<seq<'T>>
    MaybeTotalItemCount: Option<UnsignedInteger>
    GoToPage:       (* pageSize *) PositiveInteger -> (* pageNumber *) PositiveInteger -> Option<ReactEvent.Action> -> unit
} with
    member this.MakeFetching : PaginatedGridData<'T> =
        { this with Items = this.Items |> AsyncData.makeFetching }

type Input<'T> =
| Static     of Rows: ReactElement * MaybeHandheldRows: Option<ReactElement>
| Everything of AsyncData<seq<'T>> * MakeDesktopRow: ('T -> ReactElement) * MakeHandheldRow: (Option<'T -> ReactElement>)
| Paginated  of PaginatedGridData<'T> * MakeDesktopRow: ('T -> ReactElement) * MakeHandheldRow: (Option<'T -> ReactElement>)

type Props<'T> = (* GenerateMakeFunction *) {
    Headers:                 ReactElement option   // defaultWithAutoWrap None
    HeadersRaw:              ReactElement option   // defaultWithAutoWrap None
    Input:                   Input<'T>
    ItemKey:                 ('T -> string) option // defaultWithAutoWrap None
    PageSizeChoices:         List<PositiveInteger> // default ([10; 20; 50; 100] |> List.map PositiveInteger.ofLiteral)
    HandleVerticalScrolling: bool                  // default false
    key:                     string option         // defaultWithAutoWrap JsUndefined
}

type Estate<'T> = {
    JumpToPage: LibClient.Components.Input.PositiveInteger.Value
}

type Grid<'T>(_initialProps) =
    inherit EstatefulComponent<Props<'T>, Estate<'T>, Actions<'T>, Grid<'T>>("LibUiAdmin.Components.Grid", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(_initialProps: Props<'T>) = {
        JumpToPage = LibClient.Components.Input.PositiveInteger.empty
    }

and Actions<'T>(this: Grid<'T>) =
    member _.GeneratePagesToShow (currentPage: int) (totalPageCount: int) : List<int> =
        set [1; currentPage - 1; currentPage; currentPage + 1; totalPageCount]
        |> Set.filter (fun page -> page > 0 && page <= totalPageCount)
        |> Set.toList

    member _.ResizePage (data: PaginatedGridData<'T>) (newPageSize: PositiveInteger) : unit =
        // Ideally we want to preserve the offset, and just show more rows on the current page.
        // But it's likely that given the new page size, the top of page won't align to the previous
        // offset. So instead of having half-assed results, where the new page contains the old page's
        // rows somewhere within it, we'd rather just consistently switch to page 1.
        data.GoToPage newPageSize PositiveInteger.One None

    member _.OnJumpToPageNumberKeyPress (maybeSave: Option<ReactEvent.Action -> unit>) (cancel: ReactEvent.Action -> unit) (e: Browser.Types.KeyboardEvent) : unit =
        let actionEvent = ReactEvent.Keyboard.OfBrowserEvent e |> ReactEvent.Action.Make
        match e.key with
        | KeyboardEvent.Key.Enter  -> maybeSave |> Option.sideEffect (fun save -> save actionEvent )
        | KeyboardEvent.Key.Escape -> cancel actionEvent
        | _              -> Noop

    member _.OnJumpToPageInitialize (jumpToPage: LibClient.Components.Legacy.Input.PositiveInteger.InputPositiveIntegerRef) : unit =
        jumpToPage.SelectAll()
        jumpToPage.RequestFocus()

    member _.UpdateJumpToPage (page: LibClient.Components.Input.PositiveInteger.Value) : unit =
        this.setState (fun estate _ -> { estate with JumpToPage = page })


let Make = makeConstructor<Grid<'T>, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate

// TODO at some point we should probably have GoToPage return the proper result type in the first place,
// not silently swollow errors/asyncness
let goToPageAdapter (goToPage: (* pageSize *) PositiveInteger -> (* pageNumber *) PositiveInteger -> Option<ReactEvent.Action> -> unit) (pageSize: PositiveInteger) (pageNumber: PositiveInteger) : Async<Result<unit, string>> =
    async {
        goToPage pageSize pageNumber None
        return Ok ()
    }
