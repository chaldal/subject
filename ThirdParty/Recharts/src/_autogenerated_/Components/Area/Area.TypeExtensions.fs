namespace ThirdParty.Recharts.Components

open LibClient
open LibClient.JsInterop
open Fable.Core
open ThirdParty.Recharts.Components.Shared
open ThirdParty.Recharts.Components.Area
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module AreaTypeExtensions =
    type ThirdParty.Recharts.Components.Constructors.Recharts with
        static member Area(?children: ReactChildrenProp, ?``type``: Type, ?dataKey: string, ?legendType: LegendType, ?name: string, ?stroke: Color, ?strokeWidth: int, ?fill: Color, ?stackId: StackId, ?isAnimationActive: bool, ?animationEasing: AnimationEasing, ?onAnimationStart: (unit -> unit), ?onAnimationEnd: (unit -> unit), ?onClick: (unit -> unit), ?onMouseDown: (unit -> unit), ?onMouseUp: (unit -> unit), ?onMouseMove: (unit -> unit), ?onMouseOver: (unit -> unit), ?onMouseOut: (unit -> unit), ?onMouseEnter: (unit -> unit), ?onMouseLeave: (unit -> unit), ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Type = ``type`` |> Option.orElse (Some Type.Linear)
                    DataKey = dataKey |> Option.orElse (JsUndefined)
                    LegendType = legendType |> Option.orElse (JsUndefined)
                    Name = name |> Option.orElse (JsUndefined)
                    Stroke = stroke |> Option.orElse (JsUndefined)
                    StrokeWidth = strokeWidth |> Option.orElse (Some 1)
                    Fill = fill |> Option.orElse (JsUndefined)
                    StackId = stackId |> Option.orElse (JsUndefined)
                    IsAnimationActive = isAnimationActive |> Option.orElse (JsUndefined)
                    AnimationEasing = animationEasing |> Option.orElse (Some AnimationEasing.Ease)
                    OnAnimationStart = onAnimationStart |> Option.orElse (JsUndefined)
                    OnAnimationEnd = onAnimationEnd |> Option.orElse (JsUndefined)
                    OnClick = onClick |> Option.orElse (JsUndefined)
                    OnMouseDown = onMouseDown |> Option.orElse (JsUndefined)
                    OnMouseUp = onMouseUp |> Option.orElse (JsUndefined)
                    OnMouseMove = onMouseMove |> Option.orElse (JsUndefined)
                    OnMouseOver = onMouseOver |> Option.orElse (JsUndefined)
                    OnMouseOut = onMouseOut |> Option.orElse (JsUndefined)
                    OnMouseEnter = onMouseEnter |> Option.orElse (JsUndefined)
                    OnMouseLeave = onMouseLeave |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            ThirdParty.Recharts.Components.Area.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            