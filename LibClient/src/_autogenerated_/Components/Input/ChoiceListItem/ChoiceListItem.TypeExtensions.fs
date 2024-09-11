namespace LibClient.Components

open LibClient
open Fable.React
open ReactXP.Styles
open LibClient.Components.Input.ChoiceListItem
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Input_ChoiceListItemTypeExtensions =
    type LibClient.Components.Constructors.LC.Input with
        static member ChoiceListItem(value: 'T, group: LibClient.Components.Input.ChoiceList.Group<'T>, ?children: ReactChildrenProp, ?label: Label, ?iconPosition: IconPosition, ?styles: array<ViewStyles>, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Value = value
                    Group = group
                    Label = defaultArg label (Children)
                    IconPosition = defaultArg iconPosition (IconPosition.Left)
                    styles = styles |> Option.orElse (None)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Input.ChoiceListItem.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            