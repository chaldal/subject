module LibUiAdmin.Components.Legacy.QueryGrid

open LibClient
open LibClient.Components.Form.Base.Types
open LibClient.Services.Subscription
open LibUiAdmin

type PaginatedGridData<'Item> = LibUiAdmin.Components.Grid.PaginatedGridData<'Item>
type FormHandle<'Field, 'Acc, 'Acced when 'Field: comparison and 'Acc :> AbstractAcc<'Field, 'Acced>> = LibClient.Components.Form.Base.Types.FormHandle<'Field, 'Acc, 'Acced>

type Order =
| Ascending
| Descending

type Mode<'Item, 'Query> =
| OneTime                    of Execute: ('Query -> (* pageSize *) PositiveInteger -> (* pageNumber *) PositiveInteger -> Order -> Async<AsyncData<seq<'Item>>>)
| SubscribeWithoutTotalCount of MakeSubscription: ('Query -> (* pageSize *) PositiveInteger -> (* pageNumber *) PositiveInteger -> Order -> (LibClient.AsyncDataModule.AsyncData<seq<'Item>> -> unit) -> SubscribeResult)
| SubscribeWithTotalCount    of MakeSubscription: ('Query -> (* pageSize *) PositiveInteger -> (* pageNumber *) PositiveInteger -> Order -> (LibClient.AsyncDataModule.AsyncData<ItemsMaybeWithTotalCount<'Item>> -> unit) -> SubscribeResult)

