namespace LibClient.Components

open LibClient
open ReactXP.Styles
open LibClient.Components.Input.ChoiceList
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Input_ChoiceListTypeExtensions =
    type LibClient.Components.Constructors.LC.Input with
        static member ChoiceList(items: Group<'T> -> ReactElement, value: SelectableValue<'T>, validity: InputValidity, ?children: ReactChildrenProp, ?styles: array<ViewStyles>, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Items = items
                    Value = value
                    Validity = validity
                    styles = styles |> Option.orElse (None)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Input.ChoiceList.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            