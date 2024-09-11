module LibAutoUi.Components.DialogInputForm

open LibLang
open LibClient
open LibClient.Dialogs
open LibAutoUi.Types

type Parameters<'T> = {
    FormWrapper: LibAutoUi.Components.InputForm.FormWrapper<'T>
    Settings:    Settings
}

type Props<'T> = DialogProps<Parameters<'T>, 'T>

type Estate<'T> = {
    MaybeValue: Option<'T>
}

type Pstate = EmptyRecord

type DialogInputForm<'T>(initialProps) =
    inherit DialogComponent<Parameters<'T>, 'T, Estate<'T>, Pstate, Actions<'T>, DialogInputForm<'T>>("LibAutoUi.Components.DialogInputForm", initialProps.PstoreKey, initialProps, Actions, hasStyles = true)

    override _.GetDefaultPstate(_initialProps: Props<'T>) = EmptyRecord

    override _.GetInitialEstate(_initialProps: Props<'T>) = {
        MaybeValue = None
    }

    override _.CanCancel() : Async<bool> = async {
        return true
    }

and Actions<'T>(this: DialogInputForm<'T>) =
    member _.TryCancel (e: Browser.Types.Event) : unit =
        this.TryCancel

    member _.OnChange (result: InputValidationResult<'T>) : unit =
        this.SetEstate (fun estate _ _ -> { estate with MaybeValue = result |> Result.toOption })

    member _.SubmitResult (result: 'T) (e: Browser.Types.Event) : unit =
        this.props.ResponseChannel.OnResult result

let Make = makeConstructor<DialogInputForm<'T>,_,_>

// NOTE we can't take Parameters here because of circular dependency with DialogsInterface/DialogsImplementation
let Open<'T> (formWrapper: LibAutoUi.Components.InputForm.FormWrapper<'T>) (settings: Settings) (onResult: 'T -> unit) : unit =
    doOpen
        "DialogInputForm"
        {
            FormWrapper = formWrapper
            Settings    = settings
        }
        Make
        {
            OnResult      = onResult
            MaybeOnCancel = None
        }
