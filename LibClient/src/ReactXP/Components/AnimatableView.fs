[<AutoOpen>]
module ReactXP.Components.AnimatableView

open LibClient

open ReactXP.Helpers
open ReactXP.Styles
open ReactXP.Types

open Fable.Core.JsInterop
open Browser.Types

let private MakeReactXPAnimatedView: obj -> ReactElements -> ReactElement =
    LibClient.ThirdParty.wrapComponent<obj>(ReactXPRaw?Animated?View)

type ReactXP.Components.Constructors.RX with
    static member AnimatableView(
        ?children:                         ReactElements,
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
        ?onPress:                          Event -> unit,
        ?onLongPress:                      Event -> unit,
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
        ?key:                              string,
        ?styles:                           array<AnimatableViewStyles>
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
        __props?disableTouchOpacityAnimation     <- disableTouchOpacityAnimation
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
        __props?key                              <- key
        __props?style                            <- styles

        let children =
            children
            |> Option.map tellReactArrayKeysAreOkay
            |> Option.getOrElse [||]
            |> ThirdParty.fixPotentiallySingleChild

        MakeReactXPAnimatedView
            __props
            children
