namespace LibClient.Components

open LibClient
open ReactXP.Styles
open LibClient.Components.Nav.Bottom.Item
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Nav_Bottom_ItemTypeExtensions =
    type LibClient.Components.Constructors.LC.Nav.Bottom with
        static member Item(state: State, style: Style, ?children: ReactChildrenProp, ?styles: array<ViewStyles>, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    State = state
                    Style = style
                    styles = styles |> Option.orElse (None)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Nav.Bottom.Item.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            