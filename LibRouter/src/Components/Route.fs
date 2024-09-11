[<AutoOpen>]
module LibRouter.Components.Route

open Fable.React

open ReactXP.Styles
open ReactXP.Components

open LibClient
open LibClient.Components

open LibRouter.Components

type Scroll = ScrollView.Scroll

let NoScroll                                      = Scroll.NoScroll
let Horizontal                                    = Scroll.Horizontal
let Vertical                                      = Scroll.Vertical
let Both                                          = Scroll.Both

type RestoreScroll                                = ScrollView.RestoreScroll
let No                                            = RestoreScroll.No
let WhenContentApproximatelyMatchesOriginalHeight = RestoreScroll.WhenContentApproximatelyMatchesOriginalHeight

type ContentWidth =
| Full
| Fixed of int

type OnNetworkFailure =
| DefaultVisuals
| Reraise
| Custom of (unit -> ReactElement)

module private Styles =
    let view = ViewStyles.Memoize (fun (elevateFooter: bool) -> makeViewStyles {
        if not elevateFooter then
            FlexDirection.ColumnReverseZindexHack
        flex            1
        backgroundColor Color.White
    })

    let scroll_view_no_footer = makeViewStyles {
        FlexDirection.Column
        flex 1
    }

    let scroll_view_with_footer = makeViewStyles {
        flex 1
    }

    let no_scroll_view = makeViewStyles {
        FlexDirection.Column
        flex 1
    }

    let scroll_view_children_and_footer = makeViewStyles {
        flex 1
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
        | Fixed contentWidth -> maxWidth contentWidth
        | _ -> ()
    })

    let scroll_view_footer = makeViewStyles {
        flex 0
    }

type LR with
    [<Component>]
    static member Route (
            children:          ReactElements,
            ?scroll:           Scroll,
            ?contentWidth:     ContentWidth,
            ?styles:           array<ViewStyles>,
            ?onNetworkFailure: OnNetworkFailure,
            ?footer:           ReactElement,
            ?legacyTopNav:     ReactElement,
            ?bottomSection:    ReactElement,
            ?staticContent:    ReactElement,
            ?goToTopButton:    (ReactEvent.Action -> unit) -> ReactElement,
            ?restoreScroll:    RestoreScroll,
            ?onScroll:         int * int -> unit,
            ?elevateFooter:    bool
        ) : ReactElement =
            let scroll:           Scroll           = defaultArg scroll Vertical
            let contentWidth:     ContentWidth     = defaultArg contentWidth Full
            let onNetworkFailure: OnNetworkFailure = defaultArg onNetworkFailure OnNetworkFailure.DefaultVisuals
            let elevateFooter:    bool             = defaultArg elevateFooter false
            
            let createElement (items: List<ReactElement option>) : ReactElement =
                element {
                    items
                    |> List.map (
                        fun item ->
                            item
                            |> Option.map id
                            |> Option.getOrElse nothing
                    )
                    |> asFragment
                }
            
            let topElement, bottomElement =
                match elevateFooter with
                | true  ->
                    (createElement [staticContent; legacyTopNav]),
                    (createElement [bottomSection])
                | false ->
                    (*  Reversed to make drop shadow of TopNav work, which unfortunately breaks the bottom section's shadow  *)
                    (createElement [bottomSection]),
                    (createElement [legacyTopNav; staticContent])

            element {
                LR.With.Location
                    (fun location ->
                        let goToTopButtonBlock (scrollView: ScrollView.IScrollViewComponentRef) = (
                            goToTopButton
                            |> Option.map (fun goToTopButton -> goToTopButton (fun _ -> scrollView.SetScrollTop (0, false)))
                            |> Option.getOrElse nothing
                        )

                        LC.ErrorBoundary (
                            catch =
                                (fun (exn, _retry) ->
                                    match exn with
                                    | AsyncDataException AsyncDataFailure.NetworkFailure ->
                                        match onNetworkFailure with
                                        | DefaultVisuals -> LC.AppShell.NetworkFailureMessage ()
                                        | Custom makeVisuals ->  makeVisuals()
                                        | Reraise -> LC.Text $"raise {exn}"
                                    | _ ->
                                        LC.Text $"raise {exn}"
                                ),
                            ``try`` = element {
                                let url = location.Url
                                let restoreScroll = restoreScroll |> Option.getOrElse (WhenContentApproximatelyMatchesOriginalHeight url)
                                RX.View (styles   = (styles |> Option.getOrElse [| Styles.view elevateFooter |]), children = [|
                                    topElement

                                    match (scroll, footer) with
                                    | Scroll.NoScroll, maybeFooter ->
                                        RX.View (styles = [| Styles.no_scroll_view |], children = [|
                                            RX.View (styles = [| Styles.content_container |], children = [|
                                                RX.View (styles = [| Styles.content contentWidth |] , children = children)
                                            |])
                                            maybeFooter |> Option.getOrElse nothing
                                        |])

                                    | _, None ->
                                        LC.With.Ref
                                            (fun (bindScrollView, maybeScrollView) -> element {
                                                LC.ScrollView (
                                                    restoreScroll = restoreScroll,
                                                    scroll        = scroll,
                                                    ref           = bindScrollView,
                                                    children      = [|
                                                        RX.View (styles = [| Styles.scroll_view_no_footer |], children = [|
                                                            RX.View (styles = [| Styles.content_container |], children = [|
                                                                RX.View (styles = [| Styles.content contentWidth |], children = children)
                                                            |])
                                                        |])
                                                    |]
                                                )
                                                if (scroll = Vertical || scroll = Both) then
                                                    maybeScrollView
                                                    |> Option.map goToTopButtonBlock
                                                    |> Option.getOrElse nothing
                                                else nothing
                                            })

                                    | _, Some footer ->
                                        LC.With.Layout
                                            (fun (onLayoutOption, maybeLayout) ->
                                                LC.With.Ref
                                                    (fun (bindScrollView, maybeScrollView) -> element {
                                                        LC.ScrollView(
                                                            restoreScroll = restoreScroll,
                                                            ?onScroll     = onScroll,
                                                            ?onLayout     = onLayoutOption,
                                                            scroll        = scroll,
                                                            ref           = bindScrollView,
                                                            children      = [|
                                                                RX.View (styles = [| Styles.scroll_view_with_footer; makeViewStyles { maybeLayout |> Option.map (fun l -> minHeight l.Height) |> Option.getOrElse (height 0) } |] ,children = [|
                                                                    RX.View ( styles = [| Styles.scroll_view_children_and_footer; Styles.content_container |], children = [|
                                                                        RX.View (styles = [| Styles.content contentWidth |] ,children = children)
                                                                    |])

                                                                    RX.View (styles = [| Styles.scroll_view_footer |], children = [| footer |])
                                                                |])
                                                        |])

                                                        if (scroll = Vertical || scroll = Both) then
                                                            maybeScrollView
                                                            |> Option.map goToTopButtonBlock
                                                            |> Option.getOrElse nothing
                                                        else nothing
                                                    })
                                            )
                                    
                                    bottomElement
                                |])
                            }
                        )
                    )
            }
