[<AutoOpen>]
module LibClient.ActionsModule

open LibClient.AsyncHelpers

let private areYouSure: string = "Are you sure?"

type Action =
    static member alert (message: string) : unit =
        LibClient.Dialogs.AdHoc.go
            (LibClient.Components.Dialog.Confirm.OpenAsAlert None message)
            // this is a hack, I don't want to plumb this change all the way to all use sites
            ReactEvent.Action.NonUserOriginatingAction
        //Browser.Dom.window.alert message

    static member confirm (message: string) : bool =
        Browser.Dom.window.confirm message

    static member ifConfirmed (message: string, action: Async<unit>) : unit =
        if Action.confirm message then
            action |> startSafely

    static member ifConfirmed (action: Async<unit>) : unit =
        if Action.confirm areYouSure then
            action |> startSafely

    static member alertUserAndLogError (message: string, ?data: obj) : unit =
        Action.alert message
        Log.Error (message, data)

    static member runActionAlertOnError (action: Async<Result<unit, string>>) : unit =
        async {
            let! actionResult = action
            match actionResult with
            | Ok _          -> ()
            | Error message -> Action.alert message
        } |> startSafely

