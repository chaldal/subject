module LibClient.Components.Dialog.Prompt

open LibLang
open LibClient
open LibClient.Dialogs
open LibClient.Components.Form.Base.Types

type Parameters = {
    MaybeHeading: Option<string>
    Details:      string
    OnResult:     Option<NonemptyString> -> unit
}

type Props = DialogProps<Parameters, unit>

type Estate = EmptyRecordType
type Pstate = EmptyRecordType


[<RequireQualifiedAccess>]
type Field = Value

type Acc = {
    Value: Option<NonemptyString>
} with
    static member Initial : Acc =
        {
            Value = None
        }

    interface AbstractAcc<Field, NonemptyString> with
        member this.Validate () : Result<NonemptyString, ValidationErrors<Field>> = validateForm {
            let! value = Forms.GetFieldValue2 this.Value Field.Value
            return value
        }

type Prompt(initialProps) =
    inherit DialogComponent<Parameters, unit, Estate, Pstate, Actions, Prompt>("LibClient.Components.Dialog.Prompt", initialProps.PstoreKey, initialProps, Actions, hasStyles = true)

    override _.GetDefaultPstate(_initialProps: Props) = EmptyRecord

    override _.GetInitialEstate(_initialProps: Props) = EmptyRecord

    override _.CanCancel() : Async<bool> = async {
        return true
    }

and Actions(this: Prompt) =
    member _.TryCancel (e: ReactEvent.Action) : unit =
        this.TryCancel DialogCloseMethod.HistoryBack e
        this.props.Parameters.OnResult None

    member _.Submit (value: NonemptyString) (e: ReactEvent.Action) () : UDActionResult =
        async {
            this.props.Parameters.OnResult (Some value)
            this.TryCancel DialogCloseMethod.HistoryBack e
            return Ok ()
        }



let Make = makeConstructor<Prompt,_,_>

// NOTE we can't take Parameters here because of circular dependency with DialogsInterface/DialogsImplementation
let Open (maybeHeading: Option<string>) (details: string) (onResult: Option<NonemptyString> -> unit) (close: DialogCloseMethod -> ReactEvent.Action -> unit) : ReactElement =
    doOpen
        "Prompt"
        {
            MaybeHeading = maybeHeading
            Details      = details
            OnResult     = onResult
        }
        Make
        {
            OnResult      = NoopFn
            MaybeOnCancel = None
        }
        close
