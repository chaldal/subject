[<AutoOpen>]
module LibClient.Components.PaginatedScrollView

open Fable.React

open LibClient.Responsive

open LibClient
open LibClient.Components
open LibClient.Components.ScrollView

open ReactXP
open ReactXP.Styles
open ReactXP.Components

type DataFetchingState =
| Loading
| Completed
| FetchingFailed
| EndReached
| NoItem

type ContentWidth =
| Full
| Fixed of int

module private Styles =
    let scrollViewContent = makeViewStyles {
        FlexDirection.Column
        flex 1
        Overflow.Hidden
        AlignItems.Stretch
        JustifyContent.Center
    }
    
    let content_container = makeViewStyles {
        flex 1
        FlexDirection.Row
        AlignItems.Stretch
        JustifyContent.Center
    }
    
    let content = ViewStyles.Memoize (fun (contentWidth: ContentWidth) -> makeViewStyles {
        flex 1
        match contentWidth with
        | ContentWidth.Fixed width -> maxWidth width
        | _ -> ()
    })
    
    let center = makeViewStyles {
        padding 10
        AlignItems.Center
    }
    
    let bottomReached = makeTextStyles {
        marginTop    20
        marginBottom 20
        fontSize     32
        color        (Color.Rgb(128, 128, 128))
    }
    
    let error = makeTextStyles {
        margin          20
        fontSize        16
        borderRadius    5
        padding         10
        color           Color.White
        backgroundColor (Color.Rgba (226, 47, 47, 0.7))
        FontWeight.Bold
    }

    let retryButtonContainer = makeViewStyles {
        margin          20
        padding         12
        borderRadius    5
        backgroundColor (Color.Rgba (226, 47, 47, 0.7))
    }

    let retryButtonText = makeTextStyles {
        fontSize   16
        color      Color.White
        FontWeight.Bold
        AlignItems.Center
    }

    let footer = makeViewStyles {
        flexShrink 0
        flexGrow   0
        FlexDirection.Column
        AlignItems.Stretch
    }
    
    let scrollView = makeViewStyles {
        FlexDirection.Column
        minHeight (UserInterface.windowLayoutInfo().height)
        Overflow.Hidden
    }

    let noItems  = makeTextStyles {
        TextAlign.Center
        marginTop 16
        color (Color.Grey "16")
    }

type private Helpers =
    [<Component>]
    static member StaticContent (maybeContent :ReactElements option): ReactElement =
        match maybeContent with
        | None          -> nothing
        | Some elements -> element { elements }

