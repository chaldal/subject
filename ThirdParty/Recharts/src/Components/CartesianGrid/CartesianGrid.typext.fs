// https://recharts.org/en-US/api/CartesianGrid

module ThirdParty.Recharts.Components.CartesianGrid

open LibClient
open LibClient.JsInterop

open Fable.Core
open Fable.Core.JsInterop

type Props = (* GenerateMakeFunction *) {
    X:                int option         // defaultWithAutoWrap JsUndefined
    Y:                int option         // defaultWithAutoWrap JsUndefined
    Width:            int option         // defaultWithAutoWrap JsUndefined
    Height:           int option         // defaultWithAutoWrap JsUndefined
    Horizontal:       bool option        // defaultWithAutoWrap Some true
    Vertical:         bool option        // defaultWithAutoWrap Some true
    HorizontalPoints: float array option // defaultWithAutoWrap Some [||]
    VerticalPoints:   float array option // defaultWithAutoWrap Some [||]
    StrokeDashArray:  float array option // defaultWithAutoWrap Some [||]
}

let private CartesianGrid: obj = JsInterop.import "CartesianGrid" "recharts"
let Make =
    LibClient.ThirdParty.wrapComponentTransformingProps<Props>
        CartesianGrid
        (fun (props: Props) ->
            createObjWithOptionalValues [
                "x"                ==?> props.X
                "y"                ==?> props.Y
                "width"            ==?> props.Width
                "height"           ==?> props.Height
                "horizontal"       ==?> props.Horizontal
                "vertical"         ==?> props.Vertical
                "horizontalPoints" ==?> props.HorizontalPoints
                "verticalPoints"   ==?> props.VerticalPoints
                "strokeDasharray"  ==?> props.StrokeDashArray
            ]
        )

