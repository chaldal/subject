// https://recharts.org/en-US/api/Line

module ThirdParty.Recharts.Components.Line

open LibClient
open LibClient.JsInterop

open Fable.Core
open Fable.Core.JsInterop

open ThirdParty.Recharts.Components.Shared

let Line      = LegendType.Line
let PlainLine = LegendType.PlainLine
let Square    = LegendType.Square
let Rectangle = LegendType.Rectangle
let Circle    = LegendType.Circle
let Cross     = LegendType.Cross
let Diamond   = LegendType.Diamond
let Star      = LegendType.Star
let Triangle  = LegendType.Triangle
let Wye       = LegendType.Wye
let None      = LegendType.None

let InternalString = Color.InternalString
let InternalHex    = Color.InternalHex
let Rgb            = Color.Rgb
let Rgba           = Color.Rgba
let BlackAlpha     = Color.BlackAlpha
let WhiteAlpha     = Color.WhiteAlpha
let Black          = Color.Black
let White          = Color.White
let Transparent    = Color.Transparent

let Ease      = AnimationEasing.Ease
let EaseIn    = AnimationEasing.EaseIn
let EaseOut   = AnimationEasing.EaseOut
let EaseInOut = AnimationEasing.EaseInOut
let Linear    = AnimationEasing.Linear


[<RequireQualifiedAccess; StringEnum>]
type Type =
| Basis
| BasisClosed
| BasisOpen
| Linear
| LinearClosed
| Natural
| MonotoneX
| MonotoneY
| Monotone
| Step
| StepBefore
| StepAfter


type Props = (* GenerateMakeFunction *) {
    Type:              Type option            // defaultWithAutoWrap Some Type.Linear
    DataKey:           string option          // defaultWithAutoWrap JsUndefined
    LegendType:        LegendType option      // defaultWithAutoWrap JsUndefined
    Name:              string option          // defaultWithAutoWrap JsUndefined
    Stroke:            Color option           // defaultWithAutoWrap JsUndefined
    StrokeWidth:       int option             // defaultWithAutoWrap Some 1
    IsAnimationActive: bool option            // defaultWithAutoWrap JsUndefined
    AnimationEasing:   AnimationEasing option // defaultWithAutoWrap Some AnimationEasing.Ease
    OnAnimationStart:  (unit -> unit) option  // defaultWithAutoWrap JsUndefined
    OnAnimationEnd:    (unit -> unit) option  // defaultWithAutoWrap JsUndefined
    OnClick:           (unit -> unit) option  // defaultWithAutoWrap JsUndefined
    OnMouseDown:       (unit -> unit) option  // defaultWithAutoWrap JsUndefined
    OnMouseUp:         (unit -> unit) option  // defaultWithAutoWrap JsUndefined
    OnMouseMove:       (unit -> unit) option  // defaultWithAutoWrap JsUndefined
    OnMouseOver:       (unit -> unit) option  // defaultWithAutoWrap JsUndefined
    OnMouseOut:        (unit -> unit) option  // defaultWithAutoWrap JsUndefined
    OnMouseEnter:      (unit -> unit) option  // defaultWithAutoWrap JsUndefined
    OnMouseLeave:      (unit -> unit) option  // defaultWithAutoWrap JsUndefined
}

let private LineComponent: obj = JsInterop.import "Line" "recharts"
let Make =
    LibClient.ThirdParty.wrapComponentTransformingProps<Props>
        LineComponent
        (fun (props: Props) ->
            createObjWithOptionalValues [
                "type"              ==?> props.Type
                "dataKey"           ==?> props.DataKey
                "legendType"        ==?> props.LegendType
                "name"              ==?> props.Name
                "stroke"            ==?> (props.Stroke |> Option.map (fun v -> v.ToReactXPString))
                "strokeWidth"       ==?> props.StrokeWidth
                "isAnimationActive" ==?> props.IsAnimationActive
                "animationEasing"   ==?> props.AnimationEasing
                "onAnimationStart"  ==?> props.OnAnimationStart
                "onAnimationEnd"    ==?> props.OnAnimationEnd
                "onClick"           ==?> props.OnClick
                "onMouseDown"       ==?> props.OnMouseDown
                "onMouseUp"         ==?> props.OnMouseUp
                "onMouseMove"       ==?> props.OnMouseMove
                "onMouseOver"       ==?> props.OnMouseOver
                "onMouseOut"        ==?> props.OnMouseOut
                "onMouseEnter"      ==?> props.OnMouseEnter
                "onMouseLeave"      ==?> props.OnMouseLeave
            ]
        )
 