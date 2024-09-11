[<AutoOpen>]
module LibClient.Components.PaginatedVirtualListView

open Fable.React

open ReactXP.Components
open ReactXP.LegacyStyles.RulesRestricted
open ReactXP.Styles
open ReactXP.Types

open LibClient
open LibClient.Components

type PaginatedVirtualListItem<'Item> = {
    Key:      string
    Item:     'Item
    Height:   Option<int>
    Template: string
}

type private PaginatedVirtualListItems =
| CustomListItem    of PaginatedVirtualListObject
| TopContent        of ReactElement
| BottomContent     of ReactElement
| DataFetchingError of string * retry: (unit -> Async<unit>)
| Loader            of height: int
| BottomReached
| NoItem
    with
        member this.KeyAndMaybeHeight : string * Option<int> =
            match this with
            | CustomListItem item -> (item.Key, item.Height)
            | TopContent     _    -> ("TopContent",        None)
            | BottomContent  _    -> ("BottomContent",     None)
            | Loader height       -> ("Loader",            Some height)
            | BottomReached       -> ("BottomReached",     Some 50)
            | DataFetchingError _ -> ("DataFetchingError", Some 160)
            | NoItem              -> ("NoItem",            Some 25)

        member this.ToRX : VirtualListViewItemInfo =
            let key, height = this.KeyAndMaybeHeight
            {
                key                          = key
                height                       = height |> Option.defaultValue 1
                payload                      = this
                measureHeight                = Some height.IsNone
                template                     =
                    match this with
                    | CustomListItem item -> item.Template
                    | _ -> unionCaseName this
                isNavigable                  = Some false
                disableTouchOpacityAnimation = true
            }

        member this.getKey : string =
            let key, _ = this.KeyAndMaybeHeight
            key
and private PaginatedVirtualListObject = {
    Key:      string
    Item:     obj
    Height:   Option<int>
    Template: string
}

module private Styles =
    let center = makeViewStyles {
         AlignItems.Center
         JustifyContent.Center
         flex 1
    }
    let bottomReached = makeTextStyles {
        marginTop 20
        fontSize  18
        color (Color.Rgb(128, 128, 128))
    }
    let errorContainer = makeViewStyles {
        padding 10
    }
    let error = makeTextStyles {
        fontSize        12
        borderRadius    5
        padding         5
        color           Color.White
        backgroundColor (Color.Rgba (226, 47, 47, 0.85))
    }
    let retryButtonContainer = makeViewStyles {
        margin          20
        padding         12
        borderRadius    5
        maxWidth        (int ((float (ReactXP.UserInterface.windowLayoutInfo().width)) * 0.5))
        backgroundColor (Color.Rgba (226, 47, 47, 0.7))
    }
    let retryButtonText = makeTextStyles {
        fontSize   16
        color      Color.White
        FontWeight.Bold
        AlignItems.Center
    }

    let noItems  = makeTextStyles {
        TextAlign.Center
        marginTop 16
        color (Color.Grey "16")
    }

