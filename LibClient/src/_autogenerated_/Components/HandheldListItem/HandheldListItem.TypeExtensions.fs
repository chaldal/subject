namespace LibClient.Components

open LibClient
open LibClient.Components.HandheldListItem
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module HandheldListItemTypeExtensions =
    type LibClient.Components.Constructors.LC with
        static member HandheldListItem(label: Label, state: State, ?children: ReactChildrenProp, ?leftIcon: (int -> LibClient.Icons.Icon) option, ?right: Right, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Label = label
                    State = state
                    LeftIcon = defaultArg leftIcon (None)
                    Right = right |> Option.orElse (None)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.HandheldListItem.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            