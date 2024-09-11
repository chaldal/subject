[<AutoOpen>]
module LibClient.Components.VirtualListView

open System

open Fable.React

open LibClient
open LibClient.JsInterop

open ReactXP.Styles
open ReactXP.Components

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

let mutable private scrollPositionStorage: Map<string, Measurements> = Map.empty
let mutable private lastScrollPosition: ScrollPosition = { Left = 0; Top = 0 }
let mutable private maybeLastKeys: Option<list<string>> = None
let mutable private restoreWhenItemKeysMatch: Option<Measurements> = None

let mutable maybeVirtualListViewRef: Option<ReactXP.Components.VirtualListView.IVirtualListViewRef> = None

type private Helper =
    static member MaybeVirtualListViewRef
        with get () = maybeVirtualListViewRef
        and  set (value: Option<ReactXP.Components.VirtualListView.IVirtualListViewRef>) : unit =
             maybeVirtualListViewRef <- value

    static member OnScroll (top: int) (left: int) : unit =
        lastScrollPosition <- { Top = top; Left = left }
        if restoreWhenItemKeysMatch.IsSome then
            restoreWhenItemKeysMatch <- None

    static member TryRestore (items: seq<VirtualListItem<'Item>>) : unit =
        let nextKeys =
            items |> Seq.map (fun item -> item.Key) |> Seq.toList
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
    static member private StoreMeasurements (key: string) (scrollPosition: ScrollPosition) (keys: list<string>) : unit =
        scrollPositionStorage <- scrollPositionStorage.AddOrUpdate (key, { ScrollPosition = scrollPosition; Keys = keys })


type LibClient.Components.Constructors.LC with
    [<Component>]
    static member VirtualListView<'Item> (
        items:         seq<VirtualListItem<'Item>>,
        render:        'Item -> ReactElement,
        restoreScroll: RestoreScroll
    ) : ReactElement = element {
        Hooks.useEffect(
            effect =
                fun () ->
                    Fable.Core.JS.console.log "mounted"
                    match restoreScroll with
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

                    Helper.TryRestore items
        )

        Hooks.useEffect(
            dependencies = [| restoreScroll |],
            effect =
                fun () ->
                    Fable.Core.JS.console.log "updated"
                    Fable.Core.JS.console.log "surely not here"
                    Helper.TryRestore items
        )

        nothing

        // match restoreScroll with
        // | No ->
        //     RX.VirtualListView (
        //         itemList = (
        //             items
        //             |> Seq.map VirtualListItem.toRX
        //             |> Array.ofSeq
        //         ),
        //         renderItem = Actions.Bound.Render,
        //         scrollEventThrottle = 10,
        //         ref = Actions.Bound.RefVirtualListView
        //     )
        // | _ ->
        //     RX.VirtualListView (
        //         itemList = (
        //             items
        //             |> Seq.map VirtualListItem.toRX
        //             |> Array.ofSeq
        //         ),
        //         renderItem = Actions.Bound.Render,
        //         onScroll = Actions.OnScroll,
        //         scrollEventThrottle = 10,
        //         ref = Actions.Bound.RefVirtualListView
        //     )
    }

and private Actions () =
    // Otherwise Fable creates a new function every time, and an assertion within VirtualListView fails
    // let bound = {|
    //     Render = (fun (virtualListCellRenderDetails: ReactXP.Components.VirtualListView.VirtualListCellRenderDetails) ->
    //         virtualListCellRenderDetails.GetItem<'Item>()
    //         // |> render
    //     )
    //     RefVirtualListView = (fun (nullableInstance: LibClient.JsInterop.JsNullable<ReactXP.Components.VirtualListView.IVirtualListViewRef>) ->
    //         Helper.MaybeVirtualListViewRef <- nullableInstance.ToOption
    //     )
    // |}

    static member Bound render = {|
        Render = (fun (virtualListCellRenderDetails: ReactXP.Components.VirtualListView.VirtualListCellRenderDetails) ->
            virtualListCellRenderDetails.GetItem<'Item>()
            |> render
        )
        RefVirtualListView = (fun (nullableInstance: LibClient.JsInterop.JsNullable<ReactXP.Components.VirtualListView.IVirtualListViewRef>) ->
            Helper.MaybeVirtualListViewRef <- nullableInstance.ToOption
        )
    |}

    static member OnScroll: int -> int -> unit = Helper.OnScroll
