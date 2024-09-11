namespace LibClient.Components

open LibClient
open LibClient.JsInterop
open System
open ReactXP.Styles
open LibClient.Components.VirtualListView
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module VirtualListViewTypeExtensions =
    type LibClient.Components.Constructors.LC with
        static member VirtualListView(items: seq<VirtualListItem<'Item>>, render: 'Item -> ReactElement, restoreScroll: RestoreScroll, ?children: ReactChildrenProp, ?scrollSideEffect: (int * int -> unit), ?styles: array<ViewStyles>, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Items = items
                    Render = render
                    RestoreScroll = restoreScroll
                    ScrollSideEffect = scrollSideEffect |> Option.orElse (None)
                    styles = styles |> Option.orElse (None)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.VirtualListView.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            