namespace LibClient.Components

open LibClient
open Fable.React
open ReactXP.Types
open ReactXP.Styles
open LibClient.JsInterop
open LibClient.Components.ScrollView
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module ScrollViewTypeExtensions =
    type LibClient.Components.Constructors.LC with
        static member ScrollView(scroll: Scroll, restoreScroll: RestoreScroll, ?children: ReactChildrenProp, ?showsHorizontalScrollIndicatorOnNative: bool, ?showsVerticalScrollIndicatorOnNative: bool, ?onScroll: (int * int -> unit), ?onLayout: (ReactXP.Types.ViewOnLayoutEvent -> unit), ?styles: array<ViewStyles>, ?ref: (LibClient.JsInterop.JsNullable<IScrollViewComponentRef> -> unit), ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Scroll = scroll
                    RestoreScroll = restoreScroll
                    ShowsHorizontalScrollIndicatorOnNative = defaultArg showsHorizontalScrollIndicatorOnNative (true)
                    ShowsVerticalScrollIndicatorOnNative = defaultArg showsVerticalScrollIndicatorOnNative (true)
                    OnScroll = onScroll |> Option.orElse (None)
                    OnLayout = onLayout |> Option.orElse (None)
                    styles = styles |> Option.orElse (None)
                    ref = ref |> Option.orElse (Undefined)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.ScrollView.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            