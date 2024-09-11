// https://recharts.org/en-US/api/LineChart

module ThirdParty.Recharts.Components.LineChart

open LibClient
open LibClient.JsInterop

open Fable.Core
open Fable.Core.JsInterop

open ThirdParty.Recharts.Components.Shared

let Horizontal = Layout.Horizontal
let Vertical   = Layout.Vertical

type Props = (* GenerateMakeFunction *) {
    Layout:       Layout option         // defaultWithAutoWrap JsUndefined
    Width:        int option            // defaultWithAutoWrap JsUndefined
    Height:       int option            // defaultWithAutoWrap JsUndefined
    Data:         obj array option      // defaultWithAutoWrap JsUndefined
    Margin:       EdgeInsets option     // defaultWithAutoWrap Some { Top = 5; Bottom = 5; Left = 5; Right = 5; }
    OnClick:      (unit -> unit) option // defaultWithAutoWrap JsUndefined
    OnMouseEnter: (unit -> unit) option // defaultWithAutoWrap JsUndefined
    OnMouseMove:  (unit -> unit) option // defaultWithAutoWrap JsUndefined
    OnMouseLeave: (unit -> unit) option // defaultWithAutoWrap JsUndefined

}

let private LineChart: obj = JsInterop.import "LineChart" "recharts"
let Make =
    LibClient.ThirdParty.wrapComponentTransformingProps<Props>
        LineChart
        (fun (props: Props) ->
            createObjWithOptionalValues [
                "layout"       ==?> props.Layout
                "width"        ==?> props.Width
                "height"       ==?> props.Height
                "data"         ==?> props.Data
                "margin"       ==?> (props.Margin |> Option.map (fun v -> v.ToJS))
                "onClick"      ==?> props.OnClick
                "onMouseEnter" ==?> props.OnMouseEnter
                "onMouseMove"  ==?> props.OnMouseMove
                "onMouseLeave" ==?> props.OnMouseLeave
            ]
        )
