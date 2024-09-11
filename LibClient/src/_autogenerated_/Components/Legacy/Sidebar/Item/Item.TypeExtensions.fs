namespace LibClient.Components

open LibClient
open LibClient.Components.Legacy.Sidebar.Item
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Legacy_Sidebar_ItemTypeExtensions =
    type LibClient.Components.Constructors.LC.Legacy.Sidebar with
        static member Item(value: Value, isSelected: bool, onPress: (ReactEvent.Action -> unit), ?children: ReactChildrenProp, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Value = value
                    IsSelected = isSelected
                    OnPress = onPress
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Legacy.Sidebar.Item.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            