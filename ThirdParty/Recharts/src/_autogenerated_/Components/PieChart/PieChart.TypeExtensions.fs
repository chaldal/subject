namespace ThirdParty.Recharts.Components

open LibClient
open Fable.Core
open Fable.Core.JsInterop
open ThirdParty.Recharts.Components.Shared
open ThirdParty.Recharts.Components.PieChart
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module PieChartTypeExtensions =
    type ThirdParty.Recharts.Components.Constructors.Recharts with
        static member PieChart(?children: ReactChildrenProp, ?width: int, ?height: int, ?margin: EdgeInsets, ?onClick: (unit -> unit), ?onMouseEnter: (unit -> unit), ?onMouseLeave: (unit -> unit), ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Width = width |> Option.orElse (JsUndefined)
                    Height = height |> Option.orElse (JsUndefined)
                    Margin = margin |> Option.orElse (Some { Top = 5; Bottom = 5; Left = 5; Right = 5; })
                    OnClick = onClick |> Option.orElse (JsUndefined)
                    OnMouseEnter = onMouseEnter |> Option.orElse (JsUndefined)
                    OnMouseLeave = onMouseLeave |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            ThirdParty.Recharts.Components.PieChart.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            