module LibUiSubjectAdmin.Components.Subject

open LibClient

// NOTE had to inline the whole thing since otherwise the RenderDslCompiler doesn't pick up on the structured comment
type Props<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError when 'Subject :> Subject<'Id> and 'Projection :> SubjectProjection<'Id> and 'Id :> SubjectId and 'Id : comparison and 'Constructor :> Constructor and 'Action :> LifeAction and 'Event :> LifeEvent and 'OpError :> OpError and 'Index :> SubjectIndex<'OpError>> = (* GenerateMakeFunction *) {
    Data:    'Projection -> ReactElement
    Actions: 'Projection -> ReactElement
    Id:      'Id
    Service: LibUiSubject.Services.SubjectService.ISubjectService<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>

    key: string option // defaultWithAutoWrap JsUndefined
}

type Subject<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError
              when 'Subject      :> Subject<'Id>
              and  'Projection   :> SubjectProjection<'Id>
              and  'Id           :> SubjectId
              and  'Id           :  comparison
              and  'Constructor  :> Constructor
              and  'Action       :> LifeAction
              and  'Event        :> LifeEvent
              and  'OpError      :> OpError
              and  'Index        :> SubjectIndex<'OpError>>(_initialProps) =
    inherit PureStatelessComponent<Props<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>, Actions<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>, Subject<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>>("LibUiSubjectAdmin.Components.Subject", _initialProps, Actions<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>, hasStyles = true)

and Actions<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError
             when 'Subject      :>Subject<'Id>
             and  'Projection   :> SubjectProjection<'Id>
             and  'Id           :> SubjectId
             and  'Id           : comparison
             and  'Constructor  :> Constructor
             and  'Action       :> LifeAction
             and  'Event        :> LifeEvent
             and  'OpError      :> OpError
             and  'Index        :> SubjectIndex<'OpError>>(_this: Subject<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>) =
    class end

let Make = makeConstructor<Subject<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>, _, _>

// Unfortunately necessary boilerplate
type Estate<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError> = NoEstate8<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>
type Pstate = NoPstate
