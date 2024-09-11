module LibUiSubject.Components.With.SubjectsWithCount

open Fable.React
open LibClient
open LibClient.Components
open LibUiSubject
open LibClient.Components.Subscribe

type By<'Subject, 'Projection, 'Id, 'Index, 'Action, 'Event, 'OpError
                        when 'Subject      :> Subject<'Id>
                        and  'Projection   :> SubjectProjection<'Id>
                        and  'Id           :> SubjectId
                        and  'Id           :  comparison
                        and  'Action       :> LifeAction
                        and  'Event        :> LifeEvent
                        and  'OpError      :> OpError
                        and  'Index        :> SubjectIndex<'OpError>> =
| All     of ResultSetOptions<'Index>
| Indexed of Query: IndexQuery<'Index>
with
    member this.MakeSubscription
        (service: LibUiSubject.Services.SubjectService.ISubjectService<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>)
        (useCache: UseCache)
        : ((AsyncData<SubjectsWithTotalCount<'Id, 'Projection>> -> unit) -> LibClient.Services.Subscription.SubscribeResult) =
        match this with
        | By.All resultSetOptions -> fun subscriber -> service.SubscribeAllWithTotalCount     resultSetOptions useCache subscriber
        | By.Indexed query        -> fun subscriber -> service.SubscribeIndexedWithTotalCount query            useCache subscriber

// NEW hook implementation

let private getSubjects
    (store: SubscriptionsDataStore)
    (service: LibUiSubject.Services.SubjectService.ISubjectService<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>)
    (useCache: UseCache)
    (by: By<'Subject, 'Projection, 'Id, 'Index, 'Action, 'Event, 'OpError>)
    : AsyncData<SubjectsWithTotalCount<'Id,'Projection>> =
    store.Subscribe
        $"subjects-{service.LifeCycleKey.LocalLifeCycleName}-{by.ToString()}"
        (by.MakeSubscription service useCache)


// TODO need to figure out the module/namespace strategy here
type C =
    [<Component>]
    static member SubjectsWithTotalCount (
        service:                       LibUiSubject.Services.SubjectService.ISubjectService<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>,
        by:                            By<'Subject, 'Projection, 'Id, 'Index, 'Action, 'Event, 'OpError>,
        whenAvailable:                 SubjectsWithTotalCount<'Id,'Projection> -> ReactElement,
        ?whenUninitialized:            unit -> ReactElement,
        ?whenFetching:                 Option<SubjectsWithTotalCount<'Id,'Projection>> -> ReactElement,
        ?whenFailed:                   AsyncDataFailure -> ReactElement,
        ?whenUnavailable:              unit -> ReactElement,
        ?whenAccessDenied:             unit -> ReactElement,
        ?whenElse:                     unit -> ReactElement,
        ?useCache:                     UseCache,
        ?treatFetchingSomeAsAvailable: bool,
        ?key:                          string)
        : ReactElement =
            ignore key

            let shouldTreatFetchingSomeAsAvailable = defaultArg treatFetchingSomeAsAvailable false
            let store = Subscribe.useSubscriptions ()
            let subjectsAD = getSubjects store service (defaultArg useCache UseCache.IfReasonablyFresh) by
            let maybeAdjustedSubjectAD =
                match shouldTreatFetchingSomeAsAvailable with
                | false -> subjectsAD
                | true  -> AsyncData.treatFetchingSomeAsAvailable subjectsAD

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
    static member SubjectsWithTotalCount (
        service:   LibUiSubject.Services.SubjectService.ISubjectService<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>,
        by:        By<'Subject, 'Projection, 'Id, 'Index, 'Action, 'Event, 'OpError>,
        content:   AsyncData<SubjectsWithTotalCount<'Id,'Projection>> -> ReactElement,
        ?useCache: UseCache,
        ?key:      string)
        : ReactElement =
            ignore key
            let store = Subscribe.useSubscriptions ()
            let subjectsAD = getSubjects store service (defaultArg useCache UseCache.IfReasonablyFresh) by

            content subjectsAD

// LEGACY component implementation

let Raw           = WithSubjectsWithCount.Raw
let WhenAvailable = WithSubjectsWithCount.WhenAvailable

let No                = UseCache.No
let IfNewerThan       = UseCache.IfNewerThan
let IfReasonablyFresh = UseCache.IfReasonablyFresh
let IfAvailable       = UseCache.IfAvailable

// NOTE had to inline the whole thing since otherwise the RenderDslCompiler doesn't pick up on the structured comment
type Props<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError when 'Subject :> Subject<'Id> and 'Projection :> SubjectProjection<'Id> and 'Id :> SubjectId and 'Id : comparison and 'Constructor :> Constructor and 'Action :> LifeAction and 'Event :> LifeEvent and 'OpError :> OpError and 'Index :> SubjectIndex<'OpError>> = (* GenerateMakeFunction *) {
    Service:  LibUiSubject.Services.SubjectService.ISubjectService<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>
    By:       By<'Subject, 'Projection, 'Id, 'Index, 'Action, 'Event, 'OpError>
    UseCache: UseCache // default UseCache.IfReasonablyFresh
    With:     WithSubjectsWithCount<'Id, 'Projection>

    key: string option // defaultWithAutoWrap JsUndefined
}
with
    member this.Subscription: ((AsyncData<SubjectsWithTotalCount<'Id, 'Projection>> -> unit) -> LibClient.Services.Subscription.SubscribeResult) =
        this.By.MakeSubscription this.Service this.UseCache

type PropWithFactory = PropWithSubjectsWithCountFactory

type SubjectsWithCount<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError
                        when 'Subject      :> Subject<'Id>
                        and  'Projection   :> SubjectProjection<'Id>
                        and  'Id           :> SubjectId
                        and  'Id           :  comparison
                        and  'Constructor  :> Constructor
                        and  'Action       :> LifeAction
                        and  'Event        :> LifeEvent
                        and  'OpError      :> OpError
                        and  'Index        :> SubjectIndex<'OpError>>(_initialProps) =
    inherit PureStatelessComponent<Props<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>, Actions<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>, SubjectsWithCount<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>>("LibUiSubject.Components.With.SubjectsWithCount", _initialProps, Actions<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>, hasStyles = false)

and Actions<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError
                        when 'Subject      :> Subject<'Id>
                        and  'Projection   :> SubjectProjection<'Id>
                        and  'Id           :> SubjectId
                        and  'Id           :  comparison
                        and  'Constructor  :> Constructor
                        and  'Action       :> LifeAction
                        and  'Event        :> LifeEvent
                        and  'OpError      :> OpError
                        and  'Index        :> SubjectIndex<'OpError>>(_this: SubjectsWithCount<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>) =
    class end

let Make = makeConstructor<SubjectsWithCount<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>, _, _>

// Unfortunately necessary boilerplate
type Estate<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError> = NoEstate8<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>
type Pstate = NoPstate