type LibClient.Components.Constructors.LC with
    [<Component>]
    static member PaginatedScrollView (
            renderItem:           'Data  -> ReactElement,
            dataFetcher:          uint64 -> Async<AsyncData<'Data seq>>,
            ?topStaticContent:    ReactElements,
            ?bottomStaticContent: ReactElements,
            ?heightThreshold:     int,
            ?restoreKey:          string,
            ?onScroll:            int * int -> unit,
            ?styles:              array<ViewStyles>,
            ?paginatedStyle:      array<ViewStyles>,
            ?contentWidth:        ContentWidth,
            ?key:                 string
        ) : ReactElement =
            key |> ignore
            
            let contentWidth: ContentWidth                  = defaultArg contentWidth Full
            let allItems:     IStateHook<'Data seq>         = Hooks.useState Array.empty
            let cache:        IStateHook<'Data seq option>  = Hooks.useState None
            let pageNo:       IStateHook<uint64>            = Hooks.useState 0UL
            let dataFetching: IStateHook<DataFetchingState> = Hooks.useState DataFetchingState.Loading
            
            let startSafely (what: Async<unit>) : unit =
                async {
                    match! what |> Async.TryCatch with
                    | Ok _    -> Noop
                    | Error e ->
                        Log.Error ("Error in AsyncHelpers.startSafely: {error}", e.ToString())
                        dataFetching.update DataFetchingState.FetchingFailed
                }
                |> Async.StartImmediate

            let getAndUpdateItems () = async {
                dataFetching.update DataFetchingState.Loading
                
                match! dataFetcher pageNo.current with
                | AsyncData.Available data ->
                    match data |> Seq.isEmpty with
                    | true ->
                        if allItems.current |> Seq.isEmpty then
                            dataFetching.update DataFetchingState.NoItem
                        else
                            dataFetching.update DataFetchingState.EndReached
                    | false -> 
                        if (allItems.current |> Seq.isEmpty) then
                            allItems.update data
                            pageNo.update (pageNo.current + 1UL)
                        else
                            cache.update (data |> Some)
                        dataFetching.update DataFetchingState.Completed
                | AsyncData.Failed _    
                | AsyncData.AccessDenied
                | AsyncData.Unavailable  -> dataFetching.update DataFetchingState.FetchingFailed
                | _ -> ()
            }

            let loadNextPage () = async {
                match cache.current with
                | Some value->
                    cache.update None
                    pageNo.update (pageNo.current + 1UL)
                    allItems.update (Seq.append allItems.current value)
                | None ->
                    match dataFetching.current with
                    | Loading
                    | Completed
                    | FetchingFailed ->
                        do! getAndUpdateItems ()
                    | _ -> Nothing
            }

            Hooks.useEffect (
                (fun () ->
                    async {
                        if dataFetching.current <> DataFetchingState.EndReached then
                            if (allItems.current |> Seq.isEmpty) then 
                                do! loadNextPage ()
                            elif cache.current = None then
                                do! loadNextPage ()
                    }
                    |> startSafely
                ),
                [| allItems.current |]
            )

            let windowHeight = UserInterface.windowLayoutInfo().height

            let onScrollCustom (maybeLayout: Layout option) =
                let layoutHeight =
                    match Runtime.platform with
                    | Native _ ->  windowHeight
                    | Web _ ->
                        match maybeLayout with
                        | None -> windowHeight
                        | Some layout -> layout.Height

                (fun (top, right) ->
                    onScroll |> Option.sideEffect (fun onScroll -> onScroll (top, right))
                    if (layoutHeight - top) < (windowHeight * (defaultArg heightThreshold 2)) && dataFetching.current = DataFetchingState.Completed then
                        async {
                            do! loadNextPage ()
                        }
                        |> startSafely
                )
            
            element {
                LC.With.ScreenSize (fun screenSize ->
                    LC.With.Layout (
                        fun (onLayoutOption, maybeLayout) ->
                            LC.ScrollView (
                                restoreScroll = (
                                    match restoreKey with
                                    | None ->
                                        RestoreScroll.No
                                    | Some key -> 
                                        RestoreScroll.WhenContentApproximatelyMatchesOriginalHeight key
                                ),
                                ?onScroll      = (
                                    match screenSize, dataFetching.current = DataFetchingState.EndReached with
                                    | ScreenSize.Desktop, false  -> Some (onScrollCustom maybeLayout)
                                    | _                          -> onScroll
                                ),
                                ?onLayout     = onLayoutOption,
                                scroll        = (
                                    match screenSize with
                                    | ScreenSize.Handheld -> Scroll.NoScroll
                                    | ScreenSize.Desktop  -> Scroll.Vertical
                                ),
                                styles = [| Styles.scrollView |],
                                children      = [|
                                    RX.View (styles = Array.append (defaultArg styles [||]) [| Styles.scrollViewContent |], children = [|
                                        RX.View (styles = [| Styles.content_container |], children = [|
                                            RX.View (styles = [| Styles.content contentWidth |], children = elements {

                                                Helpers.StaticContent topStaticContent
                                                RX.View (styles = (defaultArg paginatedStyle [||]), children = (
                                                    allItems.current
                                                    |> Seq.map renderItem
                                                    |> Array.ofSeq
                                                ))
                                                
                                                RX.View (styles = [| Styles.center |], children = [|
                                                    match dataFetching.current  with
                                                    | Loading        -> RX.ActivityIndicator(color = "#aaaaaa")
                                                    | Completed      -> nothing 
                                                    | FetchingFailed ->
                                                        element {
                                                            LC.Text ("Apologies, there is an internet issue. Please try again.", styles = [| Styles.error |])

                                                            RX.View (styles = [| Styles.retryButtonContainer |], children = [|
                                                                LC.Text ("Retry", styles  = [| Styles.retryButtonText |])
        
                                                                LC.TapCapture (onPress = (fun _ ->loadNextPage ()|> startSafely))
                                                            |])
                                                        }
                                                    | EndReached -> LC.Text ("-x-", styles = [| Styles.bottomReached |])
                                                    | NoItem     ->
                                                        LC.UiText (
                                                            styles   = [|Styles.noItems|],
                                                            children = [| LC.Text "No Items" |]
                                                        )
                                                |])
                                            })
                                            
                                        |])
                                    |])

                                    RX.View (styles = [| Styles.footer |], children = [|
                                        Helpers.StaticContent bottomStaticContent
                                    |])
                                |]
                            )
                    )
                )
            }