namespace ThirdParty.Recharts.Components

open LibClient
open LibClient.JsInterop
open Fable.Core
open Fable.Core.JsInterop
open ThirdParty.Recharts.Components.CartesianGrid
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module CartesianGridTypeExtensions =
    type ThirdParty.Recharts.Components.Constructors.Recharts with
        static member CartesianGrid(?children: ReactChildrenProp, ?x: int, ?y: int, ?width: int, ?height: int, ?horizontal: bool, ?vertical: bool, ?horizontalPoints: float array, ?verticalPoints: float array, ?strokeDashArray: float array, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    X = x |> Option.orElse (JsUndefined)
                    Y = y |> Option.orElse (JsUndefined)
                    Width = width |> Option.orElse (JsUndefined)
                    Height = height |> Option.orElse (JsUndefined)
                    Horizontal = horizontal |> Option.orElse (Some true)
                    Vertical = vertical |> Option.orElse (Some true)
                    HorizontalPoints = horizontalPoints |> Option.orElse (Some [||])
                    VerticalPoints = verticalPoints |> Option.orElse (Some [||])
                    StrokeDashArray = strokeDashArray |> Option.orElse (Some [||])
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            ThirdParty.Recharts.Components.CartesianGrid.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            