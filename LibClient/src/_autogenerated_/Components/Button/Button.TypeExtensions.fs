namespace LibClient.Components

open LibClient
open ReactXP.Styles
open LibClient.Components.Button
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module ButtonTypeExtensions =
    type LibClient.Components.Constructors.LC with
        static member Button(label: string, state: ButtonHighLevelState, ?children: ReactChildrenProp, ?level: Level, ?icon: Icon, ?badge: Badge, ?styles: array<ViewStyles>, ?contentContainerStyles: array<ViewStyles>, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Label = label
                    State = state
                    Level = defaultArg level (Primary)
                    Icon = defaultArg icon (No)
                    Badge = badge |> Option.orElse (None)
                    styles = styles |> Option.orElse (None)
                    contentContainerStyles = contentContainerStyles |> Option.orElse (None)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Button.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            