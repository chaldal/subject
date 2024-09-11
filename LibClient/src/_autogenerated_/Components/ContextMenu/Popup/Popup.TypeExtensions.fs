namespace LibClient.Components

open LibClient
open LibClient.ContextMenus.Types
open LibClient.Components.ContextMenu.Popup
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module ContextMenu_PopupTypeExtensions =
    type LibClient.Components.Constructors.LC.ContextMenu with
        static member Popup(items: List<ContextMenuItem>, hide: unit -> unit, openingEvent: ReactEvent.Action, ?children: ReactChildrenProp, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Items = items
                    Hide = hide
                    OpeningEvent = openingEvent
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.ContextMenu.Popup.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            