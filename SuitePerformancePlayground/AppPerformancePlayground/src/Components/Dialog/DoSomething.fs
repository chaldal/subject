module AppPerformancePlayground.Components.Dialog.DoSomething

open LibLang
open LibClient
open LibClient.Components
open LibClient.Dialogs
open Fable.React
open LibClient.Components.Form.Base.Types
open AppPerformancePlayground.AppServices
open ReactXP.Styles

type Parameters = {
    Param: int
}

type Props = DialogProps<Parameters, unit>

type Estate = {
    AddCount: int
}

type Pstate = EmptyRecordType

[<RequireQualifiedAccess>]
type Field = | Name


type private Acc = {
    Name: Option<NonemptyString>
} with
    static member Initial () : Acc =
        {
            Name = None
        }

    interface AbstractAcc<Field, NonemptyString> with
        member this.Validate () : Result<NonemptyString, ValidationErrors<Field>> = validateForm {
            let! name = Forms.GetFieldValue2 this.Name Field.Name

            return name
        }

let private submit (onSuccess: unit -> unit) (accResult: NonemptyString) (_e: ReactEvent.Action) () : UDActionResult =
    Fable.Core.JS.console.log $"The string submitted was {accResult}"
    async {
        return Ok ()
    }

type DoSomething(initialProps) =
    inherit DialogComponent<Parameters, unit, Estate, Pstate, Actions, DoSomething>("AppPerformancePlayground.Components.Dialog.DoSomething", initialProps.PstoreKey, initialProps, Actions, hasStyles = false)

    override _.GetDefaultPstate(_initialProps: Props) = EmptyRecord

    override _.GetInitialEstate(_initialProps: Props) = {
        AddCount = 0
    }

    override _.CanCancel() : Async<bool> = async {
        return true
    }

    member private this.ResetForm () : unit =
        this.SetEstate (fun estate _ _ -> { estate with AddCount = estate.AddCount + 1 })

    override this.Render () : ReactElement =
        LC.Dialog.Shell.WhiteRounded.Standard (
            canClose = LibClient.Components.Dialog.Base.CanClose.When (
                [LibClient.Components.Dialog.Base.OnCloseButton; LibClient.Components.Dialog.Base.OnBackground; LibClient.Components.Dialog.Base.OnEscape],
                this.Actions.TryCancel
            ),
            heading = $"Type {this.props.Parameters.Param} in words",
            body = element {
                LC.Form.Base (
                    accumulator = LibClient.Components.Form.Base.Accumulator.ManageInternallyInitializingWith (Acc.Initial ()),
                    submit      = submit this.ResetForm,
                    key         = $"{this.state.estate.AddCount}",
                    content     = (fun form -> element {
                        LC.Input.Text (
                            styles              = [|Styles.Input|],
                            label               = "Name",
                            value               = form.Acc.Name,
                            validity            = form.FieldValidity Field.Name,
                            onChange            = (fun value -> form.UpdateAcc (fun acc -> { acc with Name = value })),
                            requestFocusOnMount = true,
                            onEnterKeyPress     = (ReactEvent.Action.Make >> form.TrySubmitLowLevel)
                        )
                        LC.Buttons [|
                            LC.Button (
                                label = "Close",
                                level = Button.Level.Secondary,
                                state = (ButtonHighLevelState.LowLevel (ButtonLowLevelState.Actionable this.Actions.TryCancel))
                            )
                            LC.Button (
                                label = "Add",
                                state = (ButtonHighLevelState.LowLevel (ButtonLowLevelState.Actionable form.TrySubmitLowLevel))
                            )
                        |]
                    })
                )
            }
        )

and Actions(this: DoSomething) =
    member _.TryCancel (e: ReactEvent.Action) : unit =
        this.TryCancel DialogCloseMethod.HistoryBack e

and private Styles() =
    static member val Input = makeViewStyles {
        marginBottom 20
    }

let Make = makeConstructor<DoSomething,_,_>

// NOTE we can't take Parameters here because of circular dependency with DialogsInterface/DialogsImplementation
let Open (param: int) (close: DialogCloseMethod -> ReactEvent.Action -> unit) : ReactElement =
    doOpen
        "DoSomething"
        {
            Param = param
        }
        Make
        {
            OnResult      = NoopFn
            MaybeOnCancel = None
        }
        close
