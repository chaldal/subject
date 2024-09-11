namespace ThirdParty.Recharts.Components

open LibClient
open Fable.Core
open Fable.Core.JsInterop
open ThirdParty.Recharts.Components.Shared
open ThirdParty.Recharts.Components.Pie
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module PieTypeExtensions =
    type ThirdParty.Recharts.Components.Constructors.Recharts with
        static member Pie(?children: ReactChildrenProp, ?cx: Offset, ?cy: Offset, ?innerRadius: Radius, ?outerRadius: Radius, ?startAngle: float, ?endAngle: float, ?minAngle: float, ?paddingAngle: float, ?nameKey: string, ?dataKey: string, ?fill: Color, ?legendType: LegendType, ?data: obj array, ?isAnimationActive: bool, ?animationEasing: AnimationEasing, ?onAnimationStart: (unit -> unit), ?onAnimationEnd: (unit -> unit), ?onClick: (unit -> unit), ?onMouseDown: (unit -> unit), ?onMouseUp: (unit -> unit), ?onMouseMove: (unit -> unit), ?onMouseOver: (unit -> unit), ?onMouseOut: (unit -> unit), ?onMouseEnter: (unit -> unit), ?onMouseLeave: (unit -> unit), ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Cx = cx |> Option.orElse (Some (Offset.Percentage 50.))
                    Cy = cy |> Option.orElse (Some (Offset.Percentage 50.))
                    InnerRadius = innerRadius |> Option.orElse (Some (Radius.Number 0))
                    OuterRadius = outerRadius |> Option.orElse (Some (Radius.Percentage 80.))
                    StartAngle = startAngle |> Option.orElse (Some 0.)
                    EndAngle = endAngle |> Option.orElse (Some 360.)
                    MinAngle = minAngle |> Option.orElse (Some 0.)
                    PaddingAngle = paddingAngle |> Option.orElse (Some 0.)
                    NameKey = nameKey |> Option.orElse (Some "Name")
                    DataKey = dataKey |> Option.orElse (JsUndefined)
                    Fill = fill |> Option.orElse (JsUndefined)
                    LegendType = legendType |> Option.orElse (Some LegendType.Rectangle)
                    Data = data |> Option.orElse (JsUndefined)
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
            ThirdParty.Recharts.Components.Pie.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            