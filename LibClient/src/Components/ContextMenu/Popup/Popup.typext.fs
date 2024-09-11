module LibClient.Components.ContextMenu.Popup

open LibClient
open LibClient.ContextMenus.Types

type Props = (* GenerateMakeFunction *) {
    Items:        List<ContextMenuItem>
    Hide:         unit -> unit
    OpeningEvent: ReactEvent.Action
}

type Popup(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Popup>("LibClient.Components.ContextMenu.Popup", _initialProps, NoActions, hasStyles = true)

and Actions = unit

let Make = makeConstructor<Popup, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
