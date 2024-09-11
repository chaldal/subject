module LibClient.Components.ScrollView

open LibClient
open Fable.React
open ReactXP.Types
open ReactXP.Styles
open LibClient.JsInterop

type IScrollViewComponentRef =
    abstract member SetScrollLeft: (* scrollLeft *) int * (* animate *) bool -> unit;
    abstract member SetScrollTop:  (* scrollTop *)  int * (* animate *) bool -> unit;

type Scroll = NoScroll | Horizontal | Vertical | Both

type RestoreScroll =
| No
| WhenContentApproximatelyMatchesOriginalHeight of Key: string
with
    member this.MaybeKey : Option<string> =
        match this with
        | WhenContentApproximatelyMatchesOriginalHeight key -> Some key
        | No -> None


type Props = (* GenerateMakeFunction *) {
    Scroll:                                 Scroll
    RestoreScroll:                          RestoreScroll
    OnScroll:                               (int * int -> unit) option                                               // defaultWithAutoWrap None
    OnLayout:                               (ReactXP.Types.ViewOnLayoutEvent -> unit) option                         // defaultWithAutoWrap None
    ShowsHorizontalScrollIndicatorOnNative: bool                                                                     // default true
    ShowsVerticalScrollIndicatorOnNative:   bool                                                                     // default true
    styles:                                 array<ViewStyles> option                                                 // defaultWithAutoWrap None
    ref:                                    (LibClient.JsInterop.JsNullable<IScrollViewComponentRef> -> unit) option // defaultWithAutoWrap Undefined
    key:                                    string option                                                            // defaultWithAutoWrap JsUndefined
}

type private ScrollPosition = {
    Left: int
    Top:  int
}

type private Measurements = {
    ScrollPosition: ScrollPosition
    ContentSize:    Layout
}

type ScrollView(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, ScrollView>("LibClient.Components.ScrollView", _initialProps, Actions, hasStyles = true)

    static let mutable measurementStorage: Map<string, Measurements> = Map.empty

    let mutable lastScrollPosition:   ScrollPosition = { Left = 0; Top = 0 }
    let mutable maybeLastContentSize: Option<Layout> = None

    let mutable restoreWhenContentApproximatelyMatchesOriginalHeight: Option<Measurements> = None

    let mutable maybeScrollViewRef: Option<ReactXP.Components.ScrollView.IScrollViewRef> = None

    static member private StoreMeasurements (key: string) (scrollPosition: ScrollPosition) (contentSize: Layout) : unit =
        measurementStorage <- measurementStorage.AddOrUpdate (key, { ScrollPosition = scrollPosition; ContentSize = contentSize })

    member _.MaybeScrollViewRef
        with get () = maybeScrollViewRef
        and  set (value: Option<ReactXP.Components.ScrollView.IScrollViewRef>) : unit =
             maybeScrollViewRef <- value

    member _.OnScroll (top: int, left: int) : unit =
        lastScrollPosition <- { Top = top; Left = left }
        if restoreWhenContentApproximatelyMatchesOriginalHeight.IsSome then
            restoreWhenContentApproximatelyMatchesOriginalHeight <- None

    member this.RestoreScroll (layout: ReactXP.Types.ViewOnLayoutEvent) : unit =
        maybeLastContentSize <- Some { Width = layout.width; Height = layout.height }
        restoreWhenContentApproximatelyMatchesOriginalHeight
        |> Option.sideEffect (fun originalMeasurements ->
            if abs (layout.height - originalMeasurements.ContentSize.Height) < (int ((float layout.height) * 0.1)) then
                restoreWhenContentApproximatelyMatchesOriginalHeight <- None
                maybeScrollViewRef |> Option.sideEffect (fun scrollViewRef ->
                    scrollViewRef.setScrollLeft (originalMeasurements.ScrollPosition.Left, (* animate *) false)
                    scrollViewRef.setScrollTop  (originalMeasurements.ScrollPosition.Top,  (* animate *) false)
                )
        )

    override this.ComponentDidMount () : unit =
        match this.props.RestoreScroll with
        | WhenContentApproximatelyMatchesOriginalHeight key ->
            measurementStorage.TryFind key
            |> Option.sideEffect (fun measurements ->
                restoreWhenContentApproximatelyMatchesOriginalHeight <- Some measurements

                async {
                    // if we couldn't restore focus for 10 seconds, give up
                    do! Async.Sleep (TimeSpan.FromSeconds 10.)
                    if restoreWhenContentApproximatelyMatchesOriginalHeight.IsSome then
                        restoreWhenContentApproximatelyMatchesOriginalHeight <- None
                } |> startSafely
            )

        | No -> Noop

    override this.ComponentWillUnmount () : unit =
        match (this.props.RestoreScroll.MaybeKey, maybeLastContentSize) with
        | (Some key, Some lastContentSize) -> ScrollView.StoreMeasurements key lastScrollPosition lastContentSize
        | _ -> Noop

    interface IScrollViewComponentRef with
        member _.SetScrollLeft (scrollLeft: int, animate: bool) : unit =
            maybeScrollViewRef |> Option.sideEffect (fun scrollView -> scrollView.setScrollLeft(scrollLeft, animate))

        member _.SetScrollTop (scrollTop: int, animate: bool) : unit =
            maybeScrollViewRef |> Option.sideEffect (fun scrollView -> scrollView.setScrollTop(scrollTop, animate))

and Actions(this: ScrollView) =
    member _.OnScroll (top: int, left: int) : unit =
        this.props.OnScroll |> Option.sideEffect (fun onScroll -> onScroll (top, left))
        this.OnScroll (top, left)

    member _.OnContentLayout (layout: ReactXP.Types.ViewOnLayoutEvent): unit =
        match this.props.RestoreScroll with
        | No -> ()
        | _ -> this.RestoreScroll layout

        this.props.OnLayout
        |> Option.sideEffect (fun onLayout -> onLayout layout)


    member _.RefScrollView (nullableInstance: LibClient.JsInterop.JsNullable<ReactXP.Components.ScrollView.IScrollViewRef>) =
        this.MaybeScrollViewRef <- nullableInstance.ToOption

let Make = makeConstructor<ScrollView, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
