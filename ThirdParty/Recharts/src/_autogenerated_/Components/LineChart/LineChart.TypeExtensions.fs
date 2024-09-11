namespace ThirdParty.Recharts.Components

open LibClient
open LibClient.JsInterop
open Fable.Core
open Fable.Core.JsInterop
open ThirdParty.Recharts.Components.Shared
open ThirdParty.Recharts.Components.LineChart
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module LineChartTypeExtensions =
    type ThirdParty.Recharts.Components.Constructors.Recharts with
        static member LineChart(?children: ReactChildrenProp, ?layout: Layout, ?width: int, ?height: int, ?data: obj array, ?margin: EdgeInsets, ?onClick: (unit -> unit), ?onMouseEnter: (unit -> unit), ?onMouseMove: (unit -> unit), ?onMouseLeave: (unit -> unit), ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Layout = layout |> Option.orElse (JsUndefined)
                    Width = width |> Option.orElse (JsUndefined)
                    Height = height |> Option.orElse (JsUndefined)
                    Data = data |> Option.orElse (JsUndefined)
                    Margin = margin |> Option.orElse (Some { Top = 5; Bottom = 5; Left = 5; Right = 5; })
                    OnClick = onClick |> Option.orElse (JsUndefined)
                    OnMouseEnter = onMouseEnter |> Option.orElse (JsUndefined)
                    OnMouseMove = onMouseMove |> Option.orElse (JsUndefined)
                    OnMouseLeave = onMouseLeave |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            ThirdParty.Recharts.Components.LineChart.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            