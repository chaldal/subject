[<AutoOpen>]
module LibUiSubjectAdmin.Types

open LibClient
open LibClient.Services.Subscription
open LibUiSubject
open LibUiAdmin

type LibUiAdmin.Components.Legacy.QueryGrid.Order with
    member this.ToSubjectOrderDirection : OrderDirection =
        match this with
        | LibUiAdmin.Components.Legacy.QueryGrid.Order.Ascending  -> OrderDirection.Ascending
        | LibUiAdmin.Components.Legacy.QueryGrid.Order.Descending -> OrderDirection.Descending

module QueryGrid =
    type SubjectQuery<'Id, 'NumericIndex, 'StringIndex, 'SearchIndex, 'GeographyIndex, 'OpError
                        when 'NumericIndex :> SubjectNumericIndex<'OpError>
                        and  'StringIndex  :> SubjectStringIndex<'OpError>
                        and  'SearchIndex  :> SubjectSearchIndex
                        and  'GeographyIndex  :> SubjectGeographyIndex
                        and  'OpError      :> OpError> =
    | All
    | Indexed of IndexPredicate<'NumericIndex, 'StringIndex, 'SearchIndex, 'GeographyIndex, 'OpError>
    | One     of 'Id

    type PaginatedSubjectQuery<'Id, 'NumericIndex, 'StringIndex, 'SearchIndex, 'GeographyIndex, 'OpError
                                 when 'NumericIndex :> SubjectNumericIndex<'OpError>
                                 and  'StringIndex  :> SubjectStringIndex<'OpError>
                                 and  'SearchIndex  :> SubjectSearchIndex
                                 and  'GeographyIndex  :> SubjectGeographyIndex
                                 and  'OpError      :> OpError> = {
        Query:     SubjectQuery<'Id, 'NumericIndex, 'StringIndex, 'SearchIndex, 'GeographyIndex, 'OpError>
        MaybePage: Option<ResultPage>
        OrderBy:   OrderBy<'NumericIndex, 'StringIndex, 'OpError>
    }

type LibUiAdmin.Components.QueryGrid.QueryPage<'Query> with
    member this.ToResultSetOptions<'SubjectId, 'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError
                                    when 'SubjectNumericIndex :> SubjectNumericIndex<'OpError>
                                    and  'SubjectStringIndex  :> SubjectStringIndex<'OpError>
                                    and  'SubjectSearchIndex  :> SubjectSearchIndex
                                    and  'SubjectGeographyIndex  :> SubjectGeographyIndex
                                    and  'OpError      :> OpError>()
            : ResultSetOptions<'SubjectNumericIndex, 'SubjectStringIndex, 'OpError> =
        {
            Page = {
                Size   = this.PageSize.Value |> uint16
                Offset = (this.PageNumber.Value - 1) * this.PageSize.Value |> uint64
            }
            OrderBy =
                match box this.Query with
                | :? QueryGrid.PaginatedSubjectQuery<'SubjectId, 'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError> as paginatedQuery ->
                    paginatedQuery.OrderBy
                | _ ->
                    match this.Order with
                    | _ -> OrderBy.FastestOrSingleSearchScoreIfAvailable
        }

open QueryGrid

let makeSubscriptionAdapter<'Query, 'Id, 'T, 'Predicate, 'OrderBy, 'NumericIndex, 'StringIndex, 'SearchIndex, 'GeographyIndex, 'OpError
                             when 'NumericIndex :> SubjectNumericIndex<'OpError>
                             and  'StringIndex  :> SubjectStringIndex<'OpError>
                             and  'SearchIndex  :> SubjectSearchIndex
                             and  'GeographyIndex  :> SubjectGeographyIndex
                             and  'OpError      :> OpError>
    (realMakeSubscription: QueryGrid.PaginatedSubjectQuery<'Id, 'NumericIndex, 'StringIndex, 'SearchIndex, 'GeographyIndex, 'OpError> -> (AsyncData<'T> -> unit) -> SubscribeResult)
    (rawPaginatedSubjectQuery: QueryGrid.PaginatedSubjectQuery<'Id, 'NumericIndex, 'StringIndex, 'SearchIndex, 'GeographyIndex, 'OpError>)
    (pageSize: PositiveInteger)
    (pageNumber: PositiveInteger)
    (_order: LibUiAdmin.Components.Legacy.QueryGrid.Order)
    : (AsyncData<'T> -> unit) -> SubscribeResult =
    // TODO don't ignore order?
    { rawPaginatedSubjectQuery with
        MaybePage =
            Some {
                Size   = pageSize.Value |> uint16
                Offset = (pageNumber.Value - 1) * pageSize.Value |> uint64
            }
    }
    |> realMakeSubscription

open LibUiAdmin.Components.Legacy.QueryGrid
open LibUiSubject.Services.SubjectService

let makeIndexQuerySubscriptionAdapterWithCount (query: IndexQuery<'Index>) (service: ISubjectService<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>) =
    (fun subscriber ->
        let internalSubscriber = (fun subjectsWithTotalCountAD ->
            subjectsWithTotalCountAD
            |> AsyncData.map (fun subjectsWithTotalCount ->
                {
                    Items           = subjectsWithTotalCount.Subjects
                    MaybeTotalCount = Some subjectsWithTotalCount.TotalCount
                }
            )
            |> subscriber
        )
        // NOTE this used to have UseCache.No; having it be IfReasonablyFresh lets us
        // use the cache on back navigation, i.e. the page reloads faster and scroll
        // restoration doesn't time out. If there's a good reason it had to be UseCache.No,
        // we should switch it back and document the reason here.
        service.SubscribeIndexedWithTotalCount query UseCache.IfReasonablyFresh internalSubscriber
    )


let makeQuerySubscriptionAdapterWithCount (query: Query<'Id, 'Index, 'OpError>) (service: ISubjectService<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>) =
    (fun subscriber ->
        let internalSubscriber = (fun subjectsWithTotalCountAD ->
            subjectsWithTotalCountAD
            |> AsyncData.map (fun subjectsWithTotalCount ->
                {
                    Items           = subjectsWithTotalCount.Subjects
                    MaybeTotalCount = Some subjectsWithTotalCount.TotalCount
                }
            )
            |> subscriber
        )
        // NOTE this used to have UseCache.No; having it be IfReasonablyFresh lets us
        // use the cache on back navigation, i.e. the page reloads faster and scroll
        // restoration doesn't time out. If there's a good reason it had to be UseCache.No,
        // we should switch it back and document the reason here.
        service.SubscribeQueryWithTotalCount query UseCache.IfReasonablyFresh internalSubscriber
    )

let getMaybeQueryWithUpdatedPageSize
    (maybeInitialQuery: Option<QueryGrid.PaginatedSubjectQuery<'Id, 'NumericIndex, 'StringIndex, 'SearchIndex, 'GeographyIndex, 'OpError>>)
    (paginatedSubjectQuery: QueryGrid.PaginatedSubjectQuery<'Id, 'NumericIndex, 'StringIndex, 'SearchIndex, 'GeographyIndex, 'OpError>)
    : Option<QueryGrid.PaginatedSubjectQuery<'Id, 'NumericIndex, 'StringIndex, 'SearchIndex, 'GeographyIndex, 'OpError>> =

    maybeInitialQuery
    |> Option.map(fun initialQuery ->
        if initialQuery.Query = paginatedSubjectQuery.Query && initialQuery.MaybePage <> paginatedSubjectQuery.MaybePage then
            initialQuery
        else
            paginatedSubjectQuery)