type private Helpers =
    [<Component>]
    static member _PaginatedVirtualListView (
            dataFetcher:          uint64 -> Async<AsyncData<PaginatedVirtualListObject seq>>,
            renderItem:           PaginatedVirtualListObject -> ReactElement,
            pageSize:             uint16,
            ?topStaticContent:    ReactElement,
            ?bottomStaticContent: ReactElement,
            ?whenLoading:         ReactElement * (* Height *) int,
            ?heightThreshold:     int,
            ?styles:              array<ViewStyles>,
            ?key:                 string
        ) : ReactElement =
            key |> ignore

            let allItems:         IStateHook<seq<PaginatedVirtualListItems>>       = Hooks.useState Seq.empty
            let nextItems:        IStateHook<seq<PaginatedVirtualListItems>option> = Hooks.useState None
            let nextPage:         IStateHook<uint64>                               = Hooks.useState 0UL
            let dataFetching:     IStateHook<DataFetchingState>                    = Hooks.useState DataFetchingState.Loading
            let hasEndReached:    IStateHook<bool>                                 = Hooks.useState false
            let nextLoadHeight:   IStateHook<int>                                  = Hooks.useState 0
            // This could produce potential bugs when the scroll view height is not the full height of the screen.
            let minLoadThreshold: int                                              = (ReactXP.UserInterface.windowLayoutInfo().height * (defaultArg heightThreshold 4))

            let appendItems (currentItems: PaginatedVirtualListItems seq) (source: PaginatedVirtualListItems seq) : PaginatedVirtualListItems seq =
                source
                |> Seq.append currentItems
                |> Seq.distinctBy (fun item -> item.getKey)

            let removeItems (key: string) (source: PaginatedVirtualListItems seq) : PaginatedVirtualListItems seq =
                source
                |> Seq.filterMap (fun item ->
                    if key = item.getKey then
                        None
                    else
                        Some item
                )
                |> Seq.distinctBy (fun item -> item.getKey)

            let startSafely (what: Async<unit>) : unit =
                async {
                    match! what |> Async.TryCatch with
                    | Ok _    -> Noop
                    | Error e ->
                        Log.Error ("Error in AsyncHelpers.startSafely: {error}", e.ToString())
                        dataFetching.update DataFetchingState.FetchingFailed
                }
                |> Async.StartImmediate

            let storeData (data: seq<PaginatedVirtualListObject>) =
                let updatedData =
                    data
                    |> Seq.map PaginatedVirtualListItems.CustomListItem

                nextItems.update (Some updatedData)
                updatedData
                |> Seq.append allItems.current
                |> allItems.update

            let getAndUpdateItems (currentPage: uint64) = async {
                dataFetching.update DataFetchingState.Loading

                match! dataFetcher currentPage with
                | AsyncData.Available data ->
                    if data |> Seq.isEmpty then
                        dataFetching.update DataFetchingState.NoItem
                    else
                        storeData data

                        if data |> Seq.length < int pageSize then
                            dataFetching.update DataFetchingState.EndReached
                            hasEndReached.update true
                        else
                            nextPage.update (currentPage + 1UL)
                            dataFetching.update DataFetchingState.Completed
                | AsyncData.Failed _
                | AsyncData.AccessDenied
                | AsyncData.Unavailable  -> dataFetching.update DataFetchingState.FetchingFailed
                | _ -> ()
            }

            let loadNextPage (page: uint64) = async {
                match nextItems.current with
                | Some value->
                    nextItems.update None
                    value
                    |> appendItems allItems.current
                    |> removeItems "Loader"
                    |> allItems.update
                | None ->
                    match dataFetching.current with
                    | Loading
                    | Completed
                    | FetchingFailed ->
                        do! getAndUpdateItems (page)
                    | _ -> Nothing
            }

            Hooks.useEffect (
                (fun () ->
                    loadNextPage (nextPage.current)
                    |> startSafely
                ),
                [||]
            )

            Hooks.useEffect (
                (fun () ->
                    match allItems.current |> Seq.isEmpty with
                    | true -> Nothing
                    | false ->
                        nextLoadHeight.update ((allItems.current |> Seq.map (fun item -> item.ToRX.height) |> Seq.sum) - minLoadThreshold)
                        match nextItems.current, hasEndReached.current with
                        | None, false ->
                            loadNextPage (nextPage.current)
                            |> startSafely
                        | _ -> Nothing
                ),
                [| allItems.current |]
            )

            let render (item : PaginatedVirtualListItems) : ReactElement =
                match item with
                | PaginatedVirtualListItems.CustomListItem item ->
                    renderItem item
                | PaginatedVirtualListItems.Loader _ ->
                    match whenLoading with
                    | None ->
                        RX.View (styles = [| Styles.center |], children = [|
                            RX.ActivityIndicator(color = "#aaaaaa")
                        |])
                    | Some (customLoader, _) ->
                        customLoader
                | PaginatedVirtualListItems.BottomReached ->
                    RX.View (styles = [| Styles.center |], children = [|
                        LC.Text ("-x-", styles = [| Styles.bottomReached |])
                    |])
                | PaginatedVirtualListItems.NoItem ->
                    LC.UiText (
                        styles   = [| Styles.noItems |],
                        children = [| LC.Text "No Items" |]
                    )
                | PaginatedVirtualListItems.TopContent content    -> content
                | PaginatedVirtualListItems.BottomContent content -> content
                | PaginatedVirtualListItems.DataFetchingError (message, retry) ->
                    element {
                        RX.View (styles = [| Styles.center; Styles.errorContainer |], children = [|
                            LC.Text (message, styles = [| Styles.error |])

                            RX.View (styles = [| Styles.retryButtonContainer |], children = [|
                                LC.Text ("Retry", styles  = [| Styles.retryButtonText |])

                                LC.TapCapture (onPress = (fun _ -> retry () |> startSafely))
                            |])
                        |])
                    }

            let paginatedVirtualListItems = (
                match bottomStaticContent with
                | None         -> Seq.empty
                | Some content -> seq { PaginatedVirtualListItems.TopContent content }
                |> appendItems
                    (
                        (
                            match dataFetching.current with
                            | DataFetchingState.Loading ->
                                match whenLoading with
                                | None             -> seq { PaginatedVirtualListItems.Loader 180 }
                                | Some (_, height) -> seq { PaginatedVirtualListItems.Loader height }
                            | DataFetchingState.Completed      -> Seq.empty
                            | DataFetchingState.FetchingFailed -> seq { PaginatedVirtualListItems.DataFetchingError ("Apologies, there is an internet issue. Please try again.", (fun _ -> loadNextPage nextPage.current)) }
                            | DataFetchingState.EndReached     -> seq { PaginatedVirtualListItems.BottomReached }
                            | DataFetchingState.NoItem         -> seq { PaginatedVirtualListItems.NoItem }
                        )
                        |> appendItems
                            (
                                allItems.current
                                |> appendItems
                                    (
                                        match topStaticContent with
                                        | None         -> Seq.empty
                                        | Some content -> seq { PaginatedVirtualListItems.TopContent content }
                                    )
                            )
                    )
            )

            let memorizedRender = Hooks.useMemo (
                (fun () ->
                    (fun (virtualListCellRenderDetails: ReactXP.Components.VirtualListView.VirtualListCellRenderDetails) ->
                        virtualListCellRenderDetails.GetItem<PaginatedVirtualListItems>()
                        |> render
                    )
                ),
                [||]
            )

            element {
                RX.VirtualListView (
                    renderItem          = memorizedRender,
                    scrollEventThrottle = 10,
                    itemList            = (paginatedVirtualListItems |> Seq.map (fun item -> item.ToRX) |> Array.ofSeq),
                    onScroll            = (fun (top, _) ->
                        if top > nextLoadHeight.current && not hasEndReached.current && dataFetching.current = DataFetchingState.Completed then
                             loadNextPage nextPage.current
                             |> startSafely
                    ),
                    ?styles = styles
                )
            }

