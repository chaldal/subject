namespace LibClient.Components

open LibClient
open ReactXP.Styles
open LibClient.Components.Legacy.Card
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Legacy_CardTypeExtensions =
    type LibClient.Components.Constructors.LC.Legacy with
        static member Card(?children: ReactChildrenProp, ?style: Style, ?onPress: (ReactEvent.Action -> unit), ?styles: array<ViewStyles>, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Style = defaultArg style (Style.Shadowed)
                    OnPress = onPress |> Option.orElse (None)
                    styles = styles |> Option.orElse (None)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Legacy.Card.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            