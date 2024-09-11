[<AutoOpen>]
module ReactXP.Components.VirtualListView

open ReactXP.Types

open Fable.Core.JsInterop
open Fable.React
open LibClient.JsInterop

type OverScrollMode =
| Auto
| Always
| Never

type TabNavigation =
| Local
| Cycle
| Once

type KeyboardDismissMode =
| [<CompiledName("none")>]KeyboardDismissModeNone
| Interactive
| [<CompiledName("on-drag")>]OnDrag

type ScrollIndicatorInsets = {
    top:    int
    left:   int
    bottom: int
    right:  int
}

type VirtualListViewItemInfo = {
    // A string that uniquely identifies this item.
    key: string

    // Specifies the known height of the item or a best guess if the
    // height isn't known.
    height: int

    // Specifies that the height is not known and needs to be measured
    // dynamically. This has a big perf overhead because it requires a
    // double layout (once offscreen to measure the item). It also
    // disables cell recycling. Wherever possible, it should be avoided,
    // especially for perf-critical views.
    measureHeight: Option<bool>

    // Specify the same "template" string for items that are rendered
    // with identical or similar view hierarchies. When a template is
    // specified, the list view attempts to recycle cells whose templates
    // match. When an item scrolls off the screen and others appear on
    // screen, the contents of the cell are simply updated rather than
    // torn down and rebuilt.
    template: string

    // Is the item navigable by keyboard or through accessibility
    // mechanisms?
    isNavigable: Option<bool>

    payload: obj

    disableTouchOpacityAnimation: bool
}


type VirtualListCellRenderDetails = {
    item: VirtualListViewItemInfo
} with
    member this.GetItem<'T>() : 'T =
        this.item.payload :?> 'T

// NOTE animation duration is rather stupidly hardcoded to 200 ms in the ReactXP source
type IVirtualListViewRef =
    abstract member scrollToTop:  (* animate *) bool * (* top *) int -> unit;

let VirtualListViewRaw: obj = Fable.Core.JsInterop.import "VirtualListView" "@chaldal/reactxp-virtuallistview"

type ReactXP.Components.Constructors.RX with
    static member VirtualListView(
        itemList:                   array<VirtualListViewItemInfo>,
        renderItem:                 VirtualListCellRenderDetails -> ReactElement,
        ?animateChanges:            bool,
        ?initialSelectedKey:        string,
        ?keyboardFocusScrollOffset: int,
        ?logInfo:                   string -> unit,
        ?onItemSelected:            VirtualListViewItemInfo -> unit,
        ?padding:                   int,
        ?showOverflow:              bool,
        ?skipRenderIfItemUnchanged: bool,
        ?testId:                    string,
        ?keyboardDismissMode:       KeyboardDismissMode,
        ?keyboardShouldPersistTaps: bool,
        ?disableScrolling:          bool,
        ?scrollsToTop:              bool,
        ?disableBouncing:           bool,
        ?scrollIndicatorInsets:     ScrollIndicatorInsets,
        ?onLayout:                  ViewOnLayoutEvent -> unit,
        ?scrollEventThrottle:       int,
        ?onScroll:                  (int * int) -> unit,
        ?ref:                       LibClient.JsInterop.JsNullable<IVirtualListViewRef> -> unit,
        ?styles:                    array<ReactXP.Styles.FSharpDialect.ViewStyles>,
        ?xLegacyStyles:             List<ReactXP.LegacyStyles.RuntimeStyles>
        // need to add these when we deal with animations
        // scrollXAnimatedValue:            RX.Types.AnimatedValue      option // defaultWithAutoWrap Undefined
        // scrollYAnimatedValue:            RX.Types.AnimatedValue      option // defaultWithAutoWrap Undefined
    ) =
        let __props = createEmpty

        __props?itemList                  <- itemList
        __props?renderItem                <- renderItem
        __props?animateChanges            <- animateChanges
        __props?initialSelectedKey        <- initialSelectedKey
        __props?keyboardFocusScrollOffset <- keyboardFocusScrollOffset
        __props?logInfo                   <- logInfo
        __props?onItemSelected            <- onItemSelected
        __props?padding                   <- padding
        __props?showOverflow              <- showOverflow
        __props?skipRenderIfItemUnchanged <- skipRenderIfItemUnchanged
        __props?testId                    <- testId
        __props?keyboardDismissMode       <- keyboardDismissMode
        __props?keyboardShouldPersistTaps <- keyboardShouldPersistTaps
        __props?disableScrolling          <- disableScrolling
        __props?scrollsToTop              <- scrollsToTop
        __props?disableBouncing           <- disableBouncing
        __props?scrollIndicatorInsets     <- scrollIndicatorInsets
        __props?onLayout                  <- onLayout
        __props?scrollEventThrottle       <- scrollEventThrottle
        __props?onScroll                  <- match onScroll with | None -> !!None | Some f -> !!(fixTupledFunctionOnRawJsBoundary f)
        __props?ref                       <- ref
        __props?style                     <- styles

        match xLegacyStyles with
        | Option.None | Option.Some [] -> ()
        | Option.Some styles -> __props?__style <- styles

        Fable.React.ReactBindings.React.createElement(
            VirtualListViewRaw,
            __props,
            [||]
        )
