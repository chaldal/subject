module LibUiSubjectAdmin.Components.SubjectRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard


open LibClient.Components
open ReactXP.Components
open LibUiAdmin.Components
open LibUiSubject.Components
open LibUiSubjectAdmin.Components
open LibUiSubjectAdmin.Components

open LibClient
open LibClient.RenderHelpers

open LibUiSubjectAdmin.Components.Subject



let render(children: array<ReactElement>, props: LibUiSubjectAdmin.Components.Subject.Props<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>, estate: LibUiSubjectAdmin.Components.Subject.Estate<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>, pstate: LibUiSubjectAdmin.Components.Subject.Pstate, actions: LibUiSubjectAdmin.Components.Subject.Actions<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "LibUiSubject.Components.With.Subject"
    LibUiSubject.Components.Constructors.UiSubject.With.Subject(
        id = (props.Id),
        service = (props.Service),
        ``with`` =
            LibUiSubject.Components.With.Subject.PropWithFactory.Make
                (fun (subject) ->
                        (castAsElementAckingKeysWarning [|
                            let __parentFQN = Some "ReactXP.Components.View"
                            let __currClass = "la-table table"
                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                            ReactXP.Components.Constructors.RX.View(
                                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                children =
                                    [|
                                        FRS.table
                                            [(FRP.ClassName ("la-table-keyvalue"))]
                                            ([|
                                                FRS.tbody
                                                    []
                                                    ([|
                                                        props.Data subject
                                                        props.Actions subject
                                                    |])
                                            |])
                                    |]
                            )
                        |])
                )
    )
