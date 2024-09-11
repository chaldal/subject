[<AutoOpen>]
module LibUiSubjectAdmin.Components.SubjectAction

open LibLang
open LibClient
open LibClient.Components
open LibClient.Dialogs
open Fable.React
open LibClient.Components.Form.Base.Types
open ReactXP.Styles

type Parameters<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError
                 when 'Subject      :> Subject<'Id>
                 and  'Projection   :> SubjectProjection<'Id>
                 and  'Id           :> SubjectId
                 and  'Id           :  comparison
                 and  'Constructor  :> Constructor
                 and  'Action       :> LifeAction
                 and  'Event        :> LifeEvent
                 and  'OpError      :> OpError
                 and  'Index        :> SubjectIndex<'OpError>> = {
    Service: LibUiSubject.Services.SubjectService.ISubjectService<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>
    Encoder: string -> Result<'Action, string>
    Id:      'Id
}

type Props<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError
                        when 'Subject      :> Subject<'Id>
                        and  'Projection   :> SubjectProjection<'Id>
                        and  'Id           :> SubjectId
                        and  'Id           :  comparison
                        and  'Constructor  :> Constructor
                        and  'Action       :> LifeAction
                        and  'Event        :> LifeEvent
                        and  'OpError      :> OpError
                        and  'Index        :> SubjectIndex<'OpError>> = DialogProps<Parameters<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>, unit>

type Estate = {
    AddCount: int
}

type Pstate = EmptyRecordType

[<RequireQualifiedAccess>]
type Field = | Json

type private Acc<'Action> = {
    Json:    Option<NonemptyString>
    Encoder: string -> Result<'Action, string>
} with
    static member Initial (encoder: string -> Result<'Action, string>) : Acc<'Action> =
        {
            Json    = None
            Encoder = encoder
        }

    interface AbstractAcc<Field, 'Action> with
        member this.Validate () : Result<'Action, ValidationErrors<Field>> = validateForm {
            let! json = Forms.GetFieldValue2 this.Json Field.Json

            return!
                this.Encoder json.Value
                |> Result.mapError (fun message -> ValidationErrors.Empty.AddInvalid Field.Json message)
        }

let private submit (service: LibUiSubject.Services.SubjectService.ISubjectService<_, _, 'Id, _, _, 'Action, _, _>) (close: ReactEvent.Action -> unit) (id: 'Id) (action: 'Action) (e: ReactEvent.Action) () : UDActionResult =
    action
    |> service.Act id
    |> Async.Map (fun result ->
        result
        |> Result.mapError (fun opError -> opError.ToString())
        |> Result.map (fun result ->
            close e
            result
        )
    )


type SubjectAction<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError
                        when 'Subject      :> Subject<'Id>
                        and  'Projection   :> SubjectProjection<'Id>
                        and  'Id           :> SubjectId
                        and  'Id           :  comparison
                        and  'Constructor  :> Constructor
                        and  'Action       :> LifeAction
                        and  'Event        :> LifeEvent
                        and  'OpError      :> OpError
                        and  'Index        :> SubjectIndex<'OpError>>(initialProps) =
    inherit DialogComponent<Parameters<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>, unit, Estate, Pstate, Actions<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>, SubjectAction<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>>("LibUiSubjectAdmin.Components.Dialog.SubjectAction", initialProps.PstoreKey, initialProps, Actions, hasStyles = false)

    override _.GetDefaultPstate(_initialProps: Props<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>) = EmptyRecord

    override _.GetInitialEstate(_initialProps: Props<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>) = {
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
            heading = $"{this.props.Parameters.Service.LifeCycleKey.LocalLifeCycleName} Action",
            body = asFragment [
                LC.Form.Base (
                    accumulator = LibClient.Components.Form.Base.Accumulator.ManageInternallyInitializingWith (Acc.Initial this.props.Parameters.Encoder),
                    submit      = submit this.props.Parameters.Service this.Actions.TryCancel this.props.Parameters.Id,
                    key         = $"{this.state.estate.AddCount}",
                    content     = (fun form -> element {
                        LC.Input.Text (
                            styles              = [|Styles.Input|],
                            label               = "JSON",
                            value               = form.Acc.Json,
                            validity            = form.FieldValidity Field.Json,
                            onChange            = (fun value -> form.UpdateAcc (fun acc -> { acc with Json = value })),
                            requestFocusOnMount = true,
                            multiline           = true
                        )
                        LC.Buttons [|
                            LC.Button (
                                label = "Close",
                                level = Button.Level.Secondary,
                                state = (ButtonHighLevelState.LowLevel (ButtonLowLevelState.Actionable this.Actions.TryCancel))
                            )
                            LC.Button (
                                label = "Validate",
                                level = Button.Level.Secondary,
                                state = (ButtonHighLevelState.LowLevel (ButtonLowLevelState.Actionable (fun _ -> form.ShowValidationErrors true)))
                            )
                            LC.Button (
                                label = "Run",
                                state = (ButtonHighLevelState.LowLevel (ButtonLowLevelState.Actionable form.TrySubmitLowLevel))
                            )
                        |]
                    })
                )
            ]
        )

and Actions<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError
                        when 'Subject      :> Subject<'Id>
                        and  'Projection   :> SubjectProjection<'Id>
                        and  'Id           :> SubjectId
                        and  'Id           :  comparison
                        and  'Constructor  :> Constructor
                        and  'Action       :> LifeAction
                        and  'Event        :> LifeEvent
                        and  'OpError      :> OpError
                        and  'Index        :> SubjectIndex<'OpError>>(this: SubjectAction<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>) =
    member _.TryCancel (e: ReactEvent.Action) : unit =
        this.TryCancel DialogCloseMethod.HistoryBack e

and private Styles() =
    static member val Input = makeViewStyles {
        marginBottom 20
        // this doesn't work, need to use theming, which we don't yet have precedent for in F# dialect
        height 300
        minWidth 300
    }

let Make = makeConstructor<SubjectAction<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>,_,_>

// NOTE we can't take Parameters here because of circular dependency with DialogsInterface/DialogsImplementation
let Open (service) (actionEncoder) (id) (close: DialogCloseMethod -> ReactEvent.Action -> unit) : ReactElement =
    doOpen
        "SubjectAction"
        {
            Service = service
            Encoder = actionEncoder
            Id      = id
        }
        Make
        {
            OnResult      = NoopFn
            MaybeOnCancel = None
        }
        close
