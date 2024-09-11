namespace LibClient.Components

open LibClient
open ReactXP.Styles
open LibClient.Components.Sidebar.Item
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Sidebar_ItemTypeExtensions =
    type LibClient.Components.Constructors.LC.Sidebar with
        static member Item(label: string, state: State, ?children: ReactChildrenProp, ?leftIcon: Icons.IconConstructor, ?right: Right, ?styles: array<ViewStyles>, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Label = label
                    State = state
                    LeftIcon = leftIcon |> Option.orElse (None)
                    Right = right |> Option.orElse (None)
                    styles = styles |> Option.orElse (None)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Sidebar.Item.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            