type LibClient.Components.Constructors.LC with
    [<Component>]
    static member PaginatedVirtualListView (
            dataFetcher:          uint64 -> Async<AsyncData<PaginatedVirtualListItem<'Item> seq>>,
            renderItem:           'Item -> ReactElement,
            pageSize:             uint16,
            key:                  string,
            ?whenLoading:         ReactElement * (*Height*) int,
            ?topStaticContent:    ReactElement,
            ?bottomStaticContent: ReactElement,
            ?heightThreshold:     int,
            ?styles:              array<ViewStyles>
        ) : ReactElement =

        element {
            let dataMapper = (fun (items: Async<AsyncData<PaginatedVirtualListItem<'Item> seq>>) ->
                items
                |> Async.Map (fun asyncData ->
                    asyncData
                    |> AsyncData.map (fun data ->
                        data
                        |> Seq.map ( fun item -> {
                            Key      = item.Key
                            Template = item.Template
                            Item     = item.Item :> obj
                            Height   = item.Height
                        })
                    )
                )
            )

            Helpers._PaginatedVirtualListView (
                dataFetcher          = (dataFetcher >> dataMapper),
                renderItem           = (fun item -> renderItem (item.Item :?> 'Item)),
                pageSize             = pageSize,
                ?topStaticContent    = topStaticContent,
                ?bottomStaticContent = bottomStaticContent,
                ?whenLoading         = whenLoading,
                ?heightThreshold     = heightThreshold,
                ?styles              = styles,
                key                  = key
            )
        }