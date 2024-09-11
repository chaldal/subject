[<AutoOpen>]
module LibClient.ContextMenus.Types

open LibClient

type ContextMenuItem =
| Divider
| Heading          of string
| InternalButton   of string * IsSelected: bool * (ReactEvent.Action -> unit)
| ButtonCautionary of string * (ReactEvent.Action -> unit)
with
    static member Button (label: string, onPress: ReactEvent.Action -> unit, ?isSelected: bool) : ContextMenuItem =
        InternalButton (label, defaultArg isSelected false, onPress)
