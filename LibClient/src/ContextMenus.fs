namespace LibClient.ContextMenus

open Fable.Core.JsInterop
open LibClient
open LibClient.Components
open LibClient.ContextMenus.Types
open LibClient.Responsive

(*
Planned Improvements:
    * nice syntax for optional entries
    * collapsing of adjacent dividers
    * proper button groups with no border radius on internal ones

NOTE whenever you need to debug popups not showing up in the desktop version, you
want to set breakpoints in the RootView.prototype._recalcPosition function of RootView.js
inside ReactXP, the ones that deal with anchor being null.
*)
type ContextMenu =
    static member Open (items: List<ContextMenuItem>) (screenSize: ScreenSize) (maybeAnchor: Option<Fable.React.ReactElement>) (onClose: unit -> unit) (e: ReactEvent.Action) : unit =
        match screenSize with
        | ScreenSize.Desktop  -> ContextMenu.OpenDesktop items maybeAnchor e onClose
        | ScreenSize.Handheld -> LibClient.Dialogs.AdHoc.go (LibClient.Components.ContextMenu.Dialog.Open items onClose onClose) e

    static member private OpenDesktop (items: List<ContextMenuItem>) (maybeAnchor: Option<Fable.React.ReactElement>) (e: ReactEvent.Action) (onClose: unit -> unit) : unit =
        let id = sprintf "popup-%i" (System.Random().Next())

        let contextMenuPopup =
            LibClient.Components.ContextMenu.Popup.Make
                {
                    Items        = items
                    Hide         =
                        fun () ->
                            ReactXP.Helpers.ReactXPRaw?Popup?dismiss(id)
                    OpeningEvent = e
                }
                [||]

        let options = createObj [
            "getAnchor"   ==> fun () -> maybeAnchor |> Option.getOrElseRaise (exn "Need an anchor react element for context menus; no time to fix now")
            "renderPopup" ==> fun (_anchorPosition: obj, _anchorOffset: int, _popupWidth: int, _popupHeight: int) -> contextMenuPopup
            "onDismiss"   ==> onClose
        ]
        ReactXP.Helpers.ReactXPRaw?Popup?show(options, id)
