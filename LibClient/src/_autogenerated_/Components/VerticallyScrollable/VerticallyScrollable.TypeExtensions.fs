namespace LibClient.Components

open LibClient
open ReactXP.Styles
open LibClient.Components.VerticallyScrollable
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module VerticallyScrollableTypeExtensions =
    type LibClient.Components.Constructors.LC with
        static member VerticallyScrollable(?children: ReactChildrenProp, ?fixedTop: ReactElement, ?fixedBottom: ReactElement, ?scrollableMiddle: ReactElement, ?styles: array<ViewStyles>, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    FixedTop = fixedTop |> Option.orElse (None)
                    FixedBottom = fixedBottom |> Option.orElse (None)
                    ScrollableMiddle = scrollableMiddle |> Option.orElse (None)
                    styles = styles |> Option.orElse (None)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.VerticallyScrollable.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            