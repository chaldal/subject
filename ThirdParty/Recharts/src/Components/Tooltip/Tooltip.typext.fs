// https://recharts.org/en-US/api/Tooltip

module ThirdParty.Recharts.Components.Tooltip

open LibClient

open Fable.Core
open Fable.Core.JsInterop
open ThirdParty.Recharts.Components.Shared

let Ease      = AnimationEasing.Ease
let EaseIn    = AnimationEasing.EaseIn
let EaseOut   = AnimationEasing.EaseOut
let EaseInOut = AnimationEasing.EaseInOut
let Linear    = AnimationEasing.Linear

type Payload = {
    color: string
    name: obj
    value: obj

    // Some other parts elided for now. See https://github.com/recharts/recharts/blob/master/src/component/DefaultTooltipContent.tsx#L24.
}

type ContentInput = {
    active: bool
    payload: Payload array
    label: obj
}

type Props = (* GenerateMakeFunction *) {
    Separator:           string option                         // defaultWithAutoWrap Some ":"
    Offset:              int option                            // defaultWithAutoWrap Some 10
    FilterNull:          bool option                           // defaultWithAutoWrap Some true
    ViewBox:             ViewBox option                        // defaultWithAutoWrap JsUndefined
    Active:              bool option                           // defaultWithAutoWrap Some false
    Position:            Position option                       // defaultWithAutoWrap JsUndefined
    Coordinate:          Position option                       // defaultWithAutoWrap JsUndefined
    Content:             (ContentInput -> ReactElement) option // defaultWithAutoWrap JsUndefined
    IsAnimationActive:   bool option                           // defaultWithAutoWrap JsUndefined
    AnimationEasing:     AnimationEasing option                // defaultWithAutoWrap Some AnimationEasing.Ease
    AnimationBeginMs:    int option                            // defaultWithAutoWrap Some 0
    AnimationDurationMs: int option                            // defaultWithAutoWrap Some 1500
}

let private Tooltip: obj = JsInterop.import "Tooltip" "recharts"
let Make =
    LibClient.ThirdParty.wrapComponentTransformingProps<Props>
        Tooltip
        (fun (props: Props) ->
            createObj [
                "separator"         ==> props.Separator
                "offset"            ==> props.Offset
                "filterNull"        ==> props.FilterNull
                "viewBox"           ==> (props.ViewBox |> Option.map (fun v -> v.ToJS))
                "active"            ==> props.Active
                "position"          ==> (props.Position |> Option.map (fun v -> v.ToJS))
                "coordinate"        ==> (props.Coordinate |> Option.map (fun v -> v.ToJS))
                "isAnimationActive" ==> props.IsAnimationActive
                "animationEasing"   ==> props.AnimationEasing
                "animationBegin"    ==> props.AnimationBeginMs
                "animationDuration" ==> props.AnimationDurationMs
                "content"           ==> props.Content
            ]
        )
