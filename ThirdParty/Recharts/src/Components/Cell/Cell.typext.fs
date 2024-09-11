// https://recharts.org/en-US/api/Cell

module ThirdParty.Recharts.Components.Cell

open LibClient

open Fable.Core
open Fable.Core.JsInterop

type Props = (* GenerateMakeFunction *) {
    Fill:        Color option // defaultWithAutoWrap JsUndefined
    Stroke:      Color option // defaultWithAutoWrap JsUndefined
    StrokeWidth: int option   // defaultWithAutoWrap JsUndefined
}

let private Cell : obj = JsInterop.import "Cell" "recharts"
let Make =
    LibClient.ThirdParty.wrapComponentTransformingProps<Props>
        Cell
        (fun (props: Props) ->
            createObj [
                "fill"        ==> (props.Fill |> Option.map (fun v -> v.ToReactXPString))
                "stroke"      ==> (props.Stroke |> Option.map (fun v -> v.ToReactXPString))
                "strokeWidth" ==> props.StrokeWidth
            ]
        )

