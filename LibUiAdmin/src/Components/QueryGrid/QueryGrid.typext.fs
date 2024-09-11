module LibUiAdmin.Components.QueryGrid

open LibClient
open LibClient.Components.Form.Base.Types
open LibClient.Services.Subscription
open LibUiAdmin

type PaginatedGridData<'Item> = LibUiAdmin.Components.Grid.PaginatedGridData<'Item>

type Order =
| Ascending
| Descending

type QueryPage<'Query> = {
    Query:      'Query
    PageSize:   PositiveInteger
    PageNumber: PositiveInteger
    Order:      Order
}

type Page<'Query> =
| BlankPage of PageSize: PositiveInteger
| QueryPage of QueryPage<'Query>
with
    member this.PageSize : PositiveInteger =
        match this with
        | BlankPage pageSize  -> pageSize
        | QueryPage queryPage -> queryPage.PageSize

    member this.PageNumber : PositiveInteger =
        match this with
        | BlankPage _         -> PositiveInteger.One
        | QueryPage queryPage -> queryPage.PageNumber

    member this.MaybeQuery : Option<'Query> =
        match this with
        | BlankPage _         -> None
        | QueryPage queryPage -> Some queryPage.Query

module Page =
    let ofQuery (query: 'Query) : Page<'Query> =
        {
            Query      = query
            PageSize   = PositiveInteger.ofLiteral 50
            PageNumber = PositiveInteger.One
            Order      = Ascending
        }
        |> QueryPage

    let maybeQuery (value: Page<'Query>) : Option<'Query> =
        value.MaybeQuery

    let withSize (size: PositiveInteger) (value: Page<'Query>) : Page<'Query> =
        match value with
        | BlankPage _         -> BlankPage size
        | QueryPage queryPage -> QueryPage { queryPage with PageSize = size }

    let withNumber (number: PositiveInteger) (value: Page<'Query>) : Page<'Query> =
        match value with
        | BlankPage size       -> BlankPage size // shouldn't be called in this case, so we're okay to not model for this possibility
        | QueryPage queryPage  -> QueryPage { queryPage with PageNumber = number }

type Mode<'Item, 'Query> =
| OneTime                    of Execute: (QueryPage<'Query> -> Async<AsyncData<seq<'Item>>>)
| SubscribeWithoutTotalCount of MakeSubscription: (QueryPage<'Query> -> (LibClient.AsyncDataModule.AsyncData<seq<'Item>> -> unit) -> SubscribeResult)
| SubscribeWithTotalCount    of MakeSubscription: (QueryPage<'Query> -> (LibClient.AsyncDataModule.AsyncData<ItemsMaybeWithTotalCount<'Item>> -> unit) -> SubscribeResult)

type Props<'Item, 'QueryFormField, 'QueryAcc, 'Query when 'QueryFormField: comparison and 'Query : equality and 'QueryAcc :> AbstractAcc<'QueryFormField, 'Query>> = (* GenerateMakeFunction *) {
    Mode:            Mode<'Item, 'Query>

    Page:            Page<'Query>
    OnPageChange:    Page<'Query> -> unit
    InitialQueryAcc: 'QueryAcc

    Headers:         ReactElement
    Row:             'Item * (* currQueryPage *) Option<QueryPage<'Query>> * (* refresh *) (unit -> unit) -> ReactElement
    HandheldRow:     Option<('Item * (* currQueryPage *) Option<QueryPage<'Query>> * (* refresh *) (unit -> unit) -> ReactElement)> // defaultWithAutoWrap None
    QueryForm:       Option<FormHandle<'QueryFormField, 'QueryAcc, 'Query> -> ReactElement>     // defaultWithAutoWrap None
    CustomRender:    ((* Form *) ReactElement * (* Grid *) ReactElement -> ReactElement) option // defaultWithAutoWrap None

    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate<'Item, 'QueryFormField, 'QueryAcc, 'Query when 'QueryFormField: comparison and 'Query : equality and 'QueryAcc :> AbstractAcc<'QueryFormField, 'Query>> = {
    GridData:              PaginatedGridData<'Item>
    MaybeCurrentQueryPage: Option<QueryPage<'Query>>
}

type QueryGrid<'Item, 'QueryFormField, 'QueryAcc, 'Query when 'QueryFormField: comparison and 'Query : equality and 'QueryAcc :> AbstractAcc<'QueryFormField, 'Query>>(_initialProps) =
    inherit EstatefulComponent<Props<'Item, 'QueryFormField, 'QueryAcc, 'Query>, Estate<'Item, 'QueryFormField, 'QueryAcc, 'Query>, Actions<'Item, 'QueryFormField, 'QueryAcc, 'Query>, QueryGrid<'Item, 'QueryFormField, 'QueryAcc, 'Query>>("LibUiAdmin.Components.QueryGrid", _initialProps, Actions<'Item, 'QueryFormField, 'QueryAcc, 'Query>, hasStyles = true)

    let mutable maybeCurrSubscriptionOff: Option<unit -> unit> = None

    member this.GoToPage (pageSize: PositiveInteger) (pageNumber: PositiveInteger) (_e: Option<ReactEvent.Action>) : unit =
        this.props.Page
        |> Page.withSize   pageSize
        |> Page.withNumber pageNumber
        |> this.props.OnPageChange

    override this.GetInitialEstate(initialProps: Props<'Item, 'QueryFormField, 'QueryAcc, 'Query>) : Estate<'Item, 'QueryFormField, 'QueryAcc, 'Query> =
        let initialItems =
            match initialProps.Page with
            | QueryPage _ -> WillStartFetchingSoonHack
            | BlankPage _ -> Available Seq.empty

        let gridData: PaginatedGridData<'Item> = {
            PageNumber          = initialProps.Page.PageNumber
            PageSize            = initialProps.Page.PageSize
            MaybePageCount      = None
            Items               = initialItems
            GoToPage            = this.GoToPage
            MaybeTotalItemCount = None
        }

        {
            GridData              = gridData
            MaybeCurrentQueryPage = None
        }

    override this.ComponentDidMount () : unit =
        this.LoadPage this.props.Page

    override this.ComponentWillReceiveProps (nextProps: Props<'Item, 'QueryFormField, 'QueryAcc, 'Query>) : unit =
        if this.props.Page <> nextProps.Page then this.LoadPage nextProps.Page

    member private this.PaginatedGridDataForInitialPageSize (size: PositiveInteger) : PaginatedGridData<'Item> =
        {
            PageNumber          = PositiveInteger.One
            PageSize            = size
            MaybePageCount      = None
            Items               = Available Seq.empty
            GoToPage            = this.GoToPage
            MaybeTotalItemCount = None
        }

    member this.Refresh () : unit =
        this.LoadPage this.props.Page

    member private this.LoadPage (page: Page<'Query>) : unit =
        maybeCurrSubscriptionOff |> Option.sideEffect (fun currSubscriptionOff ->
            currSubscriptionOff()
            maybeCurrSubscriptionOff <- None
        )

        this.SetEstate (fun estate _ -> { estate with GridData = estate.GridData.MakeFetching })

        match page with
        | BlankPage pageSize -> this.SetEstate (fun estate _ -> { estate with GridData = this.PaginatedGridDataForInitialPageSize pageSize })
        | QueryPage queryPage ->
            match this.props.Mode with
            | OneTime executeQuery ->
                async {
                    let! itemsAsyncData = executeQuery queryPage
                    this.SetLoadedPageIfQueryPageStillMatches queryPage (itemsAsyncData |> AsyncData.map ItemsMaybeWithTotalCount.withoutTotalCount)
                } |> startSafely

            | SubscribeWithoutTotalCount makeSubscription ->
                let subscription = makeSubscription queryPage
                let subscribeResult = subscription (fun itemsAsyncData ->
                    this.SetLoadedPageIfQueryPageStillMatches queryPage (itemsAsyncData |> AsyncData.map ItemsMaybeWithTotalCount.withoutTotalCount)
                )

                maybeCurrSubscriptionOff <- Some subscribeResult.Off

            | SubscribeWithTotalCount makeSubscription ->
                let subscription = makeSubscription queryPage
                let subscribeResult = subscription (fun itemsAsyncData ->
                    this.SetLoadedPageIfQueryPageStillMatches queryPage itemsAsyncData
                )

                maybeCurrSubscriptionOff <- Some subscribeResult.Off


    member private this.SetLoadedPageIfQueryPageStillMatches (queryPage: QueryPage<'Query>) (itemsMaybeWithTotalCountAD: AsyncData<ItemsMaybeWithTotalCount<'Item>>) : unit =
        // NOTE we need to make sure that if LoadPage was called from within ComponentWillReceiveProps,
        // that by the time this function is called, nextProps has made it all the way into this.props,
        // otherwise our equality check will fail, hence we run on the next tick.
        LibClient.JsInterop.runOnNextTick (fun () ->
            if this.props.Page = QueryPage queryPage then
                let (maybePageCount: Option<UnsignedInteger>, maybeTotalItemCount: Option<UnsignedInteger>) =
                    itemsMaybeWithTotalCountAD
                    |> AsyncData.toOption
                    |> Option.flatMap (fun itemsMaybeWithTotalCount ->
                        itemsMaybeWithTotalCount.MaybeTotalCount
                        |> Option.map (fun itemTotalCount ->
                            let maybeTotalItemCount = itemTotalCount |> UnsignedInteger.ofUint64 |> Some
                            let maybePageCount =
                                (double itemTotalCount) / (double queryPage.PageSize.Value)
                                |> ceil
                                |> uint64
                                |> UnsignedInteger.ofUint64
                                |> Some

                            (maybePageCount, maybeTotalItemCount)
                        )
                    )
                    |> Option.mapOrElse (None, None) id

                this.SetEstate (fun estate _ ->
                    { estate with
                        GridData =
                            { estate.GridData with
                                Items               = itemsMaybeWithTotalCountAD |> AsyncData.map ItemsMaybeWithTotalCount.items
                                PageNumber          = queryPage.PageNumber
                                PageSize            = queryPage.PageSize
                                MaybePageCount      = maybePageCount |> Option.orElse estate.GridData.MaybePageCount
                                MaybeTotalItemCount = maybeTotalItemCount
                            }
                        MaybeCurrentQueryPage = Some queryPage
                    }
                )
        )

and Actions<'Item, 'QueryFormField, 'QueryAcc, 'Query when 'QueryFormField: comparison and 'Query : equality and 'QueryAcc :> AbstractAcc<'QueryFormField, 'Query>>(this: QueryGrid<'Item, 'QueryFormField, 'QueryAcc, 'Query>) =
    member _.Submit (query: 'Query) (_e: ReactEvent.Action) () : UDActionResult =
        {
            Query      = query
            PageSize   = this.props.Page.PageSize
            PageNumber = PositiveInteger.One
            Order      = Ascending
        }
        |> QueryPage
        |> this.props.OnPageChange

        // this is a little hacky, but the connection between the submission of the form
        // and the async processing of the query is external to this component, which
        // makes it rather difficult to wrap into the same UDAction. So we'll try this hack for
        // now, in the hope that the "in progress-ness" that we need to show to the user,
        // along with error handling, are still achieved correctly.
        Ok () |> Async.Of

    member _.Refresh () : unit =
        this.Refresh ()

let Make = makeConstructor<QueryGrid<'Item, 'QueryFormField, 'QueryAcc, 'Query>, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate
