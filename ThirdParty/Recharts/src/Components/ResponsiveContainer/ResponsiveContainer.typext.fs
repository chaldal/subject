// https://recharts.org/en-US/api/ResponsiveContainer

module ThirdParty.Recharts.Components.ResponsiveContainer

open LibClient
open LibClient.JsInterop

open Fable.Core
open Fable.Core.JsInterop
open ThirdParty.Recharts.Components.Shared

type Props = (* GenerateMakeFunction *) {
    Aspect:    float option // defaultWithAutoWrap JsUndefined
    Width:     Size option  // defaultWithAutoWrap Some (Size.Percentage 100.)
    Height:    Size option  // defaultWithAutoWrap Some (Size.Percentage 100.)
    MinWidth:  int option   // defaultWithAutoWrap JsUndefined
    MinHeight: int option   // defaultWithAutoWrap JsUndefined
    Debounce:  int option   // defaultWithAutoWrap Some 0

}

let private ResponsiveContainer: obj = JsInterop.import "ResponsiveContainer" "recharts"
let Make =
    LibClient.ThirdParty.wrapComponentTransformingProps<Props>
        ResponsiveContainer
        (fun (props: Props) ->
            createObjWithOptionalValues [
                "children"  ==!> props?children // TODO check that removing unpackAsFragment doesn't break it
                "aspect"    ==?> props.Aspect
                "width"     ==?> (props.Width |> Option.map (fun v -> v.ToJS))
                "height"    ==?> (props.Height |> Option.map (fun v -> v.ToJS))
                "minWidth"  ==?> props.MinWidth
                "minHeight" ==?> props.MinHeight
                "debounce"  ==?> props.Debounce
            ]
        )