type Props<'Item, 'QueryFormField, 'QueryAcc, 'Query when 'QueryFormField: comparison and 'QueryAcc :> AbstractAcc<'QueryFormField, 'Query>> = (* GenerateMakeFunction *) {
    Headers:                    ReactElement
    MakeRow:                    ('Item * (* lastRequestQuery *) Option<'Query> * (* refresh *) (unit -> unit)) -> ReactElement
    InitialQueryAcc:            'QueryAcc
    QueryForm:                  FormHandle<'QueryFormField, 'QueryAcc, 'Query> -> ReactElement
    Mode:                       Mode<'Item, 'Query>
    PageSizeChoices:            List<PositiveInteger> // default ([10; 20; 50; 100] |> List.map PositiveInteger.ofLiteral)
    InitialPageSize:            PositiveInteger // default PositiveInteger.ofLiteral 10
    ShouldSubmitOnMountIfValid: bool // default false

    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate<'Item, 'QueryFormField, 'QueryAcc, 'Query when 'QueryFormField: comparison and 'QueryAcc :> AbstractAcc<'QueryFormField, 'Query>> = {
    GridData:         PaginatedGridData<'Item>
    LastRequestQuery: Option<'Query>
}

type QueryGrid<'Item, 'QueryFormField, 'QueryAcc, 'Query when 'QueryFormField: comparison and 'QueryAcc :> AbstractAcc<'QueryFormField, 'Query>>(initialProps) =
    inherit EstatefulComponent<Props<'Item, 'QueryFormField, 'QueryAcc, 'Query>, Estate<'Item, 'QueryFormField, 'QueryAcc, 'Query>, Actions<'Item, 'QueryFormField, 'QueryAcc, 'Query>, QueryGrid<'Item, 'QueryFormField, 'QueryAcc, 'Query>>("LibUiAdmin.Components.Legacy.QueryGrid", initialProps, Actions<'Item, 'QueryFormField, 'QueryAcc, 'Query>, hasStyles = true)

    let mutable maybeCurrSubscriptionOff: Option<unit -> unit> = None

    override this.GetInitialEstate(initialProps: Props<'Item, 'QueryFormField, 'QueryAcc, 'Query>) =
        let goToPage = fun (pageSize: PositiveInteger) (pageNumber: PositiveInteger) (_e: Option<ReactEvent.Action>) ->
            this.state.LastRequestQuery
            |> Option.sideEffect (fun lastRequestQuery ->
                this.GoToPage lastRequestQuery pageSize pageNumber
                // TODO it may be better to run this through an executor
                // to properly display errors, but hacking for now
                |> Async.Ignore
                |> startSafely
            )

        let initialItems =
            match initialProps.ShouldSubmitOnMountIfValid with
            | true  -> WillStartFetchingSoonHack
            | false -> Available Seq.empty

        let gridData: PaginatedGridData<'Item> = {
            PageNumber          = PositiveInteger.One
            PageSize            = this.props.InitialPageSize
            MaybePageCount      = None
            Items               = initialItems
            GoToPage            = goToPage
            MaybeTotalItemCount = None
        }

        {
            GridData         = gridData
            LastRequestQuery = None
        }

    override this.ComponentDidMount () : unit =
        if this.props.ShouldSubmitOnMountIfValid then
            match this.props.InitialQueryAcc.Validate() with
            | Ok query ->
                this.GoToPage query this.state.GridData.PageSize PositiveInteger.One
                // TODO must run this through an executor (likely the form's, via a ref)
                // to properly display errors, but hacking for now
                |> Async.Ignore
                |> startSafely
            | Error _ ->
                this.SetEstate (fun estate _ -> { estate with GridData = {estate.GridData with Items = Available Seq.empty } })

    member this.GoToPage (query: 'Query) (pageSize: PositiveInteger) (pageNumber: PositiveInteger) : Async<Result<unit, string>> =
        async {
            this.SetEstate (fun estate _ ->
                { estate with
                    GridData         = { estate.GridData with Items = AsyncData.makeFetching estate.GridData.Items }
                    LastRequestQuery = Some query
                }
            )

            maybeCurrSubscriptionOff |> Option.sideEffect (fun currSubscriptionOff ->
                currSubscriptionOff()
                maybeCurrSubscriptionOff <- None
            )

            let setState (itemsMaybeWithTotalCountAD: AsyncData<ItemsMaybeWithTotalCount<'Item>>) =
                let maybePageCount: Option<UnsignedInteger> =
                    itemsMaybeWithTotalCountAD
                    |> AsyncData.toOption
                    |> Option.flatMap (fun itemsMaybeWithTotalCount ->
                        itemsMaybeWithTotalCount.MaybeTotalCount
                        |> Option.map (fun itemTotalCount ->
                            (double itemTotalCount) / (double pageSize.Value)
                            |> ceil
                            |> uint64
                            |> UnsignedInteger.ofUint64
                        )
                    )

                this.SetEstate (fun estate _ ->
                    { estate with
                        GridData =
                            { estate.GridData with
                                Items          = itemsMaybeWithTotalCountAD |> AsyncData.map ItemsMaybeWithTotalCount.items
                                PageNumber     = pageNumber
                                PageSize       = pageSize
                                MaybePageCount = maybePageCount |> Option.orElse estate.GridData.MaybePageCount
                            }
                    }
                )

            match this.props.Mode with
            | OneTime executeQuery ->
                let! itemsAsyncData = executeQuery query pageSize pageNumber Order.Ascending
                setState (itemsAsyncData |> AsyncData.map ItemsMaybeWithTotalCount.withoutTotalCount)

            | SubscribeWithoutTotalCount makeSubscription ->
                let subscription = makeSubscription query pageSize pageNumber Order.Ascending
                let subscribeResult = subscription (fun itemsAsyncData ->
                    setState (itemsAsyncData |> AsyncData.map ItemsMaybeWithTotalCount.withoutTotalCount)
                )

                maybeCurrSubscriptionOff <- Some subscribeResult.Off

            | SubscribeWithTotalCount makeSubscription ->
                let subscription = makeSubscription query pageSize pageNumber Order.Ascending
                let subscribeResult = subscription (fun itemsAsyncData ->
                    setState itemsAsyncData
                )

                maybeCurrSubscriptionOff <- Some subscribeResult.Off

            return Ok ()
        }

and Actions<'Item, 'QueryFormField, 'QueryAcc, 'Query when 'QueryFormField: comparison and 'QueryAcc :> AbstractAcc<'QueryFormField, 'Query>>(this: QueryGrid<'Item, 'QueryFormField, 'QueryAcc, 'Query>) =
    member _.Refresh () : unit =
        this.state.LastRequestQuery |> Option.sideEffect (fun lastQuery ->
            this.GoToPage lastQuery this.state.GridData.PageSize this.state.GridData.PageNumber
            // TODO must run this through an executor (likely the form's, via a ref)
            // to properly display errors, but hacking for now
            |> Async.Ignore
            |> startSafely
        )

    member _.Submit (query: 'Query) (_e: ReactEvent.Action) () : UDActionResult =
        this.GoToPage query this.state.GridData.PageSize PositiveInteger.One

let Make = makeConstructor<QueryGrid<'Item, 'QueryFormField, 'QueryAcc, 'Query>, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate
