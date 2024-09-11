// https://recharts.org/en-US/api/AreaChart

module ThirdParty.Recharts.Components.AreaChart

open LibClient
open LibClient.JsInterop

open Fable.Core

open ThirdParty.Recharts.Components.Shared

let Horizontal = Layout.Horizontal
let Vertical   = Layout.Vertical

let Expand     = StackOffset.Expand
let Wiggle     = StackOffset.Wiggle
let Silhouette = StackOffset.Silhouette
let None       = StackOffset.None

type Props = (* GenerateMakeFunction *) {
    Layout:       Layout option         // defaultWithAutoWrap JsUndefined
    Width:        int option            // defaultWithAutoWrap JsUndefined
    Height:       int option            // defaultWithAutoWrap JsUndefined
    Data:         obj array option      // defaultWithAutoWrap JsUndefined
    Margin:       EdgeInsets option     // defaultWithAutoWrap Some { Top = 5; Bottom = 5; Left = 5; Right = 5; }
    StackOffset:  StackOffset option    // defaultWithAutoWrap JsUndefined
    OnClick:      (unit -> unit) option // defaultWithAutoWrap JsUndefined
    OnMouseEnter: (unit -> unit) option // defaultWithAutoWrap JsUndefined
    OnMouseMove:  (unit -> unit) option // defaultWithAutoWrap JsUndefined
    OnMouseLeave: (unit -> unit) option // defaultWithAutoWrap JsUndefined

}

let private AreaChart: obj = JsInterop.import "AreaChart" "recharts"
let Make =
    LibClient.ThirdParty.wrapComponentTransformingProps<Props>
        AreaChart
        (fun (props: Props) ->
            createObjWithOptionalValues [
                "children"     ==!> props?children // TODO check that removing unpackAsFragment doesn't break it
                "layout"       ==?> props.Layout
                "width"        ==?> props.Width
                "height"       ==?> props.Height
                "data"         ==?> props.Data
                "margin"       ==?> (props.Margin |> Option.map (fun v -> v.ToJS))
                "stackOffset"  ==?> props.StackOffset
                "onClick"      ==?> props.OnClick
                "onMouseEnter" ==?> props.OnMouseEnter
                "onMouseMove"  ==?> props.OnMouseMove
                "onMouseLeave" ==?> props.OnMouseLeave
            ]
        )
