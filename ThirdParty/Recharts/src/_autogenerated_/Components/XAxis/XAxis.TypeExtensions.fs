namespace ThirdParty.Recharts.Components

open LibClient
open LibClient.JsInterop
open Fable.Core
open Fable.Core.JsInterop
open ThirdParty.Recharts.Components.Shared
open ThirdParty.Recharts.Components.XAxis
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module XAxisTypeExtensions =
    type ThirdParty.Recharts.Components.Constructors.Recharts with
        static member XAxis(?children: ReactChildrenProp, ?hide: bool, ?dataKey: string, ?xAxisId: AxisId, ?width: int, ?height: int, ?orientation: XAxisOrientation, ?``type``: AxisType, ?allowDecimals: bool, ?allowDataOverflow: bool, ?allowDuplicatedCategory: bool, ?angle: float, ?tickCount: int, ?interval: AxisInterval, ?padding: EdgeInsets, ?minTickGap: int, ?tickSize: int, ?ticks: obj array, ?mirror: bool, ?reversed: bool, ?scale: AxisScale, ?unit: AxisUnit, ?name: AxisName, ?onClick: (unit -> unit), ?onMouseDown: (unit -> unit), ?onMouseUp: (unit -> unit), ?onMouseMove: (unit -> unit), ?onMouseOver: (unit -> unit), ?onMouseOut: (unit -> unit), ?onMouseEnter: (unit -> unit), ?onMouseLeave: (unit -> unit), ?tickFormatter: (obj -> string), ?tickMargin: int, ?domain: AxisDomain, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Hide = hide |> Option.orElse (Some false)
                    DataKey = dataKey |> Option.orElse (JsUndefined)
                    XAxisId = xAxisId |> Option.orElse (Some (AxisId.Number 0))
                    Width = width |> Option.orElse (JsUndefined)
                    Height = height |> Option.orElse (JsUndefined)
                    Orientation = orientation |> Option.orElse (Some XAxisOrientation.Bottom)
                    Type = ``type`` |> Option.orElse (Some AxisType.Category)
                    AllowDecimals = allowDecimals |> Option.orElse (Some true)
                    AllowDataOverflow = allowDataOverflow |> Option.orElse (Some false)
                    AllowDuplicatedCategory = allowDuplicatedCategory |> Option.orElse (Some true)
                    Angle = angle |> Option.orElse (Some 0.)
                    TickCount = tickCount |> Option.orElse (Some 5)
                    Interval = interval |> Option.orElse (Some AxisInterval.PreserveEnd)
                    Padding = padding |> Option.orElse (Some { Top = 0; Bottom = 0; Left = 0; Right = 0; })
                    MinTickGap = minTickGap |> Option.orElse (Some 5)
                    TickSize = tickSize |> Option.orElse (Some 6)
                    Ticks = ticks |> Option.orElse (JsUndefined)
                    Mirror = mirror |> Option.orElse (Some false)
                    Reversed = reversed |> Option.orElse (Some false)
                    Scale = scale |> Option.orElse (Some AxisScale.Auto)
                    Unit = unit |> Option.orElse (JsUndefined)
                    Name = name |> Option.orElse (JsUndefined)
                    OnClick = onClick |> Option.orElse (JsUndefined)
                    OnMouseDown = onMouseDown |> Option.orElse (JsUndefined)
                    OnMouseUp = onMouseUp |> Option.orElse (JsUndefined)
                    OnMouseMove = onMouseMove |> Option.orElse (JsUndefined)
                    OnMouseOver = onMouseOver |> Option.orElse (JsUndefined)
                    OnMouseOut = onMouseOut |> Option.orElse (JsUndefined)
                    OnMouseEnter = onMouseEnter |> Option.orElse (JsUndefined)
                    OnMouseLeave = onMouseLeave |> Option.orElse (JsUndefined)
                    TickFormatter = tickFormatter |> Option.orElse (JsUndefined)
                    TickMargin = tickMargin |> Option.orElse (JsUndefined)
                    Domain = domain |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            ThirdParty.Recharts.Components.XAxis.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            