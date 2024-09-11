module LibClient.Components.ContextMenu.Dialog

open LibLang
open LibClient
open LibClient.Dialogs
open LibClient.ContextMenus.Types

type Parameters = {
    Items: List<ContextMenuItem>
}

type Props = DialogProps<Parameters, unit>

type Estate = EmptyRecordType
type Pstate = EmptyRecordType

type Dialog(initialProps) =
    inherit DialogComponent<Parameters, unit, Estate, Pstate, Actions, Dialog>("LibClient.Components.ContextMenu.Dialog", initialProps.PstoreKey, initialProps, Actions, hasStyles = true)

    override _.GetDefaultPstate(_initialProps: Props) = EmptyRecord

    override _.GetInitialEstate(_initialProps: Props) = EmptyRecord

    override _.CanCancel() : Async<bool> = async {
        return true
    }

and Actions(this: Dialog) =
    member _.TryCancel (e: ReactEvent.Action) : unit =
        this.TryCancel DialogCloseMethod.HistoryBack e


let Make = makeConstructor<Dialog,_,_>

let Open (items: List<ContextMenuItem>) (onResult: unit -> unit) (onCancel: unit -> unit) (close: DialogCloseMethod -> ReactEvent.Action -> unit) : ReactElement =
    doOpen
        "ContextMenu.Dialog"
        {
            Items = items
        }
        Make
        {
            OnResult      = onResult
            MaybeOnCancel = Some onCancel
        }
        close
