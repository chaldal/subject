namespace ThirdParty.Recharts.Components

open LibClient
open Fable.Core
open Fable.Core.JsInterop
open ThirdParty.Recharts.Components.Shared
open ThirdParty.Recharts.Components.Tooltip
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module TooltipTypeExtensions =
    type ThirdParty.Recharts.Components.Constructors.Recharts with
        static member Tooltip(?children: ReactChildrenProp, ?separator: string, ?offset: int, ?filterNull: bool, ?viewBox: ViewBox, ?active: bool, ?position: Position, ?coordinate: Position, ?content: (ContentInput -> ReactElement), ?isAnimationActive: bool, ?animationEasing: AnimationEasing, ?animationBeginMs: int, ?animationDurationMs: int, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Separator = separator |> Option.orElse (Some ":")
                    Offset = offset |> Option.orElse (Some 10)
                    FilterNull = filterNull |> Option.orElse (Some true)
                    ViewBox = viewBox |> Option.orElse (JsUndefined)
                    Active = active |> Option.orElse (Some false)
                    Position = position |> Option.orElse (JsUndefined)
                    Coordinate = coordinate |> Option.orElse (JsUndefined)
                    Content = content |> Option.orElse (JsUndefined)
                    IsAnimationActive = isAnimationActive |> Option.orElse (JsUndefined)
                    AnimationEasing = animationEasing |> Option.orElse (Some AnimationEasing.Ease)
                    AnimationBeginMs = animationBeginMs |> Option.orElse (Some 0)
                    AnimationDurationMs = animationDurationMs |> Option.orElse (Some 1500)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            ThirdParty.Recharts.Components.Tooltip.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            