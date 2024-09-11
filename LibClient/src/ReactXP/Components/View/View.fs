[<AutoOpen>]
module ReactXP.Components.View

open LibClient.JsInterop

open ReactXP.Helpers
open ReactXP.Types

open Fable.Core.JsInterop
open Browser.Types
open Fable.Core
open LibClient

[<StringEnum>]
type ViewLayerType =
| [<CompiledName("none")>]Non
| Software
| Hardware

type LimitFocusType =
| Unlimited  = 0
| Limited    = 1
| Accessible = 2

type FocusCandidate = {
    ``component``:   obj (* RX.FocusableComponent *)
    accessibilityId: string option
}

type FocusArbitrator = ((* candidates *) List<FocusCandidate>) -> Option<FocusCandidate> (* Undefined for None *)

// XXX the TypeScript definitions use an enum without an explicit start
// value, which is flimsy... hopefully the implementation doesn't change.
type AccessibilityLiveRegion =
| None      = 0
| Polite    = 1
| Assertive = 2

type IViewRef = Fable.React.ReactElement

type ReactXP.Components.Constructors.RX with
    static member View(
        ?children:                         ReactChildrenProp,
        ?title:                            string,
        ?ignorePointerEvents:              bool,
        ?blockPointerEvents:               bool,
        ?shouldRasterizeIOS:               bool,
        ?viewLayerTypeAndroid:             ViewLayerType,
        ?restrictFocusWithin:              bool,
        ?limitFocusWithin:                 LimitFocusType,
        ?autoFocus:                        bool,
        ?arbitrateFocus:                   FocusArbitrator,
        ?importantForLayout:               bool,
        ?id:                               string,
        ?ariaLabelledBy:                   string,
        ?ariaRoleDescription:              string,
        ?accessibilityLiveRegion:          AccessibilityLiveRegion,
        ?animateChildEnter:                bool,
        ?animateChildLeave:                bool,
        ?animateChildMove:                 bool,
        ?onAccessibilityTapIOS:            Event -> unit,
        ?onLayout:                         ViewOnLayoutEvent -> unit,
        ?onMouseEnter:                     MouseEvent -> unit,
        ?onMouseLeave:                     MouseEvent -> unit,
        ?onDragStart:                      DragEvent -> unit,
        ?onDrag:                           DragEvent -> unit,
        ?onDragEnd:                        DragEvent -> unit,
        ?onDragEnter:                      DragEvent -> unit,
        ?onDragOver:                       DragEvent -> unit,
        ?onDragLeave:                      DragEvent -> unit,
        ?onDrop:                           DragEvent -> unit,
        ?onMouseOver:                      MouseEvent -> unit,
        ?onMouseMove:                      MouseEvent -> unit,
        ?onPress:                          PointerEvent -> unit,
        ?onLongPress:                      PointerEvent -> unit,
        ?onKeyPress:                       KeyboardEvent -> unit,
        ?onFocus:                          FocusEvent -> unit,
        ?onBlur:                           FocusEvent -> unit,
        ?disableTouchOpacityAnimation:     bool,
        ?activeOpacity:                    float,
        ?underlayColor:                    string,
        ?onContextMenu:                    MouseEvent -> unit,
        ?onStartShouldSetResponder:        Event -> bool,
        ?onMoveShouldSetResponder:         Event -> bool,
        ?onStartShouldSetResponderCapture: Event -> bool,
        ?onMoveShouldSetResponderCapture:  Event -> bool,
        ?onResponderGrant:                 Event -> unit,
        ?onResponderReject:                Event -> unit,
        ?onResponderRelease:               Event -> unit,
        ?onResponderStart:                 TouchEvent -> unit,
        ?onResponderMove:                  TouchEvent -> unit,
        ?onResponderEnd:                   TouchEvent -> unit,
        ?onResponderTerminate:             Event -> unit,
        ?onResponderTerminationRequest:    Event -> bool,
        ?tabIndex:                         int,
        ?useSafeInsets:                    bool,
        ?ref:                              LibClient.JsInterop.JsNullable<IViewRef> -> unit,
        ?key:                              string,
        ?styles:                           array<ReactXP.Styles.FSharpDialect.ViewStyles>
    ) =
        let __props = createEmpty
        __props?title                            <- title
        __props?ignorePointerEvents              <- ignorePointerEvents
        __props?blockPointerEvents               <- blockPointerEvents
        __props?shouldRasterizeIOS               <- shouldRasterizeIOS
        __props?viewLayerTypeAndroid             <- viewLayerTypeAndroid
        __props?restrictFocusWithin              <- restrictFocusWithin
        __props?limitFocusWithin                 <- limitFocusWithin
        __props?autoFocus                        <- autoFocus
        __props?arbitrateFocus                   <- arbitrateFocus
        __props?importantForLayout               <- importantForLayout
        __props?id                               <- id
        __props?ariaLabelledBy                   <- ariaLabelledBy
        __props?ariaRoleDescription              <- ariaRoleDescription
        __props?accessibilityLiveRegion          <- accessibilityLiveRegion
        __props?animateChildEnter                <- animateChildEnter
        __props?animateChildLeave                <- animateChildLeave
        __props?animateChildMove                 <- animateChildMove
        __props?onAccessibilityTapIOS            <- onAccessibilityTapIOS
        __props?onLayout                         <- onLayout
        __props?onMouseEnter                     <- onMouseEnter
        __props?onMouseLeave                     <- onMouseLeave
        __props?onDragStart                      <- onDragStart
        __props?onDrag                           <- onDrag
        __props?onDragEnd                        <- onDragEnd
        __props?onDragEnter                      <- onDragEnter
        __props?onDragOver                       <- onDragOver
        __props?onDragLeave                      <- onDragLeave
        __props?onDrop                           <- onDrop
        __props?onMouseOver                      <- onMouseOver
        __props?onMouseMove                      <- onMouseMove
        __props?onPress                          <- onPress
        __props?onLongPress                      <- onLongPress
        __props?onKeyPress                       <- onKeyPress
        __props?onFocus                          <- onFocus
        __props?onBlur                           <- onBlur
        __props?disableTouchOpacityAnimation     <- disableTouchOpacityAnimation |> Option.orElse (Some true)
        __props?activeOpacity                    <- activeOpacity
        __props?underlayColor                    <- underlayColor
        __props?onContextMenu                    <- onContextMenu
        __props?onStartShouldSetResponder        <- onStartShouldSetResponder
        __props?onMoveShouldSetResponder         <- onMoveShouldSetResponder
        __props?onStartShouldSetResponderCapture <- onStartShouldSetResponderCapture
        __props?onMoveShouldSetResponderCapture  <- onMoveShouldSetResponderCapture
        __props?onResponderGrant                 <- onResponderGrant
        __props?onResponderReject                <- onResponderReject
        __props?onResponderRelease               <- onResponderRelease
        __props?onResponderStart                 <- onResponderStart
        __props?onResponderMove                  <- onResponderMove
        __props?onResponderEnd                   <- onResponderEnd
        __props?onResponderTerminate             <- onResponderTerminate
        __props?onResponderTerminationRequest    <- onResponderTerminationRequest
        __props?tabIndex                         <- tabIndex
        __props?useSafeInsets                    <- useSafeInsets
        __props?ref                              <- ref
        __props?key                              <- key
        __props?style                            <- styles

        Fable.React.ReactBindings.React.createElement(
            ReactXPRaw?View,
            __props,
            ThirdParty.fixPotentiallySingleChild (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
        )
