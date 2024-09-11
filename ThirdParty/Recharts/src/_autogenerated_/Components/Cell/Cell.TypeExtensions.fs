namespace ThirdParty.Recharts.Components

open LibClient
open Fable.Core
open Fable.Core.JsInterop
open ThirdParty.Recharts.Components.Cell
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module CellTypeExtensions =
    type ThirdParty.Recharts.Components.Constructors.Recharts with
        static member Cell(?children: ReactChildrenProp, ?fill: Color, ?stroke: Color, ?strokeWidth: int, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Fill = fill |> Option.orElse (JsUndefined)
                    Stroke = stroke |> Option.orElse (JsUndefined)
                    StrokeWidth = strokeWidth |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            ThirdParty.Recharts.Components.Cell.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            