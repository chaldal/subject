[<AutoOpen>]
module ReactXP.Components.ScrollView


open ReactXP.Helpers
open ReactXP.Types

open Fable.Core.JsInterop
open Browser.Types
open LibClient
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
| [<CompiledName("none")>]KeyboardDismissMode
| Interactive
| [<CompiledName("on-drag")>]OnDrag

type ScrollIndicatorInsets = {
    top:    int
    left:   int
    bottom: int
    right:  int
}

// NOTE animation duration is rather stupidly hardcoded to 200 ms in the ReactXP source
type IScrollViewRef =
    abstract member setScrollLeft: (* scrollLeft *) int * (* animate *) bool -> unit;
    abstract member setScrollTop:  (* scrollTop *)  int * (* animate *) bool -> unit;

type ReactXP.Components.Constructors.RX with
    static member ScrollView(
        ?children:                       ReactChildrenProp,
        ?vertical:                       bool,
        ?horizontal:                     bool,
        ?onLayout:                       ViewOnLayoutEvent -> unit,
        ?onContentSizeChange:            int -> int -> unit,
        ?onScroll:                       int * int -> unit,
        ?onScrollBeginDrag:              unit -> unit,
        ?onScrollEndDrag:                unit -> unit,
        ?onKeyPress:                     KeyboardEvent -> unit,
        ?onFocus:                        FocusEvent -> unit,
        ?onBlur:                         FocusEvent -> unit,
        ?showsHorizontalScrollIndicator: bool,
        // showsVerticalScrollIndicator prop doesn't work for Web Apps. They have an open issue on this
        // https://github.com/microsoft/reactxp/issues/990
        ?showsVerticalScrollIndicator:   bool,
        ?scrollEnabled:                  bool,
        ?keyboardDismissMode:            KeyboardDismissMode,
        // keyboardShouldPersistTaps prop default value is set to true so that taps work even if the keyboard is visible
        ?keyboardShouldPersistTaps:      bool,
        ?scrollEventThrottle:            float,
        ?bounces:                        bool,
        ?pagingEnabled:                  bool,
        ?snapToInterval:                 float,
        ?scrollsToTop:                   bool,
        ?overScrollMode:                 OverScrollMode,
        ?scrollIndicatorInsets:          ScrollIndicatorInsets,
        ?tabNavigation:                  TabNavigation,
        ?ref:                            LibClient.JsInterop.JsNullable<IScrollViewRef> -> unit,
        ?styles:                         array<ReactXP.Styles.FSharpDialect.ScrollViewStyles>,
        ?xLegacyStyles:                  List<ReactXP.LegacyStyles.RuntimeStyles>
        // need to add these when we deal with animations
        // scrollXAnimatedValue:            RX.Types.AnimatedValue      option // defaultWithAutoWrap Undefined
        // scrollYAnimatedValue:            RX.Types.AnimatedValue      option // defaultWithAutoWrap Undefined
    ) =
        let __props = createEmpty

        __props?vertical                       <- vertical
        __props?horizontal                     <- horizontal
        __props?onLayout                       <- onLayout
        __props?onContentSizeChange            <- onContentSizeChange
        __props?onScroll                       <- match onScroll with | None -> !!None | Some f -> !!(fixTupledFunctionOnRawJsBoundary f)
        __props?onScrollBeginDrag              <- onScrollBeginDrag
        __props?onScrollEndDrag                <- onScrollEndDrag
        __props?onKeyPress                     <- onKeyPress
        __props?onFocus                        <- onFocus
        __props?onBlur                         <- onBlur
        __props?showsHorizontalScrollIndicator <- showsHorizontalScrollIndicator
        __props?showsVerticalScrollIndicator   <- showsVerticalScrollIndicator
        __props?scrollEnabled                  <- scrollEnabled
        __props?keyboardDismissMode            <- keyboardDismissMode
        __props?keyboardShouldPersistTaps      <- keyboardShouldPersistTaps |> Option.orElse (Some true)
        __props?scrollEventThrottle            <- scrollEventThrottle
        __props?bounces                        <- bounces
        __props?pagingEnabled                  <- pagingEnabled
        __props?snapToInterval                 <- snapToInterval
        __props?scrollsToTop                   <- scrollsToTop
        __props?overScrollMode                 <- overScrollMode
        __props?scrollIndicatorInsets          <- scrollIndicatorInsets
        __props?tabNavigation                  <- tabNavigation
        __props?ref                            <- ref
        __props?style                          <- styles

        match xLegacyStyles with
        | Option.None | Option.Some [] -> ()
        | Option.Some styles -> __props?__style <- styles

        Fable.React.ReactBindings.React.createElement(
            ReactXPRaw?ScrollView,
            __props,
            ThirdParty.fixPotentiallySingleChild (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
        )
