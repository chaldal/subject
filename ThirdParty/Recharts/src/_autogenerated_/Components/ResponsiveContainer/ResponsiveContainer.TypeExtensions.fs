namespace ThirdParty.Recharts.Components

open LibClient
open LibClient.JsInterop
open Fable.Core
open Fable.Core.JsInterop
open ThirdParty.Recharts.Components.Shared
open ThirdParty.Recharts.Components.ResponsiveContainer
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module ResponsiveContainerTypeExtensions =
    type ThirdParty.Recharts.Components.Constructors.Recharts with
        static member ResponsiveContainer(?children: ReactChildrenProp, ?aspect: float, ?width: Size, ?height: Size, ?minWidth: int, ?minHeight: int, ?debounce: int, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Aspect = aspect |> Option.orElse (JsUndefined)
                    Width = width |> Option.orElse (Some (Size.Percentage 100.))
                    Height = height |> Option.orElse (Some (Size.Percentage 100.))
                    MinWidth = minWidth |> Option.orElse (JsUndefined)
                    MinHeight = minHeight |> Option.orElse (JsUndefined)
                    Debounce = debounce |> Option.orElse (Some 0)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            ThirdParty.Recharts.Components.ResponsiveContainer.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            