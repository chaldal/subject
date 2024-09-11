module LibClient.Components.Dialog.Confirm

open LibLang
open LibClient
open LibClient.Dialogs

type Button =
| Cancel       of Label: string * LibClient.Components.Button.Level * (unit -> unit)
| Confirm      of Label: string * LibClient.Components.Button.Level * (unit -> unit)
| AsyncConfirm of Label: string * LibClient.Components.Button.Level * (unit -> Async<Result<unit, string>>)

type Parameters = {
    MaybeHeading: Option<string>
    Details:      string
    Buttons:      List<Button>
}

type Props = DialogProps<Parameters, unit>

[<RequireQualifiedAccess>]
type Mode =
| Initial
| InProgress
| Error of Message: string

type Estate = {
    Mode: Mode
}

type Pstate = EmptyRecordType

type Confirm(initialProps) =
    inherit DialogComponent<Parameters, unit, Estate, Pstate, Actions, Confirm>("LibClient.Components.Dialog.Confirm", initialProps.PstoreKey, initialProps, Actions, hasStyles = true)

    override _.GetDefaultPstate(_initialProps: Props) = EmptyRecord

    override _.GetInitialEstate(_initialProps: Props) = {
        Mode = Mode.Initial
    }

    override this.CanCancel() : Async<bool> = async {
        return this.state.estate.Mode <> Mode.InProgress
    }

and Actions(this: Confirm) =
    member _.TryCancel (e: ReactEvent.Action) : unit =
        this.TryCancel DialogCloseMethod.HistoryBack e

    member _.Hide (e: ReactEvent.Action) : unit =
        this.Hide DialogCloseMethod.HistoryBack e

    member _.AsyncConfirm (work: unit -> Async<Result<unit, string>>) (e: ReactEvent.Action) : unit =
        this.SetEstate (fun estate _ _ -> { estate with Mode = Mode.InProgress })
        async {
            match! work() with
            | Error message ->
                this.SetEstate (fun estate _ _ -> { estate with Mode = Mode.Error message })
            | Ok _ ->
                this.Hide DialogCloseMethod.HistoryBack e
        } |> startSafely

let Make = makeConstructor<Confirm,_,_>

// NOTE we can't take Parameters here because of circular dependency with DialogsInterface/DialogsImplementation
let Open (maybeHeading: Option<string>) (details: string) (buttons: List<Button>) (close: DialogCloseMethod -> ReactEvent.Action -> unit) : ReactElement =
    doOpen
        "DialogConfirm"
        {
            MaybeHeading = maybeHeading
            Details      = details
            Buttons      = buttons
        }
        Make
        {
            OnResult      = NoopFn
            MaybeOnCancel = None
        }
        close

let OpenSimple (maybeHeading: Option<string>) (details: string) (cancelLabel: string) (okLabel: string) (onResult: bool -> unit) (close: DialogCloseMethod -> ReactEvent.Action -> unit) : ReactElement =
    Open maybeHeading details [
        Button.Cancel (cancelLabel, Components.Button.Level.Secondary, fun () -> onResult false)
        Button.Confirm (okLabel, Components.Button.Level.Primary, fun () -> onResult true)
    ] close

let OpenSimpleAsync (maybeHeading: Option<string>) (details: string) (cancelLabel: string) (okLabel: string) (onConfirm: unit -> Async<Result<unit, string>>) (close: DialogCloseMethod -> ReactEvent.Action -> unit) : ReactElement =
    Open maybeHeading details [
        Button.Cancel (cancelLabel, Components.Button.Level.Secondary, ignore)
        Button.AsyncConfirm (okLabel, Components.Button.Level.Primary, onConfirm)
    ] close

let OpenAsAlert (maybeHeading: Option<string>) (details: string) (close: DialogCloseMethod -> ReactEvent.Action -> unit) : ReactElement =
    Open maybeHeading details [Cancel ("OK", LibClient.Components.Button.Level.Primary, ignore)] close
