module LibClient.Components.VirtualListView

open LibClient
open LibClient.JsInterop
open System
open ReactXP.Styles

type RestoreScroll =
| No
| WhenItemKeysMatch of Key: string
    member this.MaybeKey : Option<string> =
        match this with
        | WhenItemKeysMatch key -> Some key
        | No -> None

type private ScrollPosition = {
    Left: int
    Top:  int
}

type private Measurements = {
    ScrollPosition: ScrollPosition
    Keys:           list<string>
}

type VirtualListItem<'Item> = {
    Key:      string
    Item:     'Item
    Height:   Option<int>
    Template: string
} with
    member this.ToRX : ReactXP.Components.VirtualListView.VirtualListViewItemInfo = {
        key                          = this.Key
        height                       = this.Height |> Option.getOrElse 1
        payload                      = this.Item :> obj
        measureHeight                = Some this.Height.IsNone
        template                     = this.Template
        isNavigable                  = Some false
        disableTouchOpacityAnimation = true
    }

module VirtualListItem =
    let toRX (item: VirtualListItem<'Item>) = item.ToRX

type Props<'Item> = (* GenerateMakeFunction *) {
    Items:            seq<VirtualListItem<'Item>>
    Render:           'Item -> ReactElement
    RestoreScroll:    RestoreScroll
    ScrollSideEffect: (int * int -> unit) option // defaultWithAutoWrap None
    styles:           array<ViewStyles> option // defaultWithAutoWrap None
    key:              string option // defaultWithAutoWrap JsUndefined
} with
    member this.Keys : list<string> =
        this.Items |> Seq.map (fun item -> item.Key) |> Seq.toList

type VirtualListView<'Item>(_initialProps) =
    inherit PureStatelessComponent<Props<'Item>, Actions<'Item>, VirtualListView<'Item>>("LibClient.Components.VirtualListView", _initialProps, Actions<'Item>, hasStyles = true)

    static let mutable scrollPositionStorage: Map<string, Measurements> = Map.empty

    let mutable lastScrollPosition: ScrollPosition = { Left = 0; Top = 0 }
    let mutable maybeLastKeys:      Option<list<string>> = None

    let mutable restoreWhenItemKeysMatch: Option<Measurements> = None

    let mutable maybeVirtualListViewRef: Option<ReactXP.Components.VirtualListView.IVirtualListViewRef> = None

    static member private StoreMeasurements (key: string) (scrollPosition: ScrollPosition) (keys: list<string>) : unit =
        scrollPositionStorage <- scrollPositionStorage.AddOrUpdate (key, { ScrollPosition = scrollPosition; Keys = keys })

    member _.MaybeVirtualListViewRef
        with get () = maybeVirtualListViewRef
        and  set (value: Option<ReactXP.Components.VirtualListView.IVirtualListViewRef>) : unit =
             maybeVirtualListViewRef <- value

    member _.OnScroll (top: int, left: int) : unit =
        lastScrollPosition <- { Top = top; Left = left }
        if restoreWhenItemKeysMatch.IsSome then
            restoreWhenItemKeysMatch <- None

    override this.ComponentDidMount () : unit =
        match this.props.RestoreScroll with
        | WhenItemKeysMatch key ->
            scrollPositionStorage.TryFind key
            |> Option.sideEffect (fun scrollPosition ->
                restoreWhenItemKeysMatch <- Some scrollPosition
                async {
                    // if we couldn't restore focus for 10 seconds, give up
                    do! Async.Sleep (TimeSpan.FromSeconds 10.)
                    if restoreWhenItemKeysMatch.IsSome then
                        restoreWhenItemKeysMatch <- None
                } |> startSafely
            )
        | No -> Noop
        this.TryRestore this.props

    override this.ComponentWillUnmount () : unit =
        match (this.props.RestoreScroll.MaybeKey, maybeLastKeys) with
        | (Some key, Some keys) -> VirtualListView<'Item>.StoreMeasurements key lastScrollPosition keys
        | _ -> Noop

    member _.TryRestore (props: Props<'Item>) : unit =
        let nextKeys = props.Keys
        maybeLastKeys <- Some nextKeys
        restoreWhenItemKeysMatch
        |> Option.sideEffect (fun originalMeasurements ->
            if nextKeys = originalMeasurements.Keys then
                restoreWhenItemKeysMatch <- None
                maybeVirtualListViewRef |> Option.sideEffect (fun virtualListViewRef ->
                    // restore after render
                    runOnNextTick (fun () ->
                        virtualListViewRef.scrollToTop ((* animate *) false, originalMeasurements.ScrollPosition.Top)
                    )
                )
        )
    override this.ComponentWillReceiveProps (nextProps: Props<'Item>) : unit =
        this.TryRestore nextProps

and Actions<'Item>(this: VirtualListView<'Item>) =
    // Otherwise Fable creates a new function every time, and an assertion within VirtualListView fails
    let bound = {|
        Render = (fun (virtualListCellRenderDetails: ReactXP.Components.VirtualListView.VirtualListCellRenderDetails) ->
            virtualListCellRenderDetails.GetItem<'Item>()
            |> this.props.Render
        )
        RefVirtualListView = (fun (nullableInstance: LibClient.JsInterop.JsNullable<ReactXP.Components.VirtualListView.IVirtualListViewRef>) ->
            this.MaybeVirtualListViewRef <- nullableInstance.ToOption
        )
    |}

    member _.Bound = bound

    member _.OnScroll (top: int , left: int) : unit =
        this.props.ScrollSideEffect
        |> Option.sideEffect (fun callback -> callback (top, left))
        this.OnScroll (top, left)
    
let Make = makeConstructor<VirtualListView<'Item>, _, _>

// Unfortunately necessary boilerplate
type Estate<'Item> = NoEstate1<'Item>
type Pstate = NoPstate
