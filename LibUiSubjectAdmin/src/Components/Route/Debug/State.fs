[<AutoOpen>]
module LibUiSubjectAdmin.Components.Route_Debug_State

open Fable.React
open LibClient
open LibClient.Components
open LibRouter.Components
open LibUiSubjectAdmin.Components

module UiSubjectAdmin =
    module Route =
        module Debug =
            type Render<'Subject> =
            | AsJson      of ('Subject -> string)
            | AsComponent of ('Subject -> ReactElement)

            type Buttons = {
                AuditLog:   Option<ReactEvent.Action -> unit>
                RunAction:  Option<ReactEvent.Action -> unit>
                Additional: list<ReactElement>
            }

open UiSubjectAdmin.Route.Debug

type UiSubjectAdmin.Route.Debug with
    static member State<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError
                 when 'Subject      :> Subject<'Id>
                 and  'Projection   :> SubjectProjection<'Id>
                 and  'Id           :> SubjectId
                 and  'Id           :  comparison
                 and  'Constructor  :> Constructor
                 and  'Action       :> LifeAction
                 and  'Event        :> LifeEvent
                 and  'OpError      :> OpError
                 and  'Index        :> SubjectIndex<'OpError>> (
        service:  LibUiSubject.Services.SubjectService.ISubjectService<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>,
        idToString: 'Id -> string,
        toString:   'Projection -> string,
        id:         'Id,
        buttons:    Buttons
    ) : ReactElement =
        UiSubjectAdmin.Route.Debug.State (service, idToString, Render.AsJson toString, id, buttons)

    static member State<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError
                 when 'Subject      :> Subject<'Id>
                 and  'Projection   :> SubjectProjection<'Id>
                 and  'Id           :> SubjectId
                 and  'Id           :  comparison
                 and  'Constructor  :> Constructor
                 and  'Action       :> LifeAction
                 and  'Event        :> LifeEvent
                 and  'OpError      :> OpError
                 and  'Index        :> SubjectIndex<'OpError>> (
        service:    LibUiSubject.Services.SubjectService.ISubjectService<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>,
        idToString: 'Id -> string,
        render:     'Projection -> ReactElement,
        id:         'Id,
        buttons:    Buttons
    ) : ReactElement =
        UiSubjectAdmin.Route.Debug.State (service, idToString, Render.AsComponent render, id, buttons)

    [<Component>]
    static member State<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError
                 when 'Subject      :> Subject<'Id>
                 and  'Projection   :> SubjectProjection<'Id>
                 and  'Id           :> SubjectId
                 and  'Id           :  comparison
                 and  'Constructor  :> Constructor
                 and  'Action       :> LifeAction
                 and  'Event        :> LifeEvent
                 and  'OpError      :> OpError
                 and  'Index        :> SubjectIndex<'OpError>> (
        service: LibUiSubject.Services.SubjectService.ISubjectService<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>,
        idToString: 'Id -> string,
        render:     Render<'Projection>,
        id:         'Id,
        buttons:    Buttons
    ) : ReactElement =
        LR.Route (scroll = ScrollView.Vertical, children = [|
            LC.Section.Padded (elements {
                LC.Heading [|LC.Text $"{service.LifeCycleKey.LocalLifeCycleName} Subject State"|]
                LC.Heading (level = Heading.Level.Secondary, children = [|LC.Text $"ID {idToString id}"|])
                LC.Heading (level = Heading.Level.Secondary, children = [|LC.Text $"ID String {id.IdString}"|])

                LC.Buttons (align = HorizontalAlignment.Left, children = elements {
                    match buttons.AuditLog with
                    | None -> ()
                    | Some go ->
                        LC.Button (
                            label = "Subject Audit Log",
                            level = Button.Level.Secondary,
                            state = Input.ButtonHighLevelState.LowLevel (Input.ButtonLowLevelState.Actionable go)
                        )

                    match buttons.RunAction with
                    | None -> ()
                    | Some go ->
                        LC.Button (
                            label = "Run Action",
                            level = Button.Level.Secondary,
                            state = Input.ButtonHighLevelState.LowLevel (Input.ButtonLowLevelState.Actionable go)
                        )

                    buttons.Additional
                })

                LibUiSubject.Components.With.Subject.C.Subject (service, id, (fun (subject: 'Projection) ->
                    match render with
                    | AsJson toString ->
                        LC.Pre(text = toString subject)
                    | AsComponent toComponent ->
                        toComponent subject
                ))
            })
        |])
