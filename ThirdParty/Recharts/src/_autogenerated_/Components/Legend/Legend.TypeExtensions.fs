namespace ThirdParty.Recharts.Components

open LibClient
open LibClient.JsInterop
open Fable.Core
open ThirdParty.Recharts.Components.Shared
open ThirdParty.Recharts.Components.Legend
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module LegendTypeExtensions =
    type ThirdParty.Recharts.Components.Constructors.Recharts with
        static member Legend(?children: ReactChildrenProp, ?width: int, ?height: int, ?layout: Layout, ?horizontalAlignment: HorizontalAlignment, ?verticalAlignment: VerticalAlignment, ?iconSize: int, ?``type``: LegendType, ?margin: EdgeInsets, ?onClick: (unit -> unit), ?onMouseDown: (unit -> unit), ?onMouseUp: (unit -> unit), ?onMouseMove: (unit -> unit), ?onMouseOver: (unit -> unit), ?onMouseOut: (unit -> unit), ?onMouseEnter: (unit -> unit), ?onMouseLeave: (unit -> unit), ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Width = width |> Option.orElse (JsUndefined)
                    Height = height |> Option.orElse (JsUndefined)
                    Layout = layout |> Option.orElse (Some Layout.Horizontal)
                    HorizontalAlignment = horizontalAlignment |> Option.orElse (Some HorizontalAlignment.Center)
                    VerticalAlignment = verticalAlignment |> Option.orElse (Some VerticalAlignment.Bottom)
                    IconSize = iconSize |> Option.orElse (Some 14)
                    Type = ``type`` |> Option.orElse (JsUndefined)
                    Margin = margin |> Option.orElse (JsUndefined)
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
            ThirdParty.Recharts.Components.Legend.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            