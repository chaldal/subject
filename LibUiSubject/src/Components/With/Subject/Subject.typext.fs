module LibUiSubject.Components.With.Subject

open Fable.React
open LibClient
open LibClient.Components
open LibUiSubject

// NEW hook implementation

let private getSubject
    (store: Subscribe.SubscriptionsDataStore)
    (service: LibUiSubject.Services.SubjectService.ISubjectService<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>)
    (useCache: UseCache)
    (id: 'Id)
    : AsyncData<'Projection> =
    store.Subscribe
        $"subject-{service.LifeCycleKey.LocalLifeCycleName}-{id.IdString}"
        (fun subscriber ->
            service.SubscribeOne id useCache subscriber
        )

// TODO need to figure out the module/namespace strategy here
type C =
    [<Component>]
    static member Subject (
        service:                       LibUiSubject.Services.SubjectService.ISubjectService<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>,
        id:                            'Id,
        whenAvailable:                 'Projection -> ReactElement,
        ?whenUninitialized:            unit -> ReactElement,
        ?whenFetching:                 Option<'Projection> -> ReactElement,
        ?whenFailed:                   AsyncDataFailure -> ReactElement,
        ?whenUnavailable:              unit -> ReactElement,
        ?whenAccessDenied:             unit -> ReactElement,
        ?whenElse:                     unit -> ReactElement,
        ?useCache:                     UseCache,
        ?treatFetchingSomeAsAvailable: bool)
        : ReactElement =
            let shouldTreatFetchingSomeAsAvailable = defaultArg treatFetchingSomeAsAvailable false
            let store = Subscribe.useSubscriptions ()
            let subjectAD = getSubject store service (defaultArg useCache UseCache.IfReasonablyFresh) id
            let maybeAdjustedSubjectAD =
                match shouldTreatFetchingSomeAsAvailable with
                | false -> subjectAD
                | true  -> AsyncData.treatFetchingSomeAsAvailable subjectAD

            LC.AsyncData (
                data               = maybeAdjustedSubjectAD,
                whenAvailable      = whenAvailable,
                ?whenUninitialized = whenUninitialized,
                ?whenFetching      = whenFetching,
                ?whenFailed        = whenFailed,
                ?whenUnavailable   = whenUnavailable,
                ?whenAccessDenied  = whenAccessDenied,
                ?whenElse          = whenElse
            )

    [<Component>]
    static member Subject (
        service:   LibUiSubject.Services.SubjectService.ISubjectService<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>,
        id:        'Id,
        content:   AsyncData<'Projection> -> ReactElement,
        ?useCache: UseCache)
        : ReactElement =
            let store = Subscribe.useSubscriptions ()
            let subjectAD = getSubject store service (defaultArg useCache UseCache.IfReasonablyFresh) id

            content subjectAD



// LEGACY component implementation

let Raw           = WithSubject.Raw
let WhenAvailable = WithSubject.WhenAvailable

let No                = UseCache.No
let IfNewerThan       = UseCache.IfNewerThan
let IfReasonablyFresh = UseCache.IfReasonablyFresh
let IfAvailable       = UseCache.IfAvailable

// NOTE that IfReasonablyFresh is a reasonable, thought-out choice of a default here,
// since the whole point of having a default TTL per subject presupposes that it's okay
// to consume subjects cached in such a manner without actively thinking about, and that's
// what a default is — allowing the consumer to not actively think about a value.

// NOTE had to inline the whole thing since otherwise the RenderDslCompiler doesn't pick up on the structured comment
type Props<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError when 'Subject :> Subject<'Id> and 'Projection :> SubjectProjection<'Id> and 'Id :> SubjectId and 'Id : comparison and 'Constructor :> Constructor and 'Action :> LifeAction and 'Event :> LifeEvent and 'OpError :> OpError and 'Index :> SubjectIndex<'OpError>> = (* GenerateMakeFunction *) {
    Service:  LibUiSubject.Services.SubjectService.ISubjectService<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>
    Id:       'Id
    UseCache: UseCache // default UseCache.IfReasonablyFresh
    With:     WithSubject<'Projection>

    key: string option // defaultWithAutoWrap JsUndefined
}

type PropWithFactory = PropWithSubjectFactory

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
    inherit PureStatelessComponent<Props<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>, Actions<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>, Subject<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>>("LibUiSubject.Components.With.Subject", _initialProps, Actions<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>, hasStyles = false)

and Actions<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError
                        when 'Subject      :> Subject<'Id>
                        and  'Projection   :> SubjectProjection<'Id>
                        and  'Id           :> SubjectId
                        and  'Id           :  comparison
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
