namespace LibClient.Components

open LibClient
open ReactXP.Styles
open LibClient.Components.FloatingActionButton
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module FloatingActionButtonTypeExtensions =
    type LibClient.Components.Constructors.LC with
        static member FloatingActionButton(icon: LibClient.Icons.IconConstructor, state: ButtonHighLevelState, ?children: ReactChildrenProp, ?label: string, ?styles: array<ViewStyles>, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Icon = icon
                    State = state
                    Label = label |> Option.orElse (None)
                    styles = styles |> Option.orElse (None)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.FloatingActionButton.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            