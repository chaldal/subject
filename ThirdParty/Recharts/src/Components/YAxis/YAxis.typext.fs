// https://recharts.org/en-US/api/YAxis

module ThirdParty.Recharts.Components.YAxis

open LibClient
open LibClient.JsInterop

open Fable.Core
open Fable.Core.JsInterop
open ThirdParty.Recharts.Components.Shared

let Left  = YAxisOrientation.Left
let Right = YAxisOrientation.Right

let Number   = AxisType.Number
let Category = AxisType.Category

let PreserveStart    = AxisInterval.PreserveStart
let PreserveEnd      = AxisInterval.PreserveEnd
let PreserveStartEnd = AxisInterval.PreserveStartEnd
let Every            = AxisInterval.Every

let Auto       = AxisScale.Auto
let Linear     = AxisScale.Linear
let Pow        = AxisScale.Pow
let Sqrt       = AxisScale.Sqrt
let Log        = AxisScale.Log
let Identity   = AxisScale.Identity
let Time       = AxisScale.Time
let Band       = AxisScale.Band
let Point      = AxisScale.Point
let Ordinal    = AxisScale.Ordinal
let Quantile   = AxisScale.Quantile
let Quantize   = AxisScale.Quantize
let Utc        = AxisScale.Utc
let Sequential = AxisScale.Sequential
let Threshold  = AxisScale.Threshold

type AxisId   = Shared.AxisId
type AxisName = Shared.AxisName
type AxisUnit = Shared.AxisUnit
type AxisDomainRange = Shared.AxisDomainRange

type Props = (* GenerateMakeFunction *) {
    Hide:                    bool option             // defaultWithAutoWrap Some false
    DataKey:                 string option           // defaultWithAutoWrap JsUndefined
    YAxisId:                 AxisId option           // defaultWithAutoWrap Some (AxisId.Number 0)
    Width:                   int option              // defaultWithAutoWrap JsUndefined
    Height:                  int option              // defaultWithAutoWrap JsUndefined
    Orientation:             YAxisOrientation option // defaultWithAutoWrap Some YAxisOrientation.Left
    Type:                    AxisType option         // defaultWithAutoWrap Some AxisType.Number
    AllowDecimals:           bool option             // defaultWithAutoWrap Some true
    AllowDataOverflow:       bool option             // defaultWithAutoWrap Some false
    AllowDuplicatedCategory: bool option             // defaultWithAutoWrap Some true
    TickCount:               int option              // defaultWithAutoWrap Some 5
    Interval:                AxisInterval option     // defaultWithAutoWrap Some AxisInterval.PreserveEnd
    Padding:                 EdgeInsets option       // defaultWithAutoWrap Some { Top = 0; Bottom = 0; Left = 0; Right = 0; }
    MinTickGap:              int option              // defaultWithAutoWrap Some 5
    TickSize:                int option              // defaultWithAutoWrap Some 6
    Ticks:                   obj array option        // defaultWithAutoWrap JsUndefined
    Mirror:                  bool option             // defaultWithAutoWrap Some false
    Reversed:                bool option             // defaultWithAutoWrap Some false
    Scale:                   AxisScale option        // defaultWithAutoWrap Some AxisScale.Auto
    Unit:                    AxisUnit option         // defaultWithAutoWrap JsUndefined
    Name:                    AxisName option         // defaultWithAutoWrap JsUndefined
    OnClick:                 (unit -> unit) option   // defaultWithAutoWrap JsUndefined
    OnMouseDown:             (unit -> unit) option   // defaultWithAutoWrap JsUndefined
    OnMouseUp:               (unit -> unit) option   // defaultWithAutoWrap JsUndefined
    OnMouseMove:             (unit -> unit) option   // defaultWithAutoWrap JsUndefined
    OnMouseOver:             (unit -> unit) option   // defaultWithAutoWrap JsUndefined
    OnMouseOut:              (unit -> unit) option   // defaultWithAutoWrap JsUndefined
    OnMouseEnter:            (unit -> unit) option   // defaultWithAutoWrap JsUndefined
    OnMouseLeave:            (unit -> unit) option   // defaultWithAutoWrap JsUndefined
    TickFormatter:           (obj -> string) option  // defaultWithAutoWrap JsUndefined
    TickMargin:              int option              // defaultWithAutoWrap JsUndefined
    Domain:                  AxisDomain option       // defaultWithAutoWrap JsUndefined
}

let private YAxis: obj = JsInterop.import "YAxis" "recharts"
let Make =
    LibClient.ThirdParty.wrapComponentTransformingProps<Props>
        YAxis
        (fun (props: Props) ->
            createObjWithOptionalValues [
                "hide"                    ==?> props.Hide
                "dataKey"                 ==?> props.DataKey
                "YAxisId"                 ==?> (props.YAxisId |> Option.map (fun v -> v.ToJS))
                "width"                   ==?> props.Width
                "height"                  ==?> props.Height
                "orientation"             ==?> props.Orientation
                "type"                    ==?> props.Type
                "allowDecimals"           ==?> props.AllowDecimals
                "allowDataOverflow"       ==?> props.AllowDataOverflow
                "allowDuplicatedCategory" ==?> props.AllowDuplicatedCategory
                "tickCount"               ==?> props.TickCount
                "interval"                ==?> (props.Interval |> Option.map (fun v -> v.ToJS))
                "padding"                 ==?> (props.Padding |> Option.map (fun v -> v.ToJS))
                "minTickGap"              ==?> props.MinTickGap
                "tickSize"                ==?> props.TickSize
                "ticks"                   ==?> props.Ticks
                "mirror"                  ==?> props.Mirror
                "reversed"                ==?> props.Reversed
                "scale"                   ==?> props.Scale
                "unit"                    ==?> (props.Unit |> Option.map (fun v -> v.ToJS))
                "name"                    ==?> (props.Name |> Option.map (fun v -> v.ToJS))
                "onClick"                 ==?> props.OnClick
                "onMouseDown"             ==?> props.OnMouseDown
                "onMouseUp"               ==?> props.OnMouseUp
                "onMouseMove"             ==?> props.OnMouseMove
                "onMouseOver"             ==?> props.OnMouseOver
                "onMouseOut"              ==?> props.OnMouseOut
                "onMouseEnter"            ==?> props.OnMouseEnter
                "onMouseLeave"            ==?> props.OnMouseLeave
                "tickFormatter"           ==?> props.TickFormatter
                "tickMargin"              ==?> props.TickMargin
                "domain"                  ==?> (props.Domain |> Option.map (fun v -> v.ToJS))
            ]
        )
