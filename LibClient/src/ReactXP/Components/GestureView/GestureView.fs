[<AutoOpen>]
module ReactXP.Components.GestureView

open ReactXP.Helpers

open Fable.Core.JsInterop
open Browser.Types
open LibClient

// NOTE since these types I expect to only be used for
// consumption, I'm leaving them as auto-converted ts2fable interfaces

type [<AllowNullLiteral>] GestureState =
    abstract isTouch: bool with get, set
    abstract timeStamp: float with get, set

type [<AllowNullLiteral>] MultiTouchGestureState =
    inherit GestureState
    abstract initialCenterClientX: float with get, set
    abstract initialCenterClientY: float with get, set
    abstract initialCenterPageX: float with get, set
    abstract initialCenterPageY: float with get, set
    abstract initialWidth: float with get, set
    abstract initialHeight: float with get, set
    abstract initialDistance: float with get, set
    abstract initialAngle: float with get, set
    abstract centerClientX: float with get, set
    abstract centerClientY: float with get, set
    abstract centerPageX: float with get, set
    abstract centerPageY: float with get, set
    abstract velocityX: float with get, set
    abstract velocityY: float with get, set
    abstract width: float with get, set
    abstract height: float with get, set
    abstract distance: float with get, set
    abstract angle: float with get, set
    abstract isComplete: bool with get, set

type [<AllowNullLiteral>] ScrollWheelGestureState =
    inherit GestureState
    abstract clientX: float with get, set
    abstract clientY: float with get, set
    abstract pageX: float with get, set
    abstract pageY: float with get, set
    abstract scrollAmount: float with get, set

type [<AllowNullLiteral>] PanGestureState =
    inherit GestureState
    abstract initialClientX: float with get, set
    abstract initialClientY: float with get, set
    abstract initialPageX: float with get, set
    abstract initialPageY: float with get, set
    abstract clientX: float with get, set
    abstract clientY: float with get, set
    abstract pageX: float with get, set
    abstract pageY: float with get, set
    abstract velocityX: float with get, set
    abstract velocityY: float with get, set
    abstract isComplete: bool with get, set

type [<AllowNullLiteral>] TapGestureState =
    inherit GestureState
    abstract clientX: float with get, set
    abstract clientY: float with get, set
    abstract pageX: float with get, set
    abstract pageY: float with get, set

type GestureMouseCursor =
| Default    =  0
| Pointer    =  1
| Grab       =  2
| Move       =  3
| EWResize   =  4
| NSResize   =  5
| NESWResize =  6
| NWSEResize =  7
| NotAllowed =  8
| ZoomIn     =  9
| ZoomOut    = 10

type PreferredPanGesture =
| Horizontal = 0
| Vertical   = 1


type ReactXP.Components.Constructors.RX with
    static member GestureView(
        ?children:          ReactChildrenProp,
        ?onPinchZoom:       MultiTouchGestureState -> unit,
        ?onRotate:          MultiTouchGestureState -> unit,
        ?onScrollWheel:     ScrollWheelGestureState -> unit,
        ?mouseOverCursor:   GestureMouseCursor,
        ?onPan:             PanGestureState -> unit,
        ?onPanVertical:     PanGestureState -> unit,
        ?onPanHorizontal:   PanGestureState -> unit,
        ?onTap:             TapGestureState -> unit,
        ?onDoubleTap:       TapGestureState -> unit,
        ?onLongPress:       TapGestureState -> unit,
        ?onContextMenu:     TapGestureState -> unit,
        ?onFocus:           FocusEvent -> unit,
        ?onBlur:            FocusEvent -> unit,
        ?onKeyPress:        KeyboardEvent -> unit,
        ?preferredPan:      PreferredPanGesture,
        ?panPixelThreshold: float,
        ?releaseOnRequest:  bool,
        ?styles:            array<ReactXP.Styles.FSharpDialect.ViewStyles>,
        ?xLegacyStyles:     List<ReactXP.LegacyStyles.RuntimeStyles>
    ) =
        let __props = createEmpty

        __props?onPinchZoom       <- onPinchZoom
        __props?onRotate          <- onRotate
        __props?onScrollWheel     <- onScrollWheel
        __props?mouseOverCursor   <- mouseOverCursor
        __props?onPan             <- onPan
        __props?onPanVertical     <- onPanVertical
        __props?onPanHorizontal   <- onPanHorizontal
        __props?onTap             <- onTap
        __props?onDoubleTap       <- onDoubleTap
        __props?onLongPress       <- onLongPress
        __props?onContextMenu     <- onContextMenu
        __props?onFocus           <- onFocus
        __props?onBlur            <- onBlur
        __props?onKeyPress        <- onKeyPress
        __props?preferredPan      <- preferredPan
        __props?panPixelThreshold <- panPixelThreshold
        __props?releaseOnRequest  <- releaseOnRequest
        __props?style             <- styles

        match xLegacyStyles with
        | Option.None | Option.Some [] -> ()
        | Option.Some styles -> __props?__style <- styles

        Fable.React.ReactBindings.React.createElement(
            ReactXPRaw?GestureView,
            __props,
            ThirdParty.fixPotentiallySingleChild (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
        )